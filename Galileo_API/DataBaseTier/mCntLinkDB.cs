using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class MCntLinkDB
    {
        private readonly IConfiguration _config;

        public MCntLinkDB(IConfiguration config)
        {
            _config = config;
        }

        public string fxgCntUnidad(int codEmpresa, string pCodigo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
                    SELECT descripcion AS Descripcion
                    FROM CntX_Unidades
                    WHERE cod_unidad = @Codigo AND cod_contabilidad = @Contabilidad;";

                var info = connection.QueryFirstOrDefault<CntUnidadDto>(sql, new
                {
                    Codigo = pCodigo,
                    Contabilidad = codEmpresa
                });

                return info?.Descripcion ?? string.Empty;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return string.Empty;
            }
        }

        public string fxgCntCentroCostos(int codEmpresa, string pCodigo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
                    SELECT descripcion AS Descripcion
                    FROM CntX_Centro_Costos
                    WHERE cod_centro_Costo = @Codigo AND cod_contabilidad = @Contabilidad;";

                var info = connection.QueryFirstOrDefault<CntCentroCostosDto>(sql, new
                {
                    Codigo = pCodigo,
                    Contabilidad = codEmpresa
                });

                return info?.Descripcion ?? string.Empty;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return string.Empty;
            }
        }

        public bool fxgCntPeriodoValida(int codEmpresa, DateTime vFecha)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
                    SELECT TOP 1 1
                    FROM CntX_Periodos
                    WHERE anio = @Anio
                      AND mes = @Mes
                      AND estado = 'P'
                      AND cod_contabilidad = @Contabilidad;";

                var existe = connection.QueryFirstOrDefault<int?>(sql, new
                {
                    Anio = vFecha.Year,
                    Mes = vFecha.Month,
                    Contabilidad = codEmpresa
                });

                return existe.HasValue;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }
        }

        public string fxgCntCuentaDesc(int codEmpresa, string pCuenta)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
                    SELECT LTRIM(RTRIM(Descripcion)) AS Descripcion
                    FROM CntX_Cuentas
                    WHERE cod_cuenta = @Cuenta AND cod_contabilidad = @Contabilidad;";

                var info = connection.QueryFirstOrDefault<CntDescripCuentaDto>(sql, new
                {
                    Cuenta = pCuenta,
                    Contabilidad = codEmpresa
                });

                return info?.Descripcion ?? string.Empty;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return string.Empty;
            }
        }

        public string fxgCntCuentaDesc(int CodEmpresa, string pCuenta, int CodConta)
        {
            string result = "";
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ltrim(rtrim(Descripcion)) as 'Descripcion' from CntX_Cuentas where cod_cuenta = '{pCuenta}' and cod_contabilidad = {CodConta}";

                    var info = connection.Query<CntDescripCuentaDto>(query).FirstOrDefault();
                    if(info != null)
                    {
                        result = info.Descripcion ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }
        public bool fxgCntCuentaValida(int codEmpresa, string vCuenta)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                vCuenta = fxgCntCuentaFormato(codEmpresa, false, vCuenta, 0);

                using var connection = new SqlConnection(stringConn);

                const string sqlSif = @"SELECT TOP 1 * FROM sif_empresa;";
                var sif = connection.QueryFirstOrDefault<SifEmpresaDto>(sqlSif);
                if (sif == null) return false;

                const string sqlValida = @"
                    SELECT ISNULL(COUNT(*),0) AS Existe
                    FROM CntX_cuentas
                    WHERE cod_cuenta = @Cuenta
                      AND acepta_movimientos = 1
                      AND cod_contabilidad = @Contabilidad;";

                var info = connection.QueryFirstOrDefault<CntValidaDto>(sqlValida, new
                {
                    Cuenta = vCuenta,
                    Contabilidad = sif.Cod_Empresa_Enlace
                });

                return (info?.Existe ?? 0) > 0;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return false;
            }
        }

        public string fxgCntTipoAsientoDesc(int codEmpresa, string vTipo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
                    SELECT descripcion AS Descripcion
                    FROM CntX_tipos_asientos
                    WHERE tipo_asiento = @Tipo AND cod_contabilidad = @Contabilidad;";

                var info = connection.QueryFirstOrDefault<CntDescripTipoAsientoDto>(sql, new
                {
                    Tipo = vTipo,
                    Contabilidad = codEmpresa
                });

                return info?.Descripcion ?? string.Empty;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return string.Empty;
            }
        }

        public string fxgCntAjustaCuentaContable(int codEmpresa, string strCuenta)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sql = @"
                    SELECT TOP 1 *
                    FROM CntX_Contabilidades
                    WHERE cod_contabilidad = @Contabilidad;";

                var info = connection.QueryFirstOrDefault<CntContabilidadesDto>(sql, new
                {
                    Contabilidad = codEmpresa
                });

                if (info == null) return strCuenta.Trim();

                var total = info.Nivel1 + info.Nivel2 + info.Nivel3 + info.Nivel4
                          + info.Nivel5 + info.Nivel6 + info.Nivel7 + info.Nivel8;

                var cuenta = strCuenta.Trim();
                return cuenta.Length >= total ? cuenta : cuenta.PadRight(total, '0');
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return strCuenta.Trim();
            }
        }

        public string fxgCntCuentaFormato(int codEmpresa, bool blnMascara, string pCuenta, int optMensaje = 1)
        {
            pCuenta = (pCuenta ?? string.Empty).Trim();

            try
            {
                var param = sbgCntParametros(codEmpresa);
                var cuenta = RemoveHyphens(pCuenta);

                if (!double.TryParse(cuenta, out _))
                {
                    return optMensaje == 1 ? "Código de cuenta inválido..." : cuenta;
                }

                if (param.Result == null)
                {
                    return optMensaje == 1 ? "No se pudo obtener los parámetros de la cuenta." : cuenta;
                }

                cuenta = PadWithZeros(cuenta, param.Result.gMascaraTChar);

                if (blnMascara)
                    cuenta = ApplyMask(cuenta, param.Result.gstrMascara);

                return cuenta;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return pCuenta;
            }
        }

        private static string RemoveHyphens(string input) =>
            new string((input ?? string.Empty).Where(c => c != '-').ToArray());

        private static string PadWithZeros(string input, int totalLength) =>
            input.Length >= totalLength ? input : input.PadRight(totalLength, '0');

        private static string ApplyMask(string input, string mask)
        {
            var sb = new System.Text.StringBuilder();
            int j = 0;

            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == '#' && j < input.Length)
                    sb.Append(input[j++]);
                else
                    sb.Append(mask[i]);
            }

            return sb.ToString();
        }

        public ErrorDto<DefMascarasDto> sbgCntParametros(int codEmpresa)
        {
            var info = new ErrorDto<DefMascarasDto> { Result = new DefMascarasDto() };
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string sqlSif = @"SELECT TOP 1 * FROM sif_empresa;";
                var sif = connection.QueryFirstOrDefault<SifEmpresaDto>(sqlSif);
                if (sif == null)
                {
                    info.Code = -1;
                    info.Description = "No SifEmpresaDto found.";
                    return info;
                }

                const string sqlConta = @"
                    SELECT TOP 1 *
                    FROM CntX_Contabilidades
                    WHERE cod_contabilidad = @Contabilidad;";

                var conta = connection.QueryFirstOrDefault<CntContabilidadesDto>(sqlConta, new
                {
                    Contabilidad = sif.Cod_Empresa_Enlace
                });

                if (conta == null)
                {
                    info.Code = -1;
                    info.Description = "No CntContabilidadesDto found.";
                    return info;
                }

                info.Result.gEnlace = sif.Cod_Empresa_Enlace;

                const string sql = @"
                    SELECT TOP 1 anio, mes
                    FROM CntX_Periodos
                    WHERE estado = 'P'
                      AND cod_contabilidad = @Contabilidad;";

                var gPeriodo = connection.QueryFirstOrDefault(sql, new
                {
                    Contabilidad = sif.Cod_Empresa_Enlace
                });

                if (gPeriodo == null)
                {
                    info.Code = -1;
                    info.Description = "No CntPeriodosDto found.";
                    return info;
                }

                info.Result.gPeriodoAnio = gPeriodo.anio;
                info.Result.gPeriodoMes = gPeriodo.mes;

                int[] niveles =
                {
                    conta.Nivel1, conta.Nivel2, conta.Nivel3, conta.Nivel4,
                    conta.Nivel5, conta.Nivel6, conta.Nivel7, conta.Nivel8
                };

                var sbMascara = new StringBuilder();
                var sbNiveles = new StringBuilder();

                for (int idx = 0; idx < niveles.Length; idx++)
                {
                    int nivel = niveles[idx];
                    if (nivel <= 0) continue;

                    if (idx > 0)
                        sbMascara.Append('-');

                    sbNiveles.Append(nivel);
                    info.Result.gMascaraTChar += nivel;
                    sbMascara.Append('#', nivel);
                }
                info.Result.gstrMascara = sbMascara.ToString();
                info.Result.gstrNiveles = sbNiveles.ToString();

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }
    }
}