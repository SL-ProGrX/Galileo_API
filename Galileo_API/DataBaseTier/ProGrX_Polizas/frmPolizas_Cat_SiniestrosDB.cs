using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasCatSiniestrosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 11; 

        public FrmPolizasCatSiniestrosDB(IConfiguration config)
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
        /// Consulta la lista de tipos de siniestros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de siniestros.</returns>
        public ErrorDto<List<SiniestroTipoDto>> Siniestros_Lista(int codEmpresa)
        {
            var query = @"SELECT ID_SINIESTRO, descripcion, Activo FROM POLIZAS_SINIESTROS_TIPOS ORDER BY ID_SINIESTRO";
            return DbHelper.ExecuteListQuery<SiniestroTipoDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Verifica si existe un tipo de siniestro por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="id">Identificador del tipo de siniestro.</param>
        /// <returns>Resultado con la cantidad encontrada.</returns>
        public ErrorDto<SiniestroTipoExisteResult?> Siniestros_Existe(int codEmpresa, int id)
        {
            var query = @"SELECT COUNT(*) as Existe FROM POLIZAS_SINIESTROS_TIPOS WHERE ID_SINIESTRO = @ID";
            return DbHelper.ExecuteSingleQuery<SiniestroTipoExisteResult>(_portalDb, codEmpresa, query, default, new { ID = id });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de siniestro.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del tipo de siniestro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Siniestros_Guardar(int codEmpresa, SiniestroTipoSaveParams param)
        {
            var existe = Siniestros_Existe(codEmpresa, param.Id_Siniestro).Result?.Existe ?? 0;
            return existe == 0
                ? Siniestros_Insertar(codEmpresa, param)
                : Siniestros_Actualizar(codEmpresa, param);
        }

        /// <summary>
        /// Inserta un nuevo tipo de siniestro.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del tipo de siniestro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        private ErrorDto<bool> Siniestros_Insertar(int codEmpresa, SiniestroTipoSaveParams param)
        {
            const string sql = @"
                INSERT INTO POLIZAS_SINIESTROS_TIPOS
                (ID_SINIESTRO, descripcion, Activo, Registro_Fecha, Registro_Usuario)
                VALUES (@Id_Siniestro, @Descripcion, @Activo, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Tipos de Siniestros Id: {param.Id_Siniestro}", "Registra - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        /// <summary>
        /// Actualiza un tipo de siniestro existente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del tipo de siniestro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        private ErrorDto<bool> Siniestros_Actualizar(int codEmpresa, SiniestroTipoSaveParams param)
        {
            const string sql = @"
                UPDATE POLIZAS_SINIESTROS_TIPOS
                SET descripcion = @Descripcion,
                    Activo = @Activo,
                    Modifica_Fecha = dbo.MyGetdate(),
                    Modifica_Usuario = @Usuario
                WHERE ID_SINIESTRO = @Id_Siniestro";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Tipos de Siniestros Id: {param.Id_Siniestro}", "Modifica - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina un tipo de siniestro por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para eliminar el tipo de siniestro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Siniestros_Eliminar(int codEmpresa, SiniestroTipoDeleteParams param)
        {
            const string sql = @"DELETE FROM POLIZAS_SINIESTROS_TIPOS WHERE ID_SINIESTRO = @Id_Siniestro";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Tipos de Siniestros Id: {param.Id_Siniestro}", "Elimina - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }
    }
}
