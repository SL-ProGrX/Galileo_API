using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatPortafoliosDb : InversionesCatalogoDbBase
    {
        public FrmIvrCatPortafoliosDb(IConfiguration config)
            : base(
                config,
                "El código del portafolio es requerido.",
                "Portafolio de Inversiones: ")
        {
        }

        /// <summary>
        /// Obtiene el catálogo de portafolios ordenado por código.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa cuyos portafolios se consultan.
        /// </param>
        /// <returns>
        /// Lista de portafolios o el detalle del error de consulta.
        /// </returns>
        public ErrorDto<List<IvrCatPortafolioDto>> IVR_Cat_Portafolios_Lista_Obtener(
            int codEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_PORTAFOLIO) AS cod_portafolio,
                    RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion,
                    RTRIM(ISNULL(DESC_CORTA, '')) AS desc_corta,
                    CAST(ISNULL(ACTIVO, 0) AS bit) AS activo
                FROM IVR_PORTAFOLIOS
                ORDER BY COD_PORTAFOLIO;
                """;

            return DbHelper.ExecuteListQuery<IvrCatPortafolioDto>(
                _portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Consulta los administradores y su asignación al portafolio mediante el procedimiento utilizado por VB6.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se realiza la consulta.
        /// </param>
        /// <param name="codPortafolio">
        /// Código del portafolio cuyos administradores se consultan.
        /// </param>
        /// <returns>
        /// Administradores con su estado de asignación o el detalle del error.
        /// </returns>
        public ErrorDto<List<IvrCatPortafolioAdministradorDto>> IVR_Cat_Portafolios_Administradores_Obtener(
            int codEmpresa,
            string? codPortafolio)
        {
            if (string.IsNullOrWhiteSpace(codPortafolio))
            {
                return DbHelper.CreateErrorResponse<List<IvrCatPortafolioAdministradorDto>>(
                    "El código del portafolio es requerido.", CodigoMensajeControlado);
            }

            const string sql = "EXEC spIVR_PORTAFOLIO_ADMINISTRADORES @cod_portafolio;";
            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                using var reader = connection.ExecuteReader(sql, new
                {
                    cod_portafolio = codPortafolio.Trim()
                });
                var administradores = new List<IvrCatPortafolioAdministradorDto>();
                // VB6 carga código, descripción y asignación por posición en la grilla.
                while (reader.Read())
                {
                    var codigo = Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture)?.Trim();
                    if (string.IsNullOrEmpty(codigo))
                    {
                        continue;
                    }
                    administradores.Add(new IvrCatPortafolioAdministradorDto
                    {
                        Cod_Administrador = codigo,
                        Descripcion = Convert.ToString(reader.GetValue(1), CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
                        Asignado = !reader.IsDBNull(2) && Convert.ToInt32(reader.GetValue(2), CultureInfo.InvariantCulture) != 0
                    });
                }
                return administradores;
            });
        }

        /// <summary>
        /// Registra un portafolio nuevo y su movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se registra el portafolio.
        /// </param>
        /// <param name="request">
        /// Código, descripción, descripción corta, estado activo y usuario que registra el portafolio.
        /// </param>
        /// <returns>
        /// Resultado del registro, validación de código duplicado o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Portafolios_Registrar(
            int codEmpresa,
            IvrCatPortafolioGuardarRequest? request)
        {
            const string sql = """
                INSERT INTO IVR_PORTAFOLIOS
                (
                    COD_PORTAFOLIO, DESCRIPCION, DESC_CORTA, ACTIVO,
                    REGISTRO_USUARIO, REGISTRO_FECHA
                )
                SELECT
                    @cod_portafolio, @descripcion, @desc_corta, @activo,
                    @usuario, dbo.MyGetdate()
                WHERE NOT EXISTS
                (
                    SELECT 1 FROM IVR_PORTAFOLIOS
                    WHERE COD_PORTAFOLIO = @cod_portafolio
                );
                """;

            return Guardar(codEmpresa, request, sql, true);
        }

        /// <summary>
        /// Actualiza la descripción, descripción corta y estado del portafolio, conservando su código y datos de registro.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa donde se actualiza el portafolio.
        /// </param>
        /// <param name="request">
        /// Código del portafolio, datos que deben actualizarse y usuario del movimiento.
        /// </param>
        /// <returns>
        /// Resultado de la actualización, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Portafolios_Actualizar(
            int codEmpresa,
            IvrCatPortafolioGuardarRequest? request)
        {
            const string sql = """
                UPDATE IVR_PORTAFOLIOS
                SET DESCRIPCION = @descripcion,
                    DESC_CORTA = @desc_corta,
                    ACTIVO = @activo
                WHERE COD_PORTAFOLIO = @cod_portafolio;
                """;

            return Guardar(codEmpresa, request, sql, false);
        }

        /// <summary>
        /// Elimina el portafolio indicado y registra el movimiento en la bitácora de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa a la que pertenece el portafolio.
        /// </param>
        /// <param name="codPortafolio">
        /// Código del portafolio que debe eliminarse.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la eliminación y se registra en la bitácora.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación, validación de existencia o detalle del error.
        /// </returns>
        public ErrorDto IVR_Cat_Portafolios_Eliminar(
            int codEmpresa,
            string? codPortafolio,
            string? usuario)
        {
            var validacion = ValidarIdentificacion(
                codPortafolio, usuario);
            if (validacion != null)
            {
                return validacion;
            }

            const string sql = """
                DELETE FROM IVR_PORTAFOLIOS
                WHERE COD_PORTAFOLIO = @cod_portafolio;
                """;

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new { cod_portafolio = codPortafolio!.Trim() });

            return ResultadoMovimiento(
                codEmpresa, resultado, usuario!, "Elimina", codPortafolio,
                "El portafolio indicado no existe.");
        }

        private ErrorDto Guardar(
            int codEmpresa,
            IvrCatPortafolioGuardarRequest? request,
            string sql,
            bool nuevo)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del portafolio son requeridos.", CodigoMensajeControlado);
            }

            var req = request;
            var validacion = ValidarIdentificacion(
                req.Cod_Portafolio, req.Usuario);
            if (validacion != null)
            {
                return validacion;
            }

            var codigo = nuevo
                ? req.Cod_Portafolio.Trim().ToUpperInvariant()
                : req.Cod_Portafolio.Trim();

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new
                {
                    cod_portafolio = codigo,
                    descripcion = req.Descripcion ?? string.Empty,
                    desc_corta = req.Desc_Corta ?? string.Empty,
                    activo = req.Activo,
                    usuario = req.Usuario.Trim()
                });

            return ResultadoMovimiento(
                codEmpresa, resultado, req.Usuario,
                nuevo ? "Registra" : "Modifica", codigo,
                nuevo ? "El portafolio indicado ya existe."
                    : "El portafolio indicado no existe.");
        }
    }
}
