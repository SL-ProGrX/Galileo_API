using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndRetirosyLiquidacionesDB
    {
        private readonly IConfiguration _config;
        private readonly MFndFuncionesDb _mFNDFun;
        private readonly MProGrxMain _mMain;
        private readonly string productName = string.Empty;

        private const string SpSeguridadRango = "spFndSeguridadRango";
        private const string SpCuentasBancarias = "spSys_Cuentas_Bancarias";
        private const string SpPlanesDestino = "spFndRetirosPlanesDestinos_List";
        private const string SpConsultaRetiro = "spFndRetLiqConsulta";
        private const string SpPagoTerceros = "spFndPersonaBeneficiarios";
        private const string SpRentaGlobal = "spFnd_Renta_Global";
        private const string SpLiquidacionRebajos = "spFnd_Liquidacion_Rebajos";
        private const string SpRetLiqProceso = "spFndRetLiqProceso";

        private const string SqlBancos = @"
                    SELECT
                        B.id_banco AS item,
                        dbo.fxSys_Cuenta_Bancos_Desc(B.id_Banco) AS descripcion
                    FROM dbo.tes_banco_asg T
                    INNER JOIN dbo.Tes_Bancos B
                        ON T.id_banco = B.id_banco
                    WHERE T.nombre = @Usuario;";

        private const string SqlConceptosRetencion = @"
                    SELECT
                        RTRIM(RETENCION_CODIGO) AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.FND_RETENCION_CONCEPTOS
                    WHERE ACTIVO = 1
                      AND dbo.fxFnd_Seguridad_Acceso_Concepto(@Usuario, RETENCION_CODIGO) = 1;";

        private const string SqlRebajos = @"
                    SELECT
                        CODIGO,
                        DESCRIPCION,
                        '' AS DOCUMENTO,
                        '' AS DETALLE,
                        0 AS MONTO
                    FROM dbo.vFnd_Rebajos_Aplicables_List
                    WHERE dbo.fxFnd_Seguridad_Acceso_Concepto(@Usuario, CODIGO) = 1;";

        private const string SqlPermiteLiquidar = @"
                    SELECT ISNULL(sif_liquida, 0)
                    FROM dbo.fnd_Planes
                    WHERE cod_Operadora = @Operadora
                      AND Cod_Plan = @Plan;";

        private const string SqlMontoCaja = @"
                    SELECT Valor
                    FROM dbo.CAJAS_PARAMETROS
                    WHERE cod_parametro = '15';";

        private const string SqlValidaNotas = @"
                    SELECT dbo.fxFndRetiroValida_Notas(
                        @Operadora,
                        @Plan,
                        @Contrato,
                        @Tipo,
                        @Usuario) AS Resultado;";

        public FrmFndRetirosyLiquidacionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mFNDFun = new MFndFuncionesDb(_config);
            _mMain = new MProGrxMain(_config);
            productName = _config.GetSection("AppSettings").GetSection("ProductName").Value?.ToString() ?? string.Empty;
        }

        public ErrorDto SbSIFRegistraTags(SifRegistraTagsRequestDto data)
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
        public ErrorDto<FndSeguridadRango> FND_RetLiq_SeguridadRango_Obtener(int CodEmpresa, int Operadora, string Plan, string Usuario)
        {
            var response = DbHelper.CreateOkResponse(new FndSeguridadRango());

            try
            {
                string vParam = _mFNDFun.fxFndParametro(CodEmpresa, "01");
                if (!string.Equals(vParam, "S", StringComparison.OrdinalIgnoreCase))
                {
                    response.Result = new FndSeguridadRango
                    {
                        mAutoInicio = 0,
                        mAutoCorte = 0,
                        mAutorizacion = false
                    };

                    return response;
                }

                var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                    connection.QueryFirstOrDefault(
                        SpSeguridadRango,
                        new
                        {
                            Operadora,
                            Plan = NormalizarTexto(Plan),
                            Usuario = NormalizarTexto(Usuario)
                        },
                        commandType: System.Data.CommandType.StoredProcedure));

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        result.Description ?? "Error al obtener seguridad de rango.",
                        result.Code.GetValueOrDefault(-1),
                        new FndSeguridadRango());
                }

                response.Result = new FndSeguridadRango
                {
                    mAutoInicio = result.Result?.Inicio ?? 0,
                    mAutoCorte = result.Result?.Corte ?? 0,
                    mAutorizacion = true
                };
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(ex.Message, -1, new FndSeguridadRango());
            }

            return response;
        }

        /// <summary>
        /// Obtener bancos asignados al usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlBancos,
                new { Usuario = NormalizarTexto(Usuario) });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_CuentasBancarias_Obtener(int CodEmpresa, string Cedula, int Banco)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query(
                    SpCuentasBancarias,
                    new
                    {
                        Cedula = NormalizarTexto(Cedula),
                        Banco,
                        Tipo = 1
                    },
                    commandType: System.Data.CommandType.StoredProcedure)
                .Select(row => new DropDownListaGenericaModel
                {
                    item = row.IdX,
                    descripcion = row.itmX
                }).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Obtener conceptos de retencion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_RetencionConceptos_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlConceptosRetencion,
                new { Usuario = NormalizarTexto(Usuario) });
        }

        /// <summary>
        /// Obtener planes destino
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_PlanesDestino_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query<DropDownListaGenericaModel>(
                    SpPlanesDestino,
                    new
                    {
                        Operadora,
                        Plan = NormalizarTexto(Plan),
                        Contrato
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Obtener lista de rebajos aplicables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRetLiqRebajosData>> FND_RetLiq_Rebajos_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.ExecuteListQuery<FndRetLiqRebajosData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlRebajos,
                new { Usuario = NormalizarTexto(Usuario) });
        }

        /// <summary>
        /// Obtener datos de consulta para retiro o liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Operadora"></param>
        /// <param name="Plan"></param>
        /// <param name="Contrato"></param>
        /// <returns></returns>
        public ErrorDto<FndRetLiqConsultaData> FND_RetLiq_Consulta_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            var permite = DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlPermiteLiquidar,
                0,
                new
                {
                    Operadora,
                    Plan = NormalizarTexto(Plan)
                });

            if (permite.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    permite.Description ?? "Error al validar liquidación del plan.",
                    permite.Code.GetValueOrDefault(-1),
                    new FndRetLiqConsultaData());
            }

            if (permite.Result != 1)
            {
                return DbHelper.CreateOkResponse(new FndRetLiqConsultaData
                {
                    permiteLiquidar = false
                });
            }

            var consulta = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndRetLiqConsultaData>(
                    SpConsultaRetiro,
                    new
                    {
                        Operadora,
                        Plan = NormalizarTexto(Plan),
                        Contrato
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (consulta.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    consulta.Description ?? "Error al consultar retiro o liquidación.",
                    consulta.Code.GetValueOrDefault(-1),
                    new FndRetLiqConsultaData());
            }

            var data = consulta.Result ?? new FndRetLiqConsultaData();
            data.permiteLiquidar = true;

            if (data.tipo_Pago != null)
            {
                data.tipo_Documento = _mFNDFun.fxgFNDTipoPago(CodEmpresa, "C", data.tipo_Pago);
            }

            return DbHelper.CreateOkResponse(data);
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
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_PagoTerceros_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, string Cedula)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Query(
                    SpPagoTerceros,
                    new
                    {
                        Operadora,
                        Plan = NormalizarTexto(Plan),
                        Contrato,
                        Cedula = NormalizarTexto(Cedula)
                    },
                    commandType: System.Data.CommandType.StoredProcedure)
                .Select(row => new DropDownListaGenericaModel
                {
                    item = row.cod_Beneficiario,
                    descripcion = row.tipo + "/" + row.cod_Beneficiario + "." + row.nombre
                }).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
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
        public ErrorDto<decimal> FND_RetLiq_Multa_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, decimal Monto)
        {
            var response = new ErrorDto<decimal>
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
        public ErrorDto<FndRetLiqRentaGlobalData> FND_RetLiq_RentaGlobal_Obtener(int CodEmpresa, string Cedula, decimal RndRetiro, string Plan)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndRetLiqRentaGlobalData>(
                    SpRentaGlobal,
                    new
                    {
                        Cedula = NormalizarTexto(Cedula),
                        Fecha = DateTime.Now,
                        RndRetiro,
                        Plan = NormalizarTexto(Plan)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndRetLiqRentaGlobalData>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndRetLiqRentaGlobalData()
            };
        }

        /// <summary>
        /// Aplicar retiro o liquidacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<FndRetLiqProcesoData> FND_RetLiq_Aplicar(int CodEmpresa, string Filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosRetLiqAplicar>(Filtro) ?? new FiltrosRetLiqAplicar();
            var response = new ErrorDto<FndRetLiqProcesoData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndRetLiqProcesoData()
            };

            try
            {
                var tipoDoc = MFndFuncionesDb.fxTipoDocumento(NormalizarTexto(filtros.TipoDocumento));
                var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                {
                    ValidarRetiroCaja(connection, tipoDoc, filtros);
                    ValidarNotas(connection, filtros);
                    RegistrarRebajosLiquidacion(connection, filtros);

                    var oficina = _mMain.CargaOficinas(NormalizarTexto(filtros.Usuario), CodEmpresa).FirstOrDefault();
                    var oficinaTitular = oficina?.Titular ?? string.Empty;

                    var proceso = EjecutarProcesoRetiroLiquidacion(connection, filtros, tipoDoc, oficinaTitular);
                    RegistrarTagLiquidacion(proceso, filtros);
                    return proceso;
                });

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        result.Description ?? "Error al aplicar retiro o liquidación.",
                        result.Code.GetValueOrDefault(-1),
                        new FndRetLiqProcesoData());
                }

                response.Result = result.Result ?? new FndRetLiqProcesoData();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        private static void ValidarRetiroCaja(System.Data.IDbConnection connection, string tipoDoc, FiltrosRetLiqAplicar filtros)
        {
            if (tipoDoc != "RC" || filtros.Proceso != "D")
            {
                return;
            }

            var valorStr = connection.QueryFirstOrDefault<string>(SqlMontoCaja);
            if (!string.IsNullOrWhiteSpace(valorStr) && decimal.TryParse(valorStr, out var valorDecimal))
            {
                if (valorDecimal < filtros.MontoAplicar)
                {
                    throw new InvalidOperationException("- El Monto Máximo para Retiros de Efectivos en Cajas es de " + valorDecimal + ", Informe a su Administrador!");
                }

                return;
            }

            throw new InvalidOperationException("- No se ha configurado el Monto para Retiros de Efectivos en Cajas, Informe a su Administrador!");
        }

        private static void ValidarNotas(System.Data.IDbConnection connection, FiltrosRetLiqAplicar filtros)
        {
            var notas = connection.QueryFirstOrDefault<string>(SqlValidaNotas, new
            {
                filtros.Operadora,
                Plan = NormalizarTexto(filtros.Plan),
                filtros.Contrato,
                Tipo = NormalizarTexto(filtros.Tipo),
                Usuario = NormalizarTexto(filtros.Usuario)
            });

            if (!string.IsNullOrWhiteSpace(notas))
            {
                throw new InvalidOperationException(notas);
            }
        }

        private static void RegistrarRebajosLiquidacion(System.Data.IDbConnection connection, FiltrosRetLiqAplicar filtros)
        {
            var primero = 1;
            foreach (var item in filtros.RebajosLista ?? new List<FndRetLiqRebajosData>())
            {
                connection.Execute(
                    SpLiquidacionRebajos,
                    new
                    {
                        Usuario = NormalizarTexto(filtros.Usuario),
                        filtros.Contrato,
                        Plan = NormalizarTexto(filtros.Plan),
                        Concepto = NormalizarTexto(item.codigo),
                        Documento = NormalizarTexto(item.documento),
                        Detalle = NormalizarTexto(item.detalle),
                        Monto = item.monto,
                        filtros.TipoCambio,
                        vPrimero = primero
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                primero = 0;
            }
        }

        private FndRetLiqProcesoData EjecutarProcesoRetiroLiquidacion(
            System.Data.IDbConnection connection,
            FiltrosRetLiqAplicar filtros,
            string tipoDoc,
            string oficinaTitular)
        {
            var pagoTercero = filtros.chkPagoTercero && filtros.Proceso == "D";

            return connection.QueryFirstOrDefault<FndRetLiqProcesoData>(
                SpRetLiqProceso,
                new
                {
                    filtros.Operadora,
                    Plan = NormalizarTexto(filtros.Plan),
                    filtros.Contrato,
                    Cedula = NormalizarTexto(filtros.Cedula),
                    filtros.MontoAplicar,
                    Tipo = NormalizarTexto(filtros.Tipo),
                    Notas = NormalizarTexto(filtros.Notas),
                    Usuario = NormalizarTexto(filtros.Usuario),
                    gOficinaTitular = oficinaTitular,
                    Proceso = NormalizarTexto(filtros.Proceso),
                    RetCodigo = NormalizarTexto(filtros.RetCodigo),
                    filtros.BancoId,
                    tipoDoc,
                    CuentaBancaria = NormalizarTexto(filtros.CuentaBancaria),
                    ProductName = productName,
                    PagoTercero = pagoTercero ? 1 : 0,
                    PTTipo = pagoTercero ? NormalizarTexto(filtros.PTTipo) : "N",
                    PTId = pagoTercero ? NormalizarTexto(filtros.PTId) : string.Empty,
                    PTNombre = pagoTercero ? NormalizarTexto(filtros.PTNombre) : string.Empty,
                    Rebajos = filtros.Rebajos
                },
                commandType: System.Data.CommandType.StoredProcedure) ?? new FndRetLiqProcesoData();
        }

        private void RegistrarTagLiquidacion(FndRetLiqProcesoData proceso, FiltrosRetLiqAplicar filtros)
        {
            if (proceso.liq_Num <= 0)
            {
                return;
            }

            SbSIFRegistraTags(new SifRegistraTagsRequestDto
            {
                Codigo = proceso.liq_Num.ToString(),
                Tag = "S10",
                Usuario = NormalizarTexto(filtros.Usuario).ToUpperInvariant(),
                Observacion = "FND LIQ",
                Documento = "0",
                Modulo = "FLQ",
            });
        }
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
