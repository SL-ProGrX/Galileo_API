using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTransferenciasDb
    {
        private readonly IConfiguration? _config;
        private MProGrXAuxiliarDB _utils;
        private MTesoreria _mTesoreria;
        private int modulo = 9;
        private readonly MProGrxMain _Security_MainDB;

        public FrmTesTransferenciasDb(IConfiguration config)
        {
            _config = config;
            _utils = new MProGrXAuxiliarDB(config);
            _mTesoreria = new MTesoreria(config);
            _Security_MainDB = new MProGrxMain(config);
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                long consc = 0;
                decimal curMonto = 0;
                DateTime vFecha = DateTime.Now;
                string fecha = _utils.validaFechaGlobal(vFecha);

                using var connection = new SqlConnection(stringConn);
                {
                    var query = transferencia.gstrQuery;
                    var result = connection.Query<TransferenciasData>(query,
                        new
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
                            if (consc == 0)
                            {
                                var conseR = _mTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc, "+", transferencia.plan);
                                if (conseR.Code != -1)
                                {
                                    consc = conseR.Result;
                                }
                            }
                            else
                            {
                                consc++;
                            }

                            string vDocumento = consc.ToString("D4");

                            curMonto = curMonto + item.monto;// falta el modelo de la consulta principal

                            var queryUpdate = @"
                                    UPDATE Tes_Transacciones
                                    SET Estado = 'T',
                                        Fecha_Emision = @Fecha,
                                        Ubicacion_Actual = 'T',
                                        Fecha_Traslado = @Fecha,
                                        NDocumento = @NDocumento,
                                        user_genera = @UsuarioGenera,
                                        documento_base = @DocumentoBase,
                                        COD_PLAN = @CodPlan
                                    WHERE NSolicitud = @NSolicitud;
                                ";

                            var resultUpdate = connection.Execute(queryUpdate, new
                            {
                                Fecha = fecha, // ideal que sea DateTime, no string
                                NDocumento = vDocumento,
                                UsuarioGenera = transferencia.usuario,
                                DocumentoBase = transferencia.bancoConsec,
                                CodPlan = transferencia.plan,
                                NSolicitud = item.nSolicitud
                            });

                            // Bitácora Especial
                            var resultBitacora = connection.Execute(
                                "spTesBitacora",
                                new
                                {
                                    NSolicitud = item.nSolicitud,
                                    // si el SP no tiene nombres y usa posición, igual podés mapear por nombre
                                    Tipo = "10",
                                    Detalle = $"Transferencia...:{transferencia.bancoConsec}",
                                    Usuario = transferencia.usuario
                                },
                                commandType: System.Data.CommandType.StoredProcedure
                            );

                            // Afecta Saldo en Bancos
                            var resultSaldos = connection.Execute(
                                "spTESAfectaBancos",
                                new
                                {
                                    NSolicitud = item.nSolicitud,
                                    Accion = "E"
                                },
                                commandType: System.Data.CommandType.StoredProcedure
                            );

                            //Actualiza Cuentas Corrientes
                            if (item.modulo == "CC" && item.subModulo == "C")
                            {
                                var QueryCC = "";
                                if (item.detalle1 != null || item.detalle1 != "")
                                {
                                    if (item.referencia != null)
                                    {
                                        //'TIENE REFERENCIA
                                        QueryCC = $@"Update DesemBolsos Set 
                                                    Cod_Banco= {item.id_Banco} ,
                                                    TDocumento= '{item.tipo}',
                                                    NDocumento= '{vDocumento}' 
                                                    Where ID_Desembolso= {item.codigo}";
                                    }
                                    else
                                    {
                                        //'NO TIENE REFERENCIA
                                        QueryCC = $@"Update Reg_Creditos Set 
                                                            Cod_Banco = {item.id_Banco} ,
                                                            Documento_Referido = '{item.tipo}-{vDocumento}' 
                                                            Where ID_Solicitud= {item.detalle1}";
                                    }

                                    var resultCC = connection.Execute(QueryCC);
                                }
                            }

                        }


                        // Actualiza Consecutivo Interno
                        var queryConsec = @"
                                UPDATE tes_banco_docs
                                SET CONSECUTIVO_DET = @Consecutivo
                                WHERE Tipo = @TipoDoc
                                  AND id_banco = @IdBanco;
                            ";

                        var resultConsec = connection.Execute(queryConsec, new
                        {
                            Consecutivo = consc,
                            TipoDoc = transferencia.tipoDoc,
                            IdBanco = transferencia.id_Banco
                        });
                    }

                    //sale con reportes

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                _mTesoreria.fxTesTipoDocConsec(CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc, "-", transferencia.plan);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


    }
}
