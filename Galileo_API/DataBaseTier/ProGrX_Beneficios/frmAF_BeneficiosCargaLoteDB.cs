using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficiosCargaLoteDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficiosCargaLoteDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO Beneficio_Lote_Carga_Insertar(int CodEmpresa, string beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<BeneficioExcelData> beneficioExcelDatas = new List<BeneficioExcelData>();
            beneficioExcelDatas = JsonConvert.DeserializeObject<List<BeneficioExcelData>>(beneficio);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    foreach (BeneficioExcelData item in beneficioExcelDatas)
                    {

                        if (item.cod_beneficio == "")
                        {
                            resp.Code = -1;
                            resp.Description = "El campo Codigo no puede estar vacio";
                            return resp;
                        }

                        if (item.cedula == null)
                        {
                            resp.Code = -1;
                            resp.Description = "El campo Cedula no puede estar vacio";
                            return resp;
                        }

                        var procedure = "[spBeneficio_W_Lote_Carga]";
                        var values = new
                        {
                            Codigo = item.cod_beneficio,
                            Cedula = item.cedula,
                            Nombre = item.nombre,
                            Monto = item.monto,
                            Usuario = item.usuario,
                            Beneficiario_Id = item.beneficiario_id,
                            Beneficiario_Nombre = item.beneficiario_nombre,
                            Beneficiario_IBAN = item.beneficiario_iban,
                            Inicializa = item.inicializa
                        };

                        connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);

                    }
                    resp.Description = "Lote cargado exitosamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO<List<AfiBeneCargaLoteData>> Beneficio_Lote_Revisa_Obtener(int CodEmpresa, string cod_beneficio, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<AfiBeneCargaLoteData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spBeneficio_Lote_Revisa]";
                    var values = new
                    {
                        Codigo = cod_beneficio,
                        usuario = usuario,
                    };

                    response.Result = connection.Query<AfiBeneCargaLoteData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
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

        public ErrorDTO Beneficio_Lote_Procesa(int CodEmpresa, string cod_beneficio, string usuario, string Formato)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spBeneficio_Lote_Procesa]";
                    var values = new
                    {
                        Codigo = cod_beneficio,
                        usuario = usuario,
                        Formato = Formato
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Lote procesado exitosamente";
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