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
        private const string SpAplicaAutorizacion = "dbo.spFnd_Autorizaciones_Aplica";

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
                return DbHelper.CreateOkResponse<FndAutorizaDto>(null!);
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
                return DbHelper.CreateErrorResponse<object>("Los datos de anulación son requeridos.", -2, null!);
            }

            var resultado = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                EjecutarAnulacion(CodEmpresa, Params, connection));

            return resultado.Code == 0
                ? resultado.Result ?? DbHelper.CreateErrorResponse<object>("No se obtuvo resultado de anulación.", -1, null!)
                : DbHelper.CreateErrorResponse<object>(resultado.Description ?? "Error al procesar anulación.", resultado.Code ?? -1, null!);
        }

        private ErrorDto<object> EjecutarAnulacion(int codEmpresa, FndAnulacionesParams parametros, SqlConnection connection)
        {
            var contrato = ObtenerContratoAnulacion(connection, parametros);
            if (contrato is null)
            {
                return DbHelper.CreateErrorResponse<object>("No se encontr&oacute; el contrato...", -2, null!);
            }

            var validacion = ValidarAnulacion(codEmpresa, parametros, contrato, connection);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<object>(validacion.Description ?? "Error al validar anulación.", validacion.Code ?? -1, null!);
            }

            var fecha = DateTime.Now;
            var proceso = CrearPeriodoProceso(fecha);
            var distribucion = CalcularDistribucion(parametros.aporte ?? 0m, contrato.aportes, contrato.rendimiento);
            var recibo = ObtenerConsecutivoRecibo(codEmpresa);
            if (recibo <= 0)
            {
                return DbHelper.CreateErrorResponse<object>("No se pudo obtener el consecutivo del documento.", -2, null!);
            }

            AplicarMovimientoContrato(connection, parametros, distribucion, proceso, recibo);
            RegistrarBitacoraAnulacion(codEmpresa, parametros, recibo);
            AplicarSubCuentasSiCorresponde(codEmpresa, connection, parametros, proceso, recibo);
            AplicarAutorizacionSiCorresponde(codEmpresa, connection, parametros, recibo, validacion.Result);

            return ImprimirResultadoAnulacion(codEmpresa, parametros, recibo);
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

        private static ErrorDto<bool> ValidarSeguridadAnulacion( FndAnulacionesParams parametros, SqlConnection connection)
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

            if (parametros.aporte > parametros.autoriza_monto)
            {
                return DbHelper.CreateErrorResponse("- Este movimiento requiere AUTORIZACI&Oacute;N, verifique el estado de la misma y/o solicite una!", -2, true);
            }

            return DbHelper.CreateOkResponse(true);
        }

        private long ObtenerConsecutivoRecibo(int codEmpresa)
        {
            return new MRecibos(_config).FxDocumentoConsecutivo(codEmpresa, TipoDocumentoAnulacion);
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

        private static void AplicarMovimientoContrato(
            SqlConnection connection,
            FndAnulacionesParams parametros,
            AnulacionDistribucion distribucion,
            string proceso,
            long recibo)
        {
            const string updateContrato = @"
                    UPDATE dbo.Fnd_contratos
                    SET Aportes = Aportes - @AporteAplicado,
                        rendimiento = rendimiento - @RendimientoAplicado
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato;";

            connection.Execute(updateContrato, new
            {
                AporteAplicado = distribucion.Aporte,
                RendimientoAplicado = distribucion.Rendimiento,
                Operadora = parametros.operadora,
                Plan = parametros.plan,
                Contrato = parametros.contrato
            });

            const string insertDetalle = @"
                    INSERT INTO dbo.fnd_contratos_detalle
                    (
                        Cod_operadora,
                        Cod_plan,
                        Cod_Contrato,
                        Fecha,
                        Monto,
                        Fecha_Proceso,
                        Tcon,
                        Ncon,
                        cod_concepto,
                        usuario,
                        cod_Caja
                    )
                    VALUES
                    (
                        @Operadora,
                        @Plan,
                        @Contrato,
                        GETDATE(),
                        @Aporte * -1,
                        @Proceso,
                        @TipoComprobante,
                        @Recibo,
                        @Concepto,
                        @Usuario,
                        ''
                    );";

            connection.Execute(insertDetalle, new
            {
                Operadora = parametros.operadora,
                Plan = parametros.plan,
                Contrato = parametros.contrato,
                Aporte = parametros.aporte,
                Proceso = proceso,
                TipoComprobante,
                Recibo = recibo,
                Concepto = ConceptoAnulacion,
                Usuario = parametros.usuario
            });
        }

        private void AplicarSubCuentasSiCorresponde(
            int codEmpresa,
            SqlConnection connection,
            FndAnulacionesParams parametros,
            string proceso,
            long recibo)
        {
            if (parametros.aporteLocked != true)
            {
                return;
            }

            var subCuentas = FND_Anulaciones_SubCuentas_Obtener(codEmpresa, parametros).Result ?? new List<FndAnulacionesSubCuentasDto>();
            foreach (var item in subCuentas)
            {
                AplicarSubCuenta(connection, parametros, item, proceso, recibo);
            }
        }

        private static void AplicarSubCuenta(
            SqlConnection connection,
            FndAnulacionesParams parametros,
            FndAnulacionesSubCuentasDto item,
            string proceso,
            long recibo)
        {
            var montoSubCuenta = MontoSubCuenta;
            if (montoSubCuenta <= 0)
            {
                return;
            }

            const string insertDetalle = @"
                    INSERT INTO dbo.fnd_SubCuentas_detalle
                    (
                        Idx,
                        Cod_operadora,
                        Cod_plan,
                        Cod_Contrato,
                        Fecha,
                        Monto,
                        Fecha_Proceso,
                        Tcon,
                        Ncon
                    )
                    VALUES
                    (
                        @Id,
                        @Operadora,
                        @Plan,
                        @Contrato,
                        GETDATE(),
                        @Monto * -1,
                        @Proceso,
                        @TipoComprobante,
                        @Recibo
                    );";

            connection.Execute(insertDetalle, new
            {
                Id = item.idx,
                Operadora = parametros.operadora,
                Plan = parametros.plan,
                Contrato = parametros.contrato,
                Monto = montoSubCuenta,
                Proceso = proceso,
                TipoComprobante,
                Recibo = recibo
            });

            var distribucion = CalcularDistribucionSubCuenta(montoSubCuenta, item.aportes, item.rendimiento);
            const string updateSubCuenta = @"
                    UPDATE dbo.Fnd_subCuentas
                    SET Aportes = Aportes - @Aporte,
                        rendimiento = rendimiento - @Rendimiento
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND Idx = @Id;";

            connection.Execute(updateSubCuenta, new
            {
                Id = item.idx,
                Operadora = parametros.operadora,
                Plan = parametros.plan,
                Contrato = parametros.contrato,
                Aporte = distribucion.Aporte,
                Rendimiento = distribucion.Rendimiento
            });
        }

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

        private void AplicarAutorizacionSiCorresponde(
            int codEmpresa,
            SqlConnection connection,
            FndAnulacionesParams parametros,
            long recibo,
            bool seguridadAplica)
        {
            if (!seguridadAplica)
            {
                return;
            }

            var estadoGestion = FND_Anulaciones_SolicitaAutorizacion_Obtener(codEmpresa, parametros).Result;
            if (estadoGestion is null || estadoGestion.gestion_id <= 0 || !NormalizarTexto(estadoGestion.gestion_estado).StartsWith('A'))
            {
                return;
            }

            connection.Execute(
                SpAplicaAutorizacion,
                new
                {
                    GestionId = estadoGestion.gestion_id,
                    TCon = TipoDocumentoAnulacion,
                    Ncon = recibo,
                    Usuario = parametros.usuario
                },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        private ErrorDto<object> ImprimirResultadoAnulacion(int codEmpresa, FndAnulacionesParams parametros, long recibo)
        {
            var empresaEnlace = new MProGrxMain(_config).EmpresaEnlaceObtener();
            var sysDocVersion = empresaEnlace?.FirstOrDefault()?.SysDocVersion ?? 0;
            var result = sysDocVersion == 1
                ? _mFNDFunciones.sbgFNDImprimeRecibo(codEmpresa, recibo, TipoDocumentoAnulacion, parametros.operadora)
                : new MRecibos(_config).sbImprimeRecibo(
                    codEmpresa,
                    recibo.ToString(),
                    TipoDocumentoAnulacion,
                    parametros.usuario ?? string.Empty);

            if (result.Code == 0)
            {
                result.Description = $"Anulaci&oacute;n aplicada, con Nota de Cr&eacute;dito # {recibo}";
            }

            return result;
        }

        private void RegistrarBitacoraAnulacion(int codEmpresa, FndAnulacionesParams parametros, long recibo)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(parametros.usuario).ToUpper(),
                DetalleMovimiento = $"NC Ope:{parametros.operadora} Plan:{NormalizarTexto(parametros.plan)} Cont:{parametros.contrato} Monto:{parametros.aporte} Recibo:{recibo}    ",
                Movimiento = "Registra - WEB",
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
                Cedula = NormalizarTexto(parametros?.cedula),
                Operadora = parametros?.operadora,
                Plan = NormalizarTexto(parametros?.plan),
                Contrato = parametros?.contrato,
                MntSol = parametros?.autoriza_monto ?? 0,
                MntCal = parametros?.aporte ?? 0,
                Usuario = NormalizarTexto(parametros?.usuario)
            };
        }

        private static string CrearPeriodoProceso(DateTime fecha) => $"{fecha.Year}{fecha.Month:00}";


        private const decimal MontoSubCuenta = 0m;

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private const string ConceptoAnulacion = "FND002";
        private const string TipoDocumentoAnulacion = "FNC";
        private const string TipoComprobante = "FNC";

        private sealed class AnulacionDistribucion
        {
            public decimal Aporte { get; init; }
            public decimal Rendimiento { get; init; }
        }
    }
}
