using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFRemesasIngresosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloAfiliacion = 1;
        private const string FormNameRemesasIngresos = "frmAF_RemesasIngresos";
        private const string MsgSinPermisos = "No tiene los permisos para realizar esta opción, verifique...!!!";
        private const string TextoRemesaCerrada = "Remesa Cerrada";
        private const string OficinaTodos = "TODOS";

        private const string SqlRemesasUltimas = @"
                    SELECT TOP 150
                           COD_REMESA,
                           FECHA,
                           USUARIO,
                           FECHA_INICIO,
                           FECHA_CORTE,
                           NOTAS,
                           ESTADO,
                           CASE
                               WHEN ESTADO = 'A' THEN 'Remesa Abierta'
                               ELSE 'Remesa Cerrada'
                           END AS ESTADO_DESC,
                           MICROFILM_FECHA,
                           MICROFILM_USUARIO
                    FROM dbo.AFI_REMESAS_ING
                    ORDER BY FECHA DESC;";

        private const string SqlRemesaDelete = @"
                    DELETE FROM dbo.AFI_REMESAS_ING
                    WHERE COD_REMESA = @CodRemesa;";

        private const string SqlRemesaNuevoCodigo = @"
                    SELECT ISNULL(MAX(COD_REMESA), 0) + 1
                    FROM dbo.AFI_REMESAS_ING;";

        private const string SqlRemesaInsert = @"
                    INSERT INTO dbo.AFI_REMESAS_ING
                    (
                        cod_remesa,
                        usuario,
                        fecha,
                        estado,
                        fecha_inicio,
                        fecha_corte,
                        notas
                    )
                    VALUES
                    (
                        @CodRemesa,
                        @Usuario,
                        dbo.MyGetdate(),
                        'A',
                        @FechaInicio,
                        @FechaCorte,
                        @Notas
                    );";

        private const string SqlRemesaUpdate = @"
                    UPDATE dbo.AFI_REMESAS_ING
                    SET usuario = @Usuario,
                        fecha_inicio = @FechaInicio,
                        fecha_corte = @FechaCorte,
                        notas = @Notas
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlRemesasAbiertas = @"
                    SELECT COD_REMESA AS item,
                           RIGHT('0000' + CAST(COD_REMESA AS VARCHAR(4)), 4)
                               + '...' + RTRIM(USUARIO)
                               + '...' + CONVERT(VARCHAR(19), FECHA, 120)
                               + ' I:' + FORMAT(FECHA_INICIO, 'dd/MM/yyyy')
                               + ' C:' + FORMAT(FECHA_CORTE, 'dd/MM/yyyy') AS descripcion
                    FROM dbo.AFI_REMESAS_ING
                    WHERE ESTADO = 'A'
                    ORDER BY FECHA DESC;";

        private const string SqlRemesaFechas = @"
                    SELECT fecha_inicio AS FechaInicio,
                           fecha_corte AS FechaCorte
                    FROM dbo.AFI_REMESAS_ING
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlIngresosPendientesTodos = @"
                    SELECT A.Consec,
                           A.Cedula,
                           S.Nombre,
                           A.Fecha_Ingreso
                    FROM dbo.AFI_INGRESOS A
                    INNER JOIN dbo.Socios S
                        ON A.Cedula = S.Cedula
                       AND S.EstadoActual = 'S'
                    WHERE A.Fecha_Ingreso BETWEEN @FechaInicio AND @FechaCorte
                      AND A.cod_remesa IS NULL
                      AND dbo.fxSIFTagCierre(A.Cedula, A.Consec, 'AFI') = 1
                    ORDER BY A.Consec;";

        private const string SqlIngresosPendientesOficina = @"
                    SELECT A.Consec,
                           A.Cedula,
                           S.Nombre,
                           A.Fecha_Ingreso
                    FROM dbo.AFI_INGRESOS A
                    INNER JOIN dbo.Socios S
                        ON A.Cedula = S.Cedula
                       AND S.EstadoActual = 'S'
                    WHERE A.Fecha_Ingreso BETWEEN @FechaInicio AND @FechaCorte
                      AND A.cod_remesa IS NULL
                      AND dbo.fxSIFTagCierre(A.Cedula, A.Consec, 'AFI') = 1
                      AND A.Cod_Oficina = @Oficina
                    ORDER BY A.Consec;";

        private const string SqlRemesaAbiertaExiste = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_REMESAS_ING
                    WHERE cod_remesa = @CodRemesa
                      AND estado = 'A';";

        private const string SqlRemesaCerrar = @"
                    UPDATE dbo.AFI_REMESAS_ING
                    SET estado = 'C'
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlIngresoAsignarRemesa = @"
                    UPDATE dbo.AFI_INGRESOS
                    SET cod_remesa = @CodRemesa
                    WHERE Consec = @Consec;";

        private const string SqlRemesasPorCedula = @"
                    SELECT A.cod_remesa AS CodRemesa,
                           A.fecha AS Fecha,
                           A.usuario AS Usuario
                    FROM dbo.AFI_REMESAS_ING A
                    INNER JOIN dbo.AFI_INGRESOS X
                        ON A.cod_remesa = X.cod_remesa
                    WHERE X.cedula = @Cedula;";

        private const string SqlRemesaMicrofilm = @"
                    UPDATE dbo.AFI_REMESAS_ING
                    SET Microfilm_Fecha = dbo.MyGetdate(),
                        Microfilm_usuario = @Usuario
                    WHERE cod_remesa = @CodRemesa;";

        private const string SqlRemesaExiste = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_REMESAS_ING
                    WHERE cod_remesa = @CodRemesa;";

        public FrmAFRemesasIngresosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _securityMainDb = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene las últimas remesas de ingresos registradas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de las últimas remesas.</returns>
        public ErrorDto<List<AdiRemesaIngDto>> AFI_Remesas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AdiRemesaIngDto>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRemesasUltimas);
        }

        /// <summary>
        /// Elimina una remesa de ingresos.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AFI_Remesa_Eliminar(int codEmpresa, string codRemesa)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesaDelete,
                new { CodRemesa = NormalizarTexto(codRemesa) });

            return result.Code == 0
                ? DbHelper.OkResponse("Remesa eliminada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar remesa.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Inserta o actualiza una remesa de ingresos.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la remesa.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto AFI_Remesa_Registrar(int codEmpresa, AdiRemesaIngRequestDto request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la remesa son requeridos.", -2);
            }

            return request.CodRemesa == 0
                ? InsertarRemesa(codEmpresa, request)
                : ActualizarRemesa(codEmpresa, request);
        }


        /// <summary>
        /// Obtiene las remesas abiertas disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de remesas abiertas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_RemesaAbiertas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlRemesasAbiertas);
        }


        /// <summary>
        /// Obtiene los ingresos pendientes de asignar a una remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <param name="oficina">Código de oficina o TODOS.</param>
        /// <returns>Listado de ingresos pendientes.</returns>
        public ErrorDto<List<IngresosPendientesDto>> AFI_IngresosPendientes_Obtener(int codEmpresa, string codRemesa, string oficina = "")
        {
            var fechasResult = ObtenerFechasRemesa(codEmpresa, codRemesa);
            if (fechasResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    fechasResult.Description ?? "Error al obtener fechas de la remesa.",
                    fechasResult.Code.GetValueOrDefault(-1),
                    new List<IngresosPendientesDto>());
            }

            return ObtenerIngresosPendientes(codEmpresa, fechasResult.Result, oficina);
        }


        /// <summary>
        /// Cierra una remesa abierta.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <returns>Resultado del cierre.</returns>
        public ErrorDto AFI_Remesa_Cerrar(int codEmpresa, int codRemesa)
        {
            var abierta = ValidarRemesaAbierta(codEmpresa, codRemesa);
            if (abierta.Code != 0)
            {
                return abierta;
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesaCerrar,
                new { CodRemesa = codRemesa });

            return result.Code == 0
                ? DbHelper.OkResponse($"Remesa {codRemesa} cerrada correctamente.")
                : DbHelper.ErrorResponse(result.Description ?? "Error al cerrar remesa.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Asigna ingresos seleccionados a una remesa abierta.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <param name="ingresosSeleccionados">Consecutivos de ingresos seleccionados.</param>
        /// <returns>Resultado de la carga.</returns>
        public ErrorDto AFI_Remesa_Cargar(int codEmpresa, int codRemesa, List<int> ingresosSeleccionados)
        {
            if (ingresosSeleccionados is null || ingresosSeleccionados.Count == 0)
            {
                return DbHelper.OkResponse("No hay ingresos seleccionados para cargar.");
            }

            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    ValidarRemesaAbiertaTransaccion(connection, transaction, codRemesa);
                    foreach (var consec in ingresosSeleccionados)
                    {
                        connection.Execute(SqlIngresoAsignarRemesa, new { CodRemesa = codRemesa, Consec = consec }, transaction);
                    }

                    transaction.Commit();
                    return ingresosSeleccionados.Count;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            return result.Code == 0
                ? DbHelper.OkResponse($"Se cargaron {result.Result} ingresos a la remesa {codRemesa}.")
                : DbHelper.ErrorResponse(result.Description ?? "Error al cargar remesa.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Obtiene las remesas asociadas a una cédula.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula a consultar.</param>
        /// <returns>Listado de remesas asociadas.</returns>
        public ErrorDto<List<RemesaConsultaDto>> AFI_RemesaPorCedula_Obtener(int codEmpresa, string cedula)
        {
            var result = DbHelper.ExecuteListQuery<RemesaConsultaDto>(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesasPorCedula,
                new { Cedula = NormalizarTexto(cedula) });

            if (result.Code == 0)
            {
                result.Description = "Consulta realizada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Marca una remesa como recibida en microfilm.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que aplica el recibo.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <returns>Resultado del registro de microfilm.</returns>
        public ErrorDto AFI_Remesa_Reporte_Aplicar(int codEmpresa, string usuario, int codRemesa)
        {
            var permiso = ValidarPermiso(codEmpresa, usuario, "cmdMicrofilm");
            if (permiso.Code != 0)
            {
                return permiso;
            }

            var existe = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesaExiste,
                0,
                new { CodRemesa = codRemesa });

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar remesa.", existe.Code.GetValueOrDefault(-1));
            }

            if (existe.Result == 0)
            {
                return DbHelper.ErrorResponse("La Remesa seleccionada no existe, verifique...!!!", -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesaMicrofilm,
                new
                {
                    Usuario = NormalizarTexto(usuario),
                    CodRemesa = codRemesa
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al aplicar microfilmado.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta una nueva remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la remesa.</param>
        /// <returns>Resultado del registro.</returns>
        private ErrorDto InsertarRemesa(int codEmpresa, AdiRemesaIngRequestDto request)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var nuevoCod = connection.ExecuteScalar<int>(SqlRemesaNuevoCodigo);
                connection.Execute(SqlRemesaInsert, CrearParametrosRemesa(request, nuevoCod));
                return nuevoCod;
            });

            return result.Code == 0
                ? new ErrorDto { Code = 1, Description = $"Remesa registrada correctamente. Código: {result.Result}" }
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar remesa.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza una remesa existente cuando aún no está cerrada.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la remesa.</param>
        /// <returns>Resultado de la actualización.</returns>
        private ErrorDto ActualizarRemesa(int codEmpresa, AdiRemesaIngRequestDto request)
        {
            if (string.Equals(NormalizarTexto(request.Estado), TextoRemesaCerrada, StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse("No se puede modificar la remesa porque ya fue cerrada.", -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesaUpdate,
                CrearParametrosRemesa(request, request.CodRemesa));

            return result.Code == 0
                ? new ErrorDto { Code = 1, Description = $"Remesa actualizada correctamente. Código: {request.CodRemesa}" }
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar remesa.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea parámetros seguros para insertar o actualizar remesas.
        /// </summary>
        /// <param name="request">Datos de la remesa.</param>
        /// <param name="codRemesa">Código de remesa a guardar.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosRemesa(AdiRemesaIngRequestDto request, int codRemesa)
        {
            return new
            {
                CodRemesa = codRemesa,
                Usuario = NormalizarTexto(request.Usuario),
                request.FechaInicio,
                request.FechaCorte,
                Notas = NormalizarTexto(request.Notas)
            };
        }

        /// <summary>
        /// Obtiene las fechas de inicio y corte de una remesa.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <returns>Fechas de inicio y corte de la remesa.</returns>
        private ErrorDto<(DateTime FechaInicio, DateTime FechaCorte)> ObtenerFechasRemesa(int codEmpresa, string codRemesa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.QueryFirstOrDefault<(DateTime FechaInicio, DateTime FechaCorte)>(
                    SqlRemesaFechas,
                    new { CodRemesa = NormalizarTexto(codRemesa) }));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener fechas de la remesa.",
                    result.Code.GetValueOrDefault(-1),
                    default((DateTime FechaInicio, DateTime FechaCorte)));
            }

            if (result.Result.FechaInicio == default && result.Result.FechaCorte == default)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontraron fechas para la remesa.",
                    -1,
                    default((DateTime FechaInicio, DateTime FechaCorte)));
            }

            return DbHelper.CreateOkResponse(result.Result);
        }

        /// <summary>
        /// Obtiene ingresos pendientes aplicando filtro seguro de oficina.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="fechas">Rango de fechas de la remesa.</param>
        /// <param name="oficina">Código de oficina o TODOS.</param>
        /// <returns>Listado de ingresos pendientes.</returns>
        private ErrorDto<List<IngresosPendientesDto>> ObtenerIngresosPendientes(int codEmpresa, (DateTime FechaInicio, DateTime FechaCorte) fechas, string oficina)
        {
            var oficinaSegura = NormalizarTexto(oficina);
            var filtraOficina = !string.IsNullOrWhiteSpace(oficinaSegura)
                && !string.Equals(oficinaSegura, OficinaTodos, StringComparison.OrdinalIgnoreCase);

            var result = DbHelper.ExecuteListQuery<IngresosPendientesDto>(
                CreatePortalDb(),
                codEmpresa,
                filtraOficina ? SqlIngresosPendientesOficina : SqlIngresosPendientesTodos,
                new
                {
                    FechaInicio = fechas.FechaInicio.Date,
                    FechaCorte = fechas.FechaCorte.Date.AddDays(1).AddSeconds(-1),
                    Oficina = oficinaSegura
                });

            if (result.Code == 0)
            {
                result.Description = "Consulta realizada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Valida que la remesa exista y esté abierta.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        /// <returns>Resultado de la validación.</returns>
        private ErrorDto ValidarRemesaAbierta(int codEmpresa, int codRemesa)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                CreatePortalDb(),
                codEmpresa,
                SqlRemesaAbiertaExiste,
                0,
                new { CodRemesa = codRemesa });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar remesa.", result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.ErrorResponse("La remesa ya se encuentra cerrada.", -1)
                : DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Valida dentro de una transacción que la remesa esté abierta.
        /// </summary>
        /// <param name="connection">Conexión SQL activa.</param>
        /// <param name="transaction">Transacción SQL activa.</param>
        /// <param name="codRemesa">Código de remesa.</param>
        private static void ValidarRemesaAbiertaTransaccion(SqlConnection connection, SqlTransaction transaction, int codRemesa)
        {
            var existe = connection.ExecuteScalar<int>(SqlRemesaAbiertaExiste, new { CodRemesa = codRemesa }, transaction);
            if (existe == 0)
            {
                throw new InvalidOperationException("La remesa ya está cerrada, no se puede cargar.");
            }
        }

        /// <summary>
        /// Valida permiso para el botón indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario a validar.</param>
        /// <param name="boton">Botón o acción a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        private ErrorDto ValidarPermiso(int codEmpresa, string? usuario, string boton)
        {
            var usuarioSeguro = NormalizarTexto(usuario);
            if (string.IsNullOrWhiteSpace(usuarioSeguro))
            {
                return DbHelper.ErrorResponse("El usuario es obligatorio para validar permisos...", -1);
            }

            var permiso = _securityMainDb.Derecho(new ParametrosAccesoDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuarioSeguro.ToUpper(),
                Modulo = ModuloAfiliacion,
                FormName = FormNameRemesasIngresos,
                Boton = boton
            });

            return permiso == 0
                ? DbHelper.ErrorResponse(MsgSinPermisos, -1)
                : DbHelper.OkResponse("Ok");
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor a normalizar.</param>
        /// <returns>Texto normalizado.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
