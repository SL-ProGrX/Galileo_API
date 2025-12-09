using Newtonsoft.Json;
using Galileo.Models.KindoSinpe;
using System.Text;

namespace Galileo_API.DataBaseTier
{
    public class SinpeGalileoPin
    {
        private readonly HttpClient _client;
        private readonly string strMediaType = "application/json";

        public SinpeGalileoPin(IConfiguration config)
        {
            _client = new HttpClient();
        }

        #region 6.x IsServiceAvailable
        /// <summary>
        /// ResServiceAvailable IsServiceAvailable (ReqBase Context)
        /// Verifica si el Servicio PIN de KINDO se encuentra disponible para procesar transacciones.
        /// Endpoint: /IsServiceAvailable
        /// </summary>
        public ResServiceAvailable IsServiceAvailable(string UrlCGP_PIN, ReqBase context)
        {
            try
            {
                var json = JsonConvert.SerializeObject(context);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(UrlCGP_PIN + "/IsServiceAvailable", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ResServiceAvailable>(jsonResponse);

                    return new ResServiceAvailable
                    {
                        IsSuccessful = true,
                        OperationId = result.OperationId,
                        ServiceAvailable = result.ServiceAvailable
                    };
                }
                else
                {
                    return new ResServiceAvailable
                    {
                        IsSuccessful = false,
                        Errors = new Error[]
                        {
                            new Error { Code = (int)response.StatusCode, Message = "Error al verificar la disponibilidad del servicio." }
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
        /// Consulta la información de una cuenta IBAN en otra Entidad Financiera.
        /// Endpoint: /GetAccountInfo
        /// </summary>
        public ResAccountInfo GetAccountInfo(string UrlCGP_PIN, ReqAccountInfo accountData)
        {
            try
            {
                var json = JsonConvert.SerializeObject(accountData);
                var content = new StringContent(json, Encoding.UTF8, strMediaType);

                var response = _client.PostAsync(UrlCGP_PIN + "/GetAccountInfo", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<ResAccountInfo>(jsonResponse);

                    return
                    new ResAccountInfo
                    {
                        IsSuccessful = result.IsSuccessful,
                        OperationId = result.OperationId,
                        Account = result.Account
                    };
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

        //#region 6.x SendPIN
        ///// <summary>
        ///// ResPINSending SendPIN (ReqPINSending PINData)
        ///// Envía una transacción PIN a una Entidad Financiera participante.
        ///// Endpoint: /SendPIN
        ///// </summary>
        //public ResPINSending SendPIN( ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqPINSending pinData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(pinData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/SendPIN", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            var result = JsonConvert.DeserializeObject<ResPINSending>(jsonResponse);

        //            return new ResPINSending
        //            {
        //                IsSuccessful = true,
        //                OperationId = result.OperationId,
        //                PINSendingResult = result.PINSendingResult
        //            };
        //        }

        //        return new ResPINSending
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[]{ new Error { Code = (int)response.StatusCode, Message = "No se pudo enviar la transacción PIN." } }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResPINSending
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = -1, Message = $"Error en SendPIN: {ex.Message}" } }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x GetPINResult
        ///// <summary>
        ///// ResPINSending GetPINResult (ReqTransferInfoChannelRef PINData)
        ///// Consulta el resultado de envío de una transacción PIN usando la referencia del canal.
        ///// Endpoint: /GetPINResult
        ///// </summary>
        //public ResPINSending GetPINResult( ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqTransferInfoChannelRef pinData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(pinData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/GetPINResult", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResPINSending>(jsonResponse);
        //        }

        //        return new ResPINSending
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener el resultado de la transacción PIN." } }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResPINSending
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetPINResult: {ex.Message}" } }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x GetPINDataByChannelRef
        ///// <summary>
        ///// ResTransferInfo GetPINDataByChannelRef (ReqTransferInfoChannelRef PINData)
        ///// Consulta datos y resultado de una transacción PIN usando la referencia interna del canal.
        ///// Endpoint: /GetPINDataByChannelRef
        ///// </summary>
        //public ResTransferInfo GetPINDataByChannelRef( ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqTransferInfoChannelRef pinData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(pinData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/GetPINDataByChannelRef", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResTransferInfo>(jsonResponse);
        //        }

        //        return new ResTransferInfo
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo consultar la transacción PIN por referencia de canal." } }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResTransferInfo
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetPINDataByChannelRef: {ex.Message}" } }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x GetPINDataBySINPERef
        ///// <summary>
        ///// ResTransferInfo GetPINDataBySINPERef (ReqTransferInfoSINPERef PINData)
        ///// Consulta datos y resultado de una transacción PIN usando el número de referencia SINPE.
        ///// Endpoint: /GetPINDataBySINPERef
        ///// </summary>
        //public ResTransferInfo GetPINDataBySINPERef( ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqTransferInfoSINPERef pinData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(pinData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/GetPINDataBySINPERef", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResTransferInfo>(jsonResponse);
        //        }

        //        return new ResTransferInfo
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo consultar la transacción PIN por referencia SINPE." } }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResTransferInfo
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetPINDataBySINPERef: {ex.Message}" } }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x SendBatch
        ///// <summary>
        ///// ResBatchSending SendBatch (ReqBatchSending BatchData)
        ///// Solicita el envío de un lote de transacciones PIN. El procesamiento puede ser asincrónico.
        ///// Endpoint: /SendBatch
        ///// </summary>
        //public ResBatchSending SendBatch( ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqBatchSending batchData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(batchData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/SendBatch", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResBatchSending>(jsonResponse);
        //        }

        //        return new ResBatchSending
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "Error al enviar el lote de transacciones PIN." } }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResBatchSending
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = -1, Message = $"Error en SendBatch: {ex.Message}" } }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x GetBatchState
        ///// <summary>
        ///// ResBatchState GetBatchState (ReqBatchState BatchData)
        ///// Consulta el estado actual de procesamiento de un lote de transacciones PIN.
        ///// Endpoint: /GetBatchState
        ///// </summary>
        //public ResBatchState GetBatchState(ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqBatchState batchData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(batchData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/GetBatchState", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResBatchState>(jsonResponse);
        //        }

        //        return new ResBatchState
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener el estado del lote." } }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResBatchState
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[] { new Error { Code = -1, Message = $"Error en GetBatchState: {ex.Message}" } }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x GetCustomerTransfers
        ///// <summary>
        ///// ResCustomerTransfers GetCustomerTransfers (ReqCustomerTransfers ConsultData)
        ///// Obtiene todas las transferencias PIN que ha enviado o recibido un cliente en un rango de fechas.
        ///// Endpoint: /GetCustomerTransfers
        ///// </summary>
        //public ResCustomerTransfers GetCustomerTransfers(ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqCustomerTransfers consultData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(consultData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/GetCustomerTransfers", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResCustomerTransfers>(jsonResponse);
        //        }

        //        return new ResCustomerTransfers
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[]
        //            {
        //                new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener la lista de transferencias del cliente." }
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResCustomerTransfers
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[]
        //            {
        //                new Error { Code = -1, Message = $"Error en GetCustomerTransfers: {ex.Message}" }
        //            }
        //        };
        //    }
        //}
        //#endregion

        //#region 6.x GetAllTransfers
        ///// <summary>
        ///// ResAllTransfers GetAllTransfers (ReqAllTransfers FilterData)
        ///// Obtiene todas las transferencias PIN registradas en un rango de fechas determinado (con paginación).
        ///// Endpoint: /GetAllTransfers
        ///// </summary>
        //public ResAllTransfers GetAllTransfers(ErrorDto<(Models.KindoSinpe.ParametrosSinpe, HttpClient)> parametros, ReqAllTransfers filterData)
        //{
        //    try
        //    {
        //        var json = JsonConvert.SerializeObject(filterData);
        //        var content = new StringContent(json, Encoding.UTF8, "application/json");

        //        var response = _client.PostAsync(parametros.Result.Item1.UrlCGP_PIN + "/GetAllTransfers", content).Result;

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonResponse = response.Content.ReadAsStringAsync().Result;
        //            return JsonConvert.DeserializeObject<ResAllTransfers>(jsonResponse);
        //        }

        //        return new ResAllTransfers
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[]
        //            {
        //                new Error { Code = (int)response.StatusCode, Message = "No se pudo obtener la lista de todas las transferencias." }
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResAllTransfers
        //        {
        //            IsSuccessful = false,
        //            Errors = new Error[]
        //            {
        //                new Error { Code = -1, Message = $"Error en GetAllTransfers: {ex.Message}" }
        //            }
        //        };
        //    }
        //}
        //#endregion
    }
}
