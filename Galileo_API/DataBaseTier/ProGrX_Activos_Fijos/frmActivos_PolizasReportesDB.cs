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
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPolizasReportesLista> Activos_PolizasReportesLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<ActivosPolizasFiltros>(filtros);
            var response = new ErrorDto<ActivosPolizasReportesLista>();
            response.Result = new ActivosPolizasReportesLista();
            response.Code = 0;

            try
            {
                var query = "";
                string where = "", paginaActual = "", paginacionActual = "";

                 using var connection = _portalDB.CreateConnection(CodEmpresa);
                    if (vfiltro != null)
                    {
                        if (!string.IsNullOrEmpty(vfiltro.filtro))
                        {
                            where = "WHERE COD_POLIZA LIKE '%" + vfiltro.filtro + "%' "
                                  + "OR DESCRIPCION LIKE '%" + vfiltro.filtro + "%' "
                                  + "OR ISNULL(NUM_POLIZA,'') LIKE '%" + vfiltro.filtro + "%' "
                                  + "OR ISNULL(DOCUMENTO,'') LIKE '%" + vfiltro.filtro + "%' ";
                        }

                        if (vfiltro.pagina != null)
                        {
                            paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                            paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                        }
                    }

                    // Total
                    query = $"SELECT COUNT(*) FROM ACTIVOS_POLIZAS {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    // Datos (código + descripción)
                    query = $@"
                        SELECT COD_POLIZA AS cod_poliza,
                               DESCRIPCION AS descripcion
                        FROM ACTIVOS_POLIZAS
                        {where}
                        ORDER BY COD_POLIZA
                        {paginaActual} {paginacionActual}";
                    response.Result.lista = connection.Query<ActivosPolizasReportesData>(query).ToList();
                
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
        /// Catalgo de tipos de polizas.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
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
                    string q = @"
                SELECT 
                    RTRIM(TIPO_POLIZA)   AS item,        -- Idx en VB6
                    RTRIM(DESCRIPCION)   AS descripcion  -- ItmX en VB6
                FROM ACTIVOS_POLIZAS_TIPOS
                ORDER BY TIPO_POLIZA";

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
        /// Catalgo de tipos de Estados.
        /// <param name="CodEmpresa"></param>
        /// </summary>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Estados_Lista_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
        {
            new DropDownListaGenericaModel { item = "",  descripcion = "Todas"   },
            new DropDownListaGenericaModel { item = "1", descripcion = "Activas" },
            new DropDownListaGenericaModel { item = "0", descripcion = "Vencidas"},
        }
            };
            return result;
        }

    }
}