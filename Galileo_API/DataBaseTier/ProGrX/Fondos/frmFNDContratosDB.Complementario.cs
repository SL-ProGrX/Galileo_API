using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {
        private const string SpCuentasBancarias = "spSys_Cuentas_Bancarias";

        #region Complementario
        /// <summary>
        /// Obtiene las cuentas bancarias asociadas a una persona y banco.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula del cliente.</param>
        /// <param name="cod_banco">Código del banco.</param>
        /// <returns>Listado de cuentas bancarias disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_CuentasBancarias_Obtener(int CodEmpresa, string cedula, int cod_banco)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query(
                    SpCuentasBancarias,
                    new
                    {
                        Identificacion = NormalizarTexto(cedula),
                        BancoId = cod_banco,
                        DivisaCheck = 1
                    },
                    commandType: System.Data.CommandType.StoredProcedure)
                .Select(r => new DropDownListaGenericaModel
                {
                    item = Convert.ToString(r.IdX) ?? string.Empty,
                    descripcion = Convert.ToString(r.ItmX) ?? string.Empty
                }).ToList());

            return new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<DropDownListaGenericaModel>()
            };
        }

        #endregion

    }
}