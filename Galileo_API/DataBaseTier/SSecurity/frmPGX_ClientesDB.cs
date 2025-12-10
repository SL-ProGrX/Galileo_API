using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public class FrmPgxClientesDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        readonly MSecurityMainDb DBBitacora;

        public FrmPgxClientesDb(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }


        #region CLIENTES

        public ClientesDataLista Clientes_Obtener(int? pagina, int? paginacion, string? filtro)
        {

            ClientesDataLista info = new ClientesDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    //Busco Total
                    var countQuery = "SELECT COUNT(*) FROM PGX_CLIENTES";
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        countQuery += " WHERE COD_EMPRESA LIKE @Filtro OR NOMBRE_LARGO LIKE @Filtro OR NOMBRE_CORTO LIKE @Filtro ";
                    }
                    info.Total = connection.Query<int>(countQuery, parameters).FirstOrDefault();

                    var whereClause = "";
                    var parameters = new DynamicParameters();
                    if (!string.IsNullOrEmpty(filtro))
                    {
                        whereClause = " WHERE COD_EMPRESA LIKE @Filtro OR NOMBRE_LARGO LIKE @Filtro OR NOMBRE_CORTO LIKE @Filtro ";
                        parameters.Add("Filtro", "%" + filtro + "%");
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT * FROM PGX_CLIENTES
                                        {whereClause}
                                        ORDER BY COD_EMPRESA
                                        {paginaActual}
                                        {paginacionActual}";
                    info.Lista = connection.Query<ClienteDto>(query, parameters).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public ClienteDto Cliente_Obtener(int CodEmpresa)
        {
            ClienteDto resp = new();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "SELECT * FROM PGX_CLIENTES WHERE cod_empresa = @CodEmpresa";
                    var parameters = new { CodEmpresa };
                    resp = connection.Query<ClienteDto>(query, parameters).FirstOrDefault() ?? new ClienteDto();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Consulta siguiente Empresa #
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ClienteDto ConsultaAscDesc(int CodEmpresa, string tipo)
        {


            ClienteDto info = new ClienteDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (CodEmpresa == 0)
                        {
                            query = $@"select Top 1 cod_Empresa from PGX_Clientes
                                    order by cod_Empresa desc";
                        }
                        else
                        {
                            query = $@"select Top 1 cod_Empresa from PGX_Clientes
                                    where cod_Empresa < '{CodEmpresa}' order by cod_Empresa desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 cod_Empresa from PGX_Clientes
                                    where cod_Empresa > '{CodEmpresa}' order by cod_Empresa asc";
                    }


                    info = connection.Query<ClienteDto>(query).FirstOrDefault() ?? new ClienteDto();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public ErrorDto Cliente_Modificar(ClienteDto info)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    connection.Open();

                    // Update the existing client record
                    var updateSql = @"UPDATE PGX_Clientes SET 
                                cod_vendedor = @cod_vendedor,
                                nombre_largo = @nombre_largo,
                                nombre_corto = @nombre_corto,
                                logo_cliente = @logo_cliente,
                                estado = @estado,
                                identificacion = @identificacion,
                                email_01 = @email_01,
                                email_02 = @email_02,
                                tel_cell = @tel_cell,
                                tel_trabajo = @tel_trabajo,
                                tel_auxiliar = @tel_auxiliar,
                                web_site = @web_site,
                                facebook = @facebook,
                                suscripcion_inicial = @suscripcion_inicial,
                                suscripcion_vence = @suscripcion_vence,
                                suscripcion_mensualidad = @suscripcion_mensualidad,
                                suscripcion_anual = @suscripcion_anual,
                                pgx_core_server = @pgx_core_server,
                                pgx_core_db = @pgx_core_db,
                                pgx_core_user = @pgx_core_user,
                                pgx_core_key = @pgx_core_key,
                                pgx_analisis_server = @pgx_analisis_server,
                                pgx_analisis_db = @pgx_analisis_db,
                                pgx_analisis_user = @pgx_analisis_user,
                                pgx_analisis_key = @pgx_analisis_key,
                                pgx_auxiliar_server = @pgx_auxiliar_server,
                                pgx_auxiliar_db = @pgx_auxiliar_db,
                                pgx_auxiliar_user = @pgx_auxiliar_user,
                                pgx_auxiliar_key = @pgx_auxiliar_key,
                                pgx_pruebas_server = @pgx_pruebas_server,
                                pgx_pruebas_db = @pgx_pruebas_db,
                                pgx_pruebas_user = @pgx_pruebas_user,
                                pgx_pruebas_key = @pgx_pruebas_key,
                                registro_usuario = @registro_usuario,
                                registro_fecha = @registro_fecha,
                                direccion = @direccion,
                                apto_postal = @apto_postal,
                                pais = @pais,
                                provincia = @provincia,
                                canton = @canton,
                                distrito = @distrito,
                                cod_pais = @cod_pais,
                                cod_pais_n1 = @cod_pais_n1,
                                cod_pais_n2 = @cod_pais_n2,
                                cod_pais_n3 = @cod_pais_n3,
                                cod_clasificacion = @cod_clasificacion,
                                tipo_id = @tipo_id,
                                pgx_pruebas_activo = @pgx_pruebas_activo,
                                url_app = @url_app,
                                url_web = @url_web,
                                url_logo = @url_logo,
                                url_app_activo = @url_app_activo,
                                url_web_activo = @url_web_activo,
                                url_logo_activo = @url_logo_activo
                              WHERE cod_empresa = @cod_empresa";

                    var parameters = new
                    {
                        info.cod_empresa,
                        info.cod_vendedor,
                        info.nombre_largo,
                        info.nombre_corto,
                        info.logo_cliente,
                        info.estado,
                        info.identificacion,
                        info.email_01,
                        info.email_02,
                        info.tel_cell,
                        info.tel_trabajo,
                        info.tel_auxiliar,
                        info.web_site,
                        info.facebook,
                        info.suscripcion_inicial,
                        info.suscripcion_vence,
                        info.suscripcion_mensualidad,
                        info.suscripcion_anual,
                        info.pgx_core_server,
                        info.pgx_core_db,
                        info.pgx_core_user,
                        info.pgx_core_key,
                        info.pgx_analisis_server,
                        info.pgx_analisis_db,
                        info.pgx_analisis_user,
                        info.pgx_analisis_key,
                        info.pgx_auxiliar_server,
                        info.pgx_auxiliar_db,
                        info.pgx_auxiliar_user,
                        info.pgx_auxiliar_key,
                        info.pgx_pruebas_server,
                        info.pgx_pruebas_db,
                        info.pgx_pruebas_user,
                        info.pgx_pruebas_key,
                        info.registro_usuario,
                        info.registro_fecha,
                        info.direccion,
                        info.apto_postal,
                        info.pais,
                        info.provincia,
                        info.canton,
                        info.distrito,
                        info.cod_pais,
                        info.cod_pais_n1,
                        info.cod_pais_n2,
                        info.cod_pais_n3,
                        info.cod_clasificacion,
                        info.tipo_id,
                        info.pgx_pruebas_activo,
                        info.url_app,
                        info.url_web,
                        info.url_logo,
                        info.url_app_activo,
                        info.url_web_activo,
                        info.url_logo_activo
                    };

                    var affectedRows = connection.Execute(updateSql, parameters);

                    // Update response
                    if (affectedRows > 0)
                    {
                        resp.Code = 0; // Success code

                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = (long)(info.cod_empresa ?? 0),
                            Usuario = info.modifica_usuario ?? string.Empty,
                            DetalleMovimiento = "Cliente Id: " + info.cod_empresa,
                            Movimiento = "MODIFICA - WEB",
                            Modulo = 31
                        });
                    }
                    else
                    {
                        resp.Code = -1; // Indicate failure if no rows were affected
                    }
                }
            }
            catch (SqlException ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public RespuestaDto Cliente_Crear(ClienteDto info)
        {
            var resp = new RespuestaDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    connection.Open();

                    // First, get the next available `cod_empresa`
                    var sqlMaxCodEmpresa = "SELECT ISNULL(MAX(cod_empresa), 0) + 1 FROM PGX_Clientes";
                    var newCodEmpresa = connection.ExecuteScalar<int>(sqlMaxCodEmpresa);

                    // Insert the new client record
                    var insertSql = @"INSERT INTO PGX_Clientes (
                                  cod_empresa, cod_clasificacion, cod_vendedor, tipo_id, identificacion, 
                                  nombre_corto, nombre_largo, tel_cell, tel_trabajo, tel_auxiliar, apto_postal, 
                                  email_01, email_02, web_site, facebook, cod_pais, cod_pais_n1, cod_pais_n2, cod_pais_n3, 
                                  direccion, estado, suscripcion_inicial, suscripcion_vence, suscripcion_anual, 
                                  suscripcion_mensualidad, pgx_core_server, pgx_core_db, pgx_core_user, pgx_core_key, 
                                  pgx_pruebas_server, pgx_pruebas_db, pgx_pruebas_user, pgx_pruebas_key, 
                                  pgx_analisis_server, pgx_analisis_db, pgx_analisis_user, pgx_analisis_key, 
                                  pgx_auxiliar_server, pgx_auxiliar_db, pgx_auxiliar_user, pgx_auxiliar_key, 
                                  pgx_pruebas_activo, url_app, url_web, url_logo, url_app_activo, url_web_activo, 
                                  url_logo_activo, registro_fecha, registro_usuario)
                              VALUES (
                                  @cod_empresa, @cod_clasificacion, @cod_vendedor, @tipo_id, @identificacion, 
                                  @nombre_corto, @nombre_largo, @tel_cell, @tel_trabajo, @tel_auxiliar, @apto_postal, 
                                  @email_01, @email_02, @web_site, @facebook, @cod_pais, @cod_pais_n1, @cod_pais_n2, @cod_pais_n3, 
                                  @direccion, @estado, @suscripcion_inicial, @suscripcion_vence, @suscripcion_anual, 
                                  @suscripcion_mensualidad, @pgx_core_server, @pgx_core_db, @pgx_core_user, @pgx_core_key, 
                                  @pgx_pruebas_server, @pgx_pruebas_db, @pgx_pruebas_user, @pgx_pruebas_key, 
                                  @pgx_analisis_server, @pgx_analisis_db, @pgx_analisis_user, @pgx_analisis_key, 
                                  @pgx_auxiliar_server, @pgx_auxiliar_db, @pgx_auxiliar_user, @pgx_auxiliar_key, 
                                  @pgx_pruebas_activo, @url_app, @url_web, @url_logo, @url_app_activo, @url_web_activo, 
                                  @url_logo_activo, GETDATE(), @registro_usuario)";

                    var parameters = new
                    {
                        cod_empresa = newCodEmpresa,
                        info.cod_clasificacion,
                        info.cod_vendedor,
                        info.tipo_id,
                        info.identificacion,
                        info.nombre_corto,
                        info.nombre_largo,
                        info.tel_cell,
                        info.tel_trabajo,
                        info.tel_auxiliar,
                        info.apto_postal,
                        info.email_01,
                        info.email_02,
                        info.web_site,
                        info.facebook,
                        info.cod_pais,
                        info.cod_pais_n1,
                        info.cod_pais_n2,
                        info.cod_pais_n3,
                        info.direccion,
                        info.estado,
                        info.suscripcion_inicial,
                        info.suscripcion_vence,
                        info.suscripcion_anual,
                        info.suscripcion_mensualidad,
                        info.pgx_core_server,
                        info.pgx_core_db,
                        info.pgx_core_user,
                        info.pgx_core_key,
                        info.pgx_pruebas_server,
                        info.pgx_pruebas_db,
                        info.pgx_pruebas_user,
                        info.pgx_pruebas_key,
                        info.pgx_analisis_server,
                        info.pgx_analisis_db,
                        info.pgx_analisis_user,
                        info.pgx_analisis_key,
                        info.pgx_auxiliar_server,
                        info.pgx_auxiliar_db,
                        info.pgx_auxiliar_user,
                        info.pgx_auxiliar_key,
                        info.pgx_pruebas_activo,
                        info.url_app,
                        info.url_web,
                        info.url_logo,
                        info.url_app_activo,
                        info.url_web_activo,
                        info.url_logo_activo,
                        info.registro_usuario
                    };

                    connection.Execute(insertSql, parameters);

                    // Update response object
                    resp.Id = newCodEmpresa;
                    resp.HasError = false;


                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = (long)(info.cod_empresa ?? 0),
                        Usuario = info.registro_usuario ?? string.Empty,
                        DetalleMovimiento = "Cliente Id: " + info.cod_empresa,
                        Movimiento = "REGISTRA - WEB",
                        Modulo = 31
                    });


                }
            }
            catch (Exception ex)
            {
                // Handle exception and update the response object
                resp.HasError = true;
                resp.ErrorMessage = ex.Message;
            }

            return resp;
        }

        public ErrorDto Cliente_Eliminar(int CodEmpresa, string usuario)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "delete PGX_Clientes where cod_Empresa = @CodEmpresa";
                    var parameters = new { CodEmpresa };
                    connection.Execute(query, parameters);

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = "Cliente Id: " + CodEmpresa,
                        Movimiento = "ELIMINA - WEB",
                        Modulo = 31
                    });

                    resp.Code = 0;
                    resp.Description = "Cliente eliminado correctamente.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region LISTAS CMB

        public List<ListaDD> Cliente_TiposId_Obtener()
        {
            List<ListaDD> resp = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string sql = @"select rtrim(TIPO_ID) as  'IdX',  rtrim(Descripcion) as 'ItmX' from PGX_TIPOS_ID where activa = 1";

                    resp = connection.Query<ListaDD>(sql).ToList();

                }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<ListaDD> Cliente_Clasificaciones_Obtener()
        {
            List<ListaDD> resp = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string sql = @"select rtrim(cod_Clasificacion) as 'IdX', rtrim(descripcion) as 'ItmX' from PGX_Clientes_Clasificacion where activa = 1";

                    resp = connection.Query<ListaDD>(sql).ToList();

                }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<ListaDD> Cliente_Vendedores_Obtener()
        {
            List<ListaDD> resp = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string sql = @"select rtrim(cod_Vendedor)  as 'IdX', rtrim(Nombre) as 'ItmX' from PGX_Vendedores where activo = 1";

                    resp = connection.Query<ListaDD>(sql).ToList();

                }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        #endregion


        #region SERVICIOS

        public List<ServicioDto> ServiciosCliente_Obtener(int CodEmpresa)
        {
            List<ServicioDto> resp = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string sql = @"
                    SELECT S.Cod_Servicio, S.Descripcion, A.Monto, A.Costo, A.Cantidad_Usuarios, A.Registro_Fecha, A.Registro_Usuario
                    FROM PGX_Servicios S
                    INNER JOIN PGX_Servicios_ASG A ON S.Cod_Servicio = A.Cod_Servicio
                    WHERE A.Cod_Empresa = @CodEmpresa AND A.Activo = 1";

                    resp = connection.Query<ServicioDto>(sql, new { CodEmpresa = CodEmpresa }).ToList();

                }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        #endregion


        #region CONTACTOS CLIENTE

        public List<ContactoDto> ContactosCliente_Obtener(int CodEmpresa)
        {
            List<ContactoDto> resp = null!;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string sql = @"
                                SELECT cod_Contacto, identificacion, nombre, tel_cell, tel_trabajo, Email_01, Email_02, Activo
                                FROM PGX_Clientes_Contactos
                                WHERE cod_Empresa = @CodEmpresa";

                    resp = connection.Query<ContactoDto>(sql, new { CodEmpresa = CodEmpresa }).ToList();

                }

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto ContactoCliente_Actualizar(ContactoDto contacto)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    // SQL query with parameterized values
                    string sql = @"
                                    UPDATE PGX_Clientes_Contactos 
                                    SET Identificacion = @identificacion, 
                                        Nombre = @nombre, 
                                        Tel_Cell = @tel_cell, 
                                        Tel_Trabajo = @tel_trabajo, 
                                        Email_01 = @email_01, 
                                        Email_02 = @email_02, 
                                        Activo = @activo
                                    WHERE cod_Empresa = @cod_empresa 
                                      AND cod_Contacto = @cod_contacto";

                    // Execute the query with the contact data
                    connection.Execute(sql, contacto);

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = (long)(contacto.cod_empresa ?? 0),
                        Usuario = contacto.registro_usuario ?? string.Empty,
                        DetalleMovimiento = "Cliente Contacto: " + contacto.cod_empresa + "-->" + contacto.cod_contacto,
                        Movimiento = "MODIFICA - WEB",
                        Modulo = 31
                    });

                    // Return success if the update was executed successfully
                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "Contacto actualizado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                // Return failure in case of an exception
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al actualizar el contacto: {ex.Message}"
                };
            }
        }

        public ErrorDto ContactoCliente_Insertar(ContactoDto contacto)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    // Get the next cod_contacto value
                    string getNextCodContactoSql = @"
                            SELECT ISNULL(MAX(cod_Contacto), 0) + 1 
                            FROM PGX_Clientes_Contactos 
                            WHERE cod_Empresa = @cod_empresa";

                    // Retrieve the new cod_contacto
                    int nuevoCodContacto = connection.QuerySingle<int>(getNextCodContactoSql, new { cod_empresa = contacto.cod_empresa });
                    contacto.cod_contacto = nuevoCodContacto;

                    // SQL query for inserting the new contact
                    string insertSql = @"
                            INSERT INTO PGX_Clientes_Contactos 
                            (cod_Contacto, cod_Empresa, Identificacion, Nombre, Tel_Cell, Tel_Trabajo, Email_01, Email_02, Activo)
                            VALUES (@cod_contacto, @cod_empresa, @identificacion, @nombre, @tel_cell, @tel_trabajo, @email_01, @email_02, @activo)";

                    // Execute the insert query with the contact data
                    connection.Execute(insertSql, contacto);

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = (long)(contacto.cod_empresa ?? 0),
                        Usuario = contacto.registro_usuario ?? string.Empty,
                        DetalleMovimiento = "Cliente Contacto: " + contacto.cod_empresa + "-->" + contacto.cod_contacto,
                        Movimiento = "REGISTRA - WEB",
                        Modulo = 31
                    });

                    // Return success message
                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "Contacto insertado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                // Return failure in case of an exception
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al insertar el contacto: {ex.Message}"
                };
            }
        }

        public ErrorDto ContactoCliente_Eliminar(int cod_contacto, int cod_empresa)
        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    // SQL query to delete the contact
                    string deleteSql = @"
                            DELETE FROM PGX_Clientes_Contactos 
                            WHERE cod_Contacto = @cod_contacto 
                              AND cod_Empresa = @cod_empresa";

                    // Execute the delete query with the provided cod_contacto and cod_empresa
                    int rowsAffected = connection.Execute(deleteSql, new { cod_contacto, cod_empresa });

                    // Return success if at least one row was deleted
                    if (rowsAffected > 0)
                    {
                        return new ErrorDto
                        {
                            Code = 0,
                            Description = "Contacto eliminado correctamente."
                        };
                    }
                    else
                    {
                        return new ErrorDto
                        {
                            Code = -1,
                            Description = "No se encontró el contacto a eliminar."
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Return failure in case of an exception
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al eliminar el contacto: {ex.Message}"
                };
            }
        }

        #endregion


        #region SMTP CLIENTE

        public List<SmtpDto> ListaSMTP(int CodEmpresa)
        {
            List<SmtpDto> resp = [];
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_SMTP_Lista]";
                    var values = new
                    {
                        Cliente = CodEmpresa
                    };
                    resp = connection.Query<SmtpDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto SMTP_Autorizar(SmtpDto smtpAuth)

        {
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    // Defining the SQL stored procedure with parameters
                    string sql = "exec spPGX_SMTP_Autoriza @Cliente, @SMTP, @Usuario, @Mov";

                    // Mapping parameters from the model
                    var parameters = new DynamicParameters();
                    parameters.Add("@Cliente", smtpAuth.cod_empresa);
                    parameters.Add("@SMTP", smtpAuth.smtp_id);
                    parameters.Add("@Usuario", smtpAuth.usuario);
                    parameters.Add("@Mov", (smtpAuth.asignado ?? false) ? "A" : "B");  // A for checked, B for not

                    // Execute the stored procedure
                    connection.Execute(sql, parameters);

                    // Return success if the procedure executed successfully
                    return new ErrorDto
                    {
                        Code = 0,
                        Description = "SMTP para el cliente fue actualizado correctamente."
                    };
                }
            }
            catch (Exception ex)
            {
                // Return error details if something went wrong
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al actualizar autorización del SMTP: {ex.Message}"
                };
            }
        }

        #endregion


        #region TEST Y SINC

        public ErrorDto Clientes_Sincronizar(int CodEmpresa, bool logos)
        {
            var errorResponse = new ErrorDto(); // Initialize your ErrorDto
            int i = 0;

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string sql = @"
                SELECT 
                    COD_EMPRESA, 
                    NOMBRE_CORTO, 
                    NOMBRE_LARGO, 
                    IDENTIFICACION, 
                    PGX_CORE_DB, 
                    PGX_CORE_SERVER, 
                    PGX_CORE_USER, 
                    PGX_CORE_KEY, 
                    URL_LOGO, 
                    URL_Logo_Activo
                FROM 
                    PGX_CLIENTES 
                WHERE 
                    estado = 'A'";

                    // Add filter if CodEmpresa is greater than 0
                    if (CodEmpresa > 0)
                    {
                        sql += " AND COD_EMPRESA = @CodEmpresa";
                    }

                    var clientes = connection.Query<ClienteDto>(sql, new { CodEmpresa = CodEmpresa }).ToList();

                    foreach (var cliente in clientes)
                    {
                        i++;
                        // Assuming lblStatus is a way to update status; you may need to handle this differently in a non-UI context
                        Console.WriteLine($"Sincronizando: {cliente.nombre_corto} [{i} - {clientes.Count}]"); // Use Console for demonstration

                        // Build the connection string for the new SQL Server connection
                        using (var dbConnection = new SqlConnection($"Server={cliente.pgx_core_server};Database={cliente.pgx_core_db};User Id={cliente.pgx_core_user};Password={cliente.pgx_core_key};"))
                        {
                            dbConnection.Open();
                            // Update statement
                            string updateSql = $"UPDATE SIF_EMPRESA SET PORTAL_ID =" + cliente.cod_empresa;

                            if (logos && cliente.url_logo_activo == true)
                            {
                                updateSql += $", LOGO_WEB_SITE = " + cliente.url_logo;
                            }

                            using (var command = new SqlCommand(updateSql, dbConnection))
                            {
                                command.ExecuteNonQuery();
                                errorResponse.Code = 0;
                                errorResponse.Description = "Sincronización Finalizada";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorResponse.Description = "An error occurred during synchronization: " + ex.Message;
                errorResponse.Code = -1; // Or however you want to indicate failure
            }

            return errorResponse; // Return the ErrorDto
        }

        #endregion


        #region PAIS-PROV-CANT-DIST

        public List<PaisesDto> ObtenerPaises()
        {
            List<PaisesDto> data = new List<PaisesDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "SELECT [COD_PAIS] ,[DESCRIPCION] ,[ZONA_HORARIA] FROM [PGX_Portal].[dbo].[PGX_PAIS] WHERE ACTIVO = 1";


                    data = connection.Query<PaisesDto>(query, new { }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<ProvinciaDto> ObtenerProvincia(string CodPais)
        {
            List<ProvinciaDto> data = new List<ProvinciaDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "SELECT [COD_PAIS],[COD_PAIS_N1],[DESCRIPCION] FROM [PGX_Portal].[dbo].[PGX_PAIS_N1] WHERE ACTIVO = 1 AND COD_PAIS = @CodPais";


                    data = connection.Query<ProvinciaDto>(query, new { CodPais = CodPais }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<CantonDto> ObtenerCanton(string CodPais, string CodProvincia)
        {
            List<CantonDto> data = new List<CantonDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "SELECT [COD_PAIS] ,[COD_PAIS_N1] ,[COD_PAIS_N2] ,[DESCRIPCION] FROM[PGX_Portal].[dbo].[PGX_PAIS_N2] WHERE COD_PAIS = @CodPais and COD_PAIS_N1 = @CodProvincia and Activo = 1";


                    data = connection.Query<CantonDto>(query, new { CodPais = CodPais, CodProvincia = CodProvincia }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        public List<DistritoDto> ObtenerDistrito(string CodPais, string CodProvincia, string CodCanton)
        {
            List<DistritoDto> data = new List<DistritoDto>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var query = "SELECT [COD_PAIS],[COD_PAIS_N1],[COD_PAIS_N2],[COD_PAIS_N3],[DESCRIPCION],[REGISTRO_USUARIO] FROM [PGX_Portal].[dbo].[PGX_PAIS_N3] WHERE COD_PAIS = @CodPais and COD_PAIS_N1 = @CodProvincia and COD_PAIS_N2 = @CodCanton and Activo = 1";


                    data = connection.Query<DistritoDto>(query, new { CodPais = CodPais, CodProvincia = CodProvincia, CodCanton = CodCanton }).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return data;
        }

        #endregion


        #region YA NO SE USAN


        public List<ClienteDto> ObtenerTodasEmpresas()
        {
            List<ClienteDto> resp = [];
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Obtener_Todas_Empresas]";
                    var values = new
                    {

                    };
                    resp = connection.Query<ClienteDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<EmpresaServiciosDto> ListaEmpresaContactos(int CodEmpresa)
        {
            List<EmpresaServiciosDto> resp = [];
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Obtener_Empresa_Servicios]";
                    var values = new
                    {
                        Cod_Empresa = CodEmpresa
                    };
                    resp = connection.Query<EmpresaServiciosDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public int CrearEmpresa(ClienteDto info)
        {
            int resp = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[sp_InsertarPGX_Clientes]";
                    var values = new
                    {
                        COD_EMPRESA = info.cod_empresa,
                        COD_VENDEDOR = info.cod_vendedor,
                        NOMBRE_LARGO = info.nombre_largo,
                        NOMBRE_CORTO = info.nombre_corto,
                        LOGO_CLIENTE = info.logo_cliente,
                        ESTADO = info.estado,
                        IDENTIFICACION = info.identificacion,
                        EMAIL_01 = info.email_01,
                        EMAIL_02 = info.email_02,
                        TEL_CELL = info.tel_cell,
                        TEL_TRABAJO = info.tel_trabajo,
                        TEL_AUXILIAR = info.tel_auxiliar,
                        WEB_SITE = info.web_site,
                        FACEBOOK = info.facebook,
                        SUSCRIPCION_INICIAL = info.suscripcion_inicial,
                        SUSCRIPCION_VENCE = info.suscripcion_vence,
                        SUSCRIPCION_MENSUALIDAD = info.suscripcion_mensualidad,
                        SUSCRIPCION_ANUAL = info.suscripcion_anual,
                        PGX_CORE_SERVER = info.pgx_core_server,
                        PGX_CORE_DB = info.pgx_core_db,
                        PGX_CORE_USER = info.pgx_core_user,
                        PGX_CORE_KEY = info.pgx_core_key,
                        PGX_ANALISIS_SERVER = info.pgx_analisis_server,
                        PGX_ANALISIS_DB = info.pgx_analisis_db,
                        PGX_ANALISIS_USER = info.pgx_analisis_user,
                        PGX_ANALISIS_KEY = info.pgx_analisis_key,
                        PGX_AUXILIAR_SERVER = info.pgx_auxiliar_server,
                        PGX_AUXILIAR_DB = info.pgx_auxiliar_db,
                        PGX_AUXILIAR_USER = info.pgx_auxiliar_user,
                        PGX_AUXILIAR_KEY = info.pgx_auxiliar_key,
                        PGX_PRUEBAS_SERVER = info.pgx_pruebas_server,
                        PGX_PRUEBAS_DB = info.pgx_pruebas_db,
                        PGX_PRUEBAS_USER = info.pgx_pruebas_user,
                        PGX_PRUEBAS_KEY = info.pgx_pruebas_key,
                        REGISTRO_USUARIO = info.registro_usuario,
                        REGISTRO_FECHA = info.registro_fecha,
                        DIRECCION = info.direccion,
                        APTO_POSTAL = info.apto_postal,
                        PAIS = info.pais,
                        PROVINCIA = info.provincia,
                        CANTON = info.canton,
                        DISTRITO = info.distrito,
                        COD_PAIS = info.cod_pais,
                        COD_PAIS_N1 = info.cod_pais_n1,
                        COD_PAIS_N2 = info.cod_pais_n2,
                        COD_PAIS_N3 = info.cod_pais_n3,
                        COD_CLASIFICACION = info.cod_clasificacion,
                        TIPO_ID = info.tipo_id,
                        PGX_PRUEBAS_ACTIVO = info.pgx_pruebas_activo,
                        URL_App = info.url_app,
                        URL_Web = info.url_web,
                        URL_Logo = info.url_logo,
                        URL_App_Activo = info.url_app_activo,
                        URL_Web_Activo = info.url_web_activo,
                        URL_Logo_Activo = info.url_logo_activo
                    };


                    resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public RespuestaDto Cliente_Crear2(ClienteDto info)
        {
            var resp = new RespuestaDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[sp_InsertarPGX_Clientes]";
                    var values = new
                    {
                        COD_EMPRESA = info.cod_empresa,
                        COD_VENDEDOR = info.cod_vendedor,
                        NOMBRE_LARGO = info.nombre_largo,
                        NOMBRE_CORTO = info.nombre_corto,
                        LOGO_CLIENTE = info.logo_cliente,
                        ESTADO = info.estado,
                        IDENTIFICACION = info.identificacion,
                        EMAIL_01 = info.email_01,
                        EMAIL_02 = info.email_02,
                        TEL_CELL = info.tel_cell,
                        TEL_TRABAJO = info.tel_trabajo,
                        TEL_AUXILIAR = info.tel_auxiliar,
                        WEB_SITE = info.web_site,
                        FACEBOOK = info.facebook,
                        SUSCRIPCION_INICIAL = info.suscripcion_inicial,
                        SUSCRIPCION_VENCE = info.suscripcion_vence,
                        SUSCRIPCION_MENSUALIDAD = info.suscripcion_mensualidad,
                        SUSCRIPCION_ANUAL = info.suscripcion_anual,
                        PGX_CORE_SERVER = info.pgx_core_server,
                        PGX_CORE_DB = info.pgx_core_db,
                        PGX_CORE_USER = info.pgx_core_user,
                        PGX_CORE_KEY = info.pgx_core_key,
                        PGX_ANALISIS_SERVER = info.pgx_analisis_server,
                        PGX_ANALISIS_DB = info.pgx_analisis_db,
                        PGX_ANALISIS_USER = info.pgx_analisis_user,
                        PGX_ANALISIS_KEY = info.pgx_analisis_key,
                        PGX_AUXILIAR_SERVER = info.pgx_auxiliar_server,
                        PGX_AUXILIAR_DB = info.pgx_auxiliar_db,
                        PGX_AUXILIAR_USER = info.pgx_auxiliar_user,
                        PGX_AUXILIAR_KEY = info.pgx_auxiliar_key,
                        PGX_PRUEBAS_SERVER = info.pgx_pruebas_server,
                        PGX_PRUEBAS_DB = info.pgx_pruebas_db,
                        PGX_PRUEBAS_USER = info.pgx_pruebas_user,
                        PGX_PRUEBAS_KEY = info.pgx_pruebas_key,
                        REGISTRO_USUARIO = info.registro_usuario,
                        REGISTRO_FECHA = info.registro_fecha,
                        DIRECCION = info.direccion,
                        APTO_POSTAL = info.apto_postal,
                        PAIS = info.pais,
                        PROVINCIA = info.provincia,
                        CANTON = info.canton,
                        DISTRITO = info.distrito,
                        COD_PAIS = info.cod_pais,
                        COD_PAIS_N1 = info.cod_pais_n1,
                        COD_PAIS_N2 = info.cod_pais_n2,
                        COD_PAIS_N3 = info.cod_pais_n3,
                        COD_CLASIFICACION = info.cod_clasificacion,
                        TIPO_ID = info.tipo_id,
                        PGX_PRUEBAS_ACTIVO = info.pgx_pruebas_activo,
                        URL_App = info.url_app,
                        URL_Web = info.url_web,
                        URL_Logo = info.url_logo,
                        URL_App_Activo = info.url_app_activo,
                        URL_Web_Activo = info.url_web_activo,
                        URL_Logo_Activo = info.url_logo_activo
                    };

                    // Execute the stored procedure and get the created ID
                    int value = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    // Populate resp with the created ID
                    resp.Id = value;
                    resp.HasError = false;  // Indicate that there was no error
                }
            }
            catch (Exception ex)
            {
                // Handle the error and populate the ErrorDto
                resp.HasError = true;
                resp.ErrorMessage = ex.Message;
            }

            return resp;
        }

        public ErrorDto Cliente_Modificar2(ClienteDto info)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spPGX_Actualizar_Datos_Empresa]";
                    var values = new
                    {
                        COD_EMPRESA = info.cod_empresa,
                        COD_VENDEDOR = info.cod_vendedor,
                        NOMBRE_LARGO = info.nombre_largo,
                        NOMBRE_CORTO = info.nombre_corto,
                        LOGO_CLIENTE = info.logo_cliente,
                        ESTADO = info.estado,
                        IDENTIFICACION = info.identificacion,
                        EMAIL_01 = info.email_01,
                        EMAIL_02 = info.email_02,
                        TEL_CELL = info.tel_cell,
                        TEL_TRABAJO = info.tel_trabajo,
                        TEL_AUXILIAR = info.tel_auxiliar,
                        WEB_SITE = info.web_site,
                        FACEBOOK = info.facebook,
                        SUSCRIPCION_INICIAL = info.suscripcion_inicial,
                        SUSCRIPCION_VENCE = info.suscripcion_vence,
                        SUSCRIPCION_MENSUALIDAD = info.suscripcion_mensualidad,
                        SUSCRIPCION_ANUAL = info.suscripcion_anual,
                        PGX_CORE_SERVER = info.pgx_core_server,
                        PGX_CORE_DB = info.pgx_core_db,
                        PGX_CORE_USER = info.pgx_core_user,
                        PGX_CORE_KEY = info.pgx_core_key,
                        PGX_ANALISIS_SERVER = info.pgx_analisis_server,
                        PGX_ANALISIS_DB = info.pgx_analisis_db,
                        PGX_ANALISIS_USER = info.pgx_analisis_user,
                        PGX_ANALISIS_KEY = info.pgx_analisis_key,
                        PGX_AUXILIAR_SERVER = info.pgx_auxiliar_server,
                        PGX_AUXILIAR_DB = info.pgx_auxiliar_db,
                        PGX_AUXILIAR_USER = info.pgx_auxiliar_user,
                        PGX_AUXILIAR_KEY = info.pgx_auxiliar_key,
                        PGX_PRUEBAS_SERVER = info.pgx_pruebas_server,
                        PGX_PRUEBAS_DB = info.pgx_pruebas_db,
                        PGX_PRUEBAS_USER = info.pgx_pruebas_user,
                        PGX_PRUEBAS_KEY = info.pgx_pruebas_key,
                        REGISTRO_USUARIO = info.registro_usuario,
                        REGISTRO_FECHA = info.registro_fecha,
                        DIRECCION = info.direccion,
                        APTO_POSTAL = info.apto_postal,
                        PAIS = info.pais,
                        PROVINCIA = info.provincia,
                        CANTON = info.canton,
                        DISTRITO = info.distrito,
                        COD_PAIS = info.cod_pais,
                        COD_PAIS_N1 = info.cod_pais_n1,
                        COD_PAIS_N2 = info.cod_pais_n2,
                        COD_PAIS_N3 = info.cod_pais_n3,
                        COD_CLASIFICACION = info.cod_clasificacion,
                        TIPO_ID = info.tipo_id,
                        PGX_PRUEBAS_ACTIVO = info.pgx_pruebas_activo,
                        URL_App = info.url_app,
                        URL_Web = info.url_web,
                        URL_Logo = info.url_logo,
                        URL_App_Activo = info.url_app_activo,
                        URL_Web_Activo = info.url_web_activo,
                        URL_Logo_Activo = info.url_logo_activo
                    };
                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (SqlException ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        #endregion

    }//end class
}//end namespace
