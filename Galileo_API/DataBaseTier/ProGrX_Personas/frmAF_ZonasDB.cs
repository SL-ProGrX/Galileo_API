using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfZonasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;        

        public FrmAfZonasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de zonas con total y paginación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="filtros">JSON con filtros de búsqueda, orden y paginación.</param>
        public ErrorDto<ZonasLista> AF_ZonasLista_Obtener(int codEmpresa, FiltrosLazyLoadData? filtrosObj)
        {
            string filtro = filtrosObj?.filtro ?? string.Empty;
            // Switch seguro para columna de ordenamiento (evita SQL Injection)
            string order = filtrosObj?.sortField?.ToLowerInvariant() switch
            {
                "cod_zona" => "cod_zona",
                "descripcion" => "descripcion",
                "activa" => "activa",
                "registro_usuario" => "registro_usuario",
                "registro_fecha" => "registro_fecha",
                _ => "cod_zona"
            };
            string sortOrderStr = filtrosObj?.sortOrder == 0 ? "DESC" : "ASC";
            int pagina = filtrosObj?.pagina ?? 0;
            int paginacion = filtrosObj?.paginacion ?? 10;

            string where = string.IsNullOrWhiteSpace(filtro) ? "" : "WHERE descripcion LIKE @Filtro";
            // Solo columnas permitidas pueden llegar aquí. Revisado por switch seguro arriba.
            string sqlTotal = $"SELECT COUNT(cod_zona) FROM afi_zonas {where}";
            string sqlLista = $@"
                SELECT cod_zona AS Cod_Zona, descripcion AS Descripcion, activa AS Activa, registro_usuario AS Registro_Usuario, registro_fecha AS Registro_Fecha
                FROM afi_zonas
                {where}
                ORDER BY {order} {sortOrderStr}
                OFFSET @Pagina ROWS FETCH NEXT @Paginacion ROWS ONLY";

            var total = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sqlTotal, 0, new { Filtro = "%" + filtro + "%" });
            var lista = DbHelper.ExecuteListQuery<ZonasData>(
                _portalDb, codEmpresa, sqlLista, new { Filtro = "%" + filtro + "%", Pagina = pagina, Paginacion = paginacion });

            return new ErrorDto<ZonasLista>
            {
                Code = 0,
                Description = "OK",
                Result = new ZonasLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Obtiene una lista de zonas sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda y orden.</param>
        public ErrorDto<List<ZonasData>> AF_Zonas_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            string where = "";
            object? parametros = null;
            if (!string.IsNullOrEmpty(filtros?.filtro))
            {
                where = " WHERE (cod_zona LIKE @Filtro OR descripcion LIKE @Filtro) ";
                parametros = new { Filtro = "%" + filtros.filtro + "%" };
            }

            string query = $@"SELECT cod_zona AS Cod_Zona, descripcion AS Descripcion, activa AS Activa, registro_usuario AS Registro_Usuario, registro_fecha AS Registro_Fecha
                              FROM afi_zonas
                              {where}
                              ORDER BY cod_zona";

            return DbHelper.ExecuteListQuery<ZonasData>(_portalDb, codEmpresa, query, parametros);
        }

        
        /// <summary>
        /// Inserta o actualiza una zona según existencia.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="zona">Datos de la zona a guardar.</param>
        public ErrorDto AF_Zonas_Guardar(int codEmpresa, string usuario, ZonasData zona)
        {
            try
            {
                string sqlExiste = "SELECT COALESCE(COUNT(*), 0) FROM afi_zonas WHERE cod_zona = @cod_zona";
                var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExiste, 0, new { cod_zona = zona.Cod_Zona });

                if (existe.Result > 0)
                {
                    // Actualizar
                    string sqlUpdate = @"UPDATE afi_zonas
                                         SET descripcion = @descripcion,
                                             activa = @activa
                                         WHERE cod_zona = @cod_zona";
                    var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlUpdate, new
                    {
                        cod_zona = zona.Cod_Zona,
                        descripcion = zona.Descripcion,
                        activa = zona.Activa ? 1 : 0
                    });

                    if (resp.Code < 0)
                        return resp;

                    RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Zona Doc.: {zona.Cod_Zona} - {zona.Descripcion}");

                    return resp;
                }
                else
                {
                    // Insertar
                    string sqlInsert = @"INSERT INTO afi_zonas (cod_zona, descripcion, activa, registro_fecha, registro_usuario)
                                         VALUES (@cod_zona, @descripcion, @activa, dbo.mygetdate(), @registro_usuario)";
                    var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlInsert, new
                    {
                        cod_zona = zona.Cod_Zona,
                        descripcion = zona.Descripcion,
                        activa = zona.Activa ? 1 : 0,
                        registro_usuario = usuario
                    });

                    if (resp.Code < 0)
                        return resp;

                    RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Zona Doc.: {zona.Cod_Zona} - {zona.Descripcion}");

                    return resp;
                }
            }
            catch (SqlException ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Elimina una zona por código y registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <param name="codZona">Código de la zona a eliminar.</param>
        public ErrorDto AF_Zonas_Eliminar(int codEmpresa, string usuario, string codZona)
        {
            try
            {
                string sqlDelete = "DELETE FROM afi_zonas WHERE cod_zona = @cod_zona";
                var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sqlDelete, new { cod_zona = codZona });

                if (resp.Code < 0)
                    return resp;

                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Zona Doc.: {codZona}");

                return resp;
            }
            catch (SqlException ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }

        /// <summary>
        /// Valida si existe una zona por código.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codZona">Código de la zona a validar.</param>
        public ErrorDto<int> AF_Zonas_Valida(int codEmpresa, string codZona)
        {
            string sql = "SELECT COALESCE(COUNT(*), 0) AS Existe FROM afi_zonas WHERE cod_zona = @cod_zona";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0, new { cod_zona = codZona });
        }

        /// <summary>
        /// Obtiene los usuarios asignados a una zona (SP: spAfi_Zonas_Usuario_Asigna_Consulta).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codZona">Código de la zona.</param>
        public ErrorDto<List<ZonaUsuarioAsignadoData>> AF_Zonas_UsuariosAsignados_Obtener(int codEmpresa, string codZona)
        {
            string sp = "spAfi_Zonas_Usuario_Asigna_Consulta";
            var parametros = new { Zona = codZona };
            return DbHelper.ExecuteStoredProcedureList<ZonaUsuarioAsignadoData>(
                _portalDb.ObtenerDbConnStringEmpresa(codEmpresa), sp, parametros);
        }

        /// <summary>
        /// Obtiene las instituciones asignadas a una zona (SP: spAfi_Zonas_Inst_Asigna_Consulta).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codZona">Código de la zona.</param>
        public ErrorDto<List<ZonaInstitucionAsignadaData>> AF_Zonas_InstitucionesAsignadas_Obtener(int codEmpresa, string codZona)
        {
            string sp = "spAfi_Zonas_Inst_Asigna_Consulta";
            var parametros = new { Zona = codZona };
            return DbHelper.ExecuteStoredProcedureList<ZonaInstitucionAsignadaData>(
                _portalDb.ObtenerDbConnStringEmpresa(codEmpresa), sp, parametros);
        }

        /// <summary>
        /// Asigna/desasigna una institución a una zona (SP: spAfi_Zonas_Inst_Asigna_Registra).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codZona">Código de la zona.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="asignar">True para asignar, false para desasignar.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        public ErrorDto AF_Zonas_InstitucionAsignar_Registrar(int codEmpresa, string codZona, int codInstitucion, bool asignar, string usuario)
        {
            // El SP espera: @Zona (varchar), @Codigo (int), @Usuario (varchar), @Movimiento (char)
            string sp = "spAfi_Zonas_Inst_Asigna_Registra";
            var parametros = new
            {
                Zona = codZona,
                Codigo = codInstitucion,
                Usuario = usuario,
                Movimiento = asignar ? "I" : "E"
            };
            return DbHelper.ExecuteNonQuery(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa), sp, parametros);
        }

        /// <summary>
        /// Asigna/desasigna un usuario a una zona (SP: spAfi_Zonas_Usuario_Asigna_Registra).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="codZona">Código de la zona.</param>
        /// <param name="codUsuario">Código del usuario.</param>
        /// <param name="asignar">True para asignar, false para desasignar.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        public ErrorDto AF_Zonas_UsuarioAsignar_Registrar(int codEmpresa, string codZona, string codUsuario, bool asignar, string usuario)
        {
            // El SP espera: @Zona (varchar), @Codigo (varchar), @Usuario (varchar), @Movimiento (char)
            string sp = "spAfi_Zonas_Usuario_Asigna_Registra";
            var parametros = new
            {
                Zona = codZona,
                Codigo = codUsuario,
                Usuario = usuario,
                Movimiento = asignar ? "I" : "E"
            };
            return DbHelper.ExecuteNonQuery(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa), sp, parametros);
        }


        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
