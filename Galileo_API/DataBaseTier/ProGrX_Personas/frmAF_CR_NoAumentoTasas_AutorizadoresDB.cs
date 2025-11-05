using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;


namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CR_NoAumentoTasas_AutorizadoresDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_CR_NoAumentoTasas_AutorizadoresDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de autorizadores usando el SP y el parámetro EstadoAutorizado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="EstadoAutorizado"></param>
        /// <returns></returns>
        public ErrorDto<List<AF_NAT_Autorizadores>> AF_NAT_Autorizadores_Obtener(int CodEmpresa, int EstadoAutorizado)
        {
            var result = new ErrorDto<List<AF_NAT_Autorizadores>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AF_NAT_Autorizadores>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var parameters = new { SoloAutorizados = EstadoAutorizado };

                result.Result = connection.Query<AF_NAT_Autorizadores>(
                    "spAFI_Renuncia_NAT_Autorizadores_Obtener",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();
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
        /// Asigna los usuarios autorizados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="A_Usuario"></param>
        /// <param name="Mov"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_NAT_Autorizadores_Asignar(int CodEmpresa, string A_Usuario, string Mov, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var parameters = new { A_Usuario, Mov, Usuario };

                // Ejecuta el SP, no se espera resultado
                connection.Execute(
                    "spAFI_Renuncia_NAT_Autorizadores_Add",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );

                // Determina el tipo de movimiento para la bitácora
                string movimiento = Mov == "A" ? "Registra - WEB" : "Elimina - WEB";

                // Guarda en bitácora               
                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = Usuario,
                    DetalleMovimiento = $"Usuario Autorizador para Renuncias con No Aumento de Tasas: {A_Usuario}",
                    Movimiento = movimiento,
                    Modulo = vModulo 
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
