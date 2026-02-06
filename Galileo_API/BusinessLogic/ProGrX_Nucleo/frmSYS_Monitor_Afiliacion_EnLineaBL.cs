using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Controllers.ProGrX_Nucleo;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;


namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysMonitorAfiliacionEnLineaBL
    {
        private readonly FrmSysMonitorAfiliacionEnLineaDB _DB;
        public FrmSysMonitorAfiliacionEnLineaBL(IConfiguration config)
        {
            _DB = new FrmSysMonitorAfiliacionEnLineaDB(config);
        }

        public ErrorDto<List<AfiliacionTablaDto>> Buscar(int codEmpresa,AfiliacionFiltroDto filtros)
        {
            return _DB.Buscar(codEmpresa,filtros);
        }

        public ErrorDto<AfiliacionCasoDto?> Caso(int codEmpresa,long solicitudId)
        {
            return _DB.Caso(codEmpresa,solicitudId);
        }

        public ErrorDto<List<AfiliacionResumenDto>> Resumen(int codEmpresa,DateTime inicio,
            DateTime corte)
        {
            return _DB.Resumen(codEmpresa,inicio,corte);
        }

        public ErrorDto Resolver(int codEmpresa,long solicitudId,string estado,string usuario)
        {
            return _DB.Resolver(codEmpresa,solicitudId,estado,usuario);
        }


    }
}
