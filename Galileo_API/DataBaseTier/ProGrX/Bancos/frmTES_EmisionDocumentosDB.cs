using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Galileo_API.Controllers.WFCSinpe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesEmisionDocumentosDb
    {
        private readonly IConfiguration _config;
        private readonly MTesoreria mTesoreria;
        private readonly VerificadorCoreFactory _factory;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MReportingServicesDB mReporting;
        private readonly PortalDB _portalDB;

        private const string nSolicitudes = "solicitudes";
        private const string nFechas = "fechas";
        private const string zero6Append = "000000";
        private const string zero12Append = "000000000000";
        private const string fechaFormat = "yyyy/MM/dd";
        private const string fechaFormat2 = "ddMMyyyy";

        // Sonar: regex sin timeout es hotspot
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

        public FrmTesEmisionDocumentosDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            mTesoreria = new MTesoreria(config);
            _factory = new VerificadorCoreFactory(config);
            _Security_MainDB = new MSecurityMainDb(config);
            mReporting = new MReportingServicesDB(config);
            _portalDB = new PortalDB(config);
        }

        #region ===== Helpers comunes (reducción de duplicación / Sonar) =====

        private string GetParametro(int codEmpresa, string codigo)
            => mTesoreria.fxTesParametro(codEmpresa, codigo);

        private static string LimpiarReporte(string nombre)
            => Regex.Replace(nombre, @"\.(rdl|rdlc|RDL|RDLC)$", "", RegexOptions.IgnoreCase, RegexTimeout);

        private static void AppendIfNotEmpty(StringBuilder sb, string? line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);
        }

        /// <summary>
        /// Construye el mismo JSON de salida que repetías en varios métodos:
        /// { bancoConsec, extension, contenido }
        /// </summary>
        private static string BuildArchivoJson(long bancoConsec, string extension, StringBuilder sb)
        {
            var archivo = new
            {
                bancoConsec = bancoConsec.ToString(),
                extension,
                contenido = sb.ToString()
            };

            return JsonConvert.SerializeObject(archivo, Formatting.Indented);
        }

        /// <summary>
        /// Consecutivo del día: 1 + count(distinct documento_base)
        /// (reemplaza bloques duplicados con QueryFirstOrDefault + foreach dynamic)
        /// </summary>
        private static int GetConsecutivoArchivoDelDia(SqlConnection connection, int bancoId, DateTime fechaEmision)
        {
            const string sql = @"
select count(distinct documento_base)
from Tes_Transacciones
where id_banco = @banco
  and fecha_emision = @fecha
  and estado = 'T'";

            var count = connection.QuerySingle<int>(sql, new { banco = bancoId, fecha = fechaEmision });
            return count + 1;
        }

        private static (string numNegocio, string cedulaReg) GetEmpresaNumNegocioYReg(SqlConnection connection)
        {
            const string sql = "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA";
            var empresa = connection.QueryFirstOrDefault(sql);

            var cedula = empresa?.cedula_juridica?.ToString()?.Trim() ?? string.Empty;
            return (cedula, cedula);
        }

        private static ErrorDto<object> Error(string msg) =>
            DbHelper.CreateErrorResponse<object>(msg, -1, null!);

        #endregion

        #region ===== Consultas / catálogo =====

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Formatos_Bancos @banco";

                return conn.Query(query, new { banco })
                    .Select(row => new DropDownListaGenericaModel
                    {
                        item = row.IDX,
                        descripcion = row.ItmX
                    })
                    .ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
select Bp.COD_PLAN as item, Bp.COD_PLAN as descripcion
from TES_BANCOS B
inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
Where B.ID_BANCO = @banco And B.UTILIZA_PLAN = 1
order by Bp.COD_PLAN asc";

                return conn.Query<DropDownListaGenericaModel>(query, new { banco }).ToList();
            });
        }

        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
select isnull(count(*),0) as Total,
       isnull(Min(nsolicitud),0) as Minimo,
       isnull(Max(nsolicitud),0) as Maximo
from Tes_Transacciones
Where Estado='P' And Tipo = @tipoDoc and ID_Banco = @banco";

                var solicitudes = conn.QueryFirstOrDefault<TesTransaccionesData>(query, new { tipoDoc, banco })
                                 ?? new TesTransaccionesData();

                if (solicitudes.total == 0)
                {
                    solicitudes.minimo = 0;
                    solicitudes.maximo = 0;
                }

                solicitudes.docInicial = mTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "/", plan).Result;

                var vDato = mTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "mod_consec").Result ?? "0";
                solicitudes.docBloqueo = vDato != "1";

                return solicitudes;
            });
        }

        #endregion

        #region ===== Solicitudes =====

        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            var filtro = ParseFiltros(filtros);
            NormalizarFiltroFechas(filtro);

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var consecInt = mTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, filtro.banco, filtro.tipoDoc, "/", filtro.plan).Result;

                var query = $@"
Select TOP {filtro.cantidad} *,
       dbo.fxTes_Cuentas_Bancarias_Pass(id_Banco,Cta_Ahorros) as Pass
From Tes_Transacciones
Where Estado='P' And Tipo = @tipoDoc
  And Id_Banco=@banco And Autoriza = 'S' and fecha_hold is null";

                if (filtro.generarPor == nSolicitudes)
                    query += " And NSolicitud Between @minimo And @maximo";
                else if (filtro.generarPor == nFechas)
                    query += " And Fecha_Solicitud Between @fechaInicio And @fechaCorte";

                query += " Order by NSolicitud";

                var fechaInicio = filtro.fecha_inicio?.Date;
                var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);

                var result = conn.Query<TesSolicitudesGenData>(
                        query,
                        new
                        {
                            tipoDoc = filtro.tipoDoc,
                            banco = filtro.banco,
                            minimo = filtro.minimo,
                            maximo = filtro.maximo,
                            fechaInicio,
                            fechaCorte
                        })
                    .ToList();

                var now = DateTime.Now;
                foreach (var item in result)
                {
                    item.documento = filtro.tipoDoc == "TE"
                        ? $"{filtro.docInicial:000}-{consecInt}"
                        : filtro.docInicial.ToString();

                    item.fecha = now;
                    item.firmas = item.firmas_autoriza_fecha == null ? "No" : "Sí";
                }

                return result;
            });
        }

        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var docFinal = docInicial + (cantidadList - 1);

                const string query = @"
