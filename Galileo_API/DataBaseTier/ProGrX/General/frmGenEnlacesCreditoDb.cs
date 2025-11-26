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
            var datos = new ErrorDto<EnlaceCreditoLista>
            {
                Result = new EnlaceCreditoLista { total = 0 }
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                ConsultarEnlacesCredito(connection, ref datos, pagina, paginacion, filtro);
            }
            catch (Exception ex)
            {
                datos.Code = -1;
                datos.Description = ex.Message;
            }
            return datos;
        }

        private static void ConsultarEnlacesCredito(
            SqlConnection connection,
            ref ErrorDto<EnlaceCreditoLista> datos,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            // Normalizamos el filtro a NULL o "%valor%"
            string? filtroParam = string.IsNullOrWhiteSpace(filtro)
                ? null
                : $"%{filtro}%";

            // 1) Total de registros (query fija, sin SQL dinámico)
            const string countQuery = @"
                SELECT COUNT(I.cod_institucion)
                FROM instituciones I
                INNER JOIN PV_PARINSTITUCIONES P
                    ON I.cod_institucion = P.cod_institucion
                WHERE (@Filtro IS NULL
                    OR I.descripcion     LIKE @Filtro
                    OR I.cod_institucion LIKE @Filtro
                    OR P.cod_credito     LIKE @Filtro);";

            var totalResult = connection.ExecuteScalar<int>(
                countQuery,
                new { Filtro = filtroParam });

            if (datos.Result != null)
            {
                datos.Result.total = totalResult;
            }

            // 2) Lista (con o sin paginación) — queries fijas
            if (pagina.HasValue && paginacion.HasValue)
            {
                const string selectPagedQuery = @"
                    SELECT 
                        I.cod_institucion as codInstitucion,
                        I.descripcion,
                        P.cod_credito     as codCredito
                    FROM instituciones I
                    INNER JOIN PV_PARINSTITUCIONES P
                        ON I.cod_institucion = P.cod_institucion
                    WHERE (@Filtro IS NULL
                        OR I.descripcion     LIKE @Filtro
                        OR I.cod_institucion LIKE @Filtro
                        OR P.cod_credito     LIKE @Filtro)
                    ORDER BY I.cod_institucion
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                var selectParams = new
                {
                    Filtro = filtroParam,
                    Offset = pagina.Value,
                    PageSize = paginacion.Value
                };

                if (datos.Result != null)
                {
                    datos.Result.lista = connection
                        .Query<EnlaceCreditoDto>(selectPagedQuery, selectParams)
                        .ToList();
                }
            }
            else
            {
                const string selectAllQuery = @"
                    SELECT 
                        I.cod_institucion as codInstitucion,
                        I.descripcion,
                        P.cod_credito     as codCredito
                    FROM instituciones I
                    INNER JOIN PV_PARINSTITUCIONES P
                        ON I.cod_institucion = P.cod_institucion
                    WHERE (@Filtro IS NULL
                        OR I.descripcion     LIKE @Filtro
                        OR I.cod_institucion LIKE @Filtro
                        OR P.cod_credito     LIKE @Filtro)
                    ORDER BY I.cod_institucion;";

                var selectParams = new
                {
                    Filtro = filtroParam
                };

                if (datos.Result != null)
                {
                    datos.Result.lista = connection
                        .Query<EnlaceCreditoDto>(selectAllQuery, selectParams)
                        .ToList();
                }
            }
        }

        public ErrorDto<List<CodigoCreditoDto>> CodigoCredito_ObtenerTodos(int codEmpresa, string cod_institucion)
        {
            PgxClienteDto pgxClienteDto;
            SeguridadPortalDb seguridadPortal = new SeguridadPortalDb(_config);

            pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(codEmpresa);
            string nombreServidorCore = pgxClienteDto.PGX_CORE_SERVER;
            string nombreBDCore       = pgxClienteDto.PGX_CORE_DB;
            string userId             = pgxClienteDto.PGX_CORE_USER;
            string pass               = pgxClienteDto.PGX_CORE_KEY;

            string connectionString = $"Data Source={nombreServidorCore};" +
                                      $"Initial Catalog={nombreBDCore};" +
                                      $"Integrated Security=False;User Id={userId};Password={pass};";

            var resp = new ErrorDto<List<CodigoCreditoDto>>();

            try
            {
                using var connectionCore = new SqlConnection(connectionString);

                const string query = "SELECT CODIGO, DESCRIPCION FROM CATALOGO WHERE COD_INSTITUCION = @cod_institucion";

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
            string nombreBDCore       = pgxClienteDto.PGX_CORE_DB;
            string userId             = pgxClienteDto.PGX_CORE_USER;
            string pass               = pgxClienteDto.PGX_CORE_KEY;

            string connectionString = $"Data Source={nombreServidorCore};" +
                                      $"Initial Catalog={nombreBDCore};" +
                                      $"Integrated Security=False;User Id={userId};Password={pass};";

            ErrorDto resp = new ErrorDto();

            try
            {
                using var connectionCore = new SqlConnection(connectionString);
                const string query = @"
                    UPDATE PV_PARINSTITUCIONES 
                    SET cod_credito = @cod_credito 
                    WHERE cod_institucion = @cod_institucion";

                var parameters = new DynamicParameters();
                parameters.Add("cod_credito",     request.CodCredito,    DbType.String);
                parameters.Add("cod_institucion", request.CodInstitucion, DbType.Int32);

                // Puedes usar Execute en lugar de ExecuteAsync().Result para evitar bloqueo
                resp.Code = connectionCore.Execute(query, parameters);
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