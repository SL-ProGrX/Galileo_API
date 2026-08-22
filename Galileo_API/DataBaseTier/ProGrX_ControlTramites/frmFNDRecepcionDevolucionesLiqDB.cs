using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmFndRecepcionDevolucionesLiqDb
    {
        private const string Modulo = "FLQ";
        private readonly PortalDB _portalDb;

        public FrmFndRecepcionDevolucionesLiqDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene y valida la etiqueta configurada para la recepción de devoluciones.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <returns>Etiqueta configurada en el parámetro 12.</returns>
        public ErrorDto<FndRecepcionDevolucionesLiqInicializarData>
            FND_frmFNDRecepcionDevolucionesLiq_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                string tag = FND_frmFNDRecepcionDevolucionesLiq_Tag_Obtener(
                    connection,
                    null);

                return DbHelper.CreateOkResponse(
                    new FndRecepcionDevolucionesLiqInicializarData
                    {
                        tag_recepcion_devolucion = tag
                    });
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new FndRecepcionDevolucionesLiqInicializarData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionDevolucionesLiqInicializarData());
            }
        }

        /// <summary>
        /// Obtiene la liquidación de fondos pendiente relacionada con la boleta indicada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="numeroBoleta">Consecutivo de la boleta.</param>
        /// <returns>Información de la liquidación localizada.</returns>
        public ErrorDto<FndRecepcionDevolucionesLiqData?>
            FND_frmFNDRecepcionDevolucionesLiq_Boleta_Obtener(
                int codEmpresa,
                long numeroBoleta)
        {
            if (numeroBoleta <= 0)
            {
                return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesLiqData?>(
                    "El número de boleta no es válido.",
                    -2,
                    null);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                FND_frmFNDRecepcionDevolucionesLiq_Tag_Obtener(connection, null);
                var liquidacion =
                    FND_frmFNDRecepcionDevolucionesLiq_Boleta_Consultar(
                        connection,
                        null,
                        numeroBoleta);

                if (liquidacion is null)
                {
                    return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesLiqData?>(
                        "No se encontró una liquidación pendiente para la boleta indicada.",
                        -2,
                        null);
                }

                return DbHelper.CreateOkResponse<FndRecepcionDevolucionesLiqData?>(
                    liquidacion);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesLiqData?>(
                    ex.Message,
                    -2,
                    null);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesLiqData?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        /// <summary>
        /// Registra la etiqueta de recepción de devolución para las liquidaciones.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Liquidaciones y usuario que ejecuta el proceso.</param>
        /// <returns>Cantidad de liquidaciones procesadas.</returns>
        public ErrorDto<FndRecepcionDevolucionesLiqAplicarData>
            FND_frmFNDRecepcionDevolucionesLiq_Aplicar(
                int codEmpresa,
                FndRecepcionDevolucionesLiqAplicarRequest request)
        {
            string? validacion =
                FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Validar(request);
            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Error(
                    validacion);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    string tag = FND_frmFNDRecepcionDevolucionesLiq_Tag_Obtener(
                        connection,
                        transaction);
                    int aplicados =
                        FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Procesar(
                            connection,
                            transaction,
                            request,
                            tag);

                    transaction.Commit();
                    return DbHelper.CreateOkResponse(
                        new FndRecepcionDevolucionesLiqAplicarData
                        {
                            registros_aplicados = aplicados
                        },
                        "Proceso concluido con éxito.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (InvalidOperationException ex)
            {
                return FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Error(
                    ex.Message);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionDevolucionesLiqAplicarData());
            }
        }

        /// <summary>
        /// Consulta una liquidación de fondos pendiente por su consecutivo.
        /// </summary>
        /// <param name="connection">Conexión activa a la base de datos.</param>
        /// <param name="transaction">Transacción activa, cuando corresponde.</param>
        /// <param name="numeroBoleta">Consecutivo de la boleta.</param>
        /// <returns>Liquidación localizada o <see langword="null"/>.</returns>
        private static FndRecepcionDevolucionesLiqData?
            FND_frmFNDRecepcionDevolucionesLiq_Boleta_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long numeroBoleta)
        {
            const string sql = """
                select top 1
                    isnull(rtrim(F.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    isnull(rtrim(O.DESCRIPCION), '') as descripcion,
                    L.CONSEC as consec
                from FND_LIQUIDACION L
                inner join FND_CONTRATOS F
                    on L.COD_PLAN = F.COD_PLAN
                   and L.COD_CONTRATO = F.COD_CONTRATO
                   and L.COD_OPERADORA = F.COD_OPERADORA
                inner join SOCIOS S
                    on F.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on L.COD_OFICINA = O.COD_OFICINA
                where L.CONSEC = @NumeroBoleta
                  and L.ANALISTA_RECEPCION = 2;
                """;

            return connection.QueryFirstOrDefault<FndRecepcionDevolucionesLiqData>(
                sql,
                new { NumeroBoleta = numeroBoleta },
                transaction);
        }

        /// <summary>
        /// Obtiene el tag del parámetro 12 y valida que exista en el catálogo.
        /// </summary>
        /// <param name="connection">Conexión activa a la base de datos.</param>
        /// <param name="transaction">Transacción activa, cuando corresponde.</param>
        /// <returns>Código del tag configurado.</returns>
        private static string FND_frmFNDRecepcionDevolucionesLiq_Tag_Obtener(
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
                    "No está definida la etiqueta de recepción de devolución.");
            }

            int existe = connection.ExecuteScalar<int>(
                """
                select case
                    when exists (
                        select 1
                        from SIF_TAGS
                        where TAG_CODIGO = @Tag
                    ) then 1
                    else 0
                end;
                """,
                new { Tag = tag },
                transaction);

            if (existe == 0)
            {
                throw new InvalidOperationException(
                    "El código de tag definido para la recepción de devolución no existe.");
            }

            return tag;
        }

        /// <summary>
        /// Registra el tag para cada liquidación validada dentro de la transacción.
        /// </summary>
        /// <param name="connection">Conexión activa a la base de datos.</param>
        /// <param name="transaction">Transacción que agrupa el proceso.</param>
        /// <param name="request">Consecutivos y usuario que ejecuta el proceso.</param>
        /// <param name="tag">Código del tag que se registrará.</param>
        /// <returns>Cantidad de liquidaciones procesadas.</returns>
        private static int FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Procesar(
            SqlConnection connection,
            SqlTransaction transaction,
            FndRecepcionDevolucionesLiqAplicarRequest request,
            string tag)
        {
            int aplicados = 0;

            foreach (long consecutivo in request.consecutivos.Distinct())
            {
                var liquidacion =
                    FND_frmFNDRecepcionDevolucionesLiq_Boleta_Consultar(
                        connection,
                        transaction,
                        consecutivo);
                if (liquidacion is null)
                {
                    throw new InvalidOperationException(
                        $"No se encontró la liquidación {consecutivo}.");
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
                        Llave_01 = liquidacion.consec.ToString(),
                        Llave_02 = string.Empty,
                        Llave_03 = string.Empty
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                aplicados++;
            }

            return aplicados;
        }

        /// <summary>
        /// Valida los datos requeridos para aplicar el tag.
        /// </summary>
        /// <param name="request">Datos enviados para ejecutar el proceso.</param>
        /// <returns>Mensaje de validación o <see langword="null"/>.</returns>
        private static string?
            FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Validar(
                FndRecepcionDevolucionesLiqAplicarRequest? request)
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
                return "Debe seleccionar al menos una liquidación.";
            }

            if (request.consecutivos.Any(consecutivo => consecutivo <= 0))
            {
                return "La lista contiene consecutivos no válidos.";
            }

            return null;
        }

        /// <summary>
        /// Construye la respuesta funcional de validación del proceso.
        /// </summary>
        /// <param name="mensaje">Detalle de la validación encontrada.</param>
        /// <returns>Respuesta controlada con código de validación.</returns>
        private static ErrorDto<FndRecepcionDevolucionesLiqAplicarData>
            FND_frmFNDRecepcionDevolucionesLiq_Aplicar_Error(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new FndRecepcionDevolucionesLiqAplicarData());
        }
    }
}
