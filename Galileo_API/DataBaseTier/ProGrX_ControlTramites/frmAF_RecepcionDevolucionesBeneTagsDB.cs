using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionDevolucionesBeneTagsDb
    {
        private const string Modulo = "BEN";
        private const string TagDocumentoDevuelto = "S04";
        private const string NotasAplicar =
            "Recepción de Devolución la documentación del Beneficio";

        private readonly PortalDB _portalDb;

        public FrmAfRecepcionDevolucionesBeneTagsDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene parametros 11/12 y catalogo de beneficios activos (VB6 Form_Load + F4).
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <returns>Tags y beneficios para el formulario.</returns>
        public ErrorDto<AfRecepcionDevolucionesBeneTagsInicializarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags = FrmAfRecepcionDevolucionesTagsDb
                    .AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                        connection,
                        null);

                var beneficios = connection.Query<DropDownListaGenericaModel>(
                    """
                    -- Beneficios activos para busqueda F4
                    select
                        rtrim(cod_beneficio) as item,
                        rtrim(descripcion) as descripcion
                    from AFI_BENEFICIOS
                    where estado = 'A'
                    order by cod_beneficio;
                    """).AsList();

                return DbHelper.CreateOkResponse(
                    new AfRecepcionDevolucionesBeneTagsInicializarData
                    {
                        Tag_Aplicado = tags.Tag_Aplicado,
                        Tag_Devolucion = tags.Tag_Devolucion,
                        Beneficios = beneficios
                    });
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new AfRecepcionDevolucionesBeneTagsInicializarData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesBeneTagsInicializarData());
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesBeneTagsInicializarData());
            }
        }

        /// <summary>
        /// Obtiene el beneficio pendiente de recepcion/devolucion (VB6 sbCargaInformacion).
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="codBeneficio">Codigo del beneficio.</param>
        /// <param name="codigo">Consecutivo / documento.</param>
        /// <returns>Registro del beneficio o null.</returns>
        public ErrorDto<AfRecepcionDevolucionesBeneTagsData?>
            AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Obtener(
                int codEmpresa,
                string? codBeneficio,
                string? codigo)
        {
            string beneficioTrim = codBeneficio?.Trim() ?? string.Empty;
            string codigoTrim = codigo?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(beneficioTrim) ||
                string.IsNullOrWhiteSpace(codigoTrim))
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesBeneTagsData?>(
                    "Debe indicar el beneficio y el codigo.",
                    -2,
                    null);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var beneficio = AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Consultar(
                    connection,
                    null,
                    beneficioTrim,
                    codigoTrim);

                if (beneficio is null)
                {
                    return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesBeneTagsData?>(
                        "No se encontro un beneficio pendiente para los datos indicados.",
                        -2,
                        null);
                }

                return DbHelper.CreateOkResponse<AfRecepcionDevolucionesBeneTagsData?>(
                    beneficio);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesBeneTagsData?>(
                    ex.Message,
                    -1,
                    null);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesBeneTagsData?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        /// <summary>
        /// Aplica la etiqueta de recepcion de devolucion a los beneficios.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Items a aplicar y usuario.</param>
        /// <returns>Cantidad de registros aplicados.</returns>
        public ErrorDto<AfRecepcionDevolucionesBeneTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesBeneTagsAplicarRequest request)
        {
            string? validacion = AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Validar(
                request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Error(validacion);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    var tags = FrmAfRecepcionDevolucionesTagsDb
                        .AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                            connection,
                            transaction);

                    if (string.IsNullOrWhiteSpace(tags.Tag_Devolucion))
                    {
                        throw new InvalidOperationException(
                            "No se puede realizar el proceso: no esta definida la etiqueta de devolucion.");
                    }

                    int aplicados =
                        AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Procesar(
                            connection,
                            transaction,
                            request,
                            tags.Tag_Devolucion);

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new AfRecepcionDevolucionesBeneTagsAplicarData
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
                return AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Error(ex.Message);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesBeneTagsAplicarData());
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesBeneTagsAplicarData());
            }
        }

        /// <summary>
        /// Consulta el beneficio elegible (VB6 sbCargaInformacion).
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion opcional.</param>
        /// <param name="codBeneficio">Codigo del beneficio.</param>
        /// <param name="codigo">Documento / consecutivo.</param>
        /// <returns>Beneficio o null.</returns>
        private static AfRecepcionDevolucionesBeneTagsData?
            AF_frmAF_RecepcionDevolucionesBeneTags_Beneficio_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string codBeneficio,
                string codigo)
        {
            const string sql = """
                -- @CodBeneficio: codigo del beneficio
                -- @Codigo: documento / consecutivo
                -- @TagDocumentoDevuelto: tag S04 de documento devuelto
                -- @Modulo: modulo BEN
                select top 1
                    isnull(rtrim(B.CEDULA), '') as Cedula,
                    isnull(rtrim(S.nombre), '') as Nombre,
                    isnull(rtrim(O.DESCRIPCION), '') as Descripcion,
                    B.CONSEC as Consec,
                    isnull(rtrim(B.COD_BENEFICIO), '') as Cod_Beneficio
                from AFI_BENE_OTORGA B
                inner join SOCIOS S
                    on B.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on B.COD_OFICINA = O.COD_OFICINA
                where B.CONSEC in (
                    select CT.documento
                    from SIF_CONTROL_TAGS CT
                    where CT.documento = @Codigo
                      and CT.codigo = @CodBeneficio
                      and CT.TAG_CODIGO = @TagDocumentoDevuelto
                      and CT.cod_modulo = @Modulo
                )
                  and B.Analista_recepcion = 2;
                """;

            return connection.QueryFirstOrDefault<AfRecepcionDevolucionesBeneTagsData>(
                sql,
                new
                {
                    CodBeneficio = codBeneficio,
                    Codigo = codigo,
                    TagDocumentoDevuelto,
                    Modulo
                },
                transaction);
        }

        /// <summary>
        /// Registra tags de devolucion (VB6 sbAplicarRecepcionDevolucion).
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion activa.</param>
        /// <param name="request">Items y usuario.</param>
        /// <param name="tagDevolucion">Tag de devolucion (param 12).</param>
        /// <returns>Cantidad aplicada.</returns>
        private static int AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Procesar(
            SqlConnection connection,
            SqlTransaction transaction,
            AfRecepcionDevolucionesBeneTagsAplicarRequest request,
            string tagDevolucion)
        {
            int aplicados = 0;
            string usuario = request.Usuario.Trim();

            foreach (var item in request.Items)
            {
                string codBeneficio = item.Cod_Beneficio.Trim();

                if (string.IsNullOrWhiteSpace(codBeneficio) || item.Consec <= 0)
                {
                    throw new InvalidOperationException(
                        "La lista contiene registros no validos.");
                }

                FrmAfRecepcionDevolucionesTagsDb
                    .AF_frmAF_RecepcionDevolucionesTags_RegistraTag(
                        connection,
                        transaction,
                        codBeneficio,
                        tagDevolucion,
                        usuario,
                        NotasAplicar,
                        item.Consec.ToString(),
                        Modulo);

                aplicados++;
            }

            return aplicados;
        }

        private static string? AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Validar(
            AfRecepcionDevolucionesBeneTagsAplicarRequest? request)
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
                return "Debe agregar al menos un beneficio.";
            }

            if (request.Items.Any(item =>
                    string.IsNullOrWhiteSpace(item.Cod_Beneficio) ||
                    item.Consec <= 0))
            {
                return "La lista contiene registros no validos.";
            }

            return null;
        }

        private static ErrorDto<AfRecepcionDevolucionesBeneTagsAplicarData>
            AF_frmAF_RecepcionDevolucionesBeneTags_Aplicar_Error(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new AfRecepcionDevolucionesBeneTagsAplicarData());
        }
    }
}
