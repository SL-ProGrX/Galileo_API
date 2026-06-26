using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrMoraCargosAjustesBl
    {
        private readonly FrmCrMoraCargosAjustesDb _db;

        public FrmCrMoraCargosAjustesBl(IConfiguration config)
            => _db = new FrmCrMoraCargosAjustesDb(config);

        public ErrorDto<CrMoraCargosAjustesOperacionData?> CrMoraCargosAjustes_ConsultaOperacion_Obtener(
            int codEmpresa,
            int operacionId)
        {
            return _db.CrMoraCargosAjustes_ConsultaOperacion_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<List<CrMoraCargosAjustesCuotasData>> CrMoraCargosAjustes_CuotasMora_Obtener(
            int codEmpresa,
            int operacionId)
        {
            return _db.CrMoraCargosAjustes_CuotasMora_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto<List<CrMoraCargosAjustesCargosData>> CrMoraCargosAjustes_Cargos_Obtener(
            int codEmpresa,
            int operacionId)
        {
            return _db.CrMoraCargosAjustes_Cargos_Obtener(codEmpresa, operacionId);
        }

        public ErrorDto CrMoraCargosAjustes_Fecha_Aplicar(
            int codEmpresa,
            CrMoraCargosAjustesFechaRequest request)
        {
            return _db.CrMoraCargosAjustes_Fecha_Aplicar(codEmpresa, request);
        }

        public ErrorDto CrMoraCargosAjustes_CuotasMora_Eliminar(
            int codEmpresa,
            string request)
        {
            var data =
                JsonConvert.DeserializeObject<CrMoraCargosAjustesCuotasEliminarRequest>(request) 
                ?? new CrMoraCargosAjustesCuotasEliminarRequest();
            return _db.CrMoraCargosAjustes_CuotasMora_Eliminar(codEmpresa, data);
        }

        public ErrorDto CrMoraCargosAjustes_Cargos_Eliminar(
            int codEmpresa,
            string request)
        {
            var data =
               JsonConvert.DeserializeObject<CrMoraCargosAjustesCargosEliminarRequest>(request)
               ?? new CrMoraCargosAjustesCargosEliminarRequest();
            return _db.CrMoraCargosAjustes_Cargos_Eliminar(codEmpresa, data);
        }
    }
}