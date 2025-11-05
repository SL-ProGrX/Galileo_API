using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using System.Data;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_BancosGruposDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly mImagenes mImagenes;
        private readonly mSecurityMainDb DBBitacora;

        public frmTES_BancosGruposDB(IConfiguration config)
        {
            _config = config;
            mImagenes = new mImagenes(config);
            DBBitacora = new mSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de grupos de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<TES_BancosGruposLista> Tes_BancosGruposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TES_BancosGruposLista>();
            response.Result = new TES_BancosGruposLista();
            response.Result.data = new List<TES_BancosGruposData>();
            response.Result.total = 0;
            

            try
            {
                string paginaActual = " ", paginacionActual = " ", valWhere = " ";

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select COUNT(cod_grupo) from Tes_Bancos_Grupos";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro.filtro != null)
                    {
                        valWhere += " WHERE ( cod_grupo LIKE '%" + filtro.filtro + "%' OR" +
                                            " DESCRIPCION LIKE '%" + filtro.filtro + "%' OR " +
                                            "desc_Corta LIKE '%" + filtro.filtro + "%') ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "cod_grupo";
                    }

                    query = $@"select '',cod_grupo,desc_Corta,ID_SFN,descripcion,LCta_Interna, LCta_Interbancaria, 
                                  TCta_UTiliza,Activo, Firma_N1, Firma_N2 from Tes_Bancos_Grupos  {valWhere}
                                    order by {filtro.sortField} {(filtro.sortOrder == 1 ? "DESC" : "ASC")} {paginaActual} {paginacionActual}";

                    response.Result.data = connection.Query<TES_BancosGruposData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.data = new List<TES_BancosGruposData>();
            }
            return response;
        }

        /// <summary>
        /// Metodo para exportar los grupos de bancos a una lista.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TES_BancosGruposData>> Tes_BancosGruposExportar_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TES_BancosGruposData>>();

            try
            {
                
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"select 
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

                    response.Result = connection.Query<TES_BancosGruposData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new List<TES_BancosGruposData>();
            }
            return response;
        }

        /// <summary>
        /// Guarda una firma de banco en la base de datos.
        /// </summary>
        /// <param name="firma"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancoGrupoFirma_Guardar(TES_BancosGruposImgData firma)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(firma.cod_empresa);
            var response = new ErrorDto();
            string pFirma1 = "";
            byte[] imgByteF1 = null;
            byte[] imgByteF2 = null;
            try
            {

                if (string.IsNullOrWhiteSpace(firma.cod_grupo) || string.IsNullOrWhiteSpace(firma.imagenLogo))
                {
                    response.Code = -1;
                    response.Description = "El código de grupo y la imagen no pueden estar vacíos.";
                    return response;
                }

                //Metodo anterior con ruta de imagen

                //var query = $@"select * from Tes_Bancos_Grupos where cod_Grupo = '{firma.cod_grupo}' ";

                //switch (firma.firmaSelect)
                //{
                //    case 1: // Firma N1
                //    default:

                //        pFirma = "firma_n1";
                //        break;
                //    case 2: // Firma N2
                //        pFirma = "firma_n2";
                //        break;
                //}

                // var resp = mImagenes.fxImagen_Guardar(firma.cod_empresa, query, pFirma, firma.imagenLogo);

                if (firma.firma_n1.ToString().Contains(","))
                {
                    firma.firma_n1 = firma.firma_n1.ToString().Split(',')[1]; // Elimina el encabezado
                }

                if (firma.firma_n2.ToString().Contains(","))
                {
                    firma.firma_n2 = firma.firma_n2.ToString().Split(',')[1]; // Elimina el encabezado
                }

                byte[] imageBytes1 = Convert.FromBase64String(firma.firma_n1.ToString());
                byte[] imageBytes2 = Convert.FromBase64String(firma.firma_n2.ToString());

                using (SqlConnection conn = new SqlConnection(stringConn))
                {
                    string query = "UPDATE Tes_Bancos_Grupos SET firma_n1 = @Imagen1, firma_n2 = @Imagen2 WHERE cod_Grupo = @cod_grupo ";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@Imagen1", SqlDbType.Image).Value = (object)imageBytes1 ?? DBNull.Value;
                        cmd.Parameters.Add("@Imagen2", SqlDbType.Image).Value = (object)imageBytes2 ?? DBNull.Value;
                        cmd.Parameters.AddWithValue("@cod_grupo", firma.cod_grupo);

                        conn.Open();
                        response.Code = cmd.ExecuteNonQuery();
                    }
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
        public ErrorDto Tes_BancosGrupo_Guardar(int CodEmpresa, TES_BancosGruposData banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                if (banco.cod_grupo == "")
                {
                    response.Code = -1;
                    response.Description = "El código del grupo no puede estar vacío.";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    // Verificar si el grupo ya existe
                    var query = $@"select count(*) from Tes_Bancos_Grupos where cod_grupo = @CodGrupo";
                    var exists = connection.ExecuteScalar<int>(query, new { CodGrupo = banco.cod_grupo });

                    if(exists == 0)
                    {
                        response = BancoGrupoInsertar(CodEmpresa, banco);
                    }
                    else
                    {
                        response = BancoGrupoActualizar(CodEmpresa, banco);
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
        /// Inserta un nuevo grupo de bancos en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        private ErrorDto BancoGrupoInsertar(int CodEmpresa, TES_BancosGruposData banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
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

                    response.Code = connection.Execute(query, new
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
                    DBBitacora.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = banco.registro_usuario,
                        Modulo = vModulo,
                        Movimiento = "Registra",
                        DetalleMovimiento = $"Grupo Bancario: {banco.cod_grupo} insertado."
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
        /// Actualiza un grupo de bancos en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        private ErrorDto BancoGrupoActualizar(int CodEmpresa, TES_BancosGruposData banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
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
                   
                    response.Code = connection.Execute(query, new
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
                    DBBitacora.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = banco.registro_usuario,
                        Modulo = vModulo,
                        Movimiento = "Modifica",
                        DetalleMovimiento = $"Grupo Bancario: {banco.cod_grupo} actualizado."
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
        /// Elimina un grupo de bancos de la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancoGrupo_Eliminar(int CodEmpresa, string cod_grupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valido si el grupo existe usado en otras tablas
                    var queryCheck = $@"select count(*) from Tes_Bancos where cod_grupo = @cod_grupo";
                    var exists = connection.ExecuteScalar<int>(queryCheck, new { cod_grupo });
                    if (exists > 0)
                    {
                        response.Code = -1;
                        response.Description = "No se puede eliminar el grupo bancario porque está siendo utilizado por uno o más bancos.";
                        return response;
                    }

                    var query = $@"delete from Tes_Bancos_Grupos where cod_grupo = @cod_grupo";
                    response.Code = connection.Execute(query, new { cod_grupo });
                    //bitácora
                    DBBitacora.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = "Sistema", // Usuario que elimina
                        Modulo = vModulo,
                        Movimiento = "Elimina",
                        DetalleMovimiento = $"Grupo Bancario: {cod_grupo} eliminado."
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
        /// Método para validar si un grupo de bancos ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto Tes_BancosGrupo_Valida(int CodEmpresa, string cod_grupo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valido si el grupo existe usado en otras tablas
                    var queryCheck = $@"select count(*) from Tes_Bancos_Grupos where UPPER(cod_grupo) = @cod_grupo";
                    var exists = connection.ExecuteScalar<int>(queryCheck, new { cod_grupo = cod_grupo.ToUpper() });
                    if(exists > 0)
                    {
                        response.Code = -1;
                        response.Description = "El grupo bancario ya existe.";
                    }
                    else
                    {
                        response.Code = 0;
                        response.Description = "El grupo bancario está disponible.";
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

    }
}
