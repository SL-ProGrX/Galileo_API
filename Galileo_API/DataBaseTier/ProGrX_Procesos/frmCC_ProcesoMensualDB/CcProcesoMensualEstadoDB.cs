using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Globalization;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;


namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualEstadoDB
    {
        private readonly PortalDB _portalDb;

        public CcProcesoMensualEstadoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(int codEmpresa, int gInstitucion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var portalId = ObtenerPortalId(connection);
                var codigoAportes = ObtenerCodigoAportes(connection, gInstitucion);

                var response = new CcProcesoMensualInicialResponse
                {

                    Meses = ObtenerMeses(),
                    Aplicaciones = ObtenerAplicaciones(portalId),
                    MostrarAplicacion = portalId == 53 || portalId == 0,
                    HabilitarAhorros = !string.Equals(codigoAportes?.Trim(), "NO", StringComparison.OrdinalIgnoreCase),
                    FechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection),

                    EstadoActual = CcProcesoMensual_EstadoActualProceso_Obtener(codEmpresa, gInstitucion).Result ?? new CcProcesoMensualEstadoResponse()
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
        public ErrorDto<CcProcesoMensualEstadoResponse> CcProcesoMensual_EstadoActualProceso_Obtener(int codEmpresa, int gInstitucion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var parametros = ObtenerParametrosInstitucion(connection, gInstitucion);

                if (parametros is null)
                {
                    return DbHelper.CreateOkResponse(new CcProcesoMensualEstadoResponse
                    {
                        ExisteParametroProceso = false,
                        Mensaje = "NO EXISTEN PARAMETROS DEL PROCESO - !! DEBE CREARLOS ANTES DE ENTRAR AQUI !! "
                    });
                }

                var response = CrearEstadoResponse(
                    parametros);

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
        private static CcProcesoMensualEstadoResponse CrearEstadoResponse(CcProcesoMensualInstitucionParametrosModel parametros)
        {
            return new CcProcesoMensualEstadoResponse
            {
                ExisteParametroProceso = true,
                FrecuenciaId = parametros.Frecuencia_Id,
                Indicadores = CrearIndicadores(parametros) ?? new CcProcesoMensualIndicadoresModel(),
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
        public ErrorDto<CcProcesoMensualValidaPasoResponse> CcProcesoMensual_ValidaPaso(int codEmpresa, int codInstitucion, decimal fechaProceso, string transaccion = "08")
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var request = new CcProcesoMensualValidaPasoRequest
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso,
                    Transaccion = string.IsNullOrWhiteSpace(transaccion)
                        ? "08"
                        : transaccion.Trim()
                };

                var resultado = ValidarPaso(connection, request);

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualValidaPasoResponse>(
                    "Error al validar el paso del proceso mensual.",
                    -1,
                    new CcProcesoMensualValidaPasoResponse
                    {
                        Valido = false,
                        Mensaje = "Error al validar el paso del proceso mensual."
                    });
            }
        }
        private static CcProcesoMensualValidaPasoResponse ValidarPaso(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            var bloqueoAplicacion = ValidarBloqueoPorAplicacionFutura(connection, request);

            if (!bloqueoAplicacion.Valido)
            {
                return bloqueoAplicacion;
            }

            var bloqueoPatrimonio = ValidarBloqueoPatrimonio(connection, request);

            if (!bloqueoPatrimonio.Valido)
            {
                return bloqueoPatrimonio;
            }

            var cuadreAbonos = ValidarCuadreAbonos(connection, request);

            if (!cuadreAbonos.Valido)
            {
                return cuadreAbonos;
            }

            var desglose = ValidarDesgloseRealizado(connection, request);

            if (!desglose.Valido)
            {
                return desglose;
            }

            var mesAnterior = ValidarPlanillaMesAnterior(connection, request);

            if (!mesAnterior.Valido)
            {
                return mesAnterior;
            }

            return CrearResultadoValido();
        }
        private static CcProcesoMensualValidaPasoResponse ValidarBloqueoPorAplicacionFutura(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            if (string.Compare(request.Transaccion, "09", StringComparison.Ordinal) < 0)
            {
                var existe = ExisteBitacoraDesdeProceso(
                    connection,
                    request.CodInstitucion,
                    request.FechaProceso,
                    "08");

                if (existe)
                {
                    return CrearResultadoInvalido(
                        "No se puede realizar el movimiento seleccionado ya que se ha aplicado esta planilla y/o otra futura en los auxiliares... verifique.!");
                }
            }

            return CrearResultadoValido();
        }
        private static CcProcesoMensualValidaPasoResponse ValidarBloqueoPatrimonio(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            if (!EsTransaccion(request.Transaccion, "05"))
            {
                return CrearResultadoValido();
            }

            var existe = ExisteBitacoraDesdeProceso(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                "05");

            return existe
                ? CrearResultadoInvalido(
                    "No se puede realizar el movimiento seleccionado ya que se ha aplicado esta planilla y/o otra futura en los auxiliares... verifique.!")
                : CrearResultadoValido();
        }
        private static CcProcesoMensualValidaPasoResponse ValidarCuadreAbonos(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            if (!EsTransaccion(request.Transaccion, "08"))
            {
                return CrearResultadoValido();
            }

            const string query = @"
                SELECT dbo.fxPrmAplicacionValida(
                    @FechaProceso,
                    @CodInstitucion) AS Valida";

            var valida = connection.QueryFirstOrDefault<int>(
                query,
                new
                {
                    request.FechaProceso,
                    request.CodInstitucion
                });

            return valida == 0
                ? CrearResultadoInvalido(
                    "La información detallada de los abonos no cuadra con la información cargada... verifique.!")
                : CrearResultadoValido();
        }
        private static CcProcesoMensualValidaPasoResponse ValidarDesgloseRealizado(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            if (!EsTransaccionAplicacion(request.Transaccion))
            {
                return CrearResultadoValido();
            }

            var existe = ExisteBitacoraEnProceso(
                connection,
                request.CodInstitucion,
                request.FechaProceso,
                "04");

            return !existe
                ? CrearResultadoInvalido(
                    "No se ha realizado el proceso de detalle de Aportes/Creditos... verifique.!")
                : CrearResultadoValido();
        }
        private static CcProcesoMensualValidaPasoResponse ValidarPlanillaMesAnterior(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            if (!EsTransaccion(request.Transaccion, "08"))
            {
                return CrearResultadoValido();
            }

            var existe = ExisteAplicacionMesAnterior(
                connection,
                request);

            return !existe
                ? CrearResultadoInvalido(
                    "La Planilla del Mes anterior no ha sido aplicada... verifique.!")
                : CrearResultadoValido();
        }
        private static bool ExisteAplicacionMesAnterior(IDbConnection connection, CcProcesoMensualValidaPasoRequest request)
        {
            var procesoBase = Math.Truncate(request.FechaProceso);
            var diferencia = Math.Round(
                request.FechaProceso - procesoBase,
                1,
                MidpointRounding.AwayFromZero);

            if (diferencia == 0.1m)
            {
                const string queryQuincena = @"
                        SELECT ISNULL(COUNT(*), 0) AS Existe
                        FROM prm_bitacora
                        WHERE cod_institucion = @CodInstitucion
                          AND transaccion = '08'
                          AND (
                                proceso = dbo.fxSIFPrmProcesoAnt(@FechaProceso)
                             OR proceso = dbo.fxSIFPrmProcesoAnt(@ProcesoBase)
                          )";

                return connection.QueryFirstOrDefault<int>(
                    queryQuincena,
                    new
                    {
                        request.CodInstitucion,
                        request.FechaProceso,
                        ProcesoBase = procesoBase
                    }) > 0;
            }

            const string query = @"
                        SELECT ISNULL(COUNT(*), 0) AS Existe
                        FROM prm_bitacora
                        WHERE cod_institucion = @CodInstitucion
                          AND transaccion = '08'
                          AND proceso = dbo.fxSIFPrmProcesoAnt(@FechaProceso)";

            return connection.QueryFirstOrDefault<int>(
                query,
                new
                {
                    request.CodInstitucion,
                    request.FechaProceso
                }) > 0;
        }
        private static bool ExisteBitacoraEnProceso(IDbConnection connection, int codInstitucion, decimal fechaProceso, string transaccion)
        {
            const string query = @"
                SELECT ISNULL(COUNT(*), 0) AS Existe
                FROM prm_bitacora
                WHERE cod_institucion = @CodInstitucion
                  AND proceso = @FechaProceso
                  AND transaccion = @Transaccion";

            return connection.QueryFirstOrDefault<int>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso,
                    Transaccion = transaccion
                }) > 0;
        }
        private static bool ExisteBitacoraDesdeProceso(IDbConnection connection, int codInstitucion, decimal fechaProceso, string transaccion)
        {
            const string query = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM prm_bitacora
                    WHERE cod_institucion = @CodInstitucion
                      AND transaccion = @Transaccion
                      AND proceso >= @FechaProceso";

            return connection.QueryFirstOrDefault<int>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    Transaccion = transaccion,
                    FechaProceso = fechaProceso
                }) > 0;
        }
        private static bool EsTransaccion(string transaccion, string esperada)
        {
            return string.Equals(transaccion?.Trim(), esperada, StringComparison.OrdinalIgnoreCase);
        }
        private static bool EsTransaccionAplicacion(string transaccion)
        {
            return EsTransaccion(transaccion, "08")
                || EsTransaccion(transaccion, "05");
        }
        private static CcProcesoMensualValidaPasoResponse CrearResultadoValido()
        {
            return new CcProcesoMensualValidaPasoResponse
            {
                Valido = true,
                Mensaje = string.Empty
            };
        }
        private static CcProcesoMensualValidaPasoResponse CrearResultadoInvalido(string mensaje)
        {
            return new CcProcesoMensualValidaPasoResponse
            {
                Valido = false,
                Mensaje = mensaje
            };
        }
        public ErrorDto<CcProcesoMensualCargaConfigDbModel> DatosInstitucion_Obtener(int codEmpresa, int codInstitucion)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            const string query = @"
               SELECT
                    ISNULL(planilla, '') AS Planilla,
                    ISNULL(codigo_aportes, '') AS CodigoAportes,
                    ISNULL(codigo_creditos, '') AS CodigoCreditos
                FROM instituciones                 
                    WHERE cod_institucion = @CodInstitucion";
 
            var resultado = conn.QueryFirstOrDefault<CcProcesoMensualCargaConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion }) ?? new CcProcesoMensualCargaConfigDbModel();

            return DbHelper.CreateOkResponse(resultado);
        }
    }
}
 