using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifOficinasBD
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;


        public FrmSifOficinasBD(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        ///  Método para consultar lista de oficinas
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
                using var connection = new SqlConnection(stringConn);

                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    fetch = int.MaxValue;
                }

                // Total (mantiene comportamiento anterior: total sin filtro)
                const string totalQuery = @"SELECT COUNT(cod_oficina) FROM Sif_Oficinas";
                result.Result.total = connection.Query<int>(totalQuery).FirstOrDefault();

                const string query = @"
                        SELECT cod_oficina,descripcion,COD_UNIDAD,Cod_Centro_Costo,Telefono_01,Telefono_02,DIRECCION,
                               Registro_Usuario,Registro_Fecha, Tipo,Oficina_Omision,Estado
                        FROM Sif_Oficinas
                        WHERE (@search IS NULL
                               OR cod_oficina LIKE @search
                               OR descripcion LIKE @search
                               OR Registro_Usuario LIKE @search)
                        ORDER BY
                            -- ASC
                            CASE WHEN @sortOrder = 1 AND @sortField = 'cod_oficina' THEN cod_oficina END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'cod_unidad' THEN COD_UNIDAD END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'cod_centro_costo' THEN Cod_Centro_Costo END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_01' THEN Telefono_01 END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_02' THEN Telefono_02 END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'direccion' THEN DIRECCION END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'registro_usuario' THEN Registro_Usuario END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha' THEN Registro_Fecha END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'tipo' THEN Tipo END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'oficina_omision' THEN Oficina_Omision END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'estado' THEN Estado END ASC,

                            -- DESC
                            CASE WHEN @sortOrder = 0 AND @sortField = 'cod_oficina' THEN cod_oficina END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'cod_unidad' THEN COD_UNIDAD END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'cod_centro_costo' THEN Cod_Centro_Costo END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_01' THEN Telefono_01 END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_02' THEN Telefono_02 END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'direccion' THEN DIRECCION END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'registro_usuario' THEN Registro_Usuario END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha' THEN Registro_Fecha END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'tipo' THEN Tipo END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'oficina_omision' THEN Oficina_Omision END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'estado' THEN Estado END DESC,

                            -- Fallback determinístico
                            cod_oficina ASC
                        OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                result.Result.lista = connection.Query<SifOficinasData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SifOficinasData>();
            }
            return result;
        }


        /// <summary>
        /// Método para consultar lista unidades contables
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
                const string query = @"select cod_unidad as 'item',descripcion from CntX_Unidades";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Método para consultar lista de centros de costo
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
                const string query = @"select cod_centro_costo as 'item',descripcion from CNTX_CENTRO_COSTOS";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Método para consultar lista de oficinas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sif_Oficinas_Lista(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string query = @"select rtrim(cod_oficina) as 'item', rtrim(descripcion) as 'descripcion'
				                         from  SIF_Oficinas  where estado = 1 order by cod_oficina";
                response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = oficinaDatos.registro_usuario,
                    DetalleMovimiento = $"Oficina:  {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
        
        
        /// <summary>
        /// Método para guardar oficina
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

                //Verifico si existe usuario
                const string qUnidad = @"SELECT COUNT(cod_unidad) FROM CntX_Unidades WHERE ACTIVA = 1 AND cod_unidad = @cod_unidad";
                int existeunidad = connection.QueryFirstOrDefault<int>(qUnidad, new { cod_unidad = (oficinaDatos.cod_unidad ?? string.Empty).Trim() });
                if (existeunidad == 0)
                {
                    result.Code = -2;
                    result.Description = $"La unidad contable {oficinaDatos.cod_unidad} no existe o no está activo.";
                    return result;
                }
                const string qCentroCosto = @"SELECT COUNT(cod_centro_costo) FROM CNTX_CENTRO_COSTOS WHERE ACTIVO = 1 AND cod_centro_costo = @cod_centro_costo";
                int existeCentroCosto = connection.QueryFirstOrDefault<int>(qCentroCosto, new { cod_centro_costo = (oficinaDatos.cod_centro_costo ?? string.Empty).Trim() });
                if (existeCentroCosto == 0)
                {
                    result.Code = -2;
                    result.Description = $"El centro de costo {oficinaDatos.cod_centro_costo} no existe o no está activo.";
                    return result;
                }
                //verifico si existe el recurso
                const string query = @"select isnull(count(*),0) as Existe from sif_oficinas where cod_oficina = @Cod_oficina";
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
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Método de actualizar datos de una oficina
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

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = oficinaDatos.registro_usuario,
                    DetalleMovimiento = $"Oficina:  {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// Método de insertar oficina
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

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = oficinaDatos.registro_usuario,
                    DetalleMovimiento = $"Oficina: {oficinaDatos.cod_oficina} - {oficinaDatos.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

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
                const string sp = "spSys_Oficinas_Miembros_Consultas";
                result.Result = connection.Query<SifOficinasMiembros>(sp, new
                {
                    oficina,
                    filtro = (filtro ?? string.Empty).Trim(),
                    apoyo,
                    usuariosEstado
                }, commandType: CommandType.StoredProcedure).ToList();

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
        /// Método para agregar miembros a una oficina
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="oficina"></param>
        /// <param name="usuario"></param>
        /// <param name="apoyo"></param>
        /// <param name="usuarioRegistro"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto Sif_OficinasMiembros_Agregar(int CodEmpresa, string oficina, string usuario, int apoyo, string usuarioRegistro, string accion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto result = new ErrorDto();
            try
            {
                //Info de pruebas
                using var connection = new SqlConnection(stringConn);
                const string sp = "spSys_Oficinas_Miembros_Add";
                result.Code = connection.Query<int>(sp, new
                {
                    oficina,
                    usuario = (usuario ?? string.Empty).Trim(),
                    apoyo,
                    usuarioRegistro,
                    accion
                }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                result.Description = "Ok";
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
                const string query = @"select * from dbo.SIF_OFICINA_MIEMBROS_H where usuario = @usuario order by cod_historial desc";
                result.Result = connection.Query<SifOficinasHistorial>(query, new { usuario = filtro }).ToList();
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
        /// Método para consultar datos a exportar
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
                using var connection = new SqlConnection(stringConn);

                var search = filtros?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                var sortOrder = filtros?.sortOrder ?? 0; // 0=DESC, 1=ASC

                var offset = filtros?.pagina ?? 0;
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    fetch = int.MaxValue;
                }

                const string query = @"
                        SELECT cod_oficina,descripcion,COD_UNIDAD,Cod_Centro_Costo,Telefono_01,Telefono_02,DIRECCION,
                               Registro_Usuario,Registro_Fecha, Tipo,Oficina_Omision,Estado
                        FROM Sif_Oficinas
                        WHERE (@search IS NULL
                               OR cod_oficina LIKE @search
                               OR descripcion LIKE @search
                               OR Registro_Usuario LIKE @search)
                        ORDER BY
                            -- ASC
                            CASE WHEN @sortOrder = 1 AND @sortField = 'cod_oficina' THEN cod_oficina END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'descripcion' THEN descripcion END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'cod_unidad' THEN COD_UNIDAD END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'cod_centro_costo' THEN Cod_Centro_Costo END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_01' THEN Telefono_01 END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'telefono_02' THEN Telefono_02 END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'direccion' THEN DIRECCION END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'registro_usuario' THEN Registro_Usuario END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'registro_fecha' THEN Registro_Fecha END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'tipo' THEN Tipo END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'oficina_omision' THEN Oficina_Omision END ASC,
                            CASE WHEN @sortOrder = 1 AND @sortField = 'estado' THEN Estado END ASC,

                            -- DESC
                            CASE WHEN @sortOrder = 0 AND @sortField = 'cod_oficina' THEN cod_oficina END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'descripcion' THEN descripcion END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'cod_unidad' THEN COD_UNIDAD END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'cod_centro_costo' THEN Cod_Centro_Costo END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_01' THEN Telefono_01 END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'telefono_02' THEN Telefono_02 END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'direccion' THEN DIRECCION END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'registro_usuario' THEN Registro_Usuario END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'registro_fecha' THEN Registro_Fecha END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'tipo' THEN Tipo END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'oficina_omision' THEN Oficina_Omision END DESC,
                            CASE WHEN @sortOrder = 0 AND @sortField = 'estado' THEN Estado END DESC,

                            cod_oficina ASC
                        OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                result.Result = connection.Query<SifOficinasData>(query, new
                {
                    search = searchLike,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
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