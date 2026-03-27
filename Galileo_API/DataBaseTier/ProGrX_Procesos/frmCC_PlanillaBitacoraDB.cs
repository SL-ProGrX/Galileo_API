using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCCPlanillaBitacoraDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;

        private const int ScrollSiguiente = 1;
        private const int ScrollAnterior = 2;
        private const string MensajeProcesoInvalido = "El proceso indicado no es válido.";
        private const string MensajeInstitucionRequerida = "La institución es requerida.";
        private const string FiltroCodInstitucion = "cod_institucion";
        private const string FiltroTexto = "texto";

        public FrmCCPlanillaBitacoraDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
        }
        /// <summary>
        /// Obtiene dropdown de instituciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        cast(cod_institucion as varchar(20)) as item,
                        rtrim(descripcion) as descripcion
                    from instituciones
                    order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Navega al siguiente o anterior proceso disponible.
        /// scrollCode: 1=siguiente, 2=anterior.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="procesoActual"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaProcesosScrollDto> CC_PlanillaBitacora_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            try
            {
                if (procesoActual <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        MensajeProcesoInvalido,
                        -2,
                        new CcPlanillaProcesosScrollDto());
                }

                decimal proceso = scrollCode switch
                {
                    ScrollSiguiente => _mCobroDb.fxFechaProcesoSiguiente(CodEmpresa, procesoActual),
                    ScrollAnterior => _mCobroDb.fxFechaProcesoAnterior(CodEmpresa, procesoActual),
                    _ => procesoActual
                };

                var result = new CcPlanillaProcesosScrollDto
                {
                    proceso = proceso,
                    proceso_format = MCobroDb.fxFechaProcesoFormat(proceso)
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CcPlanillaProcesosScrollDto());
            }
        }
        /// <summary>
        /// Obtiene lista paginada de bitácora de planilla por institución y proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proceso"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaBitacoraListaResult> CC_PlanillaBitacora_Lista_Obtener(int CodEmpresa, decimal proceso, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.error != null) return filtrosResult.error;

            var filtros = filtrosResult.filtros;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var validacion = ValidarEntrada(proceso, filtros);
                if (validacion != null) return validacion;

                var data = ObtenerDatos(conn, filtros, proceso);

                MapearDatos(data);

                data = AplicarFiltro(data, filtros);
                data = AplicarSort(data, filtros);

                var total = data.Count;
                data = AplicarPaginacion(data, filtros);

                return DbHelper.CreateOkResponse(new CcPlanillaBitacoraListaResult
                {
                    total = total,
                    lista = data
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CcPlanillaBitacoraListaResult());
            }
        }
        /// <summary>
        /// Exporta lista completa de bitácora de planilla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proceso"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaBitacoraListaResult> CC_PlanillaBitacora_Lista_Export(int CodEmpresa, decimal proceso, string parametros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                    ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse(
                    jex.Message,
                    -1,
                    new CcPlanillaBitacoraListaResult());
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CC_PlanillaBitacora_Lista_Obtener(
                CodEmpresa,
                proceso,
                JsonConvert.SerializeObject(filtros));
        }
        private static string? ExtractKeyFromFiltro(string? filtroJson, string key)
        {
            if (string.IsNullOrWhiteSpace(filtroJson))
            {
                return null;
            }

            try
            {
                var dto = JsonConvert.DeserializeObject<CcPlanillaBitacoraFiltroDto>(filtroJson);
                if (dto == null)
                {
                    return null;
                }

                return key switch
                {
                    FiltroCodInstitucion => dto.cod_institucion,
                    FiltroTexto => dto.texto,
                    _ => null
                };
            }
            catch (JsonException)
            {
                return null;
            }
        }
        private static (FiltrosLazyLoadData filtros, ErrorDto<CcPlanillaBitacoraListaResult>? error)ParseFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                return (filtros, null);
            }
            catch (JsonException jex)
            {
                return (new FiltrosLazyLoadData(),
                    DbHelper.CreateErrorResponse(
                        jex.Message,
                        -1,
                        new CcPlanillaBitacoraListaResult()));
            }
        }
        private static ErrorDto<CcPlanillaBitacoraListaResult>? ValidarEntrada(decimal proceso, FiltrosLazyLoadData filtros)
        {
            if (proceso <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeProcesoInvalido,
                    -2,
                    new CcPlanillaBitacoraListaResult());
            }

            var codInstitucion = ExtractKeyFromFiltro(filtros.filtro, FiltroCodInstitucion)?.Trim();

            if (string.IsNullOrWhiteSpace(codInstitucion))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeInstitucionRequerida,
                    -2,
                    new CcPlanillaBitacoraListaResult());
            }

            return null;
        }
        private static List<CcPlanillaBitacoraData> ObtenerDatos(SqlConnection conn, FiltrosLazyLoadData filtros, decimal proceso)
        {
            var codInstitucion = ExtractKeyFromFiltro(filtros.filtro, FiltroCodInstitucion)?.Trim();

            const string sql = @"
        select
            B.id_seq,
            isnull(B.gestion,'') as gestion,
            isnull(rtrim(B.transaccion),'') as transaccion,
            isnull(rtrim(B.documento),'') as documento,
            isnull(rtrim(B.usuario),'') as usuario,
            B.fecha
        from prm_bitacora B
        where B.cod_institucion = @codInstitucion
          and B.proceso = @proceso;";

            return conn.Query<CcPlanillaBitacoraData>(sql, new
            {
                codInstitucion = Convert.ToInt32(codInstitucion),
                proceso
            }).ToList();
        }
        private static void MapearDatos(List<CcPlanillaBitacoraData> lista)
        {
            foreach (var item in lista)
            {
                item.gestion = item.gestion == "R" ? "Recepción" : "Envio";
                item.transaccion = MProcesoMensualDb.FxPlanillaTipoTransac(item.transaccion);
            }
        }
        private static List<CcPlanillaBitacoraData> AplicarFiltro(List<CcPlanillaBitacoraData> lista, FiltrosLazyLoadData filtros)
        {
            var texto = (ExtractKeyFromFiltro(filtros.filtro, FiltroTexto) ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return lista;

            return lista.Where(x =>
                   x.id_seq.ToString().Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.gestion ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.transaccion ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.documento ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.usuario ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase)
                || (x.fecha?.ToString("dd/MM/yyyy") ?? "").Contains(texto, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
        private static List<CcPlanillaBitacoraData> AplicarSort(List<CcPlanillaBitacoraData> lista, FiltrosLazyLoadData filtros)
        {
            string sortField = (filtros.sortField ?? "").Trim().ToLowerInvariant();
            int sortOrder = filtros.sortOrder == 0 ? 0 : 1;

            return sortField switch
            {
                "id_seq" => sortOrder == 1 ? lista.OrderBy(x => x.id_seq).ToList() : lista.OrderByDescending(x => x.id_seq).ToList(),
                "gestion" => sortOrder == 1 ? lista.OrderBy(x => x.gestion).ToList() : lista.OrderByDescending(x => x.gestion).ToList(),
                "transaccion" => sortOrder == 1 ? lista.OrderBy(x => x.transaccion).ToList() : lista.OrderByDescending(x => x.transaccion).ToList(),
                "documento" => sortOrder == 1 ? lista.OrderBy(x => x.documento).ToList() : lista.OrderByDescending(x => x.documento).ToList(),
                "usuario" => sortOrder == 1 ? lista.OrderBy(x => x.usuario).ToList() : lista.OrderByDescending(x => x.usuario).ToList(),
                "fecha" => sortOrder == 1 ? lista.OrderBy(x => x.fecha).ToList() : lista.OrderByDescending(x => x.fecha).ToList(),
                _ => lista.OrderByDescending(x => x.id_seq).ToList()
            };
        }
        private static List<CcPlanillaBitacoraData> AplicarPaginacion(List<CcPlanillaBitacoraData> lista, FiltrosLazyLoadData filtros)
        {
            int pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            int fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;

            if (fetch <= 0) return lista;

            int offset = pagina * fetch;

            return lista.Skip(offset).Take(fetch).ToList();
        }
    }
}