using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public class FrmCcEstadoCuentaBl
    {
        private const int CodigoValidacion = -2;

        private const string SolicitudInvalida =
            "La solicitud no tiene un formato v&aacute;lido.";

        private readonly FrmCcEstadoCuentaDb _db;

        public FrmCcEstadoCuentaBl(
            IConfiguration config)
        {
            _db = new FrmCcEstadoCuentaDb(config);
        }

        public ErrorDto<CcEstadoCuentaInicialData>
            CC_FrmCCEstadoCuenta_Inicial_Obtener(
                int codEmpresa)
        {
            return _db
                .CC_FrmCCEstadoCuenta_Inicial_Obtener(
                    codEmpresa);
        }

        public ErrorDto<CcEstadoCuentaPersonaData>
            CC_FrmCCEstadoCuenta_Persona_Obtener(
                int codEmpresa,
                string request)
        {
            var solicitud =
                Deserializar<CcEstadoCuentaPersonaRequest>(
                    request);

            if (solicitud is null)
            {
                return DbHelper.CreateErrorResponse(
                    SolicitudInvalida,
                    CodigoValidacion,
                    new CcEstadoCuentaPersonaData());
            }

            return _db
                .CC_FrmCCEstadoCuenta_Persona_Obtener(
                    codEmpresa,
                    solicitud);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_FrmCCEstadoCuenta_Departamentos_Obtener(
                int codEmpresa,
                string request)
        {
            var solicitud =
                Deserializar
                    <CcEstadoCuentaDepartamentoRequest>(
                        request);

            if (solicitud is null)
            {
                return DbHelper.CreateErrorResponse(
                    SolicitudInvalida,
                    CodigoValidacion,
                    new List<DropDownListaGenericaModel>());
            }

            return _db
                .CC_FrmCCEstadoCuenta_Departamentos_Obtener(
                    codEmpresa,
                    solicitud.cod_institucion);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_FrmCCEstadoCuenta_Secciones_Obtener(
                int codEmpresa,
                string request)
        {
            var solicitud =
                Deserializar<CcEstadoCuentaSeccionRequest>(
                    request);

            if (solicitud is null)
            {
                return DbHelper.CreateErrorResponse(
                    SolicitudInvalida,
                    CodigoValidacion,
                    new List<DropDownListaGenericaModel>());
            }

            return _db
                .CC_FrmCCEstadoCuenta_Secciones_Obtener(
                    codEmpresa,
                    solicitud);
        }

        public ErrorDto
            CC_FrmCCEstadoCuenta_Email_Enviar(
                int codEmpresa,
                CcEstadoCuentaEmailRequest request)
        {
            return _db
                .CC_FrmCCEstadoCuenta_Email_Enviar(
                    codEmpresa,
                    request);
        }

        public ErrorDto
            CC_FrmCCEstadoCuenta_EmailMasivo_Enviar(
                int codEmpresa,
                CcEstadoCuentaEmailMasivoRequest request)
        {
            return _db
                .CC_FrmCCEstadoCuenta_EmailMasivo_Enviar(
                    codEmpresa,
                    request);
        }

        public ErrorDto CC_FrmCCEstadoCuenta_Reporte_Bitacora_Registrar(
        int codEmpresa,
        CcEstadoCuentaReporteBitacoraRequest request)
        {
            return _db
                .CC_FrmCCEstadoCuenta_Reporte_Bitacora_Registrar(
                    codEmpresa,
                    request);
        }

        private static T? Deserializar<T>(
            string request)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(
                    request);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}