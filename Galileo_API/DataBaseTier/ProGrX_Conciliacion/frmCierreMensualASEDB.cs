using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX_Conciliacion
{
    public sealed class FrmCierreMensualAseDb
    {
        private const int TiempoEsperaSegundos = 900;

        private const string SqlCierreMensual = """
            set nocount on;

            declare @anio int = year(getdate());
            declare @mes int = month(getdate());

            exec dbo.spSIFAuxMain
                @anio,
                @mes,
                @usuario;
            """;

        private readonly PortalDB _portalDb;

        public FrmCierreMensualAseDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Ejecuta el cierre mensual de los auxiliares de cuentas corrientes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto
            Conciliacion_CierreMensualASE_Cierre_Ejecutar(
                int codEmpresa, string usuario)
        {
            usuario = usuario?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "No se pudo determinar el usuario que ejecutar&aacute; el cierre mensual.",
                    -2);
            }

            ErrorDto<bool> resultado =
                DbHelper.WithConn(
                    _portalDb,
                    codEmpresa,
                    connection =>
                    {
                        connection.Execute(
                            SqlCierreMensual,
                            new
                            {
                                usuario
                            },
                            commandTimeout:
                                TiempoEsperaSegundos);

                        return true;
                    });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    "Ocurri&oacute; un error al ejecutar el cierre mensual.",
                    -1);
            }

            return DbHelper.OkResponse(
                "Cierre concluido satisfactoriamente.");
        }
    }
}