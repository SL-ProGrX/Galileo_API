using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
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
        /// Guarda observaciones del expediente. Fiel a VB6 sbGuardaObservaciones
        /// (frmPreaEstudiov2.frm línea ~15116): clsEntidad.tablaName = "spCRDPreaObservaciones"
        /// → EXEC spCRDPreaObservaciones_M &lt;expediente&gt;, &lt;analista&gt;, &lt;comite&gt;, &lt;jd&gt;.
        /// El SP actualiza los 3 campos juntos, por lo que primero se leen los valores
        /// actuales y solo se sobrescribe el campo correspondiente a request.tipo
        /// ('A' Analista / 'C' Comité / 'J' Junta Directiva — optObservacion 0/1/2).
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

                var exp = request.cod_preanalisis.Trim().Replace("'", "''");

                var actualRow = connection.QueryFirstOrDefault(
                    "select OBSERVACION_ANALISTA, OBSERVACION_COMITE, OBSERVACION_JD" +
                    " from CRD_PREA_PREANALISIS where cod_Preanalisis = @Expediente",
                    new { Expediente = request.cod_preanalisis.Trim() }
                ) as IDictionary<string, object>;

                var dict = actualRow is null
                    ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, object>(actualRow, StringComparer.OrdinalIgnoreCase);

                var analista = GetString(dict, "OBSERVACION_ANALISTA");
                var comite = GetString(dict, "OBSERVACION_COMITE");
                var jd = GetString(dict, "OBSERVACION_JD");

                var nuevoValor = (request.observaciones ?? string.Empty).Trim();

                switch ((request.tipo ?? string.Empty).Trim().ToUpperInvariant())
                {
                    case "A":
                        analista = nuevoValor;
                        break;
                    case "C":
                        comite = nuevoValor;
                        break;
                    case "J":
                        jd = nuevoValor;
                        break;
                    default:
                        response.Code = -1;
                        response.Description = "Tipo de observación inválido (use 'A', 'C' o 'J').";
                        return response;
                }

                const string sql = "EXEC spCRDPreaObservaciones_M @Expediente, @Analista, @Comite, @Jd";
                connection.Execute(sql, new { Expediente = exp, Analista = analista, Comite = comite, Jd = jd });

                response.Result = "La información se registró correctamente.";
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
        /// Asigna comité resolutivo al expediente. Fiel a VB6 (frmPreaEstudiov2.frm línea
        /// ~12840, botón de asignar comité): valida con spCrdPrea_Comite_Asigna_Valida
        /// (liquidez, máximo del comité, etiquetas, justificación de edad), determina el
        /// nuevo estado según Tipo_Aprobacion ('E' Ejecutivo → RECI/editable, 'M' Mancomunado
        /// → PRCO), ejecuta spCrdPreaGestionaComiteResolutivo y, solo si es Mancomunado,
        /// registra la bitácora vía spCrdPreaGuardaBitacoraElevacionComite.
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

                var exp = request.cod_preanalisis.Trim();
                var comite = request.comite?.Trim() ?? string.Empty;

                var validacionRow = connection.QueryFirstOrDefault(
                    "EXEC spCrdPrea_Comite_Asigna_Valida @Expediente, @Comite",
                    new { Expediente = exp, Comite = comite }
                ) as IDictionary<string, object>;

                if (validacionRow is null)
                {
                    response.Code = -1;
                    response.Description = "No se pudo validar la asignación del comité.";
                    response.Result = new FrmPreaEstudiov2ComiteAsignarResponse();
                    return response;
                }

                var dict = new Dictionary<string, object>(validacionRow, StringComparer.OrdinalIgnoreCase);
                var mensajeValidacion = GetString(dict, "Mensaje");

                if (!string.IsNullOrEmpty(mensajeValidacion))
                {
                    response.Code = -1;
                    response.Description = mensajeValidacion;
                    response.Result = new FrmPreaEstudiov2ComiteAsignarResponse();
                    return response;
                }

                var tipoAprobacion = GetString(dict, "Tipo_Aprobacion");

                string nuevoEstado;
                int indicadorEditable = 0;

                switch (tipoAprobacion)
                {
                    case "E":
                        nuevoEstado = "RECI";
                        indicadorEditable = 1;
                        break;
                    case "M":
                        nuevoEstado = "PRCO";
                        break;
                    default:
                        nuevoEstado = string.Empty;
                        break;
                }

                const string sql = @"EXEC spCrdPreaGestionaComiteResolutivo
                    @Comite, @Expediente, @NuevoEstado, @IndicadorEditable";
                connection.Execute(sql, new
                {
                    Comite = comite,
                    Expediente = exp,
                    NuevoEstado = nuevoEstado,
                    IndicadorEditable = indicadorEditable
                });

                if (tipoAprobacion == "M")
                {
                    var usuario = (request.usuario ?? string.Empty).Trim();
                    var comiteDesc = (request.comite_desc ?? string.Empty).Trim();
                    var nota = $"ELEVADO AL COMITE: {comite}-{comiteDesc} Fecha envio: {DateTime.Now}";

                    const string bitacoraSql = @"EXEC spCrdPreaGuardaBitacoraElevacionComite
                        @Expediente, @Usuario, @Comite, @Nota";
                    connection.Execute(bitacoraSql, new
                    {
                        Expediente = exp,
                        Usuario = usuario,
                        Comite = comite,
                        Nota = nota
                    });
                }

                response.Result = new FrmPreaEstudiov2ComiteAsignarResponse
                {
                    comite = comite,
                    asignado = true,
                    mensaje = "Comité asignado correctamente.",
                    tipo_aprobacion = tipoAprobacion
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
        /// Guarda (inserta o modifica) un extra del expediente. Fiel a VB6 fxExtras_Inserta /
        /// fxExtras_Modifica (frmPreaEstudiov2.frm líneas ~15829/~15897), que usan
        /// clsEntidad.tablaName = "spCRDPreaDETALLE_EXTRAS" → EXEC spCRDPreaDETALLE_EXTRAS_A/_M
        /// &lt;idx&gt;, &lt;expediente&gt;, &lt;codExtras&gt;, &lt;monto&gt; (idx = 0 para insertar).
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

                var exp = request.cod_preanalisis.Trim();
                var codExtras = (request.cod_extras ?? string.Empty).Trim();
                var esNuevo = request.idx <= 0;
                const string sqlAgregar = "EXEC spCRDPreaDETALLE_EXTRAS_A @Idx, @Expediente, @CodExtras, @Monto";
                const string sqlModificar = "EXEC spCRDPreaDETALLE_EXTRAS_M @Idx, @Expediente, @CodExtras, @Monto";
                var sql = esNuevo ? sqlAgregar : sqlModificar;
                connection.Execute(sql, new
                {
                    request.idx,
                    Expediente = exp,
                    CodExtras = codExtras,
                    request.monto
                });

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

        /// <summary>
        /// Elimina un extra del expediente. Fiel a VB6 fxExtras_Borrar (frmPreaEstudiov2.frm
        /// línea ~15948): clsEntidad.tablaName = "spCRDPreaDETALLE_EXTRAS" →
        /// EXEC spCRDPreaDETALLE_EXTRAS_B &lt;idx&gt;, &lt;expediente&gt;.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Extras_Borrar(
            int codEmpresa,
            FrmPreaEstudiov2ExtraBorrarRequest request)
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

                const string sql = "EXEC spCRDPreaDETALLE_EXTRAS_B @Idx, @Expediente";
                connection.Execute(sql, new { request.idx, Expediente = request.cod_preanalisis.Trim() });

                response.Result = "Extra eliminado correctamente.";
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
