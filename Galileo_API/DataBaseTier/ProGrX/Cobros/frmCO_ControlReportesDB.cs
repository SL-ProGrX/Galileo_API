using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOControlReportesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 4;

        public FrmCOControlReportesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene el catálogo fijo de reportes de Control de Cobro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoControlReporteItemDto>> CO_ControlReportes_Catalogo_Obtener(int CodEmpresa)
        {
            var lista = new List<CoControlReporteItemDto>
            {
                new() { codigo = "01", descripcion = "Gestiones Realizadas", grupo = "GESTIONES" },
                new() { codigo = "04", descripcion = "Gestiones por Usuarios", grupo = "GESTIONES" },
                new() { codigo = "09", descripcion = "Recuperación por Gestión", grupo = "RECUPERACION" },
                new() { codigo = "10", descripcion = "Recuperación por Usuario", grupo = "RECUPERACION" },
                new() { codigo = "11", descripcion = "Recuperación por Línea de Crédito", grupo = "RECUPERACION" },
                new() { codigo = "12", descripcion = "Recuperación por Garantía", grupo = "RECUPERACION" },
                new() { codigo = "14", descripcion = "Recuperación por Causa de Mora", grupo = "RECUPERACION" },
                new() { codigo = "15", descripcion = "Recuperación por Arreglo de Pago", grupo = "RECUPERACION" },
                new() { codigo = "13", descripcion = "Recuperación Informe Estadístico", grupo = "RECUPERACION" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene los filtros base requeridos por la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CoControlReportesFiltrosDto> CO_ControlReportes_Filtros_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = new CoControlReportesFiltrosDto
                {
                    tiposSalida = new List<DropDownListaGenericaModel>
                    {
                        new() { item = "Detalle", descripcion = "Detalle" },
                        new() { item = "Resumen", descripcion = "Resumen" }
                    }
                };

                const string sqlEstadosPersona = @"
                    select
                        rtrim(cod_estado) as item,
                        rtrim(descripcion) as descripcion
                    from AFI_ESTADOS_PERSONA
                    where ACTIVO = 1
                    order by descripcion;";

                const string sqlGestiones = @"
                    select
                        rtrim(cod_gestion) as item,
                        rtrim(descripcion) as descripcion
                    from CBR_GESTIONES
                    order by descripcion;";

                const string sqlUsuarios = @"
                    select
                        rtrim(usuario) as item,
                        rtrim(nombre) as descripcion
                    from CBR_USUARIOS
                    where estado = 1
                    order by nombre;";

                result.estadosPersona = conn.Query<DropDownListaGenericaModel>(sqlEstadosPersona).ToList();
                result.gestiones = conn.Query<DropDownListaGenericaModel>(sqlGestiones).ToList();
                result.usuarios = conn.Query<DropDownListaGenericaModel>(sqlUsuarios).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoControlReportesFiltrosDto>(
                    ex.Message,
                    -1,
                    new CoControlReportesFiltrosDto());
            }
        }

        

        /// <summary>
        /// Ejecuta el proceso del cubo de recuperación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto CO_ControlReportes_Cubo_Procesar(int CodEmpresa, CoControlReportesCuboRequestDto data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                data ??= new CoControlReportesCuboRequestDto();

                var usuarioEjecuta = (data.usuarioEjecuta ?? string.Empty).Trim().ToUpperInvariant();
                var fechas = ResolverFechas(data.todasFechas, data.fechaInicio, data.fechaCorte, true);

                if (!fechas.Ok)
                    return DbHelper.ErrorResponse(fechas.Mensaje);

                conn.Execute(
                    "spCbrControlRecuperacionAnalisisCubo",
                    new
                    {
                        FechaInicio = fechas.FechaInicio,
                        FechaCorte = fechas.FechaCorte
                    },
                    commandType: CommandType.StoredProcedure);

                var bitacora = Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioEjecuta,
                    Movimiento = "PROCESA-WEB",
                    Modulo = vModulo,
                    DetalleMovimiento =
                        $"Control Reportes Cubo. Inicio={fechas.FechaInicio:yyyy-MM-dd}, Corte={fechas.FechaCorte:yyyy-MM-dd}"
                });

                if (bitacora.Code != 0)
                    return bitacora;

                return DbHelper.OkResponse("Proceso concluido correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Normaliza valores de filtro equivalentes a TODOS.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private static string? NormalizarValorFiltro(string? valor)
        {
            var tmp = (valor ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(tmp))
                return null;

            if (tmp.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                return null;

            return tmp;
        }

        /// <summary>
        /// Resuelve el metadato principal del reporte según código y tipo.
        /// </summary>
        /// <param name="codigoReporte"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private static ReporteMeta? ResolverReporte(string codigoReporte, string tipo)
        {
            var esResumen = tipo.Equals("Resumen", StringComparison.OrdinalIgnoreCase);

            return codigoReporte switch
            {
                "01" => new ReporteMeta(
                    "GESTIONES REALIZADAS",
                    esResumen ? "Cobro_ControlGestionesRealizadasRsm" : "Cobro_ControlGestionesRealizadas",
                    true,
                    "GESTIONES"),

                "04" => new ReporteMeta(
                    "GESTIONES REALIZADAS x USUARIOS",
                    esResumen ? "Cobro_ControlGestionesUsuariosRsm" : "Cobro_ControlGestionesUsuarios",
                    true,
                    "GESTIONES"),

                "09" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN POR GESTIÓN: RESUMEN" : "RECUPERACIÓN POR GESTIÓN: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Gestion_Rsm" : "Cobro_Control_Recuperacion_Gestion_Dt",
                    false,
                    "RECUPERACION"),

                "10" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN POR USUARIO: RESUMEN" : "RECUPERACIÓN POR USUARIO: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Usuario_Rsm" : "Cobro_Control_Recuperacion_Usuario_Dt",
                    false,
                    "RECUPERACION"),

                "11" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN POR LÍNEA DE CRÉDITO: RESUMEN" : "RECUPERACIÓN POR LÍNEA DE CRÉDITO: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Linea_Rsm" : "Cobro_Control_Recuperacion_Linea_Dt",
                    false,
                    "RECUPERACION"),

                "12" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN POR GARANTÍA: RESUMEN" : "RECUPERACIÓN POR GARANTÍA: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Garantia_Rsm" : "Cobro_Control_Recuperacion_Garantia_Dt",
                    false,
                    "RECUPERACION"),

                "13" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN: INFORME ESTADÍSTICO: RESUMEN" : "RECUPERACIÓN: INFORME ESTADÍSTICO: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Estadistica_Rsm" : "Cobro_Control_Recuperacion_Estadistica_Dt",
                    false,
                    "RECUPERACION"),

                "14" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN POR CAUSA DE MORA: RESUMEN" : "RECUPERACIÓN POR CAUSA DE MORA: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Causa_Rsm" : "Cobro_Control_Recuperacion_Causa_Dt",
                    false,
                    "RECUPERACION"),

                "15" => new ReporteMeta(
                    esResumen ? "RECUPERACIÓN POR TIPO ARREGLO DE PAGO: RESUMEN" : "RECUPERACIÓN POR TIPO ARREGLO DE PAGO: DETALLE",
                    esResumen ? "Cobro_Control_Recuperacion_Arreglo_Rsm" : "Cobro_Control_Recuperacion_Arreglo_Dt",
                    false,
                    "RECUPERACION"),

                _ => null
            };
        }

        /// <summary>
        /// Resuelve y valida el rango de fechas requerido por la pantalla.
        /// </summary>
        /// <param name="todasFechas"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="permiteTodasFechas"></param>
        /// <returns></returns>
        private static FechasResultado ResolverFechas(
            bool todasFechas,
            string? fechaInicio,
            string? fechaCorte,
            bool permiteTodasFechas)
        {
            if (todasFechas && permiteTodasFechas)
            {
                return new FechasResultado
                {
                    Ok = true,
                    FechaInicio = new DateTime(1900, 1, 1),
                    FechaCorte = DateTime.Today
                };
            }

            if (!TryParseFecha(fechaInicio, out var inicio))
            {
                return new FechasResultado
                {
                    Ok = false,
                    Mensaje = "Debe indicar una fecha inicial válida."
                };
            }

            if (!TryParseFecha(fechaCorte, out var corte))
            {
                return new FechasResultado
                {
                    Ok = false,
                    Mensaje = "Debe indicar una fecha corte válida."
                };
            }

            if (inicio.Date > corte.Date)
            {
                return new FechasResultado
                {
                    Ok = false,
                    Mensaje = "La fecha inicial no puede ser mayor que la fecha corte."
                };
            }

            return new FechasResultado
            {
                Ok = true,
                FechaInicio = inicio.Date,
                FechaCorte = corte.Date
            };
        }

        /// <summary>
        /// Convierte una fecha string en DateTime usando formatos controlados.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        private static bool TryParseFecha(string? valor, out DateTime fecha)
        {
            var formatos = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "yyyy/MM/dd" };
            return DateTime.TryParseExact(
                (valor ?? string.Empty).Trim(),
                formatos,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha);
        }

        private sealed record ReporteMeta(
            string Titulo,
            string NombreArchivo,
            bool PermiteTodasFechas,
            string Grupo);

        private sealed class FechasResultado
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; } = string.Empty;
            public DateTime FechaInicio { get; set; }
            public DateTime FechaCorte { get; set; }
        }
    }
}
