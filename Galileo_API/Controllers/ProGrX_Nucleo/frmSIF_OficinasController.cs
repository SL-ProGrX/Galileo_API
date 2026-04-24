using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmSifOficinasController : ControllerBase
    {
        private readonly FrmSifOficinasBL _bl;

        public FrmSifOficinasController(IConfiguration config)
        {
            _bl = new FrmSifOficinasBL(config);
        }

        [Authorize]
        [HttpGet("Sif_OficinasLista_Obtener")]
        public ErrorDto<SifOficinasLista> Sif_OficinasLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sif_OficinasLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Sif_Oficinas_Guardar")]
        public ErrorDto Sif_Oficinas_Guardar(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            return _bl.Sif_Oficinas_Guardar(CodEmpresa, oficinaDatos);
        }
        [Authorize]
        [HttpPost("Sif_Oficinas_ActualizarDatos")]
        public ErrorDto Sif_Oficinas_ActualizarDatos(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            return _bl.Sif_Oficinas_ActualizarDatos(CodEmpresa, oficinaDatos);
        }
       
        [Authorize]
        [HttpGet("Sif_OficinasUnidadContable_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasUnidadContable_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Sif_OficinasUnidadContable_Obtener(CodEmpresa, contabilidad);
        }

        [Authorize]
        [HttpGet("Sif_OficinasCentroCostos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasCentroCostos_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Sif_OficinasCentroCostos_Obtener(CodEmpresa, contabilidad);
        }

        [Authorize]
        [HttpGet("Sif_Oficinas_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_Oficinas_Lista(int CodEmpresa)
        {
            return _bl.Sif_Oficinas_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Sif_OficinasMiembros_Lista")]
        public ErrorDto<List<SifOficinasMiembros>> Sif_OficinasMiembros_Lista(int CodEmpresa, string oficina, bool apoyo , bool usuariosEstado, string filtro = "")
        {
            filtro ??= "";
            return _bl.Sif_OficinasMiembros_Lista(CodEmpresa, oficina, filtro, apoyo ? 1 : 0  , usuariosEstado ? 1 : 0);
        }        

        [Authorize]
        [HttpGet("Sif_OficinasHistorial_Lista")]
        public ErrorDto<List<SifOficinasHistorial>> Sif_OficinasHistorial_Lista(int CodEmpresa, string filtro = "")
        {
            filtro ??= "";
            return _bl.Sif_OficinasHistorial_Lista(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("Sif_OficinasMiembros_Agregar")]
        public ErrorDto Sif_OficinasMiembros_Agregar(int CodEmpresa, string oficina, string usuario, bool apoyo, string usuarioRegistro, bool accion)
        {
            return _bl.Sif_OficinasMiembros_Agregar(CodEmpresa, oficina, usuario, apoyo ? 1 : 0, usuarioRegistro, accion ? "A" : "E");
        }

        [Authorize]
        [HttpGet("Sif_Oficinas_Exportar")]
        public ErrorDto<List<SifOficinasData>> Sif_Oficinas_Exportar(int CodEmpresa, string filtros)
        {
            return _bl.Sif_Oficinas_Exportar(CodEmpresa, filtros);
        }

    }
}