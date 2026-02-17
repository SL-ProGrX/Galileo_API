
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysRaAutorizacionesBL(IConfiguration config)
    {

        private readonly FrmSysRaAutorizacionesDB _db = new FrmSysRaAutorizacionesDB(config);

        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RA_AutorizacionesUsuariosAutorizados_Obtener(int CodEmpresa)
        {
            return _db.SYS_RA_AutorizacionesUsuariosAutorizados_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_AutorizacionesCasos_Obtener(int CodEmpresa)
        {
            return _db.SYS_RA_AutorizacionesCasos_Obtener(CodEmpresa);
        }

        public ErrorDto<SysAutorizacionesData> SYS_RA_AutorizacionesCasosDatos_Obtener(int CodEmpresa, int persona_id)
        {
            return _db.SYS_RA_AutorizacionesCasosDatos_Obtener(CodEmpresa, persona_id);
        }
      
        public ErrorDto SYS_RA_Autorizaciones_Autorizar(int CodEmpresa, string usuario, SysAutorizacionesData datos, string clave)
        {
            return _db.SYS_RA_Autorizaciones_Autorizar(CodEmpresa, usuario, datos, clave);
        }
    }
}