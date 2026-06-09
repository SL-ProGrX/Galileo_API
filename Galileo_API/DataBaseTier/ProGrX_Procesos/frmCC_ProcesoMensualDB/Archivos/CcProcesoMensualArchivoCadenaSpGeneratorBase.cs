using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using Dapper;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public abstract class CcProcesoMensualArchivoCadenaSpGeneratorBase : ICcProcesoMensualArchivoGenerator
    {
        protected const string ContentTypeText = "text/plain";
        public abstract IReadOnlyCollection<string> CodigosPlanillaEnvio { get; }
        protected abstract string CodigoPlanillaEnvio { get; }
        protected abstract string CodigoFormato { get; }
        protected abstract string ExtensionArchivo { get; }
        protected abstract string ContentType { get; }
        protected abstract string QueryCadenas { get; }
        protected virtual Encoding EncodingArchivo => Encoding.GetEncoding(1252);
        private readonly ArchivosGeneradosOptions _archivosOptions;


        public CcProcesoMensualArchivoCadenaSpGeneratorBase(IOptions<ArchivosGeneradosOptions> archivosOptions)
        {
            _archivosOptions = archivosOptions.Value;
        }


        public virtual CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var configuracion = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerConfiguracionGeneral(
                connection,
                request.CodInstitucion);

            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CrearNombreArchivoEstandar(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor,
                CodigoFormato,
                ExtensionArchivo);

            var rutaBase = _archivosOptions.RutaBase;

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request, rutaBase);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaBase, rutaDirectorio, nombreArchivo);

            var cadenas = ObtenerCadenas(connection, request);

            var contenido = CrearContenidoArchivo(cadenas);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(rutaBase, rutaDirectorio, rutaArchivo, contenido, EncodingArchivo);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentType,
                ArchivoBytes = [],
                ArchivosGenerados = [rutaArchivo]
            };
        }
        protected virtual IEnumerable<string> ObtenerCadenas(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            return connection.Query<CcProcesoMensualArchivoCadenaDbModel>(
                    QueryCadenas,
                    new
                    {
                        request.CodInstitucion,
                        request.FechaProceso
                    })
                .Select(x => x.Cadena ?? string.Empty);
        }
        protected virtual string CrearContenidoArchivo(IEnumerable<string> cadenas)
        {
            var builder = new StringBuilder();

            foreach (var cadena in cadenas.Where(cadena => cadena.TrimEnd().Length > 0))
            {
                builder.AppendLine(cadena);
            }

            return builder.ToString();
        }
        private sealed class CcProcesoMensualArchivoCadenaDbModel
        {
            public string Cadena { get; set; } = string.Empty;
        }
    }
}
