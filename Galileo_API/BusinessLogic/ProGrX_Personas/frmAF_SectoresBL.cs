using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfSectoresBL
    {
        private readonly FrmAfSectoresDB _db;

        public FrmAfSectoresBL(IConfiguration config)
        {
            _db = new FrmAfSectoresDB(config);
        }        

        public ErrorDto<SectoresLista> AF_Sectores_Obtener(int codEmpresa)
        {
            return _db.AF_Sectores_Obtener(codEmpresa);
        }

        public ErrorDto AF_Sectores_Guardar(int codEmpresa, string usuario, SectoresData sector)
        {
            return _db.AF_Sectores_Guardar(codEmpresa, usuario, sector);
        }

        public ErrorDto AF_Sectores_Eliminar(int codEmpresa, string usuario, int codSector)
        {
            return _db.AF_Sectores_Eliminar(codEmpresa, usuario, codSector);
        }
    }
}
