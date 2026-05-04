using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoControlComTablaBL
    {

        private readonly IConfiguration? _config;
        private readonly FrmCoControlComTablaDB _db;

        public FrmCoControlComTablaBL(IConfiguration config)
        {
            _config = config;
            _db = new FrmCoControlComTablaDB(_config);
        }
        
        public ErrorDto<CoControlComTablaLista> CO_ControlComTabla_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CO_ControlComTabla_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto CO_ControlComTabla_Guardar(int CodEmpresa, string usuario, CoControlComTablaData request)
        {
            return _db.CO_ControlComTabla_Guardar(CodEmpresa, usuario, request);
        }
        public ErrorDto CO_ControlComTabla_Delete(int CodEmpresa, string usuario, int id_linea)
        {
            return _db.CO_ControlComTabla_Delete(CodEmpresa, usuario, id_linea);
        }

    }
}