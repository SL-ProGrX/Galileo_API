using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosDB
    {
        /// <summary>
        /// Actualiza un beneficio, reasigna su grupo y registra en bitácora los cambios relevantes.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Beneficio">Datos del beneficio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficios_Actualiza(int CodCliente, AfiBeneficiosDto Beneficio)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var anterior = ObtenerBeneficioAnterior(connection, Beneficio.cod_beneficio);

                connection.Execute(SqlActualizarBeneficio, MapBeneficioParams(Beneficio));
                ReasignarGrupo(connection, Beneficio);

                RegistrarCambiosBeneficio(CodCliente, Beneficio, anterior);

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un beneficio, asigna su grupo y registra en bitácora la creación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Beneficio">Datos del beneficio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficios_Insertar(int CodCliente, AfiBeneficiosDto Beneficio)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute(SqlInsertarBeneficio, MapBeneficioParams(Beneficio));
                ReasignarGrupo(connection, Beneficio);

                RegistrarBitacora(CodCliente, "Inserta-Web", $"Inserta [{Beneficio.vigencia_meses} meses] de vigencia del Beneficio", Beneficio.cod_beneficio, Beneficio.registra_user);
                RegistrarBitacora(CodCliente, "Inserta-Web", $"Inserta [{Beneficio.estado}] de Estado", Beneficio.cod_beneficio, Beneficio.registra_user);
                RegistrarBitacora(CodCliente, "Inserta-Web", $"Inserta categoria [{Beneficio.cod_categoria}]", Beneficio.cod_beneficio, Beneficio.registra_user);
                RegistrarBitacora(CodCliente, "Inserta-Web", $"Inserta grupo [{Beneficio.cod_grupo}]", Beneficio.cod_beneficio, Beneficio.registra_user);

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex) when (ex.Message.Contains("Cannot insert duplicate key"))
            {
                return DbHelper.ErrorResponse("El codigo de beneficio ya existe");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Cod_Beneficio">Código del beneficio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficios_Eliminar(int CodCliente, string Cod_Beneficio)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute("DELETE afi_beneficios WHERE cod_beneficio = @Cod_Beneficio", new { Cod_Beneficio });
                return new ErrorDto { Code = 0 };
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("No se puede eliminar el beneficio, ya que tiene registros asociados");
            }
        }

        /// <summary>
        /// Asocia un grupo a un beneficio (tabla AFI_BENE_GRUPOSB).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneGruposB_Insertar(int CodCliente, string cod_grupo, string cod_beneficio)
        {
            const string sql = "INSERT AFI_BENE_GRUPOSB (cod_grupo, cod_beneficio) VALUES (@cod_grupo, @cod_beneficio)";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_grupo, cod_beneficio });
        }

        /// <summary>
        /// Desasocia un grupo de un beneficio (tabla AFI_BENE_GRUPOSB).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneGruposB_Eliminar(int CodCliente, string cod_grupo, string cod_beneficio)
        {
            const string sql = "DELETE AFI_BENE_GRUPOSB WHERE cod_grupo = @cod_grupo AND cod_beneficio = @cod_beneficio";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_grupo, cod_beneficio });
        }

        /// <summary>
        /// Reasigna el grupo del beneficio (borra la asociación previa e inserta la nueva).
        /// </summary>
        private static void ReasignarGrupo(SqlConnection connection, AfiBeneficiosDto Beneficio)
        {
            connection.Execute("DELETE afi_Grupo_Beneficio WHERE cod_beneficio = @cod_beneficio", new { Beneficio.cod_beneficio });
            connection.Execute(
                "INSERT INTO afi_Grupo_Beneficio (cod_beneficio, cod_grupo) VALUES (@cod_beneficio, @cod_grupo)",
                new { Beneficio.cod_beneficio, Beneficio.cod_grupo });
        }

        /// <summary>
        /// Obtiene los valores previos del beneficio para comparación de bitácora.
        /// </summary>
        private static BeneficioAnteriorRow ObtenerBeneficioAnterior(SqlConnection connection, string cod_beneficio)
        {
            const string sql = @"SELECT vigencia_meses AS VIGENCIA_MESES, ESTADO, NOTAS, COD_CATEGORIA, COD_GRUPO
                                 FROM Afi_beneficios WHERE COD_BENEFICIO = @cod_beneficio";
            return connection.QueryFirstOrDefault<BeneficioAnteriorRow>(sql, new { cod_beneficio }) ?? new BeneficioAnteriorRow();
        }

        /// <summary>
        /// Registra en bitácora los cambios de vigencia, estado, notas, categoría y grupo.
        /// </summary>
        private void RegistrarCambiosBeneficio(int CodCliente, AfiBeneficiosDto b, BeneficioAnteriorRow ant)
        {
            if (b.vigencia_meses != ant.VIGENCIA_MESES)
            {
                RegistrarBitacora(CodCliente, "Actualiza-Web", $"Actualiza vigencia del Beneficio de [{ant.VIGENCIA_MESES} meses] por [{b.vigencia_meses} meses]", b.cod_beneficio, b.registra_user);
            }

            if (b.estado != ant.ESTADO)
            {
                var nuevo = b.estado == "A" ? "Activo" : "Inactivo";
                var previo = ant.ESTADO == "A" ? "Activo" : "Inactivo";
                RegistrarBitacora(CodCliente, "Actualiza-Web", $"Actualiza estado del Beneficio de [{previo}] por [{nuevo}]", b.cod_beneficio, b.registra_user);
            }

            if (b.notas != ant.NOTAS)
            {
                RegistrarBitacora(CodCliente, "Actualiza-Web", $"Actualiza las notas a: {b.notas}", b.cod_beneficio, b.registra_user);
            }

            if (b.cod_categoria != (ant.COD_CATEGORIA ?? string.Empty))
            {
                RegistrarBitacora(CodCliente, "Actualiza-Web", $"Actualiza categoria del Beneficio de [{ant.COD_CATEGORIA}] por [{b.cod_categoria}]", b.cod_beneficio, b.registra_user);
            }

            if (b.cod_grupo != (ant.COD_GRUPO ?? string.Empty))
            {
                RegistrarBitacora(CodCliente, "Actualiza-Web", $"Actualiza grupo del Beneficio de [{ant.COD_GRUPO}] por [{b.cod_grupo}]", b.cod_beneficio, b.registra_user);
            }
        }

        /// <summary>
        /// Arma los parámetros comunes de inserción/actualización del beneficio.
        /// </summary>
        private static object MapBeneficioParams(AfiBeneficiosDto b) => new
        {
            b.cod_beneficio,
            b.descripcion,
            b.notas,
            b.estado,
            b.registra_user,
            b.maximo_otorga,
            b.modifica_monto,
            b.modifica_diferencia,
            b.cod_cuenta,
            b.aplica_beneficiarios,
            b.aplica_parcial,
            b.tipo_monetario,
            b.tipo_producto,
            b.tipo,
            b.i_condicion_especial,
            b.i_morosidad,
            b.i_suspendidos,
            b.i_insolventes,
            b.i_cobro_judicial,
            b.cod_categoria,
            b.cod_grupo,
            b.vigencia_meses,
            b.pagos_multiples
        };

        private const string SqlActualizarBeneficio = @"
            UPDATE Afi_beneficios
            SET descripcion = @descripcion, notas = @notas, estado = @estado,
                aplica_beneficiarios = @aplica_beneficiarios, modifica_monto = @modifica_monto,
                cod_cuenta = @cod_cuenta, tipo = @tipo, modifica_diferencia = @modifica_diferencia,
                maximo_otorga = @maximo_otorga, aplica_parcial = @aplica_parcial,
                tipo_monetario = @tipo_monetario, tipo_producto = @tipo_producto,
                i_morosidad = @i_morosidad, i_condicion_especial = @i_condicion_especial,
                i_suspendidos = @i_suspendidos, i_insolventes = @i_insolventes, i_cobro_judicial = @i_cobro_judicial,
                Cod_Categoria = @cod_categoria, Cod_Grupo = @cod_grupo, VIGENCIA_MESES = @vigencia_meses, PAGOS_MULTIPLES = @pagos_multiples
            WHERE cod_beneficio = @cod_beneficio";

        private const string SqlInsertarBeneficio = @"
            INSERT INTO afi_beneficios
                (cod_beneficio, descripcion, notas, estado, registra_fecha, registra_user, maximo_otorga, modifica_monto,
                 modifica_diferencia, cod_cuenta, aplica_beneficiarios, aplica_parcial, tipo_monetario, tipo_producto, tipo,
                 i_condicion_especial, i_morosidad, i_suspendidos, i_insolventes, i_cobro_judicial,
                 Cod_Categoria, Cod_Grupo, VIGENCIA_MESES, PAGOS_MULTIPLES)
            VALUES
                (@cod_beneficio, @descripcion, @notas, @estado, GETDATE(), @registra_user, @maximo_otorga, @modifica_monto,
                 @modifica_diferencia, @cod_cuenta, @aplica_beneficiarios, @aplica_parcial, @tipo_monetario, @tipo_producto, @tipo,
                 @i_condicion_especial, @i_morosidad, @i_suspendidos, @i_insolventes, @i_cobro_judicial,
                 @cod_categoria, @cod_grupo, @vigencia_meses, @pagos_multiples)";

        /// <summary>
        /// Representa los valores previos del beneficio para comparación en bitácora.
        /// </summary>
        private sealed class BeneficioAnteriorRow
        {
            public int VIGENCIA_MESES { get; set; }
            public string ESTADO { get; set; } = string.Empty;
            public string? NOTAS { get; set; }
            public string? COD_CATEGORIA { get; set; }
            public string? COD_GRUPO { get; set; }
        }
    }
}
