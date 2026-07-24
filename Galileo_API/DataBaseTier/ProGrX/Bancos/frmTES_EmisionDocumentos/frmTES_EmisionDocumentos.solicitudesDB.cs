using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    public partial class FrmTesEmisionDocumentosDb
    {
        private const int MaximoFilasPagina = 100;
        private const int MaximoCaracteresBusqueda = 100;

        /// <summary>
        /// Obtiene una página visual de solicitudes y conserva el total del conjunto
        /// que será utilizado por la emisión.
        /// </summary>
        public ErrorDto<TesEmisionDocumentoSolicitudesPaginaResult>
            TES_EmisionDocumento_Solicitudes_Pagina_Obtener(
                int codEmpresa,
                TesEmisionDocumentoSolicitudesPaginaRequest request)
        {
            var filtro = ParseFiltros(request.filtros);
            NormalizarFiltroFechas(filtro);
            var rangos = GetRangos(filtro);
            var offset = Math.Max(request.offset, 0);
            var filas = Math.Clamp(request.filas, 1, MaximoFilasPagina);
            var busqueda = request.busqueda?.Trim() ?? string.Empty;
            if (busqueda.Length > MaximoCaracteresBusqueda)
            {
                busqueda = busqueda[..MaximoCaracteresBusqueda];
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, connection =>
            {
                var consecutivoInterno = mTesoreria
                    .fxTesTipoDocConsecInterno(
                        codEmpresa,
                        filtro.banco,
                        filtro.tipoDoc,
                        "/",
                        filtro.plan)
                    .Result;
                var usuario = filtro.usuario.Trim().ToUpperInvariant();
                var esUsuarioEspecial =
                    TES_EmisionDocumento_UsuarioEsEspecial(connection, usuario);

                using var resultados = connection.QueryMultiple(
                    SolicitudesPaginaSql,
                    new
                    {
                        top = Math.Max(filtro.cantidad, 0),
                        filtro.tipoDoc,
                        filtro.banco,
                        minimo = rangos.solInicio,
                        maximo = rangos.solCorte,
                        fechaInicio = rangos.fechaInicio,
                        fechaCorte = rangos.fechaCorte,
                        usuario,
                        especial = esUsuarioEspecial,
                        filtro.generarPor,
                        busqueda,
                        offset,
                        filas
                    });

                var resumen = resultados
                    .ReadSingle<TesSolicitudesPaginaResumen>();
                var totalFiltrado = resultados.ReadSingle<int>();
                var solicitudes = resultados
                    .Read<TesSolicitudPaginaData>()
                    .ToList();

                return new TesEmisionDocumentoSolicitudesPaginaResult
                {
                    lista = TES_EmisionDocumento_Solicitudes_Pagina_Formatear(
                        solicitudes,
                        filtro,
                        consecutivoInterno),
                    total = resumen.total,
                    totalFiltrado = totalFiltrado,
                    montoTotal = resumen.monto_total,
                    tieneRestricciones = resumen.tiene_restricciones
                };
            });
        }

        /// <summary>
        /// Resuelve en el API las solicitudes completas que integran una emisión TS.
        /// </summary>
        private ErrorDto<List<int>>
            TES_EmisionDocumento_Solicitudes_Ids_Obtener(
                int codEmpresa,
                TesEmisionDocFiltros filtro)
        {
            NormalizarFiltroFechas(filtro);
            var rangos = GetRangos(filtro);

            return DbHelper.WithConn(_portalDB, codEmpresa, connection =>
            {
                var usuario = filtro.usuario.Trim().ToUpperInvariant();
                var esUsuarioEspecial =
                    TES_EmisionDocumento_UsuarioEsEspecial(connection, usuario);

                return connection.Query<int>(
                    SolicitudesIdsSql,
                    new
                    {
                        top = Math.Max(filtro.cantidad, 0),
                        filtro.tipoDoc,
                        filtro.banco,
                        minimo = rangos.solInicio,
                        maximo = rangos.solCorte,
                        fechaInicio = rangos.fechaInicio,
                        fechaCorte = rangos.fechaCorte,
                        usuario,
                        especial = esUsuarioEspecial,
                        filtro.generarPor
                    }).ToList();
            });
        }

        private static List<TesSolicitudesGenData>
            TES_EmisionDocumento_Solicitudes_Pagina_Formatear(
                IEnumerable<TesSolicitudPaginaData> solicitudes,
                TesEmisionDocFiltros filtro,
                long consecutivoInterno)
        {
            var fechaProceso = DateTime.Now;
            return solicitudes.Select(solicitud =>
            {
                solicitud.documento =
                    TES_EmisionDocumento_Solicitud_Documento_Formatear(
                        solicitud,
                        filtro.docInicial,
                        consecutivoInterno);
                solicitud.fecha = fechaProceso;
                solicitud.firmas = solicitud.firmas_autoriza_fecha.HasValue
                    ? "Sí"
                    : "No";
                return (TesSolicitudesGenData)solicitud;
            }).ToList();
        }

        private static string
            TES_EmisionDocumento_Solicitud_Documento_Formatear(
                TesSolicitudPaginaData solicitud,
                int documentoInicial,
                long consecutivoInterno)
        {
            if (string.Equals(
                    solicitud.tipo,
                    "TE",
                    StringComparison.OrdinalIgnoreCase))
            {
                var consecutivo = consecutivoInterno + solicitud.orden_te - 1;
                var documentoVisible =
                    documentoInicial + solicitud.orden_visible;
                return string.Create(
                    CultureInfo.InvariantCulture,
                    $"{documentoVisible}-{consecutivo:000}");
            }

            if (string.Equals(
                    solicitud.tipo,
                    "TS",
                    StringComparison.OrdinalIgnoreCase))
            {
                return string.Create(
                    CultureInfo.InvariantCulture,
                    $"{documentoInicial}-{solicitud.orden_ts:000}");
            }

            return (documentoInicial + solicitud.orden_visible - 1)
                .ToString(CultureInfo.InvariantCulture);
        }

        private const string SolicitudesIdsSql = """
SELECT TOP (@top) t.NSolicitud
FROM Tes_Transacciones AS t
WHERE t.Estado = 'P'
  AND t.Autoriza = 'S'
  AND t.fecha_hold IS NULL
  AND
  (
      (@especial = 1 AND UPPER(t.USUARIO_AUTORIZA_ESPECIAL) = @usuario)
      OR
      (
          @especial = 0
          AND t.Tipo = @tipoDoc
          AND t.Id_Banco = @banco
          AND t.USUARIO_AUTORIZA_ESPECIAL IS NULL
      )
  )
  AND
  (
      (@generarPor = 'solicitudes' AND t.NSolicitud BETWEEN @minimo AND @maximo)
      OR
      (@generarPor = 'fechas' AND t.Fecha_Solicitud BETWEEN @fechaInicio AND @fechaCorte)
      OR @generarPor NOT IN ('solicitudes', 'fechas')
  )
ORDER BY t.NSolicitud;
""";

        private const string SolicitudesPaginaSql = """
SELECT TOP (@top)
       t.*,
       ROW_NUMBER() OVER (ORDER BY t.NSolicitud) AS orden_lazy,
       SUM(CASE WHEN UPPER(t.Tipo) = 'TE' THEN 1 ELSE 0 END)
           OVER (ORDER BY t.NSolicitud ROWS UNBOUNDED PRECEDING) AS orden_te,
       SUM(CASE WHEN UPPER(t.Tipo) = 'TS' THEN 1 ELSE 0 END)
           OVER (ORDER BY t.NSolicitud ROWS UNBOUNDED PRECEDING) AS orden_ts,
       SUM(CASE WHEN UPPER(t.Tipo) NOT IN ('TE', 'TS') THEN 1 ELSE 0 END)
           OVER (ORDER BY t.NSolicitud ROWS UNBOUNDED PRECEDING) AS orden_visible
INTO #SolicitudesEmision
FROM Tes_Transacciones AS t
WHERE t.Estado = 'P'
  AND t.Autoriza = 'S'
  AND t.fecha_hold IS NULL
  AND
  (
      (@especial = 1 AND UPPER(t.USUARIO_AUTORIZA_ESPECIAL) = @usuario)
      OR
      (
          @especial = 0
          AND t.Tipo = @tipoDoc
          AND t.Id_Banco = @banco
          AND t.USUARIO_AUTORIZA_ESPECIAL IS NULL
      )
  )
  AND
  (
      (@generarPor = 'solicitudes' AND t.NSolicitud BETWEEN @minimo AND @maximo)
      OR
      (@generarPor = 'fechas' AND t.Fecha_Solicitud BETWEEN @fechaInicio AND @fechaCorte)
      OR @generarPor NOT IN ('solicitudes', 'fechas')
  )
ORDER BY t.NSolicitud;

SELECT COUNT(1) AS total,
       COALESCE(SUM(Monto), 0) AS monto_total,
       CONVERT(
           bit,
           COALESCE(
               MAX(
                   CASE
                       WHEN UPPER(Tipo) = 'TE'
                            AND dbo.fxTes_Cuentas_Bancarias_Pass(
                                Id_Banco,
                                Cta_Ahorros) = 0
                       THEN 1
                       ELSE 0
                   END),
               0)) AS tiene_restricciones
FROM #SolicitudesEmision;

SELECT *
INTO #SolicitudesVisuales
FROM #SolicitudesEmision
WHERE @busqueda = ''
   OR CONVERT(varchar(20), NSolicitud) LIKE '%' + @busqueda + '%'
   OR Beneficiario LIKE '%' + @busqueda + '%'
   OR Cta_Ahorros LIKE '%' + @busqueda + '%';

SELECT COUNT(1)
FROM #SolicitudesVisuales;

SELECT q.*,
       CAST(q.id_rechazo AS varchar(10)) + ' - ' + sm.descripcion AS estadoSinpe,
       dbo.fxTes_Cuentas_Bancarias_Pass(q.Id_Banco, q.Cta_Ahorros) AS Pass
FROM #SolicitudesVisuales AS q
LEFT JOIN SINPE_MOTIVOS AS sm
    ON sm.cod_motivo = q.id_rechazo
ORDER BY q.orden_lazy
OFFSET @offset ROWS
FETCH NEXT @filas ROWS ONLY;
""";

        private sealed class TesSolicitudesPaginaResumen
        {
            public int total { get; set; } = 0;
            public decimal monto_total { get; set; } = 0;
            public bool tiene_restricciones { get; set; } = false;
        }

        private sealed class TesSolicitudPaginaData : TesSolicitudesGenData
        {
            public int orden_te { get; set; } = 0;
            public int orden_ts { get; set; } = 0;
            public int orden_visible { get; set; } = 0;
        }
    }
}
