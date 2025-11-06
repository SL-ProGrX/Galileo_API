using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CR_NoAumentoTasas_AutorizacionDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_CR_NoAumentoTasas_AutorizacionDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de autorizaciones usando el SP y los parámetros del objeto Filtro.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<AfNatAutorizacion>> AF_NAT_Autorizacion_Obtener(int CodEmpresa, AfNatAutorizacionFiltros Filtro)
        {
            var result = new ErrorDto<List<AfNatAutorizacion>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfNatAutorizacion>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Si Inicio o Corte son null, se envía string vacío
                var parameters = new
                {
                    Inicio = Filtro.Inicio,
                    Corte = Filtro.Corte,
                    TUsuario = Filtro.TUsuario,
                    Filtro = Filtro.Filtro,
                    FUserReg = Filtro.FUserReg,
                    Usuario = Filtro.Usuario,
                    Autorizadas = Filtro.Autorizadas
                };

                result.Result = connection.Query<AfNatAutorizacion>(
                    "spAFI_Renuncias_NAT_Control_Consulta",
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
        /// Autoriza renuncias con no aumento de tasas.
        /// </summary>
        /// <param name="CodEmpresa">C</param>
        /// <param name="RenunciaId"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_NAT_Autorizacion_Autorizar(int CodEmpresa, int RenunciaId, string Usuario)
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
                var parameters = new { RenunciaId, Usuario };

                // Ejecuta el SP, no se espera resultado
                connection.Execute(
                    "spAFI_Renuncia_NAT_Tag_Autoriza",
                    parameters,
                    commandType: System.Data.CommandType.StoredProcedure
                );
                
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
