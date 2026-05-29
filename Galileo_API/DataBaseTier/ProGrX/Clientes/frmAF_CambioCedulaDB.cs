using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmAfCambioCedulaDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SqlTiposCedulas = @"
                    SELECT TIPO_ID AS item,
                           RTRIM(Descripcion) AS descripcion
                    FROM dbo.AFI_TIPOS_IDS
                    ORDER BY Tipo_Id;";

        private const string SqlTipoIdLargoMinimo = @"
                    SELECT LARGO_MINIMO
                    FROM dbo.AFI_TIPOS_IDS
                    WHERE TIPO_ID = @TipoId;";

        private const string SpIdentificacionCambio = "spAFI_Identificacion_Cambio";
        private const string SpSegLogon = "spSEG_Logon";

        private const string SqlCedulaCambio = @"
                    SELECT
                        S.Cedula AS cedulaActual,
                        S.NOMBRE AS nombre,
                        S.TIPO_ID AS tipoid,
                        Tip.DESCRIPCION AS TipoId_Desc,
                        CASE
                            WHEN Ep.COD_ESTADO = 'N' THEN 'No Asociado'
                            ELSE 'Asociado'
                        END AS estado,
                        Ep.DESCRIPCION AS Estado_Persona
                    FROM dbo.socios S
                    INNER JOIN dbo.AFI_TIPOS_IDS Tip
                        ON S.TIPO_ID = Tip.TIPO_ID
                    INNER JOIN dbo.AFI_ESTADOS_PERSONA Ep
                        ON S.ESTADOACTUAL = Ep.COD_ESTADO
                    WHERE TRIM(S.cedula) = @Cedula;";

        public FrmAfCambioCedulaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtiene los tipos de cédula disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de tipos de cédula.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposCedulas_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTiposCedulas);
        }


        /// <summary>
        /// Aplica el cambio de cédula de una persona y registra la bitácora correspondiente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que aplica el cambio.</param>
        /// <param name="cambioData">JSON con los datos del cambio de cédula.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_CambioCedula_Aplicar(int CodEmpresa, string usuario, string cambioData)
        {
            var cambioCedula = DbHelper.DeserializeOrNew<AFCambioCedulaDto>(cambioData);
            var validacion = ValidarCambioCedula(cambioCedula);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var largoMinimo = connection.QueryFirstOrDefault<int>(
                    SqlTipoIdLargoMinimo,
                    new { TipoId = cambioCedula.tipo });

                if (!CedulaCumpleLargo(cambioCedula.cedulaNueva, largoMinimo))
                {
                    return DbHelper.ErrorResponse(
                        $"El n&uacute;mero de identificaci&oacute;n nuevo no cumplen con los caracteres requeridos {largoMinimo}, verifique!",
                        -2);
                }

                connection.Execute(
                    SpIdentificacionCambio,
                    CrearParametrosCambioCedula(usuario, cambioCedula),
                    commandType: CommandType.StoredProcedure);

                RegistrarBitacoraCambioCedula(CodEmpresa, usuario, cambioCedula);

                return DbHelper.OkResponse("Guardado correctamente");
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al aplicar cambio de cédula.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Obtiene la información actual de una cédula para cambio de identificación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula actual.</param>
        /// <returns>Información de la persona asociada a la cédula.</returns>
        public ErrorDto<AFCedulaCambioDto> AF_Cedula_Obtener(int CodEmpresa, string cedula)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<AFCedulaCambioDto>(
                    SqlCedulaCambio,
                    new { Cedula = NormalizarTexto(cedula) }));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener información de la cédula.",
                    result.Code.GetValueOrDefault(-1),
                    new AFCedulaCambioDto());
            }

            if (result.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró información para la cédula indicada.",
                    1,
                    new AFCedulaCambioDto());
            }

            return DbHelper.CreateOkResponse(result.Result);
        }


        /// <summary>
        /// Valida que el usuario tenga permisos para aplicar el cambio de cédula y que sus credenciales sean correctas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="parametros">JSON con usuario y clave.</param>
        /// <returns>Resultado de la validación.</returns>
        public ErrorDto AF_Usuario_Validar(int CodEmpresa, string parametros)
        {
            var param = DbHelper.DeserializeOrNew<AFUsuarioLogonDto>(parametros);
            var validacion = ValidarParametrosUsuario(param);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var permiso = ValidarPermisoCambioCedula(CodEmpresa, NormalizarTexto(param.usuario));
            if (permiso.Code != 0)
            {
                return permiso;
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    SpSegLogon,
                    new
                    {
                        usuario = NormalizarTexto(param.usuario),
                        clave = param.clave
                    },
                    commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al validar usuario.", result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.ErrorResponse("Clave de Usuario incorrecta, intente de nuevo", -2)
                : DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Valida los datos mínimos del cambio de cédula.
        /// </summary>
        /// <param name="cambioCedula">Datos del cambio.</param>
        /// <returns>Resultado de la validación.</returns>
        private static ErrorDto ValidarCambioCedula(AFCambioCedulaDto cambioCedula)
        {
            if (string.IsNullOrWhiteSpace(cambioCedula.cedulaNueva) || string.IsNullOrWhiteSpace(cambioCedula.cedulaAnterior))
            {
                return DbHelper.ErrorResponse("Datos de cambio de cédula inválidos.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Valida que la nueva cédula tenga el largo requerido.
        /// </summary>
        /// <param name="cedulaNueva">Nueva cédula.</param>
        /// <param name="largoMinimo">Largo requerido.</param>
        /// <returns>Verdadero si cumple el largo requerido.</returns>
        private static bool CedulaCumpleLargo(string? cedulaNueva, int largoMinimo)
        {
            return !string.IsNullOrWhiteSpace(cedulaNueva) && cedulaNueva.Length == largoMinimo;
        }

        /// <summary>
        /// Crea los parámetros seguros para el procedimiento de cambio de identificación.
        /// </summary>
        /// <param name="usuario">Usuario que aplica el cambio.</param>
        /// <param name="cambioCedula">Datos del cambio.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosCambioCedula(string usuario, AFCambioCedulaDto cambioCedula)
        {
            return new
            {
                CedulaNueva = NormalizarTexto(cambioCedula.cedulaNueva),
                CedulaAnterior = NormalizarTexto(cambioCedula.cedulaAnterior),
                Usuario = NormalizarTexto(usuario),
                TipoId = cambioCedula.tipo
            };
        }


        /// <summary>
        /// Registra la bitácora del cambio de cédula aplicado.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que aplica el cambio.</param>
        /// <param name="cambioCedula">Datos del cambio.</param>
        private void RegistrarBitacoraCambioCedula(int codEmpresa, string usuario, AFCambioCedulaDto cambioCedula)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Cambio de Cedula : {NormalizarTexto(cambioCedula.cedulaAnterior)} a {NormalizarTexto(cambioCedula.cedulaNueva)} : {NormalizarTexto(cambioCedula.nombre)}",
                Movimiento = "APLICA - WEB",
                Modulo = 9
            });
        }

        /// <summary>
        /// Valida los parámetros mínimos de usuario.
        /// </summary>
        /// <param name="param">Parámetros de usuario.</param>
        /// <returns>Resultado de la validación.</returns>
        private static ErrorDto ValidarParametrosUsuario(AFUsuarioLogonDto param)
        {
            return string.IsNullOrWhiteSpace(param.usuario)
                ? DbHelper.ErrorResponse("Parámetros de usuario inválidos.", -2)
                : DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Valida que el usuario tenga permiso para aplicar cambios de identificación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario a validar.</param>
        /// <returns>Resultado de la validación de permisos.</returns>
        private ErrorDto ValidarPermisoCambioCedula(int codEmpresa, string usuario)
        {
            var paramAccess = new ParametrosAccesoDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                Modulo = 1,
                FormName = "frmAF_CambioCedula",
                Boton = "cmdAplicar"
            };

            return _mSecurity.Derecho(paramAccess) != 1
                ? DbHelper.ErrorResponse($"El Usuario: {NormalizarTexto(usuario)}, no es tiene permisos de cambio de Identificaci&oacute;n de Personas!", -2)
                : DbHelper.OkResponse("Ok");
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}