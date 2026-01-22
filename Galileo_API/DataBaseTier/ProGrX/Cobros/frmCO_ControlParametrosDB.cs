using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlParametrosDB
    {
        private readonly PortalDB _portalDB;

        private const string SORT_COD_PARAMETRO = "cod_parametro";
        private const string SORT_DESCRIPCION = "descripcion";
        private const string SORT_VALOR = "valor";

        public FrmCOControlParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Lista de parámetros de Control de Cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<CoControlParametrosListaResult> Co_ControlParametros_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlParametrosListaResult>(ex.Message);
            }
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<CoControlParametrosListaResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoControlParametrosListaResult
                {
                    total = 0,
                    lista = new List<CoControlParametrosData>()
                }
            };
            try
            {
                const string q = @"SELECT
                                      RTRIM(cod_parametro) AS cod_parametro,
                                      RTRIM(descripcion)   AS descripcion,
                                      RTRIM(valor)         AS valor,
                                      CAST(0 AS bit)       AS isNew
                                   FROM dbo.cbr_parametros;";
                var raw = conn.Query<dynamic>(q).AsList();

                var lista = MapRaw(raw);
                lista = AplicarFiltro(lista, filtros.filtro);
                lista = AplicarSort(lista, filtros.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;

                bool exportAll = filtros.pagina == 0 || filtros.paginacion == 0;
                response.Result.lista = exportAll
                    ? lista
                    : AplicarPaginacion(lista, filtros.pagina, filtros.paginacion);

                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CoControlParametrosListaResult>(ex.Message);
            }
        }
        /// <summary>
        /// Exporta la lista de parámetros de Control de Cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<CoControlParametrosListaResult> Co_ControlParametros_Lista_Export(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Co_ControlParametros_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }
        /// <summary>
        /// Guarda (UPDATE) el valor del parámetro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto Co_ControlParametros_Guardar(int CodEmpresa, CoControlParametrosGuardarRequest req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string cod = (req?.cod_parametro ?? "").Trim();
                string valor = (req?.valor ?? "").Trim();

                if (string.IsNullOrWhiteSpace(cod))
                    return DbHelper.ErrorResponse("No se especificó el código de parámetro.", -2);

                const string q = @"
                    UPDATE dbo.cbr_parametros
                    SET valor = @valor
                    WHERE RTRIM(cod_parametro) = @cod;
                ";
                int rows = conn.Execute(q, new { cod, valor });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se encontró el parámetro indicado.", -2);

                return DbHelper.OkResponse("Guardado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        private static List<CoControlParametrosData> MapRaw(List<dynamic> raw)
        {
            var lista = new List<CoControlParametrosData>();

            for (int i = 0; i < raw.Count; i++)
            {
                var r = raw[i];

                string cod = (r?.cod_parametro ?? "").ToString().Trim();
                string desc = (r?.descripcion ?? "").ToString().Trim();
                string val = (r?.valor ?? "").ToString().Trim();

                lista.Add(new CoControlParametrosData
                {
                    cod_parametro = cod,
                    descripcion = desc,
                    valor = val,
                    isNew = false
                });
            }

            return lista;
        }
        private static List<CoControlParametrosData> AplicarFiltro(List<CoControlParametrosData> lista, string? filtroIn)
        {
            string filtro = (filtroIn ?? "").Trim();
            if (string.IsNullOrWhiteSpace(filtro)) return lista;

            string qf = filtro.ToUpperInvariant();
            var filtrada = new List<CoControlParametrosData>();

            for (int i = 0; i < lista.Count; i++)
            {
                var it = lista[i];

                string c = (it.cod_parametro ?? "").Trim().ToUpperInvariant();
                string d = (it.descripcion ?? "").Trim().ToUpperInvariant();
                string v = (it.valor ?? "").Trim().ToUpperInvariant();

                if (c.Contains(qf) || d.Contains(qf) || v.Contains(qf))
                    filtrada.Add(it);
            }

            return filtrada;
        }
        private static List<CoControlParametrosData> AplicarSort(List<CoControlParametrosData> lista, string? sortFieldIn, int sortOrder)
        {
            string sortField = (sortFieldIn ?? "").Trim() switch
            {
                SORT_COD_PARAMETRO => SORT_COD_PARAMETRO,
                SORT_DESCRIPCION => SORT_DESCRIPCION,
                SORT_VALOR => SORT_VALOR,
                _ => SORT_COD_PARAMETRO
            };

            bool desc = sortOrder == 0;

            lista.Sort((a, b) =>
            {
                int cmp = CompararCampo(a, b, sortField);
                return desc ? -cmp : cmp;
            });

            return lista;
        }
        private static int CompararCampo(CoControlParametrosData a, CoControlParametrosData b, string sortField)
        {
            if (sortField == SORT_COD_PARAMETRO)
                return string.Compare(a.cod_parametro ?? "", b.cod_parametro ?? "", StringComparison.OrdinalIgnoreCase);

            if (sortField == SORT_DESCRIPCION)
                return string.Compare(a.descripcion ?? "", b.descripcion ?? "", StringComparison.OrdinalIgnoreCase);

            return string.Compare(a.valor ?? "", b.valor ?? "", StringComparison.OrdinalIgnoreCase);
        }
        private static List<CoControlParametrosData> AplicarPaginacion(List<CoControlParametrosData> lista, int pagina, int paginacion)
        {
            var paged = new List<CoControlParametrosData>();

            int start = pagina < 0 ? 0 : pagina;
            int take = paginacion < 0 ? 0 : paginacion;

            int end = start + take;
            if (end > lista.Count) end = lista.Count;

            for (int i = start; i < end; i++)
                paged.Add(lista[i]);

            return paged;
        }
    }
}