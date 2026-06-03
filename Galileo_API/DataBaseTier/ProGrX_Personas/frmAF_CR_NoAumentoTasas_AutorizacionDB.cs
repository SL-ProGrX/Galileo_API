using Dapper;
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

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfNatAutorizacion>(
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
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfNatAutorizacion>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar consulta de autorización NAT.", result.Code.GetValueOrDefault(-1), new List<AfNatAutorizacion>());
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
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spAFI_Renuncia_NAT_Tag_Autoriza",
                    new { RenunciaId, Usuario },
                    commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al autorizar renuncia con no aumento de tasas.", result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
