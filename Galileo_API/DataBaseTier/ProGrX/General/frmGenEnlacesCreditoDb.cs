using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmGenEnlacesCreditoDb
    {
        private readonly IConfiguration _config;

        public FrmGenEnlacesCreditoDb(IConfiguration config)
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
                var queryParams = new EnlacesCreditoQueryParams
                {
                    Filtro = filtro,
                    Pagina = pagina,
                    Paginacion = paginacion
                };
                using var connection = new SqlConnection(clienteConnString);
                ConsultarEnlacesCredito(connection, ref datos, ref query, ref queryParams);
            }
            catch (Exception ex)
            {
                datos.Code = -1;
                datos.Description = ex.Message;
            }
            return datos;
        }

        private sealed class EnlacesCreditoQueryParams
        {
            public string? Filtro { get; set; }
            public int? Pagina { get; set; }
            public int? Paginacion { get; set; }
            public string PaginaActual { get; set; } = string.Empty;
            public string PaginacionActual { get; set; } = string.Empty;
        }

        private static void ConsultarEnlacesCredito(SqlConnection connection, ref ErrorDto<EnlaceCreditoLista> datos, ref string query, ref EnlacesCreditoQueryParams queryParams)
        {
            //Busco Total
            query = $@"SELECT count(I.cod_institucion) FROM instituciones I 
                         INNER JOIN PV_PARINSTITUCIONES P ON I.cod_institucion = P.cod_institucion";
            var totalResult = connection.Query<int>(query);
            if (datos.Result != null)
            {
                datos.Result.total = totalResult?.FirstOrDefault() ?? 0;
            }

            if (queryParams.Filtro != null)
            {
                queryParams.Filtro = " WHERE I.descripcion LIKE '%" + queryParams.Filtro + "%' " +
                            "OR I.cod_institucion LIKE '%" + queryParams.Filtro + "%' " +
                            "OR P.cod_credito LIKE '%" + queryParams.Filtro + "%'";
            }

            if (queryParams.Pagina != null)
            {
                queryParams.PaginaActual = " OFFSET " + queryParams.Pagina + " ROWS ";
                queryParams.PaginacionActual = " FETCH NEXT " + queryParams.Paginacion + " ROWS ONLY ";
            }

            query = $@"SELECT I.cod_institucion as codInstitucion,I.descripcion,P.cod_credito as codCredito 
                            FROM instituciones I INNER JOIN PV_PARINSTITUCIONES P ON I.cod_institucion = P.cod_institucion
                                {queryParams.Filtro}
                            ORDER BY I.cod_institucion
                                {queryParams.PaginaActual}
                                {queryParams.PaginacionActual} ";

            if (datos.Result != null)
            {
                datos.Result.lista = connection.Query<EnlaceCreditoDto>(query).ToList();
            }
        }

        public ErrorDto<List<CodigoCreditoDto>> CodigoCredito_ObtenerTodos(int codEmpresa, string cod_institucion)
        {

            PgxClienteDto pgxClienteDto;
            SeguridadPortalDb seguridadPortal = new SeguridadPortalDb(_config);

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
                using var connectionCore = new SqlConnection(connectionString);

                var query = "SELECT CODIGO,DESCRIPCION FROM CATALOGO WHERE COD_INSTITUCION = @cod_institucion";

                var parameters = new DynamicParameters();
                parameters.Add("cod_institucion", cod_institucion, DbType.String);

                resp.Result = connectionCore.Query<CodigoCreditoDto>(query, parameters).ToList();
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
            PgxClienteDto pgxClienteDto;
            SeguridadPortalDb seguridadPortal = new SeguridadPortalDb(_config);

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
                using var connectionCore = new SqlConnection(connectionString);
                var query = "Update PV_PARINSTITUCIONES set cod_credito = @cod_credito where cod_institucion = @cod_institucion";

                var parameters = new DynamicParameters();
                parameters.Add("cod_credito", request.CodCredito, DbType.String);
                parameters.Add("cod_institucion", request.CodInstitucion, DbType.Int32);

                resp.Code = connectionCore.ExecuteAsync(query, parameters).Result;
                resp.Description = "Ok";
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


