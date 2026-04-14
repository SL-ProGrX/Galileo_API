using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoGestorExternoModels;
using Galileo.Models.Security;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoGestorExternoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 4;

        private const string SpRegistrar = @"
            EXEC spCBR_Gestor_Externo_Add
                @Operacion,
                @GestionUsuario,
                @Cedula,
                @Nombre,
                @Expediente,
                @Notas,
                @Usuario;";

        private const string SpReversar = @"
            EXEC spCBR_Gestor_Externo_Del
                @Id,
                @Notas,
                @Usuario;";

        private const string SpDesvincularMasivo = @"
            EXEC spCBR_Gestor_Externo_Del_Masivo
                @Operacion,
                @Notas,
                @Usuario;";

        public FrmCoGestorExternoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        public ErrorDto<string> Crd_GestorExterno_Registrar(int codEmpresa, CrdGestorExternoRegistrarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var validacion = ValidarRegistro(request);
                if (validacion is not null)
                {
                    return validacion;
                }

                var result = EjecutarSp(
                    connection,
                    SpRegistrar,
                    CrearParametrosRegistrar(request));

                if (result is null)
                {
                    return CrearErrorString("No se recibió respuesta del proceso de registro.", -1);
                }

                if (result.Pass != 1)
                {
                    return CrearErrorString(
                        string.IsNullOrWhiteSpace(result.Mensaje)
                            ? "No fue posible registrar el caso."
                            : result.Mensaje,
                        -2);
                }

                RegistrarBitacora(codEmpresa, request.UsuarioEjecuta, result);

                return DbHelper.CreateOkResponse(
                    $"{result.Movimiento}Caso con Gestor Externo, procesado satisfactoriamente!");
            }
            catch (Exception)
            {
                return CrearErrorString("Error al registrar el caso con gestor externo.", -1);
            }
        }

        public ErrorDto<bool> Crd_GestorExterno_Reversar(int codEmpresa, CrdGestorExternoReversaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var validacion = ValidarReversa(request);
                if (validacion is not null)
                {
                    return validacion;
                }

                var result = EjecutarSp(
                    connection,
                    SpReversar,
                    CrearParametrosReversa(request));

                return ConstruirRespuestaBoolDesdeSp(
                    result,
                    "No se recibió respuesta del proceso de reversa.",
                    "No fue posible desvincular el caso.");
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al desvincular el caso del gestor externo.",
                    -1,
                    false);
            }
        }

        private static ErrorDto<string>? ValidarRegistro(CrdGestorExternoRegistrarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GestionUsuario))
            {
                return CrearErrorString("Debe indicar un gestor externo.", -1);
            }

            if (request.Operacion <= 0)
            {
                return CrearErrorString("Debe indicar una operación válida.", -1);
            }

            if (string.IsNullOrWhiteSpace(request.Cedula))
            {
                return CrearErrorString("Debe indicar la cédula.", -1);
            }

            if (string.IsNullOrWhiteSpace(request.Notas) || request.Notas.Trim().Length < 10)
            {
                return CrearErrorString("Debe indicar una nota válida de al menos 10 caracteres.", -1);
            }

            return null;
        }

        private static ErrorDto<bool>? ValidarReversa(CrdGestorExternoReversaRequest request)
        {
            if (request.CasoId <= 0)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Debe indicar un caso válido.",
                    -1,
                    false);
            }

            if (string.IsNullOrWhiteSpace(request.Notas) || request.Notas.Trim().Length < 30)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Debe indicar una nota válida de al menos 30 caracteres.",
                    -1,
                    false);
            }

            return null;
        }

        private static DynamicParameters CrearParametrosRegistrar(CrdGestorExternoRegistrarRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operacion", request.Operacion);
            parameters.Add("@GestionUsuario", request.GestionUsuario.Trim());
            parameters.Add("@Cedula", request.Cedula.Trim());
            parameters.Add("@Nombre", request.Nombre?.Trim() ?? string.Empty);
            parameters.Add("@Expediente", request.Expediente?.Trim() ?? string.Empty);
            parameters.Add("@Notas", request.Notas.Trim());
            parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());
            return parameters;
        }

        private static DynamicParameters CrearParametrosReversa(CrdGestorExternoReversaRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.CasoId);
            parameters.Add("@Notas", request.Notas.Trim());
            parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());
            return parameters;
        }

        private static DynamicParameters CrearParametrosAsignacion(
            CrdGestorExternoCargaMasivaRequest request,
            CrdGestorExternoCargaFilaRequest registro)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operacion", registro.Operacion);
            parameters.Add("@GestionUsuario", request.GestionUsuario.Trim());
            parameters.Add("@Cedula", string.Empty);
            parameters.Add("@Nombre", string.Empty);
            parameters.Add("@Expediente", registro.Expediente?.Trim() ?? string.Empty);
            parameters.Add("@Notas", registro.Notas.Trim());
            parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());
            return parameters;
        }

        private static DynamicParameters CrearParametrosDesvinculacion(
            CrdGestorExternoCargaMasivaRequest request,
            CrdGestorExternoCargaFilaRequest registro)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operacion", registro.Operacion);
            parameters.Add("@Notas", registro.Notas.Trim());
            parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());
            return parameters;
        }

        private static CrdGestorExternoSpResponse? EjecutarSp(
            IDbConnection connection,
            string query,
            DynamicParameters parameters)
        {
            return connection.QueryFirstOrDefault<CrdGestorExternoSpResponse>(query, parameters);
        }

        private static ErrorDto<string> CrearErrorString(string mensaje, int code)
        {
            return DbHelper.CreateErrorResponse<string>(mensaje, code, string.Empty);
        }

        private static ErrorDto<bool> ConstruirRespuestaBoolDesdeSp(
            CrdGestorExternoSpResponse? result,
            string mensajeNull,
            string mensajeDefault)
        {
            if (result is null)
            {
                return DbHelper.CreateErrorResponse<bool>(mensajeNull, -1, false);
            }

            if (result.Pass == 1)
            {
                return DbHelper.CreateOkResponse(true);
            }

            return DbHelper.CreateErrorResponse<bool>(
                string.IsNullOrWhiteSpace(result.Mensaje) ? mensajeDefault : result.Mensaje,
                -1,
                false);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, CrdGestorExternoSpResponse result)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"{result.Movimiento},{result.Mensaje}",
                Movimiento = "REGISTRA-WEB",
                Modulo = vModulo
            });
        }

        private static void ProcesarAsignacion(
            IDbConnection connection,
            CrdGestorExternoCargaMasivaRequest request,
            CrdGestorExternoCargaFilaRequest registro,
            CrdGestorExternoCargaMasivaResponse response)
        {
            var result = EjecutarSp(
                connection,
                SpRegistrar,
                CrearParametrosAsignacion(request, registro));

            ProcesarResultadoSp(result, registro.Operacion, response, "No fue posible asignar.");
        }

        private static void ProcesarDesvinculacion(
            IDbConnection connection,
            CrdGestorExternoCargaMasivaRequest request,
            CrdGestorExternoCargaFilaRequest registro,
            CrdGestorExternoCargaMasivaResponse response)
        {
            var result = EjecutarSp(
                connection,
                SpDesvincularMasivo,
                CrearParametrosDesvinculacion(request, registro));

            ProcesarResultadoSp(result, registro.Operacion, response, "No fue posible desvincular.");
        }

        private static void ProcesarResultadoSp(
            CrdGestorExternoSpResponse? result,
            long operacion,
            CrdGestorExternoCargaMasivaResponse response,
            string mensajeDefault)
        {
            if (result?.Pass == 1)
            {
                response.TotalProcesados++;
                return;
            }

            response.TotalConError++;
            response.Mensajes.Add($"Operación {operacion}: {result?.Mensaje ?? mensajeDefault}");
        }
    }
}