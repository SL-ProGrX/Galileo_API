using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndGruposBl
    {
        private readonly FrmFndGruposDb _Db;

        public FrmFndGruposBl(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndGruposDb(config);
        }

        public ErrorDto<List<FndGrupoDto>> Fnd_Grupos_Obtener(int CodEmpresa)
        {
            return _Db.Fnd_Grupos_Obtener(CodEmpresa);
        }

        public ErrorDto<FndGrupoDto> Fnd_Grupos_Guardar(int CodEmpresa, FndGrupoDto grupo)
        {
            return _Db.Fnd_Grupos_Guardar(CodEmpresa, grupo);
        }

        public ErrorDto Fnd_Grupos_Eliminar(int CodEmpresa, string CodGrupo)
        {
            return _Db.Fnd_Grupos_Eliminar(CodEmpresa, CodGrupo);
        }

        public ErrorDto<List<FndPlanGrupoDto>> Fnd_Grupos_ObtenerPlanes(int CodEmpresa, string CodGrupo)
        {
            return _Db.Fnd_Grupos_ObtenerPlanes(CodEmpresa, CodGrupo);
        }

        public ErrorDto Fnd_Grupos_ActualizarAsignacionPlan( int CodEmpresa, string CodGrupo, string CodPlan, int CodOperadora, bool Checked)
        {
            return _Db.Fnd_Grupos_ActualizarAsignacionPlan(CodEmpresa, CodGrupo, CodPlan, CodOperadora, Checked);
        }
    }
}