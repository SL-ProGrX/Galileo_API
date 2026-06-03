using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX_Personas
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
        /// Obtiene la lista de autorizadores usando el SP y el parámetro EstadoAutorizado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="EstadoAutorizado"></param>
        /// <returns></returns>
        public ErrorDto<List<AfNatAutorizadores>> AF_NAT_Autorizadores_Obtener(int CodEmpresa, int EstadoAutorizado)
        {
            return EjecutarStoredProcedureList<AfNatAutorizadores>(
                CodEmpresa,
                "spAFI_Renuncia_NAT_Autorizadores_Obtener",
                new { SoloAutorizados = EstadoAutorizado });
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
            var result = EjecutarStoredProcedure(
                CodEmpresa,
                "spAFI_Renuncia_NAT_Autorizadores_Add",
                new { A_Usuario, Mov, Usuario },
                "Error al asignar autorizador NAT.");

            if (result.Code != 0)
            {
                return result;
            }

            RegistrarBitacora(
                CodEmpresa,
                Usuario,
                $"Usuario Autorizador para Renuncias con No Aumento de Tasas: {A_Usuario}",
                Mov == "A" ? "Registra - WEB" : "Elimina - WEB");

            return DbHelper.OkResponse("Ok");
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
