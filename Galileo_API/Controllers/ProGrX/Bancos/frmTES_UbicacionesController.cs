using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesUbicacionesController : ControllerBase
    {
        private readonly FrmTesUbicacionesBL _bl;

        public FrmTesUbicacionesController(IConfiguration config)
        {
            _bl = new FrmTesUbicacionesBL(config);
        }

       
        [HttpGet("Tes_UbicacionesLista_Obtener")]
        public ErrorDto<TesUbicacionesLista> Tes_UbicacionesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Tes_UbicacionesLista_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("Tes_Ubicaciones_Guardar")]
        public ErrorDto Tes_Ubicaciones_Guardar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
        {
            return _bl.Tes_Ubicaciones_Guardar(CodEmpresa, usuario, ubicacion);
        }

        [HttpDelete("Tes_Ubicaciones_Eliminar")]
        public ErrorDto Tes_Ubicaciones_Eliminar(int CodEmpresa, string ubicacion, string usuario)
        {
            return _bl.Tes_Ubicaciones_Eliminar(CodEmpresa, ubicacion, usuario);
        }

        [HttpGet("Tes_Ubicaciones_Obtener")]
        public ErrorDto<List<TesUbicacionesData>> Tes_Ubicaciones_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Tes_Ubicaciones_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Tes_UbicacionesUsuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_UbicacionesUsuarios_Obtener(int CodEmpresa)
        {
            return _bl.Tes_UbicacionesUsuarios_Obtener(CodEmpresa);
        }

        [HttpGet("Tes_Ubicaciones_Valida")]
        public ErrorDto Tes_Ubicaciones_Valida(int CodEmpresa, string cod_ubicacion)
        {
            return _bl.Tes_Ubicaciones_Valida(CodEmpresa, cod_ubicacion);
        }
    }
}
