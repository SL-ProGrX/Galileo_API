using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionDevolucionesLiqDb
    {
        private const string Modulo = "LIQ";
        private const string TagDocumentoDevuelto = "S04";
        private readonly PortalDB _portalDb;

        public FrmAfRecepcionDevolucionesLiqDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene y valida la etiqueta configurada en el parámetro 12.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<AfRecepcionDevolucionesLiqInicializarData>
            AF_frmAF_RecepcionDevolucionesLiq_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                string tag = AF_frmAF_RecepcionDevolucionesLiq_Tag_Obtener(
                    connection,
                    null);

                return DbHelper.CreateOkResponse(
                    new AfRecepcionDevolucionesLiqInicializarData
                    {
                        tag_recepcion_devolucion = tag
                    });
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new AfRecepcionDevolucionesLiqInicializarData());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesLiqInicializarData());
            }
        }

        /// <summary>
        /// Obtiene la liquidación relacionada con una boleta devuelta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroBoleta"></param>
        /// <returns></returns>
        public ErrorDto<AfRecepcionDevolucionesLiqData?>
            AF_frmAF_RecepcionDevolucionesLiq_Boleta_Obtener(
                int codEmpresa,
                int numeroBoleta)
        {
            if (numeroBoleta <= 0)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesLiqData?>(
                    "El n&uacute;mero de boleta no es v&aacute;lido.",
                    -2,
                    null);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                AF_frmAF_RecepcionDevolucionesLiq_Tag_Obtener(
                    connection,
                    null);

                var liquidacion = AF_frmAF_RecepcionDevolucionesLiq_Boleta_Consultar(
                    connection,
                    null,
                    numeroBoleta);

                if (liquidacion is null)
                {
                    return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesLiqData?>(
                        "No se encontr&oacute; una liquidaci&oacute;n pendiente para la boleta indicada.",
                        -2,
                        null);
                }

                return DbHelper.CreateOkResponse<AfRecepcionDevolucionesLiqData?>(
                    liquidacion);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesLiqData?>(
                    ex.Message,
                    -2,
                    null);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<AfRecepcionDevolucionesLiqData?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        /// <summary>
        /// Aplica la etiqueta de recepción de devolución a las liquidaciones.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<AfRecepcionDevolucionesLiqAplicarData>
            AF_frmAF_RecepcionDevolucionesLiq_Aplicar(
                int codEmpresa,
                AfRecepcionDevolucionesLiqAplicarRequest? request)
        {
            string? validacion = AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Validar(
                request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Error(validacion);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    string tag = AF_frmAF_RecepcionDevolucionesLiq_Tag_Obtener(
                        connection,
                        transaction);

                    int aplicados = AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Procesar(
                        connection,
                        transaction,
                        request!,
                        tag);

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new AfRecepcionDevolucionesLiqAplicarData
                        {
                            registros_aplicados = aplicados
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
                return AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Error(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionDevolucionesLiqAplicarData());
            }
        }

        private static AfRecepcionDevolucionesLiqData?
            AF_frmAF_RecepcionDevolucionesLiq_Boleta_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long numeroBoleta)
        {
            const string sql = """
                select top 1
                    isnull(rtrim(L.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    isnull(rtrim(O.DESCRIPCION), '') as descripcion,
                    L.CONSEC as consec
                from LIQUIDACION L
                inner join SOCIOS S
                    on L.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on L.COD_OFICINA = O.COD_OFICINA
                where L.CEDULA in (
                    select CT.CODIGO
                    from SIF_CONTROL_TAGS CT
                    where CT.DOCUMENTO = @NumeroBoleta
                      and CT.TAG_CODIGO = @TagDocumentoDevuelto
                      and CT.COD_MODULO = @Modulo
                )
                  and L.ANALISTA_RECEPCION = 2;
                """;

            return connection.QueryFirstOrDefault<AfRecepcionDevolucionesLiqData>(
                sql,
                new
                {
                    NumeroBoleta = numeroBoleta.ToString(),
                    TagDocumentoDevuelto,
                    Modulo
                },
                transaction);
        }

        private static AfRecepcionDevolucionesLiqData?
            AF_frmAF_RecepcionDevolucionesLiq_Consecutivo_Consultar(
                SqlConnection connection,
                SqlTransaction transaction,
                long consecutivo)
        {
            const string sql = """
                select top 1
                    isnull(rtrim(L.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    isnull(rtrim(O.DESCRIPCION), '') as descripcion,
                    L.CONSEC as consec
                from LIQUIDACION L
                inner join SOCIOS S
                    on L.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on L.COD_OFICINA = O.COD_OFICINA
                where L.CONSEC = @Consecutivo
                  and L.ANALISTA_RECEPCION = 2;
                """;

            return connection.QueryFirstOrDefault<AfRecepcionDevolucionesLiqData>(
                sql,
                new
                {
                    Consecutivo = consecutivo
                },
                transaction);
        }

        private static string AF_frmAF_RecepcionDevolucionesLiq_Tag_Obtener(
            SqlConnection connection,
            SqlTransaction? transaction)
        {
            string tag = connection.QueryFirstOrDefault<string>(
                """
                select isnull(rtrim(VALOR), '')
                from SIF_PARAMETROS
                where COD_PARAMETRO = '12';
                """,
                transaction: transaction) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new InvalidOperationException(
                    "No est&aacute; definida la etiqueta de recepci&oacute;n de devoluci&oacute;n.");
            }

            int existe = connection.ExecuteScalar<int>(
                """
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
                    Tag = tag
                },
                transaction);

            if (existe == 0)
            {
                throw new InvalidOperationException(
                    "El c&oacute;digo de tag definido para la recepci&oacute;n de devoluci&oacute;n no existe.");
            }

            return tag;
        }

        private static int AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Procesar(
            SqlConnection connection,
            SqlTransaction transaction,
            AfRecepcionDevolucionesLiqAplicarRequest request,
            string tag)
        {
            int aplicados = 0;

            foreach (long consecutivo in request.consecutivos.Distinct())
            {
                var liquidacion =
                    AF_frmAF_RecepcionDevolucionesLiq_Consecutivo_Consultar(
                        connection,
                        transaction,
                        consecutivo);

                if (liquidacion is null)
                {
                    throw new InvalidOperationException(
                        $"No se encontr&oacute; la liquidaci&oacute;n {consecutivo}.");
                }

                connection.Execute(
                    "spSIFRegistraTags",
                    new
                    {
                        Codigo = liquidacion.cedula,
                        Tag = tag,
                        Usuario = request.usuario.Trim(),
                        Notas = "Recepción de Devolución la documentación de la Liquidación",
                        Documento = liquidacion.consec.ToString(),
                        Modulo,
                        Llave_01 = liquidacion.cedula,
                        Llave_02 = liquidacion.consec.ToString(),
                        Llave_03 = string.Empty
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                aplicados++;
            }

            return aplicados;
        }

        private static string? AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Validar(
            AfRecepcionDevolucionesLiqAplicarRequest? request)
        {
            if (request is null)
            {
                return "Los datos del proceso son requeridos.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "El usuario es requerido.";
            }

            if (request.consecutivos.Count == 0)
            {
                return "Debe seleccionar al menos una liquidaci&oacute;n.";
            }

            if (request.consecutivos.Any(consecutivo => consecutivo <= 0))
            {
                return "La lista contiene consecutivos no v&aacute;lidos.";
            }

            return null;
        }

        private static ErrorDto<AfRecepcionDevolucionesLiqAplicarData>
            AF_frmAF_RecepcionDevolucionesLiq_Aplicar_Error(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new AfRecepcionDevolucionesLiqAplicarData());
        }
    }
}