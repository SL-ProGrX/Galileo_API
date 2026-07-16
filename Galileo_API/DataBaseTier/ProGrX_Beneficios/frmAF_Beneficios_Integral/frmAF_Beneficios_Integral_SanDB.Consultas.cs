using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class frmAF_Beneficios_Integral_SanDB
    {
        private const string SqlSancionMotivoLista = @"
            SELECT [TIPO_SANCION] AS item,
                   [DESCRIPCION]  AS descripcion,
                   [PLAZO_MAXIMO] AS plazo,
                   CODIGO_COBRO   AS codigo_cobro
            FROM [AFI_BENE_SANCIONES_TIPOS]
            WHERE ACTIVO = 1";

        /// <summary>
        /// Carga la lista de tipos de sanción activos disponibles en el formulario.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <returns>Tipos de sanción activos.</returns>
        public List<BeneficiosSancionesLista> BeneSancionMotivoLista_Obtener(int CodCliente)
        {
            var result = DbHelper.ExecuteListQuery<BeneficiosSancionesLista>(
                CreatePortalDb(), CodCliente, SqlSancionMotivoLista);

            return result.Result ?? new List<BeneficiosSancionesLista>();
        }

        /// <summary>
        /// Obtiene las sanciones registradas del socio para mostrarlas en el formulario.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns>Lista de sanciones del socio.</returns>
        public ErrorDto<List<AfiBeneSancionesDto>> BeneSacionesSocio_Obtener(int CodCliente, string cedula)
        {
            const string sql = "EXEC spAFI_Bene_Socio_Sanciones @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneSancionesDto>(sql, new { cedula = NormalizarTexto(cedula) }).ToList());

            return new ErrorDto<List<AfiBeneSancionesDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneSancionesDto>()
            };
        }
    }
}
