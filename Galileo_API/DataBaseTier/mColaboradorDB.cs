using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class MColaboradorDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb DBBitacora;

        public MColaboradorDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => DBBitacora.Bitacora(data);

        public async Task<ErrorDto> spRH_Boleta_Pago_Email(int codEmpresa, RhBoletaDto request, string usuario, int modulo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // ✅ Parametrizado (sin injection)
                resp.Code = await connection.ExecuteAsync(
                    "spRH_Boleta_Pago_Email",
                    new
                    {
                        Nomina = request.Nomina,
                        NominaId = request.NominaId,
                        EmpleadoId = request.EmpleadoId
                    },
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Ok";

                var empleado = string.IsNullOrWhiteSpace(request.EmpleadoId) ? "TODOS" : request.EmpleadoId;

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpper(),
                    DetalleMovimiento = $"Envío de Boleta de Pago por Correo, Nom: {request.Nomina}, Nomina Id: {request.NominaId}, Empleado: {empleado}",
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

        public async Task<ErrorDto> spRH_Boleta_Aguinaldo_Email(int codEmpresa, RhBoletaDto request, string usuario, int modulo)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var resp = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // ✅ Parametrizado (sin injection)
                resp.Code = await connection.ExecuteAsync(
                    "spRH_Boleta_Aguinaldo_Email",
                    new
                    {
                        Nomina = request.Nomina,
                        PeriodoId = request.PeriodoId,
                        EmpleadoId = request.EmpleadoId
                    },
                    commandType: CommandType.StoredProcedure);

                resp.Description = "Ok";

                var empleado = string.IsNullOrWhiteSpace(request.EmpleadoId) ? "TODOS" : request.EmpleadoId;

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpper(),
                    DetalleMovimiento = $"Envío de Boleta de Aguinaldo por Correo, Nom: {request.Nomina}, Periodo Id: {request.PeriodoId}, Empleado: {empleado}",
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

        public static string ConvierteMes(int numeroMes) =>
            numeroMes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => "Numero invalido"
            };
    }
}