using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Galileo.DataBaseTier;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Las tres grillas de la pestana Salarios en un solo batch. Replican
        /// sbSalarios_Load / sbExtras_Load / sbIncapacidades_Load de VB6; el SQL de cada
        /// una es identico al que ya usaban ObtenerTablaSalarios / ObtenerExtras /
        /// ObtenerIncapacidades, solo que ahora viajan juntas.
        /// </summary>
        private const string SqlDetalleSalarios = @"
            EXEC spCrdPreaTraeSalariosExpediente @CodPreanalisis;

            EXEC spCRDPreaDETALLE_EXTRAS_TxExpediente @CodPreanalisis;

            SELECT COD_PREANALISIS, DIAS, DESDE, HASTA, ORDEN
            FROM CRD_PREA_V2_INCAPACIDADES
            WHERE COD_PREANALISIS = @CodPreanalisis
            ORDER BY ORDEN;";

        /// <summary>
        /// Arma el bloque de Salarios a partir del recordset plano de
        /// spCRDPreaPREANALISIS_T mas el detalle de las tres grillas.
        /// </summary>
        private static FrmPreaEstudiov2SalariosDto ConstruirSalarios(
            IDbConnection connection,
            IDictionary<string, object> row,
            string codPreanalisis)
        {
            var extrasFijas = GetDecimal(row, "EXTRAS_FIJAS");
            var porcComponenteAdicional = GetDecimal(row, "PORCENTAJE_COMPONENTE_AD");
            var porcentajeIngresoPrivado = row.TryGetValue("PORCENTAJE_LIBRE", out var porcentajeRaw)
                && porcentajeRaw is not null
                && porcentajeRaw is not DBNull
                ? Convert.ToDecimal(porcentajeRaw)
                : 100m;
            var detalle = ObtenerDetalleSalarios(connection, codPreanalisis);
            var tablaSalarios = InicializarTablaSalariosDesdePersona(
                connection,
                codPreanalisis,
                GetString(row, "CEDULA"),
                detalle.TablaSalarios);

            return new FrmPreaEstudiov2SalariosDto
            {
                tipo_salario = GetString(row, "tipo_salario"),
                corte_colilla = GetDateTime(row, "FECHA_CORTE_COLIILA"), // nombre real en VB6 (con typo)
                salario_devengado = GetDecimal(row, "SALARIO_DEVENGADO_COLILLA"),
                salario_mensual = GetDecimal(row, "DEVENGADO_MES"),
                salario_constancia = GetDecimal(row, "SALARIO_CONSTANCIA"),
                salario_orden_patronal = GetDecimal(row, "SALARIO_ORDEN_PATRONAL"),
                ingreso_privado = GetDecimal(row, "MONTO_ACT_PRIVADAS"),
                ingreso_privado_porc = porcentajeIngresoPrivado,
                componente_adicional_id = GetInt(row, "ID_COMPONENTE_AD"),
                componente_adicional_porc = porcComponenteAdicional,
                componente_adicional_base = extrasFijas,
                // Replica el calculo cliente de VB6: txtCompAdicional = base * porc / 100
                componentes_adicionales = Math.Round(extrasFijas * porcComponenteAdicional / 100m, 2),
                // VB6: sbExtras_Calcula totaliza la columna Monto de la grilla.
                total_extras = detalle.Extras.Sum(extra => extra.monto),
                tabla_salarios = tablaSalarios,
                extras = detalle.Extras,
                incapacidades = detalle.Incapacidades,
            };
        }

        /// <summary>
        /// Replica sbSalarios_Registro_Inicial de ProGrX: si el expediente todavía no
        /// posee una tabla salarial completa, consulta el historial de la persona por
        /// cédula, arma la secuencia mensual y la registra en el expediente.
        /// </summary>
        /// <param name="connection">Conexión abierta a la base de datos de la empresa.</param>
        /// <param name="codPreanalisis">Código del expediente que se está cargando.</param>
        /// <param name="cedula">Identificación usada para consultar el historial salarial.</param>
        /// <param name="tablaActual">Filas que ya están asociadas al expediente.</param>
        /// <returns>Tabla existente o la tabla inicializada desde el historial personal.</returns>
        private static List<FrmPreaEstudiov2SalarioDetalleDto> InicializarTablaSalariosDesdePersona(
            IDbConnection connection,
            string codPreanalisis,
            string cedula,
            List<FrmPreaEstudiov2SalarioDetalleDto> tablaActual)
        {
            // VB6: If gSalarios.MaxRows > 1 Then Exit Sub.
            if (tablaActual.Count > 1 || string.IsNullOrWhiteSpace(cedula))
            {
                return tablaActual;
            }

            var filasPersona = connection.Query(
                "EXEC spCrdPreaSalariosListaPorCedula @Cedula",
                new { Cedula = cedula.Trim() });
            var tablaInicial = new List<FrmPreaEstudiov2SalarioDetalleDto>();

            foreach (var rawRow in filasPersona)
            {
                var row = new Dictionary<string, object>(
                    (IDictionary<string, object>)rawRow,
                    StringComparer.OrdinalIgnoreCase);
                var orden = tablaInicial.Count + 1;

                tablaInicial.Add(new FrmPreaEstudiov2SalarioDetalleDto
                {
                    orden = orden,
                    fecha = GetDateTime(row, "Fecha"),
                    salario_s = GetDecimal(row, "Salario"),
                    mes = orden,
                    salario_rh = 0,
                    ca = 0
                });
            }

            if (tablaInicial.Count == 0)
            {
                return tablaActual;
            }

            GuardarTablaSalarios(connection, codPreanalisis, tablaInicial);
            return ObtenerTablaSalarios(connection, codPreanalisis);
        }

        private sealed record DetalleSalarios(
            List<FrmPreaEstudiov2SalarioDetalleDto> TablaSalarios,
            List<FrmPreaEstudiov2ExtraDto> Extras,
            List<FrmPreaEstudiov2IncapacidadDto> Incapacidades);

        /// <summary>
        /// Un viaje a la base en lugar de tres. Si el batch falla se cae a las lecturas
        /// individuales, que degradan por separado igual que antes (una grilla vacia no
        /// bloquea a las otras).
        /// </summary>
        private static DetalleSalarios ObtenerDetalleSalarios(IDbConnection connection, string codPreanalisis)
        {
            try
            {
                using var multi = connection.QueryMultiple(
                    SqlDetalleSalarios,
                    new { CodPreanalisis = codPreanalisis });

                return new DetalleSalarios(
                    MapearTablaSalarios(multi.Read()),
                    MapearExtras(multi.Read()),
                    MapearIncapacidades(multi.Read()));
            }
            catch (Exception)
            {
                return new DetalleSalarios(
                    ObtenerTablaSalarios(connection, codPreanalisis),
                    ObtenerExtras(connection, codPreanalisis),
                    ObtenerIncapacidades(connection, codPreanalisis));
            }
        }

        /// <summary>
        /// Detalle de la pestana Salarios sin recargar el expediente completo. Sustituye a
        /// la llamada que Angular hacia a Prea_frmPreaEstudiov2_Cargar al cambiar el Tipo
        /// de Salario, que traia catalogos, credito, resumen y encabezado para descartarlos.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SalariosDto> Prea_frmPreaEstudiov2_Salarios_Consultar(
            int codEmpresa, string codPreanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2SalariosDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2SalariosDto()
            };

            var expediente = codPreanalisis?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(expediente))
            {
                result.Code = -1;
                result.Description = "Debe indicar el codigo de expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = "EXEC spCRDPreaPREANALISIS_T @CodPreanalisis";
                var rawRow = connection.QueryFirstOrDefault(sql, new { CodPreanalisis = expediente })
                    as IDictionary<string, object>;

                if (rawRow is null)
                {
                    result.Code = -1;
                    result.Description = $"No se encontro el expediente {expediente}.";
                    return result;
                }

                IDictionary<string, object> row = new Dictionary<string, object>(rawRow, StringComparer.OrdinalIgnoreCase);
                result.Result = ConstruirSalarios(connection, row, expediente);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2SalariosDto();
            }

            return result;
        }

        /// <summary>
        /// Aplica el tipo salarial seleccionado.
        /// Inicializa el historial cuando corresponde y obtiene el salario del tipo.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SalariosDto> Prea_frmPreaEstudiov2_TipoSalario_Aplicar(
            int codEmpresa, string codPreanalisis, string tipoSalario)
        {
            var expediente = (codPreanalisis ?? string.Empty).Trim();
            var tipo = (tipoSalario ?? string.Empty).Trim();
            if (expediente.Length == 0 || tipo.Length == 0)
            {
                return DbHelper.CreateErrorResponse<FrmPreaEstudiov2SalariosDto>(
                    "Debe indicar el expediente y el tipo de salario.");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                var operacion = connection.QuerySingleOrDefault<string>(
                    @"SELECT operacion FROM Crd_Prea_Tipo_Salario
                    WHERE tipo_salario = @Tipo AND activo = 1",
                    new { Tipo = tipo });
                var codigoOperacion = (operacion ?? string.Empty).Trim().ToLowerInvariant();
                if (codigoOperacion.Length == 0 || !"abcdefg".Contains(codigoOperacion[0]))
                {
                    throw new InvalidOperationException("El tipo de salario no tiene una operación válida.");
                }

                var raw = connection.QueryFirstOrDefault(
                    "EXEC spCRDPreaPREANALISIS_T @Expediente",
                    new { Expediente = expediente }) as IDictionary<string, object>;
                if (raw is null)
                {
                    throw new InvalidOperationException("No se encontr&oacute; el expediente.");
                }

                var row = new Dictionary<string, object>(raw, StringComparer.OrdinalIgnoreCase);
                if (string.Equals(GetString(row, "ESTADO"), "A", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("No se puede cambiar el salario de un expediente aprobado.");
                }

                var tabla = InicializarTablaSalariosDesdePersona(
                    connection, expediente, GetString(row, "CEDULA"),
                    ObtenerTablaSalarios(connection, expediente));
                var respuesta = new FrmPreaEstudiov2SalariosDto
                {
                    tipo_salario = tipo,
                    salario_devengado = GetDecimal(row, "SALARIO_DEVENGADO_COLILLA"),
                    tabla_salarios = tabla
                };

                switch (codigoOperacion[0])
                {
                    case 'd':
                        respuesta.salario_devengado = 0;
                        respuesta.tabla_salarios = [];
                        break;
                    case 'g':
                        respuesta.tabla_salarios = [];
                        break;
                    case 'f':
                        respuesta.salarios_seleccion = connection
                            .Query<FrmPreaEstudiov2SalarioSeleccionDto>(
                                "EXEC spCRDPreaSalario @Expediente, @Tipo",
                                new { Expediente = expediente, Tipo = tipo })
                            .ToList();

                        if (respuesta.salarios_seleccion.Count == 0)
                        {
                            throw new InvalidOperationException(
                                "No hay salarios hist&oacute;ricos disponibles para seleccionar.");
                        }

                        respuesta.salario_devengado = 0;
                        break;
                    default:
                        var salarios = connection.Query(
                            "EXEC spCRDPreaSalario @Expediente, @Tipo",
                            new { Expediente = expediente, Tipo = tipo }).ToList();
                        if (salarios.Count != 1)
                        {
                            throw new InvalidOperationException(
                                "Este tipo requiere seleccionar un salario de la lista hist&oacute;rica.");
                        }
                        var salario = new Dictionary<string, object>(
                            (IDictionary<string, object>)salarios[0],
                            StringComparer.OrdinalIgnoreCase);
                        respuesta.salario_devengado = GetDecimal(salario, "Salario");
                        // Lee nuevamente por si el procedimiento actualiza el detalle.
                        respuesta.tabla_salarios = ObtenerTablaSalarios(connection, expediente);
                        break;
                }

                return respuesta;
            });
        }
    }
}
