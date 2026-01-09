using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Text;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasDatosAsociadosDb
    {
        private readonly IConfiguration _config;

        public FrmCajasDatosAsociadosDb(IConfiguration config)
        {
            _config = config;
        }

     
        /// <summary>
        /// Obtiene cajas consulta creditos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCreditoDto>> Cajas_Consulta_Creditos(
            int codEmpresa,
            string cedula)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasCreditoDto>>
            {
                Code = 0,
                Result = new List<CajasCreditoDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = "exec spCajas_Consulta_Creditos @cedula";

                response.Result = cn
                    .Query<CajasCreditoDto>(sql, new { cedula })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasCreditoDto>();
            }

            return response;
        }

          /// <summary>
          /// Obtiene cajas consulta fondos
          /// </summary>
          /// <param name="codEmpresa"></param>
          /// <param name="cedula"></param>
          /// <param name="usuario"></param>
          /// <returns></returns>
        public ErrorDto<List<CajasFondosDto>> Cajas_Consulta_Fondos(
            int codEmpresa,
            string cedula,
            string usuario)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasFondosDto>>
            {
                Code = 0,
                Result = new List<CajasFondosDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = "exec spCajas_Consulta_Fondos @cedula, @usuario";

                response.Result = cn
                    .Query<CajasFondosDto>(sql, new { cedula, usuario })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasFondosDto>();
            }

            return response;
        }

       /// <summary>
       /// Obtiene cajas consulta CxC
       /// </summary>
       /// <param name="codEmpresa"></param>
       /// <param name="cedula"></param>
       /// <returns></returns>
        public ErrorDto<List<CajasCxcDto>> Cajas_Consulta_CxC(
            int codEmpresa,
            string cedula)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasCxcDto>>
            {
                Code = 0,
                Result = new List<CajasCxcDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = "exec spCxC_PersonasCuentas @cedula, 'A'";

                response.Result = cn
                    .Query<CajasCxcDto>(sql, new { cedula })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasCxcDto>();
            }

            return response;
        }

      
        /// <summary>
        /// Obtiene consultas servicios
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasServiciosDto>> Cajas_Consulta_Servicios(
            int codEmpresa,
            string cedula)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasServiciosDto>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<CajasServiciosDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = "exec spCajas_Consulta_Servicios @cedula";

                response.Result = cn
                    .Query<CajasServiciosDto>(sql, new { cedula })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasServiciosDto>();
            }

            return response;
        }

      
        /// <summary>
        /// Obtiene consulta cajas Saldo A Favor
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="liquidados"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasSaldoFavorDto>> Cajas_Consulta_SaldosFavor(
            int codEmpresa,
            string cedula,
            bool liquidados)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasSaldoFavorDto>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<CajasSaldoFavorDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                    SELECT
                        LINEA AS linea,
                        DOC_TIPO AS documento,
                        REGISTRO_FECHA AS fecha,
                        MONTO AS monto,
                        SALDO AS saldo,
                        'Tes. Id.: ' + CAST(DOC_TRANSAC_ID AS varchar)
                            + ' ¦ Caja .: ' + COD_CAJA
                            + '  Ap.Id.: ' + CAST(COD_APERTURA AS varchar)
                            AS referencia
                    FROM CAJAS_SALDO_FAVOR
                    WHERE cedula = @cedula
                      AND (
                            (@liq = 1 AND saldo <= 0)
                         OR (@liq = 0 AND saldo > 0)
                      )
                    ORDER BY REGISTRO_FECHA DESC";

                response.Result = cn
                    .Query<CajasSaldoFavorDto>(
                        sql,
                        new { cedula, liq = liquidados ? 1 : 0 }
                    )
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasSaldoFavorDto>();
            }

            return response;
        }

        
        /// <summary>
        /// Consulta cajas Recibos Multiples
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasReciboMultipleDto>> Cajas_Consulta_RecibosMultiples(
            int codEmpresa,
            string cedula)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasReciboMultipleDto>>
            {
                Code = 0,
                Description = "Operación realizada correctamente",
                Result = new List<CajasReciboMultipleDto>()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                    SELECT
                        CAJA_AM_ID AS recibo,
                        MONTO AS monto,
                        REGISTRO_FECHA AS fecha,
                        COD_CAJA AS caja,
                        COD_APERTURA AS apertura,
                        REGISTRO_USUARIO AS usuario
                    FROM CAJAS_AM_MAIN
                    WHERE cedula = @cedula
                    ORDER BY REGISTRO_FECHA DESC";

                response.Result = cn
                    .Query<CajasReciboMultipleDto>(sql, new { cedula })
                    .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasReciboMultipleDto>();
            }

            return response;
        }
    }


}




