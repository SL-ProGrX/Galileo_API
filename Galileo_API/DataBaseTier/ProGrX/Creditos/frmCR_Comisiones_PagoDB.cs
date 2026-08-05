using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data;
using static Galileo_API.Models.ProGrX.Creditos.FrmCrComisionesPagoModels;



namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComisionesPagoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrXSecurityMainDb _MProGrXSecurityMainDb;
        private readonly int vModulo = 3;

        public FrmCrComisionesPagoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _MProGrXSecurityMainDb = new MProGrXSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las comisiones activas para el selector de comisión.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Comisiones_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            SELECT
                RTRIM(COD_COMISION) AS item,
                RTRIM(descripcion) AS descripcion
            FROM CRD_COMISIONES
            WHERE Activa = 1
            ORDER BY descripcion;
            """;

                return connection
                    .Query<DropDownListaGenericaModel>(query)
                    .ToList();
            });
        }

        /// <summary>
        /// Obtiene las cuentas bancarias disponibles para el pago de comisiones.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Bancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            EXEC spCrd_Comisiones_Pago_ConsultaBancos;
            """;
          
                return connection
                    .Query<BancoDropDownDbModel>(query)
                    .Select(item => new DropDownListaGenericaModel
                    {
                        item = item.IdX,
                        descripcion = item.ItmX
                    })
                    .ToList();
            });
        }
        /// <summary>
        /// Obtiene las oficinas disponibles para los reportes.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_Oficinas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            SELECT
                RTRIM(cod_oficina) AS item,
                RTRIM(descripcion) AS descripcion
            FROM SIF_Oficinas
            ORDER BY cod_oficina;
            """;

                return connection
                    .Query<DropDownListaGenericaModel>(query)
                    .ToList();
            });
        }
        /// <summary>
        /// Obtiene las agencias que tienen operaciones pendientes para una remesa.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_OficinasPendientes_Obtener(int CodEmpresa, int codRemesa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            EXEC spCrd_Comisiones_Pago_ConsultaPendientes_Agencias
                @CodRemesa;
            """;

                return connection
                  .Query<BancoDropDownDbModel>(query,
                   new { CodRemesa = codRemesa })
                  .Select(item => new DropDownListaGenericaModel
                  {
                      item = item.IdX,
                      descripcion = item.ItmX
                  })
                  .ToList();

            });
        }

        /// <summary>
        /// Obtiene las últimas remesas de pago de comisiones.
        /// </summary>
        public ErrorDto<List<CrdComisionesPagoRemesaModel>> CrdComisionesPago_Remesas_Obtener(int CodEmpresa, int cantidad = 50)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            SELECT TOP (@Cantidad)
                cod_remesa AS CodRemesa,
                usuario AS Usuario,
                fecha AS Fecha,
                estado AS Estado,
                CASE estado
                    WHEN 'A' THEN 'Remesa Abierta'
                    WHEN 'C' THEN 'Remesa Cerrada'
                    WHEN 'P' THEN 'Remesa en Proceso'
                    WHEN 'T' THEN 'Remesa Trasladada'
                    ELSE estado
                END AS EstadoDescripcion,
                fecha_inicio AS FechaInicio,
                fecha_corte AS FechaCorte,
                ISNULL(notas, '') AS Notas,
                ISNULL(cod_comision, '') AS CodComision,
                ISNULL(comision_desc, '') AS ComisionDescripcion,
                ISNULL(tes_banco, 0) AS TesBanco,
                ISNULL(banco_desc, '') AS BancoDescripcion,
                ISNULL(tes_tipo, '') AS TesTipo
            FROM vCRD_COMISIONES_REMESAS
            ORDER BY fecha DESC;
            """;

                var parametros = new
                {
                    Cantidad = Math.Clamp(cantidad, 1, 500)
                };

                return connection
                    .Query<CrdComisionesPagoRemesaModel>(query, parametros)
                    .ToList();
            });
        }

        /// <summary>
        /// Obtiene el detalle de una remesa.
        /// </summary>
        public ErrorDto<CrdComisionesPagoRemesaModel?> CrdComisionesPago_Remesa_Obtener(int CodEmpresa, int codRemesa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            SELECT
                cod_remesa AS CodRemesa,
                usuario AS Usuario,
                fecha AS Fecha,
                estado AS Estado,
                CASE estado
                    WHEN 'A' THEN 'Remesa Abierta'
                    WHEN 'C' THEN 'Remesa Cerrada'
                    WHEN 'P' THEN 'Remesa en Proceso'
                    WHEN 'T' THEN 'Remesa Trasladada'
                    ELSE estado
                END AS EstadoDescripcion,
                fecha_inicio AS FechaInicio,
                fecha_corte AS FechaCorte,
                ISNULL(notas, '') AS Notas,
                ISNULL(cod_comision, '') AS CodComision,
                ISNULL(comision_desc, '') AS ComisionDescripcion,
                ISNULL(tes_banco, 0) AS TesBanco,
                ISNULL(banco_desc, '') AS BancoDescripcion,
                ISNULL(tes_tipo, '') AS TesTipo
            FROM vCRD_COMISIONES_REMESAS
            WHERE cod_remesa = @CodRemesa;
            """;

                return connection.QueryFirstOrDefault<CrdComisionesPagoRemesaModel>(
                    query,
                    new { CodRemesa = codRemesa });
            });
        }

        /// <summary>
        /// Obtiene las remesas disponibles según su estado.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdComisionesPago_RemesasPorEstado_Obtener(int CodEmpresa, string estado)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            SELECT
                       cod_remesa AS item,
                       CONCAT(
                           CASE
                               WHEN cod_remesa < 10000
                                   THEN RIGHT(
                                       '0000' + CONVERT(varchar(20), cod_remesa),
                                       4
                                   )
                               ELSE CONVERT(varchar(20), cod_remesa)
                           END,
                           '...',
                           RTRIM(usuario),
                           '...',
                           CONVERT(varchar(19), fecha, 120),
                           ' I:',
                           CONVERT(varchar(10), fecha_inicio, 103),
                           ' C:',
                           CONVERT(varchar(10), fecha_corte, 103)
                       ) AS descripcion
                   FROM CRD_COMISIONES_REMESAS
                   WHERE estado = @Estado
                   ORDER BY fecha DESC;
            """;

                return connection
                    .Query<DropDownListaGenericaModel>(
                        query,
                        new { Estado = estado })
                    .ToList();
            });
        }

        /// <summary>
        /// Registra o modifica una remesa de pago de comisiones.
        /// </summary>
        public ErrorDto<CrdComisionesPagoRemesaGuardarResponse> CrdComisionesPago_Remesa_Guardar(int CodEmpresa, CrdComisionesPagoRemesaGuardarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            connection.Open();

            SqlTransaction? transaction = null;

            try
            {
                transaction = connection.BeginTransaction();

                var validacion = ValidarRemesaGuardar(request);

                if (validacion is not null)
                {
                    return DbHelper.CreateErrorResponse(
                        validacion,
                        -1,
                        new CrdComisionesPagoRemesaGuardarResponse());
                }

                var esNueva = !request.CodRemesa.HasValue ||
                              request.CodRemesa.Value <= 0;

                int codRemesa;
                if (esNueva)
                {
                    codRemesa = InsertarRemesa(connection, transaction, CodEmpresa, request);
                }
                else
                {
                    var actualizacion = ActualizarRemesa(connection, transaction, CodEmpresa, request);
                    if (actualizacion.Code != 0)
                    {
                        transaction.Rollback();
                        return DbHelper.CreateErrorResponse(
                           "La remesa no existe o no se encuentra abierta.",
                           -2,
                           new CrdComisionesPagoRemesaGuardarResponse());
                    }

                    codRemesa = actualizacion.Result;
                }

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    new CrdComisionesPagoRemesaGuardarResponse
                    {
                        CodRemesa = codRemesa,
                        EsNueva = esNueva,
                        Mensaje = esNueva
                            ? "Remesa registrada satisfactoriamente."
                            : "Remesa modificada satisfactoriamente."
                    });
            }
            catch (Exception ex)
            {
                transaction?.Rollback();

                return DbHelper.CreateErrorResponse(
                    $"Error al guardar la remesa de comisiones: {ex.Message}",
                    -1,
                    new CrdComisionesPagoRemesaGuardarResponse());
            }
        }

        /// <summary>
        /// Valida los datos de la remesa antes de guardarla.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string? ValidarRemesaGuardar(CrdComisionesPagoRemesaGuardarRequest request)
        {
            if (request.FechaCorte.Date < request.FechaInicio.Date)
            {
                return "La fecha de corte no puede ser menor que la fecha de inicio.";
            }

            if (string.IsNullOrWhiteSpace(request.CodComision))
            {
                return "Debe seleccionar la comisión.";
            }

            if (request.TesBanco <= 0)
            {
                return "Debe seleccionar la cuenta bancaria.";
            }

            if (string.IsNullOrWhiteSpace(request.TesTipo))
            {
                return "Debe seleccionar el tipo de pago.";
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                return "El usuario es requerido.";
            }

            return null;
        }

        /// <summary>
        /// Inserta una nueva remesa en la base de datos y devuelve el código de la remesa creada.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private int InsertarRemesa(IDbConnection connection, IDbTransaction transaction, int CodEmpresa, CrdComisionesPagoRemesaGuardarRequest request)
        {
            const string query = """
        DECLARE @CodRemesa int;

        SELECT @CodRemesa = ISNULL(MAX(cod_remesa), 0) + 1
        FROM CRD_COMISIONES_REMESAS WITH (UPDLOCK, HOLDLOCK);

        INSERT INTO CRD_COMISIONES_REMESAS
        (
            cod_remesa,
            usuario,
            fecha,
            estado,
            fecha_inicio,
            fecha_corte,
            notas,
            cod_comision,
            tes_banco,
            tes_tipo
        )
        VALUES
        (
            @CodRemesa,
            @Usuario,
            dbo.MyGetdate(),
            @Estado,
            @FechaInicio,
            @FechaCorte,
            @Notas,
            @CodComision,
            @TesBanco,
            @TesTipo
        );

        SELECT @CodRemesa;
        """;

            var parametros = new
            {
                request.Usuario,
                Estado = CrdComisionesPagoEstados.Abierta,
                FechaInicio = request.FechaInicio.Date,
                FechaCorte = request.FechaCorte.Date,
                Notas = request.Notas.Trim(),
                CodComision = request.CodComision.Trim(),
                request.TesBanco,
                TesTipo = request.TesTipo.Trim()
            };

            Bitacora_Registrar(CodEmpresa, request.Usuario, "Registra", $"Remesa Comisiones de Créditos:  {request.CodRemesa}");

            return connection.QuerySingle<int>(
                query,
                parametros,
                transaction);
        }

        /// <summary>
        /// Actualiza una remesa existente en la base de datos.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto<int> ActualizarRemesa(IDbConnection connection, IDbTransaction transaction, int CodEmpresa, CrdComisionesPagoRemesaGuardarRequest request)
        {
            const string query = """
                UPDATE CRD_COMISIONES_REMESAS
                SET
                    usuario = @Usuario,
                    fecha_inicio = @FechaInicio,
                    fecha_corte = @FechaCorte,
                    notas = @Notas,
                    cod_comision = @CodComision,
                    tes_banco = @TesBanco,
                    tes_tipo = @TesTipo
                WHERE cod_remesa = @CodRemesa
                  AND estado <> @EstadoCerrado;

                SELECT @@ROWCOUNT;
                """;

            var parametros = new
            {
                CodRemesa = request.CodRemesa!.Value,
                request.Usuario,
                FechaInicio = request.FechaInicio.Date,
                FechaCorte = request.FechaCorte.Date,
                Notas = request.Notas.Trim(),
                CodComision = request.CodComision.Trim(),
                request.TesBanco,
                TesTipo = request.TesTipo.Trim(),
                EstadoCerrado = CrdComisionesPagoEstados.Cerrada
            };


            var filas = connection.QuerySingle<int>(
                query,
                parametros,
                transaction);

            if (filas == 0)
            {                
                return DbHelper.CreateErrorResponse<int>(
                    "La remesa no existe o no se encuentra abierta.",
                    -2);
            }

            Bitacora_Registrar(CodEmpresa, request.Usuario, "Modifica", $"Remesa Comisiones de Créditos:  {request.CodRemesa}");
            return DbHelper.CreateOkResponse(request.CodRemesa.Value);
        }

        /// <summary>
        /// Elimina una remesa abierta.
        /// </summary>
        public ErrorDto<bool> CrdComisionesPago_Remesa_Eliminar(int CodEmpresa, CrdComisionesPagoRemesaEliminarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                const string query = """
            DELETE FROM CRD_COMISIONES_REMESAS
            WHERE cod_remesa = @CodRemesa
              AND estado = @EstadoAbierto;

            SELECT @@ROWCOUNT;
            """;

                var filas = connection.QuerySingle<int>(
                    query,
                    new
                    {
                        request.CodRemesa,
                        EstadoAbierto = CrdComisionesPagoEstados.Abierta
                    });

                Bitacora_Registrar(CodEmpresa, request.Usuario, "Elimina", $"Remesa Comisiones de Créditos:  {request.CodRemesa}");

                if (filas == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "La remesa no existe o no se encuentra abierta.",
                        -2,
                        false);
                }

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al eliminar la remesa de comisiones.",
                    -1,
                    false);
            }
        }

        /// <summary>
        /// Obtiene las operaciones pendientes de cargar en una remesa.
        /// </summary>
        public ErrorDto<List<CrdComisionesPagoPendienteModel>> CrdComisionesPago_Pendientes_Obtener(int CodEmpresa, CrdComisionesPagoPendientesRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            EXEC spCrd_Comisiones_Pago_ConsultaPendientes
                @CodRemesa,
                @CodOficina;
            """;

                var parametros = new
                {
                    request.CodRemesa,
                    CodOficina = NormalizarOficina(request.CodOficina)
                };

                return connection
                    .Query<CrdComisionesPagoPendienteModel>(
                        query,
                        parametros)
                    .ToList();
            });
        }

        /// <summary>
        /// Normaliza el código de oficina para la consulta de operaciones pendientes. Si el código es nulo, vacío o "TODOS", devuelve null; de lo contrario, devuelve el código de oficina recortado.
        /// </summary>
        /// <param name="codOficina"></param>
        /// <returns></returns>
        private static string? NormalizarOficina(string? codOficina)
        {
            return string.IsNullOrWhiteSpace(codOficina) ||
                   string.Equals(
                       codOficina,
                       "TODOS",
                       StringComparison.OrdinalIgnoreCase)
                ? null
                : codOficina.Trim();
        }

        /// <summary>
        /// Agrega operaciones seleccionadas a una remesa abierta.
        /// </summary>
        public ErrorDto<CrdComisionesPagoProcesoResponse> CrdComisionesPago_Remesa_Cargar(int CodEmpresa, CrdComisionesPagoCargaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (request.Solicitudes.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "Debe seleccionar al menos una operación.",
                        -1,
                        new CrdComisionesPagoProcesoResponse());
                }

                var validacionRemesa = ValidarRemesaProcesable(
                    connection,
                    transaction,
                    request.CodRemesa);

                if (validacionRemesa is not null)
                {
                    transaction.Rollback();

                    return DbHelper.CreateErrorResponse(
                        validacionRemesa,
                        -1,
                        new CrdComisionesPagoProcesoResponse());
                }

                var solicitudes = request.Solicitudes
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                const string query = """
                        EXEC spCrd_Comisiones_Pago_RemesaCarga
                            @CodRemesa,
                            @IdSolicitud;
                        """;

                foreach (var idSolicitud in solicitudes)
                {
                    connection.Execute(
                        query,
                        new
                        {
                            request.CodRemesa,
                            IdSolicitud = idSolicitud
                        },
                        transaction);
                }

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    new CrdComisionesPagoProcesoResponse
                    {
                        CantidadProcesada = solicitudes.Count,
                        Mensaje = "Operaciones cargadas satisfactoriamente."
                    });
            }
            catch (Exception)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    "Error al cargar las operaciones en la remesa.",
                    -1,
                    new CrdComisionesPagoProcesoResponse());
            }
        }

        /// <summary>
        /// Valida si una remesa está en un estado procesable (abierta o en proceso).
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="codRemesa"></param>
        /// <returns></returns>
        private static string? ValidarRemesaProcesable(IDbConnection connection, IDbTransaction transaction, int codRemesa)
        {
            const string query = """
        SELECT COUNT(1)
        FROM CRD_COMISIONES_REMESAS WITH (UPDLOCK, HOLDLOCK)
        WHERE cod_remesa = @CodRemesa
          AND estado IN (@EstadoAbierto, @EstadoProceso);
        """;

            var existe = connection.QuerySingle<int>(
                query,
                new
                {
                    CodRemesa = codRemesa,
                    EstadoAbierto = CrdComisionesPagoEstados.Abierta,
                    EstadoProceso = CrdComisionesPagoEstados.Proceso
                },
                transaction);

            if (existe == 0)
            {
                return "La remesa ya se encuentra cerrada o no está disponible.";
            }

            return null;
        }
        /// <summary>
        /// Cierra una remesa abierta o en proceso.
        /// </summary>
        public ErrorDto<bool> CrdComisionesPago_Remesa_Cerrar(int CodEmpresa, CrdComisionesPagoCerrarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var validacionRemesa = ValidarRemesaProcesable(
                    connection,
                    transaction,
                    request.CodRemesa);

                if (validacionRemesa is not null)
                {
                    transaction.Rollback();

                    return DbHelper.CreateErrorResponse(
                        validacionRemesa,
                        -2,
                        false);
                }

                const string query = """
                    EXEC spCrd_Comisiones_Pago_RemesaCierra
                        @CodRemesa;
                    """;

                connection.Execute(
                    query,
                    new { request.CodRemesa },
                    transaction);

                Bitacora_Registrar(CodEmpresa, request.Usuario, "Aplica", $"Comisiones de Colocación [CIERRA] Remesa Id:  {request.CodRemesa}");

                transaction.Commit();

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    "Error al cerrar la remesa de comisiones.",
                    -1,
                    false);
            }
        }

        /// <summary>
        /// Obtiene las comisiones agrupadas por ejecutivo para su traslado.
        /// </summary>
        public ErrorDto<List<CrdComisionesPagoTrasladoModel>> CrdComisionesPago_Traslado_Obtener(int CodEmpresa, int codRemesa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            EXEC spCrd_Comisiones_Pago_RemesaTraslado_Consulta
                @CodRemesa;
            """;

                return connection
                    .Query<CrdComisionesPagoTrasladoModel>(
                        query,
                        new { CodRemesa = codRemesa })
                    .ToList();
            });
        }
        /// <summary>
        /// Traslada las comisiones seleccionadas a tesorería.
        /// </summary>
        public ErrorDto<CrdComisionesPagoProcesoResponse> CrdComisionesPago_Remesa_Trasladar(int CodEmpresa, CrdComisionesPagoTrasladarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            { 

               
                ValidarRemesaCerrada(
                    connection,
                    transaction,
                    request.CodRemesa);

                var cantidadProcesada = ProcesarTraslados(
                    connection,
                    transaction,
                    request);

                ActualizarEstadoTrasladado(
                    connection,
                    transaction,
                    request.CodRemesa);

                if(cantidadProcesada > 0)
                {
                    Bitacora_Registrar(CodEmpresa, request.Usuario, "Registra", $"Comisiones de Colocación [TRASLADA] Remesa Id: {request.CodRemesa}");

                }

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    new CrdComisionesPagoProcesoResponse
                    {
                        CantidadProcesada = request.Ejecutivos.Count,
                        Mensaje = "Caso Enviados a Bancos para su desembolso!"
                    });
            }
            catch (InvalidOperationException exception)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    exception.Message,
                    -1,
                    new CrdComisionesPagoProcesoResponse());
            }
            catch (Exception)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    "Error al trasladar las comisiones a tesorería.",
                    -1,
                    new CrdComisionesPagoProcesoResponse());
            }
        }

        /// <summary>
        /// Valida si una remesa está cerrada antes de permitir su traslado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="codRemesa"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void ValidarRemesaCerrada(IDbConnection connection, IDbTransaction transaction, int codRemesa)
        {
            const string query = """
        SELECT COUNT(1)
        FROM CRD_COMISIONES_REMESAS WITH (UPDLOCK, HOLDLOCK)
        WHERE cod_remesa = @CodRemesa
          AND estado = @EstadoCerrado;
        """;

            var existe = connection.QuerySingle<int>(
                query,
                new
                {
                    CodRemesa = codRemesa,
                    EstadoCerrado = CrdComisionesPagoEstados.Cerrada
                },
                transaction);

            if (existe == 0)
            {
                throw new InvalidOperationException(
                    "La remesa no existe o no se encuentra cerrada.");
            }
        }

        /// <summary>
        /// Procesa el traslado de comisiones para una remesa específica.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static int ProcesarTraslados(IDbConnection connection, IDbTransaction transaction, CrdComisionesPagoTrasladarRequest request)
        {
            const string query = """
                EXEC spCrd_Comisiones_Pago_RemesaTraslado
                    @CodRemesa,
                    @EjecutivoId,
                    @Usuario,
                    @Aplicacion;
                """;


            var parametros = request.Ejecutivos.Select(ejecutivoId => new
            {
                request.CodRemesa,
                EjecutivoId = ejecutivoId,
                request.Usuario,
                request.Aplicacion
            });

            return connection.Execute(
                query,
                parametros,
                transaction);
        }

        private static void ActualizarEstadoTrasladado(IDbConnection connection, IDbTransaction transaction, int codRemesa)
        {
            const string query = """
                    UPDATE CRD_COMISIONES_REMESAS
                    SET estado = @EstadoTrasladado
                    WHERE cod_remesa = @CodRemesa
                      AND estado = @EstadoCerrado;
                    SELECT @@ROWCOUNT;
                    """;

            var filas = connection.QuerySingle<int>(
                query,
                new
                {
                    CodRemesa = codRemesa,
                    EstadoTrasladado = CrdComisionesPagoEstados.Trasladada,
                    EstadoCerrado = CrdComisionesPagoEstados.Cerrada
                },
                transaction);

            if (filas == 0)
            {
                throw new InvalidOperationException(
                    "No fue posible actualizar el estado de la remesa.");
            }
        }
        /// <summary>
        /// Obtiene las remesas disponibles para reportes.
        /// </summary>
        public ErrorDto<List<CrdComisionesPagoReporteModel>> CrdComisionesPago_Reportes_Obtener(int CodEmpresa, int cantidad)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                const string query = """
            SELECT TOP (@Cantidad)
                cod_remesa AS CodRemesa,
                usuario AS Usuario,
                CASE estado
                    WHEN 'A' THEN 'Abierta'
                    WHEN 'C' THEN 'Cerrada'
                    WHEN 'P' THEN 'En proceso'
                    WHEN 'T' THEN 'Trasladada'
                    ELSE estado
                END AS Estado,
                fecha_inicio AS FechaInicio,
                fecha_corte AS FechaCorte,
                ISNULL(notas, '') AS Notas,
                fecha AS Fecha
            FROM CRD_COMISIONES_REMESAS
            ORDER BY fecha DESC;
            """;

                var parametros = new
                {
                    Cantidad = Math.Clamp(cantidad, 1, 500)
                };

                return connection
                    .Query<CrdComisionesPagoReporteModel>(
                        query,
                        parametros)
                    .ToList();
            });
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="strTipoMovimiento"></param>
        /// <param name="strDetalleMovimiento"></param>
        private void Bitacora_Registrar(int codEmpresa, string usuario, string strTipoMovimiento, string strDetalleMovimiento)
        {
            _MProGrXSecurityMainDb.Bitacora(new MProGrXSecurityMainBitacora
            {
                CodEmpresa = codEmpresa,
                usuario = (usuario ?? string.Empty).ToUpper(),
                strDetalleMovimiento = strDetalleMovimiento,
                strTipoMovimiento = strTipoMovimiento,
                vModulo = vModulo
            });
        }

    }
}
