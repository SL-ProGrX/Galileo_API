using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasTiposBL
    {
        private readonly FrmActivosObrasTiposDb _db;

        public FrmActivosObrasTiposBL(IConfiguration config)
        {
            _db = new FrmActivosObrasTiposDb(config);
        }
        public ErrorDto<ActivosObrasTipoDataLista> Activos_ObrasTipos_Consultar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_ObrasTipos_Consultar(CodEmpresa, filtros);
        }
        public ErrorDto<List<ActivosObrasTipoData>> Activos_ObrasTipos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_ObrasTipos_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto Activos_ObrasTipos_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoData datos)
        {         
            return _db.Activos_ObrasTipos_Guardar(CodEmpresa, usuario, datos);
        }
        public ErrorDto Activos_ObrasTipos_Eliminar(int CodEmpresa, string usuario, string cod_desembolso)
        {         
            return _db.Activos_ObrasTipos_Eliminar(CodEmpresa, usuario, cod_desembolso);
        }
}
}
