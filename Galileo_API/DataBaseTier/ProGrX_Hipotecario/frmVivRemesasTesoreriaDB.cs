using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivRemesasTesoreriaDB
    {
        /// <summary>
        /// Obtiene las 50 remesas de tesorería más recientes, incluyendo los campos calculados Casos y Monto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <returns>ErrorDto con la lista de remesas y sus datos asociados.</returns>
        private readonly PortalDB _portalDb;

        public FrmVivRemesasTesoreriaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Obtener(int codEmpresa)
        {
            var sql = @"select TOP 50
                T.*, isnull(D.Casos, 0) as Casos, isnull(D.Monto, 0) as Monto
                from viviendaRemesasTesoreria T
                left join vCrd_Hipotecario_Remesa_Tes_Rsm D on T.Remesa = D.Remesa
                order by T.RegistroFecha desc";
            return DbHelper.ExecuteListQuery<RemesasTesoreriaObtenerDto>(_portalDb, codEmpresa, sql, null);
        }
    }
}
