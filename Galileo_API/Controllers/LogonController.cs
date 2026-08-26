using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    /// <summary>
    /// Controlador para autenticación, recuperación de contraseña y validaciones TFA.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LogonController : ControllerBase
    {
        readonly LogonBL logonBL;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de logon.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public LogonController(IConfiguration config)
        {
            logonBL = new LogonBL(config);
        }

        /// <summary>
        /// Obtiene la configuración o estado de intentos de autenticación.
        /// </summary>
        /// <returns>Datos de intentos de autenticación.</returns>
        [HttpGet("IntentosObtener")]
        public IntentosObtenerDto IntentosObtener()
        {
            return logonBL.IntentosObtener();
        }

        /// <summary>
        /// Método de autenticación de SSecurity. Valida las credenciales mediante spSEG_Logon
        /// y devuelve el resultado de validación junto con el token JWT cuando las credenciales son válidas.
        /// </summary>
        [HttpPost("LoginObtener")]
        public ErrorDto LoginObtener(LoginObtenerDto req)
        {
            return logonBL.LoginObtener(req);
        }

        /// <summary>
        /// Obtiene la lista de clientes y empresas asociadas al usuario indicado.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario para filtrar los clientes.</param>
        /// <returns>Resultado con la lista de clientes y empresas.</returns>
        [HttpGet("ClientesObtener")]
        [Authorize]
        public ErrorDto<List<ClientesEmpresasObtenerDto>> ClientesObtener(string Usuario)
        {
            return logonBL.ClientesObtener(Usuario);
        }

        /// <summary>
        /// Valida los datos del usuario para iniciar el proceso de renovación de contraseña.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario a validar.</param>
        /// <param name="Email">Correo electrónico asociado al usuario.</param>
        /// <returns>Código de resultado de la validación.</returns>
        [HttpPost("ValidarDatosParaRenovarContra")]
        //[Authorize]
        public int ValidarDatos(string Usuario, string Email)
        {
            return logonBL.ValidarDatos(Usuario, Email);
        }


        /// <summary>
        /// Valida el token enviado al usuario para continuar con la renovación de contraseña.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario asociado al token.</param>
        /// <param name="Token">Token de validación recibido por el usuario.</param>
        /// <returns>Código de resultado de la validación del token.</returns>
        [HttpPost("ValidarTokenParaRenovarContra")]
        public int ValidarToken(string Usuario, string Token)
        {
            return logonBL.ValidarToken(Usuario, Token);
        }


        /// <summary>
        /// Envía un token al usuario para iniciar el proceso de renovación de contraseña.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario al que se enviará el token.</param>
        /// <returns>Código de resultado del envío del token.</returns>
        [HttpPost("EnviarTokenParaRenovarContra")]
        public int EnviarToken(string Usuario)
        {
            return logonBL.EnviarToken(Usuario);
        }

        /// <summary>
        /// Obtiene los datos necesarios para la autenticación de dos factores (TFA) del usuario especificado.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario para el que se obtienen los datos TFA.</param>
        /// <returns>Datos necesarios para la autenticación de dos factores (TFA) del usuario.</returns>
        [HttpGet("TFA_Data_Load")]
        [Authorize]
        public TfaData TFA_Data_Load(string Usuario)
        {
            return logonBL.TFA_Data_Load(Usuario);
        }


        /// <summary>
        /// Envía un código de autenticación de dos factores (TFA) al correo electrónico del usuario especificado.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario asociado al código TFA.</param>
        /// <param name="email">Correo electrónico del usuario al que se enviará el código TFA.</param>
        /// <returns>Código de resultado del envío del código TFA.</returns>
        [HttpPost("TFA_Codigo_EnviarMAIL")]
        [Authorize]
        public Task<ErrorDto> TFA_Codigo_EnviarMAIL(string Usuario, string email)
        {
            return logonBL.TFA_Codigo_EnviarMAIL(Usuario, email);
        }


        /// <summary>
        /// Valida el código de autenticación de dos factores (TFA) proporcionado por el usuario.
        /// </summary>
        /// <param name="Usuario">Nombre de usuario asociado al código TFA.</param>
        /// <param name="codigo">Código de autenticación de dos factores (TFA) proporcionado por el usuario.</param>
        /// <returns>Código de resultado de la validación del código TFA.</returns>
        [HttpPost("TFA_Codigo_Validar")]
        [Authorize]
        public ErrorDto TFA_Codigo_Validar(string Usuario, string codigo)
        {
            return logonBL.TFA_Codigo_Validar(Usuario, codigo);
        }

    }
}