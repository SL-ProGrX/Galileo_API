using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosComprasBD
    {
        private readonly PortalDB _portalDB;

        public FrmActivosComprasBD(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Metodo de consulta de compras pendientes de activos fijos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fecha"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosComprasPendientesRegistroData>> Activos_ComprasPendientes_Consultar(int CodEmpresa, DateTime fecha, string tipo)
        {
            var result = new ErrorDto<List<ActivosComprasPendientesRegistroData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosComprasPendientesRegistroData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Parámetros totalmente tipados
                var p = new DynamicParameters();
                p.Add("@Fecha", fecha, DbType.DateTime);
                p.Add("@Tipo", tipo, DbType.String);

                // Llamada segura al SP (sin concatenaciones)
                result.Result = connection
                    .Query<ActivosComprasPendientesRegistroData>(
                        "spActivos_Compras_Pendientes_Registro",
                        p,
                        commandType: CommandType.StoredProcedure
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }
    }
}