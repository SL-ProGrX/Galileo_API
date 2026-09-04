using Dapper;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Snapshot de CRD_PREA_PARAMETROS leido en UNA sola consulta. Antes cada bloque
        /// de la carga del expediente leia esta misma tabla por su cuenta (5 viajes a la
        /// base: '07-08-09' de cargas, '13' de P.S.D., '18-19-20' de componentes
        /// adicionales, '17-22' de salario y '01-02' de edad maxima). Son parametros
        /// globales de empresa, no del expediente, asi que se traen juntos y se resuelven
        /// en memoria.
        ///
        /// VB6 hace exactamente lo mismo: mPreAnalisis.bas, sbInicializaGlobales() lee
        /// "SELECT * FROM CRD_PREA_PARAMETROS" una sola vez y reparte los valores en las
        /// variables globales (GlobalPorcCCSS, GlobalPorcPSD,
        /// GlobalSalarioMinimoInembargable, GlobalEdadMaximaPermitidaHombre, etc.).
        /// </summary>
        private sealed class ParametrosGlobales
        {
            /// <summary>Codigos que consume la carga del expediente.</summary>
            private const string Sql = @"
                SELECT COD_PARAMETRO, DESCRIPCION, VALOR
                FROM CRD_PREA_PARAMETROS
                WHERE COD_PARAMETRO IN ('01','02','07','08','09','13','17','18','19','20','22')";

            private readonly Dictionary<string, IDictionary<string, object>> _porCodigo;

            private ParametrosGlobales(Dictionary<string, IDictionary<string, object>> porCodigo)
            {
                _porCodigo = porCodigo;
            }

            /// <summary>
            /// No bloqueante, igual que el resto de lecturas de parametros de este modulo:
            /// si la consulta falla se devuelve un snapshot vacio y cada valor cae a su
            /// valor por defecto.
            /// </summary>
            public static ParametrosGlobales Leer(IDbConnection connection)
            {
                var porCodigo = new Dictionary<string, IDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    foreach (var fila in connection.Query(Sql))
                    {
                        var dict = new Dictionary<string, object>(
                            (IDictionary<string, object>)fila,
                            StringComparer.OrdinalIgnoreCase);

                        var codigo = GetString(dict, "COD_PARAMETRO");
                        if (!string.IsNullOrEmpty(codigo))
                        {
                            porCodigo[codigo] = dict;
                        }
                    }
                }
                catch (DataException)
                {
                    porCodigo.Clear();
                }

                return new ParametrosGlobales(porCodigo);
            }

            public decimal Decimal(string codParametro)
                => _porCodigo.TryGetValue(codParametro, out var fila) ? GetDecimal(fila, "VALOR") : 0m;

            public int Entero(string codParametro)
                => _porCodigo.TryGetValue(codParametro, out var fila) ? GetInt(fila, "VALOR") : 0;

            /// <summary>
            /// VB6: cboS_ComponenteAdicional se llena con
            /// "SELECT COD_PARAMETRO as IdX, DESCRIPCION + ' [ ' + VALOR + ' % ]' as ItmX
            ///  FROM CRD_PREA_PARAMETROS WHERE COD_PARAMETRO IN('18','19','20')".
            /// Se arma en memoria con el mismo formato a partir del snapshot.
            /// </summary>
            public List<FrmPreaEstudiov2DropdownDto> ComponentesAdicionales(params string[] codigos)
            {
                var lista = new List<FrmPreaEstudiov2DropdownDto>();

                foreach (var codigo in codigos)
                {
                    if (!_porCodigo.TryGetValue(codigo, out var fila))
                    {
                        continue;
                    }

                    lista.Add(new FrmPreaEstudiov2DropdownDto
                    {
                        item = codigo,
                        descripcion = GetString(fila, "DESCRIPCION").TrimEnd() + " [ " + GetString(fila, "VALOR") + " % ]",
                    });
                }

                return lista;
            }
        }
    }
}
