using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosPolizasTiposBL
    {
        private readonly FrmActivosPolizasTiposDb _db;
        public FrmActivosPolizasTiposBL(IConfiguration config)
        {
            _db = new FrmActivosPolizasTiposDb(config);
        }
        public ErrorDto<ActivosPolizasTiposLista> Activos_PolizasTiposLista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_PolizasTiposLista_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto Activos_PolizasTipos_Guardar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
        {
            return _db.Activos_PolizasTipos_Guardar(CodEmpresa, usuario, tipoPoliza);
        }
        public ErrorDto<List<ActivosPolizasTiposData>> Activos_PolizasTipos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_PolizasTipos_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Activos_PolizasTipos_Eliminar(int CodEmpresa, string usuario, string tipo_poliza)
        {
            return _db.Activos_PolizasTipos_Eliminar(CodEmpresa, usuario, tipo_poliza);
        }

        public ErrorDto Activos_PolizasTipos_Valida(int CodEmpresa, string tipo_poliza)
        {
            return _db.Activos_PolizasTipos_Valida(CodEmpresa, tipo_poliza);
        }
    }
}
