using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Text;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCRPolizasPsdDb
    {
        private readonly IConfiguration _config;

        public FrmCRPolizasPsdDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<CajasUserDto>> Cajas_Usuario_Obtener(int CodEmpresa,string usuario)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<CajasUserDto>>
            {
                Code = 0,
                Description = "Operaci�n realizada correctamente",
                Result = new List<CajasUserDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
            SELECT 
                C.cod_caja AS codigo,
                C.Descripcion AS descripcion,
                C.PERIOCIDAD_CONTRASENA AS periodicidad_contrasena
            FROM cajas_definicion C
            INNER JOIN cajas_usuarios U
                ON C.cod_caja = U.cod_caja
               AND U.usuario = @usuario
            WHERE C.Activa = 1
            ORDER BY C.cod_caja";

                response.Result = cn
                    .Query<CajasUserDto>(sql, new { usuario })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasUserDto>();
            }

            return response;
        }

        
    }


}

