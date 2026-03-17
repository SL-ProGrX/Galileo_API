using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXErCuentasDB
    {

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 20;
        private const string registra = "Registra - WEB";
        private const string modifica = "Modifica - WEB";
        private const string elimina = "Elimina - WEB";

        public FrmCntXErCuentasDB(IConfiguration config)
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
        /// Obtiene el inventario periódico por contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista de inventario periódico.</returns>
        public ErrorDto<List<CntXInvPeriodicoDto>> CntXInvPeriodico_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select I.anio, I.mes, I.cod_cuenta, C.descripcion, I.saldo_final
                from CntX_Inv_Periodico I
                inner join CntX_Cuentas C
                  on I.cod_cuenta = C.cod_cuenta
                 and I.cod_contabilidad = C.cod_contabilidad
                where I.cod_contabilidad = @codContabilidad";
            return DbHelper.ExecuteListQuery<CntXInvPeriodicoDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene la lista de cuentas clasificadas como 'A' para combos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista genérica de cuentas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentasClasificacion(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select C.cod_cuenta as item, C.descripcion
                from CntX_Cuentas C
                inner join CntX_Tipos_Cuentas T
                  on C.cod_contabilidad = T.cod_contabilidad
                 and C.tipo_cuenta = T.tipo_cuenta
                where C.cod_contabilidad = @codContabilidad
                  and T.clasificacion = 'A'
                order by C.cod_cuenta";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un registro de inventario periódico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del registro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXInvPeriodico_Guardar(int codEmpresa, CntXInvPeriodicoSaveParams param)
        {
            var sqlExist = @"SELECT COUNT(1) FROM CntX_Inv_Periodico WHERE cod_contabilidad = @CodContabilidad AND anio = @Anio AND mes = @Mes AND cod_cuenta = @CodCuenta";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExist, default, new
            {
                param.CodContabilidad,
                param.Anio,
                param.Mes,
                param.CodCuenta
            }).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
                    insert into CntX_Inv_Periodico
                        (cod_contabilidad, anio, mes, cod_cuenta, saldo_final)
                    values
                        (@CodContabilidad, @Anio, @Mes, @CodCuenta, @SaldoFinal)";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Inventario Periódico Insertado: {param.CodCuenta} Año:{param.Anio} Mes:{param.Mes}", registra);

                return result;
            }
            else
            {
                var sqlUpdate = @"
                    update CntX_Inv_Periodico
                    set saldo_final = @SaldoFinal
                    where anio = @Anio
                      and mes = @Mes
                      and cod_cuenta = @CodCuenta
                      and cod_contabilidad = @CodContabilidad";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Inventario Periódico Modificado: {param.CodCuenta} Año:{param.Anio} Mes:{param.Mes}", modifica);

                return result;
            }
        }

        /// <summary>
        /// Elimina un registro de inventario periódico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXInvPeriodico_Eliminar(int codEmpresa, CntXInvPeriodicoDeleteParams param)
        {
            var sql = @"
                delete from CntX_Inv_Periodico
                where cod_contabilidad = @CodContabilidad
                  and anio = @Anio
                  and mes = @Mes
                  and cod_cuenta = @CodCuenta";
            var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });

            if (result.Code == 0)
                RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Inventario Periódico Eliminado: {param.CodCuenta} Año:{param.Anio} Mes:{param.Mes}", elimina);

            return result;
        }

        /// <summary>
        /// Valida si una cuenta pertenece a la clasificación 'A'.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <param name="codCuenta">Código de cuenta sin formato.</param>
        /// <returns>Total de coincidencias.</returns>
        public ErrorDto<int> CntXCuentasClasificacionA_Validar(int codEmpresa, int codContabilidad, string codCuenta)
        {
            var sql = @"
                select isnull(count(*),0) as Total
                from CntX_Cuentas C
                inner join CntX_Tipos_Cuentas T
                  on C.tipo_cuenta = T.tipo_cuenta
                 and C.cod_contabilidad = T.cod_contabilidad
                where C.cod_contabilidad = @codContabilidad
                  and C.cod_cuenta = @codCuenta
                  and T.clasificacion = 'A'";
            return DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, default, new { codContabilidad, codCuenta });
        }
    }
}
