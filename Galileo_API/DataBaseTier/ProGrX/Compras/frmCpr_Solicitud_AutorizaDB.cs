using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using Dapper;
using Newtonsoft.Json;
using PgxAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_Solicitud_AutorizaDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;
        private readonly frmCpr_SolicitudDB solicitudDB;

        public frmCpr_Solicitud_AutorizaDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(config);
            solicitudDB = new frmCpr_SolicitudDB(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Método para visualizar la lista de proveedores a adjudicar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <returns></returns>
        public ErrorDto<List<CprSolicitudAdjudicaConsulta>> CprSolicitudAdjudica_Consultar(int CodEmpresa, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprSolicitudAdjudicaConsulta>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCprSolicitudProveedoresLista_Obtener {cpr_id}";
                    response.Result = connection.Query<CprSolicitudAdjudicaConsulta>(query).ToList();
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

        /// <summary>
        /// Método para traer la lista de productos de proveedor adjudicados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="proveedor"></param>
        /// <param name="cotizacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CprSolicitudAdjudicaProductosDto>> CprSolicitudAdjudicaProductos_Consultar(int CodEmpresa, int cpr_id, int proveedor, string? cotizacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprSolicitudAdjudicaProductosDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //busco tipo de solicitud y monto 
                    var query = $"select * from CPR_SOLICITUD where CPR_ID = {cpr_id}";
                    var solicitud = connection.QueryFirstOrDefault<CprSolicitudDto>(query);

                    if(solicitud.tipo_orden == solicitudDB.CprSolicitud_TipoExcepcionGM(CodEmpresa).Description)
                    {
                        query = $@"select DISTINCT
                                    SP.NO_COTIZACION,
                                    C.COD_PRODUCTO
                                    , P.DESCRIPCION + '-' + C.MODELO as DESCRIPCION
                                    ,C.MONTO
                                    ,SP.ADJUDICA_IND
                                    ,C.CANTIDAD
                                    , C.DESC_MONTO
                                    ,C.IVA_MONTO
                                    ,C.TOTAL
                                    ,(
                                        SELECT CASE 
                                            WHEN (
                                                (SELECT MONTO FROM CPR_SOLICITUD WHERE CPR_ID = PP.CPR_ID) - 
                                                ISNULL((
                                                    SELECT SUM(TOTAL)
                                                    FROM CPR_SOLICITUD_PROV_BS
                                                    WHERE CPR_ID = PP.CPR_ID AND ADJUDICA_IND = 1
                                                ), 0)
                                            ) <= 0 THEN 1
                                            ELSE 0
                                        END
                                    ) AS OCUPADO
                                    FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS C
                                    LEFT JOIN CPR_SOLICITUD_PROV_COTIZA spc ON spc.ID_COTIZACION = C.ID_COTIZACION 
                                    LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO
                                    LEFT JOIN CPR_SOLICITUD_PROV_BS SP  ON C.COD_PRODUCTO = P.COD_PRODUCTO
                                    LEFT JOIN CPR_SOLICITUD_PROV PP ON PP.CPR_ID = SP.CPR_ID
                                    WHERE  SP.PROVEEDOR_CODIGO = {proveedor} AND PP.CPR_ID = {cpr_id}
                                    AND spc.CPR_ID = SP.CPR_ID AND spc.PROVEEDOR_CODIGO = SP.PROVEEDOR_CODIGO
                                    AND SP.NO_COTIZACION = '{cotizacion}' ";
                        response.Result = connection.Query<CprSolicitudAdjudicaProductosDto>(query).ToList();
                    }
                    else if (solicitud.tipo_orden == solicitudDB.CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    {
                        query = $@"select DISTINCT
                                    SP.NO_COTIZACION,
                                    C.COD_PRODUCTO
                                    , P.DESCRIPCION + '-' + C.MODELO as DESCRIPCION
                                    ,C.MONTO
                                    ,SP.ADJUDICA_IND
                                    ,C.CANTIDAD
                                    , C.DESC_MONTO
                                    ,C.IVA_MONTO
                                    ,C.TOTAL
                                    , (SELECT CASE WHEN EXISTS 
										    (SELECT DISTINCT 1 FROM CPR_SOLICITUD_PROV_BS 
										    WHERE CPR_ID =  SP.CPR_ID AND 
										    COD_PRODUCTO = C.COD_PRODUCTO AND ADJUDICA_IND = 1)
										THEN 1 ELSE 0 END ) AS OCUPADO
                                    FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS C
                                    LEFT JOIN CPR_SOLICITUD_PROV_COTIZA spc ON spc.ID_COTIZACION = C.ID_COTIZACION 
                                    LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO
                                    LEFT JOIN CPR_SOLICITUD_PROV_BS SP  ON C.COD_PRODUCTO = P.COD_PRODUCTO
                                    LEFT JOIN CPR_SOLICITUD_PROV PP ON PP.CPR_ID = SP.CPR_ID
                                    WHERE  SP.PROVEEDOR_CODIGO = {proveedor} AND PP.CPR_ID = {cpr_id}
                                    AND spc.CPR_ID = SP.CPR_ID AND spc.PROVEEDOR_CODIGO = SP.PROVEEDOR_CODIGO
                                    AND SP.NO_COTIZACION = '{cotizacion}' ";
                        response.Result = connection.Query<CprSolicitudAdjudicaProductosDto>(query).ToList();
                    }
                    else
                    {
                        query = $@"select DISTINCT C.COD_PRODUCTO, P.DESCRIPCION, C.MONTO, C.ADJUDICA_IND, 
                    (select DISTINCT CANTIDAD from CPR_SOLICITUD_BS where CPR_ID = C.CPR_ID AND COD_PRODUCTO = C.COD_PRODUCTO) AS CANTIDAD,
                     C.DESC_MONTO, C.IVA_MONTO, (C.MONTO + C.IVA_MONTO - C.DESC_MONTO) * CANTIDAD AS TOTAL,
                    (SELECT CASE WHEN EXISTS 
	                    (SELECT DISTINCT 1 FROM CPR_SOLICITUD_PROV_BS WHERE CPR_ID = C.CPR_ID AND COD_PRODUCTO = C.COD_PRODUCTO AND ADJUDICA_IND = 1)
                    THEN 1 ELSE 0 END ) AS OCUPADO
                    from CPR_SOLICITUD_PROV_BS C  left join PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO 
                    WHERE C.CPR_ID = {cpr_id} AND C.PROVEEDOR_CODIGO = '{proveedor}' AND C.NO_COTIZACION = '{cotizacion}'";
                        response.Result = connection.Query<CprSolicitudAdjudicaProductosDto>(query).ToList();
                    }

                        
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

        public ErrorDto CprSolicitudAdjudicaProv_Upsert(int CodEmpresa, string adjudica)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CprSolicitudAdjudicaGuardar datos = JsonConvert.DeserializeObject<CprSolicitudAdjudicaGuardar>(adjudica);
            CprSolicitudAdjudicaConsulta proveedor = datos.proveedor;
            ErrorDto response = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                string query = "", where = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //valido que tipo de solicitud 
                    //busco tipo de solicitud y monto 
                    query = $@"select * from CPR_SOLICITUD where CPR_ID = {datos.cpr_id}";
                    var solicitud = connection.QueryFirstOrDefault<CprSolicitudDto>(query);


                    SolicitudMontosDto montos = new SolicitudMontosDto();

                    if (solicitud.tipo_orden == solicitudDB.CprSolicitud_TipoExcepcionGM(CodEmpresa).Description)
                    {
                        //Busco total adjudicado para la solicitud.
                        query = $@"SELECT 
                                        s.MONTO AS MontoMaximo,
                                        ISNULL((
                                            SELECT SUM(TOTAL)
                                            FROM CPR_SOLICITUD_PROV_BS spbs1
                                            WHERE spbs1.CPR_ID = s.CPR_ID AND ADJUDICA_IND = 1
                                        ), 0) AS MontoAdjudicado,
                                        (
                                            SELECT TOP 1 TOTAL
                                            FROM CPR_SOLICITUD_PROV_BS spbs2
                                            WHERE spbs2.CPR_ID = s.CPR_ID 
                                              AND spbs2.ADJUDICA_IND IS NULL 
                                              AND spbs2.PROVEEDOR_CODIGO = {proveedor.proveedor_codigo}
                                        ) AS MontoOrden
                                    FROM CPR_SOLICITUD s
                                    WHERE s.CPR_ID = {datos.cpr_id} ";
                        montos = connection.QueryFirstOrDefault<SolicitudMontosDto>(query);
                    }


                    float montoConOrden = montos.MontoAdjudicado + ((montos.MontoOrden == null) ? 0 : montos.MontoOrden.Value);

                    if (montoConOrden > montos.MontoMaximo)
                    {
                        response.Code = -1;
                        response.Description = "El monto de la compra sobrepasa el valor permitido para la solicitud";
                        return response;
                    }

                    where = $"WHERE CPR_ID = {datos.cpr_id} AND PROVEEDOR_CODIGO = {proveedor.proveedor_codigo} ";

                    foreach (var item in datos.productos)
                    {
                        if (item.adjudica_ind == true)
                        {
                            query = $@"update CPR_SOLICITUD_PROV_BS SET ADJUDICA_IND = 1, ESTADO = 'F' 
                                {where} AND COD_PRODUCTO = '{item.cod_producto}' AND NO_COTIZACION = '{proveedor.no_cotizacion}'";
                            connection.Execute(query);
                        }
                        else
                        {
                            query = $@"update CPR_SOLICITUD_PROV_BS SET ADJUDICA_IND = 0, ESTADO = 'V' 
                                {where} AND COD_PRODUCTO = '{item.cod_producto}' AND NO_COTIZACION = '{proveedor.no_cotizacion}'";
                            connection.Execute(query);
                        }
                    }
                    query = $@"select COUNT(*) from CPR_SOLICITUD_PROV_BS {where} AND ADJUDICA_IND = 1";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        query = $@"update CPR_SOLICITUD_PROV SET ADJUDICA_IND = 1, ESTADO = 'F', 
                                ADJUDICA_USUARIO = '{datos.usuario}', ADJUDICA_FECHA = getdate() {where}";
                        connection.Execute(query);
                        response.Description = "Proveedor " + proveedor.descripcion + " adjudicado satisfactoriamente!";
                    }
                    else
                    {
                        query = $@"update CPR_SOLICITUD_PROV SET ADJUDICA_IND = 0, ESTADO = 'V', 
                                ADJUDICA_USUARIO = null, ADJUDICA_FECHA = null {where}";
                        connection.Execute(query);
                        response.Description = "Proveedor " + proveedor.descripcion + " desadjudicado satisfactoriamente!";
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<string> CprSolicitudRecomendacion_Obtener(int CodEmpresa, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT RECOMENDACION FROM CPR_SOLICITUD WHERE CPR_ID = {cpr_id}";
                    response.Result = connection.Query<string>(query).FirstOrDefault();
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

        public ErrorDto<string> CprSolicitudNumContrato_Obtener(int CodEmpresa, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT COD_CONTRATO FROM CPR_SOLICITUD WHERE CPR_ID = {cpr_id}";
                    response.Result = connection.Query<string>(query).FirstOrDefault();
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

        public ErrorDto CprSolicitudRecomendacion_Guardar(int CodEmpresa, int cpr_id, string recomendacion, string? cod_contrato, bool requiereContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                response.Description = "Recomendación guardada satisfactoriamente!";
                string updateContrato = "";
                if (requiereContrato)
                {
                    updateContrato = $@", COD_CONTRATO = '{cod_contrato}', I_CONTRATO_REQUIERE = 1 ";
                    response.Description = "Recomendación y número de contrato guardados satisfactoriamente!";
                }
                else
                {
                    updateContrato = $@", COD_CONTRATO = null, I_CONTRATO_REQUIERE = 0 ";
                    response.Description = "Recomendación guardada satisfactoriamente!";
                }

                    using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update CPR_SOLICITUD SET RECOMENDACION = '{recomendacion}' {updateContrato} WHERE CPR_ID = {cpr_id}";
                    connection.Execute(query);
                    
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto CprSolicitudAdjudicacion_Cerrar(int CodEmpresa, int cpr_id, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    //Valida si existen productos sin adjudicar
                    var queryValida = $@"SELECT COUNT(*) 
                                FROM (
                                    SELECT 
                                        CASE 
                                            WHEN EXISTS (
                                                SELECT 1 FROM CPR_SOLICITUD_PROV_BS 
                                                WHERE CPR_ID = P.CPR_ID 
                                                AND COD_PRODUCTO = P.COD_PRODUCTO 
                                                AND ADJUDICA_IND = 1
                                            ) 
                                            THEN 1 
                                            ELSE 0 
                                        END AS OCUPADO
                                    FROM CPR_SOLICITUD_BS P
                                    WHERE P.CPR_ID = {cpr_id}
                                ) AS subquery
                                WHERE OCUPADO = 0";
                    int prod_Sin_Adjudicar = connection.Query<int>(queryValida).FirstOrDefault();
                    if (prod_Sin_Adjudicar > 0)
                    {
                        response.Code = -1;
                        response.Description = "Existen productos sin adjudicar, por favor verifique!";
                        return response;
                    }

                    //valido que tipo de solicitud 
                    //busco tipo de solicitud y monto 
                    var query = $@"select * from CPR_SOLICITUD where CPR_ID = {cpr_id}";
                    var solicitud = connection.QueryFirstOrDefault<CprSolicitudDto>(query);

                    OrdenDatosAcciones ordenDatosAcciones = new OrdenDatosAcciones();
                    ordenDatosAcciones.usuario = usuario;
                    ordenDatosAcciones.tipo_orden = solicitud.tipo_orden;
                    ordenDatosAcciones.nota = solicitud.detalle;
                    ordenDatosAcciones.edita = false;
                    ordenDatosAcciones.estado = "S";
                    ordenDatosAcciones.cod_orden = "";

                    //Obtengo proveedores adjudicados
                    query = $@"SELECT PROVEEDOR_CODIGO FROM CPR_SOLICITUD_PROV 
                                      WHERE CPR_ID = {cpr_id} AND ADJUDICA_IND = 1 AND COD_ORDEN IS NULL ";
                    List<string> proveedores = connection.Query<string>(query).ToList();

                    if (solicitud.tipo_orden == solicitudDB.CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    {
                        if(solicitud.documento == null || solicitud.documento.Trim() == "" || solicitud.documento == "0")
                        {
                            response.Code = -1;
                            response.Description = "Se requiere el número de factura del proveedor seleccionado para la compra directa.";
                            return response;
                        }

                       response = CompraDirectaSolicitud_Autorizar(CodEmpresa, cpr_id, usuario);

                    }
                    else
                    {
                        //Obtengo lineas adjudicadas por proveedor
                        foreach (var proveedor in proveedores)
                        {
                            List<OrdenLineas> lineas = new List<OrdenLineas>();

                            query = $@"SELECT 
                                           [COD_PRODUCTO]
                                          ,'' AS DESCRIPCION
	                                      ,[CANTIDAD]
	                                      ,[MONTO] AS PRECIO
	                                      ,[DESC_PORC] AS DESCUENTO
	                                      ,[IVA_PORC] AS IMP_VENTAS
	                                      ,[TOTAL]
                                      FROM [dbo].[CPR_SOLICITUD_PROV_BS]
                                      WHERE CPR_ID = {cpr_id} AND ADJUDICA_IND = 1 AND PROVEEDOR_CODIGO = {proveedor}";
                            lineas = connection.Query<OrdenLineas>(query).ToList();
                            ordenDatosAcciones.lineas = lineas;

                            //Guarda OC
                            var resp = OrdenesGuardar(CodEmpresa, ordenDatosAcciones, proveedor, cpr_id);
                            if (resp.Code == -1)
                            {
                                response.Code = -1;
                                response.Description = resp.Description;
                            }
                            else
                            {
                                response.Description = resp.Description;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        private ErrorDto OrdenesGuardar(int CodEmpresa, OrdenDatosAcciones ordenes, string proveedor, int cpr_id)
        {
            ErrorDto<string> mjs = new ErrorDto<string>();
            var lineas = new ErrorDto<List<OrdenLineas>>();
            ErrorDto errorDto = new()
            {
                Code = 0
            };

            mjs = fxInvVerificaLineaDetalle(CodEmpresa, ordenes.lineas, "E");

            if (mjs.Code == -1)
            {
                errorDto.Code = -1;
                errorDto.Description = mjs.Description;
            }
            else
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                try
                {
                    float curSubTotal = 0;
                    float curDescuento = 0;
                    float curIV = 0;
                    float curCantidad = 0;
                    float curTotal = 0;

                    lineas = sbCalculaTotales(
                        ordenes.lineas,
                        ref curSubTotal,
                        ref curDescuento,
                        ref curIV,
                        ref curCantidad,
                        ref curTotal);

                    if (lineas.Code == -1)
                    {
                        errorDto.Code = -1;
                        errorDto.Description = lineas.Description;
                        return errorDto;
                    }
                    else
                    {
                        ordenes.lineas = lineas.Result;
                    }

                    using var connection = new SqlConnection(stringConn);
                    {
                        string vConsecutivo = "";
                        var query = $@"select isnull(max(cod_orden),0) + 1 as Ultimo from cpr_Ordenes";
                        vConsecutivo = connection.Query<string>(query).FirstOrDefault();
                        vConsecutivo = vConsecutivo.PadLeft(10, '0');

                        query = @$"
                        INSERT INTO cpr_ordenes (
                            cod_orden, tipo_orden, estado, genera_fecha, nota, genera_user,
                            subtotal, descuento, imp_ventas, total, pin_autorizacion, pin_entrada,
                            proceso, cod_proveedor
                        )
                        VALUES (
                            @CodOrden, @TipoOrden, 'S', GETDATE(), @Nota, @Usuario,
                            @SubTotal, @Descuento, @ImpVentas, @Total, 0, '', 'P', @CodProveedor
                        )";

                        var cabeceraParams = new
                        {
                            CodOrden = vConsecutivo,
                            TipoOrden = ordenes.tipo_orden,
                            Nota = ordenes.nota,
                            Usuario = ordenes.usuario,
                            SubTotal = curSubTotal,
                            Descuento = curDescuento,
                            ImpVentas = curIV,
                            Total = curTotal,
                            CodProveedor = proveedor
                        };

                        errorDto.Code = connection.Execute(query, cabeceraParams);


                        //Bitacora
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = ordenes.usuario,
                            DetalleMovimiento = "Registra, Orden Compra:" + vConsecutivo,
                            Movimiento = "Registra - WEB",
                            Modulo = 35
                        });

                        ordenes.cod_orden = vConsecutivo;
                        errorDto.Description = vConsecutivo;


                        //Guarda Detalle
                        var qDetalle = $@"delete cpr_ordenes_detalle where cod_orden = '{ordenes.cod_orden}'";
                        connection.Execute(qDetalle);

                        int linea = 0;
                        foreach (OrdenLineas item in ordenes.lineas)
                        {
                            linea++;
                            var qLinea = $@"insert cpr_ordenes_detalle(
                                         linea,
                                         cod_orden,
                                         cod_producto,
                                         cantidad,
                                         estado,
                                         precio,
                                         descuento,
                                         imp_ventas)
                             values( {linea}, 
                                         '{ordenes.cod_orden}', 
                                         '{item.Cod_Producto}', 
                                         {item.Cantidad}, 
                                         'A', 
                                         {item.Precio}, 
                                         {item.Descuento}, 
                                         {item.Imp_Ventas}) ";

                            connection.Execute(qLinea);
                        }

                         //query = $@"exec spCpr_Orden_Proceso '{vConsecutivo}','{ordenes.usuario}','{"+"}','{proveedor}','' ";
                         //connection.Execute(query);


                        //Actualizo el estado de la solicitud
                        query = $@"update CPR_SOLICITUD  SET ESTADO = 'F' ,
                                   ADJUDICA_ORDEN = '{vConsecutivo}',
                                      ADJUDICA_USUARIO = '{ordenes.usuario}',   
                                        ADJUDICA_FECHA = getdate(),
                                        ADJUDICA_PROVEEDOR = {proveedor}
                                    WHERE CPR_ID = {cpr_id}";
                        connection.Execute(query);

                        //Actualizo ordenes de proveedores
                        query = $@"update CPR_SOLICITUD_PROV SET ESTADO = 'F', ADJUDICA_ORDEN = '{vConsecutivo}' WHERE CPR_ID = {cpr_id} AND PROVEEDOR_CODIGO = {proveedor}";
                        connection.Execute(query);

                        //regreso de la respuesta
                        errorDto.Description = "Proceso de adjudicación cerrado satisfactoriamente!";
                    }
                }
                catch (Exception ex)
                {
                    errorDto.Code = -1;
                    errorDto.Description = ex.Message;
                }
            }
            return errorDto;

        }

        private ErrorDto<string> fxInvVerificaLineaDetalle(int CodEmpresa, List<OrdenLineas> vGrid,
          string vMov, int? ColBod1 = 0, int ColBod2 = 0)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<string>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (vGrid.Count == 0)
                    {
                        response.Code = -1;
                        response.Description = "No hay productos en la orden";
                        response.Result = "";
                    }

                    foreach (OrdenLineas item in vGrid)
                    {
                        //Verifica que el producto este activo
                        if (item.Cantidad > 0)
                        {
                            var query = $@"select estado from pv_productos where cod_producto = '{item.Cod_Producto}'";
                            var resp = connection.Query<string>(query).FirstOrDefault();
                            if (resp == null)
                            {
                                response.Code = -1;
                                response.Description = "El producto " + item.Cod_Producto + " no existe";
                            }
                            if (resp == "I")
                            {
                                response.Code = -1;
                                response.Description = "El producto " + item.Cod_Producto + " no esta activo";
                            }
                        }

                        //Verifica que la Bodega Exista y que Permita Registrar el Tipo de Movimiento
                        if (ColBod1 > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{ColBod1}'";
                            List<Models.BodegaDto> exist = connection.Query<Models.BodegaDto>(query).ToList();
                            if (exist.Count == 0)
                            {
                                response.Code = -1;
                                response.Description = "La bodega " + ColBod1 + " - No existe";
                            }
                            else
                            {
                                if (exist[0].estado == "I")
                                {
                                    response.Code = -1;
                                    response.Description = "La bodega " + ColBod1 + " - Se encuentra Inactiva";
                                }
                                else
                                {
                                    switch (vMov)
                                    {
                                        case "E":
                                            if (exist[0].permite_entradas != "1")
                                            {
                                                response.Code = -1;
                                                response.Description = "La bodega " + ColBod1 + " - No permite Entradas";
                                            }
                                            break;
                                        case "S":
                                        case "R":
                                        case "T":
                                            if (exist[0].permite_salidas != "1")
                                            {
                                                response.Code = -1;
                                                response.Description = "La bodega " + ColBod1 + " - No permite Salidas";
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (ColBod2 > 0)
                        {
                            var query = $@"select permite_entradas,permite_salidas,estado from pv_bodegas where cod_bodega = '{ColBod1}'";
                            List<Models.BodegaDto> exist = connection.Query<Models.BodegaDto>(query).ToList();
                            if (exist.Count == 0)
                            {
                                response.Code = -1;
                                response.Description = "La bodega " + ColBod2 + " - No existe";
                            }
                            else
                            {
                                if (exist[0].estado == "I")
                                {
                                    response.Code = -1;
                                    response.Description = "La bodega " + ColBod2 + " - Se encuentra Inactiva";
                                }
                                else
                                {
                                    switch (vMov)
                                    {
                                        case "E":
                                        case "T":
                                            if (exist[0].permite_entradas != "1")
                                            {
                                                response.Code = -1;
                                                response.Description = "La bodega " + ColBod2 + " - No permite Entradas";
                                            }
                                            break;
                                        case "S":
                                        case "R":
                                            if (exist[0].permite_salidas != "1")
                                            {
                                                response.Code = -1;
                                                response.Description = "La bodega " + ColBod2 + " - No permite Salidas";
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "";
            }

            return response;
        }

        private ErrorDto<List<OrdenLineas>> sbCalculaTotales(List<OrdenLineas> vGrid
           , ref float curSubTotal, ref float curDescuento, ref float curIV, ref float curCantidad,
           ref float curTotal)
        {

            var response = new ErrorDto<List<OrdenLineas>>();
            response.Result = new List<OrdenLineas>();

            try
            {
                foreach (OrdenLineas item in vGrid)
                {
                    curSubTotal = curSubTotal + (item.Cantidad * item.Precio);
                    float curTmpDesc = ((item.Cantidad * item.Precio) * (item.Descuento / 100));
                    curDescuento = curDescuento + curTmpDesc;

                    float curTmpIV = (((item.Cantidad * item.Precio) - curTmpDesc) * (item.Imp_Ventas / 100));
                    curIV = curIV + curTmpIV;

                    item.Total = (item.Cantidad * item.Precio) - curTmpDesc + curTmpIV;
                    curCantidad = curCantidad + item.Cantidad;
                }
                curTotal = curSubTotal + curIV - curDescuento;

                response.Result = vGrid;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Método para autorizar la compra directa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CPR_ID"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDto CompraDirectaSolicitud_Autorizar(int CodEmpresa, int CPR_ID, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //busco datos de orden
                    var querySolicitud = $@"SELECT * FROM CPR_SOLICITUD WHERE CPR_ID = {CPR_ID}";
                    var solicitud = connection.QueryFirstOrDefault<CprSolicitudDto>(querySolicitud);

                    //busco proveedor
                    var queryProv = $@"SELECT PROVEEDOR_CODIGO as com_dir_cod_proveedor,
                                    cp.DESCRIPCION as com_dir_des_proveedor
                                    FROM CPR_SOLICITUD_PROV P
                                    left join CXP_PROVEEDORES cp ON cp.COD_PROVEEDOR = P.PROVEEDOR_CODIGO
                                    WHERE CPR_ID = {CPR_ID} ";
                    var proveedor = connection.QueryFirstOrDefault<CprSolicitudDto>(queryProv);
                    if (proveedor != null)
                    {
                        solicitud.com_dir_cod_proveedor = proveedor.com_dir_cod_proveedor;
                        solicitud.com_dir_des_proveedor = proveedor.com_dir_des_proveedor;
                    }

                    //busco detalle
                    var queryDetalle = $@"SELECT BS.COD_PRODUCTO, BS.CANTIDAD, BS.MONTO, S.COD_BODEGA, BS.IVA_PORC, BS.IVA_MONTO, BS.DESC_PORC, 
                                            BS.DESC_MONTO, BS.TOTAL FROM CPR_SOLICITUD_PROV_BS BS LEFT JOIN CPR_SOLICITUD_BS S 
                                            ON S.CPR_ID = BS.CPR_ID
                                            WHERE BS.CPR_ID = {CPR_ID} ";
                    var detalle = connection.Query<CprSolicitudBsDto>(queryDetalle).ToList();

                    frmCprCompraDirectaDB compraDirectaDB = new frmCprCompraDirectaDB(_config);
                    CompraDirectaInsert directaInsert = new CompraDirectaInsert();
                    directaInsert.cod_factura = solicitud.documento;
                    mProGrX_AuxiliarDB _utils = new mProGrX_AuxiliarDB(_config);
                    string fecha = _utils.validaFechaGlobal(DateTime.Now);
                    directaInsert.fecha = fecha;
                    directaInsert.usuario = usuario;
                    directaInsert.causa = solicitud.tipo_orden;
                    directaInsert.notas = solicitud.detalle;
                    directaInsert.cod_proveedor = (int)solicitud.com_dir_cod_proveedor;
                    directaInsert.forma_pago = solicitud.int_tipo_pago;
                    directaInsert.divisa = solicitud.divisa;
                    directaInsert.tipo_pago = solicitud.int_forma_pago;

                    List<CompraDirectaDetalle> lineas = new List<CompraDirectaDetalle>();

                    float imp_venta = 0;
                    float descuento = 0;
                    foreach (CprSolicitudBsDto item in detalle)
                    {
                        CompraDirectaDetalle linea = new CompraDirectaDetalle();
                        linea.cod_producto = item.cod_producto;
                        linea.cantidad = item.cantidad;
                        linea.precio = item.monto;
                        linea.cod_bodega = item.cod_bodega;
                        linea.imp_ventas = (float)item.iva_porc;
                        linea.descuento = (float)item.desc_porc;
                        linea.total = item.total;
                        lineas.Add(linea);

                        imp_venta += (float)item.iva_monto;
                        descuento += (float)item.desc_monto;
                    }

                    directaInsert.sub_total = lineas.Sum(x => x.precio);
                    directaInsert.imp_ventas = imp_venta;
                    directaInsert.descuento = descuento;
                    directaInsert.total = lineas.Sum(x => x.total);

                    directaInsert.lineas = new List<CompraDirectaDetalle>();
                    directaInsert.lineas = lineas;

                    info = compraDirectaDB.CompraDirecta_Insertar(CodEmpresa, directaInsert);

                    if (info.Code != -1)
                    {
                        string[] ordenCompra = info.Description.Split('-');

                        //actualizo orden de compra
                        var queryUpdate = $@"UPDATE CPR_SOLICITUD_PROV SET 
                                                ESTADO = 'F',
                                                ADJUDICA_IND = 1, 
                                                ADJUDICA_ORDEN = '{ordenCompra[0]}',
                                                ADJUDICA_USUARIO = '{usuario}' ,
                                                ADJUDICA_FECHA = GETDATE() 
                                                WHERE CPR_ID = {CPR_ID}";
                        connection.Execute(queryUpdate);

                        queryUpdate = $@"UPDATE CPR_SOLICITUD SET 
                                                ESTADO = 'F',
                                                ADJUDICA_ORDEN = '{ordenCompra[0]}',
                                                ADJUDICA_USUARIO = '{usuario}' ,
                                                ADJUDICA_FECHA = GETDATE() 
                                                WHERE CPR_ID = {CPR_ID}";
                        connection.Execute(queryUpdate);

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

    }
}