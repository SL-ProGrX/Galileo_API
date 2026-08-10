using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using System.Text;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralPerDB
    {
        /// <summary>
        /// Obtiene los datos de la persona (socio) para el formulario. Retorna null si la cédula es inválida.
        /// </summary>
        /// <param name="CodCliente">Código de empresa/cliente.</param>
        /// <param name="cedula">Cédula del socio.</param>
        /// <returns>Datos de la persona.</returns>
        public ErrorDto<AfiBeneficioIntegralPersonaData>? DatosPersona_Obtener(int CodCliente, string? cedula)
        {
            if (cedula == null || cedula.Length < 2)
            {
                return null;
            }

            const string sql = @"
                SELECT
                    SUBSTRING(s.NOMBRE, 1, CHARINDEX(' ', s.NOMBRE) - 1) AS Apellido1,
                    SUBSTRING(s.NOMBRE, CHARINDEX(' ', s.NOMBRE) + 1,
                              CHARINDEX(' ', s.NOMBRE, CHARINDEX(' ', s.NOMBRE) + 1) - CHARINDEX(' ', s.NOMBRE) - 1) AS Apellido2,
                    SUBSTRING(s.NOMBRE, CHARINDEX(' ', s.NOMBRE, CHARINDEX(' ', s.NOMBRE) + 1) + 1, LEN(s.NOMBRE)) AS Nombrev2,
                    S.NOMBRE, s.ESTADOCIVIL, s.SEXO, s.FECHA_NAC, s.FECHAINGRESO,
                    s.ct AS LUGAR_TRABAJO, s.NIVEL_ACADEMICO, s.PROFESION,
                    s.COD_NACIONALIDAD, s.COD_PAIS_NAC, s.AF_EMAIL, s.EMAIL_02, s.APTO,
                    s.PROVINCIA, s.CANTON, s.DISTRITO, s.DIRECCION, s.ESTADOACTUAL,
                    m.MEMBRESIA, s.estadolaboral
                FROM SOCIOS s
                LEFT JOIN [dbo].[vAFI_Membresias] m ON s.CEDULA = m.CEDULA
                WHERE s.CEDULA = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<AfiBeneficioIntegralPersonaData>(sql, new { cedula }));

            return new ErrorDto<AfiBeneficioIntegralPersonaData>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "DatosPersona_Obtener - " + result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Valida si el socio existe en SOCIOS.
        /// </summary>
        public ErrorDto validaSocioExiste(int CodCliente, string cedula)
        {
            const string sql = "SELECT CEDULA, NOMBRE FROM SOCIOS WHERE CEDULA = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query(sql, new { cedula }).Any());

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = "validaSocioExiste" + result.Description };
            }

            return result.Result
                ? new ErrorDto { Code = 0, Description = string.Empty }
                : new ErrorDto { Code = -1, Description = "No se encontro Socio" };
        }

        /// <summary>
        /// Actualiza los datos de la persona (socio). El parámetro persona viene serializado en JSON.
        /// </summary>
        public ErrorDto Persona_Actualizar(int CodCliente, string cedula, string persona)
        {
            var datos = JsonConvert.DeserializeObject<BeneficioPersona>(persona) ?? new BeneficioPersona();

            const string sql = @"
                UPDATE SOCIOS
                   SET ESTADOCIVIL      = @estadoCivil,
                       AF_EMAIL         = @email1,
                       EMAIL_02         = @email2,
                       APTO             = @aptoPostal,
                       DIRECCION        = @direccion,
                       COD_NACIONALIDAD = @nacionalidad,
                       PROVINCIA        = @provincia,
                       CANTON           = @canton,
                       DISTRITO         = @distrito,
                       NIVEL_ACADEMICO  = @nivelAcademico,
                       COD_PAIS_NAC     = @paisNacimiento,
                       PROFESION        = @ocupacion,
                       FECHA_NAC        = @fechaNacimiento,
                       FECHAINGRESO     = @fechaIngreso,
                       ESTADOLABORAL    = @estadoLaboral
                 WHERE cedula = @cedula";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new
                {
                    estadoCivil = datos.EstadoCivil,
                    email1 = datos.Email1,
                    email2 = datos.Email2,
                    aptoPostal = datos.AptoPostal,
                    direccion = datos.Direccion,
                    nacionalidad = datos.Nacionalidad,
                    provincia = datos.Provincia,
                    canton = datos.Canton,
                    distrito = datos.Distrito,
                    nivelAcademico = datos.NivelAcademico,
                    paisNacimiento = (datos.PaisNacimiento ?? string.Empty).Trim(),
                    ocupacion = datos.Ocupacion,
                    fechaNacimiento = datos.FechaNacimiento,
                    fechaIngreso = datos.FechaIngreso,
                    estadoLaboral = datos.estadolaboral,
                    cedula
                }));

            return new ErrorDto
            {
                Code = result.Code,
                Description =  result.Description
            };
        }

        /// <summary>
        /// Ejecuta las validaciones configuradas de persona y acumula los mensajes aplicables.
        /// </summary>
        public ErrorDto ValidarPersona(int CodCliente, string cedula)
        {
            const string sqlValidaciones = @"
                SELECT * FROM AFI_BENE_VALIDACIONES
                 WHERE ESTADO = 1 AND TIPO = 'P'
                   AND COD_VAL IN (SELECT COD_VAL FROM AFI_BENE_VALIDA_CATEGORIA WHERE ESTADO = 1)
                 ORDER BY PRIORIDAD ASC";

            var cedulaSegura = NormalizarCedula(cedula);

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var validaciones = connection.Query<ValidacionRow>(sqlValidaciones).ToList();
                var mensajes = new StringBuilder();

                foreach (var validacion in validaciones)
                {
                    if (string.IsNullOrWhiteSpace(validacion.query_val))
                    {
                        continue;
                    }

                    var valor = connection.QueryFirstOrDefault<int>(
                        validacion.query_val.Replace("@cedula", cedula));

                    if (valor == validacion.resultado_val)
                    {
                        mensajes.Append(validacion.msj_val).Append("...\n");
                    }
                }

                return mensajes.ToString();
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = $"Error al validar socio: {result.Description}" };
            }

            return new ErrorDto { Code = 0, Description = result.Result ?? string.Empty };
        }
    }
}
