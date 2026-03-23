using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXPeriodosReversaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 20;

        public FrmCntXPeriodosReversaDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config))
        {
        }

        public FrmCntXPeriodosReversaDb(PortalDB portalDb, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDb;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtener cierres de periodos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXPeriodosData>> CntXPeriodos_Cierres_Obtener(int codEmpresa, int codConta)
        {
            const string sql = @"select top 60 
                periodo_corte, estado, cierre_fecha, cierre_usuario
                from cntx_periodos
                where cod_contabilidad = @codConta and estado = 'C'
                order by periodo_corte desc;";

            return DbHelper.ExecuteListQuery<CntXPeriodosData>(
                _portalDb,
                codEmpresa,
                sql,
                new { codConta }
            );
        }

        /// <summary>
        /// Obtener bitacora de movimientos sobre periodos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXPeriodosLogData>> CntXPeriodos_Bitacora_Obtener(int codEmpresa, ReversaPeriodoRequest request)
        {
            const string sql = @" select 
                p.periodo_corte as corte, l.movimiento, l.registro_fecha, l.registro_usuario 
                from cntx_periodos_log l
                inner join cntx_periodos p
                    on l.cod_contabilidad = p.cod_contabilidad
                   and l.anio = p.anio
                   and l.mes = p.mes
                where l.cod_contabilidad = @codConta
                  and l.anio = @anio
                  and l.mes = @mes
                order by l.registro_fecha desc;";

            return DbHelper.ExecuteListQuery<CntXPeriodosLogData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    codConta = request.codigo_contabilidad,
                    anio = request.cierre.Year,
                    mes = request.cierre.Month
                }
            );
        }

        /// <summary>
        /// Reversar cierre de periodo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXPeriodos_Reversar(int codEmpresa, ReversaPeriodoRequest request)
        {
            try
            {
                const string sql = @"
                    exec spCntX_Periodo_Reversa
                        @codConta,
                        @anio,
                        @mes,
                        @notas,
                        @usuario;";

                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        codConta = request.codigo_contabilidad,
                        anio = request.cierre.Year,
                        mes = request.cierre.Month,
                        notas = request.notas,
                        usuario = request.usuario
                    }
                );

                if (resp.Code < 0)
                    return resp;

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Reversa",
                    $"Periodo Contable: {request.cierre:yyyy-MM-dd}"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Cierre Reversado Satisfactoriamente..."
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Registrar en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
