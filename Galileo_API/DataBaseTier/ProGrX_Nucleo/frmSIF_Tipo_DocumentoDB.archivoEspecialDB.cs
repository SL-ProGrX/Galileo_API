using Dapper;
using Galileo.DataBaseTier.ProGrX_Reportes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    /// <summary>
    /// Manejo del Archivo Especial (plantilla de reporte personalizada) del tipo de documento.
    /// Equivale a btnImagenes_Click del VB6, pero además deposita el archivo en el
    /// repositorio de reportes resuelto por el parámetro Rep01 y permite descargarlo.
    /// </summary>
    public partial class FrmSifTipoDocumentoDB
    {
        private const string ParametroRutaReportes = "Rep01";
        private const string CarpetaArchivoEspecial = "Sys";
        private const long TamanoMaximoArchivo = 10 * 1024 * 1024;
        private const int MaxRespaldosPorDia = 99;
        private static readonly string[] ExtensionesPermitidas = { ".rdl", ".rdlc" };

        /// <summary>
        /// Guarda el archivo especial en el repositorio de reportes y devuelve el nombre
        /// y la carpeta donde quedó. Si el archivo ya existe en el repositorio se conserva
        /// su ubicación actual y, si no se confirmó la sobrescritura, devuelve Code = -4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="sobrescribir"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ErrorDto<SifTipoDocumentoArchivoData> SIF_tipoDocumento_ArchivoEspecial_Guardar(
            int CodEmpresa,
            string usuario,
            bool sobrescribir,
            IFormFile file)
        {
            var validacion = ValidarArchivoEspecial(file);
            if (validacion.Code != 0)
            {
                return ErrorArchivo(validacion.Description ?? "Archivo inválido.");
            }

            try
            {
                var raiz = SIF_tipoDocumento_RutaReportes_Obtener(CodEmpresa);
                if (string.IsNullOrWhiteSpace(raiz))
                {
                    return ErrorArchivo($"No se pudo obtener la ruta de reportes (parámetro {ParametroRutaReportes}).");
                }

                var nombreArchivo = Path.GetFileName(file.FileName);
                var existente = BuscarArchivoEspecial(CodEmpresa, raiz, nombreArchivo);

                if (existente != null && !sobrescribir)
                {
                    return new ErrorDto<SifTipoDocumentoArchivoData>
                    {
                        Code = -4,
                        Description = "El archivo ya existe en el repositorio de reportes.",
                        Result = ArmarArchivoData(raiz, existente, true)
                    };
                }

                var destino = existente ?? DestinoPorOmision(CodEmpresa, raiz, nombreArchivo);
                var respaldo = existente != null ? RespaldarArchivoExistente(existente) : string.Empty;

                CopiarArchivo(file, destino);

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = existente != null
                        ? $"Archivo Especial: {nombreArchivo}, Respaldo: {respaldo}"
                        : $"Archivo Especial: {nombreArchivo}",
                    Movimiento = existente != null ? "Modifica - WEB" : "Registra - WEB",
                    Modulo = vModulo
                });

                var data = ArmarArchivoData(raiz, destino, true);
                data.respaldo = respaldo;

                return new ErrorDto<SifTipoDocumentoArchivoData>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = data
                };
            }
            catch (Exception ex)
            {
                return ErrorArchivo(ex.Message);
            }
        }


        /// <summary>
        /// Localiza el archivo especial configurado y devuelve su contenido para descarga.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="archivo"></param>
        /// <returns></returns>
        public ErrorDto<SifTipoDocumentoArchivoDescargaData> SIF_tipoDocumento_ArchivoEspecial_Descargar(
            int CodEmpresa,
            string archivo)
        {
            var result = new ErrorDto<SifTipoDocumentoArchivoDescargaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new SifTipoDocumentoArchivoDescargaData()
            };

            try
            {
                var nombreArchivo = Path.GetFileName(archivo ?? string.Empty);

                if (string.IsNullOrWhiteSpace(nombreArchivo) || !EsNombreSeguro(nombreArchivo))
                {
                    result.Code = -1;
                    result.Description = "Nombre de archivo inválido.";
                    result.Result = null;
                    return result;
                }

                var raiz = SIF_tipoDocumento_RutaReportes_Obtener(CodEmpresa);
                if (string.IsNullOrWhiteSpace(raiz))
                {
                    result.Code = -1;
                    result.Description = $"No se pudo obtener la ruta de reportes (parámetro {ParametroRutaReportes}).";
                    result.Result = null;
                    return result;
                }

                var origen = BuscarArchivoEspecial(CodEmpresa, raiz, nombreArchivo);

                if (origen == null)
                {
                    result.Code = -5;
                    result.Description = $"El archivo {nombreArchivo} no se encontró en el repositorio de reportes.";
                    result.Result = null;
                    return result;
                }

                // CxSuppress: PathTraversal -> ruta validada por BuscarArchivoEspecial/EstaBajoDirectorio
                result.Result.contenido = File.ReadAllBytes(origen);
                result.Result.nombre_archivo = nombreArchivo;
                result.Result.carpeta = CarpetaRelativa(raiz, origen);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Obtiene la ruta raíz del repositorio de reportes (SIF_PARAMETROS.Rep01).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        private string SIF_tipoDocumento_RutaReportes_Obtener(int CodEmpresa)
        {
            var info = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, conn =>
            {
                const string query = "SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = @Code";
                return conn.Query<string>(query, new { Code = ParametroRutaReportes }).FirstOrDefault() ?? string.Empty;
            });

            return info.Code == 0 ? (info.Result ?? string.Empty) : string.Empty;
        }


        /// <summary>
        /// Busca el archivo en las carpetas preferidas y, si no aparece, en todo el árbol
        /// del repositorio de reportes. Devuelve null si no existe.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="raiz"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        private static string? BuscarArchivoEspecial(int CodEmpresa, string raiz, string nombreArchivo)
        {
            foreach (var carpeta in CarpetasPreferidas(CodEmpresa, raiz))
            {
                if (!Directory.Exists(carpeta))
                {
                    continue;
                }

                var candidato = Path.GetFullPath(Path.Combine(carpeta, nombreArchivo));

                if (EstaBajoDirectorio(carpeta, candidato) && File.Exists(candidato))
                {
                    return candidato;
                }
            }

            return BuscarEnArbol(raiz, nombreArchivo);
        }


        /// <summary>
        /// Carpetas candidatas en orden de preferencia: Sys de la empresa, raíz de la empresa
        /// y raíz absoluta del repositorio de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="raiz"></param>
        /// <returns></returns>
        private static IEnumerable<string> CarpetasPreferidas(int CodEmpresa, string raiz)
        {
            var resolver = new RdlcPathResolver();

            yield return resolver.GetBasePath(CodEmpresa, raiz, CarpetaArchivoEspecial);
            yield return resolver.GetBasePath(CodEmpresa, raiz, null);
            yield return Path.GetFullPath(raiz);
        }


        /// <summary>
        /// Recorre recursivamente el repositorio de reportes buscando el archivo por nombre.
        /// </summary>
        /// <param name="raiz"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        private static string? BuscarEnArbol(string raiz, string nombreArchivo)
        {
            var raizCompleta = Path.GetFullPath(raiz);

            if (!Directory.Exists(raizCompleta))
            {
                return null;
            }

            return Directory
                .EnumerateFiles(raizCompleta, nombreArchivo, SearchOption.AllDirectories)
                .Select(Path.GetFullPath)
                .FirstOrDefault(ruta => EstaBajoDirectorio(raizCompleta, ruta));
        }


        /// <summary>
        /// Ruta destino cuando el archivo aún no existe en el repositorio de reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="raiz"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        private static string DestinoPorOmision(int CodEmpresa, string raiz, string nombreArchivo)
        {
            var resolver = new RdlcPathResolver();
            var basePath = resolver.GetBasePath(CodEmpresa, raiz, CarpetaArchivoEspecial);

            Directory.CreateDirectory(basePath);

            // CombineUnderRoot valida que el resultado no salga de la raíz (path traversal).
            return resolver.CombineUnderRoot(basePath, nombreArchivo);
        }


        /// <summary>
        /// Respalda el archivo que se va a reemplazar dejando una copia en la misma carpeta
        /// con el nombre original más la fecha actual (_yyyyMMdd). Si ya existe un respaldo
        /// del mismo día agrega un consecutivo para no perderlo.
        /// </summary>
        /// <param name="rutaArchivo"></param>
        /// <returns></returns>
        private static string RespaldarArchivoExistente(string rutaArchivo)
        {
            var carpeta = Path.GetDirectoryName(rutaArchivo) ?? string.Empty;
            var nombre = Path.GetFileNameWithoutExtension(rutaArchivo);
            var extension = Path.GetExtension(rutaArchivo);
            var fecha = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            var destino = Path.Combine(carpeta, $"{nombre}_{fecha}{extension}");

            for (var consecutivo = 1; File.Exists(destino) && consecutivo <= MaxRespaldosPorDia; consecutivo++)
            {
                destino = Path.Combine(carpeta, $"{nombre}_{fecha}_{consecutivo:00}{extension}");
            }

            if (File.Exists(destino))
            {
                throw new IOException($"Se alcanzó el máximo de respaldos diarios para {nombre}{extension}.");
            }

            File.Copy(rutaArchivo, destino);

            return Path.GetFileName(destino);
        }


        /// <summary>
        /// Copia el contenido del archivo recibido al destino resuelto.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="destino"></param>
        private static void CopiarArchivo(IFormFile file, string destino)
        {
            using var origen = file.OpenReadStream();
            using var salida = new FileStream(destino, FileMode.Create, FileAccess.Write, FileShare.None);
            origen.CopyTo(salida);
        }


        /// <summary>
        /// Valida el archivo recibido antes de escribirlo en el repositorio de reportes.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static ErrorDto ValidarArchivoEspecial(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return DbHelper.ErrorResponse("Archivo requerido.");
            }

            if (file.Length > TamanoMaximoArchivo)
            {
                return DbHelper.ErrorResponse("El archivo supera el tamaño máximo permitido (10 MB).");
            }

            var nombreArchivo = Path.GetFileName(file.FileName ?? string.Empty);

            if (string.IsNullOrWhiteSpace(nombreArchivo) || !EsNombreSeguro(nombreArchivo))
            {
                return DbHelper.ErrorResponse("Nombre de archivo inválido.");
            }

            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension))
            {
                return DbHelper.ErrorResponse("Extensión inválida. Solo se permiten archivos .rdl o .rdlc.");
            }

            return DbHelper.CreateOkResponse();
        }


        /// <summary>
        /// Confirma que el nombre recibido sea un segmento simple, sin rutas ni caracteres inválidos.
        /// </summary>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        private static bool EsNombreSeguro(string nombreArchivo)
        {
            return !nombreArchivo.Contains("..", StringComparison.Ordinal)
                && !Path.IsPathRooted(nombreArchivo)
                && nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }


        /// <summary>
        /// Valida que una ruta permanezca dentro de un directorio raíz controlado.
        /// </summary>
        /// <param name="raiz"></param>
        /// <param name="candidato"></param>
        /// <returns></returns>
        private static bool EstaBajoDirectorio(string raiz, string candidato)
        {
            var raizNormalizada = Path.GetFullPath(raiz)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            return Path.GetFullPath(candidato)
                .StartsWith(raizNormalizada, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Devuelve la carpeta del archivo relativa a la raíz de reportes,
        /// para no exponer rutas absolutas del servidor.
        /// </summary>
        /// <param name="raiz"></param>
        /// <param name="rutaArchivo"></param>
        /// <returns></returns>
        private static string CarpetaRelativa(string raiz, string rutaArchivo)
        {
            var carpeta = Path.GetDirectoryName(Path.GetFullPath(rutaArchivo)) ?? string.Empty;
            var raizCompleta = Path.GetFullPath(raiz).TrimEnd(Path.DirectorySeparatorChar);

            if (!EstaBajoDirectorio(raizCompleta, carpeta + Path.DirectorySeparatorChar))
            {
                return Path.GetFileName(carpeta);
            }

            return Path.GetRelativePath(raizCompleta, carpeta);
        }


        private static SifTipoDocumentoArchivoData ArmarArchivoData(string raiz, string rutaArchivo, bool existe)
        {
            return new SifTipoDocumentoArchivoData
            {
                nombre_archivo = Path.GetFileName(rutaArchivo),
                carpeta = CarpetaRelativa(raiz, rutaArchivo),
                existe = existe
            };
        }


        private static ErrorDto<SifTipoDocumentoArchivoData> ErrorArchivo(string mensaje)
        {
            return new ErrorDto<SifTipoDocumentoArchivoData>
            {
                Code = -1,
                Description = mensaje,
                Result = null
            };
        }
    }
}
