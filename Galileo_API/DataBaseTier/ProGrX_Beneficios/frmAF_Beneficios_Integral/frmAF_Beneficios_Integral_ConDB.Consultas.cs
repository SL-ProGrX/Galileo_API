using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralConDB
    {
        /// <summary>
        /// Resultado del armado del filtro de texto libre para la consulta general y su rama de becas.
        /// </summary>
        private sealed record FiltroTextoResultado(string Principal, string BecaTexto, string WhereBeca);

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
                var p = new DynamicParameters();

                var categoriaClause = ConstruirCategoriaFiltro(filtro, p);
                var where = ConstruirWhereConsulta(filtro, categoriaClause, p, out var whereBeca);
                var texto = ConstruirFiltroTexto(filtro, whereBeca, p);
                var paginacion = ConstruirPaginacion(filtro, p);

                var datos = new BeneConsultaDatosLista();

                var countSql = $"SELECT COUNT(*) FROM ( {InnerConteo(categoriaClause)} ) T {where} ";
                datos.total = connection.Query<int>(countSql, p).FirstOrDefault();

                var listaSql = $@"SELECT * FROM ( {InnerDetalle(categoriaClause)} ) T {where} {texto.Principal}
                                  ORDER BY Registra_fecha DESC, Beneficio_Desc, Consec DESC {paginacion}";
                datos.lista = connection.Query<BeneConsultaDatos>(listaSql, p).ToList();

                if (NormalizarTexto(filtro.categoria) == "B_BECA")
                {
                    var becaSql = $@"{SqlBecas} {texto.WhereBeca} {texto.BecaTexto}
                                     ORDER BY B.COD_EXPEDIENTE DESC {paginacion}";
                    datos.lista.AddRange(connection.Query<BeneConsultaDatos>(becaSql, p).ToList());
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

        // ==================== Helpers privados de armado de filtros ====================

        /// <summary>
        /// Construye la cláusula de categoría reutilizada dentro de las subconsultas de grupos.
        /// </summary>
        private static string ConstruirCategoriaFiltro(BeneConsultaFiltros filtro, DynamicParameters p)
        {
            var cat = NormalizarTexto(filtro.categoria);
            if (cat == "T" || cat.Length == 0)
            {
                return string.Empty;
            }

            p.Add("@categoriaLike", $"%{cat}%");
            return " WHERE COD_CATEGORIA LIKE @categoriaLike ";
        }

        /// <summary>
        /// Construye el WHERE principal de la consulta (categoría, fechas, estado, cédula, expediente, usuario).
        /// </summary>
        private static string ConstruirWhereConsulta(
            BeneConsultaFiltros filtro, string categoriaClause, DynamicParameters p, out string whereBeca)
        {
            whereBeca = string.Empty;
            var where = $" WHERE COD_BENEFICIO IN (SELECT COD_BENEFICIO FROM AFI_BENEFICIOS {categoriaClause}) ";

            AgregarFiltroFechas(filtro, p, ref where, ref whereBeca);

            if (NormalizarTexto(filtro.estado) != "T")
            {
                p.Add("@estadoLike", $"%{NormalizarTexto(filtro.estado)}%");
                where += " AND Estado LIKE @estadoLike ";
            }

            if (!EsCedulaVacia(filtro.cedula))
            {
                p.Add("@cedulaLike", $"%{filtro.cedula!.Trim()}%");
                where += " AND cedula LIKE @cedulaLike ";
            }

            if (filtro.noExpediente != null)
            {
                p.Add("@expLike", $"%{filtro.noExpediente}%");
                where += " AND Expediente LIKE @expLike ";
            }

            if (filtro.usuario != null)
            {
                p.Add("@usuarioLike", $"%{filtro.usuario.Trim().ToUpper()}%");
                where += " AND UPPER(registra_user) LIKE @usuarioLike ";
            }

            return where;
        }

        /// <summary>
        /// Indica si el valor de cédula debe tratarse como vacío (null, espacio, "0" o "").
        /// </summary>
        private static bool EsCedulaVacia(string? cedula)
            => cedula is null || cedula == " " || cedula == "0" || cedula == string.Empty;

        /// <summary>
        /// Agrega el filtro de rango de fechas al WHERE principal y al WHERE de becas según el tipo de fecha.
        /// </summary>
        private static void AgregarFiltroFechas(
            BeneConsultaFiltros filtro, DynamicParameters p, ref string where, ref string whereBeca)
        {
            if (filtro.todasFechas != false || filtro.tipoFecha == null)
            {
                return;
            }

            var fechaIni = DateTimeOffset.Parse(filtro.fechaInicio!).ToString("yyyy-MM-dd");
            var fechaFin = DateTimeOffset.Parse(filtro.fechaCorte!).ToString("yyyy-MM-dd");
            p.Add("@fechaIni", $"{fechaIni} 00:00:00");
            p.Add("@fechaFin", $"{fechaFin} 23:59:59");

            switch (filtro.tipoFecha)
            {
                case "R":
                    where += " AND Registra_Fecha BETWEEN @fechaIni AND @fechaFin ";
                    whereBeca += " WHERE B.REGISTRA_FECHA BETWEEN @fechaIni AND @fechaFin ";
                    break;
                case "A":
                    where += " AND Autoriza_Fecha BETWEEN @fechaIni AND @fechaFin ";
                    whereBeca += " WHERE B.APRUEBA_FECHA BETWEEN @fechaIni AND @fechaFin ";
                    break;
                case "P":
                    where += " AND Pago_Fecha BETWEEN @fechaIni AND @fechaFin ";
                    break;
            }
        }

        /// <summary>
        /// Construye el filtro de texto libre para la consulta general y su equivalente para becas.
        /// </summary>
        private static FiltroTextoResultado ConstruirFiltroTexto(
            BeneConsultaFiltros filtro, string whereBeca, DynamicParameters p)
        {
            if (string.IsNullOrEmpty(filtro.filtro))
            {
                return new FiltroTextoResultado(string.Empty, string.Empty, whereBeca);
            }

            p.Add("@filtroLike", $"%{filtro.filtro}%");

            var principal = @" AND ( Expediente LIKE @filtroLike
                                  OR cedula LIKE @filtroLike
                                  OR Beneficio_Desc LIKE @filtroLike
                                  OR NOMBRE_BENEFICIARIO LIKE @filtroLike
                                  OR registra_user LIKE @filtroLike
                                  OR SEPELIO_IDENTIFICACION LIKE @filtroLike
                                  OR PROVINCIA LIKE @filtroLike
                                  OR Grupo LIKE @filtroLike ) ";

            var whereBecaFinal = string.IsNullOrEmpty(whereBeca) ? " WHERE " : whereBeca + " AND ";

            var becaTexto = @" ( B.COD_EXPEDIENTE LIKE @filtroLike
                             OR B.CEDULA_ASO LIKE @filtroLike
                             OR B.NOMBRE_ASO LIKE @filtroLike
                             OR B.ASO_EMAIL LIKE @filtroLike
                             OR B.PROM_SAL_GESTIONAR LIKE @filtroLike ) ";

            return new FiltroTextoResultado(principal, becaTexto, whereBecaFinal);
        }

        /// <summary>
        /// Construye la cláusula de paginación (OFFSET/FETCH) cuando el filtro trae página.
        /// </summary>
        private static string ConstruirPaginacion(BeneConsultaFiltros filtro, DynamicParameters p)
        {
            if (filtro.pagina == null)
            {
                return string.Empty;
            }

            p.Add("@offset", filtro.pagina ?? 0);
            p.Add("@fetch", filtro.paginacion ?? 0);
            return " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY ";
        }

        // ==================== Cuerpos SQL (sin datos de usuario) ====================

        /// <summary>
        /// Subconsulta interna usada para el conteo total de la consulta general.
        /// </summary>
        private static string InnerConteo(string categoriaClause) => $@"
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
                              SELECT COD_GRUPO FROM AFI_BENE_GRUPOS {categoriaClause}))";

        /// <summary>
        /// Subconsulta interna con el detalle completo de la consulta general.
        /// </summary>
        private static string InnerDetalle(string categoriaClause) => $@"
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

        /// <summary>
        /// Consulta de becas socioeconómicas que se anexa cuando la categoría es B_BECA.
        /// </summary>
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
    }
}
