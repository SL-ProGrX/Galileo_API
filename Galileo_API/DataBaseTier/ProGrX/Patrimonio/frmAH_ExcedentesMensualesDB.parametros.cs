using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene la lista de parámetros de excedentes.
        /// </summary>
        public ErrorDto<List<ExcParametrosDto>> Patrimonio_frmAH_ExcedentesMensuales_Parametros_Lista(int codEmpresa)
        {
            const string sql = @"
select
    RTRIM(Cod_Parametro) as Cod_Parametro,
    RTRIM(Descripcion) as Descripcion,
    RTRIM(CONVERT(varchar(200), Valor)) as Valor
from exc_Parametros
order by cod_Parametro;";

            return DbHelper.ExecuteListQuery<ExcParametrosDto>(_portalDb, codEmpresa, sql);
        }
    }
}
