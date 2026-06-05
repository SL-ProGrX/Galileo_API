using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCrMotivosBL
    {
        private readonly FrmAFCrMotivosDB _db;

        public FrmAFCrMotivosBL(IConfiguration config)
        {
            _db = new FrmAFCrMotivosDB(config);
        }

        public ErrorDto<List<AfCrMotivosData>> AF_CRMotivos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.AF_CRMotivos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_CRMotivos_Guardar(int CodEmpresa, string usuario, AfCrMotivosData motivo)
        {
            return _db.AF_CRMotivos_Guardar(CodEmpresa, motivo, usuario);
        }

        public ErrorDto AF_CRMotivos_Eliminar(int CodEmpresa, string cod_motivo, string usuario)
        {
            return _db.AF_CRMotivos_Eliminar(CodEmpresa, cod_motivo, usuario);
        }
    }
}
