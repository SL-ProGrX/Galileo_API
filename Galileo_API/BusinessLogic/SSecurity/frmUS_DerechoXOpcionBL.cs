using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsDerechoXOpcionBl
    {
        readonly FrmUsDerechoXOpcionDb DerechoXOpcionDB;

        public FrmUsDerechoXOpcionBl(IConfiguration config)
        {
            DerechoXOpcionDB = new FrmUsDerechoXOpcionDb(config);
        }

        public ErrorDto<List<ModuloResultDto>> ModulosObtener()
        {
            return DerechoXOpcionDB.ModulosObtener();
        }

        public ErrorDto<List<FormularioResultDto>> FormulariosObtener()
        {
            return DerechoXOpcionDB.FormulariosObtener();
        }

        public ErrorDto<List<OpcionResultDto>> OpcionesObtener()
        {
            return DerechoXOpcionDB.OpcionesObtener();
        }

        public ErrorDto<List<DatosResultDto>> DatosObtener(int opcion, char estado)
        {
            return DerechoXOpcionDB.DatosObtener(opcion, estado);
        }

        public ErrorDto<List<DatosUsuarioResultDto>> DatosUsuariosObtener(int opcion, char estado, int codEmpresa)
        {
            return DerechoXOpcionDB.DatosUsuariosObtener(opcion, estado, codEmpresa);
        }

        public ErrorDto RolPermisosActualizar(OpcionRolRequestDto req)
        {
            return DerechoXOpcionDB.RolPermisosActualizar(req);
        }
    }
}
