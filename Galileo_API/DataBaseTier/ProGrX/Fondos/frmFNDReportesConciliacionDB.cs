using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndReportesConciliacionDB
    {
        private readonly PortalDB _portalDb;

        public FrmFndReportesConciliacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de operadoras para dropdown.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_Operadoras_Obtener(int codEmpresa)
        {
            var query = @"SELECT cod_operadora AS item, descripcion AS descripcion FROM FND_Operadoras";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de entidades activas para dropdown.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_Entidades_Obtener(int codEmpresa)
        {
            var query = @"SELECT cod_institucion AS item, RTRIM(descripcion) AS descripcion FROM instituciones WHERE Activa = 1 ORDER BY descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene la lista de periodos históricos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ReportesConciliacion_PeriodosHistorico_Obtener(int codEmpresa)
        {
            var query = @"SELECT id_per_historico AS item, 
                                 CAST(anio AS varchar) + '-' + RIGHT('0' + CAST(mes AS varchar), 2) AS descripcion
                          FROM fnd_per_historico
                          ORDER BY anio DESC, mes DESC";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el detalle de un periodo histórico por ID.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="idPerHistorico">ID del periodo histórico.</param>
        /// <returns></returns>
        public ErrorDto<FndPerHistoricoDetalleModel> ReportesConciliacion_PeriodoHistoricoDetalle_Obtener(int codEmpresa, string idPerHistorico)
        {
            var query = @"SELECT * FROM fnd_per_historico WHERE id_per_historico = @idPerHistorico";
            return DbHelper.ExecuteSingleQuery<FndPerHistoricoDetalleModel>(_portalDb, codEmpresa, query, default, new { idPerHistorico });
        }
    }
}
