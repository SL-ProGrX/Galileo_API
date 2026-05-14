using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Globalization;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualEstadoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx; 
        private const string FrecuenciaQuincenalId = "Q";
        private const string Mensual = "Mensual";
        private const string PrimeraQuincena = "1er Quincena";
        private const string SegundaQuincena = "2da Quincena";
        private const int ValorPrimeraQuincena = 1;
        private const int ValorSegundaQuincena = 2;

        public CcProcesoMensualEstadoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config); 
        }

        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(int codEmpresa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
                var portalId = ObtenerPortalId(connection);
                var codigoAportes = ObtenerCodigoAportes(connection, globalesResp?.Result?.GInstitucion ?? 0);

                var response = new CcProcesoMensualInicialResponse
                {

                    Meses = ObtenerMeses(),
                    Aplicaciones = ObtenerAplicaciones(portalId),
                    MostrarAplicacion = portalId == 53 || portalId == 0,
                    HabilitarAhorros = !string.Equals(codigoAportes?.Trim(), "NO", StringComparison.OrdinalIgnoreCase),
                    Globales = new CcProcesoMensualGlobalesModel
                    {
                        GInstitucion = globalesResp?.Result?.GInstitucion ?? 0,
                        GNombreInstitucion = globalesResp?.Result?.GNombreInstitucion ?? string.Empty,
                        GlngFechaCR = globalesResp?.Result?.GlngFechaCR ?? 0
                    }
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualInicialResponse>(
                    "Error al obtener la configuración inicial del proceso mensual.",
                    -1,
                    new CcProcesoMensualInicialResponse());
            }
        }
        private static int ObtenerPortalId(IDbConnection connection)
        {
            const string query = @"SELECT Portal_Id FROM sif_Empresa";

            return connection.QueryFirstOrDefault<int>(query);
        }
        private static string ObtenerCodigoAportes(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
        SELECT codigo_aportes
        FROM instituciones
        WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<string>(
                query,
                new { CodInstitucion = codInstitucion }) ?? string.Empty;
        }
        private static List<DropDownListaGenericaModel> ObtenerMeses()
        {
            return
            [
                new() { item = 1, descripcion = "Enero" },
                new() { item = 2, descripcion = "Febrero" },
                new() { item = 3, descripcion = "Marzo" },
                new() { item = 4, descripcion = "Abril" },
                new() { item = 5, descripcion = "Mayo" },
                new() { item = 6, descripcion = "Junio" },
                new() { item = 7, descripcion = "Julio" },
                new() { item = 8, descripcion = "Agosto" },
                new() { item = 9, descripcion = "Setiembre" },
                new() { item = 10, descripcion = "Octubre" },
                new() { item = 11, descripcion = "Noviembre" },
                new() { item = 12, descripcion = "Diciembre" }
            ];
        }
        private static List<DropDownListaGenericaModel> ObtenerAplicaciones(int portalId)
        {
            var aplicaciones = new List<DropDownListaGenericaModel>
            {
                new() { item = 0, descripcion = "Mensual" }
            };

            if (portalId == 53 || portalId == 0)
            {
                aplicaciones.Add(new DropDownListaGenericaModel { item = 1, descripcion = "1er Quincena" });
                aplicaciones.Add(new DropDownListaGenericaModel { item = 2, descripcion = "2da Quincena" });
            }

            return aplicaciones;
        }
        public ErrorDto<CcProcesoMensualEstadoResponse> CcProcesoMensual_EstadoActualProceso_Obtener(int codEmpresa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
                var parametros = ObtenerParametrosInstitucion(connection, globalesResp?.Result?.GInstitucion ?? 0);

                if (parametros is null)
                {
                    return DbHelper.CreateOkResponse(new CcProcesoMensualEstadoResponse
                    {
                        ExisteParametroProceso = false,
                        Institucion = globalesResp?.Result?.GNombreInstitucion ?? string.Empty,
                        Mensaje = "NO EXISTEN PARAMETROS DEL PROCESO - !! DEBE CREARLOS ANTES DE ENTRAR AQUI !! "
                    });
                }

                var response = this.CrearEstadoResponse(
                    parametros,
                    globalesResp?.Result?.GNombreInstitucion ?? string.Empty,
                    globalesResp?.Result?.GlngFechaCR ?? 0);

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualEstadoResponse>(
                    "Error al obtener el estado actual del proceso mensual.",
                    -1,
                    new CcProcesoMensualEstadoResponse());
            }
        }
        private static CcProcesoMensualEstadoResponse CrearEstadoResponse(CcProcesoMensualInstitucionParametrosModel parametros, string nombreInstitucion, decimal fechaCr)
        {
            var fechaProcesoBase = Math.Truncate(fechaCr).ToString(CultureInfo.InvariantCulture);
            var ano = ObtenerAnoProceso(fechaProcesoBase);
            var mes = ObtenerMesProceso(fechaProcesoBase);
            var frecuenciaSeleccionada = ObtenerFrecuencia(parametros.Frecuencia_Id, fechaCr); 

            return new CcProcesoMensualEstadoResponse
            {
                ExisteParametroProceso = true,
                Institucion = nombreInstitucion, 
                Ano = ano,
                Mes = mes,
                MesDescripcion = MccFuncionesDb.ObtenerNombreMes(mes),
                FrecuenciaId = parametros.Frecuencia_Id,
                Frecuencias = ObtenerFrecuencias(parametros.Frecuencia_Id),
                FrecuenciaSeleccion = frecuenciaSeleccionada,
                Indicadores = CrearIndicadores(parametros), 
            };
        }
        private static CcProcesoMensualInstitucionParametrosModel? ObtenerParametrosInstitucion(IDbConnection connection, int codInstitucion)
        {
            const string query = @"
        SELECT 
            ISNULL(Frecuencia, 'M') AS Frecuencia_Id,
            pr_genera AS Pr_Genera,
            pr_carga AS Pr_Carga,
            pr_desgloza AS Pr_Desgloza,
            pr_apAplica AS Pr_ApAplica,
            pr_apInco AS Pr_ApInco,
            pr_apDev AS Pr_ApDev,
            pr_crAplica AS Pr_CrAplica,
            pr_crInco AS Pr_CrInco,
            pr_crMora AS Pr_CrMora
        FROM instituciones
        WHERE cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualInstitucionParametrosModel>(
                query,
                new { CodInstitucion = codInstitucion });
        }
        private static List<DropDownListaGenericaModel> ObtenerFrecuencias(string frecuenciaId)
        {
            if (string.Equals(frecuenciaId, FrecuenciaQuincenalId, StringComparison.OrdinalIgnoreCase))
            {
                return
                    [
                        new()
                        {item = ValorPrimeraQuincena,descripcion = PrimeraQuincena },
                        new()
                        {item = ValorSegundaQuincena,descripcion = SegundaQuincena }
                    ];
            }
            return
                    [
                        new()
                        { item = 0,descripcion = Mensual},
                        new()
                        {item = ValorPrimeraQuincena, descripcion = PrimeraQuincena },
                        new()
                        {item = ValorSegundaQuincena, descripcion = SegundaQuincena }
                    ];
        }
        private static int ObtenerAnoProceso(string fechaProceso)
        {
            return fechaProceso.Length >= 4
                ? int.Parse(fechaProceso[..4], CultureInfo.InvariantCulture)
                : 0;
        }
        private static int ObtenerMesProceso(string fechaProceso)
        {
            return fechaProceso.Length >= 6
                ? int.Parse(fechaProceso.AsSpan(4, 2), CultureInfo.InvariantCulture)
                : 0;
        }
        private static CcProcesoMensualFrecuenciaSeleccionModel ObtenerFrecuencia(string frecuenciaId, decimal fechaProceso)
        {
            if (!string.Equals(
                frecuenciaId,
                FrecuenciaQuincenalId,
                StringComparison.OrdinalIgnoreCase))
            {
                return new CcProcesoMensualFrecuenciaSeleccionModel
                {
                    FrecuenciaSeleccionada = Mensual,
                    SufijoFechaProceso = string.Empty
                };
            }

            var quincena = ObtenerParteQuincena(fechaProceso);

            if (quincena == 0.1m)
            {
                return new CcProcesoMensualFrecuenciaSeleccionModel
                {
                    FrecuenciaSeleccionada = PrimeraQuincena,
                    SufijoFechaProceso = "_Q1"
                };
            }

            return new CcProcesoMensualFrecuenciaSeleccionModel
            {
                FrecuenciaSeleccionada = SegundaQuincena,
                SufijoFechaProceso = "_Q2"
            };
        }
        private static decimal ObtenerParteQuincena(decimal fechaProceso)
        {
            return fechaProceso - Math.Truncate(fechaProceso);
        }
        private static CcProcesoMensualIndicadoresModel CrearIndicadores(CcProcesoMensualInstitucionParametrosModel parametros)
        {
            var indicadores = new CcProcesoMensualIndicadoresModel
            {
                Genera = parametros.Pr_Genera == 1,
                Fecha = parametros.Pr_Genera == 1,
                Carga = parametros.Pr_Carga == 1,
                Desgloce = parametros.Pr_Desgloza == 1,

                AhorrosAplica = parametros.Pr_ApAplica == 1,
                AhorrosInconsistencias = parametros.Pr_ApInco == 1,
                AhorrosDevolucion = parametros.Pr_ApDev == 1,

                CreditosAplica = parametros.Pr_CrAplica == 1,
                CreditosInconsistencias = parametros.Pr_CrInco == 1,
                CreditosRecalculo = parametros.Pr_CrMora == 1
            };

            AsignarOpcionesSeleccionadas(indicadores, parametros);

            return indicadores;
        }
        private static void AsignarOpcionesSeleccionadas(CcProcesoMensualIndicadoresModel indicadores, CcProcesoMensualInstitucionParametrosModel parametros)
        {
            if (parametros.Pr_Genera == 1)
            {
                indicadores.OpcionGeneralSeleccionada = 2;
            }

            if (parametros.Pr_Carga == 1 || parametros.Pr_Desgloza == 1)
            {
                indicadores.OpcionGeneralSeleccionada = 3;
            }

            if (parametros.Pr_ApAplica == 1)
            {
                indicadores.OpcionAhorrosSeleccionada = 1;
            }

            if (parametros.Pr_ApInco == 1)
            {
                indicadores.OpcionAhorrosSeleccionada = 2;
            }

            if (parametros.Pr_CrAplica == 1)
            {
                indicadores.OpcionCreditosSeleccionada = 1;
            }

            if (parametros.Pr_CrInco == 1)
            {
                indicadores.OpcionCreditosSeleccionada = 2;
            }

            if (parametros.Pr_CrMora == 1)
            {
                indicadores.OpcionCreditosSeleccionada = 3;
            }
        }

    }
}
