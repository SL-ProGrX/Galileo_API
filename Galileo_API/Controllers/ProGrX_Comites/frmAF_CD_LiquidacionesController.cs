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
        public class FrmAfCdLiquidacionesController : ControllerBase
        {
            private readonly FrmAfCdLiquidacionesBl _bl;

            public FrmAfCdLiquidacionesController(IConfiguration config)
            {
                _bl = new FrmAfCdLiquidacionesBl(config);
            }

            [HttpGet("AfCdComites_Lista_Obtener")]
            public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
            {
                return _bl.AfCdComites_Lista_Obtener(codEmpresa);
            }

            [HttpGet("AfCdComite_Descripcion_Obtener")]
            public ErrorDto<string?> AfCdComite_Descripcion_Obtener(int codEmpresa, string codComite)
            {
                return _bl.AfCdComite_Descripcion_Obtener(codEmpresa, codComite);
            }

            [HttpGet("AfCdLiquidaciones_Pendientes_Obtener")]
            public ErrorDto<int> AfCdLiquidaciones_Pendientes_Obtener(int codEmpresa, string codComite)
            {
                return _bl.AfCdLiquidaciones_Pendientes_Obtener(codEmpresa, codComite);
            }

            [HttpGet("AfCdOperaciones_Lista_Obtener")]
            public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Lista_Obtener(int codEmpresa, string codComite)
            {
                return _bl.AfCdOperaciones_Lista_Obtener(codEmpresa, codComite);
            }

            [HttpGet("AfCdOperaciones_Detallar_Obtener")]
            public ErrorDto<List<AfCdOperacionData>> AfCdOperaciones_Detallar_Obtener(int codEmpresa, string codComite)
            {
                return _bl.AfCdOperaciones_Detallar_Obtener(codEmpresa, codComite);
            }

            [HttpGet("AfCdOperaciones_Historico_Obtener")]
            public ErrorDto<List<AfCdOperacionHistoricoData>> AfCdOperaciones_Historico_Obtener(int codEmpresa, string codComite)
            {
                return _bl.AfCdOperaciones_Historico_Obtener(codEmpresa, codComite);
            }

            [HttpGet("AfCdFacturas_Obtener")]
            public ErrorDto<List<AfCdFacturaData>> AfCdFacturas_Obtener(int codEmpresa, int operacion)
            {
                return _bl.AfCdFacturas_Obtener(codEmpresa, operacion);
            }
        }
    }

