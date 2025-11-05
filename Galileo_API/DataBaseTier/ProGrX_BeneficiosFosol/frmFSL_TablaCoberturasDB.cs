using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;


namespace PgxAPI.DataBaseTier
{
    public class frmFSL_TablaCoberturasDB
    {
        private readonly IConfiguration _config;

        public frmFSL_TablaCoberturasDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<FslTablaAplicacionDataLista> TablaAplicacion_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<FslTablaAplicacionDataLista>();

            response.Result = new FslTablaAplicacionDataLista();

            response.Result.Total = 0;
            try
            {

                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string vFiltro = "";
                FslTablaAplicacionFiltros filtro = JsonConvert.DeserializeObject<FslTablaAplicacionFiltros>(filtros);


                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "select count(*) " +
                        " from FSL_TABLAS_APLICACION";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros != null)

                    {
                        vFiltro = "WHERE TIPO = '" + filtro.tipo + "' ";
                        vFiltro += " AND (TIPO LIKE '%" + filtro.filtro + "%' OR LINEA LIKE '%" + filtro.filtro + "%')";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select TIPO,LINEA,MES_INICIO,MES_CORTE,COBERTURA
                                         from FSL_TABLAS_APLICACION 
                                         {vFiltro} 
                                        order by LINEA
                                        {paginaActual}
                                        {paginacionActual}; ";


                    response.Result.coberturas = connection.Query<FslTablaAplicacionData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "TablaAplicacion_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDto Cobertura_Guardar(int CodCliente, FslTablaAplicacionData aplicacion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                if (!ExisteCobertura(CodCliente, aplicacion.tipo, aplicacion.linea ))
                {
                    info = TablaAplicacion_Insertar(CodCliente, aplicacion);
                }
                else
                {
                    info = TablaAplicacion_Actualizar(CodCliente, aplicacion);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

        }

        public bool ExisteCobertura(int CodCliente, string tipo, int linea)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool existe = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select isnull(count(*),0) as Existe from FSL_TABLAS_APLICACION where Tipo = '{tipo}' AND linea = {linea}";
                    existe = connection.Query<int>(query).FirstOrDefault() > 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                existe = true;
            }
            return existe;
        }

        public ErrorDto TablaAplicacion_Insertar(int CodCliente, FslTablaAplicacionData aplicacion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select isnull(Max(Linea),0) + 1 as Ultimo from FSL_TABLAS_APLICACION   where Tipo = '{aplicacion.tipo}' ";
                    var codigo = connection.Query<int>(query).FirstOrDefault();

                    query = $@"insert into FSL_TABLAS_APLICACION(Tipo,Linea,Mes_Inicio,Mes_Corte,Cobertura,registra_fecha,registra_usuario) values( 
                                                                    '{aplicacion.tipo}', {codigo}, {aplicacion.mes_inicio}, {aplicacion.mes_corte}, {aplicacion.cobertura}, getdate(), '{aplicacion.registra_usuario}' )";

                    var resultado = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;


        }

        public ErrorDto TablaAplicacion_Actualizar(int CodCliente, FslTablaAplicacionData aplicacion)
        {


            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" update FSL_TABLAS_APLICACION set Mes_Inicio = {aplicacion.mes_inicio}, Mes_Corte = {aplicacion.mes_corte}, 
                                           Cobertura = {aplicacion.cobertura}  where Tipo = '{aplicacion.tipo}' and  Linea = {aplicacion.linea} ";

                    var resultado = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;


        }

        public ErrorDto TablaAplicacion_Eliminar(int CodCliente, string tipo, int linea)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" delete FSL_TABLAS_APLICACION where Tipo = '{tipo}' and Linea = {linea} ";

                    var resultado = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

        }

    }
}