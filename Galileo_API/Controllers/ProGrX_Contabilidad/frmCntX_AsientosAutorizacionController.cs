using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXAsientosAutorizacionController : ControllerBase
    {
        private readonly FrmCntXAsientosAutorizacionBl _bl;

        public FrmCntXAsientosAutorizacionController(IConfiguration config) => 
            _bl = new FrmCntXAsientosAutorizacionBl(config);

        [HttpGet("CntXAsientos_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXAsientos_Tipos_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXAsientos_Tipos_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXAsientos_ListaPendientes_Obtener")]
        public ErrorDto<List<CntXAsientoAutorizacionData>> CntXAsientos_ListaPendientes_Obtener(int codEmpresa, int codConta, string tipoAsiento, int anio, int mes)
        {
            return _bl.CntXAsientos_ListaPendientes_Obtener(codEmpresa, codConta, tipoAsiento, anio, mes);
        }

        [HttpPost("CntXAsientos_Autorizar")]
        public ErrorDto CntXAsientos_Autorizar(int codEmpresa, int codConta, string usuario, List<CntXAsientoAutorizacionData> lista)
        {
            return _bl.CntXAsientos_Autorizar(codEmpresa, codConta, usuario, lista);
        }
    }
}