using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralConDB
    {
        /// <summary>
        /// Obtiene la lista de beneficios registrados en la consulta general aplicando los filtros recibidos.
        /// </summary>
        /// <param name="Jfiltro">Filtros de la consulta serializados en JSON (BeneConsultaFiltros).</param>
        /// <returns>Total de registros y lista paginada de beneficios.</returns>
        public ErrorDto<BeneConsultaDatosLista> BeneConsultasLista_Obtener(string Jfiltro)
        {
            var filtro = JsonConvert.DeserializeObject<BeneConsultaFiltros>(Jfiltro) ?? new BeneConsultaFiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), filtro.codCliente, connection =>
            {
                var p = ConstruirParametrosConsulta(filtro);

                var datos = new BeneConsultaDatosLista
                {
                    total = connection.Query<int>(SqlConsultaCount, p).FirstOrDefault(),
                    lista = connection.Query<BeneConsultaDatos>(SqlConsultaLista, p).ToList()
                };

                if (NormalizarTexto(filtro.categoria) == "B_BECA")
                {
                    datos.lista.AddRange(connection.Query<BeneConsultaDatos>(SqlConsultaBecas, p).ToList());
                }

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
        /// Obtiene los estados configurados para el beneficio según la categoría (Configuración de Grupos).
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="categoria">Categoría del beneficio, o "T" para todas.</param>
        /// <returns>Lista de estados con la opción "TODOS" al inicio.</returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneConsultaEstados_Obtener(int CodCliente, string categoria)
        {
            var cat = NormalizarTexto(categoria);

            var sql = cat != "T"
                ? @"SELECT COD_ESTADO AS item, descripcion
                      FROM AFI_BENE_ESTADOS
                     WHERE COD_ESTADO IN (
                           SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                                 SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = @categoria))
                     ORDER BY ORDEN ASC"
                : @"SELECT COD_ESTADO AS item, descripcion FROM AFI_BENE_ESTADOS ORDER BY ORDEN ASC";

            var result = DbHelper.ExecuteListQuery<AfBeneficioIntegralDropsLista>(
                CreatePortalDb(), CodCliente, sql, new { categoria = cat });

            var lista = result.Result ?? new List<AfBeneficioIntegralDropsLista>();
            if (result.Code == 0)
            {
                lista.Insert(0, new AfBeneficioIntegralDropsLista { item = "T", descripcion = "TODOS" });
            }

            return new ErrorDto<List<AfBeneficioIntegralDropsLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = lista
            };
        }

        /// <summary>
        /// Obtiene la información del beneficio seleccionado en la consulta general.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="beneficio">Identificador del beneficio (ID_BENEFICIO).</param>
        /// <returns>Datos del beneficio.</returns>
        public ErrorDto<BeneficioDto> BeneficioIntegral_Obtener(int CodCliente, long beneficio)
        {
            const string sql = @"SELECT * FROM vBeneficios_Integral WHERE ID_BENEFICIO = @beneficio";

            var result = DbHelper.ExecuteSingleQuery<BeneficioDto>(
                CreatePortalDb(), CodCliente, sql, null, new { beneficio });

            return new ErrorDto<BeneficioDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }

        // ==================== Armado de parámetros (sin concatenar SQL) ====================

        /// <summary>
        /// Construye los parámetros de la consulta general. Los filtros no aplicados se envían como NULL,
        /// de modo que el SQL constante los descarta con la condición (@param IS NULL OR ...).
        /// </summary>
        private static DynamicParameters ConstruirParametrosConsulta(BeneConsultaFiltros filtro)
        {
            var p = new DynamicParameters();

            var cat = NormalizarTexto(filtro.categoria);
            p.Add("@categoriaLike", (cat == "T" || cat.Length == 0) ? null : $"%{cat}%");

            AgregarParametrosFecha(filtro, p);

            var estado = NormalizarTexto(filtro.estado);
            p.Add("@estadoLike", estado == "T" ? null : $"%{estado}%");
            p.Add("@cedulaLike", EsCedulaVacia(filtro.cedula) ? null : $"%{filtro.cedula!.Trim()}%");
            p.Add("@expLike", filtro.noExpediente != null ? $"%{filtro.noExpediente}%" : null);
            p.Add("@usuarioLike", filtro.usuario != null ? $"%{filtro.usuario.Trim().ToUpper()}%" : null);
            p.Add("@filtroLike", string.IsNullOrEmpty(filtro.filtro) ? null : $"%{filtro.filtro}%");

            AgregarParametrosPaginacion(filtro, p);
            return p;
        }

        /// <summary>
        /// Agrega los parámetros del rango de fechas. Solo se activa cuando todasFechas es false y hay tipo de fecha.
        /// </summary>
        private static void AgregarParametrosFecha(BeneConsultaFiltros filtro, DynamicParameters p)
        {
            var aplicaFecha = filtro.todasFechas == false && filtro.tipoFecha != null;
            p.Add("@todasFechas", aplicaFecha ? 0 : 1);
            p.Add("@tipoFecha", aplicaFecha ? filtro.tipoFecha : null);

            if (!aplicaFecha)
            {
                p.Add("@fechaIni", null);
                p.Add("@fechaFin", null);
                return;
            }

            var fechaIni = DateTimeOffset
                .Parse(filtro.fechaInicio!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            var fechaFin = DateTimeOffset
                .Parse(filtro.fechaCorte!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            p.Add("@fechaIni", $"{fechaIni} 00:00:00");
            p.Add("@fechaFin", $"{fechaFin} 23:59:59");
        }

        /// <summary>
        /// Agrega los parámetros de paginación. Sin página se recorre el conjunto (offset 0, fetch máximo).
        /// </summary>
        private static void AgregarParametrosPaginacion(BeneConsultaFiltros filtro, DynamicParameters p)
        {
            p.Add("@offset", filtro.pagina ?? 0);
            p.Add("@fetch", filtro.pagina == null ? int.MaxValue : (filtro.paginacion ?? 0));
        }

        /// <summary>
        /// Indica si el valor de cédula debe tratarse como vacío (null, espacio, "0" o "").
        /// </summary>
        private static bool EsCedulaVacia(string? cedula)
            => cedula is null || cedula == " " || cedula == "0" || cedula == string.Empty;

        // ==================== Cuerpos SQL constantes (sin datos de usuario) ====================

        /// <summary>Subconsulta interna usada para el conteo total de la consulta general.</summary>
        private const string InnerConteoSql = @"
            SELECT CONCAT(RIGHT(CONCAT('00000', H.ID_BENEFICIO), 5),
                          TRIM(H.COD_BENEFICIO),
                          RIGHT(CONCAT('00000', H.CONSEC), 5)) AS Expediente,
                   COD_BENEFICIO, registra_user, cedula, Estado,
                   Registra_Fecha, Autoriza_Fecha, Pago_Fecha
            FROM vBeneficios_W_Integral H
            LEFT JOIN AFI_BENE_ESTADOS E
                   ON E.COD_ESTADO = H.ESTADO
                  AND E.COD_ESTADO IN (
                        SELECT COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS WHERE COD_GRUPO IN (
                              SELECT COD_GRUPO FROM AFI_BENE_GRUPOS
                               WHERE (@categoriaLike IS NULL OR COD_CATEGORIA LIKE @categoriaLike)))";

        /// <summary>Subconsulta interna con el detalle completo de la consulta general.</summary>
        private const string InnerDetalleSql = @"
            SELECT
                CONCAT(RIGHT(CONCAT('00000', H.ID_BENEFICIO), 5),
                       TRIM(H.COD_BENEFICIO),
                       RIGHT(CONCAT('00000', H.CONSEC), 5)) AS Expediente,
                H.REGISTRA_FECHA, H.AUTORIZA_FECHA, H.PAGO_FECHA,
                H.ID_BENEFICIO, H.CONSEC, H.COD_BENEFICIO, H.Beneficio_Desc,
                H.MONTO, H.MONTO_APLICADO, H.ESTADO,
                CASE WHEN H.ESTADO = 'E' THEN 'ENVIADO'
                     WHEN H.ESTADO IS NULL OR H.ESTADO = '' THEN 'SIN ESTADO'
                     ELSE (SELECT E.DESCRIPCION FROM AFI_BENE_ESTADOS E WHERE E.COD_ESTADO = H.ESTADO)
                END AS ESTADO_DESC,
                H.cedula, H.NOMBRE_BENEFICIARIO, H.SEPELIO_IDENTIFICACION,
                Categoria_Desc, Estado_Persona,
                (SELECT B.CRECE_GRUPO FROM AFI_BENE_OTORGA B WHERE ID_BENEFICIO = H.ID_BENEFICIO) AS Grupo,
                CASE WHEN (SELECT CAPACITACION_CMP FROM AFI_BENE_SOCIO_CRECE C
                           WHERE C.COD_BENEFICIO = H.COD_BENEFICIO AND C.CONSEC = H.CONSEC) = 1
                     THEN 'SI' ELSE 'NO' END AS Capacitacion_Completa,
                CASE WHEN (SELECT APLICA_PRODUCTO FROM AFI_BENE_SOCIO_CRECE C
                           WHERE C.COD_BENEFICIO = H.COD_BENEFICIO AND C.CONSEC = H.CONSEC) = 1
                     THEN 'SI' ELSE 'NO' END AS APLICA_PRODUCTO_FIN,
                H.TIPO,
                CASE WHEN H.TIPO = 'M' THEN 'Monetario'
                     WHEN H.TIPO = 'P' THEN 'Producto'
                     ELSE 'Ambos' END AS TipoDesc,
                B.PAGOS_MULTIPLES, H.MONTO_EJECUTADO, H.REQUIERE_JUSTIFICACION, H.PROVINCIA,
                (SELECT C.DESCRIPCION FROM CANTONES C
                  WHERE COD_CANTON = S.CANTON AND COD_PROVINCIA = S.PROVINCIA) AS CANTON,
                (SELECT D.DESCRIPCION FROM DISTRITOS D
                  WHERE COD_CANTON = S.CANTON AND COD_PROVINCIA = S.PROVINCIA AND D.COD_DISTRITO = S.DISTRITO) AS Distrito,
                CASE WHEN S.SEXO = 'F' THEN 'Femenino'
                     WHEN S.SEXO = 'M' THEN 'Masculino'
                     ELSE 'Otro' END AS 'Genero',
                S.AF_EMAIL, H.registra_user,
                CASE WHEN I.CASO_ID != '' THEN 'Interface' ELSE 'Manual' END AS int_desk
            FROM vBeneficios_W_Integral H
            LEFT JOIN AFI_BENEFICIOS B ON B.COD_BENEFICIO = H.COD_BENEFICIO
            LEFT JOIN SOCIOS S ON S.CEDULA = H.CEDULA
            LEFT JOIN AFI_BENE_OTORGA_INT I ON I.ID_BENEFICIO = H.ID_BENEFICIO";

        /// <summary>WHERE principal de la consulta general con todos los filtros condicionales.</summary>
        private const string WhereConsulta = @"
            WHERE COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS
                                     WHERE (@categoriaLike IS NULL OR COD_CATEGORIA LIKE @categoriaLike))
              AND ( @todasFechas = 1 OR @tipoFecha IS NULL
                    OR (@tipoFecha = 'R' AND Registra_Fecha BETWEEN @fechaIni AND @fechaFin)
                    OR (@tipoFecha = 'A' AND Autoriza_Fecha BETWEEN @fechaIni AND @fechaFin)
                    OR (@tipoFecha = 'P' AND Pago_Fecha BETWEEN @fechaIni AND @fechaFin) )
              AND (@estadoLike IS NULL OR Estado LIKE @estadoLike)
              AND (@cedulaLike IS NULL OR cedula LIKE @cedulaLike)
              AND (@expLike IS NULL OR Expediente LIKE @expLike)
              AND (@usuarioLike IS NULL OR UPPER(registra_user) LIKE @usuarioLike) ";

        /// <summary>Filtro de texto libre de la consulta general (solo aplica al detalle, no al conteo).</summary>
        private const string FiltroTextoPrincipal = @"
              AND (@filtroLike IS NULL OR ( Expediente LIKE @filtroLike
                                        OR cedula LIKE @filtroLike
                                        OR Beneficio_Desc LIKE @filtroLike
                                        OR NOMBRE_BENEFICIARIO LIKE @filtroLike
                                        OR registra_user LIKE @filtroLike
                                        OR SEPELIO_IDENTIFICACION LIKE @filtroLike
                                        OR PROVINCIA LIKE @filtroLike
                                        OR Grupo LIKE @filtroLike )) ";

        /// <summary>Consulta de becas socioeconómicas que se anexa cuando la categoría es B_BECA.</summary>
        private const string SqlBecas = @"
            SELECT
                CONCAT(B.PERIODO_LECTIVO, 'BECA', B.COD_EXPEDIENTE) AS expediente,
                B.REGISTRA_FECHA AS registra_fecha,
                B.APRUEBA_FECHA AS autoriza_fecha,
                B.COD_EXPEDIENTE AS id_beneficio,
                B.COD_EXPEDIENTE AS consec,
                'BECA' AS cod_beneficio,
                'Beca Socioeconomica' AS beneficio_desc,
                B.COD_ESTADO AS estado,
                E.ESTADO AS estado_desc,
                B.CEDULA_ASO AS cedula,
                B.NOMBRE_ASO AS nombre_beneficiario,
                B.ADVERTENCIAS AS estado_persona,
                B.ASO_EMAIL AS af_email,
                B.PROM_SAL_GESTIONAR AS monto,
                'SIF' AS int_desk
            FROM BECAS_V2_EXPEDIENTES B
            LEFT JOIN BECAS_V2_ESTADOS_EXPEDIENTES E ON E.COD_ESTADO = B.COD_ESTADO ";

        /// <summary>WHERE de la rama de becas con fecha y texto condicionales.</summary>
        private const string WhereBecas = @"
            WHERE ( @todasFechas = 1 OR @tipoFecha IS NULL OR @tipoFecha = 'P'
                    OR (@tipoFecha = 'R' AND B.REGISTRA_FECHA BETWEEN @fechaIni AND @fechaFin)
                    OR (@tipoFecha = 'A' AND B.APRUEBA_FECHA BETWEEN @fechaIni AND @fechaFin) )
              AND ( @filtroLike IS NULL OR ( B.COD_EXPEDIENTE LIKE @filtroLike
                                          OR B.CEDULA_ASO LIKE @filtroLike
                                          OR B.NOMBRE_ASO LIKE @filtroLike
                                          OR B.ASO_EMAIL LIKE @filtroLike
                                          OR B.PROM_SAL_GESTIONAR LIKE @filtroLike ) ) ";

        // ==================== Comandos SQL finales (concatenación de constantes) ====================

        private const string SqlConsultaCount =
            "SELECT COUNT(*) FROM ( " + InnerConteoSql + " ) T " + WhereConsulta;

        private const string SqlConsultaLista =
            "SELECT * FROM ( " + InnerDetalleSql + " ) T " + WhereConsulta + FiltroTextoPrincipal
            + " ORDER BY Registra_fecha DESC, Beneficio_Desc, Consec DESC OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

        private const string SqlConsultaBecas =
            SqlBecas + WhereBecas + " ORDER BY B.COD_EXPEDIENTE DESC OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
    }
}
