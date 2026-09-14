using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class AdminBL
    {

        private readonly IConfiguration _config;

        public AdminBL(IConfiguration config)
        {
            _config = config;
        }

        public LoginResult Login(string username, string passw)
        {
            LoginResult resultado = new LoginResult();
            try
            {
                var logindbResultado = new AdminDb(_config).Login(username, passw);

                if (logindbResultado != null && logindbResultado.Any())
                {
                    resultado.UserId = logindbResultado[0].UserId;
                    resultado.UserName = logindbResultado[0].UserName;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;

        }
    }
}
