using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSysIvaParametrosBL(IConfiguration config)
    {
        private readonly FrmSysIvaParametrosDB _db = new FrmSysIvaParametrosDB(config);

        public ErrorDto<SysIvaParametrosLista> Sys_Iva_Parametros_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.Sys_Iva_Parametros_Lista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<SysIvaParametrosData>> Sys_Iva_Parametros_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = string.IsNullOrWhiteSpace(jfiltros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();

            return _db.Sys_Iva_Parametros_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<SysIvaParametrosUpdateResponse> Sys_Iva_Parametro_Actualizar(int CodEmpresa, string codParametro, string usuario, string jrequest)
        {
            var dto = string.IsNullOrWhiteSpace(jrequest)
                ? new SysIvaParametrosUpdateRequest { valor = "" }
                : JsonConvert.DeserializeObject<SysIvaParametrosUpdateRequest>(jrequest) ?? new SysIvaParametrosUpdateRequest { valor = "" };

            return _db.Sys_Iva_Parametro_Actualizar(CodEmpresa, codParametro, dto, usuario);
        }

        public ErrorDto<SysIvaCuentasResumenLista> Sys_Iva_Cuentas_Buscar(int CodEmpresa, int codContabilidad, string filtros)
        {
            var f = string.IsNullOrWhiteSpace(filtros)
                ? new FiltrosLazyLoadData()
                : JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();

            int? nivelMask = null;
            string? divisaRaw = null;


            var jo = string.IsNullOrWhiteSpace(filtros) ? null : JObject.Parse(filtros);
            if (jo?["niveles"] != null) nivelMask = (int?)jo["niveles"];
            if (jo?["divisa"] != null) divisaRaw = (string?)jo["divisa"];


            return _db.Sys_Iva_Cuentas_Buscar(CodEmpresa, codContabilidad, f, nivelMask, divisaRaw);
        }

        public ErrorDto<SysIvaCuentasResumenData> Sys_Iva_CuentaPorCodigo_Obtener(int CodEmpresa, int codContabilidad, string codigoSinMask)
        {
            return _db.Sys_Iva_CuentaPorCodigo_Obtener(CodEmpresa, codContabilidad, codigoSinMask);
        }

        public ErrorDto<SysIvaCuentasResumenLista> Sys_Iva_Cuentas_Todas_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _db.Sys_Iva_Cuentas_Todas_Obtener(CodEmpresa, codContabilidad);
        }

    }
}