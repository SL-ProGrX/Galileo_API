using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRazonesFinanzasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 20;
        private const string registra = "Registra - WEB";
        private const string modifica = "Modifica - WEB";

        public FrmCntXRazonesFinanzasDB(IConfiguration config)
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
        /// Obtiene la lista de tipos de razones financieras.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>Lista de tipos de razones.</returns>
        public ErrorDto<List<CntXRazonesFinanzasDto>> CntXRazonesFinanzas_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select cod_grupo as CodGrupo, descripcion, activa
                from CntX_razones_tipos
                where cod_contabilidad = @codContabilidad
                order by cod_grupo";
            return DbHelper.ExecuteListQuery<CntXRazonesFinanzasDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Valida si existen tipos de razones para la contabilidad dada.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de contabilidad.</param>
        /// <returns>True si existen registros.</returns>
        public ErrorDto<bool> CntXRazonesFinanzas_Existe(int codEmpresa, int codContabilidad)
        {
            var sql = @"select isnull(count(*),0) as Existe from CntX_razones_tipos where cod_contabilidad = @codContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, default, new { codContabilidad }).Result;
            return DbHelper.CreateOkResponse(existe > 0);
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un tipo de razón financiera.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del registro.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CntXRazonesFinanzas_Guardar(int codEmpresa, CntXRazonesFinanzasSaveParams param)
        {
            var sqlExist = @"SELECT COUNT(1) FROM CntX_razones_tipos WHERE cod_grupo = @CodGrupo AND cod_contabilidad = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExist, default, new { param.CodGrupo, param.CodContabilidad }).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
                    insert into CntX_razones_tipos(cod_grupo, cod_contabilidad, descripcion, activa)
                    values(@CodGrupo, @CodContabilidad, @Descripcion, @Activa)";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Fin. Tipo / Grupo : {param.CodGrupo} - {param.Descripcion}", registra);

                return result;
            }
            else
            {
                var sqlUpdate = @"
                    update CntX_razones_tipos
                    set descripcion = @Descripcion,
                        activa = @Activa
                    where cod_contabilidad = @CodContabilidad
                      and cod_grupo = @CodGrupo";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Fin. Tipo / Grupo : {param.CodGrupo} - {param.Descripcion}", modifica);

                return result;
            }
        }

        /// <summary>
        /// Obtiene la lista de razones financieras.
        /// </summary>
        public ErrorDto<List<CntXRazonFinancieraDto>> CntXRazonFinanciera_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select R.cod_razon as CodRazon,
                       R.descripcion,
                       R.resultado,
                       (T.cod_grupo + ' - ' + T.descripcion) as Grupo
                from CntX_Razones_Tipos T
                inner join CntX_Razones R
                    on T.cod_contabilidad = R.cod_contabilidad
                    and T.cod_grupo = R.cod_grupo
                where R.cod_contabilidad = @codContabilidad
                order by T.cod_grupo, R.cod_razon";
            return DbHelper.ExecuteListQuery<CntXRazonFinancieraDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene la lista de tipos de razones financieras (solo descripción).
        /// </summary>
        public ErrorDto<List<CntXRazonFinancieraTipoDto>> CntXRazonFinancieraTipos_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select cod_grupo as CodGrupo,
                       (cod_grupo + ' - ' + descripcion) as Descripcion
                from CntX_Razones_Tipos
                where cod_contabilidad = @codContabilidad
                order by cod_grupo";
            return DbHelper.ExecuteListQuery<CntXRazonFinancieraTipoDto>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una razón financiera.
        /// </summary>
        public ErrorDto<bool> CntXRazonFinanciera_Guardar(int codEmpresa, CntXRazonFinancieraSaveParams param)
        {
            var sqlExist = @"SELECT COUNT(1) FROM CntX_Razones WHERE cod_razon = @CodRazon AND cod_contabilidad = @CodContabilidad";
            var existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlExist, default, new { param.CodRazon, param.CodContabilidad }).Result;

            if (existe == 0)
            {
                var sqlInsert = @"
                    insert into CntX_Razones(cod_razon, descripcion, cod_contabilidad, resultado, cod_grupo, notas)
                    values(@CodRazon, @Descripcion, @CodContabilidad, @Resultado, @CodGrupo, '')";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlInsert, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Financiera Id: {param.CodRazon} - {param.Descripcion}", registra);

                return result;
            }
            else
            {
                var sqlUpdate = @"
                    update CntX_Razones
                    set descripcion = @Descripcion,
                        resultado = @Resultado,
                        cod_grupo = @CodGrupo
                    where cod_contabilidad = @CodContabilidad
                      and cod_razon = @CodRazon";
                var result = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sqlUpdate, param);
                    return rows > 0;
                });

                if (result.Code == 0)
                    RegistrarBitacora(codEmpresa, param.RegistroUsuario, $"Razon Financiera Id: {param.CodRazon} - {param.Descripcion}", modifica);

                return result;
            }            
        }

        /// <summary>
        /// Obtiene lista genérica de grupos para combos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXRazonFinancieraGrupos_Combo(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select rtrim(cod_grupo) as item, rtrim(descripcion) as descripcion
                from CntX_Razones_Tipos
                where cod_contabilidad = @codContabilidad
                order by cod_grupo";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene lista de razones financieras filtradas por grupo y ordenadas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXRazonFinancieraSimple_Lista(int codEmpresa, int codContabilidad, string codGrupo, string orden)
        {
            var sql = @"
                select cod_razon as item, descripcion
                from CntX_Razones
                where cod_contabilidad = @codContabilidad
                  and cod_grupo = @codGrupo
                order by " + (orden == "descripcion" ? "descripcion" : "cod_razon");
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad, codGrupo });
        }

        /// <summary>
        /// Obtiene las notas y la fórmula de una razón financiera.
        /// </summary>
        public ErrorDto<CntXRazonNotasDto?> CntXRazonFinanciera_Notas(int codEmpresa, int codContabilidad, string codGrupo, string codRazon)
        {
            var sql = @"
                select notas, formula
                from CntX_Razones
                where cod_grupo = @codGrupo
                  and cod_razon = @codRazon
                  and cod_contabilidad = @codContabilidad";
            return DbHelper.ExecuteSingleQuery<CntXRazonNotasDto>(_portalDb, codEmpresa, sql, default, new { codGrupo, codRazon, codContabilidad });
        }

        /// <summary>
        /// Obtiene el detalle de cuentas de una razón financiera.
        /// </summary>
        public ErrorDto<List<CntXRazonDetalleDto>> CntXRazonFinanciera_Detalle(int codEmpresa, int codContabilidad, string codRazon)
        {
            var sql = @"
                select R.*, C.descripcion, C.cod_cuenta_mask
                from CntX_Razones_detalle R
                inner join CntX_Cuentas C
                  on R.cod_contabilidad = C.cod_contabilidad
                 and R.cod_cuenta = C.cod_cuenta
                where R.cod_razon = @codRazon
                  and R.cod_contabilidad = @codContabilidad
                order by R.idx";
            return DbHelper.ExecuteListQuery<CntXRazonDetalleDto>(_portalDb, codEmpresa, sql, new { codRazon, codContabilidad });
        }

        /// <summary>
        /// Obtiene el próximo Idx para detalle de razón financiera.
        /// </summary>
        public ErrorDto<CntXRazonDetalleIdxDto?> CntXRazonDetalle_ProximoIdx(int codEmpresa, int codContabilidad, string codRazon)
        {
            var sql = @"
                select (isnull(max(idx),0) + 1) as Idx
                from CntX_Razones_detalle
                where cod_contabilidad = @codContabilidad
                  and cod_razon = @codRazon";
            return DbHelper.ExecuteSingleQuery<CntXRazonDetalleIdxDto>(_portalDb, codEmpresa, sql, default, new { codContabilidad, codRazon });
        }

        /// <summary>
        /// Valida si existe un detalle con operador 'B', con opción de excluir un idx.
        /// </summary>
        public ErrorDto<int?> CntXRazonDetalle_ValidaB(int codEmpresa, int codContabilidad, string codRazon, int? excludeIdx = null)
        {
            var sql = @"
                select idx
                from CntX_Razones_detalle
                where cod_contabilidad = @codContabilidad
                  and cod_razon = @codRazon
                  and operador = 'B'";
            if (excludeIdx.HasValue)
                sql += " and idx <> @excludeIdx";
            return DbHelper.ExecuteSingleQuery<int?>(_portalDb, codEmpresa, sql, default, new { codContabilidad, codRazon, excludeIdx });
        }

        /// <summary>
        /// Inserta un detalle de razón financiera.
        /// </summary>
        public ErrorDto<bool> CntXRazonDetalle_Insertar(int codEmpresa, CntXRazonDetalleDto param)
        {
            var sql = @"
                insert into CntX_Razones_detalle(idx, cod_contabilidad, cod_razon, cod_cuenta, operador)
                values(@Idx, @CodContabilidad, @CodRazon, @CodCuenta, @Operador)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Actualiza un detalle de razón financiera.
        /// </summary>
        public ErrorDto<bool> CntXRazonDetalle_Actualizar(int codEmpresa, CntXRazonDetalleDto param)
        {
            var sql = @"
                update CntX_Razones_detalle
                set cod_cuenta = @CodCuenta,
                    operador = @Operador
                where cod_contabilidad = @CodContabilidad
                  and cod_razon = @CodRazon
                  and idx = @Idx";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Elimina un detalle de razón financiera (excepto operador 'B').
        /// </summary>
        public ErrorDto<bool> CntXRazonDetalle_Eliminar(int codEmpresa, int codContabilidad, string codRazon, int idx)
        {
            var sql = @"
                delete from CntX_Razones_detalle
                where idx = @idx
                  and cod_contabilidad = @codContabilidad
                  and cod_razon = @codRazon
                  and operador <> 'B'";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, new { idx, codContabilidad, codRazon });
                return rows > 0;
            });
        }

        /// <summary>
        /// Actualiza las notas y la fórmula de una razón financiera.
        /// </summary>
        public ErrorDto<bool> CntXRazonFinanciera_ActualizarNotas(int codEmpresa, CntXRazonNotasUpdateParams param)
        {
            var sql = @"
                update CntX_Razones
                set notas = @Notas,
                    formula = @Formula
                where cod_contabilidad = @CodContabilidad
                  and cod_grupo = @CodGrupo
                  and cod_razon = @CodRazon";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Obtiene la lista de unidades para combos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXUnidades_Combo(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                select rtrim(cod_unidad) as item, rtrim(descripcion) as descripcion
                from CntX_Unidades
                where cod_contabilidad = @codContabilidad";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad });
        }

        /// <summary>
        /// Obtiene la lista de razones con operador 'B', con filtro opcional por grupo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CntXRazonesConOperadorB_Lista(int codEmpresa, int codContabilidad, string? codGrupo = null)
        {
            var sql = @"
                select R.cod_razon as item, R.descripcion
                from CntX_Razones R
                inner join CntX_Razones_detalle D
                  on R.cod_razon = D.cod_razon
                 and R.cod_contabilidad = D.cod_contabilidad
                where R.cod_contabilidad = @codContabilidad
                  and D.operador = 'B'";
            if (!string.IsNullOrWhiteSpace(codGrupo) && codGrupo != "TODOS")
                sql += " and R.cod_grupo = @codGrupo";
            sql += " group by R.cod_razon, R.descripcion";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql, new { codContabilidad, codGrupo });
        }

        /// <summary>
        /// Elimina registros de CntX_Razones_Reporte por usuario y contabilidad.
        /// </summary>
        public ErrorDto<bool> CntXRazonesReporte_Eliminar(int codEmpresa, string usuario, int codContabilidad)
        {
            var sql = @"
                delete from CntX_Razones_Reporte
                where usuario = @usuario
                  and cod_contabilidad = @codContabilidad";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, new { usuario, codContabilidad });
                return rows > 0;
            });
        }

        /// <summary>
        /// Inserta un registro en CntX_Razones_Reporte.
        /// </summary>
        public ErrorDto<bool> CntXRazonesReporte_Insertar(int codEmpresa, CntXRazonesReporteInsertParams param)
        {
            var sql = @"
                insert into CntX_Razones_Reporte (usuario, cod_contabilidad, cod_razon, monto)
                values (@Usuario, @CodContabilidad, @CodRazon, @Monto)";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        public ErrorDto<bool> CntXRazonesReporte_ActualizarMes(int codEmpresa, CntXRazonesReporteUpdateParams param)
        {
            string sql = "";

            // Validación estricta para evitar inyección SQL
            switch (param.Mes)
            {
                case "Mes01":
                    sql = @"
                update CntX_Razones_Reporte
                set Mes01 = @Monto
                where usuario = @Usuario
                  and cod_contabilidad = @CodContabilidad
                  and cod_razon = @CodRazon";
                    break;
                case "Mes02":
                    sql = @"
                update CntX_Razones_Reporte
                set Mes02 = @Monto
                where usuario = @Usuario
                  and cod_contabilidad = @CodContabilidad
                  and cod_razon = @CodRazon";
                    break;
                default:
                    return DbHelper.CreateErrorResponse<bool>("Mes inválido");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }

        /// <summary>
        /// Obtiene la fórmula de una razón financiera.
        /// </summary>
        public ErrorDto<CntXRazonFormulaDto?> CntXRazonFinanciera_Formula(int codEmpresa, int codContabilidad, string codRazon)
        {
            var sql = @"
                select formula
                from CntX_Razones
                where cod_contabilidad = @codContabilidad
                  and cod_razon = @codRazon";
            return DbHelper.ExecuteSingleQuery<CntXRazonFormulaDto>(_portalDb, codEmpresa, sql, default, new { codContabilidad, codRazon });
        }

        /// <summary>
        /// Obtiene el monto de una razón financiera, filtrando por unidad si corresponde.
        /// </summary>
        public ErrorDto<CntXRazonMontoDto?> CntXRazonFinanciera_Monto(int codEmpresa, CntXRazonMontoParams param)
        {
            string sql;
            if (param.Unidad == "TODOS")
            {
                sql = @"
                    select (M.saldo_inicial + M.total_debitos + M.total_creditos) as Monto
                    from CntX_Razones_detalle R
                    inner join vCntX_Mov_Cuentas_General M
                        on R.cod_contabilidad = M.cod_contabilidad
                       and R.cod_cuenta       = M.cod_cuenta
                    where R.cod_razon = @CodRazon
                      and M.Anio = @Anio
                      and M.mes  = @Mes
                      and R.idX  = @Idx
                      and R.cod_contabilidad = @CodContabilidad";
            }
            else
            {
                sql = @"
                    select (M.saldo_inicial + M.total_debitos + M.total_creditos) as Monto
                    from CntX_Razones_detalle R
                    inner join vCntX_Mov_Cuentas_Unidad M
                        on R.cod_contabilidad = M.cod_contabilidad
                       and R.cod_cuenta       = M.cod_cuenta
                       and M.cod_unidad       = @Unidad
                    where R.cod_razon = @CodRazon
                      and M.Anio = @Anio
                      and M.mes  = @Mes
                      and R.idX  = @Idx
                      and R.cod_contabilidad = @CodContabilidad";
            }
            return DbHelper.ExecuteSingleQuery<CntXRazonMontoDto>(_portalDb, codEmpresa, sql, default, param);
        }

    }
}
