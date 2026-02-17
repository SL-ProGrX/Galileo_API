using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysCorreosBandejaBL(IConfiguration config)
    {
        private readonly FrmSysCorreosBandejaDB _db = new FrmSysCorreosBandejaDB(config);

        public ErrorDto<SysCorreosBandejaLista> Sys_Correos_Bandeja_Lista_Obtener(int CodEmpresa,string Para_Buscar,string Asunto_Buscar,string Fecha_Inicio,string Fecha_Fin, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
                throw new ArgumentNullException(nameof(jfiltros), "Los filtros no pueden ser nulos.");
            return _db.Correos_Bandeja_Lista_Obtener(CodEmpresa,Para_Buscar,Asunto_Buscar,Fecha_Inicio,Fecha_Fin,filtros);
        }

        public ErrorDto<List<SysCorreosBandejaData>> Correos_Bandeja_Obtener(int CodEmpresa,string Para_Buscar,string Asunto_Buscar,string Fecha_Inicio,string Fecha_Fin,string Filtro_Global)
        {
            return _db.Correos_Bandeja_Obtener(CodEmpresa,Para_Buscar,Asunto_Buscar,Fecha_Inicio,Fecha_Fin,Filtro_Global);
        }

        public ErrorDto<SysCorreosBandejaResumenLista> Sys_Correos_Bandeja_Resumen_Lista_Obtener(int CodEmpresa, string Para_Buscar, string Asunto_Buscar, string Fecha_Inicio, string Fecha_Fin, string jfiltros)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
                throw new ArgumentNullException(nameof(jfiltros), "Los filtros no pueden ser nulos.");
            return _db.Correos_Bandeja_Resumen_Lista_Obtener(CodEmpresa, Para_Buscar, Asunto_Buscar, Fecha_Inicio, Fecha_Fin, filtros);
        }

        public ErrorDto<List<SysCorreosBandejaResumenData>> Correos_Bandeja_Resumen_Obtener(int CodEmpresa, string Para_Buscar, string Asunto_Buscar, string Fecha_Inicio, string Fecha_Fin, string Filtro_Global)
        {
            return _db.Correos_Bandeja_Resumen_Obtener(CodEmpresa, Para_Buscar, Asunto_Buscar, Fecha_Inicio, Fecha_Fin, Filtro_Global);
        }
    }
}