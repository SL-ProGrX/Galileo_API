using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRPolizasRegistroBeneficiariosBL
    {
        private readonly FrmCRPolizasRegistroBeneficiariosDB _db;

        public FrmCRPolizasRegistroBeneficiariosBL(IConfiguration config)
        {
            _db = new FrmCRPolizasRegistroBeneficiariosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistroBeneficiarios_Parentescos_Obtener(int codEmpresa)
            => _db.CrPolizasRegistroBeneficiarios_Parentescos_Obtener(codEmpresa);

        public ErrorDto<CrPolizasRegistroBeneficiariosEncabezadoData?> CrPolizasRegistroBeneficiarios_Encabezado_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _db.CrPolizasRegistroBeneficiarios_Encabezado_Obtener(codEmpresa, operacion, numPoliza);

        public ErrorDto<List<CrPolizasRegistroBeneficiariosListaData>> CrPolizasRegistroBeneficiarios_Beneficiarios_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _db.CrPolizasRegistroBeneficiarios_Beneficiarios_Obtener(codEmpresa, operacion, numPoliza);

        public ErrorDto<CrPolizasRegistroBeneficiariosNuevoData?> CrPolizasRegistroBeneficiarios_Nuevo_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _db.CrPolizasRegistroBeneficiarios_Nuevo_Obtener(codEmpresa, operacion, numPoliza);

        public ErrorDto<CrPolizasRegistroBeneficiariosDetalleData?> CrPolizasRegistroBeneficiarios_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza,
            string idBeneficiario)
            => _db.CrPolizasRegistroBeneficiarios_Detalle_Obtener(codEmpresa, operacion, numPoliza, idBeneficiario);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistroBeneficiarios_Busqueda_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _db.CrPolizasRegistroBeneficiarios_Busqueda_Obtener(codEmpresa, operacion, numPoliza);

        public ErrorDto<CrPolizasRegistroBeneficiariosGuardarData> CrPolizasRegistroBeneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            CrPolizasRegistroBeneficiariosGuardarRequest request)
            => _db.CrPolizasRegistroBeneficiarios_Guardar(codEmpresa, usuario, request);

        public ErrorDto CrPolizasRegistroBeneficiarios_Eliminar(
            int codEmpresa,
            int operacion,
            int numPoliza,
            string idBeneficiario)
            => _db.CrPolizasRegistroBeneficiarios_Eliminar(codEmpresa, operacion, numPoliza, idBeneficiario);
    }
}