using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta la resolución del expediente (comité, autorizaciones, asignaciones).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ResolucionResponse> Prea_frmPreaEstudiov2_Resolucion_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            var result = new ErrorDto<FrmPreaEstudiov2ResolucionResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2ResolucionResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@Tipo", tipo.Trim().ToUpperInvariant(), DbType.String);

                var filas = connection.Query(
                    "spCrd_Estudio_Resolucion_Detalle",
                    parameters,
                    commandType: CommandType.StoredProcedure
                )
                    .Select(row => MapearResolucionFila((IDictionary<string, object>)row))
                    .ToList();

                var encabezado = filas.FirstOrDefault();

                result.Result = new FrmPreaEstudiov2ResolucionResponse
                {
                    acta = encabezado?.acta ?? string.Empty,
                    acta_fecha = encabezado?.acta_fecha,
                    acta_sesion = encabezado?.acta_sesion ?? string.Empty,
                    detalle = filas.Select(fila => fila.detalle).ToList()
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2ResolucionResponse();
            }

            return result;
        }

        private static ResolucionFila MapearResolucionFila(
            IDictionary<string, object> row)
        {
            return new ResolucionFila
            {
                acta = GetString(row, "Acta"),
                acta_fecha = GetDateTime(row, "Acta_Fecha"),
                acta_sesion = GetString(row, "Sesion"),
                detalle = new FrmPreaEstudiov2ResolucionDetalleDto
                {
                    estado = GetString(row, "Estado"),
                    registro_fecha = GetDateTime(row, "Registro_fecha"),
                    registro_usuario = GetString(row, "Registro_Usuario"),
                    notas = GetString(row, "Notas"),
                    cedula = GetString(row, "Cedula"),
                    nombre = GetString(row, "Nombre")
                }
            };
        }

        private sealed class ResolucionFila
        {
            public string acta { get; set; } = string.Empty;
            public string acta_sesion { get; set; } = string.Empty;
            public DateTime? acta_fecha { get; set; }
            public FrmPreaEstudiov2ResolucionDetalleDto detalle { get; set; } = new();
        }
    }
}
