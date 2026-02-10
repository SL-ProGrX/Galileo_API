using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCBancosAutorizadosDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCBancosAutorizadosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Inserta bancos autorizados que no existan aún.
        /// </summary>
        public ErrorDto<bool> CxcBancosAutorizados_InsertarFaltantes(int codEmpresa, CxcBancoAutorizadoInsertParams param)
        {
            var query = @"
                INSERT INTO CxC_Bancos_Autorizados (id_banco, cheques, transferencias, registro_fecha, registro_usuario)
                SELECT id_banco, 0, 0, dbo.MyGetdate(), @Usuario
                FROM Tes_Bancos
                WHERE id_Banco NOT IN (SELECT id_Banco FROM CxC_Bancos_Autorizados)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(query, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Obtiene la lista de bancos autorizados con su descripción y permisos.
        /// </summary>
        public ErrorDto<List<CxcBancoAutorizadoResult>> CxcBancosAutorizados_Lista(int codEmpresa)
        {
            var query = @"
                SELECT X.id_banco, B.descripcion, X.cheques, X.transferencias
                FROM CxC_Bancos_Autorizados X
                INNER JOIN Tes_Bancos B ON X.id_banco = B.id_Banco
                ORDER BY B.id_banco";
            return DbHelper.ExecuteListQuery<CxcBancoAutorizadoResult>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Actualiza el campo cheques de un banco autorizado.
        /// </summary>
        public ErrorDto<bool> CxcBancosAutorizados_UpdateCheques(int codEmpresa, CxcBancoAutorizadoUpdateChequesParams param)
        {
            var query = @"UPDATE CxC_Bancos_Autorizados SET cheques = @Cheques WHERE id_Banco = @Id_Banco";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(query, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Actualiza el campo transferencias de un banco autorizado.
        /// </summary>
        public ErrorDto<bool> CxcBancosAutorizados_UpdateTransferencias(int codEmpresa, CxcBancoAutorizadoUpdateTransferenciasParams param)
        {
            var query = @"UPDATE CxC_Bancos_Autorizados SET transferencias = @Transferencias WHERE id_Banco = @Id_Banco";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(query, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Borra un banco autorizado por id_banco.
        /// </summary>
        public ErrorDto<bool> CxcBancosAutorizados_Borrar(int codEmpresa, int idBanco, string usuario)
        {
            const string query = "DELETE FROM CxC_Bancos_Autorizados WHERE id_banco = @idBanco";
            var parametros = new { idBanco };
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(query, parametros);
                return rows > 0;
            });
        }
    }
}
