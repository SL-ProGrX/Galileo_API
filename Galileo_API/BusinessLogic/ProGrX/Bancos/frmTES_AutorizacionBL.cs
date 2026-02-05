
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;
using PdfSharp.Pdf.Filters;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesAutorizacionBL
    {
        private readonly FrmTesAutorizacionDb AutorizacionDb;
        private readonly MTesoreria mTesoreria;

        public FrmTesAutorizacionBL(IConfiguration config)
        {
            AutorizacionDb = new FrmTesAutorizacionDb(config);
            mTesoreria = new MTesoreria(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_AutorizacionBancos_Obtener(int CodEmpresa, string usuario)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, "Autoriza");
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_TiposDocsAutoriza_Obtener(int CodEmpresa, string usuario, int banco, int tipo_autorizacion)
        {
            if (tipo_autorizacion == 0)
            {
                return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, usuario, banco, "A");
            }
            else
            {
                return mTesoreria.sbTesTiposDocsCargaCboAccesoFirmas(CodEmpresa, usuario, banco, "A");
            }    
        }

        public ErrorDto<TesSolicitudesLista> TES_SolicitudesPendientes_Obtener(int CodEmpresa, string filtros)
        {
            return AutorizacionDb.TES_SolicitudesPendientes_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto TES_Autorizacion_Aplicar(TesAutorizaParametros nsoliictud)
        {
            return AutorizacionDb.TES_Autorizacion_Aplicar(nsoliictud);
        }

        public ErrorDto<TesAutorizacionData> TES_AutorizacionDoc_Obtener(int CodEmpresa, string usuario)
        {
            return AutorizacionDb.TES_AutorizacionDoc_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<TesFirmasAutData> TES_AutorizacionFirma_Obtener(int CodEmpresa, string usuario, int banco)
        {
            return AutorizacionDb.TES_AutorizacionFirma_Obtener(CodEmpresa, usuario, banco);
        }

        public ErrorDto<TesAccesosUsuariosLista> TES_AutorizacionBuscar_Obtener(int CodEmpresa, string filtro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();
            return AutorizacionDb.TES_AutorizacionBuscar_Obtener(CodEmpresa, filtros);
        }
    }
}