using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class mCntLinkDB
    {
        private readonly IConfiguration _config;
        public mCntLinkDB(IConfiguration config)
        {
            _config = config;
        }

        public string fxgCntUnidad(int CodEmpresa, string pCodigo)
        {
            string result = "";
            CntUnidadDTO info = new CntUnidadDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select descripcion from CntX_Unidades where cod_unidad = {pCodigo} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntUnidadDTO>(query).FirstOrDefault();
                    result = info.Descripcion;

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
            CntCentroCostosDTO info = new CntCentroCostosDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select descripcion from CntX_Centro_Costos where cod_centro_Costo = {pCodigo} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntCentroCostosDTO>(query).FirstOrDefault();
                    result = info.Descripcion;

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
            List<CntPeriodosDTO> info = new List<CntPeriodosDTO>();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CntX_Periodos where anio = Year({vFecha}) and mes = Month({vFecha}) and estado = 'P' and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntPeriodosDTO>(query).ToList();

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
            CntDescripCuentaDTO info = new CntDescripCuentaDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ltrim(rtrim(Descripcion)) as 'Descripcion' from CntX_Cuentas where cod_cuenta = {pCuenta} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntDescripCuentaDTO>(query).FirstOrDefault();
                    result = info.Descripcion;

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
            CntValidaDTO info = new CntValidaDTO();
            SifEmpresaDTO sif = new SifEmpresaDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {


                vCuenta = fxgCntCuentaFormato(CodEmpresa, false, vCuenta, 0);

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from sif_empresa";
                    sif = connection.Query<SifEmpresaDTO>(query).FirstOrDefault();


                    query = $@"select isnull(count(*),0) as Existe from CntX_cuentas where cod_cuenta = '{vCuenta}' and acepta_movimientos = 1 and cod_contabilidad = {sif.Cod_Empresa_Enlace}";

                    info = connection.Query<CntValidaDTO>(query).FirstOrDefault();

                    if (info.Existe > 0)
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

        public string fxgCntTipoAsientoDesc(int CodEmpresa, string vTipo)
        {
            string result = "";
            CntDescripTipoAsientoDTO info = new CntDescripTipoAsientoDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select descripcion from CntX_tipos_asientos where tipo_asiento = {vTipo} and cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntDescripTipoAsientoDTO>(query).FirstOrDefault();
                    result = info.Descripcion;

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
            CntContabilidadesDTO info = new CntContabilidadesDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CntX_Contabilidades where cod_contabilidad = {CodEmpresa}";

                    info = connection.Query<CntContabilidadesDTO>(query).FirstOrDefault();
                    intCaracteres = info.Nivel1;
                    intCaracteres = intCaracteres + info.Nivel2;
                    intCaracteres = intCaracteres + info.Nivel3;
                    intCaracteres = intCaracteres + info.Nivel4;
                    intCaracteres = intCaracteres + info.Nivel5;
                    intCaracteres = intCaracteres + info.Nivel6;
                    intCaracteres = intCaracteres + info.Nivel7;
                    intCaracteres = intCaracteres + info.Nivel8;

                    result = strCuenta.Trim();


                    for (int i = result.Length; i < intCaracteres; i++)
                    {
                        result += "0";
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
            string resultmask = "";
            pCuenta = pCuenta.Trim();

            //DefMascarasDTO param = new DefMascarasDTO();
            var param = new ErrorDTO<DefMascarasDTO>();
            CntContabilidadesDTO info = new CntContabilidadesDTO();

            try
            {
                param = sbgCntParametros(CodEmpresa);

                for (int i = 0; i < pCuenta.Length; i++)
                {
                    if (pCuenta[i] != '-')
                    {
                        result += pCuenta[i];
                    }
                }

                pCuenta = result;

                if (!double.TryParse(pCuenta, out _))
                {
                    result = pCuenta;
                    if (optMensaje == 1)
                    {
                        result = "Código de cuenta inválido...";
                        return result;

                        //MessageBox.Show("Código de cuenta inválido...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                for (int i = pCuenta.Length; i < param.Result.gMascaraTChar; i++)
                {
                    pCuenta += "0";
                }

                if (blnMascara)
                {
                    // pCuenta = string.Format(param.gstrMascara, pCuenta);

                    for (int i = 0, j = 0; i < param.Result.gstrMascara.Length; i++)
                    {
                        resultmask += param.Result.gstrMascara[i] == '#' && j < pCuenta.Length ? pCuenta[j++] : param.Result.gstrMascara[i];
                    }

                    pCuenta = resultmask;

                }

                result = pCuenta;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return result;

        }

        public ErrorDTO<DefMascarasDTO> sbgCntParametros(int CodEmpresa)
        {
            var info = new ErrorDTO<DefMascarasDTO>
            {
                Result = new DefMascarasDTO() // Instantiate DefMascarasDTO to avoid null
            };
            //  DefMascarasDTO info = new DefMascarasDTO();
            CntContabilidadesDTO conta = new CntContabilidadesDTO();
            SifEmpresaDTO sif = new SifEmpresaDTO();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from sif_empresa";
                    sif = connection.Query<SifEmpresaDTO>(query).FirstOrDefault();

                    query = $@"select * from CntX_Contabilidades where cod_contabilidad = {sif.Cod_Empresa_Enlace}";

                    conta = connection.Query<CntContabilidadesDTO>(query).FirstOrDefault();


                    info.Result.gEnlace = sif.Cod_Empresa_Enlace;

                    if (conta.Nivel1 > 0)
                    {
                        info.Result.gstrNiveles += conta.Nivel1;
                        info.Result.gMascaraTChar += conta.Nivel1;

                        for (int i = 1; i <= conta.Nivel1; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel2 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel2;
                        info.Result.gMascaraTChar += conta.Nivel2;

                        for (int i = 0; i < conta.Nivel2; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel3 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel3;
                        info.Result.gMascaraTChar += conta.Nivel3;

                        for (int i = 0; i < conta.Nivel3; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel4 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel4;
                        info.Result.gMascaraTChar += conta.Nivel4;

                        for (int i = 0; i < conta.Nivel4; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel5 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel5;
                        info.Result.gMascaraTChar += conta.Nivel5;

                        for (int i = 0; i < conta.Nivel5; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel6 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel6;
                        info.Result.gMascaraTChar += conta.Nivel6;

                        for (int i = 0; i < conta.Nivel6; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel7 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel7;
                        info.Result.gMascaraTChar += conta.Nivel7;

                        for (int i = 0; i < conta.Nivel7; i++)
                        {
                            info.Result.gstrMascara += "#";
                        }
                    }

                    if (conta.Nivel8 > 0)
                    {
                        info.Result.gstrMascara += "-";
                        info.Result.gstrNiveles += conta.Nivel8;
                        info.Result.gMascaraTChar += conta.Nivel8;

                        for (int i = 0; i < conta.Nivel8; i++)
                        {
                            info.Result.gstrMascara += "#";
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
