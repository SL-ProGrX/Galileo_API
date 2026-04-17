using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasTrasladosEfectivoController : ControllerBase
    {
        private readonly FrmCajasTrasladosEfectivoBl BlCajasTrasladosEfectivo;

        public FrmCajasTrasladosEfectivoController(IConfiguration config)
        {
            BlCajasTrasladosEfectivo = new FrmCajasTrasladosEfectivoBl(config);
        }

        [Authorize]
        [HttpGet("Cajas_TrasladosEfectivo_Obtener")]
        public ErrorDto<List<CajasTrasladosEfectivoDto>> Cajas_TrasladosEfectivo_Obtener(int CodEmpresa, string Filtros)
        {
            return BlCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Obtener(CodEmpresa, Filtros);
        }

        [Authorize]
        [HttpGet("Cajas_TrasladosEfectivo_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TrasladosEfectivo_Catalogo_Obtener(int CodEmpresa, int Index, string IdCaja)
        {
            return BlCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Catalogo_Obtener(CodEmpresa, Index, IdCaja);
        }

        [Authorize]
        [HttpGet("Cajas_TrasladosEfectivo_Movimientos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TrasladosEfectivo_Movimientos_Obtener(int CodEmpresa, string IdCaja)
        {
            return BlCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Movimientos_Obtener(CodEmpresa, IdCaja);
        }

        [Authorize]
        [HttpGet("Cajas_TrasladosEfectivo_TipoCambio_Obtener")]
        public ErrorDto<decimal> Cajas_TrasladosEfectivo_TipoCambio_Obtener(int CodEmpresa, string Divisa)
        {
            return BlCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_TipoCambio_Obtener(CodEmpresa, Divisa);
        }

        [Authorize]
        [HttpPost("Cajas_TrasladosEfectivo_Resolucion_Aplicar")]
        public ErrorDto Cajas_TrasladosEfectivo_Resolucion_Aplicar(int CodEmpresa, CajasTeResolucionRequest Request)
        {
            return BlCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Resolucion_Aplicar(CodEmpresa, Request);
        }

        [Authorize]
        [HttpPost("Cajas_TrasladosEfectivo_Registrar")]
        public ErrorDto Cajas_TrasladosEfectivo_Registrar(int CodEmpresa, string Movimiento, CajasTrasladosEfectivoDto Request)
        {
            return BlCajasTrasladosEfectivo.Cajas_TrasladosEfectivo_Registrar(CodEmpresa, Movimiento, Request);
        }
    }
}