using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndSubCuentasController : ControllerBase
    {
        private readonly FrmFndSubCuentasBL _BL;

        public FrmFndSubCuentasController(IConfiguration? config)
        {
            _BL = new FrmFndSubCuentasBL(config);
        }

        [Authorize]
        [HttpGet("FND_SubCuentas_Lista_Obtener")]
        public ErrorDto<List<FndSubCuentasData>> FND_SubCuentas_Lista_Obtener(int CodEmpresa, int operadora, string plan, long contrato)
        {
            return _BL.FND_SubCuentas_Lista_Obtener(CodEmpresa, operadora, plan, contrato);
        }

        [Authorize]
        [HttpGet("FND_SubCuentas_Parentescos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_SubCuentas_Parentescos_Obtener(int CodEmpresa)
        {
            return _BL.FND_SubCuentas_Parentescos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("FND_SubCuentas_Guardar")]
        public ErrorDto FND_SubCuentas_Guardar(int CodEmpresa, string usuario, FndSubCuentasData data)
        {
            return _BL.FND_SubCuentas_Guardar(CodEmpresa, usuario, data);
        }

        [Authorize]
        [HttpDelete("FND_SubCuentas_Eliminar")]
        public ErrorDto FND_SubCuentas_Eliminar(int CodEmpresa, int consec, string usuario)
        {
            return _BL.FNDSubCuentas_Borrar(CodEmpresa, consec, usuario);
        }

        [Authorize]
        [HttpGet("FND_SubCuentas_Cedula_Obtener")]
        public ErrorDto<string> FND_SubCuentas_Cedula_Obtener(int CodEmpresa, string plan, long contrato, int operadora)
        {
            return _BL.FNDDSubCuentas_Cedula_Obtener(CodEmpresa, plan, contrato, operadora);
        }
    }
}