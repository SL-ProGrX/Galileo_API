using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhExcedentesCapIndDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private const int vModulo = 2;
        private const int LineasDefault = 100;
        private const int LineasMaximas = 1000;
        private const int VencimientoMinimo = 1900;
        private const int VencimientoMaximo = 2100;

        public FrmAhExcedentesCapIndDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Carga la información inicial del modal:
        /// listado principal, cantidad de líneas y total de casos.
        /// </summary>
        public ErrorDto<FrmAhExcedentesCapIndCargarResponse> AH_ExcedentesCapInd_Cargar(
            int codEmpresa,
            FrmAhExcedentesCapIndListaRequest? request)
        {
            var lineas = AH_ExcedentesCapInd_NormalizarLineas(request?.lineas ?? LineasDefault);

            var listadoResp = AH_ExcedentesCapInd_Capitalizaciones_Lista(codEmpresa, request);
            if (listadoResp.Code < 0)
            {
                return DbHelper.CreateErrorResponse<FrmAhExcedentesCapIndCargarResponse>(listadoResp.Description);
            }

            var capitalizaciones = listadoResp.Result ?? [];

            return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndCargarResponse
            {
                capitalizaciones = capitalizaciones,
                lineas = lineas,
                casos = capitalizaciones.Count
            });
        }

        /// <summary>
        /// Obtiene el listado de capitalizaciones individuales según filtro y cantidad de líneas.
        /// </summary>
        public ErrorDto<List<FrmAhExcedentesCapIndListadoDto>> AH_ExcedentesCapInd_Capitalizaciones_Lista(
            int codEmpresa,
            FrmAhExcedentesCapIndListaRequest? request)
        {
            var filtro = AH_ExcedentesCapInd_NormalizarFiltro(request?.filtro);
            var lineas = AH_ExcedentesCapInd_NormalizarLineas(request?.lineas ?? LineasDefault);

            const string sql = @"
SELECT TOP (@Lineas)
    ISNULL(A.EXC_CAP_IND, 0) AS exc_cap_ind,
    RTRIM(ISNULL(A.CEDULA, '')) AS cedula,
    RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
    CAST(ISNULL(A.PORCENTAJE, 0) AS decimal(18, 2)) AS porcentaje,
    YEAR(A.VENCIMIENTO) AS vencimiento
FROM EXC_CAP_INDIVIDUAL A
INNER JOIN SOCIOS S
    ON S.CEDULA = A.CEDULA
WHERE
    @Filtro = ''
    OR S.CEDULA LIKE @FiltroLike
    OR S.NOMBRE LIKE @FiltroLike
ORDER BY A.EXC_CAP_IND DESC;";

            var parametros = new
            {
                Lineas = lineas,
                Filtro = filtro,
                FiltroLike = $"%{filtro}%"
            };

            return DbHelper.ExecuteListQuery<FrmAhExcedentesCapIndListadoDto>(
                _portalDb,
                codEmpresa,
                sql,
                parametros);
        }

        /// <summary>
        /// Consulta una cédula y devuelve la última capitalización individual encontrada, si existe.
        /// También valida si el socio existe.
        /// </summary>
        public ErrorDto<FrmAhExcedentesCapIndCedulaDto> AH_ExcedentesCapInd_Cedula_Consultar(
            int codEmpresa,
            string cedula)
        {
            var cedulaNormalizada = AH_ExcedentesCapInd_NormalizarCedula(cedula);
            if (string.IsNullOrWhiteSpace(cedulaNormalizada))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar una cédula válida.",
                    -2,
                    new FrmAhExcedentesCapIndCedulaDto());
            }

            const string sqlSocio = @"
SELECT TOP 1
    RTRIM(ISNULL(CEDULA, '')) AS cedula,
    RTRIM(ISNULL(NOMBRE, '')) AS nombre
FROM SOCIOS
WHERE CEDULA = @Cedula;";

            const string sqlCapitalizacion = @"
SELECT TOP 1
    ISNULL(A.EXC_CAP_IND, 0) AS exc_cap_ind,
    RTRIM(ISNULL(A.CEDULA, '')) AS cedula,
    RTRIM(ISNULL(S.NOMBRE, '')) AS nombre,
    CAST(ISNULL(A.PORCENTAJE, 0) AS decimal(18, 2)) AS porcentaje,
    YEAR(A.VENCIMIENTO) AS vencimiento
FROM EXC_CAP_INDIVIDUAL A
INNER JOIN SOCIOS S
    ON S.CEDULA = A.CEDULA
