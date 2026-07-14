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
        /// Obtiene el acta abierta o seleccionada del comite y sus asistentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaActual_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var actaSeleccionada = (acta ?? string.Empty).Trim();

            try
            {
                const string sqlActa = @"
                    declare @acta_consulta varchar(30) = @acta;

                    if @acta_consulta = ''
                    begin
                        select @acta_consulta = cast(dbo.fxCrd_Comites_Acta_Abierta(@id_comite) as varchar(30));
                    end;

                    if @acta_consulta <> '' and not exists (
                        select 1
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and cast(ACTA as varchar(30)) = @acta_consulta
                    )
                    begin
                        select top 1 @acta_consulta = cast(ACTA as varchar(30))
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and SESION_ID = @acta_consulta
                        order by cast(ACTA as int) desc;
                    end;

                    select top 1
                        @id_comite as id_comite,
                        isnull(cast(CA.ACTA as int),0) as id_acta,
                        rtrim(isnull(CA.SESION_ID,'')) as acta,
                        CA.FECHA as fecha,
                        case CA.ESTADO
                            when 'A' then 'Abierta'
                            when 'C' then 'Cerrada'
                            else rtrim(isnull(CA.ESTADO,''))
                        end as estado,
                        rtrim(isnull(CA.NOTAS,'')) as notas
                    from CRD_COMITES_ACTAS CA
                    where CA.ID_COMITE = @id_comite
                      and cast(CA.ACTA as varchar(30)) = @acta_consulta
                    order by isnull(cast(CA.ACTA as int),0) desc;";

                var actual = conn.QueryFirstOrDefault<CrComitesAprobacionesActaActual>(
                    sqlActa,
                    new { id_comite, acta = actaSeleccionada })
                    ?? new CrComitesAprobacionesActaActual { id_comite = id_comite, acta = actaSeleccionada };

                actual.asistencia = actual.id_acta > 0
                    ? ConsultarAsistenciaActa(conn, id_comite, actual.id_acta.ToString())
                    : new List<CrComitesAprobacionesActaAsistencia>();

                return DbHelper.CreateOkResponse(actual);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesActaActual>(ex.Message, -1, new CrComitesAprobacionesActaActual());
            }
        }

        /// <summary>
        /// Crea una nueva acta de comite usando el procedimiento original.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAprobacionesActaActual> CR_ComitesAprobaciones_ActaNueva_Crear(int CodEmpresa, int id_comite, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var nueva = conn.QueryFirstOrDefault(
                    "exec spCrd_Comites_Acta_Nueva @id_comite, @usuario;",
                    new { id_comite, usuario = (usuario ?? string.Empty).Trim() });

                if (nueva == null)
                {
                    return DbHelper.CreateErrorResponse<CrComitesAprobacionesActaActual>("No fue posible generar el acta.", -1, new CrComitesAprobacionesActaActual());
                }

                var campos = (IDictionary<string, object>)nueva;
                var actaValor = ValorCampo(campos, "acta", "ACTA");
                var sesion = ValorCampo(campos, "Sesion", "SESION", "sesion");
                var fecha = ValorCampo(campos, "fecha", "Fecha", "FECHA");

                var actual = new CrComitesAprobacionesActaActual
                {
                    id_comite = id_comite,
                    id_acta = Convert.ToInt32(actaValor ?? 0),
                    acta = Convert.ToString(sesion ?? string.Empty)?.Trim() ?? string.Empty,
                    fecha = fecha == null || fecha == DBNull.Value ? null : Convert.ToDateTime(fecha),
                    estado = "Abierta",
                    notas = string.Empty,
                    asistencia = new List<CrComitesAprobacionesActaAsistencia>()
                };

                return DbHelper.CreateOkResponse(actual);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAprobacionesActaActual>(ex.Message, -1, new CrComitesAprobacionesActaActual());
            }
        }

        /// <summary>
        /// Guarda la informacion del acta usando el procedimiento original.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Acta_Guardar(int CodEmpresa, CrComitesAprobacionesActaGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Acta
                        @id_comite,
                        @acta,
                        @fecha,
                        @notas,
                        @estado,
                        @usuario,
                        @sesion;";

                conn.Execute(
                    sql,
                    new
                    {
                        request.id_comite,
                        acta = request.acta.Trim(),
                        fecha = request.fecha.Date,
                        notas = request.notas.Trim(),
                        estado = EstadoActaSql(request.estado),
                        usuario = request.usuario.Trim(),
                        sesion = request.sesion.Trim()
                    });

                return DbHelper.OkResponse("Acta guardada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Cierra el acta usando el procedimiento original.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_Acta_Cerrar(int CodEmpresa, int id_comite, string acta, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cierre = conn.QueryFirstOrDefault(
                    "exec spCrd_Comites_Acta_Cierra @id_comite, @acta, @usuario;",
                    new
                    {
                        id_comite,
                        acta = (acta ?? string.Empty).Trim(),
                        usuario = (usuario ?? string.Empty).Trim()
                    });

                if (cierre == null)
                {
                    return DbHelper.ErrorResponse("No fue posible cerrar el acta.");
                }

                var campos = (IDictionary<string, object>)cierre;
                var pass = Convert.ToInt32(ValorCampo(campos, "Pass", "PASS") ?? 0);
                var mensaje = Convert.ToString(ValorCampo(campos, "Mensaje", "MENSAJE") ?? string.Empty)?.Trim() ?? string.Empty;

                if (pass == 1)
                {
                    var mensajeOk = string.IsNullOrWhiteSpace(mensaje) ? "Acta cerrada satisfactoriamente." : mensaje;
                    return DbHelper.OkResponse(mensajeOk);
                }

                var mensajeError = string.IsNullOrWhiteSpace(mensaje) ? "No fue posible cerrar el acta." : mensaje;
                return DbHelper.ErrorResponse(mensajeError);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la asistencia registrada para el acta seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesActaAsistencia>> CR_ComitesAprobaciones_ActaAsistencia_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var actaSeleccionada = (acta ?? string.Empty).Trim();

            try
            {
                return DbHelper.CreateOkResponse(ConsultarAsistenciaActa(conn, id_comite, actaSeleccionada));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesActaAsistencia>>(ex.Message, -1, new List<CrComitesAprobacionesActaAsistencia>());
            }
        }

        /// <summary>
        /// Actualiza la asistencia de un miembro del acta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_ComitesAprobaciones_ActaAsistencia_Guardar(int CodEmpresa, CrComitesAprobacionesActaAsistenciaGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    update CRD_COMITES_ACTAS_ASISTENCIA
                       set ASISTENCIA = @asistencia,
                           REGISTRO_FECHA = dbo.myGetdate(),
                           REGISTRO_USUARIO = @usuario
                     where ID_COMITE = @id_comite
                       and ACTA = @acta
                       and CEDULA = @cedula;";

                conn.Execute(
                    sql,
                    new
                    {
                        request.id_comite,
                        acta = request.acta.Trim(),
                        cedula = request.cedula.Trim(),
                        asistencia = request.asistencia ? 1 : 0,
                        usuario = request.usuario.Trim()
                    });

                return DbHelper.OkResponse("Asistencia actualizada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static List<CrComitesAprobacionesActaAsistencia> ConsultarAsistenciaActa(SqlConnection conn, int id_comite, string acta)
        {
            const string sqlAsistencia = @"
                    declare @acta_consulta varchar(30) = @acta;

                    if @acta_consulta = ''
                    begin
                        select @acta_consulta = cast(dbo.fxCrd_Comites_Acta_Abierta(@id_comite) as varchar(30));
                    end;

                    if @acta_consulta <> '' and not exists (
                        select 1
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and cast(ACTA as varchar(30)) = @acta_consulta
                    )
                    begin
                        select top 1 @acta_consulta = cast(ACTA as varchar(30))
                        from CRD_COMITES_ACTAS
                        where ID_COMITE = @id_comite
                          and SESION_ID = @acta_consulta
                        order by cast(ACTA as int) desc;
                    end;

                    exec spCrd_Comites_Acta_Asistencia_Consulta @id_comite, @acta_consulta;";

            return conn.Query(sqlAsistencia, new { id_comite, acta = (acta ?? string.Empty).Trim() }, commandTimeout: 8)
                .Select(row =>
                {
                    var campos = (IDictionary<string, object>)row;
                    var asistencia = ValorCampo(campos, "ASISTENCIA", "Asistencia") ?? 0;
                    var cedula = ValorCampo(campos, "Cedula", "CEDULA") ?? string.Empty;
                    var nombre = ValorCampo(campos, "Nombre", "NOMBRE") ?? string.Empty;

                    return new CrComitesAprobacionesActaAsistencia
                    {
                        seleccionado = Convert.ToInt32(asistencia) == 1,
                        cedula = Convert.ToString(cedula ?? string.Empty)?.Trim() ?? string.Empty,
                        nombre = Convert.ToString(nombre ?? string.Empty)?.Trim() ?? string.Empty
                    };
                })
                .ToList();
        }

        /// <summary>
        /// Obtiene el historico de actas de comite.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="fecha_inicio"></param>
        /// <param name="fecha_corte"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        private static object? ValorCampo(IDictionary<string, object> campos, params string[] nombres)
        {
            foreach (var nombre in nombres)
            {
                if (campos.TryGetValue(nombre, out var valor))
                {
                    return valor;
                }
            }

            return null;
        }

        private static DateTime? FechaCampo(IDictionary<string, object> campos, params string[] nombres)
        {
            var valor = ValorCampo(campos, nombres);
            if (valor == null || valor == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(valor);
        }

        private static string EstadoActaSql(string estado)
        {
            var valor = (estado ?? string.Empty).Trim();
            if (valor.Equals("Abierta", StringComparison.OrdinalIgnoreCase))
            {
                return "A";
            }

            if (valor.Equals("Cerrada", StringComparison.OrdinalIgnoreCase))
            {
                return "C";
            }

            return string.IsNullOrWhiteSpace(valor) ? "A" : valor.Substring(0, 1).ToUpperInvariant();
        }

        public ErrorDto<List<CrComitesAprobacionesActaHistorico>> CR_ComitesAprobaciones_ActasHistorico_Obtener(int CodEmpresa, int id_comite, DateTime fecha_inicio, DateTime fecha_corte, string identificacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    exec spCrd_Comites_Actas_Consulta
                        @id_comite,
                        @fecha_inicio,
                        @fecha_corte,
                        @identificacion;";

                var lista = conn.Query(
                    sql,
                    new
                    {
                        id_comite,
                        fecha_inicio = fecha_inicio.Date,
                        fecha_corte = fecha_corte.Date.AddDays(1).AddTicks(-1),
                        identificacion = identificacion.Trim()
                    })
                    .Select(row =>
                    {
                        var datos = (IDictionary<string, object>)row;
                        return new CrComitesAprobacionesActaHistorico
                        {
                            id_comite = Convert.ToInt32(ValorCampo(datos, "id_Comite", "ID_COMITE", "id_comite") ?? 0),
                            id_acta = Convert.ToInt32(ValorCampo(datos, "acta", "ACTA") ?? 0),
                            sesion = Convert.ToString(ValorCampo(datos, "Sesion_Id", "SESION_ID", "sesion") ?? string.Empty)?.Trim() ?? string.Empty,
                            fecha = FechaCampo(datos, "fecha", "FECHA"),
                            estado = Convert.ToString(ValorCampo(datos, "Estado_Desc", "ESTADO_DESC", "estado") ?? string.Empty)?.Trim() ?? string.Empty,
                            comite = Convert.ToString(ValorCampo(datos, "Comite_Desc", "COMITE_DESC", "comite") ?? string.Empty)?.Trim() ?? string.Empty,
                            registro_fecha = FechaCampo(datos, "Registro_fecha", "REGISTRO_FECHA"),
                            registro_usuario = Convert.ToString(ValorCampo(datos, "Registro_Usuario", "REGISTRO_USUARIO") ?? string.Empty)?.Trim() ?? string.Empty,
                            cierre_fecha = FechaCampo(datos, "CIERRE_FECHA", "Cierre_Fecha"),
                            cierre_usuario = Convert.ToString(ValorCampo(datos, "CIERRE_USUARIO", "Cierre_Usuario") ?? string.Empty)?.Trim() ?? string.Empty
                        };
                    })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesActaHistorico>>(ex.Message, -1, new List<CrComitesAprobacionesActaHistorico>());
            }
        }

        /// <summary>
        /// Obtiene resoluciones incluidas en el acta seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <param name="acta"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAprobacionesActaResolucion>> CR_ComitesAprobaciones_ActaResoluciones_Obtener(int CodEmpresa, int id_comite, string acta)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    declare @acta_consulta varchar(30) = @acta;

                    if @acta_consulta = ''
                    begin
                        select @acta_consulta = cast(dbo.fxCrd_Comites_Acta_Abierta(@id_comite) as varchar(30));
                    end;

                    if @acta_consulta <> '' and not exists (
                        select 1
                        from vCrd_Comites_Actas_Resoluciones
                        where ID_COMITE = @id_comite
                          and cast(ACTA as varchar(30)) = @acta_consulta
                    )
                    begin
                        select top 1 @acta_consulta = cast(ACTA as varchar(30))
                        from vCrd_Comites_Actas_Resoluciones
                        where ID_COMITE = @id_comite
                          and SESION_ID = @acta_consulta;
                    end;

                    select
                        ID_COMITE as id_comite,
                        isnull(cast(ACTA as int),0) as id_acta,
                        rtrim(isnull(SESION_ID,'')) as sesion,
                        rtrim(isnull(Cedula,'')) as cedula,
                        rtrim(isnull(Nombre,'')) as nombre,
                        rtrim(isnull(Cod_Linea,'')) as linea,
                        rtrim(isnull(Garantia,'')) as garantia,
                        rtrim(isnull(Estado,'')) as estado,
                        cast(isnull(Expediente,0) as varchar(30)) as operacion
                    from vCrd_Comites_Actas_Resoluciones
                    where ID_COMITE = @id_comite
                      and cast(ACTA as varchar(30)) = @acta_consulta
                    order by Nombre, Cedula;";

                var lista = conn.Query<CrComitesAprobacionesActaResolucion>(
                    sql,
                    new { id_comite, acta = acta.Trim() }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAprobacionesActaResolucion>>(ex.Message, -1, new List<CrComitesAprobacionesActaResolucion>());
            }
        }

    }
}
