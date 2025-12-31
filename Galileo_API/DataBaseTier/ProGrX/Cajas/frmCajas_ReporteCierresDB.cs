using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasReporteCierresDb
    {
        private readonly IConfiguration _config;
        private const string OperacionRealizadaCorrectamente = "Operación realizada correctamente";

        public FrmCajasReporteCierresDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Consulta aperturas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasAperturaReporteDto>> Cajas_Aperturas_Consulta(int codEmpresa, string codCaja, DateTime fechaInicio, DateTime fechaCorte, string filtro)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasAperturaReporteDto>>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = new()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                var sql = new StringBuilder(@"
                            SELECT
                                Cod_Apertura     AS cod_apertura,
                                Apertura_Fecha  AS apertura_fecha,
                                Apertura_Usuario AS apertura_usuario,
                                Estado           AS estado,
                                Cierre_Fecha     AS cierre_fecha,
                                Cierre_Usuario   AS cierre_usuario,
                                Recibe_Fecha     AS recibe_fecha,
                                Recibe_Usuario   AS recibe_usuario,
                                Revisa_Fecha     AS revisa_fecha,
                                Revisa_Usuario   AS revisa_usuario
                            FROM CAJAS_APERTURAS_MAIN
                            WHERE COD_CAJA = @codCaja
                              AND Apertura_Fecha BETWEEN @fechaInicio AND @fechaCorte
                            ");

                if (filtro == "R")
                    sql.Append(" AND Recibe_Fecha IS NULL");
                else if (filtro == "V")
                    sql.Append(" AND Revisa_Fecha IS NULL");

                sql.Append(" ORDER BY Cod_Apertura DESC");

                response.Result = cn.Query<CajasAperturaReporteDto>(sql.ToString(), new
                {
                    codCaja,
                    fechaInicio,
                    fechaCorte
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta Accesos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaCorte"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasAccesoDto>> Cajas_Accesos_Consulta(int codEmpresa, string codCaja, DateTime fechaInicio, DateTime fechaCorte)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasAccesoDto>>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = new()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                        SELECT
                            FechaIngreso AS fecha,
                            Caja         AS caja,
                            Apertura     AS apertura,
                            Usuario      AS usuario,
                            SifVersion   AS version
                        FROM CAJAS_BITACORA_INGRESO
                        WHERE Caja = @codCaja
                          AND FechaIngreso BETWEEN @fechaInicio AND @fechaCorte
                        ORDER BY FechaIngreso DESC";

                response.Result = cn.Query<CajasAccesoDto>(sql, new
                {
                    codCaja,
                    fechaInicio,
                    fechaCorte
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta Depositos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasDepositoDto>> Cajas_Depositos_Consulta(int codEmpresa,string codCaja,int codApertura)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<List<CajasDepositoDto>>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = new()
            };

            try
            {
                using var cn = new SqlConnection(connString);

                response.Result = cn.Query<CajasDepositoDto>(
                    "spCajas_CierreDepositoDivisa",
                    new
                    {
                        Caja = codCaja,        
                        Apertura = codApertura, 
                        Divisa = "COL"           
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<CajasDepositoDto>();
            }

            return response;
        }

        /// <summary>
        /// Cierre forzado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_Cierre_Forzado(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = false
            };

            try
            {
                using var cn = new SqlConnection(connString);

                cn.Execute(
                    "spCajas_Cierre_Forzado",
                    new { codCaja, codApertura, usuario },
                    commandType: CommandType.StoredProcedure);

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Cierre recibe
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_Cierre_Recibe(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = false
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                            UPDATE CAJAS_APERTURAS_MAIN
                            SET RECIBE_FECHA = GETDATE(),
                                RECIBE_USUARIO = @usuario
                            WHERE COD_CAJA = @codCaja
                              AND COD_APERTURA = @codApertura";

                cn.Execute(sql, new
                {
                    codCaja,
                    codApertura,
                    usuario
                });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

        /// <summary>
        /// Revisa cierre
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_Cierre_Revisa(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = false
            };

            try
            {
                using var cn = new SqlConnection(connString);

                string sql = @"
                    UPDATE CAJAS_APERTURAS_MAIN
                    SET REVISA_FECHA = GETDATE(),
                        REVISA_USUARIO = @usuario
                    WHERE COD_CAJA = @codCaja
                      AND COD_APERTURA = @codApertura";

                cn.Execute(sql, new
                {
                    codCaja,
                    codApertura,
                    usuario
                });

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }



        /// <summary>
        /// Definicion de cajas lista
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Definicion_Lista(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };

            try
            {
                using var cn = new SqlConnection(
                    new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa)
                );

                string sql = @"
            SELECT 
                COD_CAJA   AS item,
                DESCRIPCION AS descripcion
            FROM CAJAS_DEFINICION
            ORDER BY COD_CAJA
        ";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Forza cierre
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="codApertura"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<bool> Cajas_Cierre_Forzar(int codEmpresa,string codCaja,int codApertura,string usuario)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "OK",
                Result = false
            };

            try
            {
                using var cn = new SqlConnection(
                    new PortalDB(_config)
                        .ObtenerDbConnStringEmpresa(codEmpresa)
                );

                cn.Execute(
                    "spCAJAS_Cierre_Forzar",
                    new
                    {
                        codCaja,
                        codApertura,
                        usuario
                    },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = true;
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

