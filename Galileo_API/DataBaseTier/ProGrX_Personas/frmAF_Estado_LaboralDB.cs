using Dapper;
using Galileo.Models;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAfEstadoLaboralDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 1;

        public FrmAfEstadoLaboralDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista paginada de estados laborales según filtros y ordenamiento.
        /// </summary>
        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Obtener(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            var sortMap = new Dictionary<string, int>
            {
                ["ESTADO_LABORAL"] = 1,
                ["descripcion"] = 2,
                ["activo"] = 3,
                ["Registro_Fecha"] = 4,
                ["Registro_Usuario"] = 5
            };
            var spec = Galileo.DataBaseTier.LazyLoadHelper.Build(filtros, sortMap, "ESTADO_LABORAL");
            string where = spec.HasFilter
                ? "WHERE (ESTADO_LABORAL LIKE @filtro OR descripcion LIKE @filtro OR Registro_Usuario LIKE @filtro)"
                : "";
            string queryTotal = $"SELECT COUNT(ESTADO_LABORAL) FROM AFI_ESTADO_LABORAL {where}";
            string sortField = string.IsNullOrWhiteSpace(filtros?.sortField) ? "ESTADO_LABORAL" : filtros.sortField;
            string queryLista = $@"SELECT ESTADO_LABORAL, descripcion, activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_ESTADO_LABORAL
                               {where}
                               ORDER BY {sortField} {(spec.IsAsc ? "ASC" : "DESC")}
                               OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";
            var total = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryTotal, 0, spec.Params);
            var lista = DbHelper.ExecuteListQuery<EstadoLaboralData>(_portalDb, codEmpresa, queryLista, spec.Params);
            return new ErrorDto<EstadoLaboralLista>
            {
                Code = 0,
                Description = "OK",
                Result = new EstadoLaboralLista
                {
                    Total = total.Result,
                    Lista = lista.Result ?? []
                }
            };
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un estado laboral según si existe o no.
        /// </summary>
        public ErrorDto AF_EstadoLaboral_Guardar(int codEmpresa, string usuario, EstadoLaboralData estado)
        {
            string queryExiste = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESTADO_LABORAL WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, queryExiste, 0, new { estado.Estado_Laboral });
            return existe.Result == 0
                ? AF_EstadoLaboral_Insertar(codEmpresa, usuario, estado)
                : AF_EstadoLaboral_Actualizar(codEmpresa, usuario, estado);
        }

        private ErrorDto AF_EstadoLaboral_Insertar(int codEmpresa, string usuario, EstadoLaboralData estado)
        {
            string query = @"INSERT INTO AFI_ESTADO_LABORAL (Estado_Laboral, Descripcion, activo, registro_fecha, registro_usuario)
                              VALUES (@Estado_Laboral, @Descripcion, @activo, GETDATE(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                estado.Estado_Laboral,
                estado.Descripcion,
                activo = estado.Activo ? 1 : 0,
                Usuario = usuario
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Estado Laboral : {estado.Estado_Laboral}");
            }
            return result;
        }

        private ErrorDto AF_EstadoLaboral_Actualizar(int codEmpresa, string usuario, EstadoLaboralData estado)
        {
            string query = @"UPDATE AFI_ESTADO_LABORAL
                              SET Descripcion = @Descripcion,
                                  activo = @activo
                              WHERE Estado_Laboral = @Estado_Laboral";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new {
                estado.Estado_Laboral,
                estado.Descripcion,
                activo = estado.Activo ? 1 : 0
            });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Estado Laboral : {estado.Estado_Laboral}");
            }
            return result;
        }

        /// <summary>
        /// Elimina un estado laboral por su identificador.
        /// </summary>
        public ErrorDto AF_EstadoLaboral_Eliminar(int codEmpresa, string usuario, string estadoLaboral)
        {
            string query = "DELETE FROM AFI_ESTADO_LABORAL WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, new { ESTADO_LABORAL = estadoLaboral });
            if (result.Code == 0)
            {
                RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Estado Laboral : {estadoLaboral}");
            }
            return result;
        }

        /// <summary>
        /// Valida si un estado laboral ya existe por su identificador.
        /// </summary>
        public ErrorDto AF_EstadoLaboral_Valida(int codEmpresa, string estadoLaboral)
        {
            string query = "SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESTADO_LABORAL WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { ESTADO_LABORAL = estadoLaboral }).Result;
            return existe > 0
                ? new ErrorDto { Code = -1, Description = "El estado laboral ya existe." }
                : new ErrorDto { Code = 0, Description = "El estado laboral es válido." };
        }

        /// <summary>
        /// Exporta la lista de estados laborales según filtros (sin paginación).
        /// </summary>
        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Exportar(int codEmpresa, FiltrosLazyLoadData? filtros)
        {
            var (where, parametros) = BuildWhereFiltros(filtros);
            string query = $@"SELECT ESTADO_LABORAL, descripcion, activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_ESTADO_LABORAL
                               {where}
                               ORDER BY ESTADO_LABORAL";
            var lista = DbHelper.ExecuteListQuery<EstadoLaboralData>(_portalDb, codEmpresa, query, parametros);
            return new ErrorDto<EstadoLaboralLista>
            {
                Code = 0,
                Description = "OK",
                Result = new EstadoLaboralLista
                {
                    Total = lista.Result?.Count ?? 0,
                    Lista = lista.Result ?? []
                }
            };
        }

        private static (string where, object? parametros) BuildWhereFiltros(FiltrosLazyLoadData? filtros)
        {
            if (filtros != null && !string.IsNullOrEmpty(filtros.filtro))
            {
                string where = " WHERE ( ESTADO_LABORAL LIKE @Filtro OR descripcion LIKE @Filtro OR Registro_Usuario LIKE @Filtro ) ";
                var parametros = new { Filtro = "%" + filtros.filtro + "%" };
                return (where, parametros);
            }
            return ("", null);
        }

        /// <summary>
        /// Construye la consulta paginada y los parámetros para Estado Laboral.
        /// </summary>


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
