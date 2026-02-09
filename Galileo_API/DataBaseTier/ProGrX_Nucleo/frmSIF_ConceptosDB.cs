using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifConceptosDB
    {
        private readonly int vModulo = 10; //Módulo de tesoreria
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        public FrmSifConceptosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
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
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var p = new DynamicParameters();

                // Filtro
                bool hasFilter = TryAddConceptosFiltro(filtros, p);

                // Total
                const string countNoFilter = "select COUNT(COD_CONCEPTO) from SIF_CONCEPTOS;";
                const string countWithFilter = @"select COUNT(COD_CONCEPTO)
from SIF_CONCEPTOS
where (COD_CONCEPTO LIKE @Q OR DESCRIPCION LIKE @Q OR REGISTRO_USUARIO LIKE @Q);";

                var total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                // Sorting (sin SQL dinámico)
                string sortField = NormalizeConceptosSortField(filtros?.sortField);
                int sortDir = (filtros?.sortOrder ?? 1) == 0 ? 0 : 1; // 1=ASC, 0=DESC
                p.Add("@SORTFIELD", sortField);
                p.Add("@SORTDIR", sortDir);

                // Paginación
                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtros?.paginacion ?? 30);
                p.Add("@OFFSET", offset);
                p.Add("@FETCH", fetch);

                var sb = new StringBuilder();
                sb.Append(@"select
   COD_CONCEPTO       AS cod_concepto,
   DESCRIPCION        AS descripcion,
   MOVIMIENTO_TIPO    AS movimiento_tipo,
   NIVEL_ACCESO       AS nivel_acceso,
   ACTIVO             AS activo,
   REGISTRO_FECHA     AS registro_fecha,
   REGISTRO_USUARIO   AS registro_usuario
from SIF_CONCEPTOS");

                if (hasFilter)
                {
                    sb.Append(" where (COD_CONCEPTO LIKE @Q OR DESCRIPCION LIKE @Q OR REGISTRO_USUARIO LIKE @Q) ");
                }

                sb.Append(@"
 order by
    CASE WHEN @SORTFIELD = 'COD_CONCEPTO'        AND @SORTDIR = 1 THEN COD_CONCEPTO END ASC,
    CASE WHEN @SORTFIELD = 'COD_CONCEPTO'        AND @SORTDIR = 0 THEN COD_CONCEPTO END DESC,
    CASE WHEN @SORTFIELD = 'DESCRIPCION'         AND @SORTDIR = 1 THEN DESCRIPCION END ASC,
    CASE WHEN @SORTFIELD = 'DESCRIPCION'         AND @SORTDIR = 0 THEN DESCRIPCION END DESC,
    CASE WHEN @SORTFIELD = 'MOVIMIENTO_TIPO'     AND @SORTDIR = 1 THEN MOVIMIENTO_TIPO END ASC,
    CASE WHEN @SORTFIELD = 'MOVIMIENTO_TIPO'     AND @SORTDIR = 0 THEN MOVIMIENTO_TIPO END DESC,
    CASE WHEN @SORTFIELD = 'NIVEL_ACCESO'        AND @SORTDIR = 1 THEN NIVEL_ACCESO END ASC,
    CASE WHEN @SORTFIELD = 'NIVEL_ACCESO'        AND @SORTDIR = 0 THEN NIVEL_ACCESO END DESC,
    CASE WHEN @SORTFIELD = 'ACTIVO'              AND @SORTDIR = 1 THEN ACTIVO END ASC,
    CASE WHEN @SORTFIELD = 'ACTIVO'              AND @SORTDIR = 0 THEN ACTIVO END DESC,
    CASE WHEN @SORTFIELD = 'REGISTRO_FECHA'      AND @SORTDIR = 1 THEN REGISTRO_FECHA END ASC,
    CASE WHEN @SORTFIELD = 'REGISTRO_FECHA'      AND @SORTDIR = 0 THEN REGISTRO_FECHA END DESC,
    CASE WHEN @SORTFIELD = 'REGISTRO_USUARIO'    AND @SORTDIR = 1 THEN REGISTRO_USUARIO END ASC,
    CASE WHEN @SORTFIELD = 'REGISTRO_USUARIO'    AND @SORTDIR = 0 THEN REGISTRO_USUARIO END DESC,
    COD_CONCEPTO ASC");

                sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");

                var lista = connection.Query<SifConceptoData>(sb.ToString(), p).ToList();

                return new SifConceptoLista
                {
                    total = total,
                    lista = lista
                };
            });
        }


        /// <summary>
        /// Obtiene una lista de conceptos  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifConceptoData>> SIF_Conceptos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                bool hasFilter = TryAddConceptosFiltro(filtros, p);

                var sb = new StringBuilder();
                sb.Append(@"select
   COD_CONCEPTO       AS cod_concepto,
   DESCRIPCION        AS descripcion,
   MOVIMIENTO_TIPO    AS movimiento_tipo,
   NIVEL_ACCESO       AS nivel_acceso,
   ACTIVO             AS activo,
   REGISTRO_FECHA     AS registro_fecha,
   REGISTRO_USUARIO   AS registro_usuario
from SIF_CONCEPTOS");

                if (hasFilter)
                {
                    sb.Append(" where (COD_CONCEPTO LIKE @Q OR DESCRIPCION LIKE @Q OR REGISTRO_USUARIO LIKE @Q) ");
                }

                sb.Append(" order by COD_CONCEPTO");

                return connection.Query<SifConceptoData>(sb.ToString(), p).ToList();
            });
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
            var query = @"DELETE FROM SIF_CONCEPTOS WHERE COD_CONCEPTO = @cod_concepto";
            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new { cod_concepto = NormalizeUpper(cod_concepto) });

            if ((res.Code ?? -1) != 0)
                return res;

            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Concepto : {cod_concepto}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }

            return res;
        }


        /// <summary>
        /// Inserta o actualiza un concepto.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// </summary>
        public ErrorDto SIF_Conceptos_Guardar(int CodEmpresa, string usuario, SifConceptoData concepto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                // Verifico si existe usuario activo
                const string qUsuario = @"select count(Nombre)
from usuarios
where estado = 'A'
  and UPPER(Nombre) = @usuario";

                int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usuario = NormalizeUpper(usuario) });

                if (existeuser == 0)
                {
                    throw new InvalidOperationException($"El usuario {NormalizeUpper(usuario)} no existe o no está activo.");
                }

                // Verifico si existe el concepto
                const string qExiste = @"select isnull(count(*),0) as Existe
