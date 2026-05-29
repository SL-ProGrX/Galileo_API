using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfPadronSalariosDB
    {
        private readonly IConfiguration _config;

        private const string SqlInstituciones = @"
                    SELECT COD_INSTITUCION AS item,
                           CONCAT('[', COD_DIVISA, ']  ', DESCRIPCION) AS descripcion
                    FROM dbo.INSTITUCIONES
                    WHERE ACTIVA = 1
                    ORDER BY COD_INSTITUCION;";

        private const string SpPadronRegistro = "spAFI_Padron_Registro";
        private const string SpPersonaSalariosAdd = "spAFI_Persona_Salarios_Add";

        public FrmAfPadronSalariosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de instituciones activas para el padrón de salarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones activas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronSalariosInstituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Procesa el padrón de empleados para una institución.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="institucion">Código de institución.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        
        /// <param name="padron">Registros de padrón a procesar.</param>
        /// <returns>Resultado del procesamiento.</returns>
        public ErrorDto AF_PadronSalarios_Padron_Procesar(int CodEmpresa, string institucion, string usuario, List<AfPadronData> padron)
        {
            if (padron is null || padron.Count == 0)
            {
                return DbHelper.OkResponse("No hay registros de padrón para procesar.");
            }

            var institucionSegura = NormalizarTexto(institucion);
            var usuarioSeguro = NormalizarTexto(usuario);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var item in padron)
                {
                    connection.Execute(
                        SpPadronRegistro,
                        CrearParametrosPadron(item, institucionSegura, usuarioSeguro),
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al procesar padrón de empleados.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Procesa los salarios de empleados para una institución.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="institucion">Código de institución.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="salario">Registros de salario a procesar.</param>
        /// <returns>Resultado del procesamiento.</returns>
        public ErrorDto AF_PadronSalarios_Salario_Procesar(int CodEmpresa, string institucion, string usuario, List<AfSalarioData> salario)
        {
            if (salario is null || salario.Count == 0)
            {
                return DbHelper.OkResponse("No hay registros de salario para procesar.");
            }

            var usuarioSeguro = NormalizarTexto(usuario);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var item in salario)
                {
                    connection.Execute(
                        SpPersonaSalariosAdd,
                        CrearParametrosSalario(item, usuarioSeguro),
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al procesar salarios de empleados.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Crea parámetros seguros para registrar un empleado en el padrón.
        /// </summary>
        private static object CrearParametrosPadron(AfPadronData item, string institucion, string usuario)
        {
            return new
            {
                Cedula = NormalizarTexto(item.identificacion),
                IdAlterno = NormalizarTexto(item.id_alterna),
                Nombre = NormalizarTexto(item.nombre),
                Institucion = institucion,
                FechaIngreso = item.fecha_ingreso,
                Usuario = usuario,
                Mov = "A"
            };
        }


        /// <summary>
        /// Crea parámetros seguros para registrar salario de una persona.
        /// </summary>
        private static object CrearParametrosSalario(AfSalarioData item, string usuario)
        {
            return new
            {
                Cedula = NormalizarTexto(item.identificacion),
                Tipo = "C",
                Divisa = NormalizarTexto(item.divisa),
                Fecha = item.fecha,
                SalarioDevengado = item.salario_bruto,
                Rebajos = item.rebajos,
                SalarioNeto = item.salario_neto,
                Embargo = item.embargos,
                Usuario = usuario,
                Mov = "A"
            };
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Normaliza valores de texto recibidos desde archivos o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}