using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComitesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCreditos = 3;
        private const string MovimientoRegistraWeb = "REGISTRA-WEB";
        private const string MovimientoModificaWeb = "MODIFICA-WEB";
        private const string MovimientoEliminaWeb = "ELIMINA-WEB";

        public FrmCrComitesDB(IConfiguration config)
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
        /// Obtiene la lista de comités de resolución de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesLista> CR_Comites_Lista_Obtener(int CodEmpresa, string parametros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        C.ID_COMITE as id_comite,
                        rtrim(isnull(C.DESCRIPCION,'')) as descripcion,
                        isnull(C.ACTA,0) as acta,
                        rtrim(isnull(C.ABREVIATURA,'')) as abreviatura,
                        rtrim(isnull(C.ORDEN,'')) as orden,
                        rtrim(isnull(C.TIPO_APROBACION,'')) as tipo_aprobacion,
                        isnull(C.NAPROBACIONES,0) as naprobaciones,
                        isnull(C.RNG_INICIO,0) as rng_inicio,
                        isnull(C.RNG_CORTE,0) as rng_corte,
                        cast(isnull(C.LINEA_FILTRA,0) as bit) as linea_filtra,
                        cast(isnull(C.ESTADO,0) as bit) as estado,
                        cast(0 as bit) as isNew
                    from COMITES C
                    order by C.ID_COMITE;";

                var lista = conn.Query<CrComitesData>(sql).ToList();

                return DbHelper.CreateOkResponse(new CrComitesLista
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesLista>(ex.Message, -1, new CrComitesLista());
            }
        }

        /// <summary>
        /// Exporta la lista de comités de resolución de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesLista> CR_Comites_Lista_Export(int CodEmpresa, string parametros)
        {
            return CR_Comites_Lista_Obtener(CodEmpresa, parametros);
        }

        /// <summary>
        /// Guarda un comité de resolución de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesGuardarResult> CR_Comites_Guardar(int CodEmpresa, CrComitesGuardarRequest request, string usuario)
        {
            var validacion = ValidarComite(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrComitesGuardarResult>(
                validacion.Description ?? "Error de validación.",
                validacion.Code.GetValueOrDefault(),
                new CrComitesGuardarResult());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var esNuevo = request.id_comite.GetValueOrDefault() <= 0;
                var result = esNuevo
                    ? InsertarComite(conn, CodEmpresa, request, usuario)
                    : ActualizarComite(conn, CodEmpresa, request, usuario);

                return result;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesGuardarResult>(ex.Message, -1, new CrComitesGuardarResult());
            }
        }

        /// <summary>
        /// Elimina un comité de resolución de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Comites_Eliminar(int CodEmpresa, int id_comite, string usuario)
        {
            if (id_comite <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar un comité válido.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Elimina
                        @ComiteId,
                        @Usuario;";

                var result = conn.QueryFirstOrDefault<CrComitesEliminarResult>(sql, new
                {
                    ComiteId = id_comite,
                    Usuario = NormalizarUsuario(usuario)
                }) ?? new CrComitesEliminarResult
                {
                    pass = 0,
                    mensaje = "No se obtuvo respuesta del proceso de eliminación."
                };

                if (result.pass != 1)
                {
                    return DbHelper.ErrorResponse(result.mensaje);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    DetalleMovimiento = $"Comites Id: {id_comite}",
                    Movimiento = MovimientoEliminaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse($"Comité Id: {id_comite}, Eliminado Satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los niveles de aprobación disponibles para comités.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesNivelAprobacionDto>> CR_Comites_NivelAprobacion_Dropdown_Obtener(int CodEmpresa)
        {
            var lista = new List<CrComitesNivelAprobacionDto>
            {
                new() { item = "E", descripcion = "Ejecutivo" },
                new() { item = "M", descripcion = "Mancomunado" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene los límites de aprobación por garantía del comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesGarantiasLista> CR_Comites_Garantias_Lista_Obtener(int CodEmpresa, int id_comite, string usuario)
        {
            if (id_comite <= 0)
            {
                return DbHelper.CreateOkResponse(new CrComitesGarantiasLista());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Garantias_Rangos_Consulta
                        @Comite,
                        @Usuario;";

                var lista = conn.Query<CrComitesGarantiasData>(sql, new
                {
                    Comite = id_comite,
                    Usuario = NormalizarUsuario(usuario)
                }).Select(x =>
                {
                    x.id_comite = id_comite;
                    x.isNew = false;
                    return x;
                }).ToList();

                return DbHelper.CreateOkResponse(new CrComitesGarantiasLista
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesGarantiasLista>(ex.Message, -1, new CrComitesGarantiasLista());
            }
        }

        /// <summary>
        /// Exporta los límites de aprobación por garantía del comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesGarantiasLista> CR_Comites_Garantias_Lista_Export(int CodEmpresa, int id_comite, string usuario)
        {
            return CR_Comites_Garantias_Lista_Obtener(CodEmpresa, id_comite, usuario);
        }

        /// <summary>
        /// Guarda los límites de aprobación por garantía del comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Comites_Garantias_Guardar(int CodEmpresa, CrComitesGarantiasGuardarRequest request, string usuario)
        {
            var validacion = ValidarGarantia(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Garantias_Rangos_Registra
                        @Comite,
                        @Garantia,
                        @RngInicio,
                        @RngCorte,
                        @Usuario;";

                conn.Execute(sql, new
                {
                    Comite = request.id_comite.GetValueOrDefault(),
                    Garantia = NormalizarTexto(request.cod_garantia),
                    RngInicio = request.rng_inicio.GetValueOrDefault(),
                    RngCorte = request.rng_corte.GetValueOrDefault(),
                    Usuario = NormalizarUsuario(usuario)
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    DetalleMovimiento = $"Comité Rng Gar. {NormalizarTexto(request.cod_garantia)}, Id Comite: {request.id_comite.GetValueOrDefault()} Mnt: {request.rng_corte.GetValueOrDefault():N2}",
                    Movimiento = MovimientoModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse($"Se ha modificado Comité > Rango Garantía > {NormalizarTexto(request.cod_garantia)} > Id Comite: {request.id_comite.GetValueOrDefault()} Mnt: {request.rng_corte.GetValueOrDefault():N2}");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las líneas de crédito autorizadas del comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesLineasLista> CR_Comites_Lineas_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            if (id_comite <= 0)
            {
                return DbHelper.CreateOkResponse(new CrComitesLineasLista());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Lineas_Asigna_Consulta
                        @Comite;";

                var lista = conn.Query<CrComitesLineasData>(sql, new
                {
                    Comite = id_comite
                }).Select(x =>
                {
                    x.id_comite = id_comite;
                    return x;
                }).ToList();

                return DbHelper.CreateOkResponse(new CrComitesLineasLista
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesLineasLista>(ex.Message, -1, new CrComitesLineasLista());
            }
        }

        /// <summary>
        /// Asigna o desasigna una línea de crédito al comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Comites_Lineas_Asignar(int CodEmpresa, CrComitesLineasAsignarRequest request, string usuario)
        {
            var validacion = ValidarLinea(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var movimiento = request.asignado.GetValueOrDefault() ? "I" : "E";

                const string sql = @"
                    exec spCrd_Comites_Lineas_Asigna_Registra
                        @Comite,
                        @Codigo,
                        @Usuario,
                        @Movimiento;";

                conn.Execute(sql, new
                {
                    Comite = request.id_comite.GetValueOrDefault(),
                    Codigo = NormalizarTexto(request.codigo),
                    Usuario = NormalizarUsuario(usuario),
                    Movimiento = movimiento
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    DetalleMovimiento = $"Comités asignación línea, Id: {request.id_comite.GetValueOrDefault()} .. Código: {NormalizarTexto(request.codigo)}",
                    Movimiento = movimiento == "I" ? MovimientoRegistraWeb : MovimientoEliminaWeb,
                    Modulo = ModuloCreditos
                });

                var mensaje = movimiento == "I"
                    ? $"Se ha vinculado la linea {NormalizarTexto(request.codigo)} al comité"
                    : $"Se ha Desvinculado la linea {NormalizarTexto(request.codigo)} al comité";

                return DbHelper.OkResponse(mensaje);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto<CrComitesGuardarResult> InsertarComite(SqlConnection conn, int CodEmpresa, CrComitesGuardarRequest request, string usuario)
        {
            var result = EjecutarGuardarComite(conn, request, usuario);

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = NormalizarUsuario(usuario),
                DetalleMovimiento = $"Comité Resolución Id: {result.id_comite}",
                Movimiento = MovimientoRegistraWeb,
                Modulo = ModuloCreditos
            });

            return DbHelper.CreateOkResponse(
                result,
                $"Se Registra Comité: {NormalizarTexto(request.descripcion)} satisfactoriamente!");
        }

        private ErrorDto<CrComitesGuardarResult> ActualizarComite(SqlConnection conn, int CodEmpresa, CrComitesGuardarRequest request, string usuario)
        {
            var result = EjecutarGuardarComite(conn, request, usuario);

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = NormalizarUsuario(usuario),
                DetalleMovimiento = $"Comité Resolución Id: {result.id_comite}",
                Movimiento = MovimientoModificaWeb,
                Modulo = ModuloCreditos
            });

            return DbHelper.CreateOkResponse(
                result,
                $"Se Modifica Comité: {NormalizarTexto(request.descripcion)} satisfactoriamente!");
        }

        private static CrComitesGuardarResult EjecutarGuardarComite(SqlConnection conn, CrComitesGuardarRequest request, string usuario)
        {
            const string sql = @"
                exec spCrd_Comites_Registro
                    @Comite,
                    @Descripcion,
                    @Acta,
                    @Tipo,
                    @NoAprobacion,
                    @RngInicio,
                    @RngCorte,
                    @LineaFiltra,
                    @Activo,
                    @Usuario,
                    @Abreviatura,
                    @Orden;";

            return conn.QueryFirstOrDefault<CrComitesGuardarResult>(sql, new
            {
                Comite = request.id_comite.GetValueOrDefault(),
                Descripcion = NormalizarTexto(request.descripcion),
                Acta = request.acta.GetValueOrDefault(),
                Tipo = NormalizarTipoAprobacion(request.tipo_aprobacion),
                NoAprobacion = request.naprobaciones.GetValueOrDefault(),
                RngInicio = request.rng_inicio.GetValueOrDefault(),
                RngCorte = request.rng_corte.GetValueOrDefault(),
                LineaFiltra = request.linea_filtra.GetValueOrDefault() ? 1 : 0,
                Activo = request.estado.GetValueOrDefault() ? 1 : 0,
                Usuario = NormalizarUsuario(usuario),
                Abreviatura = NormalizarTexto(request.abreviatura),
                Orden = NormalizarTexto(request.orden)
            }) ?? new CrComitesGuardarResult();
        }

        private static ErrorDto ValidarComite(CrComitesGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Datos del comité requeridos.");
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return DbHelper.ErrorResponse("Debe indicar la descripción del comité.");
            }

            if (string.IsNullOrWhiteSpace(NormalizarTipoAprobacion(request.tipo_aprobacion)))
            {
                return DbHelper.ErrorResponse("Debe indicar el nivel de aprobación.");
            }

            if (request.naprobaciones.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la cantidad de resolutores.");
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto ValidarGarantia(CrComitesGarantiasGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Datos de garantía requeridos.");
            }

            if (request.id_comite.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar un comité válido.");
            }

            if (string.IsNullOrWhiteSpace(request.cod_garantia))
            {
                return DbHelper.ErrorResponse("Debe indicar la garantía.");
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto ValidarLinea(CrComitesLineasAsignarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Datos de línea requeridos.");
            }

            if (request.id_comite.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse("Debe seleccionar un comité válido.");
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar la línea.");
            }

            return DbHelper.OkResponse("Ok");
        }

        private static string NormalizarTexto(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string NormalizarUsuario(string? usuario)
        {
            return (usuario ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string NormalizarTipoAprobacion(string? value)
        {
            var texto = NormalizarTexto(value).ToUpperInvariant();

            if (texto.StartsWith("E", StringComparison.Ordinal))
            {
                return "E";
            }

            if (texto.StartsWith("M", StringComparison.Ordinal))
            {
                return "M";
            }

            return string.Empty;
        }
    }
}