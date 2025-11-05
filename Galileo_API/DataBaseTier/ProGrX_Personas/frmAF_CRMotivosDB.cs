using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CRMotivosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_CRMotivosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de motivos de renuncia con filtros, orden y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_CRMotivosData>> AF_CRMotivos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<List<AF_CRMotivosData>>()
            {
                Code = 0,
                Description = "OK",
                Result = new List<AF_CRMotivosData>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                object param = new { };
                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where = " WHERE descripcion LIKE @filtro OR COD_MOTIVO LIKE @filtro ";
                    param = new { filtro = $"%{filtros.filtro}%" };
                }

                string orderBy = " ORDER BY COD_MOTIVO ";
                if (!string.IsNullOrEmpty(filtros.sortField))
                {
                    orderBy = $" ORDER BY {filtros.sortField} {(filtros.sortOrder == 1 ? "ASC" : "DESC")} ";
                }

                string paginacion = "";
                if (filtros.paginacion > 0)
                {
                    paginacion = $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }

                string query = $"SELECT COD_MOTIVO, descripcion, ACTIVO, registro_fecha, registro_usuario FROM dbo.AFI_CR_MOTIVOS_RENUNCIA{where}{orderBy}{paginacion}";
                result.Result = connection.Query<AF_CRMotivosData>(query, param).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un motivo de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="motivo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_CRMotivos_Guardar(int CodEmpresa, AF_CRMotivosData motivo, string usuario)
        {
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var existe = MotivoExiste(connection, motivo.cod_motivo);

                if (existe)
                {
                    ActualizarMotivo(connection, motivo);
                    result.Description = "Actualizado correctamente";
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Motivo Renuncia: {motivo.cod_motivo} - {motivo.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    InsertarMotivo(connection, motivo, usuario);
                    result.Description = "Insertado correctamente";
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Motivo Renuncia: {motivo.cod_motivo} - {motivo.descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Elimina un motivo de renuncia por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_motivo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_CRMotivos_Eliminar(int CodEmpresa, string cod_motivo, string usuario)
        {
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var queryDelete = "DELETE FROM dbo.AFI_CR_MOTIVOS_RENUNCIA WHERE COD_MOTIVO = @COD_MOTIVO";
                int rows = connection.Execute(queryDelete, new { COD_MOTIVO = cod_motivo.ToUpper() });
                if (rows > 0)
                {
                    result.Description = "Eliminado correctamente";
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Motivo Renuncia: {cod_motivo}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    result.Code = 1;
                    result.Description = "No se encontró el registro";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        // Métodos privados
        private bool MotivoExiste(SqlConnection connection, string cod_motivo)
        {
            var queryExiste = "SELECT ISNULL(COUNT(*),0) FROM dbo.AFI_CR_MOTIVOS_RENUNCIA WHERE COD_MOTIVO = @COD_MOTIVO";
            var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { COD_MOTIVO = cod_motivo.ToUpper() });
            return existe > 0;
        }

        private void InsertarMotivo(SqlConnection connection, AF_CRMotivosData motivo, string usuario)
        {
            var queryInsert = @"INSERT INTO dbo.AFI_CR_MOTIVOS_RENUNCIA
                                (COD_MOTIVO, descripcion, ACTIVO, registro_fecha, registro_usuario)
                                VALUES (UPPER(@COD_MOTIVO), @descripcion, @ACTIVO, dbo.myGetdate(), @registro_usuario)";
            connection.Execute(queryInsert, new
            {
                COD_MOTIVO = motivo.cod_motivo.ToUpper(),
                descripcion = motivo.descripcion,
                ACTIVO = motivo.activo,
                registro_usuario = usuario
            });
        }

        private void ActualizarMotivo(SqlConnection connection, AF_CRMotivosData motivo)
        {
            var queryUpdate = @"UPDATE dbo.AFI_CR_MOTIVOS_RENUNCIA
                                SET descripcion = @descripcion,
                                    ACTIVO = @ACTIVO
                                WHERE COD_MOTIVO = @COD_MOTIVO";
            connection.Execute(queryUpdate, new
            {
                COD_MOTIVO = motivo.cod_motivo.ToUpper(),
                descripcion = motivo.descripcion,
                ACTIVO = motivo.activo
            });
        }
    }
}
