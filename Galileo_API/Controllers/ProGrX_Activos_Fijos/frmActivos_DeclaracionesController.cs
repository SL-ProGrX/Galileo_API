using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosDeclaracionesController : ControllerBase
    {
        private readonly FrmActivosDeclaracionesBL _bl;

        public FrmActivosDeclaracionesController(IConfiguration config)
        {
            _bl = new FrmActivosDeclaracionesBL(config);
        }

        [HttpGet("Activos_Declaraciones_Lista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosDeclaracionLista> Activos_Declaraciones_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_Declaraciones_Lista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_Declaraciones_Registro_Obtener")]
        [Authorize]
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Obtener(int CodEmpresa, int id_declara)
        {
            return _bl.Activos_Declaraciones_Registro_Obtener(CodEmpresa, id_declara);
        }

        [HttpPost("Activos_Declaraciones_Registro_Guardar")]
        [Authorize]
        public ErrorDto<ActivosDeclaracionResult> Activos_Declaraciones_Registro_Guardar(int CodEmpresa, ActivosDeclaracionGuardarRequest data)
        {
            return _bl.Activos_Declaraciones_Registro_Guardar(CodEmpresa, data);
        }

        [HttpDelete("Activos_Declaraciones_Registro_Eliminar")]
        [Authorize]
        public ErrorDto Activos_Declaraciones_Registro_Eliminar(int CodEmpresa, int id_declara, string usuario)
        {
            return _bl.Activos_Declaraciones_Registro_Eliminar(CodEmpresa, id_declara, usuario);
        }

        [HttpPost("Activos_Declaraciones_Registro_Cerrar")]
        [Authorize]
        public ErrorDto Activos_Declaraciones_Registro_Cerrar(int CodEmpresa, int id_declara, string usuario)
        {
            return _bl.Activos_Declaraciones_Registro_Cerrar(CodEmpresa, id_declara, usuario);
        }

        [HttpPost("Activos_Declaraciones_Registro_Procesar")]
        [Authorize]
        public ErrorDto Activos_Declaraciones_Registro_Procesar(int CodEmpresa, int id_declara, string usuario)
        {
            return _bl.Activos_Declaraciones_Registro_Procesar(CodEmpresa, id_declara, usuario);
        }

        [HttpGet("Activos_Declaraciones_Registro_Scroll")]
        [Authorize]
        public ErrorDto<ActivosDeclaracion> Activos_Declaraciones_Registro_Scroll(int CodEmpresa, int scroll, int? id_declara, string usuario)
        {
            return _bl.Activos_Declaraciones_Registro_Scroll(CodEmpresa, scroll, id_declara, usuario);
        }
    }
}
