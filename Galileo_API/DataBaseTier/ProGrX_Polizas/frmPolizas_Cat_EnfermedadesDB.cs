using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasCatEnfermedadesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 11;

        public FrmPolizasCatEnfermedadesDB(IConfiguration config)
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
        /// Consulta la lista de enfermedades de pólizas de vida.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de enfermedades.</returns>
        public ErrorDto<List<EnfermedadVidaDto>> Enfermedades_Lista(int codEmpresa)
        {
            var query = @"SELECT ID, Nombre, Activo FROM VIV_POLIZAS_VIDA_ENFERMEDAD ORDER BY ID";
            return DbHelper.ExecuteListQuery<EnfermedadVidaDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Verifica si existe una enfermedad por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="id">Identificador de la enfermedad.</param>
        /// <returns>Resultado con la cantidad encontrada.</returns>
        public ErrorDto<EnfermedadVidaExisteResult?> Enfermedades_Existe(int codEmpresa, int id)
        {
            var query = @"SELECT COUNT(*) as Existe FROM VIV_POLIZAS_VIDA_ENFERMEDAD WHERE ID = @ID";
            return DbHelper.ExecuteSingleQuery<EnfermedadVidaExisteResult>(_portalDb, codEmpresa, query, default, new { ID = id });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una enfermedad de póliza de vida.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la enfermedad.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Enfermedades_Guardar(int codEmpresa, EnfermedadVidaSaveParams param)
        {
            var existe = param.Id.HasValue ? Enfermedades_Existe(codEmpresa, param.Id.Value).Result?.Existe ?? 0 : 0;
            if (existe == 0)
                return Enfermedades_Insertar(codEmpresa, param);
            else
                return Enfermedades_Actualizar(codEmpresa, param);
        }

        /// <summary>
        /// Inserta una nueva enfermedad de póliza de vida.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la enfermedad.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        private ErrorDto<bool> Enfermedades_Insertar(int codEmpresa, EnfermedadVidaSaveParams param)
        {
            const string sql = @"
                INSERT INTO VIV_POLIZAS_VIDA_ENFERMEDAD
                (Nombre, Activo, Registro_Fecha, Registro_Usuario)
                VALUES (@Nombre, @Activo, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Pólizas de Vida, Enfermedad Nombre: {param.Nombre}", "Registra - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        /// <summary>
        /// Actualiza una enfermedad de póliza de vida existente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la enfermedad.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        private ErrorDto<bool> Enfermedades_Actualizar(int codEmpresa, EnfermedadVidaSaveParams param)
        {
            const string sql = @"
                UPDATE VIV_POLIZAS_VIDA_ENFERMEDAD
                SET Nombre = @Nombre,
                    Activo = @Activo,
                    Actualiza_Fecha = dbo.MyGetdate(),
                    Actualiza_Usuario = @Usuario
                WHERE ID = @Id";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Pólizas de Vida, Enfermedad Id: {param.Id}", "Modifica - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        /// <summary>
        /// Elimina una enfermedad de póliza de vida por su identificador.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros para eliminar la enfermedad.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> Enfermedades_Eliminar(int codEmpresa, EnfermedadVidaDeleteParams param)
        {
            const string sql = @"DELETE FROM VIV_POLIZAS_VIDA_ENFERMEDAD WHERE ID = @Id";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Pólizas de Vida, Enfermedad Id: {param.Id}", "Elimina - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }
    }
}
