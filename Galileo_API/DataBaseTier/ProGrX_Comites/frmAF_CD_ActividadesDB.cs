using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdActividadesDB
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdActividadesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<AfCdActividadDto>> AfCdActividades_Lista(int codEmpresa, int codContabilidad)
        {
            var sql = @"
                SELECT
                    Act.COD_ACTIVIDAD AS Cod_Actividad,
                    Act.DESCRIPCION AS Descripcion,
                    Act.COD_CUENTA AS Cod_Cuenta,
                    Act.FECHAPERIOCIDAD AS FechaPeriocidad,
                    Act.FECHALIQ AS FechaLiq,
                    Act.ACTIVA AS Activa,
                    Cta.DESCRIPCION AS CuentaX,
                    Act.TIPO AS Tipo
                FROM AFI_CD_ACTIVIDADES Act
                LEFT JOIN CNTX_CUENTAS Cta
                    ON Act.cod_cuenta = Cta.Cod_cuenta
                   AND Cta.cod_contabilidad = @CodContabilidad
                ORDER BY Act.COD_ACTIVIDAD ASC";

            var parameters = new { CodContabilidad = codContabilidad };

            return DbHelper.ExecuteListQuery<AfCdActividadDto>(
                _portalDb,
                codEmpresa,
                sql,
                parameters
            );
        }

        public ErrorDto<bool> AfCdActividades_Upsert(int codEmpresa, AfCdActividadDto dto)
        {
            // Verificar existencia
            var existe = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<int>(
                    "SELECT COUNT(1) FROM afi_cd_actividades WHERE cod_actividad = @Cod_Actividad",
                    new { dto.Cod_Actividad }
                )
            );

            if (existe.Result == 0)
            {
                // Obtener nuevo código
                var nuevoCodigo = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                    conn.QueryFirstOrDefault<int>(
                        "SELECT ISNULL(MAX(CAST(cod_actividad AS INT)), 0) + 1 FROM AFI_CD_ACTIVIDADES"
                    )
                );
                dto.Cod_Actividad = nuevoCodigo.Result;

                var sql = @"
                    INSERT INTO afi_cd_actividades
                    (
                        cod_actividad,
                        descripcion,
                        cod_cuenta,
                        tipo,
                        fechaperiocidad,
                        fechaliq,
                        activa
                    )
                    VALUES
                    (
                        @Cod_Actividad,
                        @Descripcion,
                        @Cod_Cuenta,
                        @Tipo,
                        @FechaPeriocidad,
                        @FechaLiq,
                        @Activa
                    )";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }
            else
            {
                var sql = @"
                    UPDATE afi_cd_actividades
                    SET
                        descripcion = @Descripcion,
                        cod_cuenta = @Cod_Cuenta,
                        tipo = @Tipo,
                        fechaperiocidad = @FechaPeriocidad,
                        fechaliq = @FechaLiq,
                        activa = @Activa
                    WHERE cod_actividad = @Cod_Actividad";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }

            return new ErrorDto<bool> { Result = true };
        }

        public ErrorDto<List<AfCdActividadComiteDto>> AfCdActividades_ComitesPorActividad(int codEmpresa, int codActividad)
        {
            var sql = @"
                select A.cod_actividad AS Cod_Actividad, C.cod_comite AS Cod_Comite, A.descripcion AS Descripcion
                from afi_cd_actividades A
                left join afi_cd_comites_actividades C
                on A.cod_actividad = C.cod_actividad
                where A.cod_actividad = @Cod_Actividad
                  and C.cod_comite is not null";

            return DbHelper.ExecuteListQuery<AfCdActividadComiteDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cod_Actividad = codActividad }
            );
        }

        public ErrorDto<bool> AfCdActividades_EliminarComitesPorActividad(int codEmpresa, int codActividad)
        {
            var sql = @"DELETE FROM afi_cd_comites_actividades WHERE cod_actividad = @Cod_Actividad";
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Actividad = codActividad });
                return true;
            });
            return new ErrorDto<bool> { Result = true };
        }

        public ErrorDto<List<AfCdActividadSimpleDto>> AfCdActividades_SimpleLista(int codEmpresa)
        {
            var sql = @"
                SELECT cod_actividad AS Cod_Actividad, descripcion AS Descripcion
                FROM afi_cd_actividades
                ORDER BY cod_actividad ASC";

            return DbHelper.ExecuteListQuery<AfCdActividadSimpleDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        public ErrorDto<List<AfCdActividadRangoDto>> AfCdActividades_RangosPorActividad(int codEmpresa, int codActividad)
        {
            var sql = @"
                SELECT cod_monto AS Cod_Monto, monto AS Monto, minimo AS Minimo, maximo AS Maximo
                FROM afi_cd_actividades_rangos
                WHERE cod_actividad = @Cod_Actividad";

            return DbHelper.ExecuteListQuery<AfCdActividadRangoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Cod_Actividad = codActividad }
            );
        }

        public ErrorDto<bool> AfCdActividades_RangoUpsert(int codEmpresa, int codActividad, AfCdActividadRangoDto dto)
        {
            // Verificar existencia
            var existe = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<int>(
                    "SELECT COUNT(1) FROM afi_cd_actividades_rangos WHERE cod_monto = @Cod_Monto",
                    new { dto.Cod_Monto }
                )
            );

            if (existe.Result == 0)
            {
                // Calcular nuevo cod_monto
                var nuevoCodigo = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                    conn.QueryFirstOrDefault<int>(
                        "SELECT COALESCE(MAX(cod_monto),0) + 1 FROM afi_cd_actividades_rangos"
                    )
                );
                dto.Cod_Monto = nuevoCodigo.Result;

                var sql = @"
                    INSERT INTO afi_cd_actividades_rangos
                    (cod_actividad, monto, minimo, maximo, cod_monto)
                    VALUES
                    (@Cod_Actividad, @Monto, @Minimo, @Maximo, @Cod_Monto)";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, new
                    {
                        Cod_Actividad = codActividad,
                        dto.Monto,
                        dto.Minimo,
                        dto.Maximo,
                        dto.Cod_Monto
                    });
                    return true;
                });
            }
            else
            {
                var sql = @"
                    UPDATE afi_cd_actividades_rangos
                    SET
                        monto = @Monto,
                        minimo = @Minimo,
                        maximo = @Maximo
                    WHERE cod_monto = @Cod_Monto";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }

            return new ErrorDto<bool> { Result = true };
        }

        public ErrorDto<bool> AfCdActividades_RangoDelete(int codEmpresa, int codActividad, int codMonto)
        {
            var sql = @"DELETE FROM afi_cd_actividades_rangos WHERE cod_actividad = @Cod_Actividad AND cod_monto = @Cod_Monto";
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Actividad = codActividad, Cod_Monto = codMonto });
                return true;
            });
            return new ErrorDto<bool> { Result = true };
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AfCdActividades_DropDownLista(int codEmpresa)
        {
            var sql = @"
                SELECT cod_actividad AS item, descripcion AS descripcion
                FROM afi_cd_actividades
                ORDER BY cod_actividad ASC";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        public ErrorDto<List<AfCdCuentaConsultaDto>> AfCdCuentas_Consulta(int codEmpresa, DateTime fechaInicio, DateTime fechaFin, string codActividad)
        {
            var sql = @"
                select
                    Act.descripcion AS Descripcion,
                    S.descripcion AS Comite,
                    C.monto AS Monto,
                    C.noperacion AS NOperacion,
                    C.registro_usuario AS Registro_Usuario,
                    C.registro_fecha AS Registro_Fecha,
                    C.estado AS Estado,
                    Est.NombreEstado AS NombreEstado
                from afi_cd_cuentas C
                inner join afi_cd_cuentas_actividades Ca
                    on C.noperacion = Ca.noperacion
                inner join afi_cd_actividades Act
                    on Act.cod_actividad = Ca.cod_actividad
                inner join afi_cd_comites S
                    on C.Cod_comite = S.cod_comite
                inner join AFI_CD_TIPOS_ESTADOS_CUENTAS Est
                    on C.Estado = Est.CodEstado
                where C.registro_fecha between @FechaInicio and @FechaFin";

            if (!string.Equals(codActividad, "TODOS", StringComparison.OrdinalIgnoreCase))
                sql += " and Ca.Cod_Actividad = @Cod_Actividad";

            var parameters = new
            {
                FechaInicio = fechaInicio.Date,
                FechaFin = fechaFin.Date.AddDays(1).AddSeconds(-1),
                Cod_Actividad = codActividad
            };

            return DbHelper.ExecuteListQuery<AfCdCuentaConsultaDto>(
                _portalDb,
                codEmpresa,
                sql,
                parameters
            );
        }
    }
}
