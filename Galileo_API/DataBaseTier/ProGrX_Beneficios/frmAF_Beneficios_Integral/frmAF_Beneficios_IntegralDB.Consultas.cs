using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralDB
    {
        /// <summary>
        /// Obtiene la bitácora de un beneficio seleccionado.
        /// </summary>
        public ErrorDto<List<BitacoraBeneficioIntegralDto>> BitacoraBeneficioIntegral_Obtener(int CodEmpresa, string Cod_Beneficio, int Consec)
        {
            const string sql = @"
                SELECT ID_BITACORA, CONSEC, REGISTRO_FECHA, COD_BENEFICIO, REGISTRO_USUARIO, DETALLE, MOVIMIENTO
                FROM AFI_BENE_REGISTRO_BITACORA
                WHERE COD_BENEFICIO = @codBeneficio AND CONSEC = @consec
                ORDER BY 1 ASC";

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<BitacoraBeneficioIntegralDto>(sql, new { codBeneficio = Cod_Beneficio, consec = Consec }).ToList());

            return new ErrorDto<List<BitacoraBeneficioIntegralDto>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BitacoraBeneficioIntegral_Obtener: " + result.Description,
                Result = result.Result ?? new List<BitacoraBeneficioIntegralDto>()
            };
        }

        /// <summary>
        /// Obtiene el expediente del beneficio como un diccionario de tablas serializadas en JSON.
        /// </summary>
        public ErrorDto<object> BeneIntegralRepExpediente_Obtener(int CodEmpresa, string cedula, int id_beneficio, string categoria)
        {
            var response = new ErrorDto<object>();
            using var ds = new DataSet();
            var tablesAsJson = new Dictionary<string, object>();

            try
            {
                var connString = CreatePortalDb().ObtenerDbConnStringEmpresa(CodEmpresa);

                using (var connection = new SqlConnection(connString))
                using (var command = new SqlCommand("spAFI_Bene_ExpExpediente_Consulta", connection) { CommandType = CommandType.StoredProcedure })
                {
                    command.Parameters.Add(new SqlParameter("@cedula", cedula));
                    command.Parameters.Add(new SqlParameter("@id_beneficio", id_beneficio));
                    command.Parameters.Add(new SqlParameter("@categoria", categoria));

                    using var da = new SqlDataAdapter(command);
                    da.Fill(ds);
                }

                foreach (DataTable table in ds.Tables)
                {
                    tablesAsJson[table.TableName] = JsonConvert.SerializeObject(table);
                }

                response.Code = 0;
                response.Result = tablesAsJson;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = null;
                response.Description = "BeneIntegralRepExpediente_Obtener: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene la lista de beneficios para aprobación masiva, marcando validaciones por persona.
        /// </summary>
        public ErrorDto<BeneConsultaDatosLista> BeneficiosParaAprobacionMasiva_Obtener(int CodEmpresa, string Categoria, string filtroString)
        {
            var filtros = JsonConvert.DeserializeObject<AfiBeneFiltros>(filtroString) ?? new AfiBeneFiltros();

            var p = ConstruirParametrosMasiva(Categoria, filtros);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var datos = new BeneConsultaDatosLista
                {
                    total = connection.QueryFirstOrDefault<int>(SqlMasivaCount, p),
                    lista = connection.Query<BeneConsultaDatos>(SqlMasivaLista, p).ToList()
                };

                MarcarValidaciones(CodEmpresa, datos.lista);
                return datos;
            });

            return new ErrorDto<BeneConsultaDatosLista>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new BeneConsultaDatosLista()
            };
        }

        /// <summary>
        /// Arma los parámetros de la consulta de aprobación masiva (filtros condicionales por parámetro).
        /// </summary>
        private static DynamicParameters ConstruirParametrosMasiva(string categoria, AfiBeneFiltros filtros)
        {
            string? filtroLike = string.IsNullOrEmpty(filtros.filtro) ? null : $"%{filtros.filtro}%";
            string? codGrupo = (filtros.cod_grupo != null && filtros.cod_grupo != "TODOS") ? filtros.cod_grupo : null;
            var tienePagina = filtros.pagina != null;

            var p = new DynamicParameters();
            p.Add("@catLike", $"%{categoria}%");
            p.Add("@filtroLike", filtroLike);
            p.Add("@codGrupo", codGrupo);
            p.Add("@offset", filtros.pagina ?? 0);
            p.Add("@fetch", tienePagina ? (filtros.paginacion ?? 30) : int.MaxValue);
            return p;
        }

        // Comando SQL constante y parametrizado (evita SQL dinámico / S2077). Los filtros se aplican
        // como condiciones "@param IS NULL OR ..." para no interpolar texto en el comando.
        private const string SqlMasivaBase = @"
            FROM vBeneficios_W_Integral O
            LEFT JOIN AFI_BENE_ESTADOS E ON E.COD_ESTADO = O.ESTADO AND E.COD_ESTADO IN (
                SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                    SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA LIKE @catLike))
            WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA LIKE @catLike)
              AND E.P_FINALIZA = 1 AND E.PROCESO = 'T' AND E.ACTIVO = '1'
              AND (@filtroLike IS NULL OR (Expediente LIKE @filtroLike OR O.cedula LIKE @filtroLike OR O.NOMBRE_BENEFICIARIO LIKE @filtroLike))
              AND (@codGrupo IS NULL OR O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_GRUPO = @codGrupo))";

        private const string SqlMasivaCount = "SELECT COUNT(O.ID_BENEFICIO) " + SqlMasivaBase;

        private const string SqlMasivaLista =
            @"SELECT
                CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO), RIGHT(CONCAT('00000', O.CONSEC), 5)) AS Expediente,
                O.REGISTRA_FECHA, O.AUTORIZA_FECHA, O.ID_BENEFICIO, O.CONSEC, O.COD_BENEFICIO, O.Beneficio_Desc,
                O.MONTO, O.MONTO_APLICADO, O.ESTADO, ISNULL(E.DESCRIPCION, 'SIN DEFINIR') AS estado_desc,
                O.cedula, O.NOMBRE_BENEFICIARIO, O.registra_user, Categoria_Desc, Estado_Persona, O.TIPO,
                CASE WHEN O.TIPO = 'M' THEN 'Monetario' WHEN O.TIPO = 'P' THEN 'Producto' ELSE 'Ambos' END AS TipoDesc "
            + SqlMasivaBase
            + @" ORDER BY O.REGISTRA_FECHA DESC OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

        /// <summary>
        /// Obtiene la lista de beneficios para control mensual (saldo pendiente por pagar).
        /// </summary>
        public ErrorDto<BeneConsultaDatosLista> BeneficiosControMensual_Obtener(int CodEmpresa, string Categoria, string filtroString)
        {
            var filtros = JsonConvert.DeserializeObject<AfiBeneFiltros>(filtroString) ?? new AfiBeneFiltros();

            var tienePagina = filtros.pagina != null;

            var p = new DynamicParameters();
            p.Add("@categoria", Categoria);
            p.Add("@filtroLike", string.IsNullOrEmpty(filtros.filtro) ? null : $"%{filtros.filtro}%");
            p.Add("@offset", filtros.pagina ?? 0);
            p.Add("@fetch", tienePagina ? (filtros.paginacion ?? 30) : int.MaxValue);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new BeneConsultaDatosLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlControMensualCount, p),
                lista = connection.Query<BeneConsultaDatos>(SqlControMensualLista, p).ToList()
            });

            return new ErrorDto<BeneConsultaDatosLista>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new BeneConsultaDatosLista()
            };
        }

        // Comando SQL constante y parametrizado (evita SQL dinámico / S2077).
        private const string SqlControMensualBase = @"
            FROM vBeneficios_W_Integral O
            LEFT JOIN AFI_BENE_ESTADOS E ON E.COD_ESTADO = O.ESTADO AND E.COD_ESTADO IN (
                SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                    SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = @categoria))
            LEFT JOIN AFI_BENE_OTORGA OB ON O.ID_BENEFICIO = OB.ID_BENEFICIO AND OB.aplica_pago_masivo = 1
            WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = @categoria)
              AND E.P_FINALIZA = 1 AND E.PROCESO = 'A' AND OB.ID_BENEFICIO IS NOT NULL
              AND O.MONTO_APLICADO <> (SELECT COALESCE(SUM(P.MONTO), 0) FROM AFI_BENE_PAGO P
                                        WHERE P.COD_BENEFICIO = O.COD_BENEFICIO AND P.CONSEC = O.CONSEC)";

        private const string SqlControMensualCount = "SELECT COUNT(*) " + SqlControMensualBase;

        private const string SqlControMensualLista =
            @"SELECT
                CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO), RIGHT(CONCAT('00000', O.CONSEC), 5)) AS Expediente,
                O.REGISTRA_FECHA, O.AUTORIZA_FECHA, O.ID_BENEFICIO, O.CONSEC, O.COD_BENEFICIO, O.Beneficio_Desc, O.MONTO,
                O.MONTO_APLICADO - COALESCE((SELECT SUM(P.MONTO) FROM AFI_BENE_PAGO P
                    WHERE P.COD_BENEFICIO = O.COD_BENEFICIO AND P.CONSEC = O.CONSEC), 0) AS MONTO_APLICADO,
                O.ESTADO, COALESCE(E.DESCRIPCION, 'SIN DEFINIR') AS estado_desc, O.CEDULA, O.NOMBRE_BENEFICIARIO,
                O.REGISTRA_USER, O.Categoria_Desc, O.Estado_Persona, O.TIPO,
                CASE WHEN O.TIPO = 'M' THEN 'Monetario' WHEN O.TIPO = 'P' THEN 'Producto' ELSE 'Ambos' END AS TipoDesc "
            + SqlControMensualBase
            + @" AND (@filtroLike IS NULL OR (Expediente LIKE @filtroLike OR O.cedula LIKE @filtroLike OR O.NOMBRE_BENEFICIARIO LIKE @filtroLike))
                 ORDER BY O.REGISTRA_FECHA DESC OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

        /// <summary>
        /// Reporte de control mensual: pagos registrados por categoría y periodo/mes.
        /// </summary>
        public ErrorDto<BeneConsultaDatosLista> BeneficiosControMensual_Reporte(int CodEmpresa, string Categoria, string filtroString)
        {
            var filtros = JsonConvert.DeserializeObject<AfiBeneFiltros>(filtroString) ?? new AfiBeneFiltros();

            string? codGrupo = (filtros.cod_grupo != null && filtros.cod_grupo != "TODOS") ? filtros.cod_grupo : null;

            var p = new DynamicParameters();
            p.Add("@catLike", $"%{Categoria}%");
            p.Add("@codGrupo", codGrupo);
            p.Add("@periodo", filtros.periodo);
            p.Add("@mes", filtros.mes);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new BeneConsultaDatosLista
            {
                total = 0,
                lista = connection.Query<BeneConsultaDatos>(SqlControMensualReporte, p).ToList()
            });

            return new ErrorDto<BeneConsultaDatosLista>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new BeneConsultaDatosLista()
            };
        }

        // Comando SQL constante y parametrizado (evita SQL dinámico / S2077).
        private const string SqlControMensualReporte = @"
            SELECT
                CONCAT(RIGHT(CONCAT('00000', O.ID_BENEFICIO), 5), TRIM(O.COD_BENEFICIO), RIGHT(CONCAT('00000', O.CONSEC), 5)) AS Expediente,
                P.cedula, O.Nombre_beneficiario, O.Beneficio_Desc, P.monto, O.Categoria_Desc,
                P.REGISTRO_USUARIO AS registra_user, P.REGISTRO_FECHA AS registra_fecha, P.ESTADO, P.ID_PAGO, P.COD_REMESA
            FROM AFI_BENE_PAGO P
            LEFT JOIN vBeneficios_W_Integral O ON P.CEDULA = O.CEDULA AND P.COD_BENEFICIO = O.COD_BENEFICIO AND P.CONSEC = O.CONSEC
            WHERE O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA LIKE @catLike)
              AND (@codGrupo IS NULL OR O.COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_GRUPO = @codGrupo))
              AND (@periodo IS NULL OR YEAR(P.REGISTRO_FECHA) = @periodo)
              AND (@mes IS NULL OR MONTH(P.REGISTRO_FECHA) = @mes)
            ORDER BY P.REGISTRO_FECHA ASC";

        /// <summary>
        /// Marca en cada registro el mensaje de validación de persona/beneficio (si aplica).
        /// </summary>
        private void MarcarValidaciones(int CodEmpresa, List<BeneConsultaDatos> lista)
        {
            foreach (var item in lista)
            {
                var validacion = _mBeneficiosDB.ValidarPersona(CodEmpresa, item.cedula, item.cod_beneficio);
                if (validacion.Code == -1)
                {
                    item.valida_beneficio = validacion.Description;
                }
            }
        }
    }
}
