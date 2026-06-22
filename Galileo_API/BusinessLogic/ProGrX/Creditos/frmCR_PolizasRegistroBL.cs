using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPolizasRegistroBl
    {
        private readonly FrmCrPolizasRegistroDb _db;

        public FrmCrPolizasRegistroBl(IConfiguration config)
        {
            _db = new FrmCrPolizasRegistroDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PolizaLinea_Obtener(int codEmpresa)
            => _db.CrPolizasRegistro_PolizaLinea_Obtener(codEmpresa);

        public ErrorDto<CrPolizasRegistroOperacionData> CrPolizasRegistro_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_Operacion_Obtener(codEmpresa, operacion);

        public ErrorDto<List<CrPolizasRegistroListadoItem>> CrPolizasRegistro_Lista_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_Lista_Obtener(codEmpresa, operacion);

        public ErrorDto<CrPolizasRegistroFormData> CrPolizasRegistro_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Detalle_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<CrPolizasRegistroPagoItem>> CrPolizasRegistro_Pagos_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Pagos_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<CrPolizasRegistroRecaudacionItem>> CrPolizasRegistro_Recaudacion_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Recaudacion_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<CrPolizasRegistroAcreedorItem>> CrPolizasRegistro_Acreedores_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Acreedores_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PlanPagos_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_PlanPagos_Obtener(codEmpresa, operacion);
    }
}