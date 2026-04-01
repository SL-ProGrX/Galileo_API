using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Comites;
using Galileo.Models; // Para BitacoraInsertarDto
using Galileo.Models.Security; // Para MSecurityMainDb
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdParametrosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDB;
        private readonly int vModulo = 40;

        public FrmAfCdParametrosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDB = new MSecurityMainDb(config);
        }

        public ErrorDto<List<AfCdParametroDto>> AfCdParametros_Lista(int codEmpresa)
        {
            // Ejecutar el SP (no retorna datos, solo asegura los registros)
            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute("spAFI_CD_Parametros", commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            // Luego retorna la lista actualizada
            var sql = @"
                SELECT
                    COD_PARAMETRO AS Cod_Parametro,
                    DETALLE AS Detalle,
                    TIPO AS Tipo,
                    VALOR AS Valor,
                    NOTAS AS Notas,
                    REGISTRO_FECHA AS Registro_Fecha,
                    REGISTRO_USUARIO AS Registro_Usuario
                FROM AFI_CD_PARAMETROS
                ORDER BY COD_PARAMETRO";

            return DbHelper.ExecuteListQuery<AfCdParametroDto>(
                _portalDb,
                codEmpresa,
                sql,
                null
            );
        }

        public ErrorDto<bool> AfCdParametros_Update(int codEmpresa, AfCdParametroUpdateDto dto)
        {
            var sql = @"
                UPDATE AFI_CD_PARAMETROS
                SET REGISTRO_USUARIO = @Usuario,
                    REGISTRO_FECHA = GETDATE(),
                    VALOR = @Valor
                WHERE COD_PARAMETRO = @Cod_Parametro";

            DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(sql, dto);
                return true;
            });

            // Bitácora
            var detalle = $"Parámetro de Comites y Delegados: {dto.Cod_Parametro} -> {dto.Valor}";
            _securityMainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = dto.Usuario.ToUpper(),
                DetalleMovimiento = detalle,
                Movimiento = "Modifica - WEB",
                Modulo = vModulo
            });

            return new ErrorDto<bool> { Result = true };
        }
    }
}
