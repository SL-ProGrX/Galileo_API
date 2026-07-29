using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.ReportingServices.Diagnostics.Internal;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public partial class FrmTesBancosCargadoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria _mTesoreria;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int _vModulo = 9;

        public FrmTesBancosCargadoDB(IConfiguration config)
        {
            _portalDB        = new PortalDB(config);
            _mTesoreria      = new MTesoreria(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la cuenta de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaBancosCargados>> Tes_Bancos_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = "exec spTes_Cuenta_Bancaria_Acceso @usuario, @TipoDoc, @Acceso";

                return conn.Query<DropDownListaBancosCargados>(query, new { @Usuario = usuario, @TipoDoc = "DP", @Acceso = "SOL" }).ToList(); 
            });
        }


        /// <summary>
        /// Metodo para obtener los conceptos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesBancoCargadoConceptos>> Tes_BancosCargadoConceptos_Obtener(int CodEmpresa, string? concepto = null)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var conceptoTrim = concepto?.Trim();
                var hasConcepto = !string.IsNullOrWhiteSpace(conceptoTrim);

                const string sql = @"
                            SELECT
                                COD_CONCEPTO,
                                DESCRIPCION,
                                COD_CUENTA_MASK,
                                DP_TRAMITE_APL,
                                CUENTA_DESC
                            FROM vTes_Conceptos
                            WHERE AUTO_REGISTRO = 1
                              AND ESTADO = 'A'
                              AND (@concepto IS NULL OR COD_CONCEPTO = @concepto);";

                var response = conn.Query<TesBancoCargadoConceptos>(
                    sql,
                    new { concepto = hasConcepto ? conceptoTrim : null }
                ).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<List<TesBancoCargadoConceptos>>(ex.Message);
            }
            
        }

        /// <summary>
        /// Metodo para obtener las unidades asociadas 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroUnidades_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select COD_UNIDAD as 'item', DESCRIPCION from vCNTX_UNIDADES_LOCAL";
                var Result = conn.Query<DropDownListaGenericaModel>(query).ToList();
                return DbHelper.CreateOkResponse(Result);
            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Metodo para obtener los centros de costos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_BancosCargadoCentroCostos_Obtener(int CodEmpresa, int contabilidad, string? unidad = null)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var unidadFiltro = unidad?.Trim();
                if (string.IsNullOrWhiteSpace(unidadFiltro))
                {
                    return DbHelper.CreateOkResponse(new List<DropDownListaGenericaModel>());
                }

                const string query = @"
                    SELECT DISTINCT
                        RTRIM(C.COD_CENTRO_COSTO) AS item,
                        RTRIM(C.DESCRIPCION) AS descripcion
                    FROM CNTX_CENTRO_COSTOS C
                    INNER JOIN CNTX_UNIDADES_CC U
                        ON C.COD_CENTRO_COSTO = U.COD_CENTRO_COSTO
                       AND C.COD_CONTABILIDAD = U.COD_CONTABILIDAD
                       AND U.COD_UNIDAD = @unidad
                    WHERE C.COD_CONTABILIDAD = @contabilidad
                    ORDER BY item;";

                var request = conn.Query<DropDownListaGenericaModel>(
                    query,
                    new { contabilidad, unidad = unidadFiltro }
                ).ToList();

                return DbHelper.CreateOkResponse(request);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }


        /// <summary>
        /// Metodo para obtener una lista de registros de auto registro de tesorer�a con paginaci�n y filtros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAutoRegistroLista> Tes_AutoRegistroLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto<TesAutoRegistroLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAutoRegistroLista()
                {
                    total = 0,
                    lista = new List<TesAutoRegistroDto>()
                }
            };
            try
            {
                var filtro = filtros?.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(filtro);

                
                var offset = (filtros?.pagina).GetValueOrDefault(0);
                var fetch = (filtros?.paginacion).GetValueOrDefault(0);
                var usarPaginacion = fetch > 0;

                var filtroLike = hasFiltro ? $"%{filtro}%" : null;

                const string sqlCount = @"
                    SELECT COUNT(1)
                    FROM vTES_AUTO_REGISTRO
                    WHERE
                        (@filtro IS NULL)
                     OR (CAST(id_auto AS NVARCHAR(50)) LIKE @filtroLike)
                     OR (descripcion LIKE @filtroLike)
                     OR (palabras_clave LIKE @filtroLike);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? filtro : null,
                    filtroLike
                });

                var sqlList = @"
                    SELECT *
                    FROM vTES_AUTO_REGISTRO
                    WHERE
                        (@filtro IS NULL)
                     OR (CAST(id_auto AS NVARCHAR(50)) LIKE @filtroLike)
                     OR (descripcion LIKE @filtroLike)
                     OR (palabras_clave LIKE @filtroLike)
                    ORDER BY id_auto ";

                if (usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }

                result.Result.lista = conn.Query<TesAutoRegistroDto>(sqlList, new
                {
                    filtro = hasFiltro ? filtro : null,
                    filtroLike,
                    offset,
                    fetch
                }).ToList();

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<TesAutoRegistroDto>();
            }
            return result;
        }


        /// <summary>
        /// Aplica el archivo de bancos cargado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_banco"></param>
        /// <param name="usuario"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ErrorDto TES_BancosCargados_Aplicar(int CodEmpresa, string cod_banco, string usuario, List<TesCargadoExcelDto> file)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = ""
            };

            try
            {
                var sb = new StringBuilder();
                foreach (var row in file)
                {

                    var query = @"EXEC spTes_Bancos_Mov_Load @IdBanco, @Fecha, @Documento, @TipoMov, @Importe,@Descripcion";

                    var result = conn.Query<int>(query, new
                    {
                        IdBanco = cod_banco,
                        Fecha = row.fecha,
                        Documento = row.documento,
                        TipoMov = row.tipo,
                        Importe = row.importe,
                        Descripcion = row.descripcion,
                    }).FirstOrDefault();

                    if (result == -1)
                    {
                        sb.AppendLine($"Documento Repetido: [{row.documento}]");
                    }
                }

                response.Description = sb.ToString();

                if (response.Description.Length > 0)
                {
                    response.Code = -1;
                }
                else
                {
                    response.Description = "Ok";
                }
                return response;
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los registros de bancos cargados pendientes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaRegistroBancos_Obtener(int CodEmpresa, string filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            TesFiltrosRegistroBancoDto filtro = JsonConvert.DeserializeObject<TesFiltrosRegistroBancoDto>(filtros) ?? new TesFiltrosRegistroBancoDto();

            try
            {

                string query = "spTes_Bancos_Mov_Consulta";

                string fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(filtro.fechaInicio, "yyyy-MM-dd" + " 00:00:00") ?? "";
                string fechaFin = MProGrXAuxiliarDB.validaFechaGlobal(filtro.fechaCorte, "yyyy-MM-dd" + " 23:59:59") ?? "";

                var parameters = new
                {
                    BancoId = filtro.cod_cuenta,
                    Documento = filtro.ndocumento,
                    Tipo = filtro.tipoMovimiento,
                    FechaTipo = filtro.base_,
                    FInicio = fechaIni,
                    FCorte = fechaFin,
                    MntInicio = filtro.montoInicio,
                    MntCorte = filtro.montoCorte,
                    Estado = filtro.estado,
                    Descripcion = filtro.descripcion
                };

                var response = conn
                    .Query<TeslistaRegistroBancosDto>(query, parameters, commandType: CommandType.StoredProcedure)
                    .ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TeslistaRegistroBancosDto>>(ex.Message);
            }
        }


        /// <summary>
        /// Aplica el registro de bancos cargados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="registroLista"></param>
        /// <returns></returns>
        public async Task<ErrorDto> TES_RegistrosBancosCargados_Aplicar(int CodEmpresa, string registroLista)
        {
            List<RegistroBancoDto> lista = JsonConvert.DeserializeObject<List<RegistroBancoDto>>(registroLista) ?? new List<RegistroBancoDto>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                string error = string.Empty;
                foreach (var item in lista)
                {

                    var parametros = new
                    {
                        LineaId = item.Linea_Id,
                        Usuario = item.Usuario,
                        AutoId = item.Auto_Id,
                        Concepto = item.Concepto,
                        Unidad = item.Unidad,
                        Centro = item.Centro,
                        Cuenta = item.Cuenta
                    };

                    var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
                                    "spTes_Bancos_Mov_Registro",
                                    parametros,
                                    commandType: CommandType.StoredProcedure);

                    if (result is null)
                    {
                        error = " - Línea: " + item.Linea_Id + " error: el proceso no devolvió resultado.";
                        continue;
                    }

                    if (result.Ok == 0)
                    {
                        error = " - Linea: " + result.LineaId + " error: " +result.Mensaje;
                    }

                }

                if (!string.IsNullOrEmpty(error))
                {
                    return DbHelper.ErrorResponse(error);
                }

                return DbHelper.OkResponse("Registros procesados correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            
        }

        public ErrorDto TES_RegistrosBancosCargados_Elimina(int CodEmpresa, string registroLista)
        {
            List<RegistroBancoDto> lista = JsonConvert.DeserializeObject<List<RegistroBancoDto>>(registroLista) ?? new List<RegistroBancoDto>();
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                foreach (var item in lista)
                {
                    var querySP = "spTes_Bancos_Mov_Elimina";
                    conn.Execute(querySP, new
                    {
                        LineaId = item.Linea_Id
                    },
                    commandType: CommandType.StoredProcedure);
                }
                return DbHelper.OkResponse("Registro procesado correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }

        }

        /// <summary>
        /// Obtiene los movimientos del banco filtrados para el tab Detalle de Movimientos,
        /// incluyendo filtros por concepto, cuenta, unidad y centro de costos via TES_TRANSACCIONES.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtro">Filtros del detalle de movimientos</param>
        /// <returns>Lista de movimientos que cumplen los criterios</returns>
        public ErrorDto<List<TeslistaRegistroBancosDto>> TES_ListaDetalleMovimientos_Obtener(int CodEmpresa, TesFiltrosDetalleMovimientoDto filtro)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string fechaIni = MProGrXAuxiliarDB.validaFechaGlobal(filtro.FechaInicio, "yyyy-MM-dd 00:00:00") ?? "";
                string fechaFin = MProGrXAuxiliarDB.validaFechaGlobal(filtro.FechaCorte,  "yyyy-MM-dd 23:59:59") ?? "";

                var parameters = new
                {
                    BancoId        = filtro.BancoId,
                    Documento      = filtro.Documento,
                    Tipo           = filtro.Tipo,
                    FechaTipo      = filtro.FechaTipo,
                    FInicio        = fechaIni,
                    FCorte         = fechaFin,
                    MntInicio      = filtro.MontoInicio,
                    MntCorte       = filtro.MontoCorte,
                    Estado         = filtro.Estado,
                    Descripcion    = filtro.Descripcion,
                    CodConcepto    = filtro.CodConcepto,
                    CodCuenta      = filtro.CodCuenta,
                    CodUnidad      = filtro.CodUnidad,
                    CodCentroCosto = filtro.CodCentroCosto
                };

                var result = conn.Query<TeslistaRegistroBancosDto>(
                    "spTes_BancosCargado_Mov_Consulta",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TeslistaRegistroBancosDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Reclasifica el COD_CONCEPTO en TES_TRANSACCIONES para una lista de solicitudes bancarias.
        /// Valida que el asiento no haya sido generado y registra la justificación en la bitácora de tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="data">Modelo con la lista de nsolicitudes, el nuevo concepto, usuario y justificación.</param>
        public ErrorDto TES_BancosCargado_ReclasificaConcepto(int CodEmpresa, TesBancosCargadoReclasificaConceptoModel data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var exitosas = 0;
                var errores  = new List<string>();

                foreach (var nsolicitud in data.Solicitudes)
                {
                    var error = ProcesarReclasificacionConcepto(conn, CodEmpresa, nsolicitud, data.CodConcepto, data.Usuario, data.Nota, data.ReemplazarAsientos);
                    if (error != null) errores.Add(error);
                    else              exitosas++;
                }

                if (exitosas == 0)
                    return DbHelper.ErrorResponse(string.Join(" | ", errores));

                var msg = exitosas == data.Solicitudes.Count
                    ? $"Concepto reclasificado correctamente en {exitosas} solicitud(es)."
                    : $"{exitosas} de {data.Solicitudes.Count} reclasificada(s). Omitidas: {string.Join(", ", errores)}";

                return DbHelper.OkResponse(msg);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Excluye líneas del Tab Detalle Movimientos, revirtiendo Saldo a Favor y Depósito Trámite si aplican.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="data">Modelo con lista de líneas y usuario.</param>
        public ErrorDto TES_BancosCargado_DetalleExcluir(int CodEmpresa, TesBancosCargadoDetalleExcluirModel data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                return ProcesarLineasEnLote(
                    conn, data.LineasId,
                    "spTes_BancosCargado_Mov_DetalleExcluir",
                    id => new { LineaId = id, Usuario = data.Usuario },
                    "excluida(s)");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Registra la transacción y asientos para líneas ya procesadas del Tab Detalle Movimientos,
        /// usando los parámetros de contabilización provistos sin lógica de auto-registro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="data">Modelo con lista de líneas e información de contabilización.</param>
        public ErrorDto TES_BancosCargado_DetalleRegistrar(int CodEmpresa, TesBancosCargadoDetalleRegistrarModel data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                return ProcesarLineasEnLote(
                    conn, data.LineasId,
                    "spTes_BancosCargado_Mov_DetalleRegistro",
                    id => new
                    {
                        LineaId  = id,
                        Usuario  = data.Usuario,
                        Concepto = data.Concepto,
                        Unidad   = data.Unidad,
                        Centro   = data.Centro,
                        Cuenta   = data.Cuenta,
                    },
                    "registrada(s)");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta un SP por cada línea de la lista y consolida el resultado en un único ErrorDto.
        /// </summary>
        /// <param name="conn">Conexión ya abierta.</param>
        /// <param name="lineasId">Lista de identificadores de línea a procesar.</param>
        /// <param name="spName">Nombre del stored procedure a ejecutar.</param>
        /// <param name="parametrosFactory">Función que construye los parámetros del SP por cada línea.</param>
        /// <param name="verboExito">Participio del verbo para el mensaje de éxito (ej. "excluida(s)").</param>
        private static ErrorDto ProcesarLineasEnLote(
            IDbConnection conn,
            List<long> lineasId,
            string spName,
            Func<long, object> parametrosFactory,
            string verboExito)
        {
            var errores  = new List<string>();
            var exitosas = 0;

            foreach (var lineaId in lineasId)
            {
                var result = conn.QueryFirstOrDefault<dynamic>(
                    spName, parametrosFactory(lineaId),
                    commandType: CommandType.StoredProcedure);

                if (result?.Ok == 1) exitosas++;
                else errores.Add($"Línea {lineaId}: {result?.Mensaje ?? "error desconocido"}");
            }

            if (exitosas == 0)
                return DbHelper.ErrorResponse(string.Join(" | ", errores));

            var msg = exitosas == lineasId.Count
                ? $"{exitosas} línea(s) {verboExito} correctamente."
                : $"{exitosas} de {lineasId.Count} procesada(s). Omitidas: {string.Join(", ", errores)}";

            return DbHelper.OkResponse(msg);
        }

        /// <summary>
        /// Procesa la reclasificación de concepto para una solicitud individual.
        /// Retorna null si fue exitoso, o el mensaje de error si no.
        /// </summary>
        private string? ProcesarReclasificacionConcepto(
            System.Data.IDbConnection conn, int CodEmpresa,
            int nsolicitud, string codConcepto, string? usuario, string? nota,
            bool reemplazarAsientos)
        {
            if (nsolicitud <= 0)
                return $"Solicitud {nsolicitud}: sin vínculo a transacción.";

            var estadoAsiento = conn.QueryFirstOrDefault<string>(
                "SELECT estado_asiento FROM Tes_Transacciones WHERE NSOLICITUD = @nsolicitud",
                new { nsolicitud });

            if (estadoAsiento == "G")
                return $"Solicitud {nsolicitud}: asiento ya generado, no se puede reclasificar.";

            var conceptoAnterior = conn.QueryFirstOrDefault<string>(
                "SELECT ISNULL(COD_CONCEPTO, '') FROM Tes_Transacciones WHERE NSOLICITUD = @nsolicitud",
                new { nsolicitud }) ?? string.Empty;

            conn.Execute(
                "UPDATE Tes_Transacciones SET COD_CONCEPTO = @codConcepto WHERE NSOLICITUD = @nsolicitud",
                new { codConcepto, nsolicitud });

            var detalle = $"Cambio COD_CONCEPTO de {conceptoAnterior} a {codConcepto}. {nota}".Trim();
            string usuarioRegistro = usuario ?? string.Empty;
            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, nsolicitud, "09", detalle, usuarioRegistro);

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId        = CodEmpresa,
                Usuario          = usuarioRegistro,
                DetalleMovimiento = $"Solicitud {nsolicitud}: COD_CONCEPTO reclasificado de {conceptoAnterior} a {codConcepto}",
                Movimiento       = "RECLASIFICACION - WEB",
                Modulo           = _vModulo
            });

            if (reemplazarAsientos)
            {
                var errorAsiento = RegenerarAsientoConcepto(conn, CodEmpresa, nsolicitud, codConcepto);
                if (errorAsiento != null) return errorAsiento;
            }

            return null;
        }

        /// <summary>
        /// Regenera el asiento básico (banco + concepto) para una solicitud.
        /// Reutiliza el monto y tipo de cambio almacenados en el asiento existente.
        /// Retorna null si fue exitoso, o el mensaje de error si no.
        /// </summary>
        private string? RegenerarAsientoConcepto(
            IDbConnection conn, int CodEmpresa, int nsolicitud, string codConcepto)
        {
            var txn = conn.QueryFirstOrDefault<dynamic>(
                @"SELECT T.id_banco, T.tipo, T.cod_unidad,
                         A.monto AS montoAsiento, A.tipo_cambio AS tipoCambio, A.cod_divisa AS codDivisa
                  FROM Tes_Transacciones T
                  LEFT JOIN Tes_Trans_Asiento A ON T.nsolicitud = A.nsolicitud AND A.linea = 1
                  WHERE T.nsolicitud = @nsolicitud",
                new { nsolicitud });

            if (txn == null)
                return $"Solicitud {nsolicitud}: no se encontró la transacción para regenerar asiento.";

            var cuentaBanco = conn.QueryFirstOrDefault<dynamic>(
                @"SELECT TOP 1 B.CtaConta AS CodCuenta, C.cod_contabilidad AS Contabilidad
                  FROM Tes_Bancos B
                  INNER JOIN CntX_Cuentas C ON B.CtaConta = C.cod_cuenta
                  WHERE B.id_banco = @id_banco",
                new { id_banco = (int)txn.id_banco });

            if (cuentaBanco == null)
                return $"Solicitud {nsolicitud}: no se encontró cuenta contable del banco.";

            var cuentaConcepto = conn.QueryFirstOrDefault<string>(
                @"SELECT TOP 1 C.cod_cuenta
                  FROM Tes_Conceptos TC
                  INNER JOIN CntX_Cuentas C ON TC.cod_cuenta = C.cod_cuenta
                  WHERE TC.cod_concepto = @codConcepto
                    AND C.cod_contabilidad = @contabilidad",
                new { codConcepto = codConcepto, contabilidad = cuentaBanco.Contabilidad });

            if (string.IsNullOrEmpty(cuentaConcepto))
                return $"Solicitud {nsolicitud}: no se encontró cuenta para el concepto '{codConcepto}'.";

            bool esAsientoA      = _mTesoreria.fxTesTiposDocAsiento(CodEmpresa, (string)(txn.tipo ?? string.Empty)) == "A";
            string debeHaberBanco    = esAsientoA ? "H" : "D";
            string debeHaberConcepto = esAsientoA ? "D" : "H";

            decimal montoAsiento = (decimal)(txn.montoAsiento ?? 0m);
            decimal tipoCambio   = (decimal)(txn.tipoCambio   ?? 0m);
            string  codDivisa    = (string)(txn.codDivisa     ?? "DOL");
            string  codUnidad    = (string)(txn.cod_unidad    ?? string.Empty);

            conn.Execute("DELETE Tes_Trans_Asiento WHERE nsolicitud = @nsolicitud", new { nsolicitud });

            const string insertSql = @"
                INSERT Tes_Trans_Asiento(nSolicitud, Linea, Cuenta_Contable, cod_unidad, cod_cc, cod_divisa, tipo_cambio, DebeHaber, Monto)
                VALUES (@nSolicitud, @linea, @Cuenta_Contable, @cod_unidad, '', @cod_divisa, @tipo_cambio, @DebeHaber, @Monto)";

            conn.Execute(insertSql, new
            {
                nSolicitud = nsolicitud, linea = 1,
                Cuenta_Contable = (string)cuentaBanco.CodCuenta,
                cod_unidad = codUnidad, cod_divisa = codDivisa,
                tipo_cambio = tipoCambio, DebeHaber = debeHaberBanco, Monto = montoAsiento
            });

            conn.Execute(insertSql, new
            {
                nSolicitud = nsolicitud, linea = 2,
                Cuenta_Contable = cuentaConcepto,
                cod_unidad = codUnidad, cod_divisa = codDivisa,
                tipo_cambio = tipoCambio, DebeHaber = debeHaberConcepto, Monto = montoAsiento
            });

            return null;
        }
    }
}




