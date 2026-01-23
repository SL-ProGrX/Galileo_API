using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesCopiaEsquemaBL
    {
        private readonly FrmTesCopiaEsquemaDB _copiaEsquemaDb;
        
        public FrmTesCopiaEsquemaBL(IConfiguration config)
        {
            _copiaEsquemaDb = new FrmTesCopiaEsquemaDB(config);
        }

        public ErrorDto<TesCopiaEsquemaModels> Tes_CopiaEsquema_Obtener(int CodEmpresa, int solicitud, int contabilidad)
        {
            return _copiaEsquemaDb.Tes_CopiaEsquema_Obtener(CodEmpresa, solicitud, contabilidad);
        }

        public ErrorDto Tes_CopiarEsquema_Guardar(int CodEmpresa, TesCopiaEsquemaModels solicitud)
        {
            return _copiaEsquemaDb.Tes_CopiarEsquema_Guardar(CodEmpresa, solicitud);
        }

        public ErrorDto<TesCopiaEsquemaLista> Tes_CopiaEsquemaLista_Obtener(int CodEmpresa, int contabilidad, string Jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(Jfiltros) ?? new FiltrosLazyLoadData();
            return _copiaEsquemaDb.Tes_CopiaEsquemaLista_Obtener(CodEmpresa, contabilidad, filtros);
        }
    }
}
