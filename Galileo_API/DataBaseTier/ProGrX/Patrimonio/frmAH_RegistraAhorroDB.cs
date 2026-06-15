using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhRegistraAhorroDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrx;
        private readonly MCobroDb _mCobro;
        private readonly MRecibos _mRecibos;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 2;

        public FrmAhRegistraAhorroDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
            _mCobro = new MCobroDb(config);
            _mRecibos = new MRecibos(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Carga la información inicial del modal de registro de aportes:
        /// socio, rubros disponibles, fechas de proceso, documentos de caja y ticket.
        /// </summary>
        public ErrorDto<FrmAhRegistraAhorroCargarResponse> AH_RegistraAhorro_Cargar(
            int codEmpresa,
            FrmAhRegistraAhorroCargarRequest request)
        {
            var response = new FrmAhRegistraAhorroCargarResponse();

            if (request == null)
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, response);

            var cedula = AH_RegistraAhorro_NormalizarTexto(request.cedula, 30);
            var usuario = AH_RegistraAhorro_NormalizarTexto(request.usuario, 50);
            var codCaja = AH_RegistraAhorro_NormalizarTexto(request.cod_caja, 20);

            if (string.IsNullOrWhiteSpace(cedula))
                return DbHelper.CreateErrorResponse("La cédula es requerida.", -2, response);

            if (string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse("El usuario es requerido.", -2, response);

            if (string.IsNullOrWhiteSpace(codCaja))
                return DbHelper.CreateErrorResponse("La caja es requerida.", -2, response);

            var acceso = _mProGrx.fxSys_RA_Consulta(codEmpresa, cedula, usuario);
            if (acceso.Code < 0)
                return DbHelper.CreateErrorResponse(acceso.Description ?? "No fue posible validar el acceso restringido.", acceso.Code ?? -1, response);

            if (!acceso.Result)
            {
                return DbHelper.CreateErrorResponse(
                    "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!",
                    -2,
                    response);
            }

            var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code < 0 || globalesResp.Result == null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code ?? -1,
                    response);
            }

            const string sqlSocio = @"
select top 1
    rtrim(Cedula) as cedula,
    rtrim(Nombre) as nombre,
    rtrim(isnull(EstadoActual, '')) as estado_actual,
    rtrim(isnull((select top 1 COD_DIVISA from vSys_Divisas where DIVISA_LOCAL = 1), '')) as cod_divisa,
    isnull(dbo.fxCajas_Valida_Auxiliar(@codCaja, 'PAT', ''), 0) as caja_valida_concepto,
    isnull(dbo.fxPAT_Info_Aporte_Manual(Cedula), 0) as aporte_manual
from Socios
where Cedula = @cedula;";

            const string sqlDocumentos = @"
select
    rtrim(C.Tipo_Documento) as idx,
    rtrim(D.Descripcion) as itmx
from CAJAS_DOCUMENTOS C
inner join SIF_DOCUMENTOS D
    on D.Tipo_Documento = C.Tipo_Documento
where
    C.Cod_Caja = @codCaja
    and D.Tipo_Movimiento in ('A', 'D')
