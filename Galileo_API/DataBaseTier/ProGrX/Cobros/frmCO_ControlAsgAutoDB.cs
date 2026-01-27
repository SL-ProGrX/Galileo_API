using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoControlAsgAutoDB
    {
        private readonly PortalDB _portalDb;

        public FrmCoControlAsgAutoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de usuarios activos de cbr_usuarios.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de usuarios activos.</returns>
        public ErrorDto<List<CbrUsuarioResult>> CbrUsuarios_Activos_Lista(int codEmpresa)
        {
            var query = "SELECT usuario FROM cbr_usuarios WHERE estado = 1";
            return DbHelper.ExecuteListQuery<CbrUsuarioResult>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Consulta los grupos y si están vinculados con el usuario de cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros: usuario logueado y filtro opcional.</param>
        /// <returns>Lista de grupos y su estado de asignación.</returns>
        public ErrorDto<List<CbrUsuarioGrupoListResult>> CbrUsuarios_Grupos_List(int codEmpresa, CbrUsuarioGrupoListParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dbParam = new { param.Usuario, Filtro = param.Filtro ?? string.Empty };
                return conn.Query<CbrUsuarioGrupoListResult>(
                    "spCbr_Usuarios_Grupos_List",
                    dbParam,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Ejecuta la distribución de casos por rol de usuario en cobros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de distribución.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<CbrControlDistribucionResult?> CbrControlDistribucion(int codEmpresa, CbrControlDistribucionParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dbParam = new
                {
                    param.Tipo,
                    param.Inicializa,
                    param.MantenerNuevos,
                    param.CasosMorosos,
                    param.CasosAlDia,
                    param.Grupo
                };
                return conn.QueryFirstOrDefault<CbrControlDistribucionResult>(
                    "spCBRControlDistribucion",
                    dbParam,
                    commandType: System.Data.CommandType.StoredProcedure
                );
            });
        }
    }
}
