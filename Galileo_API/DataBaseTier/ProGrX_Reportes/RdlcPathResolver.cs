namespace Galileo.DataBaseTier.ProGrX_Reportes
{
    public sealed class RdlcPathResolver : IRdlcPathResolver
    {
        public string GetBasePath(int codEmpresa, string? folder, string dirRdlc)
        {
            return string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(dirRdlc, codEmpresa.ToString())
                : Path.Combine(dirRdlc, codEmpresa.ToString(), folder);
        }

        public string? ResolveReportPath(string basePath, string reportNameOrRelative)
        {
            var rel = reportNameOrRelative
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);

            var dir = Path.Combine(basePath, Path.GetDirectoryName(rel) ?? string.Empty);
            var bare = Path.GetFileName(rel);

            var candidates = new[]
            {
                Path.Combine(basePath, rel + ".rdlc"),
                Path.Combine(basePath, rel + ".rdl"),
                Path.Combine(dir, bare + ".rdlc"),
                Path.Combine(dir, bare + ".rdl"),
            };
            var foundCandidate = candidates.FirstOrDefault(c => File.Exists(c));
            if (foundCandidate != null)
                return foundCandidate;

            if (Directory.Exists(dir))
            {
                var foundFile = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(f =>
                        string.Equals(Path.GetFileNameWithoutExtension(f), bare, System.StringComparison.OrdinalIgnoreCase) &&
                        (string.Equals(Path.GetExtension(f), ".rdlc", System.StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(Path.GetExtension(f), ".rdl", System.StringComparison.OrdinalIgnoreCase))
                    );
                if (foundFile != null)
                    return foundFile;
            }

            return null;
        }
    }
}
