using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo_API.DataBaseTier
{
    public class FrmCcActualizaDatosDb
    {
        private const int TiempoEsperaSegundos = 300;
        private const string ProcedimientoActualizaDatos = "spCRDActualizaDatos";

        private readonly PortalDB _portalDb;

        public FrmCcActualizaDatosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Ejecuta el proceso de actualizacion de datos relacionados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto CC_ActualizaDatos_Proceso_Ejecutar(int CodEmpresa)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo de empresa es requerido.",
                    -2);
            }

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Execute(
                        ProcedimientoActualizaDatos,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: TiempoEsperaSegundos);

                    return true;
                });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    $"Ocurri&oacute; un error al actualizar los datos relacionados. {resultado.Description}");
            }

            return DbHelper.OkResponse(
                "Proceso terminado satisfactoriamente.");
        }
    }
}