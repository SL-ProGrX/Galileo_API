using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPFusionDB
    {
        private readonly IConfiguration _config;

        public frmCxPFusionDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<CxpProveedoresDataLista> Proveedores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<CxpProveedoresDataLista>
            {
                Code = 0,
                Result = new CxpProveedoresDataLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(cod_proveedor) from cxp_proveedores WHERE ESTADO = 'A' and fusion is null  ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND cod_proveedor LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT COD_PROVEEDOR, DESCRIPCION FROM CXP_PROVEEDORES
                                        WHERE ESTADO = 'A'  and fusion is null 
                                         {filtro} 
                                        ORDER BY DESCRIPCION
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Proveedores = connection.Query<CxpProveedorData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Proveedores = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto Fusion_Aplicar(int CodCliente, int proveedor, List<CxpProveedorData> proveedores)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    foreach (CxpProveedorData item in proveedores)
                    {
                        var query = $@"update cxp_proveedores set estado = 'I',fusion = Getdate()  where cod_proveedor = {item.Cod_Proveedor}";
                        connection.Execute(query);

                        query = $@"insert cxp_fusiones(cod_proveedor,cod_proveedor_fus) values ({proveedor}, {item.Cod_Proveedor}) ";
                        connection.Execute(query);
                    }
                    resp.Description = "Proveedores Fusionados correctamente";
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
