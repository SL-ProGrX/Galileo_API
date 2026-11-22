using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public partial class FrmPolizaReclamoDB
    {
        /// <summary>
        /// Registra o actualiza la información principal del reclamo.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del reclamo.</param>
        /// <returns>Resultado del proceso con el reclamo generado/actualizado.</returns>
        public ErrorDto<PolizaReclamoAddResponse> Poliza_Reclamo_Add(
            int codEmpresa,
            PolizaReclamoAddRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (request == null)
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>(PolizaReclamoConstFrm.valRequest, -2);

                if (request.operacion <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar la operación.", -2);

                if (request.poliza_id <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar la póliza.", -2);

                if (string.IsNullOrWhiteSpace(request.poliza_codigo))
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar el código de póliza.", -2);

                if (string.IsNullOrWhiteSpace(request.cedula))
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar la cédula.", -2);

                if (string.IsNullOrWhiteSpace(request.nombre))
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar el nombre.", -2);

                if (!request.fecha_nac.HasValue)
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar la fecha de nacimiento.", -2);

                if (request.desembolso_id < 0 || request.pago_id <= 0)
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar desembolso y método de pago válidos.", -2);

                if (string.IsNullOrWhiteSpace(request.usuario))
                    return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>("Debe indicar el usuario.", -2);

                const string sql = @"EXEC spPoliza_Reclamo_Add
            @ReclamoId = @ReclamoId,
            @Operacion = @Operacion,
            @PolizaCodigo = @PolizaCodigo,
            @PolizaId = @PolizaId,
            @Cedula = @Cedula,
            @Apellido1 = @Apellido1,
            @Apellido2 = @Apellido2,
            @Nombre = @Nombre,
            @Sexo = @Sexo,
            @FechaNac = @FechaNac,
            @Edad = @Edad,
            @Finca = @Finca,
            @SiniestroId = @SiniestroId,
            @CausaId = @CausaId,
            @MotivoId = @MotivoId,
            @EnfermedadId = @EnfermedadId,
            @DesembolsoId = @DesembolsoId,
            @PagoId = @PagoId,
            @Observaciones = @Observaciones,
            @Usuario = @Usuario";

                var row = connection.QueryFirstOrDefault<dynamic>(
                    sql,
                    new
                    {
                        ReclamoId = request.reclamo_id,
                        Operacion = request.operacion,
                        PolizaCodigo = TrimOrEmpty(request.poliza_codigo),
                        PolizaId = request.poliza_id,
                        Cedula = TrimOrEmpty(request.cedula),
                        Apellido1 = TrimOrEmpty(request.apellido1),
                        Apellido2 = TrimOrEmpty(request.apellido2),
                        Nombre = TrimOrEmpty(request.nombre),
                        Sexo = TrimOrEmpty(request.sexo),
                        FechaNac = request.fecha_nac,
                        Edad = request.edad,
                        Finca = TrimOrEmpty(request.finca),
                        SiniestroId = request.siniestro_id,
                        CausaId = request.causa_id,
                        MotivoId = request.motivo_id,
                        EnfermedadId = request.enfermedad_id,
                        DesembolsoId = request.desembolso_id,
                        PagoId = request.pago_id,
                        Observaciones = TrimOrEmpty(request.observaciones),
                        Usuario = TrimOrEmpty(request.usuario)
                    });

                Func<dynamic, PolizaReclamoAddResponse> mapResponse = x => new PolizaReclamoAddResponse
                {
                    reclamo_id = x.ReclamoId ?? request.reclamo_id,
                    mensaje = x.Mensaje?.ToString() ?? "Reclamo registrado satisfactoriamente."
                };

                return ValidarResultadoOperacion(
                    row,
                    "No se obtuvo respuesta al registrar el reclamo.",
                    "No fue posible registrar el reclamo.",
                    mapResponse);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoAddResponse>(
                    $"Error al registrar el reclamo: {ex.Message}");
            }
        }


    }
}
