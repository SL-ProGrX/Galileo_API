using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvTransacReportesDB
    {
        private const int CodigoValidacion = -2;
        private const int EmpresaMaxima = 999999;
        private const string EmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string TipoInvalido =
            "El tipo de movimiento no es v&aacute;lido.";

        private static readonly HashSet<string> TiposMovimiento =
            new(StringComparer.Ordinal) { "E", "S", "T", "R" };

        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos de los reportes de movimientos a inventarios.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTransacReportesDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las causas de entrada, salida, traslado o requisición según el tipo
        /// de movimiento. Equivale a sbInvESCombo del VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="tipo">Tipo de movimiento (E, S, T, R).</param>
        /// <returns>Listado de causas del tipo indicado.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Causas_Obtener(int CodEmpresa, string tipo)
        {
            var validacion = INV_TransacReportes_Empresa_Validar(CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (!TiposMovimiento.Contains(tipoNormalizado))
            {
                return INV_TransacReportes_Validacion_Crear(TipoInvalido);
            }

            const string query = """
                SELECT
                    RTRIM(COD_ENTSAL) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM PV_ENTRADA_SALIDA
                WHERE TIPO = @tipo
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query,
                new { tipo = tipoNormalizado });
        }

        /// <summary>
        /// Obtiene los usuarios para la búsqueda F4 del filtro de usuario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de usuarios (nombre, descripción).</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Usuarios_Obtener(int CodEmpresa)
        {
            var validacion = INV_TransacReportes_Empresa_Validar(CodEmpresa);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                SELECT
                    RTRIM(NOMBRE) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM USUARIOS
                ORDER BY NOMBRE
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Valida el código de empresa recibido.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Error de validación o null cuando el código es válido.</returns>
        private static ErrorDto<List<DropDownListaGenericaModel>>?
            INV_TransacReportes_Empresa_Validar(int CodEmpresa)
        {
            return CodEmpresa is <= 0 or > EmpresaMaxima
                ? INV_TransacReportes_Validacion_Crear(EmpresaRequerida)
                : null;
        }

        /// <summary>
        /// Crea una respuesta de validación.
        /// </summary>
        /// <param name="descripcion">Descripción del error.</param>
        /// <returns>Respuesta con código de validación.</returns>
        private static ErrorDto<List<DropDownListaGenericaModel>>
            INV_TransacReportes_Validacion_Crear(string descripcion)
        {
            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = CodigoValidacion,
                Description = descripcion,
                Result = new List<DropDownListaGenericaModel>()
            };
        }
    }
}
