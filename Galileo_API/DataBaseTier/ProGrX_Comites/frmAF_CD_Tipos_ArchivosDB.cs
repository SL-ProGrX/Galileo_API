using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmTipoArchivoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 40;

        public FrmTipoArchivoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }


        /// <summary>
        /// Obtiene los tipo de lista
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TipoArchivoLista> TipoArchivoLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                            SELECT
                                IdTipoArchivo AS id_tipo_archivo,
                                NombreTipoArchivo AS nombre_tipo_archivo,
                                Activo,
                                RegistroUsuario AS usuario
                            FROM AFI_CD_TIPO_ARCHIVO
                            ORDER BY IdTipoArchivo";

                var lista = conn.Query<TipoArchivoData>(sql).ToList();

                return DbHelper.CreateOkResponse(new TipoArchivoLista
                {
                    total = lista.Count,
                    lista = lista
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TipoArchivoLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los tipo de archivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TipoArchivoData>> TipoArchivo_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                                SELECT
                                    IdTipoArchivo AS id_tipo_archivo,
                                    NombreTipoArchivo AS nombre_tipo_archivo,
                                    Activo,
                                    RegistroUsuario AS usuario
                                FROM AFI_CD_TIPO_ARCHIVO
                                ORDER BY IdTipoArchivo";

                var lista = conn.Query<TipoArchivoData>(sql).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TipoArchivoData>>(ex.Message);
            }
        }

    

   
        /// <summary>
        /// Guarda los tipo de archivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioSesion"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto TipoArchivo_Guardar(int CodEmpresa, string usuarioSesion, TipoArchivoData data)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                if (string.IsNullOrWhiteSpace(data.nombre_tipo_archivo))
                    return DbHelper.ErrorResponse("Nombre requerido.", -2);

                if (data.isNew == true)
                {
                    const string insert = @"
                    INSERT INTO AFI_CD_TIPO_ARCHIVO
                    (NombreTipoArchivo, Activo, RegistroFecha, RegistroUsuario)
                    VALUES (@nombre, @activo, dbo.MyGetdate(), @usuario);";

                    conn.Execute(insert, new
                    {
                        nombre = data.nombre_tipo_archivo,
                        activo = data.activo,
                        usuario = usuarioSesion
                    });

                    LogBitacora(CodEmpresa, usuarioSesion,
                        $"Tipo Archivo Id: {data.nombre_tipo_archivo}",
                        "Registra - WEB");

                    return DbHelper.OkResponse("Insertado correctamente.");
                }
                else
                {
                    const string update = @"
                        UPDATE AFI_CD_TIPO_ARCHIVO
                        SET NombreTipoArchivo = @nombre,
                            Activo = @activo,
                            Modifica_Fecha = dbo.MyGetdate(),
                            Modifica_Usuario = @usuario
                        WHERE IdTipoArchivo = @id";

                    conn.Execute(update, new
                    {
                        id = data.id_tipo_archivo,
                        nombre = data.nombre_tipo_archivo,
                        activo = data.activo,
                        usuario = usuarioSesion
                    });

                    LogBitacora(CodEmpresa, usuarioSesion,
                        $"Tipo Archivo Id: {data.id_tipo_archivo}",
                        "Modifica - WEB");

                    return DbHelper.OkResponse("Actualizado correctamente.");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

     
        /// <summary>
        /// Elimina los tipo de archivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto TipoArchivo_Eliminar(int CodEmpresa, int id, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"DELETE FROM AFI_CD_TIPO_ARCHIVO WHERE IdTipoArchivo = @id";

                conn.Execute(sql, new { id });

                LogBitacora(CodEmpresa, usuario,
                    $"Tipo Archivo Id: {id}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Eliminado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

     /// <summary>
     /// Registra en bitacora
     /// </summary>
     /// <param name="empresaId"></param>
     /// <param name="usuario"></param>
     /// <param name="detalle"></param>
     /// <param name="movimiento"></param>
        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

   
    }
}