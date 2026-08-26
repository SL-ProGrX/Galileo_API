using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCcCaLineasController : ControllerBase
    {
        private readonly FrmCcCaLineasBL _bl;

        public FrmCcCaLineasController(IConfiguration config)
        {
            _bl = new FrmCcCaLineasBL(config);
        }

        [HttpGet("CC_CA_Lineas_Obtener")]
        public ErrorDto<CcCaLineasLista> CC_CA_Lineas_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CC_CA_Lineas_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("CC_CA_Lineas_Guardar")]
        public ErrorDto CC_CA_Lineas_Guardar(int CodEmpresa, string usuario, CcCaLineasData request)
        {
            return _bl.CC_CA_Lineas_Guardar(CodEmpresa, usuario, request);
        }

        [HttpDelete("CC_CA_CatalogoLineas_Delete")]
        public ErrorDto CC_CA_CatalogoLineas_Delete(int CodEmpresa, string Usuario, string cod_linea)
        {
            return _bl.CC_CA_CatalogoLineas_Delete(CodEmpresa, Usuario, cod_linea);
        }

        [HttpGet("CC_CA_Lineas_Cbo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Lineas_Cbo_Obtener(int CodEmpresa)
        {
            return _bl.CC_CA_Lineas_Cbo_Obtener(CodEmpresa);
        }

        [HttpGet("CC_CA_Origenes_Cbo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_CA_Origenes_Cbo_Obtener(int CodEmpresa, string tipoOrigen)
        {
            return _bl.CC_CA_Origenes_Cbo_Obtener(CodEmpresa, tipoOrigen);
        }


        [HttpGet("CC_CA_CatalogoLineas_Obtener")]
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoLineas_Obtener(int CodEmpresa, string cod_linea)
        {
            return _bl.CC_CA_CatalogoLineas_Obtener(CodEmpresa, cod_linea);
        }

        [HttpGet("CC_CA_CatalogoAsignaciones_Obtener")]
        public ErrorDto<List<CcCaCatalogoLineasData>> CC_CA_CatalogoAsignaciones_Obtener(
            int CodEmpresa,
            string tipoOrigen,
            string codigoOrigen)
        {
            return _bl.CC_CA_CatalogoAsignaciones_Obtener(CodEmpresa, tipoOrigen, codigoOrigen);
        }

        [HttpPost("CC_CA_LineasDetalle_Insertar")]
        public ErrorDto CC_CA_LineasDetalle_Insertar(int CodEmpresa, string usuario, string cod_linea, string codigo)
        {
            return _bl.CC_CA_LineasDetalle_Insertar(CodEmpresa, usuario, cod_linea, codigo);
        }

        [HttpDelete("CC_CA_LineasDetalle_Delete")]
        public ErrorDto CC_CA_LineasDetalle_Delete(int CodEmpresa, string usuario, string cod_linea, string codigo)
        {
            return _bl.CC_CA_LineasDetalle_Delete(CodEmpresa, usuario, cod_linea, codigo);
        }

        [HttpPost("CC_CA_Asignacion_Guardar")]
        public ErrorDto CC_CA_Asignacion_Guardar([FromBody] CcCaAsignacionGuardarRequest request)
        {
            return _bl.CC_CA_Asignacion_Guardar(request);
        }
    }
}
