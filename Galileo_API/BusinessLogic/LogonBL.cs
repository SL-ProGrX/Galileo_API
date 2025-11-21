using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Text;

namespace Galileo.BusinessLogic
{
    public class LogonBL
    {
        private readonly IConfiguration _config;
        LogonDB logonDB;
        public LogonBL(IConfiguration config)
        {
            _config = config;
            logonDB = new LogonDB(_config);
        }

        public IntentosObtenerDto IntentosObtener()
        {
            return logonDB.IntentosObtener();
        }

        public ErrorDto LoginObtener(LoginObtenerDto req)
        {
            return logonDB.LoginObtener(req);
        }

        public ErrorDto<List<ClientesEmpresasObtenerDto>> ClientesObtener(string Usuario)
        {
            return logonDB.ClientesObtener(Usuario);
        }

        public TfaData TFA_Data_Load(string Usuario)
        {
            return logonDB.TFA_Data_Load(Usuario);
        }

        public Task<ErrorDto> TFA_Codigo_EnviarMAIL(string Usuario, string email)
        {
            return logonDB.TFA_Codigo_EnviarMAIL(Usuario, email);
        }

        public ErrorDto TFA_Codigo_Validar(string Usuario, string codigo)
        {
            return logonDB.TFA_Codigo_Validar(Usuario, codigo);
        }


        static string GenerarToken(int longitud)
        {
            const string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();

            for (int i = 0; i < longitud; i++)
            {
                int index = rnd.Next(caracteres.Length);
                sb.Append(caracteres[index]);
            }

            return sb.ToString();
        }

        public int ValidarDatos(string usuario, string email)
        {
            return logonDB.ValidarDatos(usuario, email);
        }

        public int ValidarToken(string usuario, string token)
        {
            return logonDB.ValidarToken(usuario, token);
        }

        public int EnviarToken(string usuario)
        {
            string token = GenerarToken(10);
            return logonDB.EnviarToken(usuario, token, token);
        }



    }
}
