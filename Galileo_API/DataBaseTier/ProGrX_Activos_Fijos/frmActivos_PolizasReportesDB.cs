using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasReportesDB
    {
        private readonly PortalDB _portalDB;

        public FrmActivosPolizasReportesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener lista de pólizas (paginada y con filtro).
        /// </summary>
        public ErrorDto<ActivosPolizasReportesLista> Activos_PolizasReportesLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosPolizasFiltros>(filtros);

            var response = new ErrorDto<ActivosPolizasReportesLista>
            {
                Code = 0,
                Result = new ActivosPolizasReportesLista()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();

                // Paginación
                int pagina     = vfiltro?.pagina     ?? 0;
                int paginacion = vfiltro?.paginacion ?? 50;
                p.Add("@offset", pagina);
                p.Add("@rows",   paginacion);

                // Normalizamos el texto de filtro para evitar S2589
                string? filtroTexto = vfiltro?.filtro;
                bool tieneFiltro = !string.IsNullOrWhiteSpace(filtroTexto);

                if (tieneFiltro)
                {
                    p.Add("@filtro", $"%{filtroTexto!.Trim()}%");
                }

                const string baseCountSql = @"SELECT COUNT(*) FROM ACTIVOS_POLIZAS";
                const string baseDataSql  = @"
SELECT 
    COD_POLIZA  AS cod_poliza,
    DESCRIPCION AS descripcion
FROM ACTIVOS_POLIZAS";

                const string whereBlock = @"
WHERE (
       COD_POLIZA             LIKE @filtro
    OR DESCRIPCION           LIKE @filtro
    OR ISNULL(NUM_POLIZA,'') LIKE @filtro
    OR ISNULL(DOCUMENTO,'')  LIKE @filtro
)";

                string countSql;
                string dataSql;

                if (tieneFiltro)
                {
                    countSql = baseCountSql + whereBlock + ";";
                    dataSql  = baseDataSql  + whereBlock + @"
ORDER BY COD_POLIZA
OFFSET @offset ROWS 
FETCH NEXT @rows ROWS ONLY;";
                }
                else
                {
                    countSql = baseCountSql + ";";
                    dataSql  = baseDataSql  + @"
ORDER BY COD_POLIZA
OFFSET @offset ROWS 
FETCH NEXT @rows ROWS ONLY;";
                }

                // Total
                response.Result.total = connection.QueryFirstOrDefault<int>(countSql, p);

                // Datos
                response.Result.lista = connection
                    .Query<ActivosPolizasReportesData>(dataSql, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = [];
            }

            return response;
        }

        /// <summary>
        /// Catálogo de tipos de pólizas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Tipos_Lista_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string q = @"
SELECT 
    RTRIM(TIPO_POLIZA) AS item,        -- Idx en VB6
    RTRIM(DESCRIPCION) AS descripcion  -- ItmX en VB6
FROM ACTIVOS_POLIZAS_TIPOS
ORDER BY TIPO_POLIZA;";

                result.Result = connection
                    .Query<DropDownListaGenericaModel>(q)
                    .ToList();
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
        /// Catálogo de tipos de estados.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Estados_Lista_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>
                {
                    new DropDownListaGenericaModel { item = "",  descripcion = "Todas"    },
                    new DropDownListaGenericaModel { item = "1", descripcion = "Activas"  },
                    new DropDownListaGenericaModel { item = "0", descripcion = "Vencidas" },
                }
            };
            return result;
        }
    }
}