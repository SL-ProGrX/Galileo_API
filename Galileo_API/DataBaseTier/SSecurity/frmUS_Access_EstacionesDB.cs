using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmUsAccessEstacionesDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmUsAccessEstacionesDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<EstacionDto>> ObtenerEstacionesPorCliente(int codEmpresa)
        {
            var response = DbHelper.CreateOkResponse(new List<EstacionDto>());
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Estaciones_Cliente_Consultar]";
                    var values = new
                    {
                        CodEmpresa = codEmpresa,
                    };
                    var estaciones = connection.Query<EstacionDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                    response.Result = estaciones;

                    var procedureObtMACs1 = "[spPGX_Estacion_Loggin_MACs]";
                    var procedureObtMACs2 = "[spPGX_Estacion_MACs_Consultar]";

                    foreach (EstacionDto dt in estaciones)
                    {
                        dt.Estado = dt.Activa ? "ACTIVO" : "INACTIVO";

                        var valuesMACs1 = new
                        {
                            Cliente = codEmpresa,
                            Estacion = dt.Estacion,
                            Token = "q@$-&%1-mkE+1"
                        };

                        var valuesMACs2 = new
                        {
                            CodEmpresa = codEmpresa,
                            Estacion = dt.Estacion
                        };

                        //Obtiene la direccion MAC 1 y 2 para cada estacion:
                        List<string> macsList1 = connection.Query<string>(procedureObtMACs1, valuesMACs1, commandType: CommandType.StoredProcedure).ToList();
                        dt.lstMAC1 = macsList1;
                        dt.lstMAC2 = new List<string>(macsList1);

                        //Obtiene la direccion MAC 1 y 2 para cada estacion:
                        List<EstacionMacDto> macsList2 = connection.Query<EstacionMacDto>(procedureObtMACs2, valuesMACs2, commandType: CommandType.StoredProcedure).ToList();

                        // Filtrar y agregar a lstMAC1 aquellos elementos de macsList2 cuyo MAC_01 no se encuentre en lstMAC1
                        macsList2
                            .Where(estacionDto => !dt.lstMAC1.Contains(estacionDto.MAC_01))
                            .ToList()
                            .ForEach(estacionDto => dt.lstMAC1.Add(estacionDto.MAC_01));

                        // Filtrar y agregar a lstMAC2 aquellos elementos de macsList2 cuyo MAC_02 no se encuentre en lstMAC2
                        macsList2
                            .Where(estacionDto => !dt.lstMAC2.Contains(estacionDto.MAC_02))
                            .ToList()
                            .ForEach(estacionDto => dt.lstMAC2.Add(estacionDto.MAC_02));
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto EstacionRegistrar(EstacionGuardarDto estacionDto)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Estacion_Guardar]";
                    var values = new
                    {
                        CodEmpresa = estacionDto.CodEmpresa,
                        Estacion = estacionDto.Estacion,
                        Descripcion = estacionDto.Descripcion,
                        Activa = estacionDto.Activa,
                        Usuario = estacionDto.Usuario,
                        Modulo = estacionDto.Modulo,
                        AppEquipo = estacionDto.AppEquipo,
                        AppVersion = estacionDto.AppVersion,
                        AppIp = estacionDto.AppIP,
                        mac1 = estacionDto.MAC1,
                        mac2 = estacionDto.MAC2
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<EstacionSinVincularDto>> EstacionesSinVincularObtener(int codEmpresa)
        {
            var response = DbHelper.CreateOkResponse(new List<EstacionSinVincularDto>());
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Estacion_Loggin]";
                    var values = new
                    {
                        Cliente = codEmpresa,
                        Loggin = 2
                    };
                    var result = connection.Query<EstacionSinVincularDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();

                    // Filtrar los resultados excluyendo aquellos con Estacion nulo o vacío
                    response.Result = result.Where(e => !string.IsNullOrEmpty(e.Estacion)).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto EstacionVincular(EstacionVinculaDto estacionDto)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Estacion_Vincula]";
                    var values = new
                    {
                        Cliente = estacionDto.Cliente,
                        Estacion = estacionDto.Estacion,
                        Usuario = estacionDto.Usuario,
                        Vincula = estacionDto.Vincula
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto EstacionEliminar(EstacionEliminarDto estacionDto)
        {
            ErrorDto resp = DbHelper.CreateOkResponse();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Estacion_Eliminar]";
                    var values = new
                    {
                        CodEmpresa = estacionDto.CodEmpresa,
                        Estacion = estacionDto.Estacion
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
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
