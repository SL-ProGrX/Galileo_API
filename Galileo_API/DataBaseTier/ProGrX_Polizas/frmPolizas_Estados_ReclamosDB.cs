using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasEstadosReclamosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 11;

        public FrmPolizasEstadosReclamosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registra un movimiento en la bitácora (VB6: Bitacora).
        /// </summary>
        private void RegistrarBitacora(int codEmpresa, string? usuario, string movimiento, int idEstado)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = $"Estados para Proceso Reclamos Id: {idEstado}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
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
            return existe == 0
                ? EstadosReclamos_Insertar(codEmpresa, param)
                : EstadosReclamos_Actualizar(codEmpresa, param);
        }

        private ErrorDto<bool> EstadosReclamos_Insertar(int codEmpresa, PolizasEstadosReclamosSaveParams param)
        {
            var sql = @"
                INSERT INTO POLIZAS_RECLAMOS_ESTADOS
                (ID_ESTADO, descripcion, Activo, Registro_Fecha, Registro_Usuario)
                VALUES
                (@Id_Estado, @Descripcion, @Activo, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });

            if (result.Code == 0 && result.Result)
                RegistrarBitacora(codEmpresa, param.Usuario, "Registra - WEB", param.Id_Estado);

            return result;
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
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });

            if (result.Code == 0 && result.Result)
                RegistrarBitacora(codEmpresa, param.Usuario, "Modifica - WEB", param.Id_Estado);

            return result;
        }

        /// <summary>
        /// Elimina un estado de reclamo por ID.
        /// </summary>
        public ErrorDto<bool> EstadosReclamos_Eliminar(int codEmpresa, PolizasEstadosReclamosDeleteParams param)
        {
            var sql = @"DELETE FROM POLIZAS_RECLAMOS_ESTADOS WHERE ID_ESTADO = @Id_Estado";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });

            if (result.Code == 0 && result.Result)
                RegistrarBitacora(codEmpresa, param.Usuario, "Elimina - WEB", param.Id_Estado);

            return result;
        }
    }
}
