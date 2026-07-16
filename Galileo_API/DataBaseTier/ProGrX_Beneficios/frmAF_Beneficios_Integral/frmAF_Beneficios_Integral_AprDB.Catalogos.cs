using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralAprDB
    {
        /// <summary>Categorías de apremiantes (APT).</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> CategoriaAPT_Obtener(int CodCliente)
        {
            const string sql = "SELECT ID_APT_CATEGORIA AS item, DESCRIPCION AS descripcion FROM AFI_BENE_APT_CATEGORIAS WHERE Activo = 1 ORDER BY Descripcion ASC";
            return EnvolverDropsLista(DbHelper.ExecuteListQuery<AfBeneficioIntegralDropsLista>(CreatePortalDb(), CodCliente, sql));
        }

        /// <summary>Profesionales de apremiantes (APT).</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProfecionalAPT_Obtener(int CodCliente)
        {
            const string sql = "SELECT ID_PROFESIONAL AS item, NOMBRE AS descripcion FROM AFI_BENE_APT_PROFESIONALES WHERE Activo = 1 ORDER BY NOMBRE ASC";
            return EnvolverDropsLista(DbHelper.ExecuteListQuery<AfBeneficioIntegralDropsLista>(CreatePortalDb(), CodCliente, sql));
        }

        /// <summary>
        /// Lista de motivos de justificación disponibles para la categoría.
        /// </summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneMotivoLista_Obtener(int CodCliente, string? categoria)
        {
            if (categoria == null)
            {
                return new ErrorDto<List<AfBeneficioIntegralDropsLista>> { Code = 0, Description = "Ok", Result = new List<AfBeneficioIntegralDropsLista>() };
            }

            const string sql = @"
                SELECT COD_MOTIVO AS item, DESCRIPCION AS descripcion FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO IN (
                    SELECT COD_MOTIVO FROM AFI_BENE_GRUPO_MOTIVOS WHERE COD_GRUPO IN (
                        SELECT COD_GRUPO FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = @categoria))";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfBeneficioIntegralDropsLista>(sql, new { categoria }).ToList());

            return new ErrorDto<List<AfBeneficioIntegralDropsLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralDropsLista>()
            };
        }

        /// <summary>Costo de manutención configurado.</summary>
        public ErrorDto<float> CostoManutencion_Obtener(int CodCliente)
            => ObtenerCostoParametro(CodCliente, "CodManutencion");

        /// <summary>Costo de deducción configurado.</summary>
        public ErrorDto<float> CostoDeduccion_Obtener(int CodCliente)
            => ObtenerCostoParametro(CodCliente, "CodDeduccion");

        /// <summary>
        /// Lee de configuración el código de parámetro y devuelve su VALOR desde SIF_PARAMETROS.
        /// </summary>
        private ErrorDto<float> ObtenerCostoParametro(int CodCliente, string claveConfig)
        {
            var codParametro = _config.GetSection("AFI_Beneficios").GetSection(claveConfig).Value ?? string.Empty;
            const string sql = "SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = @codParametro";

            var result = DbHelper.ExecuteSingleQuery<float>(CreatePortalDb(), CodCliente, sql, 0, new { codParametro });

            return new ErrorDto<float>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result?.Result ?? 0f
            };
        }

        /// <summary>
        /// Envuelve el resultado de una consulta de catálogo en el estándar de respuesta (lista no nula).
        /// </summary>
        private static ErrorDto<List<AfBeneficioIntegralDropsLista>> EnvolverDropsLista(ErrorDto<List<AfBeneficioIntegralDropsLista>> result)
            => new()
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralDropsLista>()
            };
    }
}
