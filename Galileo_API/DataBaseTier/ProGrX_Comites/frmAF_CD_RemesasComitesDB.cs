using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdRemesasComitesDB
    {
        private readonly PortalDB _portalDb;

        public FrmAfCdRemesasComitesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las últimas 50 remesas de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de remesas.</returns>
        public ErrorDto<List<AfCdRemesaTesDto>> AfCdRemesasTes_Lista(int codEmpresa)
        {
            var sql = @"
                SELECT TOP 50
                    COD_REMESA AS Cod_Remesa,
                    FECHA AS Fecha,
                    USUARIO AS Usuario,
                    FECHA_INICIO AS Fecha_Inicio,
                    FECHA_CORTE AS Fecha_Corte,
                    NOTAS AS Notas,
                    ESTADO AS Estado
                FROM afi_cd_remesas_tes
                ORDER BY FECHA DESC";

            return DbHelper.ExecuteListQuery<AfCdRemesaTesDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        /// <summary>
        /// Inserta o actualiza una remesa de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="dto">Datos de la remesa.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_Guardar(int codEmpresa, AfCdRemesaTesSaveDto dto)
        {
            if (dto.Cod_Remesa == 0)
            {
                // Insertar: calcular nuevo código
                var nuevoCodigo = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                    conn.QueryFirstOrDefault<int>(
                        "SELECT COALESCE(MAX(cod_remesa),0) + 1 FROM afi_cd_remesas_tes"
                    )
                );
                dto.Cod_Remesa = nuevoCodigo.Result;

                var sql = @"
                    INSERT INTO afi_cd_remesas_tes
                        (cod_remesa, usuario, fecha, estado, fecha_inicio, fecha_corte, notas)
                    VALUES
                        (@Cod_Remesa, @Usuario, GETDATE(), 'A', @Fecha_Inicio, @Fecha_Corte, @Notas)";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }
            else
            {
                var sql = @"
                    UPDATE afi_cd_remesas_tes
                    SET usuario = @Usuario,
                        fecha_inicio = @Fecha_Inicio,
                        fecha_corte = @Fecha_Corte,
                        notas = @Notas
                    WHERE cod_remesa = @Cod_Remesa";
                DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    conn.Execute(sql, dto);
                    return true;
                });
            }

            return new ErrorDto<bool> { Result = true };
        }

        /// <summary>
        /// Elimina una remesa de tesorería.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codRemesa">Código de la remesa a eliminar.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> AfCdRemesasTes_Eliminar(int codEmpresa, int codRemesa)
        {
            var sql = @"DELETE FROM afi_cd_remesas_tes WHERE cod_remesa = @Cod_Remesa";
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, new { Cod_Remesa = codRemesa });
                return true;
            });
            return new ErrorDto<bool> { Result = true };
        }
    }
}
