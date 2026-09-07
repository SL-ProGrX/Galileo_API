using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmInvRepConHorizontalDb
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvRepConHorizontalDb"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvRepConHorizontalDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene el listado de bodegas para el reporte horizontal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Obtener_Bodegas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_bodega as item, descripcion FROM pv_Bodegas");
        }

        #endregion
    }
}