using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace PgxAPI.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesMotivosSinpeBL
    {
        private readonly FrmTesMotivosSinpeDB _db;

        public FrmTesMotivosSinpeBL(IConfiguration config)
        {
            _db = new FrmTesMotivosSinpeDB(config);
        }

        public ErrorDto<TesMotivosSinpeLista> TES_MotivoSinpe_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.TES_MotivoSinpe_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<List<TesMotivosSinpeDto>> TES_MotivoSinpeExportar_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return _db.TES_MotivoSinpeExportar_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto TES_MotivoSinpe_Guardar(int CodEmpresa, string usuario, TesMotivosSinpeDto motivo)
        {

            return _db.TES_MotivoSinpe_Guardar(CodEmpresa, usuario, motivo);
        }

        public ErrorDto TES_MotivoSinpe_Eliminar(int CodEmpresa, string usuario, int cod_motivo)
        {
            return _db.TES_MotivoSinpe_Eliminar(CodEmpresa, usuario, cod_motivo);
        }

    }
}
