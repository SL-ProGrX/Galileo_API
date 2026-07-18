using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXMayorizacionFullDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXMayorizacionFullDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCntXMayorizacionFullDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

        /// <summary>
        /// Lista los tipos de asiento configurados para una contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya conexión se utilizará.</param>
        /// <param name="codContabilidad">Código de la contabilidad que filtra los tipos de asiento.</param>
        /// <returns>Respuesta con los tipos de asiento disponibles.</returns>
        public ErrorDto<List<CntxTipoAsientoDto>> CntX_TiposAsientos_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = """
                SELECT
                    RTRIM(Tipo_Asiento) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Tipos_Asientos
                WHERE cod_contabilidad = @codContabilidad
                """;

            return DbHelper.ExecuteListQuery<CntxTipoAsientoDto>(
                _portalDB,
                codEmpresa,
                sql,
                new { codContabilidad });
        }

        /// <summary>
        /// Ejecuta la mayorización o reversión en lote con el filtro seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya conexión se utilizará.</param>
        /// <param name="codContabilidad">Código de la contabilidad que se procesará.</param>
        /// <param name="request">Período, operación, filtro, fechas, tipo de asiento y usuario.</param>
        /// <returns>Respuesta que indica si el proceso terminó correctamente.</returns>
        public ErrorDto<bool> Procesar(
            int codEmpresa,
            int codContabilidad,
            CntxMayorizacionProcesarDto request)
        {
            var (sql, parameters) = request.tipo_filtro switch
            {
                "PERIODO" => (
                    "EXEC spCntX_AsientosAplicacionLote_Todo @codContabilidad, @anio, @mes, @tipoAplicacion, @usuario",
                    (object)new
                    {
                        codContabilidad,
                        request.anio,
                        request.mes,
                        tipoAplicacion = request.tipo_aplicacion,
                        request.usuario
                    }),
                "FECHAS" => (
                    "EXEC spCntX_AsientosAplicacionLote_Fechas @codContabilidad, @anio, @mes, @tipoAplicacion, @usuario, @fechaInicio, @fechaFin",
                    new
                    {
                        codContabilidad,
                        request.anio,
                        request.mes,
                        tipoAplicacion = request.tipo_aplicacion,
                        request.usuario,
                        fechaInicio = request.fecha_inicio,
                        fechaFin = request.fecha_fin
                    }),
                "TIPO" => (
                    "EXEC spCntX_AsientosAplicacionLote_TipoAsiento @codContabilidad, @anio, @mes, @tipoAplicacion, @usuario, @tipoAsiento",
                    new
                    {
                        codContabilidad,
                        request.anio,
                        request.mes,
                        tipoAplicacion = request.tipo_aplicacion,
                        request.usuario,
                        tipoAsiento = request.tipo_asiento
                    }),
                "TIPO_FECHAS" => (
                    "EXEC spCntX_AsientosAplicacionLote_TipoAsientoFechas @codContabilidad, @anio, @mes, @tipoAplicacion, @usuario, @tipoAsiento, @fechaInicio, @fechaFin",
                    new
                    {
                        codContabilidad,
                        request.anio,
                        request.mes,
                        tipoAplicacion = request.tipo_aplicacion,
                        request.usuario,
                        tipoAsiento = request.tipo_asiento,
                        fechaInicio = request.fecha_inicio,
                        fechaFin = request.fecha_fin
                    }),
                _ => (string.Empty, new { })
            };

            if (string.IsNullOrWhiteSpace(sql))
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "El tipo de filtro seleccionado no es válido.");
            }

            var response = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sql,
                parameters);

            return response.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(
                    response.Description ?? "No fue posible completar el proceso.");
        }
    }
}
