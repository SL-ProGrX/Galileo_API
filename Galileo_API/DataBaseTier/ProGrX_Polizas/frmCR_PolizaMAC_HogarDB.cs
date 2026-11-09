using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaMacHogarDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;

        /// <summary>
        /// Caché corta del corte: spPoliza_Incendio_Cierre tarda ~11 s, por lo que se reutiliza el
        /// resultado de una consulta idéntica (empresa, póliza, mes de corte, beneficiarios y movimiento).
        /// </summary>
        private static readonly MemoryCache _cacheCorte = new(new MemoryCacheOptions());
        private static readonly TimeSpan _duracionCacheCorte = TimeSpan.FromMinutes(3);

        public FrmCrPolizaMacHogarDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Método para obtener la lista de pólizas MAC Hogar.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista para llenar el combo de pólizas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizaMacHogar_Polizas_Lista(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var lista = new List<DropDownListaGenericaModel>();

                using (var reader = conn.ExecuteReader(
                    "spPolizas_MAC_Incendio_List",
                    commandType: CommandType.StoredProcedure))
                {
                    var itemOrdinal = reader.GetOrdinal("IdX");
                    var descripcionOrdinal = reader.GetOrdinal("ItmX");

                    while (reader.Read())
                    {
                        lista.Add(new DropDownListaGenericaModel
                        {
                            item = reader.IsDBNull(itemOrdinal)
                                ? null
                                : reader.GetValue(itemOrdinal),
                            descripcion = !reader.IsDBNull(descripcionOrdinal)
                                ? Convert.ToString(reader.GetValue(descripcionOrdinal))?.Trim() ?? string.Empty
                                : string.Empty
                        });
                    }
                }

                return lista;
            });
        }

        /// <summary>
        /// Método para obtener la fecha del servidor.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Fecha actual del servidor.</returns>
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _proGrxMain.fxFechaServidor(codEmpresa, 0);
        }

        #region Envio

        /// <summary>
        /// Método para consultar el corte del tab Envío.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <param name="request">Póliza, corte, beneficiarios y movimiento.</param>
        /// <returns>Datos del corte en el orden original de MAC Hogar.</returns>
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Envio_Consulta(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            if (codEmpresa <= 0)
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Empresa inválida.");

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Usuario inválido.");

            if (request == null)
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Datos inválidos.");

            if (string.IsNullOrWhiteSpace(request.Poliza))
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("La póliza es requerida.");

            if (string.IsNullOrWhiteSpace(request.TipoMovimiento))
                request.TipoMovimiento = "T";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Poliza", request.Poliza, DbType.String, size: 10);
                parameters.Add("@Corte", request.Corte, DbType.DateTime);
                parameters.Add("@Beneficiarios", request.Beneficiarios, DbType.Int16);
                parameters.Add("@Usuario", usuario, DbType.String, size: 30);
                parameters.Add("@Movimiento", request.TipoMovimiento, DbType.String, size: 5);

                return ConsultarCorte(codEmpresa, conn, parameters);
            });
        }

        #endregion

        #region Consulta
        /// <summary>
        /// Método para consultar información del tab Consultas.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <param name="request">Póliza, corte y tipo de movimiento.</param>
        /// <returns>Datos del corte en el orden original de MAC Hogar.</returns>
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Consulta_Obtener(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Request inválido.");
            }

            if (string.IsNullOrWhiteSpace(request.Poliza))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Debe seleccionar la póliza.");
            }

            var movimiento = (request.TipoMovimiento ?? "T").Trim().ToUpperInvariant();
            var movimientosValidos = new HashSet<string> { "T", "I", "E", "M", "SC" };

            if (!movimientosValidos.Contains(movimiento))
            {
                return DbHelper.CreateErrorResponse<List<CrPolizaMacHogarEnvioRow>>("Movimiento inválido.");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Poliza", request.Poliza, DbType.String, size: 10);
                parameters.Add("@Corte", request.Corte, DbType.DateTime);
                parameters.Add("@Beneficiarios", 1, DbType.Int16);
                parameters.Add("@Usuario", usuario, DbType.String, size: 30);
                parameters.Add("@Movimiento", movimiento, DbType.String, size: 5);

                return ConsultarCorte(codEmpresa, conn, parameters);
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento y conserva el orden fijo de las 77 columnas
        /// definido por el vGrid del formulario original frmCR_PolizaMAC_Hogar.
        /// </summary>
        /// <param name="conn">Conexión abierta por DbHelper.</param>
        /// <param name="parameters">Parámetros de spPoliza_Incendio_Cierre.</param>
        /// <returns>Filas normalizadas para Envío y Consultas.</returns>
        private static List<CrPolizaMacHogarEnvioRow> ConsultarCorte(
            int codEmpresa,
            IDbConnection conn,
            DynamicParameters parameters)
        {
            var llaveCache = ConstruirLlaveCache(codEmpresa, parameters);

            if (_cacheCorte.TryGetValue(llaveCache, out List<CrPolizaMacHogarEnvioRow>? enCache)
                && enCache is not null)
            {
                return enCache;
            }

            var data = EjecutarCorte(conn, parameters);

            _cacheCorte.Set(llaveCache, data, _duracionCacheCorte);

            return data;
        }

        /// <summary>
        /// Llave de caché por empresa, póliza, mes del corte, beneficiarios y movimiento.
        /// El SP trabaja por mes (fxSIFDateTimeToProceso), por eso se usa año-mes del corte.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="parameters">Parámetros enviados al SP.</param>
        /// <returns>Llave única de la consulta.</returns>
        private static string ConstruirLlaveCache(int codEmpresa, DynamicParameters parameters)
        {
            var poliza = Convert.ToString(parameters.Get<object>("@Poliza"));
            var corte = Convert.ToDateTime(parameters.Get<object>("@Corte"));
            var beneficiarios = Convert.ToInt16(parameters.Get<object>("@Beneficiarios"));
            var movimiento = Convert.ToString(parameters.Get<object>("@Movimiento"));

            return $"{codEmpresa}|{poliza}|{corte:yyyyMM}|{beneficiarios}|{movimiento}";
        }

        private static List<CrPolizaMacHogarEnvioRow> EjecutarCorte(
            IDbConnection conn,
            DynamicParameters parameters)
        {
            using var reader = conn.ExecuteReader(
                "spPoliza_Incendio_Cierre",
                parameters,
                commandType: CommandType.StoredProcedure,
                // El SP tarda ~11 s (lee vPoliza_Incendio_Cierre varias veces); el valor por defecto (30 s) falla en horas de carga.
                commandTimeout: 300);

            var data = new List<CrPolizaMacHogarEnvioRow>();

            while (reader.Read())
            {
                data.Add(MapearFilaCorte(reader));
            }

            return data;
        }

        /// <summary>
        /// Mapea por ordinal porque el procedimiento usa encabezados heredados
        /// y el contrato visual original depende del orden de las columnas.
        /// </summary>
        /// <param name="row">Registro devuelto por spPoliza_Incendio_Cierre.</param>
        /// <returns>Fila de MAC Hogar con las 77 columnas.</returns>
        private static CrPolizaMacHogarEnvioRow MapearFilaCorte(IDataRecord row)
        {
            return new CrPolizaMacHogarEnvioRow
            {
                corte = ObtenerValor(row, 0),
                direccion_riesgo = ObtenerValor(row, 1),
                provincia_riesgo = ObtenerValor(row, 2),
                canton_riesgo = ObtenerValor(row, 3),
                distrito_riesgo = ObtenerValor(row, 4),
                folio_real = ObtenerValor(row, 5),
                monto_residencia = ObtenerValor(row, 6),
                monto_otras_estructuras = ObtenerValor(row, 7),
                monto_familiar = ObtenerValor(row, 8),
                cob_d = ObtenerValor(row, 9),
                cob_x = ObtenerValor(row, 10),
                cob_y = ObtenerValor(row, 11),
                correo_electronico = ObtenerValor(row, 12),
                incluir_cobertura = ObtenerValor(row, 13),
                latitud = ObtenerValor(row, 14),
                longitud = ObtenerValor(row, 15),
                pisos = ObtenerValor(row, 16),
                tipo_id = ObtenerValor(row, 17),
                identificacion = ObtenerValor(row, 18),
                primer_apellido = ObtenerValor(row, 19),
                segundo_apellido = ObtenerValor(row, 20),
                primer_nombre = ObtenerValor(row, 21),
                segundo_nombre = ObtenerValor(row, 22),
                direccion_completa = ObtenerValor(row, 23),
                fecha_nacimiento = ObtenerValor(row, 24),
                genero = ObtenerValor(row, 25),
                telefono = ObtenerValor(row, 26),
                correo_persona = ObtenerValor(row, 27),
                pais_buc_num = ObtenerValor(row, 28),
                anos_construccion = ObtenerValor(row, 29),
                bien_asegurado = ObtenerValor(row, 30),
                es_esquina = ObtenerValor(row, 31),
                tipo_ocupacion = ObtenerValor(row, 32),
                tipo_construccion = ObtenerValor(row, 33),
                cob_v = ObtenerValor(row, 34),
                no_aplica = ObtenerValor(row, 35),
                aseguramiento_m = ObtenerValor(row, 36),
                aseguramiento_p1 = ObtenerValor(row, 37),
                aseguramiento_p2 = ObtenerValor(row, 38),
                aseguramiento_p3 = ObtenerValor(row, 39),
                aseguramiento_x = ObtenerValor(row, 40),
                aseguramiento_y = ObtenerValor(row, 41),
                tarifa_cob_d = ObtenerValor(row, 42),
                tarifa_cob_v = ObtenerValor(row, 43),
                tarifa_cob_y = ObtenerValor(row, 44),
                monto_propiedad_personal = ObtenerValor(row, 45),
                solvencia = ObtenerValor(row, 46),
                poliza_migrada = ObtenerValor(row, 47),
                numero_referencia = ObtenerValor(row, 48),
                agente = ObtenerValor(row, 49),
                nombre_completo = ObtenerValor(row, 50),
                sucursal_emision = ObtenerValor(row, 51),
                numero_contrato = ObtenerValor(row, 52),
                forma_pago = ObtenerValor(row, 53),
                base_aseguramiento = ObtenerValor(row, 54),
                descripcion = ObtenerValor(row, 55),
                agrupador_riesgo = ObtenerValor(row, 56),
                cedula_intermediario = ObtenerValor(row, 57),
                observaciones_poliza = ObtenerValor(row, 58),
                numero_poliza = ObtenerValor(row, 59),
                razon_cancelacion = ObtenerValor(row, 60),
                migracion_cb = ObtenerValor(row, 61),
                mco = ObtenerValor(row, 62),
                sys = ObtenerValor(row, 63),
                poliza = ObtenerValor(row, 64),
                mod = ObtenerValor(row, 65),
                tipo_variacion_incendio = ObtenerValor(row, 66),
                fecha_formalizacion = ObtenerValor(row, 67),
                tipo_identificacion = ObtenerValor(row, 68),
                identificacion_acreedor = ObtenerValor(row, 69),
                nombre_acreedor = ObtenerValor(row, 70),
                monto = ObtenerValor(row, 71),
                rubro_acreencia = ObtenerValor(row, 72),
                observaciones = ObtenerValor(row, 73),
                movimiento = ObtenerValor(row, 74),
                nacionalidad_descripcion = ObtenerValor(row, 75),
                nacionalidad_codigo_alterno = ObtenerValor(row, 76)
            };
        }

        /// <summary>
        /// Obtiene un valor sin fallar cuando una instalación antigua devuelve
        /// menos columnas o un valor SQL NULL.
        /// </summary>
        /// <param name="row">Registro devuelto por SQL Server.</param>
        /// <param name="ordinal">Posición original de la columna.</param>
        /// <returns>Valor de la columna o null.</returns>
        private static object? ObtenerValor(IDataRecord row, int ordinal)
        {
            return ordinal < row.FieldCount && !row.IsDBNull(ordinal)
                ? row.GetValue(ordinal)
                : null;
        }

        #endregion

        #region Beneficiarios
        /// <summary>
        /// Método para obtener la lista de beneficiarios de MAC Hogar.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <returns>Beneficiarios asociados al tipo PC, igual que en ProGrX.</returns>
        public ErrorDto<List<CrPolizaMacHogarBeneficiariosRowDto>> Cr_PolizaMacHogar_Beneficiarios_Lista(
            int codEmpresa,
            string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"
                    EXEC dbo.spPoliza_Beneficiarios_Lista @TipoPoliza";

                var data = conn.Query<CrPolizaMacHogarBeneficiariosRowDto>(
                    sql,
                    new { TipoPoliza = "PC" }).ToList();
                return data;
            });
        }
        #endregion
    }
}
