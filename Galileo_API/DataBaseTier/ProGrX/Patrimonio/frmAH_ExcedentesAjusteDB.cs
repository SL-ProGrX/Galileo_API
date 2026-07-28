using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhExcedentesAjusteDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 2;
        private const int LineasDefault = 100;
        private const int LineasMaximas = 1000;

        public FrmAhExcedentesAjusteDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Carga la información inicial del modal de ajustes:
        /// períodos, listado pendiente y resumen del footer.
        /// </summary>
        public ErrorDto<FrmAhExcedentesAjusteCargarResponse> AH_ExcedentesAjuste_Cargar(
            int codEmpresa,
            FrmAhExcedentesAjustePendienteListaRequest? request)
        {
            var lineas = AH_ExcedentesAjuste_NormalizarLineas(request?.lineas ?? LineasDefault);

            var periodosResp = AH_ExcedentesAjuste_Periodos_Lista(codEmpresa);
            if (periodosResp.Code < 0)
            {
                return DbHelper.CreateErrorResponse<FrmAhExcedentesAjusteCargarResponse>(periodosResp.Description);
            }

            var pendientesResp = AH_ExcedentesAjuste_Pendientes_Lista(codEmpresa, request);
            if (pendientesResp.Code < 0)
            {
                return DbHelper.CreateErrorResponse<FrmAhExcedentesAjusteCargarResponse>(pendientesResp.Description);
            }

            var resumen = AH_ExcedentesAjuste_CalcularResumen(pendientesResp.Result ?? []);

            return DbHelper.CreateOkResponse(new FrmAhExcedentesAjusteCargarResponse
            {
                periodos = periodosResp.Result ?? [],
                pendientes = pendientesResp.Result ?? [],
                resumen = resumen,
                lineas = lineas
            });
        }

        /// <summary>
        /// Obtiene los períodos disponibles para el combo del ajuste.
        /// </summary>
        public ErrorDto<List<ExcPeriodosDto>> AH_ExcedentesAjuste_Periodos_Lista(int codEmpresa)
        {
            const string sql = @"
SELECT
    CAST(IdX AS varchar(20)) AS idx,
    RTRIM(ISNULL(ItmX, '')) AS itmx,
    RTRIM(ISNULL(Estado, '')) AS estado
FROM vExc_Periodos
ORDER BY IdX DESC;";

            return DbHelper.ExecuteListQuery<ExcPeriodosDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el listado de ajustes pendientes usando el filtro y la cantidad de líneas solicitada.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesAjustePendienteDto>> AH_ExcedentesAjuste_Pendientes_Lista(
            int codEmpresa,
            FrmAhExcedentesAjustePendienteListaRequest? request)
        {
            var filtro = AH_ExcedentesAjuste_NormalizarFiltro(request?.filtro);
            var lineas = AH_ExcedentesAjuste_NormalizarLineas(request?.lineas ?? LineasDefault);

            const string sql = @"
SELECT TOP (@Lineas)
    ISNULL(A.AJUSTE_ID, 0) AS ajuste_id,
    RTRIM(ISNULL(A.ESTADO, '')) AS estado,
    CASE
        WHEN RTRIM(ISNULL(A.ESTADO, '')) = 'P' THEN 'Pendiente'
        WHEN RTRIM(ISNULL(A.ESTADO, '')) = 'C' THEN 'Cancelado'
        ELSE RTRIM(ISNULL(A.ESTADO, ''))
    END AS estado_desc,
    RTRIM(ISNULL(A.CEDULA, '')) AS cedula,
    RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
    CAST(ISNULL(A.AJUSTE, 0) AS decimal(18, 2)) AS ajuste,
    RTRIM(ISNULL(A.DETALLE, '')) AS detalle,
    ISNULL(A.ID_PERIODO, 0) AS id_periodo,
    RTRIM(ISNULL(P.ItmX, '')) AS periodo_desc,
    RTRIM(ISNULL(A.REGISTRO_USUARIO, '')) AS registro_usuario,
    A.REGISTRO_FECHA AS registro_fecha
FROM Exc_Ajustes A
INNER JOIN Socios S
    ON S.Cedula = A.Cedula
INNER JOIN vExc_Periodos P
    ON P.IdX = A.Id_Periodo
WHERE
    A.Estado = 'P'
    AND
    (
        @Filtro = ''
        OR S.Cedula LIKE @FiltroLike
        OR S.Nombre LIKE @FiltroLike
    )
ORDER BY A.AJUSTE_ID DESC;";

            var parametros = new
            {
                Lineas = lineas,
                Filtro = filtro,
                FiltroLike = $"%{filtro}%"
            };

            return DbHelper.ExecuteListQuery<FrmAhExcedentesAjustePendienteDto>(
                _portalDb,
                codEmpresa,
                sql,
                parametros);
        }

        /// <summary>
        /// Consulta la cédula digitada y devuelve el último ajuste encontrado, si existe.
        /// También valida si el socio existe.
        /// </summary>
        public ErrorDto<FrmAhExcedentesAjusteCedulaDto> AH_ExcedentesAjuste_Cedula_Consultar(
            int codEmpresa,
            string cedula)
        {
            var cedulaNormalizada = AH_ExcedentesAjuste_NormalizarCedula(cedula);
            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar una cédula válida.",
                    -2,
                    new FrmAhExcedentesAjusteCedulaDto());
            }

            const string sqlSocio = @"
