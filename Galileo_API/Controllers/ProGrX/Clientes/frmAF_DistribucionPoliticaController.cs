using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFDistribucionPoliticaController : ControllerBase
    {
        private readonly FrmAFDistribucionPoliticaBL _bl;

        public FrmAFDistribucionPoliticaController(IConfiguration config)
        {
            _bl = new FrmAFDistribucionPoliticaBL(config);
        }

        [Authorize]
        [HttpGet("AF_DistribucionPolitica_Provincias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Provincias_Obtener(int CodEmpresa)
        {
            return _bl.AF_DistribucionPolitica_Provincias_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_DistribucionPolitica_Cantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Cantones_Obtener(int CodEmpresa, string Provincia)
        {
            return _bl.AF_DistribucionPolitica_Cantones_Obtener(CodEmpresa, Provincia);
        }

        [Authorize]
        [HttpGet("AF_DistribucionPolitica_Distritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DistribucionPolitica_Distritos_Obtener(int CodEmpresa, string Provincia, string Canton)
        {
            return _bl.AF_DistribucionPolitica_Distritos_Obtener(CodEmpresa, Provincia, Canton);
        }

        [Authorize]
        [HttpPost("AF_DistribucionPolitica_Guardar")]
        public ErrorDto AF_DistribucionPolitica_Guardar(int CodEmpresa, string Usuario, AfDistribucionesDto Info)
        {
            return _bl.AF_DistribucionPolitica_Guardar(CodEmpresa, Usuario, Info);
        }
    }
}