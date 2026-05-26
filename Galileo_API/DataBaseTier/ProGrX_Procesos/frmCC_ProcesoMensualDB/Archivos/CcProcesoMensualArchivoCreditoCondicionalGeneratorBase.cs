using Dapper;
using System.Data;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public abstract class CcProcesoMensualArchivoCreditoCondicionalGeneratorBase : CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoRegistroDbModel>
    {
        private const string CodigoNo = "NO";
        private const string TipoCredito = "C";

        protected IDbConnection? Connection { get; private set; }

        protected override string QueryRegistros => string.Empty;

        public override CcProcesoMensualArchivoGeneradoModel GenerarArchivo(
           IDbConnection connection,
           CcProcesoMensualGeneraArchivoRequest request)
        {
            var codigoCreditos = ObtenerCodigoCreditos(
                connection,
                request.CodInstitucion);

            if (EsCodigoNo(codigoCreditos))
            {
                return CrearRespuestaSinGenerar(connection, request);
            }

            Connection = connection;

            return base.GenerarArchivo(connection, request);
        }

        protected override IEnumerable<CcProcesoMensualArchivoRegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoCredito);
        }

        protected static int ObtenerTipoMovimiento(string? movimiento)
        {
            return movimiento?.Trim().ToUpperInvariant() switch
            {
                "E" => 1,
                "I" => 2,
                "C" => 3,
                _ => 4
            };
        }

        private CcProcesoMensualArchivoGeneradoModel CrearRespuestaSinGenerar(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CrearNombreArchivoEstandar(
                request.CodInstitucion,
                request.FechaProceso,
                string.Empty,
                fechaServidor,
                CodigoFormato,
                ExtensionArchivo);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = false,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentType,
                ArchivoBytes = [],
                ArchivosGenerados = []
            };
        }

        private static string ObtenerCodigoCreditos(
            IDbConnection connection,
            int codInstitucion)
        {
            const string query = @"
                SELECT
                    ISNULL(codigo_creditos, '') AS CodigoCreditos
                FROM instituciones
                WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<string>(
                query,
                new { CodInstitucion = codInstitucion }) ?? string.Empty;
        }

        private static bool EsCodigoNo(string? codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                CodigoNo,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
