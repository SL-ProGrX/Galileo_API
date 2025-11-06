using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using PgxAPI.Models.Security;
namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_ConceptosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10; //Módulo de tesoreria
        private readonly MSecurityMainDb _Security_MainDB;
        public frmSIF_ConceptosDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }
        /// <summary>
        /// Lista los conceptos existentes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        /// 
        public ErrorDto<SifConceptoLista> SIF_ConceptosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifConceptoLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifConceptoLista()
                {
                    total = 0,
                    lista = new List<SifConceptoData>()
                }
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(COD_CONCEPTO) from SIF_CONCEPTOS";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE (COD_CONCEPTO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "COD_CONCEPTO";
                    }

                    query = $@"
                            select
                               COD_CONCEPTO       AS cod_concepto,
                               DESCRIPCION        AS descripcion,
                               MOVIMIENTO_TIPO    AS movimiento_tipo,
                               NIVEL_ACCESO       AS nivel_acceso,
                               ACTIVO             AS activo,
                               REGISTRO_FECHA     AS registro_fecha,
                               REGISTRO_USUARIO   AS registro_usuario
                               from SIF_CONCEPTOS
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<SifConceptoData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }
        /// <summary>
        /// Obtiene una lista de conceptos  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConceptoData>> SIF_Conceptos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConceptoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConceptoData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_CONCEPTO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"select     COD_CONCEPTO       AS cod_concepto,
                               DESCRIPCION        AS descripcion,
                               MOVIMIENTO_TIPO    AS movimiento_tipo,
                               NIVEL_ACCESO       AS nivel_acceso,
                               ACTIVO             AS activo,
                               REGISTRO_FECHA     AS registro_fecha,
                               REGISTRO_USUARIO   AS registro_usuario
                               from SIF_CONCEPTOS
                                        {filtros.filtro} 
                                     order by COD_CONCEPTO";
                    result.Result = connection.Query<SifConceptoData>(query).ToList();
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
        /// Elimina un concepto por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_concepto"></param>
        /// <returns></returns>

        public ErrorDto SIF_Conceptos_Eliminar(int CodEmpresa, string usuario, string cod_concepto)
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
                {
                    var query = @"DELETE FROM SIF_CONCEPTOS WHERE COD_CONCEPTO = @cod_concepto";
                    connection.Execute(query, new { cod_concepto = (cod_concepto ?? string.Empty).ToUpper() });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto : {cod_concepto}",
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
        /// Inserta o actualiza un concepto.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// </summary>
        public ErrorDto SIF_Conceptos_Guardar(int CodEmpresa, string usuario, SifConceptoData concepto)
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
                {
                    // Verifico si existe usuario activo
                    var qUsuario = $@"select count(Nombre) 
                              from usuarios 
                              where estado = 'A' 
                                and UPPER(Nombre) = @usuario";
                    int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usuario = usuario.ToUpper() });

                    if (existeuser == 0)
                    {
                        result.Code = -2;
                        result.Description = $"El usuario {usuario.ToUpper()} no existe o no está activo.";
                        return result;
                    }

                    // Verifico si existe el concepto
                    var query = $@"select isnull(count(*),0) as Existe 
                          from SIF_CONCEPTOS  
                          where UPPER(COD_CONCEPTO) = '{concepto.cod_concepto.ToUpper()}' ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod = concepto.cod_concepto.ToUpper() });

                    if (concepto.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El concepto con el código {concepto.cod_concepto} ya existe.";
                        }
                        else
                        {
                            result = SIF_Conceptos_Insertar(CodEmpresa, usuario, concepto);
                        }
                    }
                    else if (existe == 0 && !concepto.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El concepto con el código {concepto.cod_concepto} no existe.";
                    }
                    else
                    {
                        result = SIF_Conceptos_Actualizar(CodEmpresa, usuario, concepto);
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
        /// Actualiza un concepto existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// </summary>
        private ErrorDto SIF_Conceptos_Actualizar(int CodEmpresa, string usuario, SifConceptoData concepto)
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
                {
                    var query = @"UPDATE SIF_CONCEPTOS
                          SET DESCRIPCION      = @descripcion,
                              MOVIMIENTO_TIPO  = @movimiento_tipo,
                              NIVEL_ACCESO     = @nivel_acceso,
                              ACTIVO           = @activo,
                              REGISTRO_FECHA   = GETDATE(),
                              REGISTRO_USUARIO = @registro_usuario
                        WHERE COD_CONCEPTO    = @cod_concepto;";
                    connection.Execute(query, new
                    {
                        cod_concepto = concepto.cod_concepto.ToUpper(),
                        descripcion = concepto.descripcion?.ToUpper(),
                        movimiento_tipo = concepto.movimiento_tipo,
                        nivel_acceso = concepto.nivel_acceso,
                        activo = concepto.activo,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto : {concepto.cod_concepto} - {concepto.descripcion}",
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
        /// Inserta un nuevo concepto.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// </summary>
        private ErrorDto SIF_Conceptos_Insertar(int CodEmpresa, string usuario, SifConceptoData concepto)
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
                {
                    var query = @"INSERT INTO SIF_CONCEPTOS
                            (COD_CONCEPTO, DESCRIPCION, MOVIMIENTO_TIPO, NIVEL_ACCESO, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                          VALUES
                            (@cod_concepto, @descripcion, @movimiento_tipo, @nivel_acceso, @activo, GETDATE(), @registro_usuario);";
                    connection.Execute(query, new
                    {
                        cod_concepto = concepto.cod_concepto.ToUpper(),
                        descripcion = concepto.descripcion?.ToUpper(),
                        movimiento_tipo = concepto.movimiento_tipo,
                        nivel_acceso = concepto.nivel_acceso,
                        activo = concepto.activo,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto : {concepto.cod_concepto} - {concepto.descripcion}",
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
        /// Valida si un código de concepto ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_concepto"></param>
        /// <returns></returns>
        public ErrorDto SIF_Conceptos_Valida(int CodEmpresa, string cod_concepto)
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
                {
                    var query = $@"SELECT count(COD_CONCEPTO) FROM SIF_CONCEPTOS WHERE UPPER(COD_CONCEPTO) = @COD_CONCEPTO";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod_parentesco = cod_concepto.ToUpper() });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "El código de concepto ya existe.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código de concepto es válido.";

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
        /// Obtiene todos los documentos y marca cuáles están asociados al concepto por el código.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_concepto"></param>
        /// </summary>
        public ErrorDto<List<SifConceptoDocumentoData>> SIF_ConceptosDocumentos_Obtener(int CodEmpresa, string cod_concepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifConceptoDocumentoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifConceptoDocumentoData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT 
                    D.Tipo_Documento     AS tipo_documento,
                    D.Descripcion        AS descripcion,
                    CASE WHEN CD.Tipo_Documento IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS asignado
                FROM SIF_DOCUMENTOS D
                LEFT JOIN SIF_CONCEPTOS_DOCUMENTO CD 
                    ON D.Tipo_Documento = CD.Tipo_Documento 
                    AND CD.Cod_Concepto = @cod_concepto
                ORDER BY D.Tipo_Documento";

                    result.Result = connection.Query<SifConceptoDocumentoData>(query, new { cod_concepto }).ToList();
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
        /// Asocia un documento a un concepto por el código.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_concepto"></param>
        /// <param name="tipo_documento"></param>
        /// </summary>
        public ErrorDto SIF_ConceptosDocumentos_Asociar(int CodEmpresa, string usuario, string cod_concepto, string tipo_documento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"INSERT INTO SIF_CONCEPTOS_DOCUMENTO 
                          (Cod_Concepto, Tipo_Documento, Registro_Usuario, Registro_Fecha)
                          VALUES (@cod_concepto, @tipo_documento, @usuario, GETDATE());";

                    connection.Execute(query, new { cod_concepto, tipo_documento, usuario });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto {cod_concepto} asociado al Documento {tipo_documento}",
                        Movimiento = "Asocia - WEB",
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
        /// Elimina la asociación entre un concepto y un documento por el código.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_concepto"></param>
        /// <param name="tipo_documento"></param>
        /// </summary>
        public ErrorDto SIF_ConceptosDocumentos_Desasociar(int CodEmpresa, string usuario, string cod_concepto, string tipo_documento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto { Code = 0, Description = "Ok" };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"DELETE FROM SIF_CONCEPTOS_DOCUMENTO 
                          WHERE Cod_Concepto = @cod_concepto AND Tipo_Documento = @tipo_documento;";

                    connection.Execute(query, new { cod_concepto, tipo_documento });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Concepto {cod_concepto} desasociado del Documento {tipo_documento}",
                        Movimiento = "Desasocia - WEB",
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

    }

    }
