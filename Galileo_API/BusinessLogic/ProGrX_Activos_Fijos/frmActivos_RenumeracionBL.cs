using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosRenumeracionBL
    {
        private readonly FrmActivosRenumeracionDb _db;

        public FrmActivosRenumeracionBL(IConfiguration config)
        {
            _db = new FrmActivosRenumeracionDb(config);
        }
        public ErrorDto<ActivosDataLista> Activos_Buscar(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Activos_Buscar(CodEmpresa, filtros);
        }
        public ErrorDto Activos_Renumeracion_Actualizar(int CodEmpresa, string usuario, string num_placa, string nuevo_num)
        {
            return _db.Activos_Renumeracion_Actualizar(CodEmpresa, usuario, num_placa, nuevo_num);
        }
        public ErrorDto<ActivosRenumeracionData> Activos_Renumeracion_Obtener(int CodEmpresa, string num_placa)
        {
            return _db.Activos_Renumeracion_Obtener(CodEmpresa, num_placa);
        }
    }
}
