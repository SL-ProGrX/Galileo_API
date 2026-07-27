using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfCartasNoCotizantesBL
    {
        private readonly FrmAfCartasNoCotizantesDB db;

        public FrmAfCartasNoCotizantesBL(IConfiguration config)
        {
            db = new FrmAfCartasNoCotizantesDB(config);
        }

        public ErrorDto<decimal> Af_CartasNoCotizantes_Obtener(int CodEmpresa, int contabilidad)
        {
            return db.Af_CartasNoCotizantes_Obtener(CodEmpresa, contabilidad);
        }

        public ErrorDto<List<AfCartasNoCotizantesData>> Af_CartasNoCotizantesDatos_Obtener(int CodEmpresa, string jFiltros)
        {
            AfCartasNoCotizantesFiltros filtros = JsonConvert.DeserializeObject<AfCartasNoCotizantesFiltros>(jFiltros)
                ?? new AfCartasNoCotizantesFiltros
                {
                    tipoDocumento = default,
                    meses = default,
                    mora = default
                };
            return db.Af_CartasNoCotizantesDatos_Obtener(CodEmpresa, filtros);
        }
    }
}