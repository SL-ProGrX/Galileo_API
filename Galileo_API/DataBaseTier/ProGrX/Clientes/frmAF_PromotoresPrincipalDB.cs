using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using System.Data;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_PromotoresPrincipalDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_PromotoresPrincipalDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de promotores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Lista_Obtener(int CodEmpresa)
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
                    var query = $@"select id_promotor as item,nombre as descripcion from promotores";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtener lista de usuarios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Promotores_Usuarios_Obtener(int CodEmpresa)
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
                    var query = "SELECT NOMBRE AS ITEM,DESCRIPCION FROM USUARIOS WHERE ESTADO = 'A' ORDER BY NOMBRE";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Scroll de promotores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ScrollCode"></param>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        public ErrorDto<AF_PromotoresPrincipalDTO> AF_Promotores_Scroll_Obtener(int CodEmpresa, int ScrollCode, int Codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_PromotoresPrincipalDTO>
            {
                Code = 0,
                Result = new AF_PromotoresPrincipalDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Top 1 id_promotor from promotores";

                    if (ScrollCode == 1)
                    {
                        query += " where id_promotor > @Codigo order by id_promotor asc";
                    }
                    else
                    {
                        query += " where id_promotor < @Codigo order by id_promotor desc";
                    }
                    var idPromotor = connection.Query<int>(query, new { Codigo }).FirstOrDefault();
                    response = AF_Promotor_Obtener(CodEmpresa, idPromotor);
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
        /// Obtener información de un promotor mediante el id_promotor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        public ErrorDto<AF_PromotoresPrincipalDTO> AF_Promotor_Obtener(int CodEmpresa, int Codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_PromotoresPrincipalDTO>
            {
                Code = 0,
                Result = new AF_PromotoresPrincipalDTO()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select P.*,B.descripcion as Banco from promotores P 
                        inner join Tes_Bancos B on P.cod_banco = B.id_banco 
                        where P.id_promotor = @Codigo";
                    response.Result = connection.Query<AF_PromotoresPrincipalDTO>(query,
                        new { Codigo } ).FirstOrDefault();
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
        /// Obtener lista de cuentas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodComision"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_Promotores_CuentasDTO>> AF_Promotores_Cuentas_Obtener(int CodEmpresa, string CodComision)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AF_Promotores_CuentasDTO>>
            {
                Code = 0,
                Result = new List<AF_Promotores_CuentasDTO>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select rtrim(B.Descripcion) as 'Banco'
                        ,case when C.tipo = 'A' then 'Ahorros' else 'Corriente' end as 'TipoDesc'
                        ,C.cod_Divisa,C.CUENTA_INTERNA, C.CUENTA_INTERBANCA, C.ACTIVA, C.DESTINO, C.REGISTRO_FECHA , C.REGISTRO_USUARIO 
                        from SYS_CUENTAS_BANCARIAS C inner join TES_BANCOS_GRUPOS B on C.cod_banco = B.cod_grupo
                        where C.Identificacion = @CodComision and C.Modulo = 'AFI'";
                    response.Result = connection.Query<AF_Promotores_CuentasDTO>(query,
                        new { CodComision }).ToList();
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
        /// Obtener lista de bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_Promotores_BancoDTO>> AF_Promotores_Bancos_Obtener(int CodEmpresa, string Usuario)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AF_Promotores_BancoDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var values = new
                    {
                        usuario = Usuario,
                    };

                    response.Result = connection.Query<AF_Promotores_BancoDTO>(
                        "spCrd_SGT_Bancos", 
                        values, 
                        commandType: CommandType.StoredProcedure
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener listado de promotores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Tipo"></param>
        /// <param name="Estado"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_PromotoresPrincipalLista> AF_Promotores_ListadoConsulta_Obtener(int CodEmpresa, string Tipo, int Estado, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_PromotoresPrincipalLista>
            {
                Code = 0,
                Result = new AF_PromotoresPrincipalLista()
                {
                    total = 0,
                    lista = new List<AF_PromotoresPrincipalDTO>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
              {
                    var queryT = @"Select COUNT(P.ID_PROMOTOR) as Banco
                        from Promotores P inner join Tes_Bancos B on P.cod_banco = B.id_Banco 
                        where P.estado = @Estado and Tipo = @Tipo ";
                    response.Result.total = connection.Query<int>(queryT, new { Tipo, Estado }).FirstOrDefault();

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = " AND (  P.Nombre like '%" + filtros.filtro + "%') ";
                    }

                    var query = $@"Select P.*,B.descripcion as Banco
                        from Promotores P inner join Tes_Bancos B on P.cod_banco = B.id_Banco 
                        where P.estado = @Estado and Tipo = @Tipo 
                                        {filtros.filtro} 
                                     order by P.nombre";
                    if (filtros.paginacion != 0 || filtros.paginacion == null)
                    {
                        query += $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    }
                    response.Result.lista = connection.Query<AF_PromotoresPrincipalDTO>(query,
                        new { Tipo, Estado } ).ToList();
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
        /// Guardar promotor, actualizar o inserta según corresponda
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_Promotores_Guardar(int CodEmpresa, string Usuario, AF_PromotoresPrincipalDTO Info)
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
                    var query = "select isnull(count(*),0) as Existe from promotores where id_promotor = @Codigo";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            Codigo = Info.id_promotor
                        }
                    );

                    if (existe == 1)
                    {
                        query = @"update promotores set nombre = @Nombre,
                            cedula_contacto = @CedJur, nombre_contacto = @PagarA,
                            observacion = @Observacion, estado = @Estado, tipo_documento = @TipoDocumento, 
                            direccion = @Direccion, aptoPostal = @ApartadoPostal, email = @Email, 
                            telefono = @Telefono1, telefono_ext = @TelefonoExt, fax = @Fax, fax_ext = @FaxExt, 
                            cod_banco = @Banco, comite = @Comite, apl_comision = @Comision, cod_comision = @CedJur,
                            Tipo = @Tipo, user_referencia = @UsuarioRef, usuario = @Usuario, fecha = GETDATE() 
                            where id_promotor = @Codigo";

                        connection.Execute(query,
                            new
                            {
                                Nombre = Truncate(Info.nombre, 60),
                                CedJur = Truncate(Info.cod_comision, 15),
                                PagarA = Truncate(Info.nombre_contacto, 60),
                                Observacion = Truncate(Info.observacion, 255),
                                Estado = Info.estado,
                                TipoDocumento = Truncate(Info.tipo_documento, 2),
                                Direccion = Truncate(Info.direccion, 255),
                                ApartadoPostal = Truncate(Info.aptopostal, 25),
                                Email = Truncate(Info.email, 100),
                                Telefono1 = Truncate(Info.telefono, 10),
                                TelefonoExt = Truncate(Info.telefono_ext, 5),
                                Fax = Truncate(Info.fax, 10),
                                FaxExt = Truncate(Info.fax_ext, 5),
                                Banco = Info.cod_banco,
                                Comite = Info.tipo == "C" ? 1 : 0,
                                Comision = Info.apl_comision ? 1 : 0,
                                Tipo = Truncate(Info.tipo, 1),
                                UsuarioRef = Truncate(Info.user_referencia, 30),
                                Usuario = Truncate(Usuario, 30),
                                Codigo = Info.id_promotor
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Ejecutivo de Cuenta Id: " + Info.id_promotor,
                            Movimiento = "Modifica - WEB",
                            Modulo = 9
                        });
                    }
                    else
                    {
                        query = @"insert into promotores(Tipo,nombre,observacion,cod_comision,fechaIng,
                            estado, telefono, telefono_ext, fax,fax_ext, email, aptopostal, direccion, tipo_documento,
                            cod_banco, cedula_contacto, nombre_contacto, comite, apl_comision, usuario, fecha, user_referencia)
                            values( @Tipo, @Nombre, @Observacion, @CedJur, GETDATE(), @Estado, @Telefono1, @TelefonoExt, 
                            @Fax, @FaxExt, @Email, @ApartadoPostal, @Direccion, @TipoDocumento, @Banco, @CedJur, @PagarA, 
                            @Comite, @Comision, @Usuario, GETDATE(), @UsuarioRef)";

                        connection.Execute(query,
                            new
                            {
                                Nombre = Truncate(Info.nombre, 60),
                                CedJur = Truncate(Info.cod_comision, 15),
                                PagarA = Truncate(Info.nombre_contacto, 60),
                                Observacion = Truncate(Info.observacion, 255),
                                Estado = Info.estado,
                                TipoDocumento = Truncate(Info.tipo_documento, 2),
                                Direccion = Truncate(Info.direccion, 255),
                                ApartadoPostal = Truncate(Info.aptopostal, 25),
                                Email = Truncate(Info.email, 100),
                                Telefono1 = Truncate(Info.telefono, 10),
                                TelefonoExt = Truncate(Info.telefono_ext, 5),
                                Fax = Truncate(Info.fax, 10),
                                FaxExt = Truncate(Info.fax_ext, 5),
                                Banco = Info.cod_banco,
                                Comite = Info.tipo == "C" ? 1 : 0,
                                Comision = Info.apl_comision ? 1 : 0,
                                Tipo = Truncate(Info.tipo, 1),
                                UsuarioRef = Truncate(Info.user_referencia, 30),
                                Usuario = Truncate(Usuario, 30)
                            }
                        );

                        var queryU = "select isnull(max(id_promotor),0) as ultimo from promotores";
                        int ultimo = connection.QueryFirstOrDefault<int>(queryU);

                        response.Code = ultimo;

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Ejecutivo de Cuenta Id: " + Info.id_promotor,
                            Movimiento = "Registra - WEB",
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
        /// Eliminar promotor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Codigo"></param>
        /// <returns></returns>
        public ErrorDto AF_Promotores_Eliminar(int CodEmpresa, string Usuario, int Codigo)
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
                    var query = "delete promotores where id_promotor = @Codigo";
                    connection.Execute(query, new { Codigo });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Ejecutivo de Cuenta Id: " + Codigo,
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

        /// <summary>
        /// Método privado para restringir la longitud de las cadenas
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
