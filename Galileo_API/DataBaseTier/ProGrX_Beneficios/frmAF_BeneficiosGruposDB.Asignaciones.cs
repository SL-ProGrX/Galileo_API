using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosGruposDB
    {
        // SP de consulta por tipo de asignación (0=Estados, 1=Requisitos, 2=Motivos, 3=Accesos).
        private static readonly string[] SpAsignaList =
        {
            "spAFI_Bene_Grupos_Estados_List",
            "spAFI_Bene_Grupos_Requisitos_List",
            "spAFI_Bene_Grupos_Motivos_List",
            "spAFI_Bene_Grupos_Accesos_List"
        };

        // SP de alta por tipo de asignación y nombre del campo esperado por cada SP.
        private static readonly (string sp, string campo)[] SpAsignaAdd =
        {
            ("spAFI_Bene_Grupos_Estados_Add", "Estado"),
            ("spAFI_Bene_Grupos_Requisitos_Add", "Requisito"),
            ("spAFI_Bene_Grupos_Motivos_Add", "Motivo"),
            ("spAFI_Bene_Grupos_Accesos_Add", "Rol")
        };

        /// <summary>
        /// Obtiene las asignaciones de un grupo según el tipo (estados, requisitos, motivos o accesos).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="asigna">Tipo de asignación (0-3).</param>
        /// <param name="grupo">Código del grupo.</param>
        /// <returns>Lista de asignaciones.</returns>
        public ErrorDto<List<AfiBeneAsignacionesData>> AfiAsignaciones_Obtener(int CodCliente, int asigna, string grupo)
        {
            if (asigna < 0 || asigna >= SpAsignaList.Length)
            {
                return DbHelper.CreateErrorResponse<List<AfiBeneAsignacionesData>>("Tipo de asignación inválido");
            }

            var sp = SpAsignaList[asigna];

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneAsignacionesData>(sp, new { GrupoId = grupo },
                    commandType: CommandType.StoredProcedure).ToList());
        }

        /// <summary>
        /// Registra una asignación de grupo según el tipo (estados, requisitos, motivos o accesos).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="asigna">Tipo de asignación (0-3).</param>
        /// <param name="grupo">Código del grupo.</param>
        /// <param name="valor">Valor a asignar.</param>
        /// <param name="usuario">Usuario que registra.</param>
        /// <param name="mov">Tipo de movimiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiAsignaciones_Actualizar(int CodCliente, int asigna, string grupo, string valor, string usuario, string mov)
        {
            if (asigna < 0 || asigna >= SpAsignaAdd.Length)
            {
                return DbHelper.ErrorResponse("Tipo de asignación inválido");
            }

            var (sp, campo) = SpAsignaAdd[asigna];

            var parametros = new Dictionary<string, object>
            {
                { "GrupoId", grupo },
                { campo, valor },
                { "Usuario", usuario },
                { "Mov", mov }
            };

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute(sp, parametros, commandType: CommandType.StoredProcedure);
                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
