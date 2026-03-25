using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepMovTipoDocumentoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCntXRepMovTipoDocumentoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta los tipos de asiento para una contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de tipos de asiento.</returns>
        public ErrorDto<List<CntXTipoAsientoDto>> TiposAsiento_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"select Tipo_Asiento, descripcion
                        from CntX_Tipos_Asientos
                        where cod_contabilidad = @CodContabilidad
                        order by Tipo_Asiento";
            return DbHelper.ExecuteListQuery<CntXTipoAsientoDto>(_portalDb, codEmpresa, sql, new { CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Consulta los asientos por tipo, año y mes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de consulta:
        /// </param>
        /// <returns>Lista de asientos.</returns>
        public ErrorDto<List<CntXAsientoDto>> Asientos_Lista(int codEmpresa, CntXAsientoParams param)
        {
            var sql = @"select Num_asiento, descripcion
                        from Cntx_Asientos
                        where cod_contabilidad = @CodContabilidad
                          and tipo_asiento = @TipoAsiento
                          and anio = @PeriodoAnio
                          and mes = @PeriodoMes
                        order by Num_Asiento";
            return DbHelper.ExecuteListQuery<CntXAsientoDto>(_portalDb, codEmpresa, sql, param);
        }
    }
}
