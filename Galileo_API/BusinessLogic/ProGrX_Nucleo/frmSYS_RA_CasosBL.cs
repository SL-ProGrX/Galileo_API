
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;


namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysRaCasosBL(IConfiguration config)
    {
        private readonly FrmSysRaCasosDB _db = new FrmSysRaCasosDB(config);

        public ErrorDto<List<SysRaCasosData>> SYS_RA_Casos_Buscar(int CodEmpresa, SysCasosFiltroData filtros)
        {
            return _db.SYS_RA_Casos_Buscar(CodEmpresa, filtros);
        }

        public ErrorDto<List<SysCasosAutorizacionesData>> SYS_RA_CasosAutorizaciones_Obtener(int CodEmpresa, int persona_id)
        {
            return _db.SYS_RA_CasosAutorizaciones_Obtener(CodEmpresa, persona_id);
        }

        public ErrorDto<List<SysCasosAccesosData>> SYS_RA_CasosAccesos_Obtener(int CodEmpresa, int autorizacionId)
        {
            return _db.SYS_RA_CasosAccesos_Obtener(CodEmpresa, autorizacionId);
        }
    }
}