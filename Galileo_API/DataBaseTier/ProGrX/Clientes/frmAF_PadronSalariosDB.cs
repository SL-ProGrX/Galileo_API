using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_PadronSalariosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;

        public frmAF_PadronSalariosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo que obtiene la lista de instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> AF_PadronSalariosInstituciones_Obtener(int CodEmpresa)
        {
            var response = new ErrorDTO<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    const string query = @"Select COD_INSTITUCION as 'item',  CONCAT('[',COD_DIVISA,']  ', DESCRIPCION) as 'descripcion'
                                             from INSTITUCIONES where ACTIVA = 1
                                             order by COD_INSTITUCION";

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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

        /// <summary>
        /// metodo que procesa el padron de los empleados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="institucion"></param>
        /// <param name="usuario"></param>
        /// <param name="padron"></param>
        /// <returns></returns>
        public ErrorDTO AF_PadronSalarios_Padron_Procesar(int CodEmpresa, string institucion, string usuario  ,List<AfPadronData> padron)
        {
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    foreach (var item in padron)
                    {
                        var query = @"exec spAFI_Padron_Registro @Cedula, @IdAlterno,@Nombre, @Institucion, @FechaIngreso,@Usuario, @Mov";
                        connection.Execute(query, new
                        {
                            Cedula = item.identificacion,
                            IdAlterno = item.id_alterna,
                            Nombre = item.nombre,
                            Institucion = institucion,
                            FechaIngreso = item.fecha_ingreso,
                            Usuario = usuario,
                            Mov = "A"
                        });
                    }  
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Metodo que procesa el salario de los empleados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="institucion"></param>
        /// <param name="usuario"></param>
        /// <param name="salario"></param>
        /// <returns></returns>
        public ErrorDTO AF_PadronSalarios_Salario_Procesar(int CodEmpresa, string institucion, string usuario,  List<AfSalarioData> salario)
        {
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    foreach (var item in salario)
                    {
                        var query = @"exec spAFI_Persona_Salarios_Add @Cedula , @Tipo , @Divisa, @Fecha ,@SalarioDevengado, @Rebajos, @SalarioNeto , @Embargo,@Usuario, @Mov";
                        connection.Execute(query , new
                        {
                            Cedula = item.identificacion,
                            Tipo = "C",
                            Divisa = item.divisa,
                            Fecha = item.fecha,
                            SalarioDevengado = item.salario_bruto,
                            Rebajos = item.rebajos,
                            SalarioNeto = item.salario_neto,
                            Embargo = item.embargos,
                            Usuario = usuario,
                            Mov = "A"
                        });
                    }  
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
