using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvRepGeneralController : ControllerBase
    {
        private readonly FrmInvRepGeneralBL _bl;

        public FrmInvRepGeneralController(IConfiguration config)
        {
            _bl = new FrmInvRepGeneralBL(config);
        }

        [HttpGet("INV_RepGeneral_Bodegas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Bodegas_Obtener(int CodEmpresa)
        {
            return _bl.INV_RepGeneral_Bodegas_Obtener(CodEmpresa);
        }

        [HttpGet("INV_RepGeneral_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Unidades_Obtener(int CodEmpresa)
        {
            return _bl.INV_RepGeneral_Unidades_Obtener(CodEmpresa);
        }

        [HttpGet("INV_RepGeneral_Departamentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Departamentos_Obtener(int CodEmpresa)
        {
            return _bl.INV_RepGeneral_Departamentos_Obtener(CodEmpresa);
        }

        [HttpGet("INV_RepGeneral_Proveedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Proveedores_Obtener(int CodEmpresa)
        {
            return _bl.INV_RepGeneral_Proveedores_Obtener(CodEmpresa);
        }

        [HttpGet("INV_RepGeneral_Lineas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            INV_RepGeneral_Lineas_Obtener(int CodEmpresa)
        {
            return _bl.INV_RepGeneral_Lineas_Obtener(CodEmpresa);
        }

        [HttpGet("INV_RepGeneral_Uens_Obtener")]
        public ErrorDto<List<CprUensLista>>
            INV_RepGeneral_Uens_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.INV_RepGeneral_Uens_Obtener(CodEmpresa, usuario);
        }
    }
}