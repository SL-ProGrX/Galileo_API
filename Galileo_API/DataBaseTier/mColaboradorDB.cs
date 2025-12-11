using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class MColaboradorDB
    {
        private readonly IConfiguration _config;
        readonly MSecurityMainDb DBBitacora;

        public MColaboradorDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto spRH_Boleta_Pago_Email(int CodEmpresa, RhBoletaDto request, string usuario, int modulo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                
                    var query = $@"exec spRH_Boleta_Pago_Email '{request.Nomina}', {request.NominaId}, '{request.EmpleadoId}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Ok";
                

                if (request.EmpleadoId == "")
                {
                    request.EmpleadoId = "TODOS";
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = "Env�o de Boleta de Pago por Correo, Nom: " + request.Nomina + ", Nomina Id: " + request.NominaId + ", Empleado: " + request.EmpleadoId,
                    Movimiento = "APLICA - WEB",
                    Modulo = modulo
                });

            }
            catch (Exception ex)
            {
                resp.Code = 1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        public ErrorDto spRH_Boleta_Aguinaldo_Email(int CodEmpresa, RhBoletaDto request, string usuario, int modulo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                
                    var query = $@"exec spRH_Boleta_Aguinaldo_Email '{request.Nomina}', {request.PeriodoId}, '{request.EmpleadoId}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Ok";
                

                if (request.EmpleadoId == "")
                {
                    request.EmpleadoId = "TODOS";
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = "Env�o de Boleta de Aguinaldo por Correo, Nom: " + request.Nomina + ", Periodo Id: " + request.PeriodoId + ", Empleado: " + request.EmpleadoId,
                    Movimiento = "APLICA - WEB",
                    Modulo = modulo
                });

            }
            catch (Exception ex)
            {
                resp.Code = 1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public static string ConvierteMes(int numeroMes)
        {
            string nombreMes;

            switch (numeroMes)
            {
                case 1:
                    nombreMes = "Enero";
                    break;
                case 2:
                    nombreMes = "Febrero";
                    break;
                case 3:
                    nombreMes = "Marzo";
                    break;
                case 4:
                    nombreMes = "Abril";
                    break;
                case 5:
                    nombreMes = "Mayo";
                    break;
                case 6:
                    nombreMes = "Junio";
                    break;
                case 7:
                    nombreMes = "Julio";
                    break;
                case 8:
                    nombreMes = "Agosto";
                    break;
                case 9:
                    nombreMes = "Septiembre";
                    break;
                case 10:
                    nombreMes = "Octubre";
                    break;
                case 11:
                    nombreMes = "Noviembre";
                    break;
                case 12:
                    nombreMes = "Diciembre";
                    break;
                default:
                    nombreMes = "Numero invalido";
                    break;
            }

            return nombreMes;
        }
    }
}