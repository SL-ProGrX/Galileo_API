using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralRecDB
    {
        /// <summary>
        /// Obtiene los datos del reconocimiento asociado a un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="id_beneficio">Identificador del beneficio.</param>
        /// <returns>Datos del reconocimiento.</returns>
        public ErrorDto<AfiBeneReconocimientosDatos> BeneReconocimiento_Obtener(int CodCliente, int id_beneficio)
        {
            const string sql = "EXEC spAFI_Bene_Socio_Reconocimiento_Consultar @id_beneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneReconocimientosDatos>(sql, new { id_beneficio }).FirstOrDefault());

            return new ErrorDto<AfiBeneReconocimientosDatos>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneReconocimiento_Obtener: " + result.Description,
                Result = result.Result
            };
        }
    }
}
