using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public partial class FrmCOPrincipalDB
    {
        /// <summary>
        /// Obtiene las identificaciones de los fiadores activos de una operación
        /// para generar sus estados de cuenta adjuntos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <returns>Identificaciones de los fiadores activos.</returns>
        public ErrorDto<List<string>> EstadosAdjuntos_Fiadores_Listar(
            int codEmpresa,
            int operacion)
        {
            const string sql = @"
                SELECT DISTINCT RTRIM(cedulaf)
                FROM fiadores
                WHERE estado = 'A'
                  AND id_solicitud = @Operacion
                  AND NULLIF(LTRIM(RTRIM(cedulaf)), '') IS NOT NULL
                ORDER BY RTRIM(cedulaf);";

            return DbHelper.ExecuteListQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                new { Operacion = operacion });
        }
    }
}
