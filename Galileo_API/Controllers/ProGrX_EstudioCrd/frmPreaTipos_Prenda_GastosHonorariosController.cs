using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPreaTiposPrendaGastosHonorariosController : ControllerBase
    {
        private readonly FrmPreaTiposPrendaGastosHonorariosBL _bl;

        public FrmPreaTiposPrendaGastosHonorariosController(IConfiguration config)
        {
            _bl = new FrmPreaTiposPrendaGastosHonorariosBL(config);
        }

        [Authorize]
        [HttpGet("CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener")]
        public ErrorDto<CrPreaTiposPrendaGastosHonorariosListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(int CodEmpresa, string tipo, string filtros)
        {
            return _bl.CR_PreaTipos_Prenda_GastosHonorarios_Lista_Obtener(CodEmpresa, tipo, filtros);
        }

        [Authorize]
        [HttpGet("CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export")]
        public ErrorDto<CrPreaTiposPrendaGastosHonorariosListaResult> CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export(int CodEmpresa, string tipo, string filtros)
        {
            return _bl.CR_PreaTipos_Prenda_GastosHonorarios_Lista_Export(CodEmpresa, tipo, filtros);
        }

        [Authorize]
        [HttpPost("CR_PreaTipos_Prenda_GastosHonorarios_Guardar")]
        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Guardar(int CodEmpresa, string usuario, string tipo, [FromBody] CrPreaTiposPrendaGastosHonorariosGuardarRequest request)
        {
            return _bl.CR_PreaTipos_Prenda_GastosHonorarios_Guardar(CodEmpresa, usuario, tipo, request);
        }

        [Authorize]
        [HttpDelete("CR_PreaTipos_Prenda_GastosHonorarios_Eliminar")]
        public ErrorDto CR_PreaTipos_Prenda_GastosHonorarios_Eliminar(int CodEmpresa, string usuario, string tipo, int id)
        {
            return _bl.CR_PreaTipos_Prenda_GastosHonorarios_Eliminar(CodEmpresa, usuario, tipo, id);
        }
    }
}

