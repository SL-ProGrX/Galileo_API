using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta las deducciones del expediente. VB6: sbDeducciones_Load, línea ~17202.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DeduccionesResponse()
            };

            try
            {
                var expediente = (cod_preanalisis ?? string.Empty).Trim();
                if (expediente.Length == 0)
                    throw new InvalidOperationException("Debe indicar el expediente.");

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var estado = connection.QuerySingleOrDefault<string>(
                    @"SELECT ESTADO FROM CRD_PREA_PREANALISIS
                      WHERE COD_PREANALISIS = @Expediente",
                    new { Expediente = expediente });
                if (string.IsNullOrWhiteSpace(estado))
                    throw new InvalidOperationException("No se encontró el expediente.");

                var permiteModificar = estado.Trim().Equals("R", StringComparison.OrdinalIgnoreCase)
                    || estado.Trim().Equals("P", StringComparison.OrdinalIgnoreCase);

                var parameters = new DynamicParameters();
                parameters.Add("@COD_PREANALISIS", expediente, DbType.String);

                var rawRows = connection.Query(
                    "spCrdPreaConsultaDeducciones",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var deducciones = new List<FrmPreaEstudiov2DeduccionesDetalleDto>();
                decimal totalColilla = 0m;
                decimal totalMensual = 0m;

                foreach (var r in rawRows)
                {
                    var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);
                    var cuotaColilla = GetDecimal(dict, "CUOTA_COLILLA");
                    var cuotaMensual = GetDecimal(dict, "CUOTA_MENSUAL");

                    deducciones.Add(new FrmPreaEstudiov2DeduccionesDetalleDto
                    {
                        id_x = GetString(dict, "IdX"),
                        tipo = GetString(dict, "Tipo"),
                        descripcion = GetString(dict, "Descripcion"),
                        cuota_colilla = cuotaColilla,
                        cuota_mensual = cuotaMensual,
                        permite_editar = permiteModificar && GetInt(dict, "AUTOMATICA") == 0,
                    });

                    totalColilla += cuotaColilla;
                    totalMensual += cuotaMensual;
                }

                result.Result = new FrmPreaEstudiov2DeduccionesResponse
                {
                    deducciones = deducciones,
                    total_colilla = totalColilla,
                    total_mensual = totalMensual,
                    permite_editar = permiteModificar,
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2DeduccionesResponse();
            }

            return result;
        }

        /// <summary>
        /// Agrega una deducción o actualiza sus cuotas mediante consultas secuenciales.
        /// Valida el expediente y la configuración antes de ejecutar el procedimiento.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
            Prea_frmPreaEstudiov2_Deducciones_Agregar(
                int codEmpresa,
                FrmPreaEstudiov2DeduccionesAgregarRequest request)
        {
            var operacionConfirmada = false;

            try
            {
                if (request is null)
                    throw new InvalidOperationException("Debe indicar la deducción.");

                var expediente = (request.cod_preanalisis ?? string.Empty).Trim();
                var usuario = (request.usuario ?? string.Empty).Trim();
                var descripcion = (request.descripcion ?? string.Empty).Trim();

                if (expediente.Length == 0 || usuario.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Debe indicar el expediente y el usuario.");
                }

                if (request.id_x < 0 || request.monto <= 0)
                {
                    throw new InvalidOperationException(
                        "El identificador debe ser válido y la cuota colilla mayor a cero.");
                }

                if (request.id_x == 0 &&
                    (descripcion.Length <= 5 || descripcion.Length > 100))
                {
                    throw new InvalidOperationException(
                        "La descripción debe tener entre 6 y 100 caracteres.");
                }

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                Prea_frmPreaEstudiov2_Deducciones_ValidarEstado(
                    connection, expediente);

                var idDeduccion = Prea_frmPreaEstudiov2_Deducciones_ObtenerTipo(
                    connection, expediente, request);

                var numPagos = ObtenerNumPagosDeduccion(connection, expediente);

                const string sql = @"EXEC spCrdPrea_Deducciones_Add
                    @Expediente, @Id, @CodDeduccion, @Descripcion,
                    @Monto, @MontoTotal, @Usuario";

                var raw = connection.QueryFirstOrDefault(
                    sql,
                    new
                    {
                        Expediente = expediente,
                        Id = request.id_x,
                        CodDeduccion = idDeduccion,
                        Descripcion = descripcion,
                        Monto = request.monto,
                        MontoTotal = decimal.Round(
                            request.monto * numPagos,
                            2,
                            MidpointRounding.AwayFromZero),
                        Usuario = usuario
                    }) as IDictionary<string, object>;

                if (raw is null)
                {
                    throw new InvalidOperationException(
                        "El procedimiento no devolvió confirmación. " +
                        "Actualice la lista antes de volver a guardar.");
                }

                var resultado = new Dictionary<string, object>(
                    raw, StringComparer.OrdinalIgnoreCase);

                if (GetInt(resultado, "Pass") != 1)
                {
                    throw new InvalidOperationException(
                        GetString(resultado, "Mensaje") +
                        " Actualice la lista antes de volver a guardar: " +
                        "el procedimiento puede haber aplicado cambios.");
                }

                operacionConfirmada = true;

                var consulta = Prea_frmPreaEstudiov2_Deducciones_Consultar(
                    codEmpresa, expediente);

                if (consulta.Code != 0)
                {
                    consulta.Description =
                        "La deducción se guardó, pero no se pudo actualizar la lista. " +
                        "No repita el registro. Detalle: " + consulta.Description;
                }

                return consulta;
            }
            catch (Exception ex)
            {
                var mensaje = operacionConfirmada
                    ? "La deducción se guardó. Error al actualizar la información: "
                        + ex.Message
                    : ex.Message;

                return DbHelper.CreateErrorResponse(
                    mensaje,
                    -1,
                    new FrmPreaEstudiov2DeduccionesResponse());
            }
        }

        /// <summary>
        /// Verifica que exista el expediente y admita cambios de deducciones.
        /// </summary>
        private static void Prea_frmPreaEstudiov2_Deducciones_ValidarEstado(
            IDbConnection connection,
            string expediente)
        {
            const string sql = @"SELECT ESTADO
                FROM CRD_PREA_PREANALISIS
                WHERE COD_PREANALISIS = @Expediente";

            var estado = connection.QuerySingleOrDefault<string>(
                sql, new { Expediente = expediente });

            if (string.IsNullOrWhiteSpace(estado))
            {
                throw new InvalidOperationException(
                    "No se encontró el expediente o no tiene un estado válido.");
            }

            var codigo = estado.Trim().ToUpperInvariant();

            if (codigo != "R" && codigo != "P")
            {
                throw new InvalidOperationException(
                    "El estado actual del estudio no permite modificar deducciones.");
            }
        }

        /// <summary>
        /// Obtiene el tipo real de una fila existente o valida el seleccionado
        /// para una nueva deducción. Rechaza deducciones automáticas.
        /// </summary>
        private static int Prea_frmPreaEstudiov2_Deducciones_ObtenerTipo(
            IDbConnection connection,
            string expediente,
            FrmPreaEstudiov2DeduccionesAgregarRequest request)
        {
            int idDeduccion;

            if (request.id_x > 0)
            {
                const string sqlDetalle = @"SELECT ID_DEDUCCION
                FROM CRD_PREA_DETALLE_DEDUC
                WHERE COD_PREANALISIS = @Expediente
                  AND IDX = @Id";

                var idExistente = connection.QuerySingleOrDefault<int?>(
                    sqlDetalle,
                    new
                    {
                        Expediente = expediente,
                        Id = request.id_x
                    });

                idDeduccion = idExistente
                    ?? throw new InvalidOperationException(
                        "La deducción no existe o no pertenece al expediente.");
            }
            else if (!int.TryParse(request.cod_deduccion, out idDeduccion)
                || idDeduccion <= 0)
            {
                throw new InvalidOperationException(
                    "Debe seleccionar un tipo de deducción válido.");
            }

            const string sqlConfiguracion = @"SELECT AUTOMATICA
                FROM CRD_PREA_V2_DEDUCCIONES_CONFIG
                WHERE ID_DEDUCCION = @IdDeduccion";

            var automatica = connection.QuerySingleOrDefault<int?>(
                sqlConfiguracion,
                new { IdDeduccion = idDeduccion });

            if (automatica is null)
            {
                throw new InvalidOperationException(
                    "La deducción no tiene una configuración válida.");
            }

            if (automatica.Value == 1)
            {
                throw new InvalidOperationException(
                    "La deducción es automática y no permite modificación manual.");
            }

            return idDeduccion;
        }

        /// <summary>
        /// Reproduce m_NumPagos de VB6 para el expediente indicado. Obtiene la cédula y
        /// FECHA_CORTE_COLIILA de CRD_PREA_PREANALISIS y ejecuta spCrd_Prea_NumPagos.
        /// Conserva 2 como valor predeterminado cuando no existe información útil.
        /// </summary>
        /// <param name="connection">Conexión abierta a la base de datos de la empresa.</param>
        /// <param name="expediente">Código del preanálisis que recibirá la deducción.</param>
        /// <returns>Cantidad de pagos que debe multiplicar el monto de la colilla.</returns>
        private static int ObtenerNumPagosDeduccion(IDbConnection connection, string expediente)
        {
            const int numPagosPredeterminado = 2;
            const string sqlExpediente = @"SELECT TOP 1 CEDULA, FECHA_CORTE_COLIILA
                FROM CRD_PREA_PREANALISIS
                WHERE COD_PREANALISIS = @Expediente";

            var expedienteRow = connection.QueryFirstOrDefault(
                sqlExpediente,
                new { Expediente = expediente }) as IDictionary<string, object>;
            if (expedienteRow is null)
            {
                return numPagosPredeterminado;
            }

            var expedienteDatos = new Dictionary<string, object>(expedienteRow, StringComparer.OrdinalIgnoreCase);
            var cedula = GetString(expedienteDatos, "CEDULA");
            var fechaCorte = GetDateTime(expedienteDatos, "FECHA_CORTE_COLIILA");
            if (string.IsNullOrWhiteSpace(cedula) || fechaCorte is null)
            {
                return numPagosPredeterminado;
            }

            var pagosRow = connection.QueryFirstOrDefault(
                "EXEC spCrd_Prea_NumPagos @Cedula, @FechaCorte",
                new { Cedula = cedula, FechaCorte = fechaCorte.Value }) as IDictionary<string, object>;
            if (pagosRow is null)
            {
                return numPagosPredeterminado;
            }

            var pagosDatos = new Dictionary<string, object>(pagosRow, StringComparer.OrdinalIgnoreCase);
            var numPagos = GetInt(pagosDatos, "Num_Pagos");
            return numPagos > 0 ? numPagos : numPagosPredeterminado;
        }

        /// <summary>
        /// Borra una deducción. VB6: sbDeducciones_Borrar, línea ~15561:
        ///   clsEntidad.tablaName = "spCRDPreaDETALLE_DEDUC"
        ///   clsEntidad.fxRemover(vID, expediente) -&gt; EXEC spCRDPreaDETALLE_DEDUC_B &lt;IdX&gt;, &lt;expediente&gt;
        /// (vID es el valor de la columna IdX de la grilla, no Id_Deduccion).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Borrar(
            int codEmpresa,
            FrmPreaEstudiov2DeduccionesBorrarRequest request)
        {
            var operacionConfirmada = false;
            try
            {
                if (request is null)
                    throw new InvalidOperationException("Debe indicar la deducción.");

                var expediente = (request.cod_preanalisis ?? string.Empty).Trim();
                if (expediente.Length == 0
                    || !int.TryParse(request.id_x, out var id) || id <= 0)
                    throw new InvalidOperationException(
                        "Debe indicar un expediente y un identificador de deducción válidos.");

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                Prea_frmPreaEstudiov2_Deducciones_ValidarEstado(connection, expediente);

                var idDeduccion = connection.QuerySingleOrDefault<int?>(
                    @"SELECT ID_DEDUCCION FROM CRD_PREA_DETALLE_DEDUC
                      WHERE COD_PREANALISIS = @Expediente AND IDX = @Id",
                    new { Expediente = expediente, Id = id });
                if (idDeduccion is null)
                    throw new InvalidOperationException(
                        "La deducción no existe o no pertenece al expediente.");

                connection.Execute(
                    "EXEC spCRDPreaDETALLE_DEDUC_B @Id, @Expediente",
                    new { Id = id, Expediente = expediente });
                operacionConfirmada = true;

                var consulta = Prea_frmPreaEstudiov2_Deducciones_Consultar(
                    codEmpresa, expediente);
                if (consulta.Code != 0)
                    consulta.Description =
                        "La deducción se eliminó, pero no se pudo actualizar la lista. " +
                        "No repita la operación. Detalle: " + consulta.Description;
                return consulta;
            }
            catch (Exception ex)
            {
                var mensaje = operacionConfirmada
                    ? "La deducción se eliminó. Error al actualizar la lista: " + ex.Message
                    : ex.Message;
                return DbHelper.CreateErrorResponse(
                    mensaje, -1, new FrmPreaEstudiov2DeduccionesResponse());
            }
        }
    }
}
