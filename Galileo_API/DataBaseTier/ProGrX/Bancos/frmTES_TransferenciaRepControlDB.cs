using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using PgxAPI.Models.TES;
using System.Text;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_TransferenciaRepControlDB
    {
        private readonly IConfiguration? _config;
        private string NumNegocio = "";
        private string CedulaReg = "";
        private string Razon = "";

        public frmTES_TransferenciaRepControlDB(IConfiguration config)
        {
            _config = config;
            NumNegocio = _config.GetSection("BCRFormat").GetSection("NumNegocio").Value.ToString();
            CedulaReg = _config.GetSection("BCRFormat").GetSection("CedulaReg").Value.ToString();
            Razon = _config.GetSection("BCRFormat").GetSection("Razon").Value.ToString();
        }

        /// <summary>
        /// Obtiene los catálogos para los dropdowns del formulario de 
        /// Informe de Control de Transferencias mediante el ID del banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <returns></returns>
        public ErrorDTO<TransferenciaRepControl_CatalogoDTO> TES_TransferenciaRepControl_Catalogos_Obtener(int CodEmpresa, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TransferenciaRepControl_CatalogoDTO>
            {
                Code = 0,
                Result = new TransferenciaRepControl_CatalogoDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query1 = "exec spTes_Formatos_Bancos @Banco";

                    var query2 = @"select rtrim(T.tipo) as IdX, rtrim(T.descripcion) as ItmX 
                        from tes_banco_docs D inner join tes_tipos_doc T on D.tipo = T.tipo 
                        where D.comprobante = '04' and D.id_Banco = @Banco";

                    var query3 = @"select Bp.COD_PLAN as IdX, Bp.COD_PLAN as ItmX
                        from TES_BANCOS B inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
                        Where B.ID_BANCO = @Banco And B.UTILIZA_PLAN = 1
                        order by Bp.COD_PLAN  asc";

                    response.Result.Formatos = connection.Query<DropDownCatalogoBancos>(query1, new { Banco }).ToList();
                    response.Result.Tipos = connection.Query<DropDownCatalogoBancos>(query2, new { Banco }).ToList();
                    response.Result.Planes = connection.Query<DropDownCatalogoBancos>(query3, new { Banco }).ToList();

                    if (response.Result.Planes == null || response.Result.Planes.Count == 0)
                    {
                        response.Result.Planes = new List<DropDownCatalogoBancos>
                        {
                            new DropDownCatalogoBancos { idx = "-sp-", itmx = "Sin Plan" }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Generar el archivo de Transferencias Bancarias en el formato seleccionado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="NTransac"></param>
        /// <param name="TipoDoc"></param>
        /// <param name="Formato"></param>
        /// <param name="Plan"></param>
        /// <returns></returns>
        public ErrorDTO<object> TES_TransferenciaRepControl_Archivo_Generar(int CodEmpresa, int Banco, int NTransac, string TipoDoc, string Formato, string Plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<object>
            {
                Code = 0
            };
            int vConsecutivo = 0;
            try
            {
                var queryTransac = @"Select * From Tes_Transacciones Where Estado = 'T' And Tipo = @TipoDoc 
                    And ID_Banco= @Banco And Autoriza='S' and documento_base = @NTransac
                    Order by Nsolicitud";
                var parametros = new
                {
                    Banco,
                    TipoDoc,
                    NTransac
                };

                using var connection = new SqlConnection(stringConn);
                {
                    switch (Formato)
                    {
                        case "A": //A - Banco Nacional

                            var queryA = @"Select sum(monto) as Monto From Tes_Transacciones Where Estado = 'T' 
                                And Tipo = @TipoDoc And ID_Banco= @Banco And Autoriza='S' and documento_base = @NTransac";
                            int vMonto = connection.QueryFirstOrDefault<int?>(queryA, parametros) ?? 0;

                            //Consulta del Detalle
                            var transaccionesList = connection.Query<TES_TransaccionDTO>(queryTransac, parametros).ToList();
                            response = sbTeBancoNacional(CodEmpresa, Banco, TipoDoc, NTransac, transaccionesList, vMonto);
                            break;
                        case "B": //B - Banco Popular

                            transaccionesList = connection.Query<TES_TransaccionDTO>(queryTransac, parametros).ToList();
                            response = sbTeBancoPopular(CodEmpresa, Banco, TipoDoc, NTransac, transaccionesList);

                            break;
                        case "C": //C - BCR

                            var queryC = @"select sum(dbo.fxTESBCRTestkey(cta_ahorros,monto)) as TestKeyX, sum(Monto) as Monto
                                From Tes_Transacciones 
                                Where Estado = 'T' And Tipo = @TipoDoc And ID_Banco= @Banco 
                                And Autoriza='S' and documento_base = @NTransac";
                            var resultC = connection.QueryFirstOrDefault(queryC, parametros);

                            long xTestKey = 0;
                            decimal totalMonto = 0;
                            if (resultC != null)
                            {
                                long testKeyX = resultC.TestKeyX ?? 0;
                                xTestKey = testKeyX > 2147483468 ? 2147483468 : testKeyX;
                                totalMonto = resultC.Monto ?? 0;
                            }

                            transaccionesList = connection.Query<TES_TransaccionDTO>(queryTransac, parametros).ToList();

                            response = sbTeBCR(CodEmpresa, Banco, TipoDoc, NTransac, transaccionesList, xTestKey, totalMonto);

                            break;
                        case "D": //D - BCR. Empresas

                            response = sbTeBCR_Empresarial(CodEmpresa, Banco, TipoDoc, NTransac);

                            break;
                        case "E": //E - BCT. Enlace

                            response = sbTeBCT_Enlace(CodEmpresa, Banco, TipoDoc, NTransac);

                            break;
                        case "F": //F - BCR. Comercial

                            response = sbTeBCR_Comercial(CodEmpresa, Banco, TipoDoc, NTransac);

                            break;
                        case "G": //G - BNCR SINPE

                            response = sbTeBNCR_Sinpe(CodEmpresa, Banco, TipoDoc, NTransac);

                            break;
                        case "DV1" or "DV2":

                            response = sbTeFormatoEstandar(CodEmpresa, Banco, TipoDoc, NTransac, Formato, Plan);

                            break;
                        case "S": //SINPE
                            break;
                        default:
                            response = sbTeFormatoEstandar(CodEmpresa, Banco, TipoDoc, NTransac, Formato, Plan);
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Emite la Transferencia en formato para el Banco Nacional. (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <param name="transaccionesList"></param>
        /// <param name="curPlanilla"></param>
        /// <returns></returns>
        public ErrorDTO<object> sbTeBancoNacional(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac, List<TES_TransaccionDTO> transaccionesList, int? curPlanilla)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            int BancoID = vBanco;
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            decimal curMonto1 = curPlanilla ?? 0;
            string strMonto = curMonto1.ToString("0000000000.00").Replace(".", "");
            string vCuentaEmpresa = "";
            string vNumCliente = "";
            decimal curMonto2 = 0;
            long curCuentas = 0;
            try
            {
                Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);
                string Empresa_Name = "TF " + seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa).PGX_CORE_DB;
                string vConcepto = Empresa_Name.PadRight(30, ' ');

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Cta,codigo_Cliente from tes_Bancos Where id_Banco = @banco";
                    var bancoData = connection.QueryFirstOrDefault(query, new { banco = BancoID });
                    if (bancoData != null)
                    {
                        vCuentaEmpresa = bancoData.Cta;
                        vCuentaEmpresa = vCuentaEmpresa.ToString().Trim().Replace("-", "");
                        vNumCliente = bancoData.codigo_Cliente;
                        vNumCliente = vNumCliente.PadLeft(6, '0');
                    }
                    query = "select DESCRIPCION from tes_Bancos where ID_BANCO = @banco";
                    string BancoNombre = connection.QueryFirstOrDefault<string>(query, new { banco = BancoID }) ?? "";

                    //Inicializa Variables de Bancos y Consecutivo
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //ENCABEZADO DEL FORMATO DE TRANSFERENCIA
                    string strCadena = "1";
                    strCadena += vNumCliente;
                    strCadena += vFecha.Day.ToString("00") + vFecha.Month.ToString("00") + vFecha.Year.ToString("0000");
                    strCadena += BancoID.ToString("D12"); // 12 dígitos con ceros a la izquierda
                    strCadena += "10000";
                    strCadena += strMonto;
                    strCadena += "000000000000000000000000"; // 24 ceros
                    sb.AppendLine(strCadena);

                    //DETALLE DE LA TRANSFERENCIA
                    int i = 0;
                    foreach (var item in transaccionesList)
                    {
                        i++;
                        string cuenta = (item.cta_ahorros ?? "").ToString().Replace("-", "").Trim();

                        string linea = "3"; //Credito
                        linea += cuenta.Substring(5, 3);   // Oficina apertura
                        linea += cuenta.Substring(0, 3);   // Tipo de cuenta (100 o 200)
                        linea += "01";                     // Moneda colones
                        linea += cuenta.Substring(cuenta.Length - 7); // 7 dígitos finales

                        decimal monto = (decimal)item.monto;
                        curMonto2 += monto;
                        curCuentas += long.Parse(cuenta.Substring(cuenta.Length - 7, 6)); // sin dígito verificador

                        linea += i.ToString("D8"); //8d Numero Comprobante (Consecutivo Interno)

                        string strMontoDet = monto.ToString("0000000000.00").Replace(".", ""); //12d Monto sin el punto decimal

                        linea += strMontoDet;
                        linea += vConcepto; //30d Concepto de Pago
                        linea += "00"; //Fin de Linea
                        sb.AppendLine(linea);
                    }

                    //CREA ULTIMA LINEA DE DETALLE CON EL DEBITO A LA EMPRESA 
                    strCadena = "2";
                    strCadena += vCuentaEmpresa.Substring(0, 3); // Movimiento de Debito, y 000 Sucursal de Apertura
                    strCadena += "10001"; //Cuenta Corriente y Moneda en Colones
                    strCadena += vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7); // 7 dígitos - Cuenta de la Empresa + Digito Verificador
                    strCadena += (i + 1).ToString("D8"); //Numero Comprobante

                    string strMontoEmpresa = curMonto2.ToString("0000000000.00").Replace(".", ""); //12d Monto sin el punto decimal
                    strCadena += strMontoEmpresa; //Total de los Creditos para Debitar a esta cuenta
                    strCadena += vConcepto; //30d Concepto de Pago
                    strCadena += "00"; //Fin de Linea
                    sb.AppendLine(strCadena);

                    curCuentas += long.Parse(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7, 6)); // sin verificador

                    // REGISTRO DE CONTROL DEL ARCHIVO DE TRANSFERENCIA 
                    string linea4 = "4"; //Codigo de Control de registro
                    decimal montoControl = curMonto1 + curMonto2; //Suma Debitos y Creditos de la Transferencia
                    string strMontoControl = montoControl.ToString("0000000000000.00").Replace(".", "");
                    linea4 += strMontoControl;
                    linea4 += curCuentas.ToString("D10"); //Sumatoria de Cuentas
                    linea4 += "0000000000";
                    linea4 += "000000000000";
                    linea4 += "000000000000";
                    linea4 += "00000000";
                    sb.AppendLine(linea4);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "ENV",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Emite la Transferencia en formato para el Banco Popular. (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <param name="transaccionesList"></param>
        /// <returns></returns>
        public ErrorDTO<object> sbTeBancoPopular(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac, List<TES_TransaccionDTO> transaccionesList)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = vBanco;
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    foreach (var item in transaccionesList)
                    {
                        switch (item.codigo!.Trim().Length)
                        {
                            case 8:
                                strCadena = "0" + item.codigo.Trim().Substring(0, 1) + "0" + item.codigo.Trim().Substring(1, 7);
                                break;
                            case 9:
                                strCadena = "0" + item.codigo.Trim();
                                break;
                            case < 8:
                                strCadena = Convert.ToInt64(item.codigo).ToString("D10");
                                break;
                            case > 10:
                                strCadena = item.codigo.Substring(0, 4) + "0" + item.codigo.Substring(5, 5);
                                break;
                            default:
                                strCadena = item.codigo.Trim();
                                break;
                        }

                        string strNombre = item.beneficiario!.Trim();

                        if (strNombre.Length > 30)
                        {
                            strNombre = strNombre.Substring(0, 30);
                        }
                        else
                        {
                            strNombre = strNombre.PadRight(30, ' ');
                        }
                        strCadena += strNombre;

                        string strCuenta = item.cta_ahorros == null ? "0" : item.cta_ahorros.Trim();

                        if (strCuenta.Length > 13)
                        {
                            strCuenta = strCuenta.Substring(0, 13);
                        }
                        else
                        {
                            strCuenta = strCuenta.PadLeft(13, '0');
                        }
                        strCadena += strCuenta;

                        string strSelf = " ";
                        strCadena += strSelf;

                        decimal monto = (decimal)item.monto!;
                        string strMonto = monto.ToString("000000000.00").Replace(".", "");

                        strCadena += strMonto;

                        string strFecha = string.Format("{0:ddMMyyyy}", vFecha);
                        strCadena += strFecha;

                        string strTipo = "A";
                        strCadena += strTipo;

                        string strProducto = "06";
                        strCadena += strProducto;

                        string strEstado = "P";
                        strCadena += strEstado;

                        strCadena += strFecha;
                        strCadena += strMonto;

                        sb.AppendLine(strCadena);
                    }

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Emite la Transferencia en formato para el Banco de Costa Rica. (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <param name="transaccionesList"></param>
        /// <param name="vTestKey"></param>
        /// <param name="vMontoTotal"></param>
        /// <returns></returns>
        public ErrorDTO<object> sbTeBCR(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac, List<TES_TransaccionDTO> transaccionesList, long vTestKey, decimal vMontoTotal)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string strCadena = "";
            try
            {
                string vRazon = Razon.PadRight(30, ' ');
                string vNumNegocio = NumNegocio;
                string vCedulaReg = CedulaReg;

                using var connection = new SqlConnection(stringConn);
                {
                    //Calcular el Numero de Archivo , Numero de la Transferencia en el Dia
                    int i = 1;
                    var query = @"select documento_base,count(*) From Tes_Transacciones 
                    where id_banco = @banco and fecha_emision = @fecha
                    and estado = 'T' group by documento_base";
                    var resultados = connection.QueryFirstOrDefault(query,
                        new { banco = vBanco, fecha = vFecha });
                    if (resultados != null)
                    {
                        foreach (var row in resultados)
                        {
                            i++;
                        }
                    }
                    string vConArchivo = i.ToString("D3");

                    //Crear y Sacar la cuenta de Tes_Bancos, se Asume que esta cuenta tiene el digito verificador
                    query = @"select Cta from Tes_Bancos where id_Banco = @banco";
                    string vCuentaBanco = connection.QueryFirstOrDefault<string>(query, new { banco = vBanco });
                    //Se indica la oficina 001 de apertura por Omision
                    vCuentaBanco = "001" + int.Parse(vCuentaBanco).ToString("D8");

                    //Calcular TestKey Complementario (de la primera Linea)
                    query = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                    int xTestKey = connection.QueryFirstOrDefault<int>(query,
                        new { cuentaBanco = vCuentaBanco, montoTotal = vMontoTotal });
                    vTestKey = Math.Min(vTestKey + xTestKey, 2147483468);

                    //Validando Largo del TestKey  = 12
                    string vTesKeyCh = vTestKey.ToString().Trim();
                    if (vTesKeyCh.Length > 12)
                    {
                        vTestKey = long.Parse(vTesKeyCh[^12..]);
                    }

                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = vBanco;
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //ENCABEZADO DEL FORMATO DE TRANSFERENCIA
                    strCadena = "000"; //Estado
                    strCadena += vNumNegocio;
                    strCadena += vConArchivo;
                    strCadena += "000000";
                    strCadena += vCedulaReg;
                    strCadena += Convert.ToInt64(vTestKey).ToString("D12"); //12 TestKey
                    strCadena += "000000";
                    strCadena += vFecha.Day.ToString("D2") + vFecha.Month.ToString("D2") + vFecha.Year.ToString("D4");
                    strCadena += new string(' ', 21);
                    strCadena += "Y"; //Señal de Y2k
                    sb.AppendLine(strCadena);

                    //DETALLE DE LA TRANSFERENCIA
                    //Linea 1 es la de Debito cuenta Bancaria
                    i = 1;
                    strCadena = "000"; //Estado Relleno con Ceros
                    strCadena += "1"; //Concepto 1 = Cuenta Corriente / 2 Cuenta Ahorro
                    strCadena += "00000"; //Filler 5
                    strCadena += vCuentaBanco.Trim().PadRight(11).Substring(0, 11); //Oficina -> 3c, Cuenta -> 7 + 1 Digito verificador
                    strCadena += "1"; //Moneda  1 = Colones, 2 = Dolares
                    strCadena += "4"; //2 -> Credito, 4 -> Debito
                    strCadena += "0000"; //Codigo de Causa
                    strCadena += BancoConsec.ToString("D4") + i.ToString("D4"); //Numero de Documento 8
                    strCadena += ((long)(vMontoTotal * 100)).ToString("D12"); //12 Sin Decimales
                    strCadena += vFecha.Day.ToString("D2") + vFecha.Month.ToString("D2") + vFecha.Year.ToString("D4");
                    strCadena += "0"; //Filler 1
                    strCadena += vRazon; //Razon de Transferencia (Detalle) 30
                    sb.AppendLine(strCadena);

                    foreach (var item in transaccionesList)
                    {
                        i = i + 1;
                        strCadena = "000"; //Estado Relleno con Ceros
                        strCadena += "2"; //Concepto 1 = Cuenta Corriente / 2 Cuenta Ahorro
                        strCadena += "00000"; //Filler 5
                        strCadena += item.cta_ahorros.PadRight(11).Substring(0, 11).Trim(); //Oficina -> 3c, Cuenta -> 7 + 1 Digito verificador
                        strCadena += "1"; //Moneda  1 = Colones, 2 = Dolares
                        strCadena += "2"; //2 -> Credito, 4 -> Debito
                        strCadena += "0000"; //Codigo de Causa
                        strCadena += BancoConsec.ToString("D4") + i.ToString("D4"); //Numero de Documento 8
                        strCadena += ((int)(item.monto * 100)).ToString("D12"); //12 Sin Decimales
                        strCadena += vFecha.Day.ToString("D2") + vFecha.Month.ToString("D2") + vFecha.Year.ToString("D4");
                        strCadena += "0"; //Filler 1
                        strCadena += vRazon; //Razon de Transferencia (Detalle) 30
                        sb.AppendLine(strCadena);
                    }

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "BCR",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCR, Banca Empresarial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <returns></returns>
        private ErrorDTO<object> sbTeBCR_Empresarial(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now;
            string vRazon = "";
            string vNumNegocio = "";
            string vCedulaReg = "";
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA";
                    var empresa = connection.QueryFirstOrDefault(query);
                    vNumNegocio = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vCedulaReg = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vRazon = "TRANSFERENCIAS " + empresa?.nombre ?? string.Empty;

                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = vBanco;
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //Calcular el Numero de Archivo , Numero de la Transferencia en el Dia
                    int i = 1;
                    query = @"select documento_base,count(*) From Tes_Transacciones 
                    where id_banco = @banco and fecha_emision = @fecha
                    and estado = 'T' group by documento_base";
                    var resultados = connection.QueryFirstOrDefault(query,
                        new { banco = BancoID, fecha = vFecha });
                    if (resultados != null)
                    {
                        foreach (var row in resultados)
                        {
                            i++;
                        }
                    }
                    string vConArchivo = i.ToString("D3");

                    query = @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as 'Cantidad'";
                    int iLineInicio = connection.QueryFirstOrDefault<int>(query,
                        new { banco = BancoID, fecha = vFecha });

                    //REGISTRO DE CONTROL
                    i = 1;
                    strCadena = "000"; //Estado 3
                    strCadena += (vCedulaReg ?? "").Trim().PadLeft(12, '0'); //Cedula Juridica 12
                    strCadena += vConArchivo; //Consecutivo Archivo 3
                    strCadena += vFecha.ToString("ddMMyyyy"); //Fecha Aplicacion 8
                    strCadena += "000000000000"; //Cedula de Registro 12
                    strCadena += "000000000000"; //12 TestKey  no se genera, se rellena con ceros
                    strCadena += "000000"; //6 Hora Estado Se rellena con ceros
                    strCadena += new string(' ', 6); //filler 6 espacios en blanco
                    strCadena += "TLB"; //Tipo de archivo
                    strCadena += new string(' ', 128); //filler 128 espacios en blanco
                    strCadena += "D"; //Tipo de movinento Debido
                    sb.AppendLine(strCadena);

                    //DEBITOS
                    query = @"exec spTES_BCR_Empresarial_Archivo 2, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, 100000";
                    var Linea2 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec
                        });
                    sb.AppendLine(Linea2);

                    //CREDITOS
                    query = @"exec spTES_BCR_Empresarial_Archivo 3, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, 100000";
                    var Linea3 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec
                        });
                    sb.AppendLine(Linea3);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCT
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <returns></returns>
        private ErrorDTO<object> sbTeBCT_Enlace(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = vBanco;
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //DETALLE DE LA TRANSFERENCIA
                    var query = @"exec spTES_BCT_Enlace_ArchivoLog @banco, @bancoTDoc, @bancoConsec";
                    var resultado = connection.QueryFirstOrDefault(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            bancoConsec = BancoConsec
                        });

                    strCadena = resultado.Linea;
                    sb.AppendLine(strCadena);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCR, Banca Comercial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <returns></returns>
        private ErrorDTO<object> sbTeBCR_Comercial(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string vRazon = "";
            string vNumNegocio = "";
            string vCedulaReg = "";
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA";
                    var empresa = connection.QueryFirstOrDefault(query);
                    vNumNegocio = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vCedulaReg = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vRazon = "TRANSFERENCIAS " + empresa?.nombre ?? string.Empty;

                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = vBanco;
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //Calcular el Numero de Archivo , Numero de la Transferencia en el Dia
                    int i = 1;
                    query = @"select documento_base,count(*) From Tes_Transacciones 
                    where id_banco = @banco and fecha_emision = @fecha
                    and estado = 'T' group by documento_base";
                    var resultados = connection.QueryFirstOrDefault(query,
                        new { banco = BancoID, fecha = vFecha });
                    if (resultados != null)
                    {
                        foreach (var row in resultados)
                        {
                            i++;
                        }
                    }
                    string vConArchivo = i.ToString("D3");

                    query = @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as 'Cantidad'";
                    int iLineInicio = connection.QueryFirstOrDefault<int>(query,
                        new { banco = BancoID, fecha = vFecha });

                    //REGISTRO DE CONTROL
                    i = 1;
                    strCadena = "000"; //Estado 3
                    strCadena += (vCedulaReg ?? "").Trim().PadLeft(12, '0'); //Cedula Juridica 12
                    strCadena += vConArchivo; //Consecutivo Archivo 3
                    strCadena += vFecha.ToString("ddMMyyyy"); //Fecha Aplicacion 8
                    strCadena += "000000000000"; //Cedula de Registro 12
                    strCadena += "000000000000"; //12 Filler con 0
                    strCadena += "000000"; //6 Hora Estado Se rellena con ceros
                    strCadena += "".PadRight(138, '0'); //filler 138 con 0
                    sb.AppendLine(strCadena);

                    //DEBITOS
                    query = @"exec spTES_BCR_Comercial_Archivo 2, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, 100000";
                    var Linea2 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec
                        });
                    sb.AppendLine(Linea2);

                    //CREDITOS
                    query = @"exec spTES_BCR_Comercial_Archivo 3, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, 100000";
                    var Linea3 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec
                        });
                    sb.AppendLine(Linea3);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Emite la Transferencia en formato SINPE para el BNCR
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <returns></returns>
        private ErrorDTO<object> sbTeBNCR_Sinpe(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = vBanco;
                    string BancoTDoc = vTipoDoc;
                    long BancoConsec = vNTransac;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //ENCABEZADO: LINEA 1
                    var query = @"exec spTES_BNCR_SINPE_Archivo 1, @banco, @bancoTDoc, 
                        @bancoConsec, 0";
                    var Linea1 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            bancoConsec = BancoConsec
                        });

                    sb.AppendLine(Linea1);

                    //DEBITOS
                    query = @"exec spTES_BNCR_SINPE_Archivo 2, @banco, @bancoTDoc, 
                        @bancoConsec, 0";
                    var Linea2 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            bancoConsec = BancoConsec
                        });
                    sb.AppendLine(Linea2);

                    //CREDITOS
                    query = @"exec spTES_BNCR_SINPE_Archivo 3, @banco, @bancoTDoc, 
                        @bancoConsec, 0";
                    var Linea3 = connection.QueryFirstOrDefault<string>(query,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            bancoConsec = BancoConsec
                        });
                    sb.AppendLine(Linea3);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "tef",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Generacion con Formatos Estandares de Transferencias Bancarias (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vBanco"></param>
        /// <param name="vTipoDoc"></param>
        /// <param name="vNTransac"></param>
        /// <param name="vFormato"></param>
        /// <param name="vPlan"></param>
        /// <returns></returns>
        private ErrorDTO<object> sbTeFormatoEstandar(int CodEmpresa, int vBanco, string vTipoDoc, int vNTransac, string vFormato, string vPlan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO<object>
            {
                Code = 0,
                Description = ""
            };
            string pFormato = vFormato;
            int BancoID = vBanco;
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string vNumNegocio = "";
            string vCedulaReg = "";
            string vRazon = "";
            string vExtension = "";
            string vProcedimiento = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select REPLACE(cedula_juridica,'-','') as 'cedula_Juridica',NOMBRE From SIF_EMPRESA";
                    var empresaData = connection.QueryFirstOrDefault(query);
                    if (empresaData != null)
                    {
                        vNumNegocio = empresaData.cedula_Juridica;
                        vCedulaReg = empresaData.cedula_Juridica;
                        vRazon = "TRANSFERENCIAS " + empresaData.nombre;
                    }

                    query = "select Procedimiento,Extension from vTes_Formatos where cod_formato = @formato";
                    var formatoData = connection.QueryFirstOrDefault(query, new { formato = pFormato });
                    if (formatoData != null)
                    {
                        vExtension = formatoData.Extension;
                        vProcedimiento = formatoData.Procedimiento;
                    }

                    query = "select DESCRIPCION from tes_Bancos where ID_BANCO = @banco";
                    string BancoNombre = connection.QueryFirstOrDefault<string>(query, new { banco = BancoID }) ?? "";

                    //Inicializa Variables de Bancos y Consecutivo
                    string BancoTDoc = vTipoDoc;
                    string BancoPlan = vPlan;
                    long BancoConsec = vNTransac;

                    int i = 1;
                    query = @"SELECT COUNT(DISTINCT documento_base)
                              FROM   Tes_Transacciones
                              WHERE  id_banco = @banco
                              AND    CONVERT(VARCHAR, fecha_emision, 106) = @fecha
                              AND    estado = 'T'";
                    i = connection.QueryFirstOrDefault<int>(
                        query, new
                        {
                            banco = BancoID,
                            fecha = vFecha.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture)
                        }) + 1;
                    string vConArchivo = i.ToString("000");

                    query = "SELECT dbo.fxTesCantidadTEDiarias(@fecha, @banco) AS Cantidad";
                    int iLineInicio = connection.QueryFirstOrDefault<int>(
                        query, new
                        {
                            fecha = vFecha.ToString("yyyy/MM/dd"),
                            banco = BancoID
                        }
                    );

                    // En vez de guardar el archivo, se devuelve como string
                    var sb = new StringBuilder();
                    for (int numLinea = 1; numLinea <= 3; numLinea++)
                    {
                        //LINEA CONTROL
                        var queryLinea = $@"EXEC {vProcedimiento}_Archivo 1, @bancoID, @bancoTDoc, @numNegocio, 
                            @bancoConsec, 100000, @bancoPlan";

                        var parametros = new
                        {
                            bancoID = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec,
                            bancoPlan = BancoPlan
                        };

                        var lineasList = connection.Query<string>(queryLinea, parametros).ToList();
                        foreach (var linea in lineasList)
                        {
                            if (!string.IsNullOrWhiteSpace(linea))
                            {
                                sb.AppendLine(linea);
                            }
                        }
                    }

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = vExtension,
                        contenido = sb.ToString()

                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }
    }
}