SELECT TOP 1
    RTRIM(ISNULL(Cedula, '')) AS cedula,
    RTRIM(ISNULL(Nombre, '')) AS nombre
FROM Socios
WHERE Cedula = @Cedula;";

            const string sqlAjuste = @"
SELECT TOP 1
    ISNULL(A.AJUSTE_ID, 0) AS ajuste_id,
    RTRIM(ISNULL(A.ESTADO, '')) AS estado,
    CASE
        WHEN RTRIM(ISNULL(A.ESTADO, '')) = 'P' THEN 'Pendiente'
        WHEN RTRIM(ISNULL(A.ESTADO, '')) = 'C' THEN 'Cancelado'
        ELSE RTRIM(ISNULL(A.ESTADO, ''))
    END AS estado_desc,
    RTRIM(ISNULL(A.CEDULA, '')) AS cedula,
    RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
    CAST(ISNULL(A.AJUSTE, 0) AS decimal(18, 2)) AS ajuste,
    RTRIM(ISNULL(A.DETALLE, '')) AS detalle,
    ISNULL(A.ID_PERIODO, 0) AS id_periodo,
    RTRIM(ISNULL(P.ItmX, '')) AS periodo_desc
FROM Exc_Ajustes A
INNER JOIN Socios S
    ON S.Cedula = A.Cedula
INNER JOIN vExc_Periodos P
    ON P.IdX = A.Id_Periodo
