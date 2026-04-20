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

                var lista = conn.Query<CoAplExcContratosRegistroCreditoRow>(
                    sql,
                    new { Cedula = cedula },
                    commandTimeout: 0).ToList();

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

                if (request.id_solicitud <= 0)
                {
                    return DbHelper.CreateErrorResponse<CoAplExcContratosRegistroGuardarResponse>(
                        "Debe indicar la operación.");
                }

                var sql = @"
exec spCbr_Excedente_Apl_Contratos_Add
    @ContratoId,
    @Cedula,
    @FechaVence,
    @Estado,
    @Operacion,
    @Notas,
    @Usuario;";

                var dbResp = conn.QueryFirstOrDefault<CoAplExcContratosRegistroGuardarDbResponse>(
                    sql,
                    new
                    {
                        ContratoId = request.id_contrato,
                        Cedula = cedula,
                        FechaVence = request.fecha_vencimiento,
                        Estado = request.estado,
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

        private sealed class CoAplExcContratosRegistroGuardarDbResponse
        {
            public int Pass { get; set; }
            public long ContratoId { get; set; }
            public string? Movimiento { get; set; }
            public string? Mensaje { get; set; }
        }
    }
}
