using CoreInterno;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Controllers.WFCSinpe;
using Microsoft.Data.SqlClient;
using Galileo.Models.KindoSinpe;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Galileo_API.DataBaseTier
{
    public class MKindoServiceDb : IWFCSinpe
    {
        private readonly IConfiguration _config;
        private readonly SinpeGalileo_PIN _PIN;

        private readonly Guid OperationId;

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        

        public MKindoServiceDb(IConfiguration config)
        {
            _config = config;
            _PIN = new SinpeGalileo_PIN(_config);
            OperationId = Guid.NewGuid();
        }

        #region Métodos de integración de uso general

        /// <summary>
        /// Servicio para Implementacion SINPE KINDO
        /// Permite verificar que su Core Financiero esté disponible para atender solicitudes de CGP.
        /// </summary>
        /// <param name="vUsuario"></param>
        /// <returns> Valor que indica si la verificación de la comunicación fue exitosa con un “true” o no con un “false”. </returns>
        public bool ServicioDisponible(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = false;
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = "SELECT COUNT(*) FROM SINPE_PARAMETROS_EMPRESA";
                response = connection.ExecuteScalar<int>(query) > 0;
                if (response)
                {
                    response = true;
                }
                else
                {
                    response = false;
                }
            }
            catch (Exception)
            {
                response = false;
            }
            return response;
        }

        /// <summary>
        /// Este método permite obtener la información básica de una cuenta por medio de su número de cuenta IBAN.
        /// En este momento, la implementación de este método es exclusivo para uso del Servicio PIN del SINPE.
        /// </summary>
        /// <param name="DatosCuenta"> Datos de la cuenta a consultar. </param>
        /// <returns> Objeto con la respuesta de la solicitud de la consulta de la cuenta. </returns>
        public CoreInterno.CuentaIBAN_Response ObtenerCuentaIBAN(int CodEmpresa, CoreInterno.CuentaIBAN_Request DatosCuenta)
        {
            var cuenta = new CoreInterno.CL_CuentaIBAN();
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = $@"exec sp_Sinpe_ObtenerCuentaIBAN @CuentaIBAN";
                var result = connection.QueryFirstOrDefault<dynamic>(query, new { CuentaIBAN = DatosCuenta.CuentaIBAN });

                if (result != null)
                {
                    //Busco el tipo de cedula.
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
                        Moneda = result.Moneda,
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
                else
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
            catch (Exception ex)
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

        /// <summary>
        /// Este método permite obtener información básica de un producto (cuenta, préstamo, tarjeta de crédito, etc.) que se encuentre registrado en su Core Financiero.
        /// Estos datos serían devueltos al SINPE cuando éste lo solicite.
        /// </summary>
        /// <param name="Identificacion"> Número de identificación del titular del producto a consultar. El número se indicará en formato SINPE.</param>
        /// <param name="CuentaIBAN"> Número de cuenta IBAN del producto a consultar. </param>
        /// <returns> Clase con el resultado de los datos del producto. </returns>
        public CoreInterno.CL_ObtieneInfoCuenta ObtieneInfoCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN)
        {
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

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

                    //Busco el tipo de cedula.
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



                CoreInterno.CL_ObtieneInfoCuenta cL_Cuenta = new CoreInterno.CL_ObtieneInfoCuenta
                {
                    Resultado = CoreInterno.E_Resultado.Exitoso,
                    Estado = CoreInterno.E_Estado.Activa,
                    Moneda = result.Moneda,
                    NombreTitular = result.NOMBRE,
                    MotivoError = 0
                };
                return cL_Cuenta;
            }
            catch (Exception ex)
            {
                return new CoreInterno.CL_ObtieneInfoCuenta
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    Estado = CoreInterno.E_Estado.NoActiva,
                    MotivoError = 28,
                };
            }
        }

        /// <summary>
        /// Este método permite validar los datos de un producto (cuenta, tarjeta de crédito, préstamo, etc.).
        /// Las validaciones -no limitadas a estas- que deben ser aplicadas por su Core Financiero son:
        /// </summary>
        /// <param name="Identificacion"> Indica Identificación del titular del producto en formato SINPE. </param>
        /// <param name="CuentaIBAN"> Número de la cuenta IBAN asociado al producto</param>
        /// <param name="CodigoMoneda"> Código de la moneda del producto, de acuerdo con la codificación 1. Colones, 2 Dolares, 3 Euros.</param>
        /// <returns> Clase con el resultado de la validación de la cuenta cliente solicitada. </returns>
        public CoreInterno.CL_ValidaCuenta ValidaCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN, int? CodigoMoneda)
        {
            var resultado = new CoreInterno.CL_ValidaCuenta();
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string tipo = CuentaIBAN.Substring(8, 2); // extrae los dígitos 9-10

                int tipoMovimiento;

                if (tipo == "01")
                    tipoMovimiento = 1;     // Fondo
                else if (tipo == "02")
                    tipoMovimiento = 2;    // Crédito
                else
                    tipoMovimiento = 1;

                var query = $@"SELECT dbo.fxSINPE_ValidaCuenta(@CUENTA, @TRANSAC_TIPO, @CEDULA , @MONEDA)";

                var valida = connection.QueryFirstOrDefault<int>(query, new
                {
                    CUENTA = CuentaIBAN,
                    TRANSAC_TIPO = tipoMovimiento,
                    CEDULA = Identificacion.Replace("-", "").Replace(" ", ""),
                    MONEDA = CodigoMoneda
                });

                resultado.MotivoError = valida;

                if (valida == 0)
                {
                    resultado.Resultado = CoreInterno.E_Resultado.Exitoso;
                }
                else
                {
                    resultado.Resultado = CoreInterno.E_Resultado.Error;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return new CoreInterno.CL_ValidaCuenta
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    MotivoError = 28
                };
            }
        }

        /// <summary>
        /// Este método permite obtener el tipo del cambio definido a nivel del sistema interno para un servicio específico.
        /// Si su Entidad Financiera no realiza compra/venta de divisas, el tipo de cambio a retornar cuando se invoque a este método debe ser el valor uno (1) y el monto el mismo monto que se envió en la solicitud.
        /// </summary>
        /// <param name="Rastro"> Datos relacionados con el usuario que realiza la petición. </param>
        /// <param name="CodigoServicio"> Código servicio que asociado a la transacción</param>
        /// <param name="Cuentaorigen"> Cuenta Origen asociada a la transacción. Pertenece a su Entidad Financiera.</param>
        /// <param name="CuentaDestino"> Cuenta destino de la transacción. Pertenece a la Entidad Financiera Destino.</param>
        /// <param name="Monto"> Moneda de la cuenta destino de la transacción.</param>
        /// <param name="Moneda"> Monto de la transacción.</param>
        /// <returns> Resultado de la ejecución de la invocación al método tipo de cambio, incluye el monto relacionado con el tipo de cambio del sistema interno. </returns>
        public CoreInterno.CL_ResultadoTipoCambio ObtenerTipoCambio(int CodEmpresa, CoreInterno.SI_Rastro? Rastro, int? CodigoServicio, string? Cuentaorigen, string? CuentaDestino, decimal? Monto, int? Moneda)
        {
            try
            {
                if (Moneda == null)
                {
                    Moneda = 2; //Por defecto Dolares
                }
                var divisa = GetCurrencyKindoCode(Moneda);
                if (Monto == null)
                {
                    Monto = 1;
                }

                if (divisa == "COL")
                {

                    return new CoreInterno.CL_ResultadoTipoCambio
                    {
                        montoTotal = Math.Round(Monto.Value, 2),
                        TipoCambioAplicado = 1
                    };
                }

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var tc = $@"exec dbo.sp_Sinpe_ObtenerTipoCambio @CODIGO_REFERENCIA";
                var tipoCambio = connection.QueryFirstOrDefault<decimal>(tc, new { CODIGO_REFERENCIA = Cuentaorigen });

                return new CoreInterno.CL_ResultadoTipoCambio
                {
                    montoTotal = Math.Round(Monto.Value * tipoCambio, 2),
                    TipoCambioAplicado = Math.Round(tipoCambio, 2)
                };
            }
            catch (Exception ex)
            {
                return new CoreInterno.CL_ResultadoTipoCambio
                {
                    montoTotal = Math.Round(Monto.Value, 2),
                    TipoCambioAplicado = 1
                };
            }
        }

        /// <summary>
        /// Servicio para Implementacion SINPE KINDO
        /// Este método permite obtener la comisión que su Entidad cobra a un cliente por el envío de una transacción SINPE.
        /// </summary>
        /// <param name="request"> Indica los datos necesarios para la petición. </param>
        /// <returns> Indicador del resultado global de la ejecución. </returns>
        public CoreInterno.ComisionRespectivaResponse ComisionRespectiva(int CodEmpresa, CoreInterno.ComisionRespectivaRequest request)
        {
            var resultado = new CoreInterno.ComisionRespectivaResponse();
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

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
            catch (Exception ex)
            {
                resultado.comision = 0;
                resultado.codigoMonedaComision = request.codigoMoneda;
                resultado.ComisionRespectivaResult = CoreInterno.E_Resultado.Error;
            }
            return resultado;
        }

        /// <summary>
        /// Servicio para Implementacion SINPE KINDO
        /// Este método permite validar que un producto (cuenta, tarjeta de crédito, préstamo, etc.) sea correcto para aplicar un movimiento de débito.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public CoreInterno.CL_ResultadoValidacion[] ValidaDebitos(int CodEmpresa, ValidaTransRequest request)
        {
            var resultado = new CoreInterno.CL_ResultadoValidacion[] { };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var transaccion in request.Transacciones)
                {
                    var query = $@"SELECT * FROM dbo.fxSinpe_ValidaDebito(
                                    @IDENTIFICACION,
                                    @CUENTAIBAN,
                                    @CODIGO_MONEDA,
                                    @CODIGO_SERVICIO,
                                    @MONTO,
                                    NULL
                                )";

                    var valida = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        IDENTIFICACION = transaccion.Identificacion.Trim().Replace("-", ""),
                        CUENTAIBAN = transaccion.CuentaIBAN,
                        CODIGO_MONEDA = transaccion.CodigoMoneda,
                        CODIGO_SERVICIO = transaccion.CodigoServicio,
                        MONTO = transaccion.Monto
                    });

                    if (valida.CODIGO_ERROR > 0)
                    {
                        resultado = resultado.Append(new CoreInterno.CL_ResultadoValidacion
                        {
                            Resultado = CoreInterno.E_Resultado.Error,
                            MotivoError = valida.CODIGO_ERROR,
                            InformacionAdicional = new CL_Adicional_Info[]{
                                new CL_Adicional_Info(){
                                     Mostrar = true,
                                     Nombre = "PgrX",
                                     NombreFisico = "Galileo",
                                     Valor = valida.DETALLE
                                }
                            }
                        }).ToArray();
                    }
                    else
                    {
                        resultado = resultado.Append(new CoreInterno.CL_ResultadoValidacion
                        {
                            Resultado = CoreInterno.E_Resultado.Exitoso,
                            MotivoError = 0
                        }).ToArray();
                    }

                }

                return resultado;
            }
            catch (Exception ex)
            {
                return new CoreInterno.CL_ResultadoValidacion[]
                {
                    new CL_ResultadoValidacion
                    {
                        Resultado = CoreInterno.E_Resultado.Error,
                        MotivoError = -1,
                    }
                };
            }
        }

        /// <summary>
        /// Este método permite validar que un producto (cuenta, tarjeta de crédito, préstamo, etc.) sea correcto para aplicar un movimiento de crédito.
        /// Las validaciones -no limitadas a estas- que deben ser aplicadas por su Core Financiero son:
        /// </summary>
        /// <param name="Rastro"></param>
        /// <param name="Creditos"></param>
        /// <returns></returns>
        public CoreInterno.CL_ResultadoValidacion[] ValidaCreditos(int CodEmpresa, ValidaTransRequest request)
        {
            var resultado = new CoreInterno.CL_ResultadoValidacion[] { };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var transaccion in request.Transacciones)
                {
                    var query = $@"SELECT * FROM dbo.fxSinpe_ValidaCredito(
                                    @IDENTIFICACION,
                                    @CUENTAIBAN,
                                    @CODIGO_MONEDA,
                                    @CODIGO_SERVICIO,
                                    @MONTO,
                                    NULL
                                )";

                    var valida = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        IDENTIFICACION = transaccion.Identificacion,
                        CUENTAIBAN = transaccion.CuentaIBAN,
                        CODIGO_MONEDA = transaccion.CodigoMoneda,
                        CODIGO_SERVICIO = transaccion.CodigoServicio,
                        MONTO = transaccion.Monto
                    });

                    if (valida.CODIGO_ERROR > 0)
                    {
                        resultado = resultado.Append(new CoreInterno.CL_ResultadoValidacion
                        {
                            Resultado = CoreInterno.E_Resultado.Error,
                            MotivoError = valida.CODIGO_ERROR,
                            InformacionAdicional = new CL_Adicional_Info[]{
                                new CL_Adicional_Info(){
                                     Mostrar = true,
                                     Nombre = "PgrX",
                                     NombreFisico = "Galileo",
                                     Valor = valida.DETALLE
                                }
                            }
                        }).ToArray();
                    }
                    else
                    {
                        resultado = resultado.Append(new CoreInterno.CL_ResultadoValidacion
                        {
                            Resultado = CoreInterno.E_Resultado.Exitoso,
                            MotivoError = 0
                        }).ToArray();
                    }

                }

                return resultado;
            }
            catch (Exception ex)
            {
                return new CoreInterno.CL_ResultadoValidacion[]
                {
                    new CL_ResultadoValidacion
                    {
                        Resultado = CoreInterno.E_Resultado.Error,
                        MotivoError = -1,
                    }
                };
            }
        }

        /// <summary>
        /// Este método permite validar y autorizar/rechazar el procesamiento de una transacción saliente o entrante para un cliente.
        /// En este momento, la implementación de este método es exclusivo para uso del Servicio PIN del SINPE.
        /// ** Si su Entidad no requiere hacer validaciones especiales sobre el perfil del cliente entonces prográmelo de tal forma que siempre responda que la transacción está autorizada. **
        /// </summary>
        /// <param name="transaccion"> Datos de la transacción a autorizar o rechazar. </param>
        /// <returns> Objeto con la respuesta de la solicitud de validación del perfil transaccional. </returns>
        public CoreInterno.ValidacionPerfilTrx_Response ValidarPerfilTransaccional(int CodEmpresa, CoreInterno.ValidacionPerfilTrx_Request transaccion)
        {
            try
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
            catch (Exception ex)
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
        }

        #endregion

        #region Métodos para la integración transaccional

        /// <summary>
        /// Este método permite la aplicación de un débito en estado “congelado” 
        /// (congelación o reserva de fondos) a una cuenta IBAN.
        /// </summary>
        /// <param name="Rastro"> Objeto con la información de rastreo de la transacción. </param>
        /// <param name="Debitos"> Arreglo de transacciones (débitos congelados) a aplicar. </param>
        /// <returns> Objeto con la respuesta del procesamiento de los débitos. </returns>
        public CoreInterno.CL_RespuestaTransaccion[] AplicaDebitosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_Transaccion[] Debitos)
        {
            var resultado = new CoreInterno.CL_RespuestaTransaccion[] { };
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var solicitud in Debitos)
                {
                    var transaccion = fxTesConsultaSolicitud(CodEmpresa, Convert.ToInt32(solicitud.CodigoReferencia));

                    var query = $@"exec sp_Sinpe_AplicaDebitosCongelados  
                                                 @CENTRO_COSTO
		                                        ,@COD_ENTIDAD 
		                                        ,@CODIGO_REFERENCIA 
		                                        ,@COMPROBANTE_CGP
		                                        ,@CUENTA_IBAN_CONTRAPARTE 
		                                        ,@DESCRIPCION
		                                        ,@FECHA_CICLO 
		                                        ,@ID_ORIGEN 
		                                        ,@SERVICIO 
		                                        ,@MONEDA_COMISION 
		                                        ,@MONTO_COMISION 
		                                        ,@NOMBRE_ORIGEN  
		                                        ,@IDENTIFICACION_CONTRAPARTE 
		                                        ,@IDENTIFICACION
		                                        ,@CUENTA_IBAN 
		                                        ,@MONTO 
		                                        ,@CODIGO_MONEDA 
		                                        ,@CODIGO_SERVICIO 
		                                        ,@IDRELACIONCLIENTE
		                                        ,@CANAL  -- 'SINPE' O 'CGP'
		                                        ,@REGISTRO_USUARIO 
                                                ,@COD_EMPRESA";

                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        CENTRO_COSTO = solicitud.CentroCosto,
                        COD_ENTIDAD = solicitud.CodEntidad,
                        CODIGO_REFERENCIA = solicitud.CodigoReferencia,
                        COMPROBANTE_CGP = solicitud.ComprobanteCGP,
                        CUENTA_IBAN_CONTRAPARTE = solicitud.CuentaIBANContraparte,
                        DESCRIPCION = solicitud.Descripcion,
                        FECHA_CICLO = solicitud.FechaCiclo,
                        ID_ORIGEN = solicitud.IdOrigen,
                        SERVICIO = solicitud.Servicio,
                        MONEDA_COMISION = solicitud.MonedaComision,
                        MONTO_COMISION = solicitud.MonedaComision,
                        NOMBRE_ORIGEN = solicitud.NombreOrigen,
                        IDENTIFICACION_CONTRAPARTE = solicitud.IdentificacionContraparte,
                        IDENTIFICACION = solicitud.Identificacion,
                        CUENTA_IBAN = solicitud.CuentaIBAN,
                        MONTO = solicitud.Monto,
                        CODIGO_MONEDA = solicitud.CodigoMoneda,
                        CODIGO_SERVICIO = solicitud.CodigoServicio,
                        IDRELACIONCLIENTE = solicitud.IdRelacionCliente,
                        CANAL = "SINPE",
                        REGISTRO_USUARIO = transaccion.Result.UsuarioGenera,
                        COD_EMPRESA = CodEmpresa
                    });


                    if (result.MOT_RECHAZO > 0)
                    {
                        resultado = resultado.Append(new CoreInterno.CL_RespuestaTransaccion
                        {
                            Resultado = CoreInterno.E_Resultado.Rechazo,
                            MotivoError = result.MOT_RECHAZO,
                            ComprobanteInterno = result.ID_REFERENCIA
                        }).ToArray();
                    }
                    else
                    {
                        resultado = resultado.Append(new CoreInterno.CL_RespuestaTransaccion
                        {
                            Resultado = CoreInterno.E_Resultado.Exitoso,
                            MotivoError = 0,
                            ComprobanteInterno = result.ID_REFERENCIA
                        }).ToArray();
                    }
                }

            }
            catch (Exception ex)
            {
                resultado = resultado.Append(new CoreInterno.CL_RespuestaTransaccion
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    MotivoError = -1,
                    ComprobanteInterno = ""
                }).ToArray();
            }
            return resultado;
        }


        /// <summary>
        /// Este método permite la aplicación de un crédito en estado “congelado” 
        /// (congelación o reserva de fondos) a una cuenta IBAN.
        /// </summary>
        /// <param name="Rastro"> Objeto con la información de rastreo de la transacción. </param>
        /// <param name="Creditos"> Arreglo de transacciones (créditos congelados) a aplicar. </param>
        /// <returns> Objeto con la respuesta del procesamiento de los créditos. </returns>
        public CoreInterno.CL_RespuestaTransaccion[] AplicaCreditosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_Transaccion[] Creditos)
        {
            var resultado = new CoreInterno.CL_RespuestaTransaccion[] { };
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var solicitud in Creditos)
                {
                    var transaccion = fxTesConsultaSolicitud(CodEmpresa, Convert.ToInt32(solicitud.CodigoReferencia));

                    var query = $@"exec sp_Sinpe_AplicaCreditosCongelados  
                                                 @CENTRO_COSTO
		                                        ,@COD_ENTIDAD 
		                                        ,@CODIGO_REFERENCIA 
		                                        ,@COMPROBANTE_CGP
		                                        ,@CUENTA_IBAN_CONTRAPARTE 
		                                        ,@DESCRIPCION
		                                        ,@FECHA_CICLO 
		                                        ,@ID_ORIGEN 
		                                        ,@SERVICIO 
		                                        ,@MONEDA_COMISION 
		                                        ,@MONTO_COMISION 
		                                        ,@NOMBRE_ORIGEN  
		                                        ,@IDENTIFICACION_CONTRAPARTE 
		                                        ,@IDENTIFICACION
		                                        ,@CUENTA_IBAN 
		                                        ,@MONTO 
		                                        ,@CODIGO_MONEDA 
		                                        ,@CODIGO_SERVICIO 
		                                        ,@IDRELACIONCLIENTE
		                                        ,@CANAL  -- 'SINPE' O 'CGP'
		                                        ,@REGISTRO_USUARIO 
                                                ,@COD_EMPRESA";

                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        CENTRO_COSTO = solicitud.CentroCosto,
                        COD_ENTIDAD = solicitud.CodEntidad,
                        CODIGO_REFERENCIA = solicitud.CodigoReferencia,
                        COMPROBANTE_CGP = solicitud.ComprobanteCGP,
                        CUENTA_IBAN_CONTRAPARTE = solicitud.CuentaIBANContraparte,
                        DESCRIPCION = solicitud.Descripcion,
                        FECHA_CICLO = solicitud.FechaCiclo,
                        ID_ORIGEN = solicitud.IdOrigen,
                        SERVICIO = solicitud.Servicio,
                        MONEDA_COMISION = solicitud.MonedaComision,
                        MONTO_COMISION = solicitud.MonedaComision,
                        NOMBRE_ORIGEN = solicitud.NombreOrigen,
                        IDENTIFICACION_CONTRAPARTE = solicitud.IdentificacionContraparte,
                        IDENTIFICACION = solicitud.Identificacion,
                        CUENTA_IBAN = solicitud.CuentaIBAN,
                        MONTO = solicitud.Monto,
                        CODIGO_MONEDA = solicitud.CodigoMoneda,
                        CODIGO_SERVICIO = solicitud.CodigoServicio,
                        IDRELACIONCLIENTE = solicitud.IdRelacionCliente,
                        CANAL = "SINPE",
                        REGISTRO_USUARIO = transaccion.Result.UsuarioGenera,
                        COD_EMPRESA = CodEmpresa
                    });


                    if (result.MOT_RECHAZO > 0)
                    {
                        resultado = resultado.Append(new CoreInterno.CL_RespuestaTransaccion
                        {
                            Resultado = CoreInterno.E_Resultado.Rechazo,
                            MotivoError = result.MOT_RECHAZO,
                            ComprobanteInterno = result.ID_REFERENCIA
                        }).ToArray();
                    }
                    else
                    {
                        resultado = resultado.Append(new CoreInterno.CL_RespuestaTransaccion
                        {
                            Resultado = CoreInterno.E_Resultado.Exitoso,
                            MotivoError = 0,
                            ComprobanteInterno = result.ID_REFERENCIA
                        }).ToArray();
                    }
                }

            }
            catch (Exception ex)
            {
                resultado = resultado.Append(new CoreInterno.CL_RespuestaTransaccion
                {
                    Resultado = CoreInterno.E_Resultado.Error,
                    MotivoError = -1,
                    ComprobanteInterno = ""
                }).ToArray();
            }
            return resultado;
        }

        /// <summary>
        /// Este método permite aplicar en firme un movimiento crédito que se congeló 
        /// previamente en una solicitud de crédito congelado.
        /// </summary>
        /// <param name="Rastro"> Datos relacionados con el usuario que realiza la petición. </param>
        /// <param name="Transacciones"> Arreglo de clases de tipo CL_ActualizaTransaccion con 
        /// todas las operaciones de crédito a confirmar en el sistema. </param>
        /// <returns> Objeto con el resultado de la aplicación de los créditos. </returns>
        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaCreditosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_ActualizaTransaccion[] Transacciones)
        {
            var resultado = new CoreInterno.CL_ResultadoActualizacion[] { };
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var solicitud in Transacciones)
                {
                    var query = $@"exec sp_Sinpe_ConfirmaCreditoCongelado 
                                                @CODIGO_RECHAZO_SINPE ,
	                                            @CODIGO_REFERENCIA , 
	                                            @COMPTOBANTE_CGP ,
	                                            @COMPROBANTE_INTERNO , 	
	                                            @DESCRIPCION_RECHAZO ";

                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        CODIGO_RECHAZO_SINPE = solicitud.CodigoRechazoSINPE,
                        CODIGO_REFERENCIA = solicitud.CodigoReferencia,
                        COMPTOBANTE_CGP = solicitud.ComprobanteCGP,
                        COMPROBANTE_INTERNO = solicitud.ComprobanteInterno,
                        DESCRIPCION_RECHAZO = solicitud.DescripcionRechazo
                    });


                    resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                    {
                        Resultado = result.Resultado,
                        IdRelacionCliente = solicitud.IdRelacionCliente
                    }).ToArray();
                }

            }
            catch (Exception ex)
            {
                resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = CoreInterno.E_ResultadoActualizacion.Error,
                    IdRelacionCliente = null
                }).ToArray();
            }
            return resultado;
        }

        /// <summary>
        /// Este método permite confirmar un movimiento débito que se congeló 
        /// previamente en una solicitud de aplicación de débito congelado.
        /// </summary>
        /// <param name="Rastro"> Datos relacionados con el usuario que realiza la petición. </param>
        /// <param name="Transacciones"> Arreglo de clases de tipo CL_ActualizaTransaccion con 
        /// todas las operaciones de débito a confirmar en el sistema. </param>
        /// <returns> Objeto con el resultado de la aplicación de los débitos. </returns>
        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaDebitosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_ActualizaTransaccion[] Transacciones)
        {
            var resultado = new CoreInterno.CL_ResultadoActualizacion[] { };
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var solicitud in Transacciones)
                {
                    var query = $@"exec sp_Sinpe_ConfirmaDebitoCongelado 
                                                @CODIGO_RECHAZO_SINPE ,
	                                            @CODIGO_REFERENCIA , 
	                                            @COMPTOBANTE_CGP ,
	                                            @COMPROBANTE_INTERNO , 	
	                                            @DESCRIPCION_RECHAZO ";

                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        CODIGO_RECHAZO_SINPE = solicitud.CodigoRechazoSINPE,
                        CODIGO_REFERENCIA = solicitud.CodigoReferencia,
                        COMPTOBANTE_CGP = solicitud.ComprobanteCGP,
                        COMPROBANTE_INTERNO = solicitud.ComprobanteInterno,
                        DESCRIPCION_RECHAZO = solicitud.DescripcionRechazo
                    });


                    resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                    {
                        Resultado = result.Resultado,
                        IdRelacionCliente = solicitud.IdRelacionCliente
                    }).ToArray();
                }

            }
            catch (Exception ex)
            {
                resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = CoreInterno.E_ResultadoActualizacion.Error,
                    IdRelacionCliente = null
                }).ToArray();
            }
            return resultado;
        }

        /// <summary>
        /// Este método permite reversar un movimiento de crédito que se encuentra en 
        /// estado congelado o que ya fue aplicado en firme.
        /// </summary>
        /// <param name="Rastro"> Datos relacionados con el usuario que realiza la petición. </param>
        /// <param name="Transacciones"> Arreglo de clases de tipo TransaccionRechazada con 
        /// todas las operaciones de crédito a reversar. </param>
        /// <returns> Objeto con el resultado de la reversión de los créditos. </returns>
        public CoreInterno.CL_ResultadoActualizacion[] ReversaCreditos(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.TransaccionRechazada[] Transacciones)
        {
            var resultado = new CoreInterno.CL_ResultadoActualizacion[] { };
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var solicitud in Transacciones)
                {
                    var query = $@"exec sp_Sinpe_ReversaCreditos 
                                                @CODIGO_RECHAZO_SINPE ,
	                                            @CODIGO_REFERENCIA , 
	                                            @COMPTOBANTE_CGP ,
	                                            @COMPROBANTE_INTERNO , 	
	                                            @DESCRIPCION_RECHAZO ";

                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        CODIGO_RECHAZO_SINPE = solicitud.CodigoRechazoSINPE,
                        CODIGO_REFERENCIA = solicitud.CodigoReferencia,
                        COMPTOBANTE_CGP = solicitud.ComprobanteCGP,
                        COMPROBANTE_INTERNO = solicitud.ComprobanteInterno,
                        DESCRIPCION_RECHAZO = solicitud.DescripcionRechazo
                    });


                    resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                    {
                        Resultado = result.Resultado,
                        IdRelacionCliente = solicitud.IdRelacionCliente
                    }).ToArray();
                }

            }
            catch (Exception ex)
            {
                resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = CoreInterno.E_ResultadoActualizacion.Error,
                    IdRelacionCliente = null
                }).ToArray();
            }
            return resultado;
        }

        /// <summary>
        /// Este método permite reversar un movimiento de débito que se encuentra en 
        /// estado congelado o que ya fue aplicado en firme.
        /// </summary>
        /// <param name="Rastro"> Datos relacionados con el usuario que realiza la petición. </param>
        /// <param name="Transacciones"> Arreglo de clases de tipo TransaccionRechazada con 
        /// todas las operaciones de débito a reversar. </param>
        /// <returns> Objeto con el resultado de la reversión de los débitos. </returns>
        public CoreInterno.CL_ResultadoActualizacion[] ReversaDebitos(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.TransaccionRechazada[] Transacciones)
        {
            var resultado = new CoreInterno.CL_ResultadoActualizacion[] { };
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                foreach (var solicitud in Transacciones)
                {
                    var query = $@"exec sp_Sinpe_ReversaDebitos
                                                @CODIGO_RECHAZO_SINPE ,
	                                            @CODIGO_REFERENCIA , 
	                                            @COMPTOBANTE_CGP ,
	                                            @COMPROBANTE_INTERNO , 	
	                                            @DESCRIPCION_RECHAZO ";

                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        CODIGO_RECHAZO_SINPE = solicitud.CodigoRechazoSINPE,
                        CODIGO_REFERENCIA = solicitud.CodigoReferencia,
                        COMPTOBANTE_CGP = solicitud.ComprobanteCGP,
                        COMPROBANTE_INTERNO = solicitud.ComprobanteInterno,
                        DESCRIPCION_RECHAZO = solicitud.DescripcionRechazo
                    });


                    resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                    {
                        Resultado = result.Resultado,
                        IdRelacionCliente = solicitud.IdRelacionCliente
                    }).ToArray();
                }

            }
            catch (Exception ex)
            {
                resultado = resultado.Append(new CoreInterno.CL_ResultadoActualizacion
                {
                    Resultado = CoreInterno.E_ResultadoActualizacion.Error,
                    IdRelacionCliente = null
                }).ToArray();
            }
            return resultado;
        }

        /// <summary>
        /// Este método permite conocer si un movimiento de fondos que previamente fue 
        /// solicitado por medio de una congelación existe en su Core Financiero y obtener su estado.
        /// </summary>
        /// <param name="Request"> Objeto con el Código de referencia SINPE como llave para ubicar la transacción. </param>
        /// <returns> Objeto que indica si la transacción fue encontrada o no, y su estado. </returns>
        public CoreInterno.ObtieneEstadoTransaccionResponse ObtieneEstadoTransaccion(int CodEmpresa, CoreInterno.ObtieneEstadoTransaccionRequest Request)
        {
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

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
                else
                {
                    return new ObtieneEstadoTransaccionResponse
                    {
                        ComprobanteInterno = null,
                        ObtieneEstadoTransaccionResult = false
                    };
                }
            }
            catch (Exception ex)
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
        /// <summary>
        /// Este método permite la actualización de la fecha de ciclo para una transacción particular.
        /// Se utiliza para permitir que una transacción que falló en su ciclo original pueda 
        /// ser enviada en un ciclo posterior.
        /// </summary>
        /// <param name="ComprobanteCGP"> Número único con que CGP identifica la transacción. </param>
        /// <param name="DocumentoSistemaInterno"> Número de documento generado a la transacción por su Sistema Interno. </param>
        /// <param name="ServicioSINPE"> Código de servicio SINPE de la transacción. </param>
        /// <param name="FechaCiclo"> Fecha de ciclo a liquidar. </param>
        /// <param name="CodigoReferenciaAnterior"> Código de referencia generado anteriormente. </param>
        /// <param name="CodigoReferenciaNuevo"> Código de referencia generado. </param>
        /// <returns> Objeto con un Boolean que indica si la actualización se realizó correctamente. </returns>
        public static bool ActualizarFechaCiclo(int CodEmpresa, CL_ActualizaFechaRequest request)
        {
            try
            {
                // NOTA: En la implementación real, la lógica del Core debe buscar la transacción
                // usando el ComprobanteCGP o el DocumentoSistemaInterno y luego actualizar:
                // 1. La Fecha de Ciclo con el nuevo valor.
                // 2. El Código de Referencia, si es necesario.

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        /// <summary>
        /// Este método permite liquidar todas las transacciones congeladas que pertenecen 
        /// a un servicio y fecha de ciclo determinado, excluyendo a las entidades aplazadas.
        /// </summary>
        /// <param name="EntidadesAplazadas"> Lista de entidades que deben permanecer con transacciones congeladas. </param>
        /// <param name="ServicioSINPE"> Código de servicio a liquidar (ej: 31, 32). </param>
        /// <param name="Modalidad"> Modalidad del servicio (S=Saliente, E = Entrante). </param>
        /// <param name="FechaCiclo"> Fecha de ciclo hasta la cual se deben liquidar las transacciones. </param>
        /// <returns> Objeto con un Boolean que indica si la liquidación de las transacciones fue correcta. </returns>
        public static bool LiquidarCiclo(int CodEmpresa, CLCierraCiclo request)
        {
            try
            {
                // NOTA: La lógica del Core Financiero debe realizar una consulta y actualización masiva:

                // 1. **Selección:** Identificar todas las transacciones en estado "Congelado" que cumplen con:
                //    * ServicioSINPE = valor_suministrado.
                //    * Fecha de Ciclo <= FechaCiclo_suministrada.
                // 2. **Exclusión:** Excluir de la selección a todas las transacciones cuya Entidad 
                //    de origen/destino se encuentre en la lista 'EntidadesAplazadas'.
                // 3. **Liquidación:** Cambiar el estado de las transacciones seleccionadas de "Congelado" 
                //    a "Aplicado" o "Liquidado".

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Métodos para la integración del PortalCGP

        /// <summary>
        /// Este método permite la validación de fondos disponibles para una determinada transacción. 
        /// Incluye validaciones adicionales como la comisión y límites transaccionales.
        /// </summary>
        /// <param name="Request"> Objeto con los datos de la petición, incluyendo monto, cuenta y servicio. </param>
        /// <returns> Objeto que indica el resultado global de la validación de saldo. </returns>
        public CoreInterno.SaldoDisponibleResponse SaldoDisponible(int CodEmpresa, CoreInterno.SaldoDisponibleRequest Request)
        {
            var resultado = new CoreInterno.SaldoDisponibleResponse();
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

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

                bool disponible = Convert.ToBoolean(result.SaldoDisponible);

                resultado.disponible = disponible;
                if (disponible)
                {
                    resultado.SaldoDisponibleResult = CoreInterno.E_Resultado.Exitoso;
                }
                else
                {
                    resultado.SaldoDisponibleResult = CoreInterno.E_Resultado.Rechazo;
                }
            }
            catch (Exception ex)
            {
                resultado = new CoreInterno.SaldoDisponibleResponse
                {
                    disponible = false,
                    SaldoDisponibleResult = CoreInterno.E_Resultado.Error
                };
            }
            return resultado;
        }

        /// <summary>
        /// Este método permite obtener la información de un cliente registrado en su Core Financiero.
        /// </summary>
        /// <param name="request"> Objeto con los datos relacionados con la petición de la información del cliente (ej: identificación). </param>
        /// <returns> Objeto que indica el resultado global de la ejecución y contiene la información del cliente. </returns>
        public CoreInterno.ObtenerInformacionClienteResponse ObtenerInformacionCliente(
             int CodEmpresa,
             CoreInterno.ObtenerInformacionClienteRequest request)
        {
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                const string query = @"exec sp_Sinpe_ObtenerInformacionCliente @Identificacion";

                var result = connection.QueryFirstOrDefault<dynamic>(query, new
                {
                    Identificacion = request.identificacion,
                });

                // por seguridad ante null
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
            catch (Exception)
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

        /// <summary>
        /// Este método permite obtener todos los productos financieros (cuentas, préstamos, tarjetas, etc.) 
        /// que un cliente determinado tiene registrados en el Core de su Entidad Financiera.
        /// </summary>
        /// <param name="request"> Objeto con los datos relacionados con la petición de productos por cliente (ej: identificación del cliente). </param>
        /// <returns> Objeto que indica el resultado global de la ejecución y contiene la lista de productos del cliente. </returns>
        public CoreInterno.ObtenerProductosPorClienteResponse ObtenerProductosPorCliente(int CodEmpresa, CoreInterno.ObtenerProductosPorClienteRequest request)
        {
            var resultado = new CoreInterno.ObtenerProductosPorClienteResponse();
            try
            {

                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

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
                    resultado.ObtenerProductosPorClienteResult = CoreInterno.E_Resultado.Exitoso;
                    resultado.productos = Array.Empty<CoreInterno.CL_ProductoCliente>();

                    foreach (var prod in result)
                    {
                        if (string.IsNullOrEmpty(prod.CuentaCliente))
                        {
                            continue;
                        }

                        if (prod.SINPE_PRODUCTO == 1)
                        {
                            resultado.productos = resultado.productos.Append(new CoreInterno.CL_ProductoCliente
                            {
                                DescripcionServicio = prod.DescripcionServicio,
                                Cuota = prod.Cuota,
                                CodigoMoneda = prod.MonedaServicio,
                                Saldo = prod.Saldo,
                                CuentaCliente = prod.CuentaCliente,
                                MovimientosPermitidos = CoreInterno.E_MovimientosPermitidosProducto.Ambos,

                            }).ToArray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.ObtenerProductosPorClienteResult = CoreInterno.E_Resultado.Error;
                resultado.productos = null;
            }
            return resultado;
        }

        #endregion

        #region Validacion formatos
        /// <summary>
        /// Valida si una fecha cumple con el formato ISO 8601.
        /// Ejemplo válido: 2025-11-19T15:30:00Z
        /// </summary>
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

        /// <summary>
        /// Valida si el código de moneda cumple con el estándar ISO 4217
        /// o con los valores definidos en la sección 8.2 del documento KINDO.
        /// (Ejemplo: CRC, USD, EUR)
        /// </summary>
        public static string GetCurrencyKindoCode(int? currencyCode = 2)
        {
            return currencyCode switch
            {
                1 => "COL", // Colones
                2 => "DOL", // Dólares
                3 => "EU",  // Euros
                _ => null   // Código no reconocido
            };
        }

        /// <summary>
        /// Mapea un código de moneda (ISO 4217 o alias) al ID interno utilizado en el Core Financiero.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public static int GetCurrencyCodeId(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                return 0;

            currency = currency.Trim().ToUpper();

            // Alias aceptados → código ISO o interno
            var aliases = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "COL", 1 },   // Colones = 1
                    { "CRC", 1 },   // ISO oficial también válido

                    { "DOL", 2 },   // Dólares = 2
                    { "USD", 2 },   // ISO oficial también válido

                    { "EU",  3 },   // Euros = 3
                    { "EUR", 3 }    // ISO oficial también válido
                };

            // Si existe en alias, devolver el ID directamente
            if (aliases.TryGetValue(currency, out var idFromAlias))
                return idFromAlias;

            // No coincide con nada
            return 0;
        }


        /// <summary>
        /// Valida si una cuenta IBAN cumple con el formato oficial del Banco Central de Costa Rica.
        /// Formato: CR + 2 dígitos de control + 18 dígitos (total 22 caracteres)
        /// </summary>
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

        /// <summary>
        /// Valida un número de identificación costarricense (físico o jurídico)
        /// según los estándares del BCCR.
        /// </summary>
        public static bool IsValidCostaRicaId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            id = id.Replace("-", "").Trim();

            // 9 (física), 10 (jurídica), 11-12 (DIMEX)
            return Regex.IsMatch(id, @"^\d{9}$|^\d{10}$|^\d{11,12}$",
                RegexOptions.CultureInvariant, RegexTimeout);
        }

        /// <summary>
        /// Valida si un número de transacción o lote cumple con el estándar AAAAMMDDSSSSSNNNNNNNNNNNN.
        /// 25 dígitos exactos.
        /// </summary>
        public static bool IsValidTransactionNumber(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return false;

            // Debe ser exactamente 25 dígitos
            if (!Regex.IsMatch(numero, @"^\d{25}$",
                RegexOptions.CultureInvariant, RegexTimeout))
            {
                return false;
            }

            string fecha = numero.Substring(0, 8);
            string canal = numero.Substring(8, 5);
            string consecutivo = numero.Substring(13, 12);

            // Validar fecha yyyyMMdd
            if (!DateTime.TryParseExact(
                    fecha,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                return false;
            }

            // Validar canal (5 dígitos numéricos)
            if (!Regex.IsMatch(canal, @"^\d{5}$",
                RegexOptions.CultureInvariant, RegexTimeout))
            {
                return false;
            }

            // Validar consecutivo (12 dígitos numéricos)
            if (!Regex.IsMatch(consecutivo, @"^\d{12}$",
                RegexOptions.CultureInvariant, RegexTimeout))
            {
                return false;
            }

            // Si ya validaste 25 dígitos arriba, canal y consecutivo
            // necesariamente son numéricos y del largo correcto.
            return true;
        }

        public sealed record TipoId(string Codigo, string Descripcion);
        // Factory methods (0 complejidad, reuso limpio)
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
            // switch expression reduce ramas anidadas
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

        /// <summary>
        /// Aplica máscara de presentación según tipo de identificación SINPE.
        /// tipo:
        /// 0 Física Nacional, 1 Residente (DIMEX), 2 Gobierno, 3 Jurídica,
        /// 4 Institución Autónoma, 5 Diplomáticos (DIDI)
        /// </summary>
        public static string MaskSinpeId(int tipo, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return id;

            // 1) Normalizar: solo dígitos
            var digits = new string(id.Where(char.IsDigit).ToArray());

            // helper local
            static string Group(string s, params int[] sizes)
            {
                int pos = 0;
                var parts = new List<string>();
                foreach (var size in sizes)
                {
                    if (pos + size > s.Length) return s; // si no alcanza, devuelvo sin máscara
                    parts.Add(s.Substring(pos, size));
                    pos += size;
                }
                // si sobran dígitos, los pego al final (caso raro, pero evita perder info)
                if (pos < s.Length) parts.Add(s.Substring(pos));
                return string.Join("-", parts);
            }

            switch (tipo)
            {
                case 0: // Física nacional: 9 dígitos XX-XXXX-XXXX 
                    //agrego zero adelante
                    digits = digits.Insert(0, "0");
                    return digits.Length == 10 ? Group(digits, 2, 4, 4) : digits;

                case 1: // Residente DIMEX: 12 dígitos, normalmente sin guiones
                    return digits; // si querés guiones, podés cambiar aquí

                case 2: // Gobierno: 10 dígitos 2-PPP-CCCCCC
                    return digits.Length == 10 ? Group(digits, 1, 3, 6) : digits;

                case 3: // Jurídica: 10 dígitos 3-XXX-XXXXXX
                    return digits.Length == 10 ? Group(digits, 1, 3, 6) : digits;

                case 4: // Institución autónoma: 10 dígitos 4-000-CCCCCC
                    return digits.Length == 10 ? Group(digits, 1, 3, 6) : digits;

                case 5: // Diplomáticos (DIDI)
                        // muchos catálogos lo tratan 10 dígitos tipo 5-000-CCCCCC
                    if (digits.Length == 10) return Group(digits, 1, 3, 6);
                    // si viene 12 (DIDI/DIMEX moderno), lo dejo continuo
                    return digits;

                default:
                    return digits;
            }
        }
        #endregion

        #region Consulta Core

        /// <summary>
        /// Consulta la información de SINPE para una solicitud específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<vInfoSinpe> fxTesConsultaInfoSinpe(int CodEmpresa, string solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<vInfoSinpe>
            {
                Code = 0,
                Description = "Ok",
                Result = new vInfoSinpe()
            };
            var infoSinpe = new vInfoSinpe();
            try
            {
                using var connection = new SqlConnection(stringConn);
                var res = connection.QueryFirstOrDefault<InfoSinpeData>(
                            "spTES_W_ConsultaInfoSinpe",
                            new { solicitud }, // si el parámetro del SP se llama distinto, cámbialo aquí
                            commandType: System.Data.CommandType.StoredProcedure
                        );

                infoSinpe.Cedula = res.Cedula;
                infoSinpe.CuentaIBAN = res.Cuenta;
                infoSinpe.tipoID = res.tipoID;
                infoSinpe.cod_divisa = res.cod_divisa;

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

        /// <summary>
        /// Método para obtener la IP pública del host.
        /// </summary>
        /// <returns></returns>
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
                // Obtener información del host por nombre de equipo
                IPHostEntry informacionDelHost = Dns.GetHostEntry(nombreEquipo);

                // Filtrar y obtener la primera dirección IPv4
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

        /// <summary>
        /// Método para obtener los parámetros de conexión para una empresa y canal específicos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="canal"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<ParametrosSinpe> GetUriEmpresa(int codEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto<ParametrosSinpe>();
            try
            {
                using var connection = new SqlConnection(stringConn);

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
                        vIpHost = fxObtenerIpEquipoActual(Environment.MachineName).Result,
                        vUserCGP = parametros.UserCGP,
                        vCanalCGP = Convert.ToInt32(parametros.CanalCGP),
                        UrlCGP_DTR = parametros.UrlCGP_DTR,
                        UrlCGP_PIN = parametros.UrlCGP_PIN,
                        vUsuarioLog = usuario,
                        ServiciosSinpe = parametros.ServiciosSinpe
                    };



                    result.Result = (parametrosSinpe);
                }
                else
                {
                    result.Result = (null);
                    result.Code = -1;
                    result.Description = "No se encontraron parametros SINPE para esta empresa";
                }
            }
            catch (Exception ex)
            {
                result.Result = (null);
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Obtiene Solicitud de Tesorería por número de solicitud.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesTransaccion> fxTesConsultaSolicitud(int CodEmpresa, int Nsolicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesTransaccion>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesTransaccion()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
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
                                    CTA_AHORROS, 
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
                                    ID_BANCO_DESTINO as 'Cuenta', 
                                    RAZON_HOLD, 
                                    DOCUMENTO_BANCO, 
                                    FECHA_BANCO, 
                                    REFERENCIA_BANCARIA, 
                                    COD_CONCEPTO_ANULACION, 
                                    VALIDA_SINPE, 
                                    USUARIO_AUTORIZA_ESPECIAL 
                               FROM 
                                    TES_TRANSACCIONES 
                              where Nsolicitud = @solicitud ";
                response.Result = connection.Query<TesTransaccion>(query, new { solicitud = Nsolicitud }).FirstOrDefault();

                //'Se valida la cedula de destino
                response.Result.Codigo = fxTesConsultaInfoSinpe(CodEmpresa, Nsolicitud.ToString()).Result.Cedula.ToString();
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = null;
            }
            return response;
        }

        //Busco el tipo de cuenta IBAN real segun el registro del Banco Central
        public int ValidoTipoMonedaBCCR(int CodEmpresa, string CuentaIBAN)
        {
            var _parametrosSinpe = GetUriEmpresa(CodEmpresa, "TS");
            //Valido si el servicio esta disponible
            ReqBase context = new ReqBase
            {
                HostId = _parametrosSinpe.Result.vHostPin,
                OperationId = OperationId.ToString(),
                ClientIPAddress = _parametrosSinpe.Result.vIpHost,
                CultureCode = "ES-CR",
                UserCode = _parametrosSinpe.Result.vUsuarioLog,
            };

            var servicio = _PIN.IsServiceAvailable(_parametrosSinpe.Result.UrlCGP_PIN, context);
            if (servicio.IsSuccessful)
            {
                //Valido informacion de la cuenta
                ReqAccountInfo accountData = new ReqAccountInfo
                {
                    HostId = context.HostId,
                    OperationId = context.OperationId,
                    ClientIPAddress = context.ClientIPAddress,
                    CultureCode = context.CultureCode,
                    UserCode = context.UserCode,
                    Id = null,
                    AccountNumber = CuentaIBAN
                };
                var cuentaValida = _PIN.GetAccountInfo(_parametrosSinpe.Result.UrlCGP_PIN, accountData);

                if (cuentaValida.Account == null)
                {
                    return 1;
                }

                return GetCurrencyCodeId(cuentaValida.Account.CurrencyCode);
            }
            return 1;
        }

        public string ValidoNombreCuenta(int CodEmpresa, int TipoId, string Cedula)
        {
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                //Busco nombre si es cedula juridica

                if (TipoId == 3)
                {
                    var empresa = connection.QueryFirstOrDefault<dynamic>("Select DESCRIPCION from CXP_PROVEEDORES WHERE REPLACE(CEDJUR, '-', '') = @cedjur", new { cedjur = Cedula.Replace("-", "") });
                    if (empresa != null)
                    {
                        return empresa.DESCRIPCION;
                    }
                }

                //Busco nombre del titular
                if (Cedula.Length < 9)
                {
                    var socio = connection.QueryFirstOrDefault<dynamic>("SELECT DESCRIPCION FROM TES_BANCOS S WHERE S.ID_BANCO = @Banco", new { Banco = Cedula });
                    if (socio != null)
                    {
                        return socio.DESCRIPCION;
                    }
                }
            }
            catch (Exception)
            {
                return "Desconocido en " + Cedula;
            }
            return "";
        }

        #endregion



    }
}
