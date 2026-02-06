using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.BusinessTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
        public class FrmCntXPeriodosDefinicionController : ControllerBase
        {
            private readonly FrmCntXPeriodosDefinicionBL _bl;

            public FrmCntXPeriodosDefinicionController(IConfiguration config)
            {
                _bl = new FrmCntXPeriodosDefinicionBL(config);
            }

            [HttpGet("Inicial")]
            public ErrorDto<PeriodosDefinicionDto> Inicial(int codEmpresa)
            {
                return _bl.Inicial(codEmpresa);
            }


            [HttpPost("Crear")]
            public ErrorDto Crear(int codEmpresa,PeriodosDefinicionDto dto
            )
            {
                return _bl.Crear(codEmpresa, dto);
            }
        }


    }
