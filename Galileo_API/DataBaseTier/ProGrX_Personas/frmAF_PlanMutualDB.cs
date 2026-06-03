using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFPlanMutualDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFPlanMutualDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método para obtener la lista de planes de mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PlanMutualLista_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select COD_PLAN as item, RTRIM(DESCRIPCION) as descripcion
                  from AFI_PLAN_MUTUAL where ACTIVO = 1
                  order by COD_PLAN");
        }

        /// <summary>
        /// Método para obtener las personas asociadas a un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AfPlanPersonaslLista> AF_PlanMutualPersonas_Obtener(int CodEmpresa, string plan, string estado, FiltrosLazyLoadData filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de personas del plan mutual son requeridos.", -2, new AfPlanPersonaslLista
                {
                    total = 0,
                    lista = new List<AfPlanMutualPersonasData>()
                });
            }

            var resultadoVacio = new AfPlanPersonaslLista
            {
                total = 0,
                lista = new List<AfPlanMutualPersonasData>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                using var multi = connection.QueryMultiple(
                    "spAFI_W_PM_Consulta",
                    new
                    {
                        Plan = plan,
                        FiltroBusqueda = filtro.filtro,
                        Filtro = estado,
                        SortField = filtro.sortField,
                        SortOrder = filtro.sortOrder,
                        Pagina = filtro.pagina,
                        Paginacion = filtro.paginacion
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return new AfPlanPersonaslLista
                {
                    total = multi.Read<int>().FirstOrDefault(),
                    lista = multi.Read<AfPlanMutualPersonasData>().ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener personas asociadas al plan mutual.", result.Code.GetValueOrDefault(-1), resultadoVacio);
        }

        /// <summary>
        /// Método para exportar la lista de personas asociadas a un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plan"></param>
        /// <param name="estado"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public ErrorDto<List<AfPlanMutualPersonasData>> AF_PlanMutualPersonas_Exportar(int CodEmpresa, string plan, string estado, int total)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfPlanMutualPersonasData>(
                    "spAFI_PM_Consulta",
                    new
                    {
                        plan,
                        cedula = string.Empty,
                        idAlterna = string.Empty,
                        nombre = string.Empty,
                        estado,
                        lineas = total
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfPlanMutualPersonasData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al exportar personas asociadas al plan mutual.", result.Code.GetValueOrDefault(-1), new List<AfPlanMutualPersonasData>());
        }

        /// <summary>
        /// Método para obtener la lista de planes de mutual con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AfPlanMutualLista> AF_PlanMutual_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            if (filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros del plan mutual son requeridos.", -2, new AfPlanMutualLista
                {
                    total = 0,
                    lista = new List<AfPlanMutualDto>()
                });
            }

            var resultadoVacio = new AfPlanMutualLista
            {
                total = 0,
                lista = new List<AfPlanMutualDto>()
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var salida = new AfPlanMutualLista
                {
                    total = connection.QueryFirstOrDefault<int>("select COUNT(*) from AFI_PLAN_MUTUAL"),
                    lista = new List<AfPlanMutualDto>()
                };

                var filtroTexto = filtro.filtro?.Trim();
                var sortField = ObtenerSortFieldPlanMutual(filtro.sortField);
                var sortDirection = ObtenerSortDirectionPlanMutual(filtro.sortOrder);
                var offsetRows = filtro.pagina;
                var fetchRows = filtro.paginacion;

                var sql = @"
                    Select COD_PLAN, DESCRIPCION, MONTO, CODIGIO_RETENCION, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO
                    from AFI_PLAN_MUTUAL
                    where (
                        @Filtro is null
                        or COD_PLAN like @Filtro
                        or DESCRIPCION like @Filtro
                        or CODIGIO_RETENCION like @Filtro
                        or REGISTRO_USUARIO like @Filtro
                    )
                    order by
                        CASE WHEN @SortField = 'COD_PLAN' AND @SortDirection = 'ASC' THEN COD_PLAN END ASC,
                        CASE WHEN @SortField = 'COD_PLAN' AND @SortDirection = 'DESC' THEN COD_PLAN END DESC,
                        CASE WHEN @SortField = 'DESCRIPCION' AND @SortDirection = 'ASC' THEN DESCRIPCION END ASC,
                        CASE WHEN @SortField = 'DESCRIPCION' AND @SortDirection = 'DESC' THEN DESCRIPCION END DESC,
                        CASE WHEN @SortField = 'MONTO' AND @SortDirection = 'ASC' THEN MONTO END ASC,
                        CASE WHEN @SortField = 'MONTO' AND @SortDirection = 'DESC' THEN MONTO END DESC,
                        CASE WHEN @SortField = 'CODIGIO_RETENCION' AND @SortDirection = 'ASC' THEN CODIGIO_RETENCION END ASC,
                        CASE WHEN @SortField = 'CODIGIO_RETENCION' AND @SortDirection = 'DESC' THEN CODIGIO_RETENCION END DESC,
                        CASE WHEN @SortField = 'ACTIVO' AND @SortDirection = 'ASC' THEN CAST(ACTIVO AS INT) END ASC,
                        CASE WHEN @SortField = 'ACTIVO' AND @SortDirection = 'DESC' THEN CAST(ACTIVO AS INT) END DESC,
                        CASE WHEN @SortField = 'REGISTRO_FECHA' AND @SortDirection = 'ASC' THEN REGISTRO_FECHA END ASC,
                        CASE WHEN @SortField = 'REGISTRO_FECHA' AND @SortDirection = 'DESC' THEN REGISTRO_FECHA END DESC,
                        CASE WHEN @SortField = 'REGISTRO_USUARIO' AND @SortDirection = 'ASC' THEN REGISTRO_USUARIO END ASC,
                        CASE WHEN @SortField = 'REGISTRO_USUARIO' AND @SortDirection = 'DESC' THEN REGISTRO_USUARIO END DESC,
                        COD_PLAN ASC";

                if (fetchRows > 0)
                {
                    sql += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                salida.lista = connection.Query<AfPlanMutualDto>(sql, parametros).ToList();
                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? resultadoVacio)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener planes mutuales.", result.Code.GetValueOrDefault(-1), resultadoVacio);
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
            if (persona is null)
            {
                return DbHelper.ErrorResponse("Los datos de la persona son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spAFI_PM_Excluye",
                    new
                    {
                        plan,
                        cedula = persona.cedula,
                        excluye = persona.excluye ? 1 : 0,
                        usuario = usuario.ToUpper()
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar la persona del plan mutual.", result.Code.GetValueOrDefault(-1));
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
            if (plan is null)
            {
                return DbHelper.ErrorResponse("Los datos del plan mutual son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spAFI_PM_Registro",
                    new
                    {
                        cod_plan = plan.cod_plan,
                        descripcion = plan.descripcion,
                        monto = plan.monto,
                        codigio_retencion = plan.codigio_retencion,
                        activo = plan.activo ? 1 : 0,
                        usuario,
                        accion = "A"
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al guardar el plan mutual.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Plan Mutual/Beneficios : {plan.cod_plan}", "Registra - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Método para eliminar un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto AF_PlanMutual_Eliminar(int CodEmpresa, string usuario, string plan)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spAFI_PM_Registro",
                    new
                    {
                        cod_plan = plan,
                        descripcion = string.Empty,
                        monto = 0,
                        codigio_retencion = string.Empty,
                        activo = 0,
                        usuario,
                        accion = "E"
                    },
                    commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar el plan mutual.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Plan Mutual/Beneficios : {plan}", "Elimina - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Método para actualizar los recaudos de un plan mutual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto AF_PlanMutual_Actualizar(int CodEmpresa, string usuario, string plan)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spAFI_PM_Recaudos_Update",
                    new { plan, usuario },
                    commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al actualizar recaudos del plan mutual.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(CodEmpresa, usuario, $"Plan Mutual/Beneficios : {plan} , Actualización de Recaudos", "Aplica - WEB");
            return DbHelper.OkResponse("Ok");
        }


        private static string ObtenerSortFieldPlanMutual(string? sortField)
        {
            return sortField switch
            {
                "COD_PLAN" => "COD_PLAN",
                "DESCRIPCION" => "DESCRIPCION",
                "MONTO" => "MONTO",
                "CODIGIO_RETENCION" => "CODIGIO_RETENCION",
                "ACTIVO" => "ACTIVO",
                "REGISTRO_FECHA" => "REGISTRO_FECHA",
                "REGISTRO_USUARIO" => "REGISTRO_USUARIO",
                _ => "COD_PLAN"
            };
        }

        private static string ObtenerSortDirectionPlanMutual(int sortOrder)
        {
            return sortOrder == -1 ? "ASC" : "DESC";
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
