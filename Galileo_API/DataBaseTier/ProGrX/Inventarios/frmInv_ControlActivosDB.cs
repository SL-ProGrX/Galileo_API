using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvControlActivosDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvControlActivosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvControlActivosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de control de activos.
        /// </summary>
        /// <returns>Lista vacía inicializada.</returns>
        private static InvControlActivosLista CrearListaVacia() => new()
        {
            total = 0,
            lista = new List<InvControlActivosDto>()
        };

        /// <summary>
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el filtro deserializado del control de activos.
        /// </summary>
        /// <param name="filtros">JSON de filtros.</param>
        /// <returns>Objeto de filtros inicializado.</returns>
        private static InvControlActivosFiltros ObtenerFiltros(string filtros)
        {
            return JsonConvert.DeserializeObject<InvControlActivosFiltros>(filtros) ?? new InvControlActivosFiltros();
        }

        /// <summary>
        /// Agrega filtro LIKE al SQL del listado de activos.
        /// </summary>
        /// <param name="filtro">Texto a buscar.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroActivos(string? filtro, System.Text.StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(@" AND (
                                    A.COD_PRODUCTO LIKE @Filtro
                                    OR A.FACTURA LIKE @Filtro
                                    OR A.COD_COMPRA LIKE @Filtro
                                  )");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH al listado.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int pagina, int paginacion, System.Text.StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (pagina < 0 || paginacion <= 0)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
            parametros.Add("Offset", pagina);
            parametros.Add("Fetch", paginacion);
        }

        /// <summary>
        /// Asigna la UEN a cada activo usando la factura asociada.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="lista">Listado de activos.</param>
        private static void AsignarUenActivos(IDbConnection connection, IEnumerable<InvControlActivosDto> lista)
        {
            const string query = @"SELECT COD_UNIDAD
                                   FROM CPR_SOLICITUD_BS
                                   WHERE CPR_ID IN (
                                       SELECT CPR_ID
                                       FROM CPR_SOLICITUD_PROV
                                       WHERE ADJUDICA_IND = 1
                                         AND ADJUDICA_ORDEN IN (
                                             SELECT COD_ORDEN
                                             FROM CPR_COMPRAS
                                             WHERE COD_FACTURA = @Factura))";

            foreach (var item in lista)
            {
                item.cod_uen = connection.QueryFirstOrDefault<string>(query, new { Factura = item.factura });
            }
        }

        /// <summary>
        /// Obtiene el tipo de activo del producto.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codProducto">Código de producto.</param>
        /// <returns>Tipo de activo encontrado.</returns>
        private static string ObtenerTipoActivo(IDbConnection connection, string codProducto)
        {
            return connection.QueryFirstOrDefault<string>(
                "select TOP 1 tipo_activo FROM PV_PRODUCTOS WHERE COD_PRODUCTO = @CodProducto",
                new { CodProducto = codProducto }) ?? string.Empty;
        }

        /// <summary>
        /// Obtiene la configuración del tipo de activo.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipoActivo">Tipo de activo.</param>
        /// <returns>Configuración del activo.</returns>
        private static InvDatosActivos ObtenerDatosTipoActivo(IDbConnection connection, string tipoActivo)
        {
            return connection.QueryFirstOrDefault<InvDatosActivos>(
                @"select MET_DEPRECIACION, VIDA_UTIL, TIPO_VIDA_UTIL
                  from Activos_tipo_activo
                  where TIPO_ACTIVO = @TipoActivo",
                new { TipoActivo = tipoActivo }) ?? new InvDatosActivos();
        }

        /// <summary>
        /// Asegura que el proveedor exista en la tabla de activos.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="activo">Activo en proceso.</param>
        private static void AsegurarProveedorActivo(IDbConnection connection, InvControlActivosDto activo)
        {
            var existe = connection.QueryFirstOrDefault<int>(
                "select count(*) from Activos_Proveedores where COD_PROVEEDOR = @CodProveedor",
                new { CodProveedor = activo.cod_proveedor });

            if (existe > 0)
            {
                return;
            }

            var nombreProveedor = connection.QueryFirstOrDefault<string>(
                "select NOMBRE from CXP_PROVEEDORES where COD_PROVEEDOR = @CodProveedor",
                new { CodProveedor = activo.cod_proveedor }) ?? string.Empty;

            connection.Execute(
                @"INSERT INTO Activos_Proveedores (COD_PROVEEDOR, NOMBRE, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                  VALUES (@CodProveedor, @Nombre, 1, getDate(), @Usuario)",
                new
                {
                    CodProveedor = activo.cod_proveedor,
                    Nombre = nombreProveedor,
                    Usuario = activo.activo_usuario
                });
        }

        /// <summary>
        /// Inserta el registro principal del activo.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="activo">Activo a registrar.</param>
        /// <param name="tipoActivo">Tipo de activo.</param>
        /// <param name="fecha">Fecha normalizada de compra.</param>
        /// <param name="datosTipoActivo">Configuración del tipo de activo.</param>
        private static void InsertarActivoPrincipal(IDbConnection connection, InvControlActivosDto activo, string tipoActivo, string? fecha, InvDatosActivos datosTipoActivo)
        {
            connection.Execute(
                @"INSERT INTO ACTIVOS_PRINCIPAL
                    ([NUM_PLACA],[TIPO_ACTIVO],[COD_DEPARTAMENTO],[COD_SECCION],[COD_PROVEEDOR],[NOMBRE],[DESCRIPCION],
                     [VALOR_HISTORICO],[VALOR_DESECHO],[FECHA_ADQUISICION],[MODELO],[NUM_SERIE],[MARCA],[COMPRA_DOCUMENTO],
                     [ESTADO],[REGISTRO_FECHA],[REGISTRO_USUARIO],[COD_LOCALIZA],UD_ANIO,UD_PRODUCCION,IDENTIFICACION,
                     VIDA_UTIL,VIDA_UTIL_EN,MET_DEPRECIACION,DEPRECIACION_MES,DEPRECIACION_ACUM,DEPRECIACION_PERIODO,
                     VALOR_LIBROS_PERIODO,FECHA_INSTALACION)
                  VALUES
                    (@NumeroPlaca,@TipoActivo,@Departamento,@Seccion,@CodProveedor,@Nombre,@Descripcion,@ValorHistorico,
                     @ValorDesecho,@FechaAdquisicion,@Modelo,@Serie,@Marca,@CompraDocumento,'R',getDate(),@RegistroUsuario,
                     @CodLocalizacion,0,0,@Identificacion,@VidaUtil,@TipoVidaUtil,@MetDepreciacion,0,0,GETDATE(),
                     @ValorLibrosPeriodo,DATEADD(day, 1, getDate()))",
                new
                {
                    NumeroPlaca = activo.numero_placa,
                    TipoActivo = tipoActivo,
                    Departamento = activo.departamento,
                    Seccion = activo.seccion,
                    CodProveedor = activo.cod_proveedor,
                    Nombre = activo.descripcion,
                    Descripcion = activo.descripcion,
                    ValorHistorico = activo.costo_unitario,
                    ValorDesecho = activo.costo_unitario,
                    FechaAdquisicion = fecha,
                    Modelo = activo.modelo,
                    Serie = activo.serie,
                    Marca = activo.marca,
                    CompraDocumento = activo.cod_compra,
                    RegistroUsuario = activo.activo_usuario,
                    CodLocalizacion = activo.cod_localizacion,
                    Identificacion = activo.id_responsable,
                    VidaUtil = datosTipoActivo.vida_util,
                    TipoVidaUtil = datosTipoActivo.tipo_vida_util,
                    MetDepreciacion = datosTipoActivo.met_depreciacion,
                    ValorLibrosPeriodo = activo.costo_unitario
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Método para obtener la lista de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que registró los activos.</param>
        /// <param name="filtros">JSON con los filtros de consulta.</param>
        /// <returns>Listado de activos pendientes.</returns>
        public ErrorDto<InvControlActivosLista> InvControlActivosLista_Obtener(int CodEmpresa, string usuario, string filtros)
        {
            var filtro = ObtenerFiltros(filtros);
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = CrearListaVacia();
                var parametros = new DynamicParameters();
                parametros.Add("Usuario", usuario);

                var totalQuery = new System.Text.StringBuilder(@"SELECT COUNT(*)
                                                                FROM PV_CONTROL_ACTIVOS A
                                                                WHERE A.ENTREGA_USUARIO = ''
                                                                  AND A.ESTADO IN ('P', 'R')
                                                                  AND A.REGISTRO_USUARIO = @Usuario");

                var detalleQuery = new System.Text.StringBuilder(@"SELECT A.*, P.DESCRIPCION
                                                                  FROM PV_CONTROL_ACTIVOS A
                                                                  LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = A.COD_PRODUCTO
                                                                  WHERE A.ENTREGA_USUARIO = ''
                                                                    AND A.ESTADO IN ('P', 'R')
                                                                    AND A.REGISTRO_USUARIO = @Usuario");

                AgregarFiltroActivos(filtro.filtro, totalQuery, parametros);
                AgregarFiltroActivos(filtro.filtro, detalleQuery, parametros);

                respuesta.total = connection.QueryFirstOrDefault<int>(totalQuery.ToString(), parametros);

                detalleQuery.Append(" ORDER BY A.ID_CONTROL");
                AgregarPaginacion(filtro.pagina, filtro.paginacion, detalleQuery, parametros);

                respuesta.lista = connection.Query<InvControlActivosDto>(detalleQuery.ToString(), parametros).ToList();
                AsignarUenActivos(connection, respuesta.lista);
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener la lista de activos.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Método para obtener el id de la placa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Identificador de placa generado.</returns>
        public ErrorDto InvNumeroPlacaId_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<string>(
                CreatePortalDb(),
                CodEmpresa,
                "select dbo.fxActivos_W_Placa_Id() as PLACA_ID",
                string.Empty);

            return result.Code == 0
                ? new ErrorDto { Code = 0, Description = result.Result ?? string.Empty }
                : DbHelper.ErrorResponse(result.Description ?? "Error al obtener el número de placa.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Método para obtener los departamentos para activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de departamentos.</returns>
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosDepartamentos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<InvCntrActvivosCombos>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(cod_departamento) as item,
                         rtrim(descripcion) as descripcion
                  from Activos_departamentos
                  order by cod_departamento");
        }

        /// <summary>
        /// Método para obtener las secciones de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="departamento">Departamento a consultar.</param>
        /// <returns>Listado de secciones.</returns>
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosSeccion_Obtener(int CodEmpresa, string? departamento)
        {
            return DbHelper.ExecuteListQuery<InvCntrActvivosCombos>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(cod_Seccion) as item,
                         rtrim(descripcion) as descripcion
                  from Activos_Secciones
                  where cod_departamento = @Departamento
                  order by cod_Seccion",
                new { Departamento = departamento });
        }

        /// <summary>
        /// Método para obtener los responsables de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="departamento">Departamento a consultar.</param>
        /// <param name="seccion">Sección a consultar.</param>
        /// <returns>Listado de responsables.</returns>
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosResponsable_Obtener(int CodEmpresa, string? departamento, string? seccion)
        {
            return DbHelper.ExecuteListQuery<InvCntrActvivosCombos>(
                CreatePortalDb(),
                CodEmpresa,
                @"select rtrim(Identificacion) as item,
                         rtrim(Nombre) as descripcion
                  from Activos_Personas
                  where cod_departamento = @Departamento
                    and cod_Seccion = @Seccion
                  order by identificacion",
                new
                {
                    Departamento = departamento,
                    Seccion = seccion
                });
        }

        /// <summary>
        /// Método para obtener las localizaciones de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de localizaciones activas.</returns>
        public ErrorDto<List<InvCntrActvivosCombos>> InvActivosLocalizaciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<InvCntrActvivosCombos>(
                CreatePortalDb(),
                CodEmpresa,
                @"select ISNULL(NULLIF(RTRIM(COD_LOCALIZA), ''), '-1') AS item,
                         rtrim(descripcion) as descripcion
                  from ACTIVOS_LOCALIZACIONES
                  where Activa = 1
                  order by descripcion");
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Método para actualizar el control de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="activo">Activo a procesar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvControlActivos_Actualizar(int CodEmpresa, InvControlActivosDto activo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string procedure = "[spCPR_CONTROL_ACTIVOS_ACTUALIZAR]";
                var values = new
                {
                    id_control = activo.id_control,
                    cod_producto = activo.cod_producto,
                    descripcion = activo.descripcion,
                    costo_total = activo.costo_total,
                    costo_unitario = activo.costo_unitario,
                    factura = activo.factura,
                    cod_compra = activo.cod_compra,
                    fecha_compra = activo.fecha_compra,
                    cod_proveedor = activo.cod_proveedor,
                    cod_bodega = activo.cod_bodega,
                    estado = activo.estado,
                    numero_placa = activo.numero_placa,
                    cod_localizacion = activo.cod_localizacion,
                    marca = activo.marca,
                    modelo = activo.modelo,
                    serie = activo.serie,
                    observaciones = activo.observaciones,
                    cod_uen = activo.cod_uen,
                    id_responsable = activo.id_responsable,
                    cod_requesicion = activo.cod_requesicion,
                    activo_usuario = activo.activo_usuario,
                    registro_usuario = activo.registro_usuario,
                    departamento = activo.departamento,
                    seccion = activo.seccion
                };

                var code = connection.QueryFirstOrDefault<int>(procedure, values, commandType: CommandType.StoredProcedure);
                if (code != 0)
                {
                    return new ErrorDto
                    {
                        Code = code,
                        Description = "Error al actualizar el control de activos."
                    };
                }

                string? fecha = MProGrXAuxiliarDB.validaFechaGlobal(activo.fecha_compra, "yyyy-MM-dd");
                var tipoActivo = ObtenerTipoActivo(connection, activo.cod_producto ?? string.Empty);
                var datosTipoActivo = ObtenerDatosTipoActivo(connection, tipoActivo);

                AsegurarProveedorActivo(connection, activo);
                InsertarActivoPrincipal(connection, activo, tipoActivo, fecha, datosTipoActivo);

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Activo actualizado correctamente"
                };
            });

            return CrearRespuestaNonQuery(result.Result ?? DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el control de activos.", result.Code.GetValueOrDefault(-1)), "Activo actualizado correctamente", "Error al actualizar el control de activos.");
        }

        #endregion
    }
}