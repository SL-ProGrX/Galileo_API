using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOCobroFiadoresBL
    {
        private readonly IConfiguration? _config;
        private readonly FrmCOCobroFiadoresDB _db;

        public FrmCOCobroFiadoresBL(IConfiguration config)
        {
            _config = config;
            _db = new FrmCOCobroFiadoresDB(_config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.Co_Instituciones_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Co_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.Co_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        public ErrorDto<FrmCOCobroFiadoresPendientesListaResult> Co_CobroFiadores_Pendientes_Lista_Obtener(int CodEmpresa, string jfiltros, string jdto)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            FrmCOCobroFiadoresPendientesConsultaDto dto = JsonConvert.DeserializeObject<FrmCOCobroFiadoresPendientesConsultaDto>(jdto) ?? new FrmCOCobroFiadoresPendientesConsultaDto();
            return _db.Co_CobroFiadores_Pendientes_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        public ErrorDto<FrmCOCobroFiadoresPendientesListaResult> Co_CobroFiadores_Pendientes_Lista_Export(int CodEmpresa, string jfiltros, string jdto)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            FrmCOCobroFiadoresPendientesConsultaDto dto = JsonConvert.DeserializeObject<FrmCOCobroFiadoresPendientesConsultaDto>(jdto) ?? new FrmCOCobroFiadoresPendientesConsultaDto();

            filtros ??= new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Co_CobroFiadores_Pendientes_Lista_Obtener(CodEmpresa, filtros, dto);
        }
        public ErrorDto<FrmCOCobroFiadoresActivosListaResult> Co_CobroFiadores_Activos_Lista_Obtener(int CodEmpresa, string jfiltros, string jdto)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            FrmCOCobroFiadoresActivosConsultaDto dto = JsonConvert.DeserializeObject<FrmCOCobroFiadoresActivosConsultaDto>(jdto) ?? new FrmCOCobroFiadoresActivosConsultaDto();
            return _db.Co_CobroFiadores_Activos_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        public ErrorDto<FrmCOCobroFiadoresActivosListaResult> Co_CobroFiadores_Activos_Lista_Export(int CodEmpresa, string jfiltros, string jdto)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            FrmCOCobroFiadoresActivosConsultaDto dto = JsonConvert.DeserializeObject<FrmCOCobroFiadoresActivosConsultaDto>(jdto) ?? new FrmCOCobroFiadoresActivosConsultaDto();

            filtros ??= new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Co_CobroFiadores_Activos_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        public ErrorDto<FrmCOCobroFiadoresConsultasListaResult> Co_CobroFiadores_Consultas_Lista_Obtener(int CodEmpresa, string jfiltros, string jdto)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            FrmCOCobroFiadoresConsultasConsultaDto dto = JsonConvert.DeserializeObject<FrmCOCobroFiadoresConsultasConsultaDto>(jdto) ?? new FrmCOCobroFiadoresConsultasConsultaDto();
            return _db.Co_CobroFiadores_Consultas_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        public ErrorDto<FrmCOCobroFiadoresConsultasListaResult> Co_CobroFiadores_Consultas_Lista_Export(int CodEmpresa, string jfiltros, string jdto)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            FrmCOCobroFiadoresConsultasConsultaDto dto = JsonConvert.DeserializeObject<FrmCOCobroFiadoresConsultasConsultaDto>(jdto) ?? new FrmCOCobroFiadoresConsultasConsultaDto();

            filtros ??= new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return _db.Co_CobroFiadores_Consultas_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        public ErrorDto Co_CobroFiadores_NotificaAdvertencia_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return _db.Co_CobroFiadores_NotificaAdvertencia_Bulk(CodEmpresa, usuario, dto);
        }

        public ErrorDto Co_CobroFiadores_ProcesaCobros_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return _db.Co_CobroFiadores_ProcesaCobros_Bulk(CodEmpresa, usuario, dto);
        }

        public ErrorDto Co_CobroFiadores_CancelaCobro_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return _db.Co_CobroFiadores_CancelaCobro_Bulk(CodEmpresa, usuario, dto);
        }
    }
}
