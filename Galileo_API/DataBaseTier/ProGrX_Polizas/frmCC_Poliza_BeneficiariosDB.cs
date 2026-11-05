using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public sealed class FrmCcPolizaBeneficiariosDB
    {
        private const int ModuloPolizas = 11;
        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario de beneficiarios de pólizas colectivas.
        /// </summary>
        /// <param name="config">Configuración de conexiones y servicios del API.</param>
        public FrmCcPolizaBeneficiariosDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los catálogos requeridos por el formulario de beneficiarios de pólizas colectivas.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Pólizas, tipos de identificación y parentescos activos.</returns>
        public ErrorDto<CcPolizaBeneficiariosCatalogosDto> CC_Poliza_Beneficiarios_Catalogos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                const string sql = """
                    SELECT CONVERT(varchar(20), COD_POLIZA) AS item,
                           RTRIM(Poliza_Desc) AS descripcion
                    FROM vPoliza_Catalogo
                    ORDER BY COD_POLIZA;

                    SELECT CONVERT(varchar(20), TIPO_ID) AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM AFI_TIPOS_IDS
                    ORDER BY TIPO_ID;

                    SELECT RTRIM(cod_Parentesco) AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM sys_Parentescos
                    WHERE activo = 1
                    ORDER BY Descripcion;
                    """;

                using var reader = connection.QueryMultiple(sql, commandTimeout: 0);
                return new CcPolizaBeneficiariosCatalogosDto
                {
                    polizas = reader.Read<CcPolizaBeneficiariosListaDto>().ToList(),
                    tipos_id = reader.Read<CcPolizaBeneficiariosListaDto>().ToList(),
                    parentescos = reader.Read<CcPolizaBeneficiariosListaDto>().ToList()
                };
            });
        }

        /// <summary>
        /// Obtiene los beneficiarios registrados para una persona y una póliza colectiva.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="cedula">Identificación de la persona asegurada.</param>
        /// <param name="codPoliza">Código de la póliza colectiva.</param>
        /// <returns>Lista de beneficiarios en el orden registrado.</returns>
        public ErrorDto<List<CcPolizaBeneficiarioDto>> CC_Poliza_Beneficiarios_Obtener(
            int codEmpresa,
            string cedula,
            string codPoliza)
        {
            if (string.IsNullOrWhiteSpace(cedula) || string.IsNullOrWhiteSpace(codPoliza))
            {
                return DbHelper.CreateErrorResponse<List<CcPolizaBeneficiarioDto>>(
                    "La identificación y la póliza son requeridas.",
                    -2,
                    []);
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query<CcPolizaBeneficiarioDto>(
                    "spPoliza_Persona_Beneficiarios",
                    new
                    {
                        Cedula = cedula.Trim(),
                        Poliza = codPoliza.Trim()
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0).ToList());
        }

        /// <summary>
        /// Consulta una persona en el padrón nacional por identificación.
        /// </summary>
        /// <param name="identificacion">Identificación por consultar.</param>
        /// <returns>Nombre de la persona encontrada.</returns>
        public ErrorDto<CcPolizaBeneficiariosPadronDto?> CC_Poliza_Beneficiarios_Padron_Obtener(
            string identificacion)
        {
            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return DbHelper.CreateErrorResponse<CcPolizaBeneficiariosPadronDto?>(
                    "La identificación es requerida.",
                    -2);
            }

            var connectionString = _config.GetConnectionString("BaseConnString")
                ?? throw new InvalidOperationException("No se encontró la conexión del padrón.");

            try
            {
                using var connection = new SqlConnection(connectionString);
                var persona = connection.QueryFirstOrDefault<CcPolizaBeneficiariosPadronDto>(
                    "spSYS_Consulta_Padron",
                    new { Identificacion = identificacion.Trim(), Pais = "CRI" },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0);

                return DbHelper.CreateOkResponse<CcPolizaBeneficiariosPadronDto?>(persona);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CcPolizaBeneficiariosPadronDto?>(
                    "No fue posible consultar el padrón.");
            }
        }

        /// <summary>
        /// Registra los beneficiarios mediante el procedimiento existente e informa
        /// la etapa exacta cuando el guardado se interrumpe.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario que realiza el registro.</param>
        /// <param name="request">Persona, póliza y beneficiarios por guardar.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto CC_Poliza_Beneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            CcPolizaBeneficiariosGuardarRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("La solicitud es requerida.", -2);
            }

            var validacion = ValidarGuardar(usuario, request);
            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(validacion, -2);
            }

            var lineaEnProceso = 0;
            var guardadoCompleto = false;

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                for (var index = 0; index < request.beneficiarios.Count; index++)
                {
                    lineaEnProceso = index + 1;
                    var beneficiario = request.beneficiarios[index];

                    connection.Execute(
                        "spPoliza_Persona_Beneficiarios_Add",
                        new
                        {
                            Cedula = request.cedula.Trim(),
                            Poliza = request.cod_poliza.Trim(),
                            Linea = lineaEnProceso,
                            Tipo_ID = beneficiario.tipo_id,
                            Identificacion = beneficiario.identificacion.Trim(),
                            Nombre = beneficiario.nombre.Trim(),
                            Parentesco = beneficiario.cod_parentesco.Trim(),
                            Porcentaje = beneficiario.porcentaje,
                            Usuario = usuario.Trim()
                        },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 0);
                }

                guardadoCompleto = true;
            }
            catch (Exception)
            {
                if (lineaEnProceso == 0)
                {
                    return DbHelper.ErrorResponse(
                        "No fue posible abrir la conexión para guardar los beneficiarios. "
                        + "Verifique el registro antes de volver a intentar.");
                }

                return DbHelper.ErrorResponse(
                    $"El guardado se interrumpió al procesar el beneficiario "
                    + $"{lineaEnProceso} de {request.beneficiarios.Count}. "
                    + "La póliza puede haber quedado con beneficiarios parciales. "
                    + "Consulte el registro antes de volver a intentar.");
            }

            if (!guardadoCompleto)
            {
                return DbHelper.ErrorResponse(
                    "No fue posible completar el guardado de beneficiarios.");
            }

            try
            {
                RegistrarBitacora(codEmpresa, usuario, request);
            }
            catch (Exception)
            {
                return DbHelper.OkResponse(
                    "Los beneficiarios se guardaron, pero no fue posible completar "
                    + "la bitácora. Verifique el registro de auditoría.");
            }

            return DbHelper.OkResponse(
                "Beneficiarios registrados satisfactoriamente.");
        }

        /// <summary>
        /// Valida los datos requeridos y que la distribución de porcentajes totalice cien.
        /// </summary>
        /// <param name="usuario">Usuario que ejecuta el guardado.</param>
        /// <param name="request">Persona, póliza y beneficiarios por validar.</param>
        /// <returns>Mensaje de validación; vacío cuando los datos son válidos.</returns>
        private static string ValidarGuardar(
            string usuario,
            CcPolizaBeneficiariosGuardarRequest request)
        {
            if (string.IsNullOrWhiteSpace(usuario)
                || string.IsNullOrWhiteSpace(request.cedula)
                || string.IsNullOrWhiteSpace(request.cod_poliza))
            {
                return "El usuario, la identificación y la póliza son requeridos.";
            }

            if (request.beneficiarios is null
                || request.beneficiarios.Count is < 1 or > 6)
            {
                return "Debe registrar entre uno y seis beneficiarios.";
            }

            if (request.beneficiarios.Any(item =>
                item is null
                || item.tipo_id <= 0
                || string.IsNullOrWhiteSpace(item.identificacion)
                || string.IsNullOrWhiteSpace(item.nombre)
                || string.IsNullOrWhiteSpace(item.cod_parentesco)
                || item.porcentaje <= 0))
            {
                return "Complete la información de todos los beneficiarios con porcentaje mayor que cero.";
            }

            return request.beneficiarios.Sum(item => item.porcentaje) == 100m
                ? string.Empty
                : "El porcentaje de todos los beneficiarios debe ser igual al 100%.";
        }

        /// <summary>
        /// Registra en bitácora el guardado general y el detalle de cada beneficiario.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa activa.</param>
        /// <param name="usuario">Usuario que realizó el registro.</param>
        /// <param name="request">Persona, póliza y beneficiarios registrados.</param>
        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            CcPolizaBeneficiariosGuardarRequest request)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario.Trim(),
                Movimiento = "Registra - WEB",
                DetalleMovimiento =
                    $"Beneficiarios Póliza Colectiva: {request.cod_poliza.Trim()}, Persona: {request.cedula.Trim()}",
                Modulo = ModuloPolizas
            });

            foreach (var beneficiario in request.beneficiarios)
            {
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario.Trim(),
                    Movimiento = "Registra - WEB",
                    DetalleMovimiento =
                        $"Beneficiarios Póliza Colectiva: {request.cod_poliza.Trim()}, "
                        + $"Persona: {request.cedula.Trim()}, "
                        + $"B.Id: {beneficiario.identificacion.Trim()}, "
                        + $"B.N: {beneficiario.nombre.Trim()}, "
                        + $"B.Porc: {beneficiario.porcentaje:N2}%",
                    Modulo = ModuloPolizas
                });
            }
        }
    }
}