using CoreInterno;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo_API.Controllers.WFCSinpe;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Galileo_API.DataBaseTier
{
    public class MKindoServiceDb : IWfcSinpe
    {
        private readonly PortalDB _portalDB;
        private readonly IConfiguration _config;
        private readonly SinpeGalileoPin _PIN;

        private readonly Guid OperationId;

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        public MKindoServiceDb(IConfiguration config)
        {
            _config = config;
            _PIN = new SinpeGalileoPin(_config);
            OperationId = Guid.NewGuid();
            _portalDB = new PortalDB(_config);
        }

        #region Helpers privados (para reducir duplicidad)

        private SqlConnection OpenConnection(int codEmpresa)
        {
            var cs = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(cs);
        }

        // Wrapper genérico para reducir try/catch repetidos
        private static T Safe<T>(Func<T> f, Func<T> fallback)
        {
            try { return f(); }
            catch { return fallback(); }
        }

        private static CoreInterno.CL_ResultadoValidacion[] ErrorValidacionArray()
            => new[]
            {
                new CL_ResultadoValidacion
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    MotivoError = -1,
                }
            };

        private static CoreInterno.CL_RespuestaTransaccion[] ErrorRespuestaTransaccionArray()
            => new[]
            {
                new CoreInterno.CL_RespuestaTransaccion
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    MotivoError = -1,
                    ComprobanteInterno = ""
                }
            };

        private static CoreInterno.CL_ResultadoActualizacion[] ErrorActualizacionArray()
            => new[]
            {
                new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = CoreInterno.E_ResultadoActualizacion.Error,
                    IdRelacionCliente = null
                }
            };

        // Parámetros compartidos entre INSERT/UPDATE de SINPE_MOV_TRANSITO
        // (reduce duplicidad y evita divergencias accidentales)
        private object BuildMovTransitoParams(
            int codEmpresa,
            string codReferencia,
            string usuario,
            ResSendingDynamic resPIN,
            TesTransaccion solicitud,
            bool incluirFechaActualiza)
        {
            return new
            {
                CodTransito = ConsecutivoMovTransito(codEmpresa),
                Cedula = solicitud.Codigo,
                CuentaCliente = solicitud.Cuenta,
                BancoOrigen = Convert.ToInt32(Inferir((solicitud.CedulaOrigen ?? "").Replace("-", "")).Codigo),
                BancoOrigenDesc = solicitud.NombreOrigen,
                CedulaOrigen = solicitud.CedulaOrigen,
                CuentaClienteOrigen = solicitud.CuentaOrigen,
                CodReferencia = codReferencia,
                CodServicio = 21,
                CodMoneda = solicitud.Divisa,
                Monto = solicitud.Monto,
                MontoComision = "",
                Accion = "C", // antes no se enviaba en Update; mantiene intención del flujo (ajusta si aplica otro valor)
                TransacTipo = "C",
                TransacDesc = (solicitud.Detalle1 ?? "") + (solicitud.Detalle2 ?? "") + (solicitud.Detalle3 ?? "") + (solicitud.Detalle4 ?? ""),
                RegistroFecha = DateTime.Now,
                RegistroUsuario = usuario,
                ComprobanteInterno = resPIN.PINSendingResult?.SINPEReference ?? string.Empty,
                RechazoCodigo = (resPIN.Errors != null && resPIN.Errors.Length > 0) ? resPIN.Errors[0].Code : 0,
                RechazoDesc = (resPIN.Errors != null && resPIN.Errors.Length > 0) ? resPIN.Errors[0].Message : string.Empty,
                Estado = resPIN.PINSendingResult!.State,
                FechaActualiza = incluirFechaActualiza ? DateTime.Now : (DateTime?)null,
                Servicio = Convert.ToInt32(Inferir((solicitud.CedulaOrigen ?? "").Replace("-", "")).Codigo)
            };
        }

        private static readonly IReadOnlyDictionary<string, string> FuncionesSinpeSql =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "fnSinpe_ValidaTransaccion",
                    @"
