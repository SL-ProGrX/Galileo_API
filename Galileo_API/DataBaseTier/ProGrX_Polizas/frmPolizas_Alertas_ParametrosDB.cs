using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasAlertasParametrosDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmPolizasAlertasParametrosDb(IConfiguration config)
       {
          _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
       }

        /// <summary>
        /// Método para obtener los parámetros de alertas de pólizas para una empresa específica. Devuelve un objeto PolAlertasParametrosDto que contiene la unidad de tiempo, los valores de alerta roja y amarilla, y la información de contacto. La unidad de tiempo se traduce a español para facilitar su comprensión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<PolAlertasParametrosDto?> POL_Alertas_Parametros_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                SELECT TOP 1
                    UnidadTiempo,
                    AlertaRoja,
                    AlertaAmarilla,
                    Contacto_Oficina   AS ContactoOficina,
                    Contacto_Telefono  AS ContactoTelefono,
                    Contacto_Email     AS ContactoEmail,
                    CASE 
                        WHEN UnidadTiempo = 'MINUTE' THEN 'Minutos'
                        WHEN UnidadTiempo = 'DAY'    THEN 'Días'
                        WHEN UnidadTiempo = 'HOUR'   THEN 'Horas'
                        ELSE ''
                    END AS UnidadTiempoEsp
                FROM POLIZAS_ALERTAS_PARAMETROS";

                return conn.QueryFirstOrDefault<PolAlertasParametrosDto>(query);
            });
        }

        /// <summary>
        /// Método para guardar o actualizar los parámetros de alertas de pólizas para una empresa específica. Recibe el código de empresa, el usuario que realiza la acción y un objeto PolAlertasParametrosGuardarDto que contiene los nuevos valores de configuración. El método ejecuta un procedimiento almacenado para insertar o actualizar los parámetros en la base de datos y registra la acción en la bitácora de movimientos para mantener un historial de cambios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto POL_Alertas_Parametros_Guardar(int CodEmpresa, string Usuario, PolAlertasParametrosGuardarDto param)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                // VB6 usa: exec spPolizas_Alertas_Parametros_Add @Unidad, @Rojo, @Amarillo, @Usuario, @Oficina, @Telefono, @Email
                const string execSp = @"
                exec spPolizas_Alertas_Parametros_Add
                    @UnidadTiempo,
                    @AlertaRoja,
                    @AlertaAmarilla,
                    @Usuario,
                    @ContactoOficina,
                    @ContactoTelefono,
                    @ContactoEmail";

                // VB6: If rs!Pass = 1 -> Bitacora(rs!Movimiento, rs!Detalle); si no, muestra rs!Mensaje
                var resultado = conn.QueryFirstOrDefault<SpResultado>(execSp, new
                {
                    UnidadTiempo = (param.UnidadTiempo ?? "DAY").Trim().ToUpper(),
                    param.AlertaRoja,
                    param.AlertaAmarilla,
                    Usuario = Usuario?.Trim(),
                    param.ContactoOficina,
                    param.ContactoTelefono,
                    param.ContactoEmail
                });

                if (resultado is null || resultado.Pass != 1)
                    return DbHelper.ErrorResponse($"No fue posible registrar el semáforo, verifique! {resultado?.Mensaje}".Trim());

                RegistrarBitacoraSp(CodEmpresa, Usuario, resultado, "MODIFICA",
                    $"Semáforo Alertas Pólizas: Unidad={param.UnidadTiempo}, Rojo={param.AlertaRoja}, Amarillo={param.AlertaAmarilla}");

                return DbHelper.OkResponse("Semáforo registrado correctamente");
            }
            catch (Exception ex)
            {
                // Log full exception details for diagnostics without changing the external error contract.
                Console.Error.WriteLine($"[POL_Alertas_Parametros_Guardar] Unexpected exception: {ex}");
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método para obtener la lista de correos electrónicos configurados para recibir alertas de pólizas. Devuelve una lista de objetos PolAlertasEmailDto que contienen el ID del registro, el correo electrónico, el usuario que insertó el registro y la fecha de inserción. La lista se ordena por ID de registro en orden descendente para mostrar los registros más recientes primero.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<PolAlertasEmailDto>> POL_Alertas_Email_Listar(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                SELECT
                    IdRegistro,
                    Email,
                    UsuarioInserta,
                    FechaInserta
                FROM POLIZAS_ALERTAS_EMAIL
                ORDER BY IdRegistro DESC";

                return conn.Query<PolAlertasEmailDto>(query).ToList();
            });
        }

        /// <summary>
        /// Método para agregar un nuevo correo electrónico a la lista de alertas de pólizas. 
        /// Recibe el código de empresa, el usuario que realiza la acción y un objeto PolAlertasEmailAgregarDto
        /// que contiene el correo electrónico a agregar. El método valida que el correo no esté vacío y luego ejecuta un procedimiento almacenado para insertar el nuevo registro. Además, registra la acción en la bitácora de movimientos para mantener un historial de cambios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto POL_Alertas_Email_Agregar(int CodEmpresa, string Usuario, PolAlertasEmailAgregarDto dto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var email = (dto.Email ?? "").Trim();
                if (string.IsNullOrWhiteSpace(email))
                    return DbHelper.ErrorResponse("El email es requerido");

                // Opción A: mantener SP (es lo más fiel a VB6)
                const string execSp = @"exec spPolizas_Alertas_Email_Add @Email, @Usuario";
                // VB6: If rs!Pass = 1 -> Bitacora(rs!Movimiento, rs!Detalle); si no, muestra rs!Mensaje
                var resultado = conn.QueryFirstOrDefault<SpResultado>(execSp, new
                {
                    Email = email,
                    Usuario = Usuario?.Trim()
                });

                if (resultado is null || resultado.Pass != 1)
                    return DbHelper.ErrorResponse($"No fue posible registrar el correo, verifique! {resultado?.Mensaje}".Trim());

                RegistrarBitacoraSp(CodEmpresa, Usuario, resultado, "INSERTA",
                    $"Email alertas pólizas: {email}");

                return DbHelper.OkResponse("Correo registrado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método para eliminar uno o más correos electrónicos de la lista de alertas de pólizas. Recibe el código de empresa, el usuario que realiza la acción y una lista de IDs de registros a eliminar. El método valida que se haya seleccionado al menos un correo para eliminar y luego ejecuta un procedimiento almacenado para cada ID proporcionado. Además, registra cada eliminación en la bitácora de movimientos para mantener un historial de cambios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ErrorDto POL_Alertas_Email_Eliminar(int CodEmpresa, string Usuario, int ids)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                const string execSp = @"exec spPolizas_Alertas_Email_Delete @IdRegistro, @Usuario";

                conn.Execute(execSp, new
                {
                    IdRegistro = ids,
                    Usuario = Usuario?.Trim()
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (Usuario ?? "").ToUpper(),
                    Movimiento = "ELIMINA - WEB",
                    DetalleMovimiento = $"Email alertas pólizas eliminado. IdRegistro={ids}",
                    Modulo = 11
                });

                return DbHelper.OkResponse("Correos eliminados correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Resultado que devuelven los SP spPolizas_Alertas_*_Add (VB6: rs!Pass, rs!Movimiento, rs!Detalle, rs!Mensaje).
        /// </summary>
        private sealed class SpResultado
        {
            public int Pass { get; set; }
            public string? Movimiento { get; set; }
            public string? Detalle { get; set; }
            public string? Mensaje { get; set; }
        }

        /// <summary>
        /// Registra la bitácora con el movimiento/detalle que devuelve el SP (como VB6); si no vienen, usa los valores por defecto.
        /// </summary>
        private void RegistrarBitacoraSp(int codEmpresa, string? usuario, SpResultado resultado, string movimientoDefecto, string detalleDefecto)
        {
            var movimiento = string.IsNullOrWhiteSpace(resultado.Movimiento) ? movimientoDefecto : resultado.Movimiento.Trim();
            var detalle = string.IsNullOrWhiteSpace(resultado.Detalle) ? detalleDefecto : resultado.Detalle.Trim();

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? "").ToUpper(),
                Movimiento = $"{movimiento} - WEB",
                DetalleMovimiento = detalle,
                Modulo = 11
            });
        }

    }
}
