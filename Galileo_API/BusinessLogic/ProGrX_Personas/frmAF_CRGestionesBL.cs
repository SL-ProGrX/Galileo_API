using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCrGestionesBL
    {
        private readonly FrmAFCrGestionesDB _db;

        public FrmAFCrGestionesBL(IConfiguration config)
        {
            _db = new FrmAFCrGestionesDB(config);
        }

        public ErrorDto<List<AfCrGestionesData>> AF_CRGestiones_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_CRGestiones_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_CRGestiones_Guardar(int CodEmpresa, string usuario, AfCrGestionesData gestion)
        {
            return _db.AF_CRGestiones_Guardar(CodEmpresa, gestion, usuario);
        }

        public ErrorDto AF_CRGestiones_Eliminar(int CodEmpresa, string cod_gestion, string usuario)
        {
            return _db.AF_CRGestiones_Eliminar(CodEmpresa, cod_gestion, usuario);
        }
    }
}
