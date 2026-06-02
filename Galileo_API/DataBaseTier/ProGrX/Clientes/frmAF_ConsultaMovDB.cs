using System.Data;
using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmAfConsultaMovDb
    {
        private readonly IConfiguration _config;

        private const string SpConsultaMovIngresos = "spAFI_ConsultaMovIngresos";
        private const string SpConsultaMovRenuncias = "spAFI_ConsultaMovRenuncias";
        private const string SpConsultaMovLiquidaciones = "spAFI_ConsultaMovLiquidaciones";
        private const string SpLiquidacionReversaValidacion = "spAFI_Liquidacion_Reversa_Validacion";
        private const string SpLiquidacionReversa = "spAFI_Liquidacion_Reversa";

        private const string SqlFechaServidor = "SELECT dbo.MyGetdate() AS Fecha;";

        public FrmAfConsultaMovDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los movimientos de ingresos asociados a una persona.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Listado de movimientos de ingresos.</returns>
        public ErrorDto<List<AfiConsultaMovIngresos>> ConsultaMovIngresos_Obtener(int CodCliente, string cedula)
        {
            return EjecutarConsultaMovimiento<AfiConsultaMovIngresos>(
                CodCliente,
                SpConsultaMovIngresos,
                cedula,
                nameof(ConsultaMovIngresos_Obtener));
        }


        /// <summary>
        /// Obtiene los movimientos de renuncias asociados a una persona.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Listado de movimientos de renuncias.</returns>
        public ErrorDto<List<AfiConsultaMovRenuncias>> ConsultaMovRenuncias_Obtener(int CodCliente, string cedula)
        {
            return EjecutarConsultaMovimiento<AfiConsultaMovRenuncias>(
                CodCliente,
                SpConsultaMovRenuncias,
                cedula,
                nameof(ConsultaMovRenuncias_Obtener));
        }


        /// <summary>
        /// Obtiene los movimientos de liquidaciones asociados a una persona.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Listado de movimientos de liquidaciones.</returns>
        public ErrorDto<List<AfiConsultaMovLiquidaciones>> ConsultaMovLiquidaciones_Obtener(int CodCliente, string cedula)
        {
            return EjecutarConsultaMovimiento<AfiConsultaMovLiquidaciones>(
                CodCliente,
                SpConsultaMovLiquidaciones,
                cedula,
                nameof(ConsultaMovLiquidaciones_Obtener));
        }


        /// <summary>
        /// Aplica la reversión de una liquidación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la reversión.</param>
        /// <param name="idLiquidacion">Identificador de liquidación.</param>
        /// <param name="cedula">Cédula de la persona asociada a la liquidación.</param>
        /// <returns>Resultado de la reversión.</returns>
        public ErrorDto AF_MovLiquidaciones_Reversion(int CodEmpresa, string usuario, string idLiquidacion, string cedula)
        {
            var response = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var liquidacion = NormalizarTexto(idLiquidacion);
                var usuarioNormalizado = NormalizarTexto(usuario);

                var mensajeValidacion = connection.QueryFirstOrDefault<string>(
                    $"EXEC {SpLiquidacionReversaValidacion} @IdLiquidacion",
                    new { IdLiquidacion = liquidacion });

                if (!string.IsNullOrWhiteSpace(mensajeValidacion))
                {
                    return mensajeValidacion;
                }

                connection.Execute(
                    $"EXEC {SpLiquidacionReversa} @IdLiquidacion, @Usuario",
                    new
                    {
                        IdLiquidacion = liquidacion,
                        Usuario = usuarioNormalizado
                    });

                RegistrarBitacoraReversion(
                    CodEmpresa,
                    usuarioNormalizado,
                    $"Reversa Liquidación # {liquidacion} - Ced:{NormalizarTexto(cedula)}");

                return string.Empty;
            });

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ?? "Error al revertir liquidación.",
                    response.Code.GetValueOrDefault(-1));
            }

            if (!string.IsNullOrWhiteSpace(response.Result))
            {
                return DbHelper.ErrorResponse(response.Result, -2);
            }

            return DbHelper.OkResponse("Reversión realizada satisfactoriamente.");
        }


        /// <summary>
        /// Obtiene la fecha actual del servidor de base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Fecha y hora actual del servidor.</returns>
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QuerySingle<DateTime>(SqlFechaServidor));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al obtener fecha del servidor.",
                    result.Code.GetValueOrDefault(-1));
            }

            return DbHelper.OkResponse(result.Result.ToString("yyyy-MM-dd HH:mm:ss"));
        }


        /// <summary>
        /// Ejecuta una consulta de movimientos por procedimiento almacenado.
        /// </summary>
        /// <typeparam name="T">Tipo de resultado esperado.</typeparam>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="storedProcedure">Procedimiento almacenado a ejecutar.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <param name="origen">Nombre del método origen para mensajes de error.</param>
        /// <returns>Listado de movimientos asociados.</returns>
        private ErrorDto<List<T>> EjecutarConsultaMovimiento<T>(
            int codEmpresa,
            string storedProcedure,
            string cedula,
            string origen)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(
                    storedProcedure,
                    new { Cedula = NormalizarTexto(cedula) },
                    commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(
                    $"{origen} - {result.Description}",
                    result.Code.GetValueOrDefault(-1),
                    new List<T>());
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        /// <summary>
        /// Registra en bitácora la reversión de una liquidación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la reversión.</param>
        /// <param name="detalle">Detalle del movimiento registrado.</param>
        private void RegistrarBitacoraReversion(int codEmpresa, string usuario, string detalle)
        {
            var securityDb = new MSecurityMainDb(_config);
            securityDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Modulo = 1,
                Movimiento = "Reversa",
                DetalleMovimiento = detalle
            });
        }

    }
}
