using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public class FrmAFBeneficiariosDB
    {
        private readonly IConfiguration _config;

        private const string SpPersonaBeneficiariosConsulta = "spAFI_PERSONA_BENEFICIARIOS_Consulta";
        private const string SpPersonaBeneficiariosRegistra = "spAFI_PERSONA_BENEFICIARIOS_Registra";

        private const string SqlTiposIdentificacionBeneficiarios = @"
                    SELECT TIPO_ID AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM dbo.AFI_TIPOS_IDS
                    WHERE TIPO_PERSONERIA = 'F'
                    ORDER BY Tipo_Id;";

        private const string SqlParentescosActivos = @"
                    SELECT RTRIM(cod_Parentesco) AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM dbo.sys_Parentescos
                    WHERE activo = 1;";

        public FrmAFBeneficiariosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los beneficiarios asociados a una persona y línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="lineaId">Identificador de línea. Si es nulo se consulta con cero.</param>
        /// <returns>Listado de beneficiarios de la persona.</returns>
        public ErrorDto<List<PersonaBeneficiarioDto>> AF_PersonaBeneficiarios_Consulta(int CodEmpresa, string cedula, int? lineaId)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<PersonaBeneficiarioDto>(
                    SpPersonaBeneficiariosConsulta,
                    new
                    {
                        Cedula = NormalizarTexto(cedula),
                        Linea = lineaId ?? 0
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<PersonaBeneficiarioDto>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<PersonaBeneficiarioDto>()
            };
        }


        /// <summary>
        /// Registra, actualiza o elimina un beneficiario de persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="dto">Datos del beneficiario.</param>
        /// <returns>Identificador de línea generado o actualizado.</returns>
        public ErrorDto<int> AF_PersonaBeneficiarios_Registro(int CodEmpresa, PersonaBeneficiarioDto dto)
        {
            if (dto is null)
            {
                return DbHelper.CreateErrorResponse("Los datos del beneficiario son requeridos.", -2, 0);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<dynamic>(
                    SpPersonaBeneficiariosRegistra,
                    CrearParametrosBeneficiario(dto),
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al registrar beneficiario.",
                    result.Code.GetValueOrDefault(-1),
                    0);
            }

            var lineaId = ObtenerLineaIdBeneficiario(result.Result);
            return lineaId.HasValue
                ? DbHelper.CreateOkResponse(lineaId.Value, "Guardado correctamente")
                : DbHelper.CreateErrorResponse("No se obtuvo respuesta del SP", -1, 0);
        }


        /// <summary>
        /// Obtiene los catálogos requeridos para beneficiarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Tipos de identificación y parentescos activos.</returns>
        public ErrorDto<BeneficiariosCatalogoDto> AF_Beneficiarios_Catalogos_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new BeneficiariosCatalogoDto
            {
                TiposIdentificacion = connection.Query<DropDownListaGenericaModel>(SqlTiposIdentificacionBeneficiarios).ToList(),
                Parentescos = connection.Query<DropDownListaGenericaModel>(SqlParentescosActivos).ToList()
            });

            return new ErrorDto<BeneficiariosCatalogoDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new BeneficiariosCatalogoDto()
            };
        }
        
        
        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Crea los parámetros seguros para registrar un beneficiario.
        /// </summary>
        /// <param name="dto">Datos del beneficiario.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosBeneficiario(PersonaBeneficiarioDto dto)
        {
            return new
            {
                Cedula = NormalizarTexto(dto.Cedula),
                dto.Linea_Id,
                Cedula_Beneficiario = NormalizarTexto(dto.Cedula_Beneficiario),
                Nombre = NormalizarTexto(dto.Nombre),
                dto.Fecha_Nac,
                Tipo_Relacion = NormalizarTexto(dto.Tipo_Relacion),
                Cod_Parentesco = NormalizarTexto(dto.Cod_Parentesco),
                dto.Porcentaje,
                AplicaSeguros = dto.Aplica_Seguros ? 1 : 0,
                Notas = NormalizarTexto(dto.Notas),
                Direccion = NormalizarTexto(dto.Direccion),
                Apto_Postal = NormalizarTexto(dto.Apto_Postal),
                Telefono1 = NormalizarTexto(dto.Telefono1),
                Telefono2 = NormalizarTexto(dto.Telefono2),
                Email = NormalizarTexto(dto.Email),
                TipoMov = NormalizarTexto(dto.TipoMov),
                Registro_Usuario = NormalizarTexto(dto.Registro_Usuario),
                Albacea = dto.Albacea_Check ? 1 : 0,
                Albacea_Cedula = NormalizarTexto(dto.Albacea_Cedula),
                Albacea_Nombre = NormalizarTexto(dto.Albacea_Nombre),
                Albacea_Movil = NormalizarTexto(dto.Albacea_Movil),
                Albacea_TelTra = NormalizarTexto(dto.Albacea_TelTra),
                Albacea_TelTra_Ext = NormalizarTexto(dto.Albacea_TelTra_Ext),
                Tipo_Id_R = dto.Tipo_Id_R
            };
        }


        /// <summary>
        /// Obtiene el identificador de línea devuelto por el procedimiento de beneficiarios.
        /// </summary>
        /// <param name="result">Resultado dinámico del procedimiento almacenado.</param>
        /// <returns>Identificador de línea si está disponible.</returns>
        private static int? ObtenerLineaIdBeneficiario(dynamic? result)
        {
            if (result is null)
            {
                return null;
            }

            return Convert.ToInt32(result.LineaId);
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}