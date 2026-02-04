using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        private const string RdlcExt = ".rdlc";
        private const string RdlExt = ".rdl";

        public string GetBasePath(int codEmpresa, string dirRdlc, string? folder = null)
        {
            // Opcional: sanitizar folder si viene de usuario (misma estrategia que reportNameOrRelative)
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(dirRdlc, codEmpresa.ToString())
                : Path.Combine(dirRdlc, codEmpresa.ToString(), folder);
        }

        public string ResolveReportPath(string basePath, string reportNameOrRelative)
        {
            if (!TryNormalizeInputs(basePath, reportNameOrRelative, out var baseFull, out var rel, out var relDir, out var bare))
                return string.Empty;

            var dir = CombineUnderBase(baseFull, relDir);
            if (dir is null)
                return string.Empty;

            var found = BuildCandidates(baseFull, rel, relDir, bare)
                .Where(p => p is not null)
                .Cast<string>()
                .FirstOrDefault(File.Exists);

            if (!string.IsNullOrEmpty(found))
                return found;

            var enumerated = FindByEnumeration(dir, bare);
            return !string.IsNullOrEmpty(enumerated) ? enumerated : string.Empty;
        }

        private static bool TryNormalizeInputs(
            string basePath,
            string reportNameOrRelative,
            out string baseFull,
            out string rel,
            out string relDir,
            out string bare)
        {
            baseFull = rel = relDir = bare = string.Empty;

            if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(reportNameOrRelative))
                return false;

            baseFull = EnsureTrailingSeparator(Path.GetFullPath(basePath));

            rel = reportNameOrRelative
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .Trim();

            // Bloquea rutas absolutas/UNC
            if (Path.IsPathRooted(rel))
                return false;

            relDir = Path.GetDirectoryName(rel) ?? string.Empty;
            bare = Path.GetFileName(rel);

            // bloquea cosas raras tipo "." ".." o nombre vacío
            if (string.IsNullOrWhiteSpace(bare) || bare == "." || bare == "..")
                return false;

            return true;
        }

        private static IEnumerable<string?> BuildCandidates(string baseFull, string rel, string relDir, string bare)
        {
            // 1) "rel" completo (con subcarpetas) + extensiones
            yield return CombineUnderBase(baseFull, rel + RdlcExt);
            yield return CombineUnderBase(baseFull, rel + RdlExt);

            // 2) "bare" + extensiones dentro de relDir
            var bareInDir = string.IsNullOrEmpty(relDir)
                ? bare
                : relDir + Path.DirectorySeparatorChar + bare;

            yield return CombineUnderBase(baseFull, bareInDir + RdlcExt);
            yield return CombineUnderBase(baseFull, bareInDir + RdlExt);
        }

        private static string FindByEnumeration(string dir, string bare)
        {
            if (!Directory.Exists(dir))
                return string.Empty;

            var found = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f => IsReportMatch(f, bare));

            if (string.IsNullOrEmpty(found))
                return string.Empty;

            var full = Path.GetFullPath(found);
            return IsUnderBase(dir, full) ? full : string.Empty;
        }

        private static bool IsReportMatch(string filePath, string bare)
        {
            if (!string.Equals(Path.GetFileNameWithoutExtension(filePath), bare, StringComparison.OrdinalIgnoreCase))
                return false;

            var ext = Path.GetExtension(filePath);
            return string.Equals(ext, RdlcExt, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(ext, RdlExt, StringComparison.OrdinalIgnoreCase);
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
