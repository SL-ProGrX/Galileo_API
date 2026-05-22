using Dapper;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Text;

namespace Galileo.DataBaseTier
{
    /// <summary>
    /// ===== Helpers Públicos reutilizables (anti-duplicidad) =====
    /// </summary>
    public partial class MTesFuncionesDb
    {

        public static void AppendIfNotEmpty(StringBuilder sb, string? line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);
        }

        public static ErrorDto<object> ArchivoResponse(long bancoConsec, string extension, StringBuilder sb)
          => DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new
          {
              bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
              extension,
              contenido = sb.ToString()
          }, Formatting.Indented));


        public static (string numNegocio, string cedulaReg) GetEmpresaNumNegocioYReg(SqlConnection conn)
        {
            const string sql = "select REPLACE(cedula_juridica,'-','') as cedula_juridica from SIF_EMPRESA";
            var empresa = conn.QueryFirstOrDefault(sql);
            var cedula = empresa?.cedula_juridica?.ToString()?.Trim() ?? string.Empty;
            return (cedula, cedula);
        }

        public static int GetConsecutivoArchivoDelDia(SqlConnection conn, int bancoId, DateTime fechaEmision)
        {
            const string sql = @"
select count(distinct documento_base)
from Tes_Transacciones
where id_banco = @banco
  and fecha_emision = @fecha
  and estado = 'T'";
            return conn.QuerySingle<int>(sql, new { banco = bancoId, fecha = fechaEmision }) + 1;
        }

        /// <summary>
        /// Ejecuta 3 líneas (numLinea 1..3) de un Stored Procedure con parámetros base.
        /// Usa DynamicParameters para “aplanar” correctamente y evitar el antipatrón new { parametrosBase }.
        /// </summary>
        public static IEnumerable<string> ExecSP3Lineas(
            SqlConnection conn,
            string spName,
            object parametrosBase,
            CommandType commandType = CommandType.StoredProcedure)
        {
            for (int numLinea = 1; numLinea <= 3; numLinea++)
            {
                var dp = new DynamicParameters(parametrosBase);
                dp.Add("numLinea", numLinea);

                var linea = conn.QueryFirstOrDefault<string>(
                    spName,
                    dp,
                    commandType: commandType);

                if (!string.IsNullOrWhiteSpace(linea))
                    yield return linea;
            }
        }

        public static string BuildControlBcrEmpresarial(string cedulaReg, string conArchivo, DateTime fecha)
        {
            var control = new StringBuilder(220);
            control.Append("000");
            control.Append((cedulaReg ?? string.Empty).Trim().PadLeft(12, '0'));
            control.Append(conArchivo);
            control.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
            control.Append(zero12Append);
            control.Append(zero12Append);
            control.Append(zero6Append);
            control.Append(new string(' ', 6));
            control.Append("TLB");
            control.Append(new string(' ', 128));
            control.Append('D');
            return control.ToString();
        }

        public static string BuildControlBcrComercial(string cedulaReg, string conArchivo, DateTime fecha)
        {
            var control = new StringBuilder(220);
            control.Append("000");
            control.Append((cedulaReg ?? string.Empty).Trim().PadLeft(12, '0'));
            control.Append(conArchivo);
            control.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
            control.Append(zero12Append);
            control.Append(zero12Append);
            control.Append(zero6Append);
            control.Append(new string('0', 138));
            return control.ToString();
        }
    }
}
