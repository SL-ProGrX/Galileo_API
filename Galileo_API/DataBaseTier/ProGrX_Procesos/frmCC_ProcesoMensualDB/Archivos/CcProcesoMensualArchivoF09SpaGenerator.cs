using Dapper;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF09SpaGenerator : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "09";
        private const string NombreArchivoSpa = "ARC-DED.TXT";
        private const string ContentTypeText = "text/plain";
        private const int LargoNombre = 30;
        private const int LargoCedula = 10;
        private const int LargoMonto = 8;

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var movimientos = ObtenerMovimientos(configuracion);

            var registros = ObtenerRegistrosPlanilla(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                movimientos);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);
            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaDirectorio, NombreArchivoSpa);

            var contenido = CrearContenidoArchivo( registros,configuracion);             

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = NombreArchivoSpa,
                RutaArchivo = rutaArchivo,
                ContentType = ContentTypeText,
                ArchivoBytes = Encoding.UTF8.GetBytes(contenido),
                ArchivosGenerados = [rutaArchivo]
            };
        }

      
        private static List<string> ObtenerMovimientos(
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            var movimientos = new List<string>();

            AgregarMovimientoSiAplica(movimientos, configuracion.IncInclusiones, "I");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncExclusiones, "E");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncModificaciones, "C");
            AgregarMovimientoSiAplica(movimientos, configuracion.IncMantienen, "M");

            movimientos.Add("P");

            return movimientos;
        }

        private static void AgregarMovimientoSiAplica(
            List<string> movimientos,
            int indicador,
            string movimiento)
        {
            if (indicador == 1)
            {
                movimientos.Add(movimiento);
            }
        }

        private static List<CcProcesoMensualArchivoF09RegistroDbModel> ObtenerRegistrosPlanilla(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso,
            IEnumerable<string> movimientos)
        {
            const string query = @"
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

            return [.. connection.Query<CcProcesoMensualArchivoF09RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    Movimientos = movimientos
                })];
        }

        private static string CrearContenidoArchivo(IEnumerable<CcProcesoMensualArchivoF09RegistroDbModel> registros,CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            var builder = new StringBuilder();

            foreach (var registro in registros)
            {
                var linea = CrearLineaArchivo(
                    registro,
                    configuracion);

                if (!string.IsNullOrEmpty(linea))
                {
                    builder.AppendLine(linea);
                }
            }

            return builder.ToString();
        }

        private static string CrearLineaArchivo(
            CcProcesoMensualArchivoF09RegistroDbModel registro,
            CcProcesoMensualArchivoConfiguracionModel configuracion)
        {
            _ = ObtenerMovimientoSpa(registro.Movimiento);

            // VB6 fuerza todas las líneas como inclusiones:
            // i = 2
            int movimientoSpa = 2;


            var codigoDeduccion = ObtenerCodigoDeduccion(
                registro.Tipo,
                configuracion);

            if (string.IsNullOrWhiteSpace(codigoDeduccion))
            {
                return string.Empty;
            }

            var montoActual = RedondearUnDecimal(registro.MontoActual);
            var montoAnterior = RedondearUnDecimal(registro.MontoAnterior);

            var builder = new StringBuilder();

            builder.Append(movimientoSpa.ToString(CultureInfo.InvariantCulture));
            builder.Append(codigoDeduccion.Trim());
            builder.Append(Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarEspaciosDerecha(LimpiarNombre(registro.Nombre), LargoNombre));
            builder.Append(FormatearCedula(registro.Cedula));
            builder.Append(Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarCerosIzquierda(FormatearMontoSpa(montoAnterior), LargoMonto));
            builder.Append(Helpers.CcProcesoMensualArchivoRutaHelperDb.RellenarCerosIzquierda(FormatearMontoSpa(montoActual), LargoMonto));

            return builder.ToString();
        }

        private static int ObtenerMovimientoSpa(string movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 1,
                "I" => 2,
                "C" => 3,
                _ => 4
            };
        }

        private static string ObtenerCodigoDeduccion(
            string tipo,
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

            return montoEntero.ToString(
                "000",
                CultureInfo.InvariantCulture);
        }

        private static string FormatearCedula(string cedula)
        {
            var valor = SoloDigitos(cedula);

            if (valor.Length > LargoCedula)
            {
                valor = valor[..LargoCedula];
            }

            return long.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numero)
                ? numero.ToString("0000000000", CultureInfo.InvariantCulture)
                : valor.PadLeft(LargoCedula, '0');
        }

        private static string SoloDigitos(string? valor)
        {
            return new string([.. (valor ?? string.Empty).Where(char.IsDigit)]);
        }
         
        private static string LimpiarNombre(string? nombre)
        {
            return (nombre ?? string.Empty)
                .Replace("\t", string.Empty)
                .Trim();
        }
        
        private static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : fechaBase;
        }

        private static string LimpiarNombreDirectorio(string valor)
        {
            var nombre = string.IsNullOrWhiteSpace(valor)
                ? "SinInstitucion"
                : valor.Trim();

            foreach (var caracter in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(caracter, '_');
            }

            return nombre;
        }

    

        private sealed class CcProcesoMensualArchivoF09RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public decimal MontoAnterior { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public int Sector { get; set; } = 0;
        }
    }
}
