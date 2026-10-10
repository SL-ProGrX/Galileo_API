using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXBalancesLoadDB
    {
        private const string MsgContabilidadUnidadRequeridas = "Debe indicar la contabilidad y la unidad.";
        private const string MsgUsuarioRequerido = "Debe indicar el usuario.";
        private const string MsgLineasRequeridas = "Debe enviar líneas para procesar.";
        private const string MsgHistoricoInvalido = "Debe indicar un histórico válido.";
        private const string MsgContabilidadInvalida = "Debe indicar una contabilidad válida.";
        private const string MsgErrorUnidades = "No fue posible cargar las unidades.";
        private const string MsgErrorConsolidacion = "No fue posible obtener la información de consolidación.";
        private const string MsgErrorValidacionImportacion = "No fue posible validar la importación.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        /// <summary>
        /// Inicializa el acceso a datos de la carga de balances.
        /// </summary>
        /// <param name="config">Configuración de conexiones de la aplicación.</param>
        public FrmCntXBalancesLoadDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las unidades activas y la configuración de consolidación de la pantalla.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="contabilidad">Código de la contabilidad seleccionada.</param>
        /// <param name="anio">Año del período contable.</param>
        /// <param name="mes">Mes del período contable.</param>
        /// <returns>Datos iniciales requeridos por el formulario.</returns>
        public ErrorDto<CntXBalancesLoadPantallaDto> CntX_Balances_Load_Pantalla_Obtener(
            int codEmpresa,
            int contabilidad,
            int anio,
            int mes)
        {
            var unidadesResp = DbHelper.ExecuteListQuery<DropDownListaGenericaModel?>(
                _portalDb,
                codEmpresa,
                @"
                select
                    Cod_Unidad as item,
                    Descripcion as descripcion
                from CntX_Unidades
                where cod_Contabilidad = @Contabilidad
                  and Activa = 1
                order by Descripcion;",
                new
                {
                    Contabilidad = contabilidad
                });

            if (unidadesResp.Code != 0)
            {
                return CrearError(
                    unidadesResp.Description ?? MsgErrorUnidades,
                    unidadesResp.Code ?? -1,
                    new CntXBalancesLoadPantallaDto());
            }

            var contaResp = DbHelper.ExecuteSingleQuery<CntXBalancesLoadContabilidadInfoDto?>(
                _portalDb,
                codEmpresa,
                @"
                select
                    isnull(I_CONSOLIDADORA, 0) as consolida_ind,
                    isnull(CONSOLIDA_CONTA_BASE, 0) as consolida_conta,
                    isnull(CONSOLIDA_UNIDAD_BASE, '') as consolida_unidad
                from CntX_Contabilidades
                where cod_contabilidad = @Contabilidad;",
                null,
                new
                {
                    Contabilidad = contabilidad
                });

            if (contaResp.Code != 0)
            {
                return CrearError(
                    contaResp.Description ?? MsgErrorConsolidacion,
                    contaResp.Code ?? -1,
                    new CntXBalancesLoadPantallaDto());
            }

            return DbHelper.CreateOkResponse(new CntXBalancesLoadPantallaDto
            {
                contabilidad = contabilidad,
                anio = anio,
                mes = mes,
                periodo_desc = FxCntXPeriodoDesc(mes, anio),
                consolida_ind = contaResp.Result?.consolida_ind ?? 0,
                consolida_conta = contaResp.Result?.consolida_conta ?? 0,
                consolida_unidad = contaResp.Result?.consolida_unidad ?? string.Empty,
                unidades = unidadesResp.Result ?? new List<DropDownListaGenericaModel?>()
            });
        }

        /// <summary>
        /// Lista los históricos disponibles para una unidad y período.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Filtros de contabilidad, unidad y período.</param>
        /// <returns>Históricos disponibles para seleccionar.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Balances_Load_Historico_Listar(
            int codEmpresa,
            CntXBalancesLoadHistoricoListarRequestDto request)
        {
            if (ContabilidadUnidadInvalidas(request.contabilidad, request.unidad))
            {
                return CrearErrorContabilidadUnidad(new List<DropDownListaGenericaModel>());
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);

                var rows = conn.Query(
                    @"exec spCntX_Balance_Cargado_Historico
                        @Contabilidad,
                        @Unidad,
                        @Anio,
                        @Mes",
                    new
                    {
                        Contabilidad = request.contabilidad,
                        Unidad = request.unidad.Trim(),
                        Anio = request.anio,
                        Mes = request.mes
                    }).AsList();

                var result = rows
                    .Select(RowToDictionary)
                    .Select(data => new DropDownListaGenericaModel
                    {
                        item = GetString(data, "item", "IdX", "IDX", "historico_id", "id", "codigo"),
                        descripcion = GetString(data, "descripcion", "ItmX", "ITMX", "detalle", "texto", "nombre")
                    })
                    .Where(item => !string.IsNullOrWhiteSpace(Convert.ToString(item.item)))
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return CrearErrorDb(ex, new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Consulta las líneas de un histórico de carga.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="historicoId">Identificador del histórico.</param>
        /// <returns>Detalle de las líneas almacenadas en el histórico.</returns>
        public ErrorDto<List<CntXBalancesLoadResultadoDto>> CntX_Balances_Load_Historico_Consultar(
            int codEmpresa,
            int historicoId)
        {
            if (historicoId <= 0)
            {
                return CrearError(
                    MsgHistoricoInvalido,
                    -2,
                    new List<CntXBalancesLoadResultadoDto>());
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);

                var result = conn.Query(
                    @"exec spCntX_Balance_Cargado_Historico_Consulta @HistoricoId",
                    new
                    {
                        HistoricoId = historicoId
                    })
                    .AsList()
                    .Select(MapResultado)
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return CrearErrorDb(ex, new List<CntXBalancesLoadResultadoDto>());
            }
        }

        /// <summary>
        /// Carga las líneas del archivo, ejecuta el automapeo y devuelve sus resultados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Archivo transformado y contexto contable de la carga.</param>
        /// <returns>Resultados temporales de la carga y validación.</returns>
        public ErrorDto<List<CntXBalancesLoadResultadoDto>> CntX_Balances_Load_Archivo_Cargar(
            int codEmpresa,
            CntXBalancesLoadArchivoCargarRequestDto request)
        {
            if (ContabilidadUnidadInvalidas(request.contabilidad, request.unidad))
            {
                return CrearErrorContabilidadUnidad(new List<CntXBalancesLoadResultadoDto>());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return CrearError(
                    MsgUsuarioRequerido,
                    -2,
                    new List<CntXBalancesLoadResultadoDto>());
            }

            if (request.lineas is null || request.lineas.Count == 0)
            {
                return CrearError(
                    MsgLineasRequeridas,
                    -2,
                    new List<CntXBalancesLoadResultadoDto>());
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);

                conn.Execute(
                    @"
                    delete from CNTX_LOAD_BALANCES
                    where USUARIO = @Usuario;",
                    new
                    {
                        Usuario = request.usuario.Trim()
                    });

                int correlativo = 0;

                foreach (var item in request.lineas)
                {
                    correlativo++;

                    var cuenta = (item.cuenta ?? string.Empty).Trim();
                    var ctaExcluye = (item.cta_excluye ?? string.Empty).Trim();

                    if (string.IsNullOrWhiteSpace(cuenta) && !string.IsNullOrWhiteSpace(ctaExcluye))
                    {
                        cuenta = ctaExcluye;
                    }

                    if (string.IsNullOrWhiteSpace(cuenta))
                    {
                        continue;
                    }

                    conn.Execute(
                        @"exec spCntX_Consolida_Balance_Importa_Cargado
                            @Consolidadora,
                            @Unidad,
                            @Anio,
                            @Mes,
                            @Cuenta,
                            @ConsolidadoraCuenta,
                            @Descripcion,
                            @SaldoInicial,
                            @Debitos,
                            @Creditos,
                            @SaldoFinal,
                            @Tc,
                            @Usuario,
                            @Linea,
                            @CtaExcluye",
                        new
                        {
                            Consolidadora = request.contabilidad,
                            Unidad = request.unidad.Trim(),
                            Anio = request.anio,
                            Mes = request.mes,
                            Cuenta = cuenta,
                            ConsolidadoraCuenta = (item.consolidadora ?? string.Empty).Trim(),
                            Descripcion = (item.descripcion ?? string.Empty).Trim(),
                            SaldoInicial = item.saldo_inicial,
                            Debitos = item.debitos,
                            Creditos = item.creditos,
                            SaldoFinal = item.saldo_final,
                            Tc = item.tc,
                            Usuario = request.usuario.Trim(),
                            Linea = correlativo,
                            CtaExcluye = ctaExcluye
                        });
                }

                conn.Execute(
                    @"exec spCntX_Consolida_Balance_Importa_Mapeo
                        @Consolidadora,
                        @Unidad,
                        @Anio,
                        @Mes,
                        @Usuario",
                    new
                    {
                        Consolidadora = request.contabilidad,
                        Unidad = request.unidad.Trim(),
                        Anio = request.anio,
                        Mes = request.mes,
                        Usuario = request.usuario.Trim()
                    });

                var result = conn.Query(
                    @"exec spCntX_Consolida_Balance_Importa_Resultados
                        @Consolidadora,
                        @Unidad,
                        @Anio,
                        @Mes,
                        @Usuario",
                    new
                    {
                        Consolidadora = request.contabilidad,
                        Unidad = request.unidad.Trim(),
                        Anio = request.anio,
                        Mes = request.mes,
                        Usuario = request.usuario.Trim()
                    })
                    .AsList()
                    .Select(MapResultado)
                    .ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return CrearErrorDb(ex, new List<CntXBalancesLoadResultadoDto>());
            }
        }

        /// <summary>
        /// Valida las líneas temporales antes de confirmar la importación.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Contabilidad, unidad, período y usuario por validar.</param>
        /// <returns>Cantidad de líneas erróneas encontradas.</returns>
        public ErrorDto<CntXBalancesLoadValidaDto?> CntX_Balances_Load_Importar_Validar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
        {
            if (ContabilidadUnidadInvalidas(request.contabilidad, request.unidad))
            {
                return CrearErrorContabilidadUnidad<CntXBalancesLoadValidaDto?>(null);
            }

            return DbHelper.ExecuteSingleQuery<CntXBalancesLoadValidaDto?>(
                _portalDb,
                codEmpresa,
                @"exec spCntX_Consolida_Balance_Importa_Valida
                    @Consolidadora,
                    @Unidad,
                    @Anio,
                    @Mes,
                    @Usuario",
                null,
                CrearParametrosProceso(request));
        }

        /// <summary>
        /// Importa el balance temporal después de verificar que no tenga errores.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Contabilidad, unidad, período y usuario que se importarán.</param>
        /// <returns>Estado y mensaje del proceso de importación.</returns>
        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_Importar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
        {
            var validaResp = CntX_Balances_Load_Importar_Validar(codEmpresa, request);

            if (validaResp.Code != 0)
            {
                return CrearError<CntXBalancesLoadProcesoResultDto?>(
                    validaResp.Description ?? MsgErrorValidacionImportacion,
                    validaResp.Code ?? -1,
                    null);
            }

            var casosErroneos = validaResp.Result?.casos_erroneos ?? 0;
            if (casosErroneos > 0)
            {
                return CrearError<CntXBalancesLoadProcesoResultDto?>(
                    $"Existen {casosErroneos} líneas erróneas, verifíquelas primero antes de importarlas.",
                    -2,
                    null);
            }

            var resp = EjecutarProceso(
                codEmpresa,
                @"exec spCntX_Consolida_Balance_Importa
                    @Consolidadora,
                    @Unidad,
                    @Anio,
                    @Mes,
                    @Usuario",
                request);

            if (resp.Code == 0 && resp.Result?.pass == 1)
            {
                RegistrarBitacoraProceso(
                    codEmpresa,
                    request,
                    $"Importación del Balance de la Contabilidad Id: [{request.contabilidad}] {FxCntXPeriodoDesc(request.mes, request.anio)} Unidad: {request.unidad}");
            }

            return resp;
        }

        /// <summary>
        /// Inicializa el balance de una unidad para el período seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Contabilidad, unidad, período y usuario del proceso.</param>
        /// <returns>Estado y mensaje de la inicialización.</returns>
        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_Inicializar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
        {
            if (ContabilidadUnidadInvalidas(request.contabilidad, request.unidad))
            {
                return CrearErrorContabilidadUnidad<CntXBalancesLoadProcesoResultDto?>(null);
            }

            var resp = EjecutarProceso(
                codEmpresa,
                @"exec spCntX_Consolida_Balance_Inicializa
                    @Consolidadora,
                    @Unidad,
                    @Anio,
                    @Mes,
                    @Usuario",
                request);

            if (resp.Code == 0 && resp.Result?.pass == 1)
            {
                RegistrarBitacoraProceso(
                    codEmpresa,
                    request,
                    $"Inicialización del Balance de la Contabilidad Id: [{request.contabilidad}] {FxCntXPeriodoDesc(request.mes, request.anio)}, Unidad: {request.unidad}");
            }

            return resp;
        }

        /// <summary>
        /// Importa el balance desde la contabilidad base configurada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="request">Contabilidad, período y usuario del proceso.</param>
        /// <returns>Estado y mensaje de la importación desde la contabilidad base.</returns>
        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_ImportarContaBase(
            int codEmpresa,
            CntXBalancesLoadImportaContaBaseRequestDto request)
        {
            if (request.contabilidad <= 0)
            {
                return CrearError<CntXBalancesLoadProcesoResultDto?>(
                    MsgContabilidadInvalida,
                    -2,
                    null);
            }

            var resp = EjecutarImportaContaBase(
                codEmpresa,
                @"exec spCntX_Consolida_Importa_Conta_Base
                    @Consolidadora,
                    @Usuario,
                    @Anio,
                    @Mes",
                request);

            if (resp.Code == 0 && resp.Result?.pass == 1)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    $"Importación del Balance de la Contabilidad Base de: {request.contabilidad} {FxCntXPeriodoDesc(request.mes, request.anio)}");
            }

            return resp;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle)
        {
            _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = "Aplica - Web",
                Modulo = vModulo
            });
        }

        private static string FxCntXPeriodoDesc(int mes, int anio)
        {
            if (mes <= 0 || mes > 12 || anio <= 0)
            {
                return string.Empty;
            }

            string[] meses =
            {
                "",
                "Enero",
                "Febrero",
                "Marzo",
                "Abril",
                "Mayo",
                "Junio",
                "Julio",
                "Agosto",
                "Septiembre",
                "Octubre",
                "Noviembre",
                "Diciembre"
            };

            return $"{meses[mes]} {anio}";
        }

        private static object CrearParametrosProceso(CntXBalancesLoadProcesoRequestDto request)
        {
            return new
            {
                Consolidadora = request.contabilidad,
                Unidad = request.unidad.Trim(),
                Anio = request.anio,
                Mes = request.mes,
                Usuario = request.usuario.Trim()
            };
        }

        private static object CrearParametrosImportaContaBase(CntXBalancesLoadImportaContaBaseRequestDto request)
        {
            return new
            {
                Consolidadora = request.contabilidad,
                Usuario = request.usuario.Trim(),
                Anio = request.anio,
                Mes = request.mes
            };
        }

        private ErrorDto<CntXBalancesLoadProcesoResultDto?> EjecutarProceso(
            int codEmpresa,
            string sql,
            CntXBalancesLoadProcesoRequestDto request)
        {
            return DbHelper.ExecuteSingleQuery<CntXBalancesLoadProcesoResultDto?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                CrearParametrosProceso(request));
        }

        private ErrorDto<CntXBalancesLoadProcesoResultDto?> EjecutarImportaContaBase(
            int codEmpresa,
            string sql,
            CntXBalancesLoadImportaContaBaseRequestDto request)
        {
            return DbHelper.ExecuteSingleQuery<CntXBalancesLoadProcesoResultDto?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                CrearParametrosImportaContaBase(request));
        }

        private void RegistrarBitacoraProceso(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request,
            string detalle)
        {
            RegistrarBitacora(codEmpresa, request.usuario, detalle);
        }

        private static bool ContabilidadUnidadInvalidas(int contabilidad, string unidad)
        {
            return contabilidad <= 0 || string.IsNullOrWhiteSpace(unidad);
        }

        private static CntXBalancesLoadResultadoDto MapResultado(dynamic row)
        {
            var data = RowToDictionary(row);

            return new CntXBalancesLoadResultadoDto
            {
                cuenta = GetString(data, "cuenta", "COD_CUENTA", "Cod_Cuenta"),
                consolidadora = GetString(data, "consolidadora", "CTA_CONSOLIDA", "CUENTA_CONSOLIDA", "Cuenta_Map"),
                descripcion = GetString(data, "descripcion", "DESCRIPCION", "Descripcion"),
                saldo_inicial = GetDecimal(data, "saldo_inicial", "SALDO_INICIAL"),
                debitos = GetDecimal(data, "debitos", "DEBITOS", "TOTAL_DEBITOS"),
                creditos = GetDecimal(data, "creditos", "CREDITOS", "TOTAL_CREDITOS"),
                saldo_final = GetDecimal(data, "saldo_final", "SALDO_FINAL"),
                validacion = GetString(data, "validacion", "VALIDACION"),
                divisa = GetString(data, "divisa", "DIVISA", "COD_DIVISA"),
                tc = GetDecimal(data, "tc", "TC", "TIPO_CAMBIO"),
                cta_excluye = GetString(data, "cta_excluye", "CTA_EXCLUYE", "COD_CUENTA_EXCLUYE")
            };
        }

        private static IDictionary<string, object?> RowToDictionary(dynamic row)
        {
            if (row is IDictionary<string, object?> dictNullable)
            {
                return dictNullable;
            }

            if (row is IDictionary<string, object> dict)
            {
                var result = new Dictionary<string, object?>();

                foreach (var item in dict)
                {
                    result[item.Key] = item.Value;
                }

                return result;
            }

            return new Dictionary<string, object?>();
        }

        private static string GetString(IDictionary<string, object?> data, params string[] keys)
        {
            var value = GetValue(data, keys);
            return value?.ToString()?.Trim() ?? string.Empty;
        }

        private static decimal GetDecimal(IDictionary<string, object?> data, params string[] keys)
        {
            var value = GetValue(data, keys);

            if (value is null || value == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToDecimal(value);
        }

        private static object? GetValue(IDictionary<string, object?> data, params string[] keys)
        {
            return data
                .Where(item => keys.Any(key => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)))
                .Select(item => item.Value)
                .FirstOrDefault();
        }

        private static ErrorDto<T> CrearError<T>(string description, int code, T result)
            => DbHelper.CreateErrorResponse(description, code, result);

        private static ErrorDto<T> CrearErrorContabilidadUnidad<T>(T result)
            => CrearError(MsgContabilidadUnidadRequeridas, -2, result);

        private static ErrorDto<T> CrearErrorDb<T>(DbException ex, T result)
            => CrearError(ex.Message, -1, result);
    }
}
