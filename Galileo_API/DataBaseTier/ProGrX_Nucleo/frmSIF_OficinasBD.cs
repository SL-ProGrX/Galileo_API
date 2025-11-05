using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient; 



namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_OficinasBD
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10 ; 
        private readonly mSecurityMainDb _Security_MainDB;


        public frmSIF_OficinasBD(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SifOficinasLista> Sif_OficinasLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SifOficinasLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SifOficinasLista()
                {
                    total = 0,
                    lista = new List<SifOficinasData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(cod_oficina) from Sif_Oficinas";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_oficina LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_oficina";
                    }

                    query = $@"select cod_oficina,descripcion,COD_UNIDAD,Cod_Centro_Costo,Telefono_01,Telefono_02,DIRECCION, Registro_Usuario,Registro_Fecha, Tipo,Oficina_Omision,Estado from Sif_Oficinas
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<SifOficinasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }

        /// <summary>
        /// Metodo para consultar lista unidades contables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasUnidadContable_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //var query = $@"select descripcion from CntX_Unidades where cod_unidad = {pCodigo} and cod_contabilidad = {CodEmpresa}";
                    var query = $@"select cod_unidad as 'item',descripcion from CntX_Unidades ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
        /// <summary>
        /// Metodo para consultar lista de centros de costo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasCentroCostos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //var query = $@"select descripcion from CntX_Unidades where cod_unidad = {pCodigo} and cod_contabilidad = {CodEmpresa}";
                    var query = $@"select cod_centro_costo as 'item',descripcion from CNTX_CENTRO_COSTOS ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_Oficinas_Lista(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_oficina) as 'item', rtrim(descripcion) as 'descripcion'
				                         from  SIF_Oficinas  where estado = 1 order by cod_oficina";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<DropDownListaGenericaModel>();
            }
            return response;
        }


        /// <summary>
        /// Actualiza los datos de la oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficinaDatos"></param>
        /// <returns></returns>

        public ErrorDto Sif_Oficinas_ActualizarDatos(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update SIF_Oficinas
                                    set Telefono_01 = @telefono1,
                                        Telefono_02 = @telefono2,
                                        direccion = @direccion                                      
                                    WHERE cod_oficina = @cod_oficina";
                    connection.Execute(query, new
                    {
                        cod_oficina = oficinaDatos.cod_oficina.Trim(),
                        telefono1 = oficinaDatos.telefono_01,
                        telefono2 = oficinaDatos.telefono_02,
                        direccion = oficinaDatos.direccion
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = oficinaDatos.registro_usuario,
                        DetalleMovimiento = $"Oficina:  {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// Metodo para guardar oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficinaDatos"></param>
        /// <returns></returns>
        public ErrorDto Sif_Oficinas_Guardar(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    //Verifico si existe usuario
                    var qUnidad = $@"select count(cod_unidad) from CntX_Unidades where ACTIVA = 1  and cod_unidad = '{oficinaDatos.cod_unidad.Trim()}' ";
                    int existeunidad = connection.QueryFirstOrDefault<int>(qUnidad);
                    if (existeunidad == 0)
                    {
                        result.Code = -2;
                        result.Description = $"La unidad contable {oficinaDatos.cod_unidad} no existe o no está activo.";
                        return result;
                    }
                    var qCentroCosto= $@"select count(cod_centro_costo) from CNTX_CENTRO_COSTOS where ACTIVO = 1  and cod_centro_costo = '{oficinaDatos.cod_centro_costo.Trim()}' ";
                    int existeCentroCosto = connection.QueryFirstOrDefault<int>(qCentroCosto);
                    if (existeCentroCosto == 0)
                    {
                        result.Code = -2;
                        result.Description = $"El centro de costo {oficinaDatos.cod_centro_costo} no existe o no está activo.";
                        return result;
                    }
                    //verifico si existe el recurso
                    var query = $@"select isnull(count(*),0) as Existe from sif_oficinas where cod_oficina = @Cod_oficina ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { Cod_oficina = oficinaDatos.cod_oficina });

                    if (oficinaDatos.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"La oficina con el código {oficinaDatos.cod_oficina} ya existe.";
                        }
                        else
                        {
                            result = Sif_Oficinas_Insertar(CodEmpresa, oficinaDatos);
                        }
                    }
                    else if (existe == 0 && !oficinaDatos.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"La oficina con el código {oficinaDatos.cod_oficina} no existe.";
                    }
                    else
                    {
                        result = Sif_Oficinas_Actualizar(CodEmpresa, oficinaDatos);
                    }
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

     /// <summary>
     /// Metodo de actualizar datos de una oficina
     /// </summary>
     /// <param name="CodEmpresa"></param>
     /// <param name="oficinaDatos"></param>
     /// <returns></returns>
        private ErrorDto Sif_Oficinas_Actualizar(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update sif_oficinas
                                    set descripcion = @Descripcion,
                                        cod_unidad = @Cod_unidad,
                                        cod_centro_costo= @Cod_centro_costo,
                                        Tipo= @tipo,
                                        Oficina_Omision= @Oficina_Omision,
                                        Estado= @estado,
                                        Telefono_01 = @telefono1,
                                        Telefono_02 = @telefono2,
                                        direccion = @Direccion                                           
                                    WHERE cod_oficina = @cod_oficina";
                    connection.Execute(query, new
                    {
                        cod_oficina = oficinaDatos.cod_oficina.Trim(),
                        Descripcion = oficinaDatos.descripcion,
                        Cod_unidad = oficinaDatos.cod_unidad,
                        Cod_centro_costo = oficinaDatos.cod_centro_costo,
                        estado = oficinaDatos.estado,
                        Tipo = oficinaDatos.tipo,
                        Oficina_Omision = oficinaDatos.oficina_omision,
                        telefono1 = oficinaDatos.telefono_01,
                        telefono2 = oficinaDatos.telefono_02,
                        Direccion = oficinaDatos.direccion
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = oficinaDatos.registro_usuario,
                        DetalleMovimiento = $"Oficina:  {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

     /// <summary>
     /// Metodo de insertar oficina
     /// </summary>
     /// <param name="CodEmpresa"></param>
     /// <param name="oficinaDatos"></param>
     /// <returns></returns>
        private ErrorDto Sif_Oficinas_Insertar(int CodEmpresa, SifOficinasData oficinaDatos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert into sif_oficinas(cod_oficina,descripcion,cod_unidad,cod_centro_costo,Tipo,Oficina_Omision,Estado,Telefono_01,Telefono_02,Direccion,registro_fecha,registro_usuario)
                                    VALUES (@cod_oficina, @Descripcion,@Cod_unidad,@Cod_centro_costo,@Tipo,@Oficina_Omision, @estado,@telefono1,@telefono2,@Direccion, dbo.MyGetdate(), @usuario)";
                    connection.Execute(query, new
                    {
                        cod_oficina = oficinaDatos.cod_oficina.Trim(),
                        Descripcion = oficinaDatos.descripcion,
                        Cod_unidad = oficinaDatos.cod_unidad,
                        Cod_centro_costo = oficinaDatos.cod_centro_costo,
                        Tipo = oficinaDatos.tipo,
                        Oficina_Omision = oficinaDatos.oficina_omision,
                        telefono1 = oficinaDatos.telefono_01,
                        telefono2 = oficinaDatos.telefono_02,
                        Direccion = oficinaDatos.direccion,
                        estado = oficinaDatos.estado,
                        usuario = oficinaDatos.registro_usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = oficinaDatos.registro_usuario,
                        DetalleMovimiento = $"Oficina: {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Medoto para consultar lista de miembros de una oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="filtro"></param>
        /// <param name="apoyo"></param>
        /// <param name="usuariosEstado"></param>
        /// <returns></returns>
        public ErrorDto<List<SifOficinasMiembros>> Sif_OficinasMiembros_Lista(int CodEmpresa, string oficina, string filtro, int apoyo, int usuariosEstado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifOficinasMiembros>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifOficinasMiembros>()
            };
           
            try
            {
                //Info de pruebas
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSys_Oficinas_Miembros_Consultas '{oficina}','{filtro.Trim()}',{apoyo},{usuariosEstado} ";
                    result.Result = connection.Query<SifOficinasMiembros>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<SifOficinasMiembros>();
            }

            return result;
        }
        /// <summary>
        /// Metodo para agregar miembros a una oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="usuario"></param>
        /// <param name="apoyo"></param>
        /// <param name="usuarioRegistro"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto Sif_OficinasMiembros_Agregar(int CodEmpresa, string oficina, string usuario, int apoyo, string usuarioRegistro,string accion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto result = new ErrorDto();
            try
            {
                //Info de pruebas
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSys_Oficinas_Miembros_Add '{oficina}','{usuario.Trim()}',{apoyo},{usuarioRegistro} ,'{accion}' ";                    
                    result.Code = connection.Query<int>(query).FirstOrDefault();
                    result.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Medoto para consultar historial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SifOficinasHistorial>> Sif_OficinasHistorial_Lista(int CodEmpresa, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifOficinasHistorial>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifOficinasHistorial>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from dbo.SIF_OFICINA_MIEMBROS_H where usuario = '{filtro}' order by cod_historial desc";
                    result.Result = connection.Query<SifOficinasHistorial>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<SifOficinasHistorial>();
            }

            return result;
        }

        /// <summary>
        /// Metodo para consultar datos a exportar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SifOficinasData>> Sif_Oficinas_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SifOficinasData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifOficinasData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                     
                  
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_oficina LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_oficina";
                    }

                    query = $@"select cod_oficina,descripcion,COD_UNIDAD,Cod_Centro_Costo,Telefono_01,Telefono_02,DIRECCION, Registro_Usuario,Registro_Fecha, Tipo,Oficina_Omision,Estado from Sif_Oficinas
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result = connection.Query<SifOficinasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


      
    }
}
