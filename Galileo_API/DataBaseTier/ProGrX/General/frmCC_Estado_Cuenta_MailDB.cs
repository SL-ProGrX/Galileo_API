using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.General;
using System.Data;
using System.Globalization;
using System.Net.Mail;

namespace Galileo_API.DataBaseTier.ProGrX.General
{
    public sealed class FrmCcEstadoCuentaMailDb
    {
        private const int ModuloGeneral = 10;
        private const int TiempoEsperaSegundos = 300;
        private const string TipoResumen = "RESUMEN";
        private const string TipoDetallado = "DETALLADO";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MProGrxMain _mProGrx_Main;

        public FrmCcEstadoCuentaMailDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mProGrx_Main = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene el correo registrado y los periodos de cierre disponibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CcEstadoCuentaMailInicialData>
            CC_Estado_Cuenta_Mail_Inicializar(
                int codEmpresa,
                string cedula)
        {
            string identificacion =
                cedula?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return DbHelper.CreateErrorResponse(
                    "Especifique la identificaci&oacute;n de la persona.",
                    -2,
                    new CcEstadoCuentaMailInicialData());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    const string sqlEmail = """
                        select rtrim(isnull(AF_Email, ''))
                        from SOCIOS
                        where CEDULA = @cedula
                        """;

                    string email =
                        connection.QueryFirstOrDefault<string>(
                            sqlEmail,
                            new
                            {
                                cedula = identificacion
                            }) ?? string.Empty;

                    List<DropDownListaGenericaModel> periodos =
                        connection
                            .Query<CcEstadoCuentaMailPeriodoData>(
                                "spSys_Periodos_Cierre_Consulta",
                                commandType:
                                    CommandType.StoredProcedure)
                            .Skip(1)
                            .Select(periodo =>
                                new DropDownListaGenericaModel
                                {
                                    item =
                                        CC_Estado_Cuenta_Mail_Periodo_Normalizar(
                                            periodo.idx),

                                    descripcion =
                                        periodo.itmx.Trim()
                                })
                            .ToList();

                    return new CcEstadoCuentaMailInicialData
                    {
                        email = email.Trim(),
                        periodos = periodos
                    };
                });
        }

        /// <summary>
        /// Envia el estado de cuenta resumen o detallado al correo registrado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CC_Estado_Cuenta_Mail_Enviar(
            int codEmpresa,
            CcEstadoCuentaMailEnviarRequest? request)
        {
            if (request is null)
            {
                return CC_Estado_Cuenta_Mail_ErrorValidacion(
                    "La solicitud es requerida.");
            }

            string tipo =
                request.tipo
                    .Trim()
                    .ToUpperInvariant();

            string? validacion =
                CC_Estado_Cuenta_Mail_Solicitud_Validar(
                    request,
                    tipo);

            if (validacion is not null)
            {
                return CC_Estado_Cuenta_Mail_ErrorValidacion(
                    validacion);
            }

            string cedula =
                request.cedula.Trim();

            string usuario =
                request.usuario.Trim();

            ErrorDto<string> resultadoEmail =
                CC_Estado_Cuenta_Mail_Email_Obtener(
                    codEmpresa,
                    cedula);

            if (resultadoEmail.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultadoEmail.Description ??
                    "No fue posible consultar el correo registrado.",
                    -1);
            }

            string email =
                resultadoEmail.Result?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                return CC_Estado_Cuenta_Mail_ErrorValidacion(
                    "La persona no cuenta con un correo registrado, verifique!");
            }

            if (!CC_Estado_Cuenta_Mail_Email_EsValido(email))
            {
                return CC_Estado_Cuenta_Mail_ErrorValidacion(
                    "El correo electr&oacute;nico registrado no es v&aacute;lido.");
            }

            if (tipo == TipoDetallado)
            {
                return _mProGrx_Main
                    .sbEstadoCuenta_Email_Corte(
                        codEmpresa,
                        usuario,
                        cedula,
                        email,
                        request.fecha_corte
                            .GetValueOrDefault());
            }

            ErrorDto<int> resultadoResumen =
                CC_Estado_Cuenta_Mail_Resumen_Enviar(
                    codEmpresa,
                    cedula);

            if (resultadoResumen.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultadoResumen.Description ??
                    "No fue posible enviar el estado de cuenta.",
                    -1);
            }

            CC_Estado_Cuenta_Mail_Resumen_Bitacora_Registrar(
                codEmpresa,
                usuario,
                cedula);

            return DbHelper.OkResponse(
                "Estado de Cuenta enviado al Correo Electr&oacute;nico registrado de la persona!");
        }

        private static string?
            CC_Estado_Cuenta_Mail_Solicitud_Validar(
                CcEstadoCuentaMailEnviarRequest request,
                string tipo)
        {
            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "Especifique la identificaci&oacute;n de la persona.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "No se pudo determinar el usuario.";
            }

            if (tipo is not (TipoResumen or TipoDetallado))
            {
                return "El tipo de estado de cuenta no es v&aacute;lido.";
            }

            if (tipo == TipoDetallado &&
                (!request.fecha_corte.HasValue ||
                 request.fecha_corte.GetValueOrDefault() == default))
            {
                return "Seleccione un per&iacute;odo de corte v&aacute;lido.";
            }

            return null;
        }

        private ErrorDto<string>
            CC_Estado_Cuenta_Mail_Email_Obtener(
                int codEmpresa,
                string cedula)
        {
            const string sql = """
                select rtrim(isnull(AF_Email, ''))
                from SOCIOS
                where CEDULA = @cedula
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection
                        .QueryFirstOrDefault<string>(
                            sql,
                            new { cedula })
                        ?.Trim() ??
                    string.Empty);
        }

        private static bool
            CC_Estado_Cuenta_Mail_Email_EsValido(
                string email)
        {
            string correo =
                email.Trim();

            return MailAddress.TryCreate(
                       correo,
                       out MailAddress? direccion) &&
                   string.Equals(
                       direccion.Address,
                       correo,
                       StringComparison.OrdinalIgnoreCase);
        }

        private ErrorDto<int>
            CC_Estado_Cuenta_Mail_Resumen_Enviar(
                int codEmpresa,
                string cedula)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    connection.Execute(
                        "spuProGrX_MOBILE_CUENTAS_ENVIAESTADO",
                        new { cedula },
                        commandType:
                            CommandType.StoredProcedure,
                        commandTimeout:
                            TiempoEsperaSegundos);

                    return 1;
                });
        }

        private void
            CC_Estado_Cuenta_Mail_Resumen_Bitacora_Registrar(
                int codEmpresa,
                string usuario,
                string cedula)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId =
                        codEmpresa,

                    Usuario =
                        usuario,

                    DetalleMovimiento =
                        $"Estado de Cuenta: [email] {cedula}",

                    Movimiento =
                        "Aplica - WEB",

                    Modulo =
                        ModuloGeneral
                });
        }

        private static object
            CC_Estado_Cuenta_Mail_Periodo_Normalizar(
                object? valor)
        {
            if (valor is DateTime fecha)
            {
                return fecha.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);
            }

            string texto =
                Convert.ToString(
                    valor,
                    CultureInfo.InvariantCulture)
                    ?.Trim() ??
                string.Empty;

            return DateTime.TryParse(
                texto,
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out DateTime fechaConvertida)
                    ? fechaConvertida.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture)
                    : texto;
        }

        private static ErrorDto
            CC_Estado_Cuenta_Mail_ErrorValidacion(
                string mensaje)
        {
            return DbHelper.ErrorResponse(
                mensaje,
                -2);
        }
    }
}