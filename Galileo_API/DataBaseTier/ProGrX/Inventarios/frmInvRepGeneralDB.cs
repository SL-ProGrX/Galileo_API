using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvRepGeneralDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvRepGeneralDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvRepGeneralDB(IConfiguration config)
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
        /// Obtiene el listado de bodegas para el reporte general de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<BodegaReporteInvDto>> Obtener_Bodegas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<BodegaReporteInvDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_BODEGA, DESCRIPCION FROM PV_BODEGAS");
        }

        /// <summary>
        /// Obtiene el listado de unidades para el reporte general de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de unidades.</returns>
        public ErrorDto<List<UnidadesReporteInvDto>> Obtener_Unidades(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<UnidadesReporteInvDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_UNIDAD, DESCRIPCION FROM PV_UNIDADES");
        }

        /// <summary>
        /// Obtiene el listado de departamentos para el reporte general de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de departamentos.</returns>
        public ErrorDto<List<DepartamentoReporteInvDto>> Obtener_Departamento(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DepartamentoReporteInvDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_DEPARTAMENTO, DESCRIPCION FROM PV_DEPARTAMENTOS");
        }

        /// <summary>
        /// Obtiene el listado de proveedores para el reporte general de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de proveedores.</returns>
        public ErrorDto<List<ProveedoresInvDto>> Obtener_Proveedor(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<ProveedoresInvDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_PROVEEDOR, DESCRIPCION FROM CXP_PROVEEDORES");
        }

        /// <summary>
        /// Obtiene el listado de líneas para el reporte general de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de líneas.</returns>
        public ErrorDto<List<LineasInvDto>> Obtener_Lineas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<LineasInvDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_PRODCLAS, DESCRIPCION FROM PV_PROD_CLASIFICA");
        }

        /// <summary>
        /// Obtiene las UENS asignadas al usuario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario a consultar.</param>
        /// <returns>Listado de UENS del usuario.</returns>
        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.ExecuteListQuery<CprUensLista>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT R.COD_UNIDAD AS item,
                         U.DESCRIPCION,
                         (SELECT TOP 1 DESCRIPCION FROM CNTX_UNIDADES WHERE COD_UNIDAD = U.CNTX_UNIDAD) AS CNTX_UNIDAD,
                         (SELECT TOP 1 DESCRIPCION FROM CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = U.CNTX_CENTRO_COSTO) AS CNTX_CENTRO_COSTO
                  FROM CORE_UENS_USUARIOS_ROLES R
                  LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
                  WHERE R.CORE_USUARIO = @usuario",
                new { usuario });
        }

        #endregion
    }
}