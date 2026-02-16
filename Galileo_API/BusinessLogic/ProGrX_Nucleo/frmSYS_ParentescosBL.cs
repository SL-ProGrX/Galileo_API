using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysParentescosBL(IConfiguration config)
    {
        private readonly FrmSysParentescosDB _db = new FrmSysParentescosDB(config);

        public ErrorDto<SysParentescosLista> SYS_ParentescosLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SYS_ParentescosLista_Obtener(CodEmpresa, filtros);
        }
        
        public ErrorDto<List<SysParentescosData>> SYS_Parentescos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SYS_Parentescos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto SYS_Parentescos_Guardar(int CodEmpresa, string usuario, SysParentescosData parentesco)
        {
            return _db.SYS_Parentescos_Guardar(CodEmpresa, usuario, parentesco);
        }

        public ErrorDto SYS_Parentescos_Eliminar(int CodEmpresa, string tipo, string usuario)
        {
            return _db.SYS_Parentescos_Eliminar(CodEmpresa, tipo, usuario);
        }

        public ErrorDto SYS_Parentescos_Valida(int CodEmpresa, string cod_parentesco)
        {
            return _db.SYS_Parentescos_Valida(CodEmpresa, cod_parentesco);
        }

    }
}