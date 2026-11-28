using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaConsultaExpeditentesDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaConsultaExpeditentesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de expedientes y subexpedientes de PREA con paginación server-side.
        /// </summary>
        public ErrorDto<FrmPreaConsultaExpeditentesListaResponse> Prea_frmPreaConsultaExpeditentes_Lista_Obtener(
            FrmPreaConsultaExpeditentesFiltroRequest request)
        {
            var result = new ErrorDto<FrmPreaConsultaExpeditentesListaResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaConsultaExpeditentesListaResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(request.codEmpresa);

                var filtro = request.buscar?.Trim() ?? string.Empty;
                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = request.pagina;
                var fetch = request.paginacion;
                var sortField = request.sortField?.Trim().ToLowerInvariant();
                var sortOrder = request.sortOrder == 1 ? 1 : -1;
                var estado = request.codEstadoV2?.Trim() ?? string.Empty;
                if (estado.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    estado = string.Empty;
                }

                const string sqlCount = @"SELECT COUNT(*) FROM CRD_PREA_PREANALISIS P
                                          WHERE ISNULL(P.COD_PREANALISIS_REF, '') = ''
                                            AND (@fechaInicio IS NULL OR P.FECHA_CREACION >= @fechaInicio)
                                            AND (@fechaFin IS NULL OR P.FECHA_CREACION < DATEADD(DAY, 1, @fechaFin))
                                            AND (@estado = '' OR ISNULL(P.COD_ESTADO_V2, '') = @estado)
                                            AND (@like IS NULL
                                             OR P.COD_PREANALISIS LIKE @like
                                             OR P.COD_LINEA LIKE @like
                                             OR P.NOMBRE LIKE @like
                                             OR P.CEDULA LIKE @like)";

                var parametros = new
                {
                    like,
                    offset,
                    fetch,
                    sortField,
                    sortOrder,
                    fechaInicio = request.fechaInicio?.Date,
                    fechaFin = request.fechaFin?.Date,
                    estado
                };

                var total = connection.QueryFirstOrDefault<int>(sqlCount, parametros);

                const string sql = @"
                    SELECT
                        RTRIM(ISNULL(P.COD_PREANALISIS, '')) AS cod_preanalisis,
                        RTRIM(ISNULL(P.COD_LINEA, '')) AS cod_linea,
                        RTRIM(ISNULL(P.NOMBRE, '')) AS nombre,
                        RTRIM(ISNULL(P.CEDULA, '')) AS cedula,
                        P.FECHA_CREACION AS fecha_creacion,
                        RTRIM(ISNULL(E.ESTADO, ISNULL(P.COD_ESTADO_V2, ''))) AS estado,
                        RTRIM(ISNULL(P.USUARIO, '')) AS usuario
                    FROM CRD_PREA_PREANALISIS P
                    LEFT JOIN CRD_PREA_V2_ESTADOS E
                      ON E.COD_ESTADO = P.COD_ESTADO_V2
                    WHERE ISNULL(P.COD_PREANALISIS_REF, '') = ''
                      AND (@fechaInicio IS NULL OR P.FECHA_CREACION >= @fechaInicio)
                      AND (@fechaFin IS NULL OR P.FECHA_CREACION < DATEADD(DAY, 1, @fechaFin))
                      AND (@estado = '' OR ISNULL(P.COD_ESTADO_V2, '') = @estado)
                      AND (@like IS NULL
                       OR P.COD_PREANALISIS LIKE @like
                       OR P.COD_LINEA LIKE @like
                       OR P.NOMBRE LIKE @like
                       OR P.CEDULA LIKE @like)
                    ORDER BY
                        CASE WHEN @sortField = 'cod_preanalisis' AND @sortOrder = 1 THEN P.COD_PREANALISIS END ASC,
                        CASE WHEN @sortField = 'cod_preanalisis' AND @sortOrder = -1 THEN P.COD_PREANALISIS END DESC,
                        CASE WHEN @sortField = 'cod_linea' AND @sortOrder = 1 THEN P.COD_LINEA END ASC,
                        CASE WHEN @sortField = 'cod_linea' AND @sortOrder = -1 THEN P.COD_LINEA END DESC,
                        CASE WHEN @sortField = 'nombre' AND @sortOrder = 1 THEN P.NOMBRE END ASC,
                        CASE WHEN @sortField = 'nombre' AND @sortOrder = -1 THEN P.NOMBRE END DESC,
                        CASE WHEN @sortField = 'cedula' AND @sortOrder = 1 THEN P.CEDULA END ASC,
                        CASE WHEN @sortField = 'cedula' AND @sortOrder = -1 THEN P.CEDULA END DESC,
                        CASE WHEN @sortField = 'fecha_creacion' AND @sortOrder = 1 THEN P.FECHA_CREACION END ASC,
                        CASE WHEN @sortField = 'fecha_creacion' AND @sortOrder = -1 THEN P.FECHA_CREACION END DESC,
                        CASE WHEN @sortField = 'estado' AND @sortOrder = 1 THEN E.ESTADO END ASC,
                        CASE WHEN @sortField = 'estado' AND @sortOrder = -1 THEN E.ESTADO END DESC,
                        P.COD_PREANALISIS DESC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var items = connection.Query<FrmPreaConsultaExpeditentesItemData>(
                    sql,
                    parametros
                ).ToList();

                result.Result = new FrmPreaConsultaExpeditentesListaResponse
                {
                    lista = items.Select(item => new FrmPreaConsultaExpeditentesItem
                    {
                        cod_preanalisis = item.cod_preanalisis,
                        cod_linea = item.cod_linea,
                        nombre = item.nombre,
                        cedula = item.cedula,
                        fecha_creacion = item.fecha_creacion?.ToString("dd/MM/yyyy") ?? string.Empty,
                        estado = item.estado,
                        usuario = item.usuario
                    }).ToList(),
                    total = total
                };
            }
            catch (DbException ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaConsultaExpeditentesListaResponse();
            }
            catch (InvalidOperationException ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaConsultaExpeditentesListaResponse();
            }

            return result;
        }

        public ErrorDto<List<FrmPreaConsultaExpeditentesEstado>> Prea_frmPreaConsultaExpeditentes_Estados_Obtener(
            int codEmpresa)
        {
            var result = new ErrorDto<List<FrmPreaConsultaExpeditentesEstado>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var rows = connection.Query("SELECT * FROM CRD_PREA_V2_ESTADOS");
                var estados = new List<FrmPreaConsultaExpeditentesEstado>
                {
                    new() { item = "TODOS", descripcion = "Todos" }
                };

                var valoresRows = rows.Select(row =>
                    new Dictionary<string, object>(
                        (IDictionary<string, object>)row,
                        StringComparer.OrdinalIgnoreCase));

                foreach (var valores in valoresRows)
                {
                    var codigo = ObtenerTexto(valores, "COD_ESTADO_V2", "COD_ESTADO", "CODIGO", "ITEM");
                    if (string.IsNullOrWhiteSpace(codigo) || estados.Any(x => x.item.Equals(codigo, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var descripcion = ObtenerTexto(valores, "ESTADO", "DESCRIPCION", "NOMBRE");
                    estados.Add(new FrmPreaConsultaExpeditentesEstado
                    {
                        item = codigo,
                        descripcion = string.IsNullOrWhiteSpace(descripcion) ? codigo : descripcion
                    });
                }

                result.Result = estados
                    .Take(1)
                    .Concat(estados.Skip(1).OrderBy(x => x.descripcion))
                    .ToList();
            }
            catch (DbException ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }
            catch (InvalidCastException ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }
            catch (InvalidOperationException ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }

            return result;
        }

        private static string ObtenerTexto(
            IReadOnlyDictionary<string, object> valores,
            params string[] campos)
        {
            foreach (var campo in campos)
            {
                if (valores.TryGetValue(campo, out var valor) && valor is not null)
                {
                    return Convert.ToString(valor)?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }
    }
}
