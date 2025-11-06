using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;


namespace PgxAPI.DataBaseTier
{
    public class AF_Beneficios_Integral_CreDB
    {
        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;
        private readonly mBeneficiosDB _mBeneficiosDB;

        public AF_Beneficios_Integral_CreDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
            _mBeneficiosDB = new mBeneficiosDB(config);
        }
        /// <summary>
        /// Busco registros de benficio Crece por id de beneficio y consecutivo
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneSocioCreceDto> BeneSocioCrece_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<AfiBeneSocioCreceDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var Query = $@"SELECT [ID_CRECE]
                                      ,[COD_BENEFICIO]
                                      ,[CONSEC]
                                      ,[CAPACITACION_CMP]
                                      ,[APLICA_PRODUCTO]
                                      ,[COUTA_INICIAL]
                                      ,[COUTA_APLICAR]
                                      ,[AHORRO]
                                      ,[LIQUIDEZ]
                                      ,[OBSERVACIONES_PROD]
                                      ,[APLICA_BENE]
                                      ,[MONTO_PRIMERA_TARJETA]
                                      ,[ENTREGA_PRIMERA_TARJETA]
                                      ,[MONTO_SEGUNDA_TARJETA]
                                      ,[ENTREGA_SEGUNDA_TARJETA]
                                      ,[REGISTRO_FECHA]
                                      ,[REGISTRO_USUARIO]
                                      ,[MODIFICA_FECHA]
                                      ,[MODIFICA_USUARIO]
                                      ,[OBSERVACIONES_BENE], [fecha_cuota_inicial], [fecha_cuota_aplicar], [fecha_ahorro] FROM [dbo].[AFI_BENE_SOCIO_CRECE]
                                  Where CONSEC = {consec} AND COD_BENEFICIO = '{cod_beneficio}' ";

