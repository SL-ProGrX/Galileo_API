using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFCrSeguimientoDB
    {
        private readonly IConfiguration _config;

        private const string SpMotivosConsulta = "spAFI_CR_Motivos_Consulta";
        private const string SpMotivosRegistra = "spAFI_CR_Motivos_Registra";
        private const string SpRenunciaCambioEstado = "spAFI_Renuncia_CambioEstado";

        private const string SqlSeguimientoConsulta = @"
                    SELECT
                        '' AS Btn,
                        R.cod_renuncia AS CodRenuncia,
                        R.Estado_Desc AS EstadoDesc,
                        R.cedula AS Cedula,
                        R.Nombre AS Nombre,
                        R.Tipo_Renuncia AS TipoRenuncia,
                        R.vencimiento AS Vencimiento,
                        R.Causa_Desc AS CausaDesc,
                        R.Ejecutivo_Desc AS EjecutivoDesc,
                        R.registro_user AS RegistroUser,
                        R.registro_Fecha AS RegistroFecha,
                        R.Resuelto_User AS ResueltoUser,
                        R.Resuelto_Fecha AS ResueltoFecha
                    FROM dbo.vAFI_Renuncias R
                    WHERE R.Cod_Renuncia BETWEEN @RenunciaIni AND @RenunciaFin
                      AND (@Estado = '' OR R.Estado = @Estado)
                      AND (@TipoChar = '' OR R.Tipo = @TipoChar)
                      AND (@Cedula = '' OR R.cedula LIKE @Cedula)
                      AND (@Nombre = '' OR R.Nombre LIKE @Nombre)
                      AND (@Usuario = '' OR R.registro_user LIKE @Usuario)
                      AND (@Ejecutivo = '' OR R.Ejecutivo_Desc LIKE @Ejecutivo)
                      AND (@IdCausa = 0 OR R.Id_Causa = @IdCausa)
                      AND (@IdInstitucion = 0 OR R.cod_Institucion = @IdInstitucion)
                      AND (@Provincia = '' OR @Provincia = '0' OR R.Provincia = @Provincia)
                      AND (@Zona = '' OR @Zona = '0' OR dbo.fxAfi_Zonas_Aplica(@Zona, @UsuarioActual, R.cod_Institucion, R.UP) = 1)
                      AND (@TipoFecha = '' OR
                          (@TipoFecha = 'Registro' AND R.registro_Fecha BETWEEN @FIni AND @FFin) OR
                          (@TipoFecha = 'Vencimiento' AND R.Vencimiento BETWEEN @FIni AND @FFin) OR
                          (@TipoFecha = 'Resolución' AND R.Resuelto_Fecha BETWEEN @FIni AND @FFin))
                      AND (@AplicarChecks = 0 OR @Mortalidad IS NULL OR R.Mortalidad = @Mortalidad)
                      AND (@AplicarChecks = 0 OR @Reingreso IS NULL OR R.APLICA_REINGRESO = @Reingreso)
                      AND (@AplicarChecks = 0 OR @Volver IS NULL OR R.Volver = @Volver)
                      AND (@AplicarChecks = 0 OR @AumentoTasas IS NULL OR R.Aumenta_Puntos = @AumentoTasas)
                    ORDER BY R.cod_renuncia;";

        private const string SqlGestiones = @"
                    SELECT RTRIM(cod_gestion) AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.afi_cr_gestiones;";

        private const string SqlCausasActivas = @"
                    SELECT id_causa AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.causas_renuncias
                    WHERE ACTIVO = 1;";

        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.Instituciones
                    ORDER BY descripcion;";

        private const string SqlProvincias = @"
                    SELECT Provincia AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.Provincias
                    ORDER BY Provincia;";

        private const string SqlZonas = @"
                    SELECT COD_ZONA AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.AFI_ZONAS
                    ORDER BY descripcion;";

        private const string SqlDetalleRenuncia = @"
                    SELECT R.*,
                           RTRIM(C.Descripcion) AS CausaX,
                           S.nombre,
                           ISNULL(P.id_promotor, 0) AS Id_Promotor,
                           ISNULL(P.nombre, 'AFILIACION UNIVERSAL') AS PromotorX
                    FROM dbo.afi_cr_renuncias R
                    INNER JOIN dbo.causas_renuncias C
                        ON R.id_causa = C.id_causa
                    INNER JOIN dbo.Socios S
                        ON R.cedula = S.cedula
                    LEFT JOIN dbo.Promotores P
                        ON R.id_Promotor = P.id_Promotor
                    WHERE R.cod_renuncia = @CodRenuncia;";

        private const string SqlHistorial = @"
                    SELECT *
                    FROM dbo.afi_cr_seguimiento
                    WHERE cod_renuncia = @CodRenuncia;";

        public FrmAFCrSeguimientoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el seguimiento de renuncias según los filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrSeguimientoData>> AF_CR_Seguimiento_Obtener(int CodEmpresa, AfCrSeguimientoFiltros filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de seguimiento son requeridos.",
                    -2,
                    new List<AfCrSeguimientoData>());
            }

            return DbHelper.ExecuteListQuery<AfCrSeguimientoData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlSeguimientoConsulta,
                CrearParametrosSeguimiento(filtros));
        }


        /// <summary>
        /// Obtiene la lista de gestiones para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestiones(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlGestiones);
        }


        /// <summary>
        /// Obtiene la lista de causas activas para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Causas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlCausasActivas);
        }


        /// <summary>
        /// Obtiene la lista de instituciones para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Institucion(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Obtiene la lista de provincias para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Provincia(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlProvincias);
        }


        /// <summary>
        /// Obtiene la lista de zonas para seguimiento.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Zona(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlZonas);
        }


        /// <summary>
        /// Obtiene el detalle de una renuncia por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codRenuncia"></param>
        /// <returns></returns>
        public ErrorDto<AfCrSeguimientoDetalle?> AF_CR_Seguimiento_Obtener_Detalle_Renuncia(int CodEmpresa, int codRenuncia)
        {
            return DbHelper.ExecuteSingleQuery<AfCrSeguimientoDetalle>(
                CreatePortalDb(),
                CodEmpresa,
                SqlDetalleRenuncia,
                null,
                new { CodRenuncia = codRenuncia });
        }


        /// <summary>
        /// Obtiene los motivos de una renuncia usando el SP spAFI_CR_Motivos_Consulta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="renunciaId"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrSeguimientoMotivo>> AF_CR_Seguimiento_Obtener_Motivos(int CodEmpresa, int renunciaId)
        {
            return EjecutarStoredProcedureList<AfCrSeguimientoMotivo>(
                CodEmpresa,
                SpMotivosConsulta,
                new
                {
                    RenunciaId = renunciaId,
                    Todos = 1
                });
        }


        /// <summary>
        /// Obtiene el historial de seguimiento de una renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codRenuncia"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCrSeguimientoHistorial>> AF_CR_Seguimiento_Obtener_Historial(int CodEmpresa, int codRenuncia)
        {
            return DbHelper.ExecuteListQuery<AfCrSeguimientoHistorial>(
                CreatePortalDb(),
                CodEmpresa,
                SqlHistorial,
                new { CodRenuncia = codRenuncia });
        }


        /// <summary>
        /// Obtiene la lista de gestiones para seguimiento (formato IdX/ItmX).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_CR_Seguimiento_Obtener_Gestion(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlGestiones);
        }


        /// <summary>
        /// Registra un motivo de renuncia usando el SP spAFI_CR_Motivos_Registra.
        /// </summary>
        public ErrorDto AF_CR_Seguimiento_Motivos_Registrar(int CodEmpresa, AfCrSeguimientoMotivosRegistrar motivos)
        {
            if (motivos is null)
            {
                return DbHelper.ErrorResponse("Los datos del motivo son requeridos.", -2);
            }

            return EjecutarStoredProcedure(
                CodEmpresa,
                SpMotivosRegistra,
                motivos,
                "Error al registrar motivo de seguimiento.");
        }


        /// <summary>
        /// Cambia el estado de una renuncia usando el SP spAFI_Renuncia_CambioEstado.
        /// </summary>
        public ErrorDto AF_CR_Seguimiento_Renuncia_Estado(int CodEmpresa, AfCrSeguimientoRenunciaEstado estado)
        {
            if (estado is null)
            {
                return DbHelper.ErrorResponse("Los datos del estado son requeridos.", -2);
            }

            return EjecutarStoredProcedure(
                CodEmpresa,
                SpRenunciaCambioEstado,
                estado,
                "Error al cambiar estado de renuncia.");
        }


        /// <summary>
        /// Crea parámetros seguros para consultar seguimiento de renuncias.
        /// </summary>
        private static object CrearParametrosSeguimiento(AfCrSeguimientoFiltros filtros)
        {
            var tipoFecha = NormalizarTipoFecha(filtros.TipoFecha, filtros.FIni, filtros.FFin);
            return new
            {
                RenunciaIni = filtros.RenunciaIni ?? 0,
                RenunciaFin = filtros.RenunciaFin ?? int.MaxValue,
                Estado = NormalizarTexto(filtros.Estado),
                TipoChar = NormalizarTexto(filtros.TipoChar),
                Cedula = CrearLikeContiene(filtros.Cedula),
                Nombre = CrearLikeContiene(filtros.Nombre),
                Usuario = CrearLikeContiene(filtros.Usuario),
                Ejecutivo = CrearLikeContiene(filtros.Ejecutivo),
                IdCausa = filtros.IdCausa.GetValueOrDefault(0),
                IdInstitucion = filtros.IdInstitucion.GetValueOrDefault(0),
                Provincia = NormalizarTexto(filtros.Provincia),
                Zona = NormalizarTexto(filtros.Zona),
                UsuarioActual = NormalizarTexto(filtros.UsuarioActual),
                TipoFecha = tipoFecha,
                FIni = filtros.FIni?.Date ?? DateTime.MinValue,
                FFin = filtros.FFin?.Date.AddHours(23).AddMinutes(59).AddSeconds(59) ?? DateTime.MaxValue,
                AplicarChecks = filtros.AplicarChecks ? 1 : 0,
                filtros.Mortalidad,
                filtros.Reingreso,
                filtros.Volver,
                AumentoTasas = filtros.AumentoTasas
            };
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado sin retorno.
        /// </summary>
        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters, string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(
                    storedProcedure,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado que retorna una lista.
        /// </summary>
        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(
                    storedProcedure,
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al ejecutar procedimiento almacenado.",
                    result.Code.GetValueOrDefault(-1),
                    new List<T>());
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza el tipo de fecha permitido para el filtro.
        /// </summary>
        private static string NormalizarTipoFecha(string? tipoFecha, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                return string.Empty;
            }

            var valor = NormalizarTexto(tipoFecha);
            return valor is "Registro" or "Vencimiento" or "Resolución" ? valor : string.Empty;
        }


        /// <summary>
        /// Crea un valor LIKE de búsqueda parcial.
        /// </summary>
        private static string CrearLikeContiene(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? string.Empty : $"%{texto}%";
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}