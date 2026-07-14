using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrComitesAprobacionesDB
    {
        /// <summary>
        /// Obtiene causas y marca las registradas para el caso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_caso"></param>
        /// <param name="operacion"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesCausa>> CR_ComitesAprobaciones_Causas_Obtener(int CodEmpresa, string tipo_caso, string operacion, string tipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlSolicitud = @"
                    select C.COD_CAUSAS as cod_causas,
                           C.DESCRIPCION as descripcion,
                           C.TIPO as tipo,
                           cast(case when G.COD_CAUSAS is null then 0 else 1 end as bit) as seleccionada
                    from OPERACION_CAUSAS C
                    left join OPERACION_GESTION G
                      on C.COD_CAUSAS = G.COD_CAUSAS
                     and C.TIPO = G.TIPO
                     and G.ID_SOLICITUD = @operacion
                    where C.ESTADO = 1
                      and C.TIPO = @tipo
                    order by C.COD_CAUSAS;";

                const string sqlPreanalisis = @"
                    select C.COD_CAUSAS as cod_causas,
                           C.DESCRIPCION as descripcion,
                           C.TIPO as tipo,
                           cast(case when G.COD_CAUSAS is null then 0 else 1 end as bit) as seleccionada
                    from OPERACION_CAUSAS C
                    left join CRD_PREA_GESTION G
                      on C.COD_CAUSAS = G.COD_CAUSAS
                     and C.TIPO = G.TIPO
                     and G.COD_PREANALISIS = @operacion
                    where C.ESTADO = 1
                      and C.TIPO = @tipo
                    order by C.COD_CAUSAS;";

                var sql = EsSolicitud(tipo_caso) ? sqlSolicitud : sqlPreanalisis;
                var lista = conn.Query<CrComitesAprobacionesCausa>(
                    sql,
                    new { operacion = operacion.Trim(), tipo = tipo.Trim() }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesCausa>>(ex.Message, -1, new List<CrComitesAprobacionesCausa>());
            }
        }

        /// <summary>
        /// Registra la resolucion del comite.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Resolucion_Guardar(int CodEmpresa, CrComitesAprobacionesResolucionRequest request)
        {
            var validacion = ValidarResolucion(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var estado = NormalizarEstado(request.estado, out var estadoComite, out var editable);
                using var tx = conn.BeginTransaction();

                var validacionNegocio = ValidarResolucionNegocio(conn, tx, request);
                if (validacionNegocio.Code != 0)
                {
                    tx.Rollback();
                    return validacionNegocio;
                }

                var tag = ObtenerTagResolucion(conn, tx, request.estado);
                var usuarios = UsuariosNormalizados(request);
                var linea = ObtenerLineaResolucion(conn, tx, request);
                var notaTag = CrearNotaTagResolucion(conn, tx, request, usuarios);

                ValidarTagResolucion(conn, tx, tag);
                RegistrarTagResolucion(conn, tx, request, linea, tag, notaTag, usuarios);

                conn.Execute(
                    @"
                    exec spCrd_Comites_Resolucion_Add
                        @Comite,
                        @Acta,
                        @Usuario,
                        @Tipo,
                        @Operacion,
                        @Observacion,
                        @Estado,
                        @EstadoComite,
                        @Editable,
                        @AcuerdoJD,
                        @Usuario2,
                        @Usuario3;",
                    new
                    {
                        Comite = request.id_comite,
                        Acta = request.acta.Trim(),
                        Usuario = PrimerUsuario(request),
                        Tipo = request.tipo_caso.Trim(),
                        Operacion = request.operacion.Trim(),
                        Observacion = Truncar(request.observacion, 1000),
                        Estado = estado,
                        EstadoComite = estadoComite,
                        Editable = editable,
                        AcuerdoJD = request.acuerdo_jd.Trim(),
                        Usuario2 = UsuarioEnIndice(request, 1),
                        Usuario3 = UsuarioEnIndice(request, 2)
                    },
                    tx);

                foreach (var usuario in usuarios)
                {
                    conn.Execute(
                        @"
                        exec spCrd_Comites_Resolucion_Autorizadores_Add
                            @Comite,
                            @Acta,
                            @UsuarioRegistra,
                            @Tipo,
                            @Operacion,
                            @Observacion,
                            @Estado,
                            @UsuarioAutoriza;",
                        new
                        {
                            Comite = request.id_comite,
                            Acta = request.acta.Trim(),
                            UsuarioRegistra = request.usuario_registra.Trim(),
                            Tipo = request.tipo_caso.Trim(),
                            Operacion = request.operacion.Trim(),
                            Observacion = Truncar(request.observacion, 1000),
                            Estado = estado,
                            UsuarioAutoriza = usuario
                        },
                        tx);
                }

                RegistrarNotificacionResolucion(conn, tx, request, tag, notaTag, estado);
                tx.Commit();

                return DbHelper.OkResponse("Resolucion registrada correctamente.");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Registra las causas seleccionadas para el caso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Causas_Guardar(int CodEmpresa, CrComitesAprobacionesCausasGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var parametrosBase = new { tipo = request.tipo.Trim(), operacion = request.operacion.Trim() };
                var esSolicitud = EsSolicitud(request.tipo_caso);

                if (esSolicitud)
                {
                    conn.Execute(
                        "delete from OPERACION_GESTION where TIPO = @tipo and ID_SOLICITUD = @operacion",
                        parametrosBase);
                }
                else
                {
                    conn.Execute(
                        "delete from CRD_PREA_GESTION where TIPO = @tipo and COD_PREANALISIS = @operacion",
                        parametrosBase);
                }

                var causasNormalizadas = request.causas
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct();

                foreach (var causa in causasNormalizadas)
                {
                    var parametros = new
                    {
                        causa,
                        tipo = request.tipo.Trim(),
                        operacion = request.operacion.Trim(),
                        usuario = request.usuario.Trim()
                    };

                    if (esSolicitud)
                    {
                        conn.Execute(
                            @"
                            insert into OPERACION_GESTION
                            (COD_CAUSAS, TIPO, ID_SOLICITUD, CODIGO, REGISTRO_FECHA, REGISTRO_USUARIO)
                            values (@causa, @tipo, @operacion, '', dbo.Mygetdate(), @usuario);",
                            parametros);
                    }
                    else
                    {
                        conn.Execute(
                            @"
                            insert into CRD_PREA_GESTION
                            (COD_CAUSAS, TIPO, COD_PREANALISIS, CODIGO, REGISTRO_FECHA, REGISTRO_USUARIO)
                            values (@causa, @tipo, @operacion, '', dbo.Mygetdate(), @usuario);",
                            parametros);
                    }
                }

                return DbHelper.OkResponse("Causas guardadas correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto ValidarResolucionNegocio(SqlConnection conn, SqlTransaction tx, CrComitesAprobacionesResolucionRequest request)
        {
            var usuarios = UsuariosNormalizados(request);
            var aprobacionesRequeridas = ObtenerAprobacionesRequeridas(conn, tx, request.id_comite);
            if (usuarios.Count < aprobacionesRequeridas)
            {
                return DbHelper.ErrorResponse("Debe validar todos los usuarios autorizadores.", -2);
            }

            var duplicado = usuarios
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Any(x => x.Count() > 1);
            if (duplicado)
            {
                return DbHelper.ErrorResponse("El usuario 1 no puede se igual que el 2, proceda a cambiarlo", -2);
            }

            const string sqlActa = "select dbo.fxCrd_Comites_Acta_Valida(@id_comite, @acta);";
            var actaValida = conn.QueryFirstOrDefault<int>(
                sqlActa,
                new { request.id_comite, acta = request.acta.Trim() },
                tx);
            if (actaValida == 0)
            {
                return DbHelper.ErrorResponse($"El Acta No.{request.acta.Trim()}, no existe o no esta abierta!", -2);
            }

            if (EsSolicitud(request.tipo_caso) || !request.estado.Trim().Equals("A", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.OkResponse(string.Empty);
            }

            var validacion = conn.QueryFirstOrDefault(
                "exec spCrdPrea_Comite_Validacion_Resolucion @operacion, @id_comite;",
                new { operacion = request.operacion.Trim(), request.id_comite },
                tx);

            if (validacion == null)
            {
                return DbHelper.OkResponse(string.Empty);
            }

            var datos = (IDictionary<string, object>)validacion;
            var lineaCaso = ObtenerLineaResolucion(conn, tx, request);
            var lineaValidar = Texto(datos, "LineaValida");
            var trasladaSalario = Decimal(datos, "TRASLADA_SALARIO") == 1;
            var montoCreditos = Decimal(datos, "MontoCreditos");
            var montoMaximo = Decimal(datos, "MontoMaximo");
            var liquidez = Decimal(datos, "Liquidez");
            var liquidezMinima = Decimal(datos, "LiquidezMinima");

            if (lineaValidar.Equals(lineaCaso, StringComparison.OrdinalIgnoreCase) && !trasladaSalario && !request.confirmar_traslado_salario)
            {
                return DbHelper.ErrorResponse(
                    $"Se esta realizando una aprobacion de una linea {lineaValidar} sin traslado de salario activo. Desea continuar con la aprobacion?",
                    -3);
            }

            if (montoCreditos > montoMaximo && montoMaximo > 0)
            {
                return DbHelper.ErrorResponse($"Excede el monto autorizado de aprobacion: {montoMaximo:N2} para este nivel resolutivo.", -2);
            }

            if (liquidez < liquidezMinima)
            {
                return DbHelper.ErrorResponse($"No se puede aprobar debido a que no cumple con el % de liquidez minima ({liquidezMinima}) requerida para el tipo de garantia, favor validar.", -2);
            }

            return DbHelper.OkResponse(string.Empty);
        }

        private static int ObtenerAprobacionesRequeridas(SqlConnection conn, SqlTransaction tx, int idComite)
        {
            const string sql = "select isnull(NAPROBACIONES, 1) from COMITES where ID_COMITE = @idComite;";
            var aprobaciones = conn.QueryFirstOrDefault<int>(sql, new { idComite }, tx);
            return aprobaciones <= 0 ? 1 : aprobaciones;
        }

        private static string ObtenerTagResolucion(SqlConnection conn, SqlTransaction tx, string estado)
        {
            var codParametro = estado.Trim().ToUpperInvariant() switch
            {
                "A" => "01",
                "D" => "02",
                "P" or "V" or "VL" => "03",
                _ => "01"
            };

            const string sql = "select isnull(valor, '') from CRD_COMITES_PARAMETROS where COD_PARAMETRO = @codParametro;";
            var tag = conn.QueryFirstOrDefault<string>(sql, new { codParametro }, tx)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new InvalidOperationException("No esta definido en parametros, el tag para este movimiento");
            }

            return tag;
        }

        private static void ValidarTagResolucion(SqlConnection conn, SqlTransaction tx, string tag)
        {
            const string sql = "select count(*) from crd_tags where tag_codigo = @tag;";
            var existe = conn.QueryFirstOrDefault<int>(sql, new { tag }, tx);
            if (existe == 0)
            {
                throw new InvalidOperationException("El tag definido en parametros para este movimiento, no existe en el catalogo de tags");
            }
        }

        private static void RegistrarTagResolucion(
            SqlConnection conn,
            SqlTransaction tx,
            CrComitesAprobacionesResolucionRequest request,
            string linea,
            string tag,
            string notaTag,
            List<string> usuarios)
        {
            var usuarioRegistro = usuarios.Count > 0 ? usuarios[0] : request.usuario_registra.Trim();

            if (EsSolicitud(request.tipo_caso))
            {
                MCredito.sbCrdOperacionTags(conn, tx, new MCredito.CrOperacionTagRegistrarRequest
                {
                    operacion = Convert.ToInt64(request.operacion.Trim()),
                    linea = linea,
                    tag = tag,
                    usuario = request.usuario_registra.Trim(),
                    notas = notaTag
                });
                return;
            }

            const string lineaSql = @"
                select isnull(max(linea),0)+1
                from CRD_PREA_TAGS
                where cod_preanalisis = @operacion;";

            var lineaTag = conn.QueryFirstOrDefault<int>(
                lineaSql,
                new { operacion = request.operacion.Trim() },
                tx);

            const string insertSql = @"
                insert CRD_PREA_TAGS
                (LINEA, CODIGO, COD_PREANALISIS, TAG_CODIGO, ASIGNADO_A, REGISTRO_FECHA, REGISTRO_USUARIO, NOTAS)
                values
                (@lineaTag, @linea, @operacion, @tag, '', dbo.MyGetdate(), @usuarioRegistro, @notaTag);";

            conn.Execute(
                insertSql,
                new
                {
                    lineaTag,
                    linea,
                    operacion = request.operacion.Trim(),
                    tag,
                    usuarioRegistro,
                    notaTag
                },
                tx);
        }

        private static void RegistrarNotificacionResolucion(
            SqlConnection conn,
            SqlTransaction tx,
            CrComitesAprobacionesResolucionRequest request,
            string tag,
            string notaTag,
            string estado)
        {
            var codParametro = estado.Trim().ToUpperInvariant() switch
            {
                "A" => "04",
                "P" => "05",
                "D" => "06",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(codParametro) || !EnviarNotificacionResolucion(conn, tx, codParametro))
            {
                return;
            }

            var email = ObtenerCorreoResolucion(conn, tx, request);
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            var asunto = ObtenerAsuntoResolucion(conn, tx, request, tag);

            conn.Execute(
                "exec spSys_CORREO_POOL @cuerpo, @asunto, 'P', @email;",
                new
                {
                    cuerpo = notaTag.Trim(),
                    asunto = asunto.Trim(),
                    email = email.Trim()
                },
                tx);
        }

        private static bool EnviarNotificacionResolucion(SqlConnection conn, SqlTransaction tx, string codParametro)
        {
            const string sql = "select isnull(valor, '') from CRD_COMITES_PARAMETROS where COD_PARAMETRO = @codParametro;";
            var valor = conn.QueryFirstOrDefault<string>(sql, new { codParametro }, tx)?.Trim() ?? string.Empty;
            return valor.Equals("S", StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenerCorreoResolucion(SqlConnection conn, SqlTransaction tx, CrComitesAprobacionesResolucionRequest request)
        {
            if (EsSolicitud(request.tipo_caso))
            {
                const string sqlSolicitud = @"
                    select isnull(U.EMAIL, '')
                    from REG_CREDITOS R
                    inner join USUARIOS U on R.USERREC = U.NOMBRE
                    where R.ID_SOLICITUD = @operacion;";

                return conn.QueryFirstOrDefault<string>(
                    sqlSolicitud,
                    new { operacion = request.operacion.Trim() },
                    tx)?.Trim() ?? string.Empty;
            }

            const string sqlEstudio = @"
                select isnull(U.EMAIL, '')
                from CRD_PREA_PREANALISIS P
                inner join USUARIOS U on P.USUARIO = U.NOMBRE
                where COD_PREANALISIS = @operacion;";

            return conn.QueryFirstOrDefault<string>(
                sqlEstudio,
                new { operacion = request.operacion.Trim() },
                tx)?.Trim() ?? string.Empty;
        }

        private static string ObtenerAsuntoResolucion(SqlConnection conn, SqlTransaction tx, CrComitesAprobacionesResolucionRequest request, string tag)
        {
            const string sql = "select descripcion from CRD_TAGS where TAG_CODIGO = @tag;";
            var descripcion = conn.QueryFirstOrDefault<string>(sql, new { tag }, tx)?.Trim() ?? request.operacion.Trim();
            var tipo = EsSolicitud(request.tipo_caso) ? " Solicitud:" : " Estudio de Credito:";
            return $"{descripcion}{tipo}{request.operacion.Trim()}";
        }

        private static string CrearNotaTagResolucion(SqlConnection conn, SqlTransaction tx, CrComitesAprobacionesResolucionRequest request, List<string> usuarios)
        {
            const string sql = "select rtrim(isnull(TIPO_APROBACION, '')) from COMITES where ID_COMITE = @idComite;";
            var tipoAprobacion = conn.QueryFirstOrDefault<string>(sql, new { idComite = request.id_comite }, tx)?.Trim() ?? string.Empty;
            var primerUsuario = usuarios.Count > 0 ? usuarios[0] : UsuarioEnIndice(request, 0);
            var prefijo = tipoAprobacion.Equals("M", StringComparison.OrdinalIgnoreCase)
                ? $"({UsuarioEnIndice(request, 0)},{UsuarioEnIndice(request, 1)},{UsuarioEnIndice(request, 2)}) "
                : $"({primerUsuario}) ";

            return Truncar(prefijo + request.observacion, 998);
        }

        private static string ObtenerLineaResolucion(SqlConnection conn, SqlTransaction tx, CrComitesAprobacionesResolucionRequest request)
        {
            const string sql = "exec spCrd_Comites_Caso_CRD @operacion, @tipo;";
            var row = conn.QueryFirstOrDefault(
                sql,
                new
                {
                    operacion = request.operacion.Trim(),
                    tipo = EsSolicitud(request.tipo_caso) ? "T" : "E"
                },
                tx);

            if (row == null)
            {
                return string.Empty;
            }

            return Texto((IDictionary<string, object>)row, "Codigo");
        }

        private static List<string> UsuariosNormalizados(CrComitesAprobacionesResolucionRequest request)
        {
            return request.usuarios
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();
        }

    }
}
