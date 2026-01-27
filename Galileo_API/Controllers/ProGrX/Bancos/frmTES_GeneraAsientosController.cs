using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesGeneraAsientosController : ControllerBase
    {
        private readonly FrmTesGeneraAsientosBL _bl;

        public FrmTesGeneraAsientosController(IConfiguration config)
        {
            _bl = new FrmTesGeneraAsientosBL(config);
        }

        [HttpGet("Tes_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.Tes_Bancos_Obtener(CodEmpresa, usuario);
        }

        [HttpGet("Tes_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_Tipos_Obtener(int CodEmpresa,string usuario, int cod_Banco)
        {
            return _bl.Tes_Tipos_Obtener(CodEmpresa, usuario,cod_Banco);
        }

        [HttpGet("TES_transaccionesAsientos_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_RecepcionDocumentos_Obtener(int CodEmpresa, string filtrosTransacciones, string filtros)
        {
            return _bl.TES_transaccionesAsientos_Obtener(CodEmpresa, filtrosTransacciones, filtros);
        }

        [HttpPost("TES_Traslado_Generar")]
        public ErrorDto TES_Traslado_Generar(int CodEmpresa, string trasladoLista)
        {
            return _bl.TES_Traslado_Generar(CodEmpresa, trasladoLista);
        }

    }
}
