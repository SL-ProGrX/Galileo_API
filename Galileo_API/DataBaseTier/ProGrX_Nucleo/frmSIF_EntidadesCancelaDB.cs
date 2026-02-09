using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifEntidadesCancelaDB
    {
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string UnexpectedErrorMessage = "Error inesperado";

        public FrmSifEntidadesCancelaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private static string? BuildSearchLike(FiltrosLazyLoadData? filtros)
        {
            var search = filtros?.filtro?.Trim();
            return string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";
        }

        private static string NormalizeUpper(string? value)
            => (value ?? string.Empty).ToUpper();

        private const string BaseSelectEntidadesPago = @"
                            SELECT
                                COD_ENTIDAD_PAGO   AS cod_entidad_pago,
                                descripcion        AS descripcion,
                                activa             AS activa,
                                Registro_Fecha     AS registro_fecha,
                                Registro_Usuario   AS registro_usuario
                            FROM SIF_ENTIDADES_PAGO
                            WHERE (@search IS NULL
                                   OR COD_ENTIDAD_PAGO LIKE @search
                                   OR descripcion LIKE @search
                                   OR Registro_Usuario LIKE @search)";


        /// <summary>
        /// Lista las entidades pagadoras existentes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifEntidadesCancelaLista> SIF_EntidadesCancelaLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var searchLike = BuildSearchLike(filtros);

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC (según UI)

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    // Evita SQL inválido y mantiene comportamiento cercano al anterior (sin paginación real)
                    fetch = int.MaxValue;
                }

                // Total (keeps existing behavior: total of all rows, not filtered)
                const string totalQuery = @"SELECT COUNT(COD_ENTIDAD_PAGO) FROM SIF_ENTIDADES_PAGO";
                var total = connection.Query<int>(totalQuery).FirstOrDefault();

                var query = $@"{BaseSelectEntidadesPago}
                            ORDER BY
                                -- ASC
                                CASE WHEN @sortOrder = 1 AND @sortField = 'cod_entidad_pago' THEN COD_ENTIDAD_PAGO END ASC,
                                CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
                                CASE WHEN @sortOrder = 1 AND @sortField = 'activa' THEN CONVERT(int, activa) END ASC,
                                CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha' THEN Registro_Fecha END ASC,
                                CASE WHEN @sortOrder = 1 AND @sortField = 'registro_usuario' THEN Registro_Usuario END ASC,

                                -- DESC
                                CASE WHEN @sortOrder = 0 AND @sortField = 'cod_entidad_pago' THEN COD_ENTIDAD_PAGO END DESC,
                                CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
                                CASE WHEN @sortOrder = 0 AND @sortField = 'activa' THEN CONVERT(int, activa) END DESC,
                                CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha' THEN Registro_Fecha END DESC,
                                CASE WHEN @sortOrder = 0 AND @sortField = 'registro_usuario' THEN Registro_Usuario END DESC,

                                -- Fallback determinístico
                                COD_ENTIDAD_PAGO ASC
                            OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                var lista = connection.Query<SifEntidadesCancelaData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();

                return new SifEntidadesCancelaLista
                {
                    total = total,
                    lista = lista
                };
            });
        }


        /// <summary>
        /// Obtiene una lista de entidades pagadoras  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifEntidadesCancelaData>> SIF_EntidadesCancela_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var searchLike = BuildSearchLike(filtros);

                var query = $@"{BaseSelectEntidadesPago}
                            ORDER BY COD_ENTIDAD_PAGO";

                return connection.Query<SifEntidadesCancelaData>(query, new { search = searchLike }).ToList();
            });
        }


        /// <summary>
        /// Elimina una entidad pagadora por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_entidad_pago"></param>
        /// <returns></returns>

        public ErrorDto SIF_EntidadesCancela_Eliminar(int CodEmpresa, string usuario, string cod_entidad_pago)
        {
            var query = @"DELETE FROM SIF_ENTIDADES_PAGO WHERE COD_ENTIDAD_PAGO = @cod_entidad_pago";
            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new { cod_entidad_pago = NormalizeUpper(cod_entidad_pago) });

            if ((res.Code ?? -1) != 0)
                return res;

            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Entidad Pagadora : {cod_entidad_pago}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? UnexpectedErrorMessage);
            }

            return res;
        }

        /// <summary>
        /// Inserta o actualiza una entidad pagadora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="entidad"></param>
        /// <returns></returns>
        /// 
        public ErrorDto SIF_EntidadesCancela_Guardar(int CodEmpresa, string usuario, SifEntidadesCancelaData entidad)
        {
            try
            {
                // Verifico si existe usuario (parametrizado)
                var qUsuario = @"SELECT COUNT(Nombre) FROM usuarios WHERE estado = 'A' AND UPPER(Nombre) LIKE @userLike";
                var userLike = $"%{NormalizeUpper(entidad.registro_usuario)}%";
                var existeUserRes = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, qUsuario, 0, new { userLike });

                if ((existeUserRes.Code ?? -1) != 0)
                    return DbHelper.ErrorResponse(existeUserRes.Description ?? "Error", existeUserRes.Code ?? -1);

                var existeuser = existeUserRes.Result;
                if (existeuser == 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = $"El usuario {NormalizeUpper(entidad.registro_usuario)} no existe o no está activo."
                    };
                }

                // Verifico si existe entidad (parametrizado)
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM SIF_ENTIDADES_PAGO WHERE UPPER(COD_ENTIDAD_PAGO) = @cod";
                var cod = NormalizeUpper(entidad.cod_entidad_pago);
                var existeRes = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, queryExiste, 0, new { cod });

                if ((existeRes.Code ?? -1) != 0)
                    return DbHelper.ErrorResponse(existeRes.Description ?? "Error", existeRes.Code ?? -1);

                var existe = existeRes.Result;

                if (entidad.isNew)
                {
                    if (existe > 0)
                    {
                        return new ErrorDto
                        {
                            Code = -2,
                            Description = $"La Entidad pagadora con el código {entidad.cod_entidad_pago} ya existe."
                        };
                    }

                    return SIF_EntidadesCancela_Insertar(CodEmpresa, usuario, entidad);
                }

                if (existe == 0 && !entidad.isNew)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = $"La Entidad pagadora con el código {entidad.cod_entidad_pago} no existe."
                    };
                }

                return SIF_EntidadesCancela_Actualizar(CodEmpresa, usuario, entidad);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? UnexpectedErrorMessage);
            }
        }


        /// <summary>
        /// Actualiza un parentesco existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="entidad"></param>
        /// <returns></returns>
        private ErrorDto SIF_EntidadesCancela_Actualizar(int CodEmpresa, string usuario, SifEntidadesCancelaData entidad)
        {
            var query = @"UPDATE SIF_ENTIDADES_PAGO
                               SET descripcion       = @descripcion,
                                   activa            = @activa,
                                   Registro_Fecha    = GETDATE(),
                                   Registro_Usuario  = @registro_usuario
                             WHERE COD_ENTIDAD_PAGO    = @cod_entidad_pago;";

            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new
            {
                cod_entidad_pago = NormalizeUpper(entidad.cod_entidad_pago),
                descripcion = NormalizeUpper(entidad.descripcion),
                activa = entidad.activa,
                registro_usuario = usuario
            });

            if ((res.Code ?? -1) != 0)
                return res;

            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Entidad Pagadora : {entidad.cod_entidad_pago} - {entidad.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? UnexpectedErrorMessage);
            }

            return res;
        }
        
        
        /// <summary>
        /// Inserta un nuevo parentesco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="entidad"></param>
        /// <returns></returns>
        private ErrorDto SIF_EntidadesCancela_Insertar(int CodEmpresa, string usuario, SifEntidadesCancelaData entidad)
        {
            var query = @"INSERT INTO SIF_ENTIDADES_PAGO
                                    (COD_ENTIDAD_PAGO, descripcion, activA, Registro_Fecha, Registro_Usuario)
                                VALUES
                                    (@cod_entidad_pago, @descripcion, @activa, GETDATE(), @registro_usuario);";

            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new
            {
                cod_entidad_pago = NormalizeUpper(entidad.cod_entidad_pago),
                descripcion = NormalizeUpper(entidad.descripcion),
                activa = entidad.activa,
                registro_usuario = usuario
            });

            if ((res.Code ?? -1) != 0)
                return res;

            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Entidad pagadora : {entidad.cod_entidad_pago} - {entidad.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? UnexpectedErrorMessage);
            }

            return res;
        }
        
        
        /// <summary>
        /// Valida si un código de entidad pagadora ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_entidad_pago"></param>
        /// <returns></returns>
        public ErrorDto SIF_EntidadesCancela_Valida(int CodEmpresa, string cod_entidad_pago)
        {
            var query = @"SELECT COUNT(COD_ENTIDAD_PAGO) FROM SIF_ENTIDADES_PAGO WHERE UPPER(COD_ENTIDAD_PAGO) = @cod_entidad_pago";
            var res = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, query, 0, new { cod_entidad_pago = NormalizeUpper(cod_entidad_pago) });

            if ((res.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(res.Description ?? "Error", res.Code ?? -1);

            var existe = res.Result;

            if (existe > 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El código de entidad ya existe."
                };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "El código de entidad es válido."
            };
        }

    }
}
