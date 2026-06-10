namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    using Galileo.DataBaseTier;
    using Galileo.Models.ERROR;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrMonitorCancelacionDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrMonitorCancelacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene monitor de cancelación de operaciones con desviación de cuota.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrMonitorCancelacionModel>> CrMonitorCancelacion_Obtener(int CodEmpresa, CrMonitorCancelacionRequest request)
        {
            const string sqlQuery = @"
                exec spCrd_Monitor_Cancelacion 
                    @Inicio,
                    @Corte,
                    @Porcentaje";

            var fechaInicio = request.Fecha_Inicio.Date;
            var fechaCorte = request.Fecha_Corte.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            return DbHelper.ExecuteListQuery<CrMonitorCancelacionModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    Inicio = fechaInicio,
                    Corte = fechaCorte,
                    request.Porcentaje
                });
        }
    }
}
