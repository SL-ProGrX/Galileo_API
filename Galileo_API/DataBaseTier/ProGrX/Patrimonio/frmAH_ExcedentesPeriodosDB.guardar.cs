using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesPeriodosDB
    {
        

        /// <summary>
        /// Inserta un nuevo período de excedentes.
        /// </summary>
        public ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Patrimonio_frmAH_ExcedentesPeriodos_Insertar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return Patrimonio_frmAH_ExcedentesPeriodos_Guardar(codEmpresa, request, false);
        }

        /// <summary>
        /// Actualiza un período de excedentes existente.
        /// </summary>
        public ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Patrimonio_frmAH_ExcedentesPeriodos_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest request)
        {
            return Patrimonio_frmAH_ExcedentesPeriodos_Guardar(codEmpresa, request, true);
        }

        /// <summary>
        /// Elimina un período de excedentes.
        /// </summary>
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_Eliminar(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            var usuarioNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(usuario);

            if (periodoId <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar el período.", -2, false);
            }

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, false);
            }

            const string sqlDelete = @"
delete from EXC_PERIODOS
where ID_PERIODO = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (!Patrimonio_frmAH_ExcedentesPeriodos_Existe(conn, periodoId))
                {
                    return DbHelper.CreateErrorResponse("El período indicado no existe.", -2, false);
                }

                conn.Execute(sqlDelete, new { PeriodoId = periodoId });

                Patrimonio_frmAH_ExcedentesPeriodos_RegistrarBitacoraSeguridad(
                    codEmpresa,
                    usuarioNormalizado,
                    "Elimina - WEB",
                    $"Excedentes> Periodo: {periodoId}");

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        private ErrorDto<FrmAhExcedentesPeriodosGuardarResponse> Patrimonio_frmAH_ExcedentesPeriodos_Guardar(
            int codEmpresa,
            FrmAhExcedentesPeriodosGuardarRequest? request,
            bool esEdicion)
        {
            var response = new FrmAhExcedentesPeriodosGuardarResponse();
            var validacion = Patrimonio_frmAH_ExcedentesPeriodos_ValidarGuardarRequest(request, response, esEdicion);

            if (validacion != null)
            {
                return validacion;
            }

            var requestNormalizado = request!;
            var usuarioNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(requestNormalizado.usuario);
            var estadoNotasNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(requestNormalizado.estado_notas);
            var tipoAplMensualNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTipoAplicacion(requestNormalizado.tipo_apl_mensual);

            const string sqlSiguienteId = @"
select isnull(max(ID_PERIODO), 0) + 1
from EXC_PERIODOS;";

            const string sqlInsert = @"
insert into EXC_PERIODOS
(
    ID_PERIODO,
    INICIO,
    CORTE,
    ESTADO,
    CAPITALIZA_PORC,
    RESERVA_PORC,
    CAPITALIZA_RENTA_APLICA,
    NC_MORA,
    NC_SALDOS,
    NC_OPCF,
    NC_FND_EXTRA,
    DOC_ASIENTO,
    VISIBLE_WEBAPP,
    VISIBLE_SYS,
    TIPO_APL_MENSUAL,
    MOSTRAR_EN_HISTORIAL,
    MOSTRAR_TABLA_RENTA,
    ESTADO_NOTAS,
    MODO_AUTOMATICO,
    REGISTRO_FECHA,
    REGISTRO_USUARIO
)
values
(
    @IdPeriodo,
    @Inicio,
    @Corte,
    'A',
    @CapitalizaPorc,
    @ReservaPorc,
    @CapitalizaRentaAplica,
    '',
    '',
    '',
    '',
    '',
    @VisibleWebapp,
    @VisibleSys,
    @TipoAplMensual,
    @MostrarEnHistorial,
    @MostrarTablaRenta,
    @EstadoNotas,
    @ModoAutomatico,
    dbo.MyGetdate(),
    @Usuario
);";

            const string sqlUpdate = @"
update EXC_PERIODOS
set
    CAPITALIZA_PORC = @CapitalizaPorc,
    RESERVA_PORC = @ReservaPorc,
    CAPITALIZA_RENTA_APLICA = @CapitalizaRentaAplica,
    INICIO = @Inicio,
    CORTE = @Corte,
    VISIBLE_WEBAPP = @VisibleWebapp,
    VISIBLE_SYS = @VisibleSys,
    MOSTRAR_EN_HISTORIAL = @MostrarEnHistorial,
    MOSTRAR_TABLA_RENTA = @MostrarTablaRenta,
    ESTADO_NOTAS = @EstadoNotas,
    MODO_AUTOMATICO = @ModoAutomatico
where ID_PERIODO = @IdPeriodo;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var idPeriodo = esEdicion
                    ? requestNormalizado.id_periodo
                    : conn.QueryFirst<int>(sqlSiguienteId);

                if (esEdicion)
                {
                    var validacionEstado = Patrimonio_frmAH_ExcedentesPeriodos_ValidarPeriodoEditable(conn, idPeriodo, response);
                    if (validacionEstado != null)
                    {
                        return validacionEstado;
                    }
                }

                conn.Execute(
                    esEdicion ? sqlUpdate : sqlInsert,
                    new
                    {
                        IdPeriodo = idPeriodo,
                        Inicio = requestNormalizado.inicio.Date,
                        Corte = requestNormalizado.corte.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                        CapitalizaPorc = requestNormalizado.capitaliza_porc,
                        ReservaPorc = requestNormalizado.reserva_porc,
                        CapitalizaRentaAplica = requestNormalizado.capitaliza_renta_aplica,
                        VisibleWebapp = requestNormalizado.visible_webapp,
                        VisibleSys = requestNormalizado.visible_sys,
                        MostrarEnHistorial = requestNormalizado.mostrar_en_historial,
                        MostrarTablaRenta = requestNormalizado.mostrar_tabla_renta,
                        EstadoNotas = estadoNotasNormalizado,
                        ModoAutomatico = requestNormalizado.modo_automatico,
                        TipoAplMensual = tipoAplMensualNormalizado,
                        Usuario = usuarioNormalizado
                    });

                var accion = esEdicion ? "Modifica" : "Registra";
                var movimiento = esEdicion ? "Modifica - WEB" : "Registra - WEB";

                Patrimonio_frmAH_ExcedentesPeriodos_RegistrarBitacoraSeguridad(
                    codEmpresa,
                    usuarioNormalizado,
                    movimiento,
                    $"Excedentes> Periodo: {idPeriodo}");

                response.id_periodo = idPeriodo;
                response.accion = accion;
                response.mensaje = "Información guardada satisfactoriamente...";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static ErrorDto<FrmAhExcedentesPeriodosGuardarResponse>? Patrimonio_frmAH_ExcedentesPeriodos_ValidarGuardarRequest(
            FrmAhExcedentesPeriodosGuardarRequest? request,
            FrmAhExcedentesPeriodosGuardarResponse response,
            bool esEdicion)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, response);
            }

            if (esEdicion && request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar el período a modificar.", -2, response);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, response);
            }

            if (request.inicio >= request.corte)
            {
                return DbHelper.CreateErrorResponse("El rango de fechas es erróneo.", -2, response);
            }

            if (request.capitaliza_porc < 0)
            {
                return DbHelper.CreateErrorResponse("El porcentaje de capitalización no es válido.", -2, response);
            }

            if (request.reserva_porc < 0)
            {
                return DbHelper.CreateErrorResponse("El porcentaje de reserva no es válido.", -2, response);
            }

            return null;
        }

        private static ErrorDto<FrmAhExcedentesPeriodosGuardarResponse>? Patrimonio_frmAH_ExcedentesPeriodos_ValidarPeriodoEditable(
            SqlConnection conn,
            int periodoId,
            FrmAhExcedentesPeriodosGuardarResponse response)
        {
            const string sql = @"
select rtrim(isnull(ESTADO, '')) as estado
from EXC_PERIODOS
where ID_PERIODO = @PeriodoId;";

            var estado = conn.QueryFirstOrDefault<string>(sql, new { PeriodoId = periodoId });

            if (string.IsNullOrWhiteSpace(estado))
            {
                return DbHelper.CreateErrorResponse("El período indicado no existe.", -2, response);
            }

            if (estado.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.CreateErrorResponse("El período ya fue cerrado.", -2, response);
            }

            return null;
        }

        private static bool Patrimonio_frmAH_ExcedentesPeriodos_Existe(
            SqlConnection conn,
            int periodoId)
        {
            const string sql = @"
select cast(count(1) as int)
from EXC_PERIODOS
where ID_PERIODO = @PeriodoId;";

            return conn.QueryFirstOrDefault<int>(sql, new { PeriodoId = periodoId }) > 0;
        }

        private static string Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(string? valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private static string Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTipoAplicacion(string? tipoAplicacion)
        {
            var valor = (tipoAplicacion ?? string.Empty).Trim().ToUpperInvariant();
            return string.IsNullOrWhiteSpace(valor) ? "M" : valor[..1];
        }

        private void Patrimonio_frmAH_ExcedentesPeriodos_RegistrarBitacoraSeguridad(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloPatrimonio
            });
        }
    }
}
