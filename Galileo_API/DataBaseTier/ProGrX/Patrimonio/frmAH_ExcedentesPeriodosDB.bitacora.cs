using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesPeriodosDB
    {
        /// <summary>
        /// Obtiene la bitácora del período según la etapa seleccionada.
        /// T = Todos, A = Aplicación Mensual, C = Cierre, X = Configuración.
        /// </summary>
        public ErrorDto<List<BitacoraExcedenteDto>> Ah_ExcedentesPeriodos_Bitacora_Lista(
            int codEmpresa,
            int periodoId,
            string etapa)
        {
            var etapaNormalizada = Ah_ExcedentesPeriodos_NormalizarEtapaBitacora(etapa);

            const string sql = @"
select
    isnull(LINEA, 0) as linea,
    REGISTRO_FECHA as registro_fecha,
    rtrim(isnull(REGISTRO_USUARIO, '')) as registro_usuario,
    rtrim(isnull(PROCESO_DESC, '')) as proceso_desc,
    rtrim(isnull(DETALLE, '')) as detalle,
    rtrim(isnull(TIPO_DOCUMENTO, '')) as tipo_documento,
    rtrim(isnull(COD_TRANSACCION, '')) as cod_transaccion,
    isnull(CASOS, 0) as casos,
    cast(isnull(MONTO, 0) as decimal(18, 2)) as monto,
    rtrim(isnull(convert(varchar(19), TIME_INICIO, 120), '')) as time_inicio,
    rtrim(isnull(convert(varchar(19), TIME_CORTE, 120), '')) as time_corte,
    rtrim(isnull(cast(DURACION as varchar(50)), '')) as duracion,
    rtrim(isnull(detalle, '')) as notas
from vExc_Periodos_Bitacora
where ID_PERIODO = @PeriodoId
  and (@Etapa = 'T' or ETAPA = @Etapa)
order by REGISTRO_FECHA desc, LINEA desc;";

            var parameters = new
            {
                PeriodoId = periodoId,
                Etapa = etapaNormalizada
            };

            return DbHelper.ExecuteListQuery<BitacoraExcedenteDto>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Registra una línea de bitácora funcional del período.
        /// </summary>
        public ErrorDto Ah_ExcedentesPeriodos_Bitacora_Registrar(
            int codEmpresa,
            FrmAhExcedentesPeriodosBitacoraRegistrarRequest request)
        {
            const string sql = @"
insert into EXC_PERIODOS_BITACORA
(
    ID_PERIODO,
    LINEA,
    COD_PROCESO,
    DETALLE,
    REGISTRO_FECHA,
    REGISTRO_USUARIO,
    TIPO_DOCUMENTO,
    COD_TRANSACCION,
    MONTO,
    CASOS,
    TIME_INICIO,
    TIME_CORTE,
    NOTAS
)
values
(
    @PeriodoId,
    (select isnull(max(LINEA), 0) + 1 from EXC_PERIODOS_BITACORA where ID_PERIODO = @PeriodoId),
    @CodProceso,
    @Detalle,
    dbo.MyGetDate(),
    @Usuario,
    @TipoDocumento,
    @CodTransaccion,
    @Monto,
    @Casos,
    dbo.MyGetDate(),
    dbo.MyGetDate(),
    @Notas
);";

            var parameters = new
            {
                PeriodoId = request.periodoId,
                CodProceso = Ah_ExcedentesPeriodos_NormalizarTexto(request.codProceso),
                Detalle = Ah_ExcedentesPeriodos_NormalizarTexto(request.detalle),
                Usuario = Ah_ExcedentesPeriodos_NormalizarTexto(request.usuario),
                TipoDocumento = Ah_ExcedentesPeriodos_NormalizarTexto(request.tipoDocumento),
                CodTransaccion = Ah_ExcedentesPeriodos_NormalizarTexto(request.codTransaccion),
                Monto = request.monto,
                Casos = request.casos,
                Notas = Ah_ExcedentesPeriodos_NormalizarTexto(request.notas)
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parameters);
        }

        private static string Ah_ExcedentesPeriodos_NormalizarEtapaBitacora(string? etapa)
        {
            var valor = (etapa ?? "T").Trim().ToUpperInvariant();
            return valor is "A" or "C" or "X" ? valor : "T";
        }
    }
}
