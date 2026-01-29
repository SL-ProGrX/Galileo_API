using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier
{
    public class FrmTesTransferenciasDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria _mTesoreria;


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

                // 2) Ejecutar SOLO SQL permitido
                var result = conn.Query<TransferenciasData>(allowedSql, new
                {
                    banco = transferencia.parametros.banco,
                    tipoDoc = transferencia.parametros.tipoDoc,
                    minimo = transferencia.parametros.minimo,
                    maximo = transferencia.parametros.maximo,
                    fechaInicio = transferencia.parametros.fechaInicio,
                    fechaCorte = transferencia.parametros.fechaCorte
                }).ToList();

                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        consc = NextConsecutivo(CodEmpresa, transferencia, consc);
                        var vDocumento = consc.ToString("D4");

                        curMonto += item.monto;

                        const string queryUpdate = @"
UPDATE Tes_Transacciones
SET Estado = 'T',
    Fecha_Emision = @fechaEmision,
    Ubicacion_Actual = 'T',
    FECHA_TRASLADO = @fechaEmision,
    NDocumento = @nDocumento,
    user_genera = @usuario,
    documento_base = @bancoConsec,
    COD_PLAN = @plan
WHERE NSolicitud = @nSolicitud;";

                        conn.Execute(queryUpdate, new
                        {
                            fechaEmision = vFecha, // DateTime, no string
                            nDocumento = vDocumento,
                            usuario = transferencia.usuario,
                            bancoConsec = transferencia.bancoConsec,
                            plan = transferencia.plan,
                            nSolicitud = item.nSolicitud
                        });

                        const string qryBitacora = @"EXEC spTesBitacora @Solicitud, '10', @Detalle, @Usuario;";
                        conn.Execute(qryBitacora, new
                        {
                            Solicitud = item.nSolicitud,
                            Detalle = $"Transferencia...:{transferencia.bancoConsec}",
                            Usuario = transferencia.usuario
                        });

                        const string qrySaldos = @"EXEC spTESAfectaBancos @NSOLICITUD, 'E';";
                        conn.Execute(qrySaldos, new { NSOLICITUD = item.nSolicitud });

                        if (item.modulo == "CC" && item.subModulo == "C")
                        {
                            ActualizaReferencia(conn, vDocumento, item);
                        }
                    }

                    ActualizaTesBancosDocsConse(conn, consc, transferencia);
                }

                return DbHelper.OkResponse("Transferencias Aceptadas Correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static class TransferSql
        {
            // SIN FILTRO
            public const string QueryTransac_Base = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null
Order by Nsolicitud";

            public const string BaseQuery_Base = @"
(SELECT TOP (@top) nsolicitud
 FROM Tes_Transacciones
 WHERE Estado = 'P' AND Tipo = @tipoDoc
   AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL
 Order by Nsolicitud)";

            // POR SOLICITUDES
            public const string QueryTransac_Solicitudes = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null
  And NSolicitud Between @minimo And @maximo
Order by Nsolicitud";

            public const string BaseQuery_Solicitudes = @"
(SELECT TOP (@top) nsolicitud
 FROM Tes_Transacciones
 WHERE Estado = 'P' AND Tipo = @tipoDoc
   AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL
   And NSolicitud Between @minimo And @maximo
 Order by Nsolicitud)";

            // POR FECHAS
            public const string QueryTransac_Fechas = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null
  And Fecha_Solicitud Between @fechaInicio And @fechaCorte
Order by Nsolicitud";

            public const string BaseQuery_Fechas = @"
(SELECT TOP (@top) nsolicitud
 FROM Tes_Transacciones
 WHERE Estado = 'P' AND Tipo = @tipoDoc
   AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL
   And Fecha_Solicitud Between @fechaInicio And @fechaCorte
 Order by Nsolicitud)";
        }

        private static string NormalizeSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return string.Empty;
            return string.Join(" ", sql.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        private static string? ResolveAllowedTransferQuery(string incomingSql)
        {
            var inc = NormalizeSql(incomingSql);

            if (inc == NormalizeSql(TransferSql.QueryTransac_Base)) return TransferSql.QueryTransac_Base;
            if (inc == NormalizeSql(TransferSql.QueryTransac_Solicitudes)) return TransferSql.QueryTransac_Solicitudes;
            if (inc == NormalizeSql(TransferSql.QueryTransac_Fechas)) return TransferSql.QueryTransac_Fechas;

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
        private void ActualizaReferencia(SqlConnection conn, string vDocumento,  TransferenciasData item)
        {
            var QueryCC = "";
            string documento = string.Empty;
            if (item.detalle1 != null || item.detalle1 != "")
            {
                if (item.referencia != null)
                {
                    documento = vDocumento;
                    
                    //'TIENE REFERENCIA
                    QueryCC = $@"Update DesemBolsos Set 
                                                    Cod_Banco= @CodBanco,
                                                    TDocumento= @tipo,
                                                    NDocumento= @Documento 
                                                    Where ID_Desembolso= @IdDesembolso";

                   
                }
                else
                {
                    documento = item.tipo + "-" + vDocumento;
                    //'NO TIENE REFERENCIA
                    QueryCC = $@"Update Reg_Creditos Set 
                                                            Cod_Banco = @CodBanco,
                                                            Documento_Referido = @Documento 
                                                            Where ID_Solicitud= @IdSolicitud";
                }

                var parametros = new
                {
                    CodBanco = item.id_Banco,
                    tipo = item.tipo,
                    Documento = documento,
                    IdDesembolso = item.codigo,
                    IdSolicitud = item.detalle1
                };

                conn.Execute(QueryCC, parametros);
            }
        }

        //Helper class para la consulta de transferencias
        private void ActualizaTesBancosDocsConse(SqlConnection conn, long consc, TesTransferenciasInfo transferencia)
        {
            //'Actualiza Consecutivo Interno
            var queryConsec = $@"update tes_banco_docs set CONSECUTIVO_DET = @cons
                                         where Tipo = @tipo and id_banco = @id_banco";

            conn.Execute(queryConsec, new { cons = consc, tipo = transferencia.tipoDoc, id_banco = transferencia.id_Banco });
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
                _mTesoreria.fxTesTipoDocConsec(CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc!, "-", transferencia.plan!);
                return DbHelper.OkResponse("Transferencia Revertida Correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


    }
}
