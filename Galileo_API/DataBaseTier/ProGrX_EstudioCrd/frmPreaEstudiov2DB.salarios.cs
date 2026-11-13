using Galileo.DataBaseTier;
using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

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
            var result = DbHelper.CreateOkResponse(new List<FrmPreaEstudiov2SalarioDetalleDto>());

            var codPreanalisis = (request?.cod_preanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(codPreanalisis))
            {
                result.Code = -2;
                result.Description = "Debe indicar el expediente.";
                return result;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                Prea_frmPreaEstudiov2_TablaSalarios_Reemplazar(connection, codPreanalisis, request!.tabla_salarios);

                result.Result = ObtenerTablaSalarios(connection, codPreanalisis);
            }
            catch (PreaSalariosValidacionException ex)
            {
                result.Code = -2;
                result.Description = ex.Message;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Reemplaza las filas de la tabla de salarios usando los mismos procedimientos
        /// de sbSalarios_Guardar de ProGrX.
        /// </summary>
        /// <param name="connection">Conexión abierta a la base de datos de la empresa.</param>
        /// <param name="codPreanalisis">Código del expediente que recibe los salarios.</param>
        /// <param name="tablaSalarios">Filas salariales que se deben persistir.</param>
        private static void Prea_frmPreaEstudiov2_TablaSalarios_Reemplazar(
            IDbConnection connection,
            string codPreanalisis,
            IEnumerable<FrmPreaEstudiov2SalarioDetalleDto> tablaSalarios)
        {
            var filas = tablaSalarios?.ToList()
                ?? throw new PreaSalariosValidacionException("Debe enviar la tabla de salarios.");
            for (var indice = 0; indice < filas.Count; indice++)
            {
                var fila = filas[indice];
                if (fila is null || !fila.fecha.HasValue || fila.fecha.Value.Year < 1753)
                    throw new PreaSalariosValidacionException($"Debe indicar una fecha válida del salario en la fila {indice + 1}.");
                if (fila.mes < 0)
                    throw new PreaSalariosValidacionException($"El mes de la fila {indice + 1} no puede ser negativo.");
            }
            if (connection.State != ConnectionState.Open) connection.Open();
            using var transaction = connection.BeginTransaction();
            // Se bloquea el expediente para impedir dos reemplazos intercalados.
            var editable = connection.QuerySingleOrDefault<int?>(
                @"SELECT CASE WHEN INDICADOR_EDITABLE = 1 THEN 1 ELSE 0 END
                  FROM CRD_PREA_PREANALISIS WITH (UPDLOCK, HOLDLOCK)
                  WHERE COD_PREANALISIS = @Expediente",
                new { Expediente = codPreanalisis }, transaction);
            if (editable is null) throw new PreaSalariosValidacionException("No se encontró el expediente.");
            if (editable != 1) throw new PreaSalariosValidacionException("El expediente no permite modificar salarios.");

            connection.Execute(
                "EXEC spCrdPreaEliminarSalarios @Expediente",
                new { Expediente = codPreanalisis }, transaction);

            const string sqlSistema = @"EXEC spCrdPreaGeneraSalariosSistema
                @Expediente, @Salario, @Fecha, @Orden, @Ca, @Mes";
            const string sqlOtros = @"EXEC spCrdPreaGeneraSalariosConsultaLinea
                @Expediente, @Salario, @Fecha, @Orden, @Ca, @Mes";


            for (var indice = 0; indice < filas.Count; indice++)
            {
                var fila = filas[indice];
                connection.Execute(
                    sqlSistema,
                    Prea_frmPreaEstudiov2_TablaSalarios_Parametros(new TablaSalariosParametros
                    {
                        CodPreanalisis = codPreanalisis,
                        Fila = fila,
                        Orden = indice + 1,
                        Salario = fila.salario_s
                    }), transaction);
            }

            for (var indice = 0; indice < filas.Count; indice++)
            {
                var fila = filas[indice];
                connection.Execute(
                    sqlOtros,
                    Prea_frmPreaEstudiov2_TablaSalarios_Parametros(new TablaSalariosParametros
                    {
                        CodPreanalisis = codPreanalisis,
                        Fila = fila,
                        Orden = indice + 1,
                        Salario = fila.salario_rh
                    }), transaction);
            }
            transaction.Commit();
        }

        /// <summary>
        /// Crea los parámetros comunes para registrar una fila de la tabla de salarios.
        /// </summary>
        private static DynamicParameters Prea_frmPreaEstudiov2_TablaSalarios_Parametros(TablaSalariosParametros valores)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Expediente", valores.CodPreanalisis, DbType.String);
            parameters.Add("@Salario", valores.Salario, DbType.Decimal);
            parameters.Add("@Fecha", valores.Fila.fecha, DbType.Date);
            parameters.Add("@Orden", valores.Orden, DbType.Int32);
            parameters.Add("@Ca", valores.Fila.ca, DbType.Decimal);
            parameters.Add("@Mes", valores.Fila.mes, DbType.Int32);
            return parameters;
        }

        public sealed class PreaSalariosValidacionException(string message) : Exception(message);

        private sealed class TablaSalariosParametros
        {
            public string CodPreanalisis { get; init; } = string.Empty;
            public FrmPreaEstudiov2SalarioDetalleDto Fila { get; init; } = new();
            public int Orden { get; init; }
            public decimal Salario { get; init; }
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
                result.Code = -2;
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
                    result.Description = "No se puede CAMBIAR la Oficina de un expediente secundario, por favor seleccione el expediente principal e intente de nuevo.";
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
            catch (PreaSalariosValidacionException ex)
            {
                result.Code = -2;
                result.Description = ex.Message;
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
