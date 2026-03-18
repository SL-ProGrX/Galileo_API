using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient; 
using static Galileo_API.Models.ProGrX.Cobros.FrmCOIncobrablesModels;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOIncobrablesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain mProGrxDll;
        private readonly IConfiguration _config;


        private const string EstadoIncobrableInactivo = "I";
        private const string EstadoIncobrableActivo = "A";

        public FrmCOIncobrablesDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
        }

        /// <summary>
        /// Consulta detalle de la deuda de una operacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idSolicitud"></param>
        /// <returns></returns>
        public ErrorDto<CrdIncobrableDetalleResponse> Crd_Incobrables_Operacion_Consultar(int codEmpresa, int idSolicitud)
        {
            try
            {
                const string sqlOperacion = @"
                    SELECT
                        R.id_solicitud AS IdSolicitud,
                        RTRIM(R.Codigo) AS Codigo,
                        RTRIM(R.cedula) AS Cedula,
                        dbo.MyGetdate() AS FechaServer,
                        ISNULL(R.saldo, 0) AS Saldo,
                        ISNULL(R.Estado, 'N') AS Estado,
                        ISNULL(R.Proceso, 'N') AS Proceso,
                        CASE WHEN ISNULL(R.Opex, 0) = 1 THEN 'SI' ELSE 'NO' END AS Opex,
                        RTRIM(C.Descripcion) AS Descripcion,
                        RTRIM(S.nombre) AS Nombre,
                        RTRIM(ISNULL(G.Descripcion, '')) AS Garantia,
                        RTRIM(ISNULL(R.cod_Divisa, '')) AS Divisa
                    FROM reg_creditos R
                    INNER JOIN Catalogo C ON R.codigo = C.codigo
                    INNER JOIN Socios S ON R.cedula = S.cedula
                    LEFT JOIN Crd_Garantia_Tipos G ON R.Garantia = G.Garantia
                    WHERE R.id_solicitud = @IdSolicitud;";

                var operacionResult = DbHelper.ExecuteSingleQuery<CrdIncobrableDetalleResponse>(
                    _portalDb,
                    codEmpresa,
                    sqlOperacion,
                    defaultValue: null,
                    parameters: new { IdSolicitud = idSolicitud });

                if (operacionResult.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                        operacionResult.Description ?? "Error al consultar la operación.");
                }

                if (operacionResult.Result is null)
                {
                    return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                        "No se encontró la operación solicitada.");
                }

                const string sqlMora = @"
                    EXEC spCbrCobroJudicialInteresesHoy @IdSolicitud, @FechaServer;";

                var moraResult = DbHelper.ExecuteSingleQuery<CrdIncobrablesMoraDbModel>(
                    _portalDb,
                    codEmpresa,
                    sqlMora,
                    defaultValue: null,
                    parameters: new
                    {
                        IdSolicitud = idSolicitud,
                        FechaServer = operacionResult.Result.FechaServer.ToString("yyyy/MM/dd")
                    });

                if (moraResult.Code != 0 || moraResult.Result is null)
                {
                    return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                        moraResult.Description ?? "Error al consultar mora de la operación.");
                }

                var response = operacionResult.Result;
                var mora = moraResult.Result;

                response.Estado = ObtenerDescripcionEstado(response.Estado);
                response.Proceso = ObtenerDescripcionProceso(response.Proceso);
                response.IntCor = mora.RegIntCor;
                response.IntMor = mora.RegIntMor;
                response.Amortizacion = mora.RegPrincipal;
                response.Cargos = mora.Cargos;
                response.Poliza = mora.Poliza;
                response.TotalMora = mora.RegIntCor + mora.RegIntMor + mora.Cargos + mora.Poliza + mora.RegPrincipal;
                response.TotalMoraLegal = mora.RegIntCor + mora.RegIntMor + mora.Cargos + mora.Poliza + response.Saldo;
                response.TotalAtrasado = response.TotalMoraLegal;
                response.EstadoMoroso = mora.Antiguedad;
                response.ExisteIncobrable = false;
                response.IncobrableActivo = false;
                response.MostrarTabRegistro = true;
                response.MostrarTabReversion = false;

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                    $"Error al consultar información del incobrable: {ex.Message}");
            }
        }

        /// <summary>
        /// Consulta los incrables que existen para una operación dada, para cargar el dropdown en la pantalla de detalle
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idSolicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Incobrables_Codigos_Obtener(int codEmpresa, int idSolicitud)
        {
            const string sql = @"
                SELECT CAST(0 AS INT) AS item, '0' AS descripcion
                UNION
                SELECT
                    cod_incobrable AS item,
                    CAST(cod_incobrable AS VARCHAR(20)) AS descripcion
                FROM cbr_incobrables
                WHERE id_solicitud = @IdSolicitud
                ORDER BY item;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { IdSolicitud = idSolicitud });
        }

        /// <summary>
        /// Consulta datos de un incobrable específico para mostrar en el detalle, si no existe devuelve la información de la operación para ese incobrable nuevo a crear
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="idSolicitud"></param>
        /// <param name="codIncobrable"></param>
        /// <returns></returns>
        public ErrorDto<CrdIncobrableDetalleResponse> Crd_Incobrables_Detalle_Obtener(
            int codEmpresa,
            string usuario,
            int codContabilidad,
            int idSolicitud,
            int codIncobrable)
        {
            try
            {
                const string sqlExiste = @"
                    SELECT TOP 1
                        I.id_solicitud AS IdSolicitud,
                        I.cod_incobrable AS CodIncobrable,
                        ISNULL(I.estado, '') AS Estado,
                        ISNULL(I.Principal, 0) AS Amortizacion,
                        ISNULL(I.IntCor, 0) AS IntCor,
                        ISNULL(I.IntMor, 0) AS IntMor,
                        ISNULL(I.Cargos, 0) AS Cargos,
                        ISNULL(I.Poliza, 0) AS Poliza,
                        ISNULL(I.saldo, 0) AS Saldo,
                        ISNULL(I.NOTAS_REGISTRO, '') AS NotasRegistro,
                        ISNULL(I.Registro_Usuario, '') AS RegistroUsuario,
                        ISNULL(CONVERT(VARCHAR(19), I.Registro_fecha, 120), '') AS RegistroFecha,
                        ISNULL(I.NOTAS_REVERSION, '') AS NotasReversion,
                        ISNULL(I.Modifica_Usuario, '') AS ReversionUsuario,
                        ISNULL(CONVERT(VARCHAR(19), I.Modifica_fecha, 120), '') AS ReversionFecha,
                        ISNULL(I.REVERSA_DOCUMENTO, '') AS ReversionDocumento,
                        ISNULL(I.REACTIVACION_RECARGO, 0) AS ReversionRecargo,
                        RTRIM(ISNULL(R.garantia, '')) AS Garantia

                    FROM cbr_incobrables I
                    INNER JOIN reg_creditos R ON I.id_solicitud = R.id_solicitud
                    WHERE I.id_solicitud = @IdSolicitud
                      AND I.cod_incobrable = @CodIncobrable;";

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var existe = connection.QueryFirstOrDefault<CrdIncobrableDetalleResponse>(
                    sqlExiste,
                    new
                    {
                        IdSolicitud = idSolicitud,
                        CodIncobrable = codIncobrable
                    });

                if (existe is null)
                {
                    return ConstruirDetalleNuevoIncobrable(
                        codEmpresa,
                        usuario,
                        codContabilidad,
                        idSolicitud,
                        codIncobrable);
                }


                const string sqlDocumentoRegistro = @"
                    SELECT TOP 1
                        CASE
                            WHEN ISNULL(I.Tipo_Documento, '') = ''
                                THEN CONCAT('NC.', ISNULL(I.Genera_Documento, ''))
                            ELSE CONCAT(I.Tipo_Documento, '.', ISNULL(I.Cod_Transaccion, ''))
                        END AS RegistroDocumento
                    FROM cbr_incobrables I
                    WHERE I.id_solicitud = @IdSolicitud
                      AND I.cod_incobrable = @CodIncobrable;";



                var registroDocumento = connection.QueryFirstOrDefault<string>(
                    sqlDocumentoRegistro,
                    new
                    {
                        IdSolicitud = idSolicitud,
                        CodIncobrable = codIncobrable
                    }) ?? string.Empty;

                var response = new CrdIncobrableDetalleResponse
                {
                    ExisteIncobrable = true,
                    Amortizacion = existe.Amortizacion,
                    Cargos = existe.Cargos,
                    Poliza = existe.Poliza,
                    Saldo = existe.Saldo,
                    IntCor = existe.IntCor,
                    IntMor = existe.IntMor,
                    NotasRegistro = existe.NotasRegistro,
                    RegistroDocumento = registroDocumento,
                    RegistroUsuario = existe.RegistroUsuario,
                    RegistroFecha = existe.RegistroFecha,
                    NotasReversion = existe.NotasReversion,
                    ReversionUsuario = existe.ReversionUsuario,
                    ReversionDocumento = existe.ReversionDocumento,
                    ReversionFecha = existe.ReversionFecha,
                    ReversionRecargo = existe.ReversionRecargo,
                    TotalAtrasado = existe.Saldo + existe.Cargos + existe.Poliza + existe.IntCor + existe.IntMor,
                    Estado = existe.Estado

                };

                AplicarVisibilidadSegunEstado(response);

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                    $"Error al obtener detalle del incobrable: {ex.Message}");
            }
        }

        /// <summary>
        /// Construye el detalle de un incobrable nuevo a crear, para lo cual consulta la información de la operación y calcula los intereses corrientes si el sistema no tiene plan de pagos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="idSolicitud"></param>
        /// <param name="codIncobrable"></param>
        /// <returns></returns>
        private ErrorDto<CrdIncobrableDetalleResponse> ConstruirDetalleNuevoIncobrable(
            int codEmpresa,
            string usuario,
            int codContabilidad,
            int idSolicitud,
            int codIncobrable)
        {
            try
            {
                var globalesDto = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad);
                var sysPlanPagos = globalesDto?.Result?.SysPlanPagos ?? 0;

                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                const string selectBase = @"
            SELECT
                @IdSolicitud AS IdSolicitud,
                @CodIncobrable AS CodIncobrable,
                RTRIM(R.cedula) AS Cedula,
                RTRIM(S.nombre) AS Nombre,
                ISNULL(R.saldo, 0) AS Saldo,
                ISNULL(R.proceso, '') AS Proceso,
                ISNULL(R.prideduc, 0) AS PriDeduc,
                R.fecult AS FecUlt,
                dbo.MyGetdate() AS FechaActual,
                ISNULL(R.interesv, 0) AS InteresV,
                RTRIM(ISNULL(R.cod_Divisa, '')) AS Divisa,
                RTRIM(R.codigo) AS Codigo,
                RTRIM(C.descripcion) AS Descripcion,
                CASE WHEN ISNULL(R.Opex, 0) = 1 THEN 'SI' ELSE 'NO' END AS Opex,
                RTRIM(ISNULL(R.garantia, '')) AS Garantia,";

                const string fromBase = @"
            FROM Socios S
            INNER JOIN reg_creditos R ON S.cedula = R.cedula
            INNER JOIN Catalogo C ON R.codigo = C.codigo";

                const string groupByBase = @"
            GROUP BY
                R.cedula, S.nombre, R.saldo, R.proceso, R.prideduc, R.fecult,
                R.interesv, R.cod_Divisa, R.codigo, C.descripcion, R.Opex, R.garantia;";

                string sql = sysPlanPagos == 1
                    ? $@"
                        {selectBase}
                        ISNULL(SUM(V.Principal), 0) AS Amortizacion,
                        dbo.MyGetdate() AS FechaServer
                        {fromBase}
                        LEFT JOIN crd_operacion_Transac V
                            ON R.id_solicitud = V.id_solicitud
                           AND V.estado = 'A'
                           AND V.mora_dias > 0
                        WHERE R.id_solicitud = @IdSolicitud
                        {groupByBase}"
                            : $@"
                        {selectBase}
                        ISNULL(SUM(V.intc), 0) AS IntCor,
                        ISNULL(SUM(V.intm), 0) AS IntMor,
                        ISNULL(SUM(V.amortiza), 0) AS Amortizacion,
                        ISNULL(SUM(V.Cargo), 0) AS Cargos,
                        CAST(0 AS DECIMAL(18,2)) AS Poliza
                        {fromBase}
                        LEFT JOIN morosidad V
                            ON R.id_solicitud = V.id_solicitud
                           AND V.estado = 'A'
                        WHERE R.id_solicitud = @IdSolicitud
                        {groupByBase}";

                var data = connection.QueryFirstOrDefault<CrdIncobrableDetalleResponse>(
                    sql,
                    new
                    {
                        IdSolicitud = idSolicitud,
                        CodIncobrable = codIncobrable
                    });

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                        "No se encontró información para generar el incobrable.");
                }

                decimal intCor = 0M;
                decimal intMor = 0M;
                decimal cargos = 0M;
                decimal poliza = 0M;
                decimal amortizacion = data.Amortizacion;
                decimal saldo = data.Saldo;

                if (sysPlanPagos == 1)
                {
                    const string sqlActualiza = @"
                        EXEC spCrdPlanPagosMoraActualizaOp @IdSolicitud, @FechaServer;";

                    connection.Execute(sqlActualiza, new
                    {
                        IdSolicitud = idSolicitud,
                        FechaServer = data.FechaServer.ToString("yyyy/MM/dd")
                    });

                    const string sqlCancelacion = @"
                        EXEC spCrdPlanPagosInfoCancelacion @IdSolicitud, @FechaServer;";

                    var infoCancelacion = connection.QueryFirstOrDefault<CrdIncobrablesMoraDbModel>(
                        sqlCancelacion,
                        new
                        {
                            IdSolicitud = idSolicitud,
                            FechaServer = data.FechaServer.ToString("yyyy/MM/dd")
                        });

                    intCor = infoCancelacion?.RegIntCor ?? 0M;
                    intMor = infoCancelacion?.RegIntMor ?? 0M;
                    cargos = infoCancelacion?.Cargos ?? 0M;
                    poliza = infoCancelacion?.Poliza ?? 0M;
                }
                else
                {
                    intMor = data.IntMor;
                    cargos = data.Cargos;
                    poliza = data.Poliza;

                    intCor = CalcularInteresCorrienteSinPlanPagos(
                             connection,
                             new CalculoInteresSinPlanPagosModel
                             {
                                 IdSolicitud = idSolicitud,
                                 Saldo = data.Saldo,
                                 InteresV = data.InteresV,
                                 PriDeduc = data.PriDeduc,
                                 FecUlt = data.FecUlt,
                                 FechaActual = data.FechaActual,
                                 MoraAmortiza = data.Amortizacion,
                                 MoraIntC = data.IntCor,
                                 MoraIntM = data.IntMor
                             });
                }

                var response = new CrdIncobrableDetalleResponse
                {
                    ExisteIncobrable = true,
                    MostrarTabRegistro = true,
                    MostrarTabReversion = false,
                    IncobrableActivo = false,

                    Saldo = saldo,
                    IntCor = intCor,
                    IntMor = intMor,
                    Amortizacion = amortizacion,
                    Cargos = cargos,
                    Poliza = poliza,
                    Proceso = data.Proceso,
                    NotasRegistro = string.Empty,
                    RegistroUsuario = string.Empty,
                    RegistroDocumento = string.Empty,
                    RegistroFecha = string.Empty,
                    NotasReversion = string.Empty,
                    ReversionUsuario = string.Empty,
                    ReversionDocumento = string.Empty,
                    ReversionFecha = string.Empty,
                    ReversionRecargo = 0M,
                    TotalMora = intCor + intMor + cargos + poliza + amortizacion,
                    TotalMoraLegal = intCor + intMor + cargos + poliza + saldo,
                    TotalAtrasado = saldo + cargos + poliza + intCor + intMor,

                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrdIncobrableDetalleResponse>(
                    $"Error al construir detalle de nuevo incobrable: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el proceso siguiente dado un proceso actual, esto se hace para calcular los intereses corrientes
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="procesoActual"></param>
        /// <returns></returns>
        private static int ObtenerProcesoSiguiente(SqlConnection connection, int procesoActual)
        {
            const string sql = @"
        SELECT dbo.fxSIFPrmProcesoSig(@proceso) AS Result;";

            var resultado = connection.QueryFirstOrDefault<decimal?>(
                sql,
                new { proceso = procesoActual });

            return Convert.ToInt32(resultado ?? 0M);
        }

        /// <summary>
        /// Convierte una fecha al formato de proceso utilizado en el sistema, que es AAAAMM, esto se hace para calcular los intereses corrientes
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        private static int ConvertirFechaAProceso(DateTime fecha)
        {
            return (fecha.Year * 100) + fecha.Month;
        }

        /// <summary>
        /// Calcula los intereses corrientes para el caso en que el sistema no tiene plan de pagos, esto se hace consultando la última cuota pagada o en mora y calculando los intereses según la fecha actual y el proceso de la última cuota
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private static decimal CalcularInteresCorrienteSinPlanPagos(
        SqlConnection connection,
        CalculoInteresSinPlanPagosModel model)
        {
            var vUltimaCuota = model.FecUlt;
            var vProceso = ConvertirFechaAProceso(model.FechaActual);
            var curInteres = 0M;

            if (TieneMorosidad(model))
            {
                vUltimaCuota = ObtenerUltimoProcesoMorosidad(connection, model.IdSolicitud, vUltimaCuota);
            }


            var noGeneraInteres =
                (vProceso < model.PriDeduc && vUltimaCuota < model.PriDeduc) ||
                (vProceso == model.PriDeduc && vUltimaCuota == model.PriDeduc) ||
                (vProceso > model.PriDeduc && vUltimaCuota > vProceso);

            if (noGeneraInteres)
            {
                curInteres = 0M;
            }
            else if (vProceso == model.PriDeduc && vUltimaCuota < vProceso)
            {
                curInteres = (model.Saldo * model.InteresV / 36000M) * model.FechaActual.Day;
            }
            else if (vProceso > model.PriDeduc && (vUltimaCuota == model.PriDeduc || vProceso > vUltimaCuota))
            {
                curInteres = CalcularInteresPorMesesAtrasados(
                    connection,
                    vProceso,
                    vUltimaCuota,
                    model.Saldo,
                    model.InteresV,
                    model.FechaActual.Day);
            }

            return model.MoraIntC + curInteres;
        }

        /// <summary>
        /// Determina si la operación tiene morosidad, esto se hace para saber si se debe consultar el proceso de la última cuota en mora o pagada para calcular los intereses corrientes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static bool TieneMorosidad(CalculoInteresSinPlanPagosModel model)
        {
            return model.MoraAmortiza + model.MoraIntC + model.MoraIntM > 0;
        }

        /// <summary>
        /// Obtiene el proceso de la última cuota en mora o pagada, esto se hace para calcular los intereses corrientes en el caso de que el sistema no tenga plan de pagos, se consulta la tabla de morosidad por el proceso más reciente registrado para la operación y se compara con la última cuota pagada para obtener el proceso a tomar en cuenta para el cálculo de intereses corrientes
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="idSolicitud"></param>
        /// <param name="vUltimaCuota"></param>
        /// <returns></returns>
        private static int ObtenerUltimoProcesoMorosidad(
            SqlConnection connection,
            int idSolicitud,
            int vUltimaCuota)
        {
            const string sqlMaxProceso = @"
        SELECT MAX(fechap)
        FROM morosidad
        WHERE estado = 'A'
          AND id_solicitud = @IdSolicitud;";

            var procesoMorosidad = connection.QueryFirstOrDefault<decimal?>(
                sqlMaxProceso,
                new { IdSolicitud = idSolicitud });

            if (!procesoMorosidad.HasValue)
            {
                return vUltimaCuota;
            }

            var procesoMaximo = Convert.ToInt32(procesoMorosidad.Value);
            return procesoMaximo > vUltimaCuota ? procesoMaximo : vUltimaCuota;
        }

        /// <summary>
        /// Calcula los intereses corrientes por meses atrasados, esto se hace para el caso en que el sistema no tiene plan de pagos y el proceso actual es mayor a la última cuota pagada o en mora, se calcula el número de meses atrasados y se multiplican por 30 días para calcular los intereses corrientes a generar por meses atrasados, sumando además los días del mes actual para el cálculo total de intereses corrientes a generar
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="vProceso"></param>
        /// <param name="vUltimaCuota"></param>
        /// <param name="saldo"></param>
        /// <param name="interesV"></param>
        /// <param name="diaActual"></param>
        /// <returns></returns>
        private static decimal CalcularInteresPorMesesAtrasados(
            SqlConnection connection,
            int vProceso,
            int vUltimaCuota,
            decimal saldo,
            decimal interesV,
            int diaActual)
        {
            var iMeses = -1;

            while (vProceso > vUltimaCuota)
            {
                iMeses++;
                vUltimaCuota = ObtenerProcesoSiguiente(connection, vUltimaCuota);
            }

            return (saldo * interesV / 36000M) * (diaActual + (iMeses * 30));
        }

        /// <summary>
        /// Aplica un incobrable para una operación, esto se hace ejecutando el procedimiento almacenado de traslado a incobrable que registra el incobrable y genera el documento contable, si el proceso es exitoso se imprime el recibo del documento generado, devolviendo el resultado del proceso de impresión como resultado de la función
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<object> Crd_Incobrables_Aplicar(int codEmpresa, CrdIncobrableAplicarRequest request)
        {
            var validacion = ValidarAplicacion(request.Notas);
            if (validacion.Code != 0)
            {
                return new ErrorDto<object>
                {
                    Code = -2,
                    Description = validacion.Description ?? "Datos inválidos.",
                    Result = null
                };
            }

            const string sql = @"EXEC spCBR_Incobrables_Traslado @IdSolicitud, @Usuario, @Notas;";

            return EjecutarProcesoIncobrable(
                codEmpresa,
                sql,
                new
                {
                    request.IdSolicitud,
                    request.Usuario,
                    Notas = request.Notas.Trim()
                },
                request.Usuario,
                "Error al aplicar incobrable.");
        }

        /// <summary>
        /// Genera el reporte de recibo para un documento dado, esto se hace ejecutando la función de impresión de recibos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        public ErrorDto<object> ImprimeRecibo(int CodEmpresa, string usuario, string tipoDocumento = "", string transaccion = "")
        {
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            try
            {

                response = new MRecibos(_config).sbImprimeRecibo(CodEmpresa, transaccion, tipoDocumento, usuario);
                if (response.Code != -1)
                {
                    response.Description = "Proceso Aplicado Satisfactoriamente...";
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
        /// Revierte un incobrable para una operación, esto se hace ejecutando el procedimiento almacenado de reversión de incobrable que revierte el incobrable y genera el documento contable de reversión, si el proceso es exitoso se imprime el recibo del documento generado, devolviendo el resultado del proceso de impresión como resultado de la función
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<object> Crd_Incobrables_Reversar(int codEmpresa, CrdIncobrableReversaRequest request)
        {
            var validacion = ValidarReversion(request.Notas, request.Recargo);
            if (validacion.Code != 0)
            {
                return new ErrorDto<object>
                {
                    Code = -2,
                    Description = validacion.Description ?? "Datos inválidos.",
                    Result = null
                };
            }

            const string sql = @"EXEC spCBR_Incobrables_Reversa @IdSolicitud, @Recargo, @Usuario, @Notas;";

            return EjecutarProcesoIncobrable(
                codEmpresa,
                sql,
                new
                {
                    request.IdSolicitud,
                    request.Recargo,
                    request.Usuario,
                    Notas = request.Notas.Trim()
                },
                request.Usuario,
                "Error al reversar incobrable.");
        }

        /// <summary>
        /// Aplica la lógica de visibilidad para el detalle del incobrable según el estado del incobrable, esto se hace para mostrar u ocultar las pestañas de registro y reversión y el botón de aplicar incobrable según corresponda para cada estado
        /// </summary>
        /// <param name="response"></param>
        private static void AplicarVisibilidadSegunEstado(CrdIncobrableDetalleResponse response)
        {
            var estado = (response.Estado ?? string.Empty).Trim().ToUpperInvariant();

            if (estado == EstadoIncobrableInactivo)
            {
                response.IncobrableActivo = false;
                response.MostrarTabRegistro = true;
                response.MostrarTabReversion = false;
                return;
            }

            if (estado == EstadoIncobrableActivo)
            {
                response.IncobrableActivo = true;
                response.MostrarTabRegistro = false;
                response.MostrarTabReversion = true;
                return;
            }

            response.IncobrableActivo = false;
            response.MostrarTabRegistro = true;
            response.MostrarTabReversion = false;
        }

        /// <summary>
        /// Valida los datos para aplicar un incobrable, esto se hace para asegurar que se especifique una nota para el registro del incobrable, ya que es un dato requerido para el proceso de aplicación de incobrable
        /// </summary>
        /// <param name="notas"></param>
        /// <returns></returns>
        private static ErrorDto ValidarAplicacion(string? notas)
        {
            if (string.IsNullOrWhiteSpace(notas))
            {
                return DbHelper.ErrorResponse("Especifique una nota para el registro.");
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Valida los datos para reversar un incobrable, esto se hace para asegurar que se especifique una nota para el registro de la reversión y que el monto de recargo sea válido, ya que son datos requeridos para el proceso de reversión de incobrable
        /// </summary>
        /// <param name="notas"></param>
        /// <param name="recargo"></param>
        /// <returns></returns>
        private static ErrorDto ValidarReversion(string? notas, decimal recargo)
        {
            if (string.IsNullOrWhiteSpace(notas))
            {
                return DbHelper.ErrorResponse("Especifique una nota para la reversión.");
            }

            if (recargo < 0)
            {
                return DbHelper.ErrorResponse("El monto de recargo no es válido.");
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Consulta el nombre del proceso según el código de proceso registrado en la operación, esto se hace para mostrar una descripción amigable del proceso en la pantalla de detalle del incobrable, ya que el sistema registra el proceso como un código (T, J, N) y se requiere mostrar una descripción (Traspaso deudas, Cobro judicial, Normal)
        /// </summary>
        /// <param name="proceso"></param>
        /// <returns></returns>
        private static string ObtenerDescripcionProceso(string? proceso)
        {
            return (proceso ?? "N").Trim().ToUpperInvariant() switch
            {
                "T" => "TRASPASO DEUDAS",
                "J" => "COBRO JUDICIAL",
                _ => "NORMAL"
            };
        }

        /// <summary>
        /// Consulta el nombre del estado del incobrable según el código de estado registrado en la operación, esto se hace para mostrar una descripción amigable del estado en la pantalla de detalle del incobrable, ya que el sistema registra el estado como un código (A, C, M, N) y se requiere mostrar una descripción (Activo, Cancelado, Moroso, Normal)
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        private static string ObtenerDescripcionEstado(string? estado)
        {
            var valor = string.IsNullOrWhiteSpace(estado) ? "N" : estado.Trim().ToUpperInvariant();

            return valor switch
            {
                "A" => "ACTIVO",
                "C" => "CANCELADO",
                "M" => "MOROSO",
                "N" => "NORMAL",
                _ => valor
            };
        }


        private ErrorDto<object> EjecutarProcesoIncobrable(
        int codEmpresa,
        string sql,
        object parameters,
        string usuario,
        string mensajeError)
        {
            var result = DbHelper.ExecuteSingleQuery<CrdIncobrableDocumentoDbModel>(
                _portalDb,
                codEmpresa,
                sql,
                defaultValue: null,
                parameters: parameters);

            if (result.Code != 0 || result.Result is null)
            {
                return new ErrorDto<object>
                {
                    Code = -1,
                    Description = result.Description ?? mensajeError,
                    Result = null
                };
            }

            string tipoDoc = result.Result.TipoDoc ?? string.Empty;
            string numDoc = result.Result.NumDoc ?? string.Empty;

            var resultado = ImprimeRecibo(codEmpresa, usuario, tipoDoc.Trim(), numDoc.Trim());

            return new ErrorDto<object>
            {
                Code = resultado.Code,
                Description = resultado.Description,
                Result = resultado.Result
            };
        }
    }
}