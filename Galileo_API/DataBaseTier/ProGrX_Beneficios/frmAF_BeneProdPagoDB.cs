using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneProdPagoDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public frmAF_BeneProdPagoDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(config);
        }

        public ErrorDto<AfiBeneProdAsgDataList> AfiBeneProdAsgLista_Obtener(int CodCliente, string cod_beneficio, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<AfiBeneProdAsgDataList>();
            response.Result = new AfiBeneProdAsgDataList();
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"Select COUNT(A.cod_beneficio) from afi_bene_prodasg A
                        left join AFI_BENE_PAGO P on A.cod_beneficio = P.cod_Beneficio and A.Consec = P.Consec
                        left join afi_bene_otorga O on A.cod_beneficio = O.cod_Beneficio and A.Consec = O.Consec
                        left join Socios S on O.cedula = S.cedula
                        where O.estado IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO IN ('A') )
                        and A.cod_beneficio = '{cod_beneficio}' AND P.ESTADO != 'E'";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND (A.consec LIKE '%" + filtro + "%' OR O.cedula LIKE '%" + filtro + "%' OR S.nombre LIKE '%" + filtro + "%')";
                    }
                    if (pagina != null)
                    {
                        paginaActual = "ORDER BY A.cod_beneficio OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"Select 
                                ROW_NUMBER() OVER (ORDER BY A.REGISTRO_FECHA desc) AS linea,                                
                                CONCAT(FORMAT(O.ID_BENEFICIO, '00000') , TRIM(A.COD_BENEFICIO) ,FORMAT(A.consec, '00000') ) as EXPEDIENTE,A.COD_PRODUCTO,A.cod_beneficio,  O.cedula, isnull(S.nombre,'') as 'Nombre'
                                ,1 as cantidad,
								A.costo_unidad as 'Monto'
								,A.REGISTRO_FECHA
								,A.CONSEC
								,O.ID_BENEFICIO
								,(SELECT DESCRIPCION FROM AFI_BENE_PRODUCTOS WHERE COD_PRODUCTO = A.COD_PRODUCTO) as ProductoDesc
                                    ,(select TARJETA_REGALO from AFI_BENE_PRODUCTOS where COD_PRODUCTO = A.COD_PRODUCTO) as tarjeta                               
                                 ,P.ID_PAGO 
                                , COALESCE( (SELECT NO_TARJETA FROM AFI_BENE_TARJETAS_REGALO 
                                 WHERE COD_PRODUCTO = P.COD_PRODUCTO AND COD_BENEFICIO = P.COD_BENEFICIO AND CONSEC = P.CONSEC AND ID_PAGO = P.ID_PAGO),  NULL ) AS noTarjeta 
                                from afi_bene_prodasg A 
                                left join AFI_BENE_PAGO P on A.cod_beneficio = P.cod_Beneficio and A.Consec = P.Consec
                                left join afi_bene_otorga O on A.cod_beneficio = O.cod_Beneficio and A.Consec = O.Consec
                                left join Socios S on O.cedula = S.cedula
                                where O.estado IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO IN ('A') )
		                        and A.cod_beneficio = '{cod_beneficio}'
                                AND P.ESTADO != 'E'
                                {filtro} 
                                {paginaActual} {paginacionActual}";

                    response.Result.Beneficios = connection.Query<AfiBeneProdAsgData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
            }
            return response;

        }

        public ErrorDto AfiBeneOtorga_Actualiza(int CodCliente, string beneficio)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                List<AfiBeneProdAsgData> beneficioModel = JsonConvert.DeserializeObject<List<AfiBeneProdAsgData>>(beneficio);


                if (beneficioModel.Count > 0)
                {
                    string expediente = "", codProdInv = "", query = "";

                    using var connection = new SqlConnection(stringConn);
                    {
                        foreach (var item in beneficioModel)
                        {
                            query = $@"SELECT cod_producto_inv FROM AFI_BENE_PRODUCTOS WHERE cod_producto = '{item.Cod_Producto}'";
                            codProdInv = connection.Query<string>(query).FirstOrDefault();

                            if(codProdInv != null) { 
                                var existencia = $@"SELECT TOP 1 EXISTENCIA FROM PV_INVENTARIO  
                                WHERE cod_producto = '{codProdInv}' 
                                ORDER BY ANIO DESC, MES DESC, ENTRADA_FECHA DESC";
                                int cantidad = connection.Query<int>(existencia).FirstOrDefault();
                                if (cantidad < item.Cantidad)
                                {
                                    info.Code = -1;
                                    info.Description = "No hay suficiente existencia para el producto " + item.Cod_Producto;
                                    return info;
                                }

                                expediente += item.expediente + " ,";
                            }
                        }

                        expediente = expediente.TrimEnd(',');

                        //creo la boleta de salida de inventario
                        var queryC = $@"select isnull(max(Boleta),0)+1 as Ultimo from pv_InvTranSac where Tipo = 'S' ";
                        var consecutivo = connection.Query<string>(queryC).FirstOrDefault();
                        consecutivo = consecutivo.PadLeft(10, '0');
                        string ultimaBoleta = consecutivo.ToString();

                        if (expediente.Length != 0)
                        {
                            var queryInsertBoleta = $@"INSERT INTO [dbo].[PV_INVTRANSAC]
                                                       ([BOLETA]
                                                       ,[TIPO]
                                                       ,[COD_ENTSAL]
                                                       ,[FECHA]
                                                       ,[ESTADO]
                                                       ,[FECHA_SISTEMA]
                                                       ,[NOTAS]
                                                       ,[DOCUMENTO]
                                                       ,[PLANTILLA]
                                                       ,[GENERA_USER]
                                                       ,[GENERA_FECHA]
                                                       ,[AUTORIZA_USER]
                                                       ,[AUTORIZA_FECHA]
                                                       ,[PROCESA_USER]
                                                       ,[PROCESA_FECHA]
                                                       ,[TOTAL] )
                                                 VALUES
                                                       ('{ultimaBoleta}'
                                                       ,'S'
                                                       ,'S'
                                                       ,getDate()
                                                       ,'A'
                                                       ,getDate()
                                                       ,'Expedientes: {expediente}'
                                                       ,'ENTREGA BENEFICIO'
                                                       ,0
                                                       ,'{beneficioModel[0].autoriza_user}'
                                                       ,getDate()
                                                       ,'{beneficioModel[0].autoriza_user}'
                                                       ,getDate()
                                                       ,'{beneficioModel[0].autoriza_user}'
                                                       ,getDate()
                                                       ,{beneficioModel.Sum(x => x.Monto)} )";
                            connection.Execute(queryInsertBoleta);
                        }

                        foreach (var item in beneficioModel)
                        {
                            int existeTarjeta = 0;
                            //Carga tarjeta en recargas de regalo
                            if (item.tarjeta)
                            {
                                var queryT = $@"select COUNT(*) from AFI_BENE_TARJETAS_REGALO 
                                where COD_PRODUCTO = '{item.Cod_Producto}' and cod_beneficio = '{item.cod_beneficio}' and consec = {item.Consec}";
                                existeTarjeta = connection.Query<int>(queryT).FirstOrDefault();

                                //valida si se le asigno una tarjeta ya recargada que este disponible
                                queryT = $@"select estado from AFI_BENE_TARJETAS_REGALO 
                                where COD_PRODUCTO = '{item.Cod_Producto}' and cod_beneficio = '{item.cod_beneficio}' and no_tarjeta = '{item.noTarjeta}'";
                                var estadoTarjeta = connection.Query<string>(queryT).FirstOrDefault();
                                if (estadoTarjeta == "D")
                                {
                                    query = $@"Update AFI_BENE_TARJETAS_REGALO set estado = 'E', consec = {item.Consec}, 
					                      id_beneficio = {item.id_beneficio}, cedula = '{item.Cedula}', id_pago = {item.id_pago} 
                                         where cod_producto = '{item.Cod_Producto}' and cod_beneficio = '{item.cod_beneficio}' and no_tarjeta = '{item.noTarjeta}'";
                                    connection.Execute(query);
                                }
                                else
                                {
                                    var insertTarjeta = $@"insert AFI_BENE_TARJETAS_REGALO
                                    (COD_PRODUCTO,REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, CONSEC, CEDULA, ID_BENEFICIO, ESTADO, NO_TARJETA, MONTO, ID_PAGO )
                                 	values('{item.Cod_Producto}', getDate(), '{item.autoriza_user}', '{item.cod_beneficio}', {item.Consec}, '{item.Cedula}'
                                    , {item.id_beneficio}, 'P', '{item.noTarjeta}', {item.Monto}, {item.id_pago})";
                                    connection.Execute(insertTarjeta);
                                }
                            }

                            query = $@"SELECT cod_producto_inv FROM AFI_BENE_PRODUCTOS WHERE cod_producto = '{item.Cod_Producto}'";
                            codProdInv = connection.Query<string>(query).FirstOrDefault();
                            if (existeTarjeta == 0 && codProdInv != null) 
                            { 
                                //Inserto detalle boleta salida
                                var QueryDet = $@"INSERT INTO [dbo].[PV_INVTRADET]
                                                   ([LINEA]
                                                   ,[BOLETA]
                                                   ,[TIPO]
                                                   ,[COD_BODEGA]
                                                   ,[COD_PRODUCTO]
                                                   ,[CANTIDAD]
                                                   ,[PRECIO]
                                                   ,[DESPACHO])
                                             VALUES
                                                   ({item.linea}
                                                   ,'{ultimaBoleta}'
                                                   ,'S'
                                                   ,(select COD_BODEGA from PV_BODEGAS where DESCRIPCION = 'Beneficios Solidarios') 
                                                   ,'{codProdInv}' 
                                                   ,{item.Cantidad}
                                                   ,{item.Monto}
                                                   ,{item.Cantidad}) ";
                                connection.Execute(QueryDet);
                            }
                            //var query = $@"Update afi_bene_otorga set estado = 'E',autoriza_user = '{item.autoriza_user}',autoriza_fecha = Getdate()
                            //      where cedula = '{item.Cedula}' and cod_beneficio = '{item.cod_beneficio}' and consec = {item.Consec}";
                            //connection.Execute(query);

                            query = $@"Update afi_bene_pago set estado = 'E',envio_user = '{item.autoriza_user}',envio_fecha = Getdate()
                                  where cedula = '{item.Cedula}' and cod_beneficio = '{item.cod_beneficio}' and consec = {item.Consec} 
                                    AND COD_PRODUCTO = '{item.Cod_Producto}'";
                            connection.Execute(query);

                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = item.cod_beneficio,
                                consec = item.Consec,
                                movimiento = "Actualiza",
                                detalle = $"Entrega de Producto COD: [{item.Cod_Producto}], Monto: [{item.Monto}]",
                                registro_usuario = item.autoriza_user.ToUpper(),
                            });
                        }

                        frmInvTransacProcesaDB _procesaBoleta = new frmInvTransacProcesaDB(_config);
                        InvTransacProcesa boleta = new InvTransacProcesa();
                        boleta.Tipo = "S";
                        boleta.Boleta = ultimaBoleta;
                        boleta.Usuario = beneficioModel[0].autoriza_user;

                        _procesaBoleta.InvTransacProcesa_SP(CodCliente, boleta);

                    }

                    info.Description = "Registro Actualizado";
                }
                else
                {
                    info.Code = 2;
                    info.Description = "No se encontraron registros para actualizar";
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        public ErrorDto<List<AfiBeneficiosData>> AfiBeneficios_Obtener(int CodCliente)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneficiosData>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_Beneficio) as 'cod_Beneficio',  rtrim(descripcion) as 'descripcion'
                                    from afi_beneficios
                                     where cod_beneficio in (select cod_beneficio from afi_bene_prodasg)
                        and cod_beneficio in (select COD_BENEFICIO from AFI_BENE_PAGO where ESTADO != 'E' AND TIPO = 'P' ) ";
                    response.Result = connection.Query<AfiBeneficiosData>(query).ToList();
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

        public ErrorDto<List<AfiBeneficiosData>> AfiBeneProdAsg_Obtener(int CodCliente, string consec, string cod_beneficio)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneficiosData>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select B.*, P.descripcion as 'ProductoDesc' 
                           from afi_bene_prodasg B inner join afi_bene_productos P on B.cod_producto = P.cod_Producto
                           where consec = {consec} and cod_beneficio = '{cod_beneficio}'";
                    response.Result = connection.Query<AfiBeneficiosData>(query).ToList();
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