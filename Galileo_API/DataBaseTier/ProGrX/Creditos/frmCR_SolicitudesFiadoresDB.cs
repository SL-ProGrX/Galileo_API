using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSolicitudesFiadoresDB
    {
        private readonly PortalDB _portalDB;
        private readonly MAfilicacionDB _mAfiliacion;
        private readonly MProGrXAuxiliarDB _mAuxiliar;

        private const int ModuloCreditos = 3;

        private const string MensajeOperacionRequerida = "La operación es requerida.";
        private const string MensajeFiadorRequerido = "El fiador es requerido.";

        public FrmCrSolicitudesFiadoresDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mAfiliacion = new MAfilicacionDB(config);
            _mAuxiliar = new MProGrXAuxiliarDB(config);
        }

        /// <summary>
        /// Obtiene instituciones activas para el combo de fiadores.
        /// </summary>
        public ErrorDto<List<CrSolicitudesFiadoresInstitucionDto>> CR_SolicitudesFiadores_Instituciones_Obtener(
            int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        cod_institucion as idx,
                        rtrim(descripcion) as itmx,
                        cast(cod_institucion as varchar(20)) as item,
                        rtrim(descripcion) as descripcion
                    from instituciones
                    where activa = 1
                    order by descripcion;";

                var lista = conn.Query<CrSolicitudesFiadoresInstitucionDto>(sql).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<CrSolicitudesFiadoresInstitucionDto>());
            }
        }

        /// <summary>
        /// Obtiene todos los fiadores de una operación. El filtro, sort y paginación se manejan en FE.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_SolicitudesFiadores_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            var filtros = ParseFiltros(parametros);

            if (!string.IsNullOrWhiteSpace(filtros.Error))
                return ErrorTabla(filtros.Error);

            if (filtros.IdSolicitud <= 0)
                return ErrorTabla(MensajeOperacionRequerida, -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = ObtenerFiadores(conn, filtros.IdSolicitud);

                return DbHelper.CreateOkResponse(new TablasListaGenericaModel
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return ErrorTabla(ex.Message);
            }
        }

        /// <summary>
        /// Exporta todos los fiadores de una operación.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_SolicitudesFiadores_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return CR_SolicitudesFiadores_Lista_Obtener(CodEmpresa, parametros);
        }

        /// <summary>
        /// Obtiene detalle de un fiador por FIA_CONSEC.
        /// </summary>
        public ErrorDto<CrSolicitudesFiadoresDetalleDto> CR_SolicitudesFiadores_Detalle_Obtener(
            int CodEmpresa,
            long fiaConsec)
        {
            if (fiaConsec <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFiadorRequerido,
                    -2,
                    new CrSolicitudesFiadoresDetalleDto());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var data = ObtenerDetalleFiador(conn, fiaConsec);

                if (data == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el fiador indicado.",
                        -2,
                        new CrSolicitudesFiadoresDetalleDto());
                }

                CompletarNombrePartes(data);
                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSolicitudesFiadoresDetalleDto());
            }
        }

        /// <summary>
        /// Obtiene nombre e institución de un socio por cédula.
        /// </summary>
        public ErrorDto<CrSolicitudesFiadoresSocioDto> CR_SolicitudesFiadores_Socio_Obtener(
            int CodEmpresa,
            string cedula)
        {
            cedula = Clean(cedula);

            if (string.IsNullOrWhiteSpace(cedula))
            {
                return DbHelper.CreateErrorResponse(
                    "La cédula es requerida.",
                    -2,
                    new CrSolicitudesFiadoresSocioDto());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = new CrSolicitudesFiadoresSocioDto
                {
                    cedula = cedula,
                    nombre = Clean(_mAfiliacion.fxNombre(CodEmpresa, cedula))
                };

                result.cod_institucion = ObtenerInstitucionSocio(conn, cedula);
                result.institucion_desc = result.cod_institucion > 0
                    ? Clean(_mAuxiliar.fxXInstitucion(CodEmpresa, result.cod_institucion))
                    : string.Empty;

                result.bloquea_institucion = result.cod_institucion > 0;

                CompletarNombrePartes(result);
                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSolicitudesFiadoresSocioDto());
            }
        }

        /// <summary>
        /// Registra o actualiza un fiador usando el SP legacy.
        /// </summary>
        public ErrorDto CR_SolicitudesFiadores_Guardar(
            int CodEmpresa,
            CrSolicitudesFiadoresGuardarRequest request)
        {
            var validation = ValidarGuardar(request);
            if (validation.Code != 0)
                return validation;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var data = NormalizarGuardar(request);
                EjecutarGuardarFiador(conn, data);

                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un fiador por FIA_CONSEC.
        /// </summary>
        public ErrorDto CR_SolicitudesFiadores_Eliminar(
            int CodEmpresa,
            CrSolicitudesFiadoresEliminarRequest request)
        {
            if (request == null || request.fia_consec <= 0)
                return DbHelper.ErrorResponse(MensajeFiadorRequerido);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"delete fiadores where FIA_CONSEC = @fia_consec;";

                var rows = conn.Execute(sql, new { request.fia_consec });

                if (rows <= 0)
                    return DbHelper.ErrorResponse("No se encontró el fiador para eliminar.");

                return DbHelper.OkResponse("Fiador eliminado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto<TablasListaGenericaModel> ErrorTabla(string mensaje, int code = -1)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                code,
                new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<CrSolicitudesFiadoresData>()
                });
        }

        private static FiadoresFiltroResult ParseFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                var idSolicitud = ParseFiltroInterno(filtros.filtro);

                return new FiadoresFiltroResult
                {
                    Filtros = filtros,
                    IdSolicitud = idSolicitud
                };
            }
            catch (JsonException ex)
            {
                return new FiadoresFiltroResult
                {
                    Error = ex.Message
                };
            }
        }

        private static long ParseFiltroInterno(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return 0;

            try
            {
                var json = JObject.Parse(filtro);
                return json.Value<long?>("id_solicitud") ?? 0;
            }
            catch (JsonException)
            {
                return 0;
            }
        }

        private static List<CrSolicitudesFiadoresData> ObtenerFiadores(
            SqlConnection conn,
            long idSolicitud)
        {
            const string sql = @"
                select
                    F.fia_consec,
                    F.id_solicitud,
                    rtrim(isnull(F.codigo,'')) as codigo,
                    rtrim(isnull(F.cedulaf,'')) as cedulaf,
                    rtrim(isnull(F.nombre,'')) as nombre,
                    rtrim(isnull(F.calidad,'')) as calidad,
                    case when F.calidad = 'F' then 'Fiador' else 'Co-Deudor' end as calidad_desc,
                    isnull(F.salario,0) as salario,
                    isnull(F.devengado,0) as devengado,
                    isnull(F.liquidez,0) as liquidez,
                    isnull(F.interno,0) as interno,
                    isnull(F.cod_institucion,0) as cod_institucion,
                    rtrim(isnull(I.descripcion,'')) as institucion_desc,
                    rtrim(isnull(F.estado,'')) as estado,
                    rtrim(isnull(F.firma,'')) as firma,
                    rtrim(isnull(F.estadoactual,'')) as estadoactual,
                    F.registro_fecha,
                    rtrim(isnull(F.registro_usuario,'')) as registro_usuario,
                    F.actualiza_fecha,
                    rtrim(isnull(F.actualiza_usuario,'')) as actualiza_usuario
                from fiadores F
                left join instituciones I
                       on F.cod_institucion = I.cod_institucion
                where F.id_solicitud = @idSolicitud
                order by F.fia_consec;";

            return conn.Query<CrSolicitudesFiadoresData>(
                sql,
                new { idSolicitud })
                .ToList();
        }

        private static CrSolicitudesFiadoresDetalleDto? ObtenerDetalleFiador(
            SqlConnection conn,
            long fiaConsec)
        {
            const string sql = @"
                select
                    F.fia_consec,
                    F.id_solicitud,
                    rtrim(isnull(F.cedulaf,'')) as cedulaf,
                    rtrim(isnull(F.nombre,'')) as nombre,
                    rtrim(isnull(F.calidad,'')) as calidad,
                    case when F.calidad = 'F' then 'Fiador' else 'Co-Deudor' end as calidad_desc,
                    isnull(F.salario,0) as salario,
                    isnull(F.devengado,0) as devengado,
                    isnull(F.liquidez,0) as liquidez,
                    isnull(F.interno,0) as interno,
                    isnull(F.cod_institucion,0) as cod_institucion,
                    rtrim(isnull(I.descripcion,'')) as institucion_desc,
                    F.registro_fecha,
                    F.actualiza_fecha
                from fiadores F
                inner join instituciones I
                        on F.cod_institucion = I.cod_institucion
                where F.fia_consec = @fiaConsec;";

            return conn.QueryFirstOrDefault<CrSolicitudesFiadoresDetalleDto>(
                sql,
                new { fiaConsec });
        }

        private static int ObtenerInstitucionSocio(SqlConnection conn, string cedula)
        {
            const string sql = @"
                select isnull(cod_institucion,0)
                from socios
                where cedula = @cedula;";

            return conn.QueryFirstOrDefault<int>(sql, new { cedula });
        }

        private static ErrorDto ValidarGuardar(CrSolicitudesFiadoresGuardarRequest request)
        {
            var errores = new List<string>();

            if (request == null)
                errores.Add("La información del fiador es requerida.");
            else
                ValidarGuardarData(request, errores);

            if (errores.Count == 0)
                return DbHelper.OkResponse("Ok");

            return new ErrorDto
            {
                Code = -2,
                Description = string.Join(Environment.NewLine, errores)
            };
        }

        private static void ValidarGuardarData(
            CrSolicitudesFiadoresGuardarRequest request,
            List<string> errores)
        {
            if (request.id_solicitud <= 0)
                errores.Add("- Cédula Incorrecta");

            if (string.IsNullOrWhiteSpace(request.cedulaf))
                errores.Add("- Cédula Incorrecta");

            if (NombreCompleto(request).Length == 0)
                errores.Add("- Nombre Incorrecto");

            if (CedulaIgualDeudor(request))
                errores.Add("- El Deudor No puede Ser Fiador de su misma Operación");

            if (request.cod_institucion <= 0)
                errores.Add("- Institución Incorrecta");

            if (request.liquidez > 100)
                errores.Add("- Liquidez Incorrecta");

            if (string.IsNullOrWhiteSpace(request.usuario))
                errores.Add("- Usuario requerido.");
        }

        private static bool CedulaIgualDeudor(CrSolicitudesFiadoresGuardarRequest request)
        {
            return string.Equals(
                Clean(request.cedulaf),
                Clean(request.cedula_deudor),
                StringComparison.OrdinalIgnoreCase);
        }

        private static CrSolicitudesFiadoresGuardarRequest NormalizarGuardar(
            CrSolicitudesFiadoresGuardarRequest request)
        {
            request.codigo = Clean(request.codigo);
            request.cedula_deudor = Clean(request.cedula_deudor);
            request.cedulaf = Clean(request.cedulaf);
            request.apellido1 = Clean(request.apellido1);
            request.apellido2 = Clean(request.apellido2);
            request.nombre = Clean(request.nombre);
            request.calidad = ResolverCalidad(request.calidad);
            request.usuario = Clean(request.usuario).ToUpperInvariant();
            request.maquina = Clean(request.maquina);
            request.version = Clean(request.version);
            request.interno = request.interno == 0 ? 0 : 1;

            return request;
        }

        private static void EjecutarGuardarFiador(
            SqlConnection conn,
            CrSolicitudesFiadoresGuardarRequest data)
        {
            const string sql = @"
                exec spCrdOperacionFiadorRegistro
                     @Operacion,
                     @Codigo,
                     @Calidad,
                     @Institucion,
                     @Usuario,
                     @Cedula,
                     @Nombre,
                     @Interno,
                     @Salario,
                     @Devengado,
                     @Liquidez,
                     @Modulo,
                     @Maquina,
                     @Version;";

            conn.Execute(sql, new
            {
                Operacion = data.id_solicitud,
                Codigo = data.codigo,
                Calidad = data.calidad,
                Institucion = data.cod_institucion,
                Usuario = data.usuario,
                Cedula = data.cedulaf,
                Nombre = NombreCompleto(data).ToUpperInvariant(),
                Interno = data.interno,
                Salario = data.salario,
                Devengado = data.devengado,
                Liquidez = data.liquidez,
                Modulo = ModuloCreditos,
                Maquina = data.maquina,
                Version = data.version
            });
        }

        private static string ResolverCalidad(string calidad)
        {
            var valor = Clean(calidad);

            if (valor.Equals("Fiador", StringComparison.OrdinalIgnoreCase))
                return "F";

            if (valor.Equals("Co-Deudor", StringComparison.OrdinalIgnoreCase))
                return "C";

            return valor.Length > 0
                ? valor[..1].ToUpperInvariant()
                : "F";
        }

        private static string NombreCompleto(CrSolicitudesFiadoresGuardarRequest request)
        {
            return Clean($"{request.apellido1} {request.apellido2} {request.nombre}");
        }

        private static void CompletarNombrePartes(CrSolicitudesFiadoresDetalleDto data)
        {
            var partes = DescomponerNombre(data.nombre);
            data.apellido1 = partes.Apellido1;
            data.apellido2 = partes.Apellido2;
            data.nombre1 = partes.Nombre1;
            data.nombre2 = partes.Nombre2;
        }

        private static void CompletarNombrePartes(CrSolicitudesFiadoresSocioDto data)
        {
            var partes = DescomponerNombre(data.nombre);
            data.apellido1 = partes.Apellido1;
            data.apellido2 = partes.Apellido2;
            data.nombre1 = partes.Nombre1;
            data.nombre2 = partes.Nombre2;
        }

        private static NombrePartes DescomponerNombre(string nombre)
        {
            var partes = Clean(nombre)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return new NombrePartes
            {
                Apellido1 = GetParte(partes, 0),
                Apellido2 = GetParte(partes, 1),
                Nombre1 = GetParte(partes, 2),
                Nombre2 = partes.Length > 3
                    ? string.Join(" ", partes.Skip(3))
                    : string.Empty
            };
        }

        private static string GetParte(string[] partes, int index)
        {
            return partes.Length > index ? partes[index] : string.Empty;
        }

        private static string Clean(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private sealed class FiadoresFiltroResult
        {
            public FiltrosLazyLoadData Filtros { get; set; } = new();
            public long IdSolicitud { get; set; }
            public string Error { get; set; } = string.Empty;
        }
        private sealed class NombrePartes
        {
            public string Apellido1 { get; set; } = string.Empty;
            public string Apellido2 { get; set; } = string.Empty;
            public string Nombre1 { get; set; } = string.Empty;
            public string Nombre2 { get; set; } = string.Empty;
        }
    }
}