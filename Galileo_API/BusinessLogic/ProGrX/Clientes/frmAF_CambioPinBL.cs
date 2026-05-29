using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Clientes;
using Galileo_API.Models.ProGrX.Clientes;

namespace Galileo_API.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfCambioPinBL
    {
        private readonly FrmAfCambioPinPinDB _db;

        public FrmAfCambioPinBL(IConfiguration config)
        {
            _db = new FrmAfCambioPinPinDB(config);
        }

        public ErrorDto<string> fxgAFIParametro(int CodEmpresa, string pCodigo)
        {
            return _db.fxgAFIParametro(CodEmpresa, pCodigo);
        }

        public ErrorDto<FrmAfCambioPinPersonaModel> Af_CambioPin_ObtenerPersona(int CodEmpresa, string cedula)
        { 
            return _db.Af_CambioPin_ObtenerPersona(CodEmpresa, cedula);
        }

        public ErrorDto fxTicketValida(int CodEmpresa, string ticket)
        { 
            return _db.fxTicketValida(CodEmpresa, ticket);
        }

        public static string GenerarPinSeguro(int CodEmpresa)
        {
            var result = FrmAfCambioPinPinDB.GenerarPinSeguro(CodEmpresa);
            return result;
        }

        public ErrorDto Af_CambioPin_Bitacora(int CodEmpresa, string usuario, string vTicket)
        {
            return _db.Af_CambioPin_Bitacora(CodEmpresa, usuario, vTicket);
        }

        public ErrorDto Af_CambioPin_RenovarClaveWeb(
           int CodEmpresa,
           string cedula,
           string email,
           string usuario)
        {
            return _db.Af_CambioPin_RenovarClaveWeb(CodEmpresa, cedula, email, usuario);
        }

        public ErrorDto Af_CambioPin_AplicarCambioPin(
           int CodEmpresa,
           FrmAfCambioPinAplicarModel model)
        {
            return _db.Af_CambioPin_AplicarCambioPin(CodEmpresa, model);
        }

    }
}