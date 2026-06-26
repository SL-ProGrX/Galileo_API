using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Creditos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComitesSemaforoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int vModulo = 3;

        public FrmCrComitesSemaforoDB(IConfiguration config)
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
        /// Obtiene los comités activos para el combo principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesSemaforo_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            try
            {
                return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
                {
                    const string sql = @"
                select
                    cast(ID_COMITE as varchar(20)) as item,
                    rtrim(DESCRIPCION) as descripcion
                from COMITES
                where ESTADO = 1
                order by DESCRIPCION;";

                    return conn.Query<DropDownListaGenericaModel>(sql).ToList();
                });
            }
            catch (SqlException)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    "Ocurrió un error al obtener la lista de comités.");
            }
        }
        /// <summary>
        /// Obtiene la configuración del semáforo para un comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idComite"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesSemaforoData> CR_ComitesSemaforo_Obtener(int CodEmpresa, int idComite)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (idComite <= 0)
                    return DbHelper.CreateOkResponse(BuildSemaforoDefault(idComite));

                const string sql = @"
                    select
                        IdRegistro as id_registro,
                        IdComite as id_comite,
                        rtrim(UnidadTiempo) as unidad_tiempo,
                        case
                            when UnidadTiempo = 'MINUTE' then 'Minutos'
                            when UnidadTiempo = 'DAY' then 'Días'
                            when UnidadTiempo = 'HOUR' then 'Horas'
                            else ''
                        end as unidad_tiempo_esp,
                        AlertaRoja as alerta_roja,
                        AlertaAmarilla as alerta_amarilla,
                        FechaInserta as fecha_inserta,
                        rtrim(UsuarioInserta) as usuario_inserta,
                        FechaActualiza as fecha_actualiza,
                        rtrim(isnull(UsuarioActualiza,'')) as usuario_actualiza
                    from CRD_COMITES_SEMAFORO
                    where IdComite = @idComite;";

                var data = conn.QueryFirstOrDefault<CrComitesSemaforoData>(sql, new { idComite })
                           ?? BuildSemaforoDefault(idComite);

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesSemaforoData>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda o actualiza la configuración del semáforo de un comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesSemaforo_Guardar(int CodEmpresa, CrComitesSemaforoGuardarRequest request)
        {
            var validacion = ValidarSemaforoRequest(request);
            if (validacion.Code != 0) return validacion;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarSemaforoAdd(conn, request);

                if (result.Pass != 1)
                    return DbHelper.ErrorResponse(BuildSpMensaje(result, "No fue posible registrar el semáforo, verifique."));

                RegistrarBitacora(CodEmpresa, request.usuario, result.Movimiento, result.Detalle);

                return DbHelper.OkResponse("Semáforo registrado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de correos configurados para notificación de resoluciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesSemaforoEmailLista> CR_ComitesSemaforo_Email_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0)
                return DbHelper.CreateErrorResponse<CrComitesSemaforoEmailLista>(
                    filtrosResult.Description ?? "Filtros inválidos.");

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
                var fx = BuildEmailFiltro(filtros);

                if (fx.IdComite <= 0)
                    return DbHelper.CreateOkResponse(BuildEmailListaVacia());

                var total = ObtenerEmailsTotal(conn, fx);
                var lista = ObtenerEmailsLista(conn, fx);

                return DbHelper.CreateOkResponse(new CrComitesSemaforoEmailLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesSemaforoEmailLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista completa de correos configurados para un comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesSemaforoEmailLista> CR_ComitesSemaforo_Email_Lista_Export(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0)
                return DbHelper.CreateErrorResponse<CrComitesSemaforoEmailLista>(filtrosResult.Description ?? "Filtros inválidos.");

            var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_ComitesSemaforo_Email_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Agrega un correo para notificación de resoluciones del comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesSemaforo_Email_Agregar(int CodEmpresa, CrComitesSemaforoEmailAgregarRequest request)
        {
            var validacion = ValidarEmailAgregarRequest(request);
            if (validacion.Code != 0) return validacion;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = EjecutarEmailAdd(conn, request);

                if (result.Pass != 1)
                    return DbHelper.ErrorResponse(BuildSpMensaje(result, "No fue posible registrar el correo, verifique."));

                RegistrarBitacora(CodEmpresa, request.usuario, result.Movimiento, result.Detalle);

                return DbHelper.OkResponse("Correo registrado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina uno o varios correos configurados para notificación de resoluciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesSemaforo_Email_Eliminar(int CodEmpresa, CrComitesSemaforoEmailEliminarRequest request)
        {
            var validacion = ValidarEmailEliminarRequest(request);
            if (validacion.Code != 0) return validacion;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var emails = ObtenerEmailsPorIds(conn, request.ids_registro);
                var procesados = ProcesarEmailsDelete(conn, CodEmpresa, request.usuario, emails);

                if (procesados <= 0)
                    return DbHelper.ErrorResponse("No se eliminó ningún correo.");

                return DbHelper.OkResponse("Correo(s) eliminado(s) correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static CrComitesSemaforoData BuildSemaforoDefault(int idComite)
        {
            return new CrComitesSemaforoData
            {
                id_comite = idComite,
                unidad_tiempo = "DAY",
                unidad_tiempo_esp = "Días",
                alerta_roja = 0,
                alerta_amarilla = 0
            };
        }

        private static CrComitesSemaforoEmailLista BuildEmailListaVacia()
        {
            return new CrComitesSemaforoEmailLista
            {
                total = 0,
                lista = new List<CrComitesSemaforoEmailData>()
            };
        }

        private static ErrorDto ValidacionOk()
        {
            return DbHelper.OkResponse("Validación correcta.");
        }

        private static ErrorDto ValidarSemaforoRequest(CrComitesSemaforoGuardarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("Datos requeridos.");

            if (request.id_comite <= 0)
                return DbHelper.ErrorResponse("Debe seleccionar un comité.");

            request.unidad_tiempo = (request.unidad_tiempo ?? string.Empty).Trim().ToUpperInvariant();

            if (!UnidadTiempoValida(request.unidad_tiempo))
                return DbHelper.ErrorResponse("Unidad de tiempo inválida.");

            if (request.alerta_roja < 0)
                return DbHelper.ErrorResponse("La alerta roja no puede ser negativa.");

            if (request.alerta_amarilla < 0)
                return DbHelper.ErrorResponse("La alerta amarilla no puede ser negativa.");

            request.usuario = (request.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.ErrorResponse("Usuario requerido.");

            return ValidacionOk();
        }

        private static ErrorDto ValidarEmailAgregarRequest(CrComitesSemaforoEmailAgregarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("Datos requeridos.");

            if (request.id_comite <= 0)
                return DbHelper.ErrorResponse("Debe seleccionar un comité.");

            request.usuario = (request.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.ErrorResponse("Usuario requerido.");

            request.email = (request.email ?? string.Empty).Trim();

            if (!MAfilicacionDB.fxEmail_Valida(request.email))
                return DbHelper.ErrorResponse("Correo inválido.");

            return ValidacionOk();
        }

        private static ErrorDto ValidarEmailEliminarRequest(CrComitesSemaforoEmailEliminarRequest request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("Datos requeridos.");

            if (request.ids_registro == null || request.ids_registro.Count == 0)
                return DbHelper.ErrorResponse("Seleccione los correos que desea eliminar.");

            request.usuario = (request.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.ErrorResponse("Usuario requerido.");

            request.ids_registro = request.ids_registro
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (request.ids_registro.Count == 0)
                return DbHelper.ErrorResponse("Seleccione correos válidos para eliminar.");

            return ValidacionOk();
        }

        private static bool UnidadTiempoValida(string unidadTiempo)
        {
            var unidad = (unidadTiempo ?? string.Empty).Trim().ToUpperInvariant();
            return unidad is "DAY" or "HOUR" or "MINUTE";
        }

        private static ErrorDto<FiltrosLazyLoadData> ParseFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                return DbHelper.CreateOkResponse(filtros);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosLazyLoadData>(ex.Message);
            }
        }

        private static EmailFiltroData BuildEmailFiltro(FiltrosLazyLoadData filtros)
        {
            var idComite = ExtractIntFromFiltro(filtros.filtro, "id_comite");
            var texto = ExtractStringFromFiltro(filtros.filtro, "texto");
            var like = string.IsNullOrWhiteSpace(texto) ? null : $"%{texto.Trim()}%";

            return new EmailFiltroData
            {
                IdComite = idComite,
                Texto = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim(),
                Like = like
            };
        }

        private static int ExtractIntFromFiltro(string? filtroJson, string key)
        {
            var value = ExtractStringFromFiltro(filtroJson, key);
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static string ExtractStringFromFiltro(string? filtroJson, string key)
        {
            if (string.IsNullOrWhiteSpace(filtroJson))
                return string.Empty;

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object?>>(filtroJson);
                if (data == null || !data.TryGetValue(key, out var value))
                    return string.Empty;

                return Convert.ToString(value) ?? string.Empty;
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        private static int ObtenerEmailsTotal(IDbConnection conn, EmailFiltroData fx)
        {
            const string sql = @"
                select count(1)
                from CRD_COMITES_SEMAFORO_EMAIL
                where IdComite = @idComite
                  and (
                         @texto is null
                      or Email like @like
                      or UsuarioInserta like @like
                      or cast(IdRegistro as varchar(20)) like @like
                  );";

            return conn.QuerySingle<int>(sql, new
            {
                idComite = fx.IdComite,
                texto = fx.Texto,
                like = fx.Like
            });
        }

        private static List<CrComitesSemaforoEmailData> ObtenerEmailsLista(IDbConnection conn, EmailFiltroData fx)
        {
            const string sql = @"
                select
                    IdRegistro as id_registro,
                    IdComite as id_comite,
                    rtrim(Email) as email,
                    FechaInserta as fecha_inserta,
                    rtrim(UsuarioInserta) as usuario_inserta
                from CRD_COMITES_SEMAFORO_EMAIL
                where IdComite = @idComite
                  and (
                         @texto is null
                      or Email like @like
                      or UsuarioInserta like @like
                      or cast(IdRegistro as varchar(20)) like @like
                  )
                order by IdRegistro;";

            return conn.Query<CrComitesSemaforoEmailData>(sql, new
            {
                idComite = fx.IdComite,
                texto = fx.Texto,
                like = fx.Like
            }).ToList();
        }

        private static SpComitesSemaforoResult EjecutarSemaforoAdd(IDbConnection conn,CrComitesSemaforoGuardarRequest request)
        {
            const string sql = @"
        exec spCrd_Comites_Semaforo_Add
            @ComiteId,
            @UnidadTiempo,
            @AlertaRoja,
            @AlertaAmarilla,
            @Usuario;";

            var unidadTiempo = (request.unidad_tiempo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            var usuario = (request.usuario ?? string.Empty).Trim();

            return conn.QueryFirstOrDefault<SpComitesSemaforoResult>(sql, new
            {
                ComiteId = request.id_comite ?? 0,
                UnidadTiempo = unidadTiempo,
                AlertaRoja = request.alerta_roja ?? 0,
                AlertaAmarilla = request.alerta_amarilla ?? 0,
                Usuario = usuario
            }) ?? new SpComitesSemaforoResult();
        }

        private static SpComitesSemaforoResult EjecutarEmailAdd(
            IDbConnection conn,
            CrComitesSemaforoEmailAgregarRequest request)
        {
            const string sql = @"
                exec spCrd_Comites_Semaforo_Email_Add
                    @ComiteId,
                    @Email,
                    @Usuario;";

            return conn.QueryFirstOrDefault<SpComitesSemaforoResult>(sql, new
            {
                ComiteId = request.id_comite,
                Email = request.email.Trim(),
                Usuario = request.usuario.Trim()
            }) ?? new SpComitesSemaforoResult();
        }

        private static SpComitesSemaforoResult EjecutarEmailDelete(
            IDbConnection conn,
            int idRegistro,
            string usuario)
        {
            const string sql = @"
                exec spCrd_Comites_Semaforo_Email_Delete
                    @RegistroId,
                    @Usuario;";

            return conn.QueryFirstOrDefault<SpComitesSemaforoResult>(sql, new
            {
                RegistroId = idRegistro,
                Usuario = usuario.Trim()
            }) ?? new SpComitesSemaforoResult();
        }

        private static List<CrComitesSemaforoEmailData> ObtenerEmailsPorIds(IDbConnection conn, List<int> ids)
        {
            const string sql = @"
                select
                    IdRegistro as id_registro,
                    IdComite as id_comite,
                    rtrim(Email) as email,
                    FechaInserta as fecha_inserta,
                    rtrim(UsuarioInserta) as usuario_inserta
                from CRD_COMITES_SEMAFORO_EMAIL
                where IdRegistro in @ids;";

            return conn.Query<CrComitesSemaforoEmailData>(sql, new { ids }).ToList();
        }

        private int ProcesarEmailsDelete(
            IDbConnection conn,
            int CodEmpresa,
            string usuario,
            List<CrComitesSemaforoEmailData> emails)
        {
            var procesados = 0;

            foreach (var email in emails)
            {
                var result = EjecutarEmailDelete(conn, email.id_registro, usuario);
                if (result.Pass != 1) continue;

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    result.Movimiento,
                    BuildDetalleEmailDelete(email));

                procesados++;
            }

            return procesados;
        }

        private void RegistrarBitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            if (string.IsNullOrWhiteSpace(movimiento))
                return;

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario.Trim(),
                Movimiento = $"{movimiento.ToUpperInvariant()}-WEB",
                DetalleMovimiento = detalle ?? string.Empty,
                Modulo = vModulo
            });
        }

        private static string BuildDetalleEmailDelete(CrComitesSemaforoEmailData email)
        {
            return $"Comite [{email.id_comite}] Email: {email.email}";
        }

        private static string BuildSpMensaje(SpComitesSemaforoResult result, string fallback)
        {
            var mensaje = (result.Mensaje ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(mensaje) ? fallback : $"{fallback} {mensaje}";
        }

        private sealed class EmailFiltroData
        {
            public int IdComite { get; set; }
            public string? Texto { get; set; }
            public string? Like { get; set; }
        }

        private sealed class SpComitesSemaforoResult
        {
            public int Pass { get; set; } = 0;
            public string Mensaje { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public string Detalle { get; set; } = string.Empty;
        }
    }
}