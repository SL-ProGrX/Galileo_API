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
        /// Obtener lista de pólizas paginada, con filtro general y tipo de póliza opcional.
        /// </summary>
        public ErrorDto<ActivosPolizasReportesLista> Activos_PolizasReportesLista_Obtener(
            int CodEmpresa,
            string filtros,
            string? tipoPoliza)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosPolizasFiltros>(filtros);

            var response = new ErrorDto<ActivosPolizasReportesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPolizasReportesLista
                {
                    total = 0,
                    lista = new List<ActivosPolizasReportesData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                int pagina = vfiltro?.pagina ?? 0;
                int paginacion = vfiltro?.paginacion ?? 50;

                string? filtroLike = null;

                if (!string.IsNullOrWhiteSpace(vfiltro?.filtro))
                {
                    filtroLike = $"%{vfiltro.filtro.Trim()}%";
                }

                string? tipoPolizaFiltro = string.IsNullOrWhiteSpace(tipoPoliza)
                    ? null
                    : tipoPoliza.Trim();

                var parametros = new DynamicParameters();
                parametros.Add("@offset", pagina);
                parametros.Add("@rows", paginacion);
                parametros.Add("@filtro", filtroLike);
                parametros.Add("@tipoPoliza", tipoPolizaFiltro);

                const string whereBlock = @"
            WHERE
                (
                    @tipoPoliza IS NULL
                    OR TIPO_POLIZA = @tipoPoliza
                )
                AND
                (
                    @filtro IS NULL
                    OR COD_POLIZA             LIKE @filtro
                    OR DESCRIPCION            LIKE @filtro
                    OR ISNULL(NUM_POLIZA, '') LIKE @filtro
                    OR ISNULL(DOCUMENTO, '')  LIKE @filtro
                )";

                string countSql = $@"
            SELECT COUNT(*)
            FROM ACTIVOS_POLIZAS
            {whereBlock};";

                string dataSql = $@"
            SELECT
                COD_POLIZA  AS cod_poliza,
                DESCRIPCION AS descripcion
            FROM ACTIVOS_POLIZAS
            {whereBlock}
            ORDER BY COD_POLIZA
            OFFSET @offset ROWS
            FETCH NEXT @rows ROWS ONLY;";

                response.Result.total = connection.QueryFirstOrDefault<int>(
                    countSql,
                    parametros);

                response.Result.lista = connection
                    .Query<ActivosPolizasReportesData>(
                        dataSql,
                        parametros)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = new List<ActivosPolizasReportesData>();
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