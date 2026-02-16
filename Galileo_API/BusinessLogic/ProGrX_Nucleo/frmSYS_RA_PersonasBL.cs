
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysRaPersonasBL(IConfiguration config)
    {
        private readonly FrmSysRaPersonasDB _db = new FrmSysRaPersonasDB(config);

        public ErrorDto<List<SysRaExpedientesData>> SYS_RA_Personas_Buscar(int CodEmpresa, SysExpedienteFiltroData filtros)
        {
            return _db.SYS_RA_Personas_Buscar(CodEmpresa, filtros);
        }
 
        public ErrorDto SYS_RA_Personas_Guardar(int CodEmpresa, int personaId, SysRaExpedientesData datos, string usuario)
        {
            return _db.SYS_RA_Personas_Guardar(CodEmpresa, personaId, datos, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SYS_Usuarios_Obtener(int CodEmpresa)           
        {
            return _db.SYS_Usuarios_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RaTipos_Obtener(int CodEmpresa)
        {
            return _db.SYS_RaTipos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_CasosPorCedula_Obtener(int CodEmpresa, string filtro)
        {
            return _db.SYS_RA_CasosPorCedula_Obtener(CodEmpresa, filtro);
        }
    }
}