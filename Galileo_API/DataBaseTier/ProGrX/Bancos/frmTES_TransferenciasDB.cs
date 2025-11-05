using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models.TES;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_TransferenciasDB
    {
        private readonly IConfiguration? _config;
        private mProGrX_AuxiliarDB _utils;
        private mTesoreria _mTesoreria;
        private int modulo = 9;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmTES_TransferenciasDB(IConfiguration config)
        {
            _config = config;
            _utils = new mProGrX_AuxiliarDB(config);
            _mTesoreria = new mTesoreria(config);
            _Security_MainDB = new mSecurityMainDb(config);
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
                        new { 
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
                            if(consc == 0)
                            {
                               var conseR = _mTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, transferencia.id_Banco, transferencia.tipoDoc, "+", transferencia.plan);
                               if(conseR.Code != -1)
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

                            var queryUpdate = $@"Update Tes_Transacciones Set Estado='T' , 
                                                Fecha_Emision = '{fecha}',
                                                Ubicacion_Actual = 'T',
                                                FECHA_TRASLADO = '{fecha}',
                                                NDocumento = '{vDocumento}',
                                                user_genera = '{transferencia.usuario}',
                                                documento_base = '{transferencia.bancoConsec}', 
                                                COD_PLAN = '{transferencia.plan}'
                                                 Where NSolicitud= {item.nSolicitud}";

                            var resultUpdate = connection.Execute(queryUpdate);

                            //Bitacora Especial
                            var qryBitacora = $"exec spTesBitacora {item.nSolicitud}, '10','Transferencia...:{transferencia.bancoConsec}', '{transferencia.usuario}' ";
                            var resultBitacora = connection.Execute(qryBitacora);

                            //'Afecta Saldo en Bancos
                            var qrySaldos = $"exec spTESAfectaBancos {item.nSolicitud}, 'E'";
                            var resultSaldos = connection.Execute(qrySaldos);

                            //Actualiza Cuentas Corrientes
                            if(item.modulo == "CC" && item.subModulo == "C")
                            {
                                var QueryCC = "";
                                if (item.detalle1 != null || item.detalle1 != "")
                                {
                                    if(item.referencia != null)
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


                        //'Actualiza Consecutivo Interno
                        var queryConsec = $@"update tes_banco_docs set CONSECUTIVO_DET = {consc} 
                                         where Tipo = '{transferencia.tipoDoc}' and id_banco = {transferencia.id_Banco}";
                        var resultConsec = connection.Execute(queryConsec);
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
