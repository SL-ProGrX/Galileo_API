using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAplFndPlanesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCOAplFndPlanesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<FondosAplConfigPrioridadResult>> FondosAplConfigPrioridades_Lista(int codEmpresa)
        {
            var query = "exec spCBR_Fondos_Apl_Config_Prioridades_Lista";
            return DbHelper.ExecuteListQuery<FondosAplConfigPrioridadResult>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<List<FondosAplConfigFondoDisponibleResult>> FondosAplConfigFondosDisponibles_Lista(int codEmpresa)
        {
            var query = "SELECT COD_PLAN AS Cod_Plan, DESCRIPCION FROM vCBR_Fondos_Apl_Config_Fondos_Disponibles";
            return DbHelper.ExecuteListQuery<FondosAplConfigFondoDisponibleResult>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<FondosAplConfigPrioridadAddResult?> FondosAplConfigPrioridad_Add(int codEmpresa, FondosAplConfigPrioridadAddParams param)
        {
            var query = "exec spCBR_Fondos_Apl_Config_Prioridades_Add @Codigo, @Orden, @Activo, @Usuario";
            var parameters = new
            {
                param.Codigo,
                param.Orden,
                param.Activo,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<FondosAplConfigPrioridadAddResult>(_portalDb, codEmpresa, query, default, parameters);
        }

        public ErrorDto<FondosAplConfigPrioridadDelResult?> FondosAplConfigPrioridad_Del(int codEmpresa, FondosAplConfigPrioridadDelParams param)
        {
            var query = "exec spCBR_Fondos_Apl_Config_Prioridades_Del @Codigo, @Usuario";
            var parameters = new
            {
                param.Codigo,
                param.Usuario
            };
            return DbHelper.ExecuteSingleQuery<FondosAplConfigPrioridadDelResult>(_portalDb, codEmpresa, query, default, parameters);
        }
    }
}
