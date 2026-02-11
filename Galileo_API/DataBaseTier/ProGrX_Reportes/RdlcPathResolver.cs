using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        private static readonly string [] exts = new[] { ".rdlc",  ".rdl" };

        public string GetBasePath(int codEmpresa, string dirRdlc, string? folder = null)
        {
            // Opcional: sanitizar folder si viene de usuario (misma estrategia que reportNameOrRelative)
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(dirRdlc, codEmpresa.ToString())
                : Path.Combine(dirRdlc, codEmpresa.ToString(), folder);
        }

        public string ResolveReportPath(string basePath)
        {
            var dir = Path.GetDirectoryName(basePath);
            var bare = Path.GetFileName(basePath);

            var found = Directory
                .EnumerateFiles(dir!, bare + ".*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(f =>
                    exts.Any(ext =>
                        f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

            return found ?? string.Empty;

        }

        public string CombineUnderRoot(string basePath, params string[] reportFile)
        {
            var rootFull = Path.GetFullPath(basePath);
            var combined = reportFile.Aggregate(rootFull, Path.Combine);
            var full = Path.GetFullPath(combined);

            var rootWithSep = rootFull.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
                throw new SecurityException("Path traversal detectado.");

            return full;
        }
    }
}
