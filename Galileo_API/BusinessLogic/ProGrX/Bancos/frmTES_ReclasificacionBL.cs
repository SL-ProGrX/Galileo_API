using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    
    public class FrmTesReclasificacionBL
    {
        private readonly FrmTesReclasificacionDB _reclasificacionDb;

        public FrmTesReclasificacionBL(IConfiguration config)
        {
            _reclasificacionDb = new FrmTesReclasificacionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_ReclasificacionBancos_Obtener(int CodEmpresa, string usuario, string gestion)
        {
            return _reclasificacionDb.TES_ReclasificacionBancos_Obtener(CodEmpresa, usuario, gestion);
        }

        public ErrorDto<TesReclasificacionDto> TES_Reclasificacion_Obtener(int CodEmpresa, int solicitud)
        {
            return _reclasificacionDb.TES_Reclasificacion_Obtener(CodEmpresa, solicitud);
        }

        public ErrorDto<string> TES_Reclasificacion_CuentaBanco(int CodEmpresa, int id_banco)
        {
            return _reclasificacionDb.TES_Reclasificacion_CuentaBanco(CodEmpresa, id_banco);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> tes_TiposDocsCargaCboAcceso_Obtener(int CodEmpresa, string usuario, int id_banco, string tipo)
        {
            return _reclasificacionDb.tes_TiposDocsCargaCboAcceso_Obtener(CodEmpresa, usuario, id_banco, tipo);
        }

        public ErrorDto TES_Reclasificacion_CambiaBanco(int CodEmpresa, TesReclasificaBancoModel data)
        {
            return _reclasificacionDb.TES_Reclasificacion_CambiaBanco(CodEmpresa, data);
        }

        public ErrorDto TES_Reclasificacion_CambiaDocumento(int CodEmpresa, TesReclasificaDocumentoModel data)
        {
            return _reclasificacionDb.TES_Reclasificacion_CambiaDocumento(CodEmpresa, data);
        }

        public ErrorDto TES_Reclasificacion_CambiaSolicitud(int CodEmpresa, TesReclasificaSolicitudModel data)
        {
            return _reclasificacionDb.TES_Reclasificacion_CambiaSolicitud(CodEmpresa, data);
        }

        public ErrorDto<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, string jFiltro)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jFiltro) ?? new FiltrosLazyLoadData();
            return _reclasificacionDb.TES_Solicitudes_Obtener(CodEmpresa, filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            return _reclasificacionDb.TiposIdentificacion_Obtener(CodEmpresa);
        }

        public ErrorDto<bool> Tes_ReclasificaId_Valida(int CodEmpresa, string? tipo)
        {
            return _reclasificacionDb.Tes_ReclasificaId_Valida(CodEmpresa, tipo);
        }

    }
}
