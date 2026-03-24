using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        private static readonly string[] AllowedExtensions = new[] { ".rdlc", ".rdl" };

        /// <summary>
        /// Construye la ruta base de reportes dentro de la carpeta controlada por empresa.
        /// </summary>
        public string GetBasePath(int codEmpresa, string dirRdlc, string? folder = null)
        {
            if (codEmpresa <= 0)
            {
                throw new SecurityException("El código de empresa no es válido.");
            }

            if (!string.IsNullOrWhiteSpace(folder) && Path.IsPathRooted(folder))
            {
                throw new SecurityException("La carpeta especificada no es válida.");
            }

            var root = Path.GetFullPath(dirRdlc);

            string? normalizedFolder = null;
            if (!string.IsNullOrWhiteSpace(folder))
            {
                // Evita que una ruta absoluta en 'folder' haga que Path.Combine ignore 'root' y 'codEmpresa'.
                normalizedFolder = Path.IsPathRooted(folder)
                    ? Path.GetFileName(folder)
                    : folder;
            }

            var basePath = string.IsNullOrWhiteSpace(normalizedFolder)
                ? Path.Combine(root, codEmpresa.ToString())
                : Path.Combine(root, codEmpresa.ToString(), normalizedFolder);

            return Path.GetFullPath(basePath);
        }

        /// <summary>
        /// Resuelve la ruta final del reporte usando únicamente extensiones permitidas.
        /// </summary>
        public string ResolveReportPath(string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
            {
                throw new SecurityException("La ruta base del reporte es requerida.");
            }

            var normalizedBasePath = Path.GetFullPath(basePath);
            var directory = Path.GetDirectoryName(normalizedBasePath);

            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                return string.Empty;
            }

            var reportName = Path.GetFileNameWithoutExtension(normalizedBasePath);
            if (string.IsNullOrWhiteSpace(reportName))
            {
                throw new SecurityException("El nombre del reporte no es válido.");
            }

            foreach (var extension in AllowedExtensions)
            {
                var candidatePath = Path.GetFullPath(Path.Combine(directory, reportName + extension));

                if (!IsUnderDirectory(directory, candidatePath))
                {
                    throw new SecurityException("La ruta del reporte no es válida.");
                }

                if (File.Exists(candidatePath))
                {
                    return candidatePath;
                }
            }

            return string.Empty;
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
    }
}
