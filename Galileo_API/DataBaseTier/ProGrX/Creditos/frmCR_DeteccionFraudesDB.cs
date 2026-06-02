using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;


namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRDeteccionFraudesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmCRDeteccionFraudesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene el catálogo de estados de operación.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Operaciones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                Item("Activa"),
                Item("Cancelada"),
                Item("Nulas"),
                Item("Todas (Activas/Canceladas)")
            });
        }

        /// <summary>
        /// Obtiene el catálogo de estados de persona.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Personas_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                Item("00 - Todos"),
                Item("01 - Socios"),
                Item("02 - Ex.Socios"),
                Item("03 - No Socios"),
                Item("04 - Ren.Interna"),
                Item("05 - Ren.Patronal")
            });
        }

        /// <summary>
        /// Obtiene el catálogo de garantías.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public static ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>
            {
                Item("TODOS"),
                Item("A - Sobre Ahorros"),
                Item("F - Fiduciaria"),
                Item("H - Hipotecaria"),
                Item("X - Acciones"),
                Item("Y - Fondos de Inversion"),
                Item("N - Sin Garantía")
            });
        }

        /// <summary>
        /// Obtiene el catálogo de grupos de usuarios.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Usuarios_Dropdown_Obtener(int CodEmpresa)
        {
            const string sql = @"
            SELECT
                RTRIM(LTRIM(ISNULL(cod_grupo,''))) + ' - ' +
                RTRIM(LTRIM(ISNULL(descripcion,''))) AS item
            FROM crd_grupos
            ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el catálogo de comités.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            const string sql = @"
            SELECT
                CAST(id_comite AS VARCHAR(20)) AS item,
                RTRIM(LTRIM(ISNULL(descripcion,''))) AS descripcion
            FROM comites
            ORDER BY descripcion;";

            return EjecutarDropdown(CodEmpresa, sql, true);
        }

        /// <summary>
        /// Obtiene el catálogo de recursos.
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Recursos_Dropdown_Obtener(
            int CodEmpresa,
            string? codigo,
            bool todasLineas)
        {
            var sql = todasLineas
                ? @"
                SELECT
                    RTRIM(LTRIM(ISNULL(cod_grupo,''))) + ' - ' +
                    RTRIM(LTRIM(ISNULL(descripcion,''))) AS item
                FROM catalogo_grupos
                ORDER BY descripcion;"
                : @"
                SELECT
                    RTRIM(LTRIM(ISNULL(R.cod_grupo,''))) + ' - ' +
                    RTRIM(LTRIM(ISNULL(R.descripcion,''))) AS item
                FROM catalogo_grupos R
                INNER JOIN catalogo_AsignaGrp A
                    ON R.cod_grupo = A.cod_grupo
                WHERE A.codigo = @codigo
                ORDER BY R.descripcion;";

            return EjecutarDropdown(
                CodEmpresa,
                sql,
                false,
                new
                {
                    codigo = (codigo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Obtiene el catálogo de destinos.
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <param name="todasLineas"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Destinos_Dropdown_Obtener(
            int CodEmpresa,
            string? codigo,
            bool todasLineas)
        {
            var sql = todasLineas
                ? @"
                SELECT
                    RTRIM(LTRIM(ISNULL(cod_destino,''))) + ' - ' +
                    RTRIM(LTRIM(ISNULL(descripcion,''))) AS item
                FROM catalogo_destinos
                ORDER BY descripcion;"
                : @"
                SELECT
                    RTRIM(LTRIM(ISNULL(R.cod_destino,''))) + ' - ' +
                    RTRIM(LTRIM(ISNULL(R.descripcion,''))) AS item
                FROM catalogo_destinos R
                INNER JOIN catalogo_destinosAsg A
                    ON R.cod_destino = A.cod_destino
                WHERE A.codigo = @codigo
                ORDER BY R.descripcion;";

            return EjecutarDropdown(
                CodEmpresa,
                sql,
                false,
                new
                {
                    codigo = (codigo ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Obtiene la descripción de una línea.
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<CrDeteccionFraudesLineaDescripcionDto> CR_DeteccionFraudes_Linea_Descripcion_Obtener(
            int CodEmpresa,
            string? codigo)
        {
            try
            {
                var cod = (codigo ?? string.Empty).Trim();

                var descripcion = _mCobroDb.fxDescribeCodigo(CodEmpresa, cod);

                return DbHelper.CreateOkResponse(new CrDeteccionFraudesLineaDescripcionDto
                {
                    codigo = cod,
                    descripcion = descripcion
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrDeteccionFraudesLineaDescripcionDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el F4 de líneas.
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_DeteccionFraudes_Lineas_F4_Obtener(
            int CodEmpresa,
            string? filtro)
        {
            var pFiltro = (filtro ?? string.Empty).Trim();

            const string sql = @"
            SELECT
                RTRIM(LTRIM(ISNULL(codigo,''))) AS item,
                RTRIM(LTRIM(ISNULL(descripcion,''))) AS descripcion
            FROM catalogo
            WHERE
                ISNULL(codigo,'') <> ''
                AND (
                    @filtro = ''
                    OR codigo LIKE '%' + @filtro + '%'
                    OR descripcion LIKE '%' + @filtro + '%'
                )
            ORDER BY descripcion;";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                var rows = conn.Query(sql, new { filtro = pFiltro });

                var lista = new List<DropDownListaGenericaModel>();

                foreach (var d in rows.Cast<IDictionary<string, object?>>())
                {
                    var item = S(V(d, "item"));

                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }

                    lista.Add(new DropDownListaGenericaModel
                    {
                        item = item,
                        descripcion = S(V(d, "descripcion"))
                    });
                }

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }
        /// <summary>
        /// Prepara la información temporal requerida por los reportes de detección de fraudes.
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto CR_DeteccionFraudes_PrepararReporte(int CodEmpresa,CrDeteccionFraudesReporteRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                var fechaInicio = request.todas_fechas
                    ? "1940/01/01"
                    : FormatoFechaSql(request.fecha_inicio);

                var fechaCorte = request.todas_fechas
                    ? FormatoFechaSql(_mProGrxMain.fxFechaServidor(CodEmpresa, 0))
                    : FormatoFechaSql(request.fecha_corte);

                var usuario = (request.usuario ?? string.Empty).Trim();

                switch ((request.tipo_reporte ?? string.Empty).Trim().ToUpperInvariant())
                {
                    case "RENOVADOS":
                        conn.Execute(
                            "spCRDReporteRenovacion",
                            new
                            {
                                FechaxI = fechaInicio,
                                FechaxC = fechaCorte,
                                Usuario = usuario,
                                Dias = request.dias.GetValueOrDefault()
                            },
                            commandType: CommandType.StoredProcedure);
                        break;

                    case "ANULADOS":
                        conn.Execute(
                            "spCRDReporteAnulados",
                            new
                            {
                                FechaxI = fechaInicio,
                                FechaxC = fechaCorte,
                                Usuario = usuario
                            },
                            commandType: CommandType.StoredProcedure);
                        break;
                }

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }
        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarDropdown(
            int CodEmpresa,
            string sql,
            bool descripcionSeparada = false,
            object? param = null)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Open();

                var rows = conn.Query(sql, param);

                var lista = new List<DropDownListaGenericaModel>();

                foreach (var d in rows.Cast<IDictionary<string, object?>>())
                {
                    var item = S(V(d, "item"));

                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }

                    lista.Add(new DropDownListaGenericaModel
                    {
                        item = item,
                        descripcion = descripcionSeparada
                            ? S(V(d, "descripcion"))
                            : item
                    });
                }

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        private static DropDownListaGenericaModel Item(string item)
        {
            return new DropDownListaGenericaModel
            {
                item = item,
                descripcion = item
            };
        }

        private static object? V(IDictionary<string, object?> d, string key)
        {
            return d.TryGetValue(key, out var value)
                ? value
                : null;
        }

        private static string S(object? value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim()
                ?? string.Empty;
        }
        private static string FormatoFechaSql(DateTime? fecha)
        {
            return fecha.HasValue
                ? fecha.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)
                : DateTime.Today.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        }
    }
}