using Dapper;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Galileo.Models.ERROR;
using System.Collections.Generic;
using System.Data.Common;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaPrendaMontoDB
    {
        private readonly PortalDB _portalDb;
        public FrmPreaPrendaMontoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Ejecuta el SP spCrdPrea_Prendas_Gastos para consultar gastos y honorarios de prendas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="preanalisis">Identificador del preanálisis.</param>
        /// <param name="tipo">Tipo de proceso.</param>
        /// <returns>ErrorDto con la lista de gastos y honorarios.</returns>
        public ErrorDto<List<PrendaGastoDto>> CrdPrea_Prendas_Gastos(int codEmpresa, string preanalisis, string tipo)
        {
            var sql = "spCrdPrea_Prendas_Gastos";
            var parametros = new { Preanalisis = preanalisis, Tipo = tipo };
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.Query<PrendaGastoDto>(sql, parametros, commandType: System.Data.CommandType.StoredProcedure).AsList();
                return new ErrorDto<List<PrendaGastoDto>> { Result = result, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<List<PrendaGastoDto>> { Result = null, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<List<PrendaGastoDto>> { Result = null, Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Ejecuta el SP spCRD_PreaAsignaHonorariosPren para asignar honorarios a un preanálisis de prenda.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <param name="request">DTO con los parámetros del procedimiento.</param>
        /// <returns>ErrorDto con el resultado del procedimiento.</returns>
        public ErrorDto<PreaAsignaHonorariosPrenResultDto> CrdPrea_AsignaHonorariosPren(int codEmpresa, PreaAsignaHonorariosPrenRequestDto request)
        {
            var sql = "spCRD_PreaAsignaHonorariosPren";
            var parametros = new {
                pPreanalisis = request.Preanalisis,
                pIdParam = request.IdParam,
                pProceso = request.Proceso,
                pUsuario = request.Usuario
            };
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<PreaAsignaHonorariosPrenResultDto>(sql, parametros, commandType: System.Data.CommandType.StoredProcedure);
                return new ErrorDto<PreaAsignaHonorariosPrenResultDto> { Result = result, Code = 0, Description = "Ok" };
            }
            catch (DbException ex)
            {
                return new ErrorDto<PreaAsignaHonorariosPrenResultDto> { Result = null, Code = -1, Description = ex.Message };
            }
            catch (InvalidOperationException ex)
            {
                return new ErrorDto<PreaAsignaHonorariosPrenResultDto> { Result = null, Code = -1, Description = ex.Message };
            }
        }
    }
}
