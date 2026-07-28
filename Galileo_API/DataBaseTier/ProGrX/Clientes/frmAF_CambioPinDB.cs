using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Clientes;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;

namespace Galileo_API.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfCambioPinPinDB
    {
        private readonly PortalDB _portalDB;
        private readonly MAfilicacionDB _mAfilicacionDB;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly MProGrxMain _mProGrx;
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";

        public FrmAfCambioPinPinDB(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
            _mAfilicacionDB = new MAfilicacionDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
            _mProGrx = new MProGrxMain(config);
        }


        /// <summary>
        /// Metodo para obtener el valor de un parámetro específico relacionado con el cambio de PIN en la afiliación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCodigo"></param>
        /// <returns></returns>
        public ErrorDto<string> fxgAFIParametro(int CodEmpresa, string pCodigo)
        {
            try
            {
                var mTipo = _mAfilicacionDB.fxgAFIParametro(CodEmpresa, pCodigo);
                return DbHelper.CreateOkResponse(mTipo);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string>($"Error al obtener el parámetro: {ex.Message}", code: -1);
            } 
        }

        /// <summary>
        /// Obtiene Nombre y Email desde SOCIOS según la cédula (VB6: txtCedula_LostFocus).
        /// </summary>
        public ErrorDto<FrmAfCambioPinPersonaModel> Af_CambioPin_ObtenerPersona(int CodEmpresa, string cedula)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            RTRIM(nombre) AS Nombre,
                            ISNULL(af_email,'') AS Email
                        FROM socios
                        WHERE cedula = @Cedula";

                return conn.QueryFirstOrDefault<FrmAfCambioPinPersonaModel>(
                    query,
                    new { Cedula = cedula.Trim() }
                ) ?? new FrmAfCambioPinPersonaModel();
            });
        }


        /// <summary>
        /// Valida si el Ticket ya fue utilizado (VB6: fxTicketValida).
        /// Retorna true si NO ha sido utilizado.
        /// </summary>
        public ErrorDto fxTicketValida(int CodEmpresa, string ticket)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            if (string.IsNullOrWhiteSpace(ticket))
                return DbHelper.ErrorResponse("El ticket no puede estar vacío.");

            var ticketFormateado = ticket.PadRight(15, ' ');

            const string query = @"
                            SELECT COUNT(*) 
                            FROM AFI_BITACORA_ESPECIAL
                            WHERE MOVIMIENTO = '28'
                            AND SUBSTRING(detalle,9,16) = @Ticket";

            var existe = conn.ExecuteScalar<int>(query, new { Ticket = ticketFormateado });

            if(existe == 0)
            {
                return DbHelper.CreateOkResponse();
            }
            else
            {
                return DbHelper.ErrorResponse($"El ticket {ticket} ya ha sido utilizado.");
            }
        }


        /// <summary>
        /// Método para generar un PIN seguro de 4 dígitos utilizando RandomNumberGenerator.
        /// </summary>
        /// <returns></returns>
        public static string GenerarPinSeguro(int CodEmpresa)
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);

            int numero = (int)(BitConverter.ToUInt32(bytes, 0) % 10000);
            return numero.ToString("D4");
        }


        /// <summary>
        /// Registra en bitácora la generación del ticket para cambio de PIN de Autogestión (VB6: sbgAFIBitacora con movimiento "Aplica - WEB").
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="vTicket"></param>
        /// <returns></returns>
        public ErrorDto Af_CambioPin_Bitacora(int CodEmpresa,string usuario, string vTicket )
        {
            return _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = "Generación de Ticket para PIN de AutoGestión No.:" + vTicket,
                Movimiento = "Aplica - WEB",
                Modulo = 1
            });
        }


        /// <summary>
        /// Renueva la clave Web/App de Autogestión (VB6: spuProGrX_MOBILE_Persona_WebKey_Renueva)
        /// </summary>
        public ErrorDto Af_CambioPin_RenovarClaveWeb(
            int CodEmpresa,
            string cedula,
            string email,
            string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            const string sp = @"
                        exec spuProGrX_MOBILE_Persona_WebKey_Renueva 
                            @Cliente,
                            @Identificacion,
                            @Correo,
                            @Usuario,
                            @Token";

            try
            {
                conn.Execute(sp, new
                {
                    Cliente = CodEmpresa,
                    Identificacion = cedula.Trim(),
                    Correo = email.Trim(),
                    Usuario = usuario,
                    Token = "" // en VB6 enviaban vacío
                });

                return DbHelper.OkResponse("Clave de AutoGestion Renovada satisfactoriamente (Enviada por E-mail)");
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse("Error al renovar la clave de AutoGestion. Por favor, intente nuevamente o contacte al soporte.");
            }
        }


        /// <summary>
        /// Aplica cambio de PIN de Autogestión (VB6: CmdAplicar_Click - bloque Cambio PIN).
        /// Ejecuta:
        /// - spPersona_PIN_WebApp
        /// - update afi_parametros cod_parametro=13
        /// - sbgAFIBitacora movimiento 28
        /// - spSys_CORREO_POOL (si tipo == "E")
        /// - sbTrazabilidad_Inserta ("13")
        /// </summary>
        public ErrorDto Af_CambioPin_AplicarCambioPin(
            int CodEmpresa,
            FrmAfCambioPinAplicarModel model)
        {
            //Empresa
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            //Portal
            using var connPortal = new SqlConnection(_config.GetConnectionString(connectionStringName));

            try
            {
                var empresaNombre = _mProGrx.sbSifParametrosInicializa(CodEmpresa, model.usuario).Result.GNombreInstitucion;

                var mAsunto = $"{empresaNombre}: Cambio de PIN de Autogestión";

                // 1) Ejecuta SP para actualizar PIN, no se puede acceder a sifrado de PIN
                const string spPin = @"
                            exec spPersona_PIN_WebApp
                                @Cliente,
                                @Identificacion,
                                @Pin,
                                @Usuario";

                connPortal.Execute(spPin, new
                {
                    Cliente = CodEmpresa,
                    Identificacion = model.cedula.Trim(),
                    Pin = model.pinSeguro.Trim(),
                    Usuario = model.usuario
                });

                // 2) Update parámetro 13 (ticket)
                const string updateParametro13 = @"
            update afi_parametros
               set valor = @Ticket
             where cod_parametro = '13'";

                conn.Execute(updateParametro13, new { Ticket = model.ticket.Trim() });

                // 3) Bitácora especial (Movimiento 28) con ticket formateado como VB6
                var ticketFormateado = model.ticket.Trim().PadRight(15, ' ');
                var detalleBitacora = $"Ticket: {ticketFormateado} PIN AutoGestión Renovado (Tipo:{model.tipo})";

                // En VB6: sbgAFIBitacora("28", vDetalle, cedula)
                _mAfilicacionDB.sbgAFIBitacora(CodEmpresa, "28", detalleBitacora, model.cedula.Trim(), model.usuario);

                // 4) Notifica por correo si tipo == "E"
                if (model.tipo == "E")
                {
                    var cuerpoHtml =
                        "<html><body><div class=WordSection1>" +
                        "<p class=MsoNormal>Se ha cambiado su PIN para el APP de autogestión. Ahora " +
                        $"puede ingresar con el PIN: <b>{model.pinPlano}</b></p>" +
                        "</div></body></html>";

                    const string spCorreo = @"
                exec spSys_CORREO_POOL
                    @DETALLE,
                    @ASUNTO,
                    'P',
                    @PARA";

                    conn.Execute(spCorreo, new
                    {
                        DETALLE = cuerpoHtml,
                        ASUNTO = mAsunto,
                        PARA = model.email.Trim()
                    });
                }

                // 5) Trazabilidad (VB6: sbTrazabilidad_Inserta("13", ticket, txtTicket))

                _mProGrx.sbTrazabilidad_Inserta(CodEmpresa, model.ticket.Trim(),
                    model.ticket.Trim(), "", model.usuario.Trim());

                var msj = "";
                switch (model.tipo)
                {
                    case "I": //Impreso
                        msj = "Pin de AutoGestion (Impresión de Seguridad)";
                        break;
                    case "T"://Talonario
                        msj = "Pin de AutoGestion Renovado satisfactoriamente (Registro en Talonario)";
                        break;
                    case "E":
                        msj = "Pin de AutoGestion Renovado satisfactoriamente (Enviado por E-mail)";
                        break;
                }

                return DbHelper.OkResponse(msj);
            }
            catch(Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al aplicar el cambio de PIN: {ex.Message}");
            }

            
        }
    }
}
