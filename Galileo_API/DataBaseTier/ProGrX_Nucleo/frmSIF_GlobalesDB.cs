using System.Security.Cryptography;
using System.Text;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifGlobalesDB
    {
        private const int Modulo = 10;
        private const string DirectorioResultados = "Directorio de Resultados";
        private const string ReportesPersonalizados = "Reportes Personalizados";
        private const string PuertosDisponibles = "Puertos Disponibles";
        private const string FondoPantalla = "Fondo de Pantalla";

        private static readonly string[] Variables =
        [
            DirectorioResultados,
            ReportesPersonalizados,
            PuertosDisponibles,
            FondoPantalla
        ];

        private static readonly object ArchivoLock = new();
        private readonly string _rutaArchivo;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmSifGlobalesDB(IConfiguration config)
        {
            const string nombreArchivoPredeterminado = "Global.ini";
            if (Path.IsPathRooted(nombreArchivoPredeterminado))
                throw new InvalidOperationException("El nombre predeterminado de Global.ini no es válido.");

            var rutaPredeterminada = Path.Combine(
                AppContext.BaseDirectory,
                nombreArchivoPredeterminado);
            _rutaArchivo = config["SIF_Globales:RutaArchivo"]
                ?? rutaPredeterminada;
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto<List<SifVariableGlobalDto>> Obtener()
        {
            try
            {
                lock (ArchivoLock)
                {
                    var valores = LeerValores();
                    return DbHelper.CreateOkResponse(Variables.Select(variable => new SifVariableGlobalDto
                    {
                        variable = variable,
                        valor = valores[variable]
                    }).ToList());
                }
            }
            catch (IOException ex)
            {
                return DbHelper.CreateErrorResponse<List<SifVariableGlobalDto>>(
                    $"No fue posible leer las variables globales. {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                return DbHelper.CreateErrorResponse<List<SifVariableGlobalDto>>(
                    $"No fue posible leer las variables globales. {ex.Message}");
            }
            catch (System.Security.SecurityException ex)
            {
                return DbHelper.CreateErrorResponse<List<SifVariableGlobalDto>>(
                    $"No fue posible leer las variables globales. {ex.Message}");
            }
            catch (CryptographicException ex)
            {
                return DbHelper.CreateErrorResponse<List<SifVariableGlobalDto>>(
                    $"No fue posible leer las variables globales. {ex.Message}");
            }
            catch (FormatException ex)
            {
                return DbHelper.CreateErrorResponse<List<SifVariableGlobalDto>>(
                    $"No fue posible leer las variables globales. {ex.Message}");
            }
        }

        public ErrorDto Guardar(int codEmpresa, string usuario, SifVariableGlobalDto dato)
        {
            if (dato is null)
                return DbHelper.ErrorResponse("Debe indicar la variable global que desea actualizar.", -2);

            var variable = Variables.FirstOrDefault(x =>
                string.Equals(x, dato.variable?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (variable is null)
                return DbHelper.ErrorResponse("La variable global indicada no es válida.", -2);

            var valor = (dato.valor ?? string.Empty).Trim();
            if (valor.Contains('\r') || valor.Contains('\n'))
                return DbHelper.ErrorResponse("El valor no puede contener saltos de línea.", -2);

            if (variable == PuertosDisponibles &&
                valor.Any(c => !char.IsDigit(c) && c != ','))
            {
                return DbHelper.ErrorResponse(
                    "Puertos Disponibles solo acepta números separados por comas.", -2);
            }

            try
            {
                lock (ArchivoLock)
                {
                    var valores = LeerValores();
                    valores[variable] = valor;
                    EscribirValores(valores);
                }

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Variable global: {variable}",
                    Movimiento = "Modifica - WEB",
                    Modulo = Modulo
                });

                return DbHelper.OkResponse("Variable global actualizada correctamente.");
            }
            catch (FileNotFoundException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
            catch (IOException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
            catch (CryptographicException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
            catch (FormatException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(
                    $"No fue posible guardar la variable global. {ex.Message}");
            }
        }

        private Dictionary<string, string> LeerValores()
        {
            if (!File.Exists(_rutaArchivo))
                throw new FileNotFoundException("No se encontró el archivo Global.ini configurado.", _rutaArchivo);

            var valores = Variables.ToDictionary(x => x, _ => string.Empty, StringComparer.OrdinalIgnoreCase);

            foreach (var linea in File.ReadAllLines(_rutaArchivo, Encoding.Latin1))
            {
                var separador = linea.IndexOf('=');
                if (separador < 0)
                    continue;

                var nombre = linea[..separador].Trim();
                var variable = Variables.FirstOrDefault(x =>
                    string.Equals(x, nombre, StringComparison.OrdinalIgnoreCase));
                if (variable is null)
                    continue;

                var valor = linea[(separador + 1)..].Trim();
                valores[variable] = variable == PuertosDisponibles
                    ? DesencriptarNumero(valor)
                    : valor;
            }

            return valores;
        }

        private void EscribirValores(IReadOnlyDictionary<string, string> valores)
        {
            var directorio = Path.GetDirectoryName(_rutaArchivo);
            if (string.IsNullOrWhiteSpace(directorio) || !Directory.Exists(directorio))
                throw new DirectoryNotFoundException("No existe el directorio configurado para Global.ini.");

            var lineas = Variables.Select(variable =>
            {
                var valor = valores.TryGetValue(variable, out var actual) ? actual : string.Empty;
                if (variable == PuertosDisponibles)
                    valor = EncriptarNumero(valor);

                return $"{variable.PadRight(30)}={valor}";
            });

            var nombreTemporal = Path.ChangeExtension(Path.GetRandomFileName(), ".tmp");
            if (Path.IsPathRooted(nombreTemporal))
                throw new InvalidOperationException("El nombre temporal generado no es válido.");

            var temporal = Path.Combine(directorio, nombreTemporal);
            try
            {
                File.WriteAllLines(temporal, lineas, Encoding.Latin1);
                File.Move(temporal, _rutaArchivo, true);
            }
            finally
            {
                if (File.Exists(temporal))
                    File.Delete(temporal);
            }
        }

        private static string EncriptarNumero(string valor)
        {
            var salida = new StringBuilder();
            foreach (var caracter in valor)
            {
                if (!char.IsDigit(caracter))
                {
                    salida.Append('.');
                    continue;
                }

                salida.Append(RandomNumberGenerator.GetInt32(1000, 10000));
                salida.Append(caracter switch
                {
                    '0' => '8', '1' => '6', '2' => '0', '3' => '1', '4' => '9',
                    '5' => '7', '6' => '2', '7' => '3', '8' => '5', '9' => '4',
                    _ => throw new InvalidOperationException("Dígito inválido.")
                });
            }

            return salida.ToString();
        }

        private static string DesencriptarNumero(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            var salida = new StringBuilder();
            var indice = 0;
            while (indice < valor.Length)
            {
                if (valor[indice] == '.')
                {
                    salida.Append(',');
                    indice++;
                    continue;
                }

                if (indice + 4 >= valor.Length)
                    throw new InvalidDataException("El valor cifrado de Puertos Disponibles no tiene un formato válido.");

                salida.Append(valor[indice + 4] switch
                {
                    '8' => '0', '6' => '1', '0' => '2', '1' => '3', '9' => '4',
                    '7' => '5', '2' => '6', '3' => '7', '5' => '8', '4' => '9',
                    _ => throw new InvalidDataException("El valor cifrado de Puertos Disponibles contiene un dígito inválido.")
                });
                indice += 5;
            }

            return salida.ToString();
        }
    }
}
