using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasDetallePagoController : ControllerBase
    {
        private readonly FrmCajasDetallePagoBL _bl;
        public FrmCajasDetallePagoController(IConfiguration config)
        {
            _bl = new FrmCajasDetallePagoBL(config);
        }

        [Authorize]
        [HttpGet("Cajas_TipoCambio")]
        public ErrorDto<decimal> Cajas_TipoCambio(int CodEmpresa, int Enlace, string Divisa)
        {
            return _bl.ObtenerTipoCambio(CodEmpresa, Enlace, Divisa);
        }

        [Authorize]
        [HttpDelete("Cajas_DesglocePago_Eliminar")]
        public ErrorDto Cajas_DesglocePago_Eliminar(int CodEmpresa, string CodCaja, int CodApertura, string Ticket, int Linea)
        {
            return _bl.Cajas_DesglocePago_Eliminar(CodEmpresa, CodCaja, CodApertura, Ticket, Linea);
        }

        [Authorize]
        [HttpGet("Cajas_DisponibleFondos")]
        public ErrorDto<CajasDisponibleFondosDto> Cajas_DisponibleFondos(int CodEmpresa, string CodCaja, int CodApertura, string Ticket, string CodPlan, int CodContrato)
        {
            return _bl.Cajas_DisponibleFondos(CodEmpresa, CodCaja, CodApertura, Ticket, CodPlan, CodContrato);
        }

        [Authorize]
        [HttpGet("Cajas_SaldoFavor")]
        public ErrorDto<List<CajasSaldoFavorDetDto>> Cajas_SaldoFavor_Obtener(int CodEmpresa, string ClienteId, int Referencia, string ReferenciaTexto)
        {
            return _bl.Cajas_SaldoFavor_Obtener(CodEmpresa, ClienteId, Referencia, ReferenciaTexto);
        }
        [Authorize]
        [HttpGet("Cajas_DivisaFuncional")]
        public ErrorDto<CajasDivisaFuncionalDto> Cajas_DivisaFuncional_Obtener(int CodEmpresa, string Enlace)
        {
            return _bl.Cajas_DivisaFuncional_Obtener(CodEmpresa, Enlace);
        }

        [Authorize]
        [HttpGet("Cajas_DepositosCuentasBancariasAut_Obtener")]
        public ErrorDto<List<CajasDepositosCuentasBancariasDto>> Cajas_DepositosCuentasBancariasAut_Obtener(int CodEmpresa, string FormaPago)
        {
            return _bl.Cajas_DepositosCuentasBancariasAut_Obtener(CodEmpresa, FormaPago);
        }

        [Authorize]
        [HttpGet("Cajas_DesglocePago")]
        public ErrorDto<List<CajasDesglocePagoDto>> Cajas_DesglocePago_Obtener(int CodEmpresa, string CodCaja, string Ticket, int CodApertura, int Linea)
        {
            return _bl.Cajas_DesglocePago_Obtener(CodEmpresa, CodCaja, Ticket, CodApertura, Linea);
        }

        [Authorize]
        [HttpPost("Cajas_DesglocePago_Insert")]
        public ErrorDto Cajas_DesglocePago_Insert(int CodEmpresa, CajasDesglocePagoDto dto)
        {
            return _bl.Cajas_DesglocePago_Insert(CodEmpresa, dto);
        }

        [Authorize]
        [HttpPut("Cajas_DesglocePago_Update")]
        public ErrorDto Cajas_DesglocePago_Update(int CodEmpresa, CajasDesglocePagoDto dto)
        {
            return _bl.Cajas_DesglocePago_Update(CodEmpresa, dto);
        }

        [Authorize]
        [HttpPost("Cajas_DistribuyeSaldoFavor")]
        public ErrorDto Cajas_DistribuyeSaldoFavor(int CodEmpresa, DistribuyeSaldoFavorDto dto)
        {
            return _bl.Cajas_DistribuyeSaldoFavor(CodEmpresa, dto);
        }

        [Authorize]
        [HttpPost("Cajas_DesglocePago_Guardar")]
        public ErrorDto Cajas_DesglocePago_Guardar(int CodEmpresa, CajasDesglocePagoRequest request)
        {
            return _bl.Cajas_DesglocePago_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("Cajas_Catalogos_Obtener")]
        public ErrorDto<CajasCatalogosDto> Cajas_Catalogos_Obtener(int CodEmpresa, string codCliente, string codCaja, int apertura,
            string? tiquete, string? productoCodigo, int? productoNumero)
        {
            return _bl.Cajas_Catalogos_Obtener(CodEmpresa, codCliente, codCaja, apertura, tiquete, productoCodigo, productoNumero);
        }

        [Authorize]
        [HttpGet("Cajas_FormasPago_Obtener")]
        public ErrorDto<List<CajasFormaPagoDto>> Cajas_FormasPago_Obtener(int CodEmpresa, string codCaja)
        {
            return _bl.Cajas_FormasPago_Obtener(CodEmpresa, codCaja);
        }

        [Authorize]
        [HttpGet("Cajas_Tiquete_Obtener")]
        public ErrorDto<List<CajasTiqueteDto>> Cajas_Tiquete_Obtener(int CodEmpresa, int Enlace, string codCaja, string tiquete, int apertura)
        {
            return _bl.Cajas_Tiquete_Obtener(CodEmpresa, Enlace, codCaja, tiquete, apertura);
        }

        [Authorize]
        [HttpGet("Cajas_ReciboDigital")]
        public ErrorDto<bool> Cajas_ReciboDigital(int CodEmpresa, string codCaja, int apertura, string tiquete)
        {
            return _bl.Cajas_ReciboDigital(CodEmpresa, codCaja, apertura, tiquete);
        }
    }
}
