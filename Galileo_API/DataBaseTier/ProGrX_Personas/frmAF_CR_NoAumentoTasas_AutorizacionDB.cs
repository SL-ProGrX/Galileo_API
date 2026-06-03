using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrNoAumentoTasasAutorizacionDB
    {
        private readonly IConfiguration _config;

        public FrmAFCrNoAumentoTasasAutorizacionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de autorizaciones usando el SP y los parámetros del objeto Filtro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfNatAutorizacion>> AF_NAT_Autorizacion_Obtener(int CodEmpresa, AfNatAutorizacionFiltros Filtro)
        {
            if (Filtro is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de autorización NAT son requeridos.", -2, new List<AfNatAutorizacion>());
            }

            return EjecutarStoredProcedureList<AfNatAutorizacion>(
                CodEmpresa,
                "spAFI_Renuncias_NAT_Control_Consulta",
                new
                {
                    Inicio = Filtro.Inicio,
                    Corte = Filtro.Corte,
                    TUsuario = Filtro.TUsuario,
                    Filtro = Filtro.Filtro,
                    FUserReg = Filtro.FUserReg,
                    Usuario = Filtro.Usuario,
                    Autorizadas = Filtro.Autorizadas
                });
        }

        /// <summary>
        /// Autoriza renuncias con no aumento de tasas.
        /// </summary>
        /// <param name="CodEmpresa">C</param>
        /// <param name="RenunciaId"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_NAT_Autorizacion_Autorizar(int CodEmpresa, int RenunciaId, string Usuario)
        {
            return EjecutarStoredProcedure(
                CodEmpresa,
                "spAFI_Renuncia_NAT_Tag_Autoriza",
                new { RenunciaId, Usuario },
                "Error al autorizar renuncia con no aumento de tasas.");
        }

        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        private ErrorDto EjecutarStoredProcedure(int codEmpresa, string storedProcedure, object parameters, string errorMessage)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                connection.Execute(storedProcedure, parameters, commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
