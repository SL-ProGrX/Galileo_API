using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXEsquemasDB
    {
        private readonly PortalDB _portalDB;

        public FrmCntXEsquemasDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las contabilidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ContabilidadDto>> ObtenerContabilidades(int codEmpresa)
        {
            var response = new ErrorDto<List<ContabilidadDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                    SELECT
                        COD_CONTABILIDAD   AS cod_contabilidad,
                        NOMBRE             AS nombre,
                        Nivel1,
                        Nivel2,
                        Nivel3,
                        Nivel4,
                        Nivel5,
                        Nivel6,
                        Nivel7,
                        Nivel8
                    FROM CntX_Contabilidades
                ";

                response.Result = cn
                    .Query<ContabilidadDto>(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Copia las contabilidades
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codFuente"></param>
        /// <param name="codDestino"></param>
        /// <param name="inicializa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Copiar(int codEmpresa, int codFuente, int codDestino, bool inicializa, string usuario)
        {
            var response = new ErrorDto();

            try
            {
                using var cn = new SqlConnection(_portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                cn.Execute("spCntX_Util_Contabilidad_Copia",
                    new
                    {
                        CodFuente = codFuente,
                        CodDestino = codDestino,
                        Inicializa = inicializa ? 1 : 0,
                        Usuario = usuario,
                        Token = "*xHM1tOk3n$"
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
