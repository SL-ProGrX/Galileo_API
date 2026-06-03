using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCcCaEntidadesController : ControllerBase
    {
        private readonly FrmCcCaEntidadesBL _bl;
        public FrmCcCaEntidadesController(IConfiguration config)
        {
            _bl = new FrmCcCaEntidadesBL(config);
        }

        [HttpGet("CC_CA_Entidades_Obtener")]
        public ErrorDto<CaEntidadLista> CC_CA_Entidades_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CC_CA_Entidades_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("CC_CA_Entidad_Guardar")]
        public ErrorDto frmCC_CA_Entidad_Guardar(int CodEmpresa, string usuario, CaEntidadData request)
        {
            return _bl.frmCC_CA_Entidad_Guardar(CodEmpresa, usuario, request);
        }

        [HttpDelete("CC_CA_Entidad_Eliminar")]
        public ErrorDto CC_CA_Entidad_Delete(int CodEmpresa, string Usuario, string Codigo)
        {
            return _bl.CC_CA_Entidad_Delete(CodEmpresa, Usuario, Codigo);
        }
    }
}