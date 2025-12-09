using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public class CoopeSanGabrielValidator
    {
        private readonly IConfiguration _config;
        private ErrorDto<Galileo.Models.KindoSinpe.ParametrosSinpe> _parametrosSinpe;
        public InfoSinpeRequest _infoSinpe = new InfoSinpeRequest();
        private readonly SinpeGalileo_DTR _sinpeDTR;
        //private readonly SinpeGalileo_PIN _sinpePIN; de momento no se usa

        private readonly mKindoServiceDB _mKindo;

        public Guid OperationId;

        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _config = config;
            _mKindo = new mKindoServiceDB(_config);
            _sinpeDTR = new SinpeGalileo_DTR(_config);
           // _sinpePIN = new SinpeGalileo_PIN(_config);
            _parametrosSinpe = new ErrorDto<ParametrosSinpe>();

            OperationId = Guid.NewGuid();
        }
        #region Validación de Solicitud SINPE
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            var response = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario);

            try
            {
                _infoSinpe.vInfo = new vInfoSinpe();
                var cntInfoSinpe = _mKindo.fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
                if (cntInfoSinpe.Code == -1)
                {
                    response.Code = cntInfoSinpe.Code;
                    response.Description = cntInfoSinpe.Description;
                    return response;
                }
                _infoSinpe.vInfo = cntInfoSinpe.Result;

                if (System.String.IsNullOrEmpty(_infoSinpe.vInfo.Cedula) == false && System.String.IsNullOrEmpty(_infoSinpe.vInfo.CuentaIBAN) == false)
                {
                    //Valido cuenta IBAN
                    if (_mKindo.IsValidCostaRicaIBAN(_infoSinpe.vInfo.CuentaIBAN))
                    {
                        //Valido si el servicio esta disponible
                        ReqBase context = new ReqBase
                        {
                            HostId = _parametrosSinpe.Result.vHostPin,
                            OperationId = OperationId.ToString(),
                            ClientIPAddress = _parametrosSinpe.Result.vIpHost,
                            CultureCode = "ES-CR",
                            UserCode = _parametrosSinpe.Result.vUsuarioLog,
                        };

                        var servicio = _sinpeDTR.IsServiceAvailable(_parametrosSinpe.Result.UrlCGP_DTR, context);
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
                                AccountNumber = _infoSinpe.vInfo.CuentaIBAN
                            };
                            var cuenta = _sinpeDTR.GetAccountInfo(_parametrosSinpe.Result.UrlCGP_DTR, accountData);
                            //if(!cuenta)
                            //{
                            //    if(cuenta.Errors.Count > 0)
                            //    {
                            //        response.Description = "";
                            //        foreach (var err in cuenta.Errors)
                            //        {
                            //            response.Description += err.Message + ", ";
                            //        }

                            //        if (string.IsNullOrEmpty(response.Description))
                            //        {
                            //            response.Description = solicitud.ToString() + " - " + "Error al obtener información de la cuenta.";
                            //        }
                            //        //elimino la ultima coma
                            //        response.Description = response.Description.TrimEnd(',', ' ');

                            //        response.Code = -1;
                            //    }
                            //}
                            //else
                            //{

                            //    response.Description = $@"La cuenta IBAN {_infoSinpe.vInfo.CuentaIBAN} registrada a 
                            //            nombre de {cuenta.Account.Holder} cédula: {cuenta.Account.HolderId} Tipo Id: {_infoSinpe.vInfo.tipoID} 
                            //            Tipo de Moneda: {cuenta.Account.CurrencyCode} Entidad: {cuenta.Account.EntityCode}-{cuenta.Account.EntityName}";

                            //    //Valido tipo de cuenta
                            //    var divisa = _mKindo.GetCurrencyIsoCode(_infoSinpe.vInfo.cod_divisa);
                            //    if(divisa != cuenta.Account.CurrencyCode)
                            //    {
                            //        return new ErrorDto
                            //        {
                            //            Code = -1,
                            //            Description = response.Description + " - No. "+ solicitud.ToString() + " - " + "La divisa de la cuenta no es válida."
                            //        };
                            //    }   

                            //    //Valida cedula
                            //    var cedula = _mKindo.IsValidCostaRicaId(_infoSinpe.vInfo.Cedula, cuenta.Account.HolderIdType);
                            //    if (!cedula)
                            //    {
                            //        return new ErrorDto
                            //        {
                            //            Code = -1,
                            //            Description = response.Description + " - No. " + solicitud.ToString() + " - " + "La cédula no es válida."
                            //        };
                            //    }
                            //    //Valido si la cedula perteneca a la cuenta, IMPLEMENTAR DESPUES
                            //    //if(_infoSinpe.vInfo.Cedula.ToString().Trim() != cuenta.Account.HolderId)
                            //    //{
                            //    //    return new ErrorDto
                            //    //    {
                            //    //        Code = -1,
                            //    //        Description = solicitud.ToString() + " - " + "La cédula no es válida."
                            //    //    };  
                            //    //}
                            //}
                        }
                        else
                        {
                            //if (servicio.Errors.Count > 0)
                            //{
                            //    foreach (var err in servicio.Errors)
                            //    {
                            //        response.Description += err.Message;
                            //    }

                            //    if (string.IsNullOrEmpty(response.Description))
                            //    {
                            //        response.Description = solicitud.ToString() + " - " + "Error al obtener información de la cuenta.";
                            //    }

                            //    response.Code = -1;
                            //}

                            response.Code = -1;
                            response.Description = servicio.Errors[0].Message;

                        }
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = "Cuenta IBAN no Valida";
                    }
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Ocurrió un problema con la validación. - " + ex.Message;
            }
            return response;

        }


        #endregion
    }
}
