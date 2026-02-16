using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.SYS;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysNacionalidadesBL(IConfiguration config)
    {
        private readonly FrmSysNacionalidadesDB _db = new FrmSysNacionalidadesDB(config);

        public ErrorDto<SysNacionalidadesLista> Sys_NacionalidadesLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sys_NacionalidadesLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<SysNacionalidadesData>> Sys_Nacionalidades_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sys_Nacionalidades_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Sys_Nacionalidades_Guardar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
        {
            return _db.Sys_Nacionalidades_Guardar(CodEmpresa, usuario, nacionalidad);
        }

        public ErrorDto Sys_Nacionalidades_Valida(int CodEmpresa, SysNacionalidadesData nacionalidad)
        {
            return _db.Sys_Nacionalidades_Valida(CodEmpresa, nacionalidad);
        }

        public ErrorDto Sys_Nacionalidades_Eliminar(int CodEmpresa, string usuario, string codNacionalidad)
        {
            return _db.Sys_Nacionalidades_Eliminar(CodEmpresa, usuario, codNacionalidad);
        }
        
    }
}