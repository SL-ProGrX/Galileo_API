using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOIncobrablesListDB
    {
        private readonly PortalDB _portalDB;

        public FrmCOIncobrablesListDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el nombre asociado a la cedula indicada.
        /// </summary>
        /// <summary>
        /// Obtiene el nombre del socio asociado a la cedula indicada.
        /// </summary>
        public ErrorDto<string> Nombre_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
            SELECT ISNULL(nombre, '')
            FROM socios
            WHERE cedula = @cedula";

                return conn.QueryFirstOrDefault<string>(
                    sql,
                    new { cedula },
                    commandTimeout: 60
                ) ?? string.Empty;
            });
        }


        /// <summary>
        /// Consulta las operaciones con registro de incobrables activos por cedula.
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// </summary>
        public ErrorDto<List<CbrIncobrableListaItem>> CoIncobrablesList_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string spName = "spCbr_Incobrable_Lista";

                var cmd = new CommandDefinition(
                    commandText: spName,
                    parameters: new { cedula },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                return conn.Query<CbrIncobrableListaItem>(cmd).ToList();
            });
        }

        /// <summary>
        /// Consulta los movimientos registrados para una operacion incobrable.
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="cxcOperacion"></param>
        /// </summary>
        public ErrorDto<List<CbrIncobrableMovimientoItem>> CoIncobrablesListMovimientos_Obtener(
            int codEmpresa,
            int operacion,
            int cxcOperacion)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string spName = "spCbr_Incobrable_Movimientos";

                var cmd = new CommandDefinition(
                    commandText: spName,
                    parameters: new
                    {
                        Operacion = operacion,
                        CxC_Operacion = cxcOperacion
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                return conn.Query<CbrIncobrableMovimientoItem>(cmd).ToList();
            });
        }
    }
}
