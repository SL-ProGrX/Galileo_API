using System.Globalization;
using System.Text;
using System.Data;
using Dapper;
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
        public static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "Select dbo.MyGetdate() as Fecha";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }
        public static List<CcProcesoMensualArchivoRegistroDbModel> ObtenerRegistrosGeneral(IDbConnection connection, int codInstitucion, decimal fechaProceso, string tipo)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    S.nombre AS Nombre,
                    S.direccion AS Direccion
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.cod_institucion = @CodInstitucion
                  AND P.tipo = @TipoCredito
                ORDER BY P.cedula";

            return [.. connection.Query<CcProcesoMensualArchivoRegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoCredito =tipo
                })];
        }

        public static CcProcesoMensualArchivoConfiguracionModel ObtenerConfiguracionGeneral(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(codigo_aportes_env, '') AS CodigoAportesEnv,
                    ISNULL(codigo_creditos_env, '') AS CodigoCreditosEnv,
                    ISNULL(porc_ahorro, 0) AS PorcAhorro,
                    ISNULL(codigo_inst_deduc, '') AS CodigoInstDeduc,
                    ISNULL(IncInclusiones, 0) AS IncInclusiones,
                    ISNULL(IncExclusiones, 0) AS IncExclusiones,
                    ISNULL(IncModificaciones, 0) AS IncModificaciones,
                    ISNULL(IncMantienen, 0) AS IncMantienen,
                    ISNULL(porc_aporte, 0) AS PorcAporte,
                    ISNULL(compara_indicador, 0) AS ComparaIndicador
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualArchivoConfiguracionModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualArchivoConfiguracionModel();
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
        public static CcProcesoMensualArchivoNombreModel SepararNombre(string? nombreCompleto)
        {
            var apellido1 = new StringBuilder();
            var apellido2 = new StringBuilder();
            var nombre1 = new StringBuilder();
            var nombre2 = new StringBuilder();

            var posicion = 1;

            foreach (var caracter in nombreCompleto ?? string.Empty)
            {
                if (caracter == ' ')
                {
                    posicion++;
                    continue;
                }

                switch (posicion)
                {
                    case 1:
                        apellido1.Append(caracter);
                        break;

                    case 2:
                        apellido2.Append(caracter);
                        break;

                    case 3:
                        nombre1.Append(caracter);
                        break;

                    case 4:
                        nombre2.Append(caracter);
                        break;
                }
            }

            return new CcProcesoMensualArchivoNombreModel
            {
                Apellido1 = apellido1.ToString(),
                Apellido2 = apellido2.ToString(),
                Nombre1 = nombre1.ToString(),
                Nombre2 = nombre2.ToString()
            };
        }
    }
}
