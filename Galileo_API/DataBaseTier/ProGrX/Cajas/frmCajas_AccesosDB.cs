using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasAccesosDb
    {
        private readonly IConfiguration _config;

        public FrmCajasAccesosDb(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene cajas disponibles
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Apertura_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = ObtenerCajasDisponibles(connection, usuario);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        private List<DropDownListaGenericaModel> ObtenerCajasDisponibles(SqlConnection connection, string usuario)
        {
            var query = "spCajas_CierreCajasDisponibles";

            return connection.Query(query, new { Usuario = usuario }, commandType: CommandType.StoredProcedure)
                .Select(row => new DropDownListaGenericaModel
                {
                    item = row.IdX,
                    descripcion = row.ItmX
                }).ToList();
        }

        /// <summary>
        /// Abre caja para el usuario
        /// </summary>
        public ErrorDto Cajas_AbreCaja(int codEmpresa, string codCaja, string usuario, string appVersion, string clave)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var claveCifrada = MCajas.FxStringCifrado(clave);

                var sqlValidar = @"
                            SELECT COUNT(*) 
                            FROM cajas_usuarios 
                            WHERE usuario = @Usuario 
                              AND contrasena = @ClaveCifrada
                              AND cod_caja = @CodCaja";

                int aceptado = connection.ExecuteScalar<int>(sqlValidar, new
                {
                    Usuario = usuario,
                    ClaveCifrada = claveCifrada,
                    CodCaja = codCaja
                });

                if (aceptado <= 0)
                {
                    response.Code = -1;
                    response.Description = "No se encuentra autorizado para utilizar esta caja.";
                    return response;
                }

                var result = connection.QueryFirstOrDefault<CajasAperturaDto>(
                    "spCajas_AbreCaja",
                    new
                    {
                        Caja = codCaja,
                        Usuario = usuario,
                    },
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    response.Code = -1;
                    response.Description = "No existe Apertura Disponible para esta caja o se encuentra en uso por otro usuario.";
                    return response;
                }
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