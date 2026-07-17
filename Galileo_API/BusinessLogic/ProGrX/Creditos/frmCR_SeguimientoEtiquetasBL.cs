using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoEtiquetasBl
    {
        private readonly FrmCrSeguimientoEtiquetasDb _db;

        public FrmCrSeguimientoEtiquetasBl(IConfiguration config)
        {
            _db = new FrmCrSeguimientoEtiquetasDb(config);
        }

        public ErrorDto<List<CrSeguimientoEtiquetasData>> Cr_SeguimientoEtiquetas_Lista_Obtener(int codEmpresa, int idSolicitud)
        {
            return _db.Cr_SeguimientoEtiquetas_Lista_Obtener(codEmpresa, idSolicitud);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(int codEmpresa, string usuario)
        {
            return _db.Cr_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(codEmpresa, usuario);
        }

        public ErrorDto Cr_SeguimientoEtiquetas_Aplicar(int codEmpresa, CrSeguimientoEtiquetasAplicarRequest request)
        {
            return _db.Cr_SeguimientoEtiquetas_Aplicar(codEmpresa, request);
        }

        public ErrorDto<int> Cr_SeguimientoEtiquetas_NotaLargo_Obtener(int codEmpresa, string tagCodigo)
        {
            return _db.Cr_SeguimientoEtiquetas_NotaLargo_Obtener(codEmpresa, tagCodigo);
        }
    }
}
