using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.Data.SqlClient;
using System.Data;
using System.ServiceModel.Channels;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{

    public class FrmPreaNotificacionDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCreditos = 3;


        public FrmPreaNotificacionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Carga la información inicial de frmPrea_Notificacion para que Angular
        /// solo pinte el modal con estado, montos y datos de contacto.
        /// </summary>
        public ErrorDto<FrmPreaNotificacionCargarResponse> Prea_frmPreaNotificacion_Cargar(
            int codEmpresa,
            FrmPreaNotificacionCargarRequest request)
        {
            var response = new ErrorDto<FrmPreaNotificacionCargarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaNotificacionCargarResponse()
            };

            try
            {
                using var conn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));
                conn.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@cod_preanalisis", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@id_solicitud", request.id_solicitud, DbType.Int64);
                parameters.Add("@cedula", request.cedula?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

                // Aquí puedes resolverlo primero con SQL directo o luego moverlo a SP.
                // Nombre sugerido del SP:
                // spPrea_frmPreaNotificacion_Cargar
                response.Result = conn.QueryFirstOrDefault<FrmPreaNotificacionCargarResponse>(
                    "spPrea_frmPreaNotificacion_Cargar",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaNotificacionCargarResponse();

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    response.Result);
            }
        }

        /// <summary>
        /// Ejecuta la lógica completa de notificación de frmPrea_Notificacion:
        /// valida permisos, determina plantilla, registra cola de envío y bitácora.
        /// </summary>
        public ErrorDto<FrmPreaNotificacionEnviarResponse> Prea_frmPreaNotificacion_Notificar(
            int codEmpresa,
            FrmPreaNotificacionEnviarRequest request)
        {
            var response = new ErrorDto<FrmPreaNotificacionEnviarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaNotificacionEnviarResponse()
            };

            try
            {
                using var conn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));
                conn.Open();

                using var tx = conn.BeginTransaction();

                var info = ObtenerDatosNotificacion(conn, tx, request);
                var datosPlantilla = ConstruirDatosPlantilla(info, request);
                var permiso = ValidarUsuarioEnviaNotificacion(conn, tx, request.usuario, datosPlantilla.estado_codigo);

                if (permiso == "NOAP")
                {
                    tx.Rollback();
                    response.Code = -1;
                    response.Description = "Favor validar la configuración de mensajes, correo electrónico o teléfono registrado.";
                    response.Result.resultado_notificacion = permiso;
                    return response;
                }

                if (permiso == "OFNC")
                {
                    tx.Rollback();
                    response.Code = -1;
                    response.Description = "La oficina a la que pertenece el usuario no está autorizada para el envío de notificaciones.";
                    response.Result.resultado_notificacion = permiso;
                    return response;
                }

                var tipoPlantilla = ObtenerTipoPlantilla(datosPlantilla.estado, request.monto_sugerido);
                var plantillaCorreo = ObtenerPlantillaCorreo(conn, tx, tipoPlantilla);
                /**
                 *  * Se debe validar este dato en el flujo con la integracion a frmPreaEstudiov2
                 * plantillaMensaje = ObtenerPlantillaMensaje(conn, tx, tipoPlantilla);
                **/
                var plantillaSms = ObtenerPlantillaMensajeSms(conn, tx, tipoPlantilla);

               var cuerpoCorreo = CompletarPlantilla(plantillaCorreo.plantilla, datosPlantilla);
                /**
               * Se debe validar este dato en el flujo con la integracion a frmPreaEstudiov2
               * var cuerpoMensaje = CompletarPlantilla(plantillaMensaje.mensaje, datosPlantilla);
               **/
                var cuerpoSms = CompletarPlantilla(plantillaSms.mensaje_sms, datosPlantilla);

                var envioCorreo = permiso is "NMYC" or "NSCO";
                var envioSms = permiso is "NMYC" or "NSMJ";

                if (envioCorreo)
                {
                    RegistrarColaNotificacion(conn, tx,
                        new RegistrarColaNotificacionRequest
                        {
                            mensaje = cuerpoCorreo,
                            asunto = plantillaCorreo.asunto,
                            tipoEnvio = "ECO",
                            correo = info.correo,
                            usuario = request.usuario,
                            celular = string.Empty
                        });
                }

                if (envioSms)
                {
                    RegistrarColaNotificacion(conn, tx, new RegistrarColaNotificacionRequest
                    {
                        mensaje = cuerpoSms,
                        asunto = plantillaCorreo.asunto,
                        tipoEnvio = "EMJ",
                        correo = info.correo,
                        usuario = request.usuario,
                        celular = info.celular
                    });
                }

                RegistrarBitacoraCorreo(conn, tx, info,
                    new RegistrarBitacoraCorreoRequest
                    {
                        mensajeCorreo = cuerpoCorreo,
                        mensajeSms = cuerpoSms,
                        envioCorreo = envioCorreo,
                        envioSms = envioSms,
                        usuario = request.usuario
                    });
                RegistrarBitacoraSif(codEmpresa, request.usuario, info, plantillaCorreo.asunto);

                tx.Commit();

                response.Result.resultado_notificacion = permiso;
                response.Result.tipo_plantilla = tipoPlantilla;
                response.Result.estado_envio = datosPlantilla.estado_codigo;
                response.Result.envio_correo = envioCorreo;
                response.Result.envio_sms = envioSms;
                response.Result.correo = info.correo;
                response.Result.celular = info.celular;
                response.Result.mensaje = "Se ha realizado la acción correctamente.";

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaNotificacionEnviarResponse();
                return response;
            }
        }

        private static string ObtenerTipoPlantilla(string estado, decimal montoSugerido)
        {
            if (estado.Trim().Equals("Denegado", StringComparison.OrdinalIgnoreCase))
                return "DENEGA";

            if (estado.Trim().Equals("Aprobado", StringComparison.OrdinalIgnoreCase) && montoSugerido > 0)
                return "APROCO";

            return "APROSO";
        }

        #region Helpers

        private static FrmPreaNotificacionInfoInterna ObtenerDatosNotificacion(
            SqlConnection conn,
            SqlTransaction tx,
            FrmPreaNotificacionEnviarRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@cod_preanalisis", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@id_solicitud", request.id_solicitud, DbType.Int64);
            parameters.Add("@cedula", request.cedula?.Trim() ?? string.Empty, DbType.String);

            return conn.QueryFirstOrDefault<FrmPreaNotificacionInfoInterna>(
                "spPrea_frmPreaNotificacion_Cargar",
                parameters,
                tx,
                commandType: CommandType.StoredProcedure
            ) ?? new FrmPreaNotificacionInfoInterna();
        }

        private static FrmPreaNotificacionPlantillaDatos ConstruirDatosPlantilla(
            FrmPreaNotificacionInfoInterna info,
            FrmPreaNotificacionEnviarRequest request)
        {
            var estado = (info.estado ?? string.Empty).Trim();
            var estadoCodigo = estado.Equals("Denegado", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "APRO";

            return new FrmPreaNotificacionPlantillaDatos
            {
                cod_preanalisis = info.cod_preanalisis,
                cedula = info.cedula,
                nombre_asociado = info.nombre_asociado,
                estado = estado,
                estado_codigo = estadoCodigo,
                monto_aprobado = info.monto_aprobado,
                monto_sugerido = request.monto_sugerido,
                tiquete = request.tiquete?.Trim() ?? string.Empty,
                correo = info.correo,
                celular = info.celular
            };
        }

        private static string ValidarUsuarioEnviaNotificacion(
            SqlConnection conn,
            SqlTransaction tx,
            string usuario,
            string estadoCodigo)
        {
            const string sql = @"
                select dbo.fxCrdPreaValidaUsuarioEnviaNotificacion(@usuario, @estado) as resultado;";

            return conn.QueryFirstOrDefault<string>(
                       sql,
                       new
                       {
                           usuario = (usuario ?? string.Empty).Trim(),
                           estado = (estadoCodigo ?? string.Empty).Trim()
                       },
                       tx
                   )?.Trim() ?? string.Empty;
        }

        private static FrmPreaPlantillaCorreoDto ObtenerPlantillaCorreo(
            SqlConnection conn,
            SqlTransaction tx,
            string tipoPlantilla)
        {
            return conn.QueryFirstOrDefault<FrmPreaPlantillaCorreoDto>(
                       "spCRD_PREA_CONSULTA_PLANTILLA_NOTIFICACION",
                       new { TipoPlantilla = (tipoPlantilla ?? string.Empty).Trim() },
                       tx,
                       commandType: CommandType.StoredProcedure
                   ) ?? new FrmPreaPlantillaCorreoDto();
        }

        private static FrmPreaPlantillaMensajeDto ObtenerPlantillaMensaje(
            SqlConnection conn,
            SqlTransaction tx,
            string tipoPlantilla)
        {
            return conn.QueryFirstOrDefault<FrmPreaPlantillaMensajeDto>(
                       "spCRD_PREA_CONSULTA_PLANTILLA_MENSAJE",
                       new { TipoPlantilla = (tipoPlantilla ?? string.Empty).Trim() },
                       tx,
                       commandType: CommandType.StoredProcedure
                   ) ?? new FrmPreaPlantillaMensajeDto();
        }

        private static FrmPreaPlantillaSmsDto ObtenerPlantillaMensajeSms(
            SqlConnection conn,
            SqlTransaction tx,
            string tipoPlantilla)
        {
            return conn.QueryFirstOrDefault<FrmPreaPlantillaSmsDto>(
                       "spCRD_PREA_CONSULTA_PLANTILLA_MENSAJE_SMS",
                       new { TipoPlantilla = (tipoPlantilla ?? string.Empty).Trim() },
                       tx,
                       commandType: CommandType.StoredProcedure
                   ) ?? new FrmPreaPlantillaSmsDto();
        }

        private static string CompletarPlantilla(
            string plantilla,
            FrmPreaNotificacionPlantillaDatos data)
        {
            if (string.IsNullOrWhiteSpace(plantilla))
            {
                return string.Empty;
            }

            var texto = plantilla;

            texto = ReemplazarMarcador(texto, "{NombreAsociado}", data.nombre_asociado);
            texto = ReemplazarMarcador(texto, "{MontoAprobado}", data.monto_aprobado.ToString("N2"));
            texto = ReemplazarMarcador(texto, "{MontoSugerido}", data.monto_sugerido.ToString("N2"));
            texto = ReemplazarMarcador(texto, "{Tiquete}", data.tiquete);
            texto = ReemplazarMarcador(texto, "{CodPreanalisis}", data.cod_preanalisis);
            texto = ReemplazarMarcador(texto, "{Cedula}", data.cedula);
            texto = ReemplazarMarcador(texto, "{Correo}", data.correo);
            texto = ReemplazarMarcador(texto, "{Celular}", data.celular);

            return texto;
        }

        private static string ReemplazarMarcador(string texto, string marcador, string valor)
        {
            return texto.Replace(marcador, valor ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private static void RegistrarColaNotificacion(
            SqlConnection conn,
            SqlTransaction tx,
            RegistrarColaNotificacionRequest request
            )
        {
            var parameters = new DynamicParameters();
            parameters.Add("@pMensaje", request.mensaje ?? string.Empty, DbType.String);
            parameters.Add("@pAsunto", request.asunto ?? string.Empty, DbType.String);
            parameters.Add("@pEstado", "P", DbType.String);
            parameters.Add("@pCorreo", request.correo?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@pTipo", request.tipoEnvio?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@pUsuario", request.usuario?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@pCelular", request.celular?.Trim() ?? string.Empty, DbType.String);

            conn.Execute(
                "spCRD_PREA_NOTIFICA_ENVIA_ALERTA",
                parameters,
                tx,
                commandType: CommandType.StoredProcedure);
        }

        private static void RegistrarBitacoraCorreo(
            SqlConnection conn,
            SqlTransaction tx,
            FrmPreaNotificacionInfoInterna info,
            RegistrarBitacoraCorreoRequest request
            )
        {
            var parameters = new DynamicParameters();
            parameters.Add("@cod_preanalisis", info.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@mensaje", request.mensajeCorreo ?? string.Empty, DbType.String);
            parameters.Add("@mensaje_sms", request.mensajeSms ?? string.Empty, DbType.String);
            parameters.Add("@correo", info.correo?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@celular", info.celular?.Trim() ?? string.Empty, DbType.String);
            parameters.Add("@ind_msj", request.envioSms ? "SI" : "NO", DbType.String);
            parameters.Add("@ind_correo", request.envioCorreo ? "SI" : "NO", DbType.String);
            parameters.Add("@usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

            conn.Execute(
                "spCRD_PREA_RegistraBitacoraCorreo",
                parameters,
                tx,
                commandType: CommandType.StoredProcedure);
        }

        private void RegistrarBitacoraSif(
            int codEmpresa,
            string usuario,
            FrmPreaNotificacionInfoInterna info,
            string asunto)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).Trim(),
                FechaHora = DateTime.Now,
                Modulo = ModuloCreditos,
                Movimiento = "REGISTRA-WEB",
                DetalleMovimiento =
                    $"Se envia notificación de resolución de estudio de crédito. " +
                    $"Preanalisis: {info.cod_preanalisis} " +
                    $"Cedula: {info.cedula} " +
                    $"Asunto: {asunto} " +
                    $"Correo: {info.correo} " +
                    $"Teléfono: {info.celular} " +
                    $"Usuario envía: {usuario}",
                Detalle = "frmPrea_Notificacion"
            });
        }

        #endregion

    }
}
