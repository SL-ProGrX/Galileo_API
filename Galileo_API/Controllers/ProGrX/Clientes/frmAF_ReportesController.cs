using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.BusinessLogic.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfReportesController : ControllerBase
    {
        private readonly FrmAfReportesBl BL_AF_Reportes;

        public FrmAfReportesController(IConfiguration config)
        {
            BL_AF_Reportes = new FrmAfReportesBl(config);
        }

        [Authorize]
        [HttpGet("AF_Reportes_Combos_Obtener")]
        public ErrorDto<AfReportesCombosDto> AF_Reportes_Combos_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Reportes_Combos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Provincias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Provincias_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Provincias_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Cantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Cantones_Obtener(int CodEmpresa, string provincia)
        {
            return BL_AF_Reportes.AF_Cantones_Obtener(CodEmpresa, provincia);
        }

        [Authorize]
        [HttpGet("AF_Distritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Distritos_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return BL_AF_Reportes.AF_Distritos_Obtener(CodEmpresa, provincia, canton);
        }

        [Authorize]
        [HttpGet("AF_UTrabajo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_UTrabajo_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_UTrabajo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_UProgramatica_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_UProgramatica_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_UProgramatica_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.FechaServidor_Obtener(CodEmpresa);
        }


        [Authorize]
        [HttpGet("AF_Configuracion_Grupos_Obtener")]
        public ErrorDto<List<AfGrupoConfiguracionDto>> AF_Configuracion_Grupos_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Configuracion_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Configuracion_Miembros_Obtener")]
        public ErrorDto<List<AfGrupoMiembroDto>> AF_Configuracion_Miembros_Obtener(int CodEmpresa, int CodGrupo)
        {
            return BL_AF_Reportes.AF_Configuracion_Miembros_Obtener(CodEmpresa, CodGrupo);
        }

        [Authorize]
        [HttpGet("AF_Configuracion_Informes_Obtener")]
        public ErrorDto<List<AfReporteDto>> AF_Configuracion_Informes_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Configuracion_Informes_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Seguridad_Grupos_Obtener")]
        public ErrorDto<List<AfSeguridadGrupoDto>> AF_Seguridad_Grupos_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Seguridad_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Seguridad_Miembros_Obtener")]
        public ErrorDto<List<AfSeguridadMiembroDto>> AF_Seguridad_Miembros_Obtener(int CodEmpresa, int CodGrupo)
        {
            return BL_AF_Reportes.AF_Seguridad_Miembros_Obtener(CodEmpresa, CodGrupo);
        }
        [Authorize]
        [HttpGet("AF_Seguridad_Reportes_Obtener")]
        public ErrorDto<List<AfSeguridadReporteDto>> AF_Seguridad_Reportes_Obtener(int CodEmpresa, string CodGrupo)
        {
            return BL_AF_Reportes.AF_Seguridad_Reportes_Obtener(CodEmpresa, CodGrupo);
        }

        [Authorize]
        [HttpGet("AF_Grupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Grupos_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Miembros_Grupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Miembros_Grupos_Obtener(int CodEmpresa)
        {
            return BL_AF_Reportes.AF_Miembros_Grupos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_Grupos_Guardar")]
        public ErrorDto AF_Grupos_Guardar(int CodEmpresa, AfGrupoConfiguracionDto grupo)
        {
            return BL_AF_Reportes.AF_Grupos_Guardar(CodEmpresa, grupo);
        }

        [Authorize]
        [HttpPost("AF_Miembros_Guardar")]
        public ErrorDto AF_Miembros_Guardar(int CodEmpresa, string cod_grupo, AfGrupoMiembroDto miembro)
        {
            return BL_AF_Reportes.AF_Miembros_Guardar(CodEmpresa, cod_grupo, miembro);
        }

        [Authorize]
        [HttpPost("AF_Reportes_Guardar")]
        public ErrorDto AF_Reportes_Guardar(int CodEmpresa, AfReporteDto reporte)
        {
            return BL_AF_Reportes.AF_Reportes_Guardar(CodEmpresa, reporte);
        }

        [Authorize]
        [HttpPost("AF_Reportes_Grupo_Guardar")]
        public ErrorDto AF_Reportes_Grupo_Guardar(int CodEmpresa, AfSeguridadGrupoDto grupo)
        {
            return BL_AF_Reportes.AF_Reportes_Grupo_Guardar(CodEmpresa, grupo);
        }

        [Authorize]
        [HttpPost("AF_Reportes_Grupo_Miembros_Guardar")]
        public ErrorDto AF_Reportes_Grupo_Miembros_Guardar(int CodEmpresa, string cod_grupo, AfSeguridadMiembroDto miembroseguridad)
        {
            return BL_AF_Reportes.AF_Reportes_Grupo_Miembros_Guardar(CodEmpresa, cod_grupo, miembroseguridad);
        }

        [Authorize]
        [HttpPost("AF_Reportes_Seguridad_Guardar")]
        public ErrorDto AF_Reportes_Seguridad_Guardar(int CodEmpresa, string id_rep, string cod_grupo)
        {
            return BL_AF_Reportes.AF_Reportes_Seguridad_Guardar(CodEmpresa, id_rep, cod_grupo);
        }
    }
}