using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.DataBaseTier.ProGrX_Personas;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Personas
{
    public class FrmAfMotivosIngresoBL
    {
        private readonly FrmAfMotivosIngresoDB _db;

        public FrmAfMotivosIngresoBL(IConfiguration config)
            => _db = new FrmAfMotivosIngresoDB(config);

        public ErrorDto<MotivoIngresoLista> AF_MotivosIngreso_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            return _db.AF_MotivosIngreso_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_MotivosIngreso_Valida(int CodEmpresa, string CodMotivo)
        {
            return _db.AF_MotivosIngreso_Valida(CodEmpresa, CodMotivo);
        }

        public ErrorDto AF_MotivosIngreso_Guardar(int CodEmpresa, string Usuario, MotivoIngresoData motivoIngreso)
        {
            return _db.AF_MotivosIngreso_Guardar(CodEmpresa, Usuario, motivoIngreso);
        }

        public ErrorDto AF_MotivosIngreso_Eliminar(int CodEmpresa, string Usuario, string CodMotivo)
        {
            return _db.AF_MotivosIngreso_Eliminar(CodEmpresa, Usuario, CodMotivo);
        }
    }
}
