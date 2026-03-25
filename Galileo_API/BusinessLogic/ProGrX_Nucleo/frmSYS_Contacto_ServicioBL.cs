using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;


namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysContactoServicioBL(IConfiguration config)
    {
        private readonly FrmSysContactoServicioDB _db = new FrmSysContactoServicioDB(config);

        // ===================== GENERAL =====================

        public ErrorDto<SysContactoServicioGeneralData?> SysContactoServicio_General_Obtener(int CodEmpresa, string identificacion, string codPais = "CRC")
        {
            return _db.SysContactoServicio_General_Obtener(CodEmpresa, identificacion, codPais);
        }

        public ErrorDto<List<SysContactoServicioGeneralData>> SysContactoServicio_Obtener(int CodEmpresa, string identificacion, string codPais, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SysContactoServicio_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        // ===================== TELÉFONOS =====================

        public ErrorDto<SysContactoServicioTelefonoLista> SysContactoServicio_Telefonos_Lista_Obtener( int CodEmpresa, string identificacion, string codPais, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SysContactoServicio_Telefonos_Lista_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        public ErrorDto<List<SysContactoServicioTelefonoData>> SysContactoServicio_Telefonos_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            return _db.SysContactoServicio_Telefonos_Obtener(CodEmpresa, identificacion, codPais);
        }

        // ===================== DIRECCIONES =====================

        public ErrorDto<SysContactoServicioDireccionLista> SysContactoServicio_Direcciones_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SysContactoServicio_Direcciones_Lista_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        public ErrorDto<List<SysContactoServicioDireccionData>> SysContactoServicio_Direcciones_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            return _db.SysContactoServicio_Direcciones_Obtener(CodEmpresa, identificacion, codPais);
        }

        // ===================== EMPRESAS =====================

        public ErrorDto<SysContactoServicioEmpresaLista> SysContactoServicio_Empresas_Lista_Obtener(int CodEmpresa, string identificacion, string codPais, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SysContactoServicio_Empresas_Lista_Obtener(CodEmpresa, identificacion, codPais, filtros);
        }

        public ErrorDto<List<SysContactoServicioEmpresaData>> SysContactoServicio_Empresas_Obtener(int CodEmpresa, string identificacion, string codPais)
        {
            return _db.SysContactoServicio_Empresas_Obtener(CodEmpresa, identificacion, codPais);
        }
        
        // ===================== CATÁLOGOS =====================

        public ErrorDto<SysContactoServicioPersonaLookupLista> SysContactoServicio_Personas_Lista_Buscar(int CodEmpresa, string codPais, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.SysContactoServicio_Personas_Lista_Buscar(CodEmpresa, codPais, filtros);
        }
    }
}

