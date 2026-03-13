using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmPolizaReclamoBL
    {
        private readonly FrmPolizaReclamoDB _db;
    
        public FrmPolizaReclamoBL(IConfiguration config)
        {
           _db = new FrmPolizaReclamoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Motivos_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _db.Poliza_Reclamo_Motivos_Lista(codEmpresa, codPoliza);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Causas_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return _db.Poliza_Reclamo_Causas_Lista(codEmpresa, codPoliza);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Estados_Lista(int codEmpresa)
        {
            return _db.Poliza_Reclamo_Estados_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Bancos_Lista(
            int codEmpresa,
            string usuario)
        {
            return _db.Poliza_Reclamo_Bancos_Lista(codEmpresa, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Cuentas_Lista(
            int codEmpresa,
            string cedula,
            int bancoId)
        {
            return _db.Poliza_Reclamo_Cuentas_Lista(codEmpresa, cedula, bancoId);
        }

        public ErrorDto<PolizaReclamoFormularioResponse> Poliza_Reclamo_Load(
            int codEmpresa,
            int reclamoId)
        {
            return _db.Poliza_Reclamo_Load(codEmpresa, reclamoId);
        }

        public ErrorDto<PolizaReclamoFormularioResponse> Poliza_Reclamo_Nuevo(
            int codEmpresa,
            PolizaReclamoRequestNuevo request)
        {
            return _db.Poliza_Reclamo_Nuevo(codEmpresa, request);
        }

        public ErrorDto<List<PolizaReclamoSeguimientoItemResponse>> Poliza_Reclamo_Seguimiento_Lista(
            int codEmpresa,
            int reclamoId)
        {
            return _db.Poliza_Reclamo_Seguimiento_Lista(codEmpresa, reclamoId);
        }

        public ErrorDto<List<PolizaReclamoFondoItemResponse>> Poliza_Reclamo_Fondo_Movimientos(
            int codEmpresa,
            string plan,
            int contrato)
        {
            return _db.Poliza_Reclamo_Fondo_Movimientos(codEmpresa, plan, contrato);
        }

        public ErrorDto<List<PolizaReclamoDesembolsoItemResponse>> Poliza_Reclamo_Desembolsos_Consulta(
            int codEmpresa,
            int reclamoId,
            string plan,
            int contrato)
        {
            return _db.Poliza_Reclamo_Desembolsos_Consulta(codEmpresa, reclamoId, plan, contrato);
        }

        public ErrorDto<List<PolizaReclamoEtiquetaItemResponse>> Poliza_Reclamo_Etiquetas_Lista(
            int codEmpresa,
            int reclamoId)
        {
            return _db.Poliza_Reclamo_Etiquetas_Lista(codEmpresa, reclamoId);
        }

        public ErrorDto Poliza_Reclamo_Actualiza_Datos_Vida(int codEmpresa, PolizaReclamoActualizarVidaRequest request)
        {
            return _db.Poliza_Reclamo_Actualiza_Datos_Vida(codEmpresa, request);
        }

        public ErrorDto Poliza_Reclamo_Actualiza_Datos_Incendio(int codEmpresa, PolizaReclamoActualizarIncendioRequest request)
        {
            return _db.Poliza_Reclamo_Actualiza_Datos_Incendio(codEmpresa, request);
        }

        public ErrorDto Poliza_Reclamo_Actualiza_Recepcion(
            int codEmpresa,
            PolizaReclamoActualizarRecepcionRequest request)
        {
            return _db.Poliza_Reclamo_Actualiza_Recepcion(codEmpresa, request);
        }

        public ErrorDto Poliza_Reclamo_Seguimiento_Manual_Add(
            int codEmpresa,
            PolizaReclamoSeguimientoManualAddRequest request)
        {
            return _db.Poliza_Reclamo_Seguimiento_Manual_Add(codEmpresa, request);
        }

        public ErrorDto<PolizaReclamoFondoCrearResponse> Poliza_Reclamo_Fondo_Creacion(
            int codEmpresa,
            PolizaReclamoFondoCrearRequest request)
        {
            return _db.Poliza_Reclamo_Fondo_Creacion(codEmpresa, request);
        }

        public ErrorDto<PolizaReclamoFondoAportacionResponse> Poliza_Reclamo_Fondo_Aportacion(
            int codEmpresa,
            PolizaReclamoFondoAportacionRequest request)
        {
            return _db.Poliza_Reclamo_Fondo_Aportacion(codEmpresa, request);
        }

        public ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_Aplica(
            int codEmpresa,
            PolizaReclamoDesembolsoAplicaRequest request)
        {
            return _db.Poliza_Reclamo_Desembolsos_Aplica(codEmpresa, request);
        }

        public ErrorDto Poliza_Reclamo_Etiqueta_Manual_Add(
            int codEmpresa,
            PolizaReclamoEtiquetaManualAddRequest request)
        {
            return _db.Poliza_Reclamo_Etiqueta_Manual_Add(codEmpresa, request);
        }

        public ErrorDto<PolizaReclamoAddResponse> Poliza_Reclamo_Add(
            int codEmpresa,
            PolizaReclamoAddRequest request)
        {
            return _db.Poliza_Reclamo_Add(codEmpresa, request);
        }

        public ErrorDto<PolizaReclamoFondoDisponibleResponse> Poliza_Reclamo_Fondo_Disponible(
                int codEmpresa,
                int reclamoId,
                string plan,
                int contrato)
        {
            return _db.Poliza_Reclamo_Fondo_Disponible(codEmpresa, reclamoId, plan, contrato);
        }
    }
}
