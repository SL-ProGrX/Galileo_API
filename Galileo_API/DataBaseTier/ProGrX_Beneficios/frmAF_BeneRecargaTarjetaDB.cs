using PgxAPI.Models;
using PgxAPI.Models.AF;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneRecargaTarjetaDB
    {
        private readonly IConfiguration? _config;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private mProGrX_AuxiliarDB mAuxiliarDB;
        mSecurityMainDb DBBitacora;
        mTESFuncionesDB mTESFuncionesDB;
        mBeneficiosDB mBeneficiosDB;
        public string sendEmail = "";

        public frmAF_BeneRecargaTarjetaDB(IConfiguration config)
        {
            _config = config;
            _envioCorreoDB = new EnvioCorreoDB(_config);
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
            DBBitacora = new mSecurityMainDb(_config);
            mTESFuncionesDB = new mTESFuncionesDB(_config);
            mBeneficiosDB = new mBeneficiosDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        #region Remesas
        public ErrorDTO<AfiBeneTarjetasRemesasDataLista> AfiTajertasRemesas_Obtener(int CodCliente, string? filtro, int? pagina, int? paginacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneTarjetasRemesasDataLista>
            {
                Code = 0,
                Result = new AfiBeneTarjetasRemesasDataLista()
            };
            string paginaActual = " ", paginacionActual = " ", vfiltro = " ";
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = "SELECT COUNT(*) FROM AFI_BENE_TARJETAS_REMESAS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        vfiltro = " WHERE cod_remesa_tr LIKE '%" + filtro + "%' OR registro_usuario LIKE '%" + filtro + "%' " +
                            "OR registro_fecha LIKE '%" + filtro + "%' OR estado LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select * from AFI_BENE_TARJETAS_REMESAS {vfiltro} order by registro_fecha desc {paginaActual} {paginacionActual}";

                    response.Result.Beneficios = connection.Query<AfiBeneTarjetasRemesasData>(query).ToList();

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

        public ErrorDTO<AfiBeneTarjetasRemesasData> AfiTarjetasRemesa_Obtener(int CodCliente, int cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneTarjetasRemesasData>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select * from AFI_BENE_TARJETAS_REMESAS where cod_remesa_tr = {cod_remesa}";

                    response.Result = connection.Query<AfiBeneTarjetasRemesasData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDTO AfiTarjetasRemesa_Insertar(int CodCliente, AfiBeneTarjetasRemesasData remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    string fecha_inicio = mAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio);
                    string fecha_corte = mAuxiliarDB.validaFechaGlobal(remesa.fecha_corte);

                    query = $@"insert into AFI_BENE_TARJETAS_REMESAS (registro_usuario,registro_fecha,estado,fecha_inicio,fecha_corte,notas)
                                values ('{remesa.registro_usuario}', Getdate(),'A', '{fecha_inicio}', '{fecha_corte}', '{remesa.notas}' )";

                    var resp = connection.Execute(query);

                    if (resp > 0)
                    {
                        //Bitacora(new BitacoraInsertarDTO
                        //{
                        //    EmpresaId = CodCliente,
                        //    Usuario = remesa.registro_usuario.ToUpper(),
                        //    DetalleMovimiento = $"Registra, Remesa de Beneficios Traslado a Tesoreria: {remesa.cod_remesa} ",
                        //    Movimiento = "REGISTRA - WEB",
                        //    Modulo = 7
                        //});
                    }
                    else
                    {
                        info.Code = -1;
                        info.Description = "Error al actualizar el registro";
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

        public ErrorDTO AfiTarjetasRemesa_Actualizar(int CodCliente, AfiBeneTarjetasRemesasData remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    DateTime fecha_inicioActual = (DateTime)remesa.fecha_inicio;
                    string fecha_inicio = mAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio);

                    DateTime fecha_corteActual = (DateTime)remesa.fecha_corte;
                    string fecha_corte = mAuxiliarDB.validaFechaGlobal(remesa.fecha_corte);

                    query = $@"update AFI_BENE_TARJETAS_REMESAS set estado = '{remesa.estado}', 
                        fecha_inicio = '{fecha_inicio}', fecha_corte = '{fecha_corte}', notas = '{remesa.notas}' where cod_remesa_tr = {remesa.cod_remesa_tr}";

                    var resp = connection.Execute(query);

                    if (resp > 0)
                    {
                        //Bitacora(new BitacoraInsertarDTO
                        //{
                        //    EmpresaId = CodCliente,
                        //    Usuario = remesa.registro_usuario.ToUpper(),
                        //    DetalleMovimiento = $"Modifica, Remesa de Beneficios Traslado a Tesoreria: {remesa.cod_remesa} ",
                        //    Movimiento = "Modifica - WEB",
                        //    Modulo = 7
                        //});
                    }
                    else
                    {
                        info.Code = -1;
                        info.Description = "Error al actualizar el registro";
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

        public ErrorDTO AfiTarjetasRemesa_Eliminar(int CodCliente, long cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "update afi_bene_pago set COD_REMESA = NULL where TIPO = 'P' AND COD_PRODUCTO IN(select cod_producto from AFI_BENE_TARJETAS_REGALO ) AND COD_REMESA = " + cod_remesa;
                    var resp = connection.Execute(query);

                    query = "delete from AFI_BENE_TARJETAS_REMESAS where COD_REMESA_TR = " + cod_remesa;
                    resp = connection.Execute(query);

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

        #region Tarjetas Regalo

        public ErrorDTO<List<AfiBeneTarjetasRemesasData>> AfiTarjetasRemesasAbiertas_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTarjetasRemesasData>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select *, CONCAT(COD_REMESA_TR, REGISTRO_USUARIO, REGISTRO_FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION 
                    from AFI_BENE_TARJETAS_REMESAS where estado = 'A' order by REGISTRO_FECHA desc";

                    response.Result = connection.Query<AfiBeneTarjetasRemesasData>(query).ToList();
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

        public ErrorDTO<AfiBeneTarjetasDataLista> AfiTarjetasRegalo_Obtener(int CodCliente, string filtros, string estado, bool? sinAsignar)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            AfiTarjetasFiltros infoFiltros = JsonConvert.DeserializeObject<AfiTarjetasFiltros>(filtros);
            var response = new ErrorDTO<AfiBeneTarjetasDataLista>();
            response.Code = 0;
            response.Result = new AfiBeneTarjetasDataLista();
            string paginaActual = " ", paginacionActual = " ", vfiltro = " ";
            try
            {
                if (infoFiltros.vfiltro != null && infoFiltros.vfiltro.Trim() != "")
                {
                    vfiltro = " AND cod_remesa_tr LIKE '%" + vfiltro + "%' OR registro_usuario LIKE '%" + vfiltro + "%' " +
                        "OR registro_fecha LIKE '%" + vfiltro + "%' OR estado LIKE '%" + vfiltro + "%' ";
                }

                if (infoFiltros.pagina != null)
                {
                    paginaActual = " OFFSET " + infoFiltros.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + infoFiltros.paginacion + " ROWS ONLY ";
                }

                if (sinAsignar != null && sinAsignar == true)
                {
                    vfiltro += " AND T.ID_PAGO IS NULL ";
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    string fechaInicioStr = mAuxiliarDB.validaFechaGlobal(infoFiltros.fecha_inicio);
                    DateTime.TryParse(fechaInicioStr, out DateTime fechaInicio);
                    string fecha_inicio = fechaInicio.ToString("yyyy-MM-dd");

                    string fechaCorteStr = mAuxiliarDB.validaFechaGlobal(infoFiltros.fecha_corte);
                    DateTime.TryParse(fechaCorteStr, out DateTime fechaCorte);
                    string fecha_corte = fechaCorte.ToString("yyyy-MM-dd");

                    if (infoFiltros.fecha_inicio != null)
                    {
                        vfiltro += $@" AND T.registro_fecha between '{fecha_inicio}T00:00:00' and '{fecha_corte}T11:59:59' ";
                    }

                    var query = $@"select COUNT(*) from AFI_BENE_TARJETAS_REGALO T where T.ESTADO = 'P' {vfiltro}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select T.*, (select NOMBRE from socios where CEDULA = T.cedula) as NOMBRE, 
                    (select DESCRIPCION from AFI_BENEFICIOS where COD_BENEFICIO = T.COD_BENEFICIO) AS BENEFICIO_DESC 
                    from AFI_BENE_TARJETAS_REGALO T 
                    where T.ESTADO = '{estado}' {vfiltro} 
                    order by T.registro_fecha desc {paginaActual} {paginacionActual} ";
                    response.Result.Tarjetas = connection.Query<AfiBeneTarjetasData>(query).ToList();

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

        public ErrorDTO AfiTarjetasRegalo_Insertar(int CodCliente, string tarjetas)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO response = new()
            {
                Code = 0
            };
            AfiBeneTarjetasData item = JsonConvert.DeserializeObject<AfiBeneTarjetasData>(tarjetas);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert AFI_BENE_TARJETAS_REGALO
                                    (COD_PRODUCTO,REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, ESTADO, NO_TARJETA, MONTO )
                                 	values('{item.cod_producto}', getDate(), '{item.registro_usuario}', 
                                    '{item.cod_beneficio}', 'P', '{item.no_tarjeta}', {item.monto})";

                    response.Code = connection.Execute(query);
                    response.Description = "Tarjeta registrada correctamente";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;

        }

        public ErrorDTO AfiTarjetasRegalo_Actualizar(int CodCliente, string tarjetas)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO response = new()
            {
                Code = 0
            };
            AfiBeneTarjetasData item = JsonConvert.DeserializeObject<AfiBeneTarjetasData>(tarjetas);
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update AFI_BENE_TARJETAS_REGALO set COD_PRODUCTO = '{item.cod_producto}', 
                    COD_BENEFICIO = '{item.cod_beneficio}', NO_TARJETA = '{item.no_tarjeta}', MONTO = {item.monto} 
                    WHERE ID_TR = {item.id_tr}";

                    response.Code = connection.Execute(query);
                    response.Description = "Tarjeta actualizada correctamente";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;

        }

        public ErrorDTO AfiTarjetasRegalo_Eliminar(int CodCliente, int id_tr)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO response = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from AFI_BENE_TARJETAS_REGALO WHERE ID_TR = {id_tr}";

                    response.Code = connection.Execute(query);
                    response.Description = "Tarjeta eliminada correctamente";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;

        }

        public ErrorDTO<List<ProductoData>> AfiTarjetasProductos_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<ProductoData>>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select * from AFI_BENE_PRODUCTOS where tarjeta_regalo = 1";

                    response.Result = connection.Query<ProductoData>(query).ToList();
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
        #endregion

        #region Recargar Tarjetas Regalo

        public ErrorDTO AfiTarjetasRegalo_Recargar(int CodCliente, string tarjetas)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO response = new()
            {
                Code = 0
            };
            AfiBeneTarjetasRecargaData infoTarjetas = JsonConvert.DeserializeObject<AfiBeneTarjetasRecargaData>(tarjetas);
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select top 1 id_token from tes_tokens where estado = 'A' order by registro_fecha";
                    var existe = connection.Query<string>(query).FirstOrDefault();
                    string vToken;
                    if (existe == null)
                    {
                        vToken = mTESFuncionesDB.fxTesToken(CodCliente, infoTarjetas.usuario);
                    }
                    else
                    {
                        vToken = existe;
                    }

                    foreach (AfiBeneTarjetasData item in infoTarjetas.tarjetas)
                    {
                        query = $@"select descripcion,cod_cuenta  from afi_beneficios where cod_beneficio =  '{item.cod_beneficio}' ";

                        AfiBeneficiosTraslado beneficio = connection.Query<AfiBeneficiosTraslado>(query).FirstOrDefault();
                        string vCtaBene = beneficio.cod_cuenta;
                        string vDetalle = item.cod_beneficio;
                        string vDetalle2 = beneficio.descripcion;

                        query = $@"select COD_PROVEEDOR, TIPO_PAGO, COD_BANCO, CEDJUR, DESCRIPCION, 
                        COD_CUENTA AS CUENTA 
                        from CXP_PROVEEDORES P 
                        where COD_PROVEEDOR = (select TOP 1 COD_PROVEEDOR from pv_producto_prov 
                        where COD_PRODUCTO = (select TOP 1 COD_PRODUCTO_INV from AFI_BENE_PRODUCTOS where COD_PRODUCTO = '{item.cod_producto}'))";
                        AfiBeneProveedorData proveedor = connection.Query<AfiBeneProveedorData>(query).FirstOrDefault();

                        var vTesoreria = mTESFuncionesDB.fxgTesoreriaMaestro(CodCliente, infoTarjetas.usuario, new TesoreriaMaestroModel
                        {

                            vTipoDocumento = proveedor.tipo_pago,
                            vBanco = proveedor.cod_banco,
                            vMonto = item.monto,
                            vBeneficiario = proveedor.descripcion,
                            vCodigo = proveedor.cedjur,
                            vOP = 0,
                            vDetalle1 = vDetalle,
                            vReferencia = 0,
                            vDetalle2 = vDetalle2,
                            vCuenta = proveedor.cuenta,
                            vFecha = DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/" + DateTime.Now.Day,
                            vRemesa = Convert.ToInt32(infoTarjetas.cod_remesa_tr),

                        });

                        if(item.id_pago != null) { 
                            //Actualiza estado en afi_bene_pago
                            query = $@"Update afi_bene_pago set tesoreria = {vTesoreria},
					                      tes_supervision_usuario = '{infoTarjetas.usuario}', tes_supervision_fecha = Getdate()
					                      ,ID_TOKEN = '{vToken}', justificacion = 'Recarga de tarjeta', cod_remesa = {infoTarjetas.cod_remesa_tr} 
					                       where cedula = '{item.cedula}' and id_pago = {item.id_pago} 
					                       and cod_beneficio = '{item.cod_beneficio}' and consec = {item.consec}";

                            var resp = connection.Execute(query);

                            //Actualiza el estado en tabla afi_bene_otorga
                            query = $@"SELECT COALESCE((
                                    SELECT COD_REMESA
                                    FROM afi_bene_otorga
                                    WHERE cedula = '{item.cedula}'
					                   and cod_beneficio = '{item.cod_beneficio}' 
                                       and consec = {item.consec}
                                ), 0) AS COD_REMESA;";
                            int existeRemesa = connection.Query<int>(query).FirstOrDefault();
                            if (existeRemesa == 0)
                            {
                                query = $@"Update afi_bene_otorga set estado = 'A',autoriza_user = '{infoTarjetas.usuario}',
					                          autoriza_fecha = Getdate(),cod_remesa = {infoTarjetas.cod_remesa_tr}  where cedula = '{item.cedula}'
					                           and cod_beneficio = '{item.cod_beneficio}' and consec = {item.consec}";

                                resp = connection.Execute(query);
                            }
                        }

                        mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                        {
                                vSolicitud = vTesoreria,
                                vCtaConta = proveedor.cuenta,
                                vMonto = item.monto,
                                vDH = "H",
                                vLinea = 1
                        });

                        mTESFuncionesDB.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
                        {
                                vSolicitud = vTesoreria,
                                vCtaConta = vCtaBene,
                                vMonto = item.monto,
                                vDH = "D",
                                vLinea = 2
                        });

                        if (item.id_pago == null)
                        {
                            query = $@"Update AFI_BENE_TARJETAS_REGALO set estado = 'D',activa_usuario = '{infoTarjetas.usuario}', 
					                      activa_fecha = Getdate(),cod_remesa_tr = {infoTarjetas.cod_remesa_tr}  where cod_producto = '{item.cod_producto}'
					                       and cod_beneficio = '{item.cod_beneficio}' and no_tarjeta = '{item.no_tarjeta}'";
                            connection.Execute(query);
                        }
                        else
                        {
                            query = $@"Update AFI_BENE_TARJETAS_REGALO set estado = 'E',activa_usuario = '{infoTarjetas.usuario}', 
					                      activa_fecha = Getdate(),cod_remesa_tr = {infoTarjetas.cod_remesa_tr}  where cod_producto = '{item.cod_producto}'
					                       and cod_beneficio = '{item.cod_beneficio}' and consec = '{item.consec}' and id_pago = {item.id_pago}";
                            connection.Execute(query);

                            mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = item.cod_beneficio,
                                consec = item.consec,
                                movimiento = "Actualiza",
                                detalle = $@"Env�o recarga de tarjeta a tesorer�a Cod.Remesa.TR: [{infoTarjetas.cod_remesa_tr}]",
                                registro_usuario = infoTarjetas.usuario
                            });
                        }
                             
                    }

                    //Actualiza y Carga Remesa
                    query = $@"update AFI_BENE_TARJETAS_REMESAS SET Estado = 'C'
		                             Where cod_remesa_tr = {infoTarjetas.cod_remesa_tr}";
                    var resp2 = connection.Execute(query);

                    //foreach (var item in infoCarga.casos)
                    //{
                    //    //Envio Correo
                    //    await CorreoNotificacionPago_Enviar(CodCliente, item);
                    //}
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
        #endregion

        #region Informes

        public ErrorDTO<AfiBeneTarjetasRemesasDataLista> AfiRecargaTarjProveedor_ObtenerRemesas(int CodCliente, string? filtro, int? pagina, int? paginacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneTarjetasRemesasDataLista>
            {
                Code = 0,
                Result = new AfiBeneTarjetasRemesasDataLista()
            };
            string paginaActual = " ", paginacionActual = " ", vfiltro = " ";
            try
            {
                if (filtro != null)
                {
                    vfiltro = " WHERE R.cod_remesa_tr LIKE '%" + filtro + "%' OR O.DESCRIPCION LIKE '%" + filtro + "%' " +
                        "OR O.COD_PROVEEDOR LIKE '%" + filtro + "%' OR R.estado LIKE '%" + filtro + "%' ";
                }

                if (pagina != null)
                {
                    paginaActual = " OFFSET " + pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select distinct COUNT(R.COD_REMESA_TR)
                    from AFI_BENE_TARJETAS_REMESAS R INNER JOIN 
                    AFI_BENE_TARJETAS_REGALO T ON T.COD_REMESA_TR = R.COD_REMESA_TR INNER JOIN 
                    AFI_BENE_PRODUCTOS P ON P.COD_PRODUCTO = T.COD_PRODUCTO INNER JOIN 
                    PV_PRODUCTO_PROV I ON I.COD_PRODUCTO = P.COD_PRODUCTO_INV INNER JOIN 
                    CXP_PROVEEDORES O ON O.COD_PROVEEDOR = I.COD_PROVEEDOR";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select distinct R.* , T.COD_PRODUCTO, P.COD_PRODUCTO_INV, I.COD_PROVEEDOR, O.DESCRIPCION AS NOMBRE_PROVEEDOR 
                    from AFI_BENE_TARJETAS_REMESAS R INNER JOIN 
                    AFI_BENE_TARJETAS_REGALO T ON T.COD_REMESA_TR = R.COD_REMESA_TR INNER JOIN 
                    AFI_BENE_PRODUCTOS P ON P.COD_PRODUCTO = T.COD_PRODUCTO INNER JOIN 
                    PV_PRODUCTO_PROV I ON I.COD_PRODUCTO = P.COD_PRODUCTO_INV INNER JOIN 
                    CXP_PROVEEDORES O ON O.COD_PROVEEDOR = I.COD_PROVEEDOR {vfiltro} 
                    order by R.registro_fecha desc {paginaActual} {paginacionActual}";
                    response.Result.Beneficios = connection.Query<AfiBeneTarjetasRemesasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDTO<List<AfiBeneTarjetasData>> AfiTarjetasRegaloRecargadas_Obtener(int CodCliente, int cod_remesa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneTarjetasData>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select T.*, 
                    (select top 1 descripcion from AFI_BENEFICIOS where COD_BENEFICIO = T.COD_BENEFICIO) AS BENEFICIO_DESC,
                    (select top 1 nombre from SOCIOS where CEDULA = T.CEDULA) AS NOMBRE 
                    from AFI_BENE_TARJETAS_REGALO T
                    where COD_REMESA_TR = {cod_remesa}";
                    response.Result = connection.Query<AfiBeneTarjetasData>(query).ToList();
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

        public async Task<ErrorDTO> AfiTarjetasRegaloRecargadas_Enviar(int CodCliente, DocArchivoBeneRecargaTarjetaDTO parametros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();
            string proveedor = "";
            List<FileTarjetasDTO> archivos = parametros.archivos;
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = @$"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                            WHERE C.COD_CATEGORIA IN (
                                            SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B
                                            WHERE B.COD_BENEFICIO IN (
                        select COD_BENEFICIO from AFI_BENE_TARJETAS_REGALO T INNER JOIN 
                        AFI_BENE_PRODUCTOS P ON P.COD_PRODUCTO = T.COD_PRODUCTO INNER JOIN 
                        PV_PRODUCTO_PROV I ON I.COD_PRODUCTO = P.COD_PRODUCTO_INV INNER JOIN 
                        CXP_PROVEEDORES O ON O.COD_PROVEEDOR = I.COD_PROVEEDOR
                        where T.COD_REMESA_TR = {parametros.cod_remesa_tr} and O.COD_PROVEEDOR = {parametros.cod_proveedor}
                                            )
                                            )";
                    string codCategoria = connection.Query<string>(queryCodBene).FirstOrDefault();

                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, codCategoria);

                    var queryProv = @$"select top 1 DESCRIPCION from CXP_PROVEEDORES where COD_PROVEEDOR = {parametros.cod_proveedor}";
                    proveedor = connection.Query<string>(queryProv).FirstOrDefault();

                }

                if (parametros.body.Trim() == "")
                {
                    parametros.body = "Estimado asociado, se le notifica la solicitud de pago para la recarga de tarjetas de regalo. Por favor, revise los archivos adjuntos para m�s detalles.";
                }

                string body = @$"<html lang=""es"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Solicitud de Pago: Transfenrencia</title>
                                    </head>
                                    <body>
                                        <p>{parametros.body}</p>
                                        <br>
                                        <p>Cod.Remesa de Tarjetas: {parametros.cod_remesa_tr}</p>
                                        <p>Cod.Proveedor: {parametros.cod_proveedor}</p>
                                        <p>Proveedor: {proveedor}</p>
                                    </body>
                                    </html>";

                List<IFormFile> Attachments = new List<IFormFile>();

                var file1 = ConvertByteArrayToIFormFileList(archivos[0].filecontent, archivos[0].filename);
                var file2 = ConvertByteArrayToIFormFileList(archivos[1].filecontent, archivos[1].filename);

                Attachments.AddRange(file1);
                Attachments.AddRange(file2);

                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = "tesoreria@aseccss.com";
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Solicitud de Pago";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "AfiTarjetasRegaloRecargadas_Enviar - " + ex.Message;
            }
            return info;
        }

        private List<IFormFile> ConvertByteArrayToIFormFileList(byte[] byteArray, string fileName)
        {
            var formFiles = new List<IFormFile>();

            if (byteArray == null || byteArray.Length == 0)
                return formFiles;

            // Crear un stream a partir del arreglo de bytes
            var stream = new MemoryStream(byteArray);

            // Crear una instancia de FormFile con el stream
            var formFile = new FormFile(stream, 0, byteArray.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream" // Puedes especificar el tipo de contenido si lo conoces
            };

            // Agregar el FormFile a la lista
            formFiles.Add(formFile);

            return formFiles;
        }
        #endregion
    }
}