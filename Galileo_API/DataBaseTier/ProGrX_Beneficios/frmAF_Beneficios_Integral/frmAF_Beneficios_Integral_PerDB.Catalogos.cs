using System.Data;
using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class frmAF_Beneficios_Integral_PerDB
    {
        private const string SqlEstadoCivil =
            "SELECT Estado_Civil AS item, Descripcion AS descripcion FROM SYS_ESTADO_CIVIL WHERE Activo = 1 ORDER BY Descripcion ASC";

        private const string SqlNivelAcademico =
            "SELECT Catalogo_Id AS item, Descripcion AS descripcion FROM AFI_CATALOGOS WHERE Tipo_Id = 3 ORDER BY Descripcion";

        private const string SqlNacionalidad =
            "SELECT cod_nacionalidad AS item, Descripcion AS descripcion FROM Sys_nacionalidades WHERE Activo = 1 ORDER BY Omision DESC, Descripcion ASC";

        private const string SqlPais =
            "SELECT cod_Pais AS item, Descripcion AS descripcion FROM Paises WHERE Activo = 1 ORDER BY Omision DESC, Descripcion ASC";

        private const string SqlProvincia =
            "SELECT Provincia AS item, RTRIM(Descripcion) AS descripcion FROM Provincias";

        private const string SqlEstadoLaboral =
            "SELECT ESTADO_LABORAL AS item, Descripcion AS descripcion FROM AFI_ESTADO_LABORAL WHERE Activo = 1 ORDER BY Descripcion ASC";

        /// <summary>Lista de estados civiles.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoCivilLista_Obtener(int CodCliente)
            => ObtenerDropsLista(CodCliente, SqlEstadoCivil, "EstadoCivilLista_Obtener");

        /// <summary>Lista de niveles académicos.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NivelAcademicoLista_Obtener(int CodCliente)
            => ObtenerDropsLista(CodCliente, SqlNivelAcademico, "NivelAcademicoLista_Obtener");

        /// <summary>Lista de nacionalidades.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> NacionalidadLista_Obtener(int CodCliente)
            => ObtenerDropsLista(CodCliente, SqlNacionalidad, "NacionalidadLista_Obtener");

        /// <summary>Lista de países.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> PaisLista_Obtener(int CodCliente)
            => ObtenerDropsLista(CodCliente, SqlPais, "PaisLista_Obtener");

        /// <summary>Lista de provincias.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> ProvinciaLista_Obtener(int CodCliente)
            => ObtenerDropsLista(CodCliente, SqlProvincia, "ProvinciaLista_Obtener");

        /// <summary>Lista de estados laborales.</summary>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> EstadoLaboral_Obtener(int CodCliente)
            => ObtenerDropsLista(CodCliente, SqlEstadoLaboral, "EstadoLaboral_Obtener");

        /// <summary>
        /// Obtiene las cuentas bancarias del socio (SP spCrd_SGT_Bancos).
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="Usuario">Usuario para el que se consultan las cuentas.</param>
        /// <returns>Lista de cuentas bancarias.</returns>
        public ErrorDto<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<CuentaListaData>(
                    "[spCrd_SGT_Bancos]",
                    new { usuario = Usuario },
                    commandType: CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<CuentaListaData>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "Cuentas_Obtener - " + result.Description,
                Result = result.Result ?? new List<CuentaListaData>()
            };
        }

        /// <summary>
        /// Helper compartido: ejecuta una consulta de catálogo y la envuelve en el estándar de respuesta.
        /// </summary>
        private ErrorDto<List<AfBeneficioIntegralDropsLista>> ObtenerDropsLista(int CodCliente, string sql, string origen)
        {
            var result = DbHelper.ExecuteListQuery<AfBeneficioIntegralDropsLista>(CreatePortalDb(), CodCliente, sql);

            return new ErrorDto<List<AfBeneficioIntegralDropsLista>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : origen + " - " + result.Description,
                Result = result.Result ?? new List<AfBeneficioIntegralDropsLista>()
            };
        }
    }
}
