using Dapper;
using Microsoft.Data.SqlClient;
using PdfSharp.Pdf.Filters;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_PlanMutualDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_PlanMutualDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Método para obtener la lista de planes de mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PlanMutualLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select COD_PLAN as 'item',  RTRIM(DESCRIPCION) as 'descripcion'
		                        from AFI_PLAN_MUTUAL where ACTIVO = 1
		                        order by COD_PLAN";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para obtener las personas asociadas a un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AfPlanPersonaslLista> AF_PlanMutualPersonas_Obtener(int CodEmpresa, string plan, string estado, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<AfPlanPersonaslLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfPlanPersonaslLista()
                {
                    total = 0,
                    lista = new List<AfPlanMutualPersonasData>()
                }
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    //Consulta Anterior
                    //var query = @$"exec spAFI_PM_Consulta '{filtros.plan}', '{filtros.cedula}', '{filtros.idAlterna}', '{filtros.nombre}', '{filtros.estado}', {filtros.lineas} ";

                    //Nuevo SP con paginación
                    var query = @$"spAFI_W_PM_Consulta";
                    var parameters = new
                    {
                        Plan = plan,
                        FiltroBusqueda = filtro.filtro,
                        Filtro = estado,
                        SortField = filtro.sortField,
                        SortOrder = filtro.sortOrder,
                        Pagina = filtro.pagina,
                        Paginacion = filtro.paginacion
                    };

                   using var multi = connection.QueryMultiple(query, parameters, commandType: System.Data.CommandType.StoredProcedure);
                    // Primer resultset: total
                    result.Result.total = multi.Read<int>().FirstOrDefault();

                    // Segundo resultset: lista de personas
                    result.Result.lista = multi.Read<AfPlanMutualPersonasData>().ToList();

                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new AfPlanPersonaslLista();
            }
            return result;
        }

        /// <summary>
        /// Metodo para exportar la lista de personas asociadas a un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plan"></param>
        /// <param name="estado"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public ErrorDto<List<AfPlanMutualPersonasData>> AF_PlanMutualPersonas_Exportar(int CodEmpresa, string plan, string estado, int total)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<AfPlanMutualPersonasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfPlanMutualPersonasData>()
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    //Consulta Anterior
                    var query = @$"exec spAFI_PM_Consulta '{plan}', '', '', '', '{estado}', {total} ";
                    result.Result = connection.Query<AfPlanMutualPersonasData>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<AfPlanMutualPersonasData>();
            }
            return result;
        }

        /// <summary>
        /// Método para obtener la lista de planes de mutual con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AfPlanMutualLista> AF_PlanMutual_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<AfPlanMutualLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AfPlanMutualLista()
                {
                    total = 0,
                    lista = new List<AfPlanMutualDto>()
                }
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = @" select COUNT(*) 
                                from AFI_PLAN_MUTUAL ";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro.filtro != null && filtro.filtro != "")
                    {
                        filtro.filtro = $@"WHERE ( 
                                                 COD_PLAN like '%{filtro.filtro}%' 
                                              OR DESCRIPCION like '%{filtro.filtro}%'
                                              OR CODIGIO_RETENCION like '%{filtro.filtro}%'
                                              OR REGISTRO_USUARIO like '%{filtro.filtro}%' 
                                          )";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "COD_PLAN";
                    }

                    if (filtro.sortOrder == 0)
                    {
                        filtro.sortOrder = 1; //Por defecto orden ascendente
                    }

                    if (filtro.pagina != null)
                    {
                        query = @$"Select COD_PLAN, DESCRIPCION, MONTO, CODIGIO_RETENCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO
		                                from AFI_PLAN_MUTUAL {filtro.filtro}
		                                order by {filtro.sortField} {(filtro.sortOrder == -1 ? "ASC" : "DESC")}
                                        OFFSET {filtro.pagina} ROWS
                                        FETCH NEXT {filtro.paginacion} ROWS ONLY ";
                        result.Result.lista = connection.Query<AfPlanMutualDto>(query).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Método para guardar la información de una persona en un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plan"></param>
        /// <param name="usuario"></param>
        /// <param name="persona"></param>
        /// <returns></returns>
        public ErrorDto AF_PlanMutualPersona_Guardar(int CodEmpresa, string plan, string usuario, AfPlanMutualPersonasData persona)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    int excluye = persona.excluye ? 1 : 0;
                    var query = @$"exec spAFI_PM_Excluye '{plan}', '{persona.cedula}', {excluye}, '{usuario.ToUpper()}' ";
                    connection.Execute(query);
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para guardar la información de un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto AF_PlanMutual_Guardar(int CodEmpresa, string usuario, AfPlanMutualDto plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    int activo = plan.activo ? 1 : 0;
                    var query = @$"exec spAFI_PM_Registro '{plan.cod_plan}', '{plan.descripcion}', {plan.monto}, '{plan.codigio_retencion}', {activo}, '{usuario}','A' ";
                    connection.Execute(query);

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Plan Mutual/Beneficios : {plan.cod_plan}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }

        /// <summary>
        /// Metodo para eliminar un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto AF_PlanMutual_Eliminar(int CodEmpresa, string usuario, string plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"exec spAFI_PM_Registro '{plan}', '', 0, '', 0, '{usuario}', 'E'";
                    connection.Execute(query);
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Plan Mutual/Beneficios : {plan}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Metodo para actualizar los recaudos de un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto AF_PlanMutual_Actualizar(int CodEmpresa, string usuario, string plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"exec spAFI_PM_Recaudos_Update '{plan}', '{usuario}' ";
                    connection.Execute(query);
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Plan Mutual/Beneficios : {plan} , Actualización de Recaudos",
                        Movimiento = "Aplica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }
    }
}
