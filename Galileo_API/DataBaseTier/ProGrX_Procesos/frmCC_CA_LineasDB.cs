using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCcCaLineasDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        private const string CampoCodLinea = "Cod_Linea";
        private const string CampoDescripcion = "descripcion";
        private const string CampoCodPlan = "cod_plan";
        private const string TipoOrigenLinea = "linea";
        private const string TipoOrigenRemesa = "remesa";

        public FrmCcCaLineasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta el listado paginado de tipos de líneas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="filtros">Filtros, ordenamiento y paginación solicitados.</param>
        /// <returns>Listado paginado de tipos de líneas.</returns>
        public ErrorDto<CcCaLineasLista> CC_CA_Lineas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de líneas son requeridos.", -2, CrearResultadoLineasVacio());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();
                var filtroSql = string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%";
                var salida = new CcCaLineasLista
                {
                    total = connection.QueryFirstOrDefault<int>(
                        @"select COUNT(1)
                          from PRM_CA_LINEAS
                          where @Filtro is null
                             or Cod_Linea like @Filtro
                             or descripcion like @Filtro
                             or cod_plan like @Filtro",
                        new { Filtro = filtroSql },
                        commandTimeout: 0),
                    lista = new List<CcCaLineasData>()
                };

                var sortField = ObtenerSortFieldLineas(filtros.sortField);
                var sortDirection = ObtenerSortDirectionLineas(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                var query = @"
                    select Cod_Linea,
                           descripcion,
                           cod_plan,
                           activo
                    from PRM_CA_LINEAS
                    where (
                        @Filtro is null
                        or Cod_Linea like @Filtro
                        or descripcion like @Filtro
                        or cod_plan like @Filtro
                    )
                    order by
                        CASE WHEN @SortField = 'Cod_Linea' AND @SortDirection = 'ASC' THEN Cod_Linea END ASC,
                        CASE WHEN @SortField = 'Cod_Linea' AND @SortDirection = 'DESC' THEN Cod_Linea END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN descripcion END DESC,
                        CASE WHEN @SortField = 'cod_plan' AND @SortDirection = 'ASC' THEN cod_plan END ASC,
                        CASE WHEN @SortField = 'cod_plan' AND @SortDirection = 'DESC' THEN cod_plan END DESC,
                        Cod_Linea ASC";

                if (fetchRows > 0)
                {
                    query += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                salida.lista = connection.Query<CcCaLineasData>(
                    query,
                    new
                    {
                        Filtro = filtroSql,
                        SortField = sortField,
                        SortDirection = sortDirection,
                        OffsetRows = offsetRows,
                        FetchRows = fetchRows
                    },
                    commandTimeout: 0).ToList();

                return salida;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearResultadoLineasVacio())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar líneas.", result.Code.GetValueOrDefault(-1), CrearResultadoLineasVacio());
        }

        /// <summary>
        /// Inserta o actualiza un tipo de línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que realiza el movimiento.</param>
        /// <param name="request">Datos del tipo de línea.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CC_CA_Lineas_Guardar(int CodEmpresa, string usuario, CcCaLineasData request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la línea son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(
                    "select isnull(count(*),0) as Existe from PRM_CA_LINEAS where Cod_Linea = @cod_linea",
                    new { request.cod_linea },
                    commandTimeout: 0);

                if (request.isNew)
                {
                    return existe > 0
                        ? DbHelper.ErrorResponse($"La línea con el código {request.cod_linea} ya existe.", -2)
                        : CC_CA_Lineas_Insertar(connection, CodEmpresa, usuario, request);
                }

                return existe == 0
                    ? DbHelper.ErrorResponse($"La línea con el código {request.cod_linea} no existe.", -2)
                    : CC_CA_Lineas_Actualizar(connection, CodEmpresa, usuario, request);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar línea.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza un tipo de línea existente.
        /// </summary>
        /// <param name="connection">Conexión abierta por DbHelper.</param>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que realiza el movimiento.</param>
        /// <param name="datos">Datos actualizados de la línea.</param>
        /// <returns>Resultado de la actualización.</returns>
        private ErrorDto CC_CA_Lineas_Actualizar(SqlConnection connection, int CodEmpresa, string usuario, CcCaLineasData datos)
        {
            connection.Execute(
                @"update PRM_CA_LINEAS
                  set descripcion = @descripcion,
                      Cod_Plan = @cod_plan,
                      Activo = @activo
                  where Cod_Linea = @cod_linea",
                new
                {
                    datos.cod_linea,
                    datos.descripcion,
                    datos.cod_plan,
                    datos.activo,
                    usuario
                },
                commandTimeout: 0);

            RegistrarBitacora(CodEmpresa, usuario, $"Cargo Automatico - Tipo Linea: {datos.cod_linea}", "Modifica - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Inserta un nuevo tipo de línea.
        /// </summary>
        /// <param name="connection">Conexión abierta por DbHelper.</param>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que realiza el movimiento.</param>
        /// <param name="datos">Datos de la nueva línea.</param>
        /// <returns>Resultado de la inserción.</returns>
        private ErrorDto CC_CA_Lineas_Insertar(SqlConnection connection, int CodEmpresa, string usuario, CcCaLineasData datos)
        {
            connection.Execute(
                @"insert into PRM_CA_LINEAS(Cod_Linea, descripcion, cod_plan, Activo, Registro_Usuario, Registro_Fecha)
                  values(@cod_linea, @descripcion, @cod_plan, @activo, @usuario, Getdate())",
                new
                {
                    datos.cod_linea,
                    datos.descripcion,
                    datos.cod_plan,
                    datos.activo,
                    usuario
                },
                commandTimeout: 0);

            RegistrarBitacora(CodEmpresa, usuario, $"Cargo Automatico - Tipo Linea: {datos.cod_linea}", "Registra - WEB");
            return DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Elimina un tipo de línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="Usuario">Usuario que realiza el movimiento.</param>
        /// <param name="cod_Linea">Código de la línea que se elimina.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto CC_CA_CatalogoLineas_Delete(int CodEmpresa, string Usuario, string cod_Linea)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "delete PRM_CA_LINEAS where Cod_Linea = @cod_Linea",
                new { cod_Linea });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar tipo de línea.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(CodEmpresa, Usuario, $"Cargo Automatico - Tipo Linea: {cod_Linea}", "Elimina - WEB");
                return DbHelper.OkResponse("Ok");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        /// <summary>
        /// Consulta el listado de líneas activas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <returns>Lista de líneas activas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Lineas_Cbo_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(Cod_Linea) as item,
                         rtrim(Cod_Linea) + '-' + descripcion as descripcion
                  FROM PRM_CA_LINEAS
                  where activo = 1");
        }

        /// <summary>
        /// Consulta las líneas o tipos de remesa activos que pueden recibir asignaciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="tipoOrigen">Origen de la asignación: linea o remesa.</param>
        /// <returns>Lista de orígenes activos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Origenes_Cbo_Obtener(int CodEmpresa, string tipoOrigen)
        {
            if (!EsTipoOrigenValido(tipoOrigen))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de origen debe ser línea o remesa.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            var sql = EsOrigenLinea(tipoOrigen)
                ? @"select rtrim(Cod_Linea) as item,
                           rtrim(Cod_Linea) + ' - ' + descripcion as descripcion
                    from PRM_CA_LINEAS
                    where activo = 1
                    order by Cod_Linea"
                : @"select cast(cod_Remesa as varchar(20)) as item,
                           cast(cod_Remesa as varchar(20)) + ' - ' + descripcion as descripcion
                    from PRM_CA_TIPOS_REMESA
                    where activo = 1
                    order by cod_Remesa";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Consulta el catálogo de códigos asignables a una línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="cod_Linea">Código de la línea seleccionada.</param>
        /// <returns>Catálogo con el estado de asignación.</returns>
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoLineas_Obtener(int CodEmpresa, string cod_Linea)
        {
            return DbHelper.ExecuteListQuery<CcCaCatalogoLineasData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select Cat.Codigo,
                         Cat.Descripcion,
                         isnull(Dt.Codigo,'-1') as Existe
                  from Catalogo Cat
                  left join prm_Ca_Lineas_Dt Dt on Cat.codigo = Dt.Codigo and Dt.cod_Linea = @cod_Linea
                  Order by isnull(Dt.Codigo,'ZZZZZZZ'), Cat.Codigo",
                new { cod_Linea });
        }

        /// <summary>
        /// Consulta el catálogo y marca los códigos asignados a una línea o tipo de remesa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="tipoOrigen">Origen de la asignación: linea o remesa.</param>
        /// <param name="codigoOrigen">Código de línea o tipo de remesa.</param>
        /// <returns>Catálogo con el estado de asignación.</returns>
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoAsignaciones_Obtener(
            int CodEmpresa,
            string tipoOrigen,
            string codigoOrigen)
        {
            if (!EsTipoOrigenValido(tipoOrigen) || string.IsNullOrWhiteSpace(codigoOrigen))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo y el código de origen son requeridos.",
                    -2,
                    new List<CcCaCatalogoLineasData>());
            }

            var codigoRemesa = 0;
            if (!EsOrigenLinea(tipoOrigen) && !int.TryParse(codigoOrigen, out codigoRemesa))
            {
                return DbHelper.CreateErrorResponse(
                    "El código del tipo de remesa no es válido.",
                    -2,
                    new List<CcCaCatalogoLineasData>());
            }

            var sql = EsOrigenLinea(tipoOrigen)
                ? @"select Cat.Codigo,
                           Cat.Descripcion,
                           isnull(Dt.Codigo, '-1') as Existe
                    from Catalogo Cat
                    left join prm_CA_LINEAS_DT Dt
                      on Cat.codigo = Dt.Codigo
                     and Dt.cod_Linea = @codigoOrigen
                    order by isnull(Dt.Codigo, 'ZZZZZZZ'), Cat.Codigo"
                : @"select Cat.Codigo,
                           Cat.Descripcion,
                           isnull(Dt.cod_Linea, '-1') as Existe
                    from Catalogo Cat
                    left join PRM_CA_REMESAS_LINEAS Dt
                      on Cat.codigo = Dt.cod_Linea
                     and Dt.cod_remesa = @codigoRemesa
                    order by isnull(Dt.cod_Linea, 'ZZZZZZZ'), Cat.Codigo";

            return DbHelper.ExecuteListQuery<CcCaCatalogoLineasData>(
                CreatePortalDb(),
                CodEmpresa,
                sql,
                new { codigoOrigen, codigoRemesa });
        }

        /// <summary>
        /// Asigna un código a una línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que realiza el movimiento.</param>
        /// <param name="cod_Linea">Código de la línea seleccionada.</param>
        /// <param name="codigo">Código de catálogo que se asigna.</param>
        /// <returns>Resultado de la asignación.</returns>
        public ErrorDto CC_CA_LineasDetalle_Insertar(int CodEmpresa, string usuario, string cod_Linea, string codigo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    @"insert prm_ca_lineas_dt(cod_linea, codigo, registro_usuario, registro_Fecha)
                      values(@cod_Linea, @codigo, @usuario, dbo.mygetdate())",
                    new
                    {
                        cod_Linea,
                        codigo,
                        usuario
                    },
                    commandTimeout: 0);

                RegistrarBitacora(CodEmpresa, usuario, $"Cargo Automatico: Linea: {cod_Linea} Cod: {codigo} ", "Registra - WEB");
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al asignar código a la línea.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina la asignación de un código a una línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa activa.</param>
        /// <param name="Usuario">Usuario que realiza el movimiento.</param>
        /// <param name="cod_Linea">Código de la línea seleccionada.</param>
        /// <param name="codigo">Código de catálogo que se desasigna.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto CC_CA_LineasDetalle_Delete(int CodEmpresa, string Usuario, string cod_Linea, string codigo)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "delete prm_ca_lineas_dt where cod_linea = @cod_Linea and codigo = @codigo",
                new { cod_Linea, codigo });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar código de la línea.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(CodEmpresa, Usuario, $"Cargo Automatico: Linea: {cod_Linea} Cod: {codigo} ", "Elimina - WEB");
                return DbHelper.OkResponse("Ok");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        /// <summary>
        /// Guarda el estado de asignación de un código para una línea o tipo de remesa.
        /// </summary>
        /// <param name="request">Datos completos de la asignación solicitada.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CC_CA_Asignacion_Guardar(CcCaAsignacionGuardarRequest request)
        {
            if (request is null
                || !EsTipoOrigenValido(request.tipoOrigen)
                || string.IsNullOrWhiteSpace(request.usuario)
                || string.IsNullOrWhiteSpace(request.codigoOrigen)
                || string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Los datos de la asignación son requeridos.", -2);
            }

            var codigoRemesa = 0;
            if (!EsOrigenLinea(request.tipoOrigen)
                && !int.TryParse(request.codigoOrigen, out codigoRemesa))
            {
                return DbHelper.ErrorResponse("El código del tipo de remesa no es válido.", -2);
            }

            var sql = ObtenerSqlAsignacion(request.tipoOrigen, request.activo);
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                request.codEmpresa,
                sql,
                new
                {
                    request.codigoOrigen,
                    codigoRemesa,
                    request.codigo,
                    request.usuario
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al guardar la asignación.",
                    result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                var movimiento = request.activo ? "Registra - WEB" : "Elimina - WEB";
                var detalle = EsOrigenLinea(request.tipoOrigen)
                    ? $"Cargo Automatico: Linea: {request.codigoOrigen} Cod: {request.codigo}"
                    : $"Cargo Automatico: Tipo Remesa: {request.codigoOrigen} Cod: {request.codigo}";

                RegistrarBitacora(request.codEmpresa, request.usuario, detalle, movimiento);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static string ObtenerSqlAsignacion(string tipoOrigen, bool activo)
        {
            if (EsOrigenLinea(tipoOrigen))
            {
                return activo
                    ? @"if not exists (
                            select 1
                            from prm_CA_LINEAS_DT
                            where cod_linea = @codigoOrigen and codigo = @codigo
                        )
                        insert prm_CA_LINEAS_DT(cod_linea, codigo, registro_usuario, registro_fecha)
                        values(@codigoOrigen, @codigo, @usuario, dbo.MyGetdate())"
                    : @"delete prm_CA_LINEAS_DT
                        where cod_linea = @codigoOrigen and codigo = @codigo";
            }

            return activo
                ? @"if not exists (
                        select 1
                        from PRM_CA_REMESAS_LINEAS
                        where cod_remesa = @codigoRemesa and cod_linea = @codigo
                    )
                    insert PRM_CA_REMESAS_LINEAS(cod_remesa, cod_linea, registro_usuario, registro_fecha)
                    values(@codigoRemesa, @codigo, @usuario, dbo.MyGetdate())"
                : @"delete PRM_CA_REMESAS_LINEAS
                    where cod_remesa = @codigoRemesa and cod_linea = @codigo";
        }

        private static bool EsTipoOrigenValido(string tipoOrigen)
        {
            return EsOrigenLinea(tipoOrigen)
                || string.Equals(tipoOrigen, TipoOrigenRemesa, StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsOrigenLinea(string tipoOrigen)
        {
            return string.Equals(tipoOrigen, TipoOrigenLinea, StringComparison.OrdinalIgnoreCase);
        }
        private static CcCaLineasLista CrearResultadoLineasVacio()
        {
            return new CcCaLineasLista
            {
                total = 0,
                lista = new List<CcCaLineasData>()
            };
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

        private static string ObtenerSortFieldLineas(string? sortField)
        {
            return sortField switch
            {
                CampoCodLinea => CampoCodLinea,
                CampoDescripcion => CampoDescripcion,
                CampoCodPlan => CampoCodPlan,
                _ => CampoCodLinea
            };
        }

        private static string ObtenerSortDirectionLineas(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
