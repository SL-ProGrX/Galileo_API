using Dapper;
using Galileo.Models;
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
    }
}
