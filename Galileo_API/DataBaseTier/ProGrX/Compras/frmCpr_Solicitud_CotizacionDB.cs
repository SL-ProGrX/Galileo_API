using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;


namespace PgxAPI.DataBaseTier
{
    public class frmCpr_Solicitud_CotizacionDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly frmCpr_SolicitudDB solicitudDB;

        public frmCpr_Solicitud_CotizacionDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
            solicitudDB = new frmCpr_SolicitudDB(config);
        }


        public ErrorDTO CprSolicitudContizacionBs_Guardar(int CodEmpresa, CprSolicitusCotizacionGuardar datos)
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
                        if (cotizacion.sel == true)
                        {
                            cotizacion.seleccionado = 1;
                        }
                        else
                        {
                            cotizacion.seleccionado = 0;
                        }

                        cotizacion.proveedor_codigo = datos.proveedor_codigo;
                        cotizacion.cotiza_numero = datos.cotiza_numero;
                        cotizacion.plazo = datos.plazo;
                        cotizacion.garantia = datos.garantia;
                        cotizacion.tipo_cambio = datos.tipo_cambio;
                        xmlOutput = _AuxiliarDB.fxConvertModelToXml<CprSolicitudCotizacionPrvBs>(cotizacion);

                        var values = new
                        {
                            datos = xmlOutput
                        };

                  
                        var idCotizacionParam = new { ID_COTIZACION_OUTPUT = 0 };

                        var query = $@"
                                    DECLARE @ID_COTIZACION_OUTPUT INT;
                                    EXEC [spCPR_SolicitudCotizacion_Guardar] 
                                        '{xmlOutput}', 
                                        @ID_COTIZACION_OUTPUT OUTPUT;
                                    SELECT @ID_COTIZACION_OUTPUT AS ID_COTIZACION";

                     
                        var result = connection.QuerySingle<int>(query, values);

                       
                        int idCotizacion = result;

                        //busco tipo de solicitud y monto 
                        query = $"select * from CPR_SOLICITUD where CPR_ID = {cotizacion.cpr_id}";
                        var solicitud = connection.QueryFirstOrDefault<CprSolicitudDTO>(query);

                        if (solicitud.tipo_orden == solicitudDB.CprSolicitud_TipoExcepcion(CodEmpresa).Description
                            || solicitud.tipo_orden == solicitudDB.CprSolicitud_TipoExcepcionGM(CodEmpresa).Description)
                        {
                            query = $@"UPDATE CPR_SOLICITUD_PROV_COTIZA 
                                                SET ESTADO = 'V' 
                                        WHERE CPR_ID = {cotizacion.cpr_id} 
                                            AND PROVEEDOR_CODIGO = {cotizacion.proveedor_codigo} AND ID_COTIZACION = {idCotizacion}";
                            connection.Execute(query);

                            query = $@"UPDATE  CPR_SOLICITUD_PROV_COTIZA_LINEAS SET SELECCIONADO = 1 WHERE ID_COTIZACION = {idCotizacion} ";
                            connection.Execute(query);

                            //elimino lineas
                            var queryDelete = $@"DELETE FROM CPR_SOLICITUD_PROV_BS WHERE CPR_ID = {solicitud.cpr_id} AND PROVEEDOR_CODIGO = {cotizacion.proveedor_codigo}";
                            connection.Execute(queryDelete);

                            //detalle
                                var queryDetalle = $@"INSERT INTO CPR_SOLICITUD_PROV_BS (
                                                        CPR_ID, 
                                                        COD_PRODUCTO, 
                                                        PROVEEDOR_CODIGO, 
                                                        CODIGO, 
                                                        MONTO, 
                                                        CANTIDAD, 
                                                        TOTAL ,  
                                                        IVA_PORC,
                                                        IVA_MONTO , 
                                                        DESC_PORC, 
                                                        DESC_MONTO, 
                                                        registro_fecha, 
                                                        registro_usuario, 
                                                        ESTADO, 
                                                        NO_COTIZACION)

                                                        SELECT 
                                                        csb.CPR_ID, 
                                                        csb.COD_PRODUCTO 
                                                        ,{cotizacion.proveedor_codigo} AS COD_PROVEEDOR 
                                                        , NULL AS CODIGO 
                                                        , spcl.MONTO, spcl.CANTIDAD, spcl.TOTAL,
                                                        spcl.IVA_PORC AS IVA_PORC, spcl.IVA_MONTO AS IVA_MONTO,spcl.DESC_PORC as DESC_PORC, spcl.DESC_MONTO AS DESC_MONTO, getdate() as registro_fecha, 
                                                        csb.registro_usuario , 'V' as ESTADO, '{cotizacion.cotiza_numero}' AS NO_COTIZACION 
                                                        FROM CPR_SOLICITUD_BS csb 
                                                        left join CPR_SOLICITUD_PROV_COTIZA cspc ON csb.CPR_ID = cspc.CPR_ID
                                                        AND cspc.PROVEEDOR_CODIGO = {cotizacion.proveedor_codigo}
                                                        left join CPR_SOLICITUD_PROV_COTIZA_LINEAS spcl ON spcl.ID_COTIZACION = cspc.ID_COTIZACION
                                                        WHERE csb.CPR_ID = {solicitud.cpr_id}";

                                connection.Execute(queryDetalle);


                            query = $@"UPDATE  CPR_SOLICITUD_PROV SET 
                                            ESTADO = 'V' , 
                                            VALORA_PUNTAJE = 1000 ,
                                            COTIZA_FECHA = GETDATE(),
                                            COTIZA_USUARIO = '{cotizacion.registro_usuario}'
                                        WHERE 
                                        PROVEEDOR_CODIGO = {cotizacion.proveedor_codigo} AND 
                                        CPR_ID = {cotizacion.cpr_id} ";
                            connection.Execute(query);
                        }
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


        public ErrorDTO CprSolicitudCotizacionBs_Eliminar(int CodEmpresa, int id_cotizacion_linea)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCPR_SolicitudCotizacion_Eliminar {id_cotizacion_linea}";


                    var result = connection.ExecuteAsync(query);
                   
                
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

        }


        public ErrorDTO<List<CprSolicitudProvCotiza>> CprSolicitudContizacionLista_Obtener(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CprSolicitudProvCotiza>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCPR_SolicitudCotiLista_Obtener {cpr_id}, '{cod_proveedor}'";
                    response.Result = connection.Query<CprSolicitudProvCotiza>(query).ToList();
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



    }
}