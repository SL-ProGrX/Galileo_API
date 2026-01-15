using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesBancosGruposDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesBancosGruposDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            DBBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de grupos de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<TesBancosGruposLista> Tes_BancosGruposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            filtro ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<TesBancosGruposLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesBancosGruposLista
                {
                    total = 0,
                    data = new List<TesBancosGruposData>()
                }
            };

            try
            {
                var texto = filtro.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                // Paginación (offset es filas a saltar; si "pagina" es pageNumber cambia esta lógica)
                var offset = filtro.pagina;
                var fetch = filtro.paginacion;
                var usarPaginacion = fetch > 0;

                // ORDER BY seguro (whitelist)
                var sortField = (filtro.sortField ?? string.Empty).Trim();
                var orderByField = sortField switch
                {
                    "cod_grupo" => "cod_grupo",
                    "desc_Corta" => "desc_Corta",
                    "ID_SFN" => "ID_SFN",
                    "descripcion" => "descripcion",
                    "LCta_Interna" => "LCta_Interna",
                    "LCta_Interbancaria" => "LCta_Interbancaria",
                    "TCta_UTiliza" => "TCta_UTiliza",
                    "Activo" => "Activo",
                    "Firma_N1" => "Firma_N1",
                    "Firma_N2" => "Firma_N2",
                    _ => "cod_grupo"
                };

                var direction = filtro.sortOrder == 1 ? "DESC" : "ASC";

                const string sqlCount = @"
                        SELECT COUNT(1)
                        FROM Tes_Bancos_Grupos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(cod_grupo AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (desc_Corta LIKE @like);";

                response.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                });

                var sqlList = $@"
                        SELECT
                            '' AS dummy,
                            cod_grupo,
                            desc_Corta,
                            ID_SFN,
                            descripcion,
                            LCta_Interna,
                            LCta_Interbancaria,
                            TCta_UTiliza,
                            Activo,
                            Firma_N1,
                            Firma_N2
                        FROM Tes_Bancos_Grupos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(cod_grupo AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (desc_Corta LIKE @like)
                        ORDER BY {orderByField} {direction}";

                if (usarPaginacion)
                {
                    sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }

                response.Result.data = conn.Query<TesBancosGruposData>(sqlList, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.data = new List<TesBancosGruposData>();
            }

            return response;
        }

        /// <summary>
        /// Método para exportar los grupos de bancos a una lista.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TesBancosGruposData>> Tes_BancosGruposExportar_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = $@"select 
                                    cod_grupo ,
                                    desc_Corta ,
                                    ID_SFN ,
                                    descripcion ,
                                    LCta_Interna , 
                                    LCta_Interbancaria , 
                                    TCta_UTiliza ,
                                    Activo 
                                from Tes_Bancos_Grupos
                                    order by cod_grupo";

                return conn.Query<TesBancosGruposData>(query).ToList();
            });
        }

        /// <summary>
        /// Guarda una firma de banco en la base de datos.
        /// </summary>
        /// <param name="firma"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancoGrupoFirma_Guardar(TesBancosGruposImgData firma)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, firma.cod_empresa);
            var response = new ErrorDto();
            try
            {

                if (string.IsNullOrWhiteSpace(firma.cod_grupo) || string.IsNullOrWhiteSpace(firma.imagenLogo))
                {
                    response.Code = -1;
                    response.Description = "El código de grupo y la imagen no pueden estar vacíos.";
                    return response;
                }

                

                if (firma.firma_n1!.ToString()!.Contains(","))
                {
                    firma.firma_n1 = firma.firma_n1.ToString()!.Split(',')[1] ?? string.Empty; // Elimina el encabezado
                }

                if (firma.firma_n2!.ToString()!.Contains(","))
                {
                    firma.firma_n2 = firma.firma_n2.ToString()!.Split(',')[1] ?? string.Empty; // Elimina el encabezado
                }

                byte[] imageBytes1 = Convert.FromBase64String(firma.firma_n1.ToString() ?? string.Empty);
                byte[] imageBytes2 = Convert.FromBase64String(firma.firma_n2.ToString() ?? string.Empty);

                string query = "UPDATE Tes_Bancos_Grupos SET firma_n1 = @Imagen1, firma_n2 = @Imagen2 WHERE cod_Grupo = @cod_grupo ";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@Imagen1", SqlDbType.Image).Value = (object)imageBytes1 ?? DBNull.Value;
                    cmd.Parameters.Add("@Imagen2", SqlDbType.Image).Value = (object)imageBytes2 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@cod_grupo", firma.cod_grupo);

                    conn.Open();
                    response.Code = cmd.ExecuteNonQuery();
                }


                if (response.Code != 0)
                {
                    response.Code = 0;
                    response.Description = "Firma guardada correctamente.";
                }
                else
                {
                    response.Code = -1;
                    response.Description = "Error al guardar la firma.";
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
        /// Guarda un grupo de bancos en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancosGrupo_Guardar(int CodEmpresa, TesBancosGruposData banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                if (banco.cod_grupo == "")
                {
                    return DbHelper.ErrorResponse("El código del grupo no puede estar vacío."); 
                }

                // Verificar si el grupo ya existe
                var query = $@"select count(*) from Tes_Bancos_Grupos where cod_grupo = @CodGrupo";
                var exists = conn.ExecuteScalar<int>(query, new { CodGrupo = banco.cod_grupo });

                if (exists == 0)
                {
                    return BancoGrupoInsertar(CodEmpresa, banco);
                }
                else
                {
                    return BancoGrupoActualizar(CodEmpresa, banco);
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un nuevo grupo de bancos en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        private ErrorDto BancoGrupoInsertar(int CodEmpresa, TesBancosGruposData banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = @$"insert into Tes_Bancos_Grupos(
                                    cod_grupo,
                                    desc_Corta,
                                    ID_SFN,
                                    descripcion,
                                    LCta_Interna, 
                                    LCta_Interbancaria, 
                                    TCta_UTiliza,
                                    Activo,
                                    Registro_Usuario,
                                    Registro_Fecha) values(
                                    @cod_grupo,
                                    @desc_Corta,
                                    @ID_SFN,
                                    @descripcion,
                                    @LCta_Interna, 
                                    @LCta_Interbancaria, 
                                    @TCta_UTiliza,
                                    @Activo,
                                    @Registro_Usuario,
                                    dbo.MyGetDATE() )";

                int activo = banco.activo ? 1 : 0; // Convertir boolean a entero

                conn.Execute(query, new
                {
                    cod_grupo = banco.cod_grupo,
                    desc_Corta = banco.desc_corta,
                    ID_SFN = banco.id_sfn,
                    descripcion = banco.descripcion,
                    LCta_Interna = banco.lcta_interna,
                    LCta_Interbancaria = banco.lcta_interbancaria,
                    TCta_UTiliza = banco.tcta_utiliza,
                    Activo = activo,
                    Registro_Usuario = banco.registro_usuario // Usuario que registra
                });

                //bitácora
                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = banco.registro_usuario,
                    Modulo = vModulo,
                    Movimiento = "Registra",
                    DetalleMovimiento = $"Grupo Bancario: {banco.cod_grupo} insertado."
                });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Actualiza un grupo de bancos en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        private ErrorDto BancoGrupoActualizar(int CodEmpresa, TesBancosGruposData banco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"update Tes_Bancos_Grupos set
                                    desc_Corta = @desc_Corta,
                                    ID_SFN = @ID_SFN,
                                    descripcion = @descripcion,
                                    LCta_Interna = @LCta_Interna, 
                                    LCta_Interbancaria = @LCta_Interbancaria, 
                                    TCta_UTiliza = @TCta_UTiliza,
                                    Activo = @Activo
                                where cod_grupo = @cod_grupo";
                int activo = banco.activo ? 1 : 0; // Convertir booleano a entero

                conn.Execute(query, new
                {
                    cod_grupo = banco.cod_grupo,
                    desc_Corta = banco.desc_corta,
                    ID_SFN = banco.id_sfn,
                    descripcion = banco.descripcion,
                    LCta_Interna = banco.lcta_interna,
                    LCta_Interbancaria = banco.lcta_interbancaria,
                    TCta_UTiliza = banco.tcta_utiliza,
                    Activo = activo
                });

                //bitácora
                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = banco.registro_usuario,
                    Modulo = vModulo,
                    Movimiento = "Modifica",
                    DetalleMovimiento = $"Grupo Bancario: {banco.cod_grupo} actualizado."
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            
        }

        /// <summary>
        /// Elimina un grupo de bancos de la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancoGrupo_Eliminar(int CodEmpresa, string cod_grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                //valido si el grupo existe usado en otras tablas
                var queryCheck = $@"select count(*) from Tes_Bancos where cod_grupo = @cod_grupo";
                var exists = conn.ExecuteScalar<int>(queryCheck, new { cod_grupo });
                if (exists > 0)
                {
                    return DbHelper.ErrorResponse("No se puede eliminar el grupo bancario porque está siendo utilizado por uno o más bancos.");
                }

                var query = $@"delete from Tes_Bancos_Grupos where cod_grupo = @cod_grupo";
                conn.Execute(query, new { cod_grupo });
                //bitácora
                DBBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = "Sistema", // Usuario que elimina
                    Modulo = vModulo,
                    Movimiento = "Elimina",
                    DetalleMovimiento = $"Grupo Bancario: {cod_grupo} eliminado."
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Método para validar si un grupo de bancos ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancosGrupo_Valida(int CodEmpresa, string cod_grupo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                //valido si el grupo existe usado en otras tablas
                var queryCheck = $@"select count(*) from Tes_Bancos_Grupos where UPPER(cod_grupo) = @cod_grupo";
                var exists = conn.ExecuteScalar<int>(queryCheck, new { cod_grupo = cod_grupo.ToUpper() });
                if (exists > 0)
                {
                    return DbHelper.ErrorResponse("El grupo bancario ya existe.");
                }
                else
                {
                    return DbHelper.OkResponse("El grupo bancario está disponible.");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

    }
}
