using PgxAPI.DataBaseTier;
using PgxAPI.Models;
using PgxAPI.Models.Security;
using System.Security.Claims;

namespace PgxAPI.BusinessLogic
{
    public class Jwt
    {
        private readonly IConfiguration _config;

        public Jwt(IConfiguration config)
        {
            _config = config;
        }

        public LoginResult validarTokent(ClaimsIdentity identity)
        {
            LoginResult resultado = new LoginResult();
            try
            {
                if (identity != null || identity.Claims.Count() > 0)
                {
                    var userIdClaim = identity.Claims.FirstOrDefault(x => x.Type == "UserId");
                    var userNameClaim = identity.Claims.FirstOrDefault(x => x.Type == "UserName");

                    if (userIdClaim != null && userNameClaim != null)
                    {
                        var UserId = userIdClaim.Value;
                        var UserName = userNameClaim.Value;

                        resultado = ObtenerInformacionUsuario(UserName, UserId);
                    }

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resultado;
        }

        private LoginResult ObtenerInformacionUsuario(string username, string userId)
        {
            LoginResult resultado = new LoginResult();
            try
            {
                var logindbResultado = new UsuarioDB(_config).ObtenerInformacionUsuario(username, userId);

                if (logindbResultado != null && logindbResultado.Any())
                {
                    resultado.UserId = logindbResultado[0].UserId;
                    resultado.UserName = logindbResultado[0].UserName;
                    resultado.EsSuperAdmin = logindbResultado[0].EsSuperAdmin;

                    foreach (var item in logindbResultado)
                    {
                        var emp = new LoginEmpresa
                        {

                            RoleId = item.RoleId,
                            EmpresaDescripcion = item.EmpresaDescripcion,
                            RolDescripcion = item.RolDescripcion,
                            EmpresaId = item.EmpresaId,

                        };

                        if (resultado.EmpresaRol == null)
                            resultado.EmpresaRol = new List<LoginEmpresa> { emp };
                        else
                            resultado.EmpresaRol.Add(emp);
                    }
                }
            }
            catch (Exception)
            {
                //todo: error handling
            }

            return resultado;

        }//fin login


    }//fin class
}//fin namespace
