using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class frmAF_Beneficios_IntegralDB
    {
        /// <summary>
        /// Obtiene catálogos de tablas SYS/BENE (SP spAFI_Bene_Catalogos_Consulta).
        /// </summary>
        public ErrorDto<List<CatalogosLista>> Catalogo_Obtener(int CodEmpresa, int tipo, int modulo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<CatalogosLista>(
                    "[spAFI_Bene_Catalogos_Consulta]",
                    new { tipo, Codigo = modulo },
                    commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<CatalogosLista>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "spAFI_Bene_Catalogos_Consulta: " + result.Description,
                Result = result.Result ?? new List<CatalogosLista>()
            };
        }

        /// <summary>
        /// Lista de categorías de beneficios (apremiante, crece, sepelio, etc.).
        /// </summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneIntegralCategorias_Obtener(int CodCliente)
        {
            const string sql = "SELECT COD_CATEGORIA AS item, DESCRIPCION AS descripcion FROM AFI_BENE_CATEGORIAS";

            var result = DbHelper.ExecuteListQuery<AfBeneficioIntegralDropsLista>(CreatePortalDb(), CodCliente, sql);

            return new ErrorDto<List<AfBeneficioIntegralDropsLista>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneIntegralCategorias_Obtener: " + result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralDropsLista>()
            };
        }

        /// <summary>
        /// Lista de grupos de beneficios de una categoría.
        /// </summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> BeneficioGrupos_Obtener(int CodEmpresa, string Categoria)
        {
            const string sql = "SELECT COD_GRUPO AS item, DESCRIPCION AS descripcion FROM AFI_BENE_GRUPOS WHERE COD_CATEGORIA = @categoria";

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfBeneficioIntegralDropsLista>(sql, new { categoria = Categoria }).ToList());

            return new ErrorDto<List<AfBeneficioIntegralDropsLista>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneficioGrupos_Obtener: " + result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralDropsLista>()
            };
        }

        /// <summary>
        /// Obtiene los permisos del usuario para la categoría de beneficios.
        /// </summary>
        public ErrorDto<BeneCategoriaPermisos> ValidaUsuarioBeneficios_Obtener(int CodEmpresa, string usuario, string cod_categoria)
        {
            const string sql = @"
                SELECT [USUARIO],[I_CAMBIAR_ESTADO],[I_MODIFICA_EXPEDIENTE],[I_TRASLADO_TESORERIA],
                       [I_PAGO_PROGRAMAR],[I_PAGO_APROBAR_M],[I_PAGO_REALIZAR],[I_INGRESAR_SOLICITUD],[I_PERIODO],
                       [I_PAGO_CONSULTA],[I_APROBAR],[I_RECHAZAR],[I_ANULAR],[I_DEVOLVER_RESOLUCION]
                FROM AFI_BENE_GRUPOS_ROLES
                WHERE USUARIO = @usuario AND COD_CATEGORIA = @codCategoria";

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<BeneCategoriaPermisos>(sql, new { usuario, codCategoria = cod_categoria })
                    ?? new BeneCategoriaPermisos());

            return new ErrorDto<BeneCategoriaPermisos>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "ValidaUsuarioBeneficios_Obtener: " + result.Description,
                Result = result.Result ?? new BeneCategoriaPermisos()
            };
        }
    }
}
