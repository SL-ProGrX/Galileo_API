using Galileo.Models;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasBalanzaPagosDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizasBalanzaPagosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de pólizas para combos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista genérica para combos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Polizas_Combo_Lista(int codEmpresa)
        {
            var query = @"SELECT CAST(COD_POLIZA AS varchar) AS item, DESCRIPCION AS descripcion FROM CRD_CATALOGO_POLIZAS";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Ejecuta el informe de balance de pagos por cierre.
        /// </summary>
        public ErrorDto<List<T>> Poliza_Informe_Balance_Pago<T>(int codEmpresa, PolizaBalancePagoParams param)
        {
            var sql = "spPoliza_Informe_Balance_Pago";
            var parameters = new
            {
                param.Poliza,
                param.Corte,
                param.TipoInforme,
                param.Balanza,
                param.Cedula,
                param.AseguradoraId
            };

            using var conn = _portalDb.CreateConnection(codEmpresa);
            var result = conn.Query<T>(sql, parameters, commandType: System.Data.CommandType.StoredProcedure);
            return DbHelper.CreateOkResponse(result.AsList());
        }
    }
}
