using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDivisasDCcB
    {
        private readonly PortalDB _portalDB;

        public FrmCntXDivisasDCcB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de divisas disponibles para una empresa específica, excluyendo la divisa local.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DivisaDto>> ObtenerDivisas(int codEmpresa)
        {
            var response = new ErrorDto<List<DivisaDto>>();

            try
            {
                using var cn = new SqlConnection( _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                     SELECT
                         RTRIM(cod_divisa)   AS cod_divisa,
                         RTRIM(descripcion) AS descripcion
                     FROM CntX_Divisas
                     WHERE cod_contabilidad = 2
                       AND divisa_local = 0
                ";

                response.Result = cn.Query<DivisaDto>(
                    sql,
                    new { codEmpresa }
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
        /// Obtiene los tipos de cambio registrados para una divisa específica, empresa, año y mes. Devuelve un máximo de 50 registros ordenados por fecha de corte descendente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<List<TipoCambioDto>> ObtenerTiposCambio( int codEmpresa,int anio,int mes,string codDivisa)
        {
            var response = new ErrorDto<List<TipoCambioDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                var sql = @"
                    SELECT TOP 50
                        ID_Cambio AS id_cambio,
                        TC_Compra AS tc_compra,
                        TC_Venta  AS tc_venta,
                        Inicio,
                        Corte
                    FROM CntX_Divisas_Tipo_Cambio
                    WHERE cod_divisa = @codDivisa
                      AND cod_contabilidad = 2
                      AND DATEPART(MONTH, Corte) = @mes
                      AND DATEPART(YEAR, Corte) = @anio
                    ORDER BY Corte DESC
                ";

                response.Result = cn.Query<TipoCambioDto>(
                    sql,
                    new
                    {
                        codDivisa,
                        codEmpresa,
                        mes,
                        anio
                    }
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
        /// Procesa diferencial cmabiario para divisas foraneas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <param name="codDivisa"></param>
        /// <param name="tcCompra"></param>
        /// <param name="tcVenta"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Procesar(int codEmpresa,int anio,int mes,string codDivisa,decimal? tcCompra,
            decimal? tcVenta,string usuario)
        {
            var response = new ErrorDto();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa)
                );

                cn.Execute(
                    "spCntX_DiferencialCambiario",
                    new
                    {
                        CodigoConta = codEmpresa,
                        Anio = anio,
                        Mes = mes,
                        Divisa = codDivisa,
                        TcCompra = tcCompra,
                        TcVenta = tcVenta,
                        Usuario = usuario
                    },
                    commandType: CommandType.StoredProcedure
                );
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
