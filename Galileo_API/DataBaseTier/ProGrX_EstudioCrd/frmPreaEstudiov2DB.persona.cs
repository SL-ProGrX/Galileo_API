using Dapper;
using System.Collections.Generic;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Datos de la persona y del expediente que VB6 resuelve con consultas sueltas
        /// (txtCedula_LostFocus, sbEdad_Verifica, SbCargaCreditoFrecuenciaPago) y que aqui
        /// viajan en un solo batch en lugar de cuatro viajes a la base.
        /// </summary>
        private sealed record DatosPersona(
            string EstadoPersona,
            int Edad,
            string FrecuenciaPago,
            int EdadAplica,
            string EdadJustificacion);

        /// <summary>
        /// Un QueryMultiple con las cuatro lecturas por cedula/expediente. El SQL de cada
        /// resultset es identico al que ya usaban ObtenerEstadoPersona, ObtenerEdad,
        /// ObtenerFrecuenciaPago y ObtenerJustificacionEdad. Si el batch falla se cae a
        /// esas mismas lecturas individuales, que degradan por separado igual que antes.
        /// </summary>
        private static DatosPersona ObtenerDatosPersona(
            IDbConnection connection,
            string cedula,
            DateTime? fechaNacimiento,
            string codPreanalisis)
        {
            var cedulaTrim = (cedula ?? string.Empty).Trim();

            try
            {
                // dbo.fxSys_Edad_Anios no se invoca sin fecha: VB6 tampoco calcula edad
                // cuando fecha_nacimiento viene nula.
                var sqlEdad = fechaNacimiento is null
                    ? "SELECT 0;"
                    : "SELECT dbo.fxSys_Edad_Anios(@FechaNacimiento);";

                var sql = @"
                    SELECT ISNULL(E.descripcion, '')
                    FROM socios S
                    LEFT JOIN AFI_ESTADOS_PERSONA E ON S.EstadoActual = E.cod_Estado
                    WHERE S.cedula = @Cedula;

                    SELECT TOP 1 1 FROM CRD_PREA_PREANALISIS WHERE cedula = @Cedula;

                    " + sqlEdad + @"

                    SELECT ISNULL(I.Frecuencia, 'M')
                    FROM socios S
                    LEFT JOIN Instituciones I ON S.cod_institucion = I.cod_Institucion
                    WHERE S.cedula = @Cedula;

                    SELECT ISNULL(APL_JUSTIFICACION_EDAD,0) AS EDAD_APLICA,
                           ISNULL(JUSTIFICACION_EDAD,'') AS EDAD_JUSTIFICACION
                    FROM CRD_PREA_PREANALISIS
                    WHERE COD_PREANALISIS = @CodPreanalisis;";

                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", cedulaTrim, DbType.String);
                parameters.Add("@CodPreanalisis", codPreanalisis ?? string.Empty, DbType.String);
                parameters.Add(
                    "@FechaNacimiento",
                    fechaNacimiento?.ToString("yyyy-MM-dd") ?? string.Empty,
                    DbType.String);

                using var multi = connection.QueryMultiple(sql, parameters);

                var estadoSocio = multi.ReadFirstOrDefault<string>();
                var existeEnPreanalisis = multi.ReadFirstOrDefault<int?>();
                var edad = multi.ReadFirstOrDefault<int?>() ?? 0;
                var frecuencia = multi.ReadFirstOrDefault<string>();
                var justificacion = multi.ReadFirstOrDefault() as IDictionary<string, object>;

                return new DatosPersona(
                    ResolverEstadoPersona(cedulaTrim, estadoSocio, existeEnPreanalisis),
                    edad,
                    string.IsNullOrWhiteSpace(frecuencia) ? "M" : frecuencia.Trim(),
                    LeerEdadAplica(justificacion),
                    LeerEdadJustificacion(justificacion));
            }
            catch (Exception)
            {
                var (edadAplica, edadJustificacion) = ObtenerJustificacionEdad(connection, codPreanalisis);

                return new DatosPersona(
                    ObtenerEstadoPersona(connection, cedulaTrim),
                    ObtenerEdad(connection, fechaNacimiento),
                    ObtenerFrecuenciaPago(connection, cedulaTrim),
                    edadAplica,
                    edadJustificacion);
            }
        }

        /// <summary>
        /// VB6 (txtCedula_LostFocus ~16804): si la cedula esta en socios se usa su estado;
        /// si no esta pero existe en CRD_PREA_PREANALISIS se marca "No Socio".
        /// </summary>
        private static string ResolverEstadoPersona(string cedula, string estadoSocio, int? existeEnPreanalisis)
        {
            if (string.IsNullOrWhiteSpace(cedula))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(estadoSocio))
            {
                return estadoSocio.Trim();
            }

            return existeEnPreanalisis.HasValue ? "No Socio" : string.Empty;
        }

        private static int LeerEdadAplica(IDictionary<string, object> fila)
        {
            if (fila is null)
            {
                return 0;
            }

            return GetInt(new Dictionary<string, object>(fila, StringComparer.OrdinalIgnoreCase), "EDAD_APLICA");
        }

        private static string LeerEdadJustificacion(IDictionary<string, object> fila)
        {
            if (fila is null)
            {
                return string.Empty;
            }

            return GetString(new Dictionary<string, object>(fila, StringComparer.OrdinalIgnoreCase), "EDAD_JUSTIFICACION");
        }
    }
}
