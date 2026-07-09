using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo.Models.Security;
using Galileo.DataBaseTier;

namespace Galileo_API.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCrNoAumentoTasasAutorizadoresDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFCrNoAumentoTasasAutorizadoresDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de autorizadores usando el SP sin parámetros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AfNatAutorizadores>> AF_NAT_Autorizadores_Obtener(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfNatAutorizadores>(
                    "spAFI_Renuncia_NAT_Autorizadores",
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? [])
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener autorizadores NAT.", result.Code.GetValueOrDefault(-1), new List<AfNatAutorizadores>());
        }

        /// <summary>
        /// Asigna los usuarios autorizados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="A_Usuario"></param>
        /// <param name="Mov"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_NAT_Autorizadores_Asignar(int CodEmpresa, string A_Usuario, string Mov, string Usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spAFI_Renuncia_NAT_Autorizadores_Add",
                    new { A_Usuario, Mov, Usuario },
                    commandType: System.Data.CommandType.StoredProcedure);
                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al asignar autorizador NAT.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacora(
                CodEmpresa,
                Usuario,
                $"Usuario Autorizador para Renuncias con No Aumento de Tasas: {A_Usuario}",
                Mov == "A" ? "Registra - WEB" : "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
