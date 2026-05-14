using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCalcePLazosDB
    {
        private readonly IConfiguration _config;

        public FrmFndCalcePLazosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            const string query = @"
                    SELECT
                        id_per_historico AS item,
                        CONCAT(anio, '-', mes) AS descripcion
                    FROM dbo.fnd_per_historico
                    ORDER BY anio DESC, mes DESC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(new PortalDB(_config), CodEmpresa, query);
        }
    }
}