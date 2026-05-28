using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrNivelesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 3;

        public FrmCrNivelesDB(IConfiguration config)
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
        /// Obtiene los procesos disponibles para niveles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Niveles_Procesos_Dropdown_Obtener(int CodEmpresa)
        {
            var lista = new List<DropDownListaGenericaModel>
            {
                new() { item = "F", descripcion = "Proceso de Formalización" },
                new() { item = "R", descripcion = "Proceso de Resolución" },
                new() { item = "N", descripcion = "Proceso de Anulación" }
            };

            return DbHelper.CreateOkResponse(lista);
        }

        /// <summary>
        /// Obtiene grupos para búsqueda F4 filtrados por proceso.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Niveles_Grupos_F4_Obtener(int CodEmpresa, string tipo, string? texto)
        {
            if (!TipoValido(tipo))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(CrNivelesConstantes.TipoProcesoInvalido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
                    select
                        cast(NV_Cod_Grupo as varchar(20)) as item,
                        rtrim(NV_Descripcion) as descripcion
                    from Nivel_Grupos
                    where NV_Tipo = @tipo
                      and (@texto = '' or NV_Descripcion like @like)
                    order by NV_Descripcion;";

                var lista = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    tipo = NormalizarTipo(tipo),
                    texto,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el grupo anterior o siguiente a partir de un grupo actual.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupoActual"></param>
        /// <param name="tipoProceso"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<CrNivelesGrupoDto> CR_Niveles_Grupo_Scroll_Obtener(int CodEmpresa, int grupoActual, string tipoProceso, int tipo)
        {
            if (!TipoValido(tipoProceso))
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>(CrNivelesConstantes.TipoProcesoInvalido);
            }

            if (tipo is not (0 or 1))
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>(CrNivelesConstantes.ScrollValido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var sql = tipo == 1
                    ? @"
                        select top 1
                            NV_Cod_Grupo as nv_cod_grupo,
                            rtrim(NV_Descripcion) as nv_descripcion,
                            rtrim(NV_Tipo) as nv_tipo,
                            isnull(NV_Desde, 0) as nv_desde,
                            isnull(NV_Hasta, 0) as nv_hasta,
                            cast(0 as bit) as isNew
                        from Nivel_Grupos
                        where NV_Tipo = @tipoProceso
                          and NV_Cod_Grupo < @grupoActual
                        order by NV_Cod_Grupo desc;"
                    : @"
                        select top 1
                            NV_Cod_Grupo as nv_cod_grupo,
                            rtrim(NV_Descripcion) as nv_descripcion,
                            rtrim(NV_Tipo) as nv_tipo,
                            isnull(NV_Desde, 0) as nv_desde,
                            isnull(NV_Hasta, 0) as nv_hasta,
                            cast(0 as bit) as isNew
                        from Nivel_Grupos
                        where NV_Tipo = @tipoProceso
                          and NV_Cod_Grupo > @grupoActual
                        order by NV_Cod_Grupo asc;";

                var item = conn.QueryFirstOrDefault<CrNivelesGrupoDto>(sql, new
                {
                    grupoActual,
                    tipoProceso = NormalizarTipo(tipoProceso)
                });

                if (item is null)
                {
                    return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>(CrNivelesConstantes.NoHayMasGrupos);
                }

                return DbHelper.CreateOkResponse(item);
            }
            catch (DbException)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>("No fue posible navegar entre grupos.");
            }
            catch (InvalidOperationException)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>("Error al navegar entre grupos.");
            }
        }

        /// <summary>
        /// Obtiene la información principal de un grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupoId"></param>
        /// <returns></returns>
        public ErrorDto<CrNivelesGrupoDetalleDto> CR_Niveles_Grupo_Obtener(int CodEmpresa, int grupoId)
        {
            if (grupoId <= 0)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDetalleDto>(CrNivelesConstantes.GrupoRequerido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    select
                        NV_Cod_Grupo as nv_cod_grupo,
                        rtrim(NV_Descripcion) as nv_descripcion,
                        rtrim(NV_Tipo) as nv_tipo,
                        isnull(NV_Desde, 0) as nv_desde,
                        isnull(NV_Hasta, 0) as nv_hasta,
                        cast(0 as bit) as isNew
                    from Nivel_Grupos
                    where NV_Cod_Grupo = @grupoId;";

                var grupo = conn.QueryFirstOrDefault<CrNivelesGrupoDto>(sql, new { grupoId });

                if (grupo is null)
                {
                    return DbHelper.CreateErrorResponse<CrNivelesGrupoDetalleDto>("El grupo indicado no existe.");
                }

                var miembros = ObtenerMiembros(conn, grupoId, string.Empty);
                var lineas = ObtenerLineas(conn, grupoId, string.Empty);

                var result = new CrNivelesGrupoDetalleDto
                {
                    grupo = grupo,
                    miembros = miembros,
                    lineas = lineas
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDetalleDto>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDetalleDto>(ex.Message);
            }
        }

        /// <summary>
        /// Guarda un grupo de niveles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrNivelesGrupoDto> CR_Niveles_Grupo_Guardar(int CodEmpresa, CrNivelesGrupoGuardarRequest request, string usuario)
        {
            var validacion = ValidarGrupo(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>(validacion.Description ?? "Error de validación.");
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var tx = conn.BeginTransaction();

            try
            {
                var grupoId = request.nv_cod_grupo.GetValueOrDefault();
                var descripcion = request.nv_descripcion.Trim().ToUpperInvariant();
                var tipo = NormalizarTipo(request.nv_tipo);
                var desde = request.nv_desde.GetValueOrDefault();
                var hasta = request.nv_hasta.GetValueOrDefault();
                var usuarioNorm = NormalizarUsuario(usuario);
                var movimiento = grupoId <= 0 ? "REGISTRA-WEB" : "MODIFICA-WEB";

                if (grupoId <= 0)
                {
                    const string sqlInsert = @"
                        insert into Nivel_Grupos(NV_Descripcion, NV_Tipo, NV_Desde, NV_Hasta)
                        values(@descripcion, @tipo, @desde, @hasta);

                        select cast(scope_identity() as int);";

                    grupoId = conn.QuerySingle<int>(sqlInsert, new
                    {
                        descripcion,
                        tipo,
                        desde,
                        hasta
                    }, tx);
                }
                else
                {
                    const string sqlUpdate = @"
                        update Nivel_Grupos
                        set NV_Descripcion = @descripcion,
                            NV_Desde = @desde,
                            NV_Hasta = @hasta
                        where NV_Cod_Grupo = @grupoId;";

                    var rows = conn.Execute(sqlUpdate, new
                    {
                        grupoId,
                        descripcion,
                        desde,
                        hasta
                    }, tx);

                    if (rows <= 0)
                    {
                        tx.Rollback();
                        return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>("No se actualizó ningún grupo.");
                    }
                }

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Niveles Grupo: {grupoId} - {descripcion}",
                    Movimiento = movimiento,
                    Modulo = vModulo
                });

                return CR_Niveles_Grupo_Obtener(CodEmpresa, grupoId).Code == 0
                    ? DbHelper.CreateOkResponse(new CrNivelesGrupoDto
                    {
                        nv_cod_grupo = grupoId,
                        nv_descripcion = descripcion,
                        nv_tipo = tipo,
                        nv_desde = desde,
                        nv_hasta = hasta,
                        isNew = false
                    })
                    : DbHelper.CreateErrorResponse<CrNivelesGrupoDto>("El grupo fue guardado, pero no fue posible recargarlo.");
            }
            catch (DbException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.CreateErrorResponse<CrNivelesGrupoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un grupo de niveles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupoId"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Niveles_Grupo_Eliminar(int CodEmpresa, int grupoId, string usuario)
        {
            if (grupoId <= 0)
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.GrupoRequerido);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var tx = conn.BeginTransaction();

            try
            {
                const string sqlDescripcion = @"
                    select rtrim(NV_Descripcion)
                    from Nivel_Grupos
                    where NV_Cod_Grupo = @grupoId;";

                var descripcion = conn.QueryFirstOrDefault<string>(sqlDescripcion, new { grupoId }, tx) ?? string.Empty;

                const string sqlDeleteDerechos = @"delete from Nivel_Derechos where NV_Cod_Grupo = @grupoId;";
                const string sqlDeleteMiembros = @"delete from Nivel_Miembros where NV_Cod_Grupo = @grupoId;";
                const string sqlDeleteGrupo = @"delete from Nivel_Grupos where NV_Cod_Grupo = @grupoId;";

                conn.Execute(sqlDeleteDerechos, new { grupoId }, tx);
                conn.Execute(sqlDeleteMiembros, new { grupoId }, tx);
                var rows = conn.Execute(sqlDeleteGrupo, new { grupoId }, tx);

                if (rows <= 0)
                {
                    tx.Rollback();
                    return DbHelper.ErrorResponse("No se eliminó ningún grupo.");
                }

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    DetalleMovimiento = $"Niveles Grupo: {grupoId} - {descripcion}",
                    Movimiento = "ELIMINA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Grupo eliminado correctamente.");
            }
            catch (DbException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de miembros asignables a un grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupoId"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<CrNivelesMiembroLista> CR_Niveles_Miembros_Lista_Obtener(int CodEmpresa, int grupoId, string? texto)
        {
            if (grupoId <= 0)
            {
                return DbHelper.CreateErrorResponse<CrNivelesMiembroLista>(CrNivelesConstantes.GrupoRequerido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                return DbHelper.CreateOkResponse(ObtenerMiembros(conn, grupoId, texto));
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrNivelesMiembroLista>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrNivelesMiembroLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista de líneas asignables a un grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="grupoId"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<CrNivelesLineaLista> CR_Niveles_Lineas_Lista_Obtener(int CodEmpresa, int grupoId, string? texto)
        {
            if (grupoId <= 0)
            {
                return DbHelper.CreateErrorResponse<CrNivelesLineaLista>(CrNivelesConstantes.GrupoRequerido);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                return DbHelper.CreateOkResponse(ObtenerLineas(conn, grupoId, texto));
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<CrNivelesLineaLista>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrNivelesLineaLista>(ex.Message);
            }
        }

        /// <summary>
        /// Asigna o desasigna un miembro a un grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Niveles_Miembro_Asignar(int CodEmpresa, CrNivelesAsignacionMiembroRequest request, string usuario)
        {
            var validacion = ValidarAsignacionMiembro(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var tx = conn.BeginTransaction();

            try
            {
                var grupoId = request.nv_cod_grupo.GetValueOrDefault();
                var nombre = request.nombre.Trim();
                var asignar = request.asignado.GetValueOrDefault();

                if (asignar)
                {
                    const string sqlInsert = @"
                        if not exists(
                            select 1
                            from Nivel_Miembros
                            where NV_Cod_Grupo = @grupoId
                              and Nombre = @nombre
                        )
                        begin
                            insert into Nivel_Miembros(NV_Cod_Grupo, Nombre)
                            values(@grupoId, @nombre)
                        end;";

                    conn.Execute(sqlInsert, new { grupoId, nombre }, tx);
                }
                else
                {
                    const string sqlDelete = @"
                        delete from Nivel_Miembros
                        where NV_Cod_Grupo = @grupoId
                          and Nombre = @nombre;";

                    conn.Execute(sqlDelete, new { grupoId, nombre }, tx);
                }

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    DetalleMovimiento = $"Niveles miembros, Grupo: {grupoId} Usuario: {nombre}",
                    Movimiento = asignar ? "REGISTRA-WEB" : "ELIMINA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(asignar ? "Miembro asignado correctamente." : "Miembro desasignado correctamente.");
            }
            catch (DbException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Asigna o desasigna una línea a un grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Niveles_Linea_Asignar(int CodEmpresa, CrNivelesAsignacionLineaRequest request, string usuario)
        {
            var validacion = ValidarAsignacionLinea(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var tx = conn.BeginTransaction();

            try
            {
                var grupoId = request.nv_cod_grupo.GetValueOrDefault();
                var codigo = request.codigo.Trim();
                var asignar = request.asignado.GetValueOrDefault();

                if (asignar)
                {
                    const string sqlInsert = @"
                        if not exists(
                            select 1
                            from Nivel_Derechos
                            where NV_Cod_Grupo = @grupoId
                              and Codigo = @codigo
                        )
                        begin
                            insert into Nivel_Derechos(NV_Cod_Grupo, Codigo)
                            values(@grupoId, @codigo)
                        end;";

                    conn.Execute(sqlInsert, new { grupoId, codigo }, tx);
                }
                else
                {
                    const string sqlDelete = @"
                        delete from Nivel_Derechos
                        where NV_Cod_Grupo = @grupoId
                          and Codigo = @codigo;";

                    conn.Execute(sqlDelete, new { grupoId, codigo }, tx);
                }

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    DetalleMovimiento = $"Niveles derechos, Grupo: {grupoId} Código: {codigo}",
                    Movimiento = asignar ? "REGISTRA-WEB" : "ELIMINA-WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse(asignar ? "Línea asignada correctamente." : "Línea desasignada correctamente.");
            }
            catch (DbException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                RollbackSeguro(tx);
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static CrNivelesMiembroLista ObtenerMiembros(IDbConnection conn, int grupoId, string? texto)
        {
            texto = (texto ?? string.Empty).Trim();
            var like = texto.Length > 0 ? $"%{texto}%" : null;

            const string sql = @"
                select
                    rtrim(U.Nombre) as nombre,
                    rtrim(U.Descripcion) as descripcion,
                    cast(case when M.NV_Cod_Grupo is null then 0 else 1 end as bit) as asignado
                from Usuarios U
                left join Nivel_Miembros M
                       on U.Nombre = M.Nombre
                      and M.NV_Cod_Grupo = @grupoId
                where U.Estado = 'A'
                  and (
                        @texto = ''
                     or U.Nombre like @like
                     or U.Descripcion like @like
                  )
                order by
                    case when M.NV_Cod_Grupo is null then 0 else 1 end desc,
                    U.Nombre;";

            var lista = conn.Query<CrNivelesMiembroDto>(sql, new
            {
                grupoId,
                texto,
                like
            }).ToList();

            return new CrNivelesMiembroLista
            {
                total = lista.Count,
                lista = lista
            };
        }

        private static CrNivelesLineaLista ObtenerLineas(IDbConnection conn, int grupoId, string? texto)
        {
            texto = (texto ?? string.Empty).Trim();
            var like = texto.Length > 0 ? $"%{texto}%" : null;

            const string sql = @"
        select
            rtrim(C.Codigo) as codigo,
            rtrim(C.Descripcion) as descripcion,
            cast(case when D.NV_Cod_Grupo is null then 0 else 1 end as bit) as asignado
        from Catalogo C
        left join Nivel_Derechos D
               on C.Codigo = D.Codigo
              and D.NV_Cod_Grupo = @grupoId
        where C.Retencion = 'N'
          and C.Poliza = 'N'
          and (
                @texto = ''
             or C.Codigo like @like
             or C.Descripcion like @like
          )
        order by
            case when D.NV_Cod_Grupo is null then 0 else 1 end desc,
            C.Codigo;";

            var lista = conn.Query<CrNivelesLineaDto>(sql, new
            {
                grupoId,
                texto,
                like
            }).ToList();

            return new CrNivelesLineaLista
            {
                total = lista.Count,
                lista = lista
            };
        }

        private static ErrorDto ValidarGrupo(CrNivelesGrupoGuardarRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Solicitud inválida.");
            }

            if (!TipoValido(request.nv_tipo))
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.TipoProcesoInvalido);
            }

            if (string.IsNullOrWhiteSpace(request.nv_descripcion))
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.GrupoDescripcionRequerida);
            }

            if (!request.nv_desde.HasValue || !request.nv_hasta.HasValue)
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.RangoInvalido);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto ValidarAsignacionMiembro(CrNivelesAsignacionMiembroRequest request)
        {
            if (request == null || request.nv_cod_grupo.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.GrupoRequerido);
            }

            if (string.IsNullOrWhiteSpace(request.nombre))
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.MiembroRequerido);
            }

            if (!request.asignado.HasValue)
            {
                return DbHelper.ErrorResponse("Debe indicar si asigna o desasigna el miembro.");
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto ValidarAsignacionLinea(CrNivelesAsignacionLineaRequest request)
        {
            if (request == null || request.nv_cod_grupo.GetValueOrDefault() <= 0)
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.GrupoRequerido);
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse(CrNivelesConstantes.LineaRequerida);
            }

            if (!request.asignado.HasValue)
            {
                return DbHelper.ErrorResponse("Debe indicar si asigna o desasigna la línea.");
            }

            return DbHelper.OkResponse("Ok");
        }

        private static bool TipoValido(string? tipo)
        {
            var t = NormalizarTipo(tipo);
            return t is "F" or "R" or "N";
        }

        private static string NormalizarTipo(string? tipo)
        {
            return (tipo ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static string NormalizarUsuario(string? usuario)
        {
            return (usuario ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static void RollbackSeguro(IDbTransaction tx)
        {
            try
            {
                tx.Rollback();
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}