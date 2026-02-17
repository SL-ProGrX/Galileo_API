using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysOrigenRecursosBL(IConfiguration config)
    {
        private readonly FrmSysOrigenRecursosDB _db = new FrmSysOrigenRecursosDB(config);

        public ErrorDto<SysOrigenRecursosLista> Sys_OrigenRecursosLista_Obtener(int CodEmpresa)
        {            
            return _db.Sys_OrigenRecursosLista_Obtener(CodEmpresa);
        }
     
        public ErrorDto Sys_OrigenRecursos_Guardar(int codEmpresa, SysOrigenRecursosData OrigenRecursos)
        {
            return _db.Sys_OrigenRecursos_Guardar(codEmpresa,OrigenRecursos);
        }

        public ErrorDto Sys_OrigenRecursos_Eliminar(int codEmpresa, string usuario, string OrigenRecursos)
        {
            return _db.Sys_OrigenRecursos_Eliminar(codEmpresa, usuario, OrigenRecursos);
        }

    }
}