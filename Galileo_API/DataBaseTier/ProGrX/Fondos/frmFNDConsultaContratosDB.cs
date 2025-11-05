using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Credito;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDConsultaContratosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; // Modulo de Fondos
        private readonly mAfilicacionDB mAfilicacion;
        private readonly mProGrX_AuxiliarDB mProGrX_Auxiliar;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmFNDConsultaContratosDB(IConfiguration? config)
        {
            _config = config;
            mAfilicacion = new mAfilicacionDB(_config);
            mProGrX_Auxiliar = new mProGrX_AuxiliarDB(_config);
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta los socios disponibles para el formulario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<CrConsultaCrdSociosData>> FND_ConsultaContratosSocios_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CrConsultaCrdSociosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCrdSociosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select cedula,cedular,nombre from SOCIOS";
                    response.Result = connection.Query<CrConsultaCrdSociosData>(query).ToList();
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
        /// Método para obtener los contratos de un socio según su cédula y la opción seleccionada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="vUsuario"></param>
        /// <param name="opcion"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaContratosData>> FND_ConsultaContratos_Contratos_Obtener(int CodEmpresa, string vCedula, string vUsuario ,string opcion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaContratosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaContratosData>()
            };

            try
            {
                string nombre = mAfilicacion.fxNombre(CodEmpresa, vCedula);
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";

                    switch(opcion)
                    {
                        case "1": //Activos
                            where = " and  F.estado = 'A' order by F.Fecha_Inicio desc,F.cod_Plan,F.cod_Contrato";
                            break;
                        case "2":
                            where = " and F.estado = 'L' order by F.Liq_Fecha desc ";
                            break;
                        case "3":
                            where = " and F.estado = 'I' order by F.Fecha_Inicio desc,F.cod_Plan,F.cod_Contrato";
                            break;
                        case "4":
                            where = " and F.estado = 'B' order by F.Fecha_Inicio desc,F.cod_Plan,F.cod_Contrato";
                            break;
                        case "5":
                            where = " and F.estado = 'C' order by F.Fecha_Inicio desc,F.cod_Plan,F.cod_Contrato";
                            break;
                        default:
                            where = "  and  F.estado = 'A' order by F.Fecha_Inicio desc,F.cod_Plan,F.cod_Contrato";
                            break;
                    }

                    var query = $@"Select S.Nombre,O.Descripcion,P.Descripcion as DPlan,F.Cod_Operadora
                                   ,F.Cod_plan,F.cod_Contrato,F.Estado,F.Liq_Fecha
                                   ,F.Fecha_Inicio,F.Monto,F.Plazo,F.Renueva,F.Inc_Anual,F.Inc_Tipo,F.Aportes
                                   ,F.Rendimiento,F.Operacion,F.Monto_Transito
                                    From Socios S
                                    inner join Fnd_Contratos F on S.Cedula = F.Cedula
                                    inner join Fnd_operadoras O on F.cod_operadora = O.cod_operadora
                                    inner join Fnd_planes P on F.Cod_plan = P.Cod_plan
                                    Where S.cedula= @cedula
                                    AND dbo.fxFndColaboradorVisualiza(F.COD_OPERADORA, F.COD_PLAN, F.cedula,S.EstadoActual, @usuario) = 1
                                    {where}";
                    response.Result = connection.Query<FndConsultaContratosData>(query, new { cedula = vCedula, usuario = vUsuario }).ToList();

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
        /// Método para obtener los subcontratos de un contrato específico
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="cod_plan"></param>
        /// <param name="cod_contrato"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaSubContratosData>> FND_ConsultaContratos_SubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, string cod_contrato) 
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaSubContratosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaSubContratosData>()
            };

            try
            {
               
                using var connection = new SqlConnection(stringConn);
                {
                    //obtengo la operadora del socio
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";

                    query = $@"select * from fnd_subCuentas where cod_operadora = @operadora and cod_plan = @cod_plan  and cod_contrato = @cod_contrato";
                    response.Result = connection.Query<FndConsultaSubContratosData>(query, new { 
                        operadora = cod_operadora, 
                        cod_plan = cod_plan, 
                        cod_contrato = cod_contrato }).ToList();

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
        /// Método para obtener las liquidaciones de un socio según su cédula
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaLiquidacionesData>> FND_ConsultaContratos_Liquidaciones_Obtener(int CodEmpresa, string vCedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaLiquidacionesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaLiquidacionesData>()
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.cod_plan,P.descripcion,C.cod_contrato,L.consec,L.fecha,L.usuario,L.aportes_liq+L.rendi_liq as 'Monto'
                                   ,L.traspaso_tesoreria,L.Traspaso_usuario,L.Solicitud_Tesoreria,isnull(L.Estado,'P') as 'Estado'
                                    from fnd_contratos C inner join fnd_liquidacion L on C.cod_operadora = L.cod_operadora
                                    and C.cod_plan = L.cod_plan and C.cod_Contrato = L.cod_contrato
                                    inner join fnd_planes P on C.cod_plan = P.cod_plan and P.cod_operadora = C.cod_operadora
                                    Where C.cedula = @cedula order by L.consec desc";
                    response.Result = connection.Query<FndConsultaLiquidacionesData>(query, new { cedula = vCedula }).ToList();

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
        /// Método para obtener los movimientos de un contrato específico
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <param name="contrato"></param>
        /// <param name="cod_plan"></param>
        /// <param name="chkTodas"></param>
        /// <returns></returns>
        public ErrorDTO<List<FndConsultaMovimientosData>> FND_ConsultaContratos_Movimiento_Obtener(
            int CodEmpresa, 
            string vCedula,
            FndConsultaMovimientosParams filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FndConsultaMovimientosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndConsultaMovimientosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";

                    if(filtros.plan != null && filtros.plan.Trim() != "")
                    {
                        where += " And D.cod_plan = @cod_plan ";
                    }

                    if(filtros.contrato != null && filtros.contrato.Trim() != "")
                    {
                        where += " And D.Cod_Contrato= @contrato ";
                    }

                    if (!filtros.chkTodas)
                    {
                        where += " And D.Fecha between CAST(@fechaDesde AS DATETIME) AND CAST(@fechaHasta AS DATETIME)   ";
                    }

                    string vFechaIni = mProGrX_Auxiliar.validaFechaGlobal(filtros.fechaInicio);
                    string vFechaCorte = mProGrX_Auxiliar.validaFechaGlobal(filtros.fechaCorte);

                    //obtengo la operadora del socio
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";


                    query = $@"Select D.cod_fnd_Detalle,D.Monto,D.Fecha_Proceso,D.Fecha,isnull(Doc.descripcion,'')  as descripcion_mov,D.nCon,D.Fecha_Acredita,D.cod_contrato, D.Cod_plan,P.descripcion 
                                   from fnd_contratos_detalle D  inner join  fnd_planes P on D.cod_plan = P.cod_plan 
                                   inner join fnd_contratos C on D.cod_plan = C.cod_plan and D.cod_contrato = C.cod_contrato 
                                   left join SIF_Documentos Doc on D.Tcon = Doc.Tipo_Documento
                                   where D.cod_operadora = @operadora and C.cedula = @cedula {where} order by D.Fecha desc";
                    response.Result = connection.Query<FndConsultaMovimientosData>(query, new {
                        operadora = cod_operadora,
                        cedula = vCedula, 
                        cod_plan = filtros.plan,
                        contrato = filtros.contrato,
                        fechaDesde = vFechaIni,
                        fechaHasta = vFechaCorte
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

        /// <summary>
        /// Metodo para obtener los planes asociados a la operadora del socio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vCedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_ConsultaContratos_Planes_Obtener(int CodEmpresa, string vCedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    //obtengo la operadora del socio
                    var query = $@"select cod_operadora from fnd_contratos where cedula = @cedula";
                    string cod_operadora = connection.QueryFirstOrDefault<string>(query, new { cedula = vCedula }) ?? "";

                    query = $@"select cod_plan as item,descripcion from fnd_Planes where Cod_operadora= @operadora";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { operadora = cod_operadora }).ToList();

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
        /// Método para reversar una liquidación
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="boleta"></param>
        /// <returns></returns>
        public ErrorDTO FND_ConsultaContratos_Reversar(int CodEmpresa, string usuario, string boleta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFndReversaLiq @boleta, @usuario";
                    connection.Execute(query, new { boleta = boleta, usuario = usuario });

                    //Bitacora
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Reversión de la Liquidación No.: {boleta}",
                        Movimiento = "Aplica - WEB",
                        Modulo = vModulo
                    });
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
