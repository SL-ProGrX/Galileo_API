using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionDevolucionesTagsDb
    {
        private const string Modulo = "AFI";
        private const string TagDocumentoDevuelto = "S04";
        private const string NotasAplicar =
            "Recepción de Devolución la documentación de la afiliación";

        private readonly PortalDB _portalDb;

        public FrmAfRecepcionDevolucionesTagsDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene y valida los parametros 11 (tag aplicado) y 12 (tag devolucion).
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <returns>Tags configurados para el formulario.</returns>
        public ErrorDto<AfRecepcionDevolucionesTagsInicializarData>
            AF_frmAF_RecepcionDevolucionesTags_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags = AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                    connection,
                    null);

                return DbHelper.CreateOkResponse(
                    new AfRecepcionDevolucionesTagsInicializarData
                    {
                        Tag_Aplicado = tags.Tag_Aplicado,
                        Tag_Devolucion = tags.Tag_Devolucion
                    });
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new AfRecepcionDevolucionesTagsInicializarData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesTagsInicializarData());
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesTagsInicializarData());
            }
        }

        /// <summary>
        /// Obtiene la afiliacion pendiente de recepcion/devolucion por cedula.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="cedula">Cedula a consultar.</param>
        /// <returns>Registro de afiliacion o null.</returns>
        public ErrorDto<AfRecepcionDevolucionesTagsData?>
            AF_frmAF_RecepcionDevolucionesTags_Cedula_Obtener(
                int codEmpresa,
                string? cedula)
        {
            string cedulaTrim = cedula?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cedulaTrim) ||
                !cedulaTrim.All(char.IsDigit))
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesTagsData?>(
                    "La cedula no es valida.",
                    -2,
                    null);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags = AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                    connection,
                    null);

                var afiliacion = AF_frmAF_RecepcionDevolucionesTags_Cedula_Consultar(
                    connection,
                    null,
                    cedulaTrim,
                    tags);

                if (afiliacion is null)
                {
                    return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesTagsData?>(
                        "No se encontro una afiliacion pendiente para la cedula indicada.",
                        -2,
                        null);
                }

                return DbHelper.CreateOkResponse<AfRecepcionDevolucionesTagsData?>(
                    afiliacion);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesTagsData?>(
                    ex.Message,
                    -2,
                    null);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesTagsData?>(
                    ex.Message,
                    -1,
                    null);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesTagsData?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        /// <summary>
        /// Aplica la etiqueta de recepcion de devolucion a las afiliaciones.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Items a aplicar y usuario.</param>
        /// <returns>Cantidad de registros aplicados.</returns>
        public ErrorDto<AfRecepcionDevolucionesTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesTags_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesTagsAplicarRequest request)
        {
            string? validacion = AF_frmAF_RecepcionDevolucionesTags_Aplicar_Validar(
                request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return AF_frmAF_RecepcionDevolucionesTags_Aplicar_Error(validacion);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    var tags = AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                        connection,
                        transaction);

                    if (string.IsNullOrWhiteSpace(tags.Tag_Devolucion))
                    {
                        throw new InvalidOperationException(
                            "No se puede realizar el proceso: no esta definida la etiqueta de devolucion.");
                    }

                    int aplicados =
                        AF_frmAF_RecepcionDevolucionesTags_Aplicar_Procesar(
                            connection,
                            transaction,
                            request,
                            tags.Tag_Devolucion);

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new AfRecepcionDevolucionesTagsAplicarData
                        {
                            Registros_Aplicados = aplicados
                        },
                        "Proceso concluido con exito.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (InvalidOperationException ex)
            {
                return AF_frmAF_RecepcionDevolucionesTags_Aplicar_Error(ex.Message);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesTagsAplicarData());
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesTagsAplicarData());
            }
        }

        /// <summary>
        /// Consulta la afiliacion elegible por cedula (VB6 sbCargaInformacion).
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion opcional.</param>
        /// <param name="cedula">Cedula a buscar.</param>
        /// <param name="tags">Tags de parametros 11 y 12.</param>
        /// <returns>Afiliacion o null.</returns>
        private static AfRecepcionDevolucionesTagsData?
            AF_frmAF_RecepcionDevolucionesTags_Cedula_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string cedula,
                TagsConfiguracion tags)
        {
            const string sql = """
                -- @Cedula: cedula a consultar
                -- @TagDocumentoDevuelto: tag S04 de documento devuelto
                -- @Modulo: modulo AFI
                -- @TagAplicado: parametro 11
                -- @TagDevolucion: parametro 12
                select top 1
                    isnull(rtrim(I.CEDULA), '') as Cedula,
                    isnull(rtrim(S.nombre), '') as Nombre,
                    isnull(rtrim(O.DESCRIPCION), '') as Descripcion,
                    I.CONSEC as Consec
                from AFI_INGRESOS I
                inner join SOCIOS S
                    on I.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on I.COD_OFICINA = O.COD_OFICINA
                where I.CEDULA in (
                    select CT.codigo
                    from SIF_CONTROL_TAGS CT
                    where CT.codigo = @Cedula
                      and CT.TAG_CODIGO = @TagDocumentoDevuelto
                      and CT.cod_modulo = @Modulo
                )
                  and I.Analista_recepcion = 2
                  and dbo.fxSIFValidaTagRev(
                        I.cedula,
                        @TagAplicado,
                        @TagDevolucion,
                        @Modulo,
                        I.consec,
                        null
                      ) <> 1;
                """;

            return connection.QueryFirstOrDefault<AfRecepcionDevolucionesTagsData>(
                sql,
                new
                {
                    Cedula = cedula,
                    TagDocumentoDevuelto,
                    Modulo,
                    TagAplicado = tags.Tag_Aplicado,
                    TagDevolucion = tags.Tag_Devolucion
                },
                transaction);
        }

        /// <summary>
        /// Obtiene y valida parametros 11 y 12 (VB6 Form_Load).
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion opcional.</param>
        /// <returns>Configuracion de tags.</returns>
        private static TagsConfiguracion
            AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                SqlConnection connection,
                SqlTransaction? transaction)
        {
            const string sql = """
                -- @ParamAplicado: codigo parametro 11
                -- @ParamDevolucion: codigo parametro 12
                select
                    isnull((
                        select nullif(rtrim(valor), '')
                        from SIF_PARAMETROS
                        where cod_parametro = @ParamAplicado
                    ), '') as Tag_Aplicado,
                    isnull((
                        select nullif(rtrim(valor), '')
                        from SIF_PARAMETROS
                        where cod_parametro = @ParamDevolucion
                    ), '') as Tag_Devolucion;
                """;

            var tags = connection.QuerySingleOrDefault<TagsConfiguracion>(
                sql,
                new
                {
                    ParamAplicado = "11",
                    ParamDevolucion = "12"
                },
                transaction)
                ?? new TagsConfiguracion();

            if (string.IsNullOrWhiteSpace(tags.Tag_Aplicado))
            {
                throw new InvalidOperationException(
                    "Falta agregar el parametro 11 en la base de datos.");
            }

            if (string.IsNullOrWhiteSpace(tags.Tag_Devolucion))
            {
                throw new InvalidOperationException(
                    "Falta agregar el parametro 12 en la base de datos.");
            }

            int existeTag = connection.ExecuteScalar<int>(
                """
                -- @Tag: codigo de tag de devolucion
                select case
                    when exists (
                        select 1
                        from SIF_TAGS
                        where TAG_CODIGO = @Tag
                    )
                    then 1
                    else 0
                end;
                """,
                new
                {
                    Tag = tags.Tag_Devolucion
                },
                transaction);

            if (existeTag == 0)
            {
                throw new InvalidOperationException(
                    "El codigo de tag definido en los parametros para la Recepcion/Devolucion no existe.");
            }

            return tags;
        }

        /// <summary>
        /// Registra tags de devolucion para cada item (VB6 sbAplicarRecepcionDevolucion).
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion activa.</param>
        /// <param name="request">Items y usuario.</param>
        /// <param name="tagDevolucion">Tag de devolucion (param 12).</param>
        /// <returns>Cantidad aplicada.</returns>
        private static int AF_frmAF_RecepcionDevolucionesTags_Aplicar_Procesar(
            SqlConnection connection,
            SqlTransaction transaction,
            AfRecepcionDevolucionesTagsAplicarRequest request,
            string tagDevolucion)
        {
            int aplicados = 0;

            foreach (var item in request.Items)
            {
                string cedula = item.Cedula.Trim();

                if (string.IsNullOrWhiteSpace(cedula) || item.Consec <= 0)
                {
                    throw new InvalidOperationException(
                        "La lista contiene registros no validos.");
                }

                connection.Execute(
                    "spSIFRegistraTags",
                    new
                    {
                        Codigo = cedula,
                        Tag = tagDevolucion,
                        Usuario = request.Usuario.Trim(),
                        Notas = NotasAplicar,
                        Documento = item.Consec.ToString(),
                        Modulo,
                        Llave_01 = cedula,
                        Llave_02 = item.Consec.ToString(),
                        Llave_03 = string.Empty
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                aplicados++;
            }

            return aplicados;
        }

        private static string? AF_frmAF_RecepcionDevolucionesTags_Aplicar_Validar(
            AfRecepcionDevolucionesTagsAplicarRequest? request)
        {
            if (request is null)
            {
                return "Los datos del proceso son requeridos.";
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                return "El usuario es requerido.";
            }

            if (request.Items.Count == 0)
            {
                return "Debe agregar al menos una cedula.";
            }

            if (request.Items.Any(item =>
                    string.IsNullOrWhiteSpace(item.Cedula) ||
                    item.Consec <= 0))
            {
                return "La lista contiene registros no validos.";
            }

            return null;
        }

        private static ErrorDto<AfRecepcionDevolucionesTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesTags_Aplicar_Error(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new AfRecepcionDevolucionesTagsAplicarData());
        }

        private sealed class TagsConfiguracion
        {
            public string Tag_Aplicado { get; set; } = string.Empty;
            public string Tag_Devolucion { get; set; } = string.Empty;
        }
    }
}
