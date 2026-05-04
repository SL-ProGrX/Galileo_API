using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCONotificaEmailBL
    {
        private readonly IConfiguration? _config;
        private readonly FrmCONotificaEmailDB _db;

        public FrmCONotificaEmailBL(IConfiguration config)
        {
            _config = config;
            _db = new FrmCONotificaEmailDB(_config);
        }

        public ErrorDto<FrmCONotificaEmailListaResult> Co_NotificaEmail_Lista_Obtener(int CodEmpresa, string jfiltros, FrmCONotificaEmailConsultaDto dto)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);
            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }
            return _db.Co_NotificaEmail_Lista_Obtener(CodEmpresa, filtros, dto );
        }

        public ErrorDto<FrmCONotificaEmailListaResult> Co_NotificaEmail_Export(int CodEmpresa, string jfiltros, FrmCONotificaEmailConsultaDto dto)
        {
            FiltrosLazyLoadData? filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros);

            if (filtros == null)
            {
                filtros = new FiltrosLazyLoadData();
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Co_NotificaEmail_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        public ErrorDto Co_NotificaEmail_Notificar_Bulk(int CodEmpresa, string usuario, FrmCONotificaEmailNotificarBulkDto dto)
        {
            return _db.Co_NotificaEmail_Notificar_Bulk(CodEmpresa, usuario, dto);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.Co_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.Co_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
    }
}