using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public class CoopeSanGabrielValidator
    {

        private readonly InfoSinpeRequest _infoSinpe = new InfoSinpeRequest();
        private readonly SinpeGalileoDtr _sinpeDTR;

        private readonly MKindoServiceDb _mKindo;

        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _mKindo = new MKindoServiceDb(config);
            _sinpeDTR = new SinpeGalileoDtr(config);
        }

        #region Validación de Solicitud SINPE
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            var response = OkResponse();

            var parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario);

            try
            {
                var infoSinpeResult = CargarInfoSinpe(CodEmpresa, solicitud);
                if (infoSinpeResult.Code == -1)
                    return ErrorResponse(infoSinpeResult.Description);

                _infoSinpe.vInfo = infoSinpeResult.Result;

                if (!TieneDatosMinimos(_infoSinpe.vInfo))
                    return response; // mismo comportamiento que tu código: si no hay datos, regresa Ok

                if (!MKindoServiceDb.IsValidCostaRicaIBAN(_infoSinpe.vInfo.CuentaIBAN))
                    return ErrorResponse("Cuenta IBAN no Valida");

                var context = CrearContexto(parametrosSinpe);

                var servicio = _sinpeDTR.IsServiceAvailable(parametrosSinpe.Result.UrlCGP_DTR, context);
                if (!servicio.IsSuccessful)
                    return ErrorResponse(servicio.Errors?[0]?.Message ?? "Servicio no disponible");

                var cuenta = ConsultarCuenta(parametrosSinpe, context, _infoSinpe.vInfo.CuentaIBAN);
                if (!cuenta.IsSuccessful)
                {
                    // tu código no hace nada aquí (comentado), así que mantenemos el comportamiento.
                    return response;
                }

                // aquí irían validaciones futuras (divisa, cédula, etc.)
                return response;
            }
            catch (Exception ex)
            {
                return ErrorResponse("Ocurrió un problema con la validación. - " + ex.Message);
            }
        }

        private static ErrorDto OkResponse() =>
    new ErrorDto { Code = 0, Description = "Ok" };

        private static ErrorDto ErrorResponse(string description) =>
            new ErrorDto { Code = -1, Description = description };

        private ErrorDto<vInfoSinpe> CargarInfoSinpe(int CodEmpresa, string solicitud)
        {
            _infoSinpe.vInfo = new vInfoSinpe();

            var cntInfoSinpe = _mKindo.fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
            return cntInfoSinpe;
        }

        private static bool TieneDatosMinimos(vInfoSinpe info) =>
            !string.IsNullOrEmpty(info?.Cedula) &&
            !string.IsNullOrEmpty(info?.CuentaIBAN);

        private static ReqBase CrearContexto(ErrorDto<ParametrosSinpe> parametrosSinpe) =>
            new ReqBase
            {
                HostId = parametrosSinpe.Result.vHostPin,
                OperationId = Guid.NewGuid().ToString(), // o usa tu OperationId de campo si es requerido por negocio
                ClientIPAddress = parametrosSinpe.Result.vIpHost,
                CultureCode = "ES-CR",
                UserCode = parametrosSinpe.Result.vUsuarioLog,
            };

        private ResAccountInfo ConsultarCuenta(
            ErrorDto<ParametrosSinpe> parametrosSinpe,
            ReqBase context,
            string cuentaIban)
        {
            var accountData = new ReqAccountInfo
            {
                HostId = context.HostId,
                OperationId = context.OperationId,
                ClientIPAddress = context.ClientIPAddress,
                CultureCode = context.CultureCode,
                UserCode = context.UserCode,
                Id = null,
                AccountNumber = cuentaIban
            };

            return _sinpeDTR.GetAccountInfo(parametrosSinpe.Result.UrlCGP_DTR, accountData);
        }


        //public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        //{
        //    var response = new ErrorDto()
        //    {
        //        Code = 0,
        //        Description = "Ok"
        //    };
        //    var _parametrosSinpe = new ErrorDto<ParametrosSinpe>();
        //    _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario);

        //    try
        //    {
        //        _infoSinpe.vInfo = new vInfoSinpe();
        //        var cntInfoSinpe = _mKindo.fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
        //        if (cntInfoSinpe.Code == -1)
        //        {
        //            response.Code = cntInfoSinpe.Code;
        //            response.Description = cntInfoSinpe.Description;
        //            return response;
        //        }
        //        _infoSinpe.vInfo = cntInfoSinpe.Result;

        //        if (!System.String.IsNullOrEmpty(_infoSinpe.vInfo.Cedula) && !System.String.IsNullOrEmpty(_infoSinpe.vInfo.CuentaIBAN))
        //        {
        //            //Valido cuenta IBAN
        //            if (MKindoServiceDb.IsValidCostaRicaIBAN(_infoSinpe.vInfo.CuentaIBAN))
        //            {
        //                //Valido si el servicio esta disponible
        //                ReqBase context = new ReqBase
        //                {
        //                    HostId = _parametrosSinpe.Result.vHostPin,
        //                    OperationId = OperationId.ToString(),
        //                    ClientIPAddress = _parametrosSinpe.Result.vIpHost,
        //                    CultureCode = "ES-CR",
        //                    UserCode = _parametrosSinpe.Result.vUsuarioLog,
        //                };

        //                var servicio = _sinpeDTR.IsServiceAvailable(_parametrosSinpe.Result.UrlCGP_DTR, context);
        //                if (servicio.IsSuccessful)
        //                {
        //                    //Valido informacion de la cuenta
        //                    ReqAccountInfo accountData = new ReqAccountInfo
        //                    {
        //                        HostId = context.HostId,
        //                        OperationId = context.OperationId,
        //                        ClientIPAddress = context.ClientIPAddress,
        //                        CultureCode = context.CultureCode,
        //                        UserCode = context.UserCode,
        //                        Id = null,
        //                        AccountNumber = _infoSinpe.vInfo.CuentaIBAN
        //                    };
        //                    var cuenta = _sinpeDTR.GetAccountInfo(_parametrosSinpe.Result.UrlCGP_DTR, accountData);
        //                    if (!cuenta.IsSuccessful)
        //                    {
        //                        //if (cuenta.Errors.Count > 0)
        //                        //{
        //                        //    response.Description = "";
        //                        //    foreach (var err in cuenta.Errors)
        //                        //    {
        //                        //        response.Description += err.Message + ", ";
        //                        //    }

        //                        //    if (string.IsNullOrEmpty(response.Description))
        //                        //    {
        //                        //        response.Description = solicitud.ToString() + " - " + "Error al obtener información de la cuenta.";
        //                        //    }
        //                        //    //elimino la ultima coma
        //                        //    response.Description = response.Description.TrimEnd(',', ' ');

        //                        //    response.Code = -1;
        //                        //}
        //                    }
        //                    else
        //                    {

        //                        //response.Description = $@"La cuenta IBAN {_infoSinpe.vInfo.CuentaIBAN} registrada a 
        //                        //        nombre de {cuenta.Account.Holder} cédula: {cuenta.Account.HolderId} Tipo Id: {_infoSinpe.vInfo.tipoID} 
        //                        //        Tipo de Moneda: {cuenta.Account.CurrencyCode} Entidad: {cuenta.Account.EntityCode}-{cuenta.Account.EntityName}";

        //                        ////Valido tipo de cuenta
        //                        //var divisa = _mKindo.GetCurrencyIsoCode(_infoSinpe.vInfo.cod_divisa);
        //                        //if (divisa != cuenta.Account.CurrencyCode)
        //                        //{
        //                        //    return new ErrorDto
        //                        //    {
        //                        //        Code = -1,
        //                        //        Description = response.Description + " - No. " + solicitud.ToString() + " - " + "La divisa de la cuenta no es válida."
        //                        //    };
        //                        //}

        //                        ////Valida cedula
        //                        //var cedula = _mKindo.IsValidCostaRicaId(_infoSinpe.vInfo.Cedula, cuenta.Account.HolderIdType);
        //                        //if (!cedula)
        //                        //{
        //                        //    return new ErrorDto
        //                        //    {
        //                        //        Code = -1,
        //                        //        Description = response.Description + " - No. " + solicitud.ToString() + " - " + "La cédula no es válida."
        //                        //    };
        //                        //}
        //                        //Valido si la cedula perteneca a la cuenta, IMPLEMENTAR DESPUES
        //                        //if(_infoSinpe.vInfo.Cedula.ToString().Trim() != cuenta.Account.HolderId)
        //                        //{
        //                        //    return new ErrorDto
        //                        //    {
        //                        //        Code = -1,
        //                        //        Description = solicitud.ToString() + " - " + "La cédula no es válida."
        //                        //    };  
        //                        //}
        //                    }
        //                }
        //                else
        //                {
        //                    //if (servicio.Errors.Count > 0)
        //                    //{
        //                    //    foreach (var err in servicio.Errors)
        //                    //    {
        //                    //        response.Description += err.Message;
        //                    //    }

        //                    //    if (string.IsNullOrEmpty(response.Description))
        //                    //    {
        //                    //        response.Description = solicitud.ToString() + " - " + "Error al obtener información de la cuenta.";
        //                    //    }

        //                    //    response.Code = -1;
        //                    //}

        //                    response.Code = -1;
        //                    response.Description = servicio.Errors[0].Message;

        //                }
        //            }
        //            else
        //            {
        //                response.Code = -1;
        //                response.Description = "Cuenta IBAN no Valida";
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Code = -1;
        //        response.Description = "Ocurrió un problema con la validación. - " + ex.Message;
        //    }
        //    return response;

        //}


        #endregion
    }
}
