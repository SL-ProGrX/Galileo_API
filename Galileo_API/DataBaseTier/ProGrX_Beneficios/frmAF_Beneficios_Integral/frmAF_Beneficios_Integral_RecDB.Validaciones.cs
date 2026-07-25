using Dapper;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralRecDB
    {
        /// <summary>
        /// Valida si el estudiante ya está registrado en otro reconocimiento del año.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="cedula">Cédula del estudiante.</param>
        /// <param name="id_beneficio">Beneficio actual a excluir de la validación.</param>
        /// <returns>Code -1 con detalle si ya existe; Code 0 en caso contrario.</returns>
        public ErrorDto ValidaEstudiante_Obtener(int CodCliente, string cedula, string id_beneficio)
        {
            const string sql = @"
                SELECT CONCAT(O.ID_BENEFICIO, TRIM(O.COD_BENEFICIO), FORMAT(O.CONSEC, '00000'), '- Cédula: ', O.CEDULA)
                FROM AFI_BENE_REGISTRO_RECONOCIMIENTOS R
                LEFT JOIN AFI_BENE_OTORGA O ON O.ID_BENEFICIO = R.ID_BENEFICIO
                WHERE R.CEDULA_ESTUDIANTE = @cedula
                  AND YEAR(O.REGISTRA_FECHA) = YEAR(GETDATE())
                  AND DATEDIFF(YEAR, O.REGISTRA_FECHA, GETDATE()) <= 1
                  AND O.ID_BENEFICIO != @idBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<string>(sql, new { cedula, idBeneficio = id_beneficio }).ToList());

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = "ValidaEstudiante_Obtener: " + result.Description };
            }

            var lista = result.Result ?? new List<string>();
            if (lista.Count == 0)
            {
                return new ErrorDto { Code = 0, Description = string.Empty };
            }

            return new ErrorDto
            {
                Code = -1,
                Description = "El estudiante ya esta registrado en " + string.Join(" ", lista) + " "
            };
        }

        /// <summary>
        /// Obtiene la nota mínima configurada para el beneficio.
        /// </summary>
        public ErrorDto ValidaNotaMinima(int CodCliente)
            => ObtenerParametroNota(CodCliente, "NotaMinima", "ValidaNotaMinima");

        /// <summary>
        /// Obtiene la nota mínima para pasar la materia.
        /// </summary>
        public ErrorDto ValidaNotaPasaMateria(int CodCliente)
            => ObtenerParametroNota(CodCliente, "NotaPasaAnho", "ValidaNotaPasaMateria");

        /// <summary>
        /// Lee de configuración el código de parámetro indicado y devuelve su VALOR desde SIF_PARAMETROS.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="claveConfig">Clave dentro de la sección AFI_Beneficios.</param>
        /// <param name="origen">Nombre del método origen, usado para el prefijo de error.</param>
        /// <returns>Description con el valor de la nota (o "0" si no existe).</returns>
        private ErrorDto ObtenerParametroNota(int CodCliente, string claveConfig, string origen)
        {
            var codParametro = _config.GetSection("AFI_Beneficios").GetSection(claveConfig).Value ?? string.Empty;
            const string sql = "SELECT VALOR FROM [SIF_PARAMETROS] WHERE COD_PARAMETRO = @codParametro";

            var result = DbHelper.ExecuteSingleQuery<float>(CreatePortalDb(), CodCliente, sql, 0, new { codParametro });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0
                    ? result.Result.ToString()
                    : origen + ": " + result.Description
            };
        }
    }
}
