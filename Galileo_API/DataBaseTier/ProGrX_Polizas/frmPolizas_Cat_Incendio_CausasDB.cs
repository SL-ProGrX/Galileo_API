using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizasCatIncendioCausasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 11;

        public FrmPolizasCatIncendioCausasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

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

        public ErrorDto<List<IncendioCausaDto>> IncendioCausas_Lista(int codEmpresa)
        {
            var query = @"SELECT ID, descripcion AS Descripcion, Activo FROM VIV_POLIZAS_INCENDIO_CAUSA ORDER BY ID";
            return DbHelper.ExecuteListQuery<IncendioCausaDto>(_portalDb, codEmpresa, query);
        }

        public ErrorDto<bool> IncendioCausas_Insertar(int codEmpresa, IncendioCausaSaveParams param)
        {
            const string sql = @"
                INSERT INTO VIV_POLIZAS_INCENDIO_CAUSA
                (descripcion, Activo, Registro_Fecha, Registro_Usuario)
                VALUES (@Descripcion, @Activo, dbo.MyGetdate(), @Usuario)";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Pólizas de Incendio, Causa: {param.Descripcion}", "Registra - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        public ErrorDto<bool> IncendioCausas_Actualizar(int codEmpresa, IncendioCausaUpdateParams param)
        {
            const string sql = @"
                UPDATE VIV_POLIZAS_INCENDIO_CAUSA
                SET descripcion = @Descripcion,
                    Activo = @Activo,
                    Actualiza_Fecha = dbo.MyGetdate(),
                    Actualiza_Usuario = @Usuario
                WHERE ID = @ID";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Pólizas de Incendio, Causa Id: {param.ID}", "Modifica - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }

        public ErrorDto<bool> IncendioCausas_Eliminar(int codEmpresa, IncendioCausaDeleteParams param)
        {
            const string sql = @"DELETE FROM VIV_POLIZAS_INCENDIO_CAUSA WHERE ID = @ID";
            var result = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.Usuario, $"Pólizas de Incendio, Causa Id: {param.ID}", "Elimina - WEB");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(true)
                : DbHelper.CreateErrorResponse<bool>(result.Description ?? "Error", result.Code ?? -1, false);
        }
    }
}
