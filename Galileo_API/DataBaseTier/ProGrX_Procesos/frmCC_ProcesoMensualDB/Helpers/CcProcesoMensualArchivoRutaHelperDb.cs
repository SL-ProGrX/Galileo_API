using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers
{
    public static class CcProcesoMensualArchivoRutaHelperDb
    {
        private const string CarpetaPlanilla = "Planilla";
        private const string NombreInstitucionDefault = "SinInstitucion";

        public static string FormatearFechaProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 6
                ? $"{fechaBase[..4]}-{fechaBase.Substring(4, 2)}"
                : fechaBase;
        }
        public static string RellenarCerosIzquierda(string valor, int largo)
        {
            var texto = valor ?? string.Empty;

            if (texto.Length > largo)
            {
                return texto[^largo..];
            }

            return texto.PadLeft(largo, '0');
        }

        public static string RellenarEspaciosDerecha(string valor, int largo)
        {
            var texto = valor ?? string.Empty;

            if (texto.Length > largo)
            {
                return texto[..largo];
            }

            return texto.PadRight(largo);
        }

        public static string ObtenerRutaPlanilla(CcProcesoMensualGeneraArchivoRequest request)
        {
            var anio = ObtenerAnioProceso(request.FechaProceso);
            var nombreInstitucion = LimpiarNombreDirectorio(request.NombreInstitucion);

            return Path.Combine(
                request.DirectorioResultados,
                CarpetaPlanilla,
                nombreInstitucion,
                anio);
        }

        public static void CrearDirectorioSiNoExiste(string rutaDirectorio)
        {
            Directory.CreateDirectory(rutaDirectorio);
        }

        public static string CombinarArchivo(string rutaDirectorio, string nombreArchivo)
        {
            return Path.Combine(rutaDirectorio, nombreArchivo);
        }

        private static string LimpiarNombreDirectorio(string valor)
        {
            var nombre = string.IsNullOrWhiteSpace(valor)
                ? NombreInstitucionDefault
                : valor.Trim();

            foreach (var caracter in Path.GetInvalidFileNameChars())
            {
                nombre = nombre.Replace(caracter, '_');
            }

            return nombre;
        }

        public static string ObtenerAnioProceso(decimal fechaProceso)
        {
            var fechaBase = Math.Truncate(fechaProceso)
                .ToString(CultureInfo.InvariantCulture);

            return fechaBase.Length >= 4
                ? fechaBase[..4]
                : fechaBase;
        }

        public static void GuardarArchivoTexto(
            string rutaDirectorio,
            string rutaArchivo,
            string contenido,
            System.Text.Encoding encoding)
        {
            CrearDirectorioSiNoExiste(rutaDirectorio);

            if (File.Exists(rutaArchivo))
            {
                File.Delete(rutaArchivo);
            }

            File.WriteAllText(rutaArchivo, contenido, encoding);
        }

        public static string DepurarCadena(string? valor)
        {
            var texto = valor?.Trim() ?? string.Empty;
            var resultado = new StringBuilder();

            foreach (var caracter in texto)
            {
                var ascii = (int)caracter;

                if ((ascii > 47 && ascii < 123) || ascii == 32)
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString();
        }
        public static string FxStringRelleno(string? cadena, string direccion, string charRelleno, int cantidad)
        {
            var valorBase = cadena?.Trim() ?? string.Empty;

            var valor = valorBase.Length > cantidad
              ? valorBase[..cantidad]
              : valorBase;

            var relleno = string.IsNullOrEmpty(charRelleno)
                ? " "
                : charRelleno[..1];

            var resultado = new StringBuilder(valor);

            if (string.Equals(direccion, "D", StringComparison.OrdinalIgnoreCase))
            {
                while (resultado.Length < cantidad)
                {
                    resultado.Append(relleno);
                }

                return resultado.ToString()[..cantidad];
            }

            while (resultado.Length < cantidad)
            {
                resultado.Insert(0, relleno);
            }

            return resultado.ToString()[..cantidad];
        }
    }
}
