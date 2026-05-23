using System.Globalization;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers
{
    public class CcProcesoMensualArchivoRutaHelperDb
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

                if (ascii > 47 && ascii < 123)
                {
                    resultado.Append(caracter);
                }
                else if (ascii == 32)
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString();
        }
        public static string FxStringRelleno( string? cadena,  string direccion,  string charRelleno, int cantidad)
        {
            var valor = (cadena ?? string.Empty).Trim();

            if (valor.Length > cantidad)
            {
                valor = valor[..cantidad];
            }

            var relleno = string.IsNullOrEmpty(charRelleno)
                ? " "
                : charRelleno[..1];

            if (string.Equals(direccion, "D", StringComparison.OrdinalIgnoreCase))
            {
                while (valor.Length < cantidad)
                {
                    valor += relleno;
                }
            }
            else
            {
                while (valor.Length < cantidad)
                {
                    valor = relleno + valor;
                }
            }

            return valor.Length > cantidad
                ? valor[..cantidad]
                : valor;
        }
    }
}
