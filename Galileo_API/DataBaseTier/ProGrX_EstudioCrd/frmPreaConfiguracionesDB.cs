using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaConfiguracionesDB
    {
        private const int ModuloEstudioCredito = 3;

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const string REGISTRA_WEB = "Registra - WEB";
        private const string SOLICITUD_REQUERIDA = "La solicitud es requerida.";
        private const string DESCRIPCION = "DESCRIPCION";
        private const string DESCRIPCION_MINUS = "descripcion";
        public FrmPreaConfiguracionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene la lista paginada de montos máximos por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesComiteMaxListaResult> CR_Prea_Configuraciones_ComiteMax_Lista_Obtener(int CodEmpresa, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesComiteMaxListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaParametrosComite",
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapComiteMaxRow)
                    .ToList();

                var filtrada = ApplyFiltroComiteMax(rows, filtros);
                var ordenada = ApplySortComiteMax(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteMaxListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteMaxListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteMaxListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteMaxListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de montos máximos por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesComiteMaxListaResult> CR_Prea_Configuraciones_ComiteMax_Lista_Export(int CodEmpresa, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_ComiteMax_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda los montos máximos por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_ComiteMax_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesComiteMaxGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateComiteMaxRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaParametroComite",
                    new
                    {
                        Id = request.id_comite,
                        MontoMaxAhorro = request.monto_max_ahorro,
                        MontoMaxPagare = request.monto_max_pagare,
                        MontoMaxHipotecario = request.monto_max_hipotecario,
                        MontoMaxPrendario = request.monto_max_prendario,
                        MontoMaxFiduciario = request.monto_max_fiduciario
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = string.Concat(
                        "Config: Comité [", request.id_comite, "] Máximos Garantía > ",
                        "[Ah: ", (request.monto_max_ahorro ?? 0m).ToString("N2"), "] ",
                        "[Pag: ", (request.monto_max_pagare ?? 0m).ToString("N2"), "] ",
                        "[Hip: ", (request.monto_max_hipotecario ?? 0m).ToString("N2"), "] ",
                        "[Pren: ", (request.monto_max_prendario ?? 0m).ToString("N2"), "] ",
                        "[Fidu: ", (request.monto_max_fiduciario ?? 0m).ToString("N2"), "]"
                    ),
                    Movimiento = "Modifica - WEB",
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse("Montos Máximos por Garantías, actualizados satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de líneas que validan monto máximo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigoLinea"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesComiteLineasListaResult> CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener(int CodEmpresa, string codigoLinea, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesComiteLineasListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);
                var codigo = NormalizeCodigo(codigoLinea, 4);
                var filtro = string.IsNullOrWhiteSpace(codigo) ? "N" : "S";

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaConsultaParametrosValidaMaximoP",
                        new
                        {
                            Linea = codigo,
                            Filtro = filtro
                        },
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapComiteLineasRow)
                    .ToList();

                var filtrada = ApplyFiltroComiteLineas(rows, filtros);
                var ordenada = ApplySortComiteLineas(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteLineasListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteLineasListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteLineasListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteLineasListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de líneas que validan monto máximo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigoLinea"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesComiteLineasListaResult> CR_Prea_Configuraciones_ComiteLineas_Lista_Export(int CodEmpresa, string codigoLinea, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener(
                CodEmpresa,
                codigoLinea,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda el indicador de validación de monto máximo por línea.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_ComiteLineas_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesComiteLineasGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateComiteLineasRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaParametroComiteValidaMaximo",
                    new
                    {
                        Codigo = request.codigo.Trim().ToUpperInvariant(),
                        Indicador = request.ind_monto_max
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = string.Concat(
                        "Config: Líneas [", request.codigo.Trim().ToUpperInvariant(),
                        "] Valida Monto Máximo [", (request.ind_monto_max == true ? "Sí" : "No"), "]"),
                    Movimiento = REGISTRA_WEB,
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse("Validación de monto máximo actualizada satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Obtiene el dropdown/F4 de líneas de crédito para comité líneas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPreaConfiguracionesLineaDropdownDto>> CR_Prea_Configuraciones_ComiteLineas_Dropdown_Obtener(int CodEmpresa, string filtro)
        {
            var result = new List<CrPreaConfiguracionesLineaDropdownDto>();

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var filtroNorm = (filtro ?? string.Empty).Trim();

                const string sql = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo
                where poliza = 'N'
                  and retencion = 'N'
                  and (
                      @filtro = '' or
                      codigo like '%' + @filtro + '%' or
                      descripcion like '%' + @filtro + '%'
                  )
                order by descripcion asc;";

                result = conn.Query<CrPreaConfiguracionesLineaDropdownDto>(sql, new { filtro = filtroNorm }).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CrPreaConfiguracionesLineaDropdownDto>>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de obligatoriedad de adjuntos por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesComiteAdjuntosListaResult> CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener(int CodEmpresa, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesComiteAdjuntosListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaComitesParametrizacionAdj",
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapComiteAdjuntosRow)
                    .ToList();

                var filtrada = ApplyFiltroComiteAdjuntos(rows, filtros);
                var ordenada = ApplySortComiteAdjuntos(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteAdjuntosListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteAdjuntosListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteAdjuntosListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesComiteAdjuntosListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de obligatoriedad de adjuntos por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesComiteAdjuntosListaResult> CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Export(int CodEmpresa, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda el indicador de adjunto obligatorio por comité.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_ComiteAdjuntos_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesComiteAdjuntosGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateComiteAdjuntosRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaParametroComiteAdjuntoOblig",
                    new
                    {
                        Id = request.id_comite,
                        IndObligatorio = request.adjunto_obligatorio
                    },
                    commandType: CommandType.StoredProcedure);

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuarioNorm,
                        DetalleMovimiento = string.Concat(
                            "Config: Comité [", request.id_comite,
                            "] Obligatorio Adjuntos [", (request.adjunto_obligatorio == true ? "Sí" : "No"), "]"),
                        Movimiento = REGISTRA_WEB,
                        Modulo = ModuloEstudioCredito
                    });

                return DbHelper.OkResponse("Obligatoriedad de adjuntos actualizada satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de liquidez mínima por garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesGarantiaLiquidezListaResult> CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener(int CodEmpresa, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesGarantiaLiquidezListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaParametrosGarantiaLiquido",
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapGarantiaLiquidezRow)
                    .ToList();

                var filtrada = ApplyFiltroGarantiaLiquidez(rows, filtros);
                var ordenada = ApplySortGarantiaLiquidez(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaLiquidezListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaLiquidezListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaLiquidezListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaLiquidezListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de liquidez mínima por garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesGarantiaLiquidezListaResult> CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Export(int CodEmpresa, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda la liquidez mínima por garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_GarantiaLiquidez_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesGarantiaLiquidezGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateGarantiaLiquidezRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaParametroGarantiaLiquido",
                    new
                    {
                        CodGarantia = request.garantia.Trim().ToUpperInvariant(),
                        Monto = request.monto_liquidez_minima
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = string.Concat(
                        "Config: Garantía [", request.garantia.Trim().ToUpperInvariant(),
                        "] % Liquidez Mínima: ", (request.monto_liquidez_minima ?? 0m).ToString("N2"), "%"),
                    Movimiento = REGISTRA_WEB,
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse("Porcentaje de Liquidez Mínima por Garantía, actualizado satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de garantías que refunden.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesGarantiaRefundeListaResult> CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener(int CodEmpresa, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesGarantiaRefundeListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaGarantiasRefunde",
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapGarantiaRefundeRow)
                    .ToList();

                var filtrada = ApplyFiltroGarantiaRefunde(rows, filtros);
                var ordenada = ApplySortGarantiaRefunde(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaRefundeListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaRefundeListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaRefundeListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesGarantiaRefundeListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de garantías que refunden.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesGarantiaRefundeListaResult> CR_Prea_Configuraciones_GarantiaRefunde_Lista_Export(int CodEmpresa, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda la configuración de refunde por garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_GarantiaRefunde_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesGarantiaRefundeGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateGarantiaRefundeRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaGarantiaRefunde",
                    new
                    {
                        GARANTIA = request.garantia.Trim().ToUpperInvariant(),
                        REFUNDE_AHORRO = request.refunde_ahorro,
                        REFUNDE_PRENDARIO = request.refunde_prendario,
                        REFUNDE_HIPOTECARIO = request.refunde_hipotecario,
                        REFUNDE_FIDUCIARIO = request.refunde_fiduciario,
                        REFUNDE_PAGARE = request.refunde_pagare,
                        REFUNDE_EXCEDENTE = request.refunde_excedente
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = string.Concat(
                        "Config: Garantía [", request.garantia.Trim().ToUpperInvariant(),
                        "] Refunde > [Ahorro: ", (request.refunde_ahorro == true ? "Sí" : "No"),
                        "] [Prendario: ", (request.refunde_prendario == true ? "Sí" : "No"),
                        "] [Hipotecario: ", (request.refunde_hipotecario == true ? "Sí" : "No"),
                        "] [Fiduciario: ", (request.refunde_fiduciario == true ? "Sí" : "No"),
                        "] [Pagaré: ", (request.refunde_pagare == true ? "Sí" : "No"),
                        "] [Excedente: ", (request.refunde_excedente == true ? "Sí" : "No"), "]"),
                    Movimiento = REGISTRA_WEB,
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse("Configuración de refunde por garantía actualizada satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de motivos de cambio de estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesCambioEstadoListaResult> CR_Prea_Configuraciones_CambioEstado_Lista_Obtener(int CodEmpresa, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesCambioEstadoListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaMotivosCambioEstado",
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapCambioEstadoRow)
                    .ToList();

                var filtrada = ApplyFiltroCambioEstado(rows, filtros);
                var ordenada = ApplySortCambioEstado(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesCambioEstadoListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesCambioEstadoListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesCambioEstadoListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesCambioEstadoListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de motivos de cambio de estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesCambioEstadoListaResult> CR_Prea_Configuraciones_CambioEstado_Lista_Export(int CodEmpresa, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_CambioEstado_Lista_Obtener(
                CodEmpresa,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda un motivo de cambio de estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_CambioEstado_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesCambioEstadoGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateCambioEstadoRequest(request);
                var tipoMov = request.id_motivo <= 0 ? "R" : "M";

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaParametroMotivoCambioEstado",
                    new
                    {
                        IdMotivo = request.id_motivo,
                        Motivo = request.motivo.Trim(),
                        Estado = request.estado,
                        Usuario = usuarioNorm,
                        TipoMov = tipoMov
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = string.Concat(
                        "Config: Cambio Estado [Id: ", request.id_motivo,
                        "...", request.motivo.Trim(),
                        "] Activo: [", (request.estado == true ? "Sí" : "No"), "]"),
                    Movimiento = request.id_motivo <= 0 ? REGISTRA_WEB : "Modifica - WEB",
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse(
                    request.id_motivo <= 0
                        ? "Motivo de cambio de estado registrado satisfactoriamente!"
                        : "Motivo de cambio de estado actualizado satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de configuración de edad pensión por línea.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigoLinea"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesEdadPensionListaResult> CR_Prea_Configuraciones_EdadPension_Lista_Obtener(int CodEmpresa, string codigoLinea, string filtrosJson)
        {
            var result = new CrPreaConfiguracionesEdadPensionListaResult();

            try
            {
                var filtros = DeserializeFiltros(filtrosJson);
                var codigo = NormalizeCodigo(codigoLinea, 4);
                var filtro = string.IsNullOrWhiteSpace(codigo) ? "N" : "S";

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var rows = conn.Query(
                        "spCrdPreaListaLineasConfigEdadPension",
                        new
                        {
                            Linea = codigo,
                            Filtro = filtro
                        },
                        commandType: CommandType.StoredProcedure)
                    .Cast<IDictionary<string, object?>>()
                    .Select(MapEdadPensionRow)
                    .ToList();

                var filtrada = ApplyFiltroEdadPension(rows, filtros);
                var ordenada = ApplySortEdadPension(filtrada, filtros).ToList();

                result.total = ordenada.Count;
                result.lista = ApplyPaging(ordenada, filtros).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesEdadPensionListaResult>(ex.Message, -1, result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesEdadPensionListaResult>(ex.Message, -1, result);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesEdadPensionListaResult>(ex.Message, -1, result);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.CreateErrorResponse<CrPreaConfiguracionesEdadPensionListaResult>(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Obtiene la lista completa de configuración de edad pensión por línea.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigoLinea"></param>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        public ErrorDto<CrPreaConfiguracionesEdadPensionListaResult> CR_Prea_Configuraciones_EdadPension_Lista_Export(int CodEmpresa, string codigoLinea, string filtrosJson)
        {
            var filtros = DeserializeFiltros(filtrosJson);
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_Prea_Configuraciones_EdadPension_Lista_Obtener(
                CodEmpresa,
                codigoLinea,
                JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda la configuración de edad pensión por línea.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Prea_Configuraciones_EdadPension_Guardar(int CodEmpresa, string usuario, CrPreaConfiguracionesEdadPensionGuardarRequest request)
        {
            try
            {
                if (request == null)
                {
                    return DbHelper.ErrorResponse(SOLICITUD_REQUERIDA);
                }

                var usuarioNorm = NormalizeUsuario(usuario);
                ValidateEdadPensionRequest(request);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                conn.Execute(
                    "spCrdPreaGuardaParametroEdadPension",
                    new
                    {
                        Codigo = request.codigo.Trim().ToUpperInvariant(),
                        Garantias = NormalizeNullableText(request.garantias),
                        Comites = NormalizeNullableText(request.comites),
                        IndicadorEstudio = request.ind_edad_pension,
                        IndicadorFormaliza = request.ind_edad_pension_for,
                        Usuario = usuarioNorm
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = string.Concat(
                        "Config: Edad Pensión [Id: ", request.codigo.Trim().ToUpperInvariant(),
                        "] Aplica: [Estudio: ", (request.ind_edad_pension == true ? "Sí" : "No"),
                        ", Formaliza: ", (request.ind_edad_pension_for == true ? "Sí" : "No"),
                        "] Garantías: ", NormalizeNullableText(request.garantias),
                        ", Comités: ", NormalizeNullableText(request.comites)),
                    Movimiento = "Modifica - WEB",
                    Modulo = ModuloEstudioCredito
                });

                return DbHelper.OkResponse("Configuración de edad pensión actualizada satisfactoriamente!");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
            catch (ArgumentException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        /// <summary>
        /// Deserializa el JSON de filtros lazy.
        /// </summary>
        /// <param name="filtrosJson"></param>
        /// <returns></returns>
        private static FiltrosLazyLoadData DeserializeFiltros(string filtrosJson)
        {
            if (string.IsNullOrWhiteSpace(filtrosJson))
            {
                return new FiltrosLazyLoadData();
            }

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtrosJson) ?? new FiltrosLazyLoadData();
        }

        /// <summary>
        /// Aplica la paginación lazy a la lista ya filtrada y ordenada.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<T> ApplyPaging<T>(IEnumerable<T> rows, FiltrosLazyLoadData filtros)
        {
            var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            var paginacion = filtros.paginacion < 0 ? 0 : filtros.paginacion;

            if (paginacion == 0)
            {
                return rows;
            }

            var skip = pagina * paginacion;
            return rows.Skip(skip).Take(paginacion);
        }

        /// <summary>
        /// Normaliza el usuario de sesión.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static string NormalizeUsuario(string usuario)
        {
            var usuarioNorm = (usuario ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(usuarioNorm))
            {
                throw new InvalidOperationException("El usuario es requerido.");
            }

            return usuarioNorm;
        }

        /// <summary>
        /// Normaliza un código opcional a la longitud esperada.
        /// </summary>
        /// <param name="codigo"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private static string NormalizeCodigo(string codigo, int maxLength)
        {
            var value = (codigo ?? string.Empty).Trim().ToUpperInvariant();
            return value.Length > maxLength ? value[..maxLength] : value;
        }

        /// <summary>
        /// Normaliza texto nullable a string no nulo.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string NormalizeNullableText(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        /// <summary>
        /// Convierte una fila dinámica en comité máximo.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesComiteMaxListaData MapComiteMaxRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesComiteMaxListaData
            {
                id_comite = GetInt(row, "ID_COMITE"),
                comite = GetString(row, "COMITE"),
                monto_max_ahorro = GetDecimal(row, "MONTO_MAX_AHORRO"),
                monto_max_pagare = GetDecimal(row, "MONTO_MAX_PAGARE"),
                monto_max_hipotecario = GetDecimal(row, "MONTO_MAX_HIPOTECARIO"),
                monto_max_prendario = GetDecimal(row, "MONTO_MAX_PRENDARIO"),
                monto_max_fiduciario = GetDecimal(row, "MONTO_MAX_FIDUCIARIO")
            };
        }

        /// <summary>
        /// Convierte una fila dinámica en comité líneas.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesComiteLineasListaData MapComiteLineasRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesComiteLineasListaData
            {
                codigo = GetString(row, "CODIGO"),
                descripcion = GetString(row, DESCRIPCION),
                ind_monto_max = GetBool(row, "IND_MONTO_MAX")
            };
        }

        /// <summary>
        /// Convierte una fila dinámica en comité adjuntos.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesComiteAdjuntosListaData MapComiteAdjuntosRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesComiteAdjuntosListaData
            {
                id_comite = GetInt(row, "ID_COMITE"),
                descripcion = GetString(row, DESCRIPCION),
                adjunto_obligatorio = GetBool(row, "ADJUNTO_OBLIGATORIO")
            };
        }

        /// <summary>
        /// Convierte una fila dinámica en garantía liquidez.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesGarantiaLiquidezListaData MapGarantiaLiquidezRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesGarantiaLiquidezListaData
            {
                garantia = GetString(row, "GARANTIA"),
                descripcion = GetString(row, DESCRIPCION),
                monto_liquidez_minima = GetDecimal(row, "MONTO_LIQUIDEZ_MINIMA", "MONTO_LIQUIDEZ_MIN")
            };
        }

        /// <summary>
        /// Convierte una fila dinámica en garantía refunde.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesGarantiaRefundeListaData MapGarantiaRefundeRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesGarantiaRefundeListaData
            {
                garantia = GetString(row, "GARANTIA"),
                descripcion = GetString(row, DESCRIPCION),
                refunde_ahorro = GetBool(row, "REFUNDE_AHORRO"),
                refunde_prendario = GetBool(row, "REFUNDE_PRENDARIO"),
                refunde_hipotecario = GetBool(row, "REFUNDE_HIPOTECARIO"),
                refunde_fiduciario = GetBool(row, "REFUNDE_FIDUCIARIO"),
                refunde_pagare = GetBool(row, "REFUNDE_PAGARE"),
                refunde_excedente = GetBool(row, "REFUNDE_EXCEDENTE")
            };
        }

        /// <summary>
        /// Convierte una fila dinámica en cambio de estado.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesCambioEstadoListaData MapCambioEstadoRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesCambioEstadoListaData
            {
                id_motivo = GetInt(row, "ID_MOTIVO"),
                motivo = GetString(row, "MOTIVO"),
                estado = GetBool(row, "ESTADO"),
                fec_registro = GetDate(row, "FEC_REGISTRO"),
                usu_registro = GetString(row, "USU_REGISTRO"),
                fec_modifica = GetDate(row, "FEC_MODIFICA"),
                usu_modifica = GetString(row, "USU_MODIFICA"),
                isNew = false
            };
        }

        /// <summary>
        /// Convierte una fila dinámica en edad pensión.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CrPreaConfiguracionesEdadPensionListaData MapEdadPensionRow(IDictionary<string, object?> row)
        {
            return new CrPreaConfiguracionesEdadPensionListaData
            {
                codigo = GetString(row, "CODIGO"),
                descripcion = GetString(row, DESCRIPCION),
                ind_edad_pension = GetBool(row, "IND_EDAD_PENSION"),
                ind_edad_pension_for = GetBool(row, "IND_EDAD_PENSION_FOR"),
                garantias = GetString(row, "GARANTIAS"),
                comites = GetString(row, "COMITES")
            };
        }

        /// <summary>
        /// Aplica el filtro global para comité máximo.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesComiteMaxListaData> ApplyFiltroComiteMax(IEnumerable<CrPreaConfiguracionesComiteMaxListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.id_comite.ToString().Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.comite.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.monto_max_ahorro.ToString("N2").Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.monto_max_pagare.ToString("N2").Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.monto_max_hipotecario.ToString("N2").Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.monto_max_prendario.ToString("N2").Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.monto_max_fiduciario.ToString("N2").Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para comité máximo.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesComiteMaxListaData> ApplySortComiteMax(IEnumerable<CrPreaConfiguracionesComiteMaxListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesComiteMaxListaData, object?> keySelector = sortField switch
            {
                "comite" => x => x.comite,
                "monto_max_ahorro" => x => x.monto_max_ahorro,
                "monto_max_pagare" => x => x.monto_max_pagare,
                "monto_max_hipotecario" => x => x.monto_max_hipotecario,
                "monto_max_prendario" => x => x.monto_max_prendario,
                "monto_max_fiduciario" => x => x.monto_max_fiduciario,
                _ => x => x.id_comite
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.id_comite)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.id_comite);
        }

        /// <summary>
        /// Aplica el filtro global para comité líneas.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesComiteLineasListaData> ApplyFiltroComiteLineas(IEnumerable<CrPreaConfiguracionesComiteLineasListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.codigo.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                (x.ind_monto_max ? "SI" : "NO").Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para comité líneas.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesComiteLineasListaData> ApplySortComiteLineas(IEnumerable<CrPreaConfiguracionesComiteLineasListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesComiteLineasListaData, object?> keySelector = sortField switch
            {
                 DESCRIPCION_MINUS => x => x.descripcion,
                "ind_monto_max" => x => x.ind_monto_max,
                _ => x => x.codigo
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.codigo)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.codigo);
        }

        /// <summary>
        /// Aplica el filtro global para comité adjuntos.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesComiteAdjuntosListaData> ApplyFiltroComiteAdjuntos(IEnumerable<CrPreaConfiguracionesComiteAdjuntosListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.id_comite.ToString().Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                (x.adjunto_obligatorio ? "SI" : "NO").Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para comité adjuntos.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesComiteAdjuntosListaData> ApplySortComiteAdjuntos(IEnumerable<CrPreaConfiguracionesComiteAdjuntosListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesComiteAdjuntosListaData, object?> keySelector = sortField switch
            {
                DESCRIPCION_MINUS => x => x.descripcion,
                "adjunto_obligatorio" => x => x.adjunto_obligatorio,
                _ => x => x.id_comite
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.id_comite)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.id_comite);
        }

        /// <summary>
        /// Aplica el filtro global para garantía liquidez.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesGarantiaLiquidezListaData> ApplyFiltroGarantiaLiquidez(IEnumerable<CrPreaConfiguracionesGarantiaLiquidezListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.garantia.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.monto_liquidez_minima.ToString("N2").Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para garantía liquidez.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesGarantiaLiquidezListaData> ApplySortGarantiaLiquidez(IEnumerable<CrPreaConfiguracionesGarantiaLiquidezListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesGarantiaLiquidezListaData, object?> keySelector = sortField switch
            {
                DESCRIPCION_MINUS => x => x.descripcion,
                "monto_liquidez_minima" => x => x.monto_liquidez_minima,
                _ => x => x.garantia
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.garantia)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.garantia);
        }

        /// <summary>
        /// Aplica el filtro global para garantía refunde.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesGarantiaRefundeListaData> ApplyFiltroGarantiaRefunde(IEnumerable<CrPreaConfiguracionesGarantiaRefundeListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.garantia.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para garantía refunde.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesGarantiaRefundeListaData> ApplySortGarantiaRefunde(IEnumerable<CrPreaConfiguracionesGarantiaRefundeListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesGarantiaRefundeListaData, object?> keySelector = sortField switch
            {
                DESCRIPCION_MINUS => x => x.descripcion,
                _ => x => x.garantia
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.garantia)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.garantia);
        }

        /// <summary>
        /// Aplica el filtro global para cambio de estado.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesCambioEstadoListaData> ApplyFiltroCambioEstado(IEnumerable<CrPreaConfiguracionesCambioEstadoListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.id_motivo.ToString().Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.motivo.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.usu_registro.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.usu_modifica.Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para cambio de estado.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesCambioEstadoListaData> ApplySortCambioEstado(IEnumerable<CrPreaConfiguracionesCambioEstadoListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesCambioEstadoListaData, object?> keySelector = sortField switch
            {
                "motivo" => x => x.motivo,
                "estado" => x => x.estado,
                "fec_registro" => x => x.fec_registro,
                "usu_registro" => x => x.usu_registro,
                "fec_modifica" => x => x.fec_modifica,
                "usu_modifica" => x => x.usu_modifica,
                _ => x => x.id_motivo
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.id_motivo)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.id_motivo);
        }

        /// <summary>
        /// Aplica el filtro global para edad pensión.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CrPreaConfiguracionesEdadPensionListaData> ApplyFiltroEdadPension(IEnumerable<CrPreaConfiguracionesEdadPensionListaData> rows, FiltrosLazyLoadData filtros)
        {
            var filtro = (filtros.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return rows;
            }

            return rows.Where(x =>
                x.codigo.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.descripcion.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.garantias.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                x.comites.Contains(filtro, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Aplica el ordenamiento para edad pensión.
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IOrderedEnumerable<CrPreaConfiguracionesEdadPensionListaData> ApplySortEdadPension(IEnumerable<CrPreaConfiguracionesEdadPensionListaData> rows, FiltrosLazyLoadData filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim().ToLowerInvariant();
            var asc = filtros.sortOrder == 1;

            Func<CrPreaConfiguracionesEdadPensionListaData, object?> keySelector = sortField switch
            {
                DESCRIPCION_MINUS => x => x.descripcion,
                "ind_edad_pension" => x => x.ind_edad_pension,
                "ind_edad_pension_for" => x => x.ind_edad_pension_for,
                "garantias" => x => x.garantias,
                "comites" => x => x.comites,
                _ => x => x.codigo
            };

            return asc
                ? rows.OrderBy(keySelector).ThenBy(x => x.codigo)
                : rows.OrderByDescending(keySelector).ThenByDescending(x => x.codigo);
        }

        /// <summary>
        /// Valida la solicitud de montos máximos por comité.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateComiteMaxRequest(CrPreaConfiguracionesComiteMaxGuardarRequest request)
        {
            if (request.id_comite <= 0)
            {
                throw new InvalidOperationException("El comité es requerido.");
            }

            if (request.monto_max_ahorro < 0 ||
                request.monto_max_pagare < 0 ||
                request.monto_max_hipotecario < 0 ||
                request.monto_max_prendario < 0 ||
                request.monto_max_fiduciario < 0)
            {
                throw new InvalidOperationException("Los montos no pueden ser negativos.");
            }
        }

        /// <summary>
        /// Valida la solicitud de comité líneas.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateComiteLineasRequest(CrPreaConfiguracionesComiteLineasGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace((request.codigo ?? string.Empty).Trim()))
            {
                throw new InvalidOperationException("La línea es requerida.");
            }
        }

        /// <summary>
        /// Valida la solicitud de comité adjuntos.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateComiteAdjuntosRequest(CrPreaConfiguracionesComiteAdjuntosGuardarRequest request)
        {
            if (request.id_comite <= 0)
            {
                throw new InvalidOperationException("El comité es requerido.");
            }
        }

        /// <summary>
        /// Valida la solicitud de garantía liquidez.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateGarantiaLiquidezRequest(CrPreaConfiguracionesGarantiaLiquidezGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace((request.garantia ?? string.Empty).Trim()))
            {
                throw new InvalidOperationException("La garantía es requerida.");
            }

            if (request.monto_liquidez_minima < 0)
            {
                throw new InvalidOperationException("El monto de liquidez mínima no puede ser negativo.");
            }
        }

        /// <summary>
        /// Valida la solicitud de garantía refunde.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateGarantiaRefundeRequest(CrPreaConfiguracionesGarantiaRefundeGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace((request.garantia ?? string.Empty).Trim()))
            {
                throw new InvalidOperationException("La garantía es requerida.");
            }
        }

        /// <summary>
        /// Valida la solicitud de cambio de estado.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateCambioEstadoRequest(CrPreaConfiguracionesCambioEstadoGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace((request.motivo ?? string.Empty).Trim()))
            {
                throw new InvalidOperationException("El motivo es requerido.");
            }
        }

        /// <summary>
        /// Valida la solicitud de edad pensión.
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateEdadPensionRequest(CrPreaConfiguracionesEdadPensionGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace((request.codigo ?? string.Empty).Trim()))
            {
                throw new InvalidOperationException("La línea es requerida.");
            }
        }

        /// <summary>
        /// Obtiene el primer valor disponible convertido al tipo indicado según las claves enviadas.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="row"></param>
        /// <param name="converter"></param>
        /// <param name="defaultValue"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static T GetValueOrDefault<T>(
            IDictionary<string, object?> row,
            Func<object?, T> converter,
            T defaultValue,
            params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            if (value == null)
            {
                return defaultValue;
            }

            return converter(value);
        }

        /// <summary>
        /// Obtiene el primer valor string disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static string GetString(IDictionary<string, object?> row, params string[] keys)
        {
            return GetValueOrDefault(
                row,
                value => Convert.ToString(value)?.Trim() ?? string.Empty,
                string.Empty,
                keys);
        }

        /// <summary>
        /// Obtiene el primer valor entero disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static int GetInt(IDictionary<string, object?> row, params string[] keys)
        {
            var text = GetString(row, keys);
            return int.TryParse(text, out var value) ? value : 0;
        }

        /// <summary>
        /// Obtiene el primer valor decimal disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static decimal GetDecimal(IDictionary<string, object?> row, params string[] keys)
        {
            return GetValueOrDefault(
                row,
                value => Convert.ToDecimal(value),
                0m,
                keys);
        }

        /// <summary>
        /// Obtiene el primer valor fecha disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static DateTime? GetDate(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            if (value == null)
            {
                return null;
            }

            return Convert.ToDateTime(value);
        }

        /// <summary>
        /// Obtiene el primer valor booleano disponible según las claves indicadas.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        private static bool GetBool(IDictionary<string, object?> row, params string[] keys)
        {
            var value = keys
                .Select(key => row.TryGetValue(key, out var v) ? v : null)
                .FirstOrDefault(v => v != null && v != DBNull.Value);

            if (value == null)
            {
                return false;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            var text = Convert.ToString(value)?.Trim() ?? string.Empty;

            return text.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                   text.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
                   text.Equals("T", StringComparison.OrdinalIgnoreCase) ||
                   text.Equals("S", StringComparison.OrdinalIgnoreCase) ||
                   text.Equals("SI", StringComparison.OrdinalIgnoreCase) ||
                   text.Equals("ACTIVO", StringComparison.OrdinalIgnoreCase);
        }
    }
}