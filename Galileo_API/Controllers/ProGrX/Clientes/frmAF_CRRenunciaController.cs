using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrRenunciaController : ControllerBase
    {
        private readonly FrmAFCrRenunciaBL _bl;

        public FrmAFCrRenunciaController(IConfiguration config)
        {
            _bl = new FrmAFCrRenunciaBL(config);
        }

        [Authorize]
        [HttpGet("AF_CR_RenunciasSocios_Obtener")]
        public ErrorDto<List<AfRenunciasSocios>> AF_CR_RenunciasSocios_Obtener(int CodEmpresa)
        {
            return _bl.AF_CR_RenunciasSocios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Estado_Obtener")]
        public ErrorDto<AfRenunciasSocioDetalle?> AF_CR_Renuncias_Estado_Obtener(int CodEmpresa, string cedula)
        {
            return _bl.AF_CR_Renuncias_Estado_Obtener(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Bancos_Obtener")]
        public ErrorDto<List<AfRenunciaBancos>> AF_CR_Renuncias_Bancos_Obtener(int CodEmpresa, [FromBody] AfRenunciaBancoFiltro filtro)
        {
            return _bl.AF_CR_Renuncias_Bancos_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Emite_TDoc")]
        public ErrorDto<List<AfRenunciaEmiteTDoc>> AF_CR_Renuncias_Emite_TDoc(int CodEmpresa, [FromBody] AfRenunciaEmiteTDocFiltro filtro)
        {
            return _bl.AF_CR_Renuncias_Emite_TDoc(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_TipoAccion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncias_TipoAccion_Obtener(int CodEmpresa)
        {
            return _bl.AF_CR_Renuncias_TipoAccion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Causas_ObtenerDetalle")]
        public ErrorDto<AfRenunciaCausasDetalle?> AF_CR_Renuncias_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            return _bl.AF_CR_Renuncias_Causas_ObtenerDetalle(CodEmpresa, Causa);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Liq_Consulta_Patrimonio")]
        public ErrorDto<List<AfRenunciaLiqConsultaPatrimonio>> AF_CR_Renuncias_Liq_Consulta_Patrimonio(int CodEmpresa, String Cedula)
        {
            return _bl.AF_CR_Renuncias_Liq_Consulta_Patrimonio(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Exc_Renta_Detallada")]
        public ErrorDto<List<AfRenunciaExcRentaDetallada>> AF_CR_Renuncias_Exc_Renta_Detallada(int CodEmpresa, decimal Monto)
        {
            return _bl.AF_CR_Renuncias_Exc_Renta_Detallada(CodEmpresa, Monto);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Liquida_ListaPlanes")]
        public ErrorDto<List<AfRenunciaLiquidaListaPlanes>> AF_CR_Renuncias_Liquida_ListaPlanes(int CodEmpresa, [FromBody] AfRenunciaLiquidaListaPlanesFiltro filtro)
        {
            return _bl.AF_CR_Renuncias_Liquida_ListaPlanes(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_CuentasBancarias_Obtener")]
        public ErrorDto<List<AfRenunciaCuentaBancaria>> AF_CR_Renuncias_CuentasBancarias_Obtener(int CodEmpresa, [FromBody] AfRenunciaCuentaBancariaFiltro filtro)
        {
            return _bl.AF_CR_Renuncias_CuentasBancarias_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Promotores_Obtener")]
        public ErrorDto<List<AfRenunciaPromotor>> AF_CR_Renuncias_Promotores_Obtener(int CodEmpresa)
        {
            return _bl.AF_CR_Renuncias_Promotores_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Activa")]
        public ErrorDto<int> AF_CR_Renuncias_Activa(int CodEmpresa, string cedula)
        {
            return _bl.AF_CR_Renuncias_Activa(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Activa_Otra")]
        public ErrorDto<int> AF_CR_Renuncias_Activa_Otra(int CodEmpresa, string cedula, int codigo)
        {
            return _bl.AF_CR_Renuncias_Activa_Otra(CodEmpresa, cedula, codigo);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Socio_Existe")]
        public ErrorDto<int> AF_CR_Renuncias_Socio_Existe(int CodEmpresa, string cedula)
        {
            return _bl.AF_CR_Renuncias_Socio_Existe(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_ObtenerPorId")]
        public ErrorDto<AfRenuncia?> AF_CR_Renuncias_ObtenerPorId(int CodEmpresa, long CodRenuncia)
        {
            return _bl.AF_CR_Renuncias_ObtenerPorId(CodEmpresa, CodRenuncia);
        }


        [Authorize]
        [HttpGet("AF_CR_Renuncia_Obtener_Causas")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Renuncia_Obtener_Causas(int CodEmpresa, String Tipo)
        {
            return _bl.AF_CR_Renuncia_Obtener_Causas(CodEmpresa, Tipo);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Renta_Global")]
        public ErrorDto<AfRenunciaRentaGlobal> AF_CR_Renuncias_Renta_Global(int CodEmpresa, [FromBody] AfRenunciaRentaGlobalFiltro filtro)
        {
            return _bl.AF_CR_Renuncias_Renta_Global(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Liquidacion_CreditosPersona")]
        public ErrorDto<List<AfRenunciaLiquidacionCreditosPersona>> AF_CR_Renuncias_Liquidacion_CreditosPersona(int CodEmpresa, [FromBody] AfRenunciaLiquidacionCreditosPersonaFiltro filtro)
        {
            return _bl.AF_CR_Renuncias_Liquidacion_CreditosPersona(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_Sinpe_Negativo")]
        public ErrorDto<AfRenunciaSinpeNegativo> AF_CR_Renuncias_Sinpe_Negativo(int CodEmpresa, String Cedula)
        {
            return _bl.AF_CR_Renuncias_Sinpe_Negativo(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_ObtenerHistorico")]
        public ErrorDto<List<AfRenunciaDetalleHistorico>> AF_CR_Renuncias_ObtenerHistorico(int CodEmpresa, string Cedula)
        {
            return _bl.AF_CR_Renuncias_ObtenerHistorico(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_CR_Renuncias_ObtenerPorCodigo")]
        public ErrorDto<AfRenuncia?> AF_CR_Renuncias_ObtenerPorCodigo(int CodEmpresa, long CodRenuncia)
        {
            return _bl.AF_CR_Renuncias_ObtenerPorCodigo(CodEmpresa, CodRenuncia);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Liquidacion_Guarda")]
        public ErrorDto<int> AF_CR_Renuncias_Liquidacion_Guarda(int CodEmpresa, [FromBody] AfRenunciaLiquidacion request)
        {
            return _bl.AF_CR_Renuncias_Liquidacion_Guarda(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Plan_Insertar")]
        public ErrorDto<bool> AF_CR_Renuncias_Plan_Insertar(int CodEmpresa, [FromBody] AfRenunciaPlan request)
        {
            return _bl.AF_CR_Renuncias_Plan_Insertar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_CR_Renuncias_Abono_Insertar")]
        public ErrorDto<bool> AF_CR_Renuncias_Abono_Insertar(int CodEmpresa, [FromBody] AfRenunciaAbono request)
        {
            return _bl.AF_CR_Renuncias_Abono_Insertar(CodEmpresa, request);
        }
    }
}