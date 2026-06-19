using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {

        private readonly PortalDB _portalDb;

        public FrmAhExcedentesMensualesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los periodos de excedentes disponibles.
        /// Este método se mantiene en el archivo principal porque se comparte entre tabs.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de periodos de excedentes.</returns>
        public ErrorDto<List<ExcPeriodosDto>> AH_ExcedentesMensuales_Periodos_Lista(int codEmpresa)
        {
            const string sql = @"
                            SELECT
                                CAST(IdX AS varchar(20)) AS idx,
                                RTRIM(ItmX) AS itmx,
                                RTRIM(ISNULL(Estado, '')) AS estado
                            FROM vExc_Periodos
                            ORDER BY IdX DESC;";

            return DbHelper.ExecuteListQuery<ExcPeriodosDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene la lista de cortes del periodo.
        /// Este método se mantiene en el archivo principal porque se comparte entre tabs.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">Periodo seleccionado.</param>
        /// <returns>Lista de cortes del periodo.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Cortes_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
                    SELECT
                        RTRIM(ISNULL(CORTE_DATETIME_STR, '')) AS item,
                        RTRIM(ISNULL(CORTE_DATE_STR, '')) AS descripcion
                    FROM vExc_Periodos_Cortes
                    WHERE id_periodo = @PeriodoId
                    ORDER BY corte DESC;";

            var parameters = new
            {
                PeriodoId = periodoId
            };

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Valida si ya existe una bitácora previa para el proceso y detalle indicados.
        /// </summary>
        public ErrorDto<bool> AH_ExcedentesMensuales_Bitacora_Valida(
            int codEmpresa,
            int periodoId,
            string codProceso,
            string detalle)
        {
            const string sql = @"
select count(*) as Existe
from EXC_PERIODOS_BITACORA
where ID_PERIODO = @PeriodoId
  and COD_PROCESO = @CodProceso
  and DETALLE = @Detalle;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var existe = conn.QueryFirstOrDefault<int>(
                    sql,
                    new
                    {
                        PeriodoId = periodoId,
                        CodProceso = codProceso,
                        Detalle = detalle
                    });

                return DbHelper.CreateOkResponse(existe == 0);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
        }

        /// <summary>
        /// Registra una línea de bitácora de excedentes.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Bitacora_Registrar(
            int codEmpresa,
            int periodoId,
            string codProceso,
            string detalle,
            string usuario,
            string tipoDocumento = "",
            string codTransaccion = "")
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
    TIME_CORTE
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
    0,
    0,
    dbo.MyGetDate(),
    dbo.MyGetDate()
);";

            var parameters = new
            {
                PeriodoId = periodoId,
                CodProceso = codProceso,
                Detalle = detalle,
                Usuario = usuario,
                TipoDocumento = tipoDocumento,
                CodTransaccion = codTransaccion
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parameters);
        }
    }
}
