using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;


namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysEstadoCivilBL(IConfiguration config)
    {
        private readonly FrmSysEstadoCivilDB _db = new FrmSysEstadoCivilDB(config);

        public ErrorDto<SysEstadoCivilLista> Sys_EstadoCivilLista_Obtener(int CodEmpresa)
        {            
            return _db.Sys_EstadoCivilLista_Obtener(CodEmpresa);
        }
     
        public ErrorDto Sys_EstadoCivil_Guardar(int codEmpresa, SysEstadoCivilData estadoCivil)
        {
            return _db.Sys_EstadoCivil_Guardar(codEmpresa,estadoCivil);
        }

        public ErrorDto Sys_EstadoCivil_Eliminar(int codEmpresa, string usuario, string estadoCivil)
        {
            return _db.Sys_EstadoCivil_Eliminar(codEmpresa, usuario, estadoCivil);
        }

    }
}