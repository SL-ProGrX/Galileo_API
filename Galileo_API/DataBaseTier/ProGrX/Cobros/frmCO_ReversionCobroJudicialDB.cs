using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using static Galileo_API.Models.ProGrX.Cobros.FrmCOReversionCobroJudicialModels;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOReversionCobroJudicialDB
    {

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain mProGrxDll;
        private readonly MCobroDb _mCobroDb;
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;
        private readonly int vModulo = 4;


        public FrmCOReversionCobroJudicialDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
            _mCobroDb = new MCobroDb(config);
            _config = config;
            DBBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la información de la pantalla frmCO_ReversionCobroJudicial.
        /// Equivale a sbConsulta en VB6.
        /// </summary>
        public ErrorDto<CrdReversionCobroJudicialConsultaResponse> Crd_ReversionCobroJudicial_Consultar(int CodEmpresa, string usuario, int codContabilidad, int operacion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                var globalesDto = mProGrxDll.sbSifParametrosInicializa(CodEmpresa, usuario, codContabilidad);
                var sysPlanPagos = globalesDto?.Result?.SysPlanPagos ?? 0;
                var plazoRestante = _mCobroDb.fxCBRPlazoRestante(CodEmpresa, operacion);

                var consulta = ObtenerDatosOperacion(connection, operacion, sysPlanPagos);

                if (consulta is null)
                {
                    return DbHelper.CreateErrorResponse<CrdReversionCobroJudicialConsultaResponse>(
                        "No se encontró información para la operación indicada.",
                        -1
                    );
                }

                var interesesHoy = ObtenerInteresesHoy(connection, operacion);
                var honorarios = ObtenerHonorarios(connection);

                var response = new CrdReversionCobroJudicialConsultaResponse
                {
                    Operacion = operacion,
                    PlazoRestante = plazoRestante,
                    Cedula = consulta.Cedula,
                    Nombre = consulta.Nombre,
                    Divisa = consulta.Cod_Divisa ?? string.Empty,
                    Tasa = consulta.Tasa,
                    TasaOriginal = consulta.TasaOriginal,
                    Saldo = consulta.Saldo,
                    InteresesCorte = consulta.Intereses,
                    Cargos = consulta.Cargos,
                    Poliza = consulta.Poliza,
                    Codigo = consulta.Codigo,
                    Descripcion = consulta.Descripcion,
                    PlazoOriginal = consulta.Plazo,
                    Opex = consulta.Opex == 1,
                    OpexDescripcion = consulta.Opex == 1 ? "Sí" : "No",
                    ProcesoCodigo = consulta.Proceso,
                    ProcesoDescripcion = ObtenerDescripcionProceso(consulta.Proceso),
                    PermiteReversar = consulta.Proceso == "J" || consulta.Proceso == "C",
                    Honorarios = honorarios,
                    Intereses = (interesesHoy?.RegIntCor ?? 0M) + (interesesHoy?.RegIntMor ?? 0M),
                    Amortizacion = interesesHoy?.RegPrincipal ?? 0M
                };

                response.TotalAtrasado = response.Amortizacion
                                        + response.Intereses
                                        + response.Cargos
                                        + response.Poliza;

                response.Total = response.Saldo
                               + response.Intereses
                               + response.Cargos
                               + response.Poliza
                               + response.Honorarios;

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdReversionCobroJudicialConsultaResponse>(
                    "Error al consultar la reversión de cobro judicial.",
                    -1
                );
            }
        }
       
        /// <summary>
        /// Ejecuta la reversión de cobro judicial.
        /// Equivale a sbReversaCobroJudicial en VB6.
        /// </summary>
        public ErrorDto<object> Crd_ReversionCobroJudicial_Reversar(
            int CodEmpresa,
            CrdReversionCobroJudicialReversaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                var validacion = ValidarReversaCobroJudicial(connection, request);

                if (validacion.Code != 0)
                {
                    return new ErrorDto<object>
                    {
                        Code = -2,
                        Description = validacion.Description ?? "No fue posible validar la reversión.",
                        Result = null
                    };

                }

                const string query = @"
                EXEC spCBR_Cobro_Judicial_Reversa
                    @Operacion,
                    @Notas,
                    @Usuario;";

                var parametros = new DynamicParameters();
                parametros.Add("@Operacion", request.Operacion, DbType.Int64);
                parametros.Add("@Notas", request.Notas.Trim(), DbType.String);
                parametros.Add("@Usuario", request.Usuario.Trim(), DbType.String);

                var result = connection.QueryFirstOrDefault<CrdReversionCobroJudicialReversaDbModel>(query, parametros);

                if (result is null)
                {
                    return new ErrorDto<object>
                    {
                        Code = -2,
                        Description = "No se obtuvo respuesta del proceso de reversa.",
                        Result = null
                    };

                }

                if (result.Pass != 1)
                {
                    return new ErrorDto<object>
                    {
                        Code = -2,
                        Description = result.Mensaje ?? "No fue posible reversar la operación.",
                        Result = null
                    };


                }

                string tipoDoc = result.TipoDoc ?? string.Empty;
                string numDoc = result.NumDoc ?? string.Empty;

                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = request.Usuario,
                    DetalleMovimiento = $"Cobro Judicial a la Operación: {request.Operacion}",
                    Movimiento = "Reversa - WEB",
                    Modulo = vModulo
                });



                var resultado = ImprimeRecibo(CodEmpresa, request.Usuario, tipoDoc.Trim(), numDoc.Trim());
                return new ErrorDto<object>
                {
                    Code = resultado.Code,
                    Description = resultado.Description,
                    Result = resultado.Result
                };

            }
            catch (Exception)
            {
                return new ErrorDto<object>
                {
                    Code = -21,
                    Description = "Error al reversar el cobro judicial.",
                    Result = null
                };
            }
        }
        /// <summary>
        /// Crea un objeto de error para validaciones, con código -1 por defecto.
        /// </summary>
        /// <param name="mensaje"></param>
        /// <returns></returns>
        private static ErrorDto CrearValidacionError(string mensaje)
        {
            return new ErrorDto
            {
                Code = -1,
                Description = mensaje
            };
        }

        /// <summary>
        /// Obtiene los datos de la operación, incluyendo cálculos de intereses, cargos y otros campos relevantes para la reversión de cobro judicial.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="operacion"></param>
        /// <param name="sysPlanPagos"></param>
        /// <returns></returns>
        private static CrdReversionCobroJudicialConsultaDbModel? ObtenerDatosOperacion(
       IDbConnection connection,
       int operacion,
       int sysPlanPagos)
        {
            const string tablaPlanPagos = "crd_operacion_Transac";
            const string tablaMorosidad = "morosidad";

            const string interesesPlanPagos = "ISNULL(SUM(V.intCor + V.intMor), 0)";
            const string interesesMorosidad = "ISNULL(SUM(V.intc + V.intm), 0)";

            const string moraIntCPlanPagos = "ISNULL(SUM(V.intCor), 0)";
            const string moraIntCMorosidad = "ISNULL(SUM(V.intc), 0)";

            const string moraIntMPlanPagos = "ISNULL(SUM(V.intMor), 0)";
            const string moraIntMMorosidad = "ISNULL(SUM(V.intm), 0)";

            const string moraAmortizaPlanPagos = "ISNULL(SUM(V.Principal), 0)";
            const string moraAmortizaMorosidad = "ISNULL(SUM(V.amortiza), 0)";

            const string cargosPlanPagos = "ISNULL(SUM(V.Cargos), 0)";
            const string cargosMorosidad = "ISNULL(SUM(V.Cargo), 0)";

            const string polizaPlanPagos = "ISNULL(SUM(V.Poliza), 0)";
            const string polizaMorosidad = "CAST(0 AS DECIMAL(18, 2))";

            var usaPlanPagos = sysPlanPagos == 1;

            var tablaDetalle = usaPlanPagos ? tablaPlanPagos : tablaMorosidad;
            var interesesTotal = usaPlanPagos ? interesesPlanPagos : interesesMorosidad;
            var moraIntC = usaPlanPagos ? moraIntCPlanPagos : moraIntCMorosidad;
            var moraIntM = usaPlanPagos ? moraIntMPlanPagos : moraIntMMorosidad;
            var moraAmortiza = usaPlanPagos ? moraAmortizaPlanPagos : moraAmortizaMorosidad;
            var cargos = usaPlanPagos ? cargosPlanPagos : cargosMorosidad;
            var poliza = usaPlanPagos ? polizaPlanPagos : polizaMorosidad;

            var query = $@"
        SELECT
            R.cedula,
            S.nombre,
            R.saldo,
            R.proceso,
            R.Interesv AS Tasa,
            R.plazo,
            R.Int AS TasaOriginal,
            {interesesTotal} AS Intereses,
            R.codigo,
            C.descripcion,
            R.Opex,
            {moraIntC} AS MoraIntC,
            {moraIntM} AS MoraIntM,
            {moraAmortiza} AS MoraAmortiza,
            {cargos} AS Cargos,
            {poliza} AS Poliza,
            R.COD_DIVISA AS Cod_Divisa
        FROM Socios S
        INNER JOIN reg_creditos R ON S.cedula = R.cedula
        INNER JOIN Catalogo C ON R.codigo = C.codigo
        LEFT JOIN " + tablaDetalle + @" V
            ON R.id_solicitud = V.id_solicitud
           AND V.estado = 'A'
        WHERE R.id_solicitud = @Operacion
        GROUP BY
            R.cedula,
            S.nombre,
            R.saldo,
            R.proceso,
            R.Interesv,
            R.plazo,
            R.Int,
            R.codigo,
            C.descripcion,
            R.Opex,
            R.COD_DIVISA;";

            return connection.QueryFirstOrDefault<CrdReversionCobroJudicialConsultaDbModel>(
                query,
                new { Operacion = operacion });
        }

        /// <summary>
        /// Obtiene los intereses generados hasta la fecha de corte para la operación, esto se hace ejecutando el procedimiento almacenado spCbrCobroJudicialInteresesHoy.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        private static CrdReversionCobroJudicialInteresesHoyDbModel? ObtenerInteresesHoy(IDbConnection connection, int operacion)
        {
            const string query = @"EXEC spCbrCobroJudicialInteresesHoy @Operacion;";

            return connection.QueryFirstOrDefault<CrdReversionCobroJudicialInteresesHoyDbModel>(query, new
            {
                Operacion = operacion
            });
        }
        
        /// <summary>
        /// Obtiene el total de honorarios asociados a la operación, esto se hace consultando la tabla CBR_CJ_TRAMITE_GASTOS filtrando por el número de trámite asociado a la operación. Si no se encuentra información, se retorna 0.00.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tramite"></param>
        /// <returns></returns>
        private static decimal ObtenerHonorarios(IDbConnection connection, int? tramite = 0)
        {
            if (!tramite.HasValue || tramite.Value <= 0)
            {
                return 0M;
            }

            const string query = @"
            SELECT ISNULL(SUM(monto), 0)
            FROM CBR_CJ_TRAMITE_GASTOS
            WHERE TESORERIA_NUMERO IS NOT NULL
              AND cod_tramite = @Tramite;";

            return connection.ExecuteScalar<decimal>(query, new
            {
                Tramite = tramite.Value
            });
        }
        
        /// <summary>
        /// Valida que la reversión de cobro judicial pueda realizarse, esto incluye verificar que la operación se encuentre en proceso de cobro judicial y que se hayan proporcionado notas válidas para la reversión.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ErrorDto ValidarReversaCobroJudicial(
        IDbConnection connection,
        CrdReversionCobroJudicialReversaRequest request)
        {
            if (request.Operacion <= 0)
            {
                return CrearValidacionError("La operación es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.Notas))
            {
                return CrearValidacionError("Especifique una nota para la reversión.");
            }

            if (request.Notas.Trim().Length < 30)
            {
                return CrearValidacionError("Indique una nota válida de al menos 30 caracteres.");
            }

            const string query = @"
            SELECT ISNULL(COUNT(*), 0)
            FROM reg_creditos
            WHERE proceso IN ('J', 'C')
              AND id_solicitud = @Operacion;";

            var existe = connection.ExecuteScalar<int>(query, new
            {
                request.Operacion
            });

            if (existe <= 0)
            {
                return CrearValidacionError("La operación no se encuentra en proceso de cobro judicial para realizar la reversión.");

            }

            return new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
        }

        /// <summary>
        /// Obtiene la descripción del proceso de cobro judicial a partir del código del proceso, esto se hace mediante un mapeo simple entre el código y su descripción correspondiente.
        /// </summary>
        /// <param name="proceso"></param>
        /// <returns></returns>
        private static string ObtenerDescripcionProceso(string? proceso)
        {
            return proceso switch
            {
                "N" => "Normal",
                "T" => "Traslado",
                "J" => "Cobro Judicial",
                "C" => "Cobro Judicial",
                _ => "Incobrable"
            };
        }

        /// <summary>
        /// Genera el reporte de recibo para un documento dado, esto se hace ejecutando la función de impresión de recibos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="documento"></param>
        /// <returns></returns>
        public ErrorDto<object> ImprimeRecibo(int CodEmpresa, string usuario, string tipoDocumento = "", string documento = "")
        {
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            try
            {

                response = new MRecibos(_config).sbImprimeRecibo(CodEmpresa, documento, tipoDocumento, usuario);
                if (response.Code != -1)
                {
                    response.Description = $"La operación fue reversada a estado NORMAL - Se generó Nota de Cobro: {tipoDocumento}-{documento}";
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
