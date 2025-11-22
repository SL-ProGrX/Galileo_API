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

        public List<ModuloResultDto> ModulosObtener()
        {
            return DerechoXOpcionDB.ModulosObtener();
        }

        public List<FormularioResultDto> FormulariosObtener()
        {
            return DerechoXOpcionDB.FormulariosObtener();
        }

        public List<OpcionResultDto> OpcionesObtener()
        {
            return DerechoXOpcionDB.OpcionesObtener();
        }

        public List<DatosResultDto> DatosObtener(int opcion, char estado)
        {
            return DerechoXOpcionDB.DatosObtener(opcion, estado);
        }

        public ErrorDto RolPermisosActualizar(OpcionRolRequestDto req)
        {
            return DerechoXOpcionDB.RolPermisosActualizar(req);
        }
    }
}