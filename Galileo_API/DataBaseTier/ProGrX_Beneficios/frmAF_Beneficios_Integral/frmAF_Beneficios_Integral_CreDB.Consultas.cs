using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class frmAF_Beneficios_Integral_CreDB
    {
        /// <summary>
        /// Obtiene el registro Crece de un beneficio por consecutivo y código de beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="consec">Consecutivo del beneficio.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Registro Crece del beneficio.</returns>
        public ErrorDto<AfiBeneSocioCreceDto> BeneSocioCrece_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            const string sql = @"
                SELECT [ID_CRECE],[COD_BENEFICIO],[CONSEC],[CAPACITACION_CMP],[APLICA_PRODUCTO],
                       [COUTA_INICIAL],[COUTA_APLICAR],[AHORRO],[LIQUIDEZ],[OBSERVACIONES_PROD],[APLICA_BENE],
                       [MONTO_PRIMERA_TARJETA],[ENTREGA_PRIMERA_TARJETA],[MONTO_SEGUNDA_TARJETA],[ENTREGA_SEGUNDA_TARJETA],
                       [REGISTRO_FECHA],[REGISTRO_USUARIO],[MODIFICA_FECHA],[MODIFICA_USUARIO],[OBSERVACIONES_BENE],
                       [fecha_cuota_inicial],[fecha_cuota_aplicar],[fecha_ahorro]
                FROM [dbo].[AFI_BENE_SOCIO_CRECE]
                WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<AfiBeneSocioCreceDto>(sql, new { consec, codBeneficio = cod_beneficio }));

            return new ErrorDto<AfiBeneSocioCreceDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Obtiene las sesiones del beneficio Crece (cursos) por consecutivo y código de beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="consec">Consecutivo del beneficio.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Lista de sesiones del beneficio Crece.</returns>
        public ErrorDto<List<AfiBeneSocioCreceSesionesDto>> BeneSocioCreceSesiones_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            const string sql = @"
                SELECT [ID_SESION],[COD_BENEFICIO],[CONSEC],[SESION],[ASISTENCIA],[TAREA],[NOTAS],
                       [SESION_FECHA],[REGISTRO_FECHA],[REGSITRO_USUARIO],[MODIFICA_FECHA],[MODIFICA_USUARIO]
                FROM [dbo].[AFI_BENE_SOCIO_CRECE_SESIONES]
                WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneSocioCreceSesionesDto>(sql, new { consec, codBeneficio = cod_beneficio }).ToList());

            return new ErrorDto<List<AfiBeneSocioCreceSesionesDto>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneSocioCreceSesiones_Obtener - " + result.Description,
                Result = result.Result ?? new List<AfiBeneSocioCreceSesionesDto>()
            };
        }
    }
}
