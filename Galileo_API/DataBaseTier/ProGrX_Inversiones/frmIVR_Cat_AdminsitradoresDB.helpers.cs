using Dapper;
using Galileo.Models.ProGrX_Inversiones;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed partial class FrmIvrCatAdminsitradoresDb
    {
        /// <summary>
        /// Valida los campos requeridos del administrador
        /// y normaliza la cuenta contable.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información recibida.
        /// </param>
        /// <returns>
        /// Mensaje y cuenta normalizada.
        /// </returns>
        private (string mensaje, string cuenta)
            IVR_Cat_Adminsitradores_Request_Validar(
                int codEmpresa,
                IvrCatAdministradorGuardarRequest? request)
        {
            if (request is null)
            {
                return (
                    "La solicitud es requerida.",
                    string.Empty);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_administrador))
            {
                return (
                    "El c&oacute;digo del administrador "
                    + "es requerido.",
                    string.Empty);
            }

            if (string.IsNullOrWhiteSpace(
                    request.descripcion))
            {
                return (
                    "El nombre del administrador "
                    + "no es v&aacute;lido.",
                    string.Empty);
            }

            if (string.IsNullOrWhiteSpace(
                    request.tipo_administrador))
            {
                return (
                    "El tipo de administrador es requerido.",
                    string.Empty);
            }

            if (request.estado.Trim().ToUpperInvariant()
                is not ("A" or "I"))
            {
                return (
                    "El estado indicado no es v&aacute;lido.",
                    string.Empty);
            }

            if (request.tes_emitir.Trim().ToUpperInvariant()
                is not ("CK" or "TE" or "ND"))
            {
                return (
                    "El tipo de documento no es v&aacute;lido.",
                    string.Empty);
            }

            if (request.tes_banco <= 0)
            {
                return (
                    "El banco es requerido.",
                    string.Empty);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return (
                    "El usuario es requerido.",
                    string.Empty);
            }

            var cuenta =
                IVR_Cat_Adminsitradores_Cuenta_Normalizar(
                    codEmpresa,
                    request.cod_cuenta_transito);

            return cuenta.es_valida
                ? (string.Empty, cuenta.cuenta)
                : (cuenta.mensaje, string.Empty);
        }

        /// <summary>
        /// Normaliza y valida una cuenta contable.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="cuenta">
        /// Cuenta capturada.
        /// </param>
        /// <returns>
        /// Estado, cuenta normalizada y mensaje.
        /// </returns>
        private (
            bool es_valida,
            string cuenta,
            string mensaje)
            IVR_Cat_Adminsitradores_Cuenta_Normalizar(
                int codEmpresa,
                string? cuenta)
        {
            var cuentaNormalizada =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    cuenta?.Trim() ?? string.Empty,
                    0);

            if (string.IsNullOrWhiteSpace(
                    cuentaNormalizada) ||
                !_cntLinkDb.fxgCntCuentaValida(
                    codEmpresa,
                    cuentaNormalizada))
            {
                return (
                    false,
                    string.Empty,
                    MensajeCuentaInvalida);
            }

            return (
                true,
                cuentaNormalizada,
                string.Empty);
        }

        /// <summary>
        /// Verifica si la identificación está asignada
        /// a otro administrador.
        /// </summary>
        /// <param name="connection">
        /// Conexión de la empresa.
        /// </param>
        /// <param name="identificacion">
        /// Identificación que debe validarse.
        /// </param>
        /// <param name="codAdministrador">
        /// Administrador que debe excluirse.
        /// </param>
        /// <returns>
        /// True cuando existe un duplicado.
        /// </returns>
        private static bool
            IVR_Cat_Adminsitradores_Identificacion_Duplicada(
                SqlConnection connection,
                string? identificacion,
                string? codAdministrador)
        {
            const string sql = """
                SELECT COUNT(1)
                FROM IVR_ADMINISTRADOR
                WHERE COD_ADMINISTRADOR <>
                    @cod_administrador
                  AND IDENTIFICACION =
                    @identificacion;
                """;

            return connection.ExecuteScalar<int>(
                sql,
                new
                {
                    cod_administrador =
                        codAdministrador?.Trim() ??
                        string.Empty,
                    identificacion =
                        identificacion?.Trim() ??
                        string.Empty
                }) > 0;
        }

        /// <summary>
        /// Crea los parámetros requeridos para registrar
        /// o actualizar un administrador.
        /// </summary>
        /// <param name="request">
        /// Información recibida.
        /// </param>
        /// <param name="cuenta">
        /// Cuenta contable normalizada.
        /// </param>
        /// <returns>
        /// Parámetros de Dapper.
        /// </returns>
        private static DynamicParameters
            IVR_Cat_Adminsitradores_Parametros_Crear(
                IvrCatAdministradorGuardarRequest request,
                string cuenta)
        {
            var parametros = new DynamicParameters();

            parametros.Add(
                "cod_administrador",
                request.cod_administrador.Trim());

            parametros.Add(
                "tipo_administrador",
                request.tipo_administrador.Trim());

            parametros.Add(
                "identificacion",
                request.identificacion.Trim());

            parametros.Add(
                "descripcion",
                request.descripcion.Trim());

            parametros.Add(
                "estado",
                request.estado
                    .Trim()
                    .ToUpperInvariant());

            parametros.Add(
                "telefono1",
                request.telefono1.Trim());

            parametros.Add(
                "telefono2",
                request.telefono2.Trim());

            parametros.Add(
                "fax",
                request.fax.Trim());

            parametros.Add(
                "email_01",
                request.email_01.Trim());

            parametros.Add(
                "email_02",
                request.email_02.Trim());

            parametros.Add(
                "apto_postal",
                request.apto_postal.Trim());

            parametros.Add(
                "website",
                request.website.Trim());

            parametros.Add(
                "direccion",
                request.direccion.Trim());

            parametros.Add(
                "cod_cuenta_transito",
                cuenta);

            parametros.Add(
                "tes_emitir",
                request.tes_emitir
                    .Trim()
                    .ToUpperInvariant());

            parametros.Add(
                "tes_banco",
                request.tes_banco);

            parametros.Add(
                "usuario",
                request.usuario
                    .Trim()
                    .ToUpperInvariant());

            return parametros;
        }

        /// <summary>
        /// Registra el movimiento del administrador
        /// en la bitácora del módulo de Inversiones.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza el movimiento.
        /// </param>
        /// <param name="movimiento">
        /// Tipo de movimiento.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        private void
            IVR_Cat_Adminsitradores_Bitacora_Registrar(
                int codEmpresa,
                string usuario,
                string movimiento,
                string codAdministrador)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario
                        .Trim()
                        .ToUpperInvariant(),
                    Modulo = ModuloInversiones,
                    Movimiento = movimiento,
                    DetalleMovimiento =
                        $"Administrador: "
                        + codAdministrador.Trim()
                });
        }

        private sealed class
            IvrCatAdministradorBancoSpData
        {
            public int idx { get; set; }

            public string itmx { get; set; } =
                string.Empty;
        }
    }
}