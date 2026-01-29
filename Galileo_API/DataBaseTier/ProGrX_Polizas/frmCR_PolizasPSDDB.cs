using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizasPsdDb
    {
        private readonly PortalDB _portalDB;

        public FrmCRPolizasPsdDb(IConfiguration config)
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
        public ErrorDto<List<PolizaPsdDto>> Poliza_PSD_Consulta(int codEmpresa,DateTime fechaCorte,string usuario,string tipo)
        {
            var response = new ErrorDto<List<PolizaPsdDto>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<PolizaPsdDto>()
            };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                response.Result = cn.Query<PolizaPsdDto>(
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

        /// <summary>
        /// Genera Polizas PSD
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>

        public ErrorDto<bool> Poliza_PSD_Genera(int codEmpresa,DateTime fechaCorte,string usuario)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "OK",
                Result = true
            };

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Execute(
                    "dbo.spPolizas_Sicama_Genera",
                    new
                    {
                        pFecha = fechaCorte,
                        pUsuario = usuario
                    },
                    commandType: System.Data.CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }
    }


}




