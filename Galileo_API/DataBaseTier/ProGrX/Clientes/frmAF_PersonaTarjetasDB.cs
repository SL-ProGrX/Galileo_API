using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmAFPersonaTarjetasDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _mSecurity;

        private const string SpPersonaTarjetasConsulta = "spAFI_PersonaTarjetas_Consulta";
        private const string SpPersonaTarjetasRegistro = "spAFI_PersonaTarjetas_Registro";

        public FrmAFPersonaTarjetasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mSecurity = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtiene las tarjetas registradas de una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula de la persona.</param>
        /// <returns>Listado de tarjetas registradas.</returns>
        public ErrorDto<List<PersonaTarjetaDto>> AF_PersonaTarjetas_Consulta(int CodEmpresa, string cedula)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<PersonaTarjetaDto>(
                    SpPersonaTarjetasConsulta,
                    new
                    {
                        ClienteCod = CodEmpresa,
                        Cedula = NormalizarTexto(cedula),
                        Token = string.Empty
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList());
        }


        /// <summary>
        /// Registra o elimina una tarjeta de una persona.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="tarjeta">Datos de la tarjeta.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_PersonaTarjetas_Registro(int CodEmpresa, PersonaTarjetaRegistroDto tarjeta)
        {
            if (tarjeta is null)
            {
                return DbHelper.ErrorResponse("Los datos de la tarjeta son requeridos.", -2);
            }

            var movimiento = ObtenerMovimiento(tarjeta.tipoMov);
            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.ErrorResponse("El tipo de movimiento no es válido.", -2);
            }

            if (DebeValidarTarjeta(tarjeta) && !MProGrxMain.FxTarjetaValida(tarjeta.tarjeta))
            {
                return DbHelper.ErrorResponse("Tarjeta no es valida", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    SpPersonaTarjetasRegistro,
                    CrearParametrosRegistro(CodEmpresa, tarjeta),
                    commandType: System.Data.CommandType.StoredProcedure);

                return true;
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al registrar tarjeta.", result.Code.GetValueOrDefault(-1));
            }

            RegistrarBitacoraTarjeta(CodEmpresa, tarjeta, movimiento);
            return DbHelper.OkResponse("Procesado correctamente");
        }


        /// <summary>
        /// Valida y obtiene el tipo de tarjeta.
        /// </summary>
        /// <param name="Tarjeta">Número de tarjeta.</param>
        /// <returns>Tipo de tarjeta identificado.</returns>
        public static ErrorDto<string> AF_PersonaTarjetas_ValidaTipo(string Tarjeta)
        {
            var response = DbHelper.CreateOkResponse(string.Empty);

            try
            {
                response.Result = MProGrxMain.FxTarjetaTipo(NormalizarTexto(Tarjeta));
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
        /// Crea parámetros seguros para registrar o eliminar una tarjeta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        private static object CrearParametrosRegistro(int codEmpresa, PersonaTarjetaRegistroDto tarjeta)
        {
            return new
            {
                ClienteCod = codEmpresa,
                Cedula = NormalizarTexto(tarjeta.cedula),
                Tarjeta = NormalizarTexto(tarjeta.tarjeta),
                Vence = tarjeta.vence,
                Code = NormalizarTexto(tarjeta.code),
                TipoMov = NormalizarTexto(tarjeta.tipoMov).ToUpperInvariant(),
                Usuario = NormalizarTexto(tarjeta.usuario),
                Token = NormalizarTexto(tarjeta.dia_apl_ca)
            };
        }


        /// <summary>
        /// Obtiene el texto de bitácora según el tipo de movimiento.
        /// </summary>
        private static string ObtenerMovimiento(string? tipoMov)
        {
            return NormalizarTexto(tipoMov).ToUpperInvariant() switch
            {
                "A" => "Registra - WEB",
                "E" => "Elimina - WEB",
                _ => string.Empty
            };
        }


        /// <summary>
        /// Indica si la tarjeta debe ser validada antes de registrar.
        /// </summary>
        private static bool DebeValidarTarjeta(PersonaTarjetaRegistroDto tarjeta)
        {
            return string.Equals(NormalizarTexto(tarjeta.tipoMov), "A", StringComparison.OrdinalIgnoreCase)
                && tarjeta.validaTarjeta;
        }


        /// <summary>
        /// Registra en bitácora el movimiento de tarjeta.
        /// </summary>
        private void RegistrarBitacoraTarjeta(int codEmpresa, PersonaTarjetaRegistroDto tarjeta, string movimiento)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(tarjeta.usuario).ToUpperInvariant(),
                DetalleMovimiento = $"Tarjeta: {NormalizarTexto(tarjeta.tarjeta)} Id:{NormalizarTexto(tarjeta.cedula)}",
                Movimiento = movimiento,
                Modulo = 9
            });
        }


        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);


        /// <summary>
        /// Normaliza valores de texto recibidos desde formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}