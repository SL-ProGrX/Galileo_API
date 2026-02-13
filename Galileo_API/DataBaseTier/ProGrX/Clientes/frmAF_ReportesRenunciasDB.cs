using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models; // Asegúrate de tener el namespace correcto para DropDownListaGenericaModel
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFReportesRenunciasDB
    {
        private readonly PortalDB _portalDb;

        public FrmAFReportesRenunciasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta la lista de oficinas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de oficinas (item, descripcion).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AfReportesRenunciasOficinas_Obtener(int codEmpresa)
        {
            var query = @"
                SELECT cod_Oficina as item, rtrim(descripcion) as descripcion
                FROM SIF_Oficinas
                ORDER BY descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }
    }
}
