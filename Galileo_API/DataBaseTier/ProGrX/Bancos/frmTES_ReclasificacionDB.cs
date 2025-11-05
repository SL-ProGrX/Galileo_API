using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;


namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_ReclasificacionDB
    {
        private readonly IConfiguration? _config;
        private readonly mTesoreria mTesoreria;
        private readonly int vModulo = 9;
        private readonly mSecurityMainDb _Security_MainDB;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmTES_ReclasificacionDB(IConfiguration config)
        {
            _config = config;
            mTesoreria = new mTesoreria(_config);
            _Security_MainDB = new mSecurityMainDb(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(config);
        }

        /// <summary>
        /// Método para obtener los bancos activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TES_ReclasificacionBancos_Obtener(int CodEmpresa,string usuario,string gestion)
        {
            return mTesoreria.sbTesBancoCargaCboAccesoGestion(CodEmpresa, usuario, gestion);
            //string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            //var resp = new ErrorDTO<List<DropDownListaGenericaModel>>();
            //resp.Code = 0;
            //resp.Result = new List<DropDownListaGenericaModel>();
            //try
            //{
            //    string query = "";
            //    using var connection = new SqlConnection(stringConn);
            //    {
            //        query = $@"select id_banco as item,rtrim(descripcion) as descripcion from Tes_Bancos where estado = 'A'";
            //        resp.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();

            //        if (resp.Result.Count == 0)
            //        {
            //            resp.Code = -1;
            //            resp.Description = "No se encontraron bancos activos";
            //        }
            //        else
            //        {
            //            resp.Code = 0;
            //            resp.Description = "Ok";
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    resp.Code = -1;
            //    resp.Description = ex.Message;
            //    resp.Result = null;
            //}
            //return resp;
        }

        /// <summary>
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Despliega en pantalla los datos pertinentes a la solicitud digitada por el
        ///'               usuario.
        ///'REFERENCIAS:   LimpiaObjetos - (Limpia los objetos que muestran informacion pertinente a
        ///'               la solicitud por reclasificar)
        ///'               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        ///'               Procedimiento)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDTO<Tes_ReclasificacionDTO> TES_Reclasificacion_Obtener(int CodEmpresa, int solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<Tes_ReclasificacionDTO>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select T.*,B.Descripcion as 'BancoDesc',B.CtaConta as 'BancoCta', Td.descripcion as 'TipoDesc'
                                        from Tes_Transacciones T 
                                        inner join Tes_Bancos B on T.id_Banco = B.id_Banco
                                        inner join tes_tipos_doc Td on T.Tipo = Td.Tipo
                                        Where T.Nsolicitud= @solicitud ";
                    response.Result = connection.Query<Tes_ReclasificacionDTO>(query,
                        new
                        {
                            solicitud = solicitud
                        }).FirstOrDefault();

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
        /// Método para obtener la cuenta contable del banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <returns></returns>
        public ErrorDTO<string> TES_Reclasificacion_CuentaBanco(int CodEmpresa, int id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<string>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ctaconta as Cuenta from Tes_Bancos where id_banco = @id_banco ";
                    response.Result = connection.Query<string>(query,
                        new
                        {
                            id_banco = id_banco
                        }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDTO<List<DropDownListaGenericaModel>> tes_TiposDocsCargaCboAcceso_Obtener(int CodEmpresa, string usuario, int id_banco, string tipo)
        {
            return mTesoreria.sbTesTiposDocsCargaCboAcceso(CodEmpresa, usuario, id_banco, tipo);
        }

        /// <summary>
        /// Método para cambiar el banco de la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO TES_Reclasificacion_CambiaBanco(int CodEmpresa, Tes_ReclasificaBancoModel data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select estado_asiento from Tes_Transacciones where nsolicitud = @nsolicitud";
                    var estado = connection.Query<string>(query,
                        new
                        {
                            nsolicitud = data.nsolicitud
                        }).FirstOrDefault();
                    if (estado == "G")
                    {
                        response.Code = -1;
                        response.Description = "El asiento de esta solicitud ya fue generado, no se puede reclasificar...";
                        return response;
                    }

                    data.bancoDestino = data.bancoDestino.Trim();

                    query = $@"exec spTes_Reclasificacion @Nsolicitud, @bancoDestino, @tipo, @usuario,@nota ";
                    var res = connection.ExecuteAsync(query,
                        new
                        {
                            Nsolicitud = data.nsolicitud,
                            bancoDestino = data.bancoDestino,
                            tipo = data.tipo,
                            usuario = data.usuario,
                            nota = data.nota
                        }).Result;
                    response.Description = "Cambio de Banco Realizado Satisfactoriamente...";

                    _Security_MainDB.Bitacora
                        (new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.usuario,
                            DetalleMovimiento = $"Solicitud {data.nsolicitud} reclasificada a Banco {data.bancoDestino}",
                            Movimiento = "RECLASIFICACION - WEB",
                            Modulo = vModulo
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
        /// ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Modifica la solicitud en cuanto al # de Documento.
        ///'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
        ///'               LimpiaObjetos - (Limpia los objetos que muestran informacion pertinente a
        ///'               la solicitud por reclasificar)
        ///'               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        /// '               Procedimiento)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO TES_Reclasificacion_CambiaDocumento(int CodEmpresa, Tes_ReclasificaDocumentoModel data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select estado_asiento from Tes_Transacciones where nsolicitud = @nsolicitud";
                    var estado = connection.Query<string>(query,
                        new
                        {
                            nsolicitud = data.nsolicitud
                        }).FirstOrDefault();
                    if(estado == "G")
                    {
                        response.Code = -1;
                        response.Description = "El asiento de esta solicitud ya fue generado, no se puede reclasificar...";
                        return response;
                    }

                    // Verifico si el # Documento anterior
                    query = $@"select Ndocumento from Tes_Transacciones 
                               where nsolicitud = @nsolicitud And Tipo = @tipo and id_banco= @id_banco";
                    var ndocumentoAnterior = connection.Query<string>(query,
                        new
                        {
                            nsolicitud = data.nsolicitud,
                            tipo = data.tipo,
                            id_banco = data.id_banco
                        }).FirstOrDefault();

                    query = $@"Select Nsolicitud from Tes_Transacciones where id_banco= @id_banco
                                      And Tipo = @tipo  and Ndocumento = @ndocumento ";
                    var solicitud = connection.Query<int>(query,
                        new
                        {
                            id_banco = data.id_banco,
                            tipo = data.tipo,
                            ndocumento = data.ndocumento
                        }).FirstOrDefault();

                    if (solicitud != 0)
                    {
                        response.Code = -1;
                        response.Description = "# Documento Ya Existe, No Se Puede Reclasificar";
                        return response;
                    }

                    query = $@"Update Tes_Transacciones Set Ndocumento = @ndocumento Where NSolicitud = @solicitud ";
                    var res = connection.ExecuteAsync(query,
                        new
                        {
                            ndocumento = data.ndocumento,
                            solicitud = data.nsolicitud
                        }).Result;

                    string bitacora = $"Cambio N.Documento de {ndocumentoAnterior} a {data.ndocumento}";
                    mTesoreria.sbTesBitacoraEspecial(CodEmpresa, data.nsolicitud, "09", bitacora, data.usuario);

                    _Security_MainDB.Bitacora
                        (new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.usuario,
                            DetalleMovimiento = $"Solicitud {data.nsolicitud} reclasificada a Documento {data.ndocumento}",
                            Movimiento = "RECLASIFICACION - WEB",
                            Modulo = vModulo
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
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        ///'OBJETIVO:      Reclasifica la solicitud en cuanto al Banco y Tipo de Documento. Ademas
        ///'               actualiza para el detalle de la solicitud el # Cuenta del Banco.
        ///'REFERENCIAS:   Bitacora - (Registra movimientos sobre la Base de Datos)
        ///'               LimpiaObjetos - (Limpia los objetos que muestran informacion pertinente a
        ///'               la solicitud por reclasificar)
        ///'               ProcedimientoErrores - (Registra error en caso de que ocurra uno dentro del
        ///'               Procedimiento)
        ///'OBSERVACIONES: Ninguna.
        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO TES_Reclasificacion_CambiaSolicitud(int CodEmpresa, Tes_ReclasificaSolicitudModel data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select estado_asiento from Tes_Transacciones where nsolicitud = @nsolicitud";
                    var estado = connection.Query<string>(query,
                        new
                        {
                            nsolicitud = data.nsolicitud
                        }).FirstOrDefault();
                    if (estado == "G")
                    {
                        response.Code = -1;
                        response.Description = "El asiento de esta solicitud ya fue generado, no se puede reclasificar...";
                        return response;
                    }

                    if (!data.permiteReqId)
                    {
                        data.tipoId = -1;
                    }

                    query = $@"exec spTes_Reclasificacion @solicitud , @id_banco , @tipo , @usuario ,@nota, @tipoId ";
                    var res = connection.ExecuteAsync(query,
                        new
                        {
                            solicitud = data.nsolicitud,
                            id_banco = data.id_banco,
                            tipo = data.tipo,
                            usuario = data.usuario,
                            nota = data.nota,
                            tipoId = data.tipoId
                        }).Result;

                    _Security_MainDB.Bitacora
                         (new BitacoraInsertarDTO
                         {
                             EmpresaId = CodEmpresa,
                             Usuario = data.usuario,
                             DetalleMovimiento = $"Solicitud {data.nsolicitud} reclasificada a Banco {data.id_banco}, Tipo {data.tipo} y Cod_ID {data.tipoId}",
                             Movimiento = "RECLASIFICACION - WEB",
                             Modulo = vModulo
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
        /// Método para obtener la lista de solicitudes de tesorería
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> TES_Solicitudes_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Busco Total
                    var query = $@"select count(NSOLICITUD) from Tes_Transacciones";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro.filtro != null && filtro.filtro != "")
                    {
                        filtro.filtro = $@"WHERE C.NSOLICITUD like '%{filtro.filtro}%' 
                                              OR C.BENEFICIARIO like '%{filtro.filtro}%'
                                              OR T.descripcion like '%{filtro.filtro}%'
                                              OR C.CODIGO like '%{filtro.filtro}%'";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "C.NSOLICITUD";
                    }
                    else
                    {
                        filtro.sortField = "C." + filtro.sortField;
                    }

                    if (filtro.sortOrder == 0)
                    {
                        filtro.sortOrder = 1; //Por defecto orden ascendente
                    }

                    if (filtro.pagina != null)
                    {
                        query = $@"select C.NSOLICITUD, rtrim(T.descripcion) as 'tipo', C.CODIGO , C.BENEFICIARIO , C.monto, C.estado, C.COD_UNIDAD  
                                            from Tes_Transacciones C inner join Tes_Tipos_doc T on C.tipo = T.tipo
                                                {filtro.filtro} 
                                                ORDER BY {filtro.sortField} {(filtro.sortOrder == -1 ? "DESC" : "ASC")}  
                                                OFFSET {filtro.pagina} ROWS
                                                FETCH NEXT {filtro.paginacion} ROWS ONLY ";

                        response.Result.lista = connection.Query<Tes_SolicitudesData>(query).ToList();
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
        /// Método para obtener los tipos de identificación
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodEmpresa)
        {
            return _AuxiliarDB.TiposIdentificacion_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Método para validar si el id de la cuenta se puede cambiar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<bool> Tes_ReclasificaId_Valida(int CodEmpresa, string? tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };
            try
            {
                if(tipo == null)
                {
                    response.Code = 0;
                    response.Description = "Tipo no puede ser nulo";
                    response.Result = false;
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select ISNULL(INT_RECLASIFICA_ID, 0) from TES_TIPOS_DOC where TIPO = @Tipo ";
                    response.Result = connection.Query<bool>(query,
                        new
                        {
                            Tipo = tipo
                        }).FirstOrDefault();
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
