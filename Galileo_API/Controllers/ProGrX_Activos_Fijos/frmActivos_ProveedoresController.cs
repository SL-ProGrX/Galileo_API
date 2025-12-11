using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosProveedoresController : ControllerBase
    {
        private readonly FrmActivosProveedoresBL _bl;

        public FrmActivosProveedoresController(IConfiguration config)
        {
            _bl = new FrmActivosProveedoresBL(config);
        }


        [Authorize]
        [HttpGet("Activos_ProveedoresLista_Obtener")]
        public ErrorDto<ActivosProveedoresLista> Activos_ProveedoresLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_ProveedoresLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Activos_Proveedores_Obtener")]
        public ErrorDto<List<ActivosProveedoresData>> Activos_Proveedores_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_Proveedores_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_Proveedores_Guardar")]
        public ErrorDto Activos_Proveedores_Guardar(int CodEmpresa, string usuario, [FromBody] ActivosProveedoresData proveedor)
        {
            return _bl.Activos_Proveedores_Guardar(CodEmpresa, usuario, proveedor);
        }

        [Authorize]
        [HttpDelete("Activos_Proveedores_Eliminar")]
        public ErrorDto Activos_Proveedores_Eliminar(int CodEmpresa, string usuario, string cod_proveedor)
        {
            return _bl.Activos_Proveedores_Eliminar(CodEmpresa, usuario, cod_proveedor);
        }

        [Authorize]
        [HttpPost("Activos_Proveedores_Importar")]
        public ErrorDto Activos_Proveedores_Importar(int CodEmpresa, string usuario)
        {
            return _bl.Activos_Proveedores_Importar(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpGet("Activos_Proveedores_Valida")]
        public ErrorDto Activos_Proveedores_Valida(int CodEmpresa, string cod_proveedor)
        {
            return _bl.Activos_Proveedores_Valida(CodEmpresa, cod_proveedor);
        }
    }
}