using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndContratosInformesModels;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndContratosInformesDB
    {
        private readonly PortalDB _portalDB; 

        public FrmCoAplFndContratosInformesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config); 
        }

        /// <summary>
        /// Consulta el listado de personas para filtro en reporte
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplFndContratosInformes_Personas_Result>> Co_AplFnd_ContratosInformes_Personas_Obtener(int codEmpresa)
        {
            const string query = @"select Cedula, cedular, nombre from socios order by nombre";
            return DbHelper.ExecuteListQuery<CoAplFndContratosInformes_Personas_Result>(_portalDB, codEmpresa, query);
        }
        
        /// <summary>
        /// Consulta de listado de aplicaiones para filtro en reporte
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplFnd_ContratosInformes_Aplicaciones_Obtener(int codEmpresa)
        {
            var query = """
                SELECT Top 100 ID_APLICACION AS Item,
                '['  + format(ID_APLICACION, '000') + '] ' + CONVERT(varchar, Fecha,21) + '   -> ' + rtrim(USUARIO)  AS Descripcion
                from CBR_APLICA_PAGOS_MORA_FONDOS order by Fecha desc
                """;

            return DbHelper.WithConn(
                _portalDB,
                codEmpresa,
                connection =>
                    connection
                        .Query<DropDownListaGenericaModel>(query)
                        .ToList());
        }

    }
}
