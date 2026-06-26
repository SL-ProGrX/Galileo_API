using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrPolizasRegistroDb
    {
        private const string FrecuenciaMensual = "Mensual";
        private const string MensajeDebeIndicarOperacion = "Debe indicar la operacion.";
        private const string MensajeDebeIndicarOperacionPoliza = "Debe indicar la operacion y el numero de poliza.";
        private const string MensajeOperacionBaseNoEncontrada = "- No se encontr&oacute; la operaci&oacute;n base.";
        private const string MensajeNoExistenPolizasConfiguradas = "- No Existen Polizas Configuradas para usar...";
        private const string MensajeCoberturaPolizaInvalida = "- La cobertura de la poliza no es v&aacute;lida verifique";

        private sealed class CrPolizasRegistroOperacionBaseData
        {
            public string cedula { get; set; } = string.Empty;
            public string codigo { get; set; } = string.Empty;
        }

        private sealed class CrPolizasRegistroPolizaRetencionData
        {
            public string codigo_retencion { get; set; } = string.Empty;
            public string codigo_cargo { get; set; } = string.Empty;
            public int integra_plan_pagos { get; set; } = 0;
        }

        private sealed class CrPolizasRegistroFxVerificaData
        {
            public bool valido { get; set; }
            public string mensaje { get; set; } = string.Empty;
        }

        private sealed class CrPolizasRegistroDetalleComplementoData
        {
            public List<DropDownListaGenericaModel> destinos { get; set; } = new();
            public List<DropDownListaGenericaModel> garantias { get; set; } = new();
            public int pri_deduc { get; set; }
            public decimal proyectado { get; set; }
            public decimal pendiente { get; set; }
            public int poliza_pagos_num { get; set; }
            public int poliza_cobertura_meses { get; set; }
        }

        private static ErrorDto<T> CrPolizasRegistro_OperacionRequerida<T>(T result)
            => DbHelper.CreateErrorResponse(
                MensajeDebeIndicarOperacion,
                -2,
                result);

        private static ErrorDto<T> CrPolizasRegistro_OperacionPolizaRequerida<T>(T result)
            => DbHelper.CreateErrorResponse(
                MensajeDebeIndicarOperacionPoliza,
                -2,
                result);

        private static int CrPolizasRegistro_PriDeduc_Anio_Obtener(int prideduc)
        {
            string valor = prideduc.ToString();
            if (valor.Length < 6)
            {
                return 0;
            }

            return int.TryParse(valor[..4], out int anio) ? anio : 0;
        }

        private static string CrPolizasRegistro_PriDeduc_Mes_Obtener(int prideduc)
        {
            string valor = prideduc.ToString();
            if (valor.Length < 6 || !int.TryParse(valor.Substring(4, 2), out int mes))
            {
                return string.Empty;
            }

            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => string.Empty
            };
        }

        private static string MapearFrecuencia(string frecuencia)
        {
            return frecuencia switch
            {
                "M" => FrecuenciaMensual,
                "T" => "Trimestral",
                "S" => "Semestral",
                "A" => "Anual",
                "I" => "Indefinida",
                _ => FrecuenciaMensual
            };
        }

        private int CrPolizasRegistro_NumeroPolizaSiguiente_Obtener(int codEmpresa, int operacion)
        {
            const string sql = @"
            select isnull(max(num_poliza), 0) + 1
            from CRD_OPERACION_POLIZAS
            where id_solicitud = @Operacion;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                1,
                new { Operacion = operacion }).Result;
        }

        private int CrPolizasRegistro_UltimaOperacion_Obtener(int codEmpresa, string cedula)
        {
            const string sql = @"
            select isnull(max(id_solicitud), 0)
            from reg_creditos
            where cedula = @Cedula;";

            return DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0,
                new { Cedula = (cedula ?? string.Empty).Trim() }).Result;
        }

        private ErrorDto<CrPolizasRegistroOperacionBaseData?> CrPolizasRegistro_OperacionBase_Obtener(
            int codEmpresa,
            int operacion)
        {
            const string sql = @"
            select top 1
                rtrim(isnull(cedula, '')) as cedula,
                rtrim(isnull(codigo, '')) as codigo
            from reg_creditos
            where id_solicitud = @Operacion;";

            return DbHelper.ExecuteSingleQuery<CrPolizasRegistroOperacionBaseData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { Operacion = operacion });
        }

        private ErrorDto<CrPolizasRegistroPolizaRetencionData?> CrPolizasRegistro_PolizaRetencionData_Obtener(
            int codEmpresa,
            string polizaLinea)
        {
            const string sql = @"
            select top 1
                rtrim(isnull(CODIGO_RETENCION, '')) as codigo_retencion,
                rtrim(isnull(CODIGO_CARGO, '')) as codigo_cargo,
                isnull(INTEGRA_PLAN_PAGOS, 0) as integra_plan_pagos
            from CRD_CATALOGO_POLIZAS
            where COD_POLIZA = @PolizaLinea;";

            return DbHelper.ExecuteSingleQuery<CrPolizasRegistroPolizaRetencionData>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { PolizaLinea = (polizaLinea ?? string.Empty).Trim() });
        }

        private decimal CrPolizasRegistro_FechaProcesoActual_Obtener(
            int codEmpresa,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return 0;
            }

            var globalesResp = _mainDb.sbSifParametrosInicializa(codEmpresa, usuario.Trim());
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return 0;
            }

            return globalesResp.Result.GlngFechaCR;
        }

        private static string MapearFrecuenciaId(string frecuencia)
        {
            return (frecuencia ?? string.Empty).Trim() switch
            {
                FrecuenciaMensual => "M",
                "Trimestral" => "T",
                "Semestral" => "S",
                "Anual" => "A",
                "Indefinida" => "I",
                _ => "M"
            };
        }

        private static int CrPolizasRegistro_FrecuenciaPagosDivisor_Obtener(string frecuencia)
        {
            return (frecuencia ?? string.Empty).Trim() switch
            {
                FrecuenciaMensual => 1,
                "Trimestral" => 4,
                "Semestral" => 2,
                "Anual" => 1,
                "Indefinida" => 1,
                _ => 1
            };
        }

        private static int CrPolizasRegistro_DiferenciaMeses_Obtener(DateTime fechaInicio, DateTime fechaFin)
        {
            return ((fechaFin.Year - fechaInicio.Year) * 12) + fechaFin.Month - fechaInicio.Month;
        }

        private static int CrPolizasRegistro_PriDeduc_Crear(int anio, string mes)
        {
            int mesNumero = CrPolizasRegistro_MesNumero_Obtener(mes);
            if (anio <= 0 || mesNumero <= 0)
            {
                return 0;
            }

            return Convert.ToInt32($"{anio}{mesNumero:00}");
        }

        private static int CrPolizasRegistro_MesNumero_Obtener(string mes)
        {
            return (mes ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "ENERO" => 1,
                "FEBRERO" => 2,
                "MARZO" => 3,
                "ABRIL" => 4,
                "MAYO" => 5,
                "JUNIO" => 6,
                "JULIO" => 7,
                "AGOSTO" => 8,
                "SEPTIEMBRE" => 9,
                "OCTUBRE" => 10,
                "NOVIEMBRE" => 11,
                "DICIEMBRE" => 12,
                _ => 0
            };
        }

        private static string CrPolizasRegistro_EstadoPoliza_Obtener(string estado)
        {
            return (estado ?? string.Empty).Trim().StartsWith("A", StringComparison.OrdinalIgnoreCase)
                ? "A"
                : "I";
        }

        private static int? CrPolizasRegistro_ContratoNumero_Obtener(string contrato)
        {
            string valor = (contrato ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            return int.TryParse(valor, out int contratoNumero)
                ? contratoNumero
                : null;
        }
    }
}