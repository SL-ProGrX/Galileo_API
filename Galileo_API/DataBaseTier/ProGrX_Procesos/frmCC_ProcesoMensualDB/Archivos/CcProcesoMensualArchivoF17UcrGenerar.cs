using Dapper; 
using System.Data;
using System.Globalization;
using System.Security;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF17UcrGenerar : ICcProcesoMensualArchivoGenerator
    {
        private const string CodigoPlanillaEnvio = "17";
        private const string ContentTypeXml = "application/xml";
        private const string ExtensionXml = ".XML";
        private const string TipoCredito = "C";

        public IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = [CodigoPlanillaEnvio];

        public CcProcesoMensualArchivoGeneradoModel GenerarArchivo(IDbConnection connection, CcProcesoMensualGeneraArchivoRequest request)
        {
            var fechaServidor = ObtenerFechaServidor(connection);

            var nombreArchivo = CrearNombreArchivo(
                request.CodInstitucion,
                request.FechaProceso,
                fechaServidor);

            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request);

            var rutaArchivo = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                rutaDirectorio,
                nombreArchivo);

            RedondearMontosPlanilla(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var registros = ObtenerRegistrosCreditos(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            var contenido = CrearContenidoXml(registros);

            Helpers.CcProcesoMensualArchivoRutaHelperDb.GuardarArchivoTexto(
                rutaDirectorio,
                rutaArchivo,
                contenido,
                Encoding.UTF8);

            return new CcProcesoMensualArchivoGeneradoModel
            {
                Generado = true,
                CodigoPlanillaEnvio = CodigoPlanillaEnvio,
                NombreArchivo = nombreArchivo,
                RutaArchivo = rutaArchivo,
                ContentType = ContentTypeXml,
                ArchivoBytes = [],
                ArchivosGenerados = [rutaArchivo]
            };
        }

        private static void RedondearMontosPlanilla(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                UPDATE prm_planilla
                SET monto_actual = ROUND(monto_actual, 0),
                    monto_anterior = ROUND(monto_anterior, 0)
                WHERE proceso = @FechaProceso
                  AND cod_institucion = @CodInstitucion";

            connection.Execute(query, new
            {
                FechaProceso = fechaProceso,
                CodInstitucion = codInstitucion
            });
        }

        private static List<CcProcesoMensualArchivoF17RegistroDbModel> ObtenerRegistrosCreditos(
            IDbConnection connection,
            int codInstitucion,
            decimal fechaProceso)
        {
            const string query = @"
                SELECT
                    P.Cedula,
                    P.Monto_Actual AS MontoActual,
                    P.Movimiento,
                    P.Tipo,
                    S.nombre AS Nombre
                FROM prm_planilla P
                INNER JOIN Socios S
                    ON P.cedula = S.cedula
                WHERE P.Proceso = @FechaProceso
                  AND P.tipo = @TipoCredito
                  AND P.cod_institucion = @CodInstitucion
                ORDER BY P.cedula, P.tipo, P.movimiento";

            return [.. connection.Query<CcProcesoMensualArchivoF17RegistroDbModel>(
                query,
                new
                {
                    FechaProceso = fechaProceso,
                    CodInstitucion = codInstitucion,
                    TipoCredito
                })];
        }

        private static string CrearContenidoXml(
            IEnumerable<CcProcesoMensualArchivoF17RegistroDbModel> registros)
        {
            var builder = new StringBuilder();

            AgregarEncabezadoXml(builder);

            foreach (var registro in registros)
            {
                AgregarDeduccion(builder, registro);
            }

            builder.AppendLine("</Deducciones_Externas>");

            return builder.ToString();
        }

        private static void AgregarEncabezadoXml(StringBuilder builder)
        {
            builder.AppendLine("""<?xml version="1.0" encoding="utf-8"?>""");
            builder.AppendLine("<Deducciones_Externas>");
            builder.AppendLine("""  <xs:schema id="Deducciones_Externas" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">""");
            builder.AppendLine("""    <xs:element name="Deducciones_Externas" msdata:IsDataSet="true" msdata:MainDataTable="Deduccion" msdata:UseCurrentLocale="true">""");
            builder.AppendLine("      <xs:complexType>");
            builder.AppendLine("""        <xs:choice minOccurs="0" maxOccurs="unbounded">""");
            builder.AppendLine("""          <xs:element name="Deduccion">""");
            builder.AppendLine("            <xs:complexType>");
            builder.AppendLine("              <xs:sequence>");
            builder.AppendLine("""                <xs:element name="Identificacion" type="xs:string" />""");
            builder.AppendLine("""                <xs:element name="Nombre" type="xs:string" />""");
            builder.AppendLine("""                <xs:element name="Valor" type="xs:double" />""");
            builder.AppendLine("              </xs:sequence>");
            builder.AppendLine("            </xs:complexType>");
            builder.AppendLine("          </xs:element>");
            builder.AppendLine("        </xs:choice>");
            builder.AppendLine("      </xs:complexType>");
            builder.AppendLine("    </xs:element>");
            builder.AppendLine("  </xs:schema>");
        }

        private static void AgregarDeduccion(
            StringBuilder builder,
            CcProcesoMensualArchivoF17RegistroDbModel registro)
        {
            builder.AppendLine("<Deduccion>");
            builder.AppendLine("    <Identificacion>" + EscaparXml(registro.Cedula.Trim()) + "</Identificacion>");
            builder.AppendLine("    <Nombre>" + EscaparXml(TomarIzquierda(registro.Nombre.Trim(), 30)) + "</Nombre>");
            builder.AppendLine("    <Valor>" + FormatearValor(registro.MontoActual) + "</Valor>");
            builder.AppendLine(" </Deduccion>");
        }

        private static string EscaparXml(string valor)
        {
            return SecurityElement.Escape(valor) ?? string.Empty;
        }

        private static string TomarIzquierda(string valor, int cantidad)
        {
            return valor.Length > cantidad
                ? valor[..cantidad]
                : valor;
        }

        private static string FormatearValor(decimal monto)
        {
            return monto.ToString(CultureInfo.InvariantCulture);
        }

        private static string CrearNombreArchivo(
            int codInstitucion,
            decimal fechaProceso,
            DateTime fechaServidor)
        {
            var codigoInstitucion = codInstitucion.ToString("00", CultureInfo.InvariantCulture);
            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F17]{ExtensionXml}";
        }

        private static DateTime ObtenerFechaServidor(IDbConnection connection)
        {
            const string query = "SELECT GETDATE()";
            return connection.QueryFirstOrDefault<DateTime>(query);
        }

        private sealed class CcProcesoMensualArchivoF17RegistroDbModel
        {
            public string Cedula { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
