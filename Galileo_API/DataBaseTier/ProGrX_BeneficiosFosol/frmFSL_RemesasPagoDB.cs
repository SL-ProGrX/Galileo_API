using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.FSL;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;


namespace PgxAPI.DataBaseTier
{
    public class frmFSL_RemesasPagoDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;

        public frmFSL_RemesasPagoDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto<List<FslRemesasListaDatos>> FslFechas_Obtener(int CodEmpresa, int cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FslRemesasListaDatos>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select fecha_inicio,fecha_corte from FSL_REMESAS_TESORERIA where TESORERIA_REMESA = {cod_remesa}";

                    response.Result = connection.Query<FslRemesasListaDatos>(query).ToList();
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

        #region REMESAS
        //REMESAS FOSOL

        public ErrorDto<FslRemesasLista> FslRemesas_Obtener(int CodEmpresa, string? filtro, int? pagina, int? paginacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FslRemesasLista>();
            response.Result = new FslRemesasLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ", vfiltro = " ";
                if (filtro != null)
                {
                    vfiltro = " WHERE TESORERIA_REMESA LIKE '%" + filtro + "%' OR REGISTRO_USUARIO LIKE '%" + filtro + "%' " +
                        "OR REGISTRO_FECHA LIKE '%" + filtro + "%' OR notas LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = " OFFSET " + pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $"SELECT COUNT(*) FROM FSL_REMESAS_TESORERIA {vfiltro}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select * from FSL_REMESAS_TESORERIA {vfiltro} order by registro_fecha desc  {paginaActual} {paginacionActual}";

                    response.Result.Lista = connection.Query<FslRemesasListaDatos>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Lista = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto FslRemesa_Agregar(int CodEmpresa, FslRemesaInsertar remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select coalesce(max(TESORERIA_REMESA),0) + 1 as Ultimo from FSL_REMESAS_TESORERIA";

                    int consecutivo = connection.Query<int>(query).FirstOrDefault();

                    query = $@"insert FSL_REMESAS_TESORERIA(
                            TESORERIA_REMESA,
                                registro_usuario,
                                registro_fecha,
                                estado,
                                fecha_inicio,
                                fecha_corte,notas
                                ) 
                            values( {consecutivo}
						   ,'{remesa.usuario}', getdate(),'A','{remesa.fecha_inicio}','{remesa.fecha_corte}','{remesa.notas}')";

                    connection.Execute(query);
                    info.Description = "Remesa agregada correctamente";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto FslRemesa_Actualizar(int CodEmpresa, FslRemesaInsertar remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            if (remesa.cod_remesa == 0)
            {
                info = FslRemesa_Agregar(CodEmpresa, remesa);
                return info;
            }

            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"update FSL_REMESAS_TESORERIA set 
                                fecha_inicio = '{remesa.fecha_inicio}',
                                fecha_corte = '{remesa.fecha_corte}',
                                notas = '{remesa.notas}'
                                where TESORERIA_REMESA = {remesa.cod_remesa}";

                    connection.Execute(query);
                    info.Description = "Remesa actualizada correctamente";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto FslRemesa_Eliminar(int CodEmpresa, int cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"delete from FSL_REMESAS_TESORERIA where TESORERIA_REMESA = {cod_remesa}";

                    connection.Execute(query);
                    info.Description = "Remesa eliminada correctamente";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto FslRemesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select count(*) as Existe from FSL_REMESAS_TESORERIA
		                           where TESORERIA_REMESA = {cod_remesa}
		                            and estado = 'A'";

                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe == 0)
                    {
                        info.Code = -1;
                        info.Description = "La Remesa actual; ya se encuentra cerrada...";

                    }
                    else
                    {
                        query = $@"update FSL_REMESAS_TESORERIA set 
                                estado = 'C'
                                where TESORERIA_REMESA = {cod_remesa}";

                        connection.Execute(query);
                        info.Description = "Remesa cerrada correctamente";

                        //Incluyo BITACORA
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario.ToUpper(),
                            DetalleMovimiento = "Cierra Remesa Traslado a Tesoreria :" + cod_remesa,
                            Movimiento = "APLICA - WEB",
                            Modulo = 7
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        #region Carga

        public ErrorDto<List<FslRemesasListaDatos>> FslCargas_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FslRemesasListaDatos>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select *, CONCAT(TESORERIA_REMESA, REGISTRO_USUARIO, REGISTRO_FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION  
                        from FSL_REMESAS_TESORERIA where estado = 'A' order by registro_fecha desc";

                    response.Result = connection.Query<FslRemesasListaDatos>(query).ToList();
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

        public ErrorDto<FslCargasLista> FslCargasLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, string? filtro, int? pagina, int? paginacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FslCargasLista>();
            response.Result = new FslCargasLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ", vfiltro = " ";
                if (filtro != null)
                {
                    vfiltro = " AND E.COD_EXPEDIENTE LIKE '%" + filtro + "%' OR E.CEDULA LIKE '%" + filtro + "%' " +
                        "OR S.NOMBRE LIKE '%" + filtro + "%' OR E.PRESENTA_NOMBRE LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = " OFFSET " + pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"SELECT COUNT(E.COD_EXPEDIENTE) FROM FSL_EXPEDIENTES E inner join SOCIOS S on E.CEDULA = S.CEDULA
			                     Where E.RESOLUCION_FECHA between '{fecha_inicio}'
			                     and '{fecha_corte}' and E.TESORERIA_REMESA is null 
			                     and E.Tipo_Desembolso = 'T' and E.Estado = 'X' and E.TOTAL_SOBRANTE > 0 
			                      {vfiltro}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"Select E.COD_EXPEDIENTE,E.CEDULA,S.NOMBRE, E.TOTAL_SOBRANTE, E.PRESENTA_CEDULA, E.PRESENTA_NOMBRE
			                     from FSL_EXPEDIENTES E inner join SOCIOS S on E.CEDULA = S.CEDULA
			                     Where E.RESOLUCION_FECHA between '{fecha_inicio}'
			                     and '{fecha_corte}' and E.TESORERIA_REMESA is null 
			                     and E.Tipo_Desembolso = 'T' and E.Estado = 'X' and E.TOTAL_SOBRANTE > 0 
			                      {vfiltro} order by E.CEDULA,S.NOMBRE {paginaActual} {paginacionActual}";

                    response.Result.Lista = connection.Query<FslCargasListaData>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Lista = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto FslCargas_Aplicar(int CodEmpresa, string cargas)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FslCargasAplicar carga = JsonConvert.DeserializeObject<FslCargasAplicar>(cargas) ?? new FslCargasAplicar();
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select count(*) as Existe from FSL_REMESAS_TESORERIA
                                where TESORERIA_REMESA = {carga.cod_remesa}
                                and estado = 'A'";

                    int existe = connection.Query<int>(query).FirstOrDefault();
                    if (existe == 0)
                    {
                        info.Code = -1;
                        info.Description = "La Remesa actual; ya se encuentra cerrada...";
                    }

                    //calcila casos a procesas
                    int vCasos = carga.casos.Count;

                    foreach (var item in carga.casos)
                    {
                        query = $@"update FSL_EXPEDIENTES set 
                                TESORERIA_REMESA = {carga.cod_remesa}
                                where COD_EXPEDIENTE = {item.cod_expediente}";

                        connection.Execute(query);
                    }

                    //Agrego bitacora
                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = carga.usuario.ToUpper(),
                        DetalleMovimiento = "update FSL_EXPEDIENTES set :" + carga.cod_remesa,
                        Movimiento = "APLICA - WEB",
                        Modulo = 7
                    });

                    info.Description = "Proceso Realizado Satisfactoriamente...";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }

        public ErrorDto FslCargas_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select count(*) as Existe from FSL_REMESAS_TESORERIA
                                   where TESORERIA_REMESA = {cod_remesa}
                                    and estado = 'A'";

                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe == 0)
                    {
                        info.Code = -1;
                        info.Description = "La Remesa actual; ya se encuentra cerrada...";

                    }
                    else
                    {
                        query = $@"update FSL_REMESAS_TESORERIA set 
                                estado = 'C'
                                where TESORERIA_REMESA = {cod_remesa}";

                        connection.Execute(query);
                        info.Description = "Remesa cerrada correctamente";

                        //Incluyo BITACORA
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario.ToUpper(),
                            DetalleMovimiento = "Cierra Remesa Traslado a Tesoreria :" + cod_remesa,
                            Movimiento = "APLICA - WEB",
                            Modulo = 7
                        });
                    }

                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        #endregion

        #region Traslados

        public ErrorDto<List<FslRemesasListaDatos>> FslTraslados_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FslRemesasListaDatos>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select *, CONCAT(TESORERIA_REMESA, REGISTRO_USUARIO, REGISTRO_FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION 
                        from FSL_REMESAS_TESORERIA where estado = 'C' order by REGISTRO_fecha desc";

                    response.Result = connection.Query<FslRemesasListaDatos>(query).ToList();

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

        public ErrorDto<List<FslTrasladoListaData>> FslTrasladoLista_Obtener(int CodEmpresa, string fecha_inicio, string fecha_corte, int cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FslTrasladoListaData>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"Select E.COD_EXPEDIENTE,E.CEDULA,S.NOMBRE, E.TOTAL_SOBRANTE, E.PRESENTA_CEDULA, E.PRESENTA_NOMBRE
			                     from FSL_EXPEDIENTES E inner join SOCIOS S on E.CEDULA = S.CEDULA
			                     Where E.RESOLUCION_FECHA between '{fecha_inicio}'
			                     and '{fecha_corte}' and E.TESORERIA_REMESA = {cod_remesa}
			                     and E.Tipo_Desembolso = 'T' and E.Estado = 'X' and E.TOTAL_SOBRANTE > 0 and isnull(E.Tesoreria_Solicitud,0) = 0
			                     order by E.CEDULA,S.NOMBRE";

                    response.Result = connection.Query<FslTrasladoListaData>(query).ToList();
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

        public ErrorDto FslTraslado_Aplicar(int CodEmpresa, string traslados)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            FslTrasladoAplicar traslado = JsonConvert.DeserializeObject<FslTrasladoAplicar>(traslados) ?? new FslTrasladoAplicar();
            mBeneficiosDB _mBene = new mBeneficiosDB(_config);

            info.Code = 0;
            try
            {
                //Variables
                int vCasos = 0;
                DateTime vFecha = DateTime.Now;
                string vCuenta = _mBene.fxFSL_Parametros(CodEmpresa, "01");
                string mConcepto = _mBene.fxFSL_Parametros(CodEmpresa, "05");
                string mUnidad = _mBene.fxFSL_Parametros(CodEmpresa, "07");
                string vToken = "", vTipo = "", vCuentaAhorros = "", vBanco = "";
                var query = "";
                long lngSolicitud = 0;
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";

                    vToken = connection.Query<string>(query).FirstOrDefault();

                    foreach (var item in traslado.casos)
                    {
                        query = $@"select Top 1 * from cuentas_Ahorros where Tipo =  1 and cedula =  '{item.cedula}'  order by Prioridad ";
                        FslCuentaAhorrosDatos dtCuentaHorros = new FslCuentaAhorrosDatos();
                        dtCuentaHorros = connection.Query<FslCuentaAhorrosDatos>(query).FirstOrDefault();


                        //Tabla cuentas_Ahorros actual no cuenta con los campos BOF y EOF que valida en V6 
                        if (dtCuentaHorros != null)
                        {
                            vTipo = "TE";
                            vCuentaAhorros = dtCuentaHorros.cuenta;
                            vBanco = dtCuentaHorros.id_banco;
                        }
                        else
                        {
                            vTipo = "CK";
                            vCuentaAhorros = "";
                            vBanco = _mBene.fxFSL_Parametros(CodEmpresa, "04");
                        }

                        lngSolicitud = fxMaestroTesoreria(CodEmpresa, vTipo, int.Parse(vBanco), item.total_sobrante, item.cod_expediente
                            , item.nombre, 0, "FOSOL", 0, "Exp.: " + item.cod_expediente
                            , vCuentaAhorros, vFecha, mUnidad, vToken, traslado.usuario, traslado.codTraslado, mConcepto);

                        //Mata el Pasivo de la Nota de Debito de la Formalizacion contra Tes_Bancos
                        sbCreaDetalle(CodEmpresa, lngSolicitud, fxTraeCuentaBanco(CodEmpresa, vBanco), item.total_sobrante, "H", 1, mUnidad);
                        sbCreaDetalle(CodEmpresa, lngSolicitud, vCuenta, item.total_sobrante, "D", 1, mUnidad);

                        //Actualiza Campo Tesoreria
                        query = $@"update FSL_EXPEDIENTES set Tesoreria_Solicitud = {lngSolicitud} , Tesoreria_Fecha = getdate()
			                               , Tesoreria_Usuario = '{traslado.usuario}'
			                                where TESORERIA_REMESA = {traslado.codTraslado}
			                                 and cod_expediente = {item.cod_expediente}";

                        connection.Execute(query);

                        vCasos = vCasos + 1;

                        //Bitacora
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = traslado.usuario.ToUpper(),
                            DetalleMovimiento = "Traspaso a Tesoreria - Expediente :" + item.cod_expediente,
                            Movimiento = "Registra - WEB",
                            Modulo = 7
                        });

                    }

                    if (vCasos > 0)
                    {
                        //Bitacora
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = traslado.usuario.ToUpper(),
                            DetalleMovimiento = "Carga Remesa Traslado a Tesoreria :" + traslado.codTraslado,
                            Movimiento = "Aplica - WEB",
                            Modulo = 7
                        });

                        query = $@"update FSL_REMESAS_TESORERIA set 
                                estado = 'T'
                                where TESORERIA_REMESA = {traslado.codTraslado}";

                        connection.Execute(query);
                    }

