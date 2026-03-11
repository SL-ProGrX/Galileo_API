using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using static Galileo_API.Models.ProGrX_Polizas.FrmCRPolizasRegistroBeneficiariosModels;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCRPolizasRegistroBeneficiariosBL
    {
        private readonly FrmCRPolizasRegistroBeneficiariosDb _db;

        public FrmCRPolizasRegistroBeneficiariosBL(IConfiguration config)
        {
            _db = new FrmCRPolizasRegistroBeneficiariosDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CRPolizasRegistroBeneficiarios_Parentescos_Obtener(int codEmpresa)
                => _db.CRPolizasRegistroBeneficiarios_Parentescos_Obtener(codEmpresa);
        public ErrorDto<CRPolizasRegistroBeneficiariosEncabezadoResponse> CRPolizasRegistroBeneficiarios_Encabezado_Obtener(
           int codEmpresa,
           int IdSolicitud,
           int NumPoliza)
            => _db.CRPolizasRegistroBeneficiarios_Encabezado_Obtener(codEmpresa, IdSolicitud, NumPoliza);
        public ErrorDto<List<CRPolizasRegistroBeneficiariosListaItem>> CRPolizasRegistroBeneficiarios_Beneficiarios_Obtener(
              int codEmpresa,
             int IdSolicitud,
              int NumPoliza)
           => _db.CRPolizasRegistroBeneficiarios_Beneficiarios_Obtener(codEmpresa, IdSolicitud, NumPoliza);
        public ErrorDto<CRPolizasRegistroBeneficiariosNuevoResponse> CRPolizasRegistroBeneficiarios_Nuevo_Obtener(
           int codEmpresa,
           int IdSolicitud,
           int NumPoliza)
       => _db.CRPolizasRegistroBeneficiarios_Nuevo_Obtener(codEmpresa, IdSolicitud, NumPoliza);
        public ErrorDto<CRPolizasRegistroBeneficiarios> CRPolizasRegistroBeneficiarios_Detalle_Obtener(
              int codEmpresa,
              int IdSolicitud,
              int NumPoliza,
              string IdBeneficiario)
          => _db.CRPolizasRegistroBeneficiarios_Detalle_Obtener(codEmpresa, IdSolicitud, NumPoliza, IdBeneficiario);
        public ErrorDto<CRPolizasRegistroBeneficiariosGuardarResponse> CRPolizasRegistroBeneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            CRPolizasRegistroBeneficiarios request)
         => _db.CRPolizasRegistroBeneficiarios_Guardar(codEmpresa, usuario, request);

        public ErrorDto CRPolizasRegistroBeneficiarios_Eliminar(
             int codEmpresa,
             int IdSolicitud,
             int NumPoliza,
             string IdBeneficiario)
   => _db.CRPolizasRegistroBeneficiarios_Eliminar(codEmpresa, IdSolicitud, NumPoliza, IdBeneficiario);

    }
}
