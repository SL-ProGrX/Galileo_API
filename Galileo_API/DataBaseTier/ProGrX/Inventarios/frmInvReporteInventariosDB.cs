using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvReporteInventariosDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvReporteInventariosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvReporteInventariosDB(IConfiguration config)
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
        /// Obtiene el listado de bodegas para el reporte de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<BodegaReporteInvMCdto>> Obtener_Bodegas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<BodegaReporteInvMCdto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_BODEGA, DESCRIPCION FROM PV_BODEGAS");
        }

        /// <summary>
        /// Obtiene el listado de líneas para el reporte de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de líneas.</returns>
        public ErrorDto<List<LineasInvMCdto>> Obtener_Lineas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<LineasInvMCdto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_PRODCLAS, DESCRIPCION FROM PV_PROD_CLASIFICA");
        }

        #endregion
    }
}