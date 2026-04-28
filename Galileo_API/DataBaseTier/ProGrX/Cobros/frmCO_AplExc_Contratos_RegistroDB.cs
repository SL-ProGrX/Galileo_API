using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros.Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAplExcContratosRegistroDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCOAplExcContratosRegistroDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data">Información de bitácora a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene el listado principal de contratos para aplicación de excedentes a mora.
        /// Equivale al listado lsw del formulario VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de búsqueda.</param>
        /// <returns>Lista de contratos.</returns>
        public ErrorDto<List<CoAplExcContratosRegistroListaRow>> CO_AplExc_Contratos_Registro_Lista_Obtener(
            int codEmpresa,
            CoAplExcContratosRegistroListaRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var filtro = (request.filtro ?? string.Empty).Trim();
                var estado = request.estado;
                var lineas = request.lineas <= 0 ? 1000 : request.lineas;

                var sql = @"exec spCbr_Excedente_Apl_Contratos_List @Filtro, @Estado, @Lineas;";

                var lista = conn.Query<CoAplExcContratosRegistroListaRow>(
                    sql,
                    new
                    {
                        Filtro = filtro,
                        Estado = estado,
                        Lineas = lineas
                    },
                    commandTimeout: 0).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CoAplExcContratosRegistroListaRow>>(
                    ex.Message,
                    -1,
                    new List<CoAplExcContratosRegistroListaRow>());
            }
        }

        /// <summary>
        /// Obtiene la información de un contrato específico.
        /// Equivale a sbConsulta del formulario VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Llaves de consulta del contrato.</param>
        /// <returns>Información del contrato.</returns>
        public ErrorDto<CoAplExcContratosRegistroData> CO_AplExc_Contratos_Registro_Obtener(
            int codEmpresa,
            CoAplExcContratosRegistroConsultaRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var sql = @"exec spCbr_Excedente_Apl_Contratos_Consulta @Contrato, @Operacion;";

                var data = conn.QueryFirstOrDefault<CoAplExcContratosRegistroData>(
                    sql,
                    new
                    {
                        Contrato = request.id_contrato,
                        Operacion = request.id_solicitud
                    },
                    commandTimeout: 0);

                if (data is null)
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroData>(
                        "No se encontró información del contrato.");
                }

                return DbHelper.CreateOkResponse(data);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroData>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los créditos activos de una persona para asociarlos al contrato.
        /// Equivale al listado lswCreditos del formulario VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Cédula de la persona.</param>
        /// <returns>Lista de créditos activos.</returns>
        public ErrorDto<List<CoAplExcContratosRegistroCreditoRow>> CO_AplExc_Contratos_Registro_Creditos_Obtener(
            int codEmpresa,
            CoAplExcContratosRegistroCreditosRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var cedula = (request.cedula ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<List<CoAplExcContratosRegistroCreditoRow>>(
                        "Debe indicar la identificación.",
                        -1,
                        new List<CoAplExcContratosRegistroCreditoRow>());
                }

                var sql = @"exec spCBR_Excedente_Apl_Contratos_Persona_Creditos_Activos @Cedula;";

                var dbRows = conn.Query<CoAplExcContratosRegistroCreditoDbRow>(
                        sql,
                        new { Cedula = cedula },
                        commandTimeout: 0).ToList();

                var lista = dbRows.Select(x => new CoAplExcContratosRegistroCreditoRow
                {
                    estado_desc = x.estado_desc,
                    id_contrato = x.id_contrato,
                    codigo = x.codigo,
                    id_solicitud = x.id_solicitud,
                    saldo = x.saldo,
                    fecha_vencimiento = DateTime.TryParseExact(
                        x.fecha_vencimiento,
                        "dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var fecha
                    ) ? fecha : null,
                    fecha_firma = DateTime.TryParseExact(
                        x.fecha_firma,
                        "dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out var fechaFirma
                    ) ? fechaFirma : null
                }).ToList();



                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CoAplExcContratosRegistroCreditoRow>>(
                    ex.Message,
                    -1,
                    new List<CoAplExcContratosRegistroCreditoRow>());
            }
        }

        /// <summary>
        /// Guarda o actualiza un contrato para aplicación de excedentes a mora.
        /// Equivale a sbGuardar del formulario VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del contrato a guardar.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto<CoAplExcContratosRegistroGuardarResponse> CO_AplExc_Contratos_Registro_Guardar(
            int codEmpresa,
            CoAplExcContratosRegistroGuardarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var cedula = (request.cedula ?? string.Empty).Trim();
                var observaciones = (request.observaciones ?? string.Empty).Trim();
                var usuario = (request.usuario ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroGuardarResponse>(
                        "Debe indicar la identificación.");
                }

                var sql = @"