WHERE A.Cedula = @Cedula
ORDER BY A.AJUSTE_ID DESC;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var socio = conn.QueryFirstOrDefault<FrmAhExcedentesAjusteSocioInternoDto>(
                    sqlSocio,
                    new { Cedula = cedulaNormalizada });

                if (socio == null)
                {
                    return DbHelper.CreateOkResponse(new FrmAhExcedentesAjusteCedulaDto
                    {
                        socio_valido = false,
                        existe_ajuste = false,
                        cedula = cedulaNormalizada,
                        nombre = string.Empty
                    });
                }

                var ajuste = conn.QueryFirstOrDefault<FrmAhExcedentesAjusteCedulaDto>(
                    sqlAjuste,
                    new { Cedula = cedulaNormalizada });

                if (ajuste == null)
                {
                    return DbHelper.CreateOkResponse(new FrmAhExcedentesAjusteCedulaDto
                    {
                        socio_valido = true,
                        existe_ajuste = false,
                        cedula = socio.cedula,
                        nombre = socio.nombre
                    });
                }

                ajuste.socio_valido = true;
                ajuste.existe_ajuste = true;

                return DbHelper.CreateOkResponse(ajuste);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhExcedentesAjusteCedulaDto());
            }
        }

        /// <summary>
        /// Registra un ajuste de excedente usando el stored procedure oficial del proceso.
        /// </summary>
        public ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_Guardar(
            int codEmpresa,
            FrmAhExcedentesAjusteGuardarRequest? request)
        {
            var validacion = AH_ExcedentesAjuste_ValidarGuardarRequest(request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            var cedula = AH_ExcedentesAjuste_NormalizarCedula(request.cedula);
            var detalle = AH_ExcedentesAjuste_NormalizarTextoLibre(request.detalle, 500);
            var usuario = AH_ExcedentesAjuste_NormalizarTextoLibre(request.usuario, 50);

            const string sqlSocio = @"
SELECT TOP 1 RTRIM(ISNULL(Cedula, ''))
FROM Socios
WHERE Cedula = @Cedula;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var socioExiste = conn.QueryFirstOrDefault<string>(
                    sqlSocio,
                    new { Cedula = cedula });

                if (string.IsNullOrWhiteSpace(socioExiste))
                {
                    return DbHelper.CreateErrorResponse(
                        "La cédula indicada no existe en socios.",
                        -2,
                        new FrmAhExcedentesAjusteProcesoResponse());
                }

                var resultado = conn.QueryFirstOrDefault<FrmAhExcedentesAjusteSpResult>(
                    "spExc_Ajustes_Add",
                    new
                    {
                        PeriodoId = request.id_periodo,
                        Cedula = cedula,
                        Ajuste = request.ajuste,
                        Detalle = detalle,
                        Usuario = usuario,
                        Mov = "A"
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                if (resultado == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "El proceso no devolvió respuesta al guardar el ajuste.",
                        -1,
                        new FrmAhExcedentesAjusteProcesoResponse());
                }

                var response = new FrmAhExcedentesAjusteProcesoResponse
                {
                    aplicado = resultado.Aplicado,
                    ajuste_id = resultado.Ajuste_Id,
                    mensaje = resultado.Mensaje
                };

                if (resultado.Aplicado != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? "No fue posible guardar el ajuste."
                            : resultado.Mensaje,
                        -2,
                        response);
                }

                AH_ExcedentesAjuste_RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    $"Excedente Ajuste Id: {resultado.Ajuste_Id}, Cedula: {cedula}, Ajuste: {request.ajuste:0.00}",
                    "Registra - WEB");

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }
        }

        /// <summary>
        /// Elimina un ajuste de excedente usando el stored procedure oficial del proceso.
        /// </summary>
        public ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_Eliminar(
            int codEmpresa,
            int ajusteId,
            string usuario)
        {
            if (ajusteId <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar un ajuste válido.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            var usuarioNormalizado = AH_ExcedentesAjuste_NormalizarTextoLibre(usuario, 50);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            const string sqlDetalle = @"
SELECT TOP 1
    RTRIM(ISNULL(CEDULA, '')) AS cedula,
    CAST(ISNULL(AJUSTE, 0) AS decimal(18, 2)) AS ajuste
FROM Exc_Ajustes
WHERE AJUSTE_ID = @AjusteId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var detalle = conn.QueryFirstOrDefault<FrmAhExcedentesAjusteDeleteInternoDto>(
                    sqlDetalle,
                    new { AjusteId = ajusteId });

                if (detalle == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "El ajuste indicado no existe.",
                        -2,
                        new FrmAhExcedentesAjusteProcesoResponse());
                }

                var resultado = conn.QueryFirstOrDefault<FrmAhExcedentesAjusteSpResult>(
                    "spExc_Ajustes_Del",
                    new
                    {
                        Ajuste_Id = ajusteId,
                        Usuario = usuarioNormalizado
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                if (resultado == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "El proceso no devolvió respuesta al eliminar el ajuste.",
                        -1,
                        new FrmAhExcedentesAjusteProcesoResponse());
                }

                var response = new FrmAhExcedentesAjusteProcesoResponse
                {
                    aplicado = resultado.Aplicado,
                    ajuste_id = resultado.Ajuste_Id > 0 ? resultado.Ajuste_Id : ajusteId,
                    mensaje = resultado.Mensaje
                };

                if (resultado.Aplicado != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        string.IsNullOrWhiteSpace(resultado.Mensaje)
                            ? "No fue posible eliminar el ajuste."
                            : resultado.Mensaje,
                        -2,
                        response);
                }

                AH_ExcedentesAjuste_RegistrarBitacora(
                    codEmpresa,
                    usuarioNormalizado,
                    $"Excedente Ajuste Id: {ajusteId}, Cedula: {detalle.cedula}, Ajuste: {detalle.ajuste:0.00}",
                    "Elimina - WEB");

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }
        }

        private static ErrorDto<FrmAhExcedentesAjusteProcesoResponse> AH_ExcedentesAjuste_ValidarGuardarRequest(
            FrmAhExcedentesAjusteGuardarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el período.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cédula.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            if (string.IsNullOrWhiteSpace(request.detalle))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el detalle del ajuste.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new FrmAhExcedentesAjusteProcesoResponse());
            }

            return DbHelper.CreateOkResponse(new FrmAhExcedentesAjusteProcesoResponse());
        }

        private static FrmAhExcedentesAjusteResumenDto AH_ExcedentesAjuste_CalcularResumen(
            List<FrmAhExcedentesAjustePendienteDto> pendientes)
        {
            var resumen = new FrmAhExcedentesAjusteResumenDto
            {
                casos = pendientes.Count
            };

            resumen.ajuste_negativo += pendientes
                .Where(x => x.ajuste < 0)
                .Sum(x => Math.Abs(x.ajuste));

            resumen.ajuste_positivo += pendientes
                .Where(x => x.ajuste >= 0)
                .Sum(x => x.ajuste);

            return resumen;
        }

        private static int AH_ExcedentesAjuste_NormalizarLineas(int lineas)
        {
            if (lineas <= 0)
            {
                return LineasDefault;
            }

            if (lineas > LineasMaximas)
            {
                return LineasMaximas;
            }

            return lineas;
        }

        private static string AH_ExcedentesAjuste_NormalizarFiltro(string? filtro)
        {
            return AH_ExcedentesAjuste_NormalizarTextoLibre(filtro, 100);
        }

        private static string AH_ExcedentesAjuste_NormalizarCedula(string? cedula)
        {
            var valor = (cedula ?? string.Empty).Trim();
            return new string(valor.Where(ch => !char.IsControl(ch)).ToArray());
        }

        private static string AH_ExcedentesAjuste_NormalizarTextoLibre(string? valor, int longitudMaxima)
        {
            var limpio = new string((valor ?? string.Empty)
                .Trim()
                .Where(ch => !char.IsControl(ch))
                .ToArray());

            if (limpio.Length > longitudMaxima)
            {
                limpio = limpio[..longitudMaxima];
            }

            return limpio;
        }

        private void AH_ExcedentesAjuste_RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string detalleMovimiento,
            string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

       
    }
}
