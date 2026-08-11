using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_Externo.Models.NewFolder;
using System.Data;
using System.Text.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class ZohoDB
    {
        private const string ExpedienteActualizarSql = @"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                    SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                        ID_BENEFICIO = @idBeneficio, [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                  WHERE ID_ZOHO = @idZoho";

        private const string ExpedienteActualizarVistoSql = @"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                    SET [N_EXPEDIENTE] = @nExpediente, [CONSEC] = @consec, COD_BENEFICIO = @codBeneficio,
                        ID_BENEFICIO = @idBeneficio, I_PENDIENTE = 1, I_VISTO = 1, VISTO_POR = @usuario, VISTO_FECHA = getdate(),
                        [ESTADO] = 'S', INCLUIDO_POR = @usuario, INCLUIDO_FECHA = getdate()
                  WHERE ID_ZOHO = @idZoho";

        private const string ExpedienteErrorActualizarSql = @"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                    SET [MSJ_INTERFACE] = @mensajeError, [ESTADO] = 'E'
                  WHERE ID_ZOHO = @idZoho";

        private const string ExpedienteErrorActualizarUsuarioSql = @"UPDATE [dbo].[AFI_BENE_OTORGA_INT]
                    SET [MSJ_INTERFACE] = @mensajeError, [ESTADO] = 'E', VISTO_POR = @usuario, VISTO_FECHA = getdate()
                  WHERE ID_ZOHO = @idZoho";

        /// <summary>
        /// Coordina la preparación específica y la persistencia compartida de un expediente proveniente de Zoho.
        /// </summary>
        private ErrorDto AF_Beneficios_Zoho_Expediente_Procesar(ZohoExpedienteProcesoRequest request)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), request.CodEmpresa);
                if (request.Preparar is null)
                {
                    return DbHelper.ErrorResponse("No se configuró la preparación del expediente de Zoho");
                }

                var preparacion = request.Preparar(connection, request);
                var mensajeError = preparacion.MensajeError;

                var beneficio = preparacion.Beneficio;
                if (beneficio is not null)
                {
                    mensajeError += AF_Beneficios_Zoho_Beneficio_Persistir(connection, request, preparacion, beneficio);
                }

                return string.IsNullOrWhiteSpace(mensajeError)
                    ? new ErrorDto { Code = 0 }
                    : AF_Beneficios_Zoho_Error_Responder(connection, request, preparacion, mensajeError);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        private string AF_Beneficios_Zoho_Beneficio_Persistir(
            IDbConnection connection,
            ZohoExpedienteProcesoRequest request,
            ZohoExpedientePreparacion preparacion,
            BeneficioGeneralDatos beneficio)
        {
            var respuesta = _beneIntegral
                .BeneficioIntegralGeneral_Guardar(request.CodEmpresa, "API", beneficio)
                .Result;

            if (respuesta.Code == -1)
            {
                return respuesta.Description + "...";
            }

            var expediente = (respuesta.Description ?? "0@0").Split('@');
            AF_Beneficios_Zoho_Expediente_Actualizar(connection, request, preparacion, expediente);

            if (expediente[0] != "0")
            {
                AF_Beneficios_Zoho_Expediente_DetalleGuardar(request, preparacion, expediente);
                AF_Beneficios_Zoho_Formularios_Incluir(request, preparacion, beneficio);
            }

            // Los adjuntos no se transfieren porque esta versión aún no dispone de la
            // infraestructura HTTP de threads y attachments de Zoho Desk.
            return string.Empty;
        }

        private static void AF_Beneficios_Zoho_Expediente_Actualizar(
            IDbConnection connection,
            ZohoExpedienteProcesoRequest request,
            ZohoExpedientePreparacion preparacion,
            string[] expediente)
        {
            var codigoBeneficio = preparacion.CodigoBeneficio.Trim();
            var nExpediente = expediente[0].PadLeft(6, '0') + codigoBeneficio + expediente[1].PadLeft(6, '0');
            var sql = request.MarcarVistoAlGuardar ? ExpedienteActualizarVistoSql : ExpedienteActualizarSql;
            connection.Execute(sql, new
            {
                nExpediente,
                consec = expediente[1],
                codBeneficio = codigoBeneficio,
                idBeneficio = expediente[0],
                usuario = request.Usuario,
                idZoho = request.Solicitud.ticket
            });
        }

        private void AF_Beneficios_Zoho_Expediente_DetalleGuardar(
            ZohoExpedienteProcesoRequest request,
            ZohoExpedientePreparacion preparacion,
            string[] expediente)
        {
            if (!preparacion.GuardarReconocimiento)
            {
                return;
            }

            Reconocimientos_Detalle_Guardar(new ReconocimientoGuardarRequest
            {
                CodEmpresa = request.CodEmpresa,
                Datos = request.Datos,
                Usuario = request.Usuario,
                CodigoBeneficio = preparacion.CodigoBeneficio,
                Expediente = expediente
            });
        }

        private void AF_Beneficios_Zoho_Formularios_Incluir(
            ZohoExpedienteProcesoRequest request,
            ZohoExpedientePreparacion preparacion,
            BeneficioGeneralDatos beneficio)
        {
            var filtros = new FrmFiltros
            {
                codCliente = request.CodEmpresa,
                cod_beneficio = preparacion.CodigoFormulario,
                id_beneficio = beneficio.id_beneficio,
                socio = beneficio.cedula,
                usuario = request.Usuario
            };

            IncluirRespuestasFormularios(filtros, request.Datos);
        }

        private static ErrorDto AF_Beneficios_Zoho_Error_Responder(
            IDbConnection connection,
            ZohoExpedienteProcesoRequest request,
            ZohoExpedientePreparacion preparacion,
            string mensajeError)
        {
            if (preparacion.RegistrarError)
            {
                var sql = request.RegistrarUsuarioEnError
                    ? ExpedienteErrorActualizarUsuarioSql
                    : ExpedienteErrorActualizarSql;
                connection.Execute(sql, new
                {
                    mensajeError,
                    usuario = request.Usuario,
                    idZoho = request.Solicitud.ticket
                });
            }

            return new ErrorDto { Code = -1, Description = mensajeError };
        }

        private static BeneficioGeneralDatos AF_Beneficios_Zoho_BeneficioBase_Crear(
            ZohoExpedienteProcesoRequest request,
            string cedula,
            string codigoBeneficio,
            string estado)
        {
            return new BeneficioGeneralDatos
            {
                cod_beneficio = new AfBeneficioIntegralDropsLista { item = codigoBeneficio },
                id_beneficio = 0,
                cedula = cedula.Trim(),
                monto_aplicado = 0,
                registra_user = request.Usuario,
                modifica_usuario = request.Usuario,
                sepelio_identificacion = null,
                estado = new AfBeneficioIntegralDropsLista { item = estado },
                consec = 0,
                requiere_justificacion = request.Solicitud.justificacion != null,
                notas = request.Solicitud.justificacion ?? string.Empty
            };
        }

        private static float AF_Beneficios_Zoho_Monto_Obtener(
            IDbConnection connection,
            string? categoria,
            string codigoBeneficio)
        {
            const string sql = @"SELECT [MONTO]
                    FROM [AFI_BENE_GRUPOS] WHERE COD_CATEGORIA = @categoria
                    AND COD_GRUPO in (
                        SELECT COD_GRUPO
                        FROM [AFI_BENEFICIOS] WHERE COD_CATEGORIA = @categoria
                        AND COD_BENEFICIO = @codigoBeneficio
                    )";

            return connection.QueryFirstOrDefault<float>(sql, new { categoria, codigoBeneficio });
        }

        private sealed class ZohoExpedienteProcesoRequest
        {
            public int CodEmpresa { get; init; }
            public Dictionary<string, JsonElement> Datos { get; init; } = [];
            public string Usuario { get; init; } = string.Empty;
            public ZohoTicketAdd Solicitud { get; init; } = new();
            public bool MarcarVistoAlGuardar { get; init; }
            public bool RegistrarUsuarioEnError { get; init; }
            public Func<IDbConnection, ZohoExpedienteProcesoRequest, ZohoExpedientePreparacion>? Preparar { get; init; }
        }

        private sealed class ZohoExpedientePreparacion
        {
            public BeneficioGeneralDatos? Beneficio { get; init; }
            public string CodigoBeneficio { get; init; } = string.Empty;
            public string? CodigoFormulario { get; init; }
            public string MensajeError { get; init; } = string.Empty;
            public bool RegistrarError { get; init; } = true;
            public bool GuardarReconocimiento { get; init; }
        }
    }
}
