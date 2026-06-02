using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFInstitucionesController : ControllerBase
    {
        private readonly FrmAFInstitucionesBL BL_AF_Instituciones;

        public FrmAFInstitucionesController(IConfiguration config)
        {
            BL_AF_Instituciones = new FrmAFInstitucionesBL(config);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Lista_Obtener(int CodEmpresa)
        {
            return BL_AF_Instituciones.AF_Instituciones_Lista_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Scroll_Obtener")]
        public ErrorDto<AfInstitucionDto?> AF_Instituciones_Scroll_Obtener(int CodEmpresa, int ScrollCode, int CodInstitucion)
        {
            return BL_AF_Instituciones.AF_Instituciones_Scroll_Obtener(CodEmpresa, ScrollCode, CodInstitucion);
        }

        [Authorize]
        [HttpGet("AF_Institucion_Obtener")]
        public ErrorDto<AfInstitucionDto?> AF_Institucion_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return BL_AF_Instituciones.AF_Institucion_Obtener(CodEmpresa, CodInstitucion);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_CargaCombo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_CargaCombo_Obtener(int CodEmpresa, string Tipo, int Conta)
        {
            return BL_AF_Instituciones.AF_Instituciones_CargaCombo_Obtener(CodEmpresa, Tipo, Conta);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Planes_Obtener(int CodEmpresa, int CodOperadora, string CodMoneda)
        {
            return BL_AF_Instituciones.AF_Instituciones_Planes_Obtener(CodEmpresa, CodOperadora, CodMoneda);
        }

        [Authorize]
        [HttpGet("AF_Institucion_Empresas_Obtener")]
        public ErrorDto<List<AfInstitucionEmpresasDto>> AF_Institucion_Empresas_Obtener(int CodEmpresa, int CodInstitucion, int Tipo)
        {
            return BL_AF_Instituciones.AF_Institucion_Empresas_Obtener(CodEmpresa, CodInstitucion, Tipo);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Codigos_Obtener")]
        public ErrorDto<List<AfInstitucionesCodigosDto>> AF_Instituciones_Codigos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return BL_AF_Instituciones.AF_Instituciones_Codigos_Obtener(CodEmpresa, CodInstitucion);
        }

        [Authorize]
        [HttpGet("AF_Instituciones_Codigos_Lineas_Obtener")]
        public ErrorDto<List<AfInstitucionesCodigosLineasDto>> AF_Instituciones_Codigos_Lineas_Obtener(int CodEmpresa, int CodInstitucion, string Codigo, int rbCodigo)
        {
            return BL_AF_Instituciones.AF_Instituciones_Codigos_Lineas_Obtener(CodEmpresa, CodInstitucion, Codigo, rbCodigo);
        }

        [Authorize]
        [HttpGet("AF_Institucion_Departamentos_Obtener")]
        public ErrorDto<List<AfInstitucionDepartamentosDto>> AF_Institucion_Departamentos_Obtener(int CodEmpresa, int CodInstitucion)
        {
            return BL_AF_Instituciones.AF_Institucion_Departamentos_Obtener(CodEmpresa, CodInstitucion);
        }

        [Authorize]
        [HttpGet("AF_Institucion_Secciones_Obtener")]
        public ErrorDto<List<AfInstitucionSeccionesDto>> AF_Institucion_Secciones_Obtener(int CodEmpresa, int CodInstitucion, string CodDepartamento)
        {
            return BL_AF_Instituciones.AF_Institucion_Secciones_Obtener(CodEmpresa, CodInstitucion, CodDepartamento);
        }

        [Authorize]
        [HttpPost("AF_Institucion_CambiarFecha")]
        public ErrorDto AF_Institucion_CambiarFecha(int CodEmpresa, int CodInstitucion, string FechaCorte, string Usuario)
        {
            return BL_AF_Instituciones.AF_Institucion_CambiarFecha(CodEmpresa, CodInstitucion, FechaCorte, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Institucion_InicializarDeduccion")]
        public ErrorDto AF_Institucion_InicializarDeduccion(int CodEmpresa, int CodInstitucion, string Proceso, string Usuario)
        {
            return BL_AF_Instituciones.AF_Institucion_InicializarDeduccion(CodEmpresa, CodInstitucion, Proceso, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Instituciones_Codigo_Guardar")]
        public ErrorDto AF_Instituciones_Codigo_Guardar(int CodEmpresa, AfInstitucionesCodigosDto Info, string Usuario)
        {
            return BL_AF_Instituciones.AF_Instituciones_Codigo_Guardar(CodEmpresa, Info, Usuario);
        }

        [Authorize]
        [HttpDelete("AF_Instituciones_Codigo_Eliminar")]
        public ErrorDto AF_Instituciones_Codigo_Eliminar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Usuario)
        {
            return BL_AF_Instituciones.AF_Instituciones_Codigo_Eliminar(CodEmpresa, CodInstitucion, CodDeduccion, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Instituciones_Lineas_Asignacion_Guardar")]
        public ErrorDto AF_Instituciones_Lineas_Asignacion_Guardar(int CodEmpresa, int CodInstitucion, string CodDeduccion, string Codigo, bool Checked, string Usuario)
        {
            return BL_AF_Instituciones.AF_Instituciones_Lineas_Asignacion_Guardar(CodEmpresa, CodInstitucion, CodDeduccion, Codigo, Checked, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Institucion_Empresas_Guardar")]
        public ErrorDto AF_Institucion_Empresas_Guardar(int CodEmpresa, int CodInstitucion, int CodDeductora, bool Checked, string Usuario)
        {
            return BL_AF_Instituciones.AF_Institucion_Empresas_Guardar(CodEmpresa, CodInstitucion, CodDeductora, Checked, Usuario);
        }


        [Authorize]
        [HttpPost("AF_Institucion_Copiar")]
        public ErrorDto AF_Institucion_Copiar(int CodEmpresa, int CodInstitucion, string CopiaDesc, string CopiaDescCorta, string Usuario)
        {
            return BL_AF_Instituciones.AF_Institucion_Copiar(CodEmpresa, CodInstitucion, CopiaDesc, CopiaDescCorta, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Institucion_Guardar")]
        public ErrorDto AF_Institucion_Guardar(int CodEmpresa, AfInstitucionDto Info, string Usuario, bool vEdita)
        {
            return BL_AF_Instituciones.AF_Institucion_Guardar(CodEmpresa, Info, Usuario, vEdita);
        }

        [Authorize]
        [HttpDelete("AF_Institucion_Eliminar")]
        public ErrorDto AF_Institucion_Eliminar(int CodEmpresa, int CodInstitucion, string Usuario)
        {
            return BL_AF_Instituciones.AF_Institucion_Eliminar(CodEmpresa, CodInstitucion, Usuario);
        }
    }
}