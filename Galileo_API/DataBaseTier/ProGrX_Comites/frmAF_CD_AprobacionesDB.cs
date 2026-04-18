using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdAprobacionesDb
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdAprobacionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }
        /// <summary>
        /// En lista las aprobaciones pendientes
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<AfcdAprobacionDto>> Listar(int codEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                string sql;

                if (banco == 0) // TODOS
                {
                    sql = @"
                        SELECT 
                            0 AS valorx,
                            C.noperacion,
                            C.cod_comite,
                            ISNULL(U.descripcion,'') AS comite,
                            C.cedula,
                            ISNULL(S.nombre,'') AS nombre,
                            C.cuenta,
                            COALESCE(SUM(M.monto),0) AS total
                        FROM afi_cd_cuentas C
                        LEFT JOIN Uprogramatica U ON C.cod_comite = U.codigo
                        LEFT JOIN Socios S ON C.cedula = S.cedula
                        LEFT JOIN afi_cd_cuentas_actividades M ON C.nOperacion = M.nOperacion
                        WHERE C.estado = 'S'
                        GROUP BY 
                            C.noperacion,
                            C.cod_comite,
                            U.descripcion,
                            C.cedula,
                            S.nombre,
                            C.cuenta
                    ";
                }
                else
                {
                    sql = @"
                        SELECT 
                            0 AS valorx,
                            C.noperacion,
                            C.cod_comite,
                            ISNULL(U.descripcion,'') AS comite,
                            C.cedula,
                            ISNULL(S.nombre,'') AS nombre,
                            C.cuenta,
                            COALESCE(SUM(M.monto),0) AS total
                        FROM afi_cd_cuentas C
                        LEFT JOIN Uprogramatica U ON C.cod_comite = U.codigo
                        LEFT JOIN Socios S ON C.cedula = S.cedula
                        LEFT JOIN afi_cd_cuentas_actividades M ON C.nOperacion = M.nOperacion
                        WHERE C.estado = 'S'
                          AND C.id_banco = @banco
                        GROUP BY 
                            C.noperacion,
                            C.cod_comite,
                            U.descripcion,
                            C.cedula,
                            S.nombre,
                            C.cuenta
                    ";
                }

                return conn.Query<AfcdAprobacionDto>(sql, new { banco }).ToList();
            });
        }

        /// <summary>
        /// Carga la lista de bancos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Bancos(int codEmpresa)
        {
            const string sql = @"
                SELECT 
                    B.id_banco AS item,
                    B.descripcion AS descripcion
                FROM Tes_bancos B
                INNER JOIN afi_cd_cuentas C ON B.id_banco = C.id_banco
                WHERE C.estado = 'S'
                GROUP BY B.id_banco, B.descripcion
            ";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Aprueba 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<bool> Aprobar(AfcdAprobacionRequest req)
        {
            return DbHelper.WithConn(_portalDb, req.codEmpresa, conn =>
            {
                foreach (var op in req.operaciones)
                {
                    conn.Execute(
                        "spAFI_CD_AsientoCuentas",
                        new
                        {
                            nOperacion = op,
                            Usuario = req.usuario,
                            Oficina = req.oficina
                        },
                        commandType: CommandType.StoredProcedure
                    );
                }

                return true;
            });
        }

        /// <summary>
        /// Rechaza
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<bool> Rechazar(AfcdRechazoRequest req)
        {
            return DbHelper.WithConn(_portalDb, req.codEmpresa, conn =>
            {
                const string sql = @"
                    UPDATE afi_cd_cuentas
                    SET estado = 'R'
                    WHERE noperacion = @op
                ";

                foreach (var op in req.operaciones)
                {
                    conn.Execute(sql, new { op });
                }

                return true;
            });
        }
    
        /// <summary>
        /// Obtiene la oficina
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
    public ErrorDto<OficinaUsuarioAprobacionDto> Oficina_ObtenerPorUsuario(int codEmpresa, string usuario)
        {
            var response = new ErrorDto<OficinaUsuarioAprobacionDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"exec sbSIFOficinasUsuario @usuario";

                var result = cn.QueryFirstOrDefault<OficinaUsuarioAprobacionDto>(
                    sql,
                    new { usuario }
                );

                response.Result = result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}