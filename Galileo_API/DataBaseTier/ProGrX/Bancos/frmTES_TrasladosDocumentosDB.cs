using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_TrasladosDocumentosDB
    {
        private readonly IConfiguration? _config;
        mSecurityMainDb DBBitacora;

        public frmTES_TrasladosDocumentosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener ubicaciones para dropdown de origen y destino según corresponda el tipo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_TrasladosDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario, string Tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (Tipo == "O")
                    {
                        query = @"select cod_ubicacion as item, rtrim(cod_ubicacion) + ' - ' + descripcion as descripcion from tes_ubicaciones
                            where usuario = @usuario";
                    } 
                    else
                    {
                        query = @"select cod_ubicacion as item, rtrim(cod_ubicacion) + ' - ' + descripcion as descripcion from tes_ubicaciones
                            where usuario <> @usuario";
                    }
                    query += " order by cod_ubicacion";

                    response.Result = connection.Query<DropDownListaGenericaModel>(query,
                        new { usuario = Usuario }).ToList();
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
        /// Obtener información de remesa mediante navegacion por scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TES_Ubi_RemesaDTO> TES_TrasladosDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_Ubi_RemesaDTO>
            {
                Code = 0,
                Result = new TES_Ubi_RemesaDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select Top 1 cod_remesa from tes_ubi_remesa";

                    if (scrollCode == 1)
                    {
                        query += " where cod_remesa > @codigo order by cod_remesa asc";
                    }
                    else
                    {
                        query += " where cod_remesa < @codigo order by cod_remesa desc";
                    }
                    int? codRemesa = connection.QueryFirstOrDefault<int?>(query, new { codigo = Remesa });

                    if (codRemesa.HasValue)
                    {
                        response = TES_TrasladosDoc_Remesa_Obtener(CodEmpresa, codRemesa.GetValueOrDefault());
                    }
                    else
                    {
                        response.Result = null;
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

        /// <summary>
        /// Obtener información de remesa 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TES_Ubi_RemesaDTO> TES_TrasladosDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_Ubi_RemesaDTO>
            {
                Code = 0,
                Result = new TES_Ubi_RemesaDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select R.*,rtrim(X.cod_ubicacion) + ' - ' + X.descripcion as OUbicacion
                        ,rtrim(Y.cod_ubicacion) + ' - ' + Y.descripcion as DUbicacion
                        from tes_ubi_remesa R inner join tes_ubicaciones X on R.cod_ubicacion = X.cod_ubicacion
                        inner join tes_ubicaciones Y on R.cod_ubicacion_Destino = Y.cod_ubicacion
                        where R.cod_remesa = @codigo";
                    response.Result = connection.QueryFirstOrDefault<TES_Ubi_RemesaDTO>(query,
                        new { codigo = Remesa });

                    if (response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "No se encontr&oacute; registro verifique...";
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

        /// <summary>
        /// Obtener lista de documentos pertenecientes a la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_TrasladosDocumentos_Obtener(int CodEmpresa, int Remesa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = @"select Count(C.nsolicitud) from Tes_Transacciones C 
                        inner join tes_ubi_remDet D on C.nsolicitud = D.nsolicitud
                        inner join Tes_Bancos B on C.id_Banco = B.id_Banco
                        inner join TES_Tipos_Doc T on C.Tipo = T.tipo
                        where D.cod_remesa = @codigo";
                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, new { codigo = Remesa });

                    var query = @"select C.nsolicitud,C.id_banco,C.tipo,C.ndocumento,D.estado as id_estado,D.observacion,D.observa_rec
                        ,B.descripcion as BancoX,T.descripcion as TipoX,D.fecha_rec,D.usuario_rec
                        from Tes_Transacciones C inner join tes_ubi_remDet D on C.nsolicitud = D.nsolicitud
                        inner join Tes_Bancos B on C.id_Banco = B.id_Banco
                        inner join TES_Tipos_Doc T on C.Tipo = T.tipo
                        where D.cod_remesa = @codigo";

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@" and (C.nsolicitud like '%{filtros.filtro}%' 
                            OR C.ndocumento like '%{filtros.filtro}%' OR C.tipo like '%{filtros.filtro}%' 
                            OR D.observa_rec like '%{filtros.filtro}%' OR D.usuario_rec like '%{filtros.filtro}%'
                            OR C.id_banco like '%{filtros.filtro}%') ";
                    }
                    if (filtros.pagina != null)
                    {
                        query = query + $@" {filtros.filtro} 
                            ORDER BY C.nsolicitud desc
                            OFFSET {filtros.pagina} ROWS
                            FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    }
                    var vLista = connection.Query<TES_TrasladoDocumentoDTO>(query,
                    new { codigo = Remesa }).ToList();

                    foreach (var item in vLista)
                    {
                        switch (item.id_estado) {
                            case 0:
                                item.estado = "Pendiente";
                                break;
                            case 1:
                                item.estado = "Recibido";
                                break;
                            case 2:
                                item.estado = "Rechazado";
                                break;
                            default:
                                item.estado = "Desconocido";
                                break;
                        }
                    }

                    response.Result.lista = vLista;
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
        /// Obtener informacion de una solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TES_TrasladoDocumentoDTO> TES_TrasladosDoc_Solicitud_Obtener(int CodEmpresa, int Solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_TrasladoDocumentoDTO>
            {
                Code = 0,
                Result = new TES_TrasladoDocumentoDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select C.nsolicitud,C.id_banco,C.tipo,C.ndocumento
                    ,B.descripcion as BancoX,T.descripcion as TipoX 
                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_Banco = B.id_Banco 
                    inner join TES_Tipos_Doc T on C.Tipo = T.tipo 
                    where C.nsolicitud = @nsolicitud and C.estado <> 'P'";

                    response.Result = connection.QueryFirstOrDefault<TES_TrasladoDocumentoDTO>(query,
                    new { nsolicitud = Solicitud });

                    if (response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "N&uacute;mero de Solicitud no se encontr&oacute;...";
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

        /// <summary>
        /// Guardar informacion de una remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vEdita"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto TES_TrasladosDocumentos_Guardar(int CodEmpresa, bool vEdita, TES_Ubi_RemesaDTO Remesa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (vEdita)
                    {
                        if (Remesa.estado == "P")
                        {
                            query = @"update tes_ubi_remesa set usuario = @usuario,notas = @notas, 
                            cod_ubicacion = @origen, cod_ubicacion_destino = @destino 
                            where cod_remesa = @codigo";
                        }
                        else
                        {
                            query = @"update tes_ubi_remesa set usuario = @usuario, notas = @notas 
                            where cod_remesa = @codigo";
                        }

                        connection.Execute(query,
                        new
                        {
                            usuario = Remesa.usuario,
                            notas = Remesa.notas,
                            origen = Remesa.cod_ubicacion,
                            destino = Remesa.cod_ubicacion_destino,
                            codigo = Remesa.cod_remesa
                        });

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Remesa.usuario.ToUpper(),
                            DetalleMovimiento = "Remesa Traspaso : " + Remesa.cod_remesa,
                            Movimiento = "MODIFICA - WEB",
                            Modulo = 9
                        });
                    }
                    else
                    {
                        var queryR = "select isnull(max(cod_remesa),0) as IDx from TES_UBI_REMESA";
                        int vCodigo = connection.QueryFirstOrDefault<int>(queryR);
                        vCodigo = vCodigo + 1;

                        query = @"insert tes_ubi_remesa(cod_remesa,cod_ubicacion,cod_ubicacion_destino,fecha,usuario,estado,notas)
                            values(@codigo, @origen, @destino, dbo.MyGetdate(), @usuario, 'P', @notas)";

                        connection.Execute(query,
                        new
                        {
                            usuario = Remesa.usuario,
                            notas = Remesa.notas,
                            origen = Remesa.cod_ubicacion,
                            destino = Remesa.cod_ubicacion_destino,
                            codigo = vCodigo
                        });

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Remesa.usuario.ToUpper(),
                            DetalleMovimiento = "Remesa Traspaso : " + vCodigo,
                            Movimiento = "REGISTRA - WEB",
                            Modulo = 9
                        });

                        response.Code = vCodigo;
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

        /// <summary>
        /// Eliminar informacion de una remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto TES_TrasladosDocumentos_Eliminar(int CodEmpresa, int Remesa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryd = "delete tes_ubi_remdet where cod_remesa = @codigo";
                    connection.Execute(queryd, new { codigo = Remesa });

                    var queryr = "delete tes_ubi_remesa where cod_remesa = @codigo";
                    connection.Execute(queryr, new { codigo = Remesa });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Remesa Traspaso : " + Remesa,
                        Movimiento = "ELIMINA - WEB",
                        Modulo = 9
                    });

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Guardar linea en la lista de documentos de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Linea"></param>
        /// <returns></returns>
        public ErrorDto TES_TrasladosDocumentos_Linea_Guardar(int CodEmpresa, TES_Ubi_RemesaDTO Remesa, TES_TrasladoDocumentoDTO Linea)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = "";
                var valida = fxVerificaLinea(CodEmpresa, Remesa.cod_ubicacion, Linea.nsolicitud, Remesa.estado);
                if(!valida.Result) {
                    response.Code = -1;
                    response.Description = valida.Description;
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    //Verifica si existe el documento
                    var queryv = "select isnull(count(*),0) as Existe from tes_ubi_remDet where nsolicitud = @solicitud and cod_remesa = @codigo";
                    int vExiste = connection.QueryFirstOrDefault<int>(queryv, new { codigo = Remesa.cod_remesa, solicitud = Linea.nsolicitud });

                    if (vExiste == 0)
                    {
                        query = @"insert tes_ubi_remDet(cod_remesa,nsolicitud,estado,observacion,fecha_rec,usuario_rec) 
                            values( @codigo, @solicitud, 0, @notas, null, '')";
                    } 
                    else
                    {
                        query = @"update tes_ubi_remDet set observacion = @notas where cod_remesa = @codigo
                            and Nsolicitud = @solicitud";
                    }
                        
                    connection.Execute(query, 
                        new {
                            codigo = Remesa.cod_remesa,
                            solicitud = Linea.nsolicitud,
                            notas = Linea.observacion
                        });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Verificar que ninguna ubicacion diferente a la actual, la tenga como recibida
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vOrigen"></param>
        /// <param name="vSolicitud"></param>
        /// <param name="vEstado"></param>
        /// <returns></returns>
        private ErrorDto<bool> fxVerificaLinea(int CodEmpresa, string vOrigen, int vSolicitud, string vEstado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string vMensaje = "";
                using var connection = new SqlConnection(stringConn);
                {
                    var queryv = @"select isnull(max(cod_remesa),0) as Remesa
                        from tes_ubi_remdet where estado = 1 and nsolicitud = @solicitud";
                    int vRemesa = connection.QueryFirstOrDefault<int>(queryv, new { solicitud = vSolicitud });

                    if (vRemesa > 0) 
                    {
                        var query = @"select isnull(count(*),0) as Existe from tes_ubi_remesa 
                            where cod_ubicacion = @origen and cod_remesa = @remesa";
                        int vExiste = connection.QueryFirstOrDefault<int>(query, 
                            new { 
                                remesa = vRemesa,
                                origen = vOrigen
                            });

                        if (vExiste == 0)
                        {
                            vMensaje = " - La Solicitud : " + vSolicitud + @" no se puede registrar en esta remesa, 
                                porque no se encuentra registrada en el Origen : " + vOrigen;
                        }
                    }
                    if (vEstado == "R")
                    {
                        vMensaje += " - La remesa ya fue recibida, no se pueden variar sus datos";
                    }

                    if (vMensaje.Length > 0)
                    {
                        response.Result = false;
                        response.Code = -1;
                        response.Description = vMensaje;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }
            return response;
        }

        /// <summary>
        /// Eliminar linea de la lista de documentos de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Solicitud"></param>
        /// <returns></returns>
        public ErrorDto TES_TrasladosDocumentos_Linea_Eliminar(int CodEmpresa, int Remesa, int Solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"delete tes_ubi_remDet where cod_remesa = @codigo and Nsolicitud = @solicitud";
                    connection.Execute(query,
                        new
                        {
                            codigo = Remesa,
                            solicitud = Solicitud
                        });
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}
