using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using static Galileo.Models.ProGrX.Clientes.FrmAfLiquidacionWModels;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfLiquidacionWController
    {
        private readonly FrmAfLiquidacionwBl _bl;

        public FrmAfLiquidacionWController(IConfiguration config)
        {
            _bl = new FrmAfLiquidacionwBl(config);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Bancos_Obtener")]
        public ErrorDto<List<AfLiquidacionBancos>> AF_Liquidacion_Bancos_Obtener(int CodEmpresa, [FromBody] AfLiquidacionBancosFiltro filtro)
        {
            return _bl.AF_Liquidacion_Bancos_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Emite_TDoc")]
        public ErrorDto<List<AfLiquidacionEmiteTDoc>> AF_Liquidacion_Emite_TDoc(int CodEmpresa, [FromBody] AfLiquidacionEmiteTDocFiltro filtro)
        {
            return _bl.AF_Liquidacion_Emite_TDoc(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_TipoAccion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_TipoAccion_Obtener(int CodEmpresa)
        {
            return _bl.AF_Liquidacion_TipoAccion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_Causas_ObtenerDetalle")]
        public ErrorDto<AfLiquidacionCausasDetalle?> AF_Liquidacion_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            return _bl.AF_Liquidacion_Causas_ObtenerDetalle(CodEmpresa, Causa);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_CuentasBancarias_Obtener")]
        public ErrorDto<List<AfLiquidacionCuentaBancaria>> AF_Liquidacion_CuentasBancarias_Obtener(int CodEmpresa, [FromBody] AfLiquidacionCuentaBancariaFiltro filtro)
        {
            return _bl.AF_Liquidacion_CuentasBancarias_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_Fondos")]
        public ErrorDto<short> AF_Liquidacion_Fondos(int CodEmpresa)
        {
            return _bl.AF_Liquidacion_Fondos(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_ActivarControl")]
        public ErrorDto<bool> AF_Liquidacion_ActivarControl(int CodEmpresa)
        {
            return _bl.AF_Liquidacion_ActivarControl(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_Renuncias_Obtener")]
        public ErrorDto<List<object>> AF_Liquidacion_Renuncias_Obtener(int CodEmpresa, bool activar_control)
        {
            return _bl.AF_Liquidacion_Renuncias_Obtener(CodEmpresa, activar_control);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_SociosRenuncia_Obtener")]
        public ErrorDto<List<AfLiquidacionSocio>> AF_Liquidacion_SociosRenuncia_Obtener(int CodEmpresa, bool activar_control)
        {
            return _bl.AF_Liquidacion_SociosRenuncia_Obtener(CodEmpresa, activar_control);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_ActualizarEstadoRenuncias")]
        public ErrorDto<int> AF_Liquidacion_ActualizarEstadoRenuncias(int CodEmpresa)
        {
            return _bl.AF_Liquidacion_ActualizarEstadoRenuncias(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_SocioDetalle_Obtener")]
        public ErrorDto<AfLiquidacionSocioDetalle?> AF_Liquidacion_SocioDetalle_Obtener(int CodEmpresa, string Cedula)
        {
            return _bl.AF_Liquidacion_SocioDetalle_Obtener(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_CausasRenuncia_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_CausasRenuncia_Obtener(int CodEmpresa, string tipo)
        {
            return _bl.AF_Liquidacion_CausasRenuncia_Obtener(CodEmpresa, tipo);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_Causas_Accion")]
        public ErrorDto<AfLiquidacionCausaAccion?> AF_Liquidacion_Causas_Accion(int CodEmpresa, int IdCausa)
        {
            return _bl.AF_Liquidacion_Causas_Accion(CodEmpresa, IdCausa);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_SocioExiste")]
        public ErrorDto<AfLiquidacionSocioExiste?> AF_Liquidacion_SocioExiste(int CodEmpresa, string Cedula)
        {
            return _bl.AF_Liquidacion_SocioExiste(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_Consulta_Patrimonio")]
        public ErrorDto<List<AfLiquidacionConsultaPatrimonio>> AF_Liquidacion_Consulta_Patrimonio(int CodEmpresa, String Cedula)
        {
            return _bl.AF_Liquidacion_Consulta_Patrimonio(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Renta_Global")]
        public ErrorDto<AfLiquidacionRentaGlobal?> AF_Liquidacion_Renta_Global(int CodEmpresa, [FromBody] AfLiquidacionRentaGlobalFiltro filtro)
        {
            return _bl.AF_Liquidacion_Renta_Global(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_ListaPlanes")]
        public ErrorDto<List<AfLiquidacionListaPlanes>> AF_Liquidacion_ListaPlanes(int CodEmpresa, [FromBody] AfLiquidacionListaPlanesFiltro filtro)
        {
            return _bl.AF_Liquidacion_ListaPlanes(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_CreditosPersona")]
        public ErrorDto<List<AfLiquidacionCreditosPersona>> AF_Liquidacion_CreditosPersona(int CodEmpresa, [FromBody] AfLiquidacionCreditosPersonaFiltro filtro)
        {
            return _bl.AF_Liquidacion_CreditosPersona(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_CodRenuncia_Obtener")]
        public ErrorDto<AfLiquidacionCodRenuncia?> AF_Liquidacion_CodRenuncia_Obtener(int CodEmpresa, string Cedula)
        {
            return _bl.AF_Liquidacion_CodRenuncia_Obtener(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_RenunciaSIF_Insertar")]
        public ErrorDto<int> AF_Liquidacion_RenunciaSIF_Insertar(int CodEmpresa, [FromBody] AfRenunciaSifModel model)
        {
            return _bl.AF_Liquidacion_RenunciaSIF_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_RenunciaASE_Insertar")]
        public ErrorDto<int> AF_Liquidacion_RenunciaASE_Insertar(int CodEmpresa, [FromBody] AfRenunciaAseModel model)
        {
            return _bl.AF_Liquidacion_RenunciaASE_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpGet("AF_Liquidacion_SocioDatosBasicos_Obtener")]
        public ErrorDto<AfSocioDatosBasicos?> AF_Liquidacion_SocioDatosBasicos_Obtener(int CodEmpresa, string Cedula)
        {
            return _bl.AF_Liquidacion_SocioDatosBasicos_Obtener(CodEmpresa, Cedula);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Insertar")]
        public ErrorDto<int> AF_Liquidacion_Insertar(int CodEmpresa, [FromBody] AfLiquidacionInsertModel model)
        {
            return _bl.AF_Liquidacion_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Patrimonio_Aplicar")]
        public ErrorDto<bool> AF_Liquidacion_Patrimonio_Aplicar(int CodEmpresa, [FromBody] AfLiquidacionPatrimonioInput input)
        {
            return _bl.AF_Liquidacion_Patrimonio_Aplicar(CodEmpresa, input);
        }

        [Authorize]
        [HttpPost("AF_LiquidaFondos_Insertar")]
        public ErrorDto<int> AF_LiquidaFondos_Insertar(int CodEmpresa, [FromBody] AfLiquidaFondosInsertModel model)
        {
            return _bl.AF_LiquidaFondos_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_LiquidaPlanes_Ejecutar")]
        public ErrorDto<bool> AF_LiquidaPlanes_Ejecutar(int CodEmpresa, [FromBody] AfLiquidaPlanesInput input)
        {
            return _bl.AF_LiquidaPlanes_Ejecutar(CodEmpresa, input);
        }

        [Authorize]
        [HttpPost("AF_LiquidaDetalle_Insertar")]
        public ErrorDto<int> AF_LiquidaDetalle_Insertar(int CodEmpresa, [FromBody] AfLiquidaDetalleInsertModel model)
        {
            return _bl.AF_LiquidaDetalle_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_AbonosPlanPagos_Ejecutar")]
        public ErrorDto<bool> AF_Liquidacion_AbonosPlanPagos_Ejecutar(int CodEmpresa, [FromBody] AfLiquidacionPatrimonioInput input)
        {
            return _bl.AF_Liquidacion_AbonosPlanPagos_Ejecutar(CodEmpresa, input);
        }

        [Authorize]
        [HttpPost("AF_Morosidad_Actualizar")]
        public ErrorDto<int> AF_Morosidad_Actualizar(int CodEmpresa, [FromBody] AfMorosidadModel model)
        {
            return _bl.AF_Morosidad_Actualizar(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_Morosidad_Actualizar_Mora")]
        public ErrorDto<int> AF_Morosidad_Actualizar_Mora(int CodEmpresa, [FromBody] AfMorosidadPorMoraModel model)
        {
            return _bl.AF_Morosidad_Actualizar_Mora(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_Morosidad_Insertar")]
        public ErrorDto<int> AF_Morosidad_Insertar(int CodEmpresa, [FromBody] AfMorosidadInsertModel model)
        {
            return _bl.AF_Morosidad_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpGet("AF_Morosidad_ConsultarPorSolicitud")]
        public ErrorDto<List<AfMorosidadConsultaModel>> AF_Morosidad_ConsultarPorSolicitud(int CodEmpresa, int IdSolicitud)
        {
            return _bl.AF_Morosidad_ConsultarPorSolicitud(CodEmpresa, IdSolicitud);
        }

        [Authorize]
        [HttpPost("AF_RegCreditos_Actualizar_Cartera")]
        public ErrorDto<int> AF_RegCreditos_Actualizar_Cartera(int CodEmpresa, [FromBody] AfRegCreditosActualizarModel model)
        {
            return _bl.AF_RegCreditos_Actualizar_Cartera(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_RegCreditos_Actualizar_RetenPlazo")]
        public ErrorDto<int> AF_RegCreditos_Actualizar_RetenPlazo(int CodEmpresa, [FromBody] AfRegCreditosActualizarModel model)
        {
            return _bl.AF_RegCreditos_Actualizar_RetenPlazo(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_RegCreditos_Actualizar_RetenIndefinida")]
        public ErrorDto<int> AF_RegCreditos_Actualizar_RetenIndefinida(int CodEmpresa, [FromBody] AfRegCreditosActualizarModel model)
        {
            return _bl.AF_RegCreditos_Actualizar_RetenIndefinida(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_CreditosDt_Insertar")]
        public ErrorDto<int> AF_CreditosDt_Insertar(int CodEmpresa, [FromBody] AfCreditosDtInsertModel model)
        {
            return _bl.AF_CreditosDt_Insertar(CodEmpresa, model);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Asiento_Ejecutar")]
        public ErrorDto<bool> AF_Liquidacion_Asiento_Ejecutar(int CodEmpresa, int LiqConsec)
        {
            return _bl.AF_Liquidacion_Asiento_Ejecutar(CodEmpresa, LiqConsec);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Traslado_OpEx_Ejecutar")]
        public ErrorDto<bool> AF_Liquidacion_Traslado_OpEx_Ejecutar(int CodEmpresa, int LiqConsec)
        {
            return _bl.AF_Liquidacion_Traslado_OpEx_Ejecutar(CodEmpresa, LiqConsec);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Fondos_Devolucion_Ejecutar")]
        public ErrorDto<bool> AF_Liquidacion_Fondos_Devolucion_Ejecutar(int CodEmpresa, int LiqConsec, string Usuario)
        {
            return _bl.AF_Liquidacion_Fondos_Devolucion_Ejecutar(CodEmpresa, LiqConsec, Usuario);
        }

        [Authorize]
        [HttpPost("AF_Liquidacion_Bitacora_Insertar")]
        public ErrorDto AF_Liquidacion_Bitacora_Insertar(int CodEmpresa, string usuario, string detalle, string movimiento, int modulo = 7)
        {
            return _bl.AF_Liquidacion_Bitacora_Insertar(CodEmpresa, usuario, detalle, movimiento, modulo);
        }
    }
}