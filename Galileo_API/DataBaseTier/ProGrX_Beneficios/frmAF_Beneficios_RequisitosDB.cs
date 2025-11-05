using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_RequisitosDB
    {
        private readonly IConfiguration _config;

        public frmAF_Beneficios_RequisitosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<BeneRequisitosDataLista> AfBeneRequisitos_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            AfiRequerimientoFiltros filtro = JsonConvert.DeserializeObject<AfiRequerimientoFiltros>(filtros);
            var response = new ErrorDto<BeneRequisitosDataLista>();
            response.Result = new BeneRequisitosDataLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(COD_REQUISITO) from AFI_BENE_REQUISITOS ";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    string vFiltro = "";
                    if (filtro.filtro != null)
                    {
                        vFiltro = " where COD_REQUISITO LIKE '%" + filtro.filtro + "%' OR descripcion LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_REQUISITO, descripcion, Activo, requerido from AFI_BENE_REQUISITOS
                                         {vFiltro} 
                                        order by COD_REQUISITO
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.lista = connection.Query<BeneRequisitosData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
            }
            return response;

        }

        public ErrorDto AfBeneRequisitos_Insertar(int CodCliente, BeneRequisitosData requiisto)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valido si existe
                    var query = $@"select isnull(count(*),0) as Existe from AFI_BENE_REQUISITOS where UPPER(TRIM(COD_REQUISITO)) = '{requiisto.cod_requisito.Trim().ToUpper()}' ";
                    var existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe == 0)
                    {
                        int activo = requiisto.activo ? 1 : 0;
                        int requerido = requiisto.requerido ? 1 : 0;
                        query = $@"insert into AFI_BENE_REQUISITOS (COD_REQUISITO, descripcion, Registro_Fecha ,Activo,requerido, registro_usuario) 
                                    values ('{requiisto.cod_requisito}', '{requiisto.descripcion}', getdate(),{activo},{requerido},'{requiisto.registro_usuario}') ";
                        connection.Execute(query);
                        resp.Description = "Cat�logo de Requsitos para Beneficios Id " + requiisto.cod_requisito;
                    }
                    else
                    {
                        resp = AfBeneRequisitos_Actualizar(CodCliente, requiisto);
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto AfBeneRequisitos_Actualizar(int CodCliente, BeneRequisitosData requiisto)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    int activo = requiisto.activo ? 1 : 0;
                    int requerido = requiisto.requerido ? 1 : 0;
                    var query = $@"update AFI_BENE_REQUISITOS set 
                            descripcion = '{requiisto.descripcion}', 
                            Activo = {activo},
                            Requerido = {requerido},
                            Modifica_Fecha = getdate() ,
                            Modifica_Usuario = '{requiisto.registro_usuario}' where COD_REQUISITO = '{requiisto.cod_requisito}' ";
                    connection.Execute(query);
                    resp.Description = "Cat�logo de Requsitos para Beneficios Id " + requiisto.cod_requisito;

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto AfBeneRequisitos_Eliminar(int CodCliente, string cod_requisito)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from AFI_BENE_REQUISITOS where COD_REQUISITO = '{cod_requisito}' ";
                    connection.Execute(query);
                    resp.Description = "Cat�logo de Requsitos para Beneficios Id " + cod_requisito;
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