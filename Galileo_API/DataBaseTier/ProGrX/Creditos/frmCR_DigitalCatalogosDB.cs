using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrDigitalCatalogosDb
    {
        private const int ModuloCreditos = 3;
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        public FrmCrDigitalCatalogosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        public ErrorDto<List<CrDigitalCatalogoData>> CrDigitalCatalogos_Obtener(
            int codEmpresa,
            string tipo)
        {
            var tipoNormalizado = NormalizarTipo(tipo);
            if (tipoNormalizado == null)
            {
                return DbHelper.CreateErrorResponse<List<CrDigitalCatalogoData>>(
                    "El tipo de catálogo digital no es válido.");
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var tabla = ObtenerTabla(tipoNormalizado);
                if (!TablaExiste(connection, tabla))
                {
                    return DbHelper.CreateErrorResponse<List<CrDigitalCatalogoData>>(
                        $"La tabla {tabla} no existe en la base de datos de la empresa.");
                }

                var sql = tipoNormalizado switch
                {
                    "ARCHIVOS" => @"
                        SELECT
                            IdTipoArchivo AS id,
                            Descripcion AS descripcion,
                            Activo AS activo,
                            RegistroFecha AS registro_fecha,
                            RegistroUsuario AS registro_usuario
                        FROM CRD_DIG_TiposArchivos
                        ORDER BY IdTipoArchivo;",
                    "CREDITOS" => @"
                        SELECT
                            D.IdTipoCredito AS id,
                            D.IdCodigoCatalogo AS codigo_catalogo,
                            ISNULL(C.Descripcion, '') AS descripcion_catalogo,
                            ISNULL(D.Nota, '') AS nota,
                            D.Activo AS activo,
                            D.RegistroFecha AS registro_fecha,
                            D.RegistroUsuario AS registro_usuario
                        FROM CRD_DIG_TiposCreditos D
                        LEFT JOIN Catalogo C ON C.Codigo = D.IdCodigoCatalogo
                        ORDER BY D.IdTipoCredito;",
                    _ => @"
                        SELECT
                            IdEstadoSolicitud AS id,
                            NombreEstadoSolicitud AS nombre_estado,
                            ISNULL(Descripcion, '') AS descripcion,
                            Orden AS orden,
                            Activo AS activo,
                            Final AS final,
                            RegistroFecha AS registro_fecha,
                            RegistroUsuario AS registro_usuario
                        FROM CRD_DIG_TiposEstadosSolicitud
                        ORDER BY Orden, IdEstadoSolicitud;"
                };

                var result = connection.Query<CrDigitalCatalogoData>(sql).ToList();
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<CrDigitalCatalogoData>>(
                    ex.Message);
            }
        }

        public ErrorDto CrDigitalCatalogos_Guardar(
            int codEmpresa,
            string usuario,
            string tipo,
            CrDigitalCatalogoData request)
        {
            var tipoNormalizado = NormalizarTipo(tipo);
            if (tipoNormalizado == null)
            {
                return DbHelper.ErrorResponse(
                    "El tipo de catálogo digital no es válido.");
            }

            var validacion = ValidarRequest(tipoNormalizado, request);
            if (validacion != null)
            {
                return DbHelper.ErrorResponse(validacion, -2);
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var tabla = ObtenerTabla(tipoNormalizado);
                if (!TablaExiste(connection, tabla))
                {
                    return DbHelper.ErrorResponse(
                        $"La tabla {tabla} no existe en la base de datos de la empresa.");
                }

                if (tipoNormalizado == "CREDITOS" &&
                    !CatalogoCreditoExiste(connection, request.codigo_catalogo))
                {
                    return DbHelper.ErrorResponse(
                        "La línea indicada no existe en el catálogo de créditos.",
                        -2);
                }

                if (request.is_new && RegistroExiste(connection, tipoNormalizado, request.id))
                {
                    return DbHelper.ErrorResponse(
                        $"Ya existe un registro con el código {request.id}.",
                        -2);
                }

                var sql = ObtenerSqlGuardar(tipoNormalizado, request.is_new);
                connection.Execute(sql, new
                {
                    Id = request.id,
                    Descripcion = request.descripcion.Trim(),
                    CodigoCatalogo = request.codigo_catalogo.Trim(),
                    NombreEstado = request.nombre_estado.Trim(),
                    Nota = string.IsNullOrWhiteSpace(request.nota)
                        ? null
                        : request.nota.Trim(),
                    Orden = request.orden,
                    Activo = request.activo,
                    Final = request.final,
                    Usuario = usuario
                });

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    request.is_new ? "Registra - WEB" : "Modifica - WEB",
                    $"Catálogo crédito digital {tipoNormalizado}, código: {request.id}");

                return DbHelper.OkResponse(
                    request.is_new
                        ? "Información registrada satisfactoriamente."
                        : "Información actualizada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto CrDigitalCatalogos_Eliminar(
            int codEmpresa,
            string usuario,
            string tipo,
            short id)
        {
            var tipoNormalizado = NormalizarTipo(tipo);
            if (tipoNormalizado == null)
            {
                return DbHelper.ErrorResponse(
                    "El tipo de catálogo digital no es válido.");
            }

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var tabla = ObtenerTabla(tipoNormalizado);
                if (!TablaExiste(connection, tabla))
                {
                    return DbHelper.ErrorResponse(
                        $"La tabla {tabla} no existe en la base de datos de la empresa.");
                }

                var sql = tipoNormalizado switch
                {
                    "ARCHIVOS" =>
                        "DELETE FROM CRD_DIG_TiposArchivos WHERE IdTipoArchivo = @Id;",
                    "CREDITOS" =>
                        "DELETE FROM CRD_DIG_TiposCreditos WHERE IdTipoCredito = @Id;",
                    _ =>
                        "DELETE FROM CRD_DIG_TiposEstadosSolicitud WHERE IdEstadoSolicitud = @Id;"
                };

                var filas = connection.Execute(sql, new { Id = id });
                if (filas == 0)
                {
                    return DbHelper.ErrorResponse(
                        "El registro indicado ya no existe.",
                        -2);
                }

                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    "Elimina - WEB",
                    $"Catálogo crédito digital {tipoNormalizado}, código: {id}");

                return DbHelper.OkResponse("Información eliminada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string? NormalizarTipo(string tipo)
        {
            var value = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            return value is "ARCHIVOS" or "CREDITOS" or "ESTADOS"
                ? value
                : null;
        }

        private static string ObtenerTabla(string tipo)
        {
            return tipo switch
            {
                "ARCHIVOS" => "CRD_DIG_TiposArchivos",
                "CREDITOS" => "CRD_DIG_TiposCreditos",
                _ => "CRD_DIG_TiposEstadosSolicitud"
            };
        }

        private static bool TablaExiste(
            System.Data.IDbConnection connection,
            string tabla)
        {
            const string sql = @"
                SELECT CASE
                    WHEN OBJECT_ID(@Tabla, 'U') IS NULL THEN 0
                    ELSE 1
                END;";
            return connection.ExecuteScalar<int>(sql, new { Tabla = $"dbo.{tabla}" }) == 1;
        }

        private static bool CatalogoCreditoExiste(
            System.Data.IDbConnection connection,
            string codigo)
        {
            const string sql = @"
                SELECT CASE
                    WHEN EXISTS(
                        SELECT 1 FROM Catalogo WHERE Codigo = @Codigo
                    ) THEN 1
                    ELSE 0
                END;";
            return connection.ExecuteScalar<int>(
                sql,
                new { Codigo = codigo.Trim() }) == 1;
        }

        private static bool RegistroExiste(
            System.Data.IDbConnection connection,
            string tipo,
            short id)
        {
            var sql = tipo switch
            {
                "ARCHIVOS" => @"
                    SELECT COUNT(1)
                    FROM CRD_DIG_TiposArchivos
                    WHERE IdTipoArchivo = @Id;",
                "CREDITOS" => @"
                    SELECT COUNT(1)
                    FROM CRD_DIG_TiposCreditos
                    WHERE IdTipoCredito = @Id;",
                _ => @"
                    SELECT COUNT(1)
                    FROM CRD_DIG_TiposEstadosSolicitud
                    WHERE IdEstadoSolicitud = @Id;"
            };
            return connection.ExecuteScalar<int>(sql, new { Id = id }) > 0;
        }

        private static string? ValidarRequest(
            string tipo,
            CrDigitalCatalogoData request)
        {
            if (request.id <= 0)
            {
                return "El código debe ser mayor que cero.";
            }

            if (tipo == "ARCHIVOS" && string.IsNullOrWhiteSpace(request.descripcion))
            {
                return "La descripción es requerida.";
            }

            if (tipo == "CREDITOS" &&
                string.IsNullOrWhiteSpace(request.codigo_catalogo))
            {
                return "La línea de crédito es requerida.";
            }

            if (tipo == "ESTADOS" && string.IsNullOrWhiteSpace(request.nombre_estado))
            {
                return "El nombre del estado es requerido.";
            }

            if (tipo == "ESTADOS" && request.orden < 0)
            {
                return "El orden no puede ser negativo.";
            }

            return null;
        }

        private static string ObtenerSqlGuardar(string tipo, bool nuevo)
        {
            return (tipo, nuevo) switch
            {
                ("ARCHIVOS", true) => @"
                    INSERT INTO CRD_DIG_TiposArchivos(
                        IdTipoArchivo,
                        Descripcion,
                        Activo,
                        RegistroFecha,
                        RegistroUsuario
                    ) VALUES (
                        @Id,
                        @Descripcion,
                        @Activo,
                        dbo.MyGetdate(),
                        @Usuario
                    );",
                ("ARCHIVOS", false) => @"
                    UPDATE CRD_DIG_TiposArchivos
                    SET Descripcion = @Descripcion,
                        Activo = @Activo
                    WHERE IdTipoArchivo = @Id;",
                ("CREDITOS", true) => @"
                    INSERT INTO CRD_DIG_TiposCreditos(
                        IdTipoCredito,
                        IdCodigoCatalogo,
                        Activo,
                        Nota,
                        RegistroFecha,
                        RegistroUsuario
                    ) VALUES (
                        @Id,
                        @CodigoCatalogo,
                        @Activo,
                        @Nota,
                        dbo.MyGetdate(),
                        @Usuario
                    );",
                ("CREDITOS", false) => @"
                    UPDATE CRD_DIG_TiposCreditos
                    SET IdCodigoCatalogo = @CodigoCatalogo,
                        Activo = @Activo,
                        Nota = @Nota
                    WHERE IdTipoCredito = @Id;",
                ("ESTADOS", true) => @"
                    INSERT INTO CRD_DIG_TiposEstadosSolicitud(
                        IdEstadoSolicitud,
                        NombreEstadoSolicitud,
                        Descripcion,
                        Orden,
                        Activo,
                        Final,
                        RegistroFecha,
                        RegistroUsuario
                    ) VALUES (
                        @Id,
                        @NombreEstado,
                        @Descripcion,
                        @Orden,
                        @Activo,
                        @Final,
                        dbo.MyGetdate(),
                        @Usuario
                    );",
                _ => @"
                    UPDATE CRD_DIG_TiposEstadosSolicitud
                    SET NombreEstadoSolicitud = @NombreEstado,
                        Descripcion = @Descripcion,
                        Orden = @Orden,
                        Activo = @Activo,
                        Final = @Final
                    WHERE IdEstadoSolicitud = @Id;"
            };
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCreditos
            });
        }
    }
}
