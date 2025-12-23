using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosParametrosEnlacesBL
    {
        private readonly FrmActivosParametrosEnlacesDB _db;

        public FrmActivosParametrosEnlacesBL(IConfiguration config)
        {
            _db = new FrmActivosParametrosEnlacesDB(config);
        }
        public ErrorDto Activos_ParametrosEnlaces_Proveedores_Guardar(int CodEmpresa)
        {
            return _db.Activos_ParametrosEnlaces_Proveedores_Guardar(CodEmpresa);
        }

        
    }
}
