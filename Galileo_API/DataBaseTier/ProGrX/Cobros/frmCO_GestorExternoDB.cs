using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoGestorExternoModels;
using Galileo.Models.Security;


namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoGestorExternoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 4;
        public FrmCoGestorExternoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene listado de casos de gestor externo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdGestorExternoListaItemModel>> Crd_GestorExterno_Listado_Obtener(int CodEmpresa, CrdGestorExternoFiltroRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var fechaInicio = request.IgnorarFechas
                    ? new DateTime(2000, 1, 1)
                    : (request.FechaInicio?.Date ?? DateTime.Today.Date);

                var fechaCorte = request.IgnorarFechas
                    ? new DateTime(2100, 1, 1)
                    : (request.FechaCorte?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Today.Date.AddDays(1).AddTicks(-1));

                var parameters = new DynamicParameters();
                parameters.Add("@Estado", string.IsNullOrWhiteSpace(request.Estado) ? "A" : request.Estado.Trim().Substring(0, 1));
                parameters.Add("@Inicio", fechaInicio);
                parameters.Add("@Corte", fechaCorte);
                parameters.Add("@Filtro", request.Filtro?.Trim() ?? string.Empty);
                parameters.Add("@Expediente", request.Expediente?.Trim() ?? string.Empty);
                parameters.Add("@Usuario", request.Usuario?.Trim() ?? string.Empty);
                parameters.Add("@Operacion", request.Operacion);
                parameters.Add("@Gestiona", string.IsNullOrWhiteSpace(request.Gestiona) || request.Gestiona.Equals("TODOS", StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : request.Gestiona.Trim());

                const string query = @"
            EXEC spCBR_Gestor_Externo_List
                @Estado,
                @Inicio,
                @Corte,
                @Filtro,
                @Expediente,
                @Usuario,
                @Operacion,
                @Gestiona;";

                return conn.Query<CrdGestorExternoListaItemModel>(query, parameters).ToList();
            });
        }

        public ErrorDto<string> Crd_GestorExterno_Registrar(int CodEmpresa, CrdGestorExternoRegistrarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
              

                if (string.IsNullOrWhiteSpace(request.GestionUsuario))
                {
                    return DbHelper.CreateErrorResponse<string>(
                        "Debe indicar un gestor externo.",
                        -1,
                        string.Empty);
                }

                if (request.Operacion <= 0)
                {
                    return DbHelper.CreateErrorResponse<string>(
                        "Debe indicar una operación válida.",
                        -1,
                        string.Empty);
                }

                if (string.IsNullOrWhiteSpace(request.Cedula))
                {
                    return DbHelper.CreateErrorResponse<string>(
                        "Debe indicar la cédula.",
                        -1,
                        string.Empty);
                }

                if (string.IsNullOrWhiteSpace(request.Notas) || request.Notas.Trim().Length < 10)
                {
                    return DbHelper.CreateErrorResponse<string>(
                        "Debe indicar una nota válida de al menos 10 caracteres.",
                        -1,
                        string.Empty);
                }

                var parameters = new DynamicParameters();
                parameters.Add("@Operacion", request.Operacion);
                parameters.Add("@Gestiona", request.GestionUsuario.Trim());
                parameters.Add("@Cedula", request.Cedula.Trim());
                parameters.Add("@Nombre", request.Nombre?.Trim() ?? string.Empty);
                parameters.Add("@Expediente", request.Expediente?.Trim() ?? string.Empty);
                parameters.Add("@Notas", request.Notas.Trim());
                parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());

                const string query = @"
            EXEC spCBR_Gestor_Externo_Add
                @Operacion,
                @GestionUsuario,
                @Cedula,
                @Nombre,
                @Expediente,
                @Notas,
                @Usuario;";

                var result = connection.QueryFirstOrDefault<CrdGestorExternoSpResponse>(query, parameters);

                if (result is null)
                {
                    return DbHelper.CreateErrorResponse<string>(
                        "No se recibió respuesta del proceso de registro.",
                        -1,
                        string.Empty);
                }

                if (result.Pass == 1)
                {
                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = request.UsuarioEjecuta,
                        DetalleMovimiento = $"{result.Movimiento},{result.Mensaje}",
                        Movimiento = "REGISTRA-WEB",
                        Modulo = vModulo
                    });

                    string msj = result.Movimiento + "Caso con Gestor Externo, procesado satisfactoriamente!";
                    return DbHelper.CreateOkResponse(msj);
                }

                return DbHelper.CreateErrorResponse<string>(
                    string.IsNullOrWhiteSpace(result.Mensaje) ? "No fue posible registrar el caso." : result.Mensaje,
                    -2,
                    string.Empty);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<string>(
                    "Error al registrar el caso con gestor externo.",
                    -1,
                    string.Empty);
            }
        }

        //btnReversa_Click
        public ErrorDto<bool> Crd_GestorExterno_Reversar(int CodEmpresa, CrdGestorExternoReversaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                if (request.CasoId <= 0)
                {
                    return DbHelper.CreateErrorResponse<bool>(
                        "Debe indicar un caso válido.",
                        -1,
                        false);
                }

                if (string.IsNullOrWhiteSpace(request.Notas) || request.Notas.Trim().Length < 30)
                {
                    return DbHelper.CreateErrorResponse<bool>(
                        "Debe indicar una nota válida de al menos 30 caracteres.",
                        -1,
                        false);
                }

                var parameters = new DynamicParameters();
                parameters.Add("@Id", request.CasoId);
                parameters.Add("@Notas", request.Notas.Trim());
                parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());

                const string query = @"
            EXEC spCBR_Gestor_Externo_Del
                @Id,
                @Notas,
                @Usuario;";

                var result = connection.QueryFirstOrDefault<CrdGestorExternoSpResponse>(query, parameters);

                if (result is null)
                {
                    return DbHelper.CreateErrorResponse<bool>(
                        "No se recibió respuesta del proceso de reversa.",
                        -1,
                        false);
                }

                if (result.Pass == 1)
                {
                    return DbHelper.CreateOkResponse(true);
                }

                return DbHelper.CreateErrorResponse<bool>(
                    string.IsNullOrWhiteSpace(result.Mensaje) ? "No fue posible desvincular el caso." : result.Mensaje,
                    -1,
                    false);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    "Error al desvincular el caso del gestor externo.",
                    -1,
                    false);
            }
        }

        public ErrorDto<CrdGestorExternoCargaMasivaResponse> Crd_GestorExterno_CargaMasiva_Procesar(int CodEmpresa, CrdGestorExternoCargaMasivaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                var response = new CrdGestorExternoCargaMasivaResponse
                {
                    TotalRecibidos = request.Registros.Count
                };

                if (request.Registros.Count == 0)
                {
                    return DbHelper.CreateErrorResponse<CrdGestorExternoCargaMasivaResponse>(
                        "No se recibieron registros para procesar.",
                        -1,
                        response);
                }

                foreach (var registro in request.Registros)
                {
                    if (registro.Operacion <= 0)
                    {
                        response.TotalConError++;
                        response.Mensajes.Add("Se omitió un registro por operación inválida.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(registro.Notas))
                    {
                        response.TotalConError++;
                        response.Mensajes.Add($"La operación {registro.Operacion} fue omitida porque no tiene notas.");
                        continue;
                    }

                    var parameters = new DynamicParameters();

                    if (request.EstadoProceso == "A")
                    {
                        if (string.IsNullOrWhiteSpace(request.GestionUsuario))
                        {
                            return DbHelper.CreateErrorResponse<CrdGestorExternoCargaMasivaResponse>(
                                "Debe indicar un gestor externo para la asignación masiva.",
                                -1,
                                response);
                        }

                        parameters.Add("@Operacion", registro.Operacion);
                        parameters.Add("@GestionUsuario", request.GestionUsuario.Trim());
                        parameters.Add("@Cedula", string.Empty);
                        parameters.Add("@Nombre", string.Empty);
                        parameters.Add("@Expediente", registro.Expediente?.Trim() ?? string.Empty);
                        parameters.Add("@Notas", registro.Notas.Trim());
                        parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());

                        const string addQuery = @"
                    EXEC spCBR_Gestor_Externo_Add
                        @Operacion,
                        @GestionUsuario,
                        @Cedula,
                        @Nombre,
                        @Expediente,
                        @Notas,
                        @Usuario;";

                        var addResult = connection.QueryFirstOrDefault<CrdGestorExternoSpResponse>(addQuery, parameters);

                        if (addResult?.Pass == 1)
                        {
                            response.TotalProcesados++;
                        }
                        else
                        {
                            response.TotalConError++;
                            response.Mensajes.Add(
                                $"Operación {registro.Operacion}: {addResult?.Mensaje ?? "No fue posible asignar."}");
                        }
                    }
                    else
                    {
                        parameters.Add("@Operacion", registro.Operacion);
                        parameters.Add("@Notas", registro.Notas.Trim());
                        parameters.Add("@Usuario", request.UsuarioEjecuta.Trim());

                        const string delQuery = @"
                    EXEC spCBR_Gestor_Externo_Del_Masivo
                        @Operacion,
                        @Notas,
                        @Usuario;";

                        var delResult = connection.QueryFirstOrDefault<CrdGestorExternoSpResponse>(delQuery, parameters);

                        if (delResult?.Pass == 1)
                        {
                            response.TotalProcesados++;
                        }
                        else
                        {
                            response.TotalConError++;
                            response.Mensajes.Add(
                                $"Operación {registro.Operacion}: {delResult?.Mensaje ?? "No fue posible desvincular."}");
                        }
                    }
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdGestorExternoCargaMasivaResponse>(
                    "Error al procesar la carga masiva de gestor externo.",
                    -1,
                    new CrdGestorExternoCargaMasivaResponse());
            }
        }

        /// <summary>
        /// Listado de gestiones 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdGestorExternoOperacionModel>> Crd_GestorExterno_Operacion_Buscar(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {

                const string query = @"
            SELECT
                Id_Solicitud,
                Cedula,
                Nombre,
                Codigo,
                Antiguedad,
                Saldo
            FROM vCbr_Gestion_Externa_Operaciones_General_Lista           
            ORDER BY Id_Solicitud;";

                return conn.Query<CrdGestorExternoOperacionModel>(query).ToList();
            });
        }

        /// <summary>
        /// Consulta para obtener los gestores externos disponibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_GestorExterno_Gestores_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        RTRIM(Usuario) AS item,
                        RTRIM(Usuario) AS descripcion
                    FROM CBR_USUARIOS
                    WHERE OPERADOR_EXTERNO = 1
                    ORDER BY Usuario;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }
    }
}
