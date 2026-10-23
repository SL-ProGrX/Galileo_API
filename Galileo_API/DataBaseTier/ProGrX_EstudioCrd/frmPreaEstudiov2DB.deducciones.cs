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
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@COD_PREANALISIS", cod_preanalisis.Trim(), DbType.String);

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
                    });

                    totalColilla += cuotaColilla;
                    totalMensual += cuotaMensual;
                }

                result.Result = new FrmPreaEstudiov2DeduccionesResponse
                {
                    deducciones = deducciones,
                    total_colilla = totalColilla,
                    total_mensual = totalMensual,
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
        /// Agrega una deducción. VB6: btnDeduccion_Click, línea ~13042:
        ///   EXEC spCrdPrea_Deducciones_Add '&lt;expediente&gt;', 0, &lt;cod_deduccion&gt;,
        ///     '&lt;descripcion&gt;', &lt;monto&gt;, &lt;monto*NumPagos&gt;, '&lt;usuario&gt;'
        /// El SP devuelve Pass (1 = éxito) y Mensaje (motivo si falla) — no bloqueante en
        /// el sentido de excepción, pero VB6 sí detiene el flujo si Pass &lt;&gt; 1.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa donde se registra la deducción.</param>
        /// <param name="request">Expediente, tipo, descripción, monto y usuario de la deducción.</param>
        /// <returns>Listado y totales de deducciones actualizados.</returns>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2DeduccionesAgregarRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var expediente = request.cod_preanalisis.Trim();
                var numPagos = ObtenerNumPagosDeduccion(connection, expediente);
                var monto = request.monto;

                const string sql = @"EXEC spCrdPrea_Deducciones_Add
                    @Expediente, 0, @CodDeduccion, @Descripcion, @Monto, @MontoTotal, @Usuario";
                var row = connection.QueryFirstOrDefault(sql, new
                {
                    Expediente = expediente,
                    CodDeduccion = string.IsNullOrWhiteSpace(request.cod_deduccion) ? "0" : request.cod_deduccion.Trim(),
                    Descripcion = request.descripcion?.Trim() ?? string.Empty,
                    Monto = monto,
                    MontoTotal = monto * numPagos,
                    Usuario = request.usuario?.Trim() ?? string.Empty
                }) as IDictionary<string, object>;
                if (row is not null)
                {
                    var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                    var pass = GetInt(dict, "Pass");
                    if (pass != 1)
                    {
                        return DbHelper.CreateErrorResponse(
                            GetString(dict, "Mensaje"),
                            -1,
                            new FrmPreaEstudiov2DeduccionesResponse());
                    }
                }

                return Prea_frmPreaEstudiov2_Deducciones_Consultar(codEmpresa, expediente);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmPreaEstudiov2DeduccionesResponse());
            }
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
            var response = new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DeduccionesResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCRDPreaDETALLE_DEDUC_B @Id, @Expediente";
                connection.Execute(sql, new
                {
                    Id = (request.id_x ?? string.Empty).Trim(),
                    Expediente = request.cod_preanalisis.Trim()
                });

                return Prea_frmPreaEstudiov2_Deducciones_Consultar(codEmpresa, request.cod_preanalisis.Trim());
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DeduccionesResponse();
                return response;
            }
        }
    }
}
