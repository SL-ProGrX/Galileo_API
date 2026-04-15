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
        private readonly string vCedValida  = "Debe indicar la cédula.";

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

        /// <summary>
        /// Envía notificaciones de cobro para los casos marcados.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Usuario, tipo y casos seleccionados.</param>
        /// <returns>Total procesado.</returns>
        public ErrorDto<int> CoControlLista_NotificarMarcados_Procesar(
            int codEmpresa,
            CoControlListaNotificarMarcadosRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<int>("Debe indicar el usuario actual.");
            }

            if (request.casos == null || request.casos.Count == 0)
            {
                return DbHelper.CreateErrorResponse<int>("Debe seleccionar al menos un caso.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                conn.Open();
                using var tran = conn.BeginTransaction();

                var total = 0;

  
                var casos = request.casos
                            .Where(x => string.IsNullOrWhiteSpace(x.cedula))
                            .Select(x => x.cedula.Trim())
                            .ToList();

                foreach (var cedula in casos)
                {
                    if (string.IsNullOrWhiteSpace(cedula))
                    {
                        continue;
                    }

                    conn.Execute(
                        "spSys_Notifica_Cobros_01_Atrasos",
                        new
                        {
                            pCedula = cedula.Trim(),
                            Tipo = string.IsNullOrWhiteSpace(request.tipo) ? "R" : request.tipo.Trim(),
                            Usuario = request.usuario.Trim()
                        },
                        transaction: tran,
                        commandType: System.Data.CommandType.StoredProcedure);

                    total++;
                }

                tran.Commit();
                return DbHelper.CreateOkResponse(total);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
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
                    vCedValida);
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

        #region Datos Persona

        /// <summary>
        /// Consulta la información del tab de datos personales para una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula seleccionada.</param>
        /// <returns>Datos personales y teléfonos.</returns>
        public ErrorDto<CoControlListaDatosPersonalesResponse> CoControlLista_DatosPersonales_Obtener(
            int codEmpresa,
            CoControlListaDatosPersonalesRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<CoControlListaDatosPersonalesResponse>(
                    vCedValida);
            }

            try
            {

                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                const string sqlDatos = """
            SELECT
                RTRIM(ISNULL(Prov.Descripcion, '')) AS prov_desc,
                RTRIM(ISNULL(Cant.Descripcion, '')) AS canton_desc,
                RTRIM(ISNULL(Dist.Descripcion, '')) AS dist_desc,
                RTRIM(ISNULL(S.direccion, '')) AS direccion,
                RTRIM(ISNULL(S.af_email, '')) AS af_email
            FROM socios S
            LEFT JOIN Provincias Prov
                ON S.Provincia = Prov.Provincia
            LEFT JOIN Cantones Cant
                ON S.Provincia = Cant.Provincia
               AND S.Canton = Cant.Canton
            LEFT JOIN Distritos Dist
                ON S.Provincia = Dist.Provincia
               AND S.Canton = Dist.Canton
               AND S.distrito = Dist.distrito
            WHERE S.cedula = @cedula
            """;

                const string sqlTelefonos = """
            SELECT
                Numero,
                Tipo,
                Ext,
                contacto
            FROM Telefonos
            WHERE Cedula = @cedula
            """;

                var cedula = request.cedula.Trim();

                var datos = conn.QueryFirstOrDefault<CoControlListaDatosPersonalesData>(
                    sqlDatos,
                    new { cedula })
                    ?? new CoControlListaDatosPersonalesData();

                var telefonosRaw = conn.Query(
             sqlTelefonos,
             new { cedula }).ToList();

                var telefonos = telefonosRaw.Select(x => new CoControlListaTelefonoRow
                {
                    numero = Convert.ToString(x.Numero)?.Trim() ?? string.Empty,
                    tipo = FxTipoTelefono(Convert.ToString(x.Tipo)?.Trim() ?? string.Empty),
                    ext = Convert.ToString(x.Ext)?.Trim() ?? string.Empty,
                    contacto = Convert.ToString(x.contacto)?.Trim() ?? string.Empty,
                }).ToList();

                return DbHelper.CreateOkResponse(new CoControlListaDatosPersonalesResponse
                {
                    datos_personales = datos,
                    telefonos = telefonos
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CoControlListaDatosPersonalesResponse>(ex.Message);
            }
        }

        private static string FxTipoTelefono(string tipo)
        {
            return tipo.Trim().ToUpperInvariant() switch
            {
                "1" => "Habitación",
                "2" => "Trabajo",
                "3" => "Celular",
                _ => tipo
            };
        }

        #endregion

        #region Gestiones
        /// <summary>
        /// Consulta la información del tab de gestiones para una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula seleccionada.</param>
        /// <returns>Gestiones y oficiales.</returns>
        public ErrorDto<CoControlListaGestionesResponse> Co_ControlLista_Gestiones_Consulta(
            int codEmpresa,
            CoControlListaGestionesRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<CoControlListaGestionesResponse>(
                    vCedValida);
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sqlGestiones = """
            SELECT
                S.cod_seg,
                S.fecha,
                DATEADD(DAY, ISNULL(S.tiempo_resolucion, 0), S.fecha) AS vencimiento,
                ISNULL(G.descripcion, '') AS gestion,
                ISNULL(S.notas, '') AS notas,
                ISNULL(S.usuario, '') AS usuario,
                ISNULL(S.monto, 0) AS monto,
                ISNULL(S.tiempo_resolucion, 0) AS tiempo_resolucion,
                ISNULL(A.descripcion, '') AS arreglo,
                S.arreglo_vence,
                ISNULL(C.descripcion, '') AS causa
            FROM CBR_Seguimiento S
            LEFT JOIN CBR_GESTIONES G
                ON S.cod_gestion = G.cod_gestion
            LEFT JOIN CBR_CAUSAS_MOROSIDAD C
                ON S.cod_causa = C.cod_causa
            LEFT JOIN CBR_TIPOS_ARREGLOS A
                ON S.cod_arreglo = A.cod_arreglo
            WHERE S.cedula = @cedula
            ORDER BY S.cod_seg DESC
            """;

                const string sqlOficiales = """
            SELECT
                fecha_asignacion,
                UPPER(ISNULL(usuario, '')) AS usuario,
                ISNULL(mantener, 0) AS mantener,
                ISNULL(rebajo_doble, 0) AS rebajo_doble,
                ISNULL(aplica_mora, 0) AS aplica_mora
            FROM cbr_asignacion_h
            WHERE cedula = @cedula
            ORDER BY fecha_asignacion DESC
            """;

                var gestiones = conn.Query<CoControlListaGestionRow>(
                    sqlGestiones,
                    new { cedula = request.cedula.Trim() }).ToList();

                var oficiales = conn.Query<CoControlListaOficialRow>(
                    sqlOficiales,
                    new { cedula = request.cedula.Trim() }).ToList();

                return new CoControlListaGestionesResponse
                {
                    gestiones = gestiones,
                    oficiales = oficiales
                };
            });
        }

        /// <summary>
        /// Envía una notificación de cobro para una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula, tipo y usuario.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto<bool> CoControlLista_Notificacion_Procesar(
            int codEmpresa,
            CoControlListaNotificacionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<bool>(vCedValida);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<bool>("Debe indicar el usuario.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

                conn.Execute(
                    "spSys_Notifica_Cobros_01_Atrasos",
                    new
                    {
                        pCedula = request.cedula.Trim(),
                        Tipo = string.IsNullOrWhiteSpace(request.tipo) ? "R" : request.tipo.Trim(),
                        Usuario = request.usuario.Trim()
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(true);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
        }
        #endregion

        #region Fiadores

        /// <summary>
        /// Consulta la información del tab de fiadores para una persona.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula seleccionada y filtro de mora.</param>
        /// <returns>Lista de fiadores.</returns>
        public ErrorDto<List<CoControlListaFiadorRow>> CoControlLista_Fiadores_Obtener(
            int codEmpresa,
            CoControlListaFiadoresRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse<List<CoControlListaFiadorRow>>(
                    vCedValida);
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                var sql = """
            SELECT
                ISNULL(M.ESTADO, '') AS estado_mora,
                F.Id_Solicitud AS id_solicitud,
                S.cedula,
                S.nombre,
                E.descripcion AS estado,
                I.descripcion AS inst
            FROM fiadores F
            INNER JOIN Socios S
                ON F.cedulaf = S.cedula
            INNER JOIN Instituciones I
                ON S.cod_institucion = I.cod_institucion
            INNER JOIN Reg_Creditos R
                ON F.Id_Solicitud = R.Id_Solicitud
            INNER JOIN AFI_ESTADOS_PERSONA E
                ON E.cod_estado = S.estadoActual
            LEFT JOIN MOROSIDAD M
                ON F.Id_Solicitud = M.Id_Solicitud
               AND M.Estado = 'A'
            WHERE F.estado = 'A'
              AND R.cedula = @cedula
              AND R.Estado = 'A'
            """;

                if (request.solo_operaciones_atrasadas)
                {
                    sql += " AND M.ESTADO = 'A'";
                }

                sql += """
             GROUP BY
                F.Id_Solicitud,
                S.cedula,
                M.ESTADO,
                S.nombre,
                E.descripcion,
                I.descripcion
             ORDER BY
                F.Id_Solicitud
            """;

                return conn.Query<CoControlListaFiadorRow>(
                    sql,
                    new { cedula = request.cedula.Trim() }).ToList();
            });
        }

        #endregion

        #region Traslados

        /// <summary>
        /// Consulta usuarios para la búsqueda del control de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtro de búsqueda.</param>
        /// <returns>Lista de usuarios.</returns>
        public ErrorDto<List<CoControlListaUsuarioBusquedaRow>> CoControlLista_UsuariosTraslado_Obtener(
            int codEmpresa,
            CoControlListaUsuarioBusquedaRequest request)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                var sql = """
            SELECT
                RTRIM(usuario) AS usuario,
                RTRIM(nombre) AS nombre
            FROM cbr_usuarios
            WHERE 1 = 1
              AND (@filtro = '' OR usuario LIKE '%' + @filtro + '%' OR nombre LIKE '%' + @filtro + '%')
            """;

                if (request.solo_activos)
                {
                    sql += " AND estado = 1";
                }

                if (!string.IsNullOrWhiteSpace(request.excluir_usuario))
                {
                    sql += " AND usuario <> @excluir_usuario";
                }

                sql += " ORDER BY nombre";

                return conn.Query<CoControlListaUsuarioBusquedaRow>(
                    sql,
                    new
                    {
                        filtro = request.filtro?.Trim() ?? string.Empty,
                        excluir_usuario = request.excluir_usuario?.Trim() ?? string.Empty
                    }).ToList();
            });
        }


        /// <summary>
        /// Actualiza mantener y rebajo doble para los casos marcados.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Usuario actual, valores y casos seleccionados.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto<bool> CoControlLista_AplicarMarcados_Procesar(
            int codEmpresa,
            CoControlListaAplicarMarcadosRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<bool>("Debe indicar el usuario actual.");
            }

            if (request.casos == null || request.casos.Count == 0)
            {
                return DbHelper.CreateErrorResponse<bool>("Debe seleccionar al menos un caso.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                conn.Open();
                using var tran = conn.BeginTransaction();

                const string sql = """
            UPDATE cbr_asignacion
            SET mantener = @mantener,
                rebajo_doble = @rebajo_doble
            WHERE usuario = @usuario
              AND cedula = @cedula
            """;

                var casos = request.casos
                    .Where(x => string.IsNullOrWhiteSpace(x.cedula))
                    .Select(x => x.cedula.Trim())
                    .ToList();

                foreach (var cedula in casos)
                {
                    if (string.IsNullOrWhiteSpace(cedula))
                    {
                        continue;
                    }

                    conn.Execute(
                        sql,
                        new
                        {
                            usuario = request.usuario.Trim(),
                            mantener = request.mantener,
                            rebajo_doble = request.rebajo_doble,
                            cedula = cedula.Trim()
                        },
                        transaction: tran);
                }

                tran.Commit();
                return DbHelper.CreateOkResponse(true);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
        }

        /// <summary>
        /// Traslada los casos marcados a otro usuario.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Usuario destino y casos seleccionados.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto<bool> CoControlLista_TrasladarMarcados_Procesar(
            int codEmpresa,
            CoControlListaTrasladarMarcadosRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.usuario_destino))
            {
                return DbHelper.CreateErrorResponse<bool>("Debe indicar el usuario destino.");
            }

            if (request.casos == null || request.casos.Count == 0)
            {
                return DbHelper.CreateErrorResponse<bool>("Debe seleccionar al menos un caso.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                conn.Open();
                using var tran = conn.BeginTransaction();


                var casos = request.casos
                    .Where(x => string.IsNullOrWhiteSpace(x.cedula))
                    .Select(x => x.cedula.Trim())
                    .ToList();

                foreach (var cedula in casos)
                {
                    if (string.IsNullOrWhiteSpace(cedula))
                    {
                        continue;
                    }

                    conn.Execute(
                        "spCBRControlAsg",
                        new
                        {
                            Cedula = cedula.Trim(),
                            Usuario = request.usuario_destino.Trim(),
                            Mantener = 1
                        },
                        transaction: tran,
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                tran.Commit();
                return DbHelper.CreateOkResponse(true);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
        }

        #endregion
    }
}
