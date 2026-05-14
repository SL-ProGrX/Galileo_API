using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndPlazosFrecuenciasDb
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 18; // Módulo Fondos
        private const string consAgregar = "agregar";
        private const string SpPlazosGuardar = "spFnd_CDP_Plazos_Vencimiento_Add";
        private const string SpPlazosEliminar = "spFnd_CDP_Plazos_Vencimiento_Delete";
        private const string SpFrecuenciaGuardar = "spFnd_CDP_Frecuencia_Cupon_Add";
        private const string SpFrecuenciaEliminar = "spFnd_CDP_Frecuencia_Cupon_Delete";

        public FrmFndPlazosFrecuenciasDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _securityMainDb = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de plazos de vencimiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con la lista de plazos de vencimiento.</returns>
        public ErrorDto<List<FndPlazoVencimientoModel>> PlazosVencimiento_Obtener(int codEmpresa)
        {
            const string query = @"
                    SELECT
                        ID_PLAZO AS IdPlazo,
                        PLAZO,
                        PLAZO_MESES AS PlazoMeses,
                        PLAZO_DIAS AS PlazoDias,
                        ESTADO
                    FROM dbo.FND_CDP_PLAZOS
                    ORDER BY PLAZO_MESES;";

            return DbHelper.ExecuteListQuery<FndPlazoVencimientoModel>(new PortalDB(_config), codEmpresa, query);
        }

        /// <summary>
        /// Inserta o actualiza un plazo de vencimiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="plazo">Modelo con los datos del plazo a guardar.</param>
        /// <param name="mov">Tipo de movimiento (consAgregar o "modificar").</param>
        /// <returns>ErrorDto con el Id generado o actualizado.</returns>
        public ErrorDto<FndPlazoVencimientoSaveResult> PlazosVencimiento_Guardar(int codEmpresa, string usuario, FndPlazoVencimientoModel plazo, string mov)
        {
            if (plazo is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del plazo son requeridos.",
                    -2,
                    new FndPlazoVencimientoSaveResult());
            }

            var result = EjecutarSpSingle<FndPlazoVencimientoSaveResult>(
                codEmpresa,
                SpPlazosGuardar,
                CrearParametrosPlazo(plazo, usuario));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                CrearDetallePlazo(plazo, result.Result, mov),
                ObtenerMovimiento(mov));

            return result;
        }

        /// <summary>
        /// Elimina un plazo de vencimiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="idPlazo">Id del plazo a eliminar.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <returns>ErrorDto con el Id eliminado.</returns>
        public ErrorDto<FndPlazoVencimientoSaveResult> PlazosVencimiento_Eliminar(int codEmpresa, int idPlazo, string usuario)
        {
            var result = EjecutarSpSingle<FndPlazoVencimientoSaveResult>(
                codEmpresa,
                SpPlazosEliminar,
                new { Id = idPlazo, Usuario = NormalizarTexto(usuario) });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(codEmpresa, usuario, $"Plazo de Inversión Id:{idPlazo}", "Elimina - WEB");
            return result;
        }

        /// <summary>
        /// Obtiene una lista de frecuencias de pago de cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con la lista de frecuencias de cupón.</returns>
        public ErrorDto<List<FndFrecuenciaCuponModel>> FrecuenciaCupon_Obtener(int codEmpresa)
        {
            const string query = @"
                    SELECT
                        ID_FRECUENCIACUPON AS IdFrecuenciaCupon,
                        CUPON,
                        FRECUENCIA_MESES AS FrecuenciaMeses,
                        FRECUENCIA_DIAS AS FrecuenciaDias,
                        ESTADO
                    FROM dbo.FND_CDP_FRECUENCIACUPONES
                    ORDER BY FRECUENCIA_MESES;";

            return DbHelper.ExecuteListQuery<FndFrecuenciaCuponModel>(new PortalDB(_config), codEmpresa, query);
        }

        /// <summary>
        /// Inserta o actualiza una frecuencia de pago de cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="frecuencia">Modelo con los datos de la frecuencia a guardar.</param>
        /// <param name="mov">Tipo de movimiento (consAgregar o "modificar").</param>
        /// <returns>ErrorDto con el Id generado o actualizado.</returns>
        public ErrorDto<FndFrecuenciaCuponSaveResult> FrecuenciaCupon_Guardar(int codEmpresa, string usuario, FndFrecuenciaCuponModel frecuencia, string mov)
        {
            if (frecuencia is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos de la frecuencia son requeridos.",
                    -2,
                    new FndFrecuenciaCuponSaveResult());
            }

            var result = EjecutarSpSingle<FndFrecuenciaCuponSaveResult>(
                codEmpresa,
                SpFrecuenciaGuardar,
                CrearParametrosFrecuencia(frecuencia, usuario));

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                CrearDetalleFrecuencia(frecuencia, result.Result, mov),
                ObtenerMovimiento(mov));

            return result;
        }

        /// <summary>
        /// Elimina una frecuencia de pago de cupón.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="idFrecuenciaCupon">Id de la frecuencia a eliminar.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <returns>ErrorDto con el Id eliminado.</returns>
        public ErrorDto<FndFrecuenciaCuponSaveResult> FrecuenciaCupon_Eliminar(int codEmpresa, int idFrecuenciaCupon, string usuario)
        {
            var result = EjecutarSpSingle<FndFrecuenciaCuponSaveResult>(
                codEmpresa,
                SpFrecuenciaEliminar,
                new { Id = idFrecuenciaCupon, Usuario = NormalizarTexto(usuario) });

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(codEmpresa, usuario, $"Frecuencia Pago Cupón Id:{idFrecuenciaCupon}", "Elimina - WEB");
            return result;
        }

        private ErrorDto<T> EjecutarSpSingle<T>(int codEmpresa, string procedimiento, object parametros)
        {
            if (!EsProcedimientoPermitido(procedimiento))
            {
                return DbHelper.CreateErrorResponse("Procedimiento almacenado no permitido.", -2, Activator.CreateInstance<T>());
            }

            var result = DbHelper.ExecuteStoredProcedureSingle<T>(
                new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa),
                procedimiento,
                default,
                parametros);

            return new ErrorDto<T>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? Activator.CreateInstance<T>()
            };
        }

        private static bool EsProcedimientoPermitido(string procedimiento)
        {
            return procedimiento is SpPlazosGuardar
                or SpPlazosEliminar
                or SpFrecuenciaGuardar
                or SpFrecuenciaEliminar;
        }

        private static object CrearParametrosPlazo(FndPlazoVencimientoModel plazo, string usuario)
        {
            return new
            {
                Id = plazo.IdPlazo,
                Descripcion = NormalizarTexto(plazo.Plazo),
                pMeses = plazo.PlazoMeses,
                Activo = plazo.Estado,
                Usuario = NormalizarTexto(usuario)
            };
        }

        private static object CrearParametrosFrecuencia(FndFrecuenciaCuponModel frecuencia, string usuario)
        {
            return new
            {
                Id = frecuencia.IdFrecuenciaCupon,
                Descripcion = NormalizarTexto(frecuencia.Cupon),
                pMeses = frecuencia.FrecuenciaMeses,
                Activo = frecuencia.Estado,
                Usuario = NormalizarTexto(usuario)
            };
        }

        private static string CrearDetallePlazo(FndPlazoVencimientoModel plazo, FndPlazoVencimientoSaveResult? result, string mov)
        {
            var id = EsAgregar(mov) ? result?.Id ?? plazo.IdPlazo : plazo.IdPlazo;
            return $"Plazo de Inversión Id:{id} - {NormalizarTexto(plazo.Plazo)}";
        }

        private static string CrearDetalleFrecuencia(FndFrecuenciaCuponModel frecuencia, FndFrecuenciaCuponSaveResult? result, string mov)
        {
            var id = EsAgregar(mov) ? result?.Id ?? frecuencia.IdFrecuenciaCupon : frecuencia.IdFrecuenciaCupon;
            return $"Frecuencia Pago Cupón Id:{id} - {NormalizarTexto(frecuencia.Cupon)}";
        }

        private static string ObtenerMovimiento(string mov) => EsAgregar(mov) ? "Registra - WEB" : "Modifica - WEB";

        private static bool EsAgregar(string mov) => string.Equals(NormalizarTexto(mov), consAgregar, StringComparison.OrdinalIgnoreCase);

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            var bitacora = new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            };

            _securityMainDb.Bitacora(bitacora);
        }
    }
}