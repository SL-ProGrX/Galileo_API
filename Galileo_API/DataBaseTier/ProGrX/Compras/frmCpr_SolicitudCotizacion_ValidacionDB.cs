using PgxAPI.Models.CPR;
using Dapper;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_SolicitudCotizacion_ValidacionDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmCpr_SolicitudCotizacion_ValidacionDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        public ErrorDTO<CprSolicitudCotizacionPrvBsLista> CprValidarCotizacionBs_Obtener(int CodEmpresa, int? cpr_id, int? cod_unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<CprSolicitudCotizacionPrvBsLista>();
            response.Result = new CprSolicitudCotizacionPrvBsLista();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    //valida si existe
                    var query = $@"exec [spCPR_ValidarCotizacion_Consultar] '{cpr_id}',{cod_unidad}";
                    response.Result.cotizaciones = connection.Query<CprSolicitudCotizacionPrvBs>(query).ToList();
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


        public ErrorDTO CprValidarContizacionBs_Guardar(int CodEmpresa, CprSolicitusCotizacionGuardar datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    string xmlOutput = "";

                    foreach (var cotizacion in datos.listacotizacion)
                    {


                        cotizacion.proveedor_codigo = datos.proveedor_codigo;
                        cotizacion.no_cotizacion = datos.no_cotizacion;
                        xmlOutput = _AuxiliarDB.fxConvertModelToXml<CprSolicitudCotizacionPrvBs>(cotizacion);

                        var values = new
                        {
                            datos = xmlOutput
                        };

                        var query = $@"exec [spCPR_ValidarCotizacion_Guardar] '{xmlOutput}'";
                        info.Code = connection.Execute(query);

                    }

                }


            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }


        public ErrorDTO CprValidacionCotizacionBs_Eliminar(int CodEmpresa, int cpr_id, string codigo, string cod_producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO info = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE CPR_SOLICITUD_PROV_COTIZA_LINEAS SET SELECCIONADO = 0 WHERE CODIGO = '{codigo}' AND COD_PRODUCTO =  '{cod_producto}' 
                        AND id_cotizacion_linea =  {cpr_id}";



                    var result = connection.Execute(query);


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