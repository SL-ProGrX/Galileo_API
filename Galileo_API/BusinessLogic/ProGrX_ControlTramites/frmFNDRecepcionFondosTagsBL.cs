using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public class FrmFndRecepcionFondosTagsBl
    {
        private readonly FrmFndRecepcionFondosTagsDb _Db;

        public FrmFndRecepcionFondosTagsBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndRecepcionFondosTagsDb(config);
        }

        public ErrorDto<FndRecepcionFondosTagsInicializarResponse>
            FND_frmFNDRecepcionFondosTags_Inicializar(int codEmpresa)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Inicializar(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            FND_frmFNDRecepcionFondosTags_Planes_Obtener(int codEmpresa)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Planes_Obtener(
                codEmpresa);
        }

        public ErrorDto<List<FndRecepcionFondosTagsContratoBusquedaResponse>>
            FND_frmFNDRecepcionFondosTags_Contratos_Obtener(
                int codEmpresa,
                string codPlan,
                string cedula)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Contratos_Obtener(
                codEmpresa,
                codPlan,
                cedula);
        }

        public ErrorDto<FndRecepcionFondosTagsContratoResponse?>
            FND_frmFNDRecepcionFondosTags_Contrato_Obtener(
                int codEmpresa,
                string codPlan,
                long codContrato,
                string movimiento)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Contrato_Obtener(
                codEmpresa,
                codPlan,
                codContrato,
                movimiento);
        }

        public ErrorDto<List<FndRecepcionFondosTagsPendienteResponse>>
            FND_frmFNDRecepcionFondosTags_Pendientes_Obtener(
                int codEmpresa,
                string movimiento)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Pendientes_Obtener(
                codEmpresa,
                movimiento);
        }

        public ErrorDto<FndRecepcionFondosTagsAplicarResponse>
            FND_frmFNDRecepcionFondosTags_Movimiento_Aplicar(
                int codEmpresa,
                FndRecepcionFondosTagsAplicarRequest request)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        public ErrorDto<List<FndRecepcionFondosTagsHistorialResponse>>
            FND_frmFNDRecepcionFondosTags_Historial_Obtener(
                int codEmpresa,
                FndRecepcionFondosTagsHistorialRequest request)
        {
            return _Db.FND_frmFNDRecepcionFondosTags_Historial_Obtener(
                codEmpresa,
                request);
        }
    }
}
