
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysRaUsuariosBL(IConfiguration config)
    {
        private readonly FrmSysRaUsuariosDB _db = new FrmSysRaUsuariosDB(config);

        public ErrorDto<List<SysUsuariosData>> Sys_RA_Usuarios_Consulta(int CodEmpresa, string filtro)        {
             
            return _db.Sys_RA_Usuarios_Consulta(CodEmpresa, filtro);
        }

        public ErrorDto Sys_RA_Usuarios_Asigna(int CodEmpresa, string ra_usuario, string usuario, bool accion)
        {
            return _db.Sys_RA_Usuarios_Asigna(CodEmpresa, ra_usuario, usuario, accion);
        }
    }
}