WHERE A.CEDULA = @Cedula
ORDER BY A.EXC_CAP_IND DESC;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var socio = conn.QueryFirstOrDefault<FrmAhExcedentesCapIndSocioInternoDto>(
                    sqlSocio,
                    new { Cedula = cedulaNormalizada });

                if (socio == null)
                {
                    return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndCedulaDto
                    {
                        socio_valido = false,
                        existe_capitalizacion = false,
                        cedula = cedulaNormalizada,
                        nombre = string.Empty
                    });
                }

                var capitalizacion = conn.QueryFirstOrDefault<FrmAhExcedentesCapIndCedulaDto>(
                    sqlCapitalizacion,
                    new { Cedula = cedulaNormalizada });

                if (capitalizacion == null)
                {
                    return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndCedulaDto
                    {
                        socio_valido = true,
                        existe_capitalizacion = false,
                        cedula = socio.cedula,
                        nombre = socio.nombre,
                        vencimiento = DateTime.Today.Year
                    });
                }

                capitalizacion.socio_valido = true;
                capitalizacion.existe_capitalizacion = true;

                return DbHelper.CreateOkResponse(capitalizacion);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhExcedentesCapIndCedulaDto());
            }
        }

        /// <summary>
        /// Registra o actualiza una capitalización individual.
        /// </summary>
        public ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_Guardar(
            int codEmpresa,
            FrmAhExcedentesCapIndGuardarRequest? request)
        {
            var validacion = AH_ExcedentesCapInd_ValidarGuardarRequest(request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            var cedula = AH_ExcedentesCapInd_NormalizarCedula(request.cedula);
            var usuario = AH_ExcedentesCapInd_NormalizarTextoLibre(request.usuario, 50);
            var vencimiento = AH_ExcedentesCapInd_NormalizarVencimiento(request.vencimiento);
            var porcentaje = request.porcentaje;

            const string sqlSocio = @"
SELECT TOP 1 RTRIM(ISNULL(CEDULA, ''))
FROM SOCIOS
WHERE CEDULA = @Cedula;";

            const string sqlNuevoId = @"
SELECT ISNULL(MAX(EXC_CAP_IND), 0) + 1
FROM EXC_CAP_INDIVIDUAL;";

            const string sqlInsert = @"
INSERT INTO EXC_CAP_INDIVIDUAL
(
    EXC_CAP_IND,
    CEDULA,
    PORCENTAJE,
    VENCIMIENTO,
    REGISTRO_FECHA,
    REGISTRO_USUARIO
)
VALUES
(
    @ExcCapInd,
    @Cedula,
    @Porcentaje,
    @VencimientoFecha,
    dbo.MyGetDate(),
    @Usuario
);";

            const string sqlUpdate = @"
UPDATE EXC_CAP_INDIVIDUAL
SET
    CEDULA = @Cedula,
    PORCENTAJE = @Porcentaje,
    VENCIMIENTO = @VencimientoFecha,
    REGISTRO_USUARIO = @Usuario,
    REGISTRO_FECHA = dbo.MyGetDate()
WHERE EXC_CAP_IND = @ExcCapInd;";

            const string sqlExiste = @"
SELECT COUNT(1)
FROM EXC_CAP_INDIVIDUAL
WHERE EXC_CAP_IND = @ExcCapInd;";

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
                        new FrmAhExcedentesCapIndProcesoResponse());
                }

                var esNuevo = request.exc_cap_ind <= 0;
                var excCapInd = request.exc_cap_ind;

                if (esNuevo)
                {
                    excCapInd = conn.QueryFirstOrDefault<int>(sqlNuevoId);
                    if (excCapInd <= 0)
                    {
                        return DbHelper.CreateErrorResponse(
                            "No fue posible generar el identificador de la capitalización.",
                            -1,
                            new FrmAhExcedentesCapIndProcesoResponse());
                    }

                    conn.Execute(
                        sqlInsert,
                        new
                        {
                            ExcCapInd = excCapInd,
                            Cedula = cedula,
                            Porcentaje = porcentaje,
                            VencimientoFecha = new DateTime(vencimiento, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
                            Usuario = usuario
                        });

                    AH_ExcedentesCapInd_RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        $"Excedente Cap.Extra Id: {excCapInd}, Cedula: {cedula}, Porcentaje: {porcentaje:0.00}, Vence: {vencimiento}",
                        "Registra - WEB");

                    return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndProcesoResponse
                    {
                        aplicado = 1,
                        exc_cap_ind = excCapInd,
                        mensaje = "Capitalización individual registrada correctamente."
                    });
                }

                var existe = conn.QueryFirstOrDefault<int>(
                    sqlExiste,
                    new { ExcCapInd = excCapInd });

                if (existe <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "La capitalización indicada no existe.",
                        -2,
                        new FrmAhExcedentesCapIndProcesoResponse());
                }

                conn.Execute(
                    sqlUpdate,
                    new
                    {
                        ExcCapInd = excCapInd,
                        Cedula = cedula,
                        Porcentaje = porcentaje,
                        VencimientoFecha = new DateTime(vencimiento, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
                        Usuario = usuario
                    });

                AH_ExcedentesCapInd_RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    $"Excedente Cap.Extra Id: {excCapInd}, Cedula: {cedula}, Porcentaje: {porcentaje:0.00}, Vence: {vencimiento}",
                    "Modifica - WEB");

                return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndProcesoResponse
                {
                    aplicado = 1,
                    exc_cap_ind = excCapInd,
                    mensaje = "Capitalización individual actualizada correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }
        }

        /// <summary>
        /// Elimina una capitalización individual por su identificador.
        /// </summary>
        public ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_Eliminar(
            int codEmpresa,
            int excCapInd,
            string usuario)
        {
            if (excCapInd <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar una capitalización válida.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            var usuarioNormalizado = AH_ExcedentesCapInd_NormalizarTextoLibre(usuario, 50);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            const string sqlDetalle = @"
SELECT TOP 1
    RTRIM(ISNULL(CEDULA, '')) AS cedula,
    CAST(ISNULL(PORCENTAJE, 0) AS decimal(18, 2)) AS porcentaje,
    YEAR(VENCIMIENTO) AS vencimiento
FROM EXC_CAP_INDIVIDUAL
WHERE EXC_CAP_IND = @ExcCapInd;";

            const string sqlDelete = @"
DELETE FROM EXC_CAP_INDIVIDUAL
WHERE EXC_CAP_IND = @ExcCapInd;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var detalle = conn.QueryFirstOrDefault<FrmAhExcedentesCapIndDeleteInternoDto>(
                    sqlDetalle,
                    new { ExcCapInd = excCapInd });

                if (detalle == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "La capitalización indicada no existe.",
                        -2,
                        new FrmAhExcedentesCapIndProcesoResponse());
                }

                conn.Execute(
                    sqlDelete,
                    new { ExcCapInd = excCapInd });

                AH_ExcedentesCapInd_RegistrarBitacora(
                    codEmpresa,
                    usuarioNormalizado,
                    $"Excedente Cap.Extra Id: {excCapInd}, Cedula: {detalle.cedula}, Porcentaje: {detalle.porcentaje:0.00}, Vence: {detalle.vencimiento}",
                    "Borra - WEB");

                return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndProcesoResponse
                {
                    aplicado = 1,
                    exc_cap_ind = excCapInd,
                    mensaje = "Capitalización individual eliminada correctamente."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }
        }

        private static ErrorDto<FrmAhExcedentesCapIndProcesoResponse> AH_ExcedentesCapInd_ValidarGuardarRequest(
            FrmAhExcedentesCapIndGuardarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la cédula.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            if (request.porcentaje < 0 || request.porcentaje > 100)
            {
                return DbHelper.CreateErrorResponse(
                    "El porcentaje debe estar entre 0 y 100.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            if (request.vencimiento < VencimientoMinimo || request.vencimiento > VencimientoMaximo)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar un año de vencimiento válido.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new FrmAhExcedentesCapIndProcesoResponse());
            }

            return DbHelper.CreateOkResponse(new FrmAhExcedentesCapIndProcesoResponse());
        }

        private static int AH_ExcedentesCapInd_NormalizarLineas(int lineas)
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

        private static int AH_ExcedentesCapInd_NormalizarVencimiento(int vencimiento)
        {
            if (vencimiento < VencimientoMinimo)
            {
                return DateTime.Today.Year;
            }

            if (vencimiento > VencimientoMaximo)
            {
                return VencimientoMaximo;
            }

            return vencimiento;
        }

        private static string AH_ExcedentesCapInd_NormalizarFiltro(string? filtro)
        {
            return AH_ExcedentesCapInd_NormalizarTextoLibre(filtro, 100);
        }

        private static string AH_ExcedentesCapInd_NormalizarCedula(string? cedula)
        {
            var valor = (cedula ?? string.Empty).Trim();
            return new string(valor.Where(ch => !char.IsControl(ch)).ToArray());
        }

        private static string AH_ExcedentesCapInd_NormalizarTextoLibre(string? valor, int longitudMaxima)
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

        private void AH_ExcedentesCapInd_RegistrarBitacora(
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
