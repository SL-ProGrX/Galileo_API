using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.ControlTramites;
using static Galileo_API.Models.ProGrX_ControlTramites.FrmSifTagsModels;

namespace Galileo_API.BusinessLogic.ProGrX.ControlTramites
{
    public class FrmSifTagsBL
    {
        private readonly FrmSifTagsDB _db;

        public FrmSifTagsBL(IConfiguration config)
        {
            _db = new FrmSifTagsDB(config);
        }

        public ErrorDto<SifTagsListaResult> SIF_Tags_Lista_Obtener(int CodEmpresa, string filtro)
        {
            return _db.SIF_Tags_Lista_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto SIF_Tags_Guardar(int CodEmpresa, bool vEdita, string Usuario, SifTagsData param)
        {
            return _db.SIF_Tags_Guardar(CodEmpresa, vEdita, Usuario, param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SIF_Tags_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.SIF_Tags_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<SifTagsNotificacionDto> SIF_Tags_Notificacion_Obtener(int CodEmpresa, string tagCodigo)
        {
            return _db.SIF_Tags_Notificacion_Obtener(CodEmpresa, tagCodigo);
        }

        public ErrorDto SIF_Tags_Notificacion_Guardar(int CodEmpresa, SifTagsNotificacionDto param)
        {
            return _db.SIF_Tags_Notificacion_Guardar(CodEmpresa, param);
        }

        public ErrorDto SIF_Tags_Notificacion_Eliminar(int CodEmpresa, string tagCodigo)
        {
            return _db.SIF_Tags_Notificacion_Eliminar(CodEmpresa, tagCodigo);
        }
    }
}