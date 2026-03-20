using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public class MSecurityMainDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public MSecurityMainDb(IConfiguration config)
        {
            _config = config;
        }

        public int Derecho(ParametrosAccesoDto req)
        {
            int resp = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {

                    var procedure = "[spSEG_Access]";

                    var values = new
                    {
                        Cliente = req.EmpresaId,
                        Usuario = req.Usuario,
                        Modulo = req.Modulo,
                        FormX = req.FormName,
                        Opcion = req.Boton
                    };

                    resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Inserta un registro en bitácora usando valores validados y normalizados.
        /// </summary>
        public ErrorDto Bitacora(BitacoraInsertarDto req)
        {
            ErrorDto resp = new ErrorDto();

            try
            {
                var empresaIdSeguro = NormalizarCodEmpresa((int)req.EmpresaId);
                var usuarioSeguro = NormalizarTextoCorto(req.Usuario, 50, "Usuario");
                var moduloSeguro = NormalizarTextoCorto(req.Modulo.ToString(), 5, "Modulo");
                var movimientoSeguro = NormalizarTextoCorto(req.Movimiento, 100, "Movimiento").ToUpperInvariant();
                var detalleSeguro = NormalizarDetalleMovimiento(req.DetalleMovimiento, 500);

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    connection.Open();

                    var strSQL = @"INSERT INTO US_Bitacora
                           (Cod_Empresa, Usuario, Fecha_Hora, Modulo, Movimiento, Detalle, APP_NOMBRE)
                           VALUES
                           (@Cod_Empresa, @Usuario, @Fecha_Hora, @Modulo, @Movimiento, @Detalle, @APP_NOMBRE)";

                    var parameters = new
                    {
                        Cod_Empresa = empresaIdSeguro,
                        Usuario = usuarioSeguro,
                        Fecha_Hora = DateTime.Now,
                        Modulo = moduloSeguro,
                        Movimiento = movimientoSeguro,
                        Detalle = detalleSeguro,
                        APP_NOMBRE = "ProGrX_WEB"
                    };

                    resp.Code = connection.Execute(strSQL, parameters);
                    resp.Description = "Ok";
                }
            }
            catch (SecurityException ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto SbSEGCuentaLog(SegLogInsertarDto req)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSEG_Log";

                    var values = new
                    {
                        AppName = req.AppName,
                        AppVersion = req.AppVersion,
                        Usuario = req.Usuario,
                        PTransac = req.PTransac,
                        PNotas = req.PNotas.Substring(0, Math.Min(500, req.PNotas.Length)),
                        PUserMov = req.PUserMov,
                        AppMaquina = req.AppMaquina
                    };

                    resp.Code = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public int DerechoMDI(DerechoMdiObtenerDto req)
        {
            int resp = 0;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "spSEG_Access";

                    var values = new
                    {
                        Cliente = req.Cliente,
                        Usuario = req.Usuario,
                        Modulo = req.Modulo,
                        FormX = req.FormX,
                        Opcion = req.Opcion
                    };

                     resp = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            }
            catch (Exception)
            {
                resp = -1;
            }
            return resp;
        }

        private static readonly Regex SafeTextRegex = new(
    @"^[\p{L}\p{N}\s_\-\.@:/#(),]+$",
    RegexOptions.Compiled,
    TimeSpan.FromMilliseconds(250));

        /// <summary>
        /// Valida y normaliza el código de empresa antes de usarlo en operaciones de bitácora.
        /// </summary>
        private static int NormalizarCodEmpresa(int empresaId)
        {
            if (empresaId <= 0 || empresaId > 999999)
            {
                throw new SecurityException("El código de empresa no es válido.");
            }

            return empresaId;
        }

        /// <summary>
        /// Valida y normaliza textos cortos de entrada para bitácora.
        /// </summary>
        private static string NormalizarTextoCorto(string? valor, int longitudMaxima, string nombreCampo)
        {
            var texto = (valor ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                throw new SecurityException($"El campo {nombreCampo} es requerido.");
            }

            if (texto.Length > longitudMaxima)
            {
                texto = texto.Substring(0, longitudMaxima);
            }

            if (!SafeTextRegex.IsMatch(texto))
            {
                throw new SecurityException($"El campo {nombreCampo} contiene caracteres no permitidos.");
            }

            return texto;
        }

        /// <summary>
        /// Normaliza texto libre para detalle de bitácora, limita longitud y elimina caracteres de control.
        /// </summary>
        private static string NormalizarDetalleMovimiento(string? detalleMovimiento, int longitudMaxima)
        {
            var texto = (detalleMovimiento ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            texto = texto.Replace("\r", " ").Replace("\n", " ").Trim();

            if (texto.Length > longitudMaxima)
            {
                texto = texto.Substring(0, longitudMaxima);
            }

            return texto;
        }

    }
}
