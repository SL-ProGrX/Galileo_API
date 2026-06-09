using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using Dapper;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{

    public abstract class CcProcesoMensualArchivoPlanoGenerarBase<TRegistro> : ICcProcesoMensualArchivoGenerator
    {
        protected const string ContentTypeText = "text/plain";
        protected const string ContentTypeCsv = "text/csv";
        public abstract IReadOnlyCollection<string> CodigosPlanillaEnvio { get; }
        protected abstract string CodigoPlanillaEnvio { get; }
        protected abstract string CodigoFormato { get; }
        protected abstract string ExtensionArchivo { get; }
        protected abstract string ContentType { get; }
        protected abstract string QueryRegistros { get; }
         
        protected virtual Encoding EncodingArchivo => Encoding.GetEncoding(1252);
        private readonly ArchivosGeneradosOptions _archivosOptions;
        protected CcProcesoMensualArchivoPlanoGenerarBase(IOptions<ArchivosGeneradosOptions> archivosOptions)
        {
            _archivosOptions = archivosOptions.Value;
        }


        public virtual CcProcesoMensualArchivoGeneradoModel GenerarArchivo( IDbConnection connection,CcProcesoMensualGeneraArchivoRequest request)
        {
            var nombreArchivo = CrearNombreArchivo(connection, request);
            
            var rutaBase = _archivosOptions.RutaBase;
            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request, rutaBase);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaBase,rutaDirectorio, nombreArchivo);

            var registros = ObtenerRegistros(connection, request);

            var contenido = CrearContenidoArchivo(registros, request);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(rutaBase, rutaDirectorio, rutaArchivo,contenido,EncodingArchivo);

            return CrearRespuesta( nombreArchivo,rutaArchivo, contenido);
        }

        protected virtual string CrearNombreArchivo(IDbConnection connection,CcProcesoMensualGeneraArchivoRequest request)
        {
            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            return Helpers.CcProcesoMensualArchivoRutaHelperDb.CrearNombreArchivoEstandar(
                request.CodInstitucion,
                request.FechaProceso,
                ObtenerCodigoInstDeduc(),
                fechaServidor,
                CodigoFormato,
                ExtensionArchivo);
        }

        protected virtual string ObtenerCodigoInstDeduc()
        {
            return string.Empty;
        }

        protected virtual IEnumerable<TRegistro> ObtenerRegistros( IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            return connection.Query<TRegistro>(
                QueryRegistros,
                CrearParametrosRegistros(request));
        }

        protected virtual object CrearParametrosRegistros(CcProcesoMensualGeneraArchivoRequest request)
        {
            return new
            {
                request.FechaProceso,
                request.CodInstitucion
            };
        }

        protected virtual string CrearContenidoArchivo(IEnumerable<TRegistro> registros,CcProcesoMensualGeneraArchivoRequest request)
        {
            var builder = new StringBuilder();

            var encabezado = CrearEncabezado();

            if (!string.IsNullOrEmpty(encabezado))
            {
                builder.AppendLine(encabezado);
            }

            foreach (var registro in FiltrarRegistros(registros))
            {
                var linea = CrearLineaArchivo(registro, request);

                if (!string.IsNullOrEmpty(linea))
                {
                    builder.AppendLine(linea);
                }
            }

            return builder.ToString();
        }

        protected virtual IEnumerable<TRegistro> FiltrarRegistros( IEnumerable<TRegistro> registros)
        {
            return registros;
        }

        protected virtual string CrearEncabezado()
        {
            return string.Empty;
        }

        protected abstract string CrearLineaArchivo(TRegistro registro, CcProcesoMensualGeneraArchivoRequest request);

        protected virtual CcProcesoMensualArchivoGeneradoModel CrearRespuesta(string nombreArchivo, string rutaArchivo, string contenido)
        {
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

        protected virtual List<string> ObtenerArchivosGenerados( string rutaArchivoPrincipal)
        {
            return [rutaArchivoPrincipal];
        }
    }
}
