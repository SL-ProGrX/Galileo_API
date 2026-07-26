using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndContratosInformesModels;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndContratosInformesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MProGrXSecurityMainDb _MProGrXSecurityMainDb;
        private readonly int vModulo = 4;
        public FrmCoAplFndContratosInformesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _MProGrXSecurityMainDb = new MProGrXSecurityMainDb(config);
        }

        /// <summary>
        /// Consulta el listado de personas para filtro en reporte
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplFndContratosInformesPersonasResult>> Co_AplFnd_ContratosInformes_Personas_Obtener(int codEmpresa)
        {
            const string query = @"select Cedula, cedular, nombre from socios order by nombre";
            return DbHelper.ExecuteListQuery<CoAplFndContratosInformesPersonasResult>(_portalDB, codEmpresa, query);
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

        /// <summary>
        /// Registra un movimiento en la bitácora del sistema
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="strTipoMovimiento"></param>
        /// <param name="strDetalleMovimiento"></param>
        /// <returns></returns>
        public ErrorDto Co_AplFnd_ContratosInformes_Bitacora_Registrar(int codEmpresa,string usuario,string strTipoMovimiento, string strDetalleMovimiento)
        { 
          return  _MProGrXSecurityMainDb.Bitacora(new MProGrXSecurityMainBitacora
            {
                    CodEmpresa = codEmpresa,
                usuario = (usuario ?? string.Empty).ToUpper(),
                strDetalleMovimiento = strDetalleMovimiento,
                strTipoMovimiento = strTipoMovimiento, 
                vModulo = vModulo
            });
        }
 

    }
}
