using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfActaAfiliacionDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1; //Modulo de clientes
        private readonly MSecurityMainDb _Security_MainDB;

        private const string SqlActaAfiliacionParametro = @"
                    SELECT TOP 1 ISNULL(Nacta, 0)
                    FROM dbo.Par_AfAh;";

        private const string SqlUpdateActaAfiliacionParametro = @"
                    UPDATE dbo.Par_AfAh
                    SET Nacta = @NuevoNacta;";

        private const string SqlFechaServidor = "SELECT GETDATE();";

        private const string SqlUpdateSociosActaAfiliacion = @"
                    UPDATE dbo.Socios
                    SET EstadoActa = 'I',
                        Nacta = @NuevoNacta,
                        FecActa = @FechaServidor
                    WHERE EstadoActa = 'P'
                      AND EstadoActual = 'S';";

        public FrmAfActaAfiliacionDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene y registra el siguiente número de acta de afiliación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que genera el acta.</param>
        /// <returns>Número de acta generado.</returns>
        public ErrorDto<long> AF_ActaAfiliacio_Obtener(int CodEmpresa, string usuario)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var actaActual = connection.QueryFirstOrDefault<long?>(SqlActaAfiliacionParametro);
                if (!actaActual.HasValue)
                {
                    return 0L;
                }

                var nuevoNacta = CalcularSiguienteActa(actaActual.Value);
                var fechaServidor = connection.ExecuteScalar<DateTime>(SqlFechaServidor);

                connection.Execute(SqlUpdateActaAfiliacionParametro, new { NuevoNacta = nuevoNacta });
                connection.Execute(SqlUpdateSociosActaAfiliacion, new
                {
                    NuevoNacta = nuevoNacta,
                    FechaServidor = fechaServidor
                });

                RegistrarBitacoraActa(CodEmpresa, usuario, nuevoNacta);

                return nuevoNacta;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al generar acta de afiliación.",
                    result.Code.GetValueOrDefault(-1),
                    0L);
            }

            if (result.Result == 0)
            {
                return DbHelper.CreateErrorResponse("No se encontró registro en Par_AfAh.", -1, 0L);
            }

            return DbHelper.CreateOkResponse(result.Result);
        }
        
        
        /// <summary>
        /// Calcula el siguiente número de acta con base en el valor actual.
        /// </summary>
        /// <param name="actaActual">Número actual de acta.</param>
        /// <returns>Siguiente número de acta.</returns>
        private static long CalcularSiguienteActa(long actaActual) => actaActual == 0 ? 1 : actaActual + 1;


        /// <summary>
        /// Registra en bitácora la generación del acta de afiliación.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que genera el acta.</param>
        /// <param name="nuevoNacta">Número de acta generado.</param>
        private void RegistrarBitacoraActa(int codEmpresa, string usuario, long nuevoNacta)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                Modulo = vModulo,
                Movimiento = "Genera - Web",
                DetalleMovimiento = $"Acta de afiliación número :  {nuevoNacta}."
            });
        }


        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}