from SIF_CONCEPTOS
where UPPER(COD_CONCEPTO) = @cod;";

                int existe = connection.QueryFirstOrDefault<int>(qExiste, new { cod = NormalizeUpper(concepto.cod_concepto) });

                if (concepto.isNew)
                {
                    if (existe > 0)
                        throw new InvalidOperationException($"El concepto con el código {concepto.cod_concepto} ya existe.");

                    ExecuteConceptosInsert(connection, CodEmpresa, usuario, concepto);
                    return 0; // dummy return for WithConn
                }

                if (existe == 0 && !concepto.isNew)
                    throw new InvalidOperationException($"El concepto con el código {concepto.cod_concepto} no existe.");

                ExecuteConceptosUpdate(connection, CodEmpresa, usuario, concepto);
                return 0; // dummy return for WithConn
            }).Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse("Error", -1);
        }


        /// <summary>
        /// Actualiza un concepto existente.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// </summary>
        private void ExecuteConceptosUpdate(SqlConnection connection, int CodEmpresa, string usuario, SifConceptoData concepto)
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
                cod_concepto = NormalizeUpper(concepto.cod_concepto),
                descripcion = NormalizeUpper(concepto.descripcion),
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


        /// <summary>
        /// Inserta un nuevo concepto.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// </summary>
        private void ExecuteConceptosInsert(SqlConnection connection, int CodEmpresa, string usuario, SifConceptoData concepto)
        {
            var query = @"INSERT INTO SIF_CONCEPTOS
                            (COD_CONCEPTO, DESCRIPCION, MOVIMIENTO_TIPO, NIVEL_ACCESO, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                          VALUES
                            (@cod_concepto, @descripcion, @movimiento_tipo, @nivel_acceso, @activo, GETDATE(), @registro_usuario);";

            connection.Execute(query, new
            {
                cod_concepto = NormalizeUpper(concepto.cod_concepto),
                descripcion = NormalizeUpper(concepto.descripcion),
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


        /// <summary>
        /// Valida si un código de concepto ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_concepto"></param>
        /// <returns></returns>
        public ErrorDto SIF_Conceptos_Valida(int CodEmpresa, string cod_concepto)
        {
            const string query = "SELECT count(COD_CONCEPTO) FROM SIF_CONCEPTOS WHERE UPPER(COD_CONCEPTO) = @COD_CONCEPTO";
            var res = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, query, 0, new { COD_CONCEPTO = NormalizeUpper(cod_concepto) });

            if ((res.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(res.Description ?? "Error", res.Code ?? -1);

            var existe = res.Result;

            if (existe > 0)
                return new ErrorDto { Code = -1, Description = "El código de concepto ya existe." };

            return new ErrorDto { Code = 0, Description = "El código de concepto es válido." };
        }


        /// <summary>
        /// Obtiene todos los documentos y marca cuáles están asociados al concepto por el código.
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_concepto"></param>
        /// </summary>
        public ErrorDto<List<SifConceptoDocumentoData>> SIF_ConceptosDocumentos_Obtener(int CodEmpresa, string cod_concepto)
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

            return DbHelper.ExecuteListQuery<SifConceptoDocumentoData>(_portalDB, CodEmpresa, query, new { cod_concepto = NormalizeUpper(cod_concepto) });
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
            var query = @"INSERT INTO SIF_CONCEPTOS_DOCUMENTO 
                          (Cod_Concepto, Tipo_Documento, Registro_Usuario, Registro_Fecha)
                          VALUES (@cod_concepto, @tipo_documento, @usuario, GETDATE());";

            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new
            {
                cod_concepto = NormalizeUpper(cod_concepto),
                tipo_documento = NormalizeUpper(tipo_documento),
                usuario
            });

            if ((res.Code ?? -1) != 0)
                return res;

            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Concepto {cod_concepto} asociado al Documento {tipo_documento}",
                    Movimiento = "Asocia - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }

            return res;
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
            var query = @"DELETE FROM SIF_CONCEPTOS_DOCUMENTO 
                          WHERE Cod_Concepto = @cod_concepto AND Tipo_Documento = @tipo_documento;";

            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new
            {
                cod_concepto = NormalizeUpper(cod_concepto),
                tipo_documento = NormalizeUpper(tipo_documento)
            });

            if ((res.Code ?? -1) != 0)
                return res;

            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Concepto {cod_concepto} desasociado del Documento {tipo_documento}",
                    Movimiento = "Desasocia - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }

            return res;
        }
        // --- Helper methods for filtros and sort normalization ---
        private static string NormalizeUpper(string? value)
            => (value ?? string.Empty).Trim().ToUpper();

        private static bool TryAddConceptosFiltro(FiltrosLazyLoadData filtros, DynamicParameters p)
        {
            string q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%");
            return true;
        }

        private static string NormalizeConceptosSortField(string? sortField)
        {
            string sf = (sortField ?? string.Empty).Trim().ToUpperInvariant();
            return sf switch
            {
                "COD_CONCEPTO" => "COD_CONCEPTO",
                "DESCRIPCION" => "DESCRIPCION",
                "MOVIMIENTO_TIPO" => "MOVIMIENTO_TIPO",
                "NIVEL_ACCESO" => "NIVEL_ACCESO",
                "ACTIVO" => "ACTIVO",
                "REGISTRO_FECHA" => "REGISTRO_FECHA",
                "REGISTRO_USUARIO" => "REGISTRO_USUARIO",
                _ => "COD_CONCEPTO"
            };
        }
    }
}