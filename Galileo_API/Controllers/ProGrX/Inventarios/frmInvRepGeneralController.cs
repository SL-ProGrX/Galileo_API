using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvRepGeneralController : ControllerBase
    {
        private readonly FrmInvRepGeneralBL _bl;
        public FrmInvRepGeneralController(IConfiguration config)
        {
            _bl = new FrmInvRepGeneralBL(config);
        }

        [HttpGet("Obtener_Bodegas")]
        public ErrorDto<List<BodegaReporteInvDto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _bl.Obtener_Bodegas(CodEmpresa);
        }

        [HttpGet("Obtener_Unidades")]
        public ErrorDto<List<UnidadesReporteInvDto>> Obtener_Unidades(int CodEmpresa)
        {
            return _bl.Obtener_Unidades(CodEmpresa);
        }

        [HttpGet("Obtener_Departamento")]
        public ErrorDto<List<DepartamentoReporteInvDto>> Obtener_Departamento(int CodEmpresa)
        {
            return _bl.Obtener_Departamento(CodEmpresa);
        }

        [HttpGet("Obtener_Proveedor")]
        public ErrorDto<List<ProveedoresInvDto>> Obtener_Proveedor(int CodEmpresa)
        {
            return _bl.Obtener_Proveedor(CodEmpresa);
        }

        [HttpGet("Obtener_Lineas")]
        public ErrorDto<List<LineasInvDto>> Obtener_Lineas(int CodEmpresa)
        {
            return _bl.Obtener_Lineas(CodEmpresa);
        }

        [HttpGet("CprUens_Obtener")]
        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.CprUens_Obtener(CodEmpresa, usuario);
        }
    }
}