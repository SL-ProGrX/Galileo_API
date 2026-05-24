using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrGruposTrabajoBL
    {
        private readonly FrmCrGruposTrabajoDB _db;

        public FrmCrGruposTrabajoBL(IConfiguration config)
        {
            _db = new FrmCrGruposTrabajoDB(config);
        }

        public ErrorDto<List<CrGrupoTrabajoGrupoData>> CR_GruposTrabajo_Grupos_Obtener(int codEmpresa)
            => _db.CR_GruposTrabajo_Grupos_Obtener(codEmpresa);

        public ErrorDto CR_GruposTrabajo_Grupos_Guardar(int codEmpresa, CrGrupoTrabajoGrupoGuardarRequest request)
            => _db.CR_GruposTrabajo_Grupos_Guardar(codEmpresa, request);

        public ErrorDto<List<CrGrupoTrabajoGrupoComboData>> CR_GruposTrabajo_GruposCombo_Obtener(int codEmpresa)
            => _db.CR_GruposTrabajo_GruposCombo_Obtener(codEmpresa);

        public ErrorDto<List<CrGrupoTrabajoMiembroData>> CR_GruposTrabajo_Miembros_Obtener(int codEmpresa, string codGrupo)
            => _db.CR_GruposTrabajo_Miembros_Obtener(codEmpresa, codGrupo);

        public ErrorDto CR_GruposTrabajo_Miembros_Marcar(int codEmpresa, CrGrupoTrabajoMiembroMarcarRequest request)
            => _db.CR_GruposTrabajo_Miembros_Marcar(codEmpresa, request);

        public ErrorDto<List<CrGrupoTrabajoEtiquetaData>> CR_GruposTrabajo_Etiquetas_Obtener(int codEmpresa, string codGrupo)
            => _db.CR_GruposTrabajo_Etiquetas_Obtener(codEmpresa, codGrupo);

        public ErrorDto CR_GruposTrabajo_Etiquetas_Marcar(int codEmpresa, CrGrupoTrabajoEtiquetaMarcarRequest request)
            => _db.CR_GruposTrabajo_Etiquetas_Marcar(codEmpresa, request);

        public ErrorDto<List<CrGrupoTrabajoComiteData>> CR_GruposTrabajo_Comites_Obtener(int codEmpresa, string codGrupo)
            => _db.CR_GruposTrabajo_Comites_Obtener(codEmpresa, codGrupo);

        public ErrorDto CR_GruposTrabajo_Comites_Marcar(int codEmpresa, CrGrupoTrabajoComiteMarcarRequest request)
            => _db.CR_GruposTrabajo_Comites_Marcar(codEmpresa, request);
    }
}