                    info.Description = "Traslado a Tesoreria realizado satisfactoriamente...";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        private long fxMaestroTesoreria(int CodEmpresa, string vTipoDocumento, int vBanco, float vMonto, string vCodigo
                              , string vBeneficiario, long vOP, string vDetalle1, long vReferencia
                              , string vDetalle2, string vCuenta, DateTime vFecha, string vUnidad
                              , string vToken, string usuario, long codTraslado, string mConcepto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            long resp = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string sql = "";
                    if (vTipoDocumento == "CK")
                    {
                        sql = $@" 'S','{usuario}',getdate(),'{vToken}','FSL', {codTraslado} ";
                    }
                    else
                    {
                        sql = $@" 'N',null,null,'{vToken}','FSL', {codTraslado} ";
                    }

                    var query = $@"insert Tes_Transacciones(
                                        cod_concepto,
                                        cod_unidad,
                                        id_banco,
                                        tipo,
                                        codigo,
                                        beneficiario,
                                        monto,
                                        fecha_solicitud,
                                        estado,
                                        estadoi,
                                        modulo,
                                        submodulo,
                                        cta_ahorros,
                                        detalle1,
                                        detalle2,
                                        referencia,
                                        op,
                                        genera,
                                        actualiza,
                                        user_solicita,
                                        autoriza,
                                        user_autoriza,
                                        fecha_autorizacion,
                                        ID_TOKEN ,
                                        REMESA_TIPO, 
                                        REMESA_ID
                                        )values(
                                        '{mConcepto}',
                                        '{vUnidad}',
                                        {vBanco},
                                        '{vTipoDocumento}',
                                        '{vCodigo}',
                                        '{vBeneficiario}',
                                        {vMonto},
                                        '{vFecha}',
                                        'P',
                                        'P',
                                        'CC',
                                        'C',
                                        '{vCuenta}',
                                        '{vDetalle1}',
                                        '{vDetalle2}',
                                        {vReferencia},
                                        {vOP},
                                        'S',
                                        'S',
                                        '{usuario}', {sql} )";

