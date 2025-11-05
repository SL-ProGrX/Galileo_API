using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_CRAutorizacionesDB
    {
        private readonly IConfiguration? _config;

        public frmAF_CRAutorizacionesDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de autorizaciones según filtros de fecha y estado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_CRAutorizacion>> AF_CRAutorizaciones_Obtener(int CodEmpresa, AF_CRAutorizacionFiltros filtros)
        {
            var result = new ErrorDTO<List<AF_CRAutorizacion>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_CRAutorizacion>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string query = @"
                    SELECT R.*, S.Nombre, ISNULL(R.autorizado_estado, 0) AS AutorizacionX
                    FROM afi_cr_renuncias R
                    INNER JOIN Socios S ON R.cedula = S.cedula
                    WHERE R.resuelto_fecha BETWEEN @Inicio AND @Corte
                      AND Estado = 'R'";

                // Filtros adicionales según EstadoAutorizacion
                if (filtros.EstadoAutorizacion == "A")
                    query += " AND R.autorizado_estado = 1";
                else if (filtros.EstadoAutorizacion == "P")
                    query += " AND ISNULL(R.autorizado_estado, 0) = 0";
                else if (filtros.EstadoAutorizacion == "D")
                    query += " AND R.autorizado_estado = 2";

                var parameters = new
                {
                    Inicio = filtros.Inicio,
                    Corte = filtros.Corte
                };

                result.Result = connection.Query<AF_CRAutorizacion>(query, parameters).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Autoriza una renuncia, actualizando el estado y las notas de autorización.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRenuncia"></param>
        /// <param name="Observaciones"></param>
        /// <param name="pAutoriza"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_CRAutorizaciones_Autorizar(int CodEmpresa, int CodRenuncia, string Observaciones, int pAutoriza, string Usuario)
        {
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = @"
                    UPDATE afi_cr_renuncias
                    SET autoriza_notas = @Observaciones,
                        Autorizado_Estado = @pAutoriza,
                        Autorizado_Fecha = dbo.MyGetdate(),
                        Autorizado_Usuario = @Usuario
                    WHERE cod_renuncia = @CodRenuncia";

                connection.Execute(query, new
                {
                    Observaciones = Observaciones?.ToUpper() ?? "",
                    pAutoriza,
                    Usuario,
                    CodRenuncia
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
