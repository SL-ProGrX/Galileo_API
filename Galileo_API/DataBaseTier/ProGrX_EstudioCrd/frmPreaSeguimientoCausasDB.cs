using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaSeguimientoCausasDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaSeguimientoCausasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las causas de seguimiento y marca las que ya están registradas para el preanálisis, tipo y código.
        /// </summary>
        public ErrorDto<FrmPreaSeguimientoCausasListaResponse> Prea_frmPreaSeguimientoCausas_Lista_Obtener(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string tipo,
            string codigo)
        {
            var Result = new FrmPreaSeguimientoCausasListaResponse();

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"
SELECT
    Cg.COD_CAUSAS AS cod_causas,
    Cg.DESCRIPCION AS descripcion,
    CASE
        WHEN ISNULL(Pa.COD_CAUSAS, '') = '' THEN CAST(0 AS bit)
        ELSE CAST(1 AS bit)
    END AS activo,
    ISNULL(CONVERT(varchar(19), Pa.REGISTRO_FECHA, 120), '') AS registro_fecha,
    ISNULL(Pa.REGISTRO_USUARIO, '') AS registro_usuario
FROM OPERACION_CAUSAS Cg
LEFT JOIN CRD_PREA_GESTION Pa
    ON Cg.COD_CAUSAS = Pa.COD_CAUSAS
    AND Cg.TIPO = Pa.TIPO
    AND Pa.COD_PREANALISIS = @cod_preanalisis
    AND Pa.CODIGO = @codigo
WHERE Cg.TIPO = @tipo
ORDER BY ISNULL(Pa.REGISTRO_FECHA, GETDATE()) ASC, Cg.COD_CAUSAS;";

                Result.lista = connection.Query<FrmPreaSeguimientoCausasDto>(
                    sql,
                    new
                    {
                        cod_preanalisis = cod_preanalisis.Trim(),
                        tipo = tipo.Trim(),
                        codigo = codigo.Trim()
                    },
                    commandType: CommandType.Text
                ).ToList();

                return DbHelper.CreateOkResponse<FrmPreaSeguimientoCausasListaResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSeguimientoCausasListaResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Registra o elimina una causa de seguimiento para el preanálisis según el estado de selección enviado.
        /// </summary>
        public ErrorDto<FrmPreaSeguimientoCausasRegistrarResponse> Prea_frmPreaSeguimientoCausas_Registrar(
            int codEmpresa,
            FrmPreaSeguimientoCausasRegistrarRequest request)
        {
            var Result = new FrmPreaSeguimientoCausasRegistrarResponse();

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                if (request.activo)
                {
                    const string insertSql = @"
INSERT INTO CRD_PREA_GESTION
(
    cod_causas,
    tipo,
    cod_preanalisis,
    codigo,
    registro_fecha,
    registro_usuario
)
VALUES
(
    @cod_causas,
    @tipo,
    @cod_preanalisis,
    @codigo,
    dbo.Mygetdate(),
    @usuario
);";

                    connection.Execute(
                        insertSql,
                        new
                        {
                            cod_causas = request.cod_causas.Trim(),
                            tipo = request.tipo.Trim(),
                            cod_preanalisis = request.cod_preanalisis.Trim(),
                            codigo = request.codigo.Trim(),
                            usuario = request.usuario.Trim()
                        },
                        commandType: CommandType.Text
                    );
                }
                else
                {
                    const string deleteSql = @"
DELETE FROM CRD_PREA_GESTION
WHERE cod_causas = @cod_causas
  AND tipo = @tipo
  AND cod_preanalisis = @cod_preanalisis
  AND codigo = @codigo;";

                    connection.Execute(
                        deleteSql,
                        new
                        {
                            cod_causas = request.cod_causas.Trim(),
                            tipo = request.tipo.Trim(),
                            cod_preanalisis = request.cod_preanalisis.Trim(),
                            codigo = request.codigo.Trim()
                        },
                        commandType: CommandType.Text
                    );
                }

                Result = new FrmPreaSeguimientoCausasRegistrarResponse
                {
                    cod_causas = request.cod_causas,
                    activo = request.activo,
                    mensaje = request.activo
                        ? "Causa registrada correctamente."
                        : "Causa eliminada correctamente."
                };

                return DbHelper.CreateOkResponse<FrmPreaSeguimientoCausasRegistrarResponse>(Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmPreaSeguimientoCausasRegistrarResponse>(ex.Message);
            }
        }
    }
}