SELECT ndocumento
FROM Tes_Transacciones
WHERE id_Banco = @banco
  AND ndocumento BETWEEN @docInicial AND @docFinal
  AND Tipo = @tipoDoc"; // (OJO: quitado el ' extra que tenías)

                var lista = conn.Query<int>(query, new { banco, docInicial, docFinal, tipoDoc }).ToList();
                var docExistente = lista.FirstOrDefault(nDoc => nDoc >= docInicial && nDoc <= docFinal);

                if (docExistente != 0)
                    return DbHelper.ErrorResponse($"\nYa existe un Documento asignado [{docExistente}] dentro del rango suministrado", -2);

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Execute("exec spTes_Cuentas_Revisa @banco", new { banco });
                return DbHelper.OkResponse("Cuentas verificadas correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
select 
    B.id_Banco as item,
    rtrim(B.descripcion) as descripcion
from Tes_Bancos B
inner join tes_Banco_ASG A 
    on B.id_Banco = A.id_Banco
   and A.nombre = @usuario
where B.estado = 'A'
  and B.puente = 1";

                return conn.Query<DropDownListaGenericaModel>(query, new { usuario = Usuario }).ToList();
            });
        }

        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(int CodEmpresa, int Banco, string Usuario, string Solicitudes)
        {
            try
            {
                var listaSolicitudes = JsonConvert.DeserializeObject<List<int>>(Solicitudes) ?? new List<int>();
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string query = @"exec spTes_Traslados_Cuenta_Puente @solicitud, @banco, @usuario";

                foreach (var solicitud in listaSolicitudes)
                {
                    connection.Execute(query, new { solicitud, banco = Banco, usuario = Usuario });
                }

                return DbHelper.OkResponse("Solicitudes movidas correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        #endregion

        #region ===== Generación principal =====

        public ErrorDto<object> TES_EmisionDocumento_Generar(int codEmpresa, string filtros)
        {
            try
            {
                var filtro = ParseFiltros(filtros);
                NormalizarFiltroFechas(filtro);

                using var connection = _portalDB.CreateConnection(codEmpresa);

                var bancoDocs = LoadBancoDocs(connection, filtro);
                if (bancoDocs == null)
                    return Error("No existe configuración en tes_banco_docs para el banco/tipoDoc indicado.");

                var bancoData = LoadBancoData(connection, filtro);
                if (bancoData == null)
                    return Error("No existe configuración en Tes_Bancos para el banco indicado.");

                var vFirmas = LoadFirmasAut(connection, filtro);
                var q = BuildQueries(filtro);

                return bancoDocs.comprobante switch
                {
                    "01" or "02" or "03" => ProcesarChequesYBoletas(codEmpresa, connection, filtro, bancoDocs, bancoData, vFirmas, q),
                    "04" => ProcesarTransferencias(codEmpresa, connection, filtro, q),
                    _ => Error($"Comprobante '{bancoDocs.comprobante}' no soportado.")
                };
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message, -1, null!);
            }
        }

        private ErrorDto<object> ProcesarChequesYBoletas(
            int codEmpresa,
            SqlConnection connection,
            TesEmisionDocFiltros filtro,
            TesBancoDocsData bancoDocs,
            TesBancoData bancoData,
            int vFirmas,
            (string QueryTransac, string BaseQuery, object Parametros) q)
        {
            var vFecha = DateTime.Now;

            var chequesReport = mTesoreria.sbCargaArchivosEspeciales(codEmpresa, filtro.banco).Result;
            if (chequesReport == null)
                return Error("No se pudo cargar archivos especiales del banco.");

            var consecutivo = ResolverConsecutivo(codEmpresa, filtro, bancoDocs, connection);
            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();

            var listaConFirmas = new List<TesTransaccionDto>();
            var listaSinFirmas = new List<TesTransaccionDto>();
            var pdfsBoleta = new List<byte[]>();

            FileContentResult? fileResultBoleta = null;
            string reporteCkConFirmas = string.Empty;
            string reporteCkSinFirmas = string.Empty;

            var reporteData = CrearReporteDataBase(codEmpresa, filtro.usuario);

            var contador = 0;
            foreach (var item in transacciones)
            {
                if (contador >= filtro.verificacion)
                    break;

                var upd = ProcesarTransaccionEmitida(codEmpresa, filtro, connection, bancoDocs, item, vFecha, consecutivo);
                if (upd.Code != 0)
                    return upd;

                if (bancoDocs.doc_auto == 1)
                    consecutivo = mTesoreria.fxTesTipoDocConsec(codEmpresa, filtro.banco, filtro.tipoDoc, "+").Result;

                var req = new EmisionClasificacionRequest
                {
                    CodEmpresa = codEmpresa,
                    Filtro = filtro,
                    BancoDocs = bancoDocs,
                    BancoData = bancoData,
                    UsaFirmas = vFirmas,
                    ChequesReport = chequesReport,
                    ReporteData = reporteData
                };

                var state = new EmisionClasificacionState
                {
                    // opcional: si ya venías con valores iniciales
                    ReporteCkConFirmas = reporteCkConFirmas,
                    ReporteCkSinFirmas = reporteCkSinFirmas,
                };

                // Si ya tenías listas existentes, lo ideal es que uses state como “source of truth”.
                // Si querés seguir usando tus listas viejas, copiá la referencia (mejor evita duplicar).
                state.ListaConFirmas.AddRange(listaConFirmas);
                state.ListaSinFirmas.AddRange(listaSinFirmas);
                state.PdfsBoleta.AddRange(pdfsBoleta);
                state.FileResultBoleta = fileResultBoleta;

                var clas = ClasificarYGenerarBoletaSiAplica(req, state, item); 

                if (clas.Code != 0)
                    return clas;

                contador++;
            }

            var (ckConFirma, ckSinFirma) = GenerarReportesCheques(
                reporteData,
                reporteCkConFirmas,
                reporteCkSinFirmas,
                listaConFirmas,
                listaSinFirmas);

            var boletaReg = GenerarBoletaRegistro(pdfsBoleta, fileResultBoleta);

            var payload = new
            {
                archivo = new
                {
                    chequeConFirma = ckConFirma,
                    chequeSinFirma = ckSinFirma,
                    boletaRegisto = boletaReg
                },
                strQuery = JsonConvert.SerializeObject(transacciones, Formatting.Indented),
                parametros = q.Parametros,
                comprobante = bancoDocs.comprobante
            };

            return DbHelper.CreateOkResponse<object>(
                JsonConvert.SerializeObject(payload, Formatting.Indented)
            );
        }

        private ErrorDto<object> ProcesarTransferencias(
            int codEmpresa,
            SqlConnection connection,
            TesEmisionDocFiltros filtro,
            (string QueryTransac, string BaseQuery, object Parametros) q)
        {
            // NOTA: aquí mantengo tu lógica existente.
            // (El hotspot S2077 por concatenación SQL no lo atacamos en este refactor, porque el objetivo era Duplicated Lines.)

            return filtro.formatoTE switch
            {
                "A" => ProcesarTE_BNCR_InternetBanking(codEmpresa, filtro, connection, q),
                "B" => ProcesarTE_BancoPopular(codEmpresa, filtro, connection, q),
                "C" => ProcesarTE_BCR_Planilla(codEmpresa, filtro, connection, q),
                "D" => sbTeBCR_Empresarial(codEmpresa, filtro),
                "E" => sbTeBCT_Enlace(codEmpresa, filtro),
                "F" => sbTeBCR_Comercial(codEmpresa, filtro),
                "G" => sbTeBNCR_Sinpe(codEmpresa, filtro),
                "DV1" or "DV2" => sbTeFormatoEstandar(codEmpresa, filtro),
                "S" => DbHelper.CreateErrorResponse<object>(
                    "No se pudo realizar la operación, debido a que la opción de SINPE se encuentra en espera", -1, null!),
                "SG" => sbTeBancoSinpeGeneral(codEmpresa, filtro,
                            connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList()),
                _ => sbTeFormatoEstandar(codEmpresa, filtro)
            };
        }

        #endregion

        #region ===== Subprocesos Cheques/Boletas (menos duplicación) =====

        private long ResolverConsecutivo(int codEmpresa, TesEmisionDocFiltros filtro, TesBancoDocsData bancoDocs, SqlConnection connection)
        {
            if (bancoDocs.doc_auto != 1)
                return 0;

            if (filtro.docBloqueo == true)
                return mTesoreria.fxTesTipoDocConsec(codEmpresa, filtro.banco, filtro.tipoDoc, "/").Result;

            if (filtro.docInicial > 0)
            {
                const string queryUpdate = @"
update tes_banco_docs
set consecutivo = @consecutivo
where id_banco = @banco and tipo = @tipoDoc";

                connection.Execute(queryUpdate, new { consecutivo = filtro.docInicial, banco = filtro.banco, tipoDoc = filtro.tipoDoc });
                return filtro.docInicial;
            }

            return mTesoreria.fxTesTipoDocConsec(codEmpresa, filtro.banco, filtro.tipoDoc, "+").Result;
        }

        private ErrorDto<object> ProcesarTransaccionEmitida(
            int codEmpresa,
            TesEmisionDocFiltros filtro,
            SqlConnection connection,
            TesBancoDocsData bancoDocs,
            TesTransaccionDto item,
            DateTime vFecha,
            long consecutivo)
        {
            var nsolicitud = item.nsolicitud;
            if (nsolicitud <= 0)
                return Error("NSolicitud inválida al procesar transacción.");

            var queryUpdate = new StringBuilder(@"
UPDATE Tes_Transacciones
SET Estado = 'I',
    Fecha_Emision = @vfecha,
    Ubicacion_Actual = 'T',
    FECHA_TRASLADO = @vfecha,
    User_Genera = @usuario");

            if (bancoDocs.doc_auto == 1)
                queryUpdate.Append(" ,NDocumento = @consecutivo");

            queryUpdate.Append(" where NSolicitud = @nsolicitud");

            connection.Execute(queryUpdate.ToString(), new
            {
                vfecha = vFecha,
                usuario = filtro.usuario,
                consecutivo,
                nsolicitud
            });

            mTesoreria.sbTesBancosAfectacion(codEmpresa, nsolicitud, "E");
            mTesoreria.sbTesBitacoraEspecial(codEmpresa, nsolicitud, "10", "", (filtro.usuario ?? "").ToUpperInvariant());

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (filtro.usuario ?? "").ToUpperInvariant(),
                DetalleMovimiento = $"Genero Solicitud {nsolicitud}",
                Movimiento = "GENERA - WEB",
                Modulo = 9
            });

            mTesoreria.sbTESActualizaCC(
                codEmpresa,
                new MTesoreria.ActualizaCCParams
                {
                    Codigo = string.IsNullOrEmpty(item.codigo) ? string.Empty : item.codigo.Trim(),
                    Tipo = item.tipo,
                    Documento = consecutivo.ToString(),
                    Banco = item.id_banco ?? 0,
                    OP = item.op == null ? 0 : item.op.Value,
                    Modulo = item.modulo,
                    SubModulo = item.submodulo,
                    Referencia = item.referencia == null ? 0 : item.referencia.Value
                });

            return DbHelper.CreateOkResponse<object>(new object());
        }

        private ErrorDto<object> ClasificarYGenerarBoletaSiAplica(
         EmisionClasificacionRequest req,
         EmisionClasificacionState state,
         TesTransaccionDto item)
        {
            // Guards para Sonar S2259 (null deref)
            if (req is null) return ErrorObj("Request de clasificación es nulo.");
            if (state is null) return ErrorObj("State de clasificación es nulo.");
            if (item is null) return ErrorObj("Transacción (item) es nula.");
            if (req.BancoDocs is null) return ErrorObj("BancoDocs es nulo.");
            if (req.BancoData is null) return ErrorObj("BancoData es nulo.");
            if (req.ChequesReport is null) return ErrorObj("ChequesReport es nulo.");
            if (req.ReporteData is null) return ErrorObj("ReporteData es nulo.");

            // Misma lógica original por comprobante
            switch (req.BancoDocs.comprobante)
            {
                case "01": // Cheques fórmula continua
                    return ClasificarChequeFormulaContinua(req, state, item);

                case "02":
                case "03":
                    return GenerarBoletaRegistro(state.PdfsBoleta, state.FileResultBoleta);

                default:
                    return OkObj(); // no aplica
            }
        }

        private ErrorDto<object> ClasificarChequeFormulaContinua(
            EmisionClasificacionRequest req,
            EmisionClasificacionState state,
            TesTransaccionDto item)
        {
            // Null-safety de rutas
            var rutaSinFirmas = req.ChequesReport.chequesSinFirmas;
            if (string.IsNullOrWhiteSpace(rutaSinFirmas))
                return ErrorObj("No está configurada la ruta del reporte de cheques sin firmas.");

            // Si NO usa firmas => siempre sin firmas
            if (req.UsaFirmas != 1)
            {
                AsignarSinFirmas(item, rutaSinFirmas, state);
                return OkObj();
            }

            var rutaConFirmas = req.ChequesReport.chequesFirmas;
            if (string.IsNullOrWhiteSpace(rutaConFirmas))
                return ErrorObj("No está configurada la ruta del reporte de cheques con firmas.");

            // Rango y autorización
            bool firmaAutorizada = item.firmas_autoriza_fecha != null;
            var desde = req.BancoData.firmas_desde;
            var hasta = req.BancoData.firmas_hasta;

            bool vaConFirmas = firmaAutorizada || (item.monto >= desde && item.monto <= hasta);

            if (vaConFirmas)
                AsignarConFirmas(item, rutaConFirmas, state);
            else
                AsignarSinFirmas(item, rutaSinFirmas, state);

            return OkObj();
        }

       

        private (string ckConFirma, string ckSinFirma) GenerarReportesCheques(
            FrmReporteGlobal reporteData,
            string reporteCkConFirmas,
            string reporteCkSinFirmas,
            List<TesTransaccionDto> listaConFirmas,
            List<TesTransaccionDto> listaSinFirmas)
        {
            var ckCon = string.Empty;
            var ckSin = string.Empty;

            if (listaConFirmas.Count == 0 && listaSinFirmas.Count == 0)
                return (ckCon, ckSin);

            if (listaConFirmas.Count > 0 && !string.IsNullOrWhiteSpace(reporteCkConFirmas))
            {
                reporteData.parametros = System.Text.Json.JsonSerializer.Serialize(listaConFirmas);
                reporteData.nombreReporte = reporteCkConFirmas;
                ckCon = SerializarResultadoReporte(mReporting.ReporteRDLC_v2(reporteData));
            }

            if (listaSinFirmas.Count > 0 && !string.IsNullOrWhiteSpace(reporteCkSinFirmas))
            {
                reporteData.parametros = System.Text.Json.JsonSerializer.Serialize(listaSinFirmas);
                reporteData.nombreReporte = reporteCkSinFirmas;
                ckSin = SerializarResultadoReporte(mReporting.ReporteRDLC_v2(reporteData));
            }

            return (ckCon, ckSin);
        }

        private static ErrorDto<object> GenerarBoletaRegistro(
    List<byte[]> pdfsBoleta,
    FileContentResult? fileResultBoleta)
        {
            if (pdfsBoleta is null || pdfsBoleta.Count == 0)
                return new ErrorDto<object>
                {
                    Code = 0,
                    Description = "OK",
                    Result = string.Empty
                };

            if (fileResultBoleta?.ContentType is null)
                return new ErrorDto<object>
                {
                    Code = -1,
                    Description = "No se pudo generar boleta: FileContentResult es nulo o inválido.",
                    Result = null
                };

            try
            {
                var combinado = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfsBoleta.ToArray());

                if (combinado is null || combinado.Length == 0)
                    return new ErrorDto<object>
                    {
                        Code = -1,
                        Description = "No se pudo generar boleta: el PDF combinado quedó vacío.",
                        Result = null
                    };

                // Creamos un nuevo objeto (evita mutar el original)
                var resultFile = new FileContentResult(combinado, fileResultBoleta.ContentType)
                {
                    FileDownloadName = fileResultBoleta.FileDownloadName
                };

                // Si tu contrato actual espera JSON string:
                var json = JsonConvert.SerializeObject(resultFile, Formatting.Indented);

                return new ErrorDto<object>
                {
                    Code = 0,
                    Description = "OK",
                    Result = json
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<object>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = null
                };
            }
        }

        private static FrmReporteGlobal CrearReporteDataBase(int codEmpresa, string? usuario) => new()
        {
            codEmpresa = codEmpresa,
            parametros = null,
            nombreReporte = "",
            usuario = usuario ?? string.Empty,
            cod_reporte = "P",
            folder = "Bancos"
        };

        private static string SerializarResultadoReporte(IActionResult action)
        {
            if (action is not ObjectResult obj)
                return JsonConvert.SerializeObject(action, Formatting.Indented);

            var res = obj.Value;
            var jres = System.Text.Json.JsonSerializer.Serialize(res);
            var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);
            return JsonConvert.SerializeObject(err, Formatting.Indented);
        }

        #endregion

        #region ===== Carga de datos / queries base (reduce duplicación) =====

        private static TesEmisionDocFiltros ParseFiltros(string filtros)
            => JsonConvert.DeserializeObject<TesEmisionDocFiltros>(filtros) ?? new TesEmisionDocFiltros();

        private static void NormalizarFiltroFechas(TesEmisionDocFiltros filtro)
        {
            if (filtro.generarPor != nFechas)
            {
                filtro.fecha_inicio = null;
                filtro.fecha_corte = null;
            }
        }

        private static TesBancoDocsData? LoadBancoDocs(SqlConnection connection, TesEmisionDocFiltros filtro)
        {
            const string sql = @"
select doc_auto, comprobante
from tes_banco_docs
where id_banco = @banco and tipo = @tipoDoc";

            return connection.QueryFirstOrDefault<TesBancoDocsData>(sql, new { banco = filtro.banco, tipoDoc = filtro.tipoDoc });
        }

        private static TesBancoData? LoadBancoData(SqlConnection connection, TesEmisionDocFiltros filtro)
        {
            const string sql = @"
select firmas_desde, firmas_hasta, formato_transferencia, Lugar_Emision
from Tes_Bancos
where id_banco = @banco";

            return connection.QueryFirstOrDefault<TesBancoData>(sql, new { banco = filtro.banco });
        }

        private static int LoadFirmasAut(SqlConnection connection, TesEmisionDocFiltros filtro)
        {
            const string sql = @"
select isnull(count(*),0)
from TES_BANCO_FIRMASAUT
where id_Banco = @banco and usuario = @usuario";

            return connection.QueryFirstOrDefault<int>(sql, new { banco = filtro.banco, usuario = filtro.usuario });
        }

        private static (string QueryTransac, string BaseQuery, object Parametros) BuildQueries(TesEmisionDocFiltros filtro)
        {
            var fechaInicio = filtro.fecha_inicio?.Date;
            var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);

            var queryTransac = $@"
Select TOP {filtro.cantidad} *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null";

            var baseQuery = $@"
(SELECT TOP {filtro.cantidad} nsolicitud
 FROM Tes_Transacciones
 WHERE Estado = 'P' AND Tipo = @tipoDoc
   AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL";

            if (filtro.generarPor == nSolicitudes)
            {
                queryTransac += " And NSolicitud Between @minimo And @maximo";
                baseQuery += " And NSolicitud Between @minimo And @maximo";
            }
            else if (filtro.generarPor == nFechas)
            {
                queryTransac += " And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                baseQuery += " And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
            }

            queryTransac += " Order by Nsolicitud";
            baseQuery += " Order by Nsolicitud)";

            var parametros = new
            {
                banco = filtro.banco,
                tipoDoc = filtro.tipoDoc,
                minimo = filtro.minimo,
                maximo = filtro.maximo,
                fechaInicio,
                fechaCorte
            };

            return (queryTransac, baseQuery, parametros);
        }

        #endregion

        #region ===== Transferencias (refactor duplicación de salida + consecutivos) =====

        private ErrorDto<object> ProcesarTE_BNCR_InternetBanking(int codEmpresa, TesEmisionDocFiltros filtro, SqlConnection connection,
            (string QueryTransac, string BaseQuery, object Parametros) q)
        {
            var queryA = "select sum(monto) as PLx from Tes_Transacciones where nsolicitud in " ;
            queryA += q.BaseQuery;
            var montoPL = connection.QueryFirstOrDefault<int>(queryA, q.Parametros);

            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();
            return sbTeBancoNacional(codEmpresa, filtro, transacciones, montoPL);
        }

        private ErrorDto<object> ProcesarTE_BancoPopular(int codEmpresa, TesEmisionDocFiltros filtro, SqlConnection connection,
            (string QueryTransac, string BaseQuery, object Parametros) q)
        {
            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();
            return sbTeBancoPopular(codEmpresa, filtro, transacciones);
        }

        private ErrorDto<object> ProcesarTE_BCR_Planilla(int codEmpresa, TesEmisionDocFiltros filtro, SqlConnection connection,
            (string QueryTransac, string BaseQuery, object Parametros) q)
        {
            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();

            var queryC = @"
select sum(dbo.fxTESBCRTestkey(cta_ahorros,monto)) as TestKeyX,
       sum(Monto) as Monto
from Tes_Transacciones
where nsolicitud in ";
            queryC += q.BaseQuery;

            var resultC = connection.QueryFirstOrDefault(queryC, q.Parametros);

            long xTestKey = 0;
            decimal totalMonto = 0;

            if (resultC != null)
            {
                xTestKey = (long?)resultC.TestKeyX ?? 0;
                totalMonto = (decimal?)resultC.Monto ?? 0m;
            }

            xTestKey = xTestKey > 2147483468 ? 2147483468 : xTestKey;
            return sbTeBCR_Planilla(codEmpresa, filtro, transacciones, xTestKey, totalMonto);
        }

        #endregion

        #region ===== Implementaciones existentes (con menos duplicación en salida) =====

        private ErrorDto<object> sbTeBancoNacional(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList, int? curPlanilla)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int BancoID = filtros.banco;
            DateTime vFecha = DateTime.Now;

            decimal curMonto1 = curPlanilla ?? 0;
            string strMonto = curMonto1.ToString("0000000000.00").Replace(".", "");
            string vCuentaEmpresa = "";
            string vNumCliente = "";
            decimal curMonto2 = 0;
            long curCuentas = 0;

            try
            {
                SeguridadPortalDb seguridadPortal = new SeguridadPortalDb(_config);
                string Empresa_Name = "TF " + seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa).PGX_CORE_DB;
                string vConcepto = Empresa_Name.PadRight(30, ' ');

                var bancoInfo = connection.QueryFirstOrDefault("select Cta,codigo_Cliente from tes_Bancos Where id_Banco = @banco",
                    new { banco = BancoID });

                if (bancoInfo != null)
                {
                    vCuentaEmpresa = (bancoInfo.Cta ?? "").ToString().Trim().Replace("-", "");
                    vNumCliente = (bancoInfo.codigo_Cliente ?? "").ToString().PadLeft(6, '0');
                }

                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                var header = new StringBuilder(120);
                header.Append('1');
                header.Append(vNumCliente);
                header.Append(vFecha.Day.ToString("00"));
                header.Append(vFecha.Month.ToString("00"));
                header.Append(vFecha.Year.ToString("0000"));
                header.Append(BancoID.ToString("D12"));
                header.Append("10000");
                header.Append(strMonto);
                header.Append("000000000000000000000000");
                sb.AppendLine(header.ToString());

                int i = 0;

                foreach (var item in transaccionesList)
                {
                    i++;

                    string cuenta = (item.cta_ahorros ?? "").Replace("-", "").Trim();
                    var linea = new StringBuilder(120);

                    linea.Append('3');
                    linea.Append(cuenta.Substring(5, 3));
                    linea.Append(cuenta.Substring(0, 3));
                    linea.Append("01");
                    linea.Append(cuenta.Substring(cuenta.Length - 7));
                    linea.Append(i.ToString("D8"));

                    decimal monto = item.monto ?? 0m;
                    string strMontoDet = monto.ToString("0000000000.00").Replace(".", "");
                    linea.Append(strMontoDet);
                    linea.Append(vConcepto);
                    linea.Append("00");

                    sb.AppendLine(linea.ToString());
                }

                var last = new StringBuilder(120);
                last.Append('2');
                last.Append(vCuentaEmpresa.Substring(0, 3));
                last.Append("10001");
                last.Append(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7));
                last.Append((i + 1).ToString("D8"));

                string strMontoEmpresa = curMonto2.ToString("0000000000.00").Replace(".", "");
                last.Append(strMontoEmpresa);
                last.Append(vConcepto);
                last.Append("00");
                sb.AppendLine(last.ToString());

                curCuentas += long.Parse(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7, 6));

                var linea4 = new StringBuilder(200);
                linea4.Append('4');
                decimal montoControl = curMonto1 + curMonto2;
                string strMontoControl = montoControl.ToString("0000000000000.00").Replace(".", "");
                linea4.Append(strMontoControl);
                linea4.Append(curCuentas.ToString("D10"));
                linea4.Append("0000000000");
                linea4.Append(zero12Append);
                linea4.Append(zero12Append);
                linea4.Append("00000000");
                sb.AppendLine(linea4.ToString());

                var json = BuildArchivoJson(BancoConsec, "ENV", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBancoPopular(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            DateTime vFecha = DateTime.Now;

            try
            {
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                foreach (var item in transaccionesList)
                {
                    string codigo10;
                    var codigoTrim = item.codigo?.Trim() ?? string.Empty;

                    codigo10 = codigoTrim.Length switch
                    {
                        8 => "0" + codigoTrim.Substring(0, 1) + "0" + codigoTrim.Substring(1, 7),
                        9 => "0" + codigoTrim,
                        < 8 => Convert.ToInt64(codigoTrim == "" ? "0" : codigoTrim).ToString("D10"),
                        > 10 => codigoTrim.Substring(0, 4) + "0" + codigoTrim.Substring(5, 5),
                        _ => codigoTrim
                    };

                    string nombre = (item.beneficiario ?? string.Empty).Trim();
                    nombre = nombre.Length > 30 ? nombre.Substring(0, 30) : nombre.PadRight(30, ' ');

                    string cuenta = (item.cta_ahorros ?? "0").Trim();
                    cuenta = cuenta.Length > 13 ? cuenta.Substring(0, 13) : cuenta.PadLeft(13, '0');

                    decimal monto = item.monto ?? 0m;
                    string strMonto = monto.ToString("000000000.00").Replace(".", "");

                    string strFecha = vFecha.ToString(fechaFormat2);

                    var line = new StringBuilder(120);
                    line.Append(codigo10);
                    line.Append(nombre);
                    line.Append(cuenta);
                    line.Append(' ');
                    line.Append(strMonto);
                    line.Append(strFecha);
                    line.Append('A');
                    line.Append("06");
                    line.Append('P');
                    line.Append(strFecha);
                    line.Append(strMonto);

                    sb.AppendLine(line.ToString());
                }

                var json = BuildArchivoJson(BancoConsec, "txt", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeFormatoEstandar(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            string pFormato = filtros.formatoTE ?? string.Empty;
            int BancoID = filtros.banco;

            string vNumNegocio = "";
            string vExtension = "";
            string vProcedimiento = "";

            try
            {
                var empresaData = connection.QueryFirstOrDefault("select REPLACE(cedula_juridica,'-','') as cedula_Juridica,NOMBRE From SIF_EMPRESA");
                if (empresaData != null)
                    vNumNegocio = empresaData.cedula_Juridica;

                var formatoData = connection.QueryFirstOrDefault("select Procedimiento,Extension from vTes_Formatos where cod_formato = @formato",
                    new { formato = pFormato });

                if (formatoData != null)
                {
                    vExtension = formatoData.Extension;
                    vProcedimiento = formatoData.Procedimiento;
                }

                string BancoTDoc = filtros.tipoDoc;
                string BancoPlan = filtros.plan;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+", BancoPlan).Result;

                var sb = new StringBuilder();

                for (int numLinea = 1; numLinea <= 3; numLinea++)
                {
                    var queryLinea = $@"EXEC {vProcedimiento} {numLinea}, @bancoID, @bancoTDoc, @numNegocio, @bancoConsec, 
@cantidadSolicitudes, @mSolInicio, @mSolCorte, @mFechaInicio, @mFechaCorte";

                    if (BancoPlan != "-sp-")
                        queryLinea += ", @bancoPlan";

                    var parametros = new
                    {
                        bancoID = BancoID,
                        bancoTDoc = BancoTDoc,
                        numNegocio = vNumNegocio,
                        bancoConsec = BancoConsec,
                        cantidadSolicitudes = filtros.cantidad,
                        mSolInicio = filtros.minimo,
                        mSolCorte = filtros.maximo,
                        mFechaInicio = filtros.fecha_inicio?.ToString(fechaFormat),
                        mFechaCorte = filtros.fecha_corte?.ToString(fechaFormat),
                        bancoPlan = BancoPlan
                    };

                    var lineas = connection.Query<string>(queryLinea, parametros);
                    foreach (var linea in lineas)
                        AppendIfNotEmpty(sb, linea);
                }

                var json = BuildArchivoJson(BancoConsec, vExtension, sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCR_Planilla(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList, long vTestKey, decimal vMontoTotal)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            DateTime vFecha = DateTime.Now;

            try
            {
                string vRazon = GetParametro(CodEmpresa, "BCRFormat3").PadRight(30, ' ');
                string vNumNegocio = GetParametro(CodEmpresa, "BCRFormat1");
                string vCedulaReg = GetParametro(CodEmpresa, "BCRFormat2");

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;

                int i = GetConsecutivoArchivoDelDia(connection, BancoID, vFecha);
                string vConArchivo = i.ToString("D3");

                var vCuentaBanco = connection.QueryFirstOrDefault<string>(
                    "select Cta from Tes_Bancos where id_Banco = @banco",
                    new { banco = filtros.banco }) ?? "0";

                vCuentaBanco = "001" + int.Parse(vCuentaBanco).ToString("D8");

                const string qTest = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                int xTestKey = connection.QueryFirstOrDefault<int>(qTest, new { cuentaBanco = vCuentaBanco, montoTotal = vMontoTotal });
                vTestKey = Math.Min(vTestKey + xTestKey, 2147483468);

                var vTesKeyCh = vTestKey.ToString().Trim();
                if (vTesKeyCh.Length > 12)
                    vTestKey = long.Parse(vTesKeyCh[^12..]);

                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                var header = new StringBuilder(220);
                header.Append("000");
                header.Append(vNumNegocio);
                header.Append(vConArchivo);
                header.Append(zero6Append);
                header.Append(vCedulaReg);
                header.Append(Convert.ToInt64(vTestKey).ToString("D12"));
                header.Append(zero6Append);
                header.Append(vFecha.Day.ToString("D2"));
                header.Append(vFecha.Month.ToString("D2"));
                header.Append(vFecha.Year.ToString("D4"));
                header.Append(new string(' ', 21));
                header.Append('Y');
                sb.AppendLine(header.ToString());

                int lineaIndex = 1;
                var debito = new StringBuilder(220);
                debito.Append("000");
                debito.Append('1');
                debito.Append("00000");
                debito.Append(vCuentaBanco.Trim().PadRight(11).Substring(0, 11));
                debito.Append('1');
                debito.Append('4');
                debito.Append("0000");
                debito.Append(BancoConsec.ToString("D4"));
                debito.Append(lineaIndex.ToString("D4"));
                debito.Append(((long)(vMontoTotal * 100)).ToString("D12"));
                debito.Append(vFecha.ToString(fechaFormat2));
                debito.Append('0');
                debito.Append(vRazon);
                sb.AppendLine(debito.ToString());

                foreach (var item in transaccionesList)
                {
                    lineaIndex++;

                    string cuenta = (item.cta_ahorros ?? string.Empty).PadRight(11).Substring(0, 11).Trim();
                    long montoCents = (long)Math.Round(((item.monto ?? 0m) * 100m), 0, MidpointRounding.AwayFromZero);

                    var credito = new StringBuilder(220);
                    credito.Append("000");
                    credito.Append('2');
                    credito.Append("00000");
                    credito.Append(cuenta);
                    credito.Append('1');
                    credito.Append('2');
                    credito.Append("0000");
                    credito.Append(BancoConsec.ToString("D4"));
                    credito.Append(lineaIndex.ToString("D4"));
                    credito.Append(montoCents.ToString("D12"));
                    credito.Append(vFecha.ToString(fechaFormat2));
                    credito.Append('0');
                    credito.Append(vRazon);

                    sb.AppendLine(credito.ToString());
                }

                var json = BuildArchivoJson(BancoConsec, "BCR", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCR_Empresarial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            DateTime vFecha = DateTime.Now;
            int txtCantidadSolicitudes = filtros.cantidad;

            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);

            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }

            try
            {
                var (vNumNegocio, vCedulaReg) = GetEmpresaNumNegocioYReg(connection);

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                int i = GetConsecutivoArchivoDelDia(connection, BancoID, vFecha);
                string vConArchivo = i.ToString("D3");

                _ = connection.QueryFirstOrDefault<int>(
                    @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as Cantidad",
                    new { banco = BancoID, fecha = vFecha });

                var control = new StringBuilder(200);
                control.Append("000");
                control.Append(vCedulaReg.Trim().PadLeft(12, '0'));
                control.Append(vConArchivo);
                control.Append(vFecha.ToString(fechaFormat2));
                control.Append(zero12Append);
                control.Append(zero12Append);
                control.Append(zero6Append);
                control.Append(new string(' ', 6));
                control.Append("TLB");
                control.Append(new string(' ', 128));
                control.Append('D');
                sb.AppendLine(control.ToString());

                const string debitosQuery = @"exec spTES_BCR_Empresarial 2, @banco, @bancoTDoc, 
@numNegocio, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var linea2 = connection.QueryFirstOrDefault<string>(debitosQuery, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    numNegocio = vNumNegocio,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                });

                AppendIfNotEmpty(sb, linea2);

                const string creditosQuery = @"exec spTES_BCR_Empresarial 3, @banco, @bancoTDoc, 
@numNegocio, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var linea3 = connection.QueryFirstOrDefault<string>(creditosQuery, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    numNegocio = vNumNegocio,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                });

                AppendIfNotEmpty(sb, linea3);

                var json = BuildArchivoJson(BancoConsec, "txt", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCT_Enlace(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int txtCantidadSolicitudes = filtros.cantidad;

            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);

            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }

            try
            {
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                const string query = @"exec spTES_BCT_Enlace 
@banco, @bancoTDoc, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var resultado = connection.QueryFirstOrDefault(query, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                });

                string linea = resultado?.Linea?.ToString() ?? string.Empty;
                AppendIfNotEmpty(sb, linea);

                var json = BuildArchivoJson(BancoConsec, "txt", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCR_Comercial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            DateTime vFecha = DateTime.Now;
            int txtCantidadSolicitudes = filtros.cantidad;

            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);

            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }

            try
            {
                var (vNumNegocio, vCedulaReg) = GetEmpresaNumNegocioYReg(connection);

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                int i = GetConsecutivoArchivoDelDia(connection, BancoID, vFecha);
                string vConArchivo = i.ToString("D3");

                _ = connection.QueryFirstOrDefault<int>(
                    @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as Cantidad",
                    new { banco = BancoID, fecha = vFecha });

                var control = new StringBuilder(200);
                control.Append("000");
                control.Append(vCedulaReg.Trim().PadLeft(12, '0'));
                control.Append(vConArchivo);
                control.Append(vFecha.ToString(fechaFormat2));
                control.Append(zero12Append);
                control.Append(zero12Append);
                control.Append(zero6Append);
                control.Append(new string('0', 138));
                sb.AppendLine(control.ToString());

                const string debitosQuery = @"exec spTES_BCR_Comercial 2, @banco, @bancoTDoc, 
@numNegocio, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var linea2 = connection.QueryFirstOrDefault<string>(debitosQuery, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    numNegocio = vNumNegocio,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                });

                AppendIfNotEmpty(sb, linea2);

                const string creditosQuery = @"exec spTES_BCR_Comercial 3, @banco, @bancoTDoc, 
@numNegocio, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var linea3 = connection.QueryFirstOrDefault<string>(creditosQuery, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    numNegocio = vNumNegocio,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                });

                AppendIfNotEmpty(sb, linea3);

                var json = BuildArchivoJson(BancoConsec, "txt", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBNCR_Sinpe(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int txtCantidadSolicitudes = filtros.cantidad;

            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);

            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }

            try
            {
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                const string sp = @"exec spTES_BNCR_SINPE 
@numLinea, @banco, @bancoTDoc, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var parametrosBase = new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                };

                var l1 = connection.QueryFirstOrDefault<string>(sp, new { numLinea = 1, parametrosBase.banco, parametrosBase.bancoTDoc, parametrosBase.bancoConsec, parametrosBase.cantidad, parametrosBase.solInicio, parametrosBase.solCorte, parametrosBase.fechaInicio, parametrosBase.fechaCorte });
                var l2 = connection.QueryFirstOrDefault<string>(sp, new { numLinea = 2, parametrosBase.banco, parametrosBase.bancoTDoc, parametrosBase.bancoConsec, parametrosBase.cantidad, parametrosBase.solInicio, parametrosBase.solCorte, parametrosBase.fechaInicio, parametrosBase.fechaCorte });
                var l3 = connection.QueryFirstOrDefault<string>(sp, new { numLinea = 3, parametrosBase.banco, parametrosBase.bancoTDoc, parametrosBase.bancoConsec, parametrosBase.cantidad, parametrosBase.solInicio, parametrosBase.solCorte, parametrosBase.fechaInicio, parametrosBase.fechaCorte });

                AppendIfNotEmpty(sb, l1);
                AppendIfNotEmpty(sb, l2);
                AppendIfNotEmpty(sb, l3);

                var json = BuildArchivoJson(BancoConsec, "tef", sb);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        #endregion

        #region ===== SINPE General (ya refactorizado para Sonar) =====

        private ErrorDto<object> sbTeBancoSinpeGeneral(int codEmpresa, TesEmisionDocFiltros filtro, List<TesTransaccionDto> transaccionesList)
        {
            if (!string.Equals(filtro.tipoDoc, "TS", StringComparison.OrdinalIgnoreCase))
                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new { results = Array.Empty<ErrorDto>() }));

            if (transaccionesList == null || transaccionesList.Count == 0)
                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new { results = Array.Empty<ErrorDto>() }));

            if (string.IsNullOrWhiteSpace(filtro.usuario))
                return DbHelper.CreateErrorResponse<object>("Usuario requerido para procesar SINPE.", -1, null!);

            try
            {
                var servicio = _factory.CrearServicio(codEmpresa, filtro.usuario);
                var results = new List<ErrorDto>(capacity: transaccionesList.Count);

                foreach (var trx in transaccionesList)
                {
                    if (!TryValidarSinpe(servicio, codEmpresa, filtro.usuario, trx, results))
                        continue;

                    EmitirSinpe(servicio, codEmpresa, filtro.usuario, trx);
                }

                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new { results }, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message, -1, null!);
            }
        }

        private static bool TryValidarSinpe(IWfcSinpe servicio, int codEmpresa, string usuario, TesTransaccionDto trx, List<ErrorDto> results)
        {
            var nsol = trx.nsolicitud;

            var valida = servicio.fxValidacionSinpe(codEmpresa, nsol.ToString(), usuario);
            if (valida is null)
                return true;

            results.Add(new ErrorDto
            {
                Code = valida.Code,
                Description = $"N° {nsol}-{valida.Description}"
            });

            return valida.Code != -1;
        }

        private static void EmitirSinpe(IWfcSinpe servicio, int codEmpresa, string usuario, TesTransaccionDto trx)
        {
            var now = DateTime.Now;

            if (codEmpresa == 61)
            {
                EmitirSinpeEmpresa61(servicio, codEmpresa, usuario, trx, now);
                return;
            }

            servicio.fxTesEmisionSinpeCreditoDirecto(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
        }

        private static void EmitirSinpeEmpresa61(IWfcSinpe servicio, int codEmpresa, string usuario, TesTransaccionDto trx, DateTime now)
        {
            switch (trx.tipo_girosinpe)
            {
                case "CD":
                    servicio.fxTesEmisionSinpeCreditoDirecto(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                    break;

                case "TR":
                    servicio.fxTesEmisionSinpeTiempoReal(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                    break;

                default:
                    break;
            }
        }

        private static void AsignarConFirmas(
    TesTransaccionDto item,
    string rutaConFirmas,
    EmisionClasificacionState state)
        {
            state.ReporteCkConFirmas = LimpiarReporte(rutaConFirmas);
            state.ListaConFirmas.Add(item);
        }

        private static void AsignarSinFirmas(
            TesTransaccionDto item,
            string rutaSinFirmas,
            EmisionClasificacionState state)
        {
            state.ReporteCkSinFirmas = LimpiarReporte(rutaSinFirmas);
            state.ListaSinFirmas.Add(item);
        }


        private ErrorDto<object> OkObj(object? result = null) => new()
        {
            Code = 0,
            Description = "",
            Result = result
        };

        private ErrorDto<object> ErrorObj(string msg) => new()
        {
            Code = -1,
            Description = msg,
            Result = null
        };

        #endregion
    }
}
