using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmFndRecepcionDevolucionesDb
    {
        private const string CodModulo = "FND";
        private const string NotasAplicar =
            "Recepción de Devolución la documentación del contrato";

        private readonly PortalDB _portalDb;

        public FrmFndRecepcionDevolucionesDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene parametros 11/12 y catalogo de planes (VB6 Form_Load + F4).
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <returns>Tags y planes para el formulario.</returns>
        public ErrorDto<FndRecepcionDevolucionesInicializarData>
            FND_frmFNDRecepcionDevoluciones_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags = FrmAfRecepcionDevolucionesTagsDb
                    .AF_frmAF_RecepcionDevolucionesTags_Tags_Obtener(
                        connection,
                        null);

                var planes = connection.Query<DropDownListaGenericaModel>(
                    """
                    -- Planes operadora 1 para busqueda F4
                    select
                        rtrim(cod_plan) as item,
                        rtrim(descripcion) as descripcion
                    from FND_Planes
                    where cod_operadora = 1
                    order by cod_plan;
                    """).AsList();

                return DbHelper.CreateOkResponse(
                    new FndRecepcionDevolucionesInicializarData
                    {
                        Tag_Aplicado = tags.Tag_Aplicado,
                        Tag_Devolucion = tags.Tag_Devolucion,
                        Planes = planes
                    });
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new FndRecepcionDevolucionesInicializarData());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionDevolucionesInicializarData());
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionDevolucionesInicializarData());
            }
        }

        /// <summary>
        /// Obtiene contratos activos para busqueda F4 de contrato.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="codPlan">Codigo del plan.</param>
        /// <param name="cedula">Cedula opcional.</param>
        /// <returns>Contratos disponibles.</returns>
        public ErrorDto<List<FndRecepcionDevolucionesContratoBusquedaData>>
            FND_frmFNDRecepcionDevoluciones_Contratos_Obtener(
                int codEmpresa,
                string? codPlan,
                string? cedula)
        {
            string plan = codPlan?.Trim() ?? string.Empty;
            string identificacion = cedula?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(plan))
            {
                return DbHelper.CreateErrorResponse(
                    "El plan es requerido.",
                    -2,
                    new List<FndRecepcionDevolucionesContratoBusquedaData>());
            }

            const string sql = """
                -- @CodPlan: codigo del plan
                -- @Cedula: cedula opcional
                select
                    F.cod_contrato as Cod_Contrato,
                    F.cod_operadora as Cod_Operadora,
                    isnull(rtrim(F.cedula), '') as Cedula,
                    isnull(rtrim(S.nombre), '') as Nombre
                from FND_Contratos F
                inner join Socios S on F.cedula = S.cedula
                where F.cod_plan = @CodPlan
                  and (@Cedula = '' or F.cedula = @Cedula)
                  and F.estado = 'A'
                order by F.cod_contrato;
                """;

            return DbHelper.ExecuteListQuery<FndRecepcionDevolucionesContratoBusquedaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodPlan = plan, Cedula = identificacion });
        }

        /// <summary>
        /// Obtiene el contrato pendiente de recepcion/devolucion (VB6 sbCargaInformacion).
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="codPlan">Codigo del plan.</param>
        /// <param name="codContrato">Codigo del contrato.</param>
        /// <returns>Registro del contrato o null.</returns>
        public ErrorDto<FndRecepcionDevolucionesData?>
            FND_frmFNDRecepcionDevoluciones_Contrato_Obtener(
                int codEmpresa,
                string? codPlan,
                long codContrato)
        {
            string plan = codPlan?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(plan) || codContrato <= 0)
            {
                return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesData?>(
                    "Debe indicar el plan y el contrato.",
                    -2,
                    null);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var contrato = FND_frmFNDRecepcionDevoluciones_Contrato_Consultar(
                    connection,
                    null,
                    plan,
                    codContrato);

                if (contrato is null)
                {
                    return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesData?>(
                        "No se encontro un contrato pendiente para los datos indicados.",
                        -2,
                        null);
                }

                return DbHelper.CreateOkResponse<FndRecepcionDevolucionesData?>(
                    contrato);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesData?>(
                    ex.Message,
                    -1,
                    null);
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse<FndRecepcionDevolucionesData?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        /// <summary>
        /// Aplica la etiqueta de recepcion de devolucion a los contratos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Items a aplicar y usuario.</param>
        /// <returns>Cantidad de registros aplicados.</returns>
        public ErrorDto<FndRecepcionDevolucionesAplicarData>
            FND_frmFNDRecepcionDevoluciones_Aplicar(
                int codEmpresa,
                FndRecepcionDevolucionesAplicarRequest request)
        {
            string? validacion = FND_frmFNDRecepcionDevoluciones_Aplicar_Validar(
                request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return FND_frmFNDRecepcionDevoluciones_Aplicar_Error(validacion);
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

                    int aplicados = FND_frmFNDRecepcionDevoluciones_Aplicar_Procesar(
                        connection,
                        transaction,
                        request,
                        tags.Tag_Devolucion);

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new FndRecepcionDevolucionesAplicarData
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
                return FND_frmFNDRecepcionDevoluciones_Aplicar_Error(ex.Message);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionDevolucionesAplicarData());
            }
            catch (DataException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionDevolucionesAplicarData());
            }
        }

        /// <summary>
        /// Consulta el contrato elegible (VB6 sbCargaInformacion).
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion opcional.</param>
        /// <param name="codPlan">Codigo del plan.</param>
        /// <param name="codContrato">Codigo del contrato.</param>
        /// <returns>Contrato o null.</returns>
        private static FndRecepcionDevolucionesData?
            FND_frmFNDRecepcionDevoluciones_Contrato_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string codPlan,
                long codContrato)
        {
            const string sql = """
                -- @CodPlan: codigo del plan
                -- @CodContrato: codigo del contrato
                select top 1
                    isnull(rtrim(F.CEDULA), '') as Cedula,
                    isnull(rtrim(S.nombre), '') as Nombre,
                    isnull(rtrim(O.DESCRIPCION), '') as Descripcion,
                    F.COD_OPERADORA as Cod_Operadora,
                    isnull(rtrim(F.COD_PLAN), '') as Cod_Plan,
                    F.COD_CONTRATO as Cod_Contrato
                from FND_CONTRATOS F
                inner join SOCIOS S
                    on F.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on F.COD_OFICINA = O.COD_OFICINA
                where F.cod_Plan = @CodPlan
                  and F.cod_contrato = @CodContrato
                  and F.Analista_recepcion = 2;
                """;

            return connection.QueryFirstOrDefault<FndRecepcionDevolucionesData>(
                sql,
                new
                {
                    CodPlan = codPlan,
                    CodContrato = codContrato
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
        private static int FND_frmFNDRecepcionDevoluciones_Aplicar_Procesar(
            SqlConnection connection,
            SqlTransaction transaction,
            FndRecepcionDevolucionesAplicarRequest request,
            string tagDevolucion)
        {
            int aplicados = 0;
            string usuario = request.Usuario.Trim();

            foreach (var item in request.Items)
            {
                string codPlan = item.Cod_Plan.Trim();

                if (string.IsNullOrWhiteSpace(codPlan) || item.Cod_Contrato <= 0)
                {
                    throw new InvalidOperationException(
                        "La lista contiene registros no validos.");
                }

                string documento = item.Cod_Contrato.ToString();

                FrmAfRecepcionDevolucionesTagsDb
                    .AF_frmAF_RecepcionDevolucionesTags_RegistraTag(
                        connection,
                        transaction,
                        new FrmAfRecepcionDevolucionesTagsDb.RecepcionDevolucionTagRegistro(
                            codPlan,
                            tagDevolucion,
                            usuario,
                            NotasAplicar,
                            documento,
                            CodModulo));

                aplicados++;
            }

            return aplicados;
        }

        private static string? FND_frmFNDRecepcionDevoluciones_Aplicar_Validar(
            FndRecepcionDevolucionesAplicarRequest? request)
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
                return "Debe agregar al menos un contrato.";
            }

            if (request.Items.Any(item =>
                    string.IsNullOrWhiteSpace(item.Cod_Plan) ||
                    item.Cod_Contrato <= 0))
            {
                return "La lista contiene registros no validos.";
            }

            return null;
        }

        private static ErrorDto<FndRecepcionDevolucionesAplicarData>
            FND_frmFNDRecepcionDevoluciones_Aplicar_Error(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new FndRecepcionDevolucionesAplicarData());
        }
    }
}
