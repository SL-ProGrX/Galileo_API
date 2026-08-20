using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.BusinessLogic.ProGrX_Conciliacion
{
    public sealed class FrmConCierreParcialesCreditosBL
    {
        private readonly FrmConCierreParcialesCreditosDB _db;

        public FrmConCierreParcialesCreditosBL(IConfiguration config)
        {
            _db = new FrmConCierreParcialesCreditosDB(config);
        }

        public ErrorDto<ConCierreParcialesCreditosUltimoCorteData?>
            ConCierreParcialesCreditos_UltimoCorte_Obtener(int codEmpresa)
        {
            return _db.ConCierreParcialesCreditos_UltimoCorte_Obtener(codEmpresa);
        }

        public ErrorDto ConCierreParcialesCreditos_CierreParcial_Ejecutar(
            int codEmpresa,
            ConCierreParcialesCreditosCierreParcialRequest request)
        {
            return _db.ConCierreParcialesCreditos_CierreParcial_Ejecutar(
                codEmpresa,
                request);
        }

        public ErrorDto<List<Dictionary<string, object?>>>
            ConCierreParcialesCreditos_ProyeccionCartera_Ejecutar(
                int codEmpresa,
                ConCierreParcialesCreditosProyeccionRequest request)
        {
            return _db.ConCierreParcialesCreditos_ProyeccionCartera_Ejecutar(
                codEmpresa,
                request);
        }

        public ErrorDto ConCierreParcialesCreditos_ProductoAcumulado_Ejecutar(
            int codEmpresa,
            ConCierreParcialesCreditosProductoAcumuladoRequest request)
        {
            return _db.ConCierreParcialesCreditos_ProductoAcumulado_Ejecutar(
                codEmpresa,
                request);
        }
    }
}
