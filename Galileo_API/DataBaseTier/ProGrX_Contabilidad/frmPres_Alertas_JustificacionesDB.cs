using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
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
            return DbHelper.WithConn(_portalDb, resquest.codEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                              *
                        FROM dbo.PRES_ALERTAS_JUSTIFICACIONES_BIT
                        WHERE cod_empresa = @codEmpresa
                          AND cod_conta = @codConta
                          AND cod_modelo = @codModelo
                          AND anio = @anio
                          AND mes = @mes
                        ORDER BY id_bitacora DESC;";

                return conn.Query<PresAlertaJustificacionBitacoraData>(
                    query,
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

            });
        }

    }
}
