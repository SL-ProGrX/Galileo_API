using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysFacturaElectronicaController : ControllerBase
    {
        private readonly FrmSysFacturaElectronicaBL BL;

        public FrmSysFacturaElectronicaController(IConfiguration config)
        {
            BL = new FrmSysFacturaElectronicaBL(config);
        }

        [Authorize]
        [HttpGet("FE_Clientes_DropDown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Clientes_DropDown_Obtener(int CodEmpresa)
        {
            return BL.FE_Clientes_DropDown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("FE_Cabys_DropDown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Cabys_DropDown_Obtener(int CodEmpresa)
        {
            return BL.FE_Cabys_DropDown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("FE_Cortes_Lista_Obtener")]
        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.FE_Cortes_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("FE_Cortes_Lista_Export")]
        public ErrorDto<FeCortesLista> FE_Cortes_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.FE_Cortes_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("FE_Corte_Registrar")]
        public ErrorDto FE_Corte_Registrar(int CodEmpresa, FeRegistrarCorteDto dto)
        {
            return BL.FE_Corte_Registrar(CodEmpresa, dto);
        }

        [Authorize]
        [HttpGet("FE_Facturas_Lista_Obtener")]
        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.FE_Facturas_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("FE_Facturas_Lista_Export")]
        public ErrorDto<FeFacturasLista> FE_Facturas_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.FE_Facturas_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("FE_Factura_Detalle_Obtener")]
        public ErrorDto<List<FeFacturaDetalleItem>> FE_Factura_Detalle_Obtener(int CodEmpresa, string codCliente, int idFactura)
        {
            return BL.FE_Factura_Detalle_Obtener(CodEmpresa, codCliente, idFactura);
        }

        [Authorize]
        [HttpGet("FE_Facturas_Resumen_Obtener")]
        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Obtener(int CodEmpresa, string parametros)
        {
            return BL.FE_Facturas_Resumen_Obtener(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("FE_Facturas_Resumen_Export")]
        public ErrorDto<FeFacturasResumen> FE_Facturas_Resumen_Export(int CodEmpresa, string parametros)
        {
            return BL.FE_Facturas_Resumen_Export(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("FE_Facturas_Estados_DropDown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Facturas_Estados_DropDown_Obtener(int CodEmpresa)
        {
            return BL.FE_Facturas_Estados_DropDown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("FE_Personas_DropDown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FE_Personas_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return BL.FE_Personas_DropDown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("FE_Clientes_Lista_Obtener")]
        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.FE_Clientes_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("FE_Clientes_Lista_Export")]
        public ErrorDto<FeClientesLista> FE_Clientes_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.FE_Clientes_Lista_Export(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("FE_Configuracion_Obtener")]
        public ErrorDto<FeConfiguracionModel> FE_Configuracion_Obtener(int CodEmpresa, string codigo)
        {
            return BL.FE_Configuracion_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("FE_Configuracion_Guardar")]
        public ErrorDto FE_Configuracion_Guardar(int CodEmpresa, FeConfiguracionGuardarDto data)
        {
            return BL.FE_Configuracion_Guardar(CodEmpresa, data);
        }

        [Authorize]
        [HttpPost("FE_Configuracion_Eliminar")]
        public ErrorDto FE_Configuracion_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            return BL.FE_Configuracion_Eliminar(CodEmpresa, codigo, usuario);
        }
        [HttpPost]
        [HttpPost("FE_Clientes_Sincronizar")]
        public ErrorDto FE_Clientes_Sincronizar(int CodEmpresa, string cod_cliente, string usuario)
        {
            return BL.FE_Clientes_Sincronizar(CodEmpresa, cod_cliente, usuario);
        }
        [Authorize]
        [HttpGet("FE_Exclusiones_Consulta")]
        public ErrorDto<List<FeExclusionItem>> FE_Exclusiones_Consulta(int CodEmpresa, string cod_cliente, string tipo)
        {
            return BL.FE_Exclusiones_Consulta(CodEmpresa, cod_cliente, tipo);
        }

        [Authorize]
        [HttpPost("FE_Exclusion_Procesar")]
        public ErrorDto FE_Exclusion_Procesar(int CodEmpresa, string cod_cliente, string codigo, string movimiento, string tipo, string usuario)
        {
            return BL.FE_Exclusion_Procesar(CodEmpresa, cod_cliente, codigo, movimiento, tipo, usuario);
        }

        [Authorize]
        [HttpPost("FE_Reactivacion_Ejecutar")]
        public ErrorDto FE_Reactivacion_Ejecutar(int CodEmpresa, DateTime fecha_inicio, DateTime fecha_corte, string usuario)
        {
            return BL.FE_Reactivacion_Ejecutar(CodEmpresa, fecha_inicio, fecha_corte, usuario);
        }
    }
}
