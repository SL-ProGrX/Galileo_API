using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPrendasDb
    {
        public ErrorDto<List<CrPrendaCotizadorData>> CrPrendas_Cotizadores_Obtener(int codEmpresa)
        {
            const string sql = @"
                select ID as id,
                       rtrim(isnull(DESCRIPCION,'')) as descripcion,
                       rtrim(isnull(NOMBRE_ARCHIVO,'')) as nombre_archivo,
                       FECHA_VENCE as fecha_vence
                from PREN_GARANTIAS_COTIZADORES with (nolock)
                order by DESCRIPCION, NOMBRE_ARCHIVO;";

            var response = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<CrPrendaCotizadorData>(sql, commandTimeout: 10).ToList());

            return new ErrorDto<List<CrPrendaCotizadorData>>
            {
                Code = response.Code,
                Description = response.Description,
                Result = response.Result ?? []
            };
        }

        public ErrorDto<CrPrendaCotizadorArchivoData> CrPrendas_CotizadorArchivo_Obtener(
            int codEmpresa,
            long id)
        {
            const string sql = @"
                select rtrim(isnull(NOMBRE_ARCHIVO,'')) as nombre_archivo,
                       ARCHIVO as archivo
                from PREN_GARANTIAS_COTIZADORES with (nolock)
                where ID = @Id;";

            var response = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.QueryFirstOrDefault<CrPrendaCotizadorArchivoData>(
                    sql,
                    new { Id = id },
                    commandTimeout: 15));

            if (response.Code < 0 || response.Result is null)
            {
                return new ErrorDto<CrPrendaCotizadorArchivoData>
                {
                    Code = -1,
                    Description = response.Description ?? "No se encontró el archivo solicitado.",
                    Result = new CrPrendaCotizadorArchivoData()
                };
            }

            return new ErrorDto<CrPrendaCotizadorArchivoData>
            {
                Code = 0,
                Result = response.Result
            };
        }
    }
}
