using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SIF;

namespace PgxAPI.DataBaseTier
{
    public class frmSIF_EstadoCuentaTextoCtaCorDB
    {
        private readonly IConfiguration _config;

        public frmSIF_EstadoCuentaTextoCtaCorDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<Sif_EmpresaDTO> NotasEstados_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO<Sif_EmpresaDTO> result = new ErrorDTO<Sif_EmpresaDTO>();
            result.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT ID_EMPRESA, EC_Nota01, EC_Nota02 FROM sif_empresa";

                    result.Result = connection.Query<Sif_EmpresaDTO>(query).FirstOrDefault();
                    if (result.Result == null)
                    {
                        result.Code = -2;
                        result.Description = "No se encontraron datos";
                    }

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        public ErrorDTO NotasEstados_Insertar(int CodCliente, Sif_EmpresaDTO notas)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE sif_empresa SET 
                                ec_nota01 = '{notas.ec_nota01}'
                                ,ec_nota02 =  '{notas.ec_nota02}'
                                WHERE id_empresa = {notas.id_empresa}";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El código de beneficio ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }








    }
}