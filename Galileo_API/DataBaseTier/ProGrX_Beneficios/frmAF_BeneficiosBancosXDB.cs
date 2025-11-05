using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficiosBancosXDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficiosBancosXDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<afBeneficiosBancosDataLista> BeneficiosBancosX_Obtener(int CodCliente, string filtros)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<afBeneficiosBancosDataLista>();

            response.Result = new afBeneficiosBancosDataLista();

            response.Result.Total = 0;
            try
            {
                BeneficiosBancosX_Existe(CodCliente);
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string vFiltro = "";
                AfBeneficioBancosfiltros filtro = JsonConvert.DeserializeObject<AfBeneficioBancosfiltros>(filtros) ?? new AfBeneficioBancosfiltros();


                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "select count(*) " +
                        " from afi_bene_Bancos_X X inner join Tes_Bancos B on X.id_banco = B.id_Banco";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros != null)
                    {
                        vFiltro = " where X.id_banco LIKE '%" + filtro.filtro + "%' OR B.descripcion LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select X.id_banco,B.descripcion,X.cheque,X.transferencia  
                                         from afi_bene_Bancos_X X inner join Tes_Bancos B on X.id_banco = B.id_Banco 
                                         {vFiltro} 
                                        order by B.id_banco
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.bancosX = connection.Query<afBeneficiosBancosData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneficiosBancosX_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        private void BeneficiosBancosX_Existe(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            using var connection = new SqlConnection(clienteConnString);
            {
                var query = "insert into afi_bene_Bancos_X(id_banco,cheque,transferencia) select id_banco,0,0 from Tes_Bancos  where id_Banco not in (select id_Banco from afi_bene_Bancos_X)";
                var result = connection.Execute(query);
            }
        }

        public ErrorDto<afBeneficiosBancosData> BeneficiosBancosX_Actualizar(int CodCliente, afBeneficiosBancosData data)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<afBeneficiosBancosData>();


            bool actualizado = false;
            response.Code = 0;
            try
            {
                int cheque = data.cheque ? 1 : 0;
                int transferencia = data.transferencia ? 1 : 0;

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update afi_bene_Bancos_X set cheque = {cheque}, transferencia = {transferencia} where id_banco = '{data.id_banco}' ";
                    actualizado = connection.Execute(query) > 0;
                }
            }

            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneficiosBancosX_Actualizar - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

    }
}