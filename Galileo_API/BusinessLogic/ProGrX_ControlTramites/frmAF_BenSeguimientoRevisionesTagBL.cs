using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfBenSeguimientoRevisionesTagBl
    {
        private readonly FrmAfBenSeguimientoRevisionesTagDb _db;

        public FrmAfBenSeguimientoRevisionesTagBl(
            IConfiguration config)
        {
            _db = new FrmAfBenSeguimientoRevisionesTagDb(
                config);
        }

        public ErrorDto<List<AfBenSeguimientoBeneficioData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Beneficios_Obtener(
                int codEmpresa,
                string? cedula)
        {
            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Beneficios_Obtener(
                    codEmpresa,
                    cedula);
        }

        public ErrorDto<List<AfBenSeguimientoRegistroData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Seguimiento_Obtener(
                int codEmpresa,
                string request)
        {
            AfBenSeguimientoClaveRequest data =
                DeserializarClave(request);

            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Seguimiento_Obtener(
                    codEmpresa,
                    data);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_frmAF_BenSeguimientoRevisionesTag_Etiquetas_Obtener(
                int codEmpresa,
                string usuario)
        {
            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Etiquetas_Obtener(
                    codEmpresa,
                    usuario);
        }

        public ErrorDto
            AF_frmAF_BenSeguimientoRevisionesTag_Aviso_Obtener(
                int codEmpresa,
                string tagCodigo)
        {
            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Aviso_Obtener(
                    codEmpresa,
                    tagCodigo);
        }

        public ErrorDto<List<AfBenSeguimientoOmisionData>>
            AF_frmAF_BenSeguimientoRevisionesTag_Omisiones_Obtener(
                int codEmpresa,
                string request)
        {
            AfBenSeguimientoClaveRequest data =
                DeserializarClave(request);

            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Omisiones_Obtener(
                    codEmpresa,
                    data);
        }

        public ErrorDto<AfBenSeguimientoOmisionCambiarData>
            AF_frmAF_BenSeguimientoRevisionesTag_Omision_Cambiar(
                int codEmpresa,
                AfBenSeguimientoOmisionCambiarRequest request)
        {
            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Omision_Cambiar(
                    codEmpresa,
                    request);
        }

        public ErrorDto
            AF_frmAF_BenSeguimientoRevisionesTag_Aplicar(
                int codEmpresa,
                AfBenSeguimientoAplicarRequest request)
        {
            return _db
                .AF_frmAF_BenSeguimientoRevisionesTag_Aplicar(
                    codEmpresa,
                    request);
        }

        private static AfBenSeguimientoClaveRequest
            DeserializarClave(
                string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return new AfBenSeguimientoClaveRequest();
            }

            return JsonConvert.DeserializeObject<
                AfBenSeguimientoClaveRequest>(
                    request)
                ?? new AfBenSeguimientoClaveRequest();
        }
    }
}