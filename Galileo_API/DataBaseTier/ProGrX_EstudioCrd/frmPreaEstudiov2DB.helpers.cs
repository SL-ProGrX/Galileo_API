using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Navega al expediente siguiente o anterior según el scroll_code.
        /// 0 = anterior, 1 = siguiente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ScrollResponse> Prea_frmPreaEstudiov2_Scroll(
            int codEmpresa,
            FrmPreaEstudiov2ScrollRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2ScrollResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2ScrollResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var current = request.cod_preanalisis?.Trim() ?? string.Empty;
                var isEmpty = string.IsNullOrEmpty(current);

                switch (request.scroll_code)
                {
                    case 0:
                        if (isEmpty) current = "99999999999";
                        break;
                    case 1:
                        if (isEmpty) current = "0";
                        break;
                    default:
                        response.Code = -1;
                        response.Description = "Código de navegación inválido.";
                        return response;
                }

                var sql = request.scroll_code == 0
                    ? @"SELECT TOP 1 cod_preanalisis 
                        FROM CRD_PREA_PREANALISIS 
                        WHERE TIPO_PREANALISIS = 'E' 
                        AND cod_preanalisis < @Current
                        AND TRY_CAST(cod_preanalisis AS BIGINT) IS NOT NULL 
                        ORDER BY TRY_CAST(cod_preanalisis AS BIGINT) DESC"
                    : @"SELECT TOP 1 cod_preanalisis 
                        FROM CRD_PREA_PREANALISIS 
                        WHERE TIPO_PREANALISIS = 'E' 
                        AND cod_preanalisis > @Current
                        AND TRY_CAST(cod_preanalisis AS BIGINT) IS NOT NULL 
                        ORDER BY TRY_CAST(cod_preanalisis AS BIGINT) DESC";

                var result = connection.QueryFirstOrDefault<string>(
                    sql,
                    new { Current = current }
                );

                response.Result = new FrmPreaEstudiov2ScrollResponse
                {
                    cod_preanalisis = result ?? string.Empty
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2ScrollResponse();
                return response;
            }
        }

        /// <summary>
        /// Guarda observaciones del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Observaciones_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2ObservacionesRequest request)
        {
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"UPDATE CRD_PREA_PREANALISIS 
                                     SET CUMPLIMIENTO_NOTAS = @Observaciones 
                                     WHERE cod_Preanalisis = @Expediente";

                connection.Execute(
                    sql,
                    new
                    {
                        Expediente = request.cod_preanalisis.Trim(),
                        Observaciones = request.observaciones?.Trim() ?? string.Empty
                    },
                    commandType: CommandType.Text
                );

                response.Result = "Observaciones guardadas correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = string.Empty;
                return response;
            }
        }

        /// <summary>
        /// Asigna comité resolutivo al expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ComiteAsignarResponse> Prea_frmPreaEstudiov2_Comite_Asignar(
            int codEmpresa,
            FrmPreaEstudiov2ComiteAsignarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2ComiteAsignarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2ComiteAsignarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                    "spCrdPreaGestionaComiteResolutivo",
                    new
                    {
                        IdComite = request.comite?.Trim() ?? string.Empty,
                        Expediente = request.cod_preanalisis.Trim(),
                        NuevoEstado = "RECI",
                        IndicadorEditable = 1
                    },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = new FrmPreaEstudiov2ComiteAsignarResponse
                {
                    comite = request.comite?.Trim() ?? string.Empty,
                    asignado = true,
                    mensaje = "Comité asignado correctamente."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2ComiteAsignarResponse();
                return response;
            }
        }

        /// <summary>
        /// Copia un expediente existente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CopiarResponse> Prea_frmPreaEstudiov2_Copiar(
            int codEmpresa,
            FrmPreaEstudiov2CopiarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2CopiarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CopiarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", request.cod_preanalisis_origen.Trim(), DbType.String);
                parameters.Add("@Usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

                var nuevoExpediente = connection.QueryFirstOrDefault<string>(
                    "spCrd_Prea_Expediente_Copia",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                response.Result = new FrmPreaEstudiov2CopiarResponse
                {
                    cod_preanalisis_nuevo = nuevoExpediente ?? string.Empty,
                    mensaje = "Expediente copiado correctamente."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2CopiarResponse();
                return response;
            }
        }

        /// <summary>
        /// Solicita el estado del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SolicitarResponse> Prea_frmPreaEstudiov2_Solicitar(
            int codEmpresa,
            FrmPreaEstudiov2SolicitarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2SolicitarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2SolicitarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                    "spCrd_Prea_Estado_Solicitado",
                    new
                    {
                        Expediente = request.cod_preanalisis.Trim(),
                        Usuario = request.usuario?.Trim() ?? string.Empty
                    },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = new FrmPreaEstudiov2SolicitarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    estado = "SOL",
                    estado_desc = "Solicitado",
                    mensaje = "Expediente marcado como solicitado."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2SolicitarResponse();
                return response;
            }
        }

        /// <summary>
        /// Guarda una incapacidad del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Incapacidades_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2IncapacidadGuardarRequest request)
        {
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var dias = (request.hasta - request.desde).Days + 1;

                connection.Execute(
                    "spCrdPreaGeneraIncapacidades",
                    new
                    {
                        Expediente = request.cod_preanalisis.Trim(),
                        Dias = dias,
                        Desde = request.desde.ToString("yyyy-MM-dd"),
                        Hasta = request.hasta.ToString("yyyy-MM-dd"),
                        Orden = 1
                    },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = "Incapacidad guardada correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = string.Empty;
                return response;
            }
        }

        /// <summary>
        /// Elimina incapacidades del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Incapacidades_Eliminar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                    "spCrdPreaEliminarIncapacidades",
                    new { Expediente = cod_preanalisis.Trim() },
                    commandType: CommandType.StoredProcedure
                );

                response.Result = "Incapacidades eliminadas correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = string.Empty;
                return response;
            }
        }

        /// <summary>
        /// Guarda un extra del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Extras_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2ExtraGuardarRequest request)
        {
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"INSERT INTO CRD_PREA_DETALLE_EXTRAS 
                                     (cod_Preanalisis, Cod_Extras, Monto) 
                                     VALUES (@Expediente, @CodExtras, @Monto)";

                connection.Execute(
                    sql,
                    new
                    {
                        Expediente = request.cod_preanalisis.Trim(),
                        CodExtras = request.cod_extras?.Trim() ?? string.Empty,
                        Monto = request.monto
                    },
                    commandType: CommandType.Text
                );

                response.Result = "Extra guardado correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = string.Empty;
                return response;
            }
        }
    }
}
