using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmFndRecepcionDevolucionesBl
    {
        private readonly FrmFndRecepcionDevolucionesDb _db;

        public FrmFndRecepcionDevolucionesBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmFndRecepcionDevolucionesDb(config);
        }

        public ErrorDto<FndRecepcionDevolucionesInicializarData>
            FND_frmFNDRecepcionDevoluciones_Inicializar(int codEmpresa)
        {
            return _db.FND_frmFNDRecepcionDevoluciones_Inicializar(codEmpresa);
        }

        public ErrorDto<List<FndRecepcionDevolucionesContratoBusquedaData>>
            FND_frmFNDRecepcionDevoluciones_Contratos_Obtener(
                int codEmpresa,
                string codPlan,
                string cedula)
        {
            return _db.FND_frmFNDRecepcionDevoluciones_Contratos_Obtener(
                codEmpresa,
                codPlan,
                cedula);
        }

        public ErrorDto<FndRecepcionDevolucionesData?>
            FND_frmFNDRecepcionDevoluciones_Contrato_Obtener(
                int codEmpresa,
                string codPlan,
                long codContrato)
        {
            return _db.FND_frmFNDRecepcionDevoluciones_Contrato_Obtener(
                codEmpresa,
                codPlan,
                codContrato);
        }

        public ErrorDto<FndRecepcionDevolucionesAplicarData>
            FND_frmFNDRecepcionDevoluciones_Aplicar(
                int codEmpresa,
                FndRecepcionDevolucionesAplicarRequest request)
        {
            return _db.FND_frmFNDRecepcionDevoluciones_Aplicar(
                codEmpresa,
                request);
        }
    }
}
