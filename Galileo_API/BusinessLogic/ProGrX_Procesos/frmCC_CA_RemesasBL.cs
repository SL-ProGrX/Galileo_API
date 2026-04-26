using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;

namespace Galileo_API.BusinessLogic.ProGrX_Procesos
{
    public class FrmCcCaRemesasBL
    {
        private readonly FrmCcCaRemesasDB _db;

        public FrmCcCaRemesasBL(IConfiguration config)
        {
            _db = new FrmCcCaRemesasDB(config);
        }

        public ErrorDto<CcCaRemesasCatalogosResponse> CcCaRemesas_Catalogos_Obtener(int codEmpresa)
        {
            return _db.CcCaRemesas_Catalogos_Obtener(codEmpresa);
        }

        public ErrorDto<List<CcCaRemesasEnvioConsultaData>> CcCaRemesas_Envio_Consulta(
            int codEmpresa,
            CcCaRemesasEnvioConsultaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>(CcCaRemesaCOnstantes.vRequestRequerido);

            if (request.cod_remesa <= 0)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("El tipo de remesa es requerido.");

            if (string.IsNullOrWhiteSpace(request.cod_entidad))
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("La entidad es requerida.");

            if (request.cuotas <= 0)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasEnvioConsultaData>>("La cantidad de cuotas debe ser mayor a cero.");

            return _db.CcCaRemesas_Envio_Consulta(codEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CcCaRemesas_Recibe_Pendientes_Obtener(int codEmpresa)
        {
            return _db.CcCaRemesas_Recibe_Pendientes_Obtener(codEmpresa);
        }

        public ErrorDto<List<CcCaRemesasRecibeDetalleData>> CcCaRemesas_Recibe_Detalle_Obtener(int codEmpresa, long remesa)
        {
            if (remesa <= 0)
                return DbHelper.CreateErrorResponse<List<CcCaRemesasRecibeDetalleData>>("La remesa es requerida.");

            return _db.CcCaRemesas_Recibe_Detalle_Obtener(codEmpresa, remesa);
        }

        public ErrorDto<CcCaRemesasEnvioPendienteData?> CcCaRemesas_Envio_Pendiente_Validar(int codEmpresa)
        {
            return _db.CcCaRemesas_Envio_Pendiente_Validar(codEmpresa);
        }

        public ErrorDto<long> CcCaRemesas_Envio_NumeroGeneracion_Obtener(int codEmpresa)
        {
            return _db.CcCaRemesas_Envio_NumeroGeneracion_Obtener(codEmpresa);
        }

        public ErrorDto CcCaRemesas_Envio_Registrar(
            int codEmpresa,
            string usuario,
            CcCaRemesasEnvioRegistrarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse(CcCaRemesaCOnstantes.vRequestRequerido, -2);

            if (request.cod_remesa <= 0)
                return DbHelper.ErrorResponse("El tipo de remesa es requerido.", -2);

            if (string.IsNullOrWhiteSpace(request.cod_entidad))
                return DbHelper.ErrorResponse("La entidad es requerida.", -2);

            if (string.IsNullOrWhiteSpace(request.proceso))
                return DbHelper.ErrorResponse("El proceso es requerido.", -2);

            if (request.numero_generacion <= 0)
                return DbHelper.ErrorResponse("El número de generación es inválido.", -2);

            if (request.seleccionados == null || request.seleccionados.Count == 0)
                return DbHelper.ErrorResponse("Debe seleccionar al menos un caso.", -2);

            return _db.CcCaRemesas_Envio_Registrar(codEmpresa, usuario, request);
        }

        public ErrorDto<ArchivoDto> CcCaRemesas_Envio_ArchivoBanco_Obtener(int codEmpresa, long numeroGeneracion)
        {
            if (numeroGeneracion <= 0)
                return DbHelper.CreateErrorResponse<ArchivoDto>(
                    "El número de generación es inválido.");

            return _db.CcCaRemesas_Envio_ArchivoBanco_Obtener(codEmpresa, numeroGeneracion);
        }

        public ErrorDto CcCaRemesas_Recibe_Autorizaciones_Cargar(
                int codEmpresa,
                CcCaRemesasRecibeAutorizacionesRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse(CcCaRemesaCOnstantes.vRequestRequerido, -2);

            if (request.numero_generacion <= 0)
                return DbHelper.ErrorResponse("El número de generación es requerido.", -2);

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.ErrorResponse("El usuario es requerido.", -2);

            if (request.autorizaciones == null || request.autorizaciones.Count == 0)
                return DbHelper.ErrorResponse("Debe indicar autorizaciones para procesar.", -2);

            return _db.CcCaRemesas_Recibe_Autorizaciones_Cargar(codEmpresa, request);
        }

        public ErrorDto CcCaRemesas_Recibe_Cierra(
            int codEmpresa,
            long numeroGeneracion,
            string usuario)
        {
            if (numeroGeneracion <= 0)
                return DbHelper.ErrorResponse("El número de generación es requerido.", -2);

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.ErrorResponse("El usuario es requerido.", -2);

            return _db.CcCaRemesas_Recibe_Cierra(codEmpresa, numeroGeneracion, usuario);
        }


        public ErrorDto<CcCaRemesasRecibeAplicaResponse> CcCaRemesas_Recibe_Aplica(
            int codEmpresa,
            CcCaRemesasRecibeAplicaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(
                   CcCaRemesaCOnstantes.vRequestRequerido);

            if (request.numero_generacion <= 0)
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(
                    "El número de generación es requerido.");

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(
                    "El usuario es requerido.");

            if (string.IsNullOrWhiteSpace(request.tipo_documento))
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(
                    "El tipo de documento es requerido.");

            if (string.IsNullOrWhiteSpace(request.numero_documento))
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(
                    "El número de documento es requerido.");

            if (request.lote <= 0)
                return DbHelper.CreateErrorResponse<CcCaRemesasRecibeAplicaResponse>(
                    "El tamaño del lote debe ser mayor a cero.");

            return _db.CcCaRemesas_Recibe_Aplica(codEmpresa, request);
        }
    }
}
