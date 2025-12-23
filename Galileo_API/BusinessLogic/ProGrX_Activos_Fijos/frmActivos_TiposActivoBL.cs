using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosTiposActivoBL
    {
        readonly FrmActivosTiposActivoDb _db;

        public FrmActivosTiposActivoBL(IConfiguration config)
        {
            _db = new FrmActivosTiposActivoDb(config);
        }

        public ErrorDto<ActivosTiposActivosLista> Activos_TiposActivosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.Activos_TiposActivosLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Activos_TiposActivosExiste_Obtener(int CodEmpresa, string tipo_activo)
        {
            return _db.Activos_TiposActivosExiste_Obtener(CodEmpresa, tipo_activo);
        }

        public ErrorDto<ActivosTiposActivosData> Activos_TiposActivos_Obtener(int CodEmpresa, string tipo_activo)
        {
            return _db.Activos_TiposActivos_Obtener(CodEmpresa, tipo_activo);
        }

        public ErrorDto<ActivosTiposActivosData> Activos_TiposActivos_Scroll(int CodEmpresa, int scroll, string? tipo_activo)
        {
            return _db.Activos_TiposActivos_Scroll(CodEmpresa, scroll, tipo_activo);
        }

        public ErrorDto Activos_TiposActivos_Guardar(int CodEmpresa, ActivosTiposActivosData tiposActivosData)
        {
            return _db.Activos_TiposActivos_Guardar(CodEmpresa, tiposActivosData);
        }

        public ErrorDto Activos_TiposActivos_Eliminar(int CodEmpresa, string usuario, string tipo_activo)
        {
            return _db.Activos_TiposActivos_Eliminar(CodEmpresa, usuario, tipo_activo);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_MetodosDepreciacion_Obtener(int CodEmpresa)
        {
            return _db.Activos_TiposActivos_MetodosDepreciacion_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_TipoVidaUtil_Obtener()
        {
            return _db.Activos_TiposActivos_TipoVidaUtil_Obtener();
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Activos_TiposActivos_TiposAsientos_Obtener(int CodEmpresa, int Codcontablidad)
        {
            return _db.Activos_TiposActivos_TiposAsientos_Obtener(CodEmpresa, Codcontablidad);
        }
    }
}