using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOCobroFiadoresBL
    {
        private readonly FrmCOCobroFiadoresDB _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly CoCobroFiadoresProcesoQueue _queue;

        public FrmCOCobroFiadoresBL(IConfiguration config, CoCobroFiadoresProcesoQueue queue)
        {
            _db = new FrmCOCobroFiadoresDB(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _queue = queue;
        }

        /// <summary>
        /// Valida, registra en GEN_PROCESO y encola el procesamiento de cobros a fiadores.
        /// Devuelve el ProcesoId para consultar el avance.
        /// </summary>
        public ErrorDto<GenProcesoIniciadoResult> Co_CobroFiadores_ProcesaCobros_Iniciar(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            var ids = (dto?.ids ?? new List<long>()).Where(id => id > 0).Select(id => (int)id).ToList();
            if (ids.Count == 0)
            {
                return DbHelper.CreateErrorResponse("Debe Seleccionar al menos un caso!", -2, new GenProcesoIniciadoResult());
            }

            var validacion = _db.ValidarProcesaCobros(CodEmpresa);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(validacion.Description ?? "Error al procesar.", -1, new GenProcesoIniciadoResult());
            }

            var procesoId = Guid.NewGuid();

            try
            {
                _procesoDb.Iniciar(CodEmpresa, procesoId, GenProcesoTipos.CoCobroFiadores, usuario, ids.Count);

                if (!_queue.Encolar(new CoCobroFiadoresProcesoTrabajo(procesoId, CodEmpresa, usuario, ids)))
                {
                    const string msgCola = "No fue posible encolar el proceso. Intente nuevamente.";
                    _procesoDb.Actualizar(CodEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, msgCola);

                    return DbHelper.CreateErrorResponse(msgCola, -1, new GenProcesoIniciadoResult());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new GenProcesoIniciadoResult());
            }

            return DbHelper.CreateOkResponse(new GenProcesoIniciadoResult { ProcesoId = procesoId });
        }

        /// <summary>
        /// Consulta el avance del procesamiento de cobros a fiadores (polling desde la pantalla).
        /// </summary>
        public ErrorDto<GenProcesoEstadoResult> Co_CobroFiadores_ProcesaCobros_Estado(int CodEmpresa, Guid procesoId)
        {
            var estado = _procesoDb.Estado_Obtener(CodEmpresa, procesoId);

            return estado is null
                ? DbHelper.CreateErrorResponse("Proceso no encontrado.", -1, new GenProcesoEstadoResult())
                : DbHelper.CreateOkResponse(estado);
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
