using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        /// <summary>
        /// Diferidos Obtener
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Diferidos_Obtener(int codEmpresa, int codContabilidad)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT cod_diferido AS Item,
                   descripcion AS Descripcion
            FROM CntX_Diferidos
            WHERE cod_contabilidad = @codContabilidad
            ORDER BY cod_diferido";

                response.Result = cn.Query<DropDownListaGenericaModel>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene plantillas diferidos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codDiferido"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxDiferidoPlantillaDto>> DiferidoPlantillas_Obtener(int codEmpresa,int codContabilidad,int codDiferido)
        {
            var response = new ErrorDto<List<CntxDiferidoPlantillaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
        SELECT 
            COD_DIFPLANTILLA      AS Item,
            DESCRIPCION           AS Descripcion,
            MONTO_DIFERIR         AS Monto,
            ACUMULADO             AS Acumulado,
            (MONTO_DIFERIR - ACUMULADO) AS Pendiente,
            PLAZO                 AS Plazo,
            FECHA_CREA            AS Inicio,
            USER_CREA             AS Usuario,
            DOCUMENTO             AS Documento
        FROM CntX_diferido_plantilla
        WHERE COD_CONTABILIDAD = @codContabilidad
          AND COD_DIFERIDO = @codDiferido
        ORDER BY COD_DIFPLANTILLA";

                response.Result = cn.Query<CntxDiferidoPlantillaDto>(
                    sql,
                    new { codContabilidad, codDiferido }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Obtiene diferidos historicos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codDiferido"></param>
        /// <param name="codPlantilla"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxDiferidoHistoricoDto>> DiferidoHistorico_Obtener(int codEmpresa, int codContabilidad, int codDiferido, int codPlantilla)
        {
            var response = new ErrorDto<List<CntxDiferidoHistoricoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT num_asiento,
                   tipo_asiento,
                   fecha,
                   anio,
                   mes,
                   usuario
            FROM CntX_Diferido_Historico
            WHERE cod_contabilidad = @codContabilidad
              AND cod_difPlantilla = @codPlantilla
            ORDER BY anio, mes";

                response.Result = cn.Query<CntxDiferidoHistoricoDto>(
                    sql,
                    new { codContabilidad, codDiferido, codPlantilla }
                ).ToList();
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
