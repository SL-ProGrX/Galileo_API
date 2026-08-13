using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Guarda la "Tabla de Salarios" (gSalarios en VB6). VB6: sbSalarios_Guardar
        /// (frmPreaEstudiov2.frm línea ~13737):
        ///   1. EXEC spCrdPreaEliminarSalarios '&lt;expediente&gt;' (elimina los registros)
        ///   2. Por cada fila: EXEC spCrdPreaGeneraSalariosSistema '&lt;expediente&gt;',
        ///      &lt;salario_s&gt;, '&lt;fecha&gt;', &lt;orden&gt;, &lt;ca&gt;, &lt;mes&gt;
        ///   3. Por cada fila: EXEC spCrdPreaGeneraSalariosConsultaLinea '&lt;expediente&gt;',
        ///      &lt;salario_rh&gt;, '&lt;fecha&gt;', &lt;orden&gt;, &lt;ca&gt;, &lt;mes&gt;
        /// En VB6 no hay un botón "Guardar" explícito para esta grilla — se dispara
        /// automáticamente tras pegar desde Excel o tras "Registro Inicial" (traer
        /// salarios anteriores del socio). En Angular se dispara al agregar/eliminar
        /// una fila de la tabla, que es el equivalente más cercano a "la grilla cambió".
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2SalarioDetalleDto>> Prea_frmPreaEstudiov2_TablaSalarios_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2TablaSalariosGuardarRequest request)
        {
            var result = new ErrorDto<List<FrmPreaEstudiov2SalarioDetalleDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            var codPreanalisis = (request.cod_preanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(codPreanalisis))
            {
                result.Code = -1;
                result.Description = "Debe indicar el expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Execute(
                    "EXEC spCrdPreaEliminarSalarios @Expediente",
                    new { Expediente = codPreanalisis });

                var expedienteEscapado = codPreanalisis.Replace("'", "''");

                var sqlSistema = new StringBuilder();
                var sqlOtros = new StringBuilder();
                var orden = 0;

                foreach (var fila in request.tabla_salarios)
                {
                    orden++;
                    var fecha = fila.fecha.HasValue ? fila.fecha.Value.ToString("yyyy-MM-dd") : string.Empty;

                    sqlSistema.Append(' ').Append(
                        "EXEC spCrdPreaGeneraSalariosSistema '" + expedienteEscapado + "', "
                        + Dec(fila.salario_s) + ", '" + fecha + "', " + orden + ", " + Dec(fila.ca) + ", " + fila.mes);

                    sqlOtros.Append(' ').Append(
                        "EXEC spCrdPreaGeneraSalariosConsultaLinea '" + expedienteEscapado + "', "
                        + Dec(fila.salario_rh) + ", '" + fecha + "', " + orden + ", " + Dec(fila.ca) + ", " + fila.mes);
                }

                if (sqlSistema.Length > 0)
                {
                    connection.Execute(sqlSistema.ToString());
                }

                if (sqlOtros.Length > 0)
                {
                    connection.Execute(sqlOtros.ToString());
                }

                result.Result = ObtenerTablaSalarios(connection, codPreanalisis);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// EXEC spCrdPreaAsignaOficina '&lt;expediente&gt;', '&lt;cod_oficina&gt;', &lt;id_promotor o Null&gt;
        /// VB6: btnOficinaCambia_Click (frmPreaEstudiov2.frm línea ~13541). Validaciones
        /// replicadas: no permitido si el expediente está Aprobado (estado 'A'), ni si es
        /// un sub-expediente (solo el expediente principal permite este cambio).
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_OficinaEjecutivo_Cambiar(
            int codEmpresa,
            FrmPreaEstudiov2OficinaEjecutivoCambiarRequest request)
        {
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };

            var codPreanalisis = (request.cod_preanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(codPreanalisis))
            {
                result.Code = -1;
                result.Description = "Debe indicar el expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var estadoActual = connection.QueryFirstOrDefault<string>(
                    "SELECT Estado FROM CRD_PREA_PREANALISIS WHERE cod_preanalisis = @Expediente",
                    new { Expediente = codPreanalisis });

                if (string.Equals(estadoActual, "A", StringComparison.OrdinalIgnoreCase))
                {
                    result.Code = -1;
                    result.Description = "No se puede cambiar la Oficina de un expediente que ya ha sido APROBADO.";
                    return result;
                }

                if (codPreanalisis.Contains('-'))
                {
                    result.Code = -1;
                    result.Description = "No se puede CAMBIAR la Oficina de un expediente secundario, por favor seleccione el expediente principal.";
                    return result;
                }

                int? idPromotor = int.TryParse(request.id_promotor, out var valorPromotor) ? valorPromotor : null;
                const string sql = "EXEC spCrdPreaAsignaOficina @Expediente, @Oficina, @IdPromotor";
                connection.Execute(sql, new
                {
                    Expediente = codPreanalisis,
                    Oficina = (request.cod_oficina ?? string.Empty).Trim(),
                    IdPromotor = idPromotor
                });

                result.Result = "Se ha actualizado la Oficina y el Ejecutivo del expediente correctamente.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}
