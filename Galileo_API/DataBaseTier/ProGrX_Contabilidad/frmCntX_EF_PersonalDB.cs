using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXEfPersonalDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 20;

        public FrmCntXEfPersonalDB(IConfiguration config)
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

        /// <summary>
        /// Obtiene la lista de personal de EF contable por código de empresa y contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista de personal de EF contable.</returns>
        public ErrorDto<List<CntXEfPersonalDto>> CntXEfPersonal_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select COD_EF, DESCRIPCION, ACTIVO
                from CNTX_EF_PERSONAL
                where cod_contabilidad = @codContabilidad
                order by COD_EF";
            return DbHelper.ExecuteListQuery<CntXEfPersonalDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un registro de personal de EF contable.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del registro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfPersonal_Guardar(int codEmpresa, string registroUsuario, CntXEfPersonalSaveParams param)
        {
            // Verifica existencia usando parámetros anónimos
            var sqlExist = @"SELECT COUNT(1) FROM CNTX_EF_PERSONAL WHERE COD_EF = @CodEf AND COD_CONTABILIDAD = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sqlExist, default,
                new { CodEf = param.CodEf, CodContabilidad = param.CodContabilidad }
            ).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
                    INSERT INTO CNTX_EF_PERSONAL
                    (
                        COD_CONTABILIDAD,
                        COD_EF,
                        DESCRIPCION,
                        ACTIVO,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @CodContabilidad,
                        @CodEf,
                        @Descripcion,
                        @Activo,
                        @RegistroUsuario,
                        dbo.Mygetdate()
                    )";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, new
                    {
                        param.CodContabilidad,
                        param.CodEf,
                        param.Descripcion,
                        param.Activo,
                        RegistroUsuario = registroUsuario
                    });
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(
                        codEmpresa,
                        registroUsuario,
                        $"EF Personalizados: {param.CodEf} - {param.Descripcion}",
                        "Registra - WEB"
                    );

                return result;
            }
            else
            {
                var sqlUpdate = @"
                    UPDATE CNTX_EF_PERSONAL
                    SET DESCRIPCION = @Descripcion,
                        ACTIVO = @Activo
                    WHERE COD_EF = @CodEf
                      AND COD_CONTABILIDAD = @CodContabilidad";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, new
                    {
                        param.CodContabilidad,
                        param.CodEf,
                        param.Descripcion,
                        param.Activo
                    });
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(
                        codEmpresa,
                        registroUsuario,
                        $"EF Personalizados: {param.CodEf} - {param.Descripcion}",
                        "Modifica - WEB"
                    );

                return result;
            }
        }

        /// <summary>
        /// Elimina un registro de personal de EF contable.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfPersonal_Eliminar(int codEmpresa, string registroUsuario, CntXEfPersonalDeleteParams param)
        {
            var sql = @"DELETE FROM CNTX_EF_PERSONAL WHERE COD_EF = @CodEf AND COD_CONTABILIDAD = @CodContabilidad";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, new
                {
                    param.CodContabilidad,
                    param.CodEf
                });
                return rows > 0;
            });

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    registroUsuario,
                    $"EF Personalizados: {param.CodEf}",
                    "Elimina - WEB"
                );

            return result;
        }

        /// <summary>
        /// Obtiene las secciones de EF personalizado por contabilidad y EF.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codEf">Código de EF.</param>
        /// <returns>Lista de secciones.</returns>
        public ErrorDto<List<CntXEfSeccionDto>> CntXEfSecciones_Lista(int codEmpresa, int codContabilidad, string codEf)
        {
            var sql = @"
                select 
                    ITEM_ID as ItemId,
                    ITEM_ID_MADRE as ItemIdMadre,
                    PRIORIDAD,
                    CAST(ES_TITULO as int) as EsTitulo,
                    isnull(TOTALES,0) as Totales,
                    DESCRIPCION
                from CNTX_EF_SECCIONES
                where cod_contabilidad = @codContabilidad
                  and cod_EF = @codEf
                order by ITEM_ID, ITEM_ID_MADRE, PRIORIDAD";
            return DbHelper.ExecuteListQuery<CntXEfSeccionDto>(_portalDb, codEmpresa, sql, new { codContabilidad, codEf });
        }
    }
}
