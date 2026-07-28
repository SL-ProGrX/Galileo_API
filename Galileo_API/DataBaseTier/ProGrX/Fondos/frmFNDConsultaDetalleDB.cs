using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo_API.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndConsultaDetalleDB
    {
        private readonly int vModulo = 18; // Módulo de Fondos
        private readonly PortalDB _portalDb;

        public FrmFndConsultaDetalleDB(IConfiguration? config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Método que obtiene el detalle cabecera del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<FndConsultaDetalleData> FndConsultaDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                //obtengo la operadora del socio
                const string qryOperadora = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qryOperadora, new { cedula = vCedula }) ?? "";

                const string query = @"
                        select C.*, C.Aportes + C.Rendimiento - isnull(C.Monto_Transito,0) as 'Disponible'
                                    ,S.nombre,O.descripcion as Operadora,P.descripcion as PlanX
                                    from fnd_contratos C inner join Socios S on C.cedula = S.cedula
                                    inner join fnd_planes P on C.cod_plan = P.cod_plan and C.cod_operadora = P.cod_operadora
                                    inner join fnd_operadoras O on C.cod_operadora = O.cod_operadora
                                    where C.cod_operadora = @operadora
                                    and C.cod_plan = @codPlan and C.cod_contrato = @contrato ";

                return conn.QueryFirstOrDefault<FndConsultaDetalleData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato
                        }) ?? new FndConsultaDetalleData();
            });
        }

        /// <summary>
        /// Método que obtiene el detalle de movimientos del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaContratoDetallesData>> FndConsultaContratos_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                //obtengo la operadora del socio
                const string qryOperadora = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qryOperadora, new { cedula = vCedula }) ?? "";

                const string query = @"
                        Select Det.*,isnull(Doc.Descripcion,'') as 'DocDesc', isnull(Con.Descripcion,'') as 'ConceptoDesc'
                                  from fnd_contratos_detalle Det left join SIF_DOCUMENTOS Doc on Det.Tcon = Doc.Tipo_Documento
                                  left join SIF_Conceptos Con on Det.Cod_Concepto = Con.Cod_Concepto
                                  where Det.cod_operadora = @operadora
                                  And Det.cod_plan = @codPlan and Det.Cod_Contrato = @contrato
                                  order by Det.cod_fnd_detalle desc";

                return conn.Query<FndConsultaContratoDetallesData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato
                        }).ToList();
            });
        }

        /// <summary>
        /// Método que obtiene el detalle de subcuentas del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaSubCuentasData>> FndConsultaSubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                //obtengo la operadora del socio
                const string qryOperadora = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qryOperadora, new { cedula = vCedula }) ?? "";

                const string query = @"
                         select * from fnd_subCuentas  where cod_operadora = @operadora
                                  And cod_plan= @codPlan and Cod_Contrato = @contrato
                                  and IdX = @subCuenta";

                return conn.Query<FndConsultaSubCuentasData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato,
                            subCuenta = subCuenta
                        }).ToList();
            }); 
        }

        /// <summary>
        /// Método que obtiene el detalle de movimientos de una subcuenta del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaSubCuentasDetalleData>> FndConsultaSubCuentasDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var qryAcceso = $@"select count(*) from fnc_ValidaAccesoModulo(@cedula,@modulo) ";
                int cod_operadora = conn.QueryFirstOrDefault<int>(qryAcceso, new { cedula = vCedula, modulo = vModulo });

                const string query = @"
                        Select Det.*,isnull(Doc.Descripcion,'') as 'DocDesc', '' as 'ConceptoDesc'
                                 ,'' as 'Usuario'
                                  from fnd_SubCuentas_detalle Det left join SIF_DOCUMENTOS Doc on Det.Tcon = Doc.Tipo_Documento
                                  where Det.cod_operadora = @operadora
                                  And Det.cod_plan = @codPlan and Det.Cod_Contrato = @contrato
                                  and Det.IDx = @subCuenta
                                  order by Det.cod_fnd_detalle desc";

                return conn.Query<FndConsultaSubCuentasDetalleData>(query,
                        new
                        {
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato,
                            subCuenta = subCuenta
                        }).ToList();
            });
        }

        /// <summary>
        /// Método que obtiene el detalle de beneficiarios del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaBeneficiarioDetalle>> FndConsultaContratosBeneficiario_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var qry = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                string cod_operadora = conn.QueryFirstOrDefault<string>(qry, new { cedula = vCedula }) ?? "";

                const string query = @"
                        Select CedulaBn,Nombre,Porcentaje,parentesco,fechanac From FND_CONTRATOS_BENEFICIARIOS where 
                                Cedula = @cedula and cod_contrato = @contrato
                                and cod_operadora = @operadora
                                and cod_plan= @codPlan";

                return conn.Query<FndConsultaBeneficiarioDetalle>(query,
                        new
                        {
                            cedula = vCedula,
                            operadora = cod_operadora,
                            codPlan = cod_plan,
                            contrato = cod_contrato
                        }).ToList();
            });
        }

        /// <summary>
        /// Método que obtiene el detalle de movimientos en transito del contrato de fondos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<List<FndConsultaMovTransitoData>> FndConsultaMovTransito_Obtener(int CodEmpresa, string cuenta)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var query = $@"exec spFndSinpeMovTransito @cuentaCliente ";

                return conn.Query<FndConsultaMovTransitoData>(query,
                        new
                        {
                            cuentaCliente = cuenta
                        }).ToList();
            });
        }
    }
}