using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesPeriodosDB
    {
        /// <summary>
        /// Obtiene la lista principal de períodos de excedentes.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesPeriodosListaDto>> Patrimonio_frmAH_ExcedentesPeriodos_Lista(int codEmpresa)
        {
            const string sql = @"
select
    isnull(ID_PERIODO, 0) as id_periodo,
    INICIO as inicio,
    CORTE as corte,
    case
        when ESTADO in ('P', 'A') then 'Abierto'
        when ESTADO = 'C' then 'Cerrado'
        else rtrim(isnull(ESTADO, ''))
    end as estado,
    cast(isnull(RESERVA_PORC, 0) as decimal(18, 2)) as reserva_porc,
    cast(isnull(CAPITALIZA_PORC, 0) as decimal(18, 2)) as capitaliza_porc,
    case when isnull(CAPITALIZA_RENTA_APLICA, 0) = 1 then 'Sí' else 'No' end as capitaliza_renta_aplica_desc,
    rtrim(isnull(NC_SALDOS, '')) as nc_saldos,
    rtrim(isnull(NC_MORA, '')) as nc_mora,
    rtrim(isnull(NC_OPCF, '')) as nc_opcf,
    case when isnull(VISIBLE_WEBAPP, 0) = 1 then 'Sí' else 'No' end as visible_webapp_desc,
    case when isnull(VISIBLE_SYS, 0) = 1 then 'Sí' else 'No' end as visible_sys_desc
from EXC_PERIODOS
order by ID_PERIODO desc;";

            return DbHelper.ExecuteListQuery<FrmAhExcedentesPeriodosListaDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el detalle del período y su tabla histórica de renta.
        /// </summary>
        public ErrorDto<FrmAhExcedentesPeriodosDetalleDto> Patrimonio_frmAH_ExcedentesPeriodos_Obtener(
            int codEmpresa,
            int periodoId)
        {
            const string sqlPeriodo = @"
select
    isnull(ID_PERIODO, 0) as id_periodo,
    INICIO as inicio,
    CORTE as corte,
    case
        when ESTADO in ('P', 'A') then 'Abierto'
        when ESTADO = 'C' then 'Cerrado'
        else rtrim(isnull(ESTADO, ''))
    end as estado,
    cast(isnull(RESERVA_PORC, 0) as decimal(18, 2)) as reserva_porc,
    cast(isnull(CAPITALIZA_PORC, 0) as decimal(18, 2)) as capitaliza_porc,
    cast(isnull(CAPITALIZA_RENTA_APLICA, 0) as bit) as capitaliza_renta_aplica,
    rtrim(isnull(NC_SALDOS, '')) as nc_saldos,
    rtrim(isnull(NC_MORA, '')) as nc_mora,
    rtrim(isnull(NC_OPCF, '')) as nc_opcf,
    rtrim(isnull(NC_FND_EXTRA, '')) as nc_fnd_extra,
    rtrim(isnull(NC_EXTRAORDINARIOS, '')) as nc_extraordinarios,
    rtrim(isnull(DOC_ASIENTO, '')) as doc_asiento,
    cast(isnull(VISIBLE_WEBAPP, 0) as bit) as visible_webapp,
    cast(isnull(VISIBLE_SYS, 0) as bit) as visible_sys,
    cast(isnull(MOSTRAR_EN_HISTORIAL, 0) as bit) as mostrar_en_historial,
    cast(isnull(MOSTRAR_TABLA_RENTA, 0) as bit) as mostrar_tabla_renta,
    rtrim(isnull(ESTADO_NOTAS, '')) as estado_notas,
    cast(isnull(MODO_AUTOMATICO, 0) as bit) as modo_automatico,
    rtrim(isnull(TIPO_APL_MENSUAL, '')) as tipo_apl_mensual,
    rtrim(isnull(TIPO_APL_MENSUAL_DESC, '')) as tipo_apl_mensual_desc
from vExc_Periodos_Consulta
where ID_PERIODO = @PeriodoId;";

            const string sqlRenta = @"
select
    cast(isnull(DESDE, 0) as decimal(18, 2)) as Desde,
    cast(isnull(HASTA, 0) as decimal(18, 2)) as Hasta,
    cast(isnull(PORCENTAJE, 0) as decimal(18, 2)) as Porcentaje
from EXC_RENTA_TABLA_H
where ID_PERIODO = @PeriodoId
order by DESDE;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                using var multi = conn.QueryMultiple(
                    $"{sqlPeriodo}{Environment.NewLine}{sqlRenta}",
                    new { PeriodoId = periodoId });

                var periodo = multi.ReadFirstOrDefault<ExcedentePeriodoDto>();
                var renta = multi.Read<RentaExcedenteDto>().ToList();

                if (periodo == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el período indicado.",
                        -2,
                        new FrmAhExcedentesPeriodosDetalleDto());
                }

                return DbHelper.CreateOkResponse(new FrmAhExcedentesPeriodosDetalleDto
                {
                    periodo = periodo,
                    renta_tabla = renta
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmAhExcedentesPeriodosDetalleDto());
            }
        }

        /// <summary>
        /// Obtiene el resumen del cierre aplicado/cargado del período.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesPeriodosResumenDto>> Patrimonio_frmAH_ExcedentesPeriodos_Resumen_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
select top 1
    cast(isnull(Excedente_Bruto, 0) as decimal(18, 2)) as excedente_bruto,
    cast(isnull(Reserva, 0) as decimal(18, 2)) as reserva,
    cast(isnull(Capitalizacion, 0) as decimal(18, 2)) as capitalizacion,
    cast(isnull(Renta, 0) as decimal(18, 2)) as renta,
    cast(isnull(Excedente_Neto, 0) as decimal(18, 2)) as excedente_neto,
    cast(isnull(Donacion, 0) as decimal(18, 2)) as donacion,
    cast(isnull(Ajuste_Aplicado, 0) as decimal(18, 2)) as ajuste_aplicado,
    cast(isnull(Ajuste_Cargado, 0) as decimal(18, 2)) as ajuste_cargado,
    cast(isnull(CEXD_Aplicado, 0) as decimal(18, 2)) as cexd_aplicado,
    cast(isnull(CEXD_Cargado, 0) as decimal(18, 2)) as cexd_cargado,
    cast(isnull(Mora_Aplicada, 0) as decimal(18, 2)) as mora_aplicada,
    cast(isnull(Mora_Cargada, 0) as decimal(18, 2)) as mora_cargada,
    cast(isnull(OPCF_Aplicado, 0) as decimal(18, 2)) as opcf_aplicado,
    cast(isnull(OPCF_Cargado, 0) as decimal(18, 2)) as opcf_cargado,
    cast(isnull(Capitalizado_Indivual, 0) as decimal(18, 2)) as capitalizado_indivual,
    cast(isnull(Excedente_Final, 0) as decimal(18, 2)) as excedente_final
from vExc_Periodos_Cierres_Resumen
where ID_PERIODO = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var row = conn.QueryFirstOrDefault(sql, new { PeriodoId = periodoId });

                if (row == null)
                {
                    return DbHelper.CreateOkResponse(new List<FrmAhExcedentesPeriodosResumenDto>());
                }

                var lista = new List<FrmAhExcedentesPeriodosResumenDto>
                {
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("Excedente Bruto", row.excedente_bruto, 0m, false, 1),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) Reserva", row.reserva, 0m, false, 2),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) Capitalizado", row.capitalizacion, 0m, false, 3),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) Renta", row.renta, 0m, false, 4),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("Excedente Neto", row.excedente_neto, 0m, true, 5),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) Donaciones", row.donacion, 0m, false, 6),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(+/-) Ajustes", row.ajuste_aplicado, row.ajuste_cargado, false, 7),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) Crédito s/Excedente", row.cexd_aplicado, row.cexd_cargado, false, 8),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) Morosidad", row.mora_aplicada, row.mora_cargada, false, 9),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("(-) O.P.C.F.", row.opcf_aplicado, row.opcf_cargado, false, 10),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("Capitaliza Extradordinario", row.capitalizado_indivual, 0m, false, 11),
                    Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen("Excedente Final", row.excedente_final, 0m, true, 12)
                };

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<FrmAhExcedentesPeriodosResumenDto>());
            }
        }

        private static FrmAhExcedentesPeriodosResumenDto Patrimonio_frmAH_ExcedentesPeriodos_CrearResumen(
            string concepto,
            decimal aplicado,
            decimal cargado,
            bool destacado,
            int orden)
        {
            return new FrmAhExcedentesPeriodosResumenDto
            {
                concepto = concepto,
                aplicado = aplicado,
                cargado = cargado,
                destacado = destacado,
                orden = orden
            };
        }
    }
}
