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
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly int vModulo = 20;

        public FrmCntXBalancesLoadDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la informacion base de la pantalla: periodo, parametros de consolidacion y unidades activas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
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
                return DbHelper.CreateErrorResponse(
                    unidadesResp.Description ?? "No fue posible cargar las unidades.",
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
                return DbHelper.CreateErrorResponse(
                    contaResp.Description ?? "No fue posible obtener la información de consolidación.",
                    contaResp.Code ?? -1,
                    new CntXBalancesLoadPantallaDto());
            }

            if ((contaResp.Result?.consolida_ind ?? 0) != 1)
            {
                return DbHelper.CreateErrorResponse(
                    "Esta Contabilidad no es Consolidadora!",
                    -2,
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
        /// Lista los historicos disponibles para una unidad y periodo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Balances_Load_Historico_Listar(
            int codEmpresa,
            CntXBalancesLoadHistoricoListarRequestDto request)
        {
            if (request.contabilidad <= 0 || string.IsNullOrWhiteSpace(request.unidad))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    "Debe indicar la contabilidad y la unidad.",
                    -2,
                    new List<DropDownListaGenericaModel>());
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

                var result = new List<DropDownListaGenericaModel>();

                foreach (var row in rows)
                {
                    var data = RowToDictionary(row);

                    var item = GetString(data, "item", "IdX", "IDX", "historico_id", "id", "codigo");
                    var descripcion = GetString(data, "descripcion", "ItmX", "ITMX", "detalle", "texto", "nombre");

                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        result.Add(new DropDownListaGenericaModel
                        {
                            item = item,
                            descripcion = descripcion
                        });
                    }
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Consulta el detalle de un histórico seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="historicoId"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXBalancesLoadResultadoDto>> CntX_Balances_Load_Historico_Consultar(
            int codEmpresa,
            int historicoId)
        {
            if (historicoId <= 0)
            {
                return DbHelper.CreateErrorResponse<List<CntXBalancesLoadResultadoDto>>(
                    "Debe indicar un histórico válido.",
                    -2,
                    new List<CntXBalancesLoadResultadoDto>());
            }

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);

                var rows = conn.Query(
                    @"exec spCntX_Balance_Cargado_Historico_Consulta @HistoricoId",
                    new
                    {
                        HistoricoId = historicoId
                    }).AsList();

                var result = new List<CntXBalancesLoadResultadoDto>();

                foreach (var row in rows)
                {
                    result.Add(MapResultado(row));
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CntXBalancesLoadResultadoDto>>(
                    ex.Message,
                    -1,
                    new List<CntXBalancesLoadResultadoDto>());
            }
        }

        /// <summary>
        /// Carga el archivo de balance ya leído desde el frontend, ejecuta mapeo y devuelve resultados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXBalancesLoadResultadoDto>> CntX_Balances_Load_Archivo_Cargar(
            int codEmpresa,
            CntXBalancesLoadArchivoCargarRequestDto request)
        {
            if (request.contabilidad <= 0 || string.IsNullOrWhiteSpace(request.unidad))
            {
                return DbHelper.CreateErrorResponse<List<CntXBalancesLoadResultadoDto>>(
                    "Debe indicar la contabilidad y la unidad.",
                    -2,
                    new List<CntXBalancesLoadResultadoDto>());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse<List<CntXBalancesLoadResultadoDto>>(
                    "Debe indicar el usuario.",
                    -2,
                    new List<CntXBalancesLoadResultadoDto>());
            }

            if (request.lineas is null || request.lineas.Count == 0)
            {
                return DbHelper.CreateErrorResponse<List<CntXBalancesLoadResultadoDto>>(
                    "Debe enviar líneas para procesar.",
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

                var rows = conn.Query(
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
                    }).AsList();

                var result = new List<CntXBalancesLoadResultadoDto>();

                foreach (var row in rows)
                {
                    result.Add(MapResultado(row));
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<CntXBalancesLoadResultadoDto>>(
                    ex.Message,
                    -1,
                    new List<CntXBalancesLoadResultadoDto>());
            }
        }

        /// <summary>
        /// Valida e importa el balance consolidado para la unidad indicada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_Importar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
        {
            if (request.contabilidad <= 0 || string.IsNullOrWhiteSpace(request.unidad))
            {
                return DbHelper.CreateErrorResponse<CntXBalancesLoadProcesoResultDto?>(
                    "Debe indicar la contabilidad y la unidad.",
                    -2,
                    null);
            }

            var validaResp = DbHelper.ExecuteSingleQuery<CntXBalancesLoadValidaDto?>(
                _portalDb,
                codEmpresa,
                @"exec spCntX_Consolida_Balance_Importa_Valida
                    @Consolidadora,
                    @Unidad,
                    @Anio,
                    @Mes,
                    @Usuario",
                null,
                new
                {
                    Consolidadora = request.contabilidad,
                    Unidad = request.unidad.Trim(),
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario.Trim()
                });

            if (validaResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CntXBalancesLoadProcesoResultDto?>(
                    validaResp.Description ?? "No fue posible validar la importación.",
                    validaResp.Code ?? -1,
                    null);
            }

            if ((validaResp.Result?.casos_erroneos ?? 0) > 0)
            {
                return DbHelper.CreateErrorResponse<CntXBalancesLoadProcesoResultDto?>(
                    $"Existen {validaResp.Result!.casos_erroneos} líneas erróneas, verifíquelas primero antes de importarlas.",
                    -2,
                    null);
            }

            var resp = DbHelper.ExecuteSingleQuery<CntXBalancesLoadProcesoResultDto?>(
                _portalDb,
                codEmpresa,
                @"exec spCntX_Consolida_Balance_Importa
                    @Consolidadora,
                    @Unidad,
                    @Anio,
                    @Mes,
                    @Usuario",
                null,
                new
                {
                    Consolidadora = request.contabilidad,
                    Unidad = request.unidad.Trim(),
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario.Trim()
                });

            if (resp.Code == 0 && resp.Result?.pass == 1)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    $"Importación del Balance de la Contabilidad Id: [{request.contabilidad}] {FxCntXPeriodoDesc(request.mes, request.anio)} Unidad: {request.unidad}");
            }

            return resp;
        }

        /// <summary>
        /// Inicializa el balance de una unidad para el período indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_Inicializar(
            int codEmpresa,
            CntXBalancesLoadProcesoRequestDto request)
        {
            if (request.contabilidad <= 0 || string.IsNullOrWhiteSpace(request.unidad))
            {
                return DbHelper.CreateErrorResponse<CntXBalancesLoadProcesoResultDto?>(
                    "Debe indicar la contabilidad y la unidad.",
                    -2,
                    null);
            }

            var resp = DbHelper.ExecuteSingleQuery<CntXBalancesLoadProcesoResultDto?>(
                _portalDb,
                codEmpresa,
                @"exec spCntX_Consolida_Balance_Inicializa
                    @Consolidadora,
                    @Unidad,
                    @Anio,
                    @Mes,
                    @Usuario",
                null,
                new
                {
                    Consolidadora = request.contabilidad,
                    Unidad = request.unidad.Trim(),
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario.Trim()
                });

            if (resp.Code == 0 && resp.Result?.pass == 1)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    $"Inicialización del Balance de la Contabilidad Id: [{request.contabilidad}] {FxCntXPeriodoDesc(request.mes, request.anio)}, Unidad: {request.unidad}");
            }

            return resp;
        }

        /// <summary>
        /// Importa el balance directamente desde la contabilidad base para el período.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CntXBalancesLoadProcesoResultDto?> CntX_Balances_Load_ImportarContaBase(
            int codEmpresa,
            CntXBalancesLoadImportaContaBaseRequestDto request)
        {
            if (request.contabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse<CntXBalancesLoadProcesoResultDto?>(
                    "Debe indicar una contabilidad válida.",
                    -2,
                    null);
            }

            var resp = DbHelper.ExecuteSingleQuery<CntXBalancesLoadProcesoResultDto?>(
                _portalDb,
                codEmpresa,
                @"exec spCntX_Consolida_Importa_Conta_Base
                    @Consolidadora,
                    @Usuario,
                    @Anio,
                    @Mes",
                null,
                new
                {
                    Consolidadora = request.contabilidad,
                    Usuario = request.usuario.Trim(),
                    Anio = request.anio,
                    Mes = request.mes
                });

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
            foreach (var key in keys)
            {
                foreach (var item in data)
                {
                    if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.Value?.ToString()?.Trim() ?? string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        private static decimal GetDecimal(IDictionary<string, object?> data, params string[] keys)
        {
            foreach (var key in keys)
            {
                foreach (var item in data)
                {
                    if (string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (item.Value is null || item.Value == DBNull.Value)
                        {
                            return 0;
                        }

                        return Convert.ToDecimal(item.Value);
                    }
                }
            }

            return 0;
        }
    }
}