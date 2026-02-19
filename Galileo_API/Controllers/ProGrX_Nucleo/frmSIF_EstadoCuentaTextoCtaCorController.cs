using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmSifEstadoCuentaTextoCtaCorController : ControllerBase
    {
        private readonly FrmSifEstadoCuentaTextoCtaCorBL _bl;
        public FrmSifEstadoCuentaTextoCtaCorController(IConfiguration config)
        {
            _bl = new FrmSifEstadoCuentaTextoCtaCorBL(config);
        }


        [HttpGet("NotasEstados_Obtener")]
        public ErrorDto<SifEmpresaDto> NotasEstados_Obtener(int CodEmpresa)
        {
            return _bl.NotasEstados_Obtener(CodEmpresa);
        }

        [HttpPost("NotasEstados_Insertar")]
        public ErrorDto NotasEstados_Insertar(int CodCliente, SifEmpresaDto notas)
        {
            return _bl.NotasEstados_Insertar(CodCliente, notas);
        }

    }
}