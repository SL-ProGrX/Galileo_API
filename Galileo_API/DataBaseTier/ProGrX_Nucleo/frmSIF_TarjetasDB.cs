using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifTarjetasDB
    {
        private readonly int vModulo = 10; // Modulo de Tesorer�a
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDb;

        public FrmSifTarjetasDB(IConfiguration config)
        {
           _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }   

        /// <summary>
        /// Obtiene una lista de tarjetas con paginaci�n y filtros (lazy loading).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifTarjetasLista> SIF_TarjetasLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var db = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                    fetch = int.MaxValue;

                // Busco Total (mantiene comportamiento anterior: total sin filtro)
                const string totalQuery = @"SELECT COUNT(cod_tarjeta) FROM sif_tarjetas";
                var total = connection.Query<int>(totalQuery).FirstOrDefault();

                const string query = @"
                    SELECT cod_tarjeta, descripcion, activa
                    FROM sif_tarjetas
                    WHERE (@search IS NULL
                           OR cod_tarjeta LIKE @search
                           OR descripcion LIKE @search)
                    ORDER BY
                        -- ASC
                        CASE WHEN @sortOrder = 1 AND @sortField = 'cod_tarjeta' THEN cod_tarjeta END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
                        CASE WHEN @sortOrder = 1 AND @sortField = 'activa' THEN CONVERT(int, activa) END ASC,

                        -- DESC
                        CASE WHEN @sortOrder = 0 AND @sortField = 'cod_tarjeta' THEN cod_tarjeta END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
                        CASE WHEN @sortOrder = 0 AND @sortField = 'activa' THEN CONVERT(int, activa) END DESC,

                        -- Fallback determinístico
                        cod_tarjeta ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var lista = connection.Query<SifTarjetasData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();

                return new SifTarjetasLista
                {
                    total = total,
                    lista = lista
                };
            });

            if (db.Code != 0)
            {
                db.Result = new SifTarjetasLista
                {
                    total = 0,
                    lista = null
                };
            }

            return db;
        }

        /// <summary>
        /// Obtiene una lista de tarjetas con filtros aplicados (sin paginaci�n).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifTarjetasData>> SIF_Tarjetas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var search = filtros?.filtro?.Trim();
            string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

            const string query = @"SELECT cod_tarjeta, descripcion, activa
                                FROM sif_tarjetas
                                WHERE (@search IS NULL
                                       OR cod_tarjeta LIKE @search
                                       OR descripcion LIKE @search)
                                ORDER BY cod_tarjeta";

            return DbHelper.ExecuteListQuery<SifTarjetasData>(_portalDb, CodEmpresa, query, new { search = searchLike });
        }

        /// <summary>
        /// Inserta o actualiza una tarjeta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tarjetas_Guardar(int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            if (tarjeta == null)
                return DbHelper.ErrorResponse("El objeto tarjeta no puede ser nulo.", -2);

            var codTarjetaUpper = NormalizeUpper(tarjeta.cod_tarjeta);
            if (string.IsNullOrWhiteSpace(codTarjetaUpper))
                return DbHelper.ErrorResponse("El código de tarjeta no puede ser vacío.", -2);

            // Validar si existe la tarjeta
            var queryExiste = @"SELECT COUNT(*) FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
            var existeDb = DbHelper.ExecuteSingleQuery<int>(_portalDb, CodEmpresa, queryExiste, 0, new { cod_tarjeta = codTarjetaUpper });
            if (existeDb.Code != 0)
                return DbHelper.ErrorResponse(existeDb.Description ?? "Error desconocido al validar existencia de tarjeta.", existeDb.Code ?? -1);

            var existe = existeDb.Result;

            if (existe == 0)
                return SIF_Tarjetas_Insertar(CodEmpresa, usuario, tarjeta);

            return SIF_Tarjetas_Actualizar(CodEmpresa, usuario, tarjeta);
        }

        /// <summary>
        /// Inserta una nueva tarjeta y registra en bit�cora.
        /// </summary>
        private ErrorDto SIF_Tarjetas_Insertar(int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            var queryInsert = @"INSERT INTO sif_tarjetas (cod_tarjeta, descripcion, activa, registro_usuario, registro_fecha)
                                    VALUES (@cod_tarjeta, @descripcion, @activa, @registro_usuario, GETDATE())";

            var db = DbHelper.ExecuteNonQuery(_portalDb, CodEmpresa, queryInsert, new
            {
                cod_tarjeta = NormalizeUpper(tarjeta.cod_tarjeta),
                tarjeta.descripcion,
                tarjeta.activa,
                registro_usuario = usuario
            });

            if (db.Code == 0)
            {
                LogTarjetaBitacora(CodEmpresa, usuario, tarjeta.cod_tarjeta, tarjeta.descripcion, "Registra - WEB");
                return DbHelper.OkResponse("Tarjeta registrada correctamente.");
            }

            return db;
        }

        /// <summary>
        /// Actualiza una tarjeta existente y registra en bit�cora.
        /// </summary>
        private ErrorDto SIF_Tarjetas_Actualizar(int CodEmpresa, string usuario, SifTarjetasData tarjeta)
        {
            var queryUpdate = @"UPDATE sif_tarjetas
                                    SET descripcion = @descripcion,
                                        activa = @activa
                                    WHERE UPPER(cod_tarjeta) = @cod_tarjeta";

            var db = DbHelper.ExecuteNonQuery(_portalDb, CodEmpresa, queryUpdate, new
            {
                cod_tarjeta = NormalizeUpper(tarjeta.cod_tarjeta),
                tarjeta.descripcion,
                tarjeta.activa
            });

            if (db.Code == 0)
            {
                LogTarjetaBitacora(CodEmpresa, usuario, tarjeta.cod_tarjeta, tarjeta.descripcion, "Modifica - WEB");
                return DbHelper.OkResponse("Tarjeta actualizada correctamente.");
            }

            return db;
        }

        /// <summary>
        /// Elimina una tarjeta por su c�digo y registra en bit�cora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_tarjeta"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tarjetas_Eliminar(int CodEmpresa, string usuario, string cod_tarjeta)
        {
            var codUpper = NormalizeUpper(cod_tarjeta);
            if (string.IsNullOrWhiteSpace(codUpper))
                return DbHelper.ErrorResponse("El código de tarjeta no puede ser vacío.", -2);

            // Verifica que exista la tarjeta antes de eliminar
            var queryExiste = @"SELECT COUNT(*) FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
            var existeDb = DbHelper.ExecuteSingleQuery<int>(_portalDb, CodEmpresa, queryExiste, 0, new { cod_tarjeta = codUpper });
            if (existeDb.Code != 0)
                return DbHelper.ErrorResponse(existeDb.Description ?? "Error desconocido al validar existencia de tarjeta.", existeDb.Code ?? -1);

            if (existeDb.Result == 0)
                return DbHelper.ErrorResponse($"La tarjeta con el c�digo {cod_tarjeta} no existe.", -2);

            var queryDelete = @"DELETE FROM sif_tarjetas WHERE UPPER(cod_tarjeta) = @cod_tarjeta";
            var db = DbHelper.ExecuteNonQuery(_portalDb, CodEmpresa, queryDelete, new { cod_tarjeta = codUpper });

            if (db.Code == 0)
                LogTarjetaBitacora(CodEmpresa, usuario, cod_tarjeta, null, "Elimina - WEB");

            return db;
        }

        /// <summary>
        /// Valida si un c�digo o descripci�n de tarjeta ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public ErrorDto SIF_Tarjetas_Valida(int CodEmpresa, SifTarjetasData tarjeta)
        {
            if (tarjeta == null)
                return DbHelper.ErrorResponse("El objeto tarjeta no puede ser nulo.", -2);

            var query = @"SELECT COUNT(*) FROM sif_tarjetas 
                                  WHERE UPPER(cod_tarjeta) = @cod_tarjeta
                                     OR UPPER(descripcion) = @descripcion";

            var db = DbHelper.ExecuteSingleQuery<int>(_portalDb, CodEmpresa, query, 0, new
            {
                cod_tarjeta = NormalizeUpper(tarjeta.cod_tarjeta),
                descripcion = NormalizeUpper(tarjeta.descripcion)
            });

            if (db.Code != 0)
                return DbHelper.ErrorResponse(db.Description ?? "Error desconocido al validar existencia de tarjeta.", db.Code ?? -1);

            if (db.Result > 0)
                return DbHelper.ErrorResponse("Ya existe una tarjeta con ese c�digo o descripci�n.", -1);

            return DbHelper.OkResponse("El c�digo y la descripci�n de tarjeta son v�lidos.");
        }

        /// <summary>
        /// Obtiene la lista de emisores y su asignaci�n para una tarjeta.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_tarjeta"></param>
        /// <returns></returns>
        public ErrorDto<List<SifEmisoresAsignadosData>> SIF_TarjetasEmisores_Obtener(int CodEmpresa, string cod_tarjeta)
        {
            var query = @"SELECT E.cod_emisor AS Codigo, E.descripcion, X.cod_emisor AS Asignado
                              FROM sif_emisores E
                              LEFT JOIN sif_emisores_tarjetas X ON E.cod_emisor = X.cod_emisor
                                AND X.cod_tarjeta = @cod_tarjeta
                              ORDER BY X.cod_emisor DESC, E.cod_emisor";

            return DbHelper.ExecuteListQuery<SifEmisoresAsignadosData>(_portalDb, CodEmpresa, query, new { cod_tarjeta });
        }

        private void LogTarjetaBitacora(int CodEmpresa, string usuario, string? codTarjeta, string? descripcion, string movimiento)
        {
            var cod = codTarjeta ?? "";
            var desc = descripcion ?? "";
            var detalle = string.IsNullOrWhiteSpace(desc)
                ? $"Mantenimiento Tarjetas: {cod}"
                : $"Mantenimiento Tarjetas: {cod} - {desc}";

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizeUpper(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}