using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXConsolidaMapeoCuentasDB
    {
        private readonly PortalDB _portalDb;

        public FrmCntXConsolidaMapeoCuentasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de unidades activas para una contabilidad, usando DropDownListaGenericaModel.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> ConsolidaMapeoCuentas_ObtenerUnidades(int codEmpresa, int mContabilidad)
        {
            var sql = @"select Cod_Unidad as item, Descripcion as descripcion from CntX_Unidades where cod_Contabilidad = @mContabilidad and Activa = 1";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { mContabilidad });
        }
    }
}
