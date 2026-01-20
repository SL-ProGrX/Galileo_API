using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo_API.BusinessLogic.ProGrX_Activos_Fijos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosExploradorController : ControllerBase
    {
        private readonly FrmActivosExploradorBL BL_ActivosExplorador;
        public FrmActivosExploradorController(IConfiguration config)
        {
            BL_ActivosExplorador = new FrmActivosExploradorBL(config);
        }

        [Authorize]
        [HttpGet("Departamentos")]
        public ErrorDto<List<DropDownListaGenericaModel>> Departamentos(int codEmpresa)
        {
            return BL_ActivosExplorador.Departamentos(codEmpresa);
        }

        [Authorize]
        [HttpGet("Secciones")]
        public ErrorDto<List<DropDownListaGenericaModel>> Secciones(
            int codEmpresa,
            string codDepartamento)
        {
            return BL_ActivosExplorador.Secciones(codEmpresa, codDepartamento);
        }

        [Authorize]
        [HttpGet("TiposActivo")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposActivo(int codEmpresa)
        {
            return BL_ActivosExplorador.TiposActivo(codEmpresa);
        }

        [Authorize]
        [HttpGet("Justificaciones")]
        public ErrorDto<List<DropDownListaGenericaModel>> Justificaciones(int codEmpresa)
        {
            return BL_ActivosExplorador.Justificaciones(codEmpresa);
        }

        [Authorize]
        [HttpPost("Listar")]
        public ErrorDto<List<ActivoExploradorDto>> Listar(
            int codEmpresa, ActivosExploradorFiltrosDto filtros)
        {
            return BL_ActivosExplorador.Listar(codEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Ubicaciones")]
        public ErrorDto<List<DropDownListaGenericaModel>> Ubicaciones(int codEmpresa)
        {
            return BL_ActivosExplorador.Ubicaciones(codEmpresa);
        }

        [Authorize]
        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return BL_ActivosExplorador.FechaServidor_Obtener(CodEmpresa);
        }


        [Authorize]
        [HttpGet("Responsables")]
        public ErrorDto<List<DropDownListaGenericaModel>> Responsables(int codEmpresa)
        {
            return BL_ActivosExplorador.Responsables(codEmpresa);
        }


        [Authorize]
        [HttpGet("Proveedores")]
        public ErrorDto<List<DropDownListaGenericaModel>> Proveedores(int codEmpresa)
        {
            return BL_ActivosExplorador.Proveedores(codEmpresa);
        }

        [Authorize]
        [HttpGet("Periodos")]
        public ErrorDto<List<PeriodoExploradorDto>> Periodos(int codEmpresa,string estado)
        {
            return BL_ActivosExplorador.Periodos(codEmpresa, estado);
        }


    }
}