using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndRenuevaContratosController : ControllerBase
    {
        private readonly FrmFndRenuevaContratosBl _bl;

        public FrmFndRenuevaContratosController(IConfiguration config)
        {
            _bl = new FrmFndRenuevaContratosBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_RenuevaContratos_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_RenuevaContratos_Catalogo_Obtener(int CodEmpresa, int Index, int Operadora)
        {
            return _bl.Fnd_RenuevaContratos_Catalogo_Obtener(CodEmpresa, Index, Operadora);
        }

        [Authorize]
        [HttpGet("Fnd_ContratoRenueva_Obtener")]
        public ErrorDto<List<FndRenuevaContratosDto>> Fnd_ContratoRenueva_Obtener(int CodEmpresa, string Filtros)
        {
            return _bl.Fnd_ContratoRenueva_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpPost("Fnd_RenuevaContratos_Aplicar")]
        public ErrorDto Fnd_RenuevaContratos_Aplicar(int CodEmpresa, FndRenuevaContratosRequest Request)
        {
            return _bl.Fnd_RenuevaContratos_Aplicar(CodEmpresa, Request);
        }
    }
}