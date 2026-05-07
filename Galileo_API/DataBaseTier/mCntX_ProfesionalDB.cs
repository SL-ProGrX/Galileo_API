using Dapper;
using Galileo.DataBaseTier;
using Galileo_API.Models;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo_API.DataBaseTier
{
    public class MCntXProfesionalDb
    {
        private readonly PortalDB _portalDb;

        public MCntXProfesionalDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Cifra o descifra la clave de portal
        /// </summary>
        /// <param name="vClave"></param>
        /// <param name="vTipo"></param>
        /// <returns></returns>
        public string FxPortalCifrado(string vClave, string vTipo = "C")
        {
            vClave = (vClave ?? string.Empty).Trim();

            if (string.Equals(vTipo, "C", StringComparison.OrdinalIgnoreCase))
            {
                var vCadena = new StringBuilder(vClave.Length);
                for (var i = 0; i < vClave.Length; i++)
                {
                    vCadena.Append((char)(vClave[i] + 1));
                }

                var vResultado = new StringBuilder(vCadena.Length);
                for (var i = vCadena.Length - 1; i >= 0; i--)
                {
                    vResultado.Append(vCadena[i]);
                }

                return vResultado.ToString();
            }
            else
            {
                var vCadena = new StringBuilder(vClave.Length);
                for (var i = 0; i < vClave.Length; i++)
                {
                    vCadena.Append((char)(vClave[i] - 1));
                }

                var vResultado = new StringBuilder(vCadena.Length);
                for (var i = 0; i < vCadena.Length; i++)
                {
                    vResultado.Insert(0, vCadena[i]);
                }

                return vResultado.ToString();
            }
        }

        /// <summary>
        /// Construye y prueba la cadena de conexión de un portal externo.
        /// </summary>
        /// <param name="vUsuario"></param>
        /// <param name="vClave"></param>
        /// <param name="vServidor"></param>
        /// <param name="vBaseDatos"></param>
        /// <returns></returns>
        public string FxPortalPrueba(
            string vUsuario, string vClave, string vServidor, string vBaseDatos)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vUsuario) ||
                    string.IsNullOrWhiteSpace(vClave) ||
                    string.IsNullOrWhiteSpace(vServidor) ||
                    string.IsNullOrWhiteSpace(vBaseDatos))
                {
                    return string.Empty;
                }

                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = vServidor.Trim(),
                    InitialCatalog = vBaseDatos.Trim(),
                    UserID = vUsuario.Trim(),
                    Password = vClave.Trim(),
                    ConnectTimeout = 15,
                    TrustServerCertificate = true
                };

                var connectionString = builder.ConnectionString;

                using var connection = new SqlConnection(connectionString);
                connection.Open();
                connection.Close();

                return connectionString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Obtiene la cuenta o descripción asociada a una consolidación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="strCoD"></param>
        /// <param name="vParametro"></param>
        /// <param name="vConsolida"></param>
        /// <returns></returns>
        public string FxConsolida_Cuenta(int codEmpresa, string strCoD, string vParametro, int vConsolida)
        {
            var stringConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            const string queryCuenta = @"
                select top 1
                    C.cod_cuenta as resultado
                from CntX_cuentas C
                inner join CNTX_CONSOLIDA_DEFINICION O
                    on C.COD_CONTABILIDAD = O.COD_CONTABILIDAD
                where C.descripcion = @vParametro
                  and O.cod_consolida = @vConsolida;";

            const string queryDescripcion = @"
                select top 1
                    C.descripcion as resultado
                from CntX_cuentas C
                inner join CNTX_CONSOLIDA_DEFINICION O
                    on C.COD_CONTABILIDAD = O.COD_CONTABILIDAD
                where C.cod_cuenta = @vParametro
                  and O.cod_consolida = @vConsolida;";

            try
            {
                using var connection = new SqlConnection(stringConn);

                var resultado = connection.QueryFirstOrDefault<string>(
                    string.Equals((strCoD ?? string.Empty).Trim(), "C", StringComparison.OrdinalIgnoreCase)
                        ? queryCuenta
                        : queryDescripcion,
                    new
                    {
                        vParametro = (vParametro ?? string.Empty).Trim(),
                        vConsolida
                    });

                return resultado ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Ajusta el formato de una cuenta según la máscara de la consolidación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="blnMascara"></param>
        /// <param name="strCuenta"></param>
        /// <param name="vConsolidado"></param>
        /// <param name="optMensaje"></param>
        /// <returns></returns>
        public string FxConsolida_CuentaFormato(
            int codEmpresa, bool blnMascara, string strCuenta, int vConsolidado, int optMensaje = 1)
        {
            var stringConn = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                var codContabilidad = connection.QueryFirstOrDefault<int?>(
                    @"select top 1 COD_CONTABILIDAD
                      from CNTX_CONSOLIDA_DEFINICION
                      where cod_consolida = @vConsolidado",
                    new { vConsolidado });

                if (!codContabilidad.HasValue || codContabilidad.Value <= 0)
                {
                    return string.Empty;
                }

                var mascaraData = connection.QueryFirstOrDefault<ContaMascaraData>(
                    @"select
                          Nivel1 as nivel1,
                          Nivel2 as nivel2,
                          Nivel3 as nivel3,
                          Nivel4 as nivel4,
                          Nivel5 as nivel5
                      from CNTX_CONTABILIDADES
                      where COD_CONTABILIDAD = @codContabilidad",
                    new { codContabilidad });

                if (mascaraData == null)
                {
                    return string.Empty;
                }

                strCuenta = (strCuenta ?? string.Empty).Trim();

                var limpia = new StringBuilder(strCuenta.Length);
                for (var i = 0; i < strCuenta.Length; i++)
                {
                    if (strCuenta[i] != '-')
                    {
                        limpia.Append(strCuenta[i]);
                    }
                }

                var cuentaSinFormato = limpia.ToString();

                if (!cuentaSinFormato.All(char.IsDigit))
                {
                    return cuentaSinFormato;
                }

                var largoTotal =
                    mascaraData.nivel1 +
                    mascaraData.nivel2 +
                    mascaraData.nivel3 +
                    mascaraData.nivel4 +
                    mascaraData.nivel5;

                if (largoTotal <= 0)
                {
                    return cuentaSinFormato;
                }

                if (cuentaSinFormato.Length < largoTotal)
                {
                    cuentaSinFormato = cuentaSinFormato.PadRight(largoTotal, '0');
                }

                if (!blnMascara)
                {
                    return cuentaSinFormato;
                }

                return AplicarMascaraCuenta(cuentaSinFormato, mascaraData);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Genera la máscara contable 
        /// </summary>
        /// <param name="iNivel1"></param>
        /// <param name="iNivel2"></param>
        /// <param name="iNivel3"></param>
        /// <param name="iNivel4"></param>
        /// <param name="iNivel5"></param>
        /// <returns></returns>
        public static string FxMascara(int iNivel1, int iNivel2, int iNivel3, int iNivel4, int iNivel5)
        {
            return ConstruirMascara(iNivel1, iNivel2, iNivel3, iNivel4, iNivel5);
        }

        private static string AplicarMascaraCuenta(string cuenta, ContaMascaraData mascara)
        {
            var segmentos = new List<string>();
            var posicion = 0;

            var niveles = new[]
            {
                mascara.nivel1,
                mascara.nivel2,
                mascara.nivel3,
                mascara.nivel4,
                mascara.nivel5
            };

            foreach (var nivel in niveles)
            {
                if (nivel <= 0)
                {
                    continue;
                }

                if (posicion + nivel > cuenta.Length)
                {
                    break;
                }

                segmentos.Add(cuenta.Substring(posicion, nivel));
                posicion += nivel;
            }

            return string.Join("-", segmentos);
        }

        private static string ConstruirMascara(int iNivel1, int iNivel2, int iNivel3, int iNivel4, int iNivel5)
        {
            var segmentos = new List<string>();

            if (iNivel1 > 0) segmentos.Add(new string('#', iNivel1));
            if (iNivel2 > 0) segmentos.Add(new string('#', iNivel2));
            if (iNivel3 > 0) segmentos.Add(new string('#', iNivel3));
            if (iNivel4 > 0) segmentos.Add(new string('#', iNivel4));
            if (iNivel5 > 0) segmentos.Add(new string('#', iNivel5));

            return string.Join("-", segmentos);
        }
    }
}
