using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatEmisoresDb : InversionesCatalogoDbBase
    {
        public FrmIvrCatEmisoresDb(IConfiguration config)
            : base(
                config,
                "El código del emisor es requerido.",
                "Emisor de Titulos Valores: ")
        {
        }

        /// <summary>
        /// Obtiene el catálogo de emisores ordenado por código.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa cuyos emisores se consultan.
        /// </param>
        /// <returns>
        /// Lista de emisores o el detalle del error de consulta.
        /// </returns>
        public ErrorDto<List<IvrCatEmisorDto>> IVR_Cat_Emisores_Lista_Obtener(
            int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(V.COD_EMISOR) AS Cod_Emisor,
                    RTRIM(ISNULL(V.DESCRIPCION, '')) AS Descripcion,
                    RTRIM(ISNULL(E.COD_SECTOR, '')) AS Cod_Sector,
                    RTRIM(ISNULL(V.Sector_ItmX, '')) AS Sector_ItmX,
                    CAST(ISNULL(V.ACTIVO, 0) AS bit) AS Activo,
                    V.REGISTRO_USUARIO AS Registro_Usuario,
                    V.REGISTRO_FECHA AS Registro_Fecha
                FROM vIVR_EMISORES V
                INNER JOIN IVR_EMISORES E ON E.COD_EMISOR = V.COD_EMISOR
                ORDER BY V.COD_EMISOR;
                """;

            return DbHelper.ExecuteListQuery<IvrCatEmisorDto>(
                _portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene los sectores ordenados por código para la selección utilizada en VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuyos sectores se consultan.</param>
        /// <returns>Lista de sectores o el detalle del error de consulta.</returns>
        public ErrorDto<List<IvrCatEmisorSectorDto>> IVR_Cat_Emisores_Sectores_Obtener(
            int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_SECTOR) AS Cod_Sector,
                    RTRIM(COD_SECTOR) + ' - ' + RTRIM(DESCRIPCION) AS Descripcion
                FROM IVR_SECTORES
                ORDER BY COD_SECTOR;
                """;

            return DbHelper.ExecuteListQuery<IvrCatEmisorSectorDto>(
                _portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Registra un emisor nuevo y su movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se registra el emisor.
        /// </param>
        /// <param name="request">
        /// Código, descripción, sector, estado activo y usuario que registra el emisor.
        /// </param>
        /// <returns>
        /// Resultado del registro, validación de código duplicado o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Emisores_Registrar(
            int codEmpresa,
            IvrCatEmisorGuardarRequest? request)
        {
            const string sql = """
                INSERT INTO IVR_EMISORES
                (
                    COD_EMISOR, DESCRIPCION, COD_SECTOR, ACTIVO,
                    REGISTRO_USUARIO, REGISTRO_FECHA
                )
                SELECT
                    @cod_emisor, @descripcion, @cod_sector, @activo,
                    @usuario, dbo.MyGetdate()
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM IVR_EMISORES
                    WHERE COD_EMISOR = @cod_emisor
                );
                """;

            return Guardar(codEmpresa, request, sql, true);
        }

        /// <summary>
        /// Actualiza la descripción, sector y estado del emisor, conservando su código y datos de registro.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se actualiza el emisor.
        /// </param>
        /// <param name="request">
        /// Código del emisor, datos que deben actualizarse y usuario del movimiento.
        /// </param>
        /// <returns>
        /// Resultado de la actualización, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Emisores_Actualizar(
            int codEmpresa,
            IvrCatEmisorGuardarRequest? request)
        {
            const string sql = """
                UPDATE IVR_EMISORES
                SET DESCRIPCION = @descripcion,
                    COD_SECTOR = @cod_sector,
                    ACTIVO = @activo
                WHERE COD_EMISOR = @cod_emisor;
                """;

            return Guardar(codEmpresa, request, sql, false);
        }

        /// <summary>
        /// Elimina el emisor indicado y registra el movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa a la que pertenece el emisor.
        /// </param>
        /// <param name="codEmisor">
        /// Código del emisor que debe eliminarse.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la eliminación y se registra en la bitácora.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Emisores_Eliminar(
            int codEmpresa,
            string? codEmisor,
            string? usuario)
        {
            var validacion = ValidarIdentificacion(
                codEmisor, usuario);
            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                DELETE FROM IVR_EMISORES
                WHERE COD_EMISOR = @cod_emisor;
                """;

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new { cod_emisor = codEmisor!.Trim() });

            return ResultadoMovimiento(
                codEmpresa, resultado, usuario!, "Elimina", codEmisor,
                "El emisor indicado no existe.");
        }

        private ErrorDto Guardar(
            int codEmpresa,
            IvrCatEmisorGuardarRequest? request,
            string sql,
            bool nuevo)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del emisor son requeridos.", CodigoMensajeControlado);
            }

            var req = request;
            var validacion = ValidarIdentificacion(
                req.Cod_Emisor, req.Usuario);
            if (validacion != null)
            {
                return validacion;
            }

            var codigo = nuevo
                ? req.Cod_Emisor.Trim().ToUpperInvariant()
                : req.Cod_Emisor.Trim();

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new
                {
                    cod_emisor = codigo,
                    descripcion = req.Descripcion ?? string.Empty,
                    cod_sector = req.Cod_Sector?.Trim() ?? string.Empty,
                    activo = req.Activo,
                    usuario = req.Usuario.Trim()
                });

            return ResultadoMovimiento(
                codEmpresa, resultado, req.Usuario,
                nuevo ? "Registra" : "Modifica", codigo,
                nuevo ? "El emisor indicado ya existe."
                    : "El emisor indicado no existe.");
        }
    }
}
