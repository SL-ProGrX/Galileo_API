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
                var cedulaEscapada = (cedula ?? string.Empty).Trim().Replace("'", "''");

                if (string.IsNullOrEmpty(cedulaEscapada))
                {
                    return result;
                }

                var sql = "SELECT dbo.fxCrdPrea_Persona_Datos_Valida('" + cedulaEscapada + "')";
                result.Result = connection.QueryFirstOrDefault<int?>(sql) ?? 0;
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

                var cedula = (request.cedula ?? string.Empty).Trim().Replace("'", "''");
                var formulario = (request.formulario ?? string.Empty).Trim().ToUpperInvariant();

                decimal monto = 0m;
                if (!string.IsNullOrEmpty(cedula))
                {
                    string? sql = formulario switch
                    {
                        "F01" => "SELECT dbo.fxCrdGarantiaPatMnt('" + cedula + "', 'A', 'M')",
                        "F06" => "SELECT dbo.fxCrdDisponibleAdelantoSalario_Estudio('" + cedula + "', 'M')",
                        _ => null
                    };

                    if (sql is not null)
                    {
                        monto = connection.QueryFirstOrDefault<decimal?>(sql) ?? 0m;
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

                var cedula = (request.cedula ?? string.Empty).Trim().Replace("'", "''");
                var fondo = (request.cod_fondo ?? string.Empty).Trim().Replace("'", "''");
                var esCambioDeFondo = string.IsNullOrWhiteSpace(request.cod_contrato);

                var sql = "EXEC spCRDGarantiaFNDCalculo '" + cedula + "', '" + fondo + "'";
                if (!esCambioDeFondo)
                {
                    sql += ", " + request.cod_contrato!.Trim().Replace("'", "''");
                }

                var response = new FrmPreaEstudiov2FondoCalcularResponse();

                var row = connection.QueryFirstOrDefault(sql) as IDictionary<string, object>;
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
                    var sqlContratos = @"select cod_contrato, Tasa_Referencia, Aportes,
                            isnull(FECHA_CORTE, getdate()) as FECHA_CORTE
                        from fnd_contratos
                        where cod_plan = '" + fondo + @"' and estado = 'A' and cedula = '" + cedula + "'";

                    var rows = connection.Query(sqlContratos);
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