exec spCbr_Excedente_Apl_Contratos_Add
    @Contrato,
    @Cedula,
    @Firma,
    @Vence,
    @Activo,
    @Operacion,
    @Notas,
    @Usuario;";

                var dbResp = conn.QueryFirstOrDefault<CoAplExcContratosRegistroGuardarDbResponse>(
                    sql,
                    new
                    {
                        Contrato = request.id_contrato,
                        Cedula = cedula,
                        Firma = request.firma_contrato,
                        Vence = request.fecha_vencimiento,
                        Activo = request.estado,
                        Operacion = request.id_solicitud,
                        Notas = observaciones,
                        Usuario = usuario
                    },
                    commandTimeout: 0);

                if (dbResp is null)
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroGuardarResponse>(
                        "No se obtuvo respuesta al guardar el contrato.");
                }

                var result = new CoAplExcContratosRegistroGuardarResponse
                {
                    pass = dbResp.Pass == 1,
                    contrato_id = dbResp.ContratoId,
                    movimiento = dbResp.Movimiento ?? string.Empty,
                    mensaje = dbResp.Mensaje ?? string.Empty
                };

                if (!result.pass)
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroGuardarResponse>(
                        result.mensaje,
                        -1,
                        result);
                }

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = result.mensaje,
                    Movimiento = $"{result.movimiento} - WEB",
                    Modulo = 4
                });

                return DbHelper.CreateOkResponse<CoAplExcContratosRegistroGuardarResponse>(result, result.mensaje);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroGuardarResponse>(ex.Message);
            }
        }


        /// <summary>
        /// Obtiene la lista de personas para buscador F4 del registro de contratos.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Texto de búsqueda opcional.</param>
        /// <returns>Lista de personas.</returns>
        public ErrorDto<List<CoAplExcContratosRegistroPersonaF4Row>> CO_AplExc_Contratos_Registro_Personas_F4_Obtener(
            int codEmpresa,
            CoAplExcContratosRegistroPersonaF4Request request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var texto = (request.texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : string.Empty;

                const string sql = @"
            SELECT
                RTRIM(ISNULL(CEDULA, '')) AS cedula,
                RTRIM(ISNULL(CEDULAR, '')) AS cedular,
                RTRIM(ISNULL(NOMBRE, '')) AS nombre
            FROM SOCIOS
            WHERE @texto = ''
               OR RTRIM(ISNULL(CEDULA, '')) LIKE @like
               OR RTRIM(ISNULL(CEDULAR, '')) LIKE @like
               OR RTRIM(ISNULL(NOMBRE, '')) LIKE @like
            ORDER BY NOMBRE;";

                var lista = conn.Query<CoAplExcContratosRegistroPersonaF4Row>(
                    sql,
                    new
                    {
                        texto,
                        like
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CoAplExcContratosRegistroPersonaF4Row>>(
                    ex.Message,
                    -1,
                    new List<CoAplExcContratosRegistroPersonaF4Row>());
            }
        }
   
    
        /// <summary>
/// Procesa la carga masiva de contratos desde archivo Excel.
/// Replica el comportamiento del VB6 ejecutando el procedimiento por cada fila.
/// </summary>
/// <param name="codEmpresa">Código de empresa.</param>
/// <param name="request">Detalle de registros a procesar.</param>
/// <returns>Resultado de la carga lote.</returns>
public ErrorDto<CoAplExcContratosRegistroCargaLoteResponse> CO_AplExc_Contratos_Registro_Carga_Lote(
    int codEmpresa,
    CoAplExcContratosRegistroCargaLoteRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            conn.Open();
            using var trx = conn.BeginTransaction();

            try
            {
                var usuario = (request.usuario ?? string.Empty).Trim().ToUpperInvariant();
                var detalle = request.detalle ?? new List<CoAplExcContratosRegistroCargaLoteRow>();

                if (string.IsNullOrWhiteSpace(usuario))
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(
                        "Debe indicar el usuario.");
                }

                if (!detalle.Any())
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(
                        "No se recibieron registros para procesar.");
                }

                var procesados = 0;
                string ultimaCedula = string.Empty;
                long ultimaOperacion = 0;

                foreach (var item in detalle)
                {
                    var cedula = (item.cedula ?? string.Empty).Trim();
                    var notas = (item.notas ?? string.Empty).Trim();
                    var operacion = item.operacion;

                    if (string.IsNullOrWhiteSpace(cedula))
                    {
                        trx.Rollback();
                        return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(
                            "Existe al menos una fila sin cédula.");
                    }

                    if (operacion <= 0)
                    {
                        trx.Rollback();
                        return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(
                            $"La operación de la cédula {cedula} no es válida.");
                    }

                    const string sql = @"
                        exec spCbr_Excedente_Apl_Contratos_Add_Lote
                            @Cedula,
                            @Operacion,
                            @Notas,
                            @Usuario;";

                    var dbResp = conn.QueryFirstOrDefault<CoAplExcContratosRegistroGuardarDbResponse>(
                        sql,
                        new
                        {
                            Cedula = cedula,
                            Operacion = operacion,
                            Notas = notas,
                            Usuario = usuario
                        },
                        transaction: trx,
                        commandTimeout: 0);

                    if (dbResp is null)
                    {
                        trx.Rollback();
                        return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(
                            $"No se obtuvo respuesta al procesar la cédula {cedula} y operación {operacion}.");
                    }

                    if (dbResp.Pass != 1)
                    {
                        trx.Rollback();

                        return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(
                            dbResp.Mensaje ?? $"Error al procesar la cédula {cedula} y operación {operacion}.",
                            -1,
                            new CoAplExcContratosRegistroCargaLoteResponse
                            {
                                pass = false,
                                mensaje = dbResp.Mensaje ?? string.Empty,
                                procesados = procesados,
                                ultima_cedula = cedula,
                                ultima_operacion = operacion
                            });
                    }

                    _securityMainDb.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = codEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = dbResp.Mensaje ?? string.Empty,
                        Movimiento = $"{dbResp.Movimiento ?? "CARGA LOTE"} - WEB",
                        Modulo = 4
                    });

                    procesados++;
                    ultimaCedula = cedula;
                    ultimaOperacion = operacion;
                }

                trx.Commit();

                return DbHelper.CreateOkResponse(
                    new CoAplExcContratosRegistroCargaLoteResponse
                    {
                        pass = true,
                        mensaje = "Información cargada satisfactoriamente.",
                        procesados = procesados,
                        ultima_cedula = ultimaCedula,
                        ultima_operacion = ultimaOperacion
                    },
                    "Información cargada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                trx.Rollback();
                return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                trx.Rollback();
                return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroCargaLoteResponse>(ex.Message);
            }
        }

    }
}