                    response.Result = connection.Query<AfiBeneSocioCreceDto>(Query).FirstOrDefault();
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
        /// Guardo el registro de beneficio Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto BeneSocioCrece_Guardar(int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                //Si el id es 0 es un insert, si no es un update
                if (beneficio.id_crece > 0)
                {
                    resp = BeneSocioCrece_Actualizar(CodCliente, beneficio);
                }
                else
                {
                    resp = BeneSocioCrece_Insertar(CodCliente, beneficio);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserto un nuevo registro de beneficio Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        private ErrorDto BeneSocioCrece_Insertar(int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int capacitacion_cmp = beneficio.capacitacion_cmp ? 1 : 0;
                    int aplica_producto = beneficio.aplica_producto ? 1 : 0;

                    int aplicar_beneficio = beneficio.aplica_bene ? 1 : 0;
                    int entrega_primera_tarjeta = beneficio.entrega_primera_tarjeta ? 1 : 0;
                    int entrega_segunda_tarjeta = beneficio.entrega_segunda_tarjeta ? 1 : 0;

                    string fecha_cuota_inicial = mAuxiliarDB.validaFechaGlobal(beneficio.fecha_cuota_inicial);
                    string fecha_cuota_aplicar = mAuxiliarDB.validaFechaGlobal(beneficio.fecha_cuota_aplicar);
                    string fecha_ahorro = mAuxiliarDB.validaFechaGlobal(beneficio.fecha_ahorro);


                    var Query = $@"INSERT INTO [dbo].[AFI_BENE_SOCIO_CRECE]
                                           ([COD_BENEFICIO]
                                           ,[CONSEC]
                                           ,[CAPACITACION_CMP]
                                           ,[APLICA_PRODUCTO]
                                           ,[COUTA_INICIAL]
                                           ,[COUTA_APLICAR]
                                           ,[AHORRO]
                                           ,[LIQUIDEZ]
                                           ,[OBSERVACIONES_PROD]
                                           ,[APLICA_BENE]
                                           ,[MONTO_PRIMERA_TARJETA]
                                           ,[ENTREGA_PRIMERA_TARJETA]
                                           ,[MONTO_SEGUNDA_TARJETA]
                                           ,[ENTREGA_SEGUNDA_TARJETA]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO]
                                           ,[OBSERVACIONES_BENE]
                                           ,[fecha_cuota_inicial]
                                           ,[fecha_cuota_aplicar]
                                           ,[fecha_ahorro] )
                                     VALUES
                                           ('{beneficio.cod_beneficio}'
                                           ,{beneficio.consec}
                                           ,{capacitacion_cmp}
                                           ,{aplica_producto}
                                           ,{beneficio.couta_inicial}
                                           ,{beneficio.couta_aplicar}
                                           ,{beneficio.ahorro}
                                           ,{beneficio.liquidez}
                                           ,'{beneficio.observaciones_prod}'
                                           ,{aplicar_beneficio}
                                           ,{beneficio.monto_primera_tarjeta}
                                           ,{entrega_primera_tarjeta}
                                           ,{beneficio.monto_segunda_tarjeta}
                                           ,{entrega_segunda_tarjeta}
                                           ,getDate()
                                           ,'{beneficio.registro_usuario}'
                                           ,'{beneficio.observaciones_bene}'
                                            ,'{fecha_cuota_inicial}'
                                            ,'{fecha_cuota_aplicar}'
                                            ,'{fecha_ahorro}'
                                    )";

                    resp.Code = connection.Execute(Query);

                    Query = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_CRECE') as 'id'";
                    var id = connection.Query<int>(Query).FirstOrDefault();

                    resp.Description = id.ToString();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Actualizo el registro de beneficio Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        private ErrorDto BeneSocioCrece_Actualizar(int CodCliente, AfiBeneSocioCreceDto beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();

            try
            {

                if (beneficio.monto_segunda_tarjeta != 0)
                {
                    if (beneficio.entrega_primera_tarjeta == false)
                    {
                        resp.Code = -1;
                        resp.Description = "No se puede ingresar monto de segunda tarjeta sin haber entregado la primera";
                        return resp;
                    }
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    int capacitacion_cmp = beneficio.capacitacion_cmp ? 1 : 0;
                    int aplica_producto = beneficio.aplica_producto ? 1 : 0;

                    int aplicar_beneficio = beneficio.aplica_bene ? 1 : 0;
                    int entrega_primera_tarjeta = beneficio.entrega_primera_tarjeta ? 1 : 0;
                    int entrega_segunda_tarjeta = beneficio.entrega_segunda_tarjeta ? 1 : 0;

                    var Query = $@"UPDATE [dbo].[AFI_BENE_SOCIO_CRECE]
                                   SET 
                                       [CAPACITACION_CMP] = {capacitacion_cmp}
                                      ,[APLICA_PRODUCTO] = {aplica_producto}
                                      ,[COUTA_INICIAL] = {beneficio.couta_inicial}
                                      ,[COUTA_APLICAR] = {beneficio.couta_aplicar}
                                      ,[AHORRO] = {beneficio.ahorro}
                                      ,[LIQUIDEZ] = {beneficio.liquidez}
                                      ,[OBSERVACIONES_PROD] = '{beneficio.observaciones_prod}'
                                      ,[OBSERVACIONES_BENE] = '{beneficio.observaciones_bene}'
                                      ,[APLICA_BENE] = {aplicar_beneficio}
                                      ,[MONTO_PRIMERA_TARJETA] = {beneficio.monto_primera_tarjeta}
                                      ,[ENTREGA_PRIMERA_TARJETA] = {entrega_primera_tarjeta}
                                      ,[MONTO_SEGUNDA_TARJETA] = {beneficio.monto_segunda_tarjeta}
                                      ,[ENTREGA_SEGUNDA_TARJETA] = {entrega_segunda_tarjeta}
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[fecha_cuota_inicial] = '{beneficio.fecha_cuota_inicial}'
                                      ,[fecha_cuota_aplicar] = '{beneficio.fecha_cuota_aplicar}'
                                      ,[fecha_ahorro] = '{beneficio.fecha_ahorro}'
                                      ,[MODIFICA_USUARIO] = '{beneficio.modifica_usuario}'
                                 WHERE ID_CRECE = {beneficio.id_crece} ";

                    resp.Code = connection.Execute(Query);

                    resp.Description = beneficio.id_crece.ToString();

                    //valida si contiene tarjetas asociadas
                    if (beneficio.monto_primera_tarjeta > 0)
                    {
                        var QueryExist = $@"DELETE FROM afi_bene_prodasg WHERE cod_beneficio = '{beneficio.cod_beneficio}' and consec = {beneficio.consec} ";
                        var Existe = connection.Execute(QueryExist);

                        //Elimina tarjeta regalo
                        //QueryExist = $@"DELETE FROM AFI_BENE_TARJETAS_REGALO WHERE cod_beneficio = '{beneficio.cod_beneficio}' and consec = {beneficio.consec} ";
                        //connection.Execute(QueryExist);

                        string codTarjeta = _config.GetSection("AFI_Beneficios").GetSection("CodProductoCrece").Value.ToString();

                       var query = $@"SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = '{codTarjeta}' ";
                            codTarjeta = connection.Query<string>(query).FirstOrDefault();

                       query = $@"insert afi_bene_prodasg(consec,cod_beneficio,cod_producto,cantidad,costo_unidad,REGISTRO_FECHA, REGISTRO_USUARIO )
                               	values({beneficio.consec},'{beneficio.cod_beneficio}','{codTarjeta}',{1},{beneficio.monto_primera_tarjeta}, getDate(), '{beneficio.modifica_usuario}')";
                       connection.Execute(query);

                       if (beneficio.entrega_primera_tarjeta == true && beneficio.monto_segunda_tarjeta > 0)
                       {
                            query = $@"UPDATE afi_bene_prodasg 
                                SET 
                                   cantidad = {2} 
                                WHERE consec = {beneficio.consec} AND cod_beneficio = '{beneficio.cod_beneficio}'";
                            connection.Execute(query);

                       }

                       //Inserta tarjeta regalo
                        //query = $@"select COUNT(*) from afi_bene_productos where tarjeta_regalo = 1 and COD_PRODUCTO = '{codTarjeta}'";
                        //var tipoTarjeta = connection.Query<int>(query).FirstOrDefault();
                        //if (tipoTarjeta > 0)
                        //{
                        //    query = $@"select id_beneficio from AFI_BENE_OTORGA where cod_beneficio = '{beneficio.cod_beneficio}' and consec = {beneficio.consec}";
                        //    string id_beneficio = connection.Query<string>(query).First();

                        //    query = $@"select cedula from AFI_BENE_OTORGA where cod_beneficio = '{beneficio.cod_beneficio}' and consec = {beneficio.consec}";
                        //    string cedula = connection.Query<string>(query).First();

                        //    query = $@"insert AFI_BENE_TARJETAS_REGALO(COD_PRODUCTO,REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, CONSEC, CEDULA, ID_BENEFICIO, ESTADO )
        	               // values('{codTarjeta}', getDate(), '{beneficio.modifica_usuario}', '{beneficio.cod_beneficio}', {beneficio.consec}, '{cedula}', {id_beneficio}, 'P' )";
                        //    connection.Execute(query);
                        //}

                        //Revisa campo monto nuevo 
                        var queryM = $@"SELECT MONTO_NUEVO FROM AFI_BENE_REGISTRO_MONTOS 
                            WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio}' ";
                        var monto = connection.Query<float>(queryM).FirstOrDefault();

                        queryM = $@"SELECT SUM(MONTO_PRIMERA_TARJETA + MONTO_SEGUNDA_TARJETA) FROM AFI_BENE_SOCIO_CRECE 
                            WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio}' ";
                        var montoNuevo = connection.Query<float>(queryM).FirstOrDefault();

                        if (monto != montoNuevo)
                        {
                            queryM = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_MONTOS]
                                   SET
                                        [MONTO_NUEVO]  = {montoNuevo}
                                       ,[MONTO_ANTERIOR] = {monto}
                                       ,[NOTAS] = '{beneficio.observaciones_bene}'
                                       ,[REGISTRO_FECHA] = GETDATE()
                                       ,[REGISTRO_USUARIO] ='{beneficio.modifica_usuario}'
                                        WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio}' ";

                            connection.Execute(queryM);

                            queryM = @$"UPDATE AFI_BENE_OTORGA SET MONTO_APLICADO = {montoNuevo} 
                                        WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio}' ";
                            connection.Execute(queryM);

                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = beneficio.cod_beneficio,
                                consec = beneficio.consec,
                                movimiento = "Actualiza",
                                detalle = $@"Actualiza Monto de {monto} a {montoNuevo} ",
                                registro_usuario = beneficio.modifica_usuario
                            });
                        }
                    }
                    else
                    {
                        var QueryExist = $@"SELECT COUNT(*) FROM afi_bene_prodasg WHERE cod_beneficio = '{beneficio.cod_beneficio}' and consec = {beneficio.consec} ";
                        var Existe = connection.Query<int>(QueryExist).FirstOrDefault();
                        if (Existe > 0)
                        {
                            QueryExist = $@"DELETE FROM afi_bene_prodasg WHERE cod_beneficio = '{beneficio.cod_beneficio}' and consec = {beneficio.consec} ";
                            connection.Execute(QueryExist);

                            QueryExist = @$"UPDATE AFI_BENE_OTORGA SET MONTO_APLICADO = 0 
                                        WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio}' ";
                            connection.Execute(QueryExist);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Busco las sesiones de un beneficio Crece, las sessiones se componen de 5 cursos y se guardan en la tabla AFI_BENE_SOCIO_CRECE_SESIONES
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneSocioCreceSesionesDto>> BeneSocioCreceSesiones_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneSocioCreceSesionesDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT [ID_SESION]
                                      ,[COD_BENEFICIO]
                                      ,[CONSEC]
                                      ,[SESION]
                                      ,[ASISTENCIA]
                                      ,[TAREA]
                                      ,[NOTAS]
                                      ,[SESION_FECHA]
                                      ,[REGISTRO_FECHA]
                                      ,[REGSITRO_USUARIO]
                                      ,[MODIFICA_FECHA]
                                      ,[MODIFICA_USUARIO]
                                  FROM [dbo].[AFI_BENE_SOCIO_CRECE_SESIONES]
                                  Where CONSEC = {consec} AND COD_BENEFICIO = '{cod_beneficio}' ";

                    response.Result = connection.Query<AfiBeneSocioCreceSesionesDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneSocioCreceSesiones_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Guardo o actualizo las sesiones de un beneficio Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto BeneSocioCreceSesion_Guardar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                //Si el id es 0 es un insert, si no es un update
                if (beneficio.id_sesion > 0)
                {
                    resp = BeneSocioCreceSession_Actualizar(CodCliente, beneficio);
                }
                else
                {
                    resp = BeneSocioCreceSession_Insertar(CodCliente, beneficio);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneSocioCreceSesion_Guardar - " + ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserto una nueva sesion de un beneficio Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        private ErrorDto BeneSocioCreceSession_Insertar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int asistencia = beneficio.asistencia ? 1 : 0;
                    int tarea = beneficio.tarea ? 1 : 0;

                    var Query = $@"INSERT INTO [dbo].[AFI_BENE_SOCIO_CRECE_SESIONES]
                                       ([COD_BENEFICIO]
                                       ,[CONSEC]
                                       ,[SESION]
                                       ,[ASISTENCIA]
                                       ,[TAREA]
                                       ,[NOTAS]
                                       ,[SESION_FECHA]
                                       ,[REGISTRO_FECHA]
                                       ,[REGSITRO_USUARIO]
                                       )
                                 VALUES
                                       ('{beneficio.cod_beneficio}'
                                       ,{beneficio.consec}
                                       ,'{beneficio.sesion}'
                                       ,{asistencia}
                                       ,{tarea}
                                       ,'{beneficio.notas}'
                                       ,'{beneficio.sesion_fecha}'
                                       ,getDate()
                                       ,'{beneficio.regsitro_usuario}'
                                       )";
                    resp.Code = connection.Execute(Query);

                    Query = "SELECT IDENT_CURRENT('AFI_BENE_SOCIO_CRECE_SESIONES') as 'id'";
                    var id = connection.Query<int>(Query).FirstOrDefault();

                    resp.Description = id.ToString();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneSocioCreceSession_Insertar - " + ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Actualizo una sesion de un beneficio Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        private ErrorDto BeneSocioCreceSession_Actualizar(int CodCliente, AfiBeneSocioCreceSesionesDto beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int asistencia = beneficio.asistencia ? 1 : 0;
                    int tarea = beneficio.tarea ? 1 : 0;

                    var Query = $@"UPDATE [dbo].[AFI_BENE_SOCIO_CRECE_SESIONES]
                                   SET 
                                       [SESION] = '{beneficio.sesion}'
                                      ,[ASISTENCIA] = {asistencia}
                                      ,[TAREA] = {tarea}
                                      ,[NOTAS] = '{beneficio.notas}'
                                      ,[SESION_FECHA] = '{beneficio.sesion_fecha}'
                                      ,[REGISTRO_FECHA] = getDate()
                                      ,[REGSITRO_USUARIO] = '{beneficio.regsitro_usuario}'
                                 WHERE ID_SESION = {beneficio.id_sesion} ";

                    resp.Code = connection.Execute(Query);

                    resp.Description = beneficio.id_sesion.ToString();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = "BeneSocioCreceSession_Actualizar - " + ex.Message;
            }
            return resp;
        }


    }
}