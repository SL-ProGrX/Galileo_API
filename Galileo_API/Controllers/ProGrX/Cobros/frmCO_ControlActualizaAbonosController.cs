using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoControlActualizaAbonosController : ControllerBase
    {
        private readonly FrmCoControlActualizaAbonosBL _bl;
        
        public FrmCoControlActualizaAbonosController(IConfiguration config)
        {
            _bl = new FrmCoControlActualizaAbonosBL(config);
        }

        [HttpPost("Co_ControlActualizaAbonos_Actualizar")]
        public ErrorDto Co_ControlActualizaAbonos_Actualizar(int CodEmpresa)
        {
            return _bl.Co_ControlActualizaAbonos_Actualizar(CodEmpresa);
        }
    }
}
