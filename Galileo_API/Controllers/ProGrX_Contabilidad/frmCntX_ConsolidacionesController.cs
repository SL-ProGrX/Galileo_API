using Galileo.Models;
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
    public class FrmCntXConsolidacionesController : ControllerBase
    {
        private readonly FrmCntXConsolidacionesBl _bl;

        public FrmCntXConsolidacionesController(IConfiguration config) =>
            _bl = new FrmCntXConsolidacionesBl(config);

        [HttpGet("CntXConsolidaciones_Consulta_Obtener")]
        public ErrorDto<CntXConsolidacionDefinicionData?> CntXConsolidaciones_Consulta_Obtener(
            int codEmpresa,
            int codConsolida)
        {
            return _bl.CntXConsolidaciones_Consulta_Obtener(codEmpresa, codConsolida);
        }

        [HttpGet("CntXConsolidaciones_Scroll_Obtener")]
        public ErrorDto<CntXConsolidacionDefinicionData?> CntXConsolidaciones_Scroll_Obtener(
            int codEmpresa,
            int codConsolidaActual,
            string direccion)
        {
            return _bl.CntXConsolidaciones_Scroll_Obtener(
                codEmpresa,
                codConsolidaActual,
                direccion);
        }


        [HttpGet("CntXConsolidaciones_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXConsolidaciones_Lista_Obtener(int codEmpresa)
        {
            return _bl.CntXConsolidaciones_Lista_Obtener(codEmpresa);
        }

        [HttpGet("CntXConsolidaciones_Contabilidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXConsolidaciones_Contabilidades_Obtener(int codEmpresa)
        {
            return _bl.CntXConsolidaciones_Contabilidades_Obtener(codEmpresa);
        }

        [HttpGet("CntXConsolidaciones_ContabilidadesLocales_Obtener")]
        public ErrorDto<List<CntXConsolidacionContabilidadData>> CntXConsolidaciones_ContabilidadesLocales_Obtener(
            int codEmpresa,
            int codContabilidad,
            int codConsolida)
        {
            return _bl.CntXConsolidaciones_ContabilidadesLocales_Obtener(
                codEmpresa,
                codContabilidad,
                codConsolida);
        }

        [HttpGet("CntXConsolidaciones_PortalesRaiz_Obtener")]
        public ErrorDto<List<CntXConsolidacionPortalNodeData>> CntXConsolidaciones_PortalesRaiz_Obtener(int codEmpresa)
        {
            return _bl.CntXConsolidaciones_PortalesRaiz_Obtener(codEmpresa);
        }

        [HttpGet("CntXConsolidaciones_PortalesContabilidades_Obtener")]
        public ErrorDto<List<CntXConsolidacionPortalNodeData>> CntXConsolidaciones_PortalesContabilidades_Obtener(
            int codEmpresa,
            int codPortal,
            int codContabilidadBase,
            int codConsolida)
        {
            return _bl.CntXConsolidaciones_PortalesContabilidades_Obtener(
                codEmpresa,
                codPortal,
                codContabilidadBase,
                codConsolida);
        }

        [HttpPost("CntXConsolidaciones_Guardar")]
        public ErrorDto CntXConsolidaciones_Guardar(
            int codEmpresa,
            string usuario,
            CntXConsolidacionesGuardarRequest request)
        {
            return _bl.CntXConsolidaciones_Guardar(codEmpresa, usuario, request);
        }

        [HttpPost("CntXConsolidaciones_Borrar")]
        public ErrorDto CntXConsolidaciones_Borrar(
            int codEmpresa,
            int codConsolida,
            string usuario)
        {
            return _bl.CntXConsolidaciones_Borrar(codEmpresa, codConsolida, usuario);
        }
    }
}
