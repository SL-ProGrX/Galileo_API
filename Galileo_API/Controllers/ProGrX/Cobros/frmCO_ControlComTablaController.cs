using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoControlComTablaController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCoControlComTablaBL _bl;

        public FrmCoControlComTablaController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCoControlComTablaBL(_config);
        }

        [Authorize]
        [HttpGet("CO_ControlComTabla_Obtener")]
        public ErrorDto<CoControlComTablaLista> CO_ControlComTabla_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CO_ControlComTabla_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CO_ControlComTabla_Guardar")]
        public ErrorDto CO_ControlComTabla_Guardar(int CodEmpresa, string usuario, CoControlComTablaData data)
        {
            return _bl.CO_ControlComTabla_Guardar(CodEmpresa, usuario, data);
        }

        [Authorize]
        [HttpDelete("CO_ControlComTabla_Delete")]
        public ErrorDto CO_ControlComTabla_Delete(int CodEmpresa, string usuario, int id_Linea)
        {
            return _bl.CO_ControlComTabla_Delete(CodEmpresa, usuario, id_Linea);
        }

    }
}
