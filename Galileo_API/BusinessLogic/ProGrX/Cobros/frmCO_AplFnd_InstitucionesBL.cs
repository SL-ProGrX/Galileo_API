using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOAplFndInstitucionesBL
    {
        private readonly FrmCOAplFndInstitucionesDB _db;

        public FrmCOAplFndInstitucionesBL(IConfiguration config)
        {
            _db = new FrmCOAplFndInstitucionesDB(config);
        }

        public ErrorDto<CoAplFndInstitucionesListaResult> Co_AplFnd_Instituciones_Lista_Obtener(int CodEmpresa, string usuario, string jfiltros)
        {

            JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);

            return _db.CoAplFndInstitucionesListaObtener(CodEmpresa, usuario);
        }

        public ErrorDto<CoAplFndInstitucionesListaResult> Co_AplFnd_Instituciones_Lista_Export(int CodEmpresa, string usuario, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;
            return _db.CoAplFndInstitucionesListaObtener(CodEmpresa, usuario);
        }

        public ErrorDto Co_AplFnd_Instituciones_Actualizar(int CodEmpresa, CoAplFndInstitucionesActualizarRequest req)
        {
            return _db.Co_AplFnd_Instituciones_Actualizar(CodEmpresa, req);
        }
    }
}
