using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesReportesAutorizacionesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MTesoreria mTesoreria;

        public FrmTesReportesAutorizacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mTesoreria = new MTesoreria(config);
        }

        /// <summary>
        /// Carga el combo de acceso general para los bancos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        /// <summary>
        /// Carga el combo de tipos de documentos para la carga de archivos en tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>  sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return mTesoreria.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        /// <summary>
        /// Obtiene una lista de usuarios activos para ubicaciones de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_RepAuthUsuarios_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select Nombre as 'item',descripcion from usuarios where estado = 'A' ";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


    }
}
