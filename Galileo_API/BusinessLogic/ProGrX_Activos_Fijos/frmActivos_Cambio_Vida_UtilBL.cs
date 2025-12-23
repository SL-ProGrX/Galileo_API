using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosCambioVidaUtilBL
    {
        private readonly FrmActivosCambioVidaUtilDb _db;

        public FrmActivosCambioVidaUtilBL(IConfiguration config)
        {
            _db = new FrmActivosCambioVidaUtilDb(config);
        }

        public ErrorDto<ActivoLiteLista> Activos_CambioVU_ActivoLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.Activos_CambioVU_ActivoLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto<ActivoBuscarResponse> Activos_CambioVU_Activo_Obtener(int CodEmpresa, string numPlaca)
        {
            return _db.Activos_CambioVU_Activo_Obtener(CodEmpresa, numPlaca);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_CambioVU_MetodosDepreciacion_Obtener(int CodEmpresa)
        {
            return _db.Activos_CambioVU_MetodosDepreciacion_Obtener(CodEmpresa);
        }
        public ErrorDto<CambioVidaUtilAplicarResponse> Activos_CambioVU_Aplicar(int CodEmpresa, string usuario, CambioVidaUtilAplicarRequest dto)
        {
            return _db.Activos_CambioVU_Aplicar(CodEmpresa, usuario, dto);
        }
    }
}