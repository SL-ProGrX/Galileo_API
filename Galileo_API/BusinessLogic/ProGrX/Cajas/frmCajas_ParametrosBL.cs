using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasParametrosBL
    {
        private readonly FrmCajasParametrosDB _db;
        public FrmCajasParametrosBL(IConfiguration config)
        {
            _db = new FrmCajasParametrosDB(config);
        }
        public ErrorDto<List<CajasParametrosData>> Cajas_Parametros_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.Cajas_Parametros_Lista_Obtener(CodEmpresa, filtros);
        }
        
        public ErrorDto Cajas_Parametros_Guardar(int CodEmpresa, CajasParametrosData parametro)
        {
            return _db.Cajas_Parametros_Guardar(CodEmpresa, parametro);
        }
    }
}