                    connection.Execute(query);

                    query = "select max(nsolicitud) as Solicitud from Tes_Transacciones";
                    long vSolicitud = connection.Query<long>(query).FirstOrDefault();

                    query = $@"select* from Tes_Transacciones where nsolicitud = {vSolicitud}";
                    FslTesTransaccionesData info = connection.Query<FslTesTransaccionesData>(query).FirstOrDefault();
                    if (info.codigo == vCodigo.Trim())
                    {
                        resp = info.nsolicitud;
                    }

                    if (resp == 0)
                    {
                        query = $@"select max(nsolicitud) as Solicitud from Tes_Transacciones where codigo = '{vCodigo}' ";
                        resp = connection.Query<long>(query).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = 0;
            }
            return resp;
        }

        private void sbCreaDetalle(int CodEmpresa, long vSolicitud, string vCtaConta, float vMonto, string vDH, int vLinea, string vUnidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert Tes_Trans_Asiento(
                                        nsolicitud,cuenta_contable,monto,debehaber,linea,cod_unidad
                                        )values(
                                        {vSolicitud},'{vCtaConta.Trim()}',{vMonto}, '{vDH}', {vLinea}, '{vUnidad}' )";

                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private string fxTraeCuentaBanco(int CodEmpresa, string vBanco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string resp = "0";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ctaconta from tes_bancos where id_banco = {vBanco}";

                    resp = connection.Query<string>(query).FirstOrDefault();

                    if (resp != null)
                    {
                        return resp;
                    }

                }
            }
            catch (Exception ex)
            {
                resp = ex.Message;
            }
            return resp;
        }

        #endregion


    }
}