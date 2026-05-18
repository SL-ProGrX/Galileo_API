using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXPeriodosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXPeriodosDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCntXPeriodosDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene la lista de periodos segun estado para la contabilidad seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxPeriodoListaData>> CntxPeriodos_Listar(
            int codEmpresa, int codConta, string estado)
        {
            estado = (estado ?? string.Empty).Trim().ToUpperInvariant();

            if (estado != "P" && estado != "C")
            {
                return new ErrorDto<List<CntxPeriodoListaData>>
                {
                    Code = -1,
                    Description = "El estado del periodo no es válido."
                };
            }

            string orderBy = estado == "P"
                ? " order by anio, mes "
                : " order by anio desc, mes desc ";

            string sql = @"
                select
                    anio,
                    mes,
                    estado
                from CntX_Periodos
                where cod_contabilidad = @codConta
                  and estado = @estado "
                + orderBy;

            return DbHelper.ExecuteListQuery<CntxPeriodoListaData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    codConta,
                    estado
                }
            );
        }
    }
}
