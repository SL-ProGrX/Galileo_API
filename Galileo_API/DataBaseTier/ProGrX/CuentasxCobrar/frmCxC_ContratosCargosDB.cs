using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCContratosCargosDB
    {
        private readonly PortalDB _portalDb;

        public FrmCxCContratosCargosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta la lista de cargos tipo 'C', ordenados por el campo indicado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetro de ordenamiento.</param>
        /// <returns>Lista de cargos.</returns>
        public ErrorDto<List<CxcCargoDto>> CxcCargos_Lista(int codEmpresa, string orden)
        {
            string orderBy = orden?.ToLower() == "cod_cargo" ? "Cod_Cargo" : "Descripcion";
            var query = $@"
                SELECT Cod_Cargo, Descripcion
                FROM CxC_Cargos
                WHERE Tipo = 'C'
                ORDER BY {orderBy}";
            return DbHelper.ExecuteListQuery<CxcCargoDto>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Consulta los cargos de un contrato, incluyendo la descripción.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns>Lista de cargos del contrato.</returns>
        public ErrorDto<List<CxcContratoCargoDto>> CxcContratoCargos_Lista(int codEmpresa, string codContrato)
        {
            var query = @"
                SELECT 
                    C.descripcion AS Descripcion,
                    S.COD_CONTRATO,
                    S.COD_CARGO,
                    S.TIPO,
                    S.VALOR,
                    S.FRECUENCIA_TIPO,
                    S.FRECUENCIA_DIAS,
                    S.MODIFICA,
                    S.REGISTRO_FECHA,
                    S.REGISTRO_USUARIO
                FROM CxC_Cargos C
                INNER JOIN CxC_Contratos_Cargos S ON C.cod_cargo = S.cod_cargo
                WHERE S.cod_contrato = @codContrato";
            return DbHelper.ExecuteListQuery<CxcContratoCargoDto>(_portalDb, codEmpresa, query, new { codContrato });
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un cargo de contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros del cargo.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcContratoCargo_Guardar(int codEmpresa, CxcContratoCargoSaveParams param)
        {
            // Verifica existencia
            var existe = DbHelper.ExecuteSingleQuery<int>(
                _portalDb, codEmpresa,
                "SELECT COUNT(1) FROM CxC_Contratos_Cargos WHERE cod_contrato = @Cod_Contrato AND cod_cargo = @Cod_Cargo",
                default, new { param.Cod_Contrato, param.Cod_Cargo }
            ).Result;

            if (existe == 0)
            {
                // Insertar
                var sql = @"
                    INSERT INTO CxC_Contratos_Cargos
                    (Cod_Cargo, cod_contrato, Tipo, Valor, frecuencia_Tipo, frecuencia_dias, modifica, registro_fecha, registro_usuario)
                    VALUES
                    (@Cod_Cargo, @Cod_Contrato, @Tipo, @Valor, @Frecuencia_Tipo, @Frecuencia_Dias, @Modifica, dbo.MyGetdate(), @Registro_Usuario)";
                return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sql, param);
                    return rows > 0;
                });
            }
            else
            {
                // Actualizar
                var sql = @"
                    UPDATE CxC_Contratos_Cargos
                    SET
                        Valor = @Valor,
                        frecuencia_dias = @Frecuencia_Dias,
                        Tipo = @Tipo,
                        Frecuencia_Tipo = @Frecuencia_Tipo,
                        modifica = @Modifica
                    WHERE Cod_Cargo = @Cod_Cargo AND cod_contrato = @Cod_Contrato";
                return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var rows = conn.Execute(sql, param);
                    return rows > 0;
                });
            }
        }

        /// <summary>
        /// Elimina un cargo de contrato.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de eliminación.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        public ErrorDto<bool> CxcContratoCargo_Eliminar(int codEmpresa, CxcContratoCargoDeleteParams param)
        {
            var sql = @"
                DELETE FROM CxC_Contratos_Cargos
                WHERE Cod_Cargo = @Cod_Cargo AND cod_contrato = @Cod_Contrato";
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var rows = conn.Execute(sql, param);
                return rows > 0;
            });
        }
    }
}
