using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public class FrmCONotificaEmailDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 4;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmCONotificaEmailDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta de cuotas por vencer.
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>"
        /// <param name="filtros"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto<FrmCONotificaEmailListaResult> Co_NotificaEmail_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros, FrmCONotificaEmailConsultaDto dto)
        {
            var portalDb = new PortalDB(_config);
            var result = CrearResultadoListaNotificaEmail();

            try
            {
                var parametrosConsulta = CrearParametrosConsulta(dto, filtros);
                var queryResult = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
                {
                    using var multi = connection.QueryMultiple(CrearSqlListaNotificaEmail(parametrosConsulta), parametrosConsulta.Parametros);

                    return new FrmCONotificaEmailListaResult
                    {
                        total = multi.ReadFirstOrDefault<int>(),
                        lista = multi.Read<FrmCONotificaEmailCaseData>().ToList()
                    };
                });

                if (queryResult.Code != 0)
                {
                    return CrearErrorListaNotificaEmail(queryResult.Description ?? "Error al consultar notificaciones de email.");
                }

                result.Result = queryResult.Result ?? new FrmCONotificaEmailListaResult
                {
                    total = 0,
                    lista = new List<FrmCONotificaEmailCaseData>()
                };
            }
            catch (Exception ex)
            {
                result = CrearErrorListaNotificaEmail(ex.Message);
            }

            return result;
        }


        /// <summary>
        /// Envía notificaciones para una lista de cédulas.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>"
        /// </summary>
        /// <returns></returns>
        public ErrorDto Co_NotificaEmail_Notificar_Bulk(int CodEmpresa, string usuario, FrmCONotificaEmailNotificarBulkDto dto)
        {
            if (dto?.cedulas == null || dto.cedulas.Count == 0)
            {
                return DbHelper.ErrorResponse("No hay registros seleccionados para notificar.", -2);
            }

            var portalDb = new PortalDB(_config);
            var tipo = NormalizarPrimerCaracter(dto.tipo, "R");
            var result = DbHelper.WithConn(portalDb, CodEmpresa, connection =>
            {
                foreach (var cedula in ObtenerCedulasValidas(dto.cedulas))
                {
                    connection.Execute(
                        "dbo.spSys_Notifica_Cobros_CtaXVencer",
                        CrearParametrosNotificacion(cedula, tipo, usuario),
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al notificar emails.");
            }

            RegistrarBitacoraNotificacion(CodEmpresa, usuario, tipo, dto.cedulas.Count);

            return DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Obtiene el catálogo de estados de persona.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        RTRIM(COD_ESTADO)  AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.AFI_ESTADOS_PERSONA
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        /// <summary>
        /// Obtiene el catálogo de instituciones.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        CAST(COD_INSTITUCION AS varchar(20)) AS item,
                        RTRIM(DESCRIPCION)                   AS descripcion
                    FROM dbo.INSTITUCIONES
                    ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }

        private static ErrorDto<FrmCONotificaEmailListaResult> CrearResultadoListaNotificaEmail()
        {
            return DbHelper.CreateOkResponse(new FrmCONotificaEmailListaResult
            {
                total = 0,
                lista = new List<FrmCONotificaEmailCaseData>()
            });
        }

        private static ErrorDto<FrmCONotificaEmailListaResult> CrearErrorListaNotificaEmail(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -1,
                new FrmCONotificaEmailListaResult
                {
                    total = 0,
                    lista = new List<FrmCONotificaEmailCaseData>()
                });
        }

        private static FrmCONotificaEmailConsultaParams CrearParametrosConsulta(
            FrmCONotificaEmailConsultaDto dto,
            FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros?.filtro ?? string.Empty).Trim();
            var pagina = filtros?.pagina ?? 0;
            var paginacion = filtros?.paginacion ?? 0;
            var exportAll = pagina == 0 || paginacion == 0;

            var parametros = new DynamicParameters();
            parametros.Add("@pInstitucion", dto?.institucionId ?? 0);
            parametros.Add("@pEstado", NormalizarTexto(dto?.estado, "T"));
            parametros.Add("@pTipoCobro", NormalizarPrimerCaracter(dto?.tipoCobro, "T"));
            parametros.Add("@tipoNotifica", NormalizarPrimerCaracter(dto?.tipoNotifica, "D"));

            AgregarFiltro(parametros, filtro);
            AgregarPaginacion(parametros, pagina, paginacion, exportAll);

            return new FrmCONotificaEmailConsultaParams
            {
                Parametros = parametros,
                TieneFiltro = !string.IsNullOrWhiteSpace(filtro),
                ExportAll = exportAll,
                SortField = ObtenerSortField(filtros?.sortField),
                SortOrder = ObtenerSortOrder(filtros?.sortOrder)
            };
        }

        private static void AgregarFiltro(DynamicParameters parametros, string filtro)
        {
            if (!string.IsNullOrWhiteSpace(filtro))
            {
                parametros.Add("@q", $"%{filtro}%");
            }
        }

        private static void AgregarPaginacion(DynamicParameters parametros, int pagina, int paginacion, bool exportAll)
        {
            if (exportAll)
            {
                return;
            }

            parametros.Add("@offset", pagina);
            parametros.Add("@fetch", paginacion);
        }

        private static string NormalizarTexto(string? valor, string valorDefault)
        {
            var texto = (valor ?? valorDefault).Trim().ToUpper();
            return string.IsNullOrWhiteSpace(texto) ? valorDefault : texto;
        }

        private static string NormalizarPrimerCaracter(string? valor, string valorDefault)
        {
            var texto = NormalizarTexto(valor, valorDefault);
            return texto[..1];
        }

        private static string ObtenerSortField(string? sortField)
        {
            return (sortField ?? string.Empty).Trim() switch
            {
                "cedula" => "Cedula",
                "nombre" => "Nombre",
                "estadoDesc" => "EstadoDesc",
                "empresa" => "InstitucionDesc",
                "email" => "Email",
                _ => "Cedula"
            };
        }

        private static string ObtenerSortOrder(int? sortOrder)
        {
            return (sortOrder ?? 1) == 0 ? "DESC" : "ASC";
        }

        private static string CrearSqlListaNotificaEmail(FrmCONotificaEmailConsultaParams consultaParams)
        {
            var filtroSql = CrearFiltroSql(consultaParams.TieneFiltro);
            var paginacionSql = CrearPaginacionSql(consultaParams.ExportAll);

            return $@"
                    IF OBJECT_ID('tempdb..#tmpNotifica') IS NOT NULL DROP TABLE #tmpNotifica;

                    CREATE TABLE #tmpNotifica
                    (
                        Cedula          varchar(50)   NULL,
                        Nombre          varchar(200)  NULL,
                        EstadoDesc      varchar(100)  NULL,
                        InstitucionDesc varchar(200)  NULL,
                        Email           varchar(250)  NULL
                    );

                    INSERT INTO #tmpNotifica
                    EXEC dbo.spCbr_Consulta_CtaXVencer @pInstitucion, @pEstado, @pTipoCobro;

                    SELECT COUNT(1)
                    FROM #tmpNotifica
                    WHERE 1=1
                    {filtroSql};

                    SELECT
                        RTRIM(ISNULL(Cedula,''))          AS cedula,
                        RTRIM(ISNULL(Nombre,''))          AS nombre,
                        RTRIM(ISNULL(EstadoDesc,''))      AS estadoDesc,
                        RTRIM(ISNULL(InstitucionDesc,'')) AS empresa,
                        RTRIM(ISNULL(Email,''))           AS email,
                        CAST(NULL AS decimal(18,2))       AS moraTotal,
                        CAST(NULL AS decimal(18,2))       AS moraCuotas,
                        CAST(NULL AS decimal(18,2))       AS ctaObreroPend,
                        CAST(NULL AS decimal(18,2))       AS ctaPatronalPend
                    FROM #tmpNotifica
                    WHERE 1=1
                    {filtroSql}
                    ORDER BY {consultaParams.SortField} {consultaParams.SortOrder}
                    {paginacionSql};";
        }

        private static string CrearFiltroSql(bool tieneFiltro)
        {
            if (!tieneFiltro)
            {
                return string.Empty;
            }

            return @"
                    AND (
                        UPPER(RTRIM(ISNULL(Cedula,'')))          LIKE UPPER(@q) OR
                        UPPER(RTRIM(ISNULL(Nombre,'')))          LIKE UPPER(@q) OR
                        UPPER(RTRIM(ISNULL(EstadoDesc,'')))      LIKE UPPER(@q) OR
                        UPPER(RTRIM(ISNULL(InstitucionDesc,''))) LIKE UPPER(@q) OR
                        UPPER(RTRIM(ISNULL(Email,'')))           LIKE UPPER(@q)
                    )";
        }

        private static string CrearPaginacionSql(bool exportAll)
        {
            return exportAll ? string.Empty : "OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
        }

        private static IEnumerable<string> ObtenerCedulasValidas(IEnumerable<string?> cedulas)
        {
            return cedulas
                .Select(cedula => (cedula ?? string.Empty).Trim())
                .Where(cedula => !string.IsNullOrWhiteSpace(cedula));
        }

        private static DynamicParameters CrearParametrosNotificacion(string cedula, string tipo, string usuario)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@pCedula", cedula);
            parametros.Add("@Tipo", tipo);
            parametros.Add("@Usuario", usuario);

            return parametros;
        }

        private void RegistrarBitacoraNotificacion(int codEmpresa, string usuario, string tipo, int totalRegistros)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Notifica Email: Tipo {tipo} - Registros {totalRegistros}",
                Movimiento = "Procesa - WEB",
                Modulo = vModulo
            });
        }
    }

    internal sealed class FrmCONotificaEmailConsultaParams
    {
        public DynamicParameters Parametros { get; init; } = new();
        public bool TieneFiltro { get; init; }
        public bool ExportAll { get; init; }
        public string SortField { get; init; } = "Cedula";
        public string SortOrder { get; init; } = "ASC";
    }
}
