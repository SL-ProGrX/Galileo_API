using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        /// <summary>
        /// Obtiene la lista de beneficios de una categoría, con su tipo y monto de grupo.
        /// </summary>
        public ErrorDto<List<BeneficiosLista>> BeneficiosLista_Obtener(int CodCliente, string categoria)
        {
            const string sql = @"
                SELECT B.COD_BENEFICIO AS item, B.DESCRIPCION, B.TIPO,
                       (SELECT MONTO FROM AFI_BENE_GRUPOS G WHERE G.COD_GRUPO = B.COD_GRUPO) AS MONTO
                FROM AFI_BENEFICIOS B
                WHERE COD_CATEGORIA = @categoria";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<BeneficiosLista>(sql, new { categoria }).ToList());

            return new ErrorDto<List<BeneficiosLista>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneficiosLista_Obtener - " + result.Description,
                Result = result.Result ?? new List<BeneficiosLista>()
            };
        }

        /// <summary>Lista de profesionales APT.</summary>
        public ErrorDto<List<BeneApreLista>> AfiBeneProfesionales_Obtener(int CodCliente)
        {
            const string sql = @"SELECT [ID_PROFESIONAL] AS item, CONCAT(IDENTIFICACION, ' ', [NOMBRE]) AS descripcion
                                  FROM AFI_BENE_APT_PROFESIONALES WHERE ACTIVO = 1";

            var result = DbHelper.ExecuteListQuery<BeneApreLista>(CreatePortalDb(), CodCliente, sql);

            return new ErrorDto<List<BeneApreLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<BeneApreLista>()
            };
        }

        /// <summary>Lista de categorías APT.</summary>
        public ErrorDto<List<BeneApreLista>> AfiBeneCategorias_Obtener(int CodCliente)
        {
            const string sql = "SELECT [ID_APT_CATEGORIA] AS item, [DESCRIPCION] AS descripcion FROM AFI_BENE_APT_CATEGORIAS WHERE ACTIVO = 1";

            var result = DbHelper.ExecuteListQuery<BeneApreLista>(CreatePortalDb(), CodCliente, sql);

            return new ErrorDto<List<BeneApreLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<BeneApreLista>()
            };
        }
    }
}
