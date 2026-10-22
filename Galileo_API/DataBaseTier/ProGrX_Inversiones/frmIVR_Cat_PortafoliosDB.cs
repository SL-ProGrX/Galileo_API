using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatPortafoliosDb
    {
        private const int CodigoMensajeControlado = -2;
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmIvrCatPortafoliosDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
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
            var validacion = ValidarIdentificacion(codPortafolio, usuario);
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
                codEmpresa, resultado, usuario!, "Elimina", codPortafolio!,
                "El portafolio indicado no existe.");
        }

        private ErrorDto Guardar(
            int codEmpresa,
            IvrCatPortafolioGuardarRequest? request,
            string sql,
            bool nuevo)
        {
            var validacion = ValidarIdentificacion(request?.Cod_Portafolio, request?.Usuario);
            if (validacion != null)
            {
                return validacion;
            }

            var codigo = nuevo
                ? request!.Cod_Portafolio.Trim().ToUpperInvariant()
                : request!.Cod_Portafolio.Trim();

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb, codEmpresa, sql,
                new
                {
                    cod_portafolio = codigo,
                    descripcion = request.Descripcion ?? string.Empty,
                    desc_corta = request.Desc_Corta ?? string.Empty,
                    activo = request.Activo,
                    usuario = request.Usuario.Trim()
                });

            return ResultadoMovimiento(
                codEmpresa, resultado, request.Usuario,
                nuevo ? "Registra" : "Modifica", codigo,
                nuevo ? "El portafolio indicado ya existe."
                    : "El portafolio indicado no existe.");
        }

        private ErrorDto ResultadoMovimiento(
            int codEmpresa,
            ErrorDto<int> resultado,
            string usuario,
            string movimiento,
            string codigo,
            string mensajeSinCambios)
        {
            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ?? "No fue posible completar el proceso.");
            }
            if (resultado.Result == 0)
            {
                return DbHelper.ErrorResponse(mensajeSinCambios, CodigoMensajeControlado);
            }

            _ = _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario.Trim().ToUpperInvariant(),
                Modulo = 22,
                Movimiento = movimiento,
                DetalleMovimiento = $"Portafolio de Inversiones: {codigo.Trim()}"
            });

            return DbHelper.OkResponse("Proceso concluido satisfactoriamente.");
        }

        private static ErrorDto? ValidarIdentificacion(string? codigo, string? usuario)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    "El código del portafolio es requerido.", CodigoMensajeControlado);
            }
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.", CodigoMensajeControlado);
            }
            return null;
        }
    }
}
