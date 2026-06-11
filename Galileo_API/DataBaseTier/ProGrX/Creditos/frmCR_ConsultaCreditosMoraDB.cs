using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRConsultaCreditosMoraDB
    {
        private const string ParametrosInvalidos = "Parámetros inválidos.";
        private readonly PortalDB _portalDB;

        public FrmCRConsultaCreditosMoraDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el encabezado de consulta de créditos en mora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCreditosMoraHeaderDto> CR_ConsultaCreditosMora_Header_Obtener(int CodEmpresa, string cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                cedula = (cedula ?? string.Empty).Trim();

                const string sql = @"
                    select
                        rtrim(cedula) as cedula,
                        rtrim(nombre) as nombre,
                        dbo.MyGetdate() as fecha
                    from socios
                    where cedula = @cedula;";

                var data = conn.QueryFirstOrDefault<CrConsultaCreditosMoraHeaderDto>(sql, new { cedula })
                           ?? new CrConsultaCreditosMoraHeaderDto();

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraHeaderDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de operaciones en mora por detalle.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>> CR_ConsultaCreditosMora_Detalle_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrConsultaCreditosMoraListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>>(
                    requestResult.Description ?? ParametrosInvalidos);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var request = NormalizarRequest(requestResult.Result);

                var lista = conn.Query<CrConsultaCreditosMoraDetalleSpDto>(
                    "spSIFEstadoCreditosMora",
                    new
                    {
                        Cedula = request.cedula,
                        CuotaTransito = request.cuota_transito
                    },
                    commandType: CommandType.StoredProcedure)
                    .Where(EsDetalleVisible)
                    .Select(MapDetalle)
                    .ToList();

                return DbHelper.CreateOkResponse(AplicarLazyDetalle(lista, request.filtros));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>>(ex.Message);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de operaciones en mora por detalle.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>> CR_ConsultaCreditosMora_Detalle_Lista_Export(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrConsultaCreditosMoraListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>>(
                    requestResult.Description ?? ParametrosInvalidos);
            }

            var request = NormalizarRequest(requestResult.Result);
            request.filtros.pagina = 0;
            request.filtros.paginacion = 0;

            return CR_ConsultaCreditosMora_Detalle_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(request));
        }

        /// <summary>
        /// Obtiene la lista de operaciones en mora agrupadas por garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>> CR_ConsultaCreditosMora_Garantia_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrConsultaCreditosMoraListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>>(
                    requestResult.Description ?? ParametrosInvalidos);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var request = NormalizarRequest(requestResult.Result);

                var lista = conn.Query<CrConsultaCreditosMoraGarantiaSpDto>(
                    "spCbrPersonaMoraGarantia",
                    new
                    {
                        Cedula = request.cedula
                    },
                    commandType: CommandType.StoredProcedure)
                    .Select(MapGarantia)
                    .ToList();

                return DbHelper.CreateOkResponse(AplicarLazyGarantia(lista, request.filtros));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>>(ex.Message);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de operaciones en mora agrupadas por garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>> CR_ConsultaCreditosMora_Garantia_Lista_Export(int CodEmpresa, string parametros)
        {
            var requestResult = ParseFiltros<CrConsultaCreditosMoraListaRequest>(parametros);
            if (requestResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>>(
                    requestResult.Description ?? ParametrosInvalidos);
            }

            var request = NormalizarRequest(requestResult.Result);
            request.filtros.pagina = 0;
            request.filtros.paginacion = 0;

            return CR_ConsultaCreditosMora_Garantia_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(request));
        }

        private static ErrorDto<T> ParseFiltros<T>(string parametros) where T : new()
        {
            try
            {
                return DbHelper.CreateOkResponse(JsonConvert.DeserializeObject<T>(parametros) ?? new T());
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<T>(ex.Message);
            }
        }

        private static CrConsultaCreditosMoraListaRequest NormalizarRequest(CrConsultaCreditosMoraListaRequest? request)
        {
            request ??= new CrConsultaCreditosMoraListaRequest();
            request.cedula = (request.cedula ?? string.Empty).Trim();
            request.cuota_transito = request.cuota_transito == 1 ? Convert.ToInt16(1) : Convert.ToInt16(0);
            request.filtros ??= new FiltrosLazyLoadData();

            return request;
        }

        private static bool EsDetalleVisible(CrConsultaCreditosMoraDetalleSpDto item)
        {
            return string.Equals(item.ProcesoCod, "J", StringComparison.OrdinalIgnoreCase) || item.MoraCuota > 0;
        }

        private static CrConsultaCreditosMoraDetalleDto MapDetalle(CrConsultaCreditosMoraDetalleSpDto item)
        {
            var data = new CrConsultaCreditosMoraDetalleDto
            {
                id_solicitud = item.id_solicitud,
                codigo = (item.codigo ?? string.Empty).Trim(),
                fecult = Convert.ToInt32(item.fecult),
                montoapr = item.montoapr,
                proceso_cod = (item.ProcesoCod ?? string.Empty).Trim(),
                saldo = item.Saldo,
                cuota = item.Cuota,
                proceso = (item.Proceso ?? string.Empty).Trim(),
                linea_x = (item.LineaX ?? string.Empty).Trim(),
                documento_referido = (item.documento_referido ?? string.Empty).Trim(),
                ndocumento = (item.NDOCUMENTO ?? string.Empty).Trim(),
                referencia = (item.referencia ?? string.Empty).Trim(),
                interesv = item.interesv,
                plazo = item.plazo,
                tasa_original = item.TasaOriginal,
                garantia = (item.Garantia ?? string.Empty).Trim(),
                mora_cuota = item.MoraCuota,
                mora_int = item.MoraInt,
                mora_principal = item.MoraPrincipal,
                mora_cargos = item.MoraCargos,
                mora_poliza = item.MoraPoliza,
                mora_antigua = Convert.ToInt32(item.MoraAntigua),
                mora_ultima = Convert.ToInt32(item.MoraUltima),
                observacion_proceso = (item.OBSERVACION_PROCESO ?? string.Empty).Trim(),
                fecha_enviaproceso = item.FECHA_ENVIAPROCESO,
                fechaforp = item.FECHAFORP,
                userfor = (item.USERFOR ?? string.Empty).Trim(),
                cod_oficina_r = (item.COD_OFICINA_R ?? string.Empty).Trim(),
                oficina_x = (item.OficinaX ?? string.Empty).Trim(),
                cbr_intereses = item.CbrIntereses,
                destino_x = (item.DestinoX ?? string.Empty).Trim(),
                indicador_cbr = item.IndicadorCbr
            };

            data.mora_financiera = data.mora_principal + data.mora_cargos + data.mora_int + data.mora_poliza;
            data.mora_legal = CalcularMoraLegal(data);
            data.en_cobro_judicial = EsCobroJudicial(data) ? data.mora_legal : 0;
            data.estado_icono = ObtenerEstadoIcono(data);
            data.estado_nota = ObtenerEstadoNota(data);
            data.linea_nota = ObtenerLineaNota(data);

            return data;
        }

        private static CrConsultaCreditosMoraGarantiaDto MapGarantia(CrConsultaCreditosMoraGarantiaSpDto item)
        {
            var data = new CrConsultaCreditosMoraGarantiaDto
            {
                garantia = (item.Garantia ?? string.Empty).Trim(),
                saldo = item.Saldo,
                operaciones = item.Operaciones,
                mor_int_cor = item.MorIntCor,
                mor_int_mor = item.MorIntMor,
                mor_cargos = item.MorCargos,
                mor_principal = item.MorPrincipal,
                mor_cuotas = item.MorCuotas,
                mor_cta_antigua = Convert.ToInt32(item.MorCtaAntigua),
                mor_cta_ultima = Convert.ToInt32(item.MorCtaUltima),
                mora_dias = Convert.ToInt32(item.MorCuotas * 30),
                antiguedad = (item.ANTIGUEDAD ?? string.Empty).Trim(),
                cod_antiguedad = (item.COD_ANTIGUEDAD ?? string.Empty).Trim()
            };

            data.mora_financiera = data.mor_principal + data.mor_cargos + data.mor_int_cor + data.mor_int_mor;
            data.mora_legal = data.saldo + data.mor_cargos + data.mor_int_cor + data.mor_int_mor;

            return data;
        }

        private static CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto> AplicarLazyDetalle(List<CrConsultaCreditosMoraDetalleDto> lista, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var data = AplicarFiltroOrdenDetalle(lista, filtros);
            var total = data.Count;
            var totales = CalcularTotalesDetalle(data);

            data = Paginar(data, filtros);

            return new CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>
            {
                total = total,
                lista = data,
                totales = totales
            };
        }

        private static CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto> AplicarLazyGarantia(List<CrConsultaCreditosMoraGarantiaDto> lista, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            var data = AplicarFiltroOrdenGarantia(lista, filtros);
            var total = data.Count;
            var totales = CalcularTotalesGarantia(data);

            data = Paginar(data, filtros);

            return new CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>
            {
                total = total,
                lista = data,
                totales = totales
            };
        }

        private static List<CrConsultaCreditosMoraDetalleDto> AplicarFiltroOrdenDetalle(List<CrConsultaCreditosMoraDetalleDto> lista, FiltrosLazyLoadData filtros)
        {
            var texto = (filtros.filtro ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                lista = lista.Where(x => DetalleContiene(x, texto)).ToList();
            }

            return OrdenarDetalle(lista, filtros);
        }

        private static List<CrConsultaCreditosMoraGarantiaDto> AplicarFiltroOrdenGarantia(List<CrConsultaCreditosMoraGarantiaDto> lista, FiltrosLazyLoadData filtros)
        {
            var texto = (filtros.filtro ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(texto))
            {
                lista = lista.Where(x => GarantiaContiene(x, texto)).ToList();
            }

            return OrdenarGarantia(lista, filtros);
        }

        private static bool DetalleContiene(CrConsultaCreditosMoraDetalleDto item, string texto)
        {
            return Contiene(item.id_solicitud.ToString(), texto)
                   || Contiene(item.codigo, texto)
                   || Contiene(item.proceso_cod, texto)
                   || Contiene(item.proceso, texto)
                   || Contiene(item.linea_x, texto)
                   || Contiene(item.documento_referido, texto)
                   || Contiene(item.ndocumento, texto)
                   || Contiene(item.referencia, texto)
                   || Contiene(item.garantia, texto)
                   || Contiene(item.userfor, texto)
                   || Contiene(item.oficina_x, texto)
                   || Contiene(item.destino_x, texto);
        }

        private static bool GarantiaContiene(CrConsultaCreditosMoraGarantiaDto item, string texto)
        {
            return Contiene(item.garantia, texto)
                   || Contiene(item.operaciones.ToString(), texto)
                   || Contiene(item.antiguedad, texto)
                   || Contiene(item.cod_antiguedad, texto);
        }

        private static List<CrConsultaCreditosMoraDetalleDto> OrdenarDetalle(List<CrConsultaCreditosMoraDetalleDto> lista, FiltrosLazyLoadData filtros)
        {
            var asc = filtros.sortOrder == 0;
            var sort = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();

            return sort switch
            {
                "id_solicitud" => Ordenar(lista, x => x.id_solicitud, asc),
                "codigo" => Ordenar(lista, x => x.codigo, asc),
                "fecult" => Ordenar(lista, x => x.fecult, asc),
                "montoapr" => Ordenar(lista, x => x.montoapr, asc),
                "proceso_cod" => Ordenar(lista, x => x.proceso_cod, asc),
                "saldo" => Ordenar(lista, x => x.saldo, asc),
                "cuota" => Ordenar(lista, x => x.cuota, asc),
                "proceso" => Ordenar(lista, x => x.proceso, asc),
                "linea_x" => Ordenar(lista, x => x.linea_x, asc),
                "garantia" => Ordenar(lista, x => x.garantia, asc),
                "mora_cuota" => Ordenar(lista, x => x.mora_cuota, asc),
                "mora_int" => Ordenar(lista, x => x.mora_int, asc),
                "mora_cargos" => Ordenar(lista, x => x.mora_cargos, asc),
                "mora_poliza" => Ordenar(lista, x => x.mora_poliza, asc),
                "mora_principal" => Ordenar(lista, x => x.mora_principal, asc),
                "mora_financiera" => Ordenar(lista, x => x.mora_financiera, asc),
                "mora_legal" => Ordenar(lista, x => x.mora_legal, asc),
                "mora_antigua" => Ordenar(lista, x => x.mora_antigua, asc),
                "mora_ultima" => Ordenar(lista, x => x.mora_ultima, asc),
                "destino_x" => Ordenar(lista, x => x.destino_x, asc),
                _ => lista
            };
        }

        private static List<CrConsultaCreditosMoraGarantiaDto> OrdenarGarantia(List<CrConsultaCreditosMoraGarantiaDto> lista, FiltrosLazyLoadData filtros)
        {
            var asc = filtros.sortOrder == 0;
            var sort = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();

            return sort switch
            {
                "garantia" => Ordenar(lista, x => x.garantia, asc),
                "saldo" => Ordenar(lista, x => x.saldo, asc),
                "operaciones" => Ordenar(lista, x => x.operaciones, asc),
                "mor_int_cor" => Ordenar(lista, x => x.mor_int_cor, asc),
                "mor_int_mor" => Ordenar(lista, x => x.mor_int_mor, asc),
                "mor_cargos" => Ordenar(lista, x => x.mor_cargos, asc),
                "mor_principal" => Ordenar(lista, x => x.mor_principal, asc),
                "mor_cuotas" => Ordenar(lista, x => x.mor_cuotas, asc),
                "mora_financiera" => Ordenar(lista, x => x.mora_financiera, asc),
                "mora_legal" => Ordenar(lista, x => x.mora_legal, asc),
                "antiguedad" => Ordenar(lista, x => x.antiguedad, asc),
                "cod_antiguedad" => Ordenar(lista, x => x.cod_antiguedad, asc),
                _ => lista
            };
        }

        private static CrConsultaCreditosMoraTotalesDto CalcularTotalesDetalle(List<CrConsultaCreditosMoraDetalleDto> lista)
        {
            return new CrConsultaCreditosMoraTotalesDto
            {
                no_cuotas = lista.Sum(x => x.mora_cuota),
                intereses_atrasados = lista.Sum(x => x.mora_int),
                cargos_registrados = lista.Sum(x => x.mora_cargos),
                polizas_registradas = lista.Sum(x => x.mora_poliza),
                principal_atrasado = lista.Sum(x => x.mora_principal),
                mora_financiera = lista.Sum(x => x.mora_financiera),
                mora_legal = lista.Sum(x => x.mora_legal + x.mora_poliza),
                en_cobro_judicial = lista.Sum(x =>
                    EsCobroJudicial(x)
                        ? x.mora_legal + x.mora_poliza
                        : 0)
            };
        }

        private static CrConsultaCreditosMoraTotalesDto CalcularTotalesGarantia(List<CrConsultaCreditosMoraGarantiaDto> lista)
        {
            return new CrConsultaCreditosMoraTotalesDto
            {
                no_cuotas = lista.Sum(x => x.mor_cuotas),
                intereses_atrasados = lista.Sum(x => x.mor_int_cor + x.mor_int_mor),
                cargos_registrados = lista.Sum(x => x.mor_cargos),
                polizas_registradas = 0,
                principal_atrasado = lista.Sum(x => x.mor_principal),
                mora_financiera = lista.Sum(x => x.mora_financiera),
                mora_legal = lista.Sum(x => x.mora_legal),
                en_cobro_judicial = 0
            };
        }

        private static decimal CalcularMoraLegal(CrConsultaCreditosMoraDetalleDto item)
        {
            return EsCobroJudicial(item)
                ? item.saldo + item.mora_cargos + item.cbr_intereses
                : item.saldo + item.mora_cargos + item.mora_int;
        }

        private static bool EsCobroJudicial(CrConsultaCreditosMoraDetalleDto item)
        {
            return string.Equals(item.proceso_cod, "J", StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenerEstadoIcono(CrConsultaCreditosMoraDetalleDto item)
        {
            if (EsCobroJudicial(item)) return "JUDICIAL";
            if (item.mora_cuota > 0) return "MORA";
            if (item.indicador_cbr > 0) return "CBR";
            if (!string.IsNullOrWhiteSpace(item.referencia)) return "REFERENCIA";

            return "NORMAL";
        }

        private static string ObtenerEstadoNota(CrConsultaCreditosMoraDetalleDto item)
        {
            if (EsCobroJudicial(item))
            {
                return $">> Cobro Judicial <<{Environment.NewLine}Fecha : {FormatoFecha(item.fecha_enviaproceso)}{Environment.NewLine}Nota  : {item.observacion_proceso}";
            }

            if (item.mora_cuota <= 0)
            {
                return string.Empty;
            }

            return $"Morosidad:  Cuotas: {item.mora_cuota}{Environment.NewLine}" +
                   $"   Intereses : {item.mora_int:N2}{Environment.NewLine}" +
                   $"   Cargos    : {item.mora_cargos:N2}{Environment.NewLine}" +
                   $"   Póliza    : {item.mora_poliza:N2}{Environment.NewLine}" +
                   $"   Principal : {item.mora_principal:N2}{Environment.NewLine}" +
                   $"   Cta.+ Vieja : {item.mora_antigua:0000-00}{Environment.NewLine}" +
                   $"   Cta. Ultima : {item.mora_ultima:0000-00}{Environment.NewLine}{Environment.NewLine}" +
                   $"   Total Mora  : {item.mora_financiera:N2}{Environment.NewLine}";
        }

        private static string ObtenerLineaNota(CrConsultaCreditosMoraDetalleDto item)
        {
            return $"{item.linea_x}{Environment.NewLine}{Environment.NewLine}" +
                   $"Formaliza: {FormatoFecha(item.fechaforp)}{Environment.NewLine}" +
                   $"Usuario: {item.userfor}{Environment.NewLine}" +
                   $"Oficina:{item.oficina_x}";
        }

        private static string FormatoFecha(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("dd/MM/yyyy") : string.Empty;
        }

        private static List<T> Paginar<T>(List<T> lista, FiltrosLazyLoadData filtros)
        {
            if (filtros.paginacion <= 0)
            {
                return lista;
            }

            return lista
                .Skip(Math.Max(filtros.pagina, 0) * filtros.paginacion)
                .Take(filtros.paginacion)
                .ToList();
        }

        private static List<T> Ordenar<T, TKey>(List<T> lista, Func<T, TKey> selector, bool asc)
        {
            return asc
                ? lista.OrderBy(selector).ToList()
                : lista.OrderByDescending(selector).ToList();
        }

        private static bool Contiene(string? value, string texto)
        {
            return (value ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase);
        }

        public sealed class CrConsultaCreditosMoraDetalleSpDto
        {
            public int id_solicitud { get; set; }
            public string codigo { get; set; } = string.Empty;
            public decimal fecult { get; set; }
            public decimal montoapr { get; set; }
            public string ProcesoCod { get; set; } = string.Empty;
            public decimal Saldo { get; set; }
            public decimal Cuota { get; set; }
            public string Proceso { get; set; } = string.Empty;
            public string LineaX { get; set; } = string.Empty;
            public string documento_referido { get; set; } = string.Empty;
            public string NDOCUMENTO { get; set; } = string.Empty;
            public string referencia { get; set; } = string.Empty;
            public decimal interesv { get; set; }
            public int plazo { get; set; }
            public decimal TasaOriginal { get; set; }
            public string Garantia { get; set; } = string.Empty;
            public decimal MoraCuota { get; set; }
            public decimal MoraInt { get; set; }
            public decimal MoraPrincipal { get; set; }
            public decimal MoraCargos { get; set; }
            public decimal MoraPoliza { get; set; }
            public decimal MoraAntigua { get; set; }
            public decimal MoraUltima { get; set; }
            public string OBSERVACION_PROCESO { get; set; } = string.Empty;
            public DateTime? FECHA_ENVIAPROCESO { get; set; }
            public DateTime? FECHAFORP { get; set; }
            public string USERFOR { get; set; } = string.Empty;
            public string COD_OFICINA_R { get; set; } = string.Empty;
            public string OficinaX { get; set; } = string.Empty;
            public decimal CbrIntereses { get; set; }
            public string DestinoX { get; set; } = string.Empty;
            public int IndicadorCbr { get; set; }
        }

        public sealed class CrConsultaCreditosMoraGarantiaSpDto
        {
            public string Garantia { get; set; } = string.Empty;
            public decimal Saldo { get; set; }
            public int Operaciones { get; set; }
            public decimal MorIntCor { get; set; }
            public decimal MorIntMor { get; set; }
            public decimal MorCargos { get; set; }
            public decimal MorPrincipal { get; set; }
            public decimal MorCuotas { get; set; }
            public decimal MorCtaAntigua { get; set; }
            public decimal MorCtaUltima { get; set; }
            public int MoraDias { get; set; }
            public string ANTIGUEDAD { get; set; } = string.Empty;
            public string COD_ANTIGUEDAD { get; set; } = string.Empty;
        }
    }
}