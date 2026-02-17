using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysRaTiposBL(IConfiguration config)
    {
        private readonly FrmSysRaTiposDB _db = new FrmSysRaTiposDB(config);

        public ErrorDto<SysRaTiposLista> Sys_RaTiposLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sys_RaTiposLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Sys_RaTipos_Guardar(int CodEmpresa, string usuario, SysRaTiposData tipo)
        {        
            return _db.Sys_RaTipos_Guardar(CodEmpresa, usuario, tipo);
        }

        public ErrorDto Sys_RaTipos_Eliminar(int CodEmpresa, string usuario, string tipo_id)
        {
            return _db.Sys_RaTipos_Eliminar(CodEmpresa, usuario, tipo_id);
        }
    }
}