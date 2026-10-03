using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHExcedentesPagoDB
    {
        private const string TipoDocumentoFondo = "FND";
        private const string ConceptoFondo = "FND001";
        private const string SalidaTransferenciaInterna = "TI";
        private const string PlanSinpe = "SINPE";
        private const string OficinaDefault = "AOC";
        private const int OperadoraDefault = 1;

        private readonly PortalDB _portalDB;

        public FrmAHExcedentesPagoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(
                config ?? throw new ArgumentNullException(nameof(config))
            );
        }

        /// <summary>
        /// Obtiene los períodos cerrados disponibles para el auxiliar de pagos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>AH_Excedentes_Pago_Periodos_Lista_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(
                _portalDB,
                CodEmpresa,
                conn =>
                {
                    const string sql = """
                        SELECT
                            IdX AS item,
                            ItmX AS descripcion
                        FROM vExc_Periodos
                        WHERE estado IN ('C')
                        ORDER BY IdX DESC;
                        """;

                    return conn
                        .Query<DropDownListaGenericaModel>(sql)
                        .ToList();
                });
        }

        /// <summary>
        /// Ejecuta uno de los tres procesos de separación de casos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Pago_Separar_Casos_Aplicar( int CodEmpresa, ExcSepararCasosRequestDto? request)
        {
            if (request is null)
            {
                return CrearErrorValidacion(
                    "Debe indicar la información del proceso.");
            }

            ErrorDto? validacion = ValidarPeriodoUsuario(
                request.PeriodoId,
                request.Usuario,
                out string usuario);

            if (validacion is not null)
            {
                return validacion;
            }

            validacion = ValidarPasoSeparacion(
                request.Paso,
                request.EnviarSinpe);

            if (validacion is not null)
            {
                return validacion;
            }

            var parametros = new
            {
                pPeriodoId = request.PeriodoId,
                pEnviarSinpe = request.EnviarSinpe,
                pPaso = request.Paso,
                pUsuario = usuario
            };

            return EjecutarProcedimiento(
                CodEmpresa,
                "spExc_ASECCSS_SepararCasos",
                parametros,
                ObtenerMensajeSeparacion(request.Paso));
        }

        /// <summary>
        /// Asigna las salidas correspondientes a casos especiales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Pago_Casos_Especiales_Aplicar(int CodEmpresa, ExcCasosEspecialesRequestDto? request)
        {
            if (request is null)
            {
                return CrearErrorValidacion(
                    "Debe indicar la información del proceso.");
            }

            ErrorDto? validacion = ValidarPeriodoUsuario(
                request.PeriodoId,
                request.Usuario,
                out string usuario);

            if (validacion is not null)
            {
                return validacion;
            }

            var parametros = new
            {
                pPeriodoId = request.PeriodoId,
                pUsuario = usuario
            };

            return EjecutarProcedimiento(
                CodEmpresa,
                "spExc_ASECCSS_CasosEspeciales",
                parametros,
                "Asignación de casos especiales realizada satisfactoriamente.");
        }

        /// <summary>
        /// Procesa por lotes las transferencias internas de excedentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<ExcPendientesDto> AH_Excedentes_Pago_Cuentas_Internas_Aplicar(int CodEmpresa,ExcAcreditarCuentasInternasRequestDto? request)
        {
            ErrorDto? validacion = CrearContextoCuentasInternas(
                request,
                out CuentasInternasContexto contexto);

            if (validacion is not null)
            {
                return CrearErrorPendientes(validacion);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(
                    _portalDB,
                    CodEmpresa);

                PrepararCuentasInternas(
                    conn,
                    contexto);

                ExcPendientesDto pendientes =
                    ProcesarLotesCuentasInternas(
                        conn,
                        contexto);

                CrearDocumentoCuentasInternas(
                    conn,
                    contexto);

                return new ErrorDto<ExcPendientesDto>
                {
                    Code = 0,
                    Description = "Proceso realizado correctamente.",
                    Result = pendientes
                };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<ExcPendientesDto>
                {
                    Code = -2,
                    Description = ex.Message,
                    Result = new ExcPendientesDto()
                };
            }
            catch (SqlException ex)
            {
                return new ErrorDto<ExcPendientesDto>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new ExcPendientesDto()
                };
            }
        }

        /// <summary>
        /// Traslada los excedentes correspondientes a Tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Pago_Tesoreria_Aplicar( int CodEmpresa, ExcTesoreriaRequestDto? request)
        {
            if (request is null)
            {
                return CrearErrorValidacion(
                    "Debe indicar la información del proceso.");
            }

            ErrorDto? validacion = ValidarPeriodoUsuario(
                request.PeriodoId,
                request.Usuario,
                out string usuario);

            if (validacion is not null)
            {
                return validacion;
            }

            string oficina = NormalizarConDefault(
                request.Oficina,
                OficinaDefault);

            var parametros = new
            {
                pPeriodoId = request.PeriodoId,
                pOficina = oficina,
                pUsuario = usuario
            };

            return EjecutarProcedimiento(
                CodEmpresa,
                "spEXC_TrasladoExcedentesTesoreria",
                parametros,
                "Traslado de excedentes a Tesorería realizado satisfactoriamente.");
        }

        /// <summary>
        /// Traslada los excedentes a los fondos de ahorro configurados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Pago_Fondos_Aplicar(int CodEmpresa, ExcFondosRequestDto? request)
        {
            if (request is null)
            {
                return CrearErrorValidacion(
                    "Debe indicar la información del proceso.");
            }

            ErrorDto? validacion = ValidarPeriodoUsuario(
                request.PeriodoId,
                request.Usuario,
                out string usuario);

            if (validacion is not null)
            {
                return validacion;
            }

            int operadora = request.Operadora > 0
                ? request.Operadora
                : OperadoraDefault;

            string concepto = NormalizarConDefault(
                request.Concepto,
                ConceptoFondo);

            var parametros = new
            {
                pPeriodoId = request.PeriodoId,
                pOperadora = operadora,
                pUsuario = usuario,
                pConcepto = concepto
            };

            return EjecutarProcedimiento(
                CodEmpresa,
                "spEXC_ASECCSS_TrasladoAFondosAhorros",
                parametros,
                "Traslado a fondos de ahorro realizado satisfactoriamente.");
        }

        /// <summary>
        /// Ejecuta el proceso de reclasificación de salidas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AH_Excedentes_Pago_Reclasificaciones_Aplicar(int CodEmpresa, ExcReclasificacionesRequestDto? request)
        {
            if (request is null)
            {
                return CrearErrorValidacion(
                    "Debe indicar la información del proceso.");
            }

            ErrorDto? validacion = ValidarPeriodoUsuario(
                request.PeriodoId,
                request.Usuario,
                out string usuario);

            if (validacion is not null)
            {
                return validacion;
            }

            var parametros = new
            {
                pPeriodoId = request.PeriodoId,
                pUsuario = usuario
            };

            return EjecutarProcedimiento(
                CodEmpresa,
                "spEXC_CasosReclasificacionesDevoluciones",
                parametros,
                "Reclasificación de salidas realizada satisfactoriamente.");
        }

        private ErrorDto EjecutarProcedimiento( int CodEmpresa,string procedimiento, object parametros,  string mensajeExito)
        {
            using var conn = DbHelper.OpenConnection(
                _portalDB,
                CodEmpresa);

            try
            {
                conn.Execute(
                    procedimiento,
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return DbHelper.OkResponse(
                    mensajeExito);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        private static ErrorDto? ValidarPeriodoUsuario(int periodoId, string? usuario,  out string usuarioNormalizado)
        {
            usuarioNormalizado =
                (usuario ?? string.Empty).Trim();

            if (periodoId <= 0)
            {
                return CrearErrorValidacion(
                    "Debe indicar un período válido.");
            }

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return CrearErrorValidacion(
                    "Debe indicar el usuario del sistema.");
            }

            return null;
        }

        private static ErrorDto? ValidarPasoSeparacion(   short paso,   short enviarSinpe)
        {
            if (paso is < 1 or > 3)
            {
                return CrearErrorValidacion(
                    "El paso de separación indicado no es válido.");
            }

            if (paso == 2 && enviarSinpe is not 0 and not 1)
            {
                return CrearErrorValidacion(
                    "El indicador de envío SINPE no es válido.");
            }

            return null;
        }

        private static string ObtenerMensajeSeparacion( short paso)
        {
            return paso switch
            {
                1 =>
                    "Separación de ex-empleados y excedentes en cero realizada satisfactoriamente.",
                2 =>
                    "Separación de casos con cuentas bancarias realizada satisfactoriamente.",
                3 =>
                    "Separación de casos sin cuenta y DIMEX inactivo realizada satisfactoriamente.",
                _ =>
                    "Proceso de separación realizado satisfactoriamente."
            };
        }

        private static string NormalizarConDefault(  string? valor,    string valorDefault)
        {
            string resultado =
                (valor ?? string.Empty).Trim();

            return string.IsNullOrWhiteSpace(resultado)
                ? valorDefault
                : resultado;
        }

        private static ErrorDto CrearErrorValidacion( string descripcion)
        {
            return new ErrorDto
            {
                Code = -2,
                Description = descripcion
            };
        }

        private static ErrorDto<ExcPendientesDto> CrearErrorPendientes(ErrorDto error)
        {
            return new ErrorDto<ExcPendientesDto>
            {
                Code = error.Code,
                Description = error.Description,
                Result = new ExcPendientesDto()
            };
        }

        private static ErrorDto? CrearContextoCuentasInternas(ExcAcreditarCuentasInternasRequestDto? request, out CuentasInternasContexto contexto)
        {
            contexto = new CuentasInternasContexto();

            if (request is null)
            {
                return CrearErrorValidacion(
                    "Debe indicar la información del proceso.");
            }

            ErrorDto? validacion = ValidarPeriodoUsuario(
                request.PeriodoId,
                request.Usuario,
                out string usuario);

            if (validacion is not null)
            {
                return validacion;
            }

            if (request.Top <= 0)
            {
                return CrearErrorValidacion(
                    "El tamaño del lote debe ser mayor a cero.");
            }

            contexto = new CuentasInternasContexto
            {
                PeriodoId = request.PeriodoId,
                Top = request.Top,
                Usuario = usuario,
                NumDoc = $"Exc_[{request.PeriodoId}]_TI"
            };

            return null;
        }

        private static void PrepararCuentasInternas(SqlConnection conn,CuentasInternasContexto contexto)
        {
            conn.Execute(
                "spExc_ASECCSS_ValidaCuentasTransfInterna",
                new
                {
                    pPeriodoId = contexto.PeriodoId,
                    pUsuario = contexto.Usuario
                },
                commandType: CommandType.StoredProcedure);

            const string sqlCuenta = """
                SELECT RTRIM(VALOR)
                FROM SIF_PARAMETROS
                WHERE COD_PARAMETRO = 'CCEX';
                """;

            contexto.Cuenta =
                conn.QueryFirstOrDefault<string>(sqlCuenta)
                ?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(contexto.Cuenta))
            {
                throw new InvalidOperationException(
                    "No se encontró la cuenta configurada en el parámetro CCEX.");
            }
        }

        private static ExcPendientesDto ProcesarLotesCuentasInternas( SqlConnection conn,CuentasInternasContexto contexto)
        {
            ExcPendientesDto pendientes =
                EjecutarLoteCuentasInternas(
                    conn,
                    contexto);

            while (pendientes.Pendientes > 0)
            {
                pendientes =
                    EjecutarLoteCuentasInternas(
                        conn,
                        contexto);
            }

            return pendientes;
        }

        private static ExcPendientesDto EjecutarLoteCuentasInternas( SqlConnection conn,  CuentasInternasContexto contexto)
        {
            ExcPendientesDto? resultado =
                conn.QueryFirstOrDefault<ExcPendientesDto>(
                    "spExc_ASECCSS_EnviaFondoTransfInterna",
                    new
                    {
                        pPeriodoId = contexto.PeriodoId,
                        pOperadora = OperadoraDefault,
                        pSalidaOrigen =
                            SalidaTransferenciaInterna,
                        pPlan = PlanSinpe,
                        pUsuario = contexto.Usuario,
                        pConcepto = ConceptoFondo,
                        pNumDoc = contexto.NumDoc,
                        pTipoDoc = TipoDocumentoFondo,
                        pTop = contexto.Top
                    },
                    commandType: CommandType.StoredProcedure);

            return resultado ?? new ExcPendientesDto();
        }

        private static void CrearDocumentoCuentasInternas( SqlConnection conn, CuentasInternasContexto contexto)
        {
            conn.Execute(
                "spExc_FNDDocumento",
                new
                {
                    pTipoDoc = TipoDocumentoFondo,
                    pNumDoc = contexto.NumDoc,
                    pConcepto = ConceptoFondo,
                    pCuenta = contexto.Cuenta,
                    pPeriodoId = contexto.PeriodoId,
                    pUsuario = contexto.Usuario,
                    pSalida = SalidaTransferenciaInterna
                },
                commandType: CommandType.StoredProcedure);
        }

        private sealed class CuentasInternasContexto
        {
            public int PeriodoId { get; init; }
            public int Top { get; init; }
            public string Usuario { get; init; } =
                string.Empty;
            public string NumDoc { get; init; } =
                string.Empty;
            public string Cuenta { get; set; } =
                string.Empty;
        }
    }
}