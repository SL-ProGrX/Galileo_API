using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrEnCobroCuotasBL
    {
        private readonly FrmCrEnCobroCuotasDB _db;

        public FrmCrEnCobroCuotasBL(IConfiguration config)
        {
            _db = new FrmCrEnCobroCuotasDB(config);
        }

        public ErrorDto<CrEnCobroCuotasInicialDto> CR_EnCobroCuotas_Inicial_Obtener(int CodEmpresa, string cedula)
        {
            return _db.CR_EnCobroCuotas_Inicial_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_EnCobroCuotas_Deductoras_Dropdown_Obtener(int CodEmpresa, int codInstitucion)
        {
            return _db.CR_EnCobroCuotas_Deductoras_Dropdown_Obtener(CodEmpresa, codInstitucion);
        }

        public ErrorDto<CrEnCobroCuotasInicialDto> CR_EnCobroCuotas_Deductora_Info_Obtener(int CodEmpresa, int codInstitucion)
        {
            return _db.CR_EnCobroCuotas_Deductora_Info_Obtener(CodEmpresa, codInstitucion);
        }

        public ErrorDto<CrEnCobroCuotasProcesoScrollDto> CR_EnCobroCuotas_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            return _db.CR_EnCobroCuotas_Proceso_Scroll_Obtener(CodEmpresa, scrollCode, procesoActual);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Resumen_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Resumen_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> CR_EnCobroCuotas_Detalle_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Detalle_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasDetalleData>> CR_EnCobroCuotas_Detalle_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Detalle_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> CR_EnCobroCuotas_Envio_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Envio_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasEnvioData>> CR_EnCobroCuotas_Envio_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Envio_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasRecepcionResult> CR_EnCobroCuotas_Recepcion_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Recepcion_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasRecepcionResult> CR_EnCobroCuotas_Recepcion_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Recepcion_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>> CR_EnCobroCuotas_Historial_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Historial_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasHistorialData>> CR_EnCobroCuotas_Historial_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Historial_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>> CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_ResumenDeductoras_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenDeductoraData>> CR_EnCobroCuotas_ResumenDeductoras_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_ResumenDeductoras_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>> CR_EnCobroCuotas_Bitacora_Lista_Obtener(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Bitacora_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasBitacoraData>> CR_EnCobroCuotas_Bitacora_Lista_Export(int CodEmpresa, CrEnCobroCuotasConsultaRequest request)
        {
            return _db.CR_EnCobroCuotas_Bitacora_Lista_Export(CodEmpresa, request);
        }
    }
}