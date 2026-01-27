using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;


namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesTrasladosDocumentosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesTrasladosDocumentosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = "";
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

                response.Result = conn.Query<DropDownListaGenericaModel>(query,
                    new { usuario = Usuario }).ToList();

                return DbHelper.CreateOkResponse<List<DropDownListaGenericaModel>>(response.Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener información de remesa mediante navegacion por scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TesUbiRemesaDto> TES_TrasladosDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TesUbiRemesaDto>
            {
                Code = 0,
                Result = new TesUbiRemesaDto()
            };
            try
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
                int? codRemesa = conn.QueryFirstOrDefault<int?>(query, new { codigo = Remesa });

                if (codRemesa.HasValue)
                {
                    response = TES_TrasladosDoc_Remesa_Obtener(CodEmpresa, codRemesa.GetValueOrDefault());
                }
                else
                {
                    response.Result = null;
                }

                return DbHelper.CreateOkResponse<TesUbiRemesaDto>(response.Result!);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesUbiRemesaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener información de remesa 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TesUbiRemesaDto> TES_TrasladosDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var query = @"select R.*,rtrim(X.cod_ubicacion) + ' - ' + X.descripcion as OUbicacion
                        ,rtrim(Y.cod_ubicacion) + ' - ' + Y.descripcion as DUbicacion
                        from tes_ubi_remesa R inner join tes_ubicaciones X on R.cod_ubicacion = X.cod_ubicacion
                        inner join tes_ubicaciones Y on R.cod_ubicacion_Destino = Y.cod_ubicacion
                        where R.cod_remesa = @codigo";
                var response = conn.QueryFirstOrDefault<TesUbiRemesaDto>(query,
                    new { codigo = Remesa });

                if (response == null)
                {
                    return DbHelper.CreateErrorResponse<TesUbiRemesaDto>("No se encontr&oacute; registro verifique...");
                }

                return DbHelper.CreateOkResponse<TesUbiRemesaDto>(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesUbiRemesaDto>(ex.Message);
            }
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                var queryT = @"select Count(C.nsolicitud) from Tes_Transacciones C 
                        inner join tes_ubi_remDet D on C.nsolicitud = D.nsolicitud
                        inner join Tes_Bancos B on C.id_Banco = B.id_Banco
                        inner join TES_Tipos_Doc T on C.Tipo = T.tipo
                        where D.cod_remesa = @codigo";
                response.Result.total = conn.QueryFirstOrDefault<int>(queryT, new { codigo = Remesa });

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
                if (filtros.pagina >= 0)
                {
                    query = query + $@" {filtros.filtro} 
                            ORDER BY C.nsolicitud desc
                            OFFSET {filtros.pagina} ROWS
                            FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }
                var vLista = conn.Query<TesTrasladoDocumentoDto>(query,
                new { codigo = Remesa }).ToList();

                foreach (var item in vLista)
                {
                    switch (item.id_estado)
                    {
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

                return DbHelper.CreateOkResponse<TablasListaGenericaModel>(response.Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener informacion de una solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Solicitud"></param>
        /// <returns></returns>
        public ErrorDto<TesTrasladoDocumentoDto> TES_TrasladosDoc_Solicitud_Obtener(int CodEmpresa, int Solicitud)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = @"select C.nsolicitud,C.id_banco,C.tipo,C.ndocumento
                    ,B.descripcion as BancoX,T.descripcion as TipoX 
                    from Tes_Transacciones C inner join Tes_Bancos B on C.id_Banco = B.id_Banco 
                    inner join TES_Tipos_Doc T on C.Tipo = T.tipo 
                    where C.nsolicitud = @nsolicitud and C.estado <> 'P'";

                var response = conn.QueryFirstOrDefault<TesTrasladoDocumentoDto>(query,
                new { nsolicitud = Solicitud });

                if (response == null)
                {
                    return DbHelper.CreateErrorResponse<TesTrasladoDocumentoDto>("N&uacute;mero de Solicitud no se encontr&oacute;...");
                }

                return DbHelper.CreateOkResponse<TesTrasladoDocumentoDto>(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesTrasladoDocumentoDto>(ex.Message);
            }
        }

        /// <summary>
        /// Guardar informacion de una remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="vEdita"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto TES_TrasladosDocumentos_Guardar(int CodEmpresa, bool vEdita, TesUbiRemesaDto Remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new int();
            try
            {
                var query = "";
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

                    conn.Execute(query,
                    new
                    {
                        usuario = Remesa.usuario,
                        notas = Remesa.notas,
                        origen = Remesa.cod_ubicacion,
                        destino = Remesa.cod_ubicacion_destino,
                        codigo = Remesa.cod_remesa
                    });

                    Bitacora(new BitacoraInsertarDto
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
                    int vCodigo = conn.QueryFirstOrDefault<int>(queryR);
                    vCodigo = vCodigo + 1;

                    query = @"insert tes_ubi_remesa(cod_remesa,cod_ubicacion,cod_ubicacion_destino,fecha,usuario,estado,notas)
                            values(@codigo, @origen, @destino, dbo.MyGetdate(), @usuario, 'P', @notas)";

                    conn.Execute(query,
                    new
                    {
                        usuario = Remesa.usuario,
                        notas = Remesa.notas,
                        origen = Remesa.cod_ubicacion,
                        destino = Remesa.cod_ubicacion_destino,
                        codigo = vCodigo
                    });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Remesa.usuario.ToUpper(),
                        DetalleMovimiento = "Remesa Traspaso : " + vCodigo,
                        Movimiento = "REGISTRA - WEB",
                        Modulo = 9
                    });

                    response = vCodigo;
                }

                return DbHelper.OkResponse($"Remesa guardada {response} correctamente.");

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var queryd = "delete tes_ubi_remdet where cod_remesa = @codigo";
                conn.Execute(queryd, new { codigo = Remesa });

                var queryr = "delete tes_ubi_remesa where cod_remesa = @codigo";
                conn.Execute(queryr, new { codigo = Remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario.ToUpper(),
                    DetalleMovimiento = "Remesa Traspaso : " + Remesa,
                    Movimiento = "ELIMINA - WEB",
                    Modulo = 9
                });

                return DbHelper.OkResponse("Linea Eliminada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Guardar linea en la lista de documentos de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <param name="Linea"></param>
        /// <returns></returns>
        public ErrorDto TES_TrasladosDocumentos_Linea_Guardar(int CodEmpresa, TesUbiRemesaDto Remesa, TesTrasladoDocumentoDto Linea)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = "";
                var valida = fxVerificaLinea(CodEmpresa, Remesa.cod_ubicacion, Linea.nsolicitud, Remesa.estado);
                if(!valida.Result) {
                    return DbHelper.ErrorResponse(valida.Description);
                }

                //Verifica si existe el documento
                var queryv = "select isnull(count(*),0) as Existe from tes_ubi_remDet where nsolicitud = @solicitud and cod_remesa = @codigo";
                int vExiste = conn.QueryFirstOrDefault<int>(queryv, new { codigo = Remesa.cod_remesa, solicitud = Linea.nsolicitud });

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

                conn.Execute(query,
                    new
                    {
                        codigo = Remesa.cod_remesa,
                        solicitud = Linea.nsolicitud,
                        notas = Linea.observacion
                    });

                return DbHelper.OkResponse("Linea guardada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };
            try
            {
                string vMensaje = "";
                var queryv = @"select isnull(max(cod_remesa),0) as Remesa
                        from tes_ubi_remdet where estado = 1 and nsolicitud = @solicitud";
                int vRemesa = conn.QueryFirstOrDefault<int>(queryv, new { solicitud = vSolicitud });

                if (vRemesa > 0)
                {
                    var query = @"select isnull(count(*),0) as Existe from tes_ubi_remesa 
                            where cod_ubicacion = @origen and cod_remesa = @remesa";
                    int vExiste = conn.QueryFirstOrDefault<int>(query,
                        new
                        {
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
                    return DbHelper.CreateErrorResponse<bool>(vMensaje);
                }

                return DbHelper.CreateOkResponse<bool>(response.Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
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
            const string query = @"delete tes_ubi_remDet where cod_remesa = @codigo and Nsolicitud = @solicitud";

            var parametros = new
            {
                codigo = Remesa,
                solicitud = Solicitud
            };

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);
        }
    }
}
