using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCajasTransacTipoCambioDB
    {
        private readonly IConfiguration _config;


        public FrmCajasTransacTipoCambioDB(IConfiguration config)
        {
            _config = config;

        }
        /// <summary>
        /// Consulta listado de tipos de documento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Caja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TransacTipoCambio_TipoDocumento_Obtener(int CodEmpresa, string Caja = "")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"select rtrim(Doc.Tipo_Documento) as 'item',rtrim(Doc.Descripcion) as 'descripcion' from Cajas_Documentos Cj 
                                inner join SIF_Documentos Doc on Cj.Tipo_Documento = Doc.Tipo_Documento
                                Where Cj.Cod_Caja = @Caja";
                result.Result = connection.Query<DropDownListaGenericaModel>(query, new { Caja }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        ///  Consulta listado de divisas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_TransacTipoCambio_Divisas_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"select COD_DIVISA as 'item',DESCRIPCION as 'descripcion'  
                                from vSys_Divisas Where DIVISA_LOCAL = 0";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}