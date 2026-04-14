using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlListaDB
    {
        private readonly PortalDB _portalDB;

        public FrmCOControlListaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        #region Principal

        /// <summary>
        /// Consulta el listado principal del control de carteras de cobro.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de búsqueda del formulario.</param>
        /// <returns>Totales y lista principal.</returns>
        public ErrorDto<CoControlListaBuscarResponse> CoControlLista_Buscar(
            int codEmpresa,
            CoControlListaBuscarRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var p = new DynamicParameters();
                p.Add("@usuario", request.usuario);
                p.Add("@todos_usuarios", request.todos_usuarios);
                p.Add("@fecha_inicio", request.fecha_inicio);
                p.Add("@fecha_corte", request.fecha_corte);
                p.Add("@todas_fechas", request.todas_fechas);
                p.Add("@casos_sin_asignar", request.casos_sin_asignar);
                p.Add("@cedula", request.cedula);
                p.Add("@nombre", request.nombre);
                p.Add("@estado", request.estado);
                p.Add("@cuotas_desde", request.cuotas_desde);
                p.Add("@cuotas_hasta", request.cuotas_hasta);
                p.Add("@cartera", request.cartera);
                p.Add("@oficina", request.oficina);
                p.Add("@institucion", request.institucion);
                p.Add("@tipo_casos", request.tipo_casos);
                p.Add("@dias_atencion", request.dias_atencion);
                p.Add("@gestion", request.gestion);
                p.Add("@causa", request.causa);
                p.Add("@arreglo", request.arreglo);
                p.Add("@todas_fechas_pago", request.todas_fechas_pago);
                p.Add("@fecha_pago_inicio", request.fecha_pago_inicio);
                p.Add("@fecha_pago_corte", request.fecha_pago_corte);
                p.Add("@incluir_info_contacto", request.incluir_info_contacto);
                p.Add("@lista_garantias", request.lista_garantias);
                p.Add("@lista_antiguedades", request.lista_antiguedades);
                p.Add("@orden", request.orden);
                p.Add("@orden_tipo", request.orden_tipo);
                p.Add("@filtro", request.filtro);
                p.Add("@pagina", request.pagina);
                p.Add("@paginacion", request.paginacion);
                p.Add("@sortOrder", request.sortOrder);
                p.Add("@sortField", request.sortField);

                using var multi = conn.QueryMultiple(
                    "spCBR_W_ControlLista_Buscar",
                    p,
                    commandType: CommandType.StoredProcedure);

                var totales = multi.ReadFirstOrDefault<CoControlListaTotales>() ?? new CoControlListaTotales();
                var lista = multi.Read<CoControlListaGridRow>().ToList();

                return DbHelper.CreateOkResponse(new CoControlListaBuscarResponse
                {
                    totales = totales,
                    lista = lista
                });
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlListaBuscarResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el usuario activo anterior o siguiente para el scroll del formulario.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Usuario actual y dirección del scroll.</param>
        /// <returns>Usuario encontrado.</returns>
        public ErrorDto<CoControlListaUsuarioScrollResponse> CoControlLista_UsuarioScroll_Obtener(
            int codEmpresa,
            CoControlListaUsuarioScrollRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<CoControlListaUsuarioScrollResponse>(
                    "Debe indicar el usuario actual.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var direccion = (request.direccion ?? string.Empty).Trim().ToUpperInvariant();
                var sql = string.Empty;

                if (direccion == "SIGUIENTE")
                {
                    sql = """
                SELECT TOP 1
                    RTRIM(usuario) AS usuario
                FROM cbr_usuarios
                WHERE estado = 1
                  AND usuario > @usuario
                ORDER BY usuario ASC
                """;
                }
                else if (direccion == "ANTERIOR")
                {
                    sql = """
                SELECT TOP 1
                    RTRIM(usuario) AS usuario
                FROM cbr_usuarios
                WHERE estado = 1
                  AND usuario < @usuario
                ORDER BY usuario DESC
                """;
                }
                else
                {
                    return DbHelper.CreateErrorResponse<CoControlListaUsuarioScrollResponse>(
                        "La dirección del scroll no es válida.");
                }

                var result = conn.QueryFirstOrDefault<CoControlListaUsuarioScrollResponse>(
                    sql,
                    new { usuario = request.usuario.Trim() })
                    ?? new CoControlListaUsuarioScrollResponse();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlListaUsuarioScrollResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Consulta usuarios para la búsqueda del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro de búsqueda.</param>
        /// <returns>Lista de usuarios.</returns>
        public ErrorDto<List<CoControlListaUsuarioBusquedaRow>> CoControlLista_Usuarios_Obtener(
            int codEmpresa,
            CoControlListaUsuarioBusquedaRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var sql = """
            SELECT
                RTRIM(usuario) AS usuario,
                RTRIM(nombre) AS nombre
            FROM cbr_usuarios
            WHERE (@filtro = '' OR usuario LIKE '%' + @filtro + '%' OR nombre LIKE '%' + @filtro + '%')
            ORDER BY nombre
            """;

                var lista = conn.Query<CoControlListaUsuarioBusquedaRow>(
                    sql,
                    new { filtro = request.filtro?.Trim() ?? string.Empty }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CoControlListaUsuarioBusquedaRow>>(ex.Message);
            }
        }

        /// <summary>
        /// Consulta las garantías para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de garantías.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Garantias_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
                    SELECT
                        RTRIM(CONVERT(VARCHAR(20), garantia)) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM CRD_GARANTIA_TIPOS
                    ORDER BY descripcion
                    """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta las antigüedades para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de antigüedades.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Antiguedades_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_antiguedad)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM CBR_ANTIGUEDAD_TIPOS
            ORDER BY dias_desde
            """;
                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta las carteras para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de carteras.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Carteras_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_clasificacion)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM CBR_CLASIFICACION_CARTERA
            ORDER BY descripcion
            """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta las oficinas para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de oficinas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Oficinas_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_oficina)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM SIF_OFICINAS
            ORDER BY descripcion
            """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta las instituciones para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Instituciones_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_institucion)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM INSTITUCIONES
            ORDER BY descripcion
            """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }


        /// <summary>
        /// Consulta las gestiones para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de gestiones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Gestiones_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_gestion)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM CBR_GESTIONES
            ORDER BY descripcion
            """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta las causas para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de causas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Causas_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_causa)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM CBR_CAUSAS_MOROSIDAD
            ORDER BY descripcion
            """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta los arreglos para filtros adicionales del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de arreglos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Arreglos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(CONVERT(VARCHAR(20), cod_arreglo)) AS item,
                RTRIM(descripcion) AS descripcion
            FROM CBR_TIPOS_ARREGLOS
            ORDER BY descripcion
            """;

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta personas del listado de cobros para búsqueda por cédula o nombre.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Usuario actual.</param>
        /// <returns>Lista de personas.</returns>
        public ErrorDto<List<CoControlListaPersonaBusquedaRow>> CoControlLista_Personas_Obtener(
            int codEmpresa,
            CoControlListaPersonaBusquedaRequest request)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = """
            SELECT
                RTRIM(Cedula) AS cedula,
                RTRIM(nombre) AS nombre
            FROM vCBRControlListado
            WHERE Usuario = @usuario
            ORDER BY nombre
            """;

                return conn.Query<CoControlListaPersonaBusquedaRow>(
                    sql,
                    new
                    {
                        usuario = request.usuario?.Trim() ?? string.Empty
                    }).ToList();
            });
        }

        #endregion

        #region Operaciones

        /// <summary>
        /// Consulta la información del tab de operaciones para una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula seleccionada.</param>
        /// <returns>Resumen, garantías y operaciones.</returns>
        public ErrorDto<CoControlListaOperacionesResponse> CoControlLista_Operaciones_Obtener(
            int codEmpresa,
            CoControlListaOperacionesRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<CoControlListaOperacionesResponse>(
                    "Debe indicar la cédula.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                var garantias = conn.Query<CoControlListaOperacionGarantiaRow>(
                    "spCbrPersonaMoraGarantia",
                    new
                    {
                        Cedula = request.cedula.Trim(),
                        Tipo = "V"
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                var operaciones = conn.Query<CoControlListaOperacionDetalleRow>(
                    "EXEC spCbrPersonaMoraDetallada @cedula",
                    new
                    {
                        cedula = request.cedula.Trim()
                    }).ToList();

                var resumen = new CoControlListaOperacionesResumen
                {
                    operaciones_al_dia = 0,
                    operaciones_mora = 0,
                    operaciones_cobro_judicial = 0,
                    saldo_al_dia = 0,
                    saldo_mora = 0,
                    saldo_cobro_judicial = 0,

                    operaciones_cartera = garantias.Sum(x => x.operaciones),
                    saldo_cartera = garantias.Sum(x => x.saldo)
                };

                resumen.operaciones_cartera =
                    resumen.operaciones_al_dia +
                    resumen.operaciones_mora +
                    resumen.operaciones_cobro_judicial;

                resumen.saldo_cartera =
                    resumen.saldo_al_dia +
                    resumen.saldo_mora +
                    resumen.saldo_cobro_judicial;

                return DbHelper.CreateOkResponse(new CoControlListaOperacionesResponse
                {
                    resumen = resumen,
                    garantias = garantias,
                    operaciones = operaciones
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CoControlListaOperacionesResponse>(ex.Message);
            }
        }

        #endregion

    }
}
