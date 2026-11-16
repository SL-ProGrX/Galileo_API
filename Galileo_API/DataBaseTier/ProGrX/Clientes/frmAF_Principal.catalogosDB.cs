using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Data;

namespace Galileo.DataBaseTier
{
    public partial class FrmAFPrincipalDB
    {
        /// <summary>
        /// Carga los catálogos de Unidad Programática y Unidad de Trabajo
        /// utilizados por la versión ASE de afiliación.
        /// </summary>
        /// <param name="connection">Conexión abierta a la base de datos de la empresa.</param>
        /// <param name="catalogos">Contenedor de catálogos que se devolverá a la pantalla.</param>
        private static void CargarCatalogosUnidadesAse(
            IDbConnection connection,
            AfCatalogosGeneralesDto catalogos)
        {
            catalogos.UProgramaticas = connection.Query<DropDownListaGenericaModel>(
                @"IF OBJECT_ID('dbo.UProgramatica', 'U') IS NOT NULL
                  SELECT RTRIM(Codigo) AS item, RTRIM(Descripcion) AS descripcion
                  FROM dbo.UProgramatica
                  ORDER BY Descripcion;").ToList();

            catalogos.UTrabajos = connection.Query<DropDownListaGenericaModel>(
                @"IF OBJECT_ID('dbo.UTrabajo', 'U') IS NOT NULL
                  SELECT RTRIM(UT_Codigo) AS item, RTRIM(UT_Descripcion) AS descripcion
                  FROM dbo.UTrabajo
                  ORDER BY UT_Descripcion;").ToList();
        }

        /// <summary>
        /// Obtiene el largo exacto de la identificación por tipo (AFI_TIPOS_IDS.LARGO_MINIMO).
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Tipos de identificación con su largo; 0 cuando la tabla no define largo.</returns>
        public ErrorDto<List<AfTipoIdLargoDto>> AF_TiposId_Largos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AfTipoIdLargoDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT TIPO_ID AS TipoId, ISNULL(LARGO_MINIMO, 0) AS Largo
                  FROM dbo.AFI_TIPOS_IDS
                  ORDER BY TIPO_ID;");
        }
    }
}
