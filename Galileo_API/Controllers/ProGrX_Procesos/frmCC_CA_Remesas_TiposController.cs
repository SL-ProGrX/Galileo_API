using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX.Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCccARemesasTiposController : ControllerBase
    {
        private readonly FrmCccARemesasTiposBL _bl;

        public FrmCccARemesasTiposController(IConfiguration config)
        {
            _bl = new FrmCccARemesasTiposBL(config);
        }
        [Authorize]
        [HttpGet("RemesasTipos_Entidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> RemesasTipos_Entidades_Obtener(int CodEmpresa)
        {
            return _bl.RemesasTipos_Entidades_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("RemesasTipos_Lista_Obtener")]
        public ErrorDto<CcCaRemesasTiposLista> RemesasTipos_Lista_Obtener(
            int CodEmpresa,
            string filtros,
            string entidad)
        {
            return _bl.RemesasTipos_Lista_Obtener(CodEmpresa, filtros, entidad);
        }
        [Authorize]
        [HttpGet("RemesasTipos_Obtener")]
        public ErrorDto<List<CcCaRemesasTiposData>> RemesasTipos_Obtener(
            int CodEmpresa,
            string filtros,
            string entidad)
        {
            return _bl.RemesasTipos_Obtener(CodEmpresa, filtros, entidad);
        }
        [Authorize]
        [HttpPost("RemesasTipos_Guardar")]
        public ErrorDto RemesasTipos_Guardar(
            int CodEmpresa,
            string usuario,
            CcCaRemesasTiposData item)
        {
            return _bl.RemesasTipos_Guardar(CodEmpresa, usuario, item);
        }
        [Authorize]
        [HttpDelete("RemesasTipos_Eliminar")]
        public ErrorDto RemesasTipos_Eliminar(
            int CodEmpresa,
            int id,
            string usuario)
        {
            return _bl.RemesasTipos_Eliminar(CodEmpresa, id, usuario);
        }
    }
}