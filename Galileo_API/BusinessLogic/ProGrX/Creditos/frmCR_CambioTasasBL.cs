using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCambioTasasBl
    {
        private readonly FrmCrCambioTasasDb _db;

        public FrmCrCambioTasasBl(IConfiguration config)
        {
            _db = new FrmCrCambioTasasDb(config);
        }

        public ErrorDto<CrCambioTasasInicialResponse> CR_CambioTasas_Inicializar(int codEmpresa, string usuario)
        {
            return _db.CR_CambioTasas_Inicializar(codEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioTasas_Deductoras(int codEmpresa, int? codInstitucion)
        {
            return _db.CR_CambioTasas_Deductoras(codEmpresa, codInstitucion);
        }

        public ErrorDto<CrCambioTasasCatalogosLineaResponse> CR_CambioTasas_Catalogos_Linea(
            int codEmpresa,
            string? codigo,
            bool todas)
        {
            return _db.CR_CambioTasas_Catalogos_Linea(codEmpresa, codigo, todas);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioTasas_Lineas_F4(int codEmpresa)
        {
            return _db.CR_CambioTasas_Lineas_F4(codEmpresa);
        }

        public ErrorDto<string> CR_CambioTasas_Linea_Describir(int codEmpresa, string codigo)
        {
            return _db.CR_CambioTasas_Linea_Describir(codEmpresa, codigo);
        }

        public ErrorDto<CrCambioTasasConsultaResponse> CR_CambioTasas_Consultar(
            int codEmpresa,
            CrCambioTasasConsultaRequest request)
        {
            return _db.CR_CambioTasas_Consultar(codEmpresa, request);
        }

        public ErrorDto CR_CambioTasas_Aplicar(int codEmpresa, CrCambioTasasAplicarRequest request)
        {
            return _db.CR_CambioTasas_Aplicar(codEmpresa, request);
        }
    }
}
