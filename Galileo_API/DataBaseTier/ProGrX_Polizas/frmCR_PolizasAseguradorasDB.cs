using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizasAseguradorasDb
    {
        private readonly PortalDB _portalDB;

        public FrmCRPolizasAseguradorasDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las polizas PSD
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<PolizaAseguradoraDto>> Poliza_PSD_Consulta(int codEmpresa,DateTime fechaCorte,string usuario,string tipo)
        {
            var response = new ErrorDto<List<PolizaAseguradoraDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PolizaAseguradoraDto>()
            };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<PolizaAseguradoraDto>(
                    "spPoliza_PSD",
                    new
                    {
                        Poliza = "",              
                        Corte = fechaCorte,
                        Usuario = usuario,         
                        Movimiento = tipo,  
                        Cedula = ""       
                    },
                    commandType: System.Data.CommandType.StoredProcedure
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




