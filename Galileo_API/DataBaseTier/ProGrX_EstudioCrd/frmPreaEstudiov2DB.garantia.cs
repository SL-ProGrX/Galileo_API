using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// VB6: txtCedula_LostFocus (frmPreaEstudiov2.frm ~línea 16868-16885):
        ///   SELECT dbo.fxCrdPrea_Persona_Datos_Valida('&lt;cedula&gt;') as 'Valida'
        /// Resultado: 0 = no necesita validación, 1 = abrir Verificación de Datos
        /// (frmCR_VerificaDatosPersonales), 2 = registro nuevo del cliente (VB6 no hace
        /// nada especial aquí para ese caso).
        /// </summary>
        public ErrorDto<int> Prea_frmPreaEstudiov2_Persona_ValidarDatos(int codEmpresa, string cedula)
        {
            var result = new ErrorDto<int> { Code = 0, Description = "Ok", Result = 0 };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var cedulaNormalizada = (cedula ?? string.Empty).Trim();

                if (string.IsNullOrEmpty(cedulaNormalizada))
                {
                    return result;
                }

                const string sql = "SELECT dbo.fxCrdPrea_Persona_Datos_Valida(@Cedula)";
                result.Result = connection.QueryFirstOrDefault<int?>(sql, new { Cedula = cedulaNormalizada }) ?? 0;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = 0;
            }

            return result;
        }

        /// <summary>
        /// VB6: cboGarantia_Click, bloque "Carga Examenes Requeridos" (~línea 14388-14418).
        /// Si el ESTADO_EXAMENES del expediente (spCrd_Prea_Prenda_Datos) no es "A" ejecuta
        /// spCrdPrea_Examenes_Requeridos_Lista(línea, garantía, edad, monto) y toma los
        /// requisitos con Id_Requisito &gt; 0 (DESCRIPCION_EXAMENES). Si hay alguno registra
        /// la notificación con spCRD_PreaRegistraHistoricoExamenes (sbPrendario_Examen_Registra).
        /// Los parámetros de los SP se envían por posición, igual que el EXEC del VB6.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ExamenesRequeridosResponse> Prea_frmPreaEstudiov2_Examenes_Requeridos(
            int codEmpresa, FrmPreaEstudiov2ExamenesRequeridosRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2ExamenesRequeridosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2ExamenesRequeridosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var expediente = (request.cod_preanalisis ?? string.Empty).Trim();

                if (ObtenerEstadoExamenes(connection, expediente) == "A")
                {
                    return result;
                }

                var requisitos = connection.Query(
                    "EXEC spCrdPrea_Examenes_Requeridos_Lista @Linea, @Garantia, @Edad, @Monto",
                    new
                    {
                        Linea = (request.linea ?? string.Empty).Trim(),
                        Garantia = (request.garantia ?? string.Empty).Trim(),
                        Edad = request.edad ?? 0,
                        Monto = request.monto ?? 0m
                    });

                foreach (var fila in requisitos.Cast<IDictionary<string, object>>())
                {
                    var dict = new Dictionary<string, object>(fila, StringComparer.OrdinalIgnoreCase);
                    if (GetInt(dict, "Id_Requisito") > 0)
                    {
                        result.Result.examenes.Add(GetString(dict, "DESCRIPCION_EXAMENES"));
                    }
                }

                if (result.Result.examenes.Count > 0 && expediente.Length > 0)
                {
                    connection.Execute(
                        "EXEC spCRD_PreaRegistraHistoricoExamenes @Expediente, @Usuario, @Nota",
                        new
                        {
                            Expediente = expediente,
                            Usuario = (request.usuario ?? string.Empty).Trim(),
                            Nota = "Examenes médicos notificados al ejecutivo."
                        });
                    result.Result.notificado = true;
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2ExamenesRequeridosResponse();
            }

            return result;
        }

        /// <summary>
        /// VB6: m_Examenes_Estado (ESTADO_EXAMENES de spCrd_Prea_Prenda_Datos). Sin
        /// expediente o sin fila se considera vacío (distinto de "A").
        /// </summary>
        private static string ObtenerEstadoExamenes(IDbConnection connection, string expediente)
        {
            if (expediente.Length == 0)
            {
                return string.Empty;
            }

            var fila = connection.QueryFirstOrDefault(
                "EXEC spCrd_Prea_Prenda_Datos @Expediente",
                new { Expediente = expediente }) as IDictionary<string, object>;

            return fila is null
                ? string.Empty
                : GetString(new Dictionary<string, object>(fila, StringComparer.OrdinalIgnoreCase), "ESTADO_EXAMENES");
        }

        /// <summary>
        /// VB6: cboGarantia_Click (frmPreaEstudiov2.frm ~línea 14292-14355). Solo dos
        /// formularios calculan Monto directamente aquí:
        ///   F01 (Sobre Ahorros): SELECT dbo.fxCrdGarantiaPatMnt('&lt;cedula&gt;','A','M')
        ///   F06 (Adelanto de Salario): SELECT dbo.fxCrdDisponibleAdelantoSalario_Estudio('&lt;cedula&gt;','M')
        /// El resto de formularios (F02/F03/F05/F07) no tocan txtMonto en este switch —
        /// F05 (Fondos) se resuelve aparte vía Prea_frmPreaEstudiov2_Fondo_Calcular.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GarantiaMontoResponse> Prea_frmPreaEstudiov2_Garantia_Monto(
            int codEmpresa, FrmPreaEstudiov2GarantiaMontoRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2GarantiaMontoResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2GarantiaMontoResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var cedula = (request.cedula ?? string.Empty).Trim();
                var formulario = (request.formulario ?? string.Empty).Trim().ToUpperInvariant();

                decimal monto = 0m;
                if (!string.IsNullOrEmpty(cedula))
                {
                    string? sql = formulario switch
                    {
                        "F01" => "SELECT dbo.fxCrdGarantiaPatMnt(@Cedula, 'A', 'M')",
                        "F06" => "SELECT dbo.fxCrdDisponibleAdelantoSalario_Estudio(@Cedula, 'M')",
                        _ => null
                    };

                    if (sql is not null)
                    {
                        monto = connection.QueryFirstOrDefault<decimal?>(sql, new { Cedula = cedula }) ?? 0m;
                    }
                }

                result.Result = new FrmPreaEstudiov2GarantiaMontoResponse { monto = monto };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2GarantiaMontoResponse();
            }

            return result;
        }

        /// <summary>
        /// VB6: cboFondo_Click / cboFondoContrato_Click (frmPreaEstudiov2.frm ~línea
        /// 14121-14235). Ambos ejecutan EXEC spCRDGarantiaFNDCalculo '&lt;cedula&gt;','&lt;fondo&gt;'
        /// [,&lt;contrato&gt;] y leen Disponible/AplicaTasa/TASA/AplicaPlazo/Plazo (columnas
        /// verificadas por nombre directamente en el .frm). cboFondo_Click además reconstruye
        /// la lista de contratos (fnd_contratos) — eso solo ocurre aquí cuando cod_contrato
        /// viene vacío (equivalente a "cambió el Fondo").
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FondoCalcularResponse> Prea_frmPreaEstudiov2_Fondo_Calcular(
            int codEmpresa, FrmPreaEstudiov2FondoCalcularRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2FondoCalcularResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2FondoCalcularResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var cedula = (request.cedula ?? string.Empty).Trim();
                var fondo = (request.cod_fondo ?? string.Empty).Trim();
                var esCambioDeFondo = string.IsNullOrWhiteSpace(request.cod_contrato);

                const string sqlFondo = "EXEC spCRDGarantiaFNDCalculo @Cedula, @Fondo";
                const string sqlContrato = "EXEC spCRDGarantiaFNDCalculo @Cedula, @Fondo, @Contrato";
                var sql = esCambioDeFondo ? sqlFondo : sqlContrato;

                var response = new FrmPreaEstudiov2FondoCalcularResponse();

                var row = connection.QueryFirstOrDefault(sql, new
                {
                    Cedula = cedula,
                    Fondo = fondo,
                    Contrato = request.cod_contrato?.Trim()
                }) as IDictionary<string, object>;
                if (row is not null)
                {
                    var dict = new Dictionary<string, object>(row, StringComparer.OrdinalIgnoreCase);
                    response.monto = GetDecimal(dict, "Disponible");
                    response.aplica_tasa = GetInt(dict, "AplicaTasa") == 1;
                    response.tasa = GetDecimal(dict, "TASA");
                    response.aplica_plazo = GetInt(dict, "AplicaPlazo") == 1;
                    response.plazo = GetInt(dict, "Plazo");
                }

                if (esCambioDeFondo && !string.IsNullOrEmpty(cedula) && !string.IsNullOrEmpty(fondo))
                {
                    // VB6 (línea 14151-14168): select cod_contrato,Tasa_Referencia,Aportes,
                    // isnull(FECHA_CORTE, getdate()) as 'FECHA_CORTE' from fnd_contratos
                    // where cod_plan = '<fondo>' and estado = 'A' and cedula = '<cedula>'
                    const string sqlContratos = @"select cod_contrato, Tasa_Referencia, Aportes,
                            isnull(FECHA_CORTE, getdate()) as FECHA_CORTE
                        from fnd_contratos
                        where cod_plan = @Fondo and estado = 'A' and cedula = @Cedula";

                    var rows = connection.Query(sqlContratos, new { Fondo = fondo, Cedula = cedula });
                    var contratos = new List<FrmPreaEstudiov2DropdownDto>();
                    foreach (var r in rows)
                    {
                        var d = new Dictionary<string, object>((IDictionary<string, object>)r, StringComparer.OrdinalIgnoreCase);
                        var codContrato = GetString(d, "cod_contrato");
                        var tasaRef = GetDecimal(d, "Tasa_Referencia");
                        var aportes = GetDecimal(d, "Aportes");
                        var fechaCorte = GetDateTime(d, "FECHA_CORTE");

                        contratos.Add(new FrmPreaEstudiov2DropdownDto
                        {
                            item = codContrato,
                            descripcion = "[Cnt: " + codContrato + "] [Tasa: " + tasaRef.ToString(CultureInfo.InvariantCulture)
                                + "] [I: " + aportes.ToString("N2", CultureInfo.InvariantCulture) + "] [V: "
                                + (fechaCorte?.ToString("yyyy-MM-dd") ?? string.Empty) + "]",
                        });
                    }

                    response.contratos = contratos;
                    response.cod_contrato_seleccionado = contratos.Count > 0 ? contratos[0].item : string.Empty;
                }

                result.Result = response;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2FondoCalcularResponse();
            }

            return result;
        }
    }
}
