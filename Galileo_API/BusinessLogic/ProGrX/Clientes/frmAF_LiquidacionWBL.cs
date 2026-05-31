using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using static Galileo.Models.ProGrX.Clientes.FrmAfLiquidacionWModels;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfLiquidacionwBl
    {
        private readonly FrmAfLiquidacionwDb _db;

        public FrmAfLiquidacionwBl(IConfiguration config)
        {
            _db = new FrmAfLiquidacionwDb(config);
        }

        public ErrorDto<List<AfLiquidacionBancos>> AF_Liquidacion_Bancos_Obtener(int CodEmpresa, AfLiquidacionBancosFiltro filtro)
        {
            return _db.AF_Liquidacion_Bancos_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfLiquidacionEmiteTDoc>> AF_Liquidacion_Emite_TDoc(int CodEmpresa, AfLiquidacionEmiteTDocFiltro filtro)
        {
            return _db.AF_Liquidacion_Emite_TDoc(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_TipoAccion_Obtener(int CodEmpresa)
        {
            return _db.AF_Liquidacion_TipoAccion_Obtener(CodEmpresa);
        }

        public ErrorDto<AfLiquidacionCausasDetalle?> AF_Liquidacion_Causas_ObtenerDetalle(int CodEmpresa, int Causa)
        {
            return _db.AF_Liquidacion_Causas_ObtenerDetalle(CodEmpresa, Causa);
        }

        public ErrorDto<List<AfLiquidacionCuentaBancaria>> AF_Liquidacion_CuentasBancarias_Obtener(int CodEmpresa, AfLiquidacionCuentaBancariaFiltro filtro)
        {
            return _db.AF_Liquidacion_CuentasBancarias_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<short> AF_Liquidacion_Fondos(int CodEmpresa)
        {
            return _db.AF_Liquidacion_Fondos(CodEmpresa);
        }

        public ErrorDto<bool> AF_Liquidacion_ActivarControl(int CodEmpresa)
        {
            return _db.AF_Liquidacion_ActivarControl(CodEmpresa);
        }

        public ErrorDto<List<object>> AF_Liquidacion_Renuncias_Obtener(int CodEmpresa, bool activar_control)
        {
            return _db.AF_Liquidacion_Renuncias_Obtener(CodEmpresa, activar_control);
        }

        public ErrorDto<List<AfLiquidacionSocio>> AF_Liquidacion_SociosRenuncia_Obtener(int CodEmpresa, bool activar_control)
        {
            return _db.AF_Liquidacion_SociosRenuncia_Obtener(CodEmpresa, activar_control);
        }

        public ErrorDto<int> AF_Liquidacion_ActualizarEstadoRenuncias(int CodEmpresa)
        {
            return _db.AF_Liquidacion_ActualizarEstadoRenuncias(CodEmpresa);
        }

        public ErrorDto<AfLiquidacionSocioDetalle?> AF_Liquidacion_SocioDetalle_Obtener(int CodEmpresa, string Cedula)
        {
            return _db.AF_Liquidacion_SocioDetalle_Obtener(CodEmpresa, Cedula);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Liquidacion_CausasRenuncia_Obtener(int CodEmpresa, string tipo)
        {
            return _db.AF_Liquidacion_CausasRenuncia_Obtener(CodEmpresa, tipo);
        }

        public ErrorDto<AfLiquidacionCausaAccion?> AF_Liquidacion_Causas_Accion(int CodEmpresa, int IdCausa)
        {
            return _db.AF_Liquidacion_Causas_Accion(CodEmpresa, IdCausa);
        }

        public ErrorDto<AfLiquidacionSocioExiste?> AF_Liquidacion_SocioExiste(int CodEmpresa, string Cedula)
        {
            return _db.AF_Liquidacion_SocioExiste(CodEmpresa, Cedula);
        }

        public ErrorDto<List<AfLiquidacionConsultaPatrimonio>> AF_Liquidacion_Consulta_Patrimonio(int CodEmpresa, String Cedula)
        {
            return _db.AF_Liquidacion_Consulta_Patrimonio(CodEmpresa, Cedula);
        }

        public ErrorDto<AfLiquidacionRentaGlobal?> AF_Liquidacion_Renta_Global(int CodEmpresa, AfLiquidacionRentaGlobalFiltro filtro)
        {
            return _db.AF_Liquidacion_Renta_Global(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfLiquidacionListaPlanes>> AF_Liquidacion_ListaPlanes(int CodEmpresa, AfLiquidacionListaPlanesFiltro filtro)
        {
            return _db.AF_Liquidacion_ListaPlanes(CodEmpresa, filtro);
        }

        public ErrorDto<List<AfLiquidacionCreditosPersona>> AF_Liquidacion_CreditosPersona(int CodEmpresa, AfLiquidacionCreditosPersonaFiltro filtro)
        {
            return _db.AF_Liquidacion_CreditosPersona(CodEmpresa, filtro);
        }

        public ErrorDto<AfLiquidacionCodRenuncia?> AF_Liquidacion_CodRenuncia_Obtener(int CodEmpresa, string Cedula)
        {
            return _db.AF_Liquidacion_CodRenuncia_Obtener(CodEmpresa, Cedula);
        }

        public ErrorDto<int> AF_Liquidacion_RenunciaSIF_Insertar(int CodEmpresa, AfRenunciaSifModel model)
        {
            return _db.AF_Liquidacion_RenunciaSIF_Insertar(CodEmpresa, model);
        }

        public ErrorDto<int> AF_Liquidacion_RenunciaASE_Insertar(int CodEmpresa, AfRenunciaAseModel model)
        {
            return _db.AF_Liquidacion_RenunciaASE_Insertar(CodEmpresa, model);
        }

        public ErrorDto<AfSocioDatosBasicos?> AF_Liquidacion_SocioDatosBasicos_Obtener(int CodEmpresa, string Cedula)
        {
            return _db.AF_Liquidacion_SocioDatosBasicos_Obtener(CodEmpresa, Cedula);
        }

        public ErrorDto<int> AF_Liquidacion_Insertar(int CodEmpresa, AfLiquidacionInsertModel model)
        {
            return _db.AF_Liquidacion_Insertar(CodEmpresa, model);
        }

        public ErrorDto<bool> AF_Liquidacion_Patrimonio_Aplicar(int CodEmpresa, AfLiquidacionPatrimonioInput input)
        {
            return _db.AF_Liquidacion_Patrimonio_Aplicar(CodEmpresa, input);
        }

        public ErrorDto<int> AF_LiquidaFondos_Insertar(int CodEmpresa, AfLiquidaFondosInsertModel model)
        {
            return _db.AF_LiquidaFondos_Insertar(CodEmpresa, model);
        }

        public ErrorDto<bool> AF_LiquidaPlanes_Ejecutar(int CodEmpresa, AfLiquidaPlanesInput input)
        {
            return _db.AF_LiquidaPlanes_Ejecutar(CodEmpresa, input);
        }

        public ErrorDto<int> AF_LiquidaDetalle_Insertar(int CodEmpresa, AfLiquidaDetalleInsertModel model)
        {
            return _db.AF_LiquidaDetalle_Insertar(CodEmpresa, model);
        }

        public ErrorDto<bool> AF_Liquidacion_AbonosPlanPagos_Ejecutar(int CodEmpresa, AfLiquidacionPatrimonioInput input)
        {
            return _db.AF_Liquidacion_AbonosPlanPagos_Ejecutar(CodEmpresa, input);
        }

        public ErrorDto<int> AF_Morosidad_Actualizar(int CodEmpresa, AfMorosidadModel model)
        {
            return _db.AF_Morosidad_Actualizar(CodEmpresa, model);
        }

        public ErrorDto<int> AF_Morosidad_Actualizar_Mora(int CodEmpresa, AfMorosidadPorMoraModel model)
        {
            return _db.AF_Morosidad_Actualizar_Mora(CodEmpresa, model);
        }

        public ErrorDto<int> AF_Morosidad_Insertar(int CodEmpresa, AfMorosidadInsertModel model)
        {
            return _db.AF_Morosidad_Insertar(CodEmpresa, model);
        }

        public ErrorDto<List<AfMorosidadConsultaModel>> AF_Morosidad_ConsultarPorSolicitud(int CodEmpresa, int IdSolicitud)
        {
            return _db.AF_Morosidad_ConsultarPorSolicitud(CodEmpresa, IdSolicitud);
        }

        public ErrorDto<int> AF_RegCreditos_Actualizar_Cartera(int CodEmpresa, AfRegCreditosActualizarModel model)
        {
            return _db.AF_RegCreditos_Actualizar_Cartera(CodEmpresa, model);
        }

        public ErrorDto<int> AF_RegCreditos_Actualizar_RetenPlazo(int CodEmpresa, AfRegCreditosActualizarModel model)
        {
            return _db.AF_RegCreditos_Actualizar_RetenPlazo(CodEmpresa, model);
        }

        public ErrorDto<int> AF_RegCreditos_Actualizar_RetenIndefinida(int CodEmpresa, AfRegCreditosActualizarModel model)
        {
            return _db.AF_RegCreditos_Actualizar_RetenIndefinida(CodEmpresa, model);
        }

        public ErrorDto<int> AF_CreditosDt_Insertar(int CodEmpresa, AfCreditosDtInsertModel model)
        {
            return _db.AF_CreditosDt_Insertar(CodEmpresa, model);
        }

        public ErrorDto<bool> AF_Liquidacion_Asiento_Ejecutar(int CodEmpresa, int LiqConsec)
        {
            return _db.AF_Liquidacion_Asiento_Ejecutar(CodEmpresa, LiqConsec);
        }

        public ErrorDto<bool> AF_Liquidacion_Traslado_OpEx_Ejecutar(int CodEmpresa, int LiqConsec)
        {
            return _db.AF_Liquidacion_Traslado_OpEx_Ejecutar(CodEmpresa, LiqConsec);
        }

        public ErrorDto<bool> AF_Liquidacion_Fondos_Devolucion_Ejecutar(int CodEmpresa, int LiqConsec, string Usuario)
        {
            return _db.AF_Liquidacion_Fondos_Devolucion_Ejecutar(CodEmpresa, LiqConsec, Usuario);
        }

        public ErrorDto AF_Liquidacion_Bitacora_Insertar(int CodEmpresa, string usuario, string detalle, string movimiento, int modulo = 7)
        {
            return _db.AF_Liquidacion_Bitacora_Insertar(CodEmpresa, usuario, detalle, movimiento, modulo);
        }
    }
}