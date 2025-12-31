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

        // ================= HELPERS =================

        private SqlConnection ObtenerConexion(int codEmpresa)
        {
            string connString = new PortalDB(_config)
                .ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(connString);
        }

        private ErrorDto<T> CrearRespuesta<T>(T defaultValue = default)
        {
            return new ErrorDto<T>
            {
                Code = 0,
                Description = OperacionRealizadaCorrectamente,
                Result = defaultValue
            };
        }

        private ErrorDto<T> ErrorRespuesta<T>(T defaultValue, Exception ex)
        {
            return new ErrorDto<T>
            {
                Code = -1,
                Description = ex.Message,
                Result = defaultValue
            };
        }

        // ================= MÉTODOS BD =================

        /// <summary>
        /// Consulta aperturas
        /// </summary>
        public ErrorDto<List<CajasAperturaReporteDto>> Cajas_Aperturas_Consulta(
            int codEmpresa,
            string codCaja,
            DateTime fechaInicio,
            DateTime fechaCorte,
            string filtro)
        {
            var response = CrearRespuesta(new List<CajasAperturaReporteDto>());

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

                var sql = new StringBuilder(@"
                    SELECT
                        Cod_Apertura      AS cod_apertura,
                        Apertura_Fecha    AS apertura_fecha,
                        Apertura_Usuario  AS apertura_usuario,
                        Estado            AS estado,
                        Cierre_Fecha      AS cierre_fecha,
                        Cierre_Usuario    AS cierre_usuario,
                        Recibe_Fecha      AS recibe_fecha,
                        Recibe_Usuario    AS recibe_usuario,
                        Revisa_Fecha      AS revisa_fecha,
                        Revisa_Usuario    AS revisa_usuario
                    FROM CAJAS_APERTURAS_MAIN
                    WHERE COD_CAJA = @codCaja
                      AND Apertura_Fecha BETWEEN @fechaInicio AND @fechaCorte");

                if (filtro == "R")
                {
                    sql.Append(" AND Recibe_Fecha IS NULL");
                }
                else if (filtro == "V")
                {
                    sql.Append(" AND Revisa_Fecha IS NULL");
                }

                sql.Append(" ORDER BY Cod_Apertura DESC");

                response.Result = cn.Query<CajasAperturaReporteDto>(
                    sql.ToString(),
                    new
                    {
                        codCaja,
                        fechaInicio,
                        fechaCorte
                    }).ToList();
            }
            catch (Exception ex)
            {
                return ErrorRespuesta(new List<CajasAperturaReporteDto>(), ex);
            }

            return response;
        }

        /// <summary>
        /// Consulta accesos
        /// </summary>
        public ErrorDto<List<CajasAccesoDto>> Cajas_Accesos_Consulta(
            int codEmpresa,
            string codCaja,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            var response = CrearRespuesta(new List<CajasAccesoDto>());

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

                const string sql = @"
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

                response.Result = cn.Query<CajasAccesoDto>(
                    sql,
                    new
                    {
                        codCaja,
                        fechaInicio,
                        fechaCorte
                    }).ToList();
            }
            catch (Exception ex)
            {
                return ErrorRespuesta(new List<CajasAccesoDto>(), ex);
            }

            return response;
        }

        /// <summary>
        /// Consulta depósitos
        /// </summary>
        public ErrorDto<List<CajasDepositoDto>> Cajas_Depositos_Consulta(
            int codEmpresa,
            string codCaja,
            int codApertura)
        {
            var response = CrearRespuesta(new List<CajasDepositoDto>());

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

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
                return ErrorRespuesta(new List<CajasDepositoDto>(), ex);
            }

            return response;
        }

        /// <summary>
        /// Cierre forzado (SP original)
        /// </summary>
        public ErrorDto<bool> Cajas_Cierre_Forzado(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            var response = CrearRespuesta(false);

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

                cn.Execute(
                    "spCajas_Cierre_Forzado",
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
                return ErrorRespuesta(false, ex);
            }

            return response;
        }

        // ================= RECIBE / REVISA =================

        /// <summary>
        /// Método reutilizable para marcar recibe/revisa sin SQL dinámico inseguro
        /// </summary>
        private ErrorDto<bool> ActualizarCampoCierre(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario,
            string tipo)
        {
            var response = CrearRespuesta(false);

            // Elegimos SQL entre constantes ? sin concatenar columnas dinámicamente
            string sql = tipo switch
            {
                "RECIBE" => @"
                    UPDATE CAJAS_APERTURAS_MAIN
                    SET RECIBE_FECHA = GETDATE(),
                        RECIBE_USUARIO = @usuario
                    WHERE COD_CAJA = @codCaja
                      AND COD_APERTURA = @codApertura",
                "REVISA" => @"
                    UPDATE CAJAS_APERTURAS_MAIN
                    SET REVISA_FECHA = GETDATE(),
                        REVISA_USUARIO = @usuario
                    WHERE COD_CAJA = @codCaja
                      AND COD_APERTURA = @codApertura",
                _ => throw new ArgumentException("Tipo de cierre no válido", nameof(tipo))
            };

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

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
                return ErrorRespuesta(false, ex);
            }

            return response;
        }

        /// <summary>
        /// Cierre recibe
        /// </summary>
        public ErrorDto<bool> Cajas_Cierre_Recibe(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            return ActualizarCampoCierre(
                codEmpresa,
                codCaja,
                codApertura,
                usuario,
                "RECIBE");
        }

        /// <summary>
        /// Revisa cierre
        /// </summary>
        public ErrorDto<bool> Cajas_Cierre_Revisa(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            return ActualizarCampoCierre(
                codEmpresa,
                codCaja,
                codApertura,
                usuario,
                "REVISA");
        }

        /// <summary>
        /// Definición de cajas lista
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Definicion_Lista(int codEmpresa)
        {
            var response = CrearRespuesta(new List<DropDownListaGenericaModel>());

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

                const string sql = @"
                    SELECT 
                        COD_CAJA    AS item,
                        DESCRIPCION AS descripcion
                    FROM CAJAS_DEFINICION
                    ORDER BY COD_CAJA";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql).ToList();
            }
            catch (Exception ex)
            {
                return ErrorRespuesta(new List<DropDownListaGenericaModel>(), ex);
            }

            return response;
        }

        /// <summary>
        /// Forza cierre (otro SP)
        /// </summary>
        public ErrorDto<bool> Cajas_Cierre_Forzar(
            int codEmpresa,
            string codCaja,
            int codApertura,
            string usuario)
        {
            var response = CrearRespuesta(false);

            try
            {
                using var cn = ObtenerConexion(codEmpresa);

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
                return ErrorRespuesta(false, ex);
            }

            return response;
        }
    }
}
