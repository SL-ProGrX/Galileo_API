using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Galileo.BusinessLogic
{
    /// <summary>
    /// Clase de lógica de negocio para la autenticación, recuperación de contraseña y validaciones TFA.
    /// </summary>
    public class LogonBL
    {
        readonly LogonDB logonDB;
        readonly IConfiguration _config;
        
        /// <summary>
        /// Inicializa una nueva instancia de la clase LogonBL con la configuración proporcionada.
        /// </summary>
        /// <param name="_config"></param>
        public LogonBL(IConfiguration _config)
        {
            this._config = _config;
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

            if (response.Code == 0)
            {
                try
                {
                    response.Token = GenerarJwt(req.Usuario);
                }
                catch (Exception ex)
                {
                    response.Code = 1;
                    response.Description = $"No fue posible generar el token JWT: {ex.Message}";
                }
            }

            return response;
        }

        private string GenerarJwt(string usuario)
        {
            var jwt = _config.GetSection("Jwt").Get<JwtDto>()
                ?? throw new InvalidOperationException("Configuración JWT incompleta.");

            if (string.IsNullOrWhiteSpace(jwt.Issuer) ||
                string.IsNullOrWhiteSpace(jwt.Audience))
            {
                throw new InvalidOperationException("Configuración JWT incompleta.");
            }

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, jwt.Subject),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new("UserId", "0"),
                new("UserName", usuario),
            };

            // La misma configuración se usa aquí para firmar y en Program.cs para validar.
            // Jwt:Secret puede venir de user-secrets, APP_CONFIG_PATH o Jwt__Secret.
            var secret = _config["Jwt:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException("Jwt:Secret no está configurada.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Math.Max(jwt.AccessTokenMinutes, 1)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
            const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder sb = new();
            Random rnd = new();

            for (int i = 0; i < longitud; i++)
            {
                int index = rnd.Next(caracteres.Length);
                sb.Append(caracteres[index]);
            }

            return sb.ToString();
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
