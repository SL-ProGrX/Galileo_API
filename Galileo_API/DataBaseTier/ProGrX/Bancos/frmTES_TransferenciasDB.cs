using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;
using Sinpe_TFT;

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
                long consc = 0;
                decimal curMonto = 0;
                DateTime vFecha = DateTime.Now;
                string fecha = MProGrXAuxiliarDB.validaFechaGlobal(vFecha, "yyyy-MM-dd HH:mm:ss") ?? string.Empty;

                var query = transferencia.gstrQuery!;
                var result = conn.Query<TransferenciasData>(query,
                    new
                    {
                        banco = transferencia.parametros!.banco,
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
                        consc = consc == 0
                        ? _mTesoreria.fxTesTipoDocConsecInterno(
                              CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc!, "+", transferencia.plan!
                          ).Result
                        : consc + 1;

                        string vDocumento = consc.ToString("D4");

                        curMonto = curMonto + item.monto;// falta el modelo de la consulta principal

                        var queryUpdate = $@"Update Tes_Transacciones Set Estado='T' , 
                                                Fecha_Emision = @fechaEmision,
                                                Ubicacion_Actual = 'T',
                                                FECHA_TRASLADO = @fechaEmision,
                                                NDocumento = @nDocumento,
                                                user_genera = @usuario,
                                                documento_base = @bancoConsec,
                                                COD_PLAN = @plan
                                                 Where NSolicitud= @nSolicitud";

                        conn.Execute(queryUpdate, new
                        {
                            fechaEmision = fecha,
                            nDocumento = vDocumento,
                            usuario = transferencia.usuario,
                            bancoConsec = transferencia.bancoConsec,
                            plan = transferencia.plan,
                            nSolicitud = item.nSolicitud
                        });

                        //Bitacora Especial
                        var qryBitacora = $"exec spTesBitacora @Solicitud, '10', @Detalle ,@Usuario ";
                       
                        conn.Execute(qryBitacora, new
                        {
                            Solicitud = item.nSolicitud,
                            Detalle = $"Transferencia...:{transferencia.bancoConsec}",
                            Usuario = transferencia.usuario
                        });

                        //'Afecta Saldo en Bancos
                        var qrySaldos = $"exec spTESAfectaBancos @NSOLICITUD , 'E'";
                        conn.Execute(qrySaldos, new { NSOLICITUD = item.nSolicitud });

                        //Actualiza Cuentas Corrientes
                        if (item.modulo == "CC" && item.subModulo == "C")
                        {
                            ActualizaReferencia(conn, vDocumento, item);
                        }

                    }

                    ActualizaTesBancosDocsConse(conn, consc, transferencia);
                }

                //sale con reportes

                return DbHelper.OkResponse("Transferencias Aceptadas Correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }


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
