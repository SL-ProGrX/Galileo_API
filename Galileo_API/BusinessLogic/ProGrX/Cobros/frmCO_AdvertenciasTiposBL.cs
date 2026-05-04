using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAdvertenciasTiposBL
    {

        private readonly IConfiguration? _config;
        private readonly FrmCoAdvertenciasTiposDB _db;

        public FrmCoAdvertenciasTiposBL(IConfiguration config)
        {
            _config = config;
            _db = new FrmCoAdvertenciasTiposDB(_config);
        }

        public ErrorDto<CoAdvertenciasTiposLista> CoAdvertenciasTipos_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.CoAdvertenciasTipos_Obtener(CodEmpresa, filtros);
        }
        public ErrorDto CoAdvertenciasTipos_Guardar(int CodEmpresa, string usuario, CoAdvertenciasTiposData request)
        {
            return _db.CoAdvertenciasTipos_Guardar(CodEmpresa, usuario, request);
        }
        public ErrorDto CoAdvertenciasTipos_Delete(int CodEmpresa, string usuario, string cod_advertencia)
        {
            return _db.CoAdvertenciasTipos_Delete(CodEmpresa, usuario, cod_advertencia);
        }

    }
}