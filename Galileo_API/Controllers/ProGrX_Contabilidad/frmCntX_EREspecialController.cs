using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXEREspecialController : ControllerBase
    {
        private readonly FrmCntXEREspecialBl _bl;

        public FrmCntXEREspecialController(IConfiguration config) =>
            _bl = new FrmCntXEREspecialBl(config);

        [HttpGet("CntX_EREspecial_Consulta_Obtener")]
        public ErrorDto<CntXEREspecialDefinicionData?> CntX_EREspecial_Consulta_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codErEspecial)
        {
            return _bl.CntX_EREspecial_Consulta_Obtener(
                codEmpresa,
                codContabilidad,
                codErEspecial);
        }

        [HttpGet("CntX_EREspecial_Lista_Obtener")]
        public ErrorDto<List<CntXEREspecialDefinicionData>> CntX_EREspecial_Lista_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return _bl.CntX_EREspecial_Lista_Obtener(
                codEmpresa,
                codContabilidad);
        }

        [HttpPost("CntX_EREspecial_Guardar")]
        public ErrorDto CntX_EREspecial_Guardar(
            int codEmpresa,
            int codContabilidad,
            string usuario,
            CntXEREspecialDefinicionData request)
        {
            return _bl.CntX_EREspecial_Guardar(
                codEmpresa,
                codContabilidad,
                usuario,
                request);
        }

        [HttpDelete("CntX_EREspecial_Borrar")]
        public ErrorDto CntX_EREspecial_Borrar(
            int codEmpresa,
            int codContabilidad,
            int codErEspecial,
            string usuario)
        {
            return _bl.CntX_EREspecial_Borrar(
                codEmpresa,
                codContabilidad,
                codErEspecial,
                usuario);
        }

        [HttpPost("CntX_EREspecial_Arbol_Obtener")]
        public ErrorDto<List<CntXEREspecialCuentaNodeData>> CntX_EREspecial_Arbol_Obtener(
            int codEmpresa,
            int codContabilidad,
            CntXEREspecialArbolRequest request)
        {
            return _bl.CntX_EREspecial_Arbol_Obtener(
                codEmpresa,
                codContabilidad,
                request);
        }

        [HttpPost("CntX_EREspecial_Cuentas_Guardar")]
        public ErrorDto CntX_EREspecial_Cuentas_Guardar(
            int codEmpresa,
            int codContabilidad,
            CntXEREspecialCuentasGuardarRequest request)
        {
            return _bl.CntX_EREspecial_Cuentas_Guardar(
                codEmpresa,
                codContabilidad,
                request);
        }
    }
}