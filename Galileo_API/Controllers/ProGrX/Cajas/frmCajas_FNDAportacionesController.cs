using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;


namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasFndaportacionesController : ControllerBase
    {
        private readonly FrmCajasFndaportacionesBL _bl;
        public FrmCajasFndaportacionesController(IConfiguration config)
        {
            _bl = new FrmCajasFndaportacionesBL(config);
        }

        [HttpGet("Cajas_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return _bl.Cajas_Documentos_Obtener(codEmpresa, codCaja);
        }

        [Authorize]
        [HttpPost("Fondos_Aporte_Aplicar")]
        public ErrorDto Fondos_Aporte_Aplicar(int codEmpresa, FondosAporteAplicarDto request)
        {
            return _bl.Fondos_Aporte_Aplicar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("Fondos_Aporte_RequiereAutorizacion")]
        public ErrorDto<FondosRequiereAutorizacionDto> Fondos_Aporte_RequiereAutorizacion(int codEmpresa, string plan, string usuario, decimal aporte)
        {
            return _bl.Fondos_Aporte_RequiereAutorizacion(codEmpresa, plan, usuario, aporte);
        }

        [Authorize]
        [HttpGet("Fondos_Gestion_Estado")]
        public ErrorDto<GestionEstadoDto> Fondos_Gestion_Estado(int codEmpresa, int gestionId)
        {
            return _bl.Fondos_Gestion_Estado(codEmpresa, gestionId);
        }

        [Authorize]
        [HttpPost("fondos_gestion_registro")]
        public ErrorDto<FondosGestionRegistroDto> fondos_gestion_registro(int CodEmpresa, FondosGestionRegistroAddDto request)
        {
            return _bl.fondos_gestion_registro(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("subcuentas_obtener")]
        public ErrorDto<List<FndSubCuentasDto>> SubCuentas_Obtener(int CodEmpresa, string operadora, string plan, int contrato)
        {
            return _bl.SubCuentas_Obtener(CodEmpresa, operadora, plan, contrato);
        }
    }
}