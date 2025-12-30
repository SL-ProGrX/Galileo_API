using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndRenuevaContratosBl
    {
        private readonly FrmFndRenuevaContratosDb _db;

        public FrmFndRenuevaContratosBl(IConfiguration config)
        {
            _db = new FrmFndRenuevaContratosDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_RenuevaContratos_Catalogo_Obtener(int CodEmpresa, int Index, int Operadora)
        {
            return _db.Fnd_RenuevaContratos_Catalogo_Obtener(CodEmpresa, Index, Operadora);
        }

        public ErrorDto<List<FndRenuevaContratosDto>> Fnd_ContratoRenueva_Obtener(int CodEmpresa, string Filtros)
        {
            FndContratosBuscarParams filtros = JsonConvert.DeserializeObject<FndContratosBuscarParams>(Filtros) ?? new FndContratosBuscarParams();
            return _db.Fnd_ContratoRenueva_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto Fnd_RenuevaContratos_Aplicar(int CodEmpresa, FndRenuevaContratosRequest Request)
        {
            return _db.Fnd_RenuevaContratos_Aplicar(CodEmpresa, Request);
        }
    }
}
