using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Fondos;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDRetencionConceptosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; 
        private readonly MSecurityMainDb _Security_MainDB;

        public frmFNDRetencionConceptosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de conceptos de retención sin paginación, con filtros aplicados (exportar).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="enlace"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRetencionConceptoData>> FND_RetencionConceptos_Obtener(int CodEmpresa, string enlace, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<FndRetencionConceptoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<FndRetencionConceptoData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        where = " WHERE ( C.RETENCION_CODIGO LIKE '%" + filtros.filtro + "%' " +
                                " OR C.descripcion LIKE '%" + filtros.filtro + "%' " +
                                " OR C.cod_Cuenta LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"SELECT
                                C.RETENCION_CODIGO AS RetencionCodigo,
                                C.descripcion AS Descripcion,
                                C.Activo,
                                C.cod_Cuenta AS CodCuenta,
                                CntX.cod_Cuenta_Mask AS CuentaMask,
                                CntX.descripcion AS CtaDesc
                            FROM FND_RETENCION_CONCEPTOS C
                            LEFT JOIN CntX_cuentas CntX
                                ON CntX.cod_Cuenta = C.cod_cuenta
                                AND CntX.cod_contabilidad = @enlace
                            {where}
                            ORDER BY C.RETENCION_CODIGO";
                    result.Result = connection.Query<FndRetencionConceptoData>(query, new { enlace }).ToList();
                }
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
        /// Obtiene la lista de conceptos de retención con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FndRetencionConceptoLista> FND_RetencionConceptosLista_Obtener(int CodEmpresa, string enlace, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<FndRetencionConceptoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndRetencionConceptoLista()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryTotal = @"SELECT COUNT(RETENCION_CODIGO) FROM FND_RETENCION_CONCEPTOS";
                    result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                    string where = "";
                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        where = " WHERE ( C.RETENCION_CODIGO LIKE '%" + filtros.filtro + "%' " +
                                " OR C.descripcion LIKE '%" + filtros.filtro + "%' " +
                                " OR C.cod_Cuenta LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    if (string.IsNullOrEmpty(filtros.sortField))
                        filtros.sortField = "RETENCION_CODIGO";

                    var query = $@"SELECT
                                    C.RETENCION_CODIGO AS RetencionCodigo,
                                    C.descripcion AS Descripcion,
                                    C.Activo,
                                    C.cod_Cuenta AS CodCuenta,
                                    CntX.cod_Cuenta_Mask AS CuentaMask,
                                    CntX.descripcion AS CtaDesc
                                FROM FND_RETENCION_CONCEPTOS C
                                LEFT JOIN CntX_cuentas CntX
                                    ON CntX.cod_Cuenta = C.cod_cuenta
                                    AND CntX.cod_contabilidad = @enlace
                                {where}
                                ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                OFFSET {filtros.pagina} ROWS 
                                FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    result.Result.Lista = connection.Query<FndRetencionConceptoData>(query, new { enlace }).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.Total = 0;
                result.Result.Lista = null;
            }
            return result;
        }

        /// <summary>
        /// Inserta o actualiza un concepto de retención.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto FND_RetencionConceptos_Guardar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Verifico si existe el concepto
                    var query = @"SELECT ISNULL(COUNT(*), 0) FROM FND_RETENCION_CONCEPTOS WHERE UPPER(RETENCION_CODIGO) = @codigo";
                    int existe = connection.QueryFirstOrDefault<int>(query, new { codigo = concepto.RetencionCodigo.ToUpper() });

                    if (concepto.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El concepto con el código {concepto.RetencionCodigo} ya existe.";
                        }
                        else
                        {
                            result = FND_RetencionConceptos_Insertar(CodEmpresa, usuario, concepto);
                        }
                    }
                    else if (existe == 0 && !concepto.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El concepto con el código {concepto.RetencionCodigo} no existe.";
                    }
                    else
                    {
                        result = FND_RetencionConceptos_Actualizar(CodEmpresa, usuario, concepto);
                    }
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
        /// Inserta un nuevo concepto de retención.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto FND_RetencionConceptos_Insertar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"INSERT INTO FND_RETENCION_CONCEPTOS
                                    (RETENCION_CODIGO, descripcion, Activo, cod_cuenta)
                                  VALUES
                                    (@RetencionCodigo, @Descripcion, @Activo, @CodCuenta)";
                    connection.Execute(query, new
                    {
                        RetencionCodigo = concepto.RetencionCodigo.ToUpper(),
                        Descripcion = concepto.Descripcion,
                        Activo = concepto.Activo ? 1 : 0,
                        CodCuenta = concepto.CodCuenta
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Retención Doc.: {concepto.RetencionCodigo} - {concepto.Descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
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
        /// Actualiza un concepto de retención existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto FND_RetencionConceptos_Actualizar(int CodEmpresa, string usuario, FndRetencionConceptoData concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"UPDATE FND_RETENCION_CONCEPTOS
                                    SET descripcion = @Descripcion,
                                        Activo = @Activo,
                                        cod_cuenta = @CodCuenta
                                  WHERE RETENCION_CODIGO = @RetencionCodigo";
                    connection.Execute(query, new
                    {
                        RetencionCodigo = concepto.RetencionCodigo.ToUpper(),
                        Descripcion = concepto.Descripcion,
                        Activo = concepto.Activo ? 1 : 0,
                        CodCuenta = concepto.CodCuenta
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Retención Doc.: {concepto.RetencionCodigo} - {concepto.Descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
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
        /// Elimina un concepto de retención por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="retencionCodigo"></param>
        /// <returns></returns>
        public ErrorDto FND_RetencionConceptos_Eliminar(int CodEmpresa, string usuario, string retencionCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"DELETE FROM FND_RETENCION_CONCEPTOS WHERE RETENCION_CODIGO = @RetencionCodigo";
                    connection.Execute(query, new { RetencionCodigo = retencionCodigo.ToUpper() });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Retención Doc.: {retencionCodigo}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
                    });
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
        /// Valida si un código de concepto de retención ya existe.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="retencionCodigo"></param>
        /// <returns></returns>
        public ErrorDto FND_RetencionConceptos_Valida(int CodEmpresa, string retencionCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT ISNULL(COUNT(*), 0) AS Existe FROM FND_RETENCION_CONCEPTOS WHERE RETENCION_CODIGO = @RetencionCodigo";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { RetencionCodigo = retencionCodigo.ToUpper() });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "El código de concepto de retención ya existe.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código de concepto de retención es válido.";
                    }
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
