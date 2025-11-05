using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_TiposIdsDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_TiposIdsDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener tipos de identificaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_TiposIdsLista> AF_TiposIds_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_TiposIdsLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_TiposIdsLista()
                {
                    total = 0,
                    lista = new List<AF_TiposIdsDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = "select count(Tipo_ID) from vSys_Tipos_Ids";
                    response.Result.total = connection.Query<int>(queryT).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( Tipo_ID LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Tipo_Personeria_Desc LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "Tipo_ID";
                    }

                    var query = $@"select Tipo_ID,descripcion, Tipo_Personeria_Desc, 
                        Largo_Minimo, Mascara, CODIGO_SUGEF, CODIGO_PIN, CODIGO_HACIENDA, CODIGO_SINPE 
                        from vSys_Tipos_Ids
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    response.Result.lista = connection.Query<AF_TiposIdsDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }

            return response;
        }

        /// <summary>
        /// Guardar tipo de identificación
        /// Insertar o actualizar según si existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_TiposIds_Guardar(int CodEmpresa, string Usuario, AF_TiposIdsDTO Info)
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
                    var query = "select isnull(count(*),0) as Existe from afi_Tipos_IDs where Tipo_ID = @TipoId";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            TipoId = Info.tipo_Id
                        }
                    );

                    if (existe == 0)
                    {
                        query = $@"insert into afi_Tipos_IDs(Tipo_ID, descripcion, Tipo_Personeria, Largo_Minimo, Mascara, 
                            CODIGO_SUGEF, CODIGO_PIN, CODIGO_HACIENDA, CODIGO_SINPE, Usuario, Fecha) 
                            values( @TipoId, @Descripcion, @TipoPersoneria, @LargoMinimo, @Mascara,
                            @CodigoSugef, @CodigoPin, @CodigoHacienda, @CodigoSinpe, @Usuario, GetDate())";

                        connection.Execute(query,
                            new
                            {
                                TipoId = Info.tipo_Id,
                                Descripcion = Info.descripcion,
                                TipoPersoneria = Info.tipo_Personeria_Desc.Substring(0, 1),
                                LargoMinimo = Info.largo_Minimo,
                                Mascara = Info.mascara,
                                CodigoSugef = Info.codigo_Sugef,
                                CodigoPin = Info.codigo_Pin,
                                CodigoHacienda = Info.codigo_Hacienda,
                                CodigoSinpe = Info.codigo_Sinpe,
                                Usuario
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Tipo de Idenficiación : " + Info.tipo_Id,
                            Movimiento = "Registra - WEB",
                            Modulo = 9
                        });
                    }
                    else
                    {
                        query = @"update afi_Tipos_IDs set descripcion = @Descripcion, Tipo_Personeria = @TipoPersoneria
                            , Largo_Minimo = @LargoMinimo, Mascara = @Mascara, CODIGO_SUGEF = @CodigoSugef
                            , CODIGO_PIN = @CodigoPin, CODIGO_HACIENDA = @CodigoHacienda, CODIGO_SINPE = @CodigoSinpe
                            , MODIFICA_USUARIO = @Usuario, MODIFICA_FECHA = GetDate() where Tipo_ID = @TipoId";

                        connection.Execute(query,
                            new
                            {
                                TipoId = Info.tipo_Id,
                                Descripcion = Info.descripcion,
                                TipoPersoneria = Info.tipo_Personeria_Desc.Substring(0, 1),
                                LargoMinimo = Info.largo_Minimo,
                                Mascara = Info.mascara,
                                CodigoSugef = Info.codigo_Sugef,
                                CodigoPin = Info.codigo_Pin,
                                CodigoHacienda = Info.codigo_Hacienda,
                                CodigoSinpe = Info.codigo_Sinpe,
                                Usuario
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Tipo de Idenficiación : " + Info.tipo_Id,
                            Movimiento = "Modifica - WEB",
                            Modulo = 9
                        });
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
        /// Eliminar tipo de identificación
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="TipoId"></param>
        /// <returns></returns>
        public ErrorDto AF_TiposIds_Eliminar(int CodEmpresa, string Usuario, int TipoId)
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
                    var query = "delete afi_Tipos_IDs where Tipo_ID = @TipoId";
                    connection.Execute(query, new { TipoId } );

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Tipo de Idenficiación : " + TipoId,
                        Movimiento = "Elimina - WEB",
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
