using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        private static readonly string[] AllowedExtensions = new[] { ".rdlc", ".rdl" };
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Construye la ruta base de reportes dentro de la carpeta controlada por empresa.
        /// </summary>
        public string GetBasePath(int codEmpresa, string dirRdlc, string? folder = null)
        {
            string empresa = string.Empty;
            if (codEmpresa <= 0)
            {
                empresa = "ProGrx";
            }

            empresa = codEmpresa.ToString();

            if (!string.IsNullOrWhiteSpace(folder) && Path.IsPathRooted(folder))
            {
                throw new SecurityException("La carpeta especificada no es válida.");
            }

            var root = Path.GetFullPath(dirRdlc);

            // Normaliza el código de empresa para que solo se use como segmento de ruta.
            var empresaSegment = Path.GetFileName(empresa);

            // Normaliza la carpeta opcional para que se use solo como segmento de ruta.
            string? safeFolder = null;
            if (!string.IsNullOrWhiteSpace(folder))
            {
                safeFolder = Path.GetFileName(folder);
            }

            var trimmedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string basePath;
            if (string.IsNullOrWhiteSpace(safeFolder))
            {
                basePath = trimmedRoot + Path.DirectorySeparatorChar + empresaSegment;
            }
            else
            {
                basePath = trimmedRoot + Path.DirectorySeparatorChar + empresaSegment
                    + Path.DirectorySeparatorChar + safeFolder;
            }

            //Valido si el forder existe, si no existe reemplazo codEmpresa por ProGrx
            if(!Directory.Exists(basePath))
            {
                empresaSegment = "ProGrx";
                if (string.IsNullOrWhiteSpace(safeFolder))
                {
                    basePath = trimmedRoot + Path.DirectorySeparatorChar + empresaSegment;
                }
                else
                {
                    basePath = trimmedRoot + Path.DirectorySeparatorChar + empresaSegment
                        + Path.DirectorySeparatorChar + safeFolder;
                }
            }

            return Path.GetFullPath(basePath);
        }

        /// <summary>
        /// Resuelve la ruta final del reporte usando únicamente extensiones permitidas.
        /// </summary>
        public string ResolveReportPath(int codEmpresa, string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                throw new SecurityException("La ruta base del reporte es requerida.");

            var normalizedBasePath = Path.GetFullPath(basePath);
            var directory = Path.GetDirectoryName(normalizedBasePath);
            var normalizedDirectory = string.IsNullOrWhiteSpace(directory) ? string.Empty : Path.GetFullPath(directory);

            if (string.IsNullOrWhiteSpace(normalizedDirectory) || !Directory.Exists(normalizedDirectory))
                return string.Empty;

            var requestedName = NormalizeSingleSegment(
                Path.GetFileNameWithoutExtension(normalizedBasePath),
                "nombreReporte");

            var match = Directory
                .EnumerateFiles(normalizedDirectory)
                .Select(Path.GetFullPath)
                .Where(path => IsUnderDirectory(normalizedDirectory, path))
                .Where(path => AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                .FirstOrDefault(path =>
                    string.Equals(
                        Path.GetFileNameWithoutExtension(path),
                        requestedName,
                        StringComparison.OrdinalIgnoreCase));
            if(match == null)
            {
                normalizedDirectory = validaRutaFinal(normalizedDirectory, codEmpresa);
                match = Directory
                .EnumerateFiles(normalizedDirectory)
                .Select(Path.GetFullPath)
                .Where(path => IsUnderDirectory(normalizedDirectory, path))
                .Where(path => AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
                .FirstOrDefault(path =>
                    string.Equals(
                        Path.GetFileNameWithoutExtension(path),
                        requestedName,
                        StringComparison.OrdinalIgnoreCase));
            }

            

            return match ?? string.Empty;
        }


        private static string NormalizeSingleSegment(string? value, string paramName)
        {
            var normalized = (value ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                throw new SecurityException($"{paramName} requerido.");

            if (normalized.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
                throw new SecurityException($"{paramName} inválido.");

            if (normalized.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(normalized))
                throw new SecurityException($"{paramName} inválido.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                    normalized,
                    @"^[A-Za-z0-9_.-]+$",
                    RegexOptions.None,
                    RegexTimeout))
            {
                throw new SecurityException($"{paramName} inválido.");
            }

            return normalized;
        }

        /// <summary>
        /// Combina segmentos bajo una raíz controlada validando que la ruta final no salga de ella.
        /// </summary>
        public string CombineUnderRoot(string basePath, params string[] reportFile)
        {
            var rootFull = Path.GetFullPath(basePath);
            var combined = reportFile.Aggregate(rootFull, Path.Combine);
            var full = Path.GetFullPath(combined);

            if (!IsUnderDirectory(rootFull, full))
            {
                throw new SecurityException("Path traversal detectado.");
            }

            return full;
        }

        /// <summary>
        /// Valida que una ruta permanezca dentro de un directorio raíz controlado.
        /// </summary>
        private static bool IsUnderDirectory(string rootPath, string candidatePath)
        {
            var normalizedRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            var normalizedCandidate = Path.GetFullPath(candidatePath);

            return normalizedCandidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private string validaRutaFinal(string ruta, int codEmpresa)
        {
            //valido si la ruta final con el documento existe si no busco el archivo en la carpeta ProGrx
            if(!File.Exists(ruta))
            {
                ruta = ruta.Replace(codEmpresa.ToString(), "ProGrx");
            }
            return ruta;
        }
    }
}
