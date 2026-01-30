using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasCatGruposDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 11;

        public FrmPolizasCatGruposDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registra un movimiento en la bitácora del sistema.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza la acción.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        /// <param name="movimiento">Tipo de movimiento (Registra, Modifica, Elimina).</param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        /// <summary>
        /// Consulta la lista de grupos de pólizas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de grupos de pólizas.</returns>
        public ErrorDto<List<PolizaGrupoDto>> PolizaGrupos_Lista(int codEmpresa)
        {
            var query = @"SELECT ID_POLIZA_GRUPO as Id_Poliza_Grupo, descripcion AS Descripcion, TIPO_APLICACION as Tipo_Aplicacion, Activo
                          FROM POLIZAS_GRUPO
                          ORDER BY ID_POLIZA_GRUPO";
            return DbHelper.ExecuteListQuery<PolizaGrupoDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Verifica si existe un grupo de póliza por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="id">Identificador del grupo.</param>
        /// <returns>Resultado con la cantidad encontrada.</returns>
        public ErrorDto<PolizaGrupoExisteResult?> PolizaGrupos_Existe(int codEmpresa, int id)
        {
            var query = @"SELECT COUNT(*) as Existe FROM POLIZAS_GRUPO WHERE ID_POLIZA_GRUPO = @ID";
            return DbHelper.ExecuteSingleQuery<PolizaGrupoExisteResult>(_portalDb, codEmpresa, query, default, new { ID = id });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un grupo de póliza.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del grupo.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> PolizaGrupos_Guardar(int codEmpresa, PolizaGrupoSaveParams param)
        {
            var existe = PolizaGrupos_Existe(codEmpresa, param.Id_Poliza_Grupo).Result?.Existe ?? 0;
            return existe == 0
                ? PolizaGrupos_Insertar(codEmpresa, param)
                : PolizaGrupos_Actualizar(codEmpresa, param);
        }

        /// <summary>
        /// Inserta un nuevo grupo de póliza.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del grupo.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        private ErrorDto<bool> PolizaGrupos_Insertar(int codEmpresa, PolizaGrupoSaveParams param)
        {
            const string sql = @"
                INSERT INTO POLIZAS_GRUPO
                (ID_POLIZA_GRUPO, descripcion, TIPO_APLICACION, Activo, Registro_Fecha, Registro_Usuario)
                VALUES (@Id_Poliza_Grupo, @Descripcion, @Tipo_Aplicacion, @Activo, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Grupo de Aplicación Id: {param.Id_Poliza_Grupo}", "Registra - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        /// <summary>
        /// Actualiza un grupo de póliza existente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del grupo.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        private ErrorDto<bool> PolizaGrupos_Actualizar(int codEmpresa, PolizaGrupoSaveParams param)
        {
            const string sql = @"
                UPDATE POLIZAS_GRUPO
                SET descripcion = @Descripcion,
                    TIPO_APLICACION = @Tipo_Aplicacion,
                    Activo = @Activo,
                    Modifica_Fecha = dbo.MyGetdate(),
                    Modifica_Usuario = @Usuario
                WHERE ID_POLIZA_GRUPO = @Id_Poliza_Grupo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Grupo de Aplicación Id: {param.Id_Poliza_Grupo}", "Modifica - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un grupo de póliza por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para eliminar el grupo.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> PolizaGrupos_Eliminar(int codEmpresa, PolizaGrupoDeleteParams param)
        {
            const string sql = @"DELETE FROM POLIZAS_GRUPO WHERE ID_POLIZA_GRUPO = @Id_Poliza_Grupo";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Grupo de Aplicación Id: {param.Id_Poliza_Grupo}", "Elimina - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }
    }
}
