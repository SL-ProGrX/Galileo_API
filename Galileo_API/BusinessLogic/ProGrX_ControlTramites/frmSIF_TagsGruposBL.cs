using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX.ControlTramites
{
    public class FrmSifTagsGruposBL
    {
        private readonly FrmSifTagsGruposDB _db;

        public FrmSifTagsGruposBL(IConfiguration config)
        {
            _db = new FrmSifTagsGruposDB(config);
        }

        public ErrorDto<List<SifGruposData>> SIF_Grupos_Lista_Obtener(int CodEmpresa)
        {
            return _db.SIF_Grupos_Lista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SIF_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.SIF_Grupos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto SIF_Grupos_Guardar(int CodEmpresa, string Usuario, SifGruposGuardarRequest param)
        {
            return _db.SIF_Grupos_Guardar(CodEmpresa, Usuario, param);
        }

        public ErrorDto<List<SifGruposMiembroData>> SIF_Grupos_Miembros_Lista_Obtener(int CodEmpresa, string codGrupo)
        {
            return _db.SIF_Grupos_Miembros_Lista_Obtener(CodEmpresa, codGrupo);
        }

        public ErrorDto SIF_Grupos_Miembro_Asignar(int CodEmpresa, SifGruposMiembroAsignarRequest param)
        {
            return _db.SIF_Grupos_Miembro_Asignar(CodEmpresa, param);
        }

        public ErrorDto<List<SifGruposTagData>> SIF_Grupos_Tags_Lista_Obtener(int CodEmpresa, string codGrupo)
        {
            return _db.SIF_Grupos_Tags_Lista_Obtener(CodEmpresa, codGrupo);
        }

        public ErrorDto SIF_Grupos_Tag_Asignar(int CodEmpresa, SifGruposTagAsignarRequest param)
        {
            return _db.SIF_Grupos_Tag_Asignar(CodEmpresa, param);
        }
    }
}