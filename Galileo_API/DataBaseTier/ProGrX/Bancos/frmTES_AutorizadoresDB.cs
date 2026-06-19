using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos.Autorizadores;
using Galileo.Models.Security;


namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesAutorizadoresDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly MSecurityMainDb _mSecurityMainDb;

        public FrmTesAutorizadoresDB(IConfiguration config)
        {
            _mSecurityMainDb = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene una lista de autorizadores de usuario para la empresa especificada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizadoresLista> Tes_AutorizadoresUsuarioLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
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

                var sortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["item"] = 1,
                    ["descripcion"] = 2,
                    ["nombre"] = 1
                };

                var lazy = LazyLoadHelper.Build(filtros, sortMap);

                const string sqlCount = @"
                        SELECT COUNT(1)
                        FROM tes_autorizaciones
                        WHERE (@filtro IS NULL OR Nombre LIKE @filtro);";

                result.Result.total = conn.QuerySingle<int>(
                    sqlCount,
                   lazy.Params);

                var sqlList = @"
                            SELECT
                                Nombre AS item,
                                Nombre AS descripcion
                            FROM tes_autorizaciones
                            WHERE (@filtro IS NULL OR Nombre LIKE @filtro)
                            ORDER BY Nombre  
                            OFFSET @offset ROWS
                            FETCH NEXT @pageSize ROWS ONLY";
                

                result.Result.lista = conn.Query<DropDownListaGenericaModel>(
                    sqlList,
                    lazy.Params).ToList();
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
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var s = scroll ?? 1;

                const string query = @"
                                SELECT TOP 1
                                    nombre, notas, clave, estado,
                                    rango_gen_inicio, rango_gen_corte,
                                    firmas_gen_inicio, firmas_gen_corte
                                FROM tes_autorizaciones
                                WHERE
                                    (@scroll = 1 AND Nombre < @nombre)
                                 OR (@scroll = 2 AND Nombre > @nombre)
                                ORDER BY
                                    CASE WHEN @scroll = 1 THEN Nombre END DESC,
                                    CASE WHEN @scroll = 2 THEN Nombre END ASC;";

                return conn.QueryFirstOrDefault<TesAutorizadoresDto>(query, new { scroll = s, nombre = nombre }) ?? new TesAutorizadoresDto();
            });
        }

        /// <summary>
        /// Obtiene un autorizador de usuario por nombre específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto<TesAutorizadoresDto> Tes_AutorizadoresUsuario_Obtener(int CodEmpresa, string nombre)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {

                const string query = $@"select nombre,
                                                notas,
                                                clave,
                                                estado,
                                                rango_gen_inicio,
                                                rango_gen_corte,
                                                firmas_gen_inicio,
                                                firmas_gen_corte
                                                 from tes_autorizaciones WHERE  nombre = @nombre ";

                return conn.QueryFirstOrDefault<TesAutorizadoresDto>(query, new { nombre }) ?? new TesAutorizadoresDto();
            });
        }

        /// <summary>
        /// Guarda un autorizador de usuario. Si el autorizador ya existe, lo actualiza; si no, lo inserta como nuevo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="autorizador"></param>
        /// <returns></returns>
        public ErrorDto Tes_Autorizadores_Guardar(int CodEmpresa,string usuario, TesAutorizadoresDto autorizador)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                var valida = fxValida(autorizador);
                if (valida.Code == -1)
                {
                    return DbHelper.ErrorResponse(valida.Description!, (int)valida.Code);
                }

                //valida si existe
                var query = $@"SELECT COUNT(*) FROM tes_autorizaciones WHERE nombre = @nombre";
                var existe = conn.QueryFirstOrDefault<int>(query, new { autorizador.nombre });
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
                   conn.Execute(query, autorizador);

                    _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
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
                    conn.Execute(query, autorizador);
                    _mSecurityMainDb.Bitacora
                        (new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Usuario Autorizador : {autorizador.nombre}",
                            Movimiento = "Registra - WEB",
                            Modulo = vModulo
                        });
                }

                return response;
            }
            catch (Exception ex)
            {
               return DbHelper.ErrorResponse(ex.Message);
            }
           
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
            
            string sql = $@"DELETE FROM tes_autorizaciones WHERE UPPER(nombre) = @nombre";
            var parametros = new { nombre = usuario.ToUpper() };

            var response = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql, parametros);

            _mSecurityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Usuario Autorizador : {usuario}",
                Movimiento = "Elimina - WEB",
                Modulo = vModulo
            });

            return response;
        }
    }
}
