using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysGestionesBitacoraBL(IConfiguration config)
    {
        private readonly FrmSysGestionesBitacoraDB _db = new FrmSysGestionesBitacoraDB(config);

        public ErrorDto<SysGestionesBitacorasLista> Sys_Gestiones_Bitacoras_Lista_Obtener(int CodEmpresa,SysGestionesBitacoraFiltro filtro)
        {
            return _db.Sys_Gestiones_Bitacoras_Lista_Obtener(CodEmpresa,filtro);
        }

        public ErrorDto<List<SysGestionesBitacorasData>> Sys_Gestiones_Bitacoras_Obtener(int CodEmpresa,SysGestionesBitacoraFiltro filtro)
        {
            return _db.Sys_Gestiones_Bitacoras_Obtener(CodEmpresa,filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Gestiones_Tipos_Obtener(int CodEmpresa)
        {
            return _db.Sys_Gestiones_Tipos_Obtener(CodEmpresa);
        }

        public ErrorDto<SociosLookupLista> Sys_Socios_Buscar_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Sys_Socios_Buscar_Lista_Obtener(CodEmpresa, filtros);
        }

    }
}