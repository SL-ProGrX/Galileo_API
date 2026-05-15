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
    public class FrmCntXErEspecialController : ControllerBase
    {
        private readonly FrmCntXErEspecialBl _bl;

        public FrmCntXErEspecialController(IConfiguration config) =>
            _bl = new FrmCntXErEspecialBl(config);

        [HttpGet("CntX_EREspecial_Consulta_Obtener")]
        public ErrorDto<CntXErEspecialDefinicionData?> CntX_EREspecial_Consulta_Obtener(
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
        public ErrorDto<List<CntXErEspecialDefinicionData>> CntX_EREspecial_Lista_Obtener(
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
            CntXErEspecialDefinicionData request)
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
        public ErrorDto<List<CntXErEspecialCuentaNodeData>> CntX_EREspecial_Arbol_Obtener(
            int codEmpresa,
            int codContabilidad,
            CntXErEspecialArbolRequest request)
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
            CntXErEspecialCuentasGuardarRequest request)
        {
            return _bl.CntX_EREspecial_Cuentas_Guardar(
                codEmpresa,
                codContabilidad,
                request);
        }
    }
}