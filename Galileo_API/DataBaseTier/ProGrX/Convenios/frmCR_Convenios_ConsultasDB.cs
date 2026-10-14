using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosConsultasDb
    {
        private readonly PortalDB _portalDb;
        private const string Todos = "TODOS";

        public FrmCrConveniosConsultasDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCrConveniosConsultasDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Obtiene los tipos de convenios para el filtro (sbInicial).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrConveniosConsultas_Tipos_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(TIPO_CONVENIO) as item,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion
                from CRD_CONVENIO_TIPO
                order by DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sqlQuery);
        }

        /// <summary>
        /// Obtiene los usuarios activos para el filtro de usuario registra (sbInicial).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrConveniosConsultas_Usuarios_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    Nombre as item,
                    Nombre as descripcion
                from Usuarios
                where Estado = 'A';";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sqlQuery);
        }

        /// <summary>
        /// Lista de convenios para la búsqueda F4 (frmBusquedas).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConvenioConsultaData>> CrConveniosConsultas_Convenios_Obtener(int codEmpresa)
        {
            const string sqlQuery = @"
                select
                    rtrim(COD_CONVENIO) as cod_convenio,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion
                from CRD_CONVENIOS
                order by DESCRIPCION;";

            return DbHelper.ExecuteListQuery<CrConvenioConsultaData>(_portalDb, codEmpresa, sqlQuery);
        }

        /// <summary>
        /// Busca un convenio por código o por descripción exacta (sbConvenioDatos).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="descripcion"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioConsultaData?> CrConveniosConsultas_Convenio_Obtener(
            int codEmpresa,
            string? codConvenio,
            string? descripcion)
        {
            const string sqlQuery = @"
                select
                    rtrim(COD_CONVENIO) as cod_convenio,
                    rtrim(isnull(DESCRIPCION, '')) as descripcion
                from CRD_CONVENIOS
                where (@CodConvenio <> '' and COD_CONVENIO = @CodConvenio)
                   or (@CodConvenio = '' and DESCRIPCION = @Descripcion);";

            return DbHelper.ExecuteSingleQuery<CrConvenioConsultaData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                null,
                new
                {
                    CodConvenio = (codConvenio ?? string.Empty).Trim(),
                    Descripcion = (descripcion ?? string.Empty).Trim()
                }
            );
        }

        /// <summary>
        /// Consulta las órdenes de convenios según los filtros (sbConsulta).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConvenioOrdenData>> CrConveniosConsultas_Ordenes_Obtener(
            int codEmpresa,
            CrConvenioConsultaFiltros filtros)
        {
            const string sqlQuery = @"
                select
                    O.COD_ORDEN as cod_orden,
                    rtrim(isnull(O.COD_FACTURA, '')) as cod_factura,
                    rtrim(O.COD_CONVENIO) as cod_convenio,
                    C.DESCRIPCION as descripcion,
                    rtrim(C.TIPO_CONVENIO) as tipo_convenio,
                    T.DESCRIPCION as tipo_convenio_desc,
                    O.ESTADO as estado,
                    case O.ESTADO
                        when 'A' then 'Abierta'
                        when 'C' then 'Cerrada'
                        when 'T' then 'Tramitada'
                        else ''
                    end as estado_desc,
                    O.REGISTRO_FECHA as registro_fecha,
                    O.REGISTRO_USUARIO as registro_usuario,
                    O.TOTAL_PAGAR as total_pagar,
                    O.TOTAL_ORDEN as total_orden,
                    (O.RETENCION_AHORROS + O.COMISIONES_NUEVOS_CREDITOS + O.COMISIONES_RECAUDACION
                        + O.TOTAL_REBAJOS_CXP + O.TOTAL_RESERVA) as total_deducciones,
                    O.COMISIONES_NUEVOS_CREDITOS as comisiones_nuevos_creditos,
                    O.COMISIONES_RECAUDACION as comisiones_recaudacion,
                    O.TOTAL_RESERVA as total_reserva,
                    O.TOTAL_REBAJOS_CXP as total_rebajos_cxp,
                    O.RETENCION_AHORROS as retencion_ahorros,
                    O.IVA_TOTAL as iva_total
                from CRD_CONVENIOS_ORDENES O
                    inner join CRD_CONVENIOS C on C.COD_CONVENIO = O.COD_CONVENIO
                    inner join CRD_CONVENIO_TIPO T on C.TIPO_CONVENIO = T.TIPO_CONVENIO
                where (@CodConvenio = '' or O.COD_CONVENIO = @CodConvenio)
                    and (@TodosTipos = 1 or C.TIPO_CONVENIO in @Tipos)
                    and (@TodasFechas = 1 or O.REGISTRO_FECHA between @FechaInicio and @FechaCorte)
                    and (@Usuario = 'TODOS' or O.REGISTRO_USUARIO = @Usuario)
                    and (@Estado = 'TODOS' or O.ESTADO = @Estado)
                group by O.COD_ORDEN, O.COD_CONVENIO, C.DESCRIPCION, C.TIPO_CONVENIO, T.DESCRIPCION, O.ESTADO,
                    O.REGISTRO_FECHA, O.REGISTRO_USUARIO, O.TOTAL_PAGAR, O.TOTAL_ORDEN,
                    O.RETENCION_AHORROS, O.COMISIONES_NUEVOS_CREDITOS, O.COMISIONES_RECAUDACION,
                    O.TOTAL_REBAJOS_CXP, O.TOTAL_RESERVA, O.COD_FACTURA, O.IVA_TOTAL;";

            return DbHelper.ExecuteListQuery<CrConvenioOrdenData>(
                _portalDb,
                codEmpresa,
                sqlQuery,
                CrearParametrosOrdenes(filtros)
            );
        }

        /// <summary>
        /// Obtiene el detalle de pago de una factura y sus órdenes canceladas (sbCargaDetallePago).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codFactura"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioDetallePagoData?> CrConveniosConsultas_DetallePago_Obtener(
            int codEmpresa,
            string codFactura)
        {
            var response = DbHelper.WithConn<CrConvenioDetallePagoData?>(_portalDb, codEmpresa, conn =>
            {
                var detalle = conn.QueryFirstOrDefault<CrConvenioDetallePagoData>(
                    "exec spConvenios_Ordenes_DetallePago @CodFactura",
                    new { CodFactura = codFactura }
                );

                if (detalle == null)
                    return null;

                detalle.estado_desc = EstadoDesembolsoDescripcion(detalle.estado);

                var nSolicitud = detalle.nSolicitud ?? 0;
                if (nSolicitud > 0)
                {
                    detalle.ordenes_canceladas = conn.Query<CrConvenioOrdenCanceladaData>(
                        "exec spConvenios_OrdenesCanceladas @NSolicitud",
                        new { NSolicitud = nSolicitud }
                    ).ToList();
                }

                return detalle;
            });

            if (response.Code < 0)
                response.Result = null;

            return response;
        }

        private static object CrearParametrosOrdenes(CrConvenioConsultaFiltros filtros)
        {
            var fechaInicio = (filtros.fecha_inicio ?? DateTime.Today).Date;
            var fechaCorte = (filtros.fecha_corte ?? DateTime.Today).Date;

            return new
            {
                CodConvenio = (filtros.cod_convenio ?? string.Empty).Trim(),
                TodosTipos = filtros.todos_tipos == true ? 1 : 0,
                Tipos = filtros.tipos ?? new List<string>(),
                TodasFechas = filtros.todas_fechas == true ? 1 : 0,
                FechaInicio = fechaInicio,
                FechaCorte = fechaCorte.AddHours(23).AddMinutes(59).AddSeconds(59),
                Usuario = string.IsNullOrWhiteSpace(filtros.usuario) ? Todos : filtros.usuario.Trim(),
                Estado = string.IsNullOrWhiteSpace(filtros.estado) ? Todos : filtros.estado.Trim()
            };
        }

        private static string EstadoDesembolsoDescripcion(string? estado)
        {
            return (estado ?? string.Empty).Trim() switch
            {
                "S" => "Solicitado",
                "I" or "T" or "E" => "Impreso",
                "A" => "Anulado",
                "P" => "Pendiente",
                _ => string.Empty
            };
        }
    }
}
