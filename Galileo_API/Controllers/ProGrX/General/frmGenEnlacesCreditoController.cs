using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmGenEnlacesCredito")]
    [ApiController]
    [Authorize]
    public class FrmGenEnlacesCreditoController : ControllerBase
    {
        private readonly FrmGenEnlacesCreditoBl _bl;

        public FrmGenEnlacesCreditoController(IConfiguration config)
        {
            _bl = new FrmGenEnlacesCreditoBl(config);
        }

        [HttpPost("Gen_EnlacesCredito_Sincronizar")]
        public ErrorDto Gen_EnlacesCredito_Sincronizar(int CodEmpresa)
        {
            return _bl.Gen_EnlacesCredito_Sincronizar(CodEmpresa);
        }

        [HttpGet("Gen_EnlacesCreditoLista_Obtener")]
        public ErrorDto<GenEnlacesCreditoLista> Gen_EnlacesCreditoLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Gen_EnlacesCreditoLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Gen_EnlacesCredito_Obtener")]
        public ErrorDto<List<GenEnlacesCreditoData>> Gen_EnlacesCredito_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Gen_EnlacesCredito_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Gen_EnlacesCreditoCatalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Gen_EnlacesCreditoCatalogo_Obtener(int CodEmpresa, int cod_institucion)
        {
            return _bl.Gen_EnlacesCreditoCatalogo_Obtener(CodEmpresa, cod_institucion);
        }

        [HttpPut("Gen_EnlacesCredito_Actualizar")]
        public ErrorDto Gen_EnlacesCredito_Actualizar(int CodEmpresa, GenEnlacesCreditoData enlace)
        {
            return _bl.Gen_EnlacesCredito_Actualizar(CodEmpresa, enlace);
        }
    }
}
