using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfComisionesController : ControllerBase
    {
        private readonly FrmAfComisionesBL _bl;
        public FrmAfComisionesController(IConfiguration config)
        {
            _bl = new FrmAfComisionesBL(config);
        }

        #region Remesa
        
        [Authorize]
        [HttpGet("AF_ComisionesRemesa_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_ComisionesRemesa_Obtener(int CodEmpresa, bool exporta, string filtros)
        {
            return _bl.AF_ComisionesRemesa_Obtener(CodEmpresa, exporta, filtros);
        }

        [Authorize]
        [HttpGet("AF_ComisionesRemesa_Total")]
        public ErrorDto<decimal> AF_ComisionesRemesa_Total(int CodEmpresa, int cod_comision)
        {
            return _bl.AF_ComisionesRemesa_Total(CodEmpresa, cod_comision);
        }

        [Authorize]
        [HttpPost("AF_ComisionesRemesa_Guardar")]
        public ErrorDto AF_ComisionesRemesa_Guardar(int CodEmpresa, string usuario, AfComisionDto comision)
        {
            return _bl.AF_ComisionesRemesa_Guardar(CodEmpresa, usuario, comision);
        }

        [Authorize]
        [HttpDelete("AF_ComisionesRemesa_Borrar")]
        public ErrorDto AF_ComisionesRemesa_Borrar(int CodEmpresa, string usuario, int cod_comision)
        {
            return _bl.AF_ComisionesRemesa_Borrar(CodEmpresa, usuario, cod_comision);
        }

        #endregion

        #region Generacion

        [Authorize]
        [HttpGet("AF_ComisionesGenera_Obtener")]
        public ErrorDto<List<AfComisionDto>> AF_ComisionesGenera_Obtener(int CodEmpresa)
        {
            return _bl.AF_ComisionesGenera_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_ComisionesGenera_Buscar")]
        public ErrorDto<List<AfComisionPromotorData>> AF_ComisionesGenera_Buscar(int CodEmpresa, string tipo)
        {
            return _bl.AF_ComisionesGenera_Buscar(CodEmpresa, tipo);
        }

        [Authorize]
        [HttpPost("AF_ComisionesGenera_Generar")]
        public ErrorDto AF_ComisionesGenera_Generar(int CodEmpresa, string usuario, int comision, List<AfComisionPromotorData> promotor)
        {
            return _bl.AF_ComisionesGenera_Generar(CodEmpresa, usuario, comision, promotor);
        }

        #endregion

        #region Pago
        
        [Authorize]
        [HttpGet("AF_ComisionesPago_Obtener")]
        public ErrorDto<List<AfComisionDto>> AF_ComisionesPago_Obtener(int CodEmpresa)
        {
            return _bl.AF_ComisionesPago_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_ComisionesPagoBanco_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesPagoBanco_Obtener(int CodEmpresa, int comision)
        {
            return _bl.AF_ComisionesPagoBanco_Obtener(CodEmpresa, comision);
        }

        [Authorize]
        [HttpGet("AF_ComisionesPago_Buscar")]
        public ErrorDto<List<AfComisionPagoData>> AF_ComisionesPago_Buscar(int CodEmpresa, int comision, int banco)
        {
            return _bl.AF_ComisionesPago_Buscar(CodEmpresa, comision, banco);
        }

        [Authorize]
        [HttpPost("AF_ComisionesPago_Generar")]
        public ErrorDto AF_ComisionesPago_Generar(int CodEmpresa, string usuario, int comision, List<AfComisionPagoData> pagos)
        {
            return _bl.AF_ComisionesPago_Generar(CodEmpresa, usuario, comision, pagos);
        }

        #endregion

        #region Reportes

        [Authorize]
        [HttpGet("Af_Comisiones_RepBancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepBancos_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            return _bl.Af_Comisiones_RepBancos_Obtener(CodEmpresa, chkRepRemesas, cod_comision);
        }

        [Authorize]
        [HttpGet("Af_Comisiones_RepPromotores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepPromotores_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            return _bl.Af_Comisiones_RepPromotores_Obtener(CodEmpresa, chkRepRemesas, cod_comision);
        }

        [Authorize]
        [HttpGet("Af_Comisiones_RepRemesa_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepRemesa_Obtener(int CodEmpresa)
        {
            return _bl.Af_Comisiones_RepRemesa_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Af_Comisiones_RepUsuario_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Af_Comisiones_RepUsuario_Obtener(int CodEmpresa, bool chkRepRemesas, int cod_comision)
        {
            return _bl.Af_Comisiones_RepUsuario_Obtener(CodEmpresa, chkRepRemesas, cod_comision);
        }

        #endregion
    }
}