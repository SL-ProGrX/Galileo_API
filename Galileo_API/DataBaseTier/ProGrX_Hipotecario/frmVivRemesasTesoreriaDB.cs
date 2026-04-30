using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivRemesasTesoreriaDB
    {        
        private readonly PortalDB _portalDb;

        public FrmVivRemesasTesoreriaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las 50 remesas de tesorería más recientes, incluyendo los campos calculados Casos y Monto.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa (para la conexión).</param>
        /// <returns>ErrorDto con la lista de remesas y sus datos asociados.</returns>
        public ErrorDto<List<RemesasTesoreriaObtenerDto>> RemesasTesoreria_Obtener(int codEmpresa)
        {
            var sql = @"select TOP 50
                T.*, isnull(D.Casos, 0) as Casos, isnull(D.Monto, 0) as Monto
                from viviendaRemesasTesoreria T
                left join vCrd_Hipotecario_Remesa_Tes_Rsm D on T.Remesa = D.Remesa
                order by T.RegistroFecha desc";
            return DbHelper.ExecuteListQuery<RemesasTesoreriaObtenerDto>(_portalDb, codEmpresa, sql, null);
        }


        /// <summary>
        /// Inserta una nueva remesa de tesorería y retorna el nuevo id Remesa.
        /// </summary>
        public ErrorDto<int> RemesasTesoreria_Insertar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
        {
            var sqlUltimo = "select isnull(max(Remesa), 0) + 1 as Ultimo from viviendaRemesasTesoreria";
            int nuevoId = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QuerySingle<int>(sqlUltimo)
            ).Result;

            var sqlInsert = @"insert viviendaRemesasTesoreria
                            (
                                Remesa, RegistroUsuario, RegistroFecha, Estado, FechaInicio, FechaCorte, notas
                            )
                            values
                            (
                                @Remesa, @Usuario, dbo.MyGetdate(), 'A', @FechaInicio, @FechaCorte, @Notas
                            )";
            var parametros = new {
                Remesa = nuevoId,
                dto.Usuario,
                dto.FechaInicio,
                dto.FechaCorte,
                dto.Notas
            };
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sqlInsert, parametros)
            );
            return new ErrorDto<int> { Result = nuevoId, Code = 0, Description = "Ok" };
        }

        /// <summary>
        /// Actualiza una remesa de tesorería existente.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreria_Actualizar(int codEmpresa, RemesaTesoreriaUpsertDto dto)
        {
            var sql = @"update viviendaRemesasTesoreria
                    set RegistroUsuario = @Usuario, FechaInicio = @FechaInicio, FechaCorte = @FechaCorte, notas = @Notas
                    where Remesa = @Remesa";
            var parametros = new {
                dto.Remesa,
                dto.Usuario,
                dto.FechaInicio,
                dto.FechaCorte,
                dto.Notas
            };
            int rows = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Execute(sql, parametros)
            ).Result;
            return new ErrorDto<bool> { Result = rows > 0, Code = rows > 0 ? 0 : -2, Description = rows > 0 ? "Ok" : "No se actualizó" };
        }

        /// <summary>
        /// Elimina los detalles de una remesa y limpia los campos de Tesorería en ViviendaDesembolsos.
        /// </summary>
        public ErrorDto<bool> RemesasTesoreriaDetalle_Eliminar(int codEmpresa, int remesa)
        {
            var sqlDelete = "DELETE FROM viviendaRemesasTesoreria_detalle WHERE Remesa = @Remesa";
            var sqlUpdate = @"UPDATE ViviendaDesembolsos
                        SET TesoreriaRemesa = NULL, TesoreriaSolicitud = NULL, TesoreriaFecha = NULL, TesoreriaUsuario = NULL
                        WHERE TesoreriaRemesa = @Remesa";
            var parameters = new { Remesa = remesa };
            int rowsDelete = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sqlDelete, parameters)).Result;
            int rowsUpdate = DbHelper.WithConn(_portalDb, codEmpresa, conn => conn.Execute(sqlUpdate, parameters)).Result;
            bool ok = rowsDelete > 0 || rowsUpdate > 0;
            return new ErrorDto<bool> { Result = ok, Code = ok ? 0 : -2, Description = ok ? "Ok" : "No se eliminó ningún registro" };
        }
    }
}
