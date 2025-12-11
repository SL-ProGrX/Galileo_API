
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosRenumeracionController : ControllerBase
    {
        private readonly FrmActivosRenumeracionBL _bl;
        public FrmActivosRenumeracionController(IConfiguration config)
        {
            _bl = new FrmActivosRenumeracionBL(config);
        }

        [Authorize]
        [HttpGet("Activos_Buscar")]
        public ErrorDto<ActivosDataLista> Activos_Buscar(int CodEmpresa, string filtros)
        {
            return _bl.Activos_Buscar(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Activos_Renumeracion_Actualizar")]
        public ErrorDto Activos_Renumeracion_Actualizar(int CodEmpresa, string usuario, string num_placa, string nuevo_num)
        {
            return _bl.Activos_Renumeracion_Actualizar(CodEmpresa, usuario, num_placa, nuevo_num);
        }

        [Authorize]
        [HttpGet("Activos_Renumeracion_Obtener")]
        public ErrorDto<ActivosRenumeracionData> Activos_Renumeracion_Obtener(int CodEmpresa, string num_placa)
        {
            return _bl.Activos_Renumeracion_Obtener(CodEmpresa, num_placa);
        }
    }
}