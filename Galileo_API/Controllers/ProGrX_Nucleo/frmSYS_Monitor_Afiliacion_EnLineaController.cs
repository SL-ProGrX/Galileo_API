using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.Models.ProGrX_Nucleo;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysMonitorAfiliacionEnLineaController : ControllerBase
    {
        private readonly FrmSysMonitorAfiliacionEnLineaBL _bl;
        public FrmSysMonitorAfiliacionEnLineaController(IConfiguration config)
        {
            _bl = new FrmSysMonitorAfiliacionEnLineaBL(config);
        }

            [Authorize]
            [HttpPost("Buscar")]
            public ErrorDto<List<AfiliacionTablaDto>> Buscar(int codEmpresa,AfiliacionFiltroDto filtros)
            {
                return _bl.Buscar(codEmpresa, filtros);
            }

            [Authorize]
            [HttpGet("Caso")]
            public ErrorDto<AfiliacionCasoDto?> Caso(int codEmpresa,long solicitudId)
            {
                return _bl.Caso(codEmpresa, solicitudId);
            }

            [Authorize]
            [HttpGet("Resumen")]
            public ErrorDto<List<AfiliacionResumenDto>> Resumen(int codEmpresa,DateTime inicio, DateTime corte)
            {
                return _bl.Resumen(codEmpresa, inicio, corte);
            }

            [Authorize]
            [HttpPost("Resolver")]
            public ErrorDto Resolver(int codEmpresa,long solicitudId,string estado,string usuario)
            {
                return _bl.Resolver(codEmpresa,solicitudId,estado,usuario
                );
            }
        }


    }

