using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmPreaSeguimientoEtiquetasBL
    {
        private readonly FrmPreaSeguimientoEtiquetasDB DB;

        public FrmPreaSeguimientoEtiquetasBL(IConfiguration config)
        {
            DB = new FrmPreaSeguimientoEtiquetasDB(config);
        }
        public ErrorDto<PreaSeguimientoEtiquetasInfoDto> Prea_SeguimientoEtiquetas_Info_Obtener(int CodEmpresa,int idSolicitud,string? codPreanalisis)
        {
            return DB.Prea_SeguimientoEtiquetas_Info_Obtener(CodEmpresa, idSolicitud, codPreanalisis);
        }
        public ErrorDto<PreaSeguimientoEtiquetasLista> Prea_SeguimientoEtiquetas_Lista_Obtener(int CodEmpresa,int idSolicitud,string? codPreanalisis)
        {
            return DB.Prea_SeguimientoEtiquetas_Lista_Obtener(CodEmpresa, idSolicitud, codPreanalisis);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Prea_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(int CodEmpresa,string usuario)
        {
            return DB.Prea_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(CodEmpresa, usuario);
        }
        public ErrorDto Prea_SeguimientoEtiquetas_Aplicar(int CodEmpresa,PreaSeguimientoEtiquetasAplicarDto data,string usuario)
        {
            return DB.Prea_SeguimientoEtiquetas_Aplicar(CodEmpresa, data, usuario);
        }
    }
}