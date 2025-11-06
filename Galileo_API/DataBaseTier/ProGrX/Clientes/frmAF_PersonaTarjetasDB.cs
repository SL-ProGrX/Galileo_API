using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_PersonaTarjetasDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;
        private readonly mProGrx_Main _mMain;

        public frmAF_PersonaTarjetasDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new MSecurityMainDb(_config);
            _mMain = new mProGrx_Main(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener tarjetas registradas de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<PersonaTarjetaDto>> AF_PersonaTarjetas_Consulta(int CodEmpresa, string cedula)
        {
            var response = new ErrorDto<List<PersonaTarjetaDto>> { Code = 0, Result = new() };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                string sql = "exec spAFI_PersonaTarjetas_Consulta @Empresa, @Cedula, ''";
                response.Result = connection.Query<PersonaTarjetaDto>(sql, new { Empresa = CodEmpresa, Cedula = cedula }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Registrar tarjeta de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public ErrorDto AF_PersonaTarjetas_Registro(int CodEmpresa, PersonaTarjetaRegistroDto tarjeta)
        {
            var response = new ErrorDto { Code = 0, Description = "Procesado correctamente" };
            try
            {
                string movimiento = "";
                switch (tarjeta.TipoMov)
                {
                    case "A":                    
                        if (_mMain.FxTarjetaValida(tarjeta.Tarjeta) == false && tarjeta.ValidaTarjeta)
                        {
                            response.Code = -2;
                            response.Description = "Tarjeta no es valida";
                            return response;
                        }
                        movimiento = "Registra - WEB";
                        break;
                    case "E":
                        movimiento = "Elimina - WEB";
                        break;
                }
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);

                string sql = "exec spAFI_PersonaTarjetas_Registro @Empresa, @Cedula, @Tarjeta, @Vence, @Code, @TipoMov, @Usuario, ''";

                connection.Execute(sql, new
                {
                    Empresa = CodEmpresa,
                    Cedula = tarjeta.Cedula,
                    Tarjeta = tarjeta.Tarjeta,
                    Vence = tarjeta.Vence,
                    Code = tarjeta.Code,
                    TipoMov = tarjeta.TipoMov,
                    Usuario = tarjeta.Usuario
                });

                
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = tarjeta.Usuario.ToUpper(),
                    DetalleMovimiento = "Tarjeta: " + tarjeta.Tarjeta + " Id:" + tarjeta.Cedula,
                    Movimiento = movimiento,
                    Modulo = 9
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Valida tipo de tarjeta
        /// </summary>
        /// <param name="Tarjeta"></param>
        /// <returns></returns>
        public ErrorDto<string> AF_PersonaTarjetas_ValidaTipo(string Tarjeta)
        {
            var response = new ErrorDto<string> 
            {
                Code = 0, 
                Description = "Ok",
                Result = ""
            };

            try { 
                response.Result = _mMain.FxTarjetaTipo(Tarjeta);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
    }
}