using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using System.Text;

namespace Galileo_API.DataBaseTier
{

   
    /// <summary>
    /// Cliente para consumir los métodos del Servicio DTR de KINDO (Sección 6 del documento).
    /// Estandarizado según el patrón definido por el método final de GetAccountInfo.
    ///
    /// Notas:
    /// - Todos los métodos retornan tipos fuertemente tipados (Res*).
    /// - Firma uniforme: (int codEmpresa, string usuario, Req* data).
    /// - Serialización via Newtonsoft.Json y POST a los endpoints especificados.
    /// - Manejo de errores consistente con List&lt;Error&gt;.
    /// - Requiere que exista el método GetUriEmpresa(int codEmpresa, int operacion, string usuario)
    ///   que devuelva un resultado con Item1.vUriCGP, Item1.vHostPin, Item1.vIpHost, Item1.vUsuarioLog, etc.
    /// </summary>
    public class SinpeGalileoDtr
    {
        private readonly HttpClient _client;
        private readonly string strMediaType = "application/json";

        public SinpeGalileoDtr(IConfiguration config)
        {
            _client = new HttpClient();
        }

        #region 6.x IsServiceAvailable
        /// <summary>
        /// Este método permite verificar si el Servicio DTR de KINDO se encuentra disponible para procesar transacciones.
        /// Endpoint: /IsServiceAvailable
        /// </summary>
        public ResServiceAvailable IsServiceAvailable(string UrlCGP_DTR, ReqBase context)
        {

            try
            {

                var json = JsonConvert.SerializeObject(context);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(UrlCGP_DTR + "/IsServiceAvailable", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ResServiceAvailable>(jsonResponse);

                    return result;
                }
                else
                {
                    return new ResServiceAvailable
                    {
                        IsSuccessful = false,
                        Errors = new Error[]
                        {
                            new Error { Code = (int)response.StatusCode, Message = "El servicio no está disponible." }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResServiceAvailable
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en IsServiceAvailable: {ex.Message}" }
                    }
                };
            }
        }
        #endregion

        #region 6.x GetAccountInfo
        /// <summary>
        /// ResAccountInfo GetAccountInfo (ReqAccountInfo AccountData)
        /// Este método permite consultar la información de una cuenta IBAN en otra Entidad Financiera.
        /// Endpoint: /GetAccountInfo
        /// </summary>
        public ResAccountInfo GetAccountInfo(string UrlCGP_DTR, ReqAccountInfo accountData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(accountData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(UrlCGP_DTR + "/GetAccountInfo", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ResAccountInfo>(jsonResponse);

                    return result;
                }
                else
                {
                    return new ResAccountInfo
                    {
                        IsSuccessful = false,
                        Errors = new Error[]
                        {
                            new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener la información de la cuenta." }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResAccountInfo
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en GetAccountInfo: {ex.Message}" }
                    }
                };
            }
        }
        #endregion

        #region 6.x SendDebit
        /// <summary>
        /// ResDTRSending SendDebit (ReqDTRSending DebitData)
        /// Este método permite enviar un Débito en Tiempo Real a cualquiera de las Entidades Financieras participantes.
        /// Endpoint: /SendDebit
        /// </summary>
        public ResDTRSending SendDebit(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqDTRSending debitData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(debitData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/SendDebit", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ResDTRSending>(jsonResponse);

                    return new ResDTRSending
                    {
                        IsSuccessful = true,
                        OperationId = result.OperationId,
                        DTRSendingResult = result.DTRSendingResult
                    };
                }

                return new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo enviar el débito." } }
                };
            }
            catch (Exception ex)
            {
                return new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = -1, Message = $"Error en SendDebit: {ex.Message}" } }
                };
            }
        }
        #endregion

        #region 6.x GetDebitResult
        /// <summary>
        /// ResDTRSending GetDebitResult (ReqDTRInfoChannelRef DebitData)
        /// Consulta los datos del resultado de envío de un DTR con referencia de canal.
        /// Endpoint: /GetDebitResult
        /// </summary>
        public ResDTRSending GetDebitResult(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqDTRInfoChannelRef debitData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(debitData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetDebitResult", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResDTRSending>(jsonResponse);
                }

                return new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener el resultado del DTR." } }
                };
            }
            catch (Exception ex)
            {
                return new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetDebitResult: {ex.Message}" } }
                };
            }
        }
        #endregion

        #region 6.x GetDebitDataByChannelRef
        /// <summary>
        /// ResDTRInfo GetDebitDataByChannelRef (ReqDTRInfoChannelRef DebitData)
        /// Consulta datos y resultado de envío de un DTR utilizando la referencia interna del canal.
        /// Endpoint: /GetDebitDataByChannelRef
        /// </summary>
        public ResDTRInfo GetDebitDataByChannelRef(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqDTRInfoChannelRef debitData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(debitData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetDebitDataByChannelRef", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResDTRInfo>(jsonResponse);
                }

                return new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo consultar el DTR por referencia de canal." } }
                };
            }
            catch (Exception ex)
            {
                return new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetDebitDataByChannelRef: {ex.Message}" } }
                };
            }
        }
        #endregion

        #region 6.x GetDebitDataBySINPERef
        /// <summary>
        /// ResDTRInfo GetDebitDataBySINPERef (ReqDTRInfoSINPERef DebitData)
        /// Consulta datos y resultado de envío de un DTR utilizando el número de referencia SINPE.
        /// Endpoint: /GetDebitDataBySINPERef
        /// </summary>
        public ResDTRInfo GetDebitDataBySINPERef(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqDTRInfoSINPERef debitData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(debitData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetDebitDataBySINPERef", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResDTRInfo>(jsonResponse);
                }

                return new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo consultar el DTR por referencia SINPE." } }
                };
            }
            catch (Exception ex)
            {
                return new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetDebitDataBySINPERef: {ex.Message}" } }
                };
            }
        }
        #endregion

        #region 6.x SendBatch
        /// <summary>
        /// ResBatchSending SendBatch (ReqBatchSending BatchData)
        /// Solicita el envío de un lote de DTRs. El procesamiento es asincrónico; consultar luego con GetBatchState.
        /// Endpoint: /SendBatch
        /// </summary>
        public ResBatchSending SendBatch(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqBatchSending batchData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(batchData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/SendBatch", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResBatchSending>(jsonResponse);
                }

                return new ResBatchSending
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "Error al enviar el lote de DTRs." } }
                };
            }
            catch (Exception ex)
            {
                return new ResBatchSending
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = -1, Message = $"Error en SendBatch: {ex.Message}" } }
                };
            }
        }
        #endregion

        #region 6.x GetBatchState
        /// <summary>
        /// ResBatchState GetBatchState (ReqBatchState BatchData)
        /// Consulta el estado actual de procesamiento de un lote de DTRs.
        /// Endpoint: /GetBatchState
        /// </summary>
        public ResBatchState GetBatchState(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqBatchState batchData)
        {

            try
            {
                var json = JsonConvert.SerializeObject(batchData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetBatchState", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResBatchState>(jsonResponse);
                }

                return new ResBatchState
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener el estado del lote." } }
                };
            }
            catch (Exception ex)
            {
                return new ResBatchState
                {
                    IsSuccessful = false,
                    Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetBatchState: {ex.Message}" } }
                };
            }
        }
        #endregion

        #region 6.x GetCustomerDebits
        /// <summary>
        /// ResCustomerDebits GetCustomerDebits (ReqCustomerDebits ConsultData)
        /// Obtiene todos los débitos en tiempo real que ha enviado o recibido un cliente en un rango de fechas.
        /// Endpoint: /GetCustomerDebits
        /// </summary>
        public ResCustomerDebits GetCustomerDebits(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqCustomerDebits consultData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(consultData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetCustomerDebits", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResCustomerDebits>(jsonResponse);
                }

                return new ResCustomerDebits
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener la lista de débitos del cliente." }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResCustomerDebits
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en GetCustomerDebits: {ex.Message}" }
                    }
                };
            }
        }
        #endregion

        #region 6.x GetAllDebits
        /// <summary>
        /// ResAllDebits GetAllDebits (ReqAllDebits FilterData)
        /// Obtiene todos los débitos registrados en un rango de fechas determinado (con paginación).
        /// Endpoint: /GetAllDebits
        /// </summary>
        public ResAllDebits GetAllDebits(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqAllDebits filterData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(filterData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetAllDebits", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResAllDebits>(jsonResponse);
                }

                return new ResAllDebits
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener la lista de todos los débitos." }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResAllDebits
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en GetAllDebits: {ex.Message}" }
                    }
                };
            }
        }
        #endregion

        #region 6.x RegisterAuthorization
        /// <summary>
        /// ResBase RegisterAuthorization (ReqCustomerServiceAuthorization pReqCustomerServiceAuthorization)
        /// Registra una autorización de un cliente para el Servicio DTR de KINDO.
        /// Endpoint: /RegisterAuthorization
        /// </summary>
        public ResBase RegisterAuthorization(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqCustomerServiceAuthorization authorizationData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(authorizationData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/RegisterAuthorization", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResBase>(jsonResponse);
                }

                return new ResBase
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = (int)response.StatusCode, Message = "No se pudo registrar la autorización del cliente." }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResBase
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en RegisterAuthorization: {ex.Message}" }
                    }
                };
            }
        }
        #endregion

        #region 6.x InactivateAuthorization
        /// <summary>
        /// ResBase InactivateAuthorization (ReqCustomerServiceAuthorization pReqCustomerServiceAuthorization)
        /// Inactiva una autorización de un cliente para el Servicio DTR de KINDO.
        /// Endpoint: /InactivateAuthorization
        /// </summary>
        public ResBase InactivateAuthorization(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqCustomerServiceAuthorization authorizationData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(authorizationData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/InactivateAuthorization", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResBase>(jsonResponse);
                }

                return new ResBase
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = (int)response.StatusCode, Message = "No se pudo inactivar la autorización del cliente." }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResBase
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en InactivateAuthorization: {ex.Message}" }
                    }
                };
            }
        }
        #endregion

        #region 6.x GetStateAuthorization
        /// <summary>
        /// ResCustomerServiceAuthorization GetStateAuthorization (ReqCustomerServiceAuthorization pReqCustomerServiceAuthorization)
        /// Consulta si una persona está autorizada o no para el Servicio DTR de KINDO.
        /// Endpoint: /GetStateAuthorization
        /// </summary>
        public ResCustomerServiceAuthorization GetStateAuthorization(ErrorDto<(ParametrosSinpe, HttpClient)> parametros, ReqCustomerServiceAuthorization authorizationData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(authorizationData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_DTR + "/GetStateAuthorization", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject<ResCustomerServiceAuthorization>(jsonResponse);
                }

                return new ResCustomerServiceAuthorization
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener el estado de autorización del cliente." }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResCustomerServiceAuthorization
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                    {
                        new Error { Code = -1, Message = $"Error en GetStateAuthorization: {ex.Message}" }
                    }
                };
            }
        }
        #endregion
    }
}
