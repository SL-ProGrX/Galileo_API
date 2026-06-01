using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPrendasTiposBl
    {
        private readonly FrmCrPrendasTiposDb _db;

        public FrmCrPrendasTiposBl(IConfiguration config)
        {
            _db = new FrmCrPrendasTiposDb(config);
        }

        public ErrorDto<List<CrPrendasTipoData>> CrPrendasTipos_Obtener(int codEmpresa)
            => _db.CrPrendasTipos_Obtener(codEmpresa);

        public ErrorDto CrPrendasTipos_Guardar(int codEmpresa, CrPrendasTipoGuardarRequest request)
            => _db.CrPrendasTipos_Guardar(codEmpresa, request);

        public ErrorDto CrPrendasTipos_Eliminar(int codEmpresa, CrPrendasTipoEliminarRequest request)
            => _db.CrPrendasTipos_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrPrendasTipoAsignacionData>> CrPrendasTipos_Asignacion_Obtener(
            int codEmpresa, string request)
        {
            CrPrendasTipoAsignacionObtenerRequest filtros = 
                JsonConvert.DeserializeObject<CrPrendasTipoAsignacionObtenerRequest>(request) ?? new CrPrendasTipoAsignacionObtenerRequest();
            return _db.CrPrendasTipos_Asignacion_Obtener(codEmpresa, filtros);
        }

        public ErrorDto CrPrendasTipos_Asignacion_Guardar(
            int codEmpresa,
            CrPrendasTipoAsignacionGuardarRequest request)
            => _db.CrPrendasTipos_Asignacion_Guardar(codEmpresa, request);
    }
}