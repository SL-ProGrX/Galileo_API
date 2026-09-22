using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndAnulacionesDb
    {
        private readonly IConfiguration _config;
        private readonly MFndFuncionesDb _mFNDFunciones;
        private readonly MSecurityMainDb _mSecurity;
        private readonly int vModulo = 18;
        private const string SpAutorizaDatos = "spFnd_Autoriza_Datos";
        private const string SpGestionRegistro = "spFnd_Gestion_Registro";
        private const string SpGestionEstado = "spFnd_Gestion_Estado";
        private const string SpSeguridadAnulacion = "dbo.spFndSeguridad_ApAnul";

        public FrmFndAnulacionesDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mFNDFunciones = new MFndFuncionesDb(_config);
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener información de anulaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public ErrorDto<FndAnulacionesDto> FND_Anulaciones_Obtener(int CodEmpresa, FndAnulacionesParams Params)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new FndAnulacionesDto());
            }
            const string query = @"
                    SELECT
                        C.cedula,
                        S.nombre,
                        P.descripcion AS PlanX,
                        O.descripcion AS OperadoraX,
                        C.cod_plan,
                        C.cod_contrato,
                        C.cod_operadora,
                        C.estado,
                        C.fecha_Inicio,
                        ISNULL(P.cuenta_Maestra,0) AS CuentaMaestra,
                        P.Tipo_CDP,
                        P.permite_mov_cajas
                    FROM dbo.fnd_contratos C
                    INNER JOIN dbo.Socios S ON C.cedula = S.cedula
                    INNER JOIN dbo.fnd_planes P ON C.cod_plan = P.cod_plan AND C.cod_operadora = P.cod_operadora
                    INNER JOIN dbo.fnd_operadoras O ON C.cod_operadora = O.cod_operadora
                    WHERE C.cod_operadora = @Operadora
                      AND C.cod_plan = @Plan
                      AND C.cod_Contrato = @Contrato;";

            var result = DbHelper.ExecuteSingleQuery<FndAnulacionesDto>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new FndAnulacionesDto(),
                CrearParametrosContrato(Params));

            return new ErrorDto<FndAnulacionesDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAnulacionesDto()
            };
        }

        /// <summary>
        /// Obtener subcuentas asociadas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public ErrorDto<List<FndAnulacionesSubCuentasDto>> FND_Anulaciones_SubCuentas_Obtener(int CodEmpresa, FndAnulacionesParams Params)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de subcuentas son requeridos.",
                    -2,
                    new List<FndAnulacionesSubCuentasDto>());
            }
            const string query = @"
                    SELECT
                        IDx,
                        Cedula,
                        Nombre,
                        aportes,
                        rendimiento
                    FROM dbo.fnd_subCuentas
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND estado = 'A';";

            return DbHelper.ExecuteListQuery<FndAnulacionesSubCuentasDto>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                CrearParametrosContrato(Params));
        }

        /// <summary>
        /// Obtener autorizadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Plan"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<FndAutorizaDto> FND_Anulaciones_Autoriza_Obtener(int CodEmpresa, string Plan, string Usuario)
        {
            if (_mFNDFunciones.fxFndParametro(CodEmpresa, "01.2") != "S")
            {
                return DbHelper.CreateOkResponse<FndAutorizaDto>(null);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndAutorizaDto>(
                    SpAutorizaDatos,
                    new
                    {
                        Plan = NormalizarTexto(Plan),
                        TipoMov = "N",
                        Usuario = NormalizarTexto(Usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndAutorizaDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAutorizaDto()
            };
        }

        /// <summary>
        /// Solicitar autorización de anulaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Params"></param>
        /// <returns></returns>
        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_SolicitaAutorizacion_Obtener(int CodEmpresa, FndAnulacionesParams Params)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de autorización son requeridos.",
                    -2,
                    new FndAnulacionesEstadoGestionDto());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndAnulacionesEstadoGestionDto>(
                    SpGestionRegistro,
                    CrearParametrosSolicitudAutorizacion(Params),
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndAnulacionesEstadoGestionDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAnulacionesEstadoGestionDto()
            };
        }

        /// <summary>
        /// Refrescar estado de autorización
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="GestionId"></param>
        /// <returns></returns>
        public ErrorDto<FndAnulacionesEstadoGestionDto> FND_Anulaciones_AutorizacionRefresh_Obtener(int CodEmpresa, int GestionId)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndAnulacionesEstadoGestionDto>(
                    SpGestionEstado,
                    new { GestionId = GestionId },
                    commandType: System.Data.CommandType.StoredProcedure));

            return new ErrorDto<FndAnulacionesEstadoGestionDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new FndAnulacionesEstadoGestionDto()
            };
        }

        /// <summary>
        /// Procesar anulacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Params"></param>
        /// <param name="Accion"></param>
        /// <param name="Notas"></param>
        /// <returns></returns>
        public ErrorDto<object> FND_Anulaciones_Anular(int CodEmpresa, FndAnulacionesParams Params, string Accion, string Notas)
        {
            if (Params is null)
            {
                return DbHelper.CreateErrorResponse<object>("Los datos de anulación son requeridos.", -2, null);
            }

            var resultado = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                EjecutarAnulacion(CodEmpresa, Params, NormalizarTexto(Accion).ToUpper(), NormalizarTexto(Notas), connection));

            return resultado.Code == 0
                ? resultado.Result ?? DbHelper.CreateErrorResponse<object>("No se obtuvo resultado de anulación.", -1, null)
                : DbHelper.CreateErrorResponse<object>(resultado.Description ?? "Error al procesar anulación.", resultado.Code ?? -1, null);
        }

        /// <summary>
        /// Paridad VB6 frmFNDAnulaciones.cmdAnular: valida, ejecuta spFondos_Anula_Aporte (documento FNC en
        /// SIF_TRANSACCIONES, asiento y saldo a favor en cajas), aplica subcuentas e imprime el recibo.
        /// </summary>
        private ErrorDto<object> EjecutarAnulacion(int codEmpresa, FndAnulacionesParams parametros, string accion, string notas, SqlConnection connection)
        {
            if (notas.Length < 30)
            {
                return DbHelper.CreateErrorResponse<object>("Indique una nota v&aacute;lida para justificar el movimiento!", -2, null);
            }

            if (accion != AccionCuentaContable && accion != AccionSaldoFavor)
            {
                return DbHelper.CreateErrorResponse<object>("Indique la acci&oacute;n a procesar (Cuenta Contable o Saldo a Favor).", -2, null);
            }

            var contrato = ObtenerContratoAnulacion(connection, parametros);
            if (contrato is null)
            {
                return DbHelper.CreateErrorResponse<object>("No se encontr&oacute; el contrato...", -2, null);
            }

            var validacion = ValidarAnulacion(codEmpresa, parametros, contrato, connection);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<object>(validacion.Description ?? "Error al validar anulación.", validacion.Code ?? -1, null);
            }

            var validacionSubCuentas = ValidarSubCuentas(codEmpresa, parametros);
            if (validacionSubCuentas.Code != 0)
            {
                return DbHelper.CreateErrorResponse<object>(validacionSubCuentas.Description ?? "Error al validar subcuentas.", validacionSubCuentas.Code ?? -1, null);
            }

            var salida = ObtenerSalidaAnulacion(connection, accion);
            if (salida is null)
            {
                return DbHelper.CreateErrorResponse<object>(
                    accion == AccionSaldoFavor
                        ? "No existe una forma de pago de Saldo a Favor activa (SIF_FORMAS_PAGO tipo 'S')."
                        : "No se especific&oacute; una cuenta v&aacute;lida...",
                    -2,
                    null);
            }

            var distribucion = CalcularDistribucion(parametros.aporte ?? 0m, contrato.aportes, contrato.rendimiento);
            var aplica = connection.QueryFirstOrDefault<FndAnulacionesAplicaResultDto>(
                SpAnulaAporte,
                new
                {
                    Operadora = parametros.operadora,
                    Plan = NormalizarTexto(parametros.plan),
                    Contrato = parametros.contrato,
                    TipoDoc = TipoDocumentoAnulacion,
                    Aportes = distribucion.Aporte,
                    Rendimiento = distribucion.Rendimiento,
                    Usuario = NormalizarTexto(parametros.usuario),
                    Notas = notas,
                    AccionTipo = accion,
                    Cuenta = salida.Cuenta,
                    SF_Codigo = salida.FormaPago,
                    Documento = string.Empty,
                    Deposito = string.Empty,
                    GestionId = parametros.gestion_id ?? 0
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (aplica is null || aplica.Pass != 1)
            {
                return DbHelper.CreateErrorResponse<object>(aplica?.Mensaje ?? "No se pudo aplicar la anulaci&oacute;n.", -2, null);
            }

            var numDoc = NormalizarTexto(aplica.NumDoc);
            RegistrarBitacoraAnulacion(codEmpresa, parametros, aplica);
            AplicarSubCuentasSiCorresponde(connection, parametros, aplica, numDoc);

            return ImprimirResultadoAnulacion(codEmpresa, parametros, numDoc);
        }

        private static FndAnulacionesSubCuentasDto? ObtenerContratoAnulacion(SqlConnection connection, FndAnulacionesParams parametros)
        {
            const string query = @"
                    SELECT aportes, rendimiento
                    FROM dbo.fnd_contratos
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

            return connection.QueryFirstOrDefault<FndAnulacionesSubCuentasDto>(query, CrearParametrosContrato(parametros));
        }

        private ErrorDto<bool> ValidarAnulacion(int codEmpresa, FndAnulacionesParams parametros, FndAnulacionesSubCuentasDto contrato, SqlConnection connection)
        {
            if ((parametros.aporte ?? 0m) <= 0m)
            {
                return DbHelper.CreateErrorResponse("No se especific&oacute; el contrato o el monto", -2, false);
            }

            if (parametros.aporte > contrato.aportes + contrato.rendimiento)
            {
                return DbHelper.CreateErrorResponse("La Anulaci&oacute;n es mayor que el total de los aportes y rendimientos del contrato...", -2, false);
            }

            if (_mFNDFunciones.fxFndParametro(codEmpresa, "01.2") != "S")
            {
                return DbHelper.CreateOkResponse(false);
            }

            return ValidarSeguridadAnulacion(parametros, connection);
        }

        private static ErrorDto<bool> ValidarSeguridadAnulacion(FndAnulacionesParams parametros, SqlConnection connection)
        {
            var autoriza = connection.QueryFirstOrDefault<int>(
                SpSeguridadAnulacion,
                new
                {
                    Operadora = parametros.operadora,
                    Plan = parametros.plan,
                    Usuario = parametros.usuario
                },
                commandType: System.Data.CommandType.StoredProcedure);

            if (autoriza == 0)
            {
                return DbHelper.CreateErrorResponse("El Usuario no tiene nivel de Autorizaci&oacute;n para realizar este movimiento!", -2, true);
            }

            // VB6: si excede el monto autorizado exige una gestion con estado Autorizado (verificado en BD, no en el cliente).
            if (parametros.aporte > parametros.autoriza_monto && !GestionAutorizada(connection, parametros.gestion_id))
            {
                return DbHelper.CreateErrorResponse("- Este movimiento requiere AUTORIZACI&Oacute;N, verifique el estado de la misma y/o solicite una!", -2, true);
            }

            return DbHelper.CreateOkResponse(true);
        }

        private static bool GestionAutorizada(SqlConnection connection, int? gestionId)
        {
            if ((gestionId ?? 0) <= 0)
            {
                return false;
            }

            var estado = connection.QueryFirstOrDefault<FndAnulacionesEstadoGestionDto>(
                SpGestionEstado,
                new { GestionId = gestionId },
                commandType: System.Data.CommandType.StoredProcedure);

            return NormalizarTexto(estado?.gestion_estado).StartsWith('A');
        }

        /// <summary>VB6: el monto por subcuenta no puede exceder aportes + rendimiento y el total debe cuadrar con la anulacion.</summary>
        private ErrorDto<bool> ValidarSubCuentas(int codEmpresa, FndAnulacionesParams parametros)
        {
            if (parametros.aporteLocked != true)
            {
                return DbHelper.CreateOkResponse(true);
            }

            var montos = (parametros.subcuentas ?? new List<FndAnulacionesSubCuentaMontoDto>())
                .Where(x => x.anulacion > 0m)
                .ToList();
            if (montos.Count == 0 || montos.Sum(x => x.anulacion) != parametros.aporte)
            {
                return DbHelper.CreateErrorResponse("El desglose de subcuentas debe coincidir con el monto a anular.", -2, false);
            }

            var actuales = (FND_Anulaciones_SubCuentas_Obtener(codEmpresa, parametros).Result ?? new List<FndAnulacionesSubCuentasDto>())
                .ToDictionary(x => x.idx);
            foreach (var monto in montos)
            {
                if (!actuales.TryGetValue(monto.idx, out var actual) || monto.anulacion > actual.aportes + actual.rendimiento)
                {
                    return DbHelper.CreateErrorResponse($"La Anulaci&oacute;n es mayor al total de los aportes y rendimientos de las subCuentas ({monto.idx})...", -2, false);
                }
            }

            return DbHelper.CreateOkResponse(true);
        }

        /// <summary>VB6 "Valida Salida": C = cuenta del documento FNC, S = forma de pago de saldo a favor.</summary>
        private static SalidaAnulacion? ObtenerSalidaAnulacion(SqlConnection connection, string accion)
        {
            if (accion == AccionSaldoFavor)
            {
                return connection.QueryFirstOrDefault<SalidaAnulacion>(@"
                    SELECT RTRIM(COD_CUENTA) AS Cuenta, RTRIM(COD_FORMA_PAGO) AS FormaPago
                    FROM SIF_FORMAS_PAGO
                    WHERE TIPO = 'S' AND Activa = 1;");
            }

            var cuenta = connection.QueryFirstOrDefault<string>(
                "SELECT RTRIM(COD_CUENTA) FROM SIF_DOCUMENTOS WHERE TIPO_DOCUMENTO = @TipoDoc;",
                new { TipoDoc = TipoDocumentoAnulacion });

            return string.IsNullOrWhiteSpace(cuenta)
                ? null
                : new SalidaAnulacion { Cuenta = cuenta.Trim(), FormaPago = string.Empty };
        }

        private static AnulacionDistribucion CalcularDistribucion(decimal monto, decimal aporteActual, decimal rendimientoActual)
        {
            var sobrante = monto;
            var aplicadoRendimiento = Math.Min(sobrante, rendimientoActual);
            sobrante -= aplicadoRendimiento;
            var aplicadoAporte = Math.Min(sobrante, aporteActual);

            return new AnulacionDistribucion
            {
                Aporte = aplicadoAporte,
                Rendimiento = aplicadoRendimiento
            };
        }

        /// <summary>VB6: detalle y rebajo por subcuenta con Fecha/Proceso/NumDoc devueltos por el SP, en una transaccion.</summary>
        private static void AplicarSubCuentasSiCorresponde(
            SqlConnection connection,
            FndAnulacionesParams parametros,
            FndAnulacionesAplicaResultDto aplica,
            string numDoc)
        {
            if (parametros.aporteLocked != true)
            {
                return;
            }

            var montos = (parametros.subcuentas ?? new List<FndAnulacionesSubCuentaMontoDto>())
                .Where(x => x.anulacion > 0m)
                .ToList();
            if (montos.Count == 0)
            {
                return;
            }

            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();
            foreach (var monto in montos)
            {
                AplicarSubCuenta(connection, transaction, parametros, monto, aplica, numDoc);
            }
            transaction.Commit();
        }

        private static void AplicarSubCuenta(
            SqlConnection connection,
            SqlTransaction transaction,
            FndAnulacionesParams parametros,
            FndAnulacionesSubCuentaMontoDto monto,
            FndAnulacionesAplicaResultDto aplica,
            string numDoc)
        {
            var llave = new
            {
                Id = monto.idx,
                Operadora = parametros.operadora,
                Plan = NormalizarTexto(parametros.plan),
                Contrato = parametros.contrato
            };

            var actual = connection.QuerySingle<FndAnulacionesSubCuentasDto>(@"
                    SELECT IdX AS idx, ISNULL(Aportes, 0) AS aportes, ISNULL(rendimiento, 0) AS rendimiento
                    FROM dbo.Fnd_subCuentas WITH (UPDLOCK)
                    WHERE cod_operadora = @Operadora AND cod_plan = @Plan AND cod_contrato = @Contrato AND IdX = @Id;",
                llave, transaction);

            connection.Execute(@"
                    INSERT INTO dbo.fnd_SubCuentas_detalle (Idx, Cod_operadora, Cod_plan, Cod_Contrato, Fecha, Monto, Fecha_Proceso, Tcon, Ncon)
                    VALUES (@Id, @Operadora, @Plan, @Contrato, @Fecha, @Monto * -1, @Proceso, @TipoDoc, @NumDoc);",
                new
                {
                    llave.Id,
                    llave.Operadora,
                    llave.Plan,
                    llave.Contrato,
                    Fecha = aplica.Fecha ?? DateTime.Now,
                    Monto = monto.anulacion,
                    aplica.Proceso,
                    TipoDoc = TipoDocumentoAnulacion,
                    NumDoc = numDoc
                },
                transaction);

            var distribucion = CalcularDistribucionSubCuenta(monto.anulacion, actual.aportes, actual.rendimiento);
            connection.Execute(@"
                    UPDATE dbo.Fnd_subCuentas
                    SET Aportes = Aportes - @Aporte,
                        rendimiento = rendimiento - @Rendimiento
                    WHERE cod_operadora = @Operadora AND cod_plan = @Plan AND cod_contrato = @Contrato AND IdX = @Id;",
                new
                {
                    llave.Id,
                    llave.Operadora,
                    llave.Plan,
                    llave.Contrato,
                    distribucion.Aporte,
                    distribucion.Rendimiento
                },
                transaction);
        }

        /// <summary>VB6 subcuentas: primero aportes, luego rendimiento.</summary>
        private static AnulacionDistribucion CalcularDistribucionSubCuenta(decimal monto, decimal aporteActual, decimal rendimientoActual)
        {
            var sobrante = monto;
            var aporte = Math.Min(sobrante, aporteActual);
            sobrante -= aporte;
            var rendimiento = Math.Min(sobrante, rendimientoActual);

            return new AnulacionDistribucion
            {
                Aporte = aporte,
                Rendimiento = rendimiento
            };
        }

        private ErrorDto<object> ImprimirResultadoAnulacion(int codEmpresa, FndAnulacionesParams parametros, string numDoc)
        {
            var empresaEnlace = new MProGrxMain(_config).EmpresaEnlaceObtener();
            var sysDocVersion = empresaEnlace?.FirstOrDefault()?.SysDocVersion ?? 0;
            var result = sysDocVersion == 1 && long.TryParse(numDoc, out var recibo)
                ? _mFNDFunciones.sbgFNDImprimeRecibo(codEmpresa, recibo, TipoDocumentoAnulacion, parametros.operadora)
                : new MRecibos(_config).sbImprimeRecibo(
                    codEmpresa,
                    numDoc,
                    TipoDocumentoAnulacion,
                    parametros.usuario ?? string.Empty);

            if (result.Code == 0)
            {
                result.Description = $"Anulaci&oacute;n aplicada, con Nota de Cr&eacute;dito # {numDoc}";
            }
            else
            {
                result.Code = -2;
                result.Description = $"Anulaci&oacute;n aplicada, con Nota de Cr&eacute;dito # {numDoc}, pero no se pudo generar el recibo: {result.Description}";
            }

            return result;
        }

        /// <summary>VB6: Call Bitacora(rs!Movimiento, rs!Mensaje).</summary>
        private void RegistrarBitacoraAnulacion(int codEmpresa, FndAnulacionesParams parametros, FndAnulacionesAplicaResultDto aplica)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(parametros.usuario).ToUpper(),
                DetalleMovimiento = NormalizarTexto(aplica.Mensaje),
                Movimiento = string.IsNullOrWhiteSpace(aplica.Movimiento) ? "Registra - WEB" : NormalizarTexto(aplica.Movimiento),
                Modulo = vModulo
            });
        }

        private static object CrearParametrosContrato(FndAnulacionesParams parametros)
        {
            return new
            {
                Operadora = parametros?.operadora,
                Plan = NormalizarTexto(parametros?.plan),
                Contrato = parametros?.contrato
            };
        }

        private static object CrearParametrosSolicitudAutorizacion(FndAnulacionesParams parametros)
        {
            return new
            {
                // VB6: exec spFnd_Gestion_Registro Cedula, 'N', Operadora, Plan, Contrato, mAutorizaMonto (MntCal), txtAporte (MntSol), Usuario, Nota
                Cedula = NormalizarTexto(parametros?.cedula),
                Tipo = TipoGestionAnulacion,
                Operadora = parametros?.operadora,
                Plan = NormalizarTexto(parametros?.plan),
                Contrato = parametros?.contrato,
                MntCal = parametros?.autoriza_monto ?? 0,
                MntSol = parametros?.aporte ?? 0,
                Usuario = NormalizarTexto(parametros?.usuario),
                Nota = NormalizarTexto(parametros?.nota)
            };
        }




        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private const string TipoDocumentoAnulacion = "FNC";
        private const string AccionCuentaContable = "C";
        private const string AccionSaldoFavor = "S";
        private const string SpAnulaAporte = "dbo.spFondos_Anula_Aporte";
        private const string TipoGestionAnulacion = "N";

        private sealed class AnulacionDistribucion
        {
            public decimal Aporte { get; init; }
            public decimal Rendimiento { get; init; }
        }

        private sealed class SalidaAnulacion
        {
            public string Cuenta { get; init; } = string.Empty;
            public string FormaPago { get; init; } = string.Empty;
        }
    }
}
