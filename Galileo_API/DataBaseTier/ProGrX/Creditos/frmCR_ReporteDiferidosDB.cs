using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrReporteDiferidosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrReporteDiferidosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de líneas de crédito para búsqueda con FrmBusquedasComponent.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReporteDiferidos_Catalogo_Obtener(
            int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene la descripción de una línea de crédito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> CrReporteDiferidos_Codigo_Descripcion_Obtener(
            int codEmpresa,
            string codigo)
        {
            codigo = NormalizarTexto(codigo);

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateOkResponse(new DropDownListaGenericaModel());
            }

            const string sql = @"
                select top 1
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                where codigo = @Codigo;";

            var resp = DbHelper.ExecuteSingleQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Codigo = codigo });

            if (resp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resp.Description ?? "No fue posible obtener la descripción de la línea.",
                    resp.Code.GetValueOrDefault(-1),
                    new DropDownListaGenericaModel());
            }

            return DbHelper.CreateOkResponse(resp.Result ?? new DropDownListaGenericaModel());
        }

        /// <summary>
        /// Consulta el reporte de cálculos de diferidos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrReporteDiferidosItem>> CrReporteDiferidos_Consulta_Obtener(
            int codEmpresa,
            CrReporteDiferidosConsultaRequest request)
        {
            var validacion = ValidarConsulta(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Parámetros inválidos.",
                    validacion.Code.GetValueOrDefault(-2),
                    new List<CrReporteDiferidosItem>());
            }

            const string sql = @"
                select
                    R.id_solicitud,
                    R.codigo,
                    R.cedula,
                    S.nombre,
                    R.montoapr,
                    R.MONTOCALCULO as montocalculo,
                    R.fechaforp,
                    R.fecha_calculo_int,
                    R.int
                from reg_creditos R
                inner join socios S on R.cedula = S.cedula
                where R.estado in ('A', 'C')
                  and R.codigo = @Codigo
                  and R.fechaforp between @FechaInicio and @FechaCorte
                order by R.id_solicitud;";

            var baseResp = DbHelper.ExecuteListQuery<CrReporteDiferidosOperacionBase>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Codigo = request.codigo,
                    FechaInicio = request.fecha_inicio,
                    FechaCorte = request.fecha_corte
                });

            if (baseResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    baseResp.Description ?? "No fue posible obtener el reporte de diferidos.",
                    baseResp.Code.GetValueOrDefault(-1),
                    new List<CrReporteDiferidosItem>());
            }

            if (!request.fecha_corte.HasValue)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la fecha corte.",
                    -2,
                    new List<CrReporteDiferidosItem>());
            }

            var fechaCorte = request.fecha_corte.Value.Date;

            List<CrReporteDiferidosItem> resultado = (baseResp.Result ?? new List<CrReporteDiferidosOperacionBase>())
                .Select(item => MapearItem(item, fechaCorte))
                .ToList();

            return DbHelper.CreateOkResponse(resultado);
        }

        private static ErrorDto ValidarConsulta(CrReporteDiferidosConsultaRequest request)
        {
            request.codigo = NormalizarTexto(request.codigo);

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar la línea de crédito.", -2);
            }

            if (!request.fecha_inicio.HasValue || !request.fecha_corte.HasValue)
            {
                return DbHelper.ErrorResponse("Debe indicar la fecha inicio y la fecha corte.", -2);
            }

            if (request.fecha_corte.Value.Date < request.fecha_inicio.Value.Date)
            {
                return DbHelper.ErrorResponse("La fecha corte no puede ser menor que la fecha inicio.", -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static CrReporteDiferidosItem MapearItem(
            CrReporteDiferidosOperacionBase item,
            DateTime fechaCorte)
        {
            decimal montoCalculo = item.montocalculo ?? item.montoapr;
            DateTime fechaForma = item.fechaforp?.Date ?? fechaCorte;
            DateTime fechaFinal = item.fecha_calculo_int?.Date ?? fechaForma;
            DateTime fechaInicioMes = new(
            fechaCorte.Year,
            fechaCorte.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Unspecified);

            int diasTotal = DateDiffIncluyente(fechaForma, fechaFinal);
            int diasCorte = DateDiffIncluyente(fechaInicioMes, fechaCorte);
            int diasAcumulados = DateDiffIncluyente(fechaForma, fechaCorte);

            return new CrReporteDiferidosItem
            {
                id_solicitud = item.id_solicitud,
                codigo = item.codigo,
                cedula = item.cedula,
                nombre = item.nombre,
                montoapr = item.montoapr,
                monto_calculo = montoCalculo,
                fechaforp = item.fechaforp,
                fecha_calculo_int = item.fecha_calculo_int,
                dias_total = diasTotal,
                total_dif = CalcularDiferido(diasTotal, montoCalculo, item.@int),
                fecha_corte = fechaCorte,
                dias_corte = diasCorte,
                dif_corte = CalcularDiferido(diasCorte, montoCalculo, item.@int),
                dias_acumulados = diasAcumulados,
                dif_acumulado = CalcularDiferido(diasAcumulados, montoCalculo, item.@int),
                tasa = item.@int
            };
        }

        private static int DateDiffIncluyente(DateTime fechaInicio, DateTime fechaFinal)
        {
            return (fechaFinal.Date - fechaInicio.Date).Days + 1;
        }

        private static decimal CalcularDiferido(int dias, decimal monto, decimal tasa)
        {
            return Math.Round((dias * monto * tasa) / 36000m, 2);
        }

        private static string NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}