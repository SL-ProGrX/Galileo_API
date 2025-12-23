using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasTipoDesemBL
    {
        private readonly FrmActivosObrasTipoDesemDb _db;

        public FrmActivosObrasTipoDesemBL(IConfiguration config)
        {
            _db = new FrmActivosObrasTipoDesemDb(config);
        }
        public ErrorDto<ActivosObrasTipoDesemDataLista> Activos_ObrasTipoDesem_Consultar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_ObrasTipoDesem_Consultar(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosObrasTipoDesemData>> Activos_ObrasTipoDesem_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_ObrasTipoDesem_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto Activos_ObrasTipoDesem_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
        {         
            return _db.Activos_ObrasTipoDesem_Guardar(CodEmpresa, usuario, datos);
        }
        public ErrorDto Activos_ObrasTipoDesem_Eliminar(int CodEmpresa, string usuario, string cod_desembolso)
        {         
            return _db.Activos_ObrasTipoDesem_Eliminar(CodEmpresa, usuario, cod_desembolso);
        }
}
}
