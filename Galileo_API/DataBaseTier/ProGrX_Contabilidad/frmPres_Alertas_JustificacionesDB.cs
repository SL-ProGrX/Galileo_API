using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class PresAlertasJustificacionesDB
    {
        private readonly PortalDB _portalDb;
        public PresAlertasJustificacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// método para obtener la bitácora de alertas y justificaciones de un periodo específico, filtrando por empresa, contabilidad, modelo, unidad, centro de costo, cuenta y tipo de alerta.
        /// </summary>
        /// <param name="resquest"></param>
        /// <returns></returns>
        public ErrorDto<List<PresAlertaJustificacionBitacoraData>> PresAlertaJustificacionBit_Obtener(
            PresAlertaJustificacionBitRequest resquest)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, resquest.codEmpresa);

            var result = new ErrorDto<List<PresAlertaJustificacionBitacoraData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PresAlertaJustificacionBitacoraData>()
            };

            try
            {
                const string sql = @"
                        SELECT
                              *
                        FROM dbo.PRES_ALERTAS_JUSTIFICACIONES_BIT
                        WHERE cod_empresa = @codEmpresa
                          AND cod_conta = @codConta
                          AND cod_modelo = @codModelo
                          AND anio = @anio
                          AND mes = @mes
                        ORDER BY id_bitacora DESC;";

                result.Result = connection.Query<PresAlertaJustificacionBitacoraData>(
                    sql,
                    new
                    {
                        resquest.codEmpresa,
                        resquest.codConta,
                        resquest.codModelo,
                        resquest.codUnidad,
                        resquest.codCentroCosto,
                        resquest.codCuenta,
                        resquest.anio,
                        resquest.mes,
                        resquest.tipoAlerta
                    }
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<PresAlertaJustificacionBitacoraData>();
            }

            return result;
        }

    }
}
