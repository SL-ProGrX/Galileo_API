using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfLiquidacionReportesDB
    {
        private readonly IConfiguration _config;
        
        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           descripcion
                    FROM dbo.instituciones;";

        private const string SqlLiquidacion = @"
                    SELECT L.mortalidad,
                           L.tneto,
                           B.descripcion
                    FROM dbo.liquidacion L
                    LEFT JOIN dbo.Tes_Bancos B
                        ON L.cod_banco = B.id_banco
                    WHERE L.consec = @Liquidacion;";

        public FrmAfLiquidacionReportesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de instituciones disponibles para reportes de liquidación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqReportes_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Obtiene los datos de una liquidación para reportes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="liquidacion">Consecutivo de liquidación.</param>
        /// <returns>Datos de la liquidación.</returns>
        public ErrorDto<AfLiquidacionReportesData?> AF_LiqReportes_Obtener(int CodEmpresa, int liquidacion)
        {
            return DbHelper.ExecuteSingleQuery<AfLiquidacionReportesData>(
                CreatePortalDb(),
                CodEmpresa,
                SqlLiquidacion,
                null,
                new
                {
                    Liquidacion = liquidacion
                });
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);
    }
}