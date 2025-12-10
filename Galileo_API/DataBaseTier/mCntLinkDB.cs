using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class MCntLinkDB
    {
        private readonly IConfiguration _config;
        public MCntLinkDB(IConfiguration config)
        {
            _config = config;
        }

        public string fxgCntUnidad(int CodEmpresa, string pCodigo)
        {
            string result = "";
            CntUnidadDto? info;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select descripcion from CntX_Unidades where cod_unidad = {pCodigo} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntUnidadDto>(query).FirstOrDefault();
                    result = info?.Descripcion ?? "";

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public string fxgCntCentroCostos(int CodEmpresa, string pCodigo)
        {
            string result = "";
            CntCentroCostosDto? info;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select descripcion from CntX_Centro_Costos where cod_centro_Costo = {pCodigo} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntCentroCostosDto>(query).FirstOrDefault();
                    result = info != null ? info.Descripcion : "";

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public bool fxgCntPeriodoValida(int CodEmpresa, DateTime vFecha)
        {
            bool result = false;
            List<CntPeriodosDto> info;

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CntX_Periodos where anio = Year({vFecha}) and mes = Month({vFecha}) and estado = 'P' and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntPeriodosDto>(query).ToList();

                    if (info.Count > 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public string fxgCntCuentaDesc(int CodEmpresa, string pCuenta)
        {
            string result = "";
            CntDescripCuentaDto? info;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ltrim(rtrim(Descripcion)) as 'Descripcion' from CntX_Cuentas where cod_cuenta = {pCuenta} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntDescripCuentaDto>(query).FirstOrDefault();
                    result = info != null ? info.Descripcion : "";

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public bool fxgCntCuentaValida(int CodEmpresa, string vCuenta)
        {
            bool result = false;
            CntValidaDto info;
            SifEmpresaDto sif;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {


                vCuenta = fxgCntCuentaFormato(CodEmpresa, false, vCuenta, 0);

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from sif_empresa";
                    var sifResult = connection.Query<SifEmpresaDto>(query).FirstOrDefault();
                    if (sifResult == null)
                    {
                        return false;
                    }
                    sif = sifResult;


                    query = @"select isnull(count(*),0) as Existe from CntX_cuentas where cod_cuenta = @codCuenta and acepta_movimientos = 1 and cod_contabilidad = @codContabilidad";

                    var validaResult = connection.Query<CntValidaDto>(query, new { codCuenta = vCuenta, codContabilidad = sif.Cod_Empresa_Enlace }).FirstOrDefault();
                    if (validaResult != null)
                    {
                        info = validaResult;
                        if (info.Existe > 0)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public string fxgCntTipoAsientoDesc(int CodEmpresa, string vTipo)
        {
            string result = "";
            CntDescripTipoAsientoDto? info;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select descripcion from CntX_tipos_asientos where tipo_asiento = {vTipo} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntDescripTipoAsientoDto>(query).FirstOrDefault();
                    result = info != null ? info.Descripcion : "";

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public string fxgCntAjustaCuentaContable(int CodEmpresa, string strCuenta)
        {
            string result = "";
            int intCaracteres = 0;
            CntContabilidadesDto? info;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CntX_Contabilidades where cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntContabilidadesDto>(query).FirstOrDefault();
                    if (info != null)
                    {
                        intCaracteres = info.Nivel1;
                        intCaracteres = intCaracteres + info.Nivel2;
                        intCaracteres = intCaracteres + info.Nivel3;
                        intCaracteres = intCaracteres + info.Nivel4;
                        intCaracteres = intCaracteres + info.Nivel5;
                        intCaracteres = intCaracteres + info.Nivel6;
                        intCaracteres = intCaracteres + info.Nivel7;
                        intCaracteres = intCaracteres + info.Nivel8;

                        result = strCuenta.Trim();

                        var sb = new System.Text.StringBuilder(result);
                        for (int i = result.Length; i < intCaracteres; i++)
                        {
                            sb.Append('0');
                        }
                        result = sb.ToString();
                    }
                    else
                    {
                        result = strCuenta.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public string fxgCntCuentaFormato(int CodEmpresa, bool blnMascara, string pCuenta, int optMensaje = 1)
        {
            string result = "";
            pCuenta = pCuenta.Trim();

            try
            {
                var param = sbgCntParametros(CodEmpresa);

                result = RemoveHyphens(pCuenta);

                pCuenta = result;

                if (!double.TryParse(pCuenta, out _))
                {
                    result = pCuenta;
                    if (optMensaje == 1)
                    {
                        result = "Código de cuenta inválido...";
                        return result;
                    }
                }

                if (param.Result != null)
                {
                    pCuenta = PadWithZeros(pCuenta, param.Result.gMascaraTChar);

                    if (blnMascara)
                    {
                        pCuenta = ApplyMask(pCuenta, param.Result.gstrMascara);
                    }
                }
                else
                {
                    if (optMensaje == 1)
                    {
                        result = "No se pudo obtener los parámetros de la cuenta.";
                        return result;
                    }
                }

                result = pCuenta;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;
        }

        private static string RemoveHyphens(string input)
        {
            return new string(input.Where(c => c != '-').ToArray());
        }

        private static string PadWithZeros(string input, int totalLength)
        {
            if (input.Length >= totalLength)
                return input;
            return input.PadRight(totalLength, '0');
        }

        private static string ApplyMask(string input, string mask)
        {
            var sbMask = new System.Text.StringBuilder();
            int j = 0;
            for (int i = 0; i < mask.Length; i++)
            {
                if (mask[i] == '#' && j < input.Length)
                {
                    sbMask.Append(input[j]);
                    j++;
                }
                else
                {
                    sbMask.Append(mask[i]);
                }
            }
            return sbMask.ToString();
        }

        public ErrorDto<DefMascarasDto> sbgCntParametros(int CodEmpresa)
        {
            var info = new ErrorDto<DefMascarasDto>
            {
                Result = new DefMascarasDto()
            };
            CntContabilidadesDto conta;
            SifEmpresaDto sif;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from sif_empresa";
                    var sifResult = connection.Query<SifEmpresaDto>(query).FirstOrDefault();

                    if (sifResult == null)
                    {
                        info.Code = -1;
                        info.Description = "No SifEmpresaDto found.";
                        return info;
                    }
                    sif = sifResult;

                    query = $@"select * from CntX_Contabilidades where cod_contabilidad = {sif.Cod_Empresa_Enlace}";
                    var contaResult = connection.Query<CntContabilidadesDto>(query).FirstOrDefault();

                    if (contaResult == null)
                    {
                        info.Code = -1;
                        info.Description = "No CntContabilidadesDto found.";
                        return info;
                    }
                    conta = contaResult;

                    info.Result.gEnlace = sif.Cod_Empresa_Enlace;

                    int[] niveles = new int[]
                    {
                        conta.Nivel1, conta.Nivel2, conta.Nivel3, conta.Nivel4,
                        conta.Nivel5, conta.Nivel6, conta.Nivel7, conta.Nivel8
                    };

                    for (int idx = 0; idx < niveles.Length; idx++)
                    {
                        int nivel = niveles[idx];
                        if (nivel > 0)
                        {
                            if (idx > 0)
                                info.Result.gstrMascara += "-";
                            info.Result.gstrNiveles += nivel;
                            info.Result.gMascaraTChar += nivel;
                            info.Result.gstrMascara += new string('#', nivel);
                        }
                    }
                }
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
