using System.Data;
using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralReqDB
    {
        /// <summary>
        /// Obtiene la lista de requisitos del beneficio para el formulario de registro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="consec">Consecutivo del beneficio.</param>
        /// <returns>Lista de requisitos con indicador de asignación.</returns>
        public ErrorDto<List<BeneRegRequisito>> Bene_Registro_Requisitos_Obtener(int CodCliente, int consec)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<BeneRegRequisito>(
                    "[spAFI_Bene_Registro_Requisitos_List]",
                    new { Consec_Bene = consec },
                    commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<BeneRegRequisito>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "Bene_Registro_Requisitos_Obtener: " + result.Description,
                Result = result.Result ?? new List<BeneRegRequisito>()
            };
        }
    }
}
