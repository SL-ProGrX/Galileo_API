using Dapper; 
using System.Data;
using System.Globalization;
using System.Security;
using System.Text;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF17UcrGenerar :   CcProcesoMensualArchivoPlanoGenerarBase<CcProcesoMensualArchivoRegistroDbModel>
    {
     private const string TipoCredito = "C";

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["17"];

        protected override string CodigoPlanillaEnvio => "17";
        protected override string CodigoFormato => "F17";
        protected override string ExtensionArchivo => ".XML";
        protected override string ContentType => "application/xml";
       protected override Encoding EncodingArchivo => CcProcesoMensualEncodingHelper.Utf8SinBom;

        protected override string QueryRegistros => string.Empty;

        protected override IEnumerable<CcProcesoMensualArchivoRegistroDbModel> ObtenerRegistros(
            IDbConnection connection,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            RedondearMontosPlanilla(
                connection,
                request.CodInstitucion,
                request.FechaProceso);

            return Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRegistrosGeneral(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                TipoCredito);
        }

        protected override string CrearContenidoArchivo(
            IEnumerable<CcProcesoMensualArchivoRegistroDbModel> registros,
            CcProcesoMensualGeneraArchivoRequest request)
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

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoRegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            return string.Empty;
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
            CcProcesoMensualArchivoRegistroDbModel registro)
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
  
    }
}
