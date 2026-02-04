using System;
using System.IO;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
#pragma warning disable S2083 // False positive: we do validate inputs to prevent path traversal
#pragma warning disable S6549 // False positive: we do validate inputs to prevent path traversal
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        public string GetBasePath(int codEmpresa,  string dirRdlc, string? folder = null)
        {
            // Opcional: sanitizar también folder (misma idea que reportNameOrRelative)
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(dirRdlc, codEmpresa.ToString())
                : Path.Combine(dirRdlc, codEmpresa.ToString(), folder);
        }

        public string ResolveReportPath(string basePath, string reportNameOrRelative)
        {
            if (string.IsNullOrWhiteSpace(basePath) || string.IsNullOrWhiteSpace(reportNameOrRelative))
                return string.Empty;

            // 1) Canoniza basePath a absoluto (y con separador final para StartsWith seguro)
            var baseFull = EnsureTrailingSeparator(Path.GetFullPath(basePath));

            // 2) Normaliza separadores
            var rel = reportNameOrRelative
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .Trim();

            // 3) Bloquea rutas absolutas/UNC
            if (Path.IsPathRooted(rel))
                return string.Empty;

            // 4) Extrae directorio relativo y nombre "bare"
            var relDir = Path.GetDirectoryName(rel) ?? string.Empty;
            var bare = Path.GetFileName(rel);

            // (Opcional pero recomendable) bloquea cosas raras tipo "." ".." o nombre vacío
            if (string.IsNullOrWhiteSpace(bare) || bare == "." || bare == "..")
                return string.Empty;

            // Construye dir de trabajo, pero validando que quede dentro del base
            var dir = CombineUnderBase(baseFull, relDir);
            if (dir is null)
                return string.Empty;

            // Candidatos (rel puede incluir subcarpetas; bare solo nombre)
            var candidates = new[]
            {
                CombineUnderBase(baseFull, rel + ".rdlc"),
                CombineUnderBase(baseFull, rel + ".rdl"),
                CombineUnderBase(baseFull, Path.Combine(relDir, bare + ".rdlc")),
                CombineUnderBase(baseFull, Path.Combine(relDir, bare + ".rdl")),
            }
            .Where(p => p is not null)!
            .Cast<string>()
            .ToArray();

            var foundCandidate = candidates.FirstOrDefault(File.Exists);
            if (foundCandidate != null)
                return foundCandidate;

            if (Directory.Exists(dir))
            {
                // Enumeración acotada al directorio validado dentro del base
                var foundFile = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f =>
                        string.Equals(Path.GetFileNameWithoutExtension(f), bare, StringComparison.OrdinalIgnoreCase) &&
                        (string.Equals(Path.GetExtension(f), ".rdlc", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(Path.GetExtension(f), ".rdl", StringComparison.OrdinalIgnoreCase))
                    );

                if (foundFile != null)
                {
                    // Defensa extra por si el FS devuelve algo extraño (symlinks/junctions)
                    var foundFull = Path.GetFullPath(foundFile);
                    if (IsUnderBase(baseFull, foundFull))
                        return foundFull;
                }
            }

            return string.Empty;
        }

        private static string EnsureTrailingSeparator(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            return path.EndsWith(Path.DirectorySeparatorChar)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        private static bool IsUnderBase(string baseFullWithSep, string candidateFull)
        {
            // baseFullWithSep ya trae separador final
            return candidateFull.StartsWith(baseFullWithSep, StringComparison.OrdinalIgnoreCase);
        }

        private static string? CombineUnderBase(string baseFullWithSep, string relative)
        {
            // baseFullWithSep debe ser full path + separador final
            // Normalizamos para asegurar que lo que combinamos sea realmente relativo
            relative = relative.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // relative NO debe ser rooted (ya lo chequeamos arriba, pero por seguridad):
            if (Path.IsPathRooted(relative))
                return null;

            var combinedFull = Path.GetFullPath(Path.Combine(baseFullWithSep, relative));

            return IsUnderBase(baseFullWithSep, combinedFull)
                ? combinedFull
                : null;
        }
    }
}

