using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasEstadosReclamosDB
    {
        private readonly PortalDB _portalDb;

        public FrmPolizasEstadosReclamosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Lista todos los estados de reclamos.
        /// </summary>
        public ErrorDto<List<PolizasEstadosReclamosDto>> EstadosReclamos_Listar(int codEmpresa)
        {
            var query = @"SELECT ID_ESTADO, descripcion, Activo FROM POLIZAS_RECLAMOS_ESTADOS ORDER BY ID_ESTADO";
            return DbHelper.ExecuteListQuery<PolizasEstadosReclamosDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Verifica si existe un estado por ID.
        /// </summary>
        public ErrorDto<PolizasEstadosReclamosExisteResult?> EstadosReclamos_Existe(int codEmpresa, int idEstado)
        {
            var query = @"SELECT COUNT(*) as Existe FROM POLIZAS_RECLAMOS_ESTADOS WHERE ID_ESTADO = @ID_ESTADO";
            return DbHelper.ExecuteSingleQuery<PolizasEstadosReclamosExisteResult>(_portalDb, codEmpresa, query, default, new { ID_ESTADO = idEstado });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un estado de reclamo.
        /// </summary>
        public ErrorDto<bool> EstadosReclamos_Guardar(int codEmpresa, PolizasEstadosReclamosSaveParams param)
        {
            var existe = EstadosReclamos_Existe(codEmpresa, param.Id_Estado).Result?.Existe ?? 0;
            if (existe == 0)
                return EstadosReclamos_Insertar(codEmpresa, param);
            else
                return EstadosReclamos_Actualizar(codEmpresa, param);
        }

        private ErrorDto<bool> EstadosReclamos_Insertar(int codEmpresa, PolizasEstadosReclamosSaveParams param)
        {
            var sql = @"
                INSERT INTO POLIZAS_RECLAMOS_ESTADOS
                (ID_ESTADO, descripcion, Activo, Registro_Fecha, Registro_Usuario)
                VALUES
                (@Id_Estado, @Descripcion, @Activo, dbo.MyGetdate(), @Usuario)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        private ErrorDto<bool> EstadosReclamos_Actualizar(int codEmpresa, PolizasEstadosReclamosSaveParams param)
        {
            var sql = @"
                UPDATE POLIZAS_RECLAMOS_ESTADOS
                SET descripcion = @Descripcion,
                    Activo = @Activo,
                    Modifica_Fecha = dbo.MyGetdate(),
                    Modifica_Usuario = @Usuario
                WHERE ID_ESTADO = @Id_Estado";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina un estado de reclamo por ID.
        /// </summary>
        public ErrorDto<bool> EstadosReclamos_Eliminar(int codEmpresa, PolizasEstadosReclamosDeleteParams param)
        {
            var sql = @"DELETE FROM POLIZAS_RECLAMOS_ESTADOS WHERE ID_ESTADO = @Id_Estado";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }
    }
}
