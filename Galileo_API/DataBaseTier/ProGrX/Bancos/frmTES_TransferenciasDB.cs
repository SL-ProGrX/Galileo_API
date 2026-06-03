using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Microsoft.Data.SqlClient;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesTransferenciasDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria _mTesoreria;

        const int commandTimeoutSeconds = 600;
        public FrmTesTransferenciasDB(IConfiguration config)
        {
            _mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para aceptar las transferencias bancarias y actualiza los registros correspondientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_Banco"></param>
        /// <param name="TipoDoc"></param>
        /// <param name="plan"></param>
        /// <param name="usuario"></param>
        /// <param name="BancoConsec"></param>
        /// <param name="gstrQuery"></param>
        /// <returns></returns>
        public ErrorDto TES_Transferencia_Aceptar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (string.IsNullOrWhiteSpace(transferencia.gstrQuery))
                    return DbHelper.ErrorResponse("Consulta de transferencia inválida o no especificada.");

                // 1) Resolver la consulta a una plantilla permitida (servidor)
                var allowedSql = ResolveAllowedTransferQuery(transferencia.gstrQuery);
                if (allowedSql is null)
                    return DbHelper.ErrorResponse("Consulta de transferencia no permitida.");

                long consc = 0;
                decimal curMonto = 0m;
                var vFecha = DateTime.Now;

                if(transferencia.tipoDoc == "TS")
                {
                    allowedSql= allowedSql.Replace("Estado = 'P'", "Estado IN ('P', 'I')");
                }

                var cantidadSolicitudes = transferencia.parametros!.cantidad;

                if (cantidadSolicitudes <= 0 &&
                    transferencia.parametros.maximo >= transferencia.parametros.minimo &&
                    transferencia.parametros.minimo > 0)
                {
                    cantidadSolicitudes =
                        (transferencia.parametros.maximo - transferencia.parametros.minimo) + 1;
                }

                if (cantidadSolicitudes <= 0)
                {
                    cantidadSolicitudes = int.MaxValue;
                }

                // 2) Ejecutar SOLO SQL permitido
                var result = conn.Query<TransferenciasData>(allowedSql.Trim('(', ')'), new
                {
                    top = cantidadSolicitudes,
                    banco = transferencia.parametros.banco,
                    tipoDoc = transferencia.parametros.tipoDoc,
                    minimo = transferencia.parametros.minimo,
                    maximo = transferencia.parametros.maximo,
                    fechaInicio = transferencia.parametros.fecha_inicio,
                    fechaCorte = transferencia.parametros.fecha_corte
                },
                commandTimeout: commandTimeoutSeconds).ToList();

                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        consc = NextConsecutivo(CodEmpresa, transferencia, consc);
                        
                        var vDocumento = consc.ToString("D4");

                        item.documento = vDocumento;
                        curMonto += item.monto;

                        conn.Execute(FrmTesAutorizacionSql.Query_UpdateTransacciones, new
                        {
                            
                            fechaEmision = vFecha, // DateTime, no string
                            nDocumento = vDocumento,
                            usuario = transferencia.usuario,
                            bancoConsec = transferencia.bancoConsec,
                            plan = transferencia.plan,
                            nSolicitud = item.nSolicitud
                        },
                        commandTimeout: commandTimeoutSeconds);

                        const string qryBitacora = @"EXEC spTesBitacora @Solicitud, '10', @Detalle, @Usuario;";
                        conn.Execute(qryBitacora, new
                        {
                            Solicitud = item.nSolicitud,
                            Detalle = $"Transferencia...:{transferencia.bancoConsec}",
                            Usuario = transferencia.usuario
                        },
                        commandTimeout: commandTimeoutSeconds
                        );

                        const string qrySaldos = @"EXEC spTESAfectaBancos @NSOLICITUD, 'E';";
                        conn.Execute(qrySaldos, new { NSOLICITUD = item.nSolicitud }, commandTimeout: commandTimeoutSeconds);

                        if (item.modulo == "CC" && item.subModulo == "C")
                        {
                            ActualizaReferencia(conn, vDocumento, item);
                        }
                    }
                    ActualizaTesBancosDocsConse(conn, consc, transferencia);

                    spTes_TEI_Acreaditacion(CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc!, consc.ToString("D4"), transferencia.usuario!);

                }

                return DbHelper.OkResponse(
                     $"Transferencias procesadas {result.Count} Correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string NormalizeSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return string.Empty;
            // Use StringSplitOptions.RemoveEmptyEntries to split on whitespace, avoiding null argument
            return string.Join(" ", sql.Trim()                 // elimina \r, \n, tabs, espacios
                                .Trim('(', ')')         // ahora sí, paréntesis externos
                                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string? ResolveAllowedTransferQuery(string incomingSql)
        {
            var inc = NormalizeSql(incomingSql);

            if (inc == NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Base))
                return NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Base);

            if (inc == NormalizeSql(FrmTesAutorizacionSql.BaseQuery_Base))
                return NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Base);

            if (inc == NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Solicitudes))
                return NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Solicitudes);

            if (inc == NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Fechas))
                return NormalizeSql(FrmTesAutorizacionSql.QueryTransac_Fechas);

            return null;
        }
        private long NextConsecutivo(int CodEmpresa, TesTransferenciasInfo transferencia, long actual)
        {
            if (actual > 0) return actual + 1;

            var conseR = _mTesoreria.fxTesTipoDocConsecInterno(
                CodEmpresa,
                transferencia.id_Banco,
                transferencia.tipoDoc ?? string.Empty,
                "+",
                transferencia.plan ?? string.Empty);

            if (conseR.Code == -1)
                throw new InvalidOperationException("No fue posible obtener el consecutivo interno.");

            return conseR.Result;
        }


        /// <summary>
        /// Método para actualizar la referencia de una transferencia en la base de datos.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="vDocumento"></param>
        /// <param name="item"></param>
        private void ActualizaReferencia(SqlConnection conn, string vDocumento, TransferenciasData item)
        {
            if (string.IsNullOrWhiteSpace(item.detalle1))
                return;

            if (!int.TryParse(item.detalle1.Trim(), out var idSolicitud))
                return;

            var tieneReferencia =
                !string.IsNullOrWhiteSpace(item.referencia) &&
                int.TryParse(item.referencia.Trim(), out var referencia) &&
                referencia > 0;

            if (tieneReferencia)
            {
                if (string.IsNullOrWhiteSpace(item.codigo))
                    return;

                if (!int.TryParse(item.codigo.Trim(), out var idDesembolso))
                    return;

                const string queryDesembolso = @"
Update DesemBolsos Set
    Cod_Banco = @CodBanco,
    TDocumento = @Tipo,
    NDocumento = @Documento
Where ID_Desembolso = @IdDesembolso";

                conn.Execute(queryDesembolso, new
                {
                    CodBanco = item.id_Banco,
                    Tipo = item.tipo,
                    Documento = vDocumento,
                    IdDesembolso = idDesembolso
                }, commandTimeout: commandTimeoutSeconds);

                return;
            }

            const string queryRegCreditos = @"
Update Reg_Creditos Set
    Cod_Banco = @CodBanco,
    Documento_Referido = @Documento
Where ID_Solicitud = @IdSolicitud";

            conn.Execute(queryRegCreditos, new
            {
                CodBanco = item.id_Banco,
                Documento = $"{item.tipo}-{vDocumento}",
                IdSolicitud = idSolicitud
            },
            commandTimeout: commandTimeoutSeconds);
        }

        //Helper class para la consulta de transferencias
        private void ActualizaTesBancosDocsConse(SqlConnection conn, long consc, TesTransferenciasInfo transferencia)
        {
            //'Actualiza Consecutivo Interno
            var queryConsec = $@"update tes_banco_docs set CONSECUTIVO_DET = @cons
                                         where Tipo = @tipo and id_banco = @id_banco";

            conn.Execute(queryConsec, new { cons = consc, tipo = transferencia.tipoDoc, id_banco = transferencia.id_Banco }
            , commandTimeout: commandTimeoutSeconds);
        }


        /// <summary>
        /// Método para revertir una transferencia bancaria, actualizando los registros correspondientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_Banco"></param>
        /// <param name="TipoDoc"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto TES_Transferencia_Reversar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            try
            {
                if(transferencia.tipoDoc == "TS")
                {
                    return new ErrorDto{
                        Code = 0,
                        Description = "Transferencias SINPE deben ser reversadas por otra via."
                         };
                }

                _mTesoreria.fxTesTipoDocConsec(CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc!, "-", transferencia.plan!);
                return DbHelper.OkResponse("Transferencia Revertida Correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private void spTes_TEI_Acreaditacion(int CodEmpresa, int banco, string tipo, string documento, string usuario)
        {
            var connectionString = _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa);

            var parametros = new DynamicParameters();

            parametros.Add("@Banco", banco, DbType.Int64);
            parametros.Add("@Tipo", tipo, DbType.String);
            parametros.Add("@Documento", documento, DbType.String);
            parametros.Add("@Usuario", usuario, DbType.String);

            var result = DbHelper.ExecuteStoredProcedureSingle<ErrorDto>(
                  connectionString,
                  "dbo.spTes_TEI_Acreaditacion",
                  default,
                  parametros
              );

        }
    }
}
