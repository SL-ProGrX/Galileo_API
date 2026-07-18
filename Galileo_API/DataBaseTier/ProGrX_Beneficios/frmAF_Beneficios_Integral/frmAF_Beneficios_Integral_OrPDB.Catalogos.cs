using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralOrPDb
    {
        /// <summary>
        /// Obtiene los tipos de identificación (delegado al auxiliar compartido).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
            => _AuxiliarDB.TiposIdentificacion_Obtener(CodCliente);

        /// <summary>
        /// Obtiene la lista de divisas locales.
        /// </summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> DivisasLista_Obtener(int CodCliente)
        {
            const string sql = "SELECT COD_DIVISA AS item, DESCRIPCION AS descripcion FROM vSys_Divisas WHERE DIVISA_LOCAL = 1";

            var result = DbHelper.ExecuteListQuery<AfBeneficioIntegralDropsLista>(CreatePortalDb(), CodCliente, sql);

            return new ErrorDto<List<AfBeneficioIntegralDropsLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralDropsLista>()
            };
        }

        /// <summary>
        /// Obtiene la lista de bancos para el usuario (SP spCrd_W_SGT_Bancos).
        /// </summary>
        public ErrorDto<List<AfBeneficioIntegralGenericLista>> BancosLista_Obtener(int CodCliente, string Usuario)
        {
            const string sql = "EXEC spCrd_W_SGT_Bancos @usuario";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfBeneficioIntegralGenericLista>(sql, new { usuario = Usuario }).ToList());

            return new ErrorDto<List<AfBeneficioIntegralGenericLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralGenericLista>()
            };
        }

        /// <summary>
        /// Obtiene las cuentas bancarias del socio, con reintento sin guiones y sin duplicados por itmx.
        /// </summary>
        public ErrorDto<List<AfBeneIntegralCuentasLista>> CuentasBancariasLista_Obtener(int CodCliente, string? Cedula, int CodBanco)
        {
            if (Cedula == null)
            {
                return new ErrorDto<List<AfBeneIntegralCuentasLista>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = new List<AfBeneIntegralCuentasLista>()
                };
            }

            const string sql = "EXEC spSys_W_Cuentas_Bancarias @cedula, @codBanco";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var lista = connection.Query<AfBeneIntegralCuentasLista>(
                    sql, new { cedula = Cedula.Replace("-", ""), codBanco = CodBanco }).ToList();

                if (lista.Count == 0)
                {
                    lista = connection.Query<AfBeneIntegralCuentasLista>(
                        sql, new { cedula = Cedula, codBanco = CodBanco }).ToList();
                }

                return lista.GroupBy(x => x.itmx).Select(g => g.First()).ToList();
            });

            return new ErrorDto<List<AfBeneIntegralCuentasLista>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfBeneIntegralCuentasLista>()
            };
        }

        /// <summary>
        /// Obtiene la lista de productos de beneficios.
        /// </summary>
        public ErrorDto<List<AfiBeneProductos>> ProductosLista_Obtener(int CodCliente)
        {
            const string sql = "SELECT * FROM AFI_BENE_PRODUCTOS";

            var result = DbHelper.ExecuteListQuery<AfiBeneProductos>(CreatePortalDb(), CodCliente, sql);

            return new ErrorDto<List<AfiBeneProductos>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneProductos>()
            };
        }
    }
}
