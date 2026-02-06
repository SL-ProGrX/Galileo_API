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
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        public FrmSifEntidadesCancelaDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }


        /// <summary>
        /// Lista las entidades pagadoras existentes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifEntidadesCancelaLista> SIF_EntidadesCancelaLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifEntidadesCancelaLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifEntidadesCancelaLista()
                {
                    total = 0,
                    lista = new List<SifEntidadesCancelaData>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

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
                result.Result.total = connection.Query<int>(totalQuery).FirstOrDefault();

                const string query = @"
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
                                   OR Registro_Usuario LIKE @search)
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

                result.Result.lista = connection.Query<SifEntidadesCancelaData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SifEntidadesCancelaData>();
            }
            return result;
        }


        /// <summary>
        /// Obtiene una lista de entidades pagadoras  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifEntidadesCancelaData>> SIF_EntidadesCancela_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifEntidadesCancelaData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifEntidadesCancelaData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                const string query = @"SELECT
                                COD_ENTIDAD_PAGO   AS cod_entidad_pago,
                                descripcion        AS descripcion,
                                activa             AS activa,
                                Registro_Fecha     AS registro_fecha,
                                Registro_Usuario   AS registro_usuario
                            FROM SIF_ENTIDADES_PAGO
                            WHERE (@search IS NULL
                                   OR COD_ENTIDAD_PAGO LIKE @search
                                   OR descripcion LIKE @search
                                   OR Registro_Usuario LIKE @search)
                            ORDER BY COD_ENTIDAD_PAGO";

                result.Result = connection.Query<SifEntidadesCancelaData>(query, new { search = searchLike }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM SIF_ENTIDADES_PAGO WHERE COD_ENTIDAD_PAGO = @cod_entidad_pago";
                connection.Execute(query, new { cod_entidad_pago = (cod_entidad_pago ?? string.Empty).ToUpper() });

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
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                // Verifico si existe usuario (parametrizado)
                var qUsuario = @"SELECT COUNT(Nombre) FROM usuarios WHERE estado = 'A' AND UPPER(Nombre) LIKE @userLike";
                var userLike = $"%{(entidad.registro_usuario ?? string.Empty).ToUpper()}%";
                int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { userLike });
                if (existeuser == 0)
                {
                    result.Code = -2;
                    result.Description = $"El usuario {(entidad.registro_usuario ?? string.Empty).ToUpper()} no existe o no está activo.";
                    return result;
                }

                // Verifico si existe entidad (parametrizado)
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM SIF_ENTIDADES_PAGO WHERE UPPER(COD_ENTIDAD_PAGO) = @cod";
                var cod = (entidad.cod_entidad_pago ?? string.Empty).ToUpper();
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod });

                if (entidad.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"La Entidad pagadora con el código {entidad.cod_entidad_pago} ya existe.";
                    }
                    else
                    {
                        result = SIF_EntidadesCancela_Insertar(CodEmpresa, usuario, entidad);
                    }
                }
                else if (existe == 0 && !entidad.isNew)
                {
                    result.Code = -2;
                    result.Description = $"La Entidad pagadora con el código {entidad.cod_entidad_pago} no existe.";
                }
                else
                {
                    result = SIF_EntidadesCancela_Actualizar(CodEmpresa, usuario, entidad);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"UPDATE SIF_ENTIDADES_PAGO
                               SET descripcion       = @descripcion,
                                   activa            = @activa,
                                   Registro_Fecha    = GETDATE(),
                                   Registro_Usuario  = @registro_usuario
                             WHERE COD_ENTIDAD_PAGO    = @cod_entidad_pago;";
                connection.Execute(query, new
                {
                    cod_entidad_pago = (entidad.cod_entidad_pago ?? string.Empty).ToUpper(),
                    descripcion = (entidad.descripcion ?? string.Empty).ToUpper(),
                    activa = entidad.activa,
                    registro_usuario = usuario
                });

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
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"INSERT INTO SIF_ENTIDADES_PAGO
                                    (COD_ENTIDAD_PAGO, descripcion, activA, Registro_Fecha, Registro_Usuario)
                                VALUES
                                    (@cod_entidad_pago, @descripcion, @activa, GETDATE(), @registro_usuario);";
                connection.Execute(query, new
                {
                    cod_entidad_pago = (entidad.cod_entidad_pago ?? string.Empty).ToUpper(),
                    descripcion = (entidad.descripcion ?? string.Empty).ToUpper(),
                    activa = entidad.activa,
                    registro_usuario = usuario
                });

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
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
        
        
        /// <summary>
        /// Valida si un código de entidad pagadora ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_entidad_pago"></param>
        /// <returns></returns>
        public ErrorDto SIF_EntidadesCancela_Valida(int CodEmpresa, string cod_entidad_pago)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT COUNT(COD_ENTIDAD_PAGO) FROM SIF_ENTIDADES_PAGO WHERE UPPER(COD_ENTIDAD_PAGO) = @cod_entidad_pago";
                var existe = connection.QueryFirstOrDefault<int>(query, new { cod_entidad_pago = (cod_entidad_pago ?? string.Empty).ToUpper() });

                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El código de entidad ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El código de entidad es válido.";

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

    }
}
