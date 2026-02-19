using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmSysEducacionController : ControllerBase
    {
        private readonly FrmSysEducacionBL _bl;
        public FrmSysEducacionController(IConfiguration config)
        {
            _bl = new FrmSysEducacionBL(config);
        }


        [Authorize]
        [HttpGet("Sys_EducacionlLista_Obtener")]
        public ErrorDto<SysEducacionLista> Sys_EducacionlLista_Obtener(int CodEmpresa, string tipo, string filtros)
        {
           
            return _bl.Sys_EducacionlLista_Obtener(CodEmpresa, tipo, filtros);
        }
         
        [Authorize]
        [HttpPost("Sys_Educacion_Guardar")]
        public ErrorDto  Sys_Educacion_Guardar(int CodEmpresa, string usuario, SysEducacionData datos)
        {
            return _bl.Sys_Educacion_Guardar(CodEmpresa, usuario, datos);
        }
       
        [Authorize]
        [HttpDelete("Sys_Educacion_Eliminar")]
        public ErrorDto Sys_Educacion_Eliminar(int CodEmpresa, string usuario, string cod_Educ, string tipo)
        {
            return _bl.Sys_Educacion_Eliminar(CodEmpresa, usuario,cod_Educ, tipo);
        }

        [Authorize]
        [HttpGet("Sys_EducacionDetalle_Consulta")]
        public ErrorDto<List<SysEducacionDetalleData>> Sys_EducacionDetalle_Consulta(int CodEmpresa, string tipoDetalleEduc, string cod_Educ)
        {

            return _bl.Sys_EducacionDetalle_Consulta(CodEmpresa, tipoDetalleEduc, cod_Educ);
        }

        [Authorize]
        [HttpPost("Sys_EducacionDetalle_Asignar")]
        public ErrorDto Sys_EducacionDetalle_Asignar(int CodEmpresa, string usuario , string cod_DetalleEduc, string cod_Educ, bool accion)
        {
            return _bl.Sys_EducacionDetalle_Asignar(CodEmpresa, usuario, cod_Educ, cod_DetalleEduc, accion);
        }
     
    }
}