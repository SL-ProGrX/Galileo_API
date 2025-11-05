using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class VendedorDB
    {
        private readonly IConfiguration _config;

        public VendedorDB(IConfiguration config)
        {
            _config = config;
        }

        public List<Vendedor> Vendedor_ObtenerTodos()
        {
            List<Vendedor> servs = new List<Vendedor>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Vendedor_Obtener]";

                    servs = connection.Query<Vendedor>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    foreach (Vendedor dt in servs)
                    {
                        dt.Estado = dt.Activo == 1 ? "ACTIVO" : "INACTIVO";

                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return servs;
        }

        public ErrorVendedorDTO Vendedor_Insertar(Vendedor request)
        {
            ErrorVendedorDTO resp = new ErrorVendedorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Vendedor_Insertar]";
                    var values = new
                    {
                        Cod_Vendedor = request.Cod_Vendedor,
                        Identificacion = request.Identificacion,
                        Nombre = request.Nombre,
                        Activo = request.Activo,
                        Comision_Tipo = request.Comision_Tipo,
                        Comision_Cliente = request.Comision_Cliente,
                        Cuenta_Cliente = request.Cuenta_Cliente,
                        Registro_Usuario = request.Registro_Usuario,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorVendedorDTO Vendedor_Eliminar(Vendedor request)
        {
            ErrorVendedorDTO resp = new ErrorVendedorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Vendedor_Eliminar]";
                    var values = new
                    {
                        Cod_Vendedor = request.Cod_Vendedor,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorVendedorDTO Vendedor_Actualizar(Vendedor request)
        {
            ErrorVendedorDTO resp = new ErrorVendedorDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Vendedor_Editar]";
                    var values = new
                    {
                        Cod_Vendedor = request.Cod_Vendedor,
                        Identificacion = request.Identificacion,
                        Nombre = request.Nombre,
                        Activo = request.Activo,
                        Comision_Tipo = request.Comision_Tipo,
                        Comision_Cliente = request.Comision_Cliente,
                        Cuenta_Cliente = request.Cuenta_Cliente,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}
