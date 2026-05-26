
using Dapper;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers;
using System.Data;
using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF16StarHGenerar : CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoF16StarHGenerar.CcProcesoMensualArchivoRegistroDbModel>
    {
        private const string TipoAporte = "A";
        private const string TipoCredito = "C";

        private const string MovimientoExclusion = "E";
        private const string MovimientoInclusion = "I";
        private const string MovimientoCambio = "C";
        private const string MovimientoMantiene = "M";

        private CcProcesoMensualArchivoF16ConfigDbModel _configuracion = new();

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["16"];

        protected override string CodigoPlanillaEnvio => "16";
        protected override string CodigoFormato => "F16";
        protected override string ExtensionArchivo => ".txt";
        protected override string ContentType => ContentTypeText;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                P.Tipo,
                P.Movimiento,
                P.Monto_Actual AS MontoActual,
                S.nombre AS Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.cedula, P.tipo, P.movimiento";

        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
                   IDbConnection connection,
                   CcProcesoMensualGeneraArchivoRequest request)
        {
            _configuracion = ObtenerConfiguracion(
                connection,
                request.CodInstitucion);

            return base.GenerarArchivo(connection, request);
        }

        protected override string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var linea = CrearLineaArchivo(registro, request);

                if (!string.IsNullOrEmpty(linea))
                {
                    builder.AppendLine(linea);
                }
            }

            // VB6: Print #fnFile, "!"
            builder.AppendLine("!");

            return builder.ToString();
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var tipo = registro.Tipo?.Trim().ToUpperInvariant();

            return tipo switch
            {
                TipoAporte => CrearLineaAporte(registro, request.FechaProceso),
                TipoCredito => CrearLineaCredito(registro, request.FechaProceso),
                _ => string.Empty
            };
        }

        private string CrearLineaAporte(
            CcProcesoMensualArchivoRegistroDbModel registro,
            decimal fechaProceso)
        {
            if (string.Equals(
                registro.Movimiento?.Trim(),
                MovimientoMantiene,
                StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return CrearLineaBase(
                    registro,
                    _configuracion.CodigoAportesEnv,
                    ObtenerTipoMovimiento(registro.Movimiento),
                    FormatearMontoSinPunto(registro.MontoActual),
                    fechaProceso)
                + new string(' ', 12)
                + _configuracion.PorcAhorro.ToString("00.00", CultureInfo.InvariantCulture)
                + " "
                + fechaProceso.ToString(CultureInfo.InvariantCulture)
                + "1";
        }

        private string CrearLineaCredito(
            CcProcesoMensualArchivoRegistroDbModel registro,
            decimal fechaProceso)
        {
            return CrearLineaBase(
                    registro,
                    _configuracion.CodigoCreditosEnv,
                    ObtenerTipoMovimiento(registro.Movimiento),
                    FormatearMontoSinPunto(registro.MontoActual),
                    fechaProceso)
                + new string(' ', 12)
                + "00.00 "
                + fechaProceso.ToString(CultureInfo.InvariantCulture)
                + "1";
        }

        private static string CrearLineaBase(
            CcProcesoMensualArchivoRegistroDbModel registro,
            string codigoTipo,
            string tipoMovimiento,
            string monto,
            decimal fechaProceso)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarEspaciosDerecha(
                    FormatearCedula(registro.Cedula),
                    15)
                + codigoTipo.Trim()
                + " "
                + tipoMovimiento
                + " "
                + monto
                + " 0000000000 0000000000 01/"
                + ObtenerMesProceso(fechaProceso)
                + "/"
                + ObtenerAnioProceso(fechaProceso);
        }

        private static CcProcesoMensualArchivoF16ConfigDbModel ObtenerConfiguracion(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(porc_ahorro, 0) AS PorcAhorro
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoF16ConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoF16ConfigDbModel();
        }

        private static string ObtenerTipoMovimiento(string? movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                MovimientoExclusion => "B",
                MovimientoInclusion => "F",
                MovimientoCambio => "F",
                _ => "F"
            };
        }

        private static string FormatearMontoSinPunto(decimal monto)
        {
            var montoTexto = monto.ToString("00000000.00", CultureInfo.InvariantCulture);

            return montoTexto.Replace(".", string.Empty);
        }

        private static string FormatearCedula(string? cedula)
        {
            var texto = cedula?.Trim() ?? string.Empty;

            if (decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var numero))
            {
                return numero.ToString("0000000000", CultureInfo.InvariantCulture);
            }

            return texto;
        }

        private static string ObtenerMesProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 6
                ? fechaBase.Substring(4, 2)
                : string.Empty;
        }

        private static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : string.Empty;
        }

        public sealed class CcProcesoMensualArchivoRegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Nombre { get; set; } = string.Empty;
        }

        private sealed class CcProcesoMensualArchivoF16ConfigDbModel
        {
            public string CodigoAportesEnv { get; set; } = string.Empty;
            public string CodigoCreditosEnv { get; set; } = string.Empty;
            public decimal PorcAhorro { get; set; } =  0;
        }

    }
}
