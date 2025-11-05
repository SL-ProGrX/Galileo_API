using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_AccesosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly mProGrX_AuxiliarDB _utils;

        public frmTES_AccesosDB(IConfiguration? config)
        {
            _config = config;
            _utils = new mProGrX_AuxiliarDB(_config);
        }

        /// <summary>
        /// Obtiene los grupos de bancos activos para la empresa especificada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosBancos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(cod_grupo) as  'item', rtrim(Descripcion) as 'descripcion' from TES_BANCOS_GRUPOS where Activo = 1";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene las cuentas bancarias activas de la empresa especificada, filtradas por el código del banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosCuentas_Obtener(int CodEmpresa, string cod_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    if (cod_banco != "0000")
                    {
                        where = $" and cod_grupo = '{cod_banco}' ";
                    }

                    var query = $@"select id_banco as 'item',rtrim(descripcion) as 'descripcion' 
                                     from Tes_Bancos where estado = 'A' {where}";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Metodo para buscar y obtener los usuarios activos de la empresa especificada, con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<tesAccesosUsuariosLista> Tes_AccesosUsuarioBuscar_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<tesAccesosUsuariosLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new tesAccesosUsuariosLista()
                {
                    total = 0,
                    lista = new List<DropDownListaGenericaModel>()
                }
            };
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(Nombre) from usuarios WHERE Estado = 'A'";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " AND ( Nombre LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "item";
                    }

                    query = $@"select item, descripcion from ( 
                                select Nombre as 'item',descripcion as 'descripcion' 
                                     from usuarios where estado = 'A'  
                                        {filtros.filtro} ) t
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Busca y obtiene un usuario activo de la empresa especificada, con paginación hacia adelante o hacia atrás.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> Tes_AccesosUsuarioBuscar_scroll(int CodEmpresa, string nombre, int? scroll)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<DropDownListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new DropDownListaGenericaModel()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    scroll = (scroll == null) ? 1 : scroll;
                    if (scroll == 1) //busca el registro anterior
                    {
                        where = $" AND Nombre < '{nombre}' ORDER BY NOMBRE desc";
                    }
                    else if (scroll == 2) //busca el registro siguiente
                    {
                        where = $" AND Nombre > '{nombre}' ORDER BY NOMBRE ASC";
                    }

                    var query = $@"select top 1 Nombre as 'item',descripcion as 'descripcion' from usuarios where estado = 'A' {where} ";
                    response.Result = connection.QueryFirstOrDefault<DropDownListaGenericaModel>(query);

                    if(response.Result == null)
                    {
                        response.Code = -1;
                        response.Description = "No se encontraron mas resultados";
                        response.Result = new DropDownListaGenericaModel();
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

        #region cuentas

        /// <summary>
        /// Obtiene los usuarios con acceso a las cuentas bancarias de la empresa especificada y el banco indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<tesAccesosUsuariosData>> Tes_AccesosUsuarios_Obtener(int CodEmpresa, int cod_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesAccesosUsuariosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<tesAccesosUsuariosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select U.nombre,U.descripcion,U.estado,A.id_banco
                                    from Usuarios U left join tes_Banco_Asg A on U.nombre = A.nombre
                                    and A.id_banco = {cod_banco} where U.estado = 'A' order by A.id_banco desc ";
                    response.Result = connection.Query<tesAccesosUsuariosData>(query).ToList();
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
        /// Asigna un usuario a un banco específico en la empresa indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto Tes_AccesosCuentas_Asignar(int CodEmpresa, int id_banco, string nombre)
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
                    //valida si el registro existe
                    var qryExiste = $@"select isnull(count(*),0) as Existe from tes_Banco_Asg where id_banco = @id_bancos and nombre = @nombre";
                    var existe = connection.QueryFirstOrDefault<int>(qryExiste, new { id_bancos = id_banco, nombre = nombre });
                    if (existe == 0)
                    {
                        var query = $@"insert tes_Banco_Asg(id_banco,nombre) values(@id_bancos,@nombre)";
                        connection.Execute(query, new { id_bancos = id_banco, nombre = nombre });
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
        /// Elimina un usuario asignado a un banco específico en la empresa indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto Tes_AccesosCuentas_Eliminar(int CodEmpresa, int id_banco, string nombre)
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

                var query = @"
                        DELETE FROM TES_DOCUMENTOS_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_CONCEPTOS_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_UNIDAD_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_BANCO_FIRMASAUT WHERE id_banco = @id_banco AND usuario = @nombre;
                        DELETE FROM tes_Banco_Asg WHERE id_banco = @id_banco AND nombre = @nombre;
                    ";

                connection.Execute(query, new { id_banco, nombre });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        #region usuarios

        /// <summary>
        /// Obtiene la lista de usuarios activos de la empresa especificada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosUsuarioLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Nombre as 'item',Descripcion as 'descripcion' from usuarios WHERE Estado = 'A'";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene los usuarios asignados a los bancos de la empresa especificada, filtrados por el nombre del usuario y el grupo de banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto<List<tesAccesosBancosData>> Tes_AccesosUserBancos_Obtener(int CodEmpresa, string nombre, string cod_grupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesAccesosBancosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<tesAccesosBancosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    if (cod_grupo != "0000")
                    {
                        where = $" where B.cod_grupo = '{cod_grupo}' and B.estado = 'A'";
                    }
                    else
                    {
                        where = $" where B.estado = 'A'";
                    }

                        var query = $@"select B.id_banco,B.descripcion,B.cta,A.nombre
                                    from Tes_Bancos B left join tes_Banco_Asg A on B.id_banco = A.id_banco
                                    and A.nombre = @nombre {where}  order by A.nombre desc";
                    response.Result = connection.Query<tesAccesosBancosData>(query, new { nombre }).ToList();
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
        /// Asigna un usuario a un banco específico en la empresa indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto Tes_AccesosUsuarios_Asignar(int CodEmpresa, int id_banco, string nombre)
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
                    var query = $@"insert tes_Banco_Asg(id_banco,nombre) values(@id_bancos,@nombre)";
                    connection.Execute(query, new { id_bancos = id_banco, nombre = nombre });
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
        /// Elimina un usuario asignado a un banco específico en la empresa indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_banco"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto Tes_AccesosUsuarios_Eliminar(int CodEmpresa, int id_banco, string nombre)
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

                var query = @"
                        DELETE FROM TES_DOCUMENTOS_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_CONCEPTOS_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_UNIDAD_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_BANCO_FIRMASAUT WHERE id_banco = @id_banco AND usuario = @nombre;
                        DELETE FROM tes_Banco_Asg WHERE id_banco = @id_banco AND nombre = @nombre;
                    ";

                connection.Execute(query, new { id_banco, nombre });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        #region accesos

        /// <summary>
        /// Obtiene los accesos a bancos de un usuario específico en la empresa indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto<List<tesAccesosBancosData>> Tes_AccesosBancoUser_Obtener(int CodEmpresa, string nombre)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesAccesosBancosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<tesAccesosBancosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@" select B.id_banco,B.descripcion,B.cta,A.nombre
                                        from Tes_Bancos B inner join tes_Banco_Asg A on B.id_banco = A.id_banco
                                        and A.nombre = @nombre WHERE B.estado = 'A' order by A.nombre asc";
                    response.Result = connection.Query<tesAccesosBancosData>(query, new { nombre }).ToList();
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

        public ErrorDto<List<tesAccesosDocumentosData>> Tes_AccesosDocumentos_Obtener(int CodEmpresa, string usuario, int id_banco )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesAccesosDocumentosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<tesAccesosDocumentosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select T.Tipo,T.descripcion,isnull(A.Solicita,0) as Solicita,isnull(A.Autoriza,0) as Autoriza
                                   ,isnull(A.Genera,0) as Genera,isnull(A.asientos,0) as Asientos,isnull(A.ANULA,0) as Anula
                                    from tes_tipos_doc T left join tes_documentos_asg A on T.tipo = A.tipo
                                    and A.id_banco = @banco and A.nombre = @usuario
                                    Where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)";
                    response.Result = connection.Query<tesAccesosDocumentosData>(query, new { banco = id_banco, usuario = usuario }).ToList();
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

        public ErrorDto<List<tesAccesosConceptosData>> Tes_AccesosConceptos_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesAccesosConceptosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<tesAccesosConceptosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.cod_concepto,C.descripcion,A.id_Banco
                                               from tes_conceptos C left join tes_conceptos_asg A on C.cod_concepto = A.cod_concepto
                                               and A.id_banco = @banco and A.nombre = @usuario
                                               WHERE c.estado = 'A' Order by A.id_Banco desc";
                    response.Result = connection.Query<tesAccesosConceptosData>(query, new { banco = id_banco, usuario = usuario }).ToList();
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

        public ErrorDto<List<tesAccesosUnidadesData>> Tes_AccesosUnidades_Obtener(int CodEmpresa, string usuario, int id_banco, int contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<tesAccesosUnidadesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<tesAccesosUnidadesData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select U.cod_unidad,U.descripcion,A.id_Banco
                                       from CntX_Unidades U left join tes_unidad_asg A on U.cod_unidad = A.cod_unidad
                                       and A.id_banco = @banco and A.nombre = @usuario 
                                       where U.cod_Contabilidad = @contabilidad and u.activa = 1 Order by A.id_Banco desc";
                    response.Result = connection.Query<tesAccesosUnidadesData>(query, new { 
                        banco = id_banco, 
                        usuario = usuario,
                        contabilidad = contabilidad }).ToList();
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

        public ErrorDto<tesAccesosFirmasData> Tes_AccesosFirmas_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<tesAccesosFirmasData>
            {
                Code = 0,
                Description = "Ok",
                Result = new tesAccesosFirmasData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from TES_BANCO_FIRMASAUT where id_banco = @banco and usuario = @usuario";
                    response.Result = connection.QueryFirstOrDefault<tesAccesosFirmasData>(query, new { banco = id_banco, usuario = usuario });
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

        public ErrorDto Tes_AccesosDocumentos_Guardar(int CodEmpresa, string usuario, int id_banco, tesAccesosDocumentosData documento)
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
                    //valida si existe
                    var query = $@" select isnull(count(*),0) as Existe from tes_documentos_asg
                                  where nombre = @usuario and id_banco = @banco
                                  and Tipo = @tipo";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { usuario, banco = id_banco, tipo = documento.tipo });
                
                    if(existe == 0)
                    {
                        query = $@"insert into tes_documentos_asg(nombre,id_banco,tipo,solicita,autoriza,genera,asientos,anula)
                                  values(@usuario,@banco,@tipo, @solicita, @autoriza, @genera, @asientos, @anula)";
                        connection.Execute(query, new
                        {
                            usuario = usuario,
                            banco = id_banco,
                            tipo = documento.tipo,
                            solicita = documento.solicita ? 1 : 0,
                            autoriza = documento.autoriza ? 1 : 0,
                            genera = documento.genera ? 1 : 0,
                            asientos = documento.asientos ? 1 : 0,
                            anula = documento.anula ? 1 : 0
                        });
                    }
                    else
                    {
                        query = $@"update tes_documentos_asg set solicita = @solicita, autoriza = @autoriza, 
                                  genera = @genera, asientos = @asientos, anula = @anula
                                  where nombre = @usuario and id_banco = @banco and tipo = @tipo";
                        connection.Execute(query, new
                        {
                            usuario = usuario,
                            banco = id_banco,
                            tipo = documento.tipo,
                            solicita = documento.solicita ? 1 : 0,
                            autoriza = documento.autoriza ? 1 : 0,
                            genera = documento.genera ? 1 : 0,
                            asientos = documento.asientos ? 1 : 0,
                            anula = documento.anula ? 1 : 0
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

        public ErrorDto Tes_AccesosConceptos_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked ,tesAccesosConceptosData concepto)
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
                    var query = "";
                    if (itemChecked)
                    {
                        query = "delete tes_conceptos_asg where nombre = @usuario and cod_concepto = @cod_concepto and id_banco = @banco";
                        connection.Execute(query, new
                        {
                            usuario = usuario,
                            cod_concepto = concepto.cod_concepto,
                            banco = id_banco
                        });

                        query = "insert tes_conceptos_asg(nombre,cod_concepto,id_banco) values(@usuario, @cod_concepto ,@banco)";
                    }
                    else
                    {
                        query = "delete tes_conceptos_asg where nombre = @usuario and cod_concepto = @cod_concepto and id_banco = @banco";
                    }
                    connection.Execute(query, new
                    {
                        usuario = usuario,
                        cod_concepto = concepto.cod_concepto,
                        banco = id_banco
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

        public ErrorDto Tes_AccesosUnidades_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked, tesAccesosUnidadesData unidad)
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
                    var query = "";
                    if (itemChecked)
                    {
                        query = "delete tes_unidad_asg where nombre = @usuario and cod_unidad = @cod_unidad and id_banco = @banco";
                        connection.Execute(query, new
                        {
                            usuario = usuario,
                            cod_unidad = unidad.cod_unidad,
                            banco = id_banco
                        });

                        query = "insert tes_unidad_asg(nombre,cod_unidad,id_banco) values(@usuario, @cod_unidad ,@banco)";
                    }
                    else
                    {
                        query = "delete tes_unidad_asg where nombre = @usuario and cod_unidad = @cod_unidad and id_banco = @banco";
                    }
                    connection.Execute(query, new
                    {
                        usuario = usuario,
                        cod_unidad = unidad.cod_unidad,
                        banco = id_banco
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

        public ErrorDto Tes_AccesosFirmas_Guardar(int CodEmpresa, tesAccesosFirmasData firmas)
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
                    //valida si existe
                    query = $@"select count(*) as Existe from TES_BANCO_FIRMASAUT 
                                 where id_banco = @banco and usuario = @usuario";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { usuario = firmas.usuario, banco = firmas.id_banco });
                    

                    if (existe == 0 )
                    {
                        query = $@"
                                    insert TES_BANCO_FIRMASAUT(
                                    usuario,
                                    id_banco,
                                    UTILIZA_FIRMAS_AUTORIZA,
                                    APLICA_RANGO_AUTORIZACION,
                                    FIRMAS_AUTORIZA_INICIO,
                                    FIRMAS_AUTORIZA_CORTE
                                    ) values(
                                    @usuario,
                                    @banco,
                                    @chkUserFirma,
                                    @chkFirmaRango,
                                    @RngFirmasDesde,
                                    @txtRngFirmasHasta
                                    )";
                    }
                    else
                    {
                        if (firmas.utiliza_firmas_autoriza)
                        {
                            query = $@"
                                    update TES_BANCO_FIRMASAUT set 
                                    UTILIZA_FIRMAS_AUTORIZA = @chkUserFirma,
                                    APLICA_RANGO_AUTORIZACION = @chkFirmaRango,
                                    FIRMAS_AUTORIZA_INICIO = @RngFirmasDesde,
                                    FIRMAS_AUTORIZA_CORTE = @txtRngFirmasHasta
                                    where id_banco = @banco and usuario = @usuario";
                        }
                        else
                        {
                            query = $@"delete TES_BANCO_FIRMASAUT where id_banco = @banco and usuario = @usuario";
                        }
                    }

                    connection.Execute(query, new
                    {
                        usuario = firmas.usuario,
                        banco = firmas.id_banco,
                        chkUserFirma = firmas.utiliza_firmas_autoriza ? 1 : 0,
                        chkFirmaRango = firmas.aplica_rango_autorizacion ? 1 : 0,
                        RngFirmasDesde = firmas.firmas_autoriza_inicio,
                        txtRngFirmasHasta = firmas.firmas_autoriza_corte
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

        #endregion

        #region copia

        public ErrorDto Tes_AccesosUsuarios_Copiar(int CodEmpresa, string usuarioOrigen, string usuarioDestino)
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
                connection.Open();

                using var transaction = connection.BeginTransaction();

                // 1. Borrar accesos actuales del usuario destino
                var deleteQuery = @"
                        DELETE FROM TES_DOCUMENTOS_ASG WHERE nombre = @usuarioDestino;
                        DELETE FROM TES_UNIDAD_ASG WHERE nombre = @usuarioDestino;
                        DELETE FROM TES_CONCEPTOS_ASG WHERE nombre = @usuarioDestino;
                        DELETE FROM TES_BANCO_FIRMASAUT WHERE usuario = @usuarioDestino;
                        DELETE FROM TES_BANCO_ASG WHERE nombre = @usuarioDestino;
                    ";

                connection.Execute(deleteQuery, new { usuarioDestino }, transaction);

                // 2. Insertar accesos del usuario origen al usuario destino
                var insertQuery = @"
                        INSERT INTO TES_BANCO_ASG(id_banco, nombre)
                        SELECT id_banco, @usuarioDestino FROM TES_BANCO_ASG WHERE nombre = @usuarioOrigen;

                        INSERT INTO TES_BANCO_FIRMASAUT(id_banco, usuario, UTILIZA_FIRMAS_AUTORIZA, APLICA_RANGO_AUTORIZACION, FIRMAS_AUTORIZA_INICIO, FIRMAS_AUTORIZA_CORTE)
                        SELECT id_banco, @usuarioDestino, UTILIZA_FIRMAS_AUTORIZA, APLICA_RANGO_AUTORIZACION, FIRMAS_AUTORIZA_INICIO, FIRMAS_AUTORIZA_CORTE
                        FROM TES_BANCO_FIRMASAUT WHERE usuario = @usuarioOrigen;

                        INSERT INTO TES_DOCUMENTOS_ASG(nombre, tipo, id_banco, solicita, autoriza, genera, asientos, anula)
                        SELECT @usuarioDestino, tipo, id_banco, solicita, autoriza, genera, asientos, anula
                        FROM TES_DOCUMENTOS_ASG WHERE nombre = @usuarioOrigen;

                        INSERT INTO TES_CONCEPTOS_ASG(nombre, id_banco, cod_concepto)
                        SELECT @usuarioDestino, id_banco, cod_concepto FROM TES_CONCEPTOS_ASG WHERE nombre = @usuarioOrigen;

                        INSERT INTO TES_UNIDAD_ASG(nombre, id_banco, cod_unidad)
                        SELECT @usuarioDestino, id_banco, cod_unidad FROM TES_UNIDAD_ASG WHERE nombre = @usuarioOrigen;
                    ";

                connection.Execute(insertQuery, new { usuarioOrigen, usuarioDestino }, transaction);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto Tes_AccesosUsuarios_EliminarInactivos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Permisos de usuarios inactivos eliminados correctamente."
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = @"
                            DELETE FROM TES_DOCUMENTOS_ASG 
                            WHERE nombre IN (SELECT nombre FROM usuarios WHERE estado = 'I');

                            DELETE FROM TES_CONCEPTOS_ASG 
                            WHERE nombre IN (SELECT nombre FROM usuarios WHERE estado = 'I');

                            DELETE FROM TES_UNIDAD_ASG 
                            WHERE nombre IN (SELECT nombre FROM usuarios WHERE estado = 'I');

                            DELETE FROM TES_BANCO_FIRMASAUT 
                            WHERE usuario IN (SELECT nombre FROM usuarios WHERE estado = 'I');

                            DELETE FROM TES_BANCO_ASG 
                            WHERE nombre IN (SELECT nombre FROM usuarios WHERE estado = 'I');
                        ";

                response.Code = connection.Execute(query);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al eliminar accesos: {ex.Message}";
            }

            return response;
        }

        #endregion
    }
}
