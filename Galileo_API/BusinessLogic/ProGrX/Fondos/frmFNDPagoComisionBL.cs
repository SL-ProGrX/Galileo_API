using Newtonsoft.Json;
using Galileo_API.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndPagoComisionBl
    {
        private readonly FrmFndPagoComisionDb _db;

        public FrmFndPagoComisionBl(IConfiguration config) => _db = new FrmFndPagoComisionDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> FND_PagoComision_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _db.FND_PagoComision_Bancos_Obtener(CodEmpresa, Usuario);
        }

        public ErrorDto<List<FndPagoComisionVendedorData>> FND_PagoComision_Obtener(int CodEmpresa, string Filtros)
        {
            FndPagoComisionFiltros filtros = JsonConvert.DeserializeObject<FndPagoComisionFiltros>(Filtros) ?? new FndPagoComisionFiltros();
            return _db.FND_PagoComision_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto FND_PagoComision_Generar(int CodEmpresa, string Filtros, List<FndPagoComisionVendedorData> Vendedores)
        {
            FndPagoComisionFiltros filtros = JsonConvert.DeserializeObject<FndPagoComisionFiltros>(Filtros) ?? new FndPagoComisionFiltros();
            return _db.FND_PagoComision_Generar(CodEmpresa, filtros, Vendedores);
        }
    }
}
