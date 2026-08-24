using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesMonitoreoDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesMonitoreoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtener el monitoreo de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Obtener(int CodEmpresa, DateTime fechaCorte)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string fechaCorteStr = MProGrXAuxiliarDB.validaFechaGlobal(fechaCorte, "yyyy-MM-dd 23:59:59") ?? string.Empty;

                var query = $@"exec spTes_Monitoreo_Saldos_Movimientos @pFechaCorte ";
                var result = conn.Query<TesMonitoreoDto>(query,
                new
                {
                    pFechaCorte = fechaCorteStr
                },
                commandTimeout: 120 // en segundos
                ).ToList();

                return DbHelper.CreateOkResponse<List<TesMonitoreoDto>>(result);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesMonitoreoDto>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener el monitoreo de los documentos de los bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Corte"></param>
        /// <returns></returns>
        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Documentos_Obtener(int CodEmpresa, string Corte)
        {
            DateTime fecha = DateTime.Parse(Corte, System.Globalization.CultureInfo.InvariantCulture);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string fechaCorteStr = MProGrXAuxiliarDB.validaFechaGlobal(fecha, "yyyy-MM-dd 23:59:59") ?? string.Empty;

                var query = $@"exec spTes_W_Monitoreo_Saldos_MovTesoreria @pFechaCorte ";
                var result = conn.Query<TesMonitoreoDto>(query,
                new
                {
                    pFechaCorte = fechaCorteStr
                },
                commandTimeout: 0 // en segundos
                ).ToList();

                return DbHelper.CreateOkResponse<List<TesMonitoreoDto>>(result);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesMonitoreoDto>>(ex.Message);
            }
        }
    }
}
