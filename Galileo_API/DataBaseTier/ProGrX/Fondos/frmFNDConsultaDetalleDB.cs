using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Credito;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDConsultaDetalleDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; // Modulo de Fondos

        public frmFNDConsultaDetalleDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo que obtiene el detalle cabecera del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDTO<FndConsultaDetalleData> FndConsultaDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<FndConsultaDetalleData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndConsultaDetalleData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //obtengo la operadora del socio
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";

                    query = $@"select C.*, C.Aportes + C.Rendimiento - isnull(C.Monto_Transito,0) as 'Disponible'
                                    ,S.nombre,O.descripcion as Operadora,P.descripcion as PlanX
                                    from fnd_contratos C inner join Socios S on C.cedula = S.cedula
                                    inner join fnd_planes P on C.cod_plan = P.cod_plan and C.cod_operadora = P.cod_operadora
                                    inner join fnd_operadoras O on C.cod_operadora = O.cod_operadora
                                    where C.cod_operadora = @operadora
                                    and C.cod_plan = @codPlan and C.cod_contrato = @contrato ";
                    response.Result = connection.Query<FndConsultaDetalleData>(query,
                        new {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato
                        }
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

        /// <summary>
        /// Metodo que obtiene el detalle de movimientos del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaContratoDetallesData>> FndConsultaContratos_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaContratoDetallesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaContratoDetallesData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //obtengo la operadora del socio
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";

                    query = $@"Select Det.*,isnull(Doc.Descripcion,'') as 'DocDesc', isnull(Con.Descripcion,'') as 'ConceptoDesc'
                                  from fnd_contratos_detalle Det left join SIF_DOCUMENTOS Doc on Det.Tcon = Doc.Tipo_Documento
                                  left join SIF_Conceptos Con on Det.Cod_Concepto = Con.Cod_Concepto
                                  where Det.cod_operadora = @operadora
                                  And Det.cod_plan = @codPlan and Det.Cod_Contrato = @contrato
                                  order by Det.cod_fnd_detalle desc";
                    response.Result = connection.Query<FndConsultaContratoDetallesData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato
                        }
                        ).ToList();
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

        /// <summary>
        /// Metodo que obtiene el detalle de subcuentas del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaSubCuentasData>> FndConsultaSubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaSubCuentasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaSubCuentasData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //obtengo la operadora del socio
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";

                    query = $@" select * from fnd_subCuentas  where cod_operadora = @operadora
                                  And cod_plan= @codPlan and Cod_Contrato = @contrato
                                  and IdX = @subCuenta";
                    response.Result = connection.Query<FndConsultaSubCuentasData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato,
                            subCuenta = subCuenta
                        }
                        ).ToList();
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

        /// <summary>
        /// Metodo que obtiene el detalle de movimientos de una subcuenta del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaSubCuentasDetalleData>> FndConsultaSubCuentasDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaSubCuentasDetalleData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaSubCuentasDetalleData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select count(*) from fnc_ValidaAccesoModulo(@cedula,@modulo) ";
                    int cod_operadora = connection.QueryFirstOrDefault<int>(query, new { cedula = vCedula, modulo = vModulo });

                    query = $@"Select Det.*,isnull(Doc.Descripcion,'') as 'DocDesc', '' as 'ConceptoDesc'
                                 ,'' as 'Usuario'
                                  from fnd_SubCuentas_detalle Det left join SIF_DOCUMENTOS Doc on Det.Tcon = Doc.Tipo_Documento
                                  where Det.cod_operadora = @operadora
                                  And Det.cod_plan = @codPlan and Det.Cod_Contrato = @contrato
                                  and Det.IDx = @subCuenta
                                  order by Det.cod_fnd_detalle desc";
                    response.Result = connection.Query<FndConsultaSubCuentasDetalleData>(query,
                         new
                         {
                             operadora = cod_operadora,
                             codPlan = cod_plan,
                             contrato = cod_contrato,
                             subCuenta = subCuenta
                         }).ToList();
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

        /// <summary>
        /// Metodo que obtiene el detalle de beneficiarios del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaBeneficiarioDetalle>> FndConsultaContratosBeneficiario_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaBeneficiarioDetalle>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaBeneficiarioDetalle>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //NOTA: COnsultar por esa funcion
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";

                    query = $@"Select CedulaBn,Nombre,Porcentaje,parentesco,fechanac From FND_CONTRATOS_BENEFICIARIOS where 
                                Cedula = @cedula and cod_contrato = @contrato
                                and cod_operadora = @operadora
                                and cod_plan= @codPlan";
                    response.Result = connection.Query<FndConsultaBeneficiarioDetalle>(query,
                         new
                         {
                             cedula = vCedula,
                             operadora = cod_operadora,
                             codPlan = cod_plan,
                             contrato = cod_contrato,
                         }).ToList();
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

        /// <summary>
        /// Metodo que obtiene el detalle de movimientos en transito del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaMovTransitoData>> FndConsultaMovTransito_Obtener(int CodEmpresa, string cuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaMovTransitoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaMovTransitoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFndSinpeMovTransito @cuentaCliente ";
                    
                    response.Result = connection.Query<FndConsultaMovTransitoData>(query,
                         new
                         {
                             cuentaCliente = cuenta
                         }).ToList();
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

    }
}
