using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using System.Data;


namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesAccesosDB
    {
        private readonly PortalDB _portalDB;

        public FrmTesAccesosDB(IConfiguration? config)
        {
            _portalDB = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Obtiene los grupos de bancos activos para la empresa especificada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosBancos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select rtrim(cod_grupo) as  'item', rtrim(Descripcion) as 'descripcion' 
                                             from TES_BANCOS_GRUPOS where Activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene las cuentas bancarias activas de la empresa especificada, filtradas por el código del banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosCuentas_Obtener(int CodEmpresa, string cod_banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select id_banco as 'item',rtrim(descripcion) as 'descripcion' 
                                     from Tes_Bancos where estado = 'A' and 
                                     (@cod_banco = '0000' or @cod_banco is null or  cod_grupo = @cod_banco )";
                var Result = conn.Query<DropDownListaGenericaModel>(query, new { cod_banco  = cod_banco }).ToList();

                return DbHelper.CreateOkResponse(Result);

            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }
        }

        /// <summary>
        /// Método para buscar y obtener los usuarios activos de la empresa especificada, con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAccesosUsuariosLista> Tes_AccesosUsuarioBuscar_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TesAccesosUsuariosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAccesosUsuariosLista
                {
                    total = 0,
                    lista = new List<DropDownListaGenericaModel>()
                }
            };

            try
            {

                var sortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["item"] = 1,
                    ["descripcion"] = 2,
                    ["nombre"] = 1
                };

                var lazy = LazyLoadHelper.Build(filtros, sortMap);

                // --- COUNT: SQL 100% estático ---
                var sqlCount = @"
            SELECT COUNT(1)
            FROM usuarios
            WHERE Estado = 'A'
              AND (
                    @hasFilter = 0
                 OR  Nombre      LIKE @filtro
                 OR  descripcion LIKE @filtro
              );";
                result.Result.total = conn.ExecuteScalar<int>(sqlCount, lazy.Params);

                // --- DATA: SQL 100% estático; ORDER BY con CASE + flags ---
                var sqlData = @"
            WITH base AS (
                SELECT
                    Nombre       AS item,
                    RTRIM(descripcion) AS descripcion
                FROM usuarios
                WHERE Estado = 'A'
                  AND (
                        @hasFilter = 0
                     OR  Nombre      LIKE @filtro
                     OR  descripcion LIKE @filtro
                  )
            )
            SELECT item, descripcion
            FROM base t
            ORDER BY
                -- item ASC/DESC
                CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN t.item END ASC,
                CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN t.item END DESC,
                -- descripcion ASC/DESC
                CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN t.descripcion END ASC,
                CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN t.descripcion END DESC
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;";

                result.Result.lista = conn.Query<DropDownListaGenericaModel>(sqlData, lazy.Params).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<DropDownListaGenericaModel>();
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<DropDownListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new DropDownListaGenericaModel()
            };
            try
            {
                scroll ??= 1; // si viene null, lo seteamos a 1

                // definimos flags para la dirección del scroll
                var anterior = scroll == 1;
                var siguiente = scroll == 2;

                // Query única, completamente parametrizada
                var sql = @"
                        SELECT TOP 1 Nombre AS item, descripcion AS descripcion
                        FROM usuarios
                        WHERE estado = 'A'
                          AND (
                                (@anterior = 1 AND Nombre < @nombre)
                             OR (@siguiente = 1 AND Nombre > @nombre)
                          )
                        ORDER BY
                            CASE 
                                WHEN @anterior = 1 THEN Nombre 
                            END DESC,
                            CASE 
                                WHEN @siguiente = 1 THEN Nombre 
                            END ASC;";

                response.Result = conn.QueryFirstOrDefault<DropDownListaGenericaModel>(
                    sql,
                    new { nombre, anterior, siguiente }
                );
                if (response.Result == null)
                {
                    response.Code = -1;
                    response.Description = "No se encontraron mas resultados";
                    response.Result = new DropDownListaGenericaModel();
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
        public ErrorDto<List<TesAccesosUsuariosData>> Tes_AccesosUsuarios_Obtener(int CodEmpresa, int cod_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select U.nombre,U.descripcion,U.estado,A.id_banco
                                    from Usuarios U left join tes_Banco_Asg A on U.nombre = A.nombre
                                    and A.id_banco = @cod_banco where U.estado = 'A' order by A.id_banco desc ";

                return conn.Query<TesAccesosUsuariosData>(query, new { cod_banco }).ToList();
            });
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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                //valida si el registro existe
                var qryExiste = $@"select isnull(count(*),0) as Existe from tes_Banco_Asg where id_banco = @id_bancos and nombre = @nombre";
                var existe = conn.QueryFirstOrDefault<int>(qryExiste, new { id_bancos = id_banco, nombre });
                if (existe == 0)
                {
                    var query = $@"insert tes_Banco_Asg(id_banco,nombre) values(@id_bancos,@nombre)";
                    conn.Execute(query, new { id_bancos = id_banco, nombre });
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
            var sql = @"
                        DELETE FROM TES_DOCUMENTOS_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_CONCEPTOS_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_UNIDAD_ASG WHERE id_banco = @id_banco AND nombre = @nombre;
                        DELETE FROM TES_BANCO_FIRMASAUT WHERE id_banco = @id_banco AND usuario = @nombre;
                        DELETE FROM tes_Banco_Asg WHERE id_banco = @id_banco AND nombre = @nombre;
                    ";

            var parameters = new { id_banco = id_banco, nombre = nombre };

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, parameters);
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
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select Nombre as 'item',Descripcion as 'descripcion' from usuarios WHERE Estado = 'A'";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Obtiene los usuarios asignados a los bancos de la empresa especificada, filtrados por el nombre del usuario y el grupo de banco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto<List<TesAccesosBancosData>> Tes_AccesosUserBancos_Obtener(int CodEmpresa, string nombre, string cod_grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<TesAccesosBancosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesAccesosBancosData>()
            };
            try
            {
                // Normaliza cod_grupo si viene null
                cod_grupo ??= "0000";


                var query = @"
                        SELECT B.id_banco, B.descripcion, B.cta, A.nombre
                        FROM Tes_Bancos B
                        LEFT JOIN tes_Banco_Asg A
                            ON B.id_banco = A.id_banco
                           AND A.nombre = @nombre
                        WHERE B.estado = 'A'
                          AND (@cod_grupo = '0000' OR B.cod_grupo = @cod_grupo)
                        ORDER BY A.nombre DESC;";
                response.Result = conn.Query<TesAccesosBancosData>(query, new { nombre, cod_grupo }).ToList();
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
            var sql = @"insert tes_Banco_Asg(id_banco,nombre) values(@id_bancos,@nombre)";

            var parameters = new { id_bancos = id_banco, nombre = nombre };

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, parameters);
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
           return Tes_AccesosCuentas_Eliminar(CodEmpresa, id_banco, nombre);
        }

        #endregion

        #region accesos

        /// <summary>
        /// Obtiene los accesos a bancos de un usuario específico en la empresa indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto<List<TesAccesosBancosData>> Tes_AccesosBancoUser_Obtener(int CodEmpresa, string nombre)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@" select B.id_banco,B.descripcion,B.cta,A.nombre
                                        from Tes_Bancos B inner join tes_Banco_Asg A on B.id_banco = A.id_banco
                                        and A.nombre = @nombre WHERE B.estado = 'A' order by A.nombre asc"; 

                return conn.Query<TesAccesosBancosData>(query, new { nombre }).ToList();
            });
        }

        public ErrorDto<List<TesAccesosDocumentosData>> Tes_AccesosDocumentos_Obtener(int CodEmpresa, string usuario, int id_banco )
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select T.Tipo,T.descripcion,isnull(A.Solicita,0) as Solicita,isnull(A.Autoriza,0) as Autoriza
                                   ,isnull(A.Genera,0) as Genera,isnull(A.asientos,0) as Asientos,isnull(A.ANULA,0) as Anula
                                    from tes_tipos_doc T left join tes_documentos_asg A on T.tipo = A.tipo
                                    and A.id_banco = @banco and A.nombre = @usuario
                                    Where T.tipo in(select Tipo from tes_banco_docs where id_banco = @banco)";

                return conn.Query<TesAccesosDocumentosData>(query, new { banco = id_banco, usuario }).ToList();
            });
        }

        public ErrorDto<List<TesAccesosConceptosData>> Tes_AccesosConceptos_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select C.cod_concepto,C.descripcion,A.id_Banco
                                               from tes_conceptos C left join tes_conceptos_asg A on C.cod_concepto = A.cod_concepto
                                               and A.id_banco = @banco and A.nombre = @usuario
                                               WHERE c.estado = 'A' Order by A.id_Banco desc";

                return conn.Query<TesAccesosConceptosData>(query, new { banco = id_banco, usuario }).ToList();
            });
        }

        public ErrorDto<List<TesAccesosUnidadesData>> Tes_AccesosUnidades_Obtener(int CodEmpresa, string usuario, int id_banco, int contabilidad)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select U.cod_unidad,U.descripcion,A.id_Banco
                                       from CntX_Unidades U left join tes_unidad_asg A on U.cod_unidad = A.cod_unidad
                                       and A.id_banco = @banco and A.nombre = @usuario 
                                       where U.cod_Contabilidad = @contabilidad and u.activa = 1 Order by A.id_Banco desc";

                return conn.Query<TesAccesosUnidadesData>(query, new
                {
                    banco = id_banco,
                    usuario,
                    contabilidad
                }).ToList();
            });
        }

        public ErrorDto<TesAccesosFirmasData> Tes_AccesosFirmas_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select * from TES_BANCO_FIRMASAUT where id_banco = @banco and usuario = @usuario";

                return conn.QueryFirstOrDefault<TesAccesosFirmasData>(query, new { banco = id_banco, usuario }) ?? new TesAccesosFirmasData();
            });
        }

        public ErrorDto Tes_AccesosDocumentos_Guardar(int CodEmpresa, string usuario, int id_banco, TesAccesosDocumentosData documento)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                //valida si existe
                var query = $@" select isnull(count(*),0) as Existe from tes_documentos_asg
                                  where nombre = @usuario and id_banco = @banco
                                  and Tipo = @tipo";
                var existe = conn.QueryFirstOrDefault<int>(query, new { usuario, banco = id_banco, documento.tipo });

                if (existe == 0)
                {
                    query = $@"insert into tes_documentos_asg(nombre,id_banco,tipo,solicita,autoriza,genera,asientos,anula)
                                  values(@usuario,@banco,@tipo, @solicita, @autoriza, @genera, @asientos, @anula)";
                }
                else
                {
                    query = $@"update tes_documentos_asg set solicita = @solicita, autoriza = @autoriza, 
                                  genera = @genera, asientos = @asientos, anula = @anula
                                  where nombre = @usuario and id_banco = @banco and tipo = @tipo";
                    
                }

                conn.Execute(query, new
                {
                    usuario,
                    banco = id_banco,
                    documento.tipo,
                    solicita = documento.solicita ? 1 : 0,
                    autoriza = documento.autoriza ? 1 : 0,
                    genera = documento.genera ? 1 : 0,
                    asientos = documento.asientos ? 1 : 0,
                    anula = documento.anula ? 1 : 0
                });

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Tes_AccesosConceptos_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked ,TesAccesosConceptosData concepto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = "";
                if (itemChecked)
                {
                    query = "delete tes_conceptos_asg where nombre = @usuario and cod_concepto = @cod_concepto and id_banco = @banco";
                    conn.Execute(query, new
                    {
                        usuario,
                        concepto.cod_concepto,
                        banco = id_banco
                    });

                    query = "insert tes_conceptos_asg(nombre,cod_concepto,id_banco) values(@usuario, @cod_concepto ,@banco)";
                }
                else
                {
                    query = "delete tes_conceptos_asg where nombre = @usuario and cod_concepto = @cod_concepto and id_banco = @banco";
                }
                conn.Execute(query, new
                {
                    usuario,
                    concepto.cod_concepto,
                    banco = id_banco
                });

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Tes_AccesosUnidades_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked, TesAccesosUnidadesData unidad)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = "";
                if (itemChecked)
                {
                    query = "delete tes_unidad_asg where nombre = @usuario and cod_unidad = @cod_unidad and id_banco = @banco";
                    conn.Execute(query, new
                    {
                        usuario,
                        unidad.cod_unidad,
                        banco = id_banco
                    });

                    query = "insert tes_unidad_asg(nombre,cod_unidad,id_banco) values(@usuario, @cod_unidad ,@banco)";
                }
                else
                {
                    query = "delete tes_unidad_asg where nombre = @usuario and cod_unidad = @cod_unidad and id_banco = @banco";
                }
                conn.Execute(query, new
                {
                    usuario,
                    unidad.cod_unidad,
                    banco = id_banco
                });

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Tes_AccesosFirmas_Guardar(int CodEmpresa, TesAccesosFirmasData firmas)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var query = "";
                //valida si existe
                query = $@"select count(*) as Existe from TES_BANCO_FIRMASAUT 
                                 where id_banco = @banco and usuario = @usuario";
                var existe = conn.QueryFirstOrDefault<int>(query, new { firmas.usuario, banco = firmas.id_banco });


                if (existe == 0)
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

                conn.Execute(query, new
                {
                    firmas.usuario,
                    banco = firmas.id_banco,
                    chkUserFirma = firmas.utiliza_firmas_autoriza ? 1 : 0,
                    chkFirmaRango = firmas.aplica_rango_autorizacion ? 1 : 0,
                    RngFirmasDesde = firmas.firmas_autoriza_inicio,
                    txtRngFirmasHasta = firmas.firmas_autoriza_corte
                });

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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {

                conn.Open();

                using var transaction = conn.BeginTransaction();

                // 1. Borrar accesos actuales del usuario destino
                var deleteQuery = @"
                        DELETE FROM TES_DOCUMENTOS_ASG WHERE nombre = @usuarioDestino;
                        DELETE FROM TES_UNIDAD_ASG WHERE nombre = @usuarioDestino;
                        DELETE FROM TES_CONCEPTOS_ASG WHERE nombre = @usuarioDestino;
                        DELETE FROM TES_BANCO_FIRMASAUT WHERE usuario = @usuarioDestino;
                        DELETE FROM TES_BANCO_ASG WHERE nombre = @usuarioDestino;
                    ";

                conn.Execute(deleteQuery, new { usuarioDestino }, transaction);

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

                conn.Execute(insertQuery, new { usuarioOrigen, usuarioDestino }, transaction);

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
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Permisos de usuarios inactivos eliminados correctamente."
            };

            try
            {

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

                response.Code = conn.Execute(query);
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
