using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GEN;
using PgxAPI.Models.INV;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier
{
    public class frmGenEnlacesCreditoDB
    {
        private readonly IConfiguration _config;

        public frmGenEnlacesCreditoDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<EnlaceCreditoLista> EnlacesCreditoConsultar(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var datos = new ErrorDto<EnlaceCreditoLista>();
            datos.Result = new EnlaceCreditoLista();
            datos.Result.total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = $@"SELECT count(I.cod_institucion) FROM instituciones I 
                                 INNER JOIN PV_PARINSTITUCIONES P ON I.cod_institucion = P.cod_institucion";
                    datos.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE I.descripcion LIKE '%" + filtro + "%' " +
                                    "OR I.cod_institucion LIKE '%" + filtro + "%' " +
                                    "OR P.cod_credito LIKE '%" + filtro + "%'";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT I.cod_institucion as codInstitucion,I.descripcion,P.cod_credito as codCredito 
                                    FROM instituciones I INNER JOIN PV_PARINSTITUCIONES P ON I.cod_institucion = P.cod_institucion
                                        {filtro}
                                    ORDER BY I.cod_institucion
                                        {paginaActual}
                                        {paginacionActual} ";


                    datos.Result.lista = connection.Query<EnlaceCreditoDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                datos.Code = -1;
                datos.Description = ex.Message;
                datos.Result = null;
            }

            return datos;


        }


        public ErrorDto<List<CodigoCreditoDto>> CodigoCredito_ObtenerTodos(int codEmpresa, string cod_institucion)
        {

            PgxClienteDTO pgxClienteDto;
            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);

            pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(codEmpresa);
            string nombreServidorCore = pgxClienteDto.PGX_CORE_SERVER;
            string nombreBDCore = pgxClienteDto.PGX_CORE_DB;
            string userId = pgxClienteDto.PGX_CORE_USER;
            string pass = pgxClienteDto.PGX_CORE_KEY;

            string connectionString = $"Data Source={nombreServidorCore};" +
                                  $"Initial Catalog={nombreBDCore};" +
                                  $"Integrated Security=False;User Id={userId};Password={pass};";

            var resp = new ErrorDto<List<CodigoCreditoDto>>();

            try
            {
                using (var connectionCore = new SqlConnection(connectionString))
                {

                    var query = "SELECT CODIGO,DESCRIPCION FROM CATALOGO WHERE COD_INSTITUCION = @cod_institucion";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_institucion", cod_institucion, DbType.String);

                    resp.Result = connectionCore.Query<CodigoCreditoDto>(query, parameters).ToList();

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }


        public ErrorDto EnlaceCredito_Actualizar(EnlaceCreditoDto request)
        {
            PgxClienteDTO pgxClienteDto;
            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);

            pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(request.CodEmpresa);
            string nombreServidorCore = pgxClienteDto.PGX_CORE_SERVER;
            string nombreBDCore = pgxClienteDto.PGX_CORE_DB;
            string userId = pgxClienteDto.PGX_CORE_USER;
            string pass = pgxClienteDto.PGX_CORE_KEY;

            string connectionString = $"Data Source={nombreServidorCore};" +
                                  $"Initial Catalog={nombreBDCore};" +
                                  $"Integrated Security=False;User Id={userId};Password={pass};";

            ErrorDto resp = new ErrorDto();


            try
            {
                using (var connectionCore = new SqlConnection(connectionString))
                {
                    var query = "Update PV_PARINSTITUCIONES set cod_credito = @cod_credito where cod_institucion = @cod_institucion";

                    var parameters = new DynamicParameters();
                    parameters.Add("cod_credito", request.CodCredito, DbType.String);
                    parameters.Add("cod_institucion", request.CodInstitucion, DbType.Int32);

                    resp.Code = connectionCore.ExecuteAsync(query, parameters).Result;
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
