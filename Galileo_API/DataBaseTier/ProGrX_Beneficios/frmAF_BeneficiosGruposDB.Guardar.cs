using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosGruposDB
    {
        /// <summary>
        /// Guarda un grupo de beneficios: inserta si es nuevo (cod_grupo = 0) o actualiza si ya existe.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="grupo">Datos del grupo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneGrupo_Guardar(int CodCliente, AfiBeneGrupos grupo)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return grupo.cod_grupo == 0
                    ? AfiBeneGrupos_Insertar(connection, CodCliente, grupo)
                    : AfiBeneGrupos_Actualizar(connection, CodCliente, grupo);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un grupo de beneficios calculando su consecutivo y deja traza en bitácora.
        /// </summary>
        private ErrorDto AfiBeneGrupos_Insertar(SqlConnection connection, int CodCliente, AfiBeneGrupos grupo)
        {
            const string sqlConsec = "SELECT ISNULL(MAX(cod_grupo), 0) + 1 AS consec FROM afi_bene_grupos";
            var consecutivo = connection.QueryFirstOrDefault<int>(sqlConsec);

            const string sql = @"INSERT INTO AFI_BENE_GRUPOS (cod_grupo, descripcion, cod_categoria, monto, estado, fecha, user_registra)
                                 VALUES (@consecutivo, @descripcion, @cod_categoria, @monto, @estado, GETDATE(), @user_registra)";

            connection.Execute(sql, new
            {
                consecutivo,
                grupo.descripcion,
                grupo.cod_categoria,
                grupo.monto,
                estado = grupo.estado ? 1 : 0,
                grupo.user_registra
            });

            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = consecutivo.ToString(),
                consec = -2,
                movimiento = "Inserta frmAF_BeneficiosGrupos-Web",
                detalle = $"Inserta el monto del Beneficio, Codigo grupo: {consecutivo}, {grupo.descripcion} por [{grupo.monto}]",
                registro_usuario = grupo.user_registra
            });

            return DbHelper.OkResponse("Registro Insertado");
        }

        /// <summary>
        /// Actualiza un grupo de beneficios y deja traza en bitácora si cambió el monto.
        /// </summary>
        private ErrorDto AfiBeneGrupos_Actualizar(SqlConnection connection, int CodCliente, AfiBeneGrupos grupo)
        {
            const string sqlMonto = @"SELECT MONTO FROM AFI_BENE_GRUPOS
                                      WHERE COD_CATEGORIA = @cod_categoria AND COD_GRUPO = @cod_grupo";
            var montoAnterior = connection.QueryFirstOrDefault<int>(sqlMonto, new { grupo.cod_categoria, grupo.cod_grupo });

            const string sql = @"UPDATE AFI_BENE_GRUPOS
                                 SET descripcion = @descripcion, cod_categoria = @cod_categoria, monto = @monto, estado = @estado
                                 WHERE cod_grupo = @cod_grupo";

            connection.Execute(sql, new
            {
                grupo.descripcion,
                grupo.cod_categoria,
                grupo.monto,
                estado = grupo.estado ? 1 : 0,
                grupo.cod_grupo
            });

            const decimal montoEpsilon = 0.0001m;
            if (Math.Abs(Convert.ToDecimal(grupo.monto) - Convert.ToDecimal(montoAnterior)) > montoEpsilon)
            {
                _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                {
                    EmpresaId = CodCliente,
                    cod_beneficio = grupo.cod_grupo.ToString(),
                    consec = -2,
                    movimiento = "Actualiza",
                    detalle = $"Actualiza el monto del Beneficio, Codigo grupo: {grupo.cod_grupo}, {grupo.descripcion} de [{montoAnterior}] por [{grupo.monto}]",
                    registro_usuario = grupo.user_registra
                });
            }

            return DbHelper.OkResponse("Registro Actualizado");
        }

        /// <summary>
        /// Elimina un grupo de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_grupo">Código del grupo a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneGrupos_Eliminar(int CodCliente, int cod_grupo)
        {
            const string sql = "DELETE FROM AFI_BENE_GRUPOS WHERE cod_grupo = @cod_grupo";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_grupo });

            if (result.Code == 0)
            {
                result.Description = "Registro Eliminado";
            }

            return result;
        }

        /// <summary>
        /// Asocia un beneficio a un grupo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="grupo">Datos de la asociación beneficio-grupo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiGrupoBeneficio_Insertar(int CodCliente, AfiGrupoBeneficioData grupo)
        {
            const string sql = "INSERT AFI_GRUPO_BENEFICIO (cod_beneficio, cod_grupo) VALUES (@cod_beneficio, @cod_grupo)";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { grupo.cod_beneficio, grupo.cod_grupo });

            if (result.Code == 0)
            {
                result.Description = "Registro Insertado";
            }

            return result;
        }

        /// <summary>
        /// Desasocia un beneficio de un grupo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="grupo">Datos de la asociación beneficio-grupo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiGrupoBeneficio_Eliminar(int CodCliente, AfiGrupoBeneficioData grupo)
        {
            const string sql = "DELETE FROM AFI_GRUPO_BENEFICIO WHERE cod_beneficio = @cod_beneficio AND cod_grupo = @cod_grupo";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { grupo.cod_beneficio, grupo.cod_grupo });

            if (result.Code == 0)
            {
                result.Description = "Registro Eliminado";
            }

            return result;
        }
    }
}
