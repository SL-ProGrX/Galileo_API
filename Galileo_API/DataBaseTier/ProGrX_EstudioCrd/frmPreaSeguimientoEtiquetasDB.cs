using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmPreaSeguimientoEtiquetasDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCreditos = 3;

        public FrmPreaSeguimientoEtiquetasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene información de encabezado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idSolicitud"></param>
        /// <param name="codPreanalisis"></param>
        /// <returns></returns>
        public ErrorDto<PreaSeguimientoEtiquetasInfoDto> Prea_SeguimientoEtiquetas_Info_Obtener(
            int CodEmpresa,
            int idSolicitud,
            string? codPreanalisis)
        {
            var response = new ErrorDto<PreaSeguimientoEtiquetasInfoDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new PreaSeguimientoEtiquetasInfoDto()
            };

            try
            {
                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

                if (idSolicitud > 0)
                {
                    const string sql = @"
                        select
                            'Solicitud:' as titulo,
                            cast(R.id_solicitud as varchar(30)) as operacion,
                            rtrim(R.codigo) as codigo,
                            rtrim(S.cedula) as cedula,
                            rtrim(S.nombre) as nombre,
                            '[ ' + rtrim(S.cedula) + ' ] ' + rtrim(S.nombre) as identificacion
                        from socios S
                        inner join reg_creditos R on S.cedula = R.cedula
                        where R.id_solicitud = @idSolicitud;";

                    response.Result = conn.QueryFirstOrDefault<PreaSeguimientoEtiquetasInfoDto>(
                        sql,
                        new { idSolicitud }) ?? new PreaSeguimientoEtiquetasInfoDto();

                    return response;
                }

                codPreanalisis = (codPreanalisis ?? string.Empty).Trim();

                const string sqlPrea = @"
                    select
                        'Estudio de Crédito:' as titulo,
                        rtrim(R.cod_preanalisis) as operacion,
                        rtrim(R.cod_linea) as codigo,
                        rtrim(S.cedula) as cedula,
                        rtrim(S.nombre) as nombre,
                        '[ ' + rtrim(S.cedula) + ' ] ' + rtrim(S.nombre) as identificacion
                    from socios S
                    inner join CRD_PREA_PREANALISIS R on S.cedula = R.cedula
                    where R.cod_preanalisis = @codPreanalisis;";

                response.Result = conn.QueryFirstOrDefault<PreaSeguimientoEtiquetasInfoDto>(
                    sqlPrea,
                    new { codPreanalisis }) ?? new PreaSeguimientoEtiquetasInfoDto();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<PreaSeguimientoEtiquetasInfoDto>(
                    ex.Message,
                    -1,
                    response.Result);
            }
        }

        /// <summary>
        /// Obtiene lista de etiquetas registradas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idSolicitud"></param>
        /// <param name="codPreanalisis"></param>
        /// <returns></returns>
        public ErrorDto<PreaSeguimientoEtiquetasLista> Prea_SeguimientoEtiquetas_Lista_Obtener(
            int CodEmpresa,
            int idSolicitud,
            string? codPreanalisis)
        {
            var response = new ErrorDto<PreaSeguimientoEtiquetasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new PreaSeguimientoEtiquetasLista()
            };

            try
            {
                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

                IEnumerable<PreaSeguimientoEtiquetasData> lista;

                if (idSolicitud > 0)
                {
                    const string sql = @"
                        select
                            O.Linea as linea,
                            O.Registro_Fecha as registro_fecha,
                            isnull(rtrim(O.Registro_Usuario),'') as registro_usuario,
                            isnull(rtrim(O.Tag_Codigo),'') as tag_codigo,
                            isnull(rtrim(T.descripcion),'') as etiqueta,
                            isnull(rtrim(O.Asignado_A),'') as asignado_a,
                            isnull(rtrim(O.Notas),'') as notas
                        from CRD_OPERACION_TAGS O
                        inner join CRD_TAGS T on O.Tag_Codigo = T.Tag_Codigo
                        where O.Id_Solicitud = @idSolicitud
                        order by O.Linea;";

                    lista = conn.Query<PreaSeguimientoEtiquetasData>(sql, new { idSolicitud });
                }
                else
                {
                    codPreanalisis = (codPreanalisis ?? string.Empty).Trim();

                    const string sql = @"
                        select
                            O.Linea as linea,
                            O.Registro_Fecha as registro_fecha,
                            isnull(rtrim(O.Registro_Usuario),'') as registro_usuario,
                            isnull(rtrim(O.Tag_Codigo),'') as tag_codigo,
                            isnull(rtrim(T.descripcion),'') as etiqueta,
                            isnull(rtrim(O.Asignado_A),'') as asignado_a,
                            isnull(rtrim(O.Notas),'') as notas
                        from CRD_PREA_TAGS O
                        inner join CRD_TAGS T on O.Tag_Codigo = T.Tag_Codigo
                        where O.Cod_Preanalisis = @codPreanalisis
                        order by O.Linea;";

                    lista = conn.Query<PreaSeguimientoEtiquetasData>(sql, new { codPreanalisis });
                }

                response.Result.lista = lista.ToList();
                response.Result.total = response.Result.lista.Count;

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<PreaSeguimientoEtiquetasLista>(
                    ex.Message,
                    -1,
                    response.Result);
            }
        }

        /// <summary>
        /// Obtiene etiquetas disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Prea_SeguimientoEtiquetas_Etiquetas_Dropdown_Obtener(
            int CodEmpresa,
            string usuario)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));

                const string sql = @"
                    select distinct
                        rtrim(T.Tag_Codigo) as item,
                        rtrim(T.Descripcion) as descripcion
                    from CRD_TAGS T
                    inner join CRD_TAGS_GRUPOS TG on TG.TAG_CODIGO = T.TAG_CODIGO
                    inner join CRD_GRPUSERS GU on GU.COD_GRUPO = TG.COD_GRUPO
                    where isnull(T.ACTIVO,0) = 1
                      and GU.USUARIO = @usuario
                    order by rtrim(T.Descripcion);";

                response.Result = conn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { usuario = (usuario ?? string.Empty).Trim() }).ToList();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    response.Result);
            }
        }

        /// <summary>
        /// Aplica etiqueta de seguimiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Prea_SeguimientoEtiquetas_Aplicar(
            int CodEmpresa,
            PreaSeguimientoEtiquetasAplicarDto data,
            string usuario)
        {
            try
            {
                var validacion = ValidarAplicar(data);
                if (validacion.Code != 0) return validacion;

                using var conn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(CodEmpresa));
                conn.Open();

                using var tx = conn.BeginTransaction();

                var esOperacion = data.id_solicitud > 0;
                var linea = ObtenerSiguienteLinea(conn, tx, esOperacion, data);

                if (esOperacion)
                {
                    InsertarOperacionTag(conn, tx, data, usuario, linea);
                }
                else
                {
                    InsertarPreanalisisTag(conn, tx, data, usuario, linea);
                }

                tx.Commit();

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = (usuario ?? string.Empty).Trim(),
                    Movimiento = "REGISTRA-WEB",
                    DetalleMovimiento = esOperacion
                        ? $"Etiqueta solicitud: {data.id_solicitud} tag: {data.tag_codigo}"
                        : $"Etiqueta preanálisis: {data.cod_preanalisis} tag: {data.tag_codigo}",
                    Modulo = ModuloCreditos
                });

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Etiqueta Registrada Satisfactoriamente..."
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto ValidarAplicar(PreaSeguimientoEtiquetasAplicarDto data)
        {
            if (data.id_solicitud <= 0 && string.IsNullOrWhiteSpace(data.cod_preanalisis))
                return DbHelper.ErrorResponse("Debe indicar la solicitud o el estudio de crédito.", -2);

            if (string.IsNullOrWhiteSpace(data.codigo))
                return DbHelper.ErrorResponse("Debe indicar el código de la operación.", -2);

            if (string.IsNullOrWhiteSpace(data.tag_codigo))
                return DbHelper.ErrorResponse("Debe seleccionar una etiqueta.", -2);

            return new ErrorDto { Code = 0, Description = "Ok" };
        }

        private static short ObtenerSiguienteLinea(
            SqlConnection conn,
            SqlTransaction tx,
            bool esOperacion,
            PreaSeguimientoEtiquetasAplicarDto data)
        {
            if (esOperacion)
            {
                const string sql = @"
                    select cast(isnull(max(Linea),0) + 1 as smallint)
                    from CRD_OPERACION_TAGS
                    where Id_Solicitud = @idSolicitud;";

                return conn.QuerySingle<short>(
                    sql,
                    new { idSolicitud = data.id_solicitud },
                    tx);
            }

            const string sqlPrea = @"
                select cast(isnull(max(Linea),0) + 1 as smallint)
                from CRD_PREA_TAGS
                where Cod_Preanalisis = @codPreanalisis;";

            return conn.QuerySingle<short>(
                sqlPrea,
                new { codPreanalisis = (data.cod_preanalisis ?? string.Empty).Trim() },
                tx);
        }

        private static void InsertarOperacionTag(
            SqlConnection conn,
            SqlTransaction tx,
            PreaSeguimientoEtiquetasAplicarDto data,
            string usuario,
            short linea)
        {
            const string sql = @"
                insert CRD_OPERACION_TAGS
                (
                    Linea,
                    Tag_Codigo,
                    Codigo,
                    Id_Solicitud,
                    Registro_Fecha,
                    Registro_Usuario,
                    Asignado_A,
                    Notas
                )
                values
                (
                    @linea,
                    @tagCodigo,
                    @codigo,
                    @idSolicitud,
                    dbo.MyGetdate(),
                    @usuario,
                    @asignadoA,
                    @notas
                );";

            conn.Execute(sql, new
            {
                linea,
                tagCodigo = (data.tag_codigo ?? string.Empty).Trim(),
                codigo = (data.codigo ?? string.Empty).Trim(),
                idSolicitud = data.id_solicitud,
                usuario = (usuario ?? string.Empty).Trim(),
                asignadoA = (data.asignado_a ?? string.Empty).Trim(),
                notas = (data.notas ?? string.Empty).Trim()
            }, tx);
        }

        private static void InsertarPreanalisisTag(
            SqlConnection conn,
            SqlTransaction tx,
            PreaSeguimientoEtiquetasAplicarDto data,
            string usuario,
            short linea)
        {
            const string sql = @"
                insert CRD_PREA_TAGS
                (
                    Linea,
                    Tag_Codigo,
                    Codigo,
                    Cod_Preanalisis,
                    Registro_Fecha,
                    Registro_Usuario,
                    Asignado_A,
                    Notas
                )
                values
                (
                    @linea,
                    @tagCodigo,
                    @codigo,
                    @codPreanalisis,
                    dbo.MyGetdate(),
                    @usuario,
                    @asignadoA,
                    @notas
                );";

            conn.Execute(sql, new
            {
                linea,
                tagCodigo = (data.tag_codigo ?? string.Empty).Trim(),
                codigo = (data.codigo ?? string.Empty).Trim(),
                codPreanalisis = (data.cod_preanalisis ?? string.Empty).Trim(),
                usuario = (usuario ?? string.Empty).Trim(),
                asignadoA = (data.asignado_a ?? string.Empty).Trim(),
                notas = (data.notas ?? string.Empty).Trim()
            }, tx);
        }
    }
}