SELECT *
FROM dbo.fnSinpe_ValidaTransaccion(
    @IDENTIFICACION,
    @CUENTAIBAN,
    @CODIGO_MONEDA,
    @CODIGO_SERVICIO,
    @MONTO,
    NULL
);"
                },
                {
                    "fnSinpe_ValidaTransaccionMasiva",
                    @"
SELECT *
FROM dbo.fnSinpe_ValidaTransaccionMasiva(
    @IDENTIFICACION,
    @CUENTAIBAN,
    @CODIGO_MONEDA,
    @CODIGO_SERVICIO,
    @MONTO,
    NULL
);"
                },
                {
                    "fxSinpe_ValidaDebito",
                    @"
SELECT *
FROM dbo.fxSinpe_ValidaDebito(
    @IDENTIFICACION,
    @CUENTAIBAN,
    @CODIGO_MONEDA,
    @CODIGO_SERVICIO,
    @MONTO,
    @REGISTROUSUARIO
);"
                },
                {
                    "fxSinpe_ValidaCredito",
                    @"SELECT *
FROM dbo.fxSinpe_ValidaCredito(
    @IDENTIFICACION,
    @CUENTAIBAN,
    @CODIGO_MONEDA,
    @CODIGO_SERVICIO,
    @MONTO,
    @REGISTROUSUARIO
);"
                },
            };

        private CoreInterno.CL_ResultadoValidacion[] ValidaTransacciones(
            int codEmpresa,
            ValidaTransRequest request,
            string sqlFunctionName,
            bool normalizarId)
        {
            if (string.IsNullOrWhiteSpace(sqlFunctionName) ||
                !FuncionesSinpeSql.TryGetValue(sqlFunctionName, out var query))
            {
                throw new ArgumentException(
                    $"Función SQL no permitida: '{sqlFunctionName}'",
                    nameof(sqlFunctionName));
            }

            var resultado = new List<CoreInterno.CL_ResultadoValidacion>();
            using var connection = OpenConnection(codEmpresa);

            foreach (var t in request.Transacciones!)
            {
                var identificacion = normalizarId
                    ? (t.Identificacion ?? "").Trim().Replace("-", "").Replace(" ", "")
                    : t.Identificacion;

                var valida = connection.QueryFirstOrDefault<dynamic>(query, new
                {
                    IDENTIFICACION = identificacion,
                    CUENTAIBAN = t.CuentaIBAN,
                    CODIGO_MONEDA = t.CodigoMoneda,
                    CODIGO_SERVICIO = t.CodigoServicio,
                    MONTO = t.Monto,
                    REGISTROUSUARIO = request.Rastro!.Usuario
                });

                if ((int?)valida?.CODIGO_ERROR > 0)
                {
                    resultado.Add(new CoreInterno.CL_ResultadoValidacion
                    {
                        Resultado = CoreInterno.E_Resultado.Error,
                        MotivoError = valida.CODIGO_ERROR,
                        InformacionAdicional = new CL_Adicional_Info[]
                        {
                            new CL_Adicional_Info
                            {
                                Mostrar = true,
                                Nombre = "PgrX",
                                NombreFisico = "Galileo",
                                Valor = valida.DETALLE
                            }
                        }
                    });
                }
                else
                {
                    resultado.Add(new CoreInterno.CL_ResultadoValidacion
                    {
                        Resultado = CoreInterno.E_Resultado.Exitoso,
                        MotivoError = 0,
                        InformacionAdicional = new CL_Adicional_Info[]
                        {
                            new CL_Adicional_Info
                            {
                                Mostrar = true,
                                Nombre = "PgrX",
                                NombreFisico = "Galileo",
                                Valor = valida!.DETALLE
                            }
                        }
                    });
                }
            }

            return resultado.ToArray();
        }

        private static readonly HashSet<string> SpCongeladosPermitidos =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "spSinpe_AplicaCongelados",
                "spSinpe_AplicaCongeladosMasivo",
            };

        private CoreInterno.CL_RespuestaTransaccion[] AplicaCongelados(
            int codEmpresa,
            CoreInterno.CL_Transaccion[] transacciones,
            string storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(storedProcedure) ||
                !SpCongeladosPermitidos.Contains(storedProcedure))
            {
                throw new ArgumentException(
                    $"Stored procedure no permitido: '{storedProcedure}'",
                    nameof(storedProcedure));
            }

            var resultado = new List<CoreInterno.CL_RespuestaTransaccion>();
            using var connection = OpenConnection(codEmpresa);

            foreach (var s in transacciones)
            {
                var trx = fxTesConsultaSolicitud(codEmpresa, Convert.ToInt32(s.CodigoReferencia));

                var parametros = new
                {
                    CENTRO_COSTO = s.CentroCosto,
                    COD_ENTIDAD = s.CodEntidad,
                    CODIGO_REFERENCIA = s.CodigoReferencia,
                    COMPROBANTE_CGP = s.ComprobanteCGP,
                    CUENTA_IBAN_CONTRAPARTE = s.CuentaIBANContraparte,
                    DESCRIPCION = s.Descripcion,
                    FECHA_CICLO = s.FechaCiclo,
                    ID_ORIGEN = s.IdOrigen,
                    SERVICIO = s.Servicio,
                    MONEDA_COMISION = s.MonedaComision,
                    MONTO_COMISION = s.MontoComision,
                    NOMBRE_ORIGEN = s.NombreOrigen,
                    IDENTIFICACION_CONTRAPARTE = s.IdentificacionContraparte,
                    IDENTIFICACION = s.Identificacion,
                    CUENTA_IBAN = s.CuentaIBAN,
                    MONTO = s.Monto,
                    CODIGO_MONEDA = s.CodigoMoneda,
                    CODIGO_SERVICIO = s.CodigoServicio,
                    IDRELACIONCLIENTE = s.IdRelacionCliente,
                    CANAL = "SINPE",
                    REGISTRO_USUARIO = trx?.Result?.UsuarioGenera,
                    COD_EMPRESA = codEmpresa
                };

                var res = connection.QueryFirstOrDefault<dynamic>(
                    storedProcedure,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                bool rechazo = (res?.MOT_RECHAZO ?? 0) > 0;

                resultado.Add(new CoreInterno.CL_RespuestaTransaccion
                {
                    Resultado = rechazo
                        ? CoreInterno.E_Resultado.Rechazo
                        : CoreInterno.E_Resultado.Exitoso,
                    MotivoError = rechazo ? res!.MOT_RECHAZO : 0,
                    ComprobanteInterno = res?.ID_REFERENCIA
                });
            }

            return resultado.ToArray();
        }

        private static readonly HashSet<string> SpActualizacionPermitidos =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "spSinpe_ActualizaTransaccion",
                "spSinpe_ReversaTransaccion",
            };

        private CoreInterno.CL_ResultadoActualizacion[] EjecutaActualizacion(
            int codEmpresa,
            IEnumerable<CoreInterno.CL_ActualizaTransaccion> transacciones,
            string storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(storedProcedure) ||
                !SpActualizacionPermitidos.Contains(storedProcedure))
            {
                throw new ArgumentException(
                    $"Stored procedure no permitido: '{storedProcedure}'",
                    nameof(storedProcedure));
            }

            var resultado = new List<CoreInterno.CL_ResultadoActualizacion>();
            using var connection = OpenConnection(codEmpresa);

            foreach (var s in transacciones)
            {
                var res = connection.QueryFirstOrDefault<dynamic>(
                    storedProcedure,
                    new
                    {
                        CODIGO_RECHAZO_SINPE = s.CodigoRechazoSINPE,
                        CODIGO_REFERENCIA = s.CodigoReferencia,
                        COMPTOBANTE_CGP = s.ComprobanteCGP,
                        COMPROBANTE_INTERNO = s.ComprobanteInterno,
                        DESCRIPCION_RECHAZO = s.DescripcionRechazo
                    },
                    commandType: CommandType.StoredProcedure
                );

                resultado.Add(new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = res?.Resultado,
                    IdRelacionCliente = s.IdRelacionCliente
                });
            }

            return resultado.ToArray();
        }

        private static readonly HashSet<string> SpReversaPermitidos =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "sp_Sinpe_ReversaCreditos",
                "sp_Sinpe_ReversaDebitos"
            };

        private CoreInterno.CL_ResultadoActualizacion[] EjecutaReversa(
            int codEmpresa,
            IEnumerable<CoreInterno.TransaccionRechazada> transacciones,
            string storedProcedure)
        {
            if (string.IsNullOrWhiteSpace(storedProcedure) ||
                !SpReversaPermitidos.Contains(storedProcedure))
            {
                throw new ArgumentException(
                    $"Stored procedure no permitido: '{storedProcedure}'",
                    nameof(storedProcedure));
            }

            var resultado = new List<CoreInterno.CL_ResultadoActualizacion>();
            using var connection = OpenConnection(codEmpresa);

            foreach (var s in transacciones)
            {
                var res = connection.QueryFirstOrDefault<dynamic>(
                    storedProcedure,
                    new
                    {
                        CODIGO_RECHAZO_SINPE = s.CodigoRechazoSINPE,
                        CODIGO_REFERENCIA = s.CodigoReferencia,
                        COMPTOBANTE_CGP = s.ComprobanteCGP,
                        COMPROBANTE_INTERNO = s.ComprobanteInterno,
                        DESCRIPCION_RECHAZO = s.DescripcionRechazo
                    },
                    commandType: CommandType.StoredProcedure
                );

                var r = res?.Resultado ?? CoreInterno.E_ResultadoActualizacion.Error;

                resultado.Add(new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = r,
                    IdRelacionCliente = s.IdRelacionCliente
                });
            }

            return resultado.ToArray();
        }

        #endregion

        #region Métodos de integración de uso general

        public bool ServicioDisponible(int CodEmpresa)
        {
            var response = false;
            try
            {
                using var connection = OpenConnection(CodEmpresa);
                var query = "SELECT COUNT(*) FROM SINPE_PARAMETROS_EMPRESA";
                response = connection.ExecuteScalar<int>(query) > 0;
            }
            catch
            {
                response = false;
            }
            return response;
        }

        public CoreInterno.CuentaIBAN_Response ObtenerCuentaIBAN(int CodEmpresa, CoreInterno.CuentaIBAN_Request DatosCuenta)
        {
            var cuenta = new CoreInterno.CL_CuentaIBAN();
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = $@"exec sp_Sinpe_ObtenerCuentaIBAN @CuentaIBAN";
                var result = connection.QueryFirstOrDefault<dynamic>(query, new { CuentaIBAN = DatosCuenta.CuentaIBAN });

                if (result != null)
                {
                    var identificacion = "";
                    if (result.Dimex_Activo == 1)
                    {
                        identificacion = Inferir(result.Dimex_Cedula).Codigo;
                    }
                    else
                    {
                        identificacion = Inferir(result.CEDULA).Codigo;
                    }

                    result.IdTitular = result.CEDULA;
                    result.NombreTitular = result.NOMBRE;
                    result.TipoId = Convert.ToInt32(identificacion);

                    if (string.IsNullOrEmpty(result.NombreTitular))
                    {
                        result.NombreTitular = ValidoNombreCuenta(CodEmpresa, result.TipoId, result.CEDULA.Replace("-", "").Replace(" ", ""));
                    }

                    result.IdTitular = MaskSinpeId(result.TipoId, result.CEDULA.Replace("-", "").Replace(" ", ""));

                    if (string.IsNullOrEmpty(result.NombreTitular))
                    {
                        result.NombreTitular = "Desconocido en " + result.IdTitular;
                    }

                    cuenta = new CoreInterno.CL_CuentaIBAN()
                    {
                        CuentaIBAN = result.CUENTA_IBAN,
                        DesProducto = result.DesProducto,
                        Estado = (result.ESTADO == "A") ? 1 : 23,
                        IdTitular = result.IdTitular,
                        Moneda = result.MONEDA,
                        NombreTitular = result.NombreTitular,
                        TipoId = result.TipoId
                    };

                    return new CoreInterno.CuentaIBAN_Response()
                    {
                        CuentaIBAN = cuenta,
                        Errores = null,
                        Resultado = true
                    };
                }

                return new CoreInterno.CuentaIBAN_Response()
                {
                    CuentaIBAN = cuenta,
                    Errores = new CL_Error[]
                    {
                        new CL_Error()
                        {
                            NumError = 28,
                            Descripcion = "La cuenta IBAN no existe."
                        }
                    },
                    Resultado = false
                };
            }
            catch
            {
                return new CoreInterno.CuentaIBAN_Response()
                {
                    CuentaIBAN = cuenta,
                    Errores = new CL_Error[]
                    {
                        new CL_Error()
                        {
                            NumError = 28,
                            Descripcion = "La cuenta IBAN no existe."
                        }
                    },
                    Resultado = false
                };
            }
        }

        public CoreInterno.CL_ObtieneInfoCuenta ObtieneInfoCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = $@"exec sp_Sinpe_ObtieneInfoCuenta @Identificacion, @CuentaIBAN ";
                var result = connection.QueryFirstOrDefault<dynamic>(query, new { Identificacion = Identificacion, CuentaIBAN = CuentaIBAN });

                if (result != null)
                {
                    if (result.CodigoResultado > 0)
                    {
                        return new CoreInterno.CL_ObtieneInfoCuenta
                        {
                            Resultado = CoreInterno.E_Resultado.Error,
                            Estado = CoreInterno.E_Estado.Bloqueada,
                            MotivoError = result.CodigoResultado,
                        };
                    }

                    var identificacion = Inferir(result.CEDULA).Codigo;
                    string cedulaIBAN = result.CEDULA;

                    result.IdTitular = cedulaIBAN;
                    result.TipoId = Convert.ToInt32(identificacion);

                    if (string.IsNullOrEmpty(result.NOMBRE))
                    {
                        result.NOMBRE = ValidoNombreCuenta(CodEmpresa, result.TipoId, result.CEDULA.Replace("-", "").Replace(" ", ""));
                    }

                    result.IdTitular = MaskSinpeId(result.TipoId, result.IdTitular.Trim().Replace("-", "").Replace(" ", ""));

                    if (string.IsNullOrEmpty(result.NOMBRE))
                    {
                        result.NOMBRE = "Desconocido en " + result.IdTitular;
                    }
                }

                return new CoreInterno.CL_ObtieneInfoCuenta
                {
                    Resultado = CoreInterno.E_Resultado.Exitoso,
                    Estado = CoreInterno.E_Estado.Activa,
                    Moneda = result!.Moneda,
                    NombreTitular = result.NOMBRE,
                    MotivoError = 0
                };
            }
            catch
            {
                return new CoreInterno.CL_ObtieneInfoCuenta
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    Estado = CoreInterno.E_Estado.NoActiva,
                    MotivoError = 28,
                };
            }
        }

        public CoreInterno.CL_ValidaCuenta ValidaCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN, int? CodigoMoneda)
        {
            var resultado = new CoreInterno.CL_ValidaCuenta();
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                string tipo = CuentaIBAN!.Substring(8, 2);

                int tipoMovimiento;
                if (tipo == "01")
                    tipoMovimiento = 1;
                else if (tipo == "02")
                    tipoMovimiento = 2;
                else
                    tipoMovimiento = 1;

                var query = $@"SELECT dbo.fxSINPE_ValidaCuenta(@CUENTA, @TRANSAC_TIPO, @CEDULA , @MONEDA)";

                var valida = connection.QueryFirstOrDefault<int>(query, new
                {
                    CUENTA = CuentaIBAN,
                    TRANSAC_TIPO = tipoMovimiento,
                    CEDULA = Identificacion!.Replace("-", "").Replace(" ", ""),
                    MONEDA = CodigoMoneda
                });

                resultado.MotivoError = valida;
                resultado.Resultado = (valida == 0)
                    ? CoreInterno.E_Resultado.Exitoso
                    : CoreInterno.E_Resultado.Error;

                return resultado;
            }
            catch
            {
                return new CoreInterno.CL_ValidaCuenta
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    MotivoError = 28
                };
            }
        }

        public CoreInterno.CL_ResultadoTipoCambio ObtenerTipoCambio(int CodEmpresa, CoreInterno.SI_Rastro? Rastro, int? CodigoServicio, string? Cuentaorigen, string? CuentaDestino, decimal? Monto, int? Moneda)
        {
            try
            {
                Moneda ??= 2;
                var divisa = GetCurrencyKindoCode(Moneda);
                Monto ??= 1;

                if (divisa == "COL")
                {
                    return new CoreInterno.CL_ResultadoTipoCambio
                    {
                        montoTotal = Math.Round(Monto.Value, 2),
                        TipoCambioAplicado = 1
                    };
                }

                using var connection = OpenConnection(CodEmpresa);

                var tc = $@"exec dbo.sp_Sinpe_ObtenerTipoCambio @CODIGO_REFERENCIA";
                var tipoCambio = connection.QueryFirstOrDefault<decimal>(tc, new { CODIGO_REFERENCIA = Cuentaorigen });

                return new CoreInterno.CL_ResultadoTipoCambio
                {
                    montoTotal = Math.Round(Monto.Value * tipoCambio, 2),
                    TipoCambioAplicado = Math.Round(tipoCambio, 2)
                };
            }
            catch
            {
                return new CoreInterno.CL_ResultadoTipoCambio
                {
                    montoTotal = Math.Round(Monto.GetValueOrDefault(1), 2),
                    TipoCambioAplicado = 1
                };
            }
        }

        public CoreInterno.ComisionRespectivaResponse ComisionRespectiva(int CodEmpresa, CoreInterno.ComisionRespectivaRequest request)
        {
            var resultado = new CoreInterno.ComisionRespectivaResponse();
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = $@"SELECT dbo.fxPSL_VerificaComision(
                                    @Cedula, 
                                    @MontoSolicitado,
                                    @CodServicio, 
                                    @Origen
                                )";

                if (new[] { "21", "31", "22", "2222", "83", "84" }.Contains(request.codigoServicio.ToString().Trim()))
                {
                    var valida = connection.QueryFirstOrDefault<decimal>(query, new
                    {
                        Cedula = request.identificacion.Trim().Replace("-", ""),
                        MontoSolicitado = request.monto,
                        CodServicio = request.codigoServicio,
                        Origen = "CGPWEB",
                    });

                    resultado.comision = valida;
                    resultado.codigoMonedaComision = request.codigoMoneda;
                    resultado.ComisionRespectivaResult = CoreInterno.E_Resultado.Exitoso;
                }
                else
                {
                    resultado.comision = 0;
                    resultado.codigoMonedaComision = request.codigoMoneda;
                    resultado.ComisionRespectivaResult = CoreInterno.E_Resultado.Exitoso;
                }
            }
            catch
            {
                resultado.comision = 0;
                resultado.codigoMonedaComision = request.codigoMoneda;
                resultado.ComisionRespectivaResult = CoreInterno.E_Resultado.Error;
            }
            return resultado;
        }

        public CoreInterno.CL_ResultadoValidacion[] ValidaDebitos(int CodEmpresa, ValidaTransRequest request)
        {
            return Safe(
                () => ValidaTransacciones(CodEmpresa, request, "fxSinpe_ValidaDebito", normalizarId: true),
                ErrorValidacionArray);
        }

        public CoreInterno.CL_ResultadoValidacion[] ValidaCreditos(int CodEmpresa, ValidaTransRequest request)
        {
            return Safe(
                () => ValidaTransacciones(CodEmpresa, request, "fxSinpe_ValidaCredito", normalizarId: false),
                ErrorValidacionArray);
        }

        public CoreInterno.ValidacionPerfilTrx_Response ValidarPerfilTransaccional(int CodEmpresa, CoreInterno.ValidacionPerfilTrx_Request transaccion)
        {
            return new CoreInterno.ValidacionPerfilTrx_Response
            {
                Resultado = true,
                Autorizacion = new CoreInterno.CL_AutorizacionPerfilTrx
                {
                    CodMotivoRechazo = "0",
                    Estado = 1,
                    MotivoRechazo = "Transacción Autorizada",
                    NumRefProcesamiento = Guid.NewGuid().ToString()
                },
                Errores = null
            };
        }

        #endregion

        #region Métodos para la integración transaccional

        public CoreInterno.CL_RespuestaTransaccion[] AplicaDebitosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_Transaccion[] Debitos)
        {
            return Safe(
                () => AplicaCongelados(CodEmpresa, Debitos, "sp_Sinpe_AplicaDebitosCongelados"),
                ErrorRespuestaTransaccionArray);
        }

        public CoreInterno.CL_RespuestaTransaccion[] AplicaCreditosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_Transaccion[] Creditos)
        {
            return Safe(
                () => AplicaCongelados(CodEmpresa, Creditos, "sp_Sinpe_AplicaCreditosCongelados"),
                ErrorRespuestaTransaccionArray);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaCreditosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_ActualizaTransaccion[] Transacciones)
        {
            return Safe(
                () => EjecutaActualizacion(CodEmpresa, Transacciones, "sp_Sinpe_ConfirmaCreditoCongelado"),
                ErrorActualizacionArray);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaDebitosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_ActualizaTransaccion[] Transacciones)
        {
            return Safe(
                () => EjecutaActualizacion(CodEmpresa, Transacciones, "sp_Sinpe_ConfirmaDebitoCongelado"),
                ErrorActualizacionArray);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ReversaCreditos(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.TransaccionRechazada[] Transacciones)
        {
            return Safe(
                () => EjecutaReversa(CodEmpresa, Transacciones, "sp_Sinpe_ReversaCreditos"),
                ErrorActualizacionArray);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ReversaDebitos(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.TransaccionRechazada[] Transacciones)
        {
            return Safe(
                () => EjecutaReversa(CodEmpresa, Transacciones, "sp_Sinpe_ReversaDebitos"),
                ErrorActualizacionArray);
        }

        public CoreInterno.ObtieneEstadoTransaccionResponse ObtieneEstadoTransaccion(int CodEmpresa, CoreInterno.ObtieneEstadoTransaccionRequest Request)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = $@"exec sp_Sinpe_ObtieneEstadoTransaccion
                                             @CODIGO_REFERENCIA ";

                var result = connection.QueryFirstOrDefault<dynamic>(query, new
                {
                    CODIGO_REFERENCIA = Request.CodigoReferenciaSINPE
                });

                if (result != null)
                {
                    return new ObtieneEstadoTransaccionResponse
                    {
                        ComprobanteInterno = result.COMPROBANTE_INTERNO,
                        ObtieneEstadoTransaccionResult = true
                    };
                }

                return new ObtieneEstadoTransaccionResponse
                {
                    ComprobanteInterno = null,
                    ObtieneEstadoTransaccionResult = false
                };
            }
            catch
            {
                return new ObtieneEstadoTransaccionResponse
                {
                    ComprobanteInterno = null,
                    ObtieneEstadoTransaccionResult = false
                };
            }
        }

        #endregion

        #region Métodos para la integración de la liquidación de la cámara

        public static bool ActualizarFechaCiclo(int CodEmpresa, CL_ActualizaFechaRequest request)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool LiquidarCiclo(int CodEmpresa, CLCierraCiclo request)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Métodos para la integración del PortalCGP

        public CoreInterno.SaldoDisponibleResponse SaldoDisponible(int CodEmpresa, CoreInterno.SaldoDisponibleRequest Request)
        {
            var resultado = new CoreInterno.SaldoDisponibleResponse();
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = $@"exec sp_Sinpe_SaldoDisponible
                                        @Identificacion ,
	                                    @CuentaIBAN ,
	                                    @MontoSolicitado,
	                                    @CodigoServicio ";

                var result = connection.QueryFirstOrDefault<dynamic>(query, new
                {
                    Identificacion = Request.identificacion,
                    CuentaIBAN = Request.cuentaIBAN,
                    MontoSolicitado = Request.monto,
                    CodigoServicio = Request.codigoServicio
                });

                bool disponible = Convert.ToBoolean(result!.SaldoDisponible);

                resultado.disponible = disponible;
                resultado.SaldoDisponibleResult = disponible
                    ? CoreInterno.E_Resultado.Exitoso
                    : CoreInterno.E_Resultado.Rechazo;
            }
            catch
            {
                resultado = new CoreInterno.SaldoDisponibleResponse
                {
                    disponible = false,
                    SaldoDisponibleResult = CoreInterno.E_Resultado.Error
                };
            }
            return resultado;
        }

        public CoreInterno.ObtenerInformacionClienteResponse ObtenerInformacionCliente(
             int CodEmpresa,
             CoreInterno.ObtenerInformacionClienteRequest request)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                const string query = @"exec sp_Sinpe_ObtenerInformacionCliente @Identificacion";

                var result = connection.QueryFirstOrDefault<dynamic>(query, new
                {
                    Identificacion = request.identificacion,
                });

                var nombre = (string?)result?.NOMBRE;
                bool existe = !string.IsNullOrWhiteSpace(nombre);

                return new CoreInterno.ObtenerInformacionClienteResponse
                {
                    informacionCliente = new CoreInterno.CL_InformacionCliente
                    {
                        Nombre = existe ? nombre : null,
                        Existe = existe
                    },
                    ObtenerInformacionClienteResult = existe
                        ? CoreInterno.E_Resultado.Exitoso
                        : CoreInterno.E_Resultado.Error
                };
            }
            catch
            {
                return new CoreInterno.ObtenerInformacionClienteResponse
                {
                    informacionCliente = new CoreInterno.CL_InformacionCliente
                    {
                        Nombre = null,
                        Existe = false
                    },
                    ObtenerInformacionClienteResult = CoreInterno.E_Resultado.Error
                };
            }
        }

        public CoreInterno.ObtenerProductosPorClienteResponse ObtenerProductosPorCliente(int CodEmpresa, CoreInterno.ObtenerProductosPorClienteRequest request)
        {
            var resultado = new CoreInterno.ObtenerProductosPorClienteResponse();
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                var query = $@"exec sp_Sinpe_Obtener_ProductosporCliente
                                        @CEDULAPERSONA ";

                var result = connection.Query<dynamic>(query, new
                {
                    CEDULAPERSONA = request.identificacion,
                }).ToList();

                if (result.Count == 0)
                {
                    resultado.ObtenerProductosPorClienteResult = CoreInterno.E_Resultado.Error;
                    resultado.productos = new CoreInterno.CL_ProductoCliente[0];
                }
                else
                {
                    var productos = new List<CoreInterno.CL_ProductoCliente>();

                    foreach (var prod in result)
                    {
                        if (string.IsNullOrEmpty(prod.CuentaCliente))
                            continue;

                        if (prod.SINPE_PRODUCTO == 1)
                        {
                            productos.Add(new CoreInterno.CL_ProductoCliente
                            {
                                DescripcionServicio = prod.DescripcionServicio,
                                Cuota = prod.Cuota,
                                CodigoMoneda = prod.MonedaServicio,
                                Saldo = prod.Saldo,
                                CuentaCliente = prod.CuentaCliente,
                                MovimientosPermitidos = CoreInterno.E_MovimientosPermitidosProducto.Ambos,
                            });
                        }
                    }

                    resultado.ObtenerProductosPorClienteResult = CoreInterno.E_Resultado.Exitoso;
                    resultado.productos = productos.ToArray();
                }
            }
            catch
            {
                resultado.ObtenerProductosPorClienteResult = CoreInterno.E_Resultado.Error;
                resultado.productos = null;
            }
            return resultado;
        }

        #endregion

        #region Validacion formatos

        public static bool IsValidISO8601Date(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha)) return false;
            return DateTime.TryParseExact(
                fecha,
                new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.fffZ" },
                CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal,
                out _
            );
        }

        public static string GetCurrencyKindoCode(int? currencyCode = 2)
        {
            return currencyCode switch
            {
                1 => "COL",
                2 => "DOL",
                3 => "EU",
                _ => ""
            };
        }

        public static int GetCurrencyCodeId(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return 0;

            currency = currency.Trim().ToUpper();

            var aliases = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "COL", 1 }, { "CRC", 1 },
                { "DOL", 2 }, { "USD", 2 },
                { "EU",  3 }, { "EUR", 3 }
            };

            if (aliases.TryGetValue(currency, out var idFromAlias))
                return idFromAlias;

            return 0;
        }

        public static string GetCurrencyCodeDes(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return "CRC";

            currency = currency.Trim().ToUpper();

            var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "COL", "CRC" },
                { "DOL","USD" },
                { "EU",  "EUR" }
            };

            if (aliases.TryGetValue(currency, out var idFromAlias))
                return idFromAlias;

            return "CRC";
        }

        public static bool IsValidCostaRicaIBAN(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban)) return false;
            iban = iban.Replace(" ", "").ToUpperInvariant();

            if (!Regex.IsMatch(iban, @"^CR\d{2}\d{18}$", RegexOptions.None, RegexTimeout)) return false;
            if (iban.Length != 22) return false;

            string rearranged = iban.Substring(4) + iban.Substring(0, 4);
            string numericIban = Regex.Replace(
                rearranged,
                "[A-Z]",
                m => (m.Value[0] - 55).ToString(),
                RegexOptions.CultureInvariant,
                RegexTimeout
            );

            int remainder = int.Parse(numericIban.Substring(0, 9)) % 97;
            string rest = numericIban.Substring(9);

            while (rest.Length > 0)
            {
                string part = remainder + rest.Substring(0, Math.Min(7, rest.Length));
                rest = rest.Substring(Math.Min(7, rest.Length));
                remainder = int.Parse(part) % 97;
            }

            return remainder == 1;
        }

        public static bool IsValidCostaRicaId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            id = id.Replace("-", "").Trim();

            return Regex.IsMatch(id, @"^\d{9}$|^\d{10}$|^\d{11,12}$|^\d{11,12}$",
                RegexOptions.CultureInvariant, RegexTimeout);
        }

        public string IsValidTransactionNumber(int CodCliente, string iban)
        {
            string fecha = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string? canal = ObtenerCanal(iban);
            string consecutivo = ConsecutivoTransSinpe(CodCliente);

            return fecha + canal + consecutivo;
        }

        public static string? ObtenerCanal(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban))
                return null;

            var match = Regex.Match(iban, @"-(\d{4})-", RegexOptions.IgnoreCase | RegexOptions.Singleline,
                RegexTimeout);

            if (!match.Success)
                return null;

            var bloque = match.Groups[1].Value; // "1234"

            var canal = bloque.Substring(0, 2).PadLeft(3, '0');
            var subCanal = bloque.Substring(2, 2);

            return canal + subCanal;
        }

        public string ConsecutivoTransSinpe(int CodCliente)
        {
            const string query = @"SELECT ISNULL(MAX(COD_REFERENCIA), 0) + 1
                            FROM SINPE_MOV_TRANSITO
                            WHERE CAST(REGISTRO_FECHA AS DATE) = CAST(GETDATE() AS DATE)";

            var response = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                CodCliente,
                query, new int(), null);

            return response.Result.ToString("D12");
        }

        public sealed record TipoId(string Codigo, string Descripcion);

        private static TipoId Desconocido() => new TipoId("", "Desconocido");
        private static TipoId ExtranjeroNoResidente() => new TipoId("9", "Extranjero No Residente");
        private static TipoId FisicaNacional() => new TipoId("0", "Persona Física Nacional (Cédula)");
        private static TipoId Juridica() => new TipoId("3", "Persona Jurídica");
        private static TipoId Gobierno() => new TipoId("2", "Gobierno");
        private static TipoId InstitucionAutonoma() => new TipoId("4", "Institución Autónoma");
        private static TipoId Diplomaticos() => new TipoId("5", "Diplomáticos");
        private static TipoId FisicaResidente() => new TipoId("1", "Persona Física Residente");
        private static TipoId BancoInterna() => new TipoId("3", "Banco Interna");

        public static TipoId Inferir(string cedula)
        {
            var id = PrepararId(cedula);
            if (id == null) return Desconocido();

            if (TieneLetras(id)) return ExtranjeroNoResidente();

            return InferirPorLongitud(id);
        }

        private static string? PrepararId(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;
            var id = Normalizar(cedula);
            return id.Length == 0 ? null : id;
        }

        private static bool TieneLetras(string id) =>
            !id.All(char.IsDigit);

        private static TipoId InferirPorLongitud(string id)
        {
            return id.Length switch
            {
                9 => FisicaNacional(),
                10 => InferirLongitud10(id),
                11 or 12 => FisicaResidente(),
                < 9 => BancoInterna(),
                _ => ExtranjeroNoResidente()
            };
        }

        private static TipoId InferirLongitud10(string id)
        {
            return id[0] switch
            {
                '3' => Juridica(),
                '2' => Gobierno(),
                '4' => InstitucionAutonoma(),
                '5' => Diplomaticos(),
                _ => Juridica()
            };
        }

        private static string Normalizar(string x)
            => x.Trim()
                .Replace("-", "")
                .Replace(" ", "")
                .Replace("\t", "")
                .Replace("\r", "")
                .Replace("\n", "");

        public static string MaskSinpeId(int tipo, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return id;

            var digits = new string(id.Where(char.IsDigit).ToArray());

            static string Group(string s, params int[] sizes)
            {
                int pos = 0;
                var parts = new List<string>();
                foreach (var size in sizes)
                {
                    if (pos + size > s.Length) return s;
                    parts.Add(s.Substring(pos, size));
                    pos += size;
                }
                if (pos < s.Length) parts.Add(s.Substring(pos));
                return string.Join("-", parts);
            }

            switch (tipo)
            {
                case 0:
                    digits = digits.Insert(0, "0");
                    return digits.Length == 10 ? Group(digits, 2, 4, 4) : digits;

                case 1:
                    return digits;

                case 2:
                    return digits.Length == 10 ? Group(digits, 1, 3, 6) : digits;

                case 3:
                    return digits.Length == 10 ? Group(digits, 1, 3, 6) : digits;

                case 4:
                    return digits.Length == 10 ? Group(digits, 1, 3, 6) : digits;

                case 5:
                    if (digits.Length == 10) return Group(digits, 1, 3, 6);
                    return digits;

                default:
                    return digits;
            }
        }

        #endregion

        #region Consulta Core

        public ErrorDto<vInfoSinpe> fxTesConsultaInfoSinpe(int CodEmpresa, string solicitud)
        {
            var response = new ErrorDto<vInfoSinpe>
            {
                Code = 0,
                Description = "Ok",
                Result = new vInfoSinpe()
            };

            try
            {
                using var connection = OpenConnection(CodEmpresa);
                var res = connection.QueryFirstOrDefault<InfoSinpeData>(
                            "spTES_W_ConsultaInfoSinpe",
                            new { solicitud },
                            commandType: CommandType.StoredProcedure
                        );

                var infoSinpe = new vInfoSinpe
                {
                    Cedula = res!.Cedula,
                    CuentaIBAN = res!.Cuenta,
                    tipoID = res!.tipoID,
                    cod_divisa = res!.cod_divisa
                };

                response.Result = infoSinpe;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDto<string> fxObtenerIpEquipoActual(string nombreEquipo)
        {
            var response = new ErrorDto<string>()
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };
            try
            {
                IPHostEntry informacionDelHost = Dns.GetHostEntry(nombreEquipo);

                string? ip = informacionDelHost.AddressList
                    .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                    .ToString();

                response.Result = ip;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "";
            }
            return response;
        }

        public ErrorDto<ParametrosSinpe> GetUriEmpresa(int codEmpresa, string usuario)
        {
            var result = new ErrorDto<ParametrosSinpe>();
            try
            {
                using var connection = OpenConnection(codEmpresa);

                var query = "SELECT * FROM SINPE_PARAMETROS_EMPRESA WHERE COD_EMPRESA = @codEmpresa";
                var parametros = connection.Query(query, new
                {
                    codEmpresa = codEmpresa
                }).FirstOrDefault();

                if (parametros != null)
                {
                    var parametrosSinpe = new ParametrosSinpe
                    {
                        vHost = Environment.MachineName,
                        vHostPin = parametros.HostIdPIN,
                        vIpHost = fxObtenerIpEquipoActual(Environment.MachineName).Result ?? string.Empty,
                        vUserCGP = parametros.UserCGP,
                        vCanalCGP = Convert.ToInt32(parametros.CanalCGP),
                        UrlCGP_DTR = parametros.UrlCGP_DTR,
                        UrlCGP_PIN = parametros.UrlCGP_PIN,
                        vUsuarioLog = usuario,
                        ServiciosSinpe = parametros.ServiciosSinpe
                    };

                    result.Result = parametrosSinpe;
                }
                else
                {
                    result.Result = null;
                    result.Code = -1;
                    result.Description = "No se encontraron parametros SINPE para esta empresa";
                }
            }
            catch (Exception ex)
            {
                result.Result = null;
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        public ErrorDto<TesTransaccion> fxTesConsultaSolicitud(int CodEmpresa, int Nsolicitud)
        {
            var response = new ErrorDto<TesTransaccion>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesTransaccion()
            };

            try
            {
                using var connection = OpenConnection(CodEmpresa);
                var query = $@"SELECT 
                                    NSOLICITUD as 'NumeroSolicitud', 
                                    ID_BANCO, 
                                    TIPO, 
                                    CODIGO, 
                                    BENEFICIARIO, 
                                    MONTO, 
                                    FECHA_SOLICITUD, 
                                    ESTADO, 
                                    FECHA_EMISION as 'FechaEmision', 
                                    FECHA_ANULA, 
                                    ESTADOI, 
                                    MODULO, 
                                    CTA_AHORROS as Cuenta, 
                                    NDOCUMENTO, 
                                    DETALLE1, 
                                    DETALLE2, 
                                    DETALLE3, 
                                    DETALLE4, 
                                    DETALLE5, 
                                    REFERENCIA as 'CodigoReferencia', 
                                    SUBMODULO, 
                                    GENERA, 
                                    ACTUALIZA, 
                                    UBICACION_ACTUAL, 
                                    FECHA_TRASLADO as 'FechaTraslado', 
                                    UBICACION_ANTERIOR, 
                                    ENTREGADO, 
                                    AUTORIZA, 
                                    FECHA_ASIENTO, 
                                    FECHA_ASIENTO2, 
                                    ESTADO_ASIENTO, 
                                    FECHA_AUTORIZACION, 
                                    USER_AUTORIZA, 
                                    OP, 
                                    DETALLE_ANULACION, 
                                    USER_ASIENTO_EMISION, 
                                    USER_ASIENTO_ANULA, 
                                    COD_CONCEPTO, 
                                    COD_UNIDAD, 
                                    USER_GENERA as 'UsuarioGenera', 
                                    USER_SOLICITA, 
                                    USER_ANULA, 
                                    USER_ENTREGA, 
                                    FECHA_ENTREGA, 
                                    DOCUMENTO_REF, 
                                    DOCUMENTO_BASE as 'DocumentoBase', 
                                    DETALLE, 
                                    USER_HOLD, 
                                    FECHA_HOLD, 
                                    FIRMAS_AUTORIZA_FECHA, 
                                    FIRMAS_AUTORIZA_USUARIO, 
                                    TIPO_CAMBIO as 'tipoCambio', 
                                    COD_DIVISA as 'Divisa', 
                                    TIPO_BENEFICIARIO, 
                                    COD_APP, 
                                    REF_01,
                                    REF_02, 
                                    REF_03, 
                                    ID_TOKEN, 
                                    REMESA_TIPO, 
                                    REMESA_ID, 
                                    ASIENTO_NUMERO, 
                                    ASIENTO_NUMERO_ANU, 
                                    CONCILIA_ID, 
                                    CONCILIA_TIPO, 
                                    CONCILIA_FECHA, 
                                    CONCILIA_USUARIO, 
                                    COD_PLAN, 
                                    MODO_PROTEGIDO, 
                                    REPOSICION_IND, 
                                    REPOSICION_USUARIO, 
                                    REPOSICION_FECHA, 
                                    REPOSICION_AUTORIZA, 
                                    REPOSICION_NOTA, 
                                    CEDULA_ORIGEN as 'CedulaOrigen', 
                                    CTA_IBAN_ORIGEN as 'CuentaOrigen', 
                                    TIPO_CED_ORIGEN as 'tipoCedOrigen', 
                                    CORREO_NOTIFICA as 'CorreoNotifica', 
                                    ESTADO_SINPE as 'estadoSinpe', 
                                    ID_RECHAZO as 'IdMotivoRechazo', 
                                    TIPO_GIROSINPE, 
                                    ID_DESEMBOLSO, 
                                    TIPO_CED_DESTINO as 'tipoCedDestino', 
                                    NOMBRE_ORIGEN as 'NombreOrigen', 
                                    REFERENCIA_SINPE, 
                                    ID_BANCO_DESTINO, 
                                    RAZON_HOLD, 
                                    DOCUMENTO_BANCO, 
                                    FECHA_BANCO, 
                                    REFERENCIA_BANCARIA, 
                                    COD_CONCEPTO_ANULACION, 
                                    VALIDA_SINPE, 
                                    USUARIO_AUTORIZA_ESPECIAL 
                               FROM TES_TRANSACCIONES 
                              where Nsolicitud = @solicitud ";

                response.Result = connection.Query<TesTransaccion>(query, new { solicitud = Nsolicitud }).FirstOrDefault();

                var infoSinpe = fxTesConsultaInfoSinpe(CodEmpresa, Nsolicitud.ToString()).Result;

                if (response.Result is not null)
                {
                    response.Result.Codigo = infoSinpe?.Cedula?.ToString();
                }
            }
            catch
            {
                response.Code = -1;
                response.Description = "Error al consultar solicitud.";
                response.Result = null;
            }
            return response;
        }

        public int ValidoTipoMonedaBCCR(int CodEmpresa, string CuentaIBAN)
        {
            var _parametrosSinpe = GetUriEmpresa(CodEmpresa, "TS");

            var parametros = _parametrosSinpe?.Result;

            ReqBase context = new ReqBase
            {
                HostId = parametros?.vHostPin ?? string.Empty,
                OperationId = OperationId.ToString(),
                ClientIPAddress = parametros?.vIpHost ?? string.Empty,
                CultureCode = "ES-CR",
                UserCode = parametros?.vUsuarioLog ?? string.Empty,
            };

            var servicio = _PIN.IsServiceAvailable((parametros?.UrlCGP_PIN ?? ""), context);
            if (servicio.IsSuccessful)
            {
                ReqAccountInfo accountData = new ReqAccountInfo
                {
                    HostId = context.HostId,
                    OperationId = context.OperationId,
                    ClientIPAddress = context.ClientIPAddress,
                    CultureCode = context.CultureCode,
                    UserCode = context.UserCode,
                    Id = string.Empty,
                    AccountNumber = CuentaIBAN
                };
                var cuentaValida = _PIN.GetAccountInfo((parametros?.UrlCGP_PIN ?? string.Empty), accountData);

                if (cuentaValida.Account == null)
                {
                    return 1;
                }

                return GetCurrencyCodeId(cuentaValida.Account.CurrencyCode!);
            }
            return 1;
        }

        public string ValidoNombreCuenta(int CodEmpresa, int TipoId, string Cedula)
        {
            try
            {
                using var connection = OpenConnection(CodEmpresa);

                if (TipoId == 3)
                {
                    var empresa = connection.QueryFirstOrDefault<dynamic>(
                        "Select DESCRIPCION from CXP_PROVEEDORES WHERE REPLACE(CEDJUR, '-', '') = @cedjur",
                        new { cedjur = Cedula.Replace("-", "") });

                    if (empresa != null)
                    {
                        return empresa.DESCRIPCION;
                    }
                }

                if (Cedula.Length < 9)
                {
                    var socio = connection.QueryFirstOrDefault<dynamic>(
                        "SELECT DESCRIPCION FROM TES_BANCOS S WHERE S.ID_BANCO = @Banco",
                        new { Banco = Cedula });

                    if (socio != null)
                    {
                        return socio.DESCRIPCION;
                    }
                }
            }
            catch
            {
                return "Desconocido en " + Cedula;
            }
            return "";
        }

        public ErrorDto<string> fxTesConsultaMotivo(int CodEmpresa, int idRechazo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"SELECT DESCRIPCION FROM SINPE_MOTIVOS where COD_MOTIVO = @rechazo ";
                response.Result = connection.Query<string>(query, new { rechazo = idRechazo }).FirstOrDefault();
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = "Motivo desconocido.";
            }
            return response;
        }

        public ErrorDto<bool> RegistraCreditoCuenta(int CodEmpresa, int Nsolicitud, ResSendingDynamic resPIN)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"UPDATE TES_TRANSACCIONES SET 
                                    REFERENCIA_SINPE = @refSinpe ,
                                    ID_RECHAZO = @idRechazo ,
                                    ESTADO_SINPE = @estadoSinpe
                                    WHERE Nsolicitud = @solicitud ";
                response.Result = connection.Execute(query, new
                {
                    refSinpe = resPIN.PINSendingResult?.SINPEReference ?? string.Empty,
                    idRechazo = (resPIN.Errors != null && resPIN.Errors.Length > 0) ? resPIN.Errors[0].Code : 0,
                    estadoSinpe = resPIN.PINSendingResult!.State,
                    solicitud = Nsolicitud
                }) > 0;
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al Actualizar Solicitud.";
                response.Result = false;
            }
            return response;
        }

        public ErrorDto<bool> RegistraDibitoCuenta(int CodEmpresa, int Nsolicitud, ResSendingDynamic resPIN)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"UPDATE TES_TRANSACCIONES SET 
                                    REFERENCIA_SINPE = @refSinpe ,
                                    ID_RECHAZO = @idRechazo ,
                                    ESTADO_SINPE = @estadoSinpe
                                    WHERE Nsolicitud = @solicitud ";
                response.Result = connection.Execute(query, new
                {
                    refSinpe = resPIN.DTRSendingResult?.SINPERefNumber ?? string.Empty,
                    idRechazo = (resPIN.Errors != null && resPIN.Errors.Length > 0) ? resPIN.Errors[0].Code : 0,
                    estadoSinpe = resPIN.DTRSendingResult!.State,
                    solicitud = Nsolicitud
                }) > 0;
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al Actualizar Solicitud.";
                response.Result = false;
            }
            return response;
        }

        public ErrorDto<bool> fxTesRespuestaSinpe(int CodEmpresa, TesTransaccion datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            string nDocumento = "";

            try
            {
                using var connection = new SqlConnection(stringConn);
                if (datos.IdMotivoRechazo != 201)
                {
                    nDocumento = (datos.DocumentoBase + "-" + datos.contador.ToString())
                                 .Substring(0, Math.Min(30, (datos.DocumentoBase + "-" + datos.contador.ToString()).Length));


                    var query = $@"Update Tes_Transacciones Set Estado='I',Fecha_Emision= @FECHAEMITE, 
                                    Ubicacion_Actual='T',FECHA_TRASLADO= @FECHATRASLADO, User_Genera = @USUARIO, 
                                    Estado_Sinpe= @ESTADOSINPE, Id_Rechazo= @RECHAZO, Referencia_Sinpe= @REFERENCIA, 
                                    Documento_Base = @DOCBASE, NDocumento = CASE WHEN USUARIO_AUTORIZA_ESPECIAL IS 
                                    NULL THEN @NDOC ELSE @REFERENCIA END where NSolicitud= @SOLICITUD";
                    var result = connection.Execute(query, new
                    {
                        FECHAEMITE = datos.FechaEmision,
                        FECHATRASLADO = datos.FechaTraslado,
                        USUARIO = datos.UsuarioGenera,
                        ESTADOSINPE = datos.estadoSinpe,
                        RECHAZO = datos.IdMotivoRechazo,
                        REFERENCIA = datos.CodigoReferencia,
                        DOCBASE = datos.DocumentoBase,
                        NDOC = nDocumento,
                        SOLICITUD = datos.NumeroSolicitud
                    });
                    if (result > 0)
                    {
                        response.Result = true;
                    }
                }
                else
                {
                    response.Result = false;
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = false;
            }
            return response;
        }

        public bool fxSinpe_Valida_MovimientosPermitidos(int CodEmpresa, string iban)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            const string Query = @"SELECT *
                             FROM dbo.fxSinpe_Valida_MovimientosPermitidos(@iban);";

            var response = conn.QueryFirstOrDefault<dynamic>(Query, new { iban });

            return response != null && response!.TIPO_SINPE == 1 && response!.SINPE_PRODUCTO == 1;
        }

        public void RegistraMovTransito(int CodEmpresa, string cod_referencia, string usuario, ResSendingDynamic resPIN, TesTransaccion solicitud)
        {
            var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            string Query = @"SELECT COUNT(COD_REFERENCIA) existe
                             FROM SINPE_MOV_TRANSITO WHERE COD_REFERENCIA = @referencia";

            var existe = conn.Query<int>(Query, new { referencia = cod_referencia }).FirstOrDefault();

            if (existe > 0)
            {
                UpdateMovTransito(CodEmpresa, cod_referencia, usuario, resPIN, solicitud);
            }
            else
            {
                IngresaMovTransito(CodEmpresa, cod_referencia, usuario, resPIN, solicitud);
            }
        }

        private void IngresaMovTransito(int CodEmpresa, string cod_referencia, string usuario, ResSendingDynamic resPIN, TesTransaccion solicitud)
        {
            const string Query = @"INSERT INTO SINPE_MOV_TRANSITO
                            (
                                COD_TRANSITO,
                                CEDULA,
                                CUENTA_CLIENTE,
                                BANCO_ORIGEN,
                                BANCO_ORIGEN_DESC,
                                CEDULA_ORIGEN,
                                CUENTA_CLIENTE_ORIGEN,
                                COD_REFERENCIA,
                                COD_SERVICIO,
                                COD_MONEDA,
                                MONTO,
                                MONTO_COMISION,
                                ACCION,
                                TRANSAC_TIPO,
                                TRANSAC_DESC,
                                REGISTRO_FECHA,
                                REGISTRO_USUARIO,
                                COMPROBANTE_INTERNO,
                                RECHAZO_CODIGO,
                                RECHAZO_DESC,
                                ESTADO,
                                SERVICIO
                            )
                            VALUES
                            (
                                @CodTransito,
                                @Cedula,
                                @CuentaCliente,
                                @BancoOrigen,
                                @BancoOrigenDesc,
                                @CedulaOrigen,
                                @CuentaClienteOrigen,
                                @CodReferencia,
                                @CodServicio,
                                @CodMoneda,
                                @Monto,
                                @MontoComision,
                                @Accion,
                                @TransacTipo,
                                @TransacDesc,
                                @RegistroFecha,
                                @RegistroUsuario,
                                @ComprobanteInterno,
                                @RechazoCodigo,
                                @RechazoDesc,
                                @Estado,
                                @Servicio
                            );";

            var parametros = BuildMovTransitoParams(CodEmpresa, cod_referencia, usuario, resPIN, solicitud, incluirFechaActualiza: false);
            DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, Query, parametros);
        }

        private void UpdateMovTransito(int CodEmpresa, string cod_referencia, string usuario, ResSendingDynamic resPIN, TesTransaccion solicitud)
        {
            const string Query = @"UPDATE SINPE_MOV_TRANSITO
                                SET
                                    CEDULA               = @Cedula,
                                    CUENTA_CLIENTE       = @CuentaCliente,
                                    BANCO_ORIGEN          = @BancoOrigen,
                                    BANCO_ORIGEN_DESC     = @BancoOrigenDesc,
                                    CEDULA_ORIGEN         = @CedulaOrigen,
                                    CUENTA_CLIENTE_ORIGEN = @CuentaClienteOrigen,
                                    COD_SERVICIO          = @CodServicio,
                                    COD_MONEDA            = @CodMoneda,
                                    MONTO                 = @Monto,
                                    MONTO_COMISION        = @MontoComision,
                                    ACCION                = @Accion,
                                    TRANSAC_TIPO          = @TransacTipo,
                                    TRANSAC_DESC          = @TransacDesc,
                                    COMPROBANTE_INTERNO   = @ComprobanteInterno,
                                    RECHAZO_CODIGO        = @RechazoCodigo,
                                    RECHAZO_DESC          = @RechazoDesc,
                                    ESTADO                = @Estado,
                                    FECHA_ACTUALIZA       = @FechaActualiza,
                                    SERVICIO              = @Servicio
                                WHERE
                                     COD_REFERENCIA = @CodReferencia;";

            var parametros = BuildMovTransitoParams(CodEmpresa, cod_referencia, usuario, resPIN, solicitud, incluirFechaActualiza: true);
            DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, Query, parametros);
        }

        private long ConsecutivoMovTransito(int CodEmpresa)
        {
            string Query = @"SELECT ISNULL(MAX(COD_TRANSITO), 0) + 1 AS SiguienteConsecutivo
                                FROM SINPE_MOV_TRANSITO";

            return DbHelper.ExecuteSingleQuery<long>(_portalDB, CodEmpresa, Query, 0, null).Result;
        }

        #endregion
    }
}
