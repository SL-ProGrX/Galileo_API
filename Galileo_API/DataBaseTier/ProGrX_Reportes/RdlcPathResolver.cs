using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        private static readonly string [] exts = new[] { ".rdlc", ".RDLC", ".rdl", ".RDL" };

        public string GetBasePath(int codEmpresa, string dirRdlc, string? folder = null)
        {
            // Opcional: sanitizar folder si viene de usuario (misma estrategia que reportNameOrRelative)
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(dirRdlc, codEmpresa.ToString())
                : Path.Combine(dirRdlc, codEmpresa.ToString(), folder);
        }

        public string ResolveReportPath(string basePath, string reportNameOrRelative)
        {
            if (!TryNormalizeInputs(
                    basePath,
                    reportNameOrRelative,
                    out var baseFull,
                    out var bare))
                return string.Empty;

            // Ruta base combinada con el input (puede ser "stem" o carpeta)
            var dirOrStem = CombineUnderBase(baseFull, reportNameOrRelative);
            if (string.IsNullOrEmpty(dirOrStem))
                return string.Empty;

            // 1) Intento directo: "<stem>.ext"
            // Ej: C:\...\Banking_BoletaRegistro.rdlc
            var found = exts
                .Select(ext => dirOrStem + ext)
                .FirstOrDefault(File.Exists);

            if (!string.IsNullOrEmpty(found))
                return found;

            // 2) Si es carpeta: "<carpeta>\<carpeta>.ext"
            // Ej: C:\...\Banking_BoletaRegistro\Banking_BoletaRegistro.rdlc
            if (Directory.Exists(dirOrStem))
            {
                var folderName = Path.GetFileName(
                    dirOrStem.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar));

                found = exts
                    .Select(ext => Path.Combine(dirOrStem, folderName + ext))
                    .FirstOrDefault(File.Exists);

                if (!string.IsNullOrEmpty(found))
                    return found;

                // 3) Enumeración acotada dentro de la carpeta: "bare.*"
                found = Directory
                    .EnumerateFiles(dirOrStem, bare + ".*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f =>
                        exts.Any(ext => f.EndsWith(ext, StringComparison.Ordinal)));

                return found ?? string.Empty;
            }

            // 4) No es carpeta → enumera en el directorio padre
            var parentDir = Path.GetDirectoryName(dirOrStem);
            if (string.IsNullOrEmpty(parentDir) || !Directory.Exists(parentDir))
                return string.Empty;

            found = Directory
                .EnumerateFiles(parentDir, bare + ".*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f =>
                    exts.Any(ext => f.EndsWith(ext, StringComparison.Ordinal)));

            return found ?? string.Empty;
        }

        private static bool TryNormalizeInputs(
            string basePath,
            string reportNameOrRelative,
            out string baseFull,
            out string bare)
        {
            string relDir = string.Empty;
            string rel = string.Empty;
            baseFull = bare = string.Empty;

            if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(reportNameOrRelative))
                return false;

            baseFull = EnsureTrailingSeparator(Path.GetFullPath(basePath));

            rel = reportNameOrRelative
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .Trim();
            // Extra: bloquea caracteres inválidos
            if (rel.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                return false;

            // Bloquea rutas absolutas/UNC
            if (Path.IsPathRooted(rel))
                return false;

            bare = Path.GetFileName(rel);

            // bloquea cosas raras tipo "." ".." o nombre vacío
            if (string.IsNullOrWhiteSpace(bare) || bare == "." || bare == "..")
                return false;

            return true;
        }

        private static string EnsureTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            return path.EndsWith(Path.DirectorySeparatorChar)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Verifica que candidateFull esté dentro de baseFull (misma raíz),
        /// usando un check robusto basado en rutas relativas.
        /// </summary>
        private static bool IsUnderBase(string baseFullWithSep, string candidateFull)
        {
            // Ambos deberían ser absolutos
            var relative = Path.GetRelativePath(baseFullWithSep, candidateFull);

            // Si empieza con ".." o es rooted, se salió del base
            return !relative.StartsWith("..", StringComparison.Ordinal) &&
                   !Path.IsPathRooted(relative);
        }

        /// <summary>
        /// Combina y normaliza una ruta relativa bajo un base absoluto. Si se sale del base, retorna null.
        /// </summary>
        private static string? CombineUnderBase(string baseFullWithSep, string relative)
        {
            relative = (relative ?? string.Empty).Trim()
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.IsNullOrEmpty(relative) || Path.IsPathRooted(relative))
                return null;

            var combinedFull = Path.GetFullPath(Path.Combine(baseFullWithSep, relative));
            return IsUnderBase(baseFullWithSep, combinedFull) ? combinedFull : null;
        }
    }
}
