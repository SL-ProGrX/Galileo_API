using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene el último periodo cerrado para el tab Aplicaciones.
        /// </summary>
        public ErrorDto<ExcPeriodosDto?> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(int codEmpresa)
        {
            const string sql = @"
select top 1
    CAST(IdX AS varchar(20)) as idx,
    RTRIM(ItmX) as itmx,
    RTRIM(ISNULL(Estado, '')) as estado
from vExc_Periodos
where idx in (select max(idx) from vExc_Periodos where estado = 'C');";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<ExcPeriodosDto>(sql);
                return DbHelper.CreateOkResponse<ExcPeriodosDto?>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<ExcPeriodosDto?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la bitácora del periodo en etapa de cierre/aplicaciones.
        /// </summary>
        public ErrorDto<List<BitacoraExcedenteDto>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Log_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
select
    ISNULL(Linea, 0) as linea,
    Registro_Fecha,
    RTRIM(ISNULL(Registro_Usuario, '')) as Registro_Usuario,
    ISNULL(Linea, 0) as Transaccion,
    RTRIM(ISNULL(Detalle, '')) as Detalle,
    RTRIM(ISNULL(Tipo_Documento, '')) as Tipo_Documento,
    RTRIM(ISNULL(Cod_Transaccion, '')) as Cod_Transaccion
from vExc_Periodos_Bitacora
where id_periodo = @PeriodoId
  and Etapa = 'C'
  and Cod_Proceso not in ('01', '12')
order by Registro_Fecha asc;";

            return DbHelper.ExecuteListQuery<BitacoraExcedenteDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { PeriodoId = periodoId });
        }

        /// <summary>
        /// Obtiene la lista de procesos pendientes de aplicaciones para el periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
exec spExc_Aplicaciones_Procesos_Pendientes @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.Query<dynamic>(
                    sql,
                    new { PeriodoId = periodoId })
                    .Select(x =>
                    {
                        x.item = x.COD_PROCESO?.Trim();
                        x.descripcion = x.DESCRIPCION?.Trim();
                        return x;
                    })
                    .ToList();

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.item,
                    descripcion = x.descripcion
                }).ToList();

                return DbHelper.CreateOkResponse<List<DropDownListaGenericaModel>>(lista);  
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta la separación de salidas del periodo.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            const string sql = @"
exec spExc_Procesos_Salidas_Separa @PeriodoId, @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    PeriodoId = periodoId,
                    Usuario = usuario
                });
        }

        /// <summary>
        /// Obtiene las salidas pendientes de traslado a fondos del periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
select
    RTRIM(COD_SALIDA) as item,
    RTRIM(DESCRIPCION) as descripcion
from EXC_TIPOS_SALIDAS
where DESTINO_PLAN <> ''
  and ('Salida: ' + COD_SALIDA) not in
  (
      select DETALLE
      from EXC_PERIODOS_BITACORA
      where COD_PROCESO = '12'
        and ID_PERIODO = @PeriodoId
  )
  and COD_SALIDA in
  (
      select COD_SALIDA
      from vExc_Cierre_Salida_Rsm
      where ID_PERIODO = @PeriodoId
        and EXCEDENTE_FINAL > 0
  );";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { PeriodoId = periodoId });
        }

        /// <summary>
        /// Ejecuta el traslado a fondos de una salida del periodo.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
            int codEmpresa,
            int periodoId,
            string salida,
            string usuario)
        {
            const string sql = @"
exec spExc_Procesos_Salidas_Fondos @PeriodoId, @Salida, @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    PeriodoId = periodoId,
                    Salida = salida,
                    Usuario = usuario
                });
        }
    }
}
