using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos.Autorizadores;
using PgxAPI.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_AutorizadoresDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly MSecurityMainDb mProGrX_Security;

        public frmTES_AutorizadoresDB(IConfiguration config)
        {
            _config = config;
            mProGrX_Security = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de autorizadores de usuario para la empresa especificada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizadoresLista> Tes_AutorizadoresUsuarioLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<TesAutorizadoresLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAutorizadoresLista()
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
                    query = $@"select COUNT(nombre) from tes_autorizaciones";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE Nombre LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select Nombre as 'item',Nombre as 'descripcion' 
                                     from tes_autorizaciones
                                        {filtros.filtro} 
                                     ORDER BY Nombre 
                                        {paginaActual}
                                        {paginacionActual} ";
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
        /// Busca un autorizador de usuario por nombre y permite la navegación hacia adelante o hacia atrás en los registros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <param name="scroll"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuarioBuscar_scroll(int CodEmpresa, string nombre, int? scroll)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesAutorizadoresDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAutorizadoresDto()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = "";
                    scroll = (scroll == null) ? 1 : scroll;
                    if (scroll == 1) //busca el registro anterior
                    {
                        where = $" WHERE Nombre < '{nombre}' ORDER BY NOMBRE desc";
                    }
                    else if (scroll == 2) //busca el registro siguiente
                    {
                        where = $" WHERE Nombre > '{nombre}' ORDER BY NOMBRE ASC";
                    }

                    var query = $@"select top 1 nombre,
                                                notas,
                                                clave,
                                                estado,
                                                rango_gen_inicio,
                                                rango_gen_corte,
                                                firmas_gen_inicio,
                                                firmas_gen_corte
                                                 from tes_autorizaciones {where} ";
                    response.Result = connection.QueryFirstOrDefault<TesAutorizadoresDto>(query);
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
        /// Obtiene un autorizador de usuario por nombre específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuario_Obtener(int CodEmpresa, string nombre)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesAutorizadoresDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesAutorizadoresDto()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select nombre,
                                                notas,
                                                clave,
                                                estado,
                                                rango_gen_inicio,
                                                rango_gen_corte,
                                                firmas_gen_inicio,
                                                firmas_gen_corte
                                                 from tes_autorizaciones WHERE  nombre = @nombre ";
                    response.Result = connection.QueryFirstOrDefault<TesAutorizadoresDto>(query, new { nombre });
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
        /// Guarda un autorizador de usuario. Si el autorizador ya existe, lo actualiza; si no, lo inserta como nuevo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autorizador"></param>
        /// <returns></returns>
        public ErrorDto Tes_Autorizadores_Guardar(int CodEmpresa,string usuario, TesAutorizadoresDto autorizador)
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

                    var valida = fxValida(autorizador);
                    if (valida.Code == -1)
                    {
                        response.Code = valida.Code;
                        response.Description = valida.Description;
                        return response;
                    }

                    //valida si existe
                    var query = $@"SELECT COUNT(*) FROM tes_autorizaciones WHERE nombre = @nombre";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { autorizador.nombre });
                    if (existe > 0)
                    {
                        // Actualiza el registro existente
                        query = $@"UPDATE tes_autorizaciones 
                                   SET notas = @notas, 
                                       clave = @clave, 
                                       estado = @estado, 
                                       rango_gen_inicio = @rango_gen_inicio, 
                                       rango_gen_corte = @rango_gen_corte, 
                                       firmas_gen_inicio = @firmas_gen_inicio, 
                                       firmas_gen_corte = @firmas_gen_corte 
                                   WHERE nombre = @nombre";
                        response.Code = connection.Execute(query, autorizador);

                        mProGrX_Security.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Usuario Autorizador : {autorizador.nombre}",
                            Movimiento = "Modifica - WEB",
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        // Inserta un nuevo registro
                        query = $@"INSERT INTO tes_autorizaciones (nombre, notas, clave, estado, rango_gen_inicio, rango_gen_corte, firmas_gen_inicio, firmas_gen_corte) 
                                   VALUES (@nombre, @notas, @clave, @estado, @rango_gen_inicio, @rango_gen_corte, @firmas_gen_inicio, @firmas_gen_corte)";
                        response.Code = connection.Execute(query, autorizador);
                        mProGrX_Security.Bitacora
                            (new BitacoraInsertarDto
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = usuario,
                                DetalleMovimiento = $"Usuario Autorizador : {autorizador.nombre}",
                                Movimiento = "Registra - WEB",
                                Modulo = vModulo
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
        /// Valida los campos de un autorizador de usuario.
        /// </summary>
        /// <param name="autorizador"></param>
        /// <returns></returns>
        private ErrorDto fxValida(TesAutorizadoresDto autorizador)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = ""
            };
            try
            {
                bool resultado = true;

                if (string.IsNullOrWhiteSpace(autorizador.nombre))
                    response.Description += "\n - Nombre del Usuario no es válido ...";

                if (autorizador.rango_gen_inicio == null)
                    response.Description += "\n - El Rango de Autorización de Emisión [DESDE] no es válido...";

                if (autorizador.rango_gen_corte == null)
                    response.Description += "\n - El Rango de Autorización de Emisión [HASTA] no es válido...";

                if (autorizador.firmas_gen_inicio == null)
                    response.Description += "\n - El Rango de Autorización de Firmas [DESDE] no es válido...";

                if (autorizador.firmas_gen_corte == null)
                    response.Description += "\n - El Rango de Autorización de Firmas [HASTA] no es válido...";

                if (response.Description == "")
                {
                    //valida que los rangos de inicio no sea mayor que los rangos de corte
                    if (autorizador.rango_gen_inicio > autorizador.rango_gen_corte)
                        response.Description += "\n - El Rango de Autorización de Emisión [DESDE es Mayor que HASTA]";

                    if (autorizador.firmas_gen_inicio > autorizador.firmas_gen_corte)
                        response.Description += "\n - El Rango de Autorización de Firmas [DESDE es Mayor que HASTA]";
                }

                if (response.Description != "")
                {
                    response.Code = -1;
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
        /// Elimina un autorizador de usuario por nombre.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto Tes_Autorizadores_Eliminar(int CodEmpresa, string nombre, string usuario)
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
                    var query = $@"DELETE FROM tes_autorizaciones WHERE nombre = @nombre";
                    response.Code = connection.Execute(query, new { nombre = usuario });

                    mProGrX_Security.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Usuario Autorizador : {usuario}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
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
