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
        private const string registra = "Registra - WEB";
        private const string modifica = "Modifica - WEB";
        private const string elimina = "Elimina - WEB";

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
        /// <param name="registroUsuario">Usuario que realiza la operación.</param>
        /// <param name="param">Parámetros del registro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfPersonal_Guardar(int codEmpresa, string registroUsuario, CntXEfPersonalSaveParams param)
        {
            // Verifica existencia usando parámetros anónimos
            var sqlExist = @"SELECT COUNT(1) FROM CNTX_EF_PERSONAL WHERE COD_EF = @CodEf AND COD_CONTABILIDAD = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sqlExist, default,
                new {param.CodEf, param.CodContabilidad }
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
                        registra
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
                        modifica
                    );

                return result;
            }
        }

        /// <summary>
        /// Elimina un registro de personal de EF contable.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="registroUsuario">Usuario que realiza la operación.</param>
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
                    elimina
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

        /// <summary>
        /// Guarda (inserta o actualiza) una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la sección.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfSeccion_Guardar(int codEmpresa, CntXEfSeccionSaveParams param)
        {
            // Verifica existencia
            var sqlExist = @"SELECT COUNT(1) FROM CNTX_EF_SECCIONES WHERE ITEM_ID = @ItemId AND COD_EF = @CodEf AND COD_CONTABILIDAD = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa, sqlExist, default,
                new { param.ItemId, param.CodEf, param.CodContabilidad }
            ).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
            INSERT INTO CNTX_EF_SECCIONES
            (
                COD_EF, COD_CONTABILIDAD, ITEM_ID, ITEM_ID_MADRE, PRIORIDAD, ES_TITULO, TOTALES, DESCRIPCION,
                REGISTRO_USUARIO, REGISTRO_FECHA
            )
            VALUES
            (
                @CodEf, @CodContabilidad, @ItemId, @ItemIdMadre, @Prioridad, @EsTitulo, @Totales, @Descripcion,
                @RegistroUsuario, dbo.Mygetdate()
            )";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(
                        codEmpresa,
                        param.RegistroUsuario,
                        $"EF Sección Insertada: {param.ItemId} - {param.Descripcion}",
                        registra
                    );

                return result;
            }
            else
            {
                var sqlUpdate = @"
            UPDATE CNTX_EF_SECCIONES
            SET
                ITEM_ID_MADRE = @ItemIdMadre,
                PRIORIDAD = @Prioridad,
                ES_TITULO = @EsTitulo,
                TOTALES = @Totales,
                DESCRIPCION = @Descripcion
            WHERE
                ITEM_ID = @ItemId
                AND COD_EF = @CodEf
                AND COD_CONTABILIDAD = @CodContabilidad";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(
                        codEmpresa,
                        param.RegistroUsuario,
                        $"EF Sección Modificada: {param.ItemId} - {param.Descripcion}",
                        modifica
                    );

                return result;
            }
        }

        /// <summary>
        /// Elimina una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación de la sección.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfSeccion_Eliminar(int codEmpresa, CntXEfSeccionDeleteParams param)
        {
            var sql = @"DELETE FROM CNTX_EF_SECCIONES WHERE ITEM_ID = @ItemId AND COD_EF = @CodEf AND COD_CONTABILIDAD = @CodContabilidad";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });

            if (result.Code == 0)
                RegistrarBitacora(
                    codEmpresa,
                    param.RegistroUsuario,
                    $"EF Sección Eliminada: {param.ItemId}",
                    elimina
                );

            return result;
        }

        /// <summary>
        /// Obtiene la lista simple de secciones (items) de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codEf">Código de EF.</param>
        /// <returns>Lista simple de secciones.</returns>
        public ErrorDto<List<CntXEfSeccionSimpleDto>> CntXEfSeccionesItems_Lista(int codEmpresa, int codContabilidad, string codEf)
        {
            var sql = @"
                select 
                    ITEM_ID as ItemId,
                    DESCRIPCION
                from CNTX_EF_SECCIONES
                where ES_TITULO = 0
                  and COD_EF = @codEf
                  and COD_CONTABILIDAD = @codContabilidad
                order by ITEM_ID_MADRE, PRIORIDAD, ITEM_ID";
            return DbHelper.ExecuteListQuery<CntXEfSeccionSimpleDto>(_portalDb, codEmpresa, sql, new { codEf, codContabilidad });
        }

        /// <summary>
        /// Obtiene la lista de cuentas disponibles para asignar a una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro de cuentas.</param>
        /// <returns>Lista de cuentas disponibles.</returns>
        public ErrorDto<List<CntXCuentaDto>> CntXEfCuentasDisponibles_Lista(int codEmpresa, CntXCuentaFiltroParams param)
        {
            var sql = @"
                select 
                    Cta.COD_CUENTA as CodCuenta,
                    Cta.COD_CUENTA_MASK as CodCuentaMask,
                    Cta.DESCRIPCION,
                    Cta.COD_DIVISA as CodDivisa,
                    Cta.ACEPTA_MOVIMIENTOS as AceptaMovimientos
                from CntX_Cuentas Cta
                where Cta.COD_CONTABILIDAD = @CodContabilidad
                  and Cta.COD_CUENTA NOT IN (
                        select R.Cuenta
                        from CntX_EF_Cuentas Efc
                        cross apply dbo.fxCntX_CuentasCascada_Down(Efc.cod_contabilidad, Efc.cod_Cuenta) R
                        where Efc.COD_CONTABILIDAD = @CodContabilidad
                          and Efc.COD_EF = @CodEf
                          and Efc.ITEM_ID = @ItemId
                  )
            ";

            // Lógica de rango
            if (!string.IsNullOrWhiteSpace(param.CuentaInicio) && !string.IsNullOrWhiteSpace(param.CuentaFin))
            {
                sql += " and Cta.COD_CUENTA BETWEEN @CuentaInicio AND @CuentaFin";
            }
            else if (!string.IsNullOrWhiteSpace(param.CuentaInicio))
            {
                sql += " and Cta.COD_CUENTA_MASK like @CuentaInicioMask";
            }

            sql += " order by Cta.COD_CUENTA";

            var parametros = new
            {
                param.CodContabilidad,
                param.CodEf,
                param.ItemId,
                param.CuentaInicio,
                param.CuentaFin,
                CuentaInicioMask = param.CuentaInicio + "%"
            };

            return DbHelper.ExecuteListQuery<CntXCuentaDto>(_portalDb, codEmpresa, sql, parametros);
        }

        /// <summary>
        /// Obtiene la lista de cuentas asignadas a una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codEf">Código de EF.</param>
        /// <param name="itemId">Identificador del item/sección.</param>
        /// <returns>Lista de cuentas asignadas.</returns>
        public ErrorDto<List<CntXCuentaAsignadaDto>> CntXEfCuentasAsignadas_Lista(int codEmpresa, int codContabilidad, string codEf, string itemId)
        {
            var sql = @"
                select 
                    Cta.COD_CUENTA as CodCuenta,
                    Cta.COD_CUENTA_MASK as CodCuentaMask,
                    Cta.DESCRIPCION,
                    Cta.COD_DIVISA as CodDivisa,
                    Cta.ACEPTA_MOVIMIENTOS as AceptaMovimientos,
                    case when isnull(Efc.COD_CUENTA,'') = '' then 0 else 1 end as Asignado
                from CntX_Cuentas Cta
                inner join CntX_EF_Cuentas Efc
                    on Cta.COD_CONTABILIDAD = Efc.COD_CONTABILIDAD
                    and Cta.COD_CUENTA = Efc.COD_CUENTA
                    and Efc.COD_EF = @codEf
                    and Efc.ITEM_ID = @itemId
                where Cta.COD_CONTABILIDAD = @codContabilidad
                order by Cta.COD_CUENTA";
            return DbHelper.ExecuteListQuery<CntXCuentaAsignadaDto>(_portalDb, codEmpresa, sql, new { codContabilidad, codEf, itemId });
        }

        /// <summary>
        /// Obtiene la lista de funciones (FX) asignadas a una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codEf">Código de EF.</param>
        /// <param name="itemId">Identificador del item/sección.</param>
        /// <returns>Lista de funciones asignadas.</returns>
        public ErrorDto<List<CntXFxAsignadaDto>> CntXEfFuncionesAsignadas_Lista(int codEmpresa, int codContabilidad, string codEf, string itemId)
        {
            var sql = @"
                select 
                    Cta.COD_FX as CodFx,
                    Cta.FX_NAME as FxName,
                    case when isnull(Efc.COD_FX,'') = '' then 0 else 1 end as Asignado
                from CNTX_EF_FUNCIONES Cta
                left join CNTX_EF_FX Efc
                    on Cta.COD_CONTABILIDAD = Efc.COD_CONTABILIDAD
                    and Cta.COD_FX = Efc.COD_FX
                    and Efc.COD_EF = @codEf
                    and Efc.ITEM_ID = @itemId
                where Cta.COD_CONTABILIDAD = @codContabilidad
                order by Cta.COD_FX";
            return DbHelper.ExecuteListQuery<CntXFxAsignadaDto>(_portalDb, codEmpresa, sql, new { codContabilidad, codEf, itemId });
        }

        /// <summary>
        /// Procesa la asignación o eliminación de una cuenta a una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la operación de cuenta.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfCuenta_Proc(int codEmpresa, CntXEfCuentaProcParams param)
        {
            var sql = "spCntX_EF_Cuentas";
            var dapperParams = new DynamicParameters();
            dapperParams.Add("@Contabilidad", param.CodContabilidad);
            dapperParams.Add("@CodEF", param.CodEf);
            dapperParams.Add("@ItemId", param.ItemId);
            dapperParams.Add("@Cuenta", param.Cuenta);
            dapperParams.Add("@Usuario", param.RegistroUsuario);
            dapperParams.Add("@Mov", param.Movimiento);

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, dapperParams, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            // Bitácora
            if (param.Movimiento == 'A')
                RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Cuenta asignada: {param.Cuenta} a EF:{param.CodEf} Item:{param.ItemId}", "Registra - WEB");
            else if (param.Movimiento == 'E')
                RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Cuenta eliminada: {param.Cuenta} de EF:{param.CodEf} Item:{param.ItemId}", "Elimina - WEB");

            return DbHelper.CreateOkResponse(true);
        }

        /// <summary>
        /// Procesa la asignación o eliminación de una función (FX) a una sección de EF personalizado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de la operación de función.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfFx_Proc(int codEmpresa, CntXEfFxProcParams param)
        {
            var sql = "spCntX_EF_Fxs";
            var dapperParams = new DynamicParameters();
            dapperParams.Add("@Contabilidad", param.CodContabilidad);
            dapperParams.Add("@CodEF", param.CodEf);
            dapperParams.Add("@ItemId", param.ItemId);
            dapperParams.Add("@Cod_Fx", param.CodFx);
            dapperParams.Add("@Usuario", param.RegistroUsuario);
            dapperParams.Add("@Mov", param.Movimiento);

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, dapperParams, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            // Bitácora
            if (param.Movimiento == 'A')
                RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"FX asignada: {param.CodFx} a EF:{param.CodEf} Item:{param.ItemId}", "Registra - WEB");
            else if (param.Movimiento == 'E')
                RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"FX eliminada: {param.CodFx} de EF:{param.CodEf} Item:{param.ItemId}", "Elimina - WEB");

            return DbHelper.CreateOkResponse(true);
        }

        /// <summary>
        /// Procesa los resultados de EF personalizado para un periodo y tipo específico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del proceso.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXEfProcesa(int codEmpresa, CntXEfProcesaParams param)
        {
            var sql = "spCntX_EF_Procesa";
            var dapperParams = new DynamicParameters();
            dapperParams.Add("@Contabilidad", param.CodContabilidad);
            dapperParams.Add("@CodEF", param.CodEf);
            dapperParams.Add("@Anio", param.Anio);
            dapperParams.Add("@Mes", param.Mes);
            dapperParams.Add("@Usuario", param.RegistroUsuario);
            dapperParams.Add("@Tipo", param.Tipo);
            dapperParams.Add("@Expresado", param.Expresado);

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, dapperParams, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            RegistrarBitacora(
                codEmpresa,
                param.RegistroUsuario,
                $"Procesa resultados EF: {param.CodEf} Año: {param.Anio} Mes: {param.Mes} Tipo: {param.Tipo} Expresado: {param.Expresado}",
                "Procesa - WEB"
            );

            return DbHelper.CreateOkResponse(true);
        }
    }
}
