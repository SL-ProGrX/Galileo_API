using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoGruposBl
    {
        private readonly FrmCoGruposDb _db;

        public FrmCoGruposBl(IConfiguration config)
        {
            _db = new FrmCoGruposDb(config);
        }

        public ErrorDto<List<CoGruposData>> CO_Grupos_Obtener(int CodEmpresa)
        {
            return _db.CO_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto CO_Grupos_Guardar(int CodEmpresa, CoGruposData data)
        {
            return _db.CO_Grupos_Guardar(CodEmpresa, data);
        }

        public ErrorDto CO_Grupos_Eliminar(int CodEmpresa, int GrupoId, string Usuario)
        {
            return _db.CO_Grupos_Eliminar(CodEmpresa, GrupoId, Usuario);
        }

        public ErrorDto<List<CoGruposAsignacionData>> CO_Grupos_Asignacion_Obtener(int CodEmpresa, int GrupoId, string Filtro, int Tipo)
        {
            return _db.CO_Grupos_Asignacion_Obtener(CodEmpresa, GrupoId, Filtro, Tipo);
        }

        public ErrorDto CO_Grupos_Asignar(int CodEmpresa, int GrupoId, int Tipo, string Codigo, bool IsChecked, string Usuario)
        {
            return _db.CO_Grupos_Asignar(CodEmpresa, GrupoId, Tipo, Codigo, IsChecked, Usuario);
        }
    }
}
