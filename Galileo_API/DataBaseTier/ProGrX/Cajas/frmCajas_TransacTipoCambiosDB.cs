using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;

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

        /// <summary>
        /// Obtiene el tipo de cambio vigente de Cajas para la divisa
        /// y operación indicadas, utilizando la contabilidad de la empresa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Divisa"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Cajas_TransacTipoCambio_TipoCambio_Obtener(
            int CodEmpresa,
            string? Divisa,
            string? Tipo)
        {
            var divisa = (Divisa ?? string.Empty).Trim();
            var tipo = (Tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse<decimal>(
                    "Debe indicar una empresa v&aacute;lida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(divisa))
            {
                return DbHelper.CreateErrorResponse<decimal>(
                    "Debe seleccionar una divisa.",
                    -2);
            }

            if (tipo != "C" && tipo != "V")
            {
                return DbHelper.CreateErrorResponse<decimal>(
                    "Debe seleccionar Compra o Venta.",
                    -2);
            }

            try
            {
                var parametros = new MCntLinkDB(_config)
                    .sbgCntParametros(CodEmpresa);

                if (parametros.Code.GetValueOrDefault() != 0 ||
                    parametros.Result is null)
                {
                    return DbHelper.CreateErrorResponse<decimal>(
                        parametros.Description
                            ?? "No fue posible obtener los par&aacute;metros contables.",
                        -1);
                }

                var tipoCambio = new MCajas(_config).fxCajasTipoCambio(
                    CodEmpresa,
                    parametros.Result.gEnlace,
                    divisa,
                    tipo);

                if (tipoCambio == 0m)
                {
                    return DbHelper.CreateErrorResponse<decimal>(
                        "La consulta de Cajas no devolvi&oacute; un tipo de cambio utilizable.",
                        -1);
                }

                return DbHelper.CreateOkResponse(tipoCambio);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<decimal>(
                    ex.Message,
                    -1);
            }
        }
    }
}