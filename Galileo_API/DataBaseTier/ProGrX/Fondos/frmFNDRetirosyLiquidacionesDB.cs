using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Fondos;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDRetirosyLiquidacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly mFNDFuncionesDB _mFNDFun;
        private readonly mProGrx_Main _mMain;
        private string productName = "";

        public frmFNDRetirosyLiquidacionesDB(IConfiguration? config)
        {
            _config = config;
            _mFNDFun = new mFNDFuncionesDB(_config);
            _mMain = new mProGrx_Main(_config);
            productName = _config.GetSection("AppSettings").GetSection("ProductName").Value.ToString();
        }

        public ErrorDTO SbSIFRegistraTags(SIFRegistraTagsRequestDTO data)
        {
            return _mMain.SbSIFRegistraTags(data);
        }

        /// <summary>
        /// Obtener rango de seguridad 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO<FND_SeguridadRango> FND_RetLiq_SeguridadRango_Obtener(int CodEmpresa, int Operadora, string Plan, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<FND_SeguridadRango>
            {
                Code = 0,
                Description = "Ok",
                Result = new FND_SeguridadRango()
            };
            try
            {
                string vParam = _mFNDFun.fxFndParametro(CodEmpresa, "01");
                if (!string.IsNullOrWhiteSpace(vParam) && vParam == "S")
                {
                    using var connection = new SqlConnection(stringConn);
                    {
                        var query = @$"exec spFndSeguridadRango @Operadora, @Plan, @Usuario";
                        var result = connection.QueryFirstOrDefault(query, new { Operadora, Plan, Usuario });

                        response.Result = new FND_SeguridadRango
                        {
                            mAutoInicio = result?.Inicio ?? 0,
                            mAutoCorte = result?.Corte ?? 0,
                            mAutorizacion = true
                        };
                    }
                } 
                else
                {
                    response.Result = new FND_SeguridadRango
                    {
                        mAutoInicio = 0,
                        mAutoCorte = 0,
                        mAutorizacion = false
                    };
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
        /// Obtener bancos asignados al usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_RetLiq_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select B.id_banco as item,dbo.fxSys_Cuenta_Bancos_Desc(B.id_Banco) as descripcion 
                        from tes_banco_asg T inner join Tes_Bancos B on T.id_banco = B.id_banco
                        where T.nombre = @Usuario";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Usuario }).ToList();
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

        public ErrorDTO<List<DropDownListaGenericaModel>> FND_RetLiq_CuentasBancarias_Obtener(int CodEmpresa, string Cedula, int Banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spSys_Cuentas_Bancarias @Cedula, @Banco, 1";
                    response.Result = connection.Query(query, new { Cedula, Banco })
                     .Select(row => new DropDownListaGenericaModel
                     {
                         item = row.IdX,
                         descripcion = row.itmX
                     }).ToList();
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
        /// Obtener conceptos de retencion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_RetLiq_RetencionConceptos_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select rtrim(RETENCION_CODIGO) as item, RTRIM(DESCRIPCION) as descripcion
                        From FND_RETENCION_CONCEPTOS  Where ACTIVO = 1
                        and dbo.fxFnd_Seguridad_Acceso_Concepto(@Usuario, RETENCION_CODIGO) = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Usuario }).ToList();
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
        /// Obtener planes destino
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Contrato"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_RetLiq_PlanesDestino_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spFndRetirosPlanesDestinos_List @Operadora, @Plan, @Contrato";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new { Operadora, Plan, Contrato }).ToList();
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
        /// Obtener lista de rebajos aplicables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<FND_RetLiq_RebajosData>> FND_RetLiq_Rebajos_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<FND_RetLiq_RebajosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FND_RetLiq_RebajosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select CODIGO, DESCRIPCION, '' AS DOCUMENTO, '' AS DETALLE, 0 AS 'MONTO'
                        From vFnd_Rebajos_Aplicables_List Where dbo.fxFnd_Seguridad_Acceso_Concepto(@Usuario, CODIGO) = 1";
                    response.Result = connection.Query<FND_RetLiq_RebajosData>(query, new { Usuario }).ToList();
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
        /// Obtener datos de consulta para retiro o liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Contrato"></param>
        /// <returns></returns>
        public ErrorDTO<FND_RetLiq_ConsultaData> FND_RetLiq_Consulta_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<FND_RetLiq_ConsultaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FND_RetLiq_ConsultaData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @$"select isnull(sif_liquida,0) as 'PermiteLiquidar' 
                        from fnd_Planes where cod_Operadora = @Operadora and Cod_Plan = @Plan";
                    int PermiteLiquidar = connection.QueryFirstOrDefault<int>(query, new { Operadora, Plan });

                    if (PermiteLiquidar == 1)
                    {
                        response.Result.permiteLiquidar = true;

                        query = "exec spFndRetLiqConsulta @Operadora, @Plan, @Contrato";
                        response.Result = connection.QueryFirstOrDefault<FND_RetLiq_ConsultaData>(query, new { Operadora, Plan, Contrato });

                        if (response.Result != null && response.Result.tipo_Pago != null) 
                        {
                            response.Result.tipo_Documento = _mFNDFun.fxgFNDTipoPago(CodEmpresa, "C", response.Result.tipo_Pago);
                        }
                    }
                    else
                    {
                        response.Result.permiteLiquidar = false;
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
        /// Obtener beneficiarios para pago a terceros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Contrato"></param>
        /// <param name="Cedula"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> FND_RetLiq_PagoTerceros_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, string Cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spFndPersonaBeneficiarios @Operadora, @Plan, @Contrato, @Cedula";
                    response.Result = connection.Query(query, new { Operadora, Plan, Contrato, Cedula })
                     .Select(row => new DropDownListaGenericaModel
                     {
                         item = row.cod_Beneficiario,
                         descripcion = row.tipo + "/" + row.cod_Beneficiario + "." + row.nombre
                     }).ToList();
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
        /// Obtener multa por retiro 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Contrato"></param>
        /// <param name="Monto"></param>
        /// <returns></returns>
        public ErrorDTO<decimal> FND_RetLiq_Multa_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, decimal Monto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<decimal>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                response.Result = _mFNDFun.fxgFNDCodigoMulta(CodEmpresa, Operadora, Plan, Contrato, Monto);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Obtener datos de renta global
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cedula"></param>
        /// <param name="RndRetiro"></param>
        /// <param name="Plan"></param>
        /// <returns></returns>
        public ErrorDTO<FND_RetLiq_RentaGlobalData> FND_RetLiq_RentaGlobal_Obtener(int CodEmpresa, string Cedula, decimal RndRetiro, string Plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<FND_RetLiq_RentaGlobalData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FND_RetLiq_RentaGlobalData()
            };
            DateTime Fecha = DateTime.Now;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spFnd_Renta_Global @Cedula, @Fecha, @RndRetiro, @Plan";
                    response.Result = connection.QueryFirstOrDefault<FND_RetLiq_RentaGlobalData>(query, new { Cedula, Fecha, RndRetiro, Plan });
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
        /// Aplicar retiro o liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDTO<FND_RetLiq_ProcesoData> FND_RetLiq_Aplicar(int CodEmpresa, string Filtro)
        {

            Filtros_RetLiq_Aplicar filtros = JsonConvert.DeserializeObject<Filtros_RetLiq_Aplicar>(Filtro) ?? new Filtros_RetLiq_Aplicar();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<FND_RetLiq_ProcesoData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FND_RetLiq_ProcesoData()
            };
            try
            {
                string tipoDoc = _mFNDFun.fxTipoDocumento(filtros.TipoDocumento);
                using var connection = new SqlConnection(stringConn);
                {
                    //Retiros en Cajas> Validacion
                    if (tipoDoc == "RC" && filtros.Proceso == "D")
                    {
                        var query = @$"select Valor from CAJAS_PARAMETROS where cod_parametro = '15'";
                        string valorStr = connection.QueryFirstOrDefault<string>(query);

                        if (!string.IsNullOrWhiteSpace(valorStr) && decimal.TryParse(valorStr, out decimal valorDecimal))
                        {
                            if (valorDecimal < filtros.MontoAplicar)
                            {
                                response.Description = "- El Monto Máximo para Retiros de Efectivos en Cajas es de " + valorDecimal + ", Informe a su Administrador!";
                                response.Code = -1;
                                return response;
                            }
                        }
                        else
                        {
                            response.Description = "- No se ha configurado el Monto para Retiros de Efectivos en Cajas, Informe a su Administrador!";
                            response.Code = -1;
                            return response;
                        }
                    }
                    //Validaciones Adicionales (CDPs y Otros)
                    var queryN = "select dbo.fxFndRetiroValida_Notas(@Operadora, @Plan, @Contrato, @Tipo, @Usuario) as 'Resultado'";
                    string notas = connection.QueryFirstOrDefault<string>(queryN,
                        new
                        {
                            filtros.Operadora,
                            filtros.Plan,
                            filtros.Contrato,
                            filtros.Tipo,
                            filtros.Usuario
                        }
                    );

                    if (notas.Length > 0)
                    {
                        response.Description = notas;
                        response.Code = -1;
                        return response;
                    }

                    int vPrimero = 1;
                    foreach (var item in filtros.RebajosLista)
                    {
                        var queryL = @"exec spFnd_Liquidacion_Rebajos @Usuario, @Contrato, @Plan, @Concepto, @Documento,
                            @Detalle, @Monto, @TipoCambio, @vPrimero";
                        connection.Execute(queryL,
                            new
                            {
                                filtros.Usuario,
                                filtros.Contrato,
                                filtros.Plan,
                                item.codigo,
                                item.documento,
                                item.detalle,
                                item.monto,
                                filtros.TipoCambio,
                                vPrimero
                            }
                        );
                        vPrimero = 0;
                    }

                    string gOficinaTitular = _mMain.CargaOficinas(filtros.Usuario, CodEmpresa).FirstOrDefault().Titular;

                    //Valida, aplica y envia a tesoreria (si es que aplica)
                    var strSQL = @"exec spFndRetLiqProceso @Operadora, @Plan, @Contrato, @Cedula, @MontoAplicar, 
                        @Tipo, @Notas, @Usuario, @gOficinaTitular, @Proceso, @RetCodigo, 
                        @BancoId, @tipoDoc, @CuentaBancaria, @App.ProductName";

                    if (filtros.chkPagoTercero == true && filtros.Proceso == "D")
                    {
                        strSQL += ",null,1, @PTTipo, @PTId, @PTNombre, @Rebajos";
                    } else
                    {
                        strSQL += ",null,0,'N','','', @Rebajos)";
                    }

                    response.Result = connection.QueryFirstOrDefault<FND_RetLiq_ProcesoData>(strSQL,
                        new
                        {
                            filtros.Operadora,
                            filtros.Plan,
                            filtros.Contrato,
                            filtros.Cedula,
                            filtros.MontoAplicar,
                            filtros.Tipo,
                            filtros.Notas,
                            filtros.Usuario,
                            gOficinaTitular,
                            filtros.Proceso,
                            filtros.RetCodigo,
                            filtros.BancoId,
                            tipoDoc,
                            filtros.CuentaBancaria,
                            productName,
                            filtros.Rebajos,
                            filtros.PTTipo,
                            filtros.PTId,
                            filtros.PTNombre
                        }
                    );

                    //sbTrazabilidad_Inserta de mProGrx_Main

                    SbSIFRegistraTags(new SIFRegistraTagsRequestDTO
                    {
                        Codigo = response.Result.liq_Num.ToString(),
                        Tag = "S10",
                        Usuario = filtros.Usuario.ToUpper(),
                        Observacion = "FND LIQ",
                        Documento = "0",
                        Modulo = "FLQ",
                    });
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
    }
}
