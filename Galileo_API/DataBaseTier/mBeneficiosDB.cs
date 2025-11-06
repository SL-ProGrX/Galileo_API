using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class mBeneficiosDB
    {
        private readonly IConfiguration _config;
        public mBeneficiosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto fxNombre(int CodEmpresa, string cedula)
        {
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select nombre from socios where cedula = '{cedula.Trim()}'";
                    info.Description = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto fxDescribeBanco(int CodEmpresa, int codBanco)
        {
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $"select descripcion from Tes_Bancos where id_banco =  '{codBanco}'";
                    info.Description = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public string fxEstadoBeneficio(string estado)
        {
            string fxEstadoBeneficio = "";
            switch (estado.ToUpper())
            {
                case "A":
                    fxEstadoBeneficio = "APROBADO";
                    break;
                case "S":
                    fxEstadoBeneficio = "SOLICITADO";
                    break;
                case "R":
                    fxEstadoBeneficio = "RECHAZADO";
                    break;
                case "E":
                    fxEstadoBeneficio = "EJECUTADO";
                    break;
                case "P":
                    fxEstadoBeneficio = "PENDIENTE";
                    break;
                case "APROBADO":
                    fxEstadoBeneficio = "A";
                    break;
                case "SOLICITADO":
                    fxEstadoBeneficio = "S";
                    break;
                case "RECHAZADO":
                    fxEstadoBeneficio = "R";
                    break;
                case "EJECUTADO":
                    fxEstadoBeneficio = "E";
                    break;
                case "PENDIENTE":
                    fxEstadoBeneficio = "P";
                    break;
                default:
                    // Manejar el caso donde Estado no coincide con ningún valor esperado
                    fxEstadoBeneficio = "DESCONOCIDO"; // O cualquier valor por defecto que consideres
                    break;
            }
            return fxEstadoBeneficio;
        }

        public string fxSIFParametros(int CodEmpresa, string cod_parametro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string resp = "N";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $"Select valor from SIF_parametros where cod_parametro = '{cod_parametro}'";
                    resp = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                resp = "N";
                _ = ex.Message;
            }
            return resp;
        }

        public string fxFSL_Parametros(int CodEmpresa, string cod_parametro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string resp = "N";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $"select valor from fsl_parametros where cod_parametro = '{cod_parametro}' ";
                    resp = connection.Query<string>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = "N";
            }
            return resp;
        }

        /// <summary>
        /// Registra en bitácora los movimientos del beneficio Integral
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto BitacoraBeneficios(BitacoraBeneInsertarDto req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(req.EmpresaId);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    //prueba de consecutivo
                    //if(req.consec == 0)
                    //{
                    //    var consec = connection.Query<int>("select isnull(max(consec),0)+1 from AFI_BENE_REGISTRO_BITACORA where COD_BENEFICIO = @cod_beneficio", new { cod_beneficio = req.cod_beneficio });
                    //    req.consec = consec.FirstOrDefault();
                    //}


                    var strSQL = $@"INSERT INTO [dbo].[AFI_BENE_REGISTRO_BITACORA]
                                           ([COD_BENEFICIO]
                                           ,[CONSEC]
                                           ,[MOVIMIENTO]
                                           ,[DETALLE]
                                           ,[REGISTRO_FECHA]
                                           ,[REGISTRO_USUARIO])
                                     VALUES
                                           ('{req.cod_beneficio}'
                                           ,{req.consec}
                                           ,'{req.movimiento}' 
                                           , '{req.detalle}'
                                           , getdate()
                                           , '{req.registro_usuario}' )";

                    resp.Code = connection.Execute(strSQL);
                    resp.Description = "Ok";
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
        /// Valida si el socio cumple con los requisitos y validaciones para aplicar un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        //public ErrorDto<BeneficioGeneralDatos> ValidaBeneficio(int CodCliente, BeneficioGeneralDatos beneficio)
        //{
        //    var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
        //    var response = new ErrorDto<BeneficioGeneralDatos>();

        //    try
        //    {
        //        string estadoItem = "S";
        //        if (beneficio.estado != null)
        //        {
        //            estadoItem = beneficio.estado.item;
        //        }
        //        string result = "";
        //        using (var connection = new SqlConnection(clienteConnString))
        //        {
        //            var Query = $@"SELECT dbo.fxBeneficio_Persona_ValidaBeneIntegral('{beneficio.cedula}','{beneficio.cod_beneficio.item}',
        //                           {beneficio.monto_aplicado}, '{beneficio.registra_user}', '{beneficio.id_beneficio}')";
        //            result += connection.Query<string>(Query).FirstOrDefault();

        //            //valida que la sepelio_identificacion sea unica para la cedula
        //            if (beneficio.sepelio_identificacion != null)
        //            {
        //                Query = $@"SELECT COUNT(*) FROM AFI_BENE_OTORGA WHERE CEDULA = '{beneficio.cedula}'
        //                              AND SEPELIO_IDENTIFICACION = '{beneficio.sepelio_identificacion}' ";
        //                var sepeId = connection.Query<int>(Query).FirstOrDefault();

        //                if (sepeId == 1)
        //                {
        //                    response.Code = -1;
        //                    response.Description = " La identificación de sepelio ya fue registrada";
        //                    return response;
        //                }
        //            }

        //            var estado = $@"SELECT COD_ESTADO
        //                              FROM [dbo].[AFI_BENE_ESTADOS]
        //                              WHERE COD_ESTADO = '{estadoItem}' AND P_FINALIZA = 1 AND PROCESO = 'A'";
        //            string finaliza = connection.Query<string>(estado).FirstOrDefault();

        //            // Verifica si el estado finaliza el beneficio
        //            if (finaliza == null)
        //            {

        //                estado = $@"SELECT COD_ESTADO
        //                              FROM [dbo].[AFI_BENE_ESTADOS]
        //                              WHERE COD_ESTADO = '{estadoItem}' AND P_INICIA = 1";
        //                string inicia = connection.Query<string>(estado).FirstOrDefault();
        //            }


        //            // Verifica el resultado
        //            if (result == "1")
        //            {
        //                return response;
        //            }

        //            if (result != "")
        //            {
        //                response.Code = -1;
        //                response.Description = result;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Code = -1;
        //        response.Description = "ValidaBeneficio - " + ex.Message;
        //        response.Result = null;
        //    }
        //    return response;
        //}


        //public ErrorDto<BeneficioGeneralDatos> ValidaBeneficioActualizar(int CodCliente, BeneficioGeneralDatos beneficio)
        //{
        //    var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
        //    var response = new ErrorDto<BeneficioGeneralDatos>();

        //    try
        //    {
        //        string estadoItem = "S";
        //        if (beneficio.estado != null)
        //        {
        //            estadoItem = beneficio.estado.item;
        //        }
        //        string result = "";
        //        using (var connection = new SqlConnection(clienteConnString))
        //        {
        //            var Query = $@"SELECT dbo.fxBeneficio_Persona_ValidaBeneIntegral('{beneficio.cedula}','{beneficio.cod_beneficio.item}',
        //                           {beneficio.monto_aplicado}, '{beneficio.registra_user}')";
        //            result += connection.Query<string>(Query).FirstOrDefault();

        //            ////valida que la sepelio_identificacion sea unica para la cedula
        //            //if (beneficio.sepelio_identificacion != null)
        //            //{
        //            //    var Query = $@"SELECT COUNT(*) FROM AFI_BENE_OTORGA WHERE CEDULA = '{beneficio.cedula}'
        //            //                  AND SEPELIO_IDENTIFICACION = '{beneficio.sepelio_identificacion}' ";
        //            //    var sepeId = connection.Query<int>(Query).FirstOrDefault();

        //            //    if (sepeId == 1)
        //            //    {
        //            //        response.Code = -1;
        //            //        response.Description = " La identificación de sepelio ya fue registrada";
        //            //        return response;
        //            //    }
        //            //}

        //            var estado = $@"SELECT COD_ESTADO
        //                              FROM [dbo].[AFI_BENE_ESTADOS]
        //                              WHERE COD_ESTADO = '{estadoItem}' AND P_FINALIZA = 1";
        //            string finaliza = connection.Query<string>(estado).FirstOrDefault();

        //            // Verifica si el estado finaliza el beneficio
        //            if (finaliza == null)
        //            {

        //                estado = $@"SELECT COD_ESTADO
        //                              FROM [dbo].[AFI_BENE_ESTADOS]
        //                              WHERE COD_ESTADO = '{estadoItem}' AND P_INICIA = 1";
        //                string inicia = connection.Query<string>(estado).FirstOrDefault();
        //            }



        //            if (finaliza != null)
        //            {
        //                //Valida requisitos para beneficio integral
        //                var query = $@"SELECT 
        //                                CASE 
        //                                    WHEN COUNT(CASE WHEN R.REQUERIDO = 1 AND RR.COD_BENEFICIO IS NOT NULL THEN 1 END) = COUNT(CASE WHEN R.REQUERIDO = 1 THEN 1 END)
        //                                    THEN 0
        //                                    ELSE 1
        //                                END AS CumplenRequisito
        //                            FROM [AFI_BENE_GRUPO_REQUISITOS] GR
        //                            LEFT JOIN AFI_BENE_REQUISITOS R ON R.COD_REQUISITO = GR.COD_REQUISITO
        //                            LEFT JOIN AFI_BENE_REGISTRO_REQUISITOS RR ON RR.COD_REQUISITO = GR.COD_REQUISITO
        //                                AND RR.COD_BENEFICIO = '{beneficio.cod_beneficio.item}' 
        //                                AND RR.CONSEC = {beneficio.consec}
        //                            WHERE GR.COD_GRUPO = 
        //                                  (SELECT COD_GRUPO FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = '{beneficio.cod_beneficio.item}')";
        //                var cumpleRequisito = connection.Query<int>(query).FirstOrDefault();
        //                if (cumpleRequisito == 1)
        //                {
        //                    response.Code = 1;
        //                    response.Description = "No cumple con los requisitos del beneficio";
        //                    return response;
        //                }
        //            }


        //            // Verifica el resultado
        //            if (result == "1")
        //            {
        //                return response;
        //            }

        //            if (result != "")
        //            {
        //                response.Code = -1;
        //                response.Description = result;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Code = -1;
        //        response.Description = "ValidaBeneficio - " + ex.Message;
        //        response.Result = null;
        //    }
        //    return response;
        //}

        /// <summary>
        /// Busca el ultimo consecutivo de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public long fxConsec(int CodCliente, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            long vBeneConsec = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select isnull(Max(consec),0) as consecutivo from afi_bene_otorga where cod_beneficio = '{cod_beneficio}'";
                    vBeneConsec = connection.Query<long>(query).FirstOrDefault() + 1;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                vBeneConsec = 0;
            }
            return vBeneConsec;
        }

        /// <summary>
        /// Valida si es socio esta activo o inactivo.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<BeneficioGeneralDatos> ValidaEstadoSocio(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioGeneralDatos>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT ESTADOACTUAL FROM SOCIOS WHERE CEDULA = '{cedula}'";
                    string estado = connection.Query<string>(query).FirstOrDefault();

                    if (estado != "S")
                    {
                        response.Code = -1;
                        response.Description = "El asociado se encuentra inactivo";
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaEstadoSocio - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto ValidarPersona(int CodCliente, string cedula, string? cod_beneficio)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = "";
                    if (cod_beneficio == null)
                    {
                        query = "SELECT * FROM AFI_BENE_VALIDACIONES WHERE ESTADO = 1 AND TIPO = 'P' AND REGISTRO = 1 ORDER BY PRIORIDAD ASC";
                    }
                    else
                    {
                        query = @$"select abv.* FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                        WHERE COD_CATEGORIA = 
                                        (
	                                        SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab WHERE ab.COD_BENEFICIO = '{cod_beneficio}'
                                        ) AND c.ESTADO = 1 AND TIPO = 'P' AND REGISTRO = 1 order by abv.PRIORIDAD asc";
                    }

                    
                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {
                        query = validacion.query_val
                            .Replace("@cedula", cedula)
                          //  .Replace("@usuario", beneficio.registra_user)
                          //  .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", cod_beneficio)
                          //  .Replace("@cod_categoria", beneficio.cod_categoria)
                          //  .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                          //  .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion)
                            ;
                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            info.Code = 0;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;

            //var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            //ErrorDto info = new ErrorDto();
            //info.Code = 0;
            //info.Description = null;
            //try
            //{
            //    using (var connection = new SqlConnection(clienteConnString))
            //    {
            //        connection.Open();

            //        // Execute the SQL function and retrieve the result
            //        var result = connection.ExecuteScalar<string>("SELECT dbo.fxBeneficio_Persona_Validacion(@Cedula)", new
            //        {
            //            Cedula = cedula
            //        });

            //        if (info.Code == -1)
            //        {
            //            return info;
            //        }
            //        else
            //        {
            //            info.Code = 0;
            //            info.Description = result;
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    info.Code = -1;
            //    info.Description = $"Error al validar socio: {ex.Message}";
            //}

            //return info;
        }

        public ErrorDto ValidarPersonaPago(int CodCliente, string cedula, string? cod_beneficio)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = "";
                    if (cod_beneficio == null)
                    {
                        query = "SELECT * FROM AFI_BENE_VALIDACIONES WHERE ESTADO = 1 AND PAGO = 1 AND TIPO = 'P' ORDER BY PRIORIDAD ASC";
                    }
                    else
                    {
                        query = @$"select abv.* FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                        WHERE COD_CATEGORIA = 
                                        (
	                                        SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab WHERE ab.COD_BENEFICIO = '{cod_beneficio}'
                                        ) AND c.ESTADO = 1 AND TIPO = 'P' AND PAGO = 1 order by abv.PRIORIDAD asc";
                    }


                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {
                        query = validacion.query_val
                            .Replace("@cedula", cedula)
                            //  .Replace("@usuario", beneficio.registra_user)
                            //  .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", cod_beneficio)
                            //  .Replace("@cod_categoria", beneficio.cod_categoria)
                            //  .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                            //  .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion)
                            ;
                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            info.Code = 0;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidaRequisitos(int CodCliente, string estado, string cod_beneficio, int consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();

            try
            {
            
                string result = "";
                using (var connection = new SqlConnection(clienteConnString))
                {
                    
                    var dtEstado = $@"SELECT COD_ESTADO
                                      FROM [dbo].[AFI_BENE_ESTADOS]
                                      WHERE COD_ESTADO = '{estado}' AND P_FINALIZA = 1 AND PROCESO = 'A' ";
                    string finaliza = connection.Query<string>(dtEstado).FirstOrDefault();

                    // Verifica si el estado finaliza el beneficio
                    if (finaliza == null)
                    {

                        estado = $@"SELECT COD_ESTADO
                                      FROM [dbo].[AFI_BENE_ESTADOS]
                                      WHERE COD_ESTADO = '{estado}' AND P_INICIA = 1 AND PROCESO = 'A' ";
                        string inicia = connection.Query<string>(estado).FirstOrDefault();
                    }



                    if (finaliza != null)
                    {
                        //Valida requisitos para beneficio integral
                        var query = $@"SELECT 
                                        CASE 
                                            WHEN COUNT(CASE WHEN R.REQUERIDO = 1 AND RR.COD_BENEFICIO IS NOT NULL THEN 1 END) = COUNT(CASE WHEN R.REQUERIDO = 1 THEN 1 END)
                                            THEN 0
                                            ELSE 1
                                        END AS CumplenRequisito
                                    FROM [AFI_BENE_GRUPO_REQUISITOS] GR
                                    LEFT JOIN AFI_BENE_REQUISITOS R ON R.COD_REQUISITO = GR.COD_REQUISITO
                                    LEFT JOIN AFI_BENE_REGISTRO_REQUISITOS RR ON RR.COD_REQUISITO = GR.COD_REQUISITO
                                        AND RR.COD_BENEFICIO = '{cod_beneficio}' 
                                        AND RR.CONSEC = {consec}
                                    WHERE GR.COD_GRUPO = 
                                          (SELECT COD_GRUPO FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = '{cod_beneficio}')";
                        var cumpleRequisito = connection.Query<int>(query).FirstOrDefault();
                        if (cumpleRequisito == 1)
                        {
                            response.Code = -1;
                            response.Description = "No cumple con los requisitos del beneficio";
                            return response;
                        }
                    }


                    // Verifica el resultado
                    if (result == "1")
                    {
                        return response;
                    }

                    if (result != "")
                    {
                        response.Code = -1;
                        response.Description = result;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaBeneficio - " + ex.Message;
            }
            return response;
        }
    
        public ErrorDto ValidaFallecido(int CodCliente, string cedulafallecido)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();
            response.Code = 0;
            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = $@"SELECT CONCAT(O.ID_BENEFICIO, TRIM(O.COD_BENEFICIO), FORMAT(O.CONSEC,'00000'), '- cédula: ', O.CEDULA) 
                                         FROM AFI_BENE_OTORGA O WHERE SEPELIO_IDENTIFICACION = '{cedulafallecido}' ";
                    var fallecido = connection.Query(query).ToList();

                    if (fallecido.Count > 0)
                    {
                        string otrosRegostros = "";
                        foreach (var item in fallecido)
                        {
                            otrosRegostros += item + " - ";
                        }

                        response.Code = -1;
                        response.Description = "La cédula del fallecido se encuentra en los siguientes expedientes: " + otrosRegostros.Replace("DapperRow,  = ", "");
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "ValidaFallecido - " + ex.Message;
            }
            return response;
        }

        public ErrorDto ValidarBeneficioDato(int CodCliente, BeneficioGeneralDatos beneficio)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = @$"select abv.* FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                WHERE COD_CATEGORIA = 
                                (
	                                SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab 
                                    WHERE ab.COD_BENEFICIO = '{beneficio.cod_beneficio.item}'
                                ) AND c.ESTADO = 1 AND TIPO = 'G' AND REGISTRO = 1 order by abv.PRIORIDAD asc";
                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {
                        query = validacion.query_val
                            .Replace("@cedula", beneficio.cedula)
                            .Replace("@usuario", beneficio.registra_user)
                            .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", beneficio.cod_beneficio.item.ToString())
                            .Replace("@cod_categoria", beneficio.cod_categoria)
                            .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                            .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion);

                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            info.Code = -1;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidarBeneficioPagoDato(int CodCliente, BeneficioGeneralDatos beneficio)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = @$"select abv.* FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                WHERE COD_CATEGORIA = 
                                (
	                                SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab 
                                    WHERE ab.COD_BENEFICIO = '{beneficio.cod_beneficio.item}'
                                ) AND c.ESTADO = 1 AND TIPO = 'G' AND PAGO = 1 order by abv.PRIORIDAD asc";
                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {
                        query = validacion.query_val
                            .Replace("@cedula", beneficio.cedula)
                            .Replace("@usuario", beneficio.registra_user)
                            .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", beneficio.cod_beneficio.item.ToString())
                            .Replace("@cod_categoria", beneficio.cod_categoria)
                            .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                            .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion);

                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            info.Code = -1;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidarBeneficioJustificaDato(int CodCliente, BeneficioGeneralDatos beneficio, bool justifica)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                int justificadas = 0;
                int obligatorias = 0;

                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = @$"select abv.*, c.registro_justifica FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                WHERE COD_CATEGORIA = 
                                    (
	                                    SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab 
                                        WHERE ab.COD_BENEFICIO = '{beneficio.cod_beneficio.item}'
                                    ) AND c.ESTADO = 1 AND REGISTRO = 1 AND TIPO != 'G' order by abv.PRIORIDAD asc";
                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {

                        query = validacion.query_val
                            .Replace("@cedula", beneficio.cedula)
                            .Replace("@usuario", beneficio.registra_user)
                            .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", beneficio.cod_beneficio.item.ToString())
                            .Replace("@cod_categoria", beneficio.cod_categoria)
                            .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                            .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion);

                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            obligatorias++;
                            if (validacion.registro_justifica)
                            {
                                justificadas++;
                            }

                            info.Code = 0;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                    if (justificadas > 0 && !justifica)
                    {
                        info.Code = -1;
                    }
                    else if (justificadas > 0 && justifica)
                    {
                        info.Code = 0;
                    }

                    if (obligatorias > 0 && info.Description.Length > 0)
                    {
                        int activa = obligatorias - justificadas;

                        if(activa > 0)
                        {
                            info.Code = -1;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidarBeneficioPagoJustificaDato(int CodCliente, BeneficioGeneralDatos beneficio, bool justifica)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                int justificadas = 0;
                int obligatorias = 0;

                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = @$"select abv.*, c.pago_justifica FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                WHERE COD_CATEGORIA = 
                                    (
	                                    SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab 
                                        WHERE ab.COD_BENEFICIO = '{beneficio.cod_beneficio.item}'
                                    ) AND c.ESTADO = 1 AND PAGO = 1 AND TIPO != 'G' order by abv.PRIORIDAD asc";
                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {

                        query = validacion.query_val
                            .Replace("@cedula", beneficio.cedula)
                            .Replace("@usuario", beneficio.registra_user)
                            .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", beneficio.cod_beneficio.item.ToString())
                            .Replace("@cod_categoria", beneficio.cod_categoria)
                            .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                            .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion);

                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            obligatorias++;
                            if (validacion.pago_justifica)
                            {
                                justificadas++;
                            }

                            info.Code = 0;
                            info.Description += validacion.msj_val + "...\n";
                        }
                    }

                    if (justificadas > 0 && !justifica)
                    {
                        info.Code = -1;
                    }
                    else if (justificadas > 0 && justifica)
                    {
                        info.Code = 0;
                    }

                    if (obligatorias > 0 && info.Description.Length > 0)
                    {
                        int activa = obligatorias - justificadas;

                        if (activa > 0)
                        {
                            info.Code = -1;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }

        public ErrorDto ValidaCargaPagos(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                int justificadas = 0;
                int obligatorias = 0;

                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = @$"select abv.*, c.pago_justifica FROM AFI_BENE_VALIDA_CATEGORIA c left join AFI_BENE_VALIDACIONES abv ON abv.COD_VAL = c.COD_VAL
                                WHERE COD_CATEGORIA = 
                                    (
	                                    SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab 
                                        WHERE ab.COD_BENEFICIO = '{beneficio.cod_beneficio.item}'
                                    ) AND c.ESTADO = 1 AND PAGO = 1 AND TIPO != 'G' order by abv.PRIORIDAD asc";
                    var validaciones = connection.Query<AfiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {

                        query = validacion.query_val
                            .Replace("@cedula", beneficio.cedula)
                            .Replace("@usuario", beneficio.registra_user)
                            .Replace("@id_beneficio", beneficio.id_beneficio.ToString())
                            .Replace("@cod_beneficio", beneficio.cod_beneficio.item.ToString())
                            .Replace("@cod_categoria", beneficio.cod_categoria)
                            .Replace("@monto_usuario", beneficio.monto_aplicado.ToString())
                            .Replace("@sepelio_identificacion", beneficio.sepelio_identificacion);

                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            if (validacion.pago_justifica)
                            {
                                info.Description += " ** " + validacion.msj_val + " ** ...\n";
                            }
                            else
                            {
                                info.Description += validacion.msj_val + "...\n";
                            }

                            info.Code = 0;
                            
                        }
                    }

                    //if (justificadas > 0 && !justifica)
                    //{
                    //    info.Code = -1;
                    //}
                    //else if (justificadas > 0 && justifica)
                    //{
                    //    info.Code = 0;
                    //}

                    //if (obligatorias > 0 && info.Description.Length > 0)
                    //{
                    //    int activa = obligatorias - justificadas;

                    //    if (activa > 0)
                    //    {
                    //        info.Code = -1;
                    //    }
                    //}

                }


            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = $"Error al validar socio: {ex.Message}";
            }

            return info;
        }
    
    }
}
