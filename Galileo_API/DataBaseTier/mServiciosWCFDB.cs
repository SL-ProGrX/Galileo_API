using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using Sinpe_CCD;
using Sinpe_PIN;
using Sinpe_TFT;
using System.Net;
using System.Security.Cryptography.Xml;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier
{
    public class mServiciosWCFDB
    {
        private readonly IConfiguration _config;
        private readonly Sinpe_PIN.SINPE_PINClient _srvSinpePin = new SINPE_PINClient();
        private readonly Sinpe_TFT.SINPE_TFTClient _srvSinpeTft = new SINPE_TFTClient();
        private readonly Sinpe_CCD.SINPE_CCDClient _srvSinpeCcd = new SINPE_CCDClient();
        private ParametrosSinpe _parametrosSinpe = new ParametrosSinpe();
        private readonly FactElectronica.ServicioClient _srvFactElectronica  = new FactElectronica.ServicioClient();

        private readonly mTesoreria _mTesoreria;

        public mServiciosWCFDB(IConfiguration config)
        {
            _config = config;
            _mTesoreria = new mTesoreria(_config);

            _parametrosSinpe.vHostPin = _config.GetSection("Sinpe").GetSection("HostIdPIN").Value.ToString();
            _parametrosSinpe.vUserCGP = _config.GetSection("Sinpe").GetSection("vUserCGP").Value.ToString();
            _parametrosSinpe.vCanalCGP = int.Parse(_config.GetSection("Sinpe").GetSection("vCanalCGP").Value.ToString());
        }

        #region Validación de Solicitud SINPE

        /// <summary>
        /// Método principal para la validación de una solicitud SINPE
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>

        public ErrorDTO fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            ErrorDTO response = new();

            string fxValidacionSinpe = "";
            vInfoSinpe? info = new();
            try
            {
                var cntInfoSinpe = fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
                if (cntInfoSinpe.Code == -1)
                {
                    response.Code = cntInfoSinpe.Code;
                    response.Description = cntInfoSinpe.Description;
                    return response;
                }
                info = cntInfoSinpe.Result; 
                

                if (System.String.IsNullOrEmpty(info.Cedula) == false && System.String.IsNullOrEmpty(info.CuentaIBAN) == false)
                {
                    if(ConsultarIsPINEntity(info.CuentaIBAN).Result == true)
                    {
                        if (ConsultarIsServiceAvailable(usuario).Result == false)
                        {
                            response.Code = -1;
                            response.Description = solicitud.ToString() + " - " + "No se ha podido establecer comunicación con el servidor de forma adecuada, intente de nuevo o más tarde.";
                        }
                        else
                        {
                            Sinpe_PIN.ResAccountInfo LaInformacionDeLaCuentaPIN = new Sinpe_PIN.ResAccountInfo();
                            LaInformacionDeLaCuentaPIN.Errors = new Errores[0];
                            LaInformacionDeLaCuentaPIN = ConsultarAccountInfo(info.CuentaIBAN).Result;
                            
                            if (LaInformacionDeLaCuentaPIN.IsSuccessful == false || 
                                LaInformacionDeLaCuentaPIN.Errors.Length > 0 || 
                                LaInformacionDeLaCuentaPIN.Account.State != "1")
                            {
                                if(LaInformacionDeLaCuentaPIN.Errors.Length > 0)
                                {
                                    response.Code = -1;
                                    response.Description = LaInformacionDeLaCuentaPIN.Errors[0].Message;
                                }
                                else
                                {
                                    response.Code = -1;
                                    response.Description = "Error de estado " + LaInformacionDeLaCuentaPIN.Account.State;
                                }

                                if (!string.IsNullOrEmpty(response.Description))
                                {
                                    response.Code = -1;
                                    response.Description = solicitud.ToString() + " - " + response.Description;
                                }
                            }
                            else
                            {
                                var cedulaFormateada = fxFormatoIdentificacionSinpe(info.Cedula.Trim(), info.tipoID).Result;
                                if (cedulaFormateada != LaInformacionDeLaCuentaPIN.Account.HolderId)
                                {
                                    response.Code = -1;
                                    response.Description = "La cédula obtenida no corresponde con la cédula de la cuenta.";
                                }
                                else
                                {
                                    response.Code = 0;
                                    response.Description = $@"La cuenta IBAN {info.CuentaIBAN} registrada a 
                                        nombre de {LaInformacionDeLaCuentaPIN.Account.Holder} cédula: {LaInformacionDeLaCuentaPIN.Account.HolderId} Tipo Id: {info.tipoID} Tipo de Moneda: @moneda";
                                }
                            }

                        }
                    }
                    else
                    {

                        response.Description = fxTesValidaRechazoCuentaIBAN(CodEmpresa, info.Cedula, info.CuentaIBAN, info.tipoID).Result;
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = solicitud.ToString() + " - Solicitud SINPE incompleta.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                _ = ex.Message; //bandera para visualizar error técnico. 
                response.Description = "Ocurrió un problema con la validación.";
            }
            return response;
        }
   
        private ErrorDTO<bool> ConsultarIsPINEntity(string AccountNumber)
        {
            Sinpe_PIN.ResPINEntity Elresutado = new Sinpe_PIN.ResPINEntity();
            ErrorDTO<bool> errorDTO = new ErrorDTO<bool>();
            errorDTO.Result = true;
            var resultado = new Sinpe_PIN.ReqPINEntity();
            try
            {
                // Generar un nuevo GUID para OperationId
                Guid newGuid = Guid.NewGuid();
                Sinpe_PIN.ReqPINEntity PINEntity = new Sinpe_PIN.ReqPINEntity();
                // Crear instancia del objeto de solicitud ReqPINEntity
                var obtenerIpEquipoActual = fxObtenerIpEquipoActual(_parametrosSinpe.vHost);
                if(obtenerIpEquipoActual.Code == -1)
                {
                    return new ErrorDTO<bool>
                    {
                        Code = -1,
                        Description = obtenerIpEquipoActual.Description,
                        Result = false
                    };
                }

                PINEntity.HostId = _parametrosSinpe.vHostPin;
                PINEntity.OperationId = newGuid;
                PINEntity.ClientIpAddress = obtenerIpEquipoActual.Result;
                PINEntity.AccountNumber = AccountNumber;

                Sinpe_PIN.ReqPINEntityBody pINEntityBody = new ReqPINEntityBody();
                pINEntityBody.ReqPINEntity = PINEntity;

                // Llamar al método IsPINEntity con los parámetros
                Elresutado = IsPINEntity(PINEntity).Result;
                errorDTO.Result = Elresutado.PINEntity;
            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
                errorDTO.Result = false;
            }

            // Retornar el resultado del campo PINEntity
            return errorDTO;
        }

        private ErrorDTO<string> fxObtenerIpEquipoActual(string nombreEquipo)
        {
            ErrorDTO<string> response = new ErrorDTO<string>();
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

        private ErrorDTO<string> fxTesValidaRechazoCuentaIBAN(int CodEmpresa, string ced, string iban, int tipoID)
        {
            ErrorDTO<string> response = new ErrorDTO<string>();
            string? fxTesValidaRechazoCuentaIBAN = "";
            try
            {
                var bodySINPE = new Sinpe_TFT.ObtenerInformacionCuentaBody();
                var result = new Sinpe_TFT.InformacionCuenta();
                string cedulaValida = "";
                var body =  _srvSinpeTft.ObtenerInformacionCuentaAsync;

                if(!string.IsNullOrEmpty(ced) && !string.IsNullOrEmpty(ced)) 
                {
                    // Formatear la cédula
                    var valCedula = fxFormatoIdentificacionSinpe(ced.Trim(), tipoID);
                    if(valCedula.Code == -1)
                    {
                        return new ErrorDTO<string>
                        {
                            Code = -1,
                            Description = valCedula.Description,
                            Result = ""
                        };
                    }

                    cedulaValida = valCedula.Result;
                    bodySINPE.body.Identificacion  = cedulaValida.Trim();
                    bodySINPE.body.IBAN = iban.Trim();
                    bodySINPE.rastro = fxCrearRastroSINPESIF_TFT().Result;

                    var infoResult = fxObtenerInformacionSINPE(bodySINPE);
                    if (infoResult.Code == -1)
                    {
                        response.Code = infoResult.Code;
                        response.Description = infoResult.Description;
                        response.Result = "";
                        return response;
                    }

                    if (result.MotivoRechazo > 0)
                    {
                        var valMotivo = fxTesConsultarMotivoRechazo(CodEmpresa, result.MotivoRechazo);
                        if(valMotivo.Code == -1)
                        {
                            response.Code = valMotivo.Code;
                            response.Description = valMotivo.Description;
                            response.Result = valMotivo.Result;
                            return response;
                        }
                        fxTesValidaRechazoCuentaIBAN = valMotivo.Result;
                    }

                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "";
            }
            return response;
        }

        private ErrorDTO<string> fxFormatoIdentificacionSinpe(string cedula, int tipoID)
        {
            ErrorDTO<string> errorDTO = new();
            string response = "";
            string formato = "";
            int idSINPE = -1;

            try
            {
                if (cedula.Contains("-"))
                {
                    errorDTO.Code = 0;
                    errorDTO.Description = "";
                    errorDTO.Result = cedula;
                    return errorDTO;
                }

                cedula = cedula.Trim();
                idSINPE = tipoID; // setCodigoSugefEstandar(tipoID).Result;

                switch (idSINPE)
                {
                    case 0: // Cédula Nacional
                        if (cedula.Length != 9)
                        {
                            errorDTO.Code = -1;
                            errorDTO.Description = "Formato de cédula inválido.";
                            errorDTO.Result = cedula;
                            return errorDTO;
                        }

                        formato = "0{0}-{1}-{2}";
                        response = string.Format(formato, cedula.Substring(0, 1), cedula.Substring(1, 4), cedula.Substring(5));
                        break;
                    case 1: // Dimex
                    case 5: // Didi
                        if (cedula.Length != 12)
                        {
                            errorDTO.Code = -1;
                            errorDTO.Description = "Formato de cédula inválido.";
                            errorDTO.Result = cedula;
                            return errorDTO;
                        }

                        formato = "{0}";
                        response = string.Format(formato, cedula);
                        break;
                    case 3: //Jurídica
                    case 4: //Institución autónoma
                        if (cedula.Length != 10)
                        {
                            errorDTO.Code = -1;
                            errorDTO.Description = "Formato de cédula inválido.";
                            errorDTO.Result = cedula;
                            return errorDTO;
                        }

                        formato = "{0}-{1}-{2}";
                        response = string.Format(formato, cedula.Substring(0, 1), cedula.Substring(1, 3), cedula.Substring(4));
                        break;
                    case 9: // Cédula de residencia.
                        if (cedula.Length != 13)
                        {
                            errorDTO.Code = -1;
                            errorDTO.Description = "Formato de cédula inválido.";
                            errorDTO.Result = cedula;
                            return errorDTO;
                        }

                        formato = "{0}-{1}-{2}";
                        response = string.Format(
                            formato,
                            cedula.Substring(0, 3),  // Primer bloque de 3 dígitos
                            cedula.Substring(3, 5),  // Segundo bloque de 5 dígitos
                            cedula.Substring(8, 5)   // Tercer bloque de 5 dígitos
                        );

                        break;
                    case 2: // Pasaporte 
                        if (cedula.Length != 9)
                        {
                            errorDTO.Code = -1;
                            errorDTO.Description = "Formato de cédula inválido.";
                            errorDTO.Result = cedula;
                            return errorDTO;
                        }

                        response = cedula;

                        break;
                    default:
                        errorDTO.Code = -1;
                        errorDTO.Description = "Tipo de identificación inválido.";
                        errorDTO.Result = cedula;
                        break;
                }
                errorDTO.Result = response;

            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
                errorDTO.Result = "";
            }
            return errorDTO;
        }

        private ErrorDTO<string> fxTesConsultarMotivoRechazo(int CodEmpresa, int idMotivo)
        {
            ErrorDTO<string> response = new ErrorDTO<string>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select concat(descripcion, ' (',cod_motivo,')') as rechazo " +
                        $"from sinpe_motivos where cod_motivo = {idMotivo} ";
                    response.Result = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "Rechazo desconocido.";
            }
            return response;
        }

        private ErrorDTO<bool> ConsultarIsServiceAvailable(string vUsuario)
        {
            ErrorDTO<bool> errorDTO = new ErrorDTO<bool>();
            try
            {
                // Generar un nuevo GUID
                Guid newGuid = Guid.NewGuid();

                var context = new Sinpe_PIN.BaseRequest
                {
                    HostId = _parametrosSinpe.vHostPin,
                    OperationId = newGuid,
                    ClientIpAddress = fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result
                };

                Sinpe_PIN.BaseRequestBody contextBody = new Sinpe_PIN.BaseRequestBody();
                contextBody.BaseRequest = context;

                var elResultado = IsServiceAvailable(context, vUsuario);
                if (!elResultado.Result.ServiceAvailable)
                {
                    errorDTO.Code = -1;
                    errorDTO.Description = "Servicio PIN NO Disponible RESPUESTA: " + elResultado.Result.Errors;
                    errorDTO.Result = false;
                }
                else
                {
                    errorDTO.Result = elResultado.Result.ServiceAvailable;
                }

            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
                errorDTO.Result = false;
            }
             return errorDTO;
        }

        private ErrorDTO<Sinpe_PIN.ResAccountInfo> ConsultarAccountInfo(string iban)
        {
            ErrorDTO<Sinpe_PIN.ResAccountInfo> errorDTO = new ErrorDTO<Sinpe_PIN.ResAccountInfo>();
            errorDTO = new ErrorDTO<Sinpe_PIN.ResAccountInfo>();
            try
            {
                Guid newGuid = Guid.NewGuid();
                var accountData = new Sinpe_PIN.ReqAccountInfo
                {
                    HostId = _parametrosSinpe.vHostPin, 
                    OperationId = newGuid,
                    ClientIpAddress = fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result,
                    AccountNumber = iban
                };


                var laInformacionDeLaCuentaPIN = fxCrearAccountInfo(GetAccountInfo(accountData).Result);// fxCrearAccountInfo(GetAccountInfo(accountData)).Result;
                errorDTO.Result = laInformacionDeLaCuentaPIN.Result;
            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
                errorDTO.Result = new Sinpe_PIN.ResAccountInfo();
            }

            return errorDTO;
        }

        private ErrorDTO<Sinpe_PIN.ResAccountInfo> fxCrearAccountInfo(Sinpe_PIN.ResAccountInfo accountInfo)
        {
            ErrorDTO<Sinpe_PIN.ResAccountInfo> errorDTO = new ErrorDTO<Sinpe_PIN.ResAccountInfo>();
            errorDTO = new ErrorDTO<Sinpe_PIN.ResAccountInfo>();

            try
            {
                var response = new ResponseData();
                response.IsSuccessful = accountInfo.IsSuccessful;
                response.OperationId = accountInfo.OperationId;
                response.Errors = new List<Errores>();

                foreach (var error in accountInfo.Errors)
                {
                    response.Errors.Add(new Errores
                    {
                        Code = error.Code,
                        Message = error.Message
                    });
                }


                response.Account = accountInfo.Account;

                errorDTO.Result = new Sinpe_PIN.ResAccountInfo
                {
                    Errors = response.Errors.ToArray(),
                    OperationId = response.OperationId,
                    IsSuccessful = response.IsSuccessful,
                    Account = response.Account
                };
            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
                errorDTO.Result = new Sinpe_PIN.ResAccountInfo();
            }
            return errorDTO;
        }

        private ErrorDTO<Sinpe_PIN.ResAccountInfo> GetAccountInfo(Sinpe_PIN.ReqAccountInfo AccountData)
        {
            ErrorDTO<Sinpe_PIN.ResAccountInfo> response = new ErrorDTO<Sinpe_PIN.ResAccountInfo>();
            var body = new Sinpe_PIN.ReqAccountInfoBody();
            try
            {
                body.ReqAccountInfo = AccountData;
                body.rastro = fxCrearRastroSINPESIF_PIN().Result;

                response.Result = _srvSinpePin.GetAccountInfoAsync(body).Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.ResAccountInfo();

            }
            return response;
        }

        private ErrorDTO<Sinpe_PIN.IModelosRastroSIF> fxCrearRastroSINPESIF_PIN()
        {
            ErrorDTO<Sinpe_PIN.IModelosRastroSIF> response = new ErrorDTO<Sinpe_PIN.IModelosRastroSIF>();
            response.Result = new Sinpe_PIN.IModelosRastroSIF();
            try
            {
                response.Result.IP = _parametrosSinpe.vIpHost;
                response.Result.Equipo = _parametrosSinpe.vHost;
                response.Result.Usuario = _parametrosSinpe.vUserCGP;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.IModelosRastroSIF();
            }
            return response;
        }

        private ErrorDTO<Sinpe_PIN.IModelosRastroSIF> fxCrearRastroSINPESIF_PIN(string vUsuario)
        {
            ErrorDTO<Sinpe_PIN.IModelosRastroSIF> response = new ErrorDTO<Sinpe_PIN.IModelosRastroSIF>();
            response.Result = new Sinpe_PIN.IModelosRastroSIF();
            try
            {
                response.Result.IP = _parametrosSinpe.vIpHost;
                response.Result.Equipo = _parametrosSinpe.vHost;
                response.Result.Usuario = vUsuario;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.IModelosRastroSIF();
            }
            return response;
        }

        private ErrorDTO<Sinpe_PIN.ResPINEntity> IsPINEntity(Sinpe_PIN.ReqPINEntity PINEntity)
        {
            ErrorDTO<Sinpe_PIN.ResPINEntity> response = new ErrorDTO<Sinpe_PIN.ResPINEntity>();
            var body = new Sinpe_PIN.ReqPINEntityBody();

            try
            {
                body.ReqPINEntity = PINEntity;
                _parametrosSinpe.vIpHost = PINEntity.ClientIpAddress;
                _parametrosSinpe.vUserCGP = _parametrosSinpe.vUserCGP;


                body.rastro = fxCrearRastroSINPESIF_PIN().Result;
                response.Result = new Sinpe_PIN.ResPINEntity();
                response.Result.PINEntity = body.ReqPINEntity.Equals(PINEntity);

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.ResPINEntity();
            }
            return response;
        }

        private ErrorDTO<Sinpe_PIN.ResServiceAvailable> IsServiceAvailable(Sinpe_PIN.BaseRequest Context,string vUsuario)
        {
            ErrorDTO<Sinpe_PIN.ResServiceAvailable> response = new ErrorDTO<Sinpe_PIN.ResServiceAvailable>();
            var body = new Sinpe_PIN.BaseRequestBody();
            try
            {
                body.BaseRequest = Context;
                body.rastro = fxCrearRastroSINPESIF_PIN(vUsuario).Result;

                response.Result = _srvSinpePin.IsServiceAvailableAsync(body).Result;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.ResServiceAvailable();
            }

            return response;
        }
    
        private ErrorDTO<Sinpe_TFT.IModelosRastroSIF> fxCrearRastroSINPESIF_TFT() 
        {
            var response = new ErrorDTO<Sinpe_TFT.IModelosRastroSIF>();
            try
            {
                Sinpe_TFT.IModelosRastroSIF rastro = new Sinpe_TFT.IModelosRastroSIF();
                rastro.IP = _parametrosSinpe.vIpHost;
                rastro.Equipo = _parametrosSinpe.vHost;
                rastro.Usuario = _parametrosSinpe.vUserCGP;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDTO<Sinpe_TFT.ObtenerInformacionCuentaResponseBody> fxObtenerInformacionSINPE(ObtenerInformacionCuentaBody body)
        {
            var response = new ErrorDTO<Sinpe_TFT.ObtenerInformacionCuentaResponseBody>();
            try
            {
                response.Result = _srvSinpeTft.ObtenerInformacionCuentaAsync(body).Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #endregion

        #region Emisión de SINPE Credito Directo

        /// <summary>
        /// Realiza el proceso de emisión de una transferencia SINPE Crédito Directo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        public ErrorDTO fxTesEmisionSinpeCreditoDirecto(int CodEmpresa,
            int Nsolicitud,DateTime vfecha,string vUsuario, int doc_base,int contador )
        {
            //(Realiza el proceso de envio y recibido de SINPE)
            var response = new ErrorDTO 
            {
                Code = 0,
                Description = "Ok"
            };

            var respuesta = new Sinpe_CCD.RespuestaRegistro();
            var datos = new TesTransaccion();
            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazo = "";
            try
            {
                if(Nsolicitud > 0)
                {
                    if(fxTesConsultarServicioDisponible(CodEmpresa, vUsuario).Result == false)
                    {
                        estadoSinpe = false;
                        idRechazo = 83;
                        rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                    }
                    else
                    {
                        respuesta = fxTesEnvioSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vUsuario).Result;

                        if (respuesta.MotivoError != 0)
                        {
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;                           
                        }
                        else
                        {
                            estadoSinpe = true;
                        }
                        
                    }

                    //Guardar la respuesta en la transacción
                    datos.NumeroSolicitud = Nsolicitud;
                    datos.FechaEmision = vfecha;
                    datos.FechaTraslado = vfecha;
                    datos.UsuarioGenera = vUsuario;
                    datos.estadoSinpe = estadoSinpe;
                    datos.IdMotivoRechazo = idRechazo;
                    datos.CodigoReferencia = respuesta.CodigoReferencia;
                    datos.DocumentoBase = doc_base.ToString();
                    datos.contador = contador.ToString();

                    if (fxTesRespuestaSinpe(CodEmpresa, datos).Result == false)
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Se produjo un error al actualizar la transacción", vUsuario);
                    }

                    if (estadoSinpe == true)
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Emisión Transferencia Sinpe: Exitosa", vUsuario);
                    }
                    else
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Transferencia Sinpe rechazada: {rechazo} ", vUsuario);
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se ha indicado una solicitud válida.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private ErrorDTO<bool> fxTesConsultarServicioDisponible(int CodEmpresa, string vUsuario)
        {
            var response = new ErrorDTO<bool> 
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            var rastro = new Sinpe_CCD.IModelosRastroSIF();

            try
            {
                rastro = fxCrearRastroSINPESIF_CCD(vUsuario).Result;  //fxCrearRastroSINPESIF(vUsuario);

                if (_srvSinpeCcd.ServicioDisponibleAsync(rastro).Result == false)
                {
                    response.Result = false;
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el servicio disponible de SINPE.";
                response.Result = false;
            }
            return response;
        }

        private ErrorDTO<Sinpe_CCD.IModelosRastroSIF> fxCrearRastroSINPESIF_CCD(string vUsuario)
        {
            ErrorDTO<Sinpe_CCD.IModelosRastroSIF> response = new ErrorDTO<Sinpe_CCD.IModelosRastroSIF>();
            response.Result = new Sinpe_CCD.IModelosRastroSIF();
            try
            {
                response.Result.IP = _parametrosSinpe.vIpHost;
                response.Result.Equipo = _parametrosSinpe.vHost;
                response.Result.Usuario = vUsuario;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_CCD.IModelosRastroSIF();
            }
            return response;
        }

        private ErrorDTO<Sinpe_CCD.RespuestaRegistro> fxTesEnvioSinpeCreditoDirecto(int CodEmpresa, int Nsolicitud, string vUsuario)
        {
            //(Realiza el proceso de emisión de la transferencia SINPE)
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<Sinpe_CCD.RespuestaRegistro>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            var solicitud = new TesTransaccion();

            var response = new Sinpe_CCD.RegistrarDebitoCuentaResponseBody();
            var body = new Sinpe_CCD.RegistrarDebitoCuentaRequestBody();
            var rastro = new Sinpe_CCD.Rastro();
            var transaccion = new Sinpe_CCD.TransferenciaAS400();
            var responseDetail = new Sinpe_CCD.RespuestaRegistro();
            var bodyWCF = new Sinpe_CCD.RegistrarDebitoCuentaBody();

            string detalle = "";

            try
            {
                solicitud = fxTesConsultaSolicitud(CodEmpresa, Nsolicitud).Result;

                body.Rastro = new Sinpe_CCD.Rastro();
                detalle = (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5)
                    .Substring(0, Math.Min(255,
                        (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5).Length));
           
                if(detalle.Length < 15)
                {
                    detalle = (detalle.Replace("\r\n", "") + " Transferencia SINPE")
                             .Substring(0, Math.Min(255, (detalle.Replace("\r\n", "") + " Transferencia SINPE").Length));
                }

                body.Rastro.IP = _parametrosSinpe.vIpHost;
                body.Rastro.Usuario = _parametrosSinpe.vUserCGP;
                body.Rastro.Canal = _parametrosSinpe.vCanalCGP;

                transaccion.DatosTransaccion = new Sinpe_CCD.Transaccion();
                transaccion.DatosTransaccion.Moneda = (solicitud.Divisa == "DOL")
                                                    ? Sinpe_CCD.E_Monedas.Dolares
                                                    : Sinpe_CCD.E_Monedas.Colones;

                // Monto
                if (solicitud.Divisa == "DOL")
                {
                    transaccion.DatosTransaccion.Monto = (solicitud.tipoCambio > 0)
                        ? (solicitud.Monto / solicitud.tipoCambio)
                        : 0;
                    transaccion.DatosTransaccion.Monto = Math.Round(transaccion.DatosTransaccion.Monto, 4); // Redondeo para dólares
                }
                else
                {
                    transaccion.DatosTransaccion.Monto = solicitud.Monto;
                }

                // Otros campos
                transaccion.DatosTransaccion.Descripcion = detalle.Replace("\r\n", "");
                transaccion.DatosTransaccion.Servicio = "CCD";
                transaccion.DatosTransaccion.PuntoIntegracion = 1;
                transaccion.DatosTransaccion.CentroCosto = 1;
                transaccion.DatosTransaccion.CodigoConcepto = 1;
                transaccion.DatosTransaccion.EntidadOrigen = 205;
                transaccion.DatosTransaccion.FirmaDigital = false;
                transaccion.DatosTransaccion.IDCorrelation = "1";
                transaccion.DatosTransaccion.eMAIL = solicitud.CorreoNotifica?.Trim();

                transaccion.ClienteOrigen = new Sinpe_CCD.ClienteAS400();
                transaccion.ClienteOrigen.Identificacion = solicitud.CedulaOrigen.Replace("-", "");
                transaccion.ClienteOrigen.Nombre = solicitud.NombreOrigen;
                transaccion.ClienteOrigen.IBAN = solicitud.CuentaOrigen;
                transaccion.ClienteOrigen.TipoCedula = solicitud.tipoCedOrigen;

                transaccion.ClienteDestino = new Sinpe_CCD.ClienteAS400();
                transaccion.ClienteDestino.Identificacion = solicitud.Codigo.Replace("-", "");
                transaccion.ClienteDestino.Nombre = solicitud.Beneficiario;
                transaccion.ClienteDestino.IBAN = solicitud.Cuenta;
                transaccion.ClienteDestino.TipoCedula = solicitud.tipoCedDestino;

                try
                {
                    body.Transacciones = new Sinpe_CCD.TransferenciaAS400[] { transaccion };

                    bodyWCF.body = body;
                    bodyWCF.rastro = fxCrearRastroSINPESIF_CCD(vUsuario).Result;

                    response = _srvSinpeCcd.RegistrarDebitoCuentaAsync(bodyWCF).Result;
                    responseDetail = response.RegistrarDebitoCuentaResult[0];
                    ;

                    resp.Result = responseDetail;
                }
                catch (Exception)
                {
                    responseDetail.MotivoError = 201;

                    resp.Result = responseDetail;
                }

            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al enviar la solicitud de crédito directo a SINPE.";
                resp.Result = null;
            }

            return resp;
        }

        #endregion

        #region Emisión de SINPE Tiempo Real

        /// <summary>
        /// Realiza el proceso de emisión de una transferencia SINPE Tiempo Real
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        public ErrorDTO fxTesEmisionSinpeTiempoReal(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };

            var solicitud = new TesTransaccion();
            var ElResultadoDeSendTransfer = new Sinpe_PIN.ResPINSending();
            var respuesta = new Sinpe_TFT.RespuestaRegistro();
            var datos = new TesTransaccion();
            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazo = null;

            try
            {
                if(Nsolicitud > 0)
                {
                    solicitud = fxTesConsultaSolicitud(CodEmpresa, Nsolicitud).Result;

                    if(ConsultarIsPINEntity(solicitud.Cuenta).Result == true)
                    {
                        if(ConsultarIsServiceAvailable(vUsuario).Result == false)
                        {
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                        }
                        else
                        {
                            //Servicio disponible
                            ElResultadoDeSendTransfer = SendTransfer(CodEmpresa, solicitud).Result;

                            if(ElResultadoDeSendTransfer == null || ElResultadoDeSendTransfer.IsSuccessful == false)
                            {
                                estadoSinpe = false;

                                if (ElResultadoDeSendTransfer.Errors.Length >= 0)
                                {
                                    idRechazo = ElResultadoDeSendTransfer.Errors[0].Code;
                                    rechazo = ElResultadoDeSendTransfer.Errors[0].Message;
                                }
                                else
                                {
                                    idRechazo = -1;
                                    rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                                }
                            }
                            else
                            {
                                estadoSinpe = true;
                            }
                        }

                        switch (ElResultadoDeSendTransfer.PINSendingResult.State)
                        {
                            case 32:
                                estadoSinpe = true;
                                break;
                            case 128:
                                estadoSinpe = false;
                                rechazo = "Se ha rechazado la transacción. Favor intente mas tarde.";
                                break;
                            case 256:
                                estadoSinpe = false;
                                rechazo = "La confirmación la transacción esta en espera, favor consulte sus movimientos mas tarde.";
                                break;
                        }

                        datos.NumeroSolicitud = Nsolicitud;
                        datos.FechaEmision = vfecha;
                        datos.FechaTraslado = vfecha;
                        datos.UsuarioGenera = vUsuario;
                        datos.estadoSinpe = estadoSinpe;
                        datos.IdMotivoRechazo = idRechazo;
                        datos.CodigoReferencia = ElResultadoDeSendTransfer.PINSendingResult.SINPERefNumber;
                        datos.DocumentoBase = doc_base.ToString();
                        datos.contador = contador.ToString();

                        if (fxTesRespuestaSinpe(CodEmpresa, datos).Result == false)
                        {
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Se produjo un error al actualizar la transacción", vUsuario);
                        }

                        //'Actualización nueva
                        if (estadoSinpe == true)
                        {
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Emisión Transferencia Sinpe: Exitosa", vUsuario);

                            //Se agrega proceso para realizar el envio de notificaciones
                            EnviaNotificacionesCajas(CodEmpresa, datos.CodigoReferencia);
                        }
                        else
                        {
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Transferencia Sinpe rechazada: {rechazo} ", vUsuario);
                        }
                    }
                    else //Uso de canal CanalTFT
                    {
                        if (fxTesConsultarServicioDisponible(CodEmpresa, vUsuario).Result == false)
                        {
                            //'Se registra el error por servicio no disponible
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Emisión Transferencia Sinpe: {rechazo}" , vUsuario);
                        }
                        else
                        {
                            respuesta = fxTesEnvioSinpeTiempoReal(solicitud).Result;

                            if (respuesta.MotivoError != 0)
                            {
                                estadoSinpe = false;
                                idRechazo = 83;
                                rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                                _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Transferencia Sinpe rechazada: {rechazo}", vUsuario);
                            }
                            else
                            {
                                estadoSinpe = true;
                                _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Emisión Transferencia Sinpe: Exitosa", vUsuario);
                            }

                            datos.NumeroSolicitud = Nsolicitud;
                            datos.FechaEmision = vfecha;
                            datos.FechaTraslado = vfecha;
                            datos.UsuarioGenera = vUsuario;
                            datos.estadoSinpe = estadoSinpe;
                            datos.IdMotivoRechazo = idRechazo;
                            datos.CodigoReferencia = respuesta.CodigoReferencia;
                            datos.DocumentoBase = doc_base.ToString();
                            datos.contador = contador.ToString();

                            if (estadoSinpe)
                            {
                                EnviaNotificacionesCajas(CodEmpresa, datos.CodigoReferencia);
                            }

                            if (fxTesRespuestaSinpe(CodEmpresa, datos).Result == false)
                            {
                                _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Se produjo un error al actualizar la transacción", vUsuario);
                            }
                        }
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se ha indicado una solicitud válida.";
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al procesar la solicitud de emisión de SINPE Tiempo Real.";
            }
            return response;
        }

        private ErrorDTO<Sinpe_PIN.ResPINSending> SendTransfer(int CodEmpresa, TesTransaccion solicitud)
        {
            var ErrorDTO = new ErrorDTO<Sinpe_PIN.ResPINSending>
            {
                Code = 0,
                Description = "Ok",
                Result = new Sinpe_PIN.ResPINSending()
            };
            Sinpe_PIN.ReqPINSending TransferData = new Sinpe_PIN.ReqPINSending();
            Sinpe_PIN.ResPINSending ElResultadoDeSendTransfer = new Sinpe_PIN.ResPINSending();

            string detalle = "";
            try
            {
                detalle = (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5)
                   .Substring(0, Math.Min(255,
                       (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5).Length));


                if (detalle.Length < 15)
                {
                    detalle = (detalle.Replace("\r\n", "") + " Transferencia SINPE")
                             .Substring(0, Math.Min(255, (detalle.Replace("\r\n", "") + " Transferencia SINPE").Length));
                }

                if (solicitud.Divisa == "DOL")
                {
                    solicitud.Monto = (solicitud.tipoCambio > 0)
                         ? (solicitud.Monto / solicitud.tipoCambio)
                         : 0;
                    solicitud.Monto = Math.Round(solicitud.Monto, 4); // Redondeo para dólares
                    solicitud.Divisa = "USD";
                }
                else
                {
                    solicitud.Divisa = "CRC";
                }

                Guid newGuid = Guid.NewGuid();
                TransferData.HostId = _parametrosSinpe.vHostPin;
                TransferData.OperationId = newGuid;
                TransferData.ClientIpAddress = fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result;
                TransferData.CoreIntegrationPoint = 1; // Punto de integración
                TransferData.CostCenter = 1; // Centro de costo
                TransferData.UserCode = _parametrosSinpe.vUserCGP;

                TransferData.Transfer = new Sinpe_PIN.PINTransfer();
                TransferData.Transfer.ChannelRefNumber = ConsultarConsecutivoSinpe(CodEmpresa).Result;
                TransferData.Transfer.Amount = solicitud.Monto;
                TransferData.Transfer.CurrencyCode = solicitud.Divisa;
                TransferData.Transfer.Description = detalle;
                TransferData.Transfer.OriginEntityIBAN = "";
                TransferData.Transfer.OriginCustomer = new Sinpe_PIN.OriginCustomer();
                TransferData.Transfer.OriginCustomer.Id = fxFormatoIdentificacionSinpe(solicitud.CedulaOrigen.Trim(), solicitud.tipoCedOrigen.GetHashCode()).Result;
                TransferData.Transfer.OriginCustomer.IdType = PIN_OBTENER_TIPO_IDENTIFICACION(CodEmpresa, solicitud.tipoCedOrigen.GetHashCode()).Result;
                TransferData.Transfer.OriginCustomer.Name = solicitud.NombreOrigen;
                TransferData.Transfer.OriginCustomer.IBAN = solicitud.CuentaOrigen;
                TransferData.Transfer.OriginCustomer.Email = "";
                TransferData.Transfer.OriginCustomer.DebitIBAN = true;
                TransferData.Transfer.DestinationCustomer = new Sinpe_PIN.DestinationCustomer();
                TransferData.Transfer.DestinationCustomer.Id = fxFormatoIdentificacionSinpe(solicitud.Codigo.Trim(), solicitud.tipoCedDestino.GetHashCode()).Result;
                TransferData.Transfer.DestinationCustomer.IdType = PIN_OBTENER_TIPO_IDENTIFICACION(CodEmpresa, solicitud.tipoCedDestino.GetHashCode()).Result;
                TransferData.Transfer.DestinationCustomer.Name = solicitud.Beneficiario;
                TransferData.Transfer.DestinationCustomer.IBAN = solicitud.Cuenta;
                TransferData.Transfer.DestinationCustomer.Email = "";
                
                ElResultadoDeSendTransfer = SendTransfer(CodEmpresa, TransferData, solicitud.UsuarioGenera).Result;

                ErrorDTO.Result = ElResultadoDeSendTransfer;
            }
            catch (Exception)
            {
                ErrorDTO.Code = -1;
                ErrorDTO.Description = "Error al procesar los detalles de la solicitud.";
                ErrorDTO.Result = new Sinpe_PIN.ResPINSending();
            }
            return ErrorDTO;

        }

        private ErrorDTO<Sinpe_PIN.ResPINSending> SendTransfer(int CodEmpresa, Sinpe_PIN.ReqPINSending TransferData, string vUsuario)
        {
            var res = new ErrorDTO<Sinpe_PIN.ResPINSending> 
            {
                Code = 0,
                Description = "Ok",
                Result = new Sinpe_PIN.ResPINSending()
            };
            try
            {
                var response = new Sinpe_PIN.ResPINSending();
                var body = new Sinpe_PIN.ReqPINSendingBody();

                body.ReqPINSending = TransferData;
                body.rastro = fxCrearRastroSINPESIF_PIN(vUsuario).Result;

                response = _srvSinpePin.SendTransferAsync(body).Result;

                res.Result = response;
            }
            catch (Exception)
            {
                res.Code = -1;
                res.Description = "Error al enviar la transferencia SINPE Tiempo Real.";
                res.Result = new Sinpe_PIN.ResPINSending();
            }
            return res;
        }

        private ErrorDTO<int> PIN_OBTENER_TIPO_IDENTIFICACION(int CodEmpresa, int CODIGO_SUGEF)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select CODIGO_PIN from AFI_TIPOS_IDS where CODIGO_SUGEF = {CODIGO_SUGEF}";
                    response.Result = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al obtener el tipo de identificación PIN.";
                response.Result = 0;
            }

            return response;
        }

        private ErrorDTO<string> ConsultarConsecutivoSinpe(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<string>
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"exec spPSL_ConsultarConsecutivoSinpe @CANAL";
                    response.Result = connection.Query<string>(query, new { CANAL = _parametrosSinpe.vCanalCGP }).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el consecutivo de SINPE.";
                response.Result = null;
            }

            return response;
        }

        private ErrorDTO<Sinpe_TFT.RespuestaRegistro> fxTesEnvioSinpeTiempoReal(TesTransaccion solicitud)
        {
            var resp = new ErrorDTO<Sinpe_TFT.RespuestaRegistro>
            {
                Code = 0,
                Description = "Ok",
                Result = new Sinpe_TFT.RespuestaRegistro()
            };

            var response = new Sinpe_TFT.EnvioDirectoDebitoCuentaResponseBody();
            var body = new Sinpe_TFT.EnvioDirectoDebitoCuentaRequestBody();
            var transaccion = new Sinpe_TFT.Transferencia();
            var responseDetail = new Sinpe_TFT.RespuestaTransaccion();
            var bodyWCF = new Sinpe_TFT.RegistrarDirectoDebitoCuentaBody();

            string detalle = "";

            try
            {
                body.Rastro = new Sinpe_TFT.Rastro();
                detalle = (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5)
                    .Substring(0, Math.Min(255,
                        (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5).Length));

                if (detalle.Length < 15)
                {
                    detalle = (detalle.Replace("\r\n", "") + " Transferencia SINPE")
                             .Substring(0, Math.Min(255, (detalle.Replace("\r\n", "") + " Transferencia SINPE").Length));
                }

                body.Rastro.IP = _parametrosSinpe.vIpHost;
                body.Rastro.Usuario = _parametrosSinpe.vUserCGP;
                body.Rastro.Canal = _parametrosSinpe.vCanalCGP;

                transaccion.DatosDebito = new Sinpe_TFT.Transaccion();
                transaccion.DatosDebito.Moneda = (solicitud.Divisa == "DOL")
                                                    ? Sinpe_TFT.E_Monedas.Dolares
                                                    : Sinpe_TFT.E_Monedas.Colones;

                // Monto
                if (solicitud.Divisa == "DOL")
                {
                    transaccion.DatosDebito.Monto = (solicitud.tipoCambio > 0)
                        ? (solicitud.Monto / solicitud.tipoCambio)
                        : 0;
                    transaccion.DatosDebito.Monto = Math.Round(transaccion.DatosDebito.Monto, 4); // Redondeo para dólares
                }
                else
                {
                    transaccion.DatosDebito.Monto = solicitud.Monto;
                }

                // Otros campos
                transaccion.DatosDebito.Descripcion = detalle.Replace("\r\n", "");
                transaccion.DatosDebito.Servicio = "TFT";
                transaccion.DatosDebito.PuntoIntegracion = 1;
                transaccion.DatosDebito.CentroCosto = 1;
                transaccion.DatosDebito.CodigoConcepto = 1;
                transaccion.DatosDebito.EntidadOrigen = 205;
                transaccion.DatosDebito.FirmaDigital = false;
                transaccion.DatosDebito.IDCorrelation = "1";
                transaccion.DatosDebito.eMAIL = solicitud.CorreoNotifica.Trim();

                transaccion.ClienteOrigen = new Sinpe_TFT.Cliente
                {
                    Identificacion = fxFormatoIdentificacionSinpe(solicitud.CedulaOrigen.Trim(), solicitud.tipoCedOrigen.GetHashCode()).Result,
                    Nombre = solicitud.NombreOrigen,
                    IBAN = solicitud.CuentaOrigen,
                };

                transaccion.ClienteDestino = new Sinpe_TFT.Cliente
                {
                    Identificacion = fxFormatoIdentificacionSinpe(solicitud.Codigo.Trim(), solicitud.tipoCedDestino.GetHashCode()).Result,
                    Nombre = solicitud.Beneficiario,
                    IBAN = solicitud.Cuenta,
                };

                try
                {
                    body.Transacciones = new Sinpe_TFT.Transferencia[] { transaccion };
                    bodyWCF.body = body;
                    bodyWCF.rastro = fxCrearRastroSINPESIF_TFT().Result;
                    response = _srvSinpeTft.EnvioDirectoDebitoCuentaAsync(bodyWCF).Result;
                    responseDetail = response.EnvioDirectoDebitoCuentaResult[0];
                    resp.Result = responseDetail;
                }
                catch (Exception)
                {
                    responseDetail.MotivoError = 201;
                    resp.Result = responseDetail;
                }


            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al procesar la solicitud de emisión de SINPE Tiempo Real.";
                resp.Result = new Sinpe_TFT.RespuestaRegistro();
            }

            return resp;
        }

        #endregion

        #region Factura Electrónica
        /// <summary>
        /// Este se utiliza para el modulo de cajas al pagar IVA. 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCedula"></param>
        /// <param name="pNumeroDocumento"></param>
        /// <param name="pTipoDoc"></param>
        /// <param name="pTipoDocEletronico"></param>
        /// <param name="pNotas"></param>
        /// <param name="pTipoTramite"></param>
        /// <returns></returns>
        public ErrorDTO<bool> GenerarFacturacionElectronica(int CodEmpresa,
            string pCedula  , 
            string pNumeroDocumento, string pTipoDoc,
            byte pTipoDocEletronico, string pNotas, string pTipoTramite)
        {
            var response = new ErrorDTO<bool>();
            try
            {
                bool Respuesta = false;
                var Facturacion = new FactElectronica.FE_Facturacion();

                Facturacion.Encabezado = ObtieneEncabezado(CodEmpresa,pCedula, pNumeroDocumento, pTipoDoc, pTipoDocEletronico).Result;
                Facturacion.Detalles = ObtieneDetalles(CodEmpresa,pNumeroDocumento, pTipoDoc, pTipoTramite).Result;
                Facturacion.Parametros = GenerarParametros(pNumeroDocumento, pTipoDoc, pNotas).Result;

                response.Result = _srvFactElectronica.ProcesarDocumentoAsync(Facturacion).Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }
            return response;
        }

        private ErrorDTO<FactElectronica.FE_JsonEncabezado> ObtieneEncabezado (int CodEmpresa, string pCedula,
            string pNumComprobante, string pTipoDoc,
            byte pTipoDocEletronico)
        {
            var response = new ErrorDTO<FactElectronica.FE_JsonEncabezado>();
            try
            {
                var EncabezadoServicio = new FactElectronica.FE_JsonEncabezado();
                var parametrosEncabezado = new FE_ParametrosEncabezado();

                EncabezadoServicio.FechaFactura = DateTime.Now;
                EncabezadoServicio.TipoDoc = pTipoDocEletronico;
                EncabezadoServicio.SituacionEnvio = (byte)E_SituacionEnvio.Normal;

                parametrosEncabezado = ObtieneParametrosEncabezado(CodEmpresa).Result;
                EncabezadoServicio.CantDeci = parametrosEncabezado.CantDeci; //Cantidad de decimales
                EncabezadoServicio.Sucursal = parametrosEncabezado.Sucursal; //Sucursal asignada por GTI
                EncabezadoServicio.CodigoActividad = parametrosEncabezado.CodigoActividad; //Codigo de actividad de la empresa
                EncabezadoServicio.Terminal = parametrosEncabezado.Terminal; //Terminal asignada por GTI

                EncabezadoServicio.Moneda = (short)E_Moneda.Colones;
                EncabezadoServicio.MedioPago = ObtieneMediosPago(CodEmpresa, pNumComprobante, pTipoDoc).Result;
                EncabezadoServicio.CondicionVenta = (byte)E_CondicionVenta.Contado;

                if(pTipoDocEletronico == 1)
                {
                    EncabezadoServicio.Receptor = receptorValidado(CodEmpresa, pCedula).Result;
                }
                else
                {
                    EncabezadoServicio.Receptor = null;
                }

                if(EncabezadoServicio.Receptor == null && pTipoDocEletronico == 1)
                {
                    EncabezadoServicio = null;
                }
                    
                response.Result = EncabezadoServicio;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDTO<FE_ParametrosEncabezado> ObtieneParametrosEncabezado(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<FE_ParametrosEncabezado> response = new ErrorDTO<FE_ParametrosEncabezado>();
            response.Result = new FE_ParametrosEncabezado();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFE_ObtieneParametrosEncabezado ";
                    response.Result = connection.Query<FE_ParametrosEncabezado>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FE_ParametrosEncabezado();
            }

            return response;
        }

        private ErrorDTO<short[]> ObtieneMediosPago(int CodEmpresa, string pNumComprobante, string pTipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<short[]> response = new ErrorDTO<short[]>();
            response.Result = new short[0];
            try
            {
                //Info de pruebas
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFE_ObtieneMedioPagos '{pNumComprobante}', '{pTipoDocumento}' ";
                    response.Result = connection.Query<short>(query).ToArray();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new short[0];
            }

            return response;
        }

        private ErrorDTO<FactElectronica.FE_JsonReceptor> receptorValidado(int CodEmpresa, string pCedula)
        {
            var response = new ErrorDTO<FactElectronica.FE_JsonReceptor>();
            try
            {
                var receptorValServicio = new FactElectronica.FE_JsonReceptor();
                var receptorVal = new FE_Receptor();

                //TODO: Cargar servicio para el modelo y eliminar class
                receptorVal = receptorValidado(CodEmpresa, pCedula, null).Result;
                receptorValServicio.Nombre = receptorVal.Nombre;
                receptorValServicio.Correo = receptorVal.Correo;
                receptorValServicio.TipoIdent = receptorVal.TipoIdent;
                receptorValServicio.Identificacion = receptorVal.Identificacion;

                response.Result = receptorValServicio;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDTO<FE_Receptor> receptorValidado(int CodEmpresa, string pCedula, string? n)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<FE_Receptor>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFE_ObtieneEncabezado '{pCedula}' ";
                    response.Result = connection.Query<FE_Receptor>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDTO<FactElectronica.FE_JsonLineas[]> ObtieneDetalles(int CodEmpresa, string pNumComprobante, string pTipoDoc, string pTipoTramite)
        {
            var response = new ErrorDTO<FactElectronica.FE_JsonLineas[]>();
            try
            {
                var ListaLineaServicio = new FactElectronica.FE_JsonLineas[0];
                var LineaServicio = new FactElectronica.FE_JsonLineas();
                var ListaLinea = new List<FE_Detalles>();
                short Linea = 0;

                ListaLinea = ObtieneDetalles(CodEmpresa, pNumComprobante, pTipoDoc, pTipoTramite, null).Result;


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;

            }
            return response;
        }

        private ErrorDTO<List<FE_Detalles>> ObtieneDetalles(int CodEmpresa, string pNumComprobante, string pTipoDocumento, string pTipoTramite, string? n)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FE_Detalles>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFE_ObtieneDetalle '{pNumComprobante}', '{pTipoDocumento}', '{pTipoTramite}' ";
                    response.Result = connection.Query<FE_Detalles>(query).ToList();

                    if(response.Result != null)
                    {
                        foreach (var item in response.Result)
                        {
                            if(item.PrecioUnitario != "0")
                            {
                                item.Descuentos = ObtieneDescuentos(CodEmpresa, pNumComprobante, pTipoDocumento).Result;

                                item.Impuestos = ObtieneImpuestos(CodEmpresa, pNumComprobante, pTipoDocumento, pTipoTramite, item.NumeroLinea).Result;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDTO<List<FactElectronica.FE_JsonDescuentos>> ObtieneDescuentos(int CodEmpresa ,string pNumComprobante, string pTipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FactElectronica.FE_JsonDescuentos>>();
            var ListaDescuentoServicio = new List<FactElectronica.FE_JsonDescuentos>();
            var DescuentoServicio = new FactElectronica.FE_JsonDescuentos();
            try
            {
                
                //'//-------------------------------------------- //
                //'   Los creditos Jaules no tienen descuento
                //'//-------------------------------------------- //
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec dbo.spFE_ObtieneDescuentos '{pNumComprobante}', '{pTipoDocumento}' ";
                    ListaDescuentoServicio = connection.Query<FactElectronica.FE_JsonDescuentos>(query).ToList();
                }

                response.Result = ListaDescuentoServicio;

            }
            catch (Exception ex)
            {
                DescuentoServicio.MontoDescuento = 0;
                DescuentoServicio.DetalleDescuento = "";
                ListaDescuentoServicio.Add(DescuentoServicio);

                response.Code = -1;
                response.Description = ex.Message;
                response.Result = ListaDescuentoServicio;
            }
            return response;
        }

        private ErrorDTO<List<FactElectronica.FE_JsonImpuestos>> ObtieneImpuestos(int CodEmpresa, string pNumComprobante, string pTipoDocumento, string pTipoTramite, short pNumeroLinea)
        {
            var response = new ErrorDTO<List<FactElectronica.FE_JsonImpuestos>>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec dbo.spFE_ObtieneImpuestos '{pNumComprobante}', '{pTipoDocumento}', '{pTipoTramite}', {pNumeroLinea} ";
                    response.Result = connection.Query<FactElectronica.FE_JsonImpuestos>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        private ErrorDTO<FactElectronica.FE_ParametrosSistemas> GenerarParametros(string pNumeroComprobante, string pTipoDoc, string pNotas)
        {
            var response = new ErrorDTO<FactElectronica.FE_ParametrosSistemas>();
            try
            {
                if(pNotas == "" && pNotas == null)
                {
                    pNotas = "Recibo de Pago ASECCSS";
                }

                response.Result.TipoDocumento = pTipoDoc;
                response.Result.SistemaSiglas = "ProGrx ASECCSS";
                response.Result.NumeroDeComprobante = pNumeroComprobante;
                response.Result.Notas = pNotas;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #endregion

        #region General/Compartido
        /// <summary>
        /// Obtiene Solicitud de Tesorería por número de solicitud.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <returns></returns>
        private ErrorDTO<TesTransaccion> fxTesConsultaSolicitud(int CodEmpresa, int Nsolicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TesTransaccion>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesTransaccion()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
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
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Consulta la información de SINPE para una solicitud específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        private ErrorDTO<vInfoSinpe> fxTesConsultaInfoSinpe(int CodEmpresa, string solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<vInfoSinpe>
            {
                Code = 0,
                Description = "Ok",
                Result = new vInfoSinpe()
            };
            var infoSinpe = new vInfoSinpe();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spTES_W_ConsultaInfoSinpe {solicitud} ";
                    InfoSinpeData res = connection.Query<InfoSinpeData>(query).FirstOrDefault();

                    infoSinpe.Cedula = res.Cedula;
                    infoSinpe.CuentaIBAN = res.Cuenta;
                    infoSinpe.tipoID = res.tipoID;

                    response.Result = infoSinpe;
                }
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
        /// Consulta el motivo de rechazo de una transacción SINPE.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idRechazo"></param>
        /// <returns></returns>
        private ErrorDTO<string> fxTesConsultaMotivo(int CodEmpresa, int idRechazo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<string>
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT DESCRIPCION FROM SINPE_MOTIVOS where COD_MOTIVO = @rechazo ";
                    response.Result = connection.Query<string>(query, new { rechazo = idRechazo }).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = "Motivo desconocido.";
            }
            return response;
        }

        /// <summary>
        /// Actualiza el estado de una transacción SINPE a "Sinpe" o "Rechazada".
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDTO<bool> fxTesRespuestaSinpe(int CodEmpresa, TesTransaccion datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            string nDocumento = "";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    if(datos.IdMotivoRechazo != 201)
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
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = false;
            }
            return response;
        }

        /// <summary>
        /// Establece el código SUGEF estándar basado en el tipo de identificación.
        /// </summary>
        /// <param name="TipoId"></param>
        /// <returns></returns>
        private ErrorDTO<int> setCodigoSugefEstandar(int TipoId)
        {
            var response = new ErrorDTO<int>();
            try
            {
                switch (TipoId)
                {
                    case 1: // Cédula Nacional
                        response.Result = 3;
                        break;
                    case 3: // Dimex
                        response.Result = 1;
                        break;
                    case 4: // Jurídica
                        response.Result = 5;
                        break;
                    case 5: // Institución autónoma
                        response.Result = 4;
                        break;
                    default:
                        response.Result = TipoId;
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error al validar la información del código SUGEF.";
                response.Result = TipoId;
            }
            return response;
        }

        /// <summary>
        /// Envia notificaciones a las cajas para una transacción específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodigoReferencia"></param>
        /// <returns></returns>
        private ErrorDTO<bool> EnviaNotificacionesCajas(int CodEmpresa, string CodigoReferencia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec sp_Sinpe_Notificaciones_Cajas @CODIGO_REFERENCIA";
                    var result = connection.Execute(query, new { CODIGO_REFERENCIA = CodigoReferencia });
                    if (result > 0)
                    {
                        resp.Result = true;
                    }
                    else
                    {
                        resp.Code = -1;
                        resp.Description = "No se enviaron notificaciones a las cajas.";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = $"Error al enviar notificaciones a las cajas: {ex.Message}";
            }
            return resp;
        }
        #endregion
    }
}
