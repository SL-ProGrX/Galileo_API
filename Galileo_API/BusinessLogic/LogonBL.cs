using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Security.Cryptography;

namespace Galileo.BusinessLogic
{
    /// <summary>
    /// Clase de lógica de negocio para la autenticación, recuperación de contraseña y validaciones TFA.
    /// </summary>
    public class LogonBL
    {
        readonly LogonDB logonDB;
        
        /// <summary>
        /// Inicializa una nueva instancia de la clase LogonBL con la configuración proporcionada.
        /// </summary>
        /// <param name="_config"></param>
        public LogonBL(IConfiguration _config)
        {
            logonDB = new LogonDB(_config);
        }

        /// <summary>
        /// Obtiene la configuración o estado de intentos de autenticación.
        /// </summary>
        /// <returns></returns>
        public IntentosObtenerDto IntentosObtener()
        {
            var result = logonDB.IntentosObtener();
            if (result == null)
            {
                // Return a default instance or handle as needed
                return new IntentosObtenerDto();
            }
            return result;
        }

        /// <summary>
        /// Método de autenticación de SSecurity. Valida las credenciales mediante spSEG_Logon
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto LoginObtener(LoginObtenerDto req)
        {
            var response = logonDB.LoginObtener(req);

            if (response is null)
            {
                return new ErrorDto
                {
                    Code = 1,
                    Description = "No fue posible completar el inicio de sesión."
                };
            }

            return response;
        }

        /// <summary>
        /// Obtiene la lista de clientes y empresas asociadas al usuario indicado.
        /// </summary>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<ClientesEmpresasObtenerDto>> ClientesObtener(string Usuario)
        {
            return logonDB.ClientesObtener(Usuario);
        }


        /// <summary>
        /// Obtiene los datos necesarios para la autenticación de dos factores (TFA) del usuario especificado.
        /// </summary>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public TfaData TFA_Data_Load(string Usuario)
        {
            return logonDB.TFA_Data_Load(Usuario);
        }

        /// <summary>
        /// Envía un código de verificación al correo electrónico del usuario para la autenticación de dos factores (TFA).
        /// </summary>
        /// <param name="Usuario"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<ErrorDto> TFA_Codigo_EnviarMAIL(string Usuario, string email)
        {
            return logonDB.TFA_Codigo_EnviarMAIL(Usuario, email);
        }


        /// <summary>
        /// Valida el código de verificación proporcionado por el usuario para la autenticación de dos factores (TFA).
        /// </summary>
        /// <param name="Usuario"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto TFA_Codigo_Validar(string Usuario, string codigo)
        {
            return logonDB.TFA_Codigo_Validar(Usuario, codigo);
        }

        /// <summary>
        /// Genera un token aleatorio de la longitud especificada.
        /// </summary>
        /// <param name="longitud"></param>
        /// <returns></returns>
        static string GenerarToken(int longitud)
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(Math.Max(longitud, 16)))
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        /// <summary>
        /// Valida los datos del usuario y el correo electrónico proporcionados para la recuperación de contraseña.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public int ValidarDatos(string usuario, string email)
        {
            return logonDB.ValidarDatos(usuario, email);
        }

        /// <summary>
        /// Valida el token proporcionado para la renovación de contraseña del usuario especificado.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public int ValidarToken(string usuario, string token)
        {
            return logonDB.ValidarToken(usuario, token);
        }


        /// <summary>
        /// Envía un token al usuario para iniciar el proceso de renovación de contraseña.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public int EnviarToken(string usuario)
        {
            string token = GenerarToken(10);
            return logonDB.EnviarToken(usuario, token, token);
        }

    }
}
