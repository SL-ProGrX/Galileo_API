using System.Text.Json;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmSifRecepcionNdNcBl
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly FrmSifRecepcionNdNcDb _db;

        public FrmSifRecepcionNdNcBl(IConfiguration config)
        {
            _db = new FrmSifRecepcionNdNcDb(config);
        }

        public ErrorDto<SifRecepcionNdNcInicializaData>
            SIF_RecepcionNdNc_Inicializar(int codEmpresa)
        {
            return _db.SIF_RecepcionNdNc_Inicializar(codEmpresa);
        }

        public ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            SIF_RecepcionNdNc_Documentos_Obtener(
                int codEmpresa,
                string? request)
        {
            return DeserializarYProcesar<
                SifRecepcionNdNcDocumentosRequest,
                List<SifRecepcionNdNcDocumentoData>>(
                    request,
                    "Los filtros de documentos son requeridos.",
                    "El formato de los filtros de documentos no es v&aacute;lido.",
                    [],
                    filtros =>
                        _db.SIF_RecepcionNdNc_Documentos_Obtener(
                            codEmpresa,
                            filtros));
        }

        public ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            SIF_RecepcionNdNc_Pendientes_Obtener(
                int codEmpresa,
                string? request)
        {
            return DeserializarYProcesar<
                SifRecepcionNdNcPendientesRequest,
                List<SifRecepcionNdNcDocumentoData>>(
                    request,
                    "Los filtros de pendientes son requeridos.",
                    "El formato de los filtros de pendientes no es v&aacute;lido.",
                    [],
                    filtros =>
                        _db.SIF_RecepcionNdNc_Pendientes_Obtener(
                            codEmpresa,
                            filtros));
        }

        public ErrorDto<List<SifRecepcionNdNcConsultaData>>
            SIF_RecepcionNdNc_Consulta_Obtener(
                int codEmpresa,
                string? request)
        {
            return DeserializarYProcesar<
                SifRecepcionNdNcConsultaRequest,
                List<SifRecepcionNdNcConsultaData>>(
                    request,
                    "Los filtros de consulta son requeridos.",
                    "El formato de los filtros de consulta no es v&aacute;lido.",
                    [],
                    filtros =>
                        _db.SIF_RecepcionNdNc_Consulta_Obtener(
                            codEmpresa,
                            filtros));
        }

        public ErrorDto<int>
            SIF_RecepcionNdNc_Movimiento_Aplicar(
                int codEmpresa,
                SifRecepcionNdNcAplicarRequest request)
        {
            return _db.SIF_RecepcionNdNc_Movimiento_Aplicar(
                codEmpresa,
                request);
        }

        private static ErrorDto<TResponse>
            DeserializarYProcesar<TRequest, TResponse>(
                string? request,
                string mensajeRequerido,
                string mensajeInvalido,
                TResponse resultadoVacio,
                Func<TRequest, ErrorDto<TResponse>> procesar)
            where TRequest : class
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return DbHelper.CreateErrorResponse(
                    mensajeRequerido,
                    -2,
                    resultadoVacio);
            }

            try
            {
                TRequest? filtros = JsonSerializer.Deserialize<TRequest>(
                    request,
                    JsonOptions);

                if (filtros is null)
                {
                    return DbHelper.CreateErrorResponse(
                        mensajeInvalido,
                        -2,
                        resultadoVacio);
                }

                return procesar(filtros);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    mensajeInvalido,
                    -2,
                    resultadoVacio);
            }
        }
    }
}