using Dapper;
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
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

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
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2DeduccionesAgregarRequest request)
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
                connection.Open();

                // VB6: m_NumPagos (por defecto 2, ver mismo default usado en Salarios).
                const int numPagos = 2;
                var monto = request.monto;

                const string sql = @"EXEC spCrdPrea_Deducciones_Add
                    @Expediente, 0, @CodDeduccion, @Descripcion, @Monto, @MontoTotal, @Usuario";
                var row = connection.QueryFirstOrDefault(sql, new
                {
                    Expediente = request.cod_preanalisis.Trim(),
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
                        response.Code = -1;
                        response.Description = GetString(dict, "Mensaje");
                        return response;
                    }
                }

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
