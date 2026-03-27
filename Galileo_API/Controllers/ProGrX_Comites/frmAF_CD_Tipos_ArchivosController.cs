using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

    namespace Galileo_API.Controllers.ProGrX_Comites
{
        [Route("api/[controller]")]
        [ApiController]
        [Authorize]
        public class FrmTipoArchivoController : ControllerBase
        {
            private readonly FrmTipoArchivoBL _bl;

            public FrmTipoArchivoController(IConfiguration config)
            {
                _bl = new FrmTipoArchivoBL(config);
            }

            [HttpGet("TipoArchivoLista_Obtener")]
            public ErrorDto<TipoArchivoLista> TipoArchivoLista_Obtener(int CodEmpresa, string filtros)
            {
                return _bl.TipoArchivoLista_Obtener(CodEmpresa, filtros);
            }

            [HttpPost("TipoArchivo_Guardar")]
            public ErrorDto TipoArchivo_Guardar(int CodEmpresa, string usuario, TipoArchivoData data)
            {
                return _bl.TipoArchivo_Guardar(CodEmpresa, usuario, data);
            }

            [HttpDelete("TipoArchivo_Eliminar")]
            public ErrorDto TipoArchivo_Eliminar(int CodEmpresa, int id, string usuario)
            {
                return _bl.TipoArchivo_Eliminar(CodEmpresa, id, usuario);
            }

            [HttpGet("TipoArchivo_Obtener")]
            public ErrorDto<List<TipoArchivoData>> TipoArchivo_Obtener(int CodEmpresa, string filtros)
            {
                return _bl.TipoArchivo_Obtener(CodEmpresa, filtros);
            }
        }
    }

