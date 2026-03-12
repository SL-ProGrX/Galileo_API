using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizaReclamoDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizaReclamoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Retorna la lista de motivos de póliza para el formulario frmPoliza_Reclamo.
        /// Ejecuta el SP spPolizas_Motivos según el código de póliza recibido.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codPoliza">Código de póliza.</param>
        /// <returns>Lista de motivos en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Motivos_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(codPoliza))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spPolizas_Motivos @CodPoliza";

                var dataMotivo = conn.Query<dynamic>(
                    query,
                    new { CodPoliza = codPoliza.Trim() }
                ).ToList();

                var resultMotivo = dataMotivo.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return resultMotivo;
            });
        }

        /// <summary>
        /// Retorna la lista de causas de póliza para el formulario frmPoliza_Reclamo.
        /// Ejecuta el SP spPolizas_Causas según el código de póliza recibido.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codPoliza">Código de póliza.</param>
        /// <returns>Lista de causas en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Causas_Lista(
            int codEmpresa,
            string codPoliza)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(codPoliza))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spPolizas_Causas @CodPoliza";

                var dataCausa = conn.Query<dynamic>(
                    query,
                    new { CodPoliza = codPoliza.Trim() }
                ).ToList();

                var resultCausa = dataCausa.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return resultCausa;
            });
        }


        /// <summary>
        /// Retorna la lista de estados activos para el seguimiento
        /// del formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Lista de estados activos en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Estados_Lista(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        ID_ESTADO AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM POLIZAS_RECLAMOS_ESTADOS
                    WHERE ACTIVO = 1
                    ORDER BY Descripcion";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Retorna la lista de bancos disponibles para el usuario logueado
        /// en el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario logueado.</param>
        /// <returns>Lista de bancos en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Bancos_Lista(
            int codEmpresa,
            string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(usuario))
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spCrd_SGT_Bancos @Usuario";

                var dataBanco = conn.Query<dynamic>(
                    query,
                    new { Usuario = usuario.Trim() }
                ).ToList();

                var resultBanco = dataBanco.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return resultBanco;
            });
        }


        /// <summary>
        /// Retorna la lista de cuentas bancarias disponibles para una persona
        /// según la cédula y el banco seleccionado en frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="bancoId">Id del banco seleccionado.</param>
        /// <returns>Lista de cuentas bancarias en formato item / descripcion.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Poliza_Reclamo_Cuentas_Lista(
            int codEmpresa,
            string cedula,
            int bancoId)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (string.IsNullOrWhiteSpace(cedula) || bancoId <= 0)
                    return new List<DropDownListaGenericaModel>();

                const string query = @"EXEC spSys_Cuentas_Bancarias @Cedula, @BancoId, @Tipo";

                var dataCuentas = conn.Query<dynamic>(
                    query,
                    new
                    {
                        Cedula = cedula.Trim(),
                        BancoId = bancoId,
                        Tipo = 1
                    }
                ).ToList();

                var resultCuentas = dataCuentas.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return resultCuentas;
            });
        }

        /// <summary>
        /// Carga la información completa de un reclamo existente
        /// para el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="reclamoId">Id del reclamo.</param>
        /// <returns>Datos del formulario.</returns>
        public ErrorDto<PolizaReclamoFormularioResponse> Poliza_Reclamo_Load(
            int codEmpresa,
            int reclamoId)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sql = @"EXEC spPoliza_Reclamo_Load @ReclamoId";


                var row = connection.QueryFirstOrDefault<PolizaReclamoLoadDbModel>(
                       sql,
                       new { ReclamoId = reclamoId }
                   );

                if (row == null)
                    return DbHelper.CreateOkResponse<PolizaReclamoFormularioResponse>();

                var tipoPoliza = row.TIPO_POLIZA ?? string.Empty;

                var response = new PolizaReclamoFormularioResponse
                {
                    reclamoId = row.ID,
                    operacionId = row.ID_SOLICITUD,
                    polizaId = row.ID_SOLICITUD_POLIZA,
                    polizaCodigo = row.CODIGO_POLIZA ?? string.Empty,

                    cedula = row.CEDULA ?? string.Empty,
                    apellido1 = row.PRIMER_APELLIDO ?? string.Empty,
                    apellido2 = row.SEGUNDO_APELLIDO ?? string.Empty,
                    nombre = row.NOMBRE ?? string.Empty,
                    fechaNacimiento = row.FECHA_NACIMIENTO,
                    sexo = row.SEXO ?? string.Empty,

                    estadoDescripcion = row.ESTADO_DESC ?? string.Empty,
                    polizaDescripcion = row.POLIZA_DESC ?? string.Empty,
                    tipoPoliza = tipoPoliza,

                    finca = row.FINCA ?? string.Empty,
                    edad = row.EDAD,

                    registroFecha = row.REGISTRO_FECHA,
                    registroUsuario = row.REGISTRO_USUARIO ?? string.Empty,
                    registroObservaciones = row.REGISTRO_OBSERVACIONES ?? string.Empty,

                    recepcionFecha = row.RECEPCION_FECHA,
                    recepcionUsuario = row.RECEPCION_USUARIO ?? string.Empty,
                    recepcionObservaciones = row.RECEPCION_OBSERVACIONES ?? string.Empty,
                    recepcionAplicada = row.RECEPCION_FECHA != null,

                    montoAprobado = row.MONTO_APROBADO ?? 0,
                    montoOperacion = row.MONTO_CREDITO ?? 0,
                    plan = row.COD_PLAN ?? string.Empty,
                    contrato = row.CODIGO_FONDO,

                    fondoGenerado = (row.I_FONDO_GENERADO ?? 0) == 1,
                    aportacionAplicada = (row.I_APORTACION_APLICADA ?? 0) == 1,

                    estadoActualId = row.ESTADO_ACTUAL,
                    formaDesembolsoId = row.FORMA_DESEMBOLSO,
                    metodoPagoId = row.METODO_PAGO,
                    motivoId = row.MOTIVO_ID,
                    enfermedadId = row.ENFERMEDAD,
                    siniestroId = row.TIPO_SINIESTRO,
                    causaId = row.CAUSA_ID,

                    mostrarVida = tipoPoliza == "V",
                    mostrarIncendio = tipoPoliza != "V",
                    esNuevo = false
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>($"Error al cargar el reclamo: {ex.Message}");
            }
        }


        /// <summary>
        /// Prepara la información inicial de un reclamo nuevo
        /// para el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Parámetros base del reclamo nuevo.</param>
        /// <returns>Datos iniciales del formulario.</returns>
        public ErrorDto<PolizaReclamoFormularioResponse> Poliza_Reclamo_Nuevo(
            int codEmpresa,
            PolizaReclamoRequestNuevo request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>("Request inválido.");

                if (string.IsNullOrWhiteSpace(request.cedula))
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>("Debe indicar la cédula.");

                if (request.operacion <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>("Debe indicar la operación.");

                if (request.poliza <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>("Debe indicar la póliza.");

                if (string.IsNullOrWhiteSpace(request.polizaCodigo))
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>("Debe indicar el código de póliza.");

                const string sql = @"EXEC spPoliza_Reclamo_Nuevo @Cedula, @Operacion, @Poliza, @PolizaCodigo";

                var row = connection.QueryFirstOrDefault<PolizaReclamoNuevoDbModel>(
                    sql,
                    new
                    {
                        Cedula = request.cedula.Trim(),
                        Operacion = request.operacion,
                        Poliza = request.poliza,
                        PolizaCodigo = request.polizaCodigo.Trim()
                    }
                );

                if (row == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>(
                        "No se encontró el registro de la persona o póliza.");

                if (row.Reclamo_Id > 0 || row.ID > 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>(
                        $"Existe un reclamo en proceso para esta póliza. Reclamo No. {row.Reclamo_Id}");

                var tipoPoliza = row.TIPO_POLIZA ?? string.Empty;

                var response = new PolizaReclamoFormularioResponse
                {
                    reclamoId = 0,
                    operacionId = row.ID_SOLICITUD,
                    polizaId = row.POLIZA_ID,
                    polizaCodigo = row.POLIZA_CODIGO ?? string.Empty,

                    cedula = row.CEDULA ?? string.Empty,
                    apellido1 = row.APELLIDO1 ?? string.Empty,
                    apellido2 = row.APELLIDO2 ?? string.Empty,
                    nombre = row.NOMBREV2 ?? string.Empty,
                    fechaNacimiento = row.FECHA_NAC,
                    sexo = row.SEXO ?? string.Empty,

                    estadoDescripcion = "Borrador",
                    polizaDescripcion = row.POLIZA_DESC ?? string.Empty,
                    tipoPoliza = tipoPoliza,

                    finca = row.Finca ?? string.Empty,
                    edad = row.EDAD,

                    registroFecha = null,
                    registroUsuario = string.Empty,
                    registroObservaciones = string.Empty,

                    recepcionFecha = null,
                    recepcionUsuario = string.Empty,
                    recepcionObservaciones = string.Empty,
                    recepcionAplicada = false,

                    montoAprobado = 0,
                    montoOperacion = row.SALDO_CREDITO ?? 0,
                    plan = string.Empty,
                    contrato = null,

                    fondoGenerado = false,
                    aportacionAplicada = false,

                    estadoActualId = null,
                    formaDesembolsoId = 1,
                    metodoPagoId = 1,
                    motivoId = null,
                    enfermedadId = null,
                    siniestroId = null,
                    causaId = null,

                    mostrarVida = tipoPoliza == "V",
                    mostrarIncendio = tipoPoliza != "V",
                    esNuevo = true
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoFormularioResponse>(
                    $"Error al preparar el reclamo nuevo: {ex.Message}");
            }
        }

        /// <summary>
        /// Retorna el histórico de seguimiento del reclamo
        /// para el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="reclamoId">Id del reclamo.</param>
        /// <returns>Lista de líneas de seguimiento.</returns>
        public ErrorDto<List<PolizaReclamoSeguimientoItemResponse>> Poliza_Reclamo_Seguimiento_Lista(
            int codEmpresa,
            int reclamoId)
        {
            if (reclamoId <= 0)
                return DbHelper.CreateErrorResponse<List<PolizaReclamoSeguimientoItemResponse>>("Debe indicar el reclamoId.");


            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"EXEC spPoliza_Reclamo_Seguimiento_List @ReclamoId";

                return conn.Query<PolizaReclamoSeguimientoItemResponse>(
                    sql,
                    new { ReclamoId = reclamoId }
                ).ToList();
            });
        }

        /// <summary>
        /// Retorna los movimientos del fondo del reclamo
        /// para el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="plan">Código de plan.</param>
        /// <param name="contrato">Código de contrato/fondo.</param>
        /// <returns>Lista de movimientos del fondo.</returns>
        public ErrorDto<List<PolizaReclamoFondoItemResponse>> Poliza_Reclamo_Fondo_Movimientos(
            int codEmpresa,
            string plan,
            int contrato)
        {
            if (string.IsNullOrWhiteSpace(plan))
                return DbHelper.CreateErrorResponse<List<PolizaReclamoFondoItemResponse>>("Debe indicar el plan.");

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"EXEC spPoliza_Reclamo_Fondo_Movimientos @Plan, @Contrato";

                return conn.Query<PolizaReclamoFondoItemResponse>(
                    sql,
                    new
                    {
                        Plan = plan.Trim(),
                        Contrato = contrato
                    }
                ).ToList();
            });
        }


        /// <summary>
        /// Retorna la lista de desembolsos del reclamo
        /// para el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="reclamoId">Id del reclamo.</param>
        /// <param name="plan">Código de plan.</param>
        /// <param name="contrato">Código de contrato/fondo.</param>
        /// <returns>Lista de desembolsos.</returns>
        public ErrorDto<List<PolizaReclamoDesembolsoItemResponse>> Poliza_Reclamo_Desembolsos_Consulta(
            int codEmpresa,
            int reclamoId,
            string plan,
            int contrato)
        {
            if (reclamoId <= 0)
                return DbHelper.CreateErrorResponse<List<PolizaReclamoDesembolsoItemResponse>>("Debe indicar el reclamoId.");

            if (string.IsNullOrWhiteSpace(plan))
                return DbHelper.CreateErrorResponse<List<PolizaReclamoDesembolsoItemResponse>>("Debe indicar el plan.");

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"EXEC spPoliza_Reclamo_Desembolsos_Consulta @ReclamoId, @Plan, @Contrato";

                return conn.Query<PolizaReclamoDesembolsoItemResponse>(
                    sql,
                    new
                    {
                        ReclamoId = reclamoId,
                        Plan = plan.Trim(),
                        Contrato = contrato
                    }
                ).ToList();
            });
        }


        /// <summary>
        /// Retorna la lista de etiquetas del reclamo
        /// para el formulario frmPoliza_Reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="reclamoId">Id del reclamo.</param>
        /// <returns>Lista de etiquetas.</returns>
        public ErrorDto<List<PolizaReclamoEtiquetaItemResponse>> Poliza_Reclamo_Etiquetas_Lista(
            int codEmpresa,
            int reclamoId)
        {
            if (reclamoId <= 0)
                return DbHelper.CreateErrorResponse<List<PolizaReclamoEtiquetaItemResponse>>("Debe indicar el reclamoId.");

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sql = @"EXEC spPoliza_Reclamo_Etiquetas_List @ReclamoId";

                return conn.Query<PolizaReclamoEtiquetaItemResponse>(
                    sql,
                    new { ReclamoId = reclamoId }
                ).ToList();
            });
        }

        /// <summary>
        /// Actualiza los datos de vida del reclamo.
        /// </summary>
        public ErrorDto Poliza_Reclamo_Actualiza_Datos_Vida(int codEmpresa, PolizaReclamoActualizarVidaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el reclamo.");

                const string sql = @"EXEC spPoliza_Reclamo_Actualiza_Datos_Vida
                       @ReclamoId, @MotivoId, @Enfermedad, @Edad, @Usuario";

                connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        MotivoId = request.motivo_id,
                        Enfermedad = request.enfermedad_id,
                        Edad = request.edad,
                        Usuario = request.usuario
                    });

                return DbHelper.OkResponse("Datos actualizados correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar datos de vida: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza los datos de incendio del reclamo.
        /// </summary>
        public ErrorDto Poliza_Reclamo_Actualiza_Datos_Incendio(int codEmpresa, PolizaReclamoActualizarIncendioRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el reclamo.");

                const string sql = @"EXEC spPoliza_Reclamo_Actualiza_Datos_Incendio
            @ReclamoId, @SiniestroId, @Causa, @Finca, @Usuario";

                connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        SiniestroId = request.siniestro_id,
                        Causa = request.causa_id,
                        Finca = request.finca?.Trim() ?? string.Empty,
                        Usuario = request.usuario
                    });


                return DbHelper.OkResponse("Datos actualizados correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar datos de incendio: {ex.Message}");
            }
        }

        /// <summary>
        /// Actualiza la recepción del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de recepción del reclamo.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto Poliza_Reclamo_Actualiza_Recepcion(
            int codEmpresa,
            PolizaReclamoActualizarRecepcionRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el reclamo.");

                if (string.IsNullOrWhiteSpace(request.observaciones) || request.observaciones.Trim().Length < 5)
                    return DbHelper.ErrorResponse("Indique una observación válida.");

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.ErrorResponse("Debe indicar el usuario.");

                const string sql = @"EXEC spPoliza_Reclamo_Actualiza_Recepcion
            @ReclamoId, @Fecha, @Observaciones, @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        Fecha = request.fecha,
                        Observaciones = request.observaciones.Trim(),
                        Usuario = request.usuario.Trim()
                    });

                if (row == null)
                    return DbHelper.ErrorResponse("No se obtuvo respuesta al actualizar la recepción.");

                return (row.Pass ?? 0) == 1
                    ? DbHelper.CreateOkResponse(row.Mensaje?.ToString() ?? "Recepción actualizada correctamente.")
                    : DbHelper.ErrorResponse(row.Mensaje?.ToString() ?? "No fue posible actualizar la recepción.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al actualizar la recepción: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra un seguimiento manual del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del seguimiento.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto Poliza_Reclamo_Seguimiento_Manual_Add(
            int codEmpresa,
            PolizaReclamoSeguimientoManualAddRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el reclamo.");

                if (request.estado_id <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el estado.");

                if (string.IsNullOrWhiteSpace(request.observaciones) || request.observaciones.Trim().Length < 5)
                    return DbHelper.ErrorResponse("Indique una observación válida.");

                if (request.i_correo == 1 && (string.IsNullOrWhiteSpace(request.destinatarios) || request.destinatarios.Trim().Length < 5))
                    return DbHelper.ErrorResponse("No ha indicado destinatarios válidos.");

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.ErrorResponse("Debe indicar el usuario.");

                const string sql = @"EXEC spPoliza_Reclamo_Seguimiento_Manual_Add
            @ReclamoId, @EstadoId, @Observaciones, @ICorreo, @Destinatarios, @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        EstadoId = request.estado_id,
                        Observaciones = request.observaciones.Trim(),
                        ICorreo = request.i_correo,
                        Destinatarios = request.destinatarios?.Trim() ?? string.Empty,
                        Usuario = request.usuario.Trim()
                    });

                if (row == null)
                    return DbHelper.ErrorResponse("No se obtuvo respuesta al registrar el seguimiento.");

                return (row.Pass ?? 0) == 1
                    ? DbHelper.CreateOkResponse(row.Mensaje?.ToString() ?? "Seguimiento registrado correctamente.")
                    : DbHelper.ErrorResponse(row.Mensaje?.ToString() ?? "No fue posible registrar el seguimiento.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al registrar el seguimiento: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera el fondo del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos para generar el fondo.</param>
        /// <returns>Plan, contrato y mensaje del proceso.</returns>
        public ErrorDto<PolizaReclamoFondoCrearResponse> Poliza_Reclamo_Fondo_Creacion(
            int codEmpresa,
            PolizaReclamoFondoCrearRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoCrearResponse>("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoCrearResponse>("Debe indicar el reclamo.");

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoCrearResponse>("Debe indicar el usuario.");

                const string sql = @"EXEC spPoliza_Reclamo_Fondo_Creacion @ReclamoId, @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        Usuario = request.usuario.Trim()
                    });

                if (row == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoCrearResponse>("No se obtuvo respuesta al generar el fondo.");

                if ((row.Pass ?? 0) != 1)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoCrearResponse>(
                        row.Mensaje?.ToString() ?? "No fue posible generar el fondo.");

                var response = new PolizaReclamoFondoCrearResponse
                {
                    cod_plan = row.Cod_Plan ?? string.Empty,
                    codigo_fondo = row.Codigo_Fondo,
                    mensaje = row.Mensaje?.ToString() ?? "Fondo generado correctamente."
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoFondoCrearResponse>(
                    $"Error al generar el fondo: {ex.Message}");
            }
        }


        /// <summary>
        /// Aplica la aportación al fondo del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos para aplicar la aportación.</param>
        /// <returns>Plan, contrato y mensaje del proceso.</returns>
        public ErrorDto<PolizaReclamoFondoAportacionResponse> Poliza_Reclamo_Fondo_Aportacion(
            int codEmpresa,
            PolizaReclamoFondoAportacionRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoAportacionResponse>("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoAportacionResponse>("Debe indicar el reclamo.");

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoAportacionResponse>("Debe indicar el usuario.");

                const string sql = @"EXEC spPoliza_Reclamo_Fondo_Aportacion @ReclamoId, @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        Usuario = request.usuario.Trim()
                    });

                if (row == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoAportacionResponse>("No se obtuvo respuesta al aplicar la aportación.");

                if ((row.Pass ?? 0) != 1)
                    return DbHelper.CreateErrorResponse<PolizaReclamoFondoAportacionResponse>(
                        row.Mensaje?.ToString() ?? "No fue posible aplicar la aportación.");

                var response = new PolizaReclamoFondoAportacionResponse
                {
                    cod_plan = row.Cod_Plan ?? string.Empty,
                    codigo_fondo = row.Codigo_Fondo,
                    mensaje = row.Mensaje?.ToString() ?? "Aportación aplicada correctamente."
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoFondoAportacionResponse>(
                    $"Error al aplicar la aportación al fondo: {ex.Message}");
            }
        }

        /// <summary>
        /// Aplica un desembolso al reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del desembolso.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_Aplica(
            int codEmpresa,
            PolizaReclamoDesembolsoAplicaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar el reclamo.");

                if (request.monto <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar un monto superior a 0.");

                if (string.IsNullOrWhiteSpace(request.plan))
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar el plan.");

                if (request.contrato <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar el contrato.");

                if (request.banco_id <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar el banco.");

                if (string.IsNullOrWhiteSpace(request.cuenta))
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar la cuenta.");

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar el usuario.");

                const string sql = @"EXEC spPoliza_Reclamo_Desembolsos_Aplica
            @ReclamoId, @Monto, @Plan, @Contrato, @BancoId, @Cuenta, @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        Monto = request.monto,
                        Plan = request.plan.Trim(),
                        Contrato = request.contrato,
                        BancoId = request.banco_id,
                        Cuenta = request.cuenta.Trim(),
                        Usuario = request.usuario.Trim()
                    });

                if (row == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(
                        "No se obtuvo respuesta al aplicar el desembolso.");

                if ((row.Pass ?? 0) != 1)
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(
                        row.Mensaje?.ToString() ?? "No fue posible aplicar el desembolso.");

                return DbHelper.CreateOkResponse(new PolizaReclamoDesembolsoAplicaResponse
                {
                    mensaje = row.Mensaje?.ToString() ?? "Desembolso aplicado correctamente.",
                    movimiento = row.Movimiento?.ToString() ?? string.Empty
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(
                    $"Error al aplicar el desembolso: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra una etiqueta manual del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la etiqueta.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto Poliza_Reclamo_Etiqueta_Manual_Add(
            int codEmpresa,
            PolizaReclamoEtiquetaManualAddRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.ErrorResponse("Request inválido.");

                if (request.reclamo_id <= 0)
                    return DbHelper.ErrorResponse("Debe indicar el reclamo.");

                if (string.IsNullOrWhiteSpace(request.observaciones) || request.observaciones.Trim().Length < 10)
                    return DbHelper.ErrorResponse("Indique una observación válida.");

                if (request.i_correo == 1 && (string.IsNullOrWhiteSpace(request.destinatarios) || request.destinatarios.Trim().Length < 10))
                    return DbHelper.ErrorResponse("No ha indicado destinatarios válidos.");

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.ErrorResponse("Debe indicar el usuario.");

                const string sql = @"EXEC spPoliza_Reclamo_Etiqueta_Manual_Add
            @ReclamoId, @Observaciones, @ICorreo, @Destinatarios, @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        Observaciones = request.observaciones.Trim(),
                        ICorreo = request.i_correo,
                        Destinatarios = request.destinatarios?.Trim() ?? string.Empty,
                        Usuario = request.usuario.Trim()
                    });

                if (row == null)
                    return DbHelper.ErrorResponse("No se obtuvo respuesta al registrar la etiqueta.");

                return (row.Pass ?? 0) == 1
                    ? DbHelper.CreateOkResponse(row.Mensaje?.ToString() ?? "Etiqueta registrada correctamente.")
                    : DbHelper.ErrorResponse(row.Mensaje?.ToString() ?? "No fue posible registrar la etiqueta.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al registrar la etiqueta: {ex.Message}");
            }
        }

    }
}
