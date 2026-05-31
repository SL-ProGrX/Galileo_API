using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrNivelesBL
    {
        private readonly FrmCrNivelesDB _db;

        public FrmCrNivelesBL(IConfiguration configuration)
        {
            _db = new FrmCrNivelesDB(configuration);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Niveles_Procesos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CR_Niveles_Procesos_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_Niveles_Grupos_F4_Obtener(int CodEmpresa, string tipo, string? texto)
        {
            return _db.CR_Niveles_Grupos_F4_Obtener(CodEmpresa, tipo, texto);
        }

        public ErrorDto<CrNivelesGrupoDto> CR_Niveles_Grupo_Scroll_Obtener(int CodEmpresa, int grupoActual, string tipoProceso, int tipo)
        {
            return _db.CR_Niveles_Grupo_Scroll_Obtener(CodEmpresa, grupoActual, tipoProceso, tipo);
        }

        public ErrorDto<CrNivelesGrupoDetalleDto> CR_Niveles_Grupo_Obtener(int CodEmpresa, int grupoId)
        {
            return _db.CR_Niveles_Grupo_Obtener(CodEmpresa, grupoId);
        }

        public ErrorDto<CrNivelesGrupoDto> CR_Niveles_Grupo_Guardar(int CodEmpresa, CrNivelesGrupoGuardarRequest request, string usuario)
        {
            return _db.CR_Niveles_Grupo_Guardar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_Niveles_Grupo_Eliminar(int CodEmpresa, int grupoId, string usuario)
        {
            return _db.CR_Niveles_Grupo_Eliminar(CodEmpresa, grupoId, usuario);
        }

        public ErrorDto<CrNivelesMiembroLista> CR_Niveles_Miembros_Lista_Obtener(int CodEmpresa, int grupoId, string? texto)
        {
            return _db.CR_Niveles_Miembros_Lista_Obtener(CodEmpresa, grupoId, texto);
        }

        public ErrorDto<CrNivelesLineaLista> CR_Niveles_Lineas_Lista_Obtener(int CodEmpresa, int grupoId, string? texto)
        {
            return _db.CR_Niveles_Lineas_Lista_Obtener(CodEmpresa, grupoId, texto);
        }

        public ErrorDto CR_Niveles_Miembro_Asignar(int CodEmpresa, CrNivelesAsignacionMiembroRequest request, string usuario)
        {
            return _db.CR_Niveles_Miembro_Asignar(CodEmpresa, request, usuario);
        }

        public ErrorDto CR_Niveles_Linea_Asignar(int CodEmpresa, CrNivelesAsignacionLineaRequest request, string usuario)
        {
            return _db.CR_Niveles_Linea_Asignar(CodEmpresa, request, usuario);
        }
    }
}