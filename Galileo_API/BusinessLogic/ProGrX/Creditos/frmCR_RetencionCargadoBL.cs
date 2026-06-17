using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrRetencionCargadoBl
    {
        private readonly FrmCrRetencionCargadoDb _db;

        public FrmCrRetencionCargadoBl(IConfiguration config)
        {
            _db = new FrmCrRetencionCargadoDb(config);
        }

        public ErrorDto<CrRetencionCargadoPantallaData> CrRetencionCargado_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _db.CrRetencionCargado_Pantalla_Obtener(codEmpresa, usuario);

        public ErrorDto<List<DropDownListaGenericaModel>> CrRetencionCargado_Deductoras_Obtener(
            int codEmpresa,
            int codInstitucion)
            => _db.CrRetencionCargado_Deductoras_Obtener(codEmpresa, codInstitucion);

        public ErrorDto<CrRetencionCargadoDeductoraDetalleData> CrRetencionCargado_DeductoraDetalle_Obtener(
            int codEmpresa,
            string codigo,
            int codDeductora)
            => _db.CrRetencionCargado_DeductoraDetalle_Obtener(codEmpresa, codigo, codDeductora);

        public ErrorDto<CrRetencionCargadoCargaData> CrRetencionCargado_Cargar(
            int codEmpresa,
            string usuario,
            CrRetencionCargadoCargaRequest request)
            => _db.CrRetencionCargado_Cargar(codEmpresa, usuario, request);

        public ErrorDto CrRetencionCargado_Aplicar(
            int codEmpresa,
            string usuario,
            CrRetencionCargadoAplicarRequest request)
            => _db.CrRetencionCargado_Aplicar(codEmpresa, usuario, request);
    }
}