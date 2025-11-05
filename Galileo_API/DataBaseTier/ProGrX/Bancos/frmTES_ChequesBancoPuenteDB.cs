using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models.TES;
using System.Reflection;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_ChequesBancoPuenteDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;
        private readonly mSecurityMainDb _Security_MainDB;
        private int vModulo = 9;
        private string AppVersion;

        public frmTES_ChequesBancoPuenteDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(config);
            _Security_MainDB = new mSecurityMainDb(config);
            AppVersion = _config.GetSection("AppSettings").GetSection("AppVersion").Value.ToString();
        }

        public ErrorDTO<List<DropDownListaGenericaModel>> TES_BancosGestion_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
        }

        public ErrorDTO<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select id_banco as item,descripcion from tes_bancos where estado = 'A' and puente  = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        private ErrorDTO<string> TES_CuentaBanco(int CodEmpresa, int id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<string>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ctaconta as Cuenta from tes_bancos where id_banco = @banco ";
                    response.Result = connection.Query<string>(query,
                        new { banco = id_banco }
                        ).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDTO TES_ChequesBanco_Aplica(int CodEmpresa, int id_banco, int banco, string usuario ,List<ChequesBancoPuenteData> data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in data)
                    {
                       if(id_banco == banco)
                       {
                            response.Code = -1;
                            response.Description += "- Banco de traslado es igual al banco puente ";
                            return response;
                        }

                        string vCuentaI = TES_CuentaBanco(CodEmpresa, id_banco).Result;
                        string vCuentaD = TES_CuentaBanco(CodEmpresa, banco).Result; ;

                        var query = $@"select estado_asiento from cheques where nsolicitud = @nsolicitud ";
                        var estado = connection.Query<string>(query, new { nsolicitud = item.nsolicitud }).FirstOrDefault();

                        if (estado == "G")
                        {
                            response.Code = -1;
                            response.Description += "El asiento de esta solicitud ya fue generado, no se puede reclasificar...";
                            return response;
                        }

                        var updateQuery = $@"update cheques set 
                                                    id_banco = @banco
                                                    where nsolicitud = @solicitud ";
                        connection.Execute(updateQuery, new { banco = banco, solicitud = item.nsolicitud });

                        updateQuery = $@"update ck_detalle set 
                                                cuenta_contable = @cuentaD
                                                where cuenta_contable = @cuentaI 
                                                and nsolicitud = @solicitud ";

                        connection.Execute(updateQuery, new { cuentaD = vCuentaD, cuentaI = vCuentaI, solicitud = item.nsolicitud });

                        //Bitácora
                        _Security_MainDB.Bitacora
                            (new BitacoraInsertarDTO
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = usuario,
                                DetalleMovimiento = $"Cambia de Banco Solicitud N. {item.nsolicitud} ",
                                Movimiento = "Modifica - WEB",
                                Modulo = vModulo
                            }); 

                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDTO<List<ChequesBancoPuenteData>> TES_ChequePuenteLista_Obtener(int CodEmpresa, int id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<ChequesBancoPuenteData>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select 0 as control ,nsolicitud,codigo,beneficiario,monto,fecha_solicitud 
                                    from cheques 
                                    where id_banco = @banco and 
                                    ESTADO = 'P' and tipo = 'CK'";
                    response.Result = connection.Query<ChequesBancoPuenteData>(query,
                        new
                        {
                            banco = id_banco
                        }).ToList();
                }
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
