using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF09SpaGenerator : CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoF09SpaGenerator.CcProcesoMensualArchivoF09RegistroDbModel>

    {
        private const string NombreArchivoSpa = "ARC-DED.TXT";
        private const int LargoNombre = 30;
        private const int LargoCedula = 10;
        private const int LargoMonto = 8;

        private List<string> _movimientos = [];
        private CcProcesoMensualArchivoConfiguracionModel _configuracion = new();

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["09"];

        protected override string CodigoPlanillaEnvio => "09";
        protected override string CodigoFormato => "F09";
        protected override string ExtensionArchivo => ".TXT";
        protected override string ContentType => ContentTypeText;
       protected override Encoding EncodingArchivo => Helpers.CcProcesoMensualArchivoRutaHelperDb.Utf8SinBom;

        protected override string QueryRegistros => @"
            SELECT
                P.Cedula,
                S.nombre AS Nombre,
                P.Tipo,
                P.Monto_Actual AS MontoActual,
                P.Monto_Anterior AS MontoAnterior,
                P.Movimiento,
                ISNULL(S.cod_sector, 0) AS Sector
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.cod_institucion = @CodInstitucion
              AND P.movimiento IN @Movimientos
            ORDER BY P.cedula, P.tipo, P.movimiento";

        public CcProcesoMensualArchivoF09SpaGenerator(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
        }
        
        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            _configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            _movimientos = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerMovimientosPorIndicadores(
                _configuracion);

            return base.GenerarArchivo(connection, request);
        }

        protected override string CrearNombreArchivo(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return NombreArchivoSpa;
        }

        protected override object CrearParametrosRegistros(
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
                request.FechaProceso,
               request.CodInstitucion,
                Movimientos = _movimientos
            };
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF09RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            const int movimientoSpa = 2;

            var montoActual = RedondearUnDecimal(registro.MontoActual);
            var montoAnterior = RedondearUnDecimal(registro.MontoAnterior);

            var builder = new StringBuilder();

            builder.Append(movimientoSpa.ToString(CultureInfo.InvariantCulture));

            var codigoDeduccion = ObtenerCodigoDeduccion(
                registro.Tipo,
                _configuracion);

            if (!string.IsNullOrWhiteSpace(codigoDeduccion))
            {
                builder.Append(codigoDeduccion.Trim());
            }

            builder.Append(
                Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarEspaciosDerecha(
                    LimpiarNombre(registro.Nombre),
                    LargoNombre));

            builder.Append(FormatearCedula(registro.Cedula));

            builder.Append(
                Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarCerosIzquierda(
                    FormatearMontoSpa(montoAnterior),
                    LargoMonto));

            builder.Append(
                Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarCerosIzquierda(
                    FormatearMontoSpa(montoActual),
                    LargoMonto));

            return builder.ToString();
        }

        protected override CcProcesoMensualArchivoGeneradoModel CrearRespuesta(
            string nombreArchivo,
            string rutaArchivo,
            string contenido)
        {
            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentType,
                ArchivoBytes = EncodingArchivo.GetBytes(contenido),
                ArchivosGenerados = [rutaArchivo]
            };
        }

        private static string ObtenerCodigoDeduccion(
            string? tipo,
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            return tipo?.Trim().ToUpperInvariant() switch
            {
                "A" => configuracion.CodigoAportesEnv,
                "E" => configuracion.CodigoAportesEnv,
                "C" => configuracion.CodigoCreditosEnv,
                _ => string.Empty
            };
        }

        private static decimal RedondearUnDecimal(decimal monto)
        {
            return Math.Round(monto, 1, MidpointRounding.AwayFromZero);
        }

        private static string FormatearMontoSpa(decimal monto)
        {
            var montoEntero = Convert.ToInt64(monto * 100);

            return montoEntero.ToString("00000000", CultureInfo.InvariantCulture);
        }

        private static string FormatearCedula(string? cedula)
        {
            var valor = (cedula ?? string.Empty).Trim();

            if (valor.Length > LargoCedula)
            {
                valor = valor[..LargoCedula];
            }

            return long.TryParse(
                valor,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var numero)
                    ? numero.ToString("0000000000", CultureInfo.InvariantCulture)
                    : valor.PadLeft(LargoCedula, '0');
        }

 

        private static string LimpiarNombre(string? nombre)
        {
            return (nombre ?? string.Empty)
                .Replace("\t", string.Empty)
                .Trim();
        }

        public sealed class CcProcesoMensualArchivoF09RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public decimal MontoActual { get; set; }
            public decimal MontoAnterior { get; set; }
            public string Movimiento { get; set; } = string.Empty;
            public int Sector { get; set; }
        }
    }
}
