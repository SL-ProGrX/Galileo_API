using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvRepGeneralDB
    {
        private const int CodigoValidacion = -2;
        private const int EmpresaMaxima = 999999;
        private const string EmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string UsuarioRequerido =
            "El usuario es requerido.";

        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del reporte general de inventarios.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvRepGeneralDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las bodegas disponibles para el reporte general de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Bodegas_Obtener(int CodEmpresa)
        {
            var validacion =
                INV_RepGeneral_Empresa_Validar<DropDownListaGenericaModel>(
                    CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                SELECT
                    COD_BODEGA AS item,
                    DESCRIPCION AS descripcion
                FROM PV_BODEGAS
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene las unidades disponibles para el reporte general de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de unidades.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Unidades_Obtener(int CodEmpresa)
        {
            var validacion =
                INV_RepGeneral_Empresa_Validar<DropDownListaGenericaModel>(
                    CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                SELECT
                    COD_UNIDAD AS item,
                    DESCRIPCION AS descripcion
                FROM PV_UNIDADES
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene los departamentos disponibles para el reporte general de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de departamentos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Departamentos_Obtener(int CodEmpresa)
        {
            var validacion =
                INV_RepGeneral_Empresa_Validar<DropDownListaGenericaModel>(
                    CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                SELECT
                    COD_DEPARTAMENTO AS item,
                    DESCRIPCION AS descripcion
                FROM PV_DEPARTAMENTOS
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene los proveedores disponibles para el reporte general de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de proveedores.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Proveedores_Obtener(int CodEmpresa)
        {
            var validacion =
                INV_RepGeneral_Empresa_Validar<DropDownListaGenericaModel>(
                    CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                SELECT
                    COD_PROVEEDOR AS item,
                    DESCRIPCION AS descripcion
                FROM CXP_PROVEEDORES
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene las líneas de productos disponibles para el reporte general de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de líneas de productos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Lineas_Obtener(int CodEmpresa)
        {
            var validacion =
                INV_RepGeneral_Empresa_Validar<DropDownListaGenericaModel>(
                    CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                SELECT
                    COD_PRODCLAS AS item,
                    DESCRIPCION AS descripcion
                FROM PV_PROD_CLASIFICA
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene las UENS asignadas al usuario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Código del usuario.</param>
        /// <returns>Listado de UENS asignadas.</returns>
        public ErrorDto<List<CprUensLista>> INV_RepGeneral_Uens_Obtener(
            int CodEmpresa,
            string usuario)
        {
            var validacion =
                INV_RepGeneral_Empresa_Validar<CprUensLista>(CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return INV_RepGeneral_Validacion_Crear<CprUensLista>(
                    UsuarioRequerido);
            }

            const string query = """
                SELECT
                    R.COD_UNIDAD AS item,
                    U.DESCRIPCION AS descripcion,
                    (
                        SELECT TOP 1 CU.DESCRIPCION
                        FROM CNTX_UNIDADES CU
                        WHERE CU.COD_UNIDAD = U.CNTX_UNIDAD
                    ) AS cntx_unidad,
                    (
                        SELECT TOP 1 CC.DESCRIPCION
                        FROM CNTX_CENTRO_COSTOS CC
                        WHERE CC.COD_CENTRO_COSTO = U.CNTX_CENTRO_COSTO
                    ) AS cntx_centro_costo
                FROM CORE_UENS_USUARIOS_ROLES R
                LEFT JOIN CORE_UENS U
                    ON U.COD_UNIDAD = R.COD_UNIDAD
                WHERE R.CORE_USUARIO = @usuario
                """;

            return DbHelper.ExecuteListQuery<CprUensLista>(
                _portalDb,
                CodEmpresa,
                query,
                new { usuario = usuario.Trim() });
        }

        /// <summary>
        /// Valida el código de empresa recibido.
        /// </summary>
        /// <typeparam name="T">Tipo contenido por la respuesta.</typeparam>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Error de validación o null cuando el código es válido.</returns>
        private static ErrorDto<List<T>>?
            INV_RepGeneral_Empresa_Validar<T>(int CodEmpresa)
        {
            return CodEmpresa is <= 0 or > EmpresaMaxima
                ? INV_RepGeneral_Validacion_Crear<T>(EmpresaRequerida)
                : null;
        }

        /// <summary>
        /// Crea una respuesta de validación.
        /// </summary>
        /// <typeparam name="T">Tipo contenido por la respuesta.</typeparam>
        /// <param name="descripcion">Descripción del error.</param>
        /// <returns>Respuesta con código de validación.</returns>
        private static ErrorDto<List<T>>
            INV_RepGeneral_Validacion_Crear<T>(string descripcion)
        {
            return new ErrorDto<List<T>>
            {
                Code = CodigoValidacion,
                Description = descripcion,
                Result = new List<T>()
            };
        }
    }
}