order by C.Tipo_Documento;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var socio = conn.QueryFirstOrDefault<FrmAhRegistraAhorroSocioDto>(
                    sqlSocio,
                    new { cedula, codCaja });

                if (socio == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se localizó la persona indicada.",
                        -2,
                        response);
                }

                var tiposDocumento = conn.Query<SifDocumentosDto>(
                    sqlDocumentos,
                    new { codCaja }).ToList();

                var rubros = AH_RegistraAhorro_ConstruirRubros(
                    socio.estado_actual,
                    socio.aporte_manual);

                var procesos = AH_RegistraAhorro_ConstruirProcesos(
                    codEmpresa,
                    globalesResp.Result.GlngFechaCR);

                var rubroDefault = rubros.FirstOrDefault();
                var procesoDefault = procesos.FirstOrDefault();
                var tipoDocumentoDefault = tiposDocumento.FirstOrDefault();

                response.cedula = socio.cedula;
                response.nombre = socio.nombre;
                response.cod_divisa = socio.cod_divisa;
                response.estado_actual = socio.estado_actual;
                response.aporte_manual = socio.aporte_manual;
                response.caja_valida_concepto = socio.caja_valida_concepto > 0;
                response.caja_validacion_mensaje = response.caja_valida_concepto
                    ? string.Empty
                    : "Esta caja no está autorizada para registrar movimientos a este Plan/Fondo.";
                response.tiquete = AH_RegistraAhorro_CrearTiquete(socio.cedula);
                response.tipo_rubro_default = rubroDefault?.idx ?? string.Empty;
                response.fecha_proceso_default = procesoDefault?.idx ?? string.Empty;
                response.tipo_documento_default = tipoDocumentoDefault?.idx ?? string.Empty;
                response.aporte_autorizado_default = rubroDefault?.aporte_autorizado ?? 0;
                response.rubros = rubros;
                response.procesos = procesos;
                response.tipos_documento = tiposDocumento;

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Registra la solicitud de autorización para el rubro y monto seleccionados.
        /// </summary>
        public ErrorDto<FrmAhRegistraAhorroGestionResponse> AH_RegistraAhorro_Gestion_Registrar(
            int codEmpresa,
            FrmAhRegistraAhorroGestionRegistrarRequest request)
        {
            var response = new FrmAhRegistraAhorroGestionResponse();

            if (request == null)
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, response);

            var cedula = AH_RegistraAhorro_NormalizarTexto(request.cedula, 30);
            var tipo = AH_RegistraAhorro_NormalizarTipoRubro(request.tipo);
            var usuario = AH_RegistraAhorro_NormalizarTexto(request.usuario, 50);

            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(usuario))
                return DbHelper.CreateErrorResponse("Los datos de la gestión son requeridos.", -2, response);

            const string sql = @"
exec spPAT_Gestion_Registro
    @Cedula,
    @Tipo,
    @MntCal,
    @MntSol,
    @Usuario;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                response,
                new
                {
                    Cedula = cedula,
                    Tipo = tipo,
                    MntCal = request.mnt_cal,
                    MntSol = request.mnt_sol,
                    Usuario = usuario
                });
        }

        /// <summary>
        /// Consulta el estado actual de una gestión de autorización.
        /// </summary>
        public ErrorDto<FrmAhRegistraAhorroGestionResponse> AH_RegistraAhorro_Gestion_Estado(
            int codEmpresa,
            int gestionId)
        {
            var response = new FrmAhRegistraAhorroGestionResponse();

            if (gestionId <= 0)
                return DbHelper.CreateErrorResponse("Debe indicar una gestión válida.", -2, response);

            const string sql = @"
exec spPAT_Gestion_Estado
    @GestionId;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                response,
                new { GestionId = gestionId });
        }

        /// <summary>
        /// Aplica la aportación de patrimonio usando el resultado final de Caja y la autorización correspondiente.
        /// </summary>
        public ErrorDto<FrmAhRegistraAhorroAplicarResponse> AH_RegistraAhorro_Aplicar(
            int codEmpresa,
            FrmAhRegistraAhorroAplicarRequest request)
        {
            var response = new FrmAhRegistraAhorroAplicarResponse();

            var validacion = AH_RegistraAhorro_ValidarAplicarRequest(request, response);
            if (validacion != null)
                return validacion;

            var cedula = AH_RegistraAhorro_NormalizarTexto(request.cedula, 30);
            var nombre = AH_RegistraAhorro_NormalizarTexto(request.nombre, 150);
            var usuario = AH_RegistraAhorro_NormalizarTexto(request.usuario, 50);
            var codCaja = AH_RegistraAhorro_NormalizarTexto(request.cod_caja, 20);
            var tipoRubro = AH_RegistraAhorro_NormalizarTipoRubro(request.tipo_rubro);
            var fechaProceso = AH_RegistraAhorro_NormalizarTexto(request.fecha_proceso, 20);
            var tipoDocumento = AH_RegistraAhorro_NormalizarTexto(request.tipo_documento, 20);
            var tiquete = AH_RegistraAhorro_NormalizarTexto(request.tiquete, 80);
            var notas = AH_RegistraAhorro_NormalizarTexto(request.notas, 500);
            var montoAplicar = request.total_cajas > 0 ? request.total_cajas : request.monto;

            var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code < 0 || globalesResp.Result == null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code ?? -1,
                    response);
            }

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var cajaValidacion = conn.QueryFirstOrDefault<CajasTransacValidacionResult>(@"
exec spCajas_Transac_Validacion
    @Caja,
    @Usuario,
    @Apertura,
    @SesionId,
    @TipoProc,
    @Producto,
    @Monto,
    @Ticket;",
                    new
                    {
                        Caja = codCaja,
                        Usuario = usuario,
                        Apertura = request.apertura,
                        SesionId = request.sesion_id,
                        TipoProc = "PAT",
                        Producto = tipoRubro,
                        Monto = montoAplicar,
                        Ticket = tiquete
                    },
                    tx) ?? new CajasTransacValidacionResult();

                if (!string.IsNullOrWhiteSpace(cajaValidacion.Validacion))
                {
                    return DbHelper.CreateErrorResponse(
                        cajaValidacion.Validacion.Trim(),
                        -2,
                        response);
                }

                response.advertencias = AH_RegistraAhorro_NormalizarTexto(cajaValidacion.Advertencias, 500);

                var numDocumento = _mRecibos
                    .FxDocumentoConsecutivo(codEmpresa, tipoDocumento)
                    .ToString(CultureInfo.InvariantCulture);

                AH_RegistraAhorro_InsertarTransaccion(
                    conn,
                    tx,
                    request,
                    nombre,
                    numDocumento,
                    montoAplicar,
                    globalesResp.Result.GOficinaTitular);

                conn.Execute(@"
exec spCajas_DesglocePagosDocFinal
    @Caja,
    @Apertura,
    @Ticket,
    @Usuario,
    @TipoDoc,
    @NumDoc,
    @Unidad,
    @Ref_01,
    @Ref_02;",
                    new
                    {
                        Caja = codCaja,
                        Apertura = request.apertura,
                        Ticket = tiquete,
                        Usuario = usuario,
                        TipoDoc = tipoDocumento,
                        NumDoc = numDocumento,
                        Unidad = globalesResp.Result.GOficinaUnidad,
                        Ref_01 = cedula,
                        Ref_02 = string.Empty
                    },
                    tx);

                conn.Execute(@"
exec spPAT_Aportacion
    @Cedula,
    @Tipo,
    @Aporte,
    @TipoDoc,
    @NumDoc,
    @Usuario,
    @Caja,
    @Concepto,
    @Asiento,
    @Proceso;",
                    new
                    {
                        Cedula = cedula,
                        Tipo = tipoRubro,
                        Aporte = montoAplicar,
                        TipoDoc = tipoDocumento,
                        NumDoc = numDocumento,
                        Usuario = usuario,
                        Caja = codCaja,
                        Concepto = "PAT001",
                        Asiento = 0,
                        Proceso = fechaProceso
                    },
                    tx);

                if (!request.es_ajuste
                    && request.gestion_id > 0
                    && AH_RegistraAhorro_EsAutorizada(request.gestion_estado))
                {
                    conn.Execute(@"
exec spPAT_Autorizaciones_Aplica
    @GestionId,
    @TCon,
    @Ncon,
    @Usuario;",
                        new
                        {
                            GestionId = request.gestion_id,
                            TCon = tipoDocumento,
                            Ncon = numDocumento,
                            Usuario = usuario
                        },
                        tx);
                }

                tx.Commit();

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Movimiento = "Registra",
                    Modulo = vModulo,
                    DetalleMovimiento = $"Patrimonio {tipoRubro} Ced:{cedula} Mto:{montoAplicar:N2} Doc:{tipoDocumento}:{numDocumento}"
                });

                if (request.recibo_digital)
                {
                    DbHelper.ExecuteNonQuery(
                        _portalDb,
                        codEmpresa,
                        "exec spCajasReciboDigital @NumeroDocumento, @TipoDocumento, @TipoComprobante;",
                        new
                        {
                            NumeroDocumento = numDocumento,
                            TipoDocumento = tipoDocumento,
                            TipoComprobante = "Patrimonio"
                        });

                    response.recibo_digital_enviado = true;
                }

                var impresion = _mRecibos.sbImprimeRecibo(codEmpresa, numDocumento, tipoDocumento, usuario);

                response.tipo_documento = tipoDocumento;
                response.num_documento = numDocumento;
                response.monto_aplicado = montoAplicar;
                response.reporte_resultado = impresion.Code == -1 ? null : impresion.Result?.ToString();
                response.mensaje = impresion.Code == -1
                    ? $"Aporte aplicado con {tipoDocumento}:{numDocumento}, pero no se pudo generar el recibo: {impresion.Description}"
                    : $"Aporte aplicado correctamente con {tipoDocumento}:{numDocumento}.";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                string rollbackError = null;
                try
                {
                    tx.Rollback();
                }
                catch (Exception rollbackEx)
                {
                    rollbackError = rollbackEx.Message;
                }

                var errorMessage = rollbackError == null
                    ? ex.Message
                    : $"{ex.Message} | Rollback failed: {rollbackError}";

                return DbHelper.CreateErrorResponse(errorMessage, -1, response);
            }
        }

        private static void AH_RegistraAhorro_InsertarTransaccion(
            SqlConnection conn,
            SqlTransaction tx,
            FrmAhRegistraAhorroAplicarRequest request,
            string nombre,
            string numDocumento,
            decimal montoAplicar,
            string oficinaTitular)
        {
            var lineas = new[]
            {
                $"Plan            : {request.tipo_rubro}",
                $"Proceso         : {request.fecha_proceso}",
                $"Monto Aplicado  : {montoAplicar:N2}",
                $"Usuario         : {request.usuario}"
            };

            conn.Execute(@"
insert into SIF_TRANSACCIONES(
    COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
    Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
    Referencia_01, Referencia_02, Referencia_03, cod_oficina,
    linea1, linea2, linea3, linea4, detalle, documento,
    cod_caja, cod_apertura, id_sesion
)
values(
    @Cod_Transaccion, @Tipo_Documento, dbo.MyGetdate(), @Registro_Usuario,
    @Cliente_Identificacion, @Cliente_Nombre, @Cod_Concepto, @Monto, 'P',
    @Referencia_01, @Referencia_02, @Referencia_03, @Cod_Oficina,
    @Linea1, @Linea2, @Linea3, @Linea4, @Detalle, @Documento,
    @Cod_Caja, @Cod_Apertura, @Id_Sesion
);",
                new
                {
                    Cod_Transaccion = numDocumento,
                    Tipo_Documento = request.tipo_documento,
                    Registro_Usuario = request.usuario,
                    Cliente_Identificacion = request.cedula,
                    Cliente_Nombre = nombre,
                    Cod_Concepto = "PAT001",
                    Monto = montoAplicar,
                    Referencia_01 = request.cedula,
                    Referencia_02 = string.Empty,
                    Referencia_03 = string.Empty,
                    Cod_Oficina = oficinaTitular,
                    Linea1 = lineas[0],
                    Linea2 = lineas[1],
                    Linea3 = lineas[2],
                    Linea4 = lineas[3],
                    Detalle = string.IsNullOrWhiteSpace(request.notas) ? "Aporte a Patrimonio" : request.notas.Trim(),
                    Documento = string.Empty,
                    Cod_Caja = request.cod_caja,
                    Cod_Apertura = request.apertura,
                    Id_Sesion = request.sesion_id
                },
                tx);
        }

        private static List<FrmAhRegistraAhorroRubroDto> AH_RegistraAhorro_ConstruirRubros(
            string estadoActual,
            decimal aporteManual)
        {
            var estado = AH_RegistraAhorro_NormalizarTexto(estadoActual, 5).ToUpperInvariant();
            var rubros = new List<FrmAhRegistraAhorroRubroDto>();

            if (estado == "S")
            {
                rubros.Add(new FrmAhRegistraAhorroRubroDto
                {
                    idx = "O",
                    itmx = "Aporte Obrero",
                    aporte_autorizado = aporteManual,
                    requiere_autorizacion = true
                });
                rubros.Add(new FrmAhRegistraAhorroRubroDto
                {
                    idx = "P",
                    itmx = "Aporte Patronal",
                    aporte_autorizado = 0,
                    requiere_autorizacion = false
                });
                rubros.Add(new FrmAhRegistraAhorroRubroDto
                {
                    idx = "C",
                    itmx = "Capitalización",
                    aporte_autorizado = 0,
                    requiere_autorizacion = false
                });
            }
            else if (estado == "A")
            {
                rubros.Add(new FrmAhRegistraAhorroRubroDto
                {
                    idx = "X",
                    itmx = "Aporte en Custodia",
                    aporte_autorizado = 0,
                    requiere_autorizacion = false
                });
            }

            return rubros;
        }

        private List<SifDocumentosDto> AH_RegistraAhorro_ConstruirProcesos(
            int codEmpresa,
            decimal procesoBase)
        {
            var result = new List<SifDocumentosDto>();
            if (procesoBase <= 0)
                return result;

            var cursor = procesoBase;
            for (var i = 0; i < 6; i++)
            {
                result.Add(new SifDocumentosDto
                {
                    idx = cursor.ToString("0", CultureInfo.InvariantCulture),
                    itmx = cursor.ToString("0", CultureInfo.InvariantCulture)
                });

                cursor = _mCobro.fxFechaProcesoAnterior(codEmpresa, cursor);
                if (cursor <= 0)
                    break;
            }

            return result;
        }

        private static ErrorDto<FrmAhRegistraAhorroAplicarResponse>? AH_RegistraAhorro_ValidarAplicarRequest(
            FrmAhRegistraAhorroAplicarRequest request,
            FrmAhRegistraAhorroAplicarResponse response)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, response);

            if (string.IsNullOrWhiteSpace(request.cedula)
                || string.IsNullOrWhiteSpace(request.usuario)
                || string.IsNullOrWhiteSpace(request.cod_caja)
                || string.IsNullOrWhiteSpace(request.tipo_rubro)
                || string.IsNullOrWhiteSpace(request.fecha_proceso)
                || string.IsNullOrWhiteSpace(request.tipo_documento)
                || string.IsNullOrWhiteSpace(request.tiquete))
            {
                return DbHelper.CreateErrorResponse("Faltan datos requeridos para aplicar el aporte.", -2, response);
            }

            if (request.apertura <= 0 || request.sesion_id <= 0)
                return DbHelper.CreateErrorResponse("La apertura y la sesión de caja son requeridas.", -2, response);

            if (request.total_cajas <= 0)
                return DbHelper.CreateErrorResponse("No se especificó ningún monto detallado en Caja.", -2, response);

            if (!request.es_ajuste
                && request.aporte_autorizado != request.total_cajas
                && !AH_RegistraAhorro_EsAutorizada(request.gestion_estado))
            {
                return DbHelper.CreateErrorResponse(
                    "Este movimiento requiere AUTORIZACION, verifique el estado de la misma y/o solicite una.",
                    -2,
                    response);
            }

            return null;
        }

        private static bool AH_RegistraAhorro_EsAutorizada(string? gestionEstado)
        {
            return !string.IsNullOrWhiteSpace(gestionEstado)
                && gestionEstado.Trim().StartsWith("A", StringComparison.OrdinalIgnoreCase);
        }

        private static string AH_RegistraAhorro_NormalizarTipoRubro(string? tipoRubro)
        {
            var tipo = AH_RegistraAhorro_NormalizarTexto(tipoRubro, 5).ToUpperInvariant();
            return tipo switch
            {
                "O" or "P" or "C" or "X" => tipo,
                _ => tipo
            };
        }

        private static string AH_RegistraAhorro_NormalizarTexto(string? value, int maxLength)
        {
            var text = (value ?? string.Empty).Trim();
            return text.Length <= maxLength ? text : text[..maxLength];
        }

        private static string AH_RegistraAhorro_CrearTiquete(string cedula)
        {
            return $"PId.{cedula}.{DateTime.Now:HH:mm:ss}";
        }
    }
}
