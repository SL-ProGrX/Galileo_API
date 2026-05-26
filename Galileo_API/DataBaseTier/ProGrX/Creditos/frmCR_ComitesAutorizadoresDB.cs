using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComitesAutorizadoresDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCreditos = 3;
        private const string RegistraWeb = "REGISTRA-WEB";
        private const string ModificaWeb = "MODIFICA-WEB";
        private const string EliminaWeb = "ELIMINA-WEB";
        private const string ERRROFILTROS = "Error al procesar filtros.";
        private const string CEDULAREQUERIDA = "Cédula requerida.";

        private sealed class PersonaNormalizada
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string UsuarioMiembro { get; set; } = string.Empty;
            public string IdPuesto { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
        }

        private sealed class PaginacionInfo
        {
            public int Offset { get; set; }
            public int Fetch { get; set; }
            public bool UsarPaginacion { get; set; }
        }

        public FrmCrComitesAutorizadoresDB(IConfiguration config)
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
        /// Obtiene lista de puestos por lazy loading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>> CR_Puestos_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrComitesAutorizadoresLista<CrComitesPuestoDto>>(
                    filtrosResult.Description ?? ERRROFILTROS,
                    filtrosResult.Code ?? -1,
                    CrearListaVacia<CrComitesPuestoDto>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
                var paging = ObtenerPaginacion(filtros);
                var filtro = NormalizarTexto(filtros.filtro);
                var like = filtro.Length > 0 ? $"%{filtro}%" : null;
                var orderBy = ObtenerOrdenPuestos(filtros);

                const string sqlCount = @"
                    select count(1)
                    from CRD_COMITES_MIEMBROS_PUESTOS
                    where (@filtro = '' or ID_PUESTO like @like or DESCRIPCION like @like);";

                var sql = new StringBuilder(@"
                    select
                        rtrim(ID_PUESTO) as id_puesto,
                        rtrim(DESCRIPCION) as descripcion,
                        cast(0 as bit) as isNew
                    from CRD_COMITES_MIEMBROS_PUESTOS
                    where (@filtro = '' or ID_PUESTO like @like or DESCRIPCION like @like)");

                sql.Append(orderBy);

                if (paging.UsarPaginacion)
                {
                    sql.Append(" offset @offset rows fetch next @fetch rows only");
                }

                var args = new
                {
                    filtro,
                    like,
                    offset = paging.Offset,
                    fetch = paging.Fetch
                };

                return new ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = new CrComitesAutorizadoresLista<CrComitesPuestoDto>
                    {
                        total = conn.QuerySingle<int>(sqlCount, args),
                        lista = conn.Query<CrComitesPuestoDto>(sql.ToString(), args).ToList()
                    }
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAutorizadoresLista<CrComitesPuestoDto>>(
                    ex.Message,
                    -1,
                    CrearListaVacia<CrComitesPuestoDto>());
            }
        }

        /// <summary>
        /// Exporta lista de puestos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPuestoDto>> CR_Puestos_Lista_Export(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrComitesAutorizadoresLista<CrComitesPuestoDto>>(
                    filtrosResult.Description ?? ERRROFILTROS,
                    filtrosResult.Code ?? -1,
                    CrearListaVacia<CrComitesPuestoDto>());
            }

            var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Puestos_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda un puesto de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Puestos_Guardar(int CodEmpresa, CrComitesPuestoDto data, string usuario)
        {
            if (data == null)
            {
                return DbHelper.ErrorResponse("Datos requeridos.", -2);
            }

            var isNew = data.isNew.GetValueOrDefault();

            return isNew
                ? InsertarPuesto(CodEmpresa, data, usuario)
                : ActualizarPuesto(CodEmpresa, data, usuario);
        }

        /// <summary>
        /// Elimina un puesto de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_puesto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Puestos_Eliminar(int CodEmpresa, string id_puesto, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var puesto = NormalizarTexto(id_puesto).ToUpperInvariant();
                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(puesto))
                {
                    return DbHelper.ErrorResponse("Código de puesto requerido.", -2);
                }

                var result = conn.QueryFirstOrDefault<CrComitesSpPassDto>(
                    "spCrd_Comites_Puesto_Elimina",
                    new { Puesto = puesto, Usuario = usuarioNorm },
                    commandType: CommandType.StoredProcedure) ?? new CrComitesSpPassDto();

                if (result.Pass != 1)
                {
                    return DbHelper.ErrorResponse(result.Mensaje, -2);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Comités > Puesto: {puesto}",
                    Movimiento = EliminaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Puesto eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene lista de personas por lazy loading.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>> CR_Personas_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrComitesAutorizadoresLista<CrComitesPersonaDto>>(
                    filtrosResult.Description ?? ERRROFILTROS,
                    filtrosResult.Code ?? -1,
                    CrearListaVacia<CrComitesPersonaDto>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
                var paging = ObtenerPaginacion(filtros);
                var filtro = NormalizarTexto(filtros.filtro);
                var like = filtro.Length > 0 ? $"%{filtro}%" : null;
                var orderBy = ObtenerOrdenPersonas(filtros);

                const string sqlCount = @"
                    select count(1)
                    from CRD_COMITES_MIEMBROS M
                    inner join CRD_COMITES_MIEMBROS_PUESTOS P on M.ID_PUESTO = P.ID_PUESTO
                    where (
                            @filtro = ''
                         or M.CEDULA like @like
                         or M.NOMBRE like @like
                         or M.USUARIO like @like
                         or M.ID_PUESTO like @like
                         or P.DESCRIPCION like @like
                    );";

                var sql = new StringBuilder(@"
                    select
                        rtrim(M.CEDULA) as cedula,
                        rtrim(M.NOMBRE) as nombre,
                        rtrim(M.USUARIO) as usuario,
                        rtrim(M.ID_PUESTO) as id_puesto,
                        isnull(rtrim(P.ID_PUESTO) + ' - ' + rtrim(P.DESCRIPCION), '') as puesto,
                        case when M.ESTADO = 'A' then cast(1 as bit) else cast(0 as bit) end as activo,
                        M.FECHA_ACTIVA as fecha_activa,
                        isnull(rtrim(M.USUARIO_ACTIVA), '') as usuario_activa,
                        M.FECHA_BLOQUEO as fecha_bloqueo,
                        isnull(rtrim(M.USUARIO_BLOQUEO), '') as usuario_bloqueo,
                        cast(0 as bit) as isNew
                    from CRD_COMITES_MIEMBROS M
                    inner join CRD_COMITES_MIEMBROS_PUESTOS P on M.ID_PUESTO = P.ID_PUESTO
                    where (
                            @filtro = ''
                         or M.CEDULA like @like
                         or M.NOMBRE like @like
                         or M.USUARIO like @like
                         or M.ID_PUESTO like @like
                         or P.DESCRIPCION like @like
                    )");

                sql.Append(orderBy);

                if (paging.UsarPaginacion)
                {
                    sql.Append(" offset @offset rows fetch next @fetch rows only");
                }

                var args = new
                {
                    filtro,
                    like,
                    offset = paging.Offset,
                    fetch = paging.Fetch
                };

                return new ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = new CrComitesAutorizadoresLista<CrComitesPersonaDto>
                    {
                        total = conn.QuerySingle<int>(sqlCount, args),
                        lista = conn.Query<CrComitesPersonaDto>(sql.ToString(), args).ToList()
                    }
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrComitesAutorizadoresLista<CrComitesPersonaDto>>(
                    ex.Message,
                    -1,
                    CrearListaVacia<CrComitesPersonaDto>());
            }
        }

        /// <summary>
        /// Exporta lista de personas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrComitesAutorizadoresLista<CrComitesPersonaDto>> CR_Personas_Lista_Export(int CodEmpresa, string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrComitesAutorizadoresLista<CrComitesPersonaDto>>(
                    filtrosResult.Description ?? ERRROFILTROS,
                    filtrosResult.Code ?? -1,
                    CrearListaVacia<CrComitesPersonaDto>());
            }

            var filtros = filtrosResult.Result ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Personas_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda una persona miembro de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Personas_Guardar(int CodEmpresa, CrComitesPersonaDto data, string usuario)
        {
            if (data == null)
            {
                return DbHelper.ErrorResponse("Datos requeridos.", -2);
            }

            var isNew = data.isNew.GetValueOrDefault();

            return isNew
                ? InsertarPersona(CodEmpresa, data, usuario)
                : ActualizarPersona(CodEmpresa, data, usuario);
        }

        /// <summary>
        /// Elimina una persona miembro de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Personas_Eliminar(int CodEmpresa, string cedula, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedulaNorm = NormalizarTexto(cedula);
                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cedulaNorm))
                {
                    return DbHelper.ErrorResponse(CEDULAREQUERIDA, -2);
                }

                var result = conn.QueryFirstOrDefault<CrComitesSpPassDto>(
                    "spCrd_Comites_Autorizador_Elimina",
                    new { Cedula = cedulaNorm, Usuario = usuarioNorm },
                    commandType: CommandType.StoredProcedure) ?? new CrComitesSpPassDto();

                if (result.Pass != 1)
                {
                    return DbHelper.ErrorResponse(result.Mensaje, -2);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Comités > Miembro Autorizador: {cedulaNorm}",
                    Movimiento = EliminaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Persona eliminada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene puestos para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Puestos_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        rtrim(ID_PUESTO) as item,
                        rtrim(ID_PUESTO) + ' - ' + rtrim(DESCRIPCION) as descripcion
                    from CRD_COMITES_MIEMBROS_PUESTOS
                    order by ID_PUESTO;";

                return DbHelper.CreateOkResponse(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene comités para dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                    select
                        cast(ID_COMITE as varchar(20)) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from COMITES
                    order by ID_COMITE;";

                return DbHelper.CreateOkResponse(conn.Query<DropDownListaGenericaModel>(sql).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message, -1, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene lista de miembros asignables por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAsignacionDto>> CR_Asignacion_Miembros_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = conn.Query<CrComitesAsignacionDto>(
                    "spCrd_Comites_Miembros_Consulta",
                    new { Comite = id_comite },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAsignacionDto>>(ex.Message, -1, new List<CrComitesAsignacionDto>());
            }
        }

        /// <summary>
        /// Asigna o desasigna un miembro de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Asignacion_Miembros_Asignar(int CodEmpresa, CrComitesAsignacionRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var validacion = ValidarAsignacion(request);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                var asignado = request.asignado.GetValueOrDefault();
                var tipo = asignado ? "E" : "S";
                var usuario = NormalizarTexto(request.usuario).ToUpperInvariant();
                var cedula = NormalizarTexto(request.cedula);

                conn.Execute(
                    "spCrd_Comites_Miembros_Add",
                    new
                    {
                        Comite = request.id_comite,
                        IdMiembro = cedula,
                        Usuario = usuario,
                        Tipo = tipo
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Asignación de Miembro de Comité Id: {request.id_comite} a {cedula}",
                    Movimiento = asignado ? RegistraWeb : EliminaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Asignación actualizada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene lista de autorizadores asignables por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_comite"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComitesAsignacionDto>> CR_Asignacion_Autorizadores_Lista_Obtener(int CodEmpresa, int id_comite)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = conn.Query<CrComitesAsignacionDto>(
                    "spCrd_Comites_Miembros_Autoriza_Consulta",
                    new { Comite = id_comite },
                    commandType: CommandType.StoredProcedure).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrComitesAsignacionDto>>(ex.Message, -1, new List<CrComitesAsignacionDto>());
            }
        }

        /// <summary>
        /// Asigna o desasigna un autorizador de comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Asignacion_Autorizadores_Asignar(int CodEmpresa, CrComitesAsignacionRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var validacion = ValidarAsignacion(request);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                var asignado = request.asignado.GetValueOrDefault();
                var usuario = NormalizarTexto(request.usuario).ToUpperInvariant();
                var cedula = NormalizarTexto(request.cedula);

                if (asignado)
                {
                    const string sqlInsert = @"
                if not exists (
                    select 1
                    from CRD_COMITES_AUTORIZADORES
                    where ID_COMITE = @id_comite and CEDULA = @cedula
                )
                begin
                    insert CRD_COMITES_AUTORIZADORES
                        (ID_COMITE, CEDULA, REGISTRO_FECHA, REGISTRO_USUARIO)
                    values
                        (@id_comite, @cedula, dbo.MyGetdate(), @usuario)
                end;";

                    conn.Execute(sqlInsert, new
                    {
                        request.id_comite,
                        cedula,
                        usuario
                    });
                }
                else
                {
                    const string sqlDelete = @"
                delete CRD_COMITES_AUTORIZADORES
                where ID_COMITE = @id_comite
                  and CEDULA = @cedula;";

                    conn.Execute(sqlDelete, new
                    {
                        request.id_comite,
                        cedula
                    });
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Asignación de Autorizador de Comité Id: {request.id_comite} a {cedula}",
                    Movimiento = asignado ? RegistraWeb : EliminaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Asignación actualizada correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto InsertarPuesto(int CodEmpresa, CrComitesPuestoDto data, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var puesto = NormalizarTexto(data.id_puesto).ToUpperInvariant();
                var descripcion = NormalizarTexto(data.descripcion).ToUpperInvariant();
                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();

                var validacion = ValidarPuesto(puesto, descripcion);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                const string sqlExiste = @"
                    select isnull(count(1), 0)
                    from CRD_COMITES_MIEMBROS_PUESTOS
                    where ID_PUESTO = @puesto;";

                var existe = conn.QuerySingle<int>(sqlExiste, new { puesto });
                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("El puesto indicado ya existe.", -2);
                }

                const string sqlInsert = @"
                    insert into CRD_COMITES_MIEMBROS_PUESTOS
                        (ID_PUESTO, DESCRIPCION, REGISTRO_FECHA, REGISTRO_USUARIO)
                    values
                        (@puesto, @descripcion, dbo.MyGetdate(), @usuario);";

                conn.Execute(sqlInsert, new
                {
                    puesto,
                    descripcion,
                    usuario = usuarioNorm
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Comités Puestos Miembros Autorizadores: {puesto}",
                    Movimiento = RegistraWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Puesto guardado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto ActualizarPuesto(int CodEmpresa, CrComitesPuestoDto data, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var puesto = NormalizarTexto(data.id_puesto).ToUpperInvariant();
                var descripcion = NormalizarTexto(data.descripcion).ToUpperInvariant();
                var usuarioNorm = NormalizarTexto(usuario).ToUpperInvariant();

                var validacion = ValidarPuesto(puesto, descripcion);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                const string sqlUpdate = @"
                    update CRD_COMITES_MIEMBROS_PUESTOS
                    set DESCRIPCION = @descripcion
                    where ID_PUESTO = @puesto;";

                var rows = conn.Execute(sqlUpdate, new
                {
                    puesto,
                    descripcion
                });

                if (rows <= 0)
                {
                    return DbHelper.ErrorResponse("El puesto indicado no existe.", -2);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Comités Puestos Miembros Autorizadores: {puesto}",
                    Movimiento = ModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Puesto actualizado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto InsertarPersona(int CodEmpresa, CrComitesPersonaDto data, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var persona = NormalizarPersona(data, usuario);
                var validacion = ValidarPersona(persona);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                const string sqlExiste = @"
                    select isnull(count(1), 0)
                    from CRD_COMITES_MIEMBROS
                    where CEDULA = @cedula;";

                var existe = conn.QuerySingle<int>(sqlExiste, new { cedula = persona.Cedula });
                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("La persona indicada ya existe.", -2);
                }

                const string sqlInsert = @"
                    insert into CRD_COMITES_MIEMBROS
                        (CEDULA, NOMBRE, USUARIO, ID_PUESTO, ESTADO, FECHA_ACTIVA, USUARIO_ACTIVA)
                    values
                        (@cedula, @nombre, @usuario_miembro, @id_puesto, @estado, dbo.MyGetdate(), @usuario);";

                conn.Execute(sqlInsert, new
                {
                    cedula = persona.Cedula,
                    nombre = persona.Nombre,
                    usuario_miembro = persona.UsuarioMiembro,
                    id_puesto = persona.IdPuesto,
                    estado = persona.Estado,
                    usuario = persona.Usuario
                });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = persona.Usuario,
                    DetalleMovimiento = $"Comités Miembros Autorizadores: {persona.Cedula}",
                    Movimiento = RegistraWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Persona guardada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private ErrorDto ActualizarPersona(int CodEmpresa, CrComitesPersonaDto data, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var persona = NormalizarPersona(data, usuario);

                if (string.IsNullOrWhiteSpace(persona.Cedula))
                {
                    return DbHelper.ErrorResponse("Cédula requerida.", -2);
                }

                if (string.IsNullOrWhiteSpace(persona.Nombre))
                {
                    return DbHelper.ErrorResponse("Nombre requerido.", -2);
                }

                if (string.IsNullOrWhiteSpace(persona.UsuarioMiembro))
                {
                    return DbHelper.ErrorResponse("Usuario requerido.", -2);
                }

                if (string.IsNullOrWhiteSpace(persona.IdPuesto))
                {
                    return DbHelper.ErrorResponse("Puesto requerido.", -2);
                }

                var cedulaOriginal = NormalizarTexto(data.cedula_original);

                if (string.IsNullOrWhiteSpace(cedulaOriginal))
                {
                    cedulaOriginal = persona.Cedula;
                }

                if (!string.Equals(cedulaOriginal, persona.Cedula, StringComparison.OrdinalIgnoreCase))
                {
                    const string sqlExiste = @"
                select isnull(count(1), 0)
                from CRD_COMITES_MIEMBROS
                where CEDULA = @cedula;";

                    var existe = conn.QuerySingle<int>(sqlExiste, new
                    {
                        cedula = persona.Cedula
                    });

                    if (existe > 0)
                    {
                        return DbHelper.ErrorResponse("Ya existe un autorizador con la cédula indicada.", -2);
                    }
                }

                const string sqlEstadoActual = @"
            select ESTADO
            from CRD_COMITES_MIEMBROS
            where CEDULA = @cedula_original;";

                var estadoActual = conn.QueryFirstOrDefault<string>(sqlEstadoActual, new
                {
                    cedula_original = cedulaOriginal
                }) ?? string.Empty;

                if (string.IsNullOrWhiteSpace(estadoActual))
                {
                    return DbHelper.ErrorResponse("No se encontró la persona autorizadora indicada.", -2);
                }

                const string sql = @"
            update CRD_COMITES_MIEMBROS
            set CEDULA = @cedula,
                NOMBRE = @nombre,
                USUARIO = @usuario_miembro,
                ID_PUESTO = @id_puesto,
                ESTADO = @estado,
                USUARIO_BLOQUEO = case
                    when @estado_actual = 'A' and @estado = 'B' then @usuario
                    else USUARIO_BLOQUEO
                end,
                FECHA_BLOQUEO = case
                    when @estado_actual = 'A' and @estado = 'B' then dbo.MyGetdate()
                    else FECHA_BLOQUEO
                end,
                USUARIO_ACTIVA = case
                    when @estado_actual = 'B' and @estado = 'A' then @usuario
                    else USUARIO_ACTIVA
                end,
                FECHA_ACTIVA = case
                    when @estado_actual = 'B' and @estado = 'A' then dbo.MyGetdate()
                    else FECHA_ACTIVA
                end
            where CEDULA = @cedula_original;";

                var rows = conn.Execute(sql, new
                {
                    cedula = persona.Cedula,
                    cedula_original = cedulaOriginal,
                    nombre = persona.Nombre,
                    usuario_miembro = persona.UsuarioMiembro,
                    id_puesto = persona.IdPuesto,
                    estado = persona.Estado,
                    estado_actual = estadoActual.Trim(),
                    usuario = persona.Usuario
                });

                if (rows <= 0)
                {
                    return DbHelper.ErrorResponse("No se actualizó ningún registro.", -2);
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = persona.Usuario,
                    DetalleMovimiento = $"Autorizador de comites : {persona.Cedula}",
                    Movimiento = ModificaWeb,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse($"Autorizador de comites : {persona.Cedula}, Modificado satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto ValidarPuesto(string puesto, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(puesto))
            {
                return DbHelper.ErrorResponse("Código de puesto requerido.", -2);
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return DbHelper.ErrorResponse("Descripción requerida.", -2);
            }

            if (puesto.Length > 10)
            {
                return DbHelper.ErrorResponse("El código de puesto no puede superar 10 caracteres.", -2);
            }

            if (descripcion.Length > 50)
            {
                return DbHelper.ErrorResponse("La descripción no puede superar 50 caracteres.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto ValidarPersona(PersonaNormalizada persona)
        {
            if (string.IsNullOrWhiteSpace(persona.Cedula))
            {
                return DbHelper.ErrorResponse(CEDULAREQUERIDA, -2);
            }

            if (string.IsNullOrWhiteSpace(persona.Nombre))
            {
                return DbHelper.ErrorResponse("Nombre requerido.", -2);
            }

            if (string.IsNullOrWhiteSpace(persona.IdPuesto))
            {
                return DbHelper.ErrorResponse("Puesto requerido.", -2);
            }

            if (persona.Cedula.Length > 20)
            {
                return DbHelper.ErrorResponse("La cédula no puede superar 20 caracteres.", -2);
            }

            if (persona.Nombre.Length > 100)
            {
                return DbHelper.ErrorResponse("El nombre no puede superar 100 caracteres.", -2);
            }

            if (persona.UsuarioMiembro.Length > 35)
            {
                return DbHelper.ErrorResponse("El usuario no puede superar 35 caracteres.", -2);
            }

            if (persona.IdPuesto.Length > 10)
            {
                return DbHelper.ErrorResponse("El puesto no puede superar 10 caracteres.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto ValidarAsignacion(CrComitesAsignacionRequest request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Datos requeridos.", -2);
            }

            if (request.id_comite <= 0)
            {
                return DbHelper.ErrorResponse("Comité requerido.", -2);
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse(CEDULAREQUERIDA, -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse("Usuario requerido.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static PersonaNormalizada NormalizarPersona(CrComitesPersonaDto data, string usuario)
        {
            return new PersonaNormalizada
            {
                Cedula = NormalizarTexto(data.cedula),
                Nombre = NormalizarTexto(data.nombre).ToUpperInvariant(),
                UsuarioMiembro = NormalizarTexto(data.usuario).ToUpperInvariant(),
                IdPuesto = NormalizarTexto(data.id_puesto).ToUpperInvariant(),
                Estado = data.activo.GetValueOrDefault() ? "A" : "I",
                Usuario = NormalizarTexto(usuario).ToUpperInvariant()
            };
        }

        private static ErrorDto<FiltrosLazyLoadData> ParseFiltros(string parametros)
        {
            try
            {
                return DbHelper.CreateOkResponse(
                    JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                    ?? new FiltrosLazyLoadData());
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosLazyLoadData>(
                    ex.Message,
                    -1,
                    new FiltrosLazyLoadData());
            }
        }

        private static CrComitesAutorizadoresLista<T> CrearListaVacia<T>()
        {
            return new CrComitesAutorizadoresLista<T>
            {
                total = 0,
                lista = new List<T>()
            };
        }

        private static PaginacionInfo ObtenerPaginacion(FiltrosLazyLoadData filtros)
        {
            var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
            var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;

            return new PaginacionInfo
            {
                Offset = offset,
                Fetch = fetch,
                UsarPaginacion = fetch > 0
            };
        }

        private static string ObtenerOrdenPuestos(FiltrosLazyLoadData filtros)
        {
            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var sortOrder = filtros.sortOrder == 0 ? "desc" : "asc";

            var columns = new Dictionary<string, string>
            {
                ["id_puesto"] = "ID_PUESTO",
                ["descripcion"] = "DESCRIPCION"
            };

            var column = columns.TryGetValue(sortField, out var selected)
                ? selected
                : "ID_PUESTO";

            return $" order by {column} {sortOrder}";
        }

        private static string ObtenerOrdenPersonas(FiltrosLazyLoadData filtros)
        {
            var sortField = NormalizarTexto(filtros.sortField).ToLowerInvariant();
            var sortOrder = filtros.sortOrder == 0 ? "desc" : "asc";

            var columns = new Dictionary<string, string>
            {
                ["cedula"] = "M.CEDULA",
                ["nombre"] = "M.NOMBRE",
                ["usuario"] = "M.USUARIO",
                ["id_puesto"] = "M.ID_PUESTO",
                ["puesto"] = "P.DESCRIPCION",
                ["activo"] = "M.ESTADO"
            };

            var column = columns.TryGetValue(sortField, out var selected)
                ? selected
                : "M.ID_PUESTO";

            return column == "M.NOMBRE"
                ? $" order by {column} {sortOrder}"
                : $" order by {column} {sortOrder}, M.NOMBRE asc";
        }

        private static string NormalizarTexto(string? value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}