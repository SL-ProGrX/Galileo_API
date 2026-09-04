using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaEstudiov2DB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Carga la información completa de un expediente de estudio de crédito.
        /// Obtiene estado, encabezado, crédito, salarios y catálogos en una sola llamada.
        /// </summary>
        /// <remarks>
        /// VB6 (frmPreaEstudiov2.frm) no llama a "spCRDPreaPREANALISIS" directamente.
        /// Usa clsEntidad, que arma el SP a partir de una convención genérica:
        ///   clsEntidad.tablaName = "spCRDPreaPREANALISIS"
        ///   clsEntidad.fxTraerUno(parametro)  -->  EXEC spCRDPreaPREANALISIS_T '&lt;param&gt;'
        /// (ver clsEntidad.fxTraerUno: "EXEC " &amp; m_tableName &amp; "_T " &amp; ParamsArray)
        ///
        /// El resultado es UN solo recordset plano con ~80 columnas (no varios result sets),
        /// leído campo a campo en sbLigarDatos. Aquí se mapean únicamente los campos que
        /// las pantallas de Angular actualmente consumen (estado/encabezado/credito/salarios).
        ///
        /// Campos que NO vienen de este SP en VB6 y se resuelven aparte:
        ///   - encabezado.estado_persona: ObtenerEstadoPersona (réplica de txtCedula_LostFocus
        ///     contra socios/AFI_ESTADOS_PERSONA).
        ///   - encabezado.edad: ObtenerEdad (réplica de dbo.fxSys_Edad_Anios).
        ///   - salarios.tabla_salarios/extras/incapacidades: ObtenerTablaSalarios/ObtenerExtras/
        ///     ObtenerIncapacidades (réplica de sbSalarios_Load/sbExtras_Load/sbIncapacidades_Load).
        ///   - comite_resolutivo: rs!ID_COMITE (sí está en el recordset plano).
        ///   - salario_minimo_inembargable/salario_normativa, edad máxima permitida,
        ///     % P.S.D. y % FRAP/FAP: ParametrosGlobales (parámetros globales de empresa,
        ///     CRD_PREA_PARAMETROS, no del expediente; una sola lectura por carga).
        ///
        /// Campos que SIGUEN sin origen confirmado (quedan con su valor por defecto):
        ///   - encabezado.clasificacion_crediticia (rs!CATEGORIA en LigarDatosClasificacion, sub aparte)
        ///   - encabezado.edad_aplica / edad_justificacion (query aparte sobre
        ///     APL_JUSTIFICACION_EDAD/JUSTIFICACION_EDAD)
        ///   - estado.tiene_alerta / mensaje_alerta (no se encontró origen confirmado en el .frm)
        ///   - credito.contrato (cboFondoContrato no se restaura en sbLigarDatos)
        ///   - credito.respaldo / cph: en VB6 "Respaldo" es solo un Label junto a cboGarantia,
        ///     no hay un combo propio con ese nombre; y el Angular actual espera una estructura
        ///     ("respaldo_options" embebido por garantía) que no existe en ningún lado del VB6
        ///     revisado. Requiere investigación aparte antes de implementarlo — no se adivinó.
        /// </remarks>
        public ErrorDto<FrmPreaEstudiov2CargaResponse> Prea_frmPreaEstudiov2_Cargar(
            int codEmpresa,
            FrmPreaEstudiov2CargaRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CargaResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CargaResponse()
            };

            var codPreanalisis = request.cod_preanalisis?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(codPreanalisis))
            {
                result.Code = -1;
                result.Description = "Debe indicar el código de expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                // La carga de un expediente encadena decenas de consultas sobre esta misma
                // conexión. Dapper la abre y la cierra en cada Query cuando llega cerrada,
                // así que se abre una sola vez aquí y el using se encarga de cerrarla.
                connection.Open();

                // Snapshot único de CRD_PREA_PARAMETROS (ver ParametrosGlobales): antes se
                // leía 5 veces durante esta misma carga.
                var parametros = ParametrosGlobales.Leer(connection);

                // clsEntidad concatena el parámetro como literal SQL (fxFormatearValor),
                // no como parámetro nombrado. Se replica igual, escapando comillas simples.
                const string sql = "EXEC spCRDPreaPREANALISIS_T @CodPreanalisis";
                var rawRow = connection.QueryFirstOrDefault(sql, new { CodPreanalisis = codPreanalisis })
                    as IDictionary<string, object>;

                if (rawRow is null)
                {
                    result.Code = -1;
                    result.Description = $"No se encontró el expediente {codPreanalisis}.";
                    return result;
                }

                // VB6 accede a los campos del recordset con "rs!Campo" (comparación
                // insensible a mayúsculas/minúsculas de ADO), así que no se puede saber
                // con certeza el casing real de las columnas devueltas por el SP.
                // Se normaliza a un diccionario case-insensitive para que los nombres de
                // columna mapeados abajo (tomados literalmente del código VB6) coincidan
                // sin importar cómo los haya declarado el SP en la base de datos.
                IDictionary<string, object> row = new Dictionary<string, object>(rawRow, StringComparer.OrdinalIgnoreCase);

                // Estado de la persona, edad, frecuencia de pago y justificación de edad
                // en un solo batch (antes eran cuatro consultas sueltas).
                var datosPersona = ObtenerDatosPersona(
                    connection,
                    GetString(row, "Cedula"),
                    GetDateTime(row, "fecha_nacimiento"),
                    codPreanalisis);

                var lineaActual = GetString(row, "Cod_Linea");
                // '09' -> GlobalPorcFRAPFAP, '13' -> GlobalPorcPSD (mPreAnalisis.bas,
                // sbInicializaGlobales).
                var porcFrapFap = parametros.Decimal("09");
                var porcPsd = parametros.Decimal("13");
                var ptsExtraFrap = row.ContainsKey("PTS_EXTRA_FRAP")
                    ? GetDecimal(row, "PTS_EXTRA_FRAP")
                    : GetDecimal(row, "PTS_EXTRA_FAP");
                var credito = ConstruirCredito(connection, row, datosPersona.FrecuenciaPago);
                var esExpedientePrincipal = EsExpedientePrincipal(codPreanalisis);
                var psd = CalcularPsd(credito.monto, porcPsd, esExpedientePrincipal);
                var montoGirar = CalcularMontoGirar(
                    credito.monto,
                    new RebajosMontoGirar(
                        GetDecimal(row, "REFUNDICIONES"),
                        GetDecimal(row, "DESEMBOLSOS"),
                        credito.primera_cuota ? credito.cuota : 0m,
                        psd,
                        GetDecimal(row, "Monto_Interes"),
                        GetDecimal(row, "MONTO_COMISION")),
                    esExpedientePrincipal);

                result.Result = new FrmPreaEstudiov2CargaResponse
                {
                    estado = new FrmPreaEstudiov2EstadoDto
                    {
                        cod_preanalisis = codPreanalisis,
                        estado = GetString(row, "Estado"),
                        estado_desc = GetString(row, "EstadoDesc"),
                        estado_v2 = GetString(row, "COD_ESTADO_V2"),
                        estado_v2_desc = GetString(row, "EstadoV2Desc"),
                        editable = GetBool(row, "INDICADOR_EDITABLE"),
                    },
                    encabezado = new FrmPreaEstudiov2EncabezadoDto
                    {
                        cedula = GetString(row, "Cedula"),
                        nombre = GetString(row, "Nombre"),
                        sexo = GetString(row, "sexo"),
                        fecha_nacimiento = GetDateTime(row, "fecha_nacimiento"),
                        estado_persona = datosPersona.EstadoPersona,
                        edad = datosPersona.Edad,
                        clasificacion_crediticia = ObtenerClasificacionCrediticia(connection, GetString(row, "Cedula"), GetString(row, "Estado")),
                        edad_aplica = datosPersona.EdadAplica,
                        edad_justificacion = datosPersona.EdadJustificacion,
                        observacion_analista = GetString(row, "OBSERVACION_ANALISTA"),
                        observacion_comite = GetString(row, "OBSERVACION_COMITE"),
                        observacion_jd = GetString(row, "OBSERVACION_JD"),
                        registro_usuario = GetString(row, "Usuario"),
                        registro_fecha = GetDateTime(row, "FECHA_CREACION"),
                    },
                    credito = credito,
                    salarios = ConstruirSalarios(connection, row, codPreanalisis),
                    resumen = new FrmPreaEstudiov2ResumenDto
                    {
                        salario_real = GetDecimal(row, "SALARIO_REAL"),
                        cargas = GetDecimal(row, "TOTAL_CARGA_CCSS"),
                        carga_ccss = GetDecimal(row, "CARGA_CCSS"),
                        carga_asociacion = GetDecimal(row, "CARGA_ASOCIACION"),
                        carga_frap = GetDecimal(row, "CARGA_FRAP"),
                        carga_impuesto_salario = GetDecimal(row, "CARGA_IMPUESTO_SALARIO"),
                        pts_extra_frap = ptsExtraFrap,
                        porc_frap_fap = porcFrapFap,
                        aplica_carga_asociacion = GetDecimal(row, "CARGA_ASOCIACION") > 0m,
                        aplica_carga_frap = GetDecimal(row, "CARGA_FRAP") > 0m,
                        porc_sobre_salario = GetDecimal(row, "PORCENTAJE_LIBRE"),
                        deducciones = GetDecimal(row, "DEDUCCIONES"),
                        creditos_cancelados = GetDecimal(row, "CRD_TRANSITO_CANCELADOS"),
                        creditos_por_cobrar = GetDecimal(row, "CRD_TRANSITO_XCOBRAR"),
                        salario_liquido = GetDecimal(row, "SALARIO_LIQUIDO"),
                        refundiciones = GetDecimal(row, "REFUNDICIONES"),
                        refundiciones_cuota = GetDecimal(row, "REFUNDICIONES_CUOTA"),
                        desembolsos = GetDecimal(row, "DESEMBOLSOS"),
                        desembolsos_cuota = GetDecimal(row, "DESEMBOLSOS_CUOTA"),
                        total_liquido_persona = GetDecimal(row, "LIQUIDO_TOTAL"),
                        total_liquido_grupo = GetDecimal(row, "LIQUIDO_TOTAL_GRUPO"),
                        fianzas = GetDecimal(row, "FIANZAS"),
                        comisiones = GetDecimal(row, "MONTO_COMISION"),
                        intereses = GetDecimal(row, "Monto_Interes"),
                        psd = psd,
                        monto_girar = montoGirar,
                        // VB6 (frmPreaEstudiov2.frm ~10120): Abs(Cuota - (REFUNDICIONES_CUOTA + DESEMBOLSOS_CUOTA))
                        diferencia_cuota = Math.Abs(GetDecimal(row, "Cuota") - (GetDecimal(row, "REFUNDICIONES_CUOTA") + GetDecimal(row, "DESEMBOLSOS_CUOTA"))),
                        salario_minimo_estudio = GetDecimal(row, "SALARIO_USURA"),
                        salario_normativa_estudio = GetDecimal(row, "SALARIO_NORMATIVA"),
                        liquidez_sin_fianzas = GetDecimal(row, "LIQUIDEZ_SIMPLE"),
                        liquidez_sin_fianzas_porc = GetDecimal(row, "PORC_LIQ_SIN_FIANZA"),
                        liquidez_con_fianzas = GetDecimal(row, "LIQUIDEZ_CFIANZAS"),
                        liquidez_con_fianzas_porc = GetDecimal(row, "PORC_LIQ_CON_FIANZA"),
                        liquidez_sin_fianzas_comp = GetDecimal(row, "LIQUIDEZ_SFIANZAS_CA"),
                        liquidez_sin_fianzas_comp_porc = GetDecimal(row, "PORC_LIQ_SIN_FIANZA_CA"),
                        liquidez_con_fianzas_comp = GetDecimal(row, "LIQUIDEZ_CFIANZAS_CA"),
                        liquidez_con_fianzas_comp_porc = GetDecimal(row, "PORC_LIQ_CON_FIANZA_CA"),
                    },
                    catalogos = ObtenerDestinosGarantiasResponse(connection, lineaActual),
                    comite_resolutivo = GetString(row, "ID_COMITE"),
                };

                // '17' -> GlobalSalarioMinimoInembargable,
                // GlobalSalarioNormativo = VALOR('17') + VALOR('22').
                var salarioMinimoInembargable = parametros.Decimal("17");
                result.Result.salario_minimo_inembargable = salarioMinimoInembargable;
                result.Result.salario_normativa = salarioMinimoInembargable + parametros.Decimal("22");

                // '01' -> GlobalEdadMaximaPermitidaHombre, '02' -> ...Mujeres.
                result.Result.edad_maxima_hombres = parametros.Entero("01");
                result.Result.edad_maxima_mujeres = parametros.Entero("02");

                // VB6 recarga cboSubExpediente dentro de la misma carga; se devuelve aquí
                // para que Angular no necesite una segunda llamada HTTP.
                result.Result.sub_expedientes = ObtenerSubExpedientes(
                    connection,
                    ObtenerExpedientePadre(codPreanalisis));
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2CargaResponse();
            }

            return result;
        }

        /// <summary>
        /// Replica v_Expediente de VB6: txtExpediente sin guion calcula P.S.D. y monto a girar.
        /// </summary>
        private static bool EsExpedientePrincipal(string codPreanalisis)
            => !string.IsNullOrWhiteSpace(codPreanalisis)
                && codPreanalisis.IndexOf("-", StringComparison.Ordinal) < 0;

        /// <summary>
        /// Replica ePolizaSD: txtPSD = txtMonto * GlobalPorcPSD / 100.
        /// </summary>
        private static decimal CalcularPsd(decimal monto, decimal porcentajePsd, bool esExpedientePrincipal)
            => esExpedientePrincipal
                ? Math.Round(monto * porcentajePsd / 100m, 2)
                : 0m;

        private readonly record struct RebajosMontoGirar(
            decimal Refundiciones,
            decimal Desembolsos,
            decimal PrimeraCuota,
            decimal Psd,
            decimal Intereses,
            decimal Comisiones);

        /// <summary>
        /// Replica eMontoGirar restando P.S.D., rebajos de formalización y primera cuota cuando aplica.
        /// </summary>
        private static decimal CalcularMontoGirar(
            decimal monto,
            RebajosMontoGirar rebajos,
            bool esExpedientePrincipal)
        {
            if (!esExpedientePrincipal)
            {
                return 0m;
            }

            return monto - (
                rebajos.Refundiciones
                + rebajos.Desembolsos
                + rebajos.PrimeraCuota
                + rebajos.Psd
                + rebajos.Intereses
                + rebajos.Comisiones);
        }
        /// <summary>
        /// Edad de la persona en años. VB6 (txtCedula_LostFocus, línea ~16869):
        ///   SELECT dbo.fxSys_Edad_Anios('&lt;fecha_nacimiento&gt;') as 'Edad'
        /// </summary>
        private static int ObtenerEdad(IDbConnection connection, DateTime? fechaNacimiento)
        {
            if (fechaNacimiento is null)
            {
                return 0;
            }

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@FechaNacimiento", fechaNacimiento.Value.ToString("yyyy-MM-dd"), DbType.String);

                return connection.QueryFirstOrDefault<int?>(
                    "SELECT dbo.fxSys_Edad_Anios(@FechaNacimiento)",
                    parameters
                ) ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Estado de la persona (socio/no socio). En VB6 (txtCedula_LostFocus, línea ~16804)
        /// se ejecuta primero contra "socios" y, si no hay resultado, contra
        /// CRD_PREA_PREANALISIS. No es parte de spCRDPreaPREANALISIS_T.
        /// </summary>
        private static string ObtenerEstadoPersona(IDbConnection connection, string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return string.Empty;
            }

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", cedula, DbType.String);

                var estado = connection.QueryFirstOrDefault<string>(
                    @"SELECT ISNULL(E.descripcion, '')
                      FROM socios S
                      LEFT JOIN AFI_ESTADOS_PERSONA E ON S.EstadoActual = E.cod_Estado
                      WHERE S.cedula = @Cedula",
                    parameters
                );

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    return estado.Trim();
                }

                // No es socio: en VB6 se marca como "No Socio" cuando la cédula
                // existe en CRD_PREA_PREANALISIS pero no en socios.
                var existeEnPreanalisis = connection.QueryFirstOrDefault<int?>(
                    "SELECT TOP 1 1 FROM CRD_PREA_PREANALISIS WHERE cedula = @Cedula",
                    parameters
                );

                return existeEnPreanalisis.HasValue ? "No Socio" : string.Empty;
            }
            catch (DataException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Clasificación crediticia. VB6 (txtCedula_LostFocus, línea ~16888):
        ///   Si Estado empieza con 'R' o 'P': EXEC spCRDConsultarCategoriaAsociado '&lt;cedula&gt;'
        ///   campo CATEGORIA.
        /// </summary>
        private static string ObtenerClasificacionCrediticia(IDbConnection connection, string cedula, string estado)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return string.Empty;
            }

            var estadoTrim = (estado ?? string.Empty).Trim();
            if (!estadoTrim.StartsWith("R", StringComparison.OrdinalIgnoreCase)
                && !estadoTrim.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            try
            {
                const string sql = "EXEC spCRDConsultarCategoriaAsociado @Cedula";
                var row = connection.QueryFirstOrDefault(sql, new { Cedula = cedula }) as IDictionary<string, object>;

                if (row is null)
                {
                    return string.Empty;
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return GetString(dict, "CATEGORIA");
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Justificación de edad. VB6 (sbEdad_Verifica, línea ~9380):
        ///   SELECT ISNULL(APL_JUSTIFICACION_EDAD,0) 'EDAD_APLICA', ISNULL(JUSTIFICACION_EDAD,'') 'EDAD_JUSTIFICACION'
        ///   FROM CRD_PREA_PREANALISIS WHERE COD_PREANALISIS = '&lt;expediente&gt;'
        /// </summary>
        private static (int edadAplica, string edadJustificacion) ObtenerJustificacionEdad(IDbConnection connection, string codPreanalisis)
        {
            if (string.IsNullOrWhiteSpace(codPreanalisis))
            {
                return (0, string.Empty);
            }

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CodPreanalisis", codPreanalisis, DbType.String);

                var row = connection.QueryFirstOrDefault(
                    @"SELECT ISNULL(APL_JUSTIFICACION_EDAD,0) AS EDAD_APLICA, ISNULL(JUSTIFICACION_EDAD,'') AS EDAD_JUSTIFICACION
                      FROM CRD_PREA_PREANALISIS WHERE COD_PREANALISIS = @CodPreanalisis",
                    parameters
                ) as IDictionary<string, object>;

                if (row is null)
                {
                    return (0, string.Empty);
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                return (GetInt(dict, "EDAD_APLICA"), GetString(dict, "EDAD_JUSTIFICACION"));
            }
            catch
            {
                return (0, string.Empty);
            }
        }

        /// <summary>
        /// Destino y Garantía en cascada por línea de crédito. VB6:
        ///   sbSTCargaCboDestinos (mValidacion.bas ~275): catalogo_destinos D
        ///     INNER JOIN catalogo_destinosASG C ON D.cod_destino = C.cod_destino
        ///     WHERE C.codigo = '&lt;linea&gt;' ORDER BY D.prioridad
        ///   sbSTCargaCboGarantiav2 (mValidacion.bas ~308): crd_catalogo_garantias C
        ///     INNER JOIN crd_garantia_tipos T ON C.garantia = T.garantia
        ///     WHERE C.codigo = '&lt;linea&gt;'
        /// Si no hay línea seleccionada, VB6 llama con "-1" al inicializar el formulario
        /// (ambas consultas típicamente no devuelven filas en ese caso).
        /// </summary>
        private static (List<FrmPreaEstudiov2DropdownDto> destinos, List<FrmPreaEstudiov2DropdownDto> garantias) ObtenerDestinosGarantias(
            IDbConnection connection, string linea)
        {
            var lineaParam = string.IsNullOrWhiteSpace(linea) ? "-1" : linea.Trim();

            List<FrmPreaEstudiov2DropdownDto> destinos;
            try
            {
                const string sql = @"SELECT rtrim(D.cod_destino) AS item, rtrim(D.descripcion) AS descripcion
                            FROM catalogo_destinos D
                            INNER JOIN catalogo_destinosASG C ON D.cod_destino = C.cod_destino
                            WHERE C.codigo = @Linea
                            ORDER BY D.prioridad ASC";
                destinos = connection.Query<FrmPreaEstudiov2DropdownDto>(sql, new { Linea = lineaParam }).ToList();
            }
            catch
            {
                destinos = [];
            }

            List<FrmPreaEstudiov2DropdownDto> garantias;
            try
            {
                const string sql = @"SELECT T.Garantia AS item, rtrim(T.descripcion) AS descripcion, rtrim(T.Formulario) AS formulario
                            FROM crd_catalogo_garantias C
                            INNER JOIN crd_garantia_tipos T ON C.garantia = T.garantia
                            WHERE C.codigo = @Linea";
                garantias = connection.Query<FrmPreaEstudiov2DropdownDto>(sql, new { Linea = lineaParam }).ToList();
            }
            catch
            {
                garantias = [];
            }

            return (destinos, garantias);
        }

        /// <summary>
        /// Consulta pública de destinos/garantías por línea, para recargar los combos
        /// cuando el usuario cambia la Línea en Angular (equivalente a lo que VB6 hace
        /// en txtLinea_LostFocus).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DestinosGarantiasResponse> Prea_frmPreaEstudiov2_DestinosGarantias_Consultar(
            int codEmpresa, string linea)
        {
            var result = new ErrorDto<FrmPreaEstudiov2DestinosGarantiasResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DestinosGarantiasResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var (destinos, garantias) = ObtenerDestinosGarantias(connection, linea);
                result.Result.destinos = destinos;
                result.Result.garantias = garantias;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Indica si el par Línea/Destino aplica Primera Cuota. Fiel a VB6
        /// (frmPreaEstudiov2.frm, sbAplicaPrimeraCta ~línea 14082):
        ///   clsEntidad.tablaName = "spCRDPreaDestinos"
        ///   clsEntidad.fxTraerFiltrado("AplicaPrimCta", "'&lt;linea&gt;','&lt;destino&gt;'")
        ///   -&gt; EXEC spCRDPreaDestinos_TXAplicaPrimCta '&lt;linea&gt;','&lt;destino&gt;'
        ///   chkPrimerCuota.Value = rs!PRIMER_CUOTA (0 si el SP no devuelve fila).
        /// Reemplaza la llamada que Angular hacía a Prea_frmPreaEstudiov2_Cargar completo
        /// (decenas de consultas) solo para resolver este único indicador.
        /// </summary>
        public ErrorDto<bool> Prea_frmPreaEstudiov2_Destino_PrimeraCuota(
            int codEmpresa, string linea, string destino)
        {
            var result = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            var lineaTrim = linea?.Trim() ?? string.Empty;
            var destinoTrim = destino?.Trim() ?? string.Empty;

            // VB6 sale sin marcar el check si no hay destino o no hay línea descrita.
            if (string.IsNullOrEmpty(lineaTrim) || string.IsNullOrEmpty(destinoTrim))
            {
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCRDPreaDestinos_TXAplicaPrimCta @Linea, @Destino";
                var row = connection.QueryFirstOrDefault(
                    sql,
                    new { Linea = lineaTrim, Destino = destinoTrim }) as IDictionary<string, object>;

                if (row is not null)
                {
                    var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                    result.Result = GetBool(dict, "PRIMER_CUOTA");
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = false;
            }

            return result;
        }

        /// <summary>
        /// Expediente padre de un sub-expediente. VB6 usa el prefijo antes del guion
        /// (v_Expediente / txtExpediente): "5424-1" -&gt; "5424".
        /// </summary>
        private static string ObtenerExpedientePadre(string codPreanalisis)
        {
            var valor = (codPreanalisis ?? string.Empty).Trim();
            var idx = valor.IndexOf('-', StringComparison.Ordinal);
            return idx > -1 ? valor[..idx] : valor;
        }

        /// <summary>
        /// Códigos de los sub-expedientes de un expediente principal. VB6
        /// (sbLlenarComboFiltrado sobre cboSubExpediente, línea ~11198):
        ///   EXEC spCRDPreaPREANALISIS_TXSubExpediente '&lt;padre&gt;'
        /// No incluye al expediente principal; eso lo agrega el consumidor.
        /// </summary>
        private static List<string> ObtenerSubExpedientes(IDbConnection connection, string expedientePadre)
        {
            var padre = (expedientePadre ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(padre))
            {
                return [];
            }

            try
            {
                const string sql = "EXEC spCRDPreaPREANALISIS_TXSubExpediente @Padre";
                var rows = connection.Query(sql, new { Padre = padre });

                var subExpedientes = new List<string>();
                foreach (var r in rows)
                {
                    var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);
                    var codPreanalisis = GetString(dict, "COD_PREANALISIS");

                    // VB6 (sbLlenarComboFiltrado) descarta filas con ValueMember = "-1".
                    if (!string.IsNullOrEmpty(codPreanalisis) && codPreanalisis != "-1")
                    {
                        subExpedientes.Add(codPreanalisis);
                    }
                }

                return subExpedientes;
            }
            catch (DataException)
            {
                return [];
            }
        }

        /// <summary>
        /// Destinos y garantías en cascada por línea, ya empaquetados en la respuesta que
        /// consume tanto Cargar como el endpoint de cambio de línea.
        /// </summary>
        private static FrmPreaEstudiov2DestinosGarantiasResponse ObtenerDestinosGarantiasResponse(
            IDbConnection connection, string linea)
        {
            var (destinos, garantias) = ObtenerDestinosGarantias(connection, linea);
            return new FrmPreaEstudiov2DestinosGarantiasResponse
            {
                destinos = destinos,
                garantias = garantias,
            };
        }

        /// <summary>
        /// Catálogos estáticos del formulario (los que VB6 llena una sola vez en
        /// Form_Load). Angular los pide al abrir la pantalla, no en cada expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CatalogosResponse> Prea_frmPreaEstudiov2_Catalogos_Consultar(int codEmpresa)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CatalogosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CatalogosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var parametros = ParametrosGlobales.Leer(connection);
                result.Result = CargarCatalogosEstaticos(connection, parametros);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2CatalogosResponse();
            }

            return result;
        }

        /// <summary>
        /// Sub-expedientes (fiadores) ligados a un expediente principal. VB6
        /// (sbLlenarComboFiltrado sobre cboSubExpediente, línea ~11198):
        ///   clsEntidad.tablaName = "spCRDPreaPREANALISIS"
        ///   clsEntidad.fxTraerFiltrado("SubExpediente", "&lt;padre&gt;")
        ///   -&gt; EXEC spCRDPreaPREANALISIS_TXSubExpediente '&lt;padre&gt;'
        /// Devuelve únicamente el campo COD_PREANALISIS de cada sub-expediente
        /// (no incluye al expediente principal; eso lo agrega el consumidor).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SubExpedientesResponse> Prea_frmPreaEstudiov2_SubExpedientes_Consultar(
            int codEmpresa, string expedientePadre)
        {
            var result = new ErrorDto<FrmPreaEstudiov2SubExpedientesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2SubExpedientesResponse()
            };

            var padre = (expedientePadre ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(padre))
            {
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                result.Result.sub_expedientes = ObtenerSubExpedientes(connection, padre);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Genera/valida el número de un nuevo sub-expediente (fiador). VB6
        /// (cboSubExpediente_Click, caso "Nuevo SubExpediente", línea ~14676):
        ///   EXEC spCrd_Prea_Expediente_Numero '&lt;expediente&gt;', 'S'
        /// Si rs!Expediente = "0" es un error de validación (rs!Mensaje trae el motivo);
        /// en VB6 el valor de rs!Expediente devuelto NO se asigna a txtExpediente.Text
        /// (línea comentada) — el código real del sub-expediente lo genera el SP de
        /// guardado (spCrdPreaPreanalisisNuevo/Modifica) al persistir, no este paso.
        /// Esta llamada es solo la validación previa que hace VB6 antes de habilitar
        /// la captura del nuevo fiador.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SubExpedienteGenerarResponse> Prea_frmPreaEstudiov2_SubExpediente_Generar(
            int codEmpresa, string expediente)
        {
            var result = new ErrorDto<FrmPreaEstudiov2SubExpedienteGenerarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2SubExpedienteGenerarResponse()
            };

            var expedienteTrim = (expediente ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(expedienteTrim))
            {
                result.Code = -1;
                result.Description = "Debe indicar el expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                const string sql = "EXEC spCrd_Prea_Expediente_Numero @Expediente, 'S'";
                var row = connection.QueryFirstOrDefault(sql, new { Expediente = expedienteTrim })
                    as IDictionary<string, object>;

                if (row is null)
                {
                    result.Code = -1;
                    result.Description = "No se pudo validar el nuevo sub-expediente.";
                    return result;
                }

                var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                var expedienteResultado = GetString(dict, "Expediente");
                var mensaje = GetString(dict, "Mensaje");

                result.Result.expediente = expedienteResultado;
                result.Result.mensaje = mensaje;
                result.Result.exito = expedienteResultado != "0";

                if (!result.Result.exito)
                {
                    result.Code = -1;
                    result.Description = mensaje;
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Tabla de salarios del expediente (grid "Tabla de Salarios").
        /// VB6: sbSalarios_Load -> exec spCrdPreaTraeSalariosExpediente '&lt;expediente&gt;'
        /// </summary>
        private static List<FrmPreaEstudiov2SalarioDetalleDto> ObtenerTablaSalarios(IDbConnection connection, string codPreanalisis)
        {
            try
            {
                const string sql = "EXEC spCrdPreaTraeSalariosExpediente @CodPreanalisis";
                return MapearTablaSalarios(connection.Query(sql, new { CodPreanalisis = codPreanalisis }));
            }
            catch
            {
                return [];
            }
        }

        /// <summary>Mapeo de las filas de spCrdPreaTraeSalariosExpediente.</summary>
        private static List<FrmPreaEstudiov2SalarioDetalleDto> MapearTablaSalarios(IEnumerable<dynamic> rows)
        {
            var lista = new List<FrmPreaEstudiov2SalarioDetalleDto>();

            foreach (var r in rows)
            {
                var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);

                // VB6 sólo agrega la fila si Salario1 + Salario0 + Monto_CA > 0
                var salario1 = GetDecimal(dict, "Salario1");
                var salario0 = GetDecimal(dict, "Salario0");
                var montoCa = GetDecimal(dict, "Monto_CA");

                if (salario1 + salario0 + montoCa <= 0)
                {
                    continue;
                }

                lista.Add(new FrmPreaEstudiov2SalarioDetalleDto
                {
                    orden = GetInt(dict, "Orden"),
                    fecha = GetDateTime(dict, "Fecha1"),
                    salario_s = salario1,
                    mes = GetInt(dict, "Mes1"),
                    salario_rh = salario0,
                    ca = montoCa,
                });
            }

            return lista;
        }

        /// <summary>
        /// Extras del expediente (grid "Extras").
        /// VB6: sbExtras_Load -> exec spCRDPreaDETALLE_EXTRAS_TxExpediente '&lt;expediente&gt;'
        /// </summary>
        private static List<FrmPreaEstudiov2ExtraDto> ObtenerExtras(IDbConnection connection, string codPreanalisis)
        {
            try
            {
                const string sql = "EXEC spCRDPreaDETALLE_EXTRAS_TxExpediente @CodPreanalisis";
                return MapearExtras(connection.Query(sql, new { CodPreanalisis = codPreanalisis }));
            }
            catch
            {
                return [];
            }
        }

        /// <summary>Mapeo de las filas de spCRDPreaDETALLE_EXTRAS_TxExpediente.</summary>
        private static List<FrmPreaEstudiov2ExtraDto> MapearExtras(IEnumerable<dynamic> rows)
        {
            var lista = new List<FrmPreaEstudiov2ExtraDto>();

            foreach (var r in rows)
            {
                var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);

                lista.Add(new FrmPreaEstudiov2ExtraDto
                {
                    idx = GetInt(dict, "IdX"),
                    cod_extras = GetString(dict, "COD_EXTRAS"),
                    tipo_extra = GetString(dict, "TipoExtra"),
                    monto = GetDecimal(dict, "Monto"),
                });
            }

            return lista;
        }

        /// <summary>
        /// Incapacidades del expediente.
        /// VB6: sbIncapacidades_Load -> SELECT directo a CRD_PREA_V2_INCAPACIDADES.
        /// </summary>
        private static List<FrmPreaEstudiov2IncapacidadDto> ObtenerIncapacidades(IDbConnection connection, string codPreanalisis)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@CodPreanalisis", codPreanalisis, DbType.String);

                var registros = connection.Query(
                    @"SELECT COD_PREANALISIS, DIAS, DESDE, HASTA, ORDEN
                      FROM CRD_PREA_V2_INCAPACIDADES
                      WHERE COD_PREANALISIS = @CodPreanalisis
                      ORDER BY ORDEN",
                    parameters
                );

                return MapearIncapacidades(registros);
            }
            catch
            {
                return [];
            }
        }

        /// <summary>Mapeo de las filas de CRD_PREA_V2_INCAPACIDADES.</summary>
        private static List<FrmPreaEstudiov2IncapacidadDto> MapearIncapacidades(IEnumerable<dynamic> rows)
        {
            var lista = new List<FrmPreaEstudiov2IncapacidadDto>();

            foreach (var r in rows)
            {
                var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);

                lista.Add(new FrmPreaEstudiov2IncapacidadDto
                {
                    orden = GetInt(dict, "Orden"),
                    desde = GetDateTime(dict, "Desde"),
                    hasta = GetDateTime(dict, "Hasta"),
                    dias = GetInt(dict, "Dias"),
                });
            }

            return lista;
        }

        private static string GetString(IDictionary<string, object> row, string column)
        {
            if (!row.TryGetValue(column, out var value) || value is null || value is DBNull)
            {
                return string.Empty;
            }

            return value.ToString()?.Trim() ?? string.Empty;
        }

        private static decimal GetDecimal(IDictionary<string, object> row, string column)
        {
            if (!row.TryGetValue(column, out var value) || value is null || value is DBNull)
            {
                return 0m;
            }

            return Convert.ToDecimal(value);
        }

        private static int GetInt(IDictionary<string, object> row, string column)
        {
            if (!row.TryGetValue(column, out var value) || value is null || value is DBNull)
            {
                return 0;
            }

            return Convert.ToInt32(Convert.ToDecimal(value));
        }

        private static bool GetBool(IDictionary<string, object> row, string column)
        {
            if (!row.TryGetValue(column, out var value) || value is null || value is DBNull)
            {
                return false;
            }

            if (value is bool b) return b;

            return Convert.ToDecimal(value) != 0;
        }

        private static DateTime? GetDateTime(IDictionary<string, object> row, string column)
        {
            if (!row.TryGetValue(column, out var value) || value is null || value is DBNull)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }

        /// <summary>
        /// Carga los catálogos (combos) necesarios para el formulario.
        /// Fuentes verificadas contra frmPreaEstudiov2.frm / mValidacion.bas (las tablas
        /// usadas antes -CRD_LINEA_CREDITO, CRD_DESTINO_CREDITO, CRD_GARANTIA_CREDITO,
        /// CRD_TIPO_SALARIO, CRD_COMPONENTE_ADICIONAL, CRD_COMITE_RESOLUTIVO- no existen
        /// en VB6 y eran una suposición sin verificar):
        ///   - lineas: txtLinea es un campo de texto con búsqueda F4 (Case "txtLinea",
        ///     línea ~11504) contra "select codigo,descripcion from catalogo where
        ///     Activo = 1 and Retencion = 'N'". No es un combo precargado en VB6, pero se
        ///     expone como lista para el p-select de Angular.
        ///   - destinos/garantias: dependen de la línea seleccionada (combos en cascada).
        ///     Ver sbSTCargaCboDestinos/sbSTCargaCboGarantiav2 en mValidacion.bas.
        ///     Cuando se llama desde Cargar() con la línea del expediente ya cargado, se
        ///     usan esas tablas filtradas por esa línea (igual que sbLigarDatos hace tras
        ///     poblar txtLinea). Cuando se llama desde el endpoint de cambio de línea
        ///     (onChangeLinea en Angular) se recalculan con la nueva línea.
        ///   - tipos_salario: cboSalario se llena con sbLlenarComboTodosV2(cboSalario,
        ///     "spCRDPreaTIPO_SALARIO", "TIPO_SALARIO", "DescTipoSalario") -> por
        ///     convención de clsEntidad.fxTraerTodos esto ejecuta "spCRDPreaTIPO_SALARIO_TT".
        ///   - componentes_adicionales: cboS_ComponenteAdicional se llena con
        ///     "SELECT COD_PARAMETRO as IdX, DESCRIPCION + ' [ ' + VALOR + ' % ]' as ItmX
        ///     FROM CRD_PREA_PARAMETROS WHERE COD_PARAMETRO IN('18','19','20')".
        ///   - comites: sbCargaCboComites -> "Select id_comite as IdX, descripcion as ItmX
        ///     from comites where estado = 1".
        ///
        /// NO se cargan aquí (se quitaron por costo, no por paridad):
        ///   - expedientes: era un SELECT completo de CRD_PREA_PREANALISIS en cada carga
        ///     de expediente y ningún control de Angular lo consumía (en VB6 la búsqueda
        ///     de expedientes es frmPreaConsultaExpeditentes, no un combo precargado).
        ///   - bancos: el tab Desembolsos los obtiene de
        ///     Prea_frmPreaEstudiov2_Desembolsos_Consultar, que ya ejecuta
        ///     spCrd_SGT_Bancos_Desembolso con el usuario de sesión.
        /// </summary>
        private static FrmPreaEstudiov2CatalogosResponse CargarCatalogosEstaticos(
            System.Data.IDbConnection connection,
            ParametrosGlobales parametros)
        {
            var catalogos = new FrmPreaEstudiov2CatalogosResponse();

            try
            {
                var lineas = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT codigo AS item, descripcion FROM catalogo WHERE Activo = 1 AND Retencion = 'N' ORDER BY descripcion"
                ).ToList();
                catalogos.lineas = lineas;
            }
            catch
            {
                catalogos.lineas = [];
            }

            catalogos.tipos_documento =
            [
                new() { item = "CK", descripcion = "Cheque" },
                new() { item = "TE", descripcion = "Transferencia" },
                new() { item = "TS", descripcion = "Transferencia SINPE" },
                new() { item = "ND", descripcion = "Nota Debito" },
            ];

            try
            {
                // VB6: sbCargarCombos -> cboTipoId desde AFI_TIPOS_IDS.
                var tiposId = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    @"SELECT CONVERT(varchar(10), TIPO_ID) AS item, RTRIM(Descripcion) AS descripcion
                      FROM AFI_TIPOS_IDS
                      ORDER BY TIPO_ID"
                ).ToList();
                catalogos.tipos_id = tiposId;
            }
            catch
            {
                catalogos.tipos_id = [];
            }

            try
            {
                // VB6: sbCargarCombos -> cboDivisa desde vSys_Divisas.
                var divisas = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    @"SELECT RTRIM(COD_DIVISA) AS item, RTRIM(DESCRIPCION) AS descripcion
                      FROM vSys_Divisas
                      ORDER BY DESCRIPCION"
                ).ToList();
                catalogos.divisas = divisas;
            }
            catch
            {
                catalogos.divisas = [];
            }

            try
            {
                var rawRows = connection.Query("EXEC spCRDPreaTIPO_SALARIO_TT");
                var tipos_salario = new List<FrmPreaEstudiov2DropdownDto>();
                foreach (var r in rawRows)
                {
                    var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);
                    tipos_salario.Add(new FrmPreaEstudiov2DropdownDto
                    {
                        item = GetString(dict, "TIPO_SALARIO"),
                        descripcion = GetString(dict, "DescTipoSalario"),
                        // Flag que indica si el campo Base (EXTRAS_FIJAS) es editable.
                        // VB6: frmPreaEstudiov2.frm línea 10466 — solo desbloquea si MODIFICA_EXTRAS_FIJAS = 1.
                        modifica_extras_fijas = GetBool(dict, "MODIFICA_EXTRAS_FIJAS"),
                    });
                }
                catalogos.tipos_salario = tipos_salario;
            }
            catch
            {
                catalogos.tipos_salario = [];
            }

            // Se resuelve desde el snapshot de CRD_PREA_PARAMETROS que ya trajo Cargar,
            // en lugar de repetir la consulta a la misma tabla.
            catalogos.componentes_adicionales = parametros.ComponentesAdicionales("18", "19", "20");

            try
            {
                var comites = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT id_comite AS item, descripcion FROM comites WHERE estado = 1 ORDER BY descripcion"
                ).ToList();
                catalogos.comites = comites;
            }
            catch
            {
                catalogos.comites = [];
            }

            try
            {
                // VB6: sbCargarCombos -> cboDeduccion (frmPreaEstudiov2.frm línea ~11432).
                var deducciones = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    @"SELECT ID_DEDUCCION AS item, DESCRIPCION AS descripcion
                      FROM CRD_PREA_V2_DEDUCCIONES_CONFIG
                      WHERE AUTOMATICA = 0
                      ORDER BY PRIORIDAD"
                ).ToList();
                catalogos.deducciones = deducciones;
            }
            catch
            {
                catalogos.deducciones = [];
            }

            try
            {
                // VB6: cboEtiquetas (frmPreaEstudiov2.frm línea ~11407).
                var etiquetas = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    @"SELECT COD_ETIQUETA AS item, DESCRIPCION AS descripcion
                      FROM CRD_PREA_V2_ETIQUETAS
                      WHERE SISTEMA = 0 AND MANEJO_ERRORES = 0"
                ).ToList();
                catalogos.etiquetas = etiquetas;
            }
            catch
            {
                catalogos.etiquetas = [];
            }

            try
            {
                // VB6: mTipoExtraLista (frmPreaEstudiov2.frm línea ~11412) — combo de gExtras.
                var tiposExtra = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    @"SELECT RTRIM(cod_Extras) AS item, RTRIM(cod_Extras) + ' - ' + RTRIM(descripcion) AS descripcion
                      FROM CRD_PREA_TIPOS_EXTRAS
                      ORDER BY cod_Extras"
                ).ToList();
                catalogos.tipos_extra = tiposExtra;
            }
            catch
            {
                catalogos.tipos_extra = [];
            }

            try
            {
                // VB6: cboFondo (Form_Load, línea ~11461-11462) -> EXEC spCRDGarantiaFND.
                // sbCbo_Llena_New (mProGrX_Dlls.bas) liga por nombre de columna (IdX/ItmX),
                // no posicional; el SP debe devolver esas columnas ya aliasadas.
                var rawRows = connection.Query("EXEC spCRDGarantiaFND");
                var fondos = new List<FrmPreaEstudiov2DropdownDto>();
                foreach (var r in rawRows)
                {
                    var dict = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);
                    fondos.Add(new FrmPreaEstudiov2DropdownDto
                    {
                        item = GetString(dict, "IdX"),
                        descripcion = GetString(dict, "ItmX"),
                    });
                }
                catalogos.fondos = fondos;
            }
            catch
            {
                catalogos.fondos = [];
            }

            try
            {
                // VB6: txtOficina_KeyDown (frmPreaEstudiov2.frm F4) -> select Cod_Oficina,
                // Descripcion from SIF_OFICINAS con Filtro " and Estado = 1", Orden =
                // Descripcion. Catálogo para el lookup del botón Cambiar.
                var oficinas = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    @"SELECT RTRIM(Cod_Oficina) AS item, RTRIM(Descripcion) AS descripcion
                      FROM SIF_OFICINAS
                      WHERE Estado = 1
                      ORDER BY Descripcion"
                ).ToList();
                catalogos.oficinas = oficinas;
            }
            catch
            {
                catalogos.oficinas = [];
            }

            try
            {
                // VB6: txtEjecutivo_KeyDown (frmPreaEstudiov2.frm F4) -> select ID_PROMOTOR
                // as 'Id.', Nombre, Usuario from promotores con Filtro " and Estado = 1",
                // Columna/Orden = ID_PROMOTOR. Catálogo para el lookup del botón Cambiar.
                var ejecutivos = connection.Query<FrmPreaEstudiov2EjecutivoDto>(
                    @"SELECT CAST(ID_PROMOTOR AS varchar(20)) AS id_promotor,
                             RTRIM(Nombre) AS nombre,
                             RTRIM(Usuario) AS usuario
                      FROM promotores
                      WHERE Estado = 1
                      ORDER BY ID_PROMOTOR"
                ).ToList();
                catalogos.ejecutivos = ejecutivos;
            }
            catch
            {
                catalogos.ejecutivos = [];
            }

            return catalogos;
        }

        /// <summary>
        /// Suma el avalúo Factor CFIA del expediente. Fiel a VB6 btnHipotecario_Click
        /// Case 1 "Avalúos CFIA" (frmPreaEstudiov2.frm línea ~13418):
        /// exec spCrdPreaSumarAvaluoCFIA '&lt;expediente&gt;', '&lt;usuario&gt;'.
        /// Reemplaza al endpoint Prea_frmPreaEstudiov2_Hipotecario_Obtener que llamaba
        /// a un SP inexistente en VB6 (spPrea_frmPreaEstudiov2_Hipotecario_Obtener) y
        /// que no tenía ningún consumidor en Angular.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_SumarAvaluoCfia(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioSumarAvaluoRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2HipotecarioResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCrdPreaSumarAvaluoCFIA @Expediente, @Usuario";
                connection.Execute(sql, new
                {
                    Expediente = request.cod_preanalisis.Trim(),
                    Usuario = (request.usuario ?? string.Empty).Trim()
                });

                var monto = connection.QueryFirstOrDefault<decimal?>(
                    "select MONTO_AVALUO_CFIA from CRD_PREA_PREANALISIS where cod_Preanalisis = @Expediente",
                    new { Expediente = request.cod_preanalisis.Trim() }
                ) ?? 0;

                result.Result = new FrmPreaEstudiov2HipotecarioResponse
                {
                    monto_avaluo_cfia = monto
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2HipotecarioResponse();
            }

            return result;
        }

        /// <summary>
        /// Cambia el estado hipotecario del expediente. Fiel a VB6 btnHipotecario_Click
        /// Case 4 "Cambio de Estado" (frmPreaEstudiov2.frm línea ~13466): valida que
        /// exista comité asignado (dbo.fxValidaAsignacionComite), valida el comité
        /// seleccionado (spCrdPrea_Comite_Asigna_Valida) y, solo si la garantía es
        /// Hipotecaria ('H'), ejecuta spCRDPreaEstadoHipotecarioAprob.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_CambiarEstado(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioCambiarEstadoRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2HipotecarioResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var exp = request.cod_preanalisis.Trim();

                var tieneComiteAsignado = connection.QueryFirstOrDefault<int?>(
                    "select dbo.fxValidaAsignacionComite(@Expediente) as Estado",
                    new { Expediente = exp }
                ) ?? 0;

                if (tieneComiteAsignado == 0)
                {
                    result.Code = -1;
                    result.Description = "Debe seleccionar un comité para poder continuar, favor validar.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(request.cod_comite))
                {
                    result.Code = -1;
                    result.Description = "Debe seleccionar un comité para poder continuar, favor validar.";
                    return result;
                }

                var codComite = request.cod_comite.Trim();
                var validacionRow = connection.QueryFirstOrDefault(
                    "exec spCrdPrea_Comite_Asigna_Valida @Expediente, @Comite",
                    new { Expediente = exp, Comite = codComite }
                ) as IDictionary<string, object>;

                var validacion = validacionRow is null
                    ? string.Empty
                    : GetString(new Dictionary<string, object>(validacionRow, StringComparer.OrdinalIgnoreCase), "Mensaje");

                if (!string.IsNullOrEmpty(validacion))
                {
                    result.Code = -1;
                    result.Description = validacion;
                    return result;
                }

                if ((request.cod_garantia ?? string.Empty).Trim() == "H")
                {
                    const string sql = "exec spCRDPreaEstadoHipotecarioAprob @Expediente, @Usuario, 0, ''";
                    connection.Execute(sql, new
                    {
                        Expediente = exp,
                        Usuario = (request.usuario ?? string.Empty).Trim()
                    });
                }

                result.Result = new FrmPreaEstudiov2HipotecarioResponse
                {
                    mensaje = "Estado actualizado correctamente."
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2HipotecarioResponse();
            }

            return result;
        }

        /// <summary>
        /// Cambia el expediente a estado abandonado.
        /// VB6: btnAbandonar_Click (frmPreaEstudiov2.frm línea ~12507).
        /// Validaciones defensivas replicadas del VB6:
        ///   1. Estado "D" (Descartado) → no se puede abandonar
        ///   2. Estado "B" (Abandonado) → ya fue abandonado anteriormente
        ///   3. Estado "A" + formalización válida → no se puede abandonar
        ///   4. Sub-expediente → no se puede abandonar
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2AbandonarResponse> Prea_frmPreaEstudiov2_Abandonar(
            int codEmpresa,
            FrmPreaEstudiov2AbandonarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2AbandonarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2AbandonarResponse()
            };

            var codPreanalisis = (request.cod_preanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(codPreanalisis))
            {
                response.Code = -1;
                response.Description = "Debe indicar el expediente.";
                return response;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                // Validación 1: Verificar si es sub-expediente
                if (EsSubExpediente(codPreanalisis))
                {
                    response.Code = -1;
                    response.Description = "No se puede ABANDONAR un expediente secundario, por favor seleccione el expediente principal e intente de nuevo.";
                    return response;
                }

                // Obtener estado actual para validaciones
                var estadoActual = ObtenerEstadoExpediente(connection, codPreanalisis);

                if (string.IsNullOrEmpty(estadoActual))
                {
                    response.Code = -1;
                    response.Description = "No se encontró el expediente especificado.";
                    return response;
                }

                // Validación 2: Estado "D" (Descartado)
                if (string.Equals(estadoActual, "D", StringComparison.OrdinalIgnoreCase))
                {
                    response.Code = -1;
                    response.Description = "No se puede ABANDONAR un expediente que ya ha sido DESCARTADO.";
                    return response;
                }

                // Validación 3: Estado "B" (Abandonado)
                if (string.Equals(estadoActual, "B", StringComparison.OrdinalIgnoreCase))
                {
                    response.Code = -1;
                    response.Description = "Ya este estudio ha sido ABANDONADO anteriormente, no se puede realizar la accin nuevamente.";
                    return response;
                }

                // Validación 4: VB6 solo bloquea estado "A" cuando fxValidaFormalizacion = True.
                if (
                    string.Equals(estadoActual, "A", StringComparison.OrdinalIgnoreCase) &&
                    EstaFormalizado(connection, codPreanalisis)
                )
                {
                    response.Code = -1;
                    response.Description = "No se puede ABANDONAR un expediente que ya ha sido FORMALIZADO.";
                    return response;
                }

                const string sql = @"EXEC spCrdPreaCambiaEstadoPreanalisis @cod_preanalisis, @estado";

                connection.Execute(
                    sql,
                    new
                    {
                        cod_preanalisis = codPreanalisis,
                        estado = "B"
                    },
                    commandType: CommandType.Text
                );

                response.Result = new FrmPreaEstudiov2AbandonarResponse
                {
                    cod_preanalisis = codPreanalisis
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2AbandonarResponse();
                return response;
            }
        }

        /// <summary>
        /// Consulta las causas de seguimiento del expediente.
        /// Obtiene la lista de causas registradas para un tipo específico (Denegados/Pendientes).
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2CausaDto>> Prea_frmPreaEstudiov2_Causas_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            var result = new ErrorDto<List<FrmPreaEstudiov2CausaDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"
                    SELECT Pa.COD_CAUSAS AS cod_causas,
                           Cg.DESCRIPCION AS descripcion,
                           Pa.REGISTRO_FECHA AS registro_fecha,
                           Pa.REGISTRO_USUARIO AS registro_usuario,
                           Pa.TIPO AS tipo
                    FROM CRD_PREA_GESTION Pa
                    INNER JOIN OPERACION_CAUSAS Cg
                        ON Pa.COD_CAUSAS = Cg.COD_CAUSAS AND Pa.TIPO = Cg.TIPO
                    WHERE Pa.COD_PREANALISIS = @cod_preanalisis
                      AND Pa.TIPO = @tipo
                    ORDER BY Pa.REGISTRO_FECHA";

                var parameters = new DynamicParameters();
                parameters.Add("@cod_preanalisis", cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@tipo", tipo.Trim(), DbType.String);

                result.Result = connection.Query<FrmPreaEstudiov2CausaDto>(
                    sql,
                    parameters,
                    commandType: CommandType.Text
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }

            return result;
        }

    }
}
