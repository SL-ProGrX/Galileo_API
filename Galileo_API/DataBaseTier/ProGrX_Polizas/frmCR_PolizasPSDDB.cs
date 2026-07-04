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

                response.Result = cn.Query<dynamic>(
                    "spPoliza_PSD",
                    new
                    {
                        Poliza = "",              
                        Corte = fechaCorte,
                        Usuario = usuario,         
                        Movimiento = tipo
                    },
                    commandType: System.Data.CommandType.StoredProcedure,
                    commandTimeout: 600
                )
                .Select(MapPolizaPsdRow)
                .ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        private static PolizaPsdDto MapPolizaPsdRow(dynamic row)
        {
            var values = ((IDictionary<string, object?>)row).Values.ToArray();

            return new PolizaPsdDto
            {
                corte = ToNullableDateTime(ValueAt(values, 0)),
                identificacion = ToStringValue(ValueAt(values, 1)),
                nombre = ToStringValue(ValueAt(values, 2)),
                monto = ToNullableDecimal(ValueAt(values, 3)),
                fechaNacimiento = ToNullableDateTime(ValueAt(values, 4)),
                genero = ToStringValue(ValueAt(values, 5)),
                nacionalidad = ToStringValue(ValueAt(values, 6)),
                movimiento = ToStringValue(ValueAt(values, 7)),
            };
        }

        private static object? ValueAt(object?[] values, int index)
        {
            return values.Length > index ? values[index] : null;
        }

        private static string ToStringValue(object? value)
        {
            return value is null or DBNull ? string.Empty : Convert.ToString(value) ?? string.Empty;
        }

        private static decimal? ToNullableDecimal(object? value)
        {
            if (value is null or DBNull)
            {
                return null;
            }

            return Convert.ToDecimal(value);
        }

        private static DateTime? ToNullableDateTime(object? value)
        {
            if (value is null or DBNull)
            {
                return null;
            }

            return Convert.ToDateTime(value);
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




