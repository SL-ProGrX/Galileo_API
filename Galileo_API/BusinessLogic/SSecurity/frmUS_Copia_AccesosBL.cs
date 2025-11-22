using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsCopiaAccesosBl
    {
        readonly FrmUsCopiaAccesosDb CopiarUsuarioDB;

        public FrmUsCopiaAccesosBl(IConfiguration config)
        {
            CopiarUsuarioDB = new FrmUsCopiaAccesosDb(config);
        }

        public List<UsuarioEmpresa> UsuariosEmpresa_Obtener(int codEmpresa)
        {
            return CopiarUsuarioDB.UsuariosEmpresa_Obtener(codEmpresa);
        }

        public ErrorDto UsuarioAccesos_Copiar(UsuarioPermisosCopiar info)
        {
            return CopiarUsuarioDB.UsuarioAccesos_Copiar(info);
        }

        public UsuarioEmpresa UsuarioEmpresa_Obtener(string nombreUsuario, int codEmpresa)
        {
            return CopiarUsuarioDB.UsuarioEmpresa_Obtener(nombreUsuario, codEmpresa);
        }

    }
}
