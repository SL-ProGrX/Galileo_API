using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_RecepcionDocumentosDB
    {
        private readonly IConfiguration? _config;
        mSecurityMainDb DBBitacora;

        public frmTES_RecepcionDocumentosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener ubicaciones para dropdown
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_RecepcionDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select cod_ubicacion as item, rtrim(cod_ubicacion) + ' - ' + descripcion as descripcion from tes_ubicaciones
                        where usuario = @usuario order by cod_ubicacion";
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
        /// Obtener remesa mediante navegacion por scroll,
        /// busca la remesa siguiente o anterior mediante el scrollCode
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TES_Ubi_RemesaDTO> TES_RecepcionDoc_Remesa_Scroll_Obtener(int CodEmpresa, int scrollCode, int Remesa)
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
                        response = TES_RecepcionDoc_Remesa_Obtener(CodEmpresa, codRemesa.GetValueOrDefault());
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
        /// Obtener información de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TES_Ubi_RemesaDTO> TES_RecepcionDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
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
                        response.Description = "No se encontró registro verifique...";
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
        /// Obtener información de las solicitudes para recepción de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_RecepcionDocumentos_Obtener(int CodEmpresa, int Remesa, FiltrosLazyLoadData filtros)
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

                    var query = @"select C.nsolicitud,C.id_banco,C.tipo,C.ndocumento,D.estado,D.observacion,D.observa_rec
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
                    response.Result.lista = connection.Query<TES_RecepcionDocumentoDTO>(query,
                    new { codigo = Remesa }).ToList();
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
        /// Aplicar la recepción de documentos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto TES_RecepcionDocumentos_Aplicar(int CodEmpresa, TES_RecepcionDocumento_Filtros parametros)
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
                    foreach (var item in parametros.solicitudes)
                    {
                        query = @"update tes_ubi_remDet set observa_rec = @notas,fecha_rec = dbo.MyGetdate(), 
                        usuario_rec = @usuario, estado = @estado  where cod_remesa = @remesa and Nsolicitud = @solicitud";
                        connection.Execute(query,
                        new
                        {
                            notas = item.observa_rec,
                            usuario = parametros.usuario,
                            estado = item.estado ? 1 : 0,
                            remesa = parametros.cod_remesa,
                            solicitud = item.nsolicitud
                        });
                    }

                    query = @"update tes_ubi_remesa set estado = 'R' where cod_remesa = @remesa";
                    connection.Query<TES_RecepcionDocumentoDTO>(query,
                        new { remesa = parametros.cod_remesa });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = parametros.usuario.ToUpper(),
                        DetalleMovimiento = "Recepcion de la Remesa Documentos: " + parametros.cod_remesa,
                        Movimiento = "APLICA - WEB",
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
    }
}
