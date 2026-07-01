using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier
{
    public class MCobroDb
    {
        private readonly PortalDB _portalDB;
        private readonly IConfiguration _config;

        public MCobroDb(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
        }

        public decimal fxFechaProcesoAnterior(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoAnt(@proceso) as Result";

            var parametros = new
            {
                proceso = pProceso
            };

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB,
                CodEmpresa,
                sql,
                0,
                parametros
            ).Result;
        }

        public decimal fxFechaProcesoSiguiente(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoSig(@proceso) as Result";

            var parametros = new
            {
                proceso = pProceso
            };

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB,
                CodEmpresa,
                sql,
                0,
                parametros
            ).Result;
        }

        public static decimal fxCalcula_Cuota(decimal Monto, int Plazo, object Interes, string? Frecuencia = "M")
        {
            if (Plazo <= 0)
                return 0m;

            var tasa = Convert.ToDecimal(Interes);

            if (tasa == 0m)
                return Math.Round(Monto / Plazo, 2);

            decimal interesPeriodo;

            switch ((Frecuencia ?? "M").Trim().ToUpperInvariant())
            {
                case "Q":
                    interesPeriodo = tasa / (24m * 100m);
                    break;

                default:
                    interesPeriodo = tasa / (12m * 100m);
                    break;
            }

            decimal factor = 1m;

            for (int i = 1; i <= Plazo; i++)
                factor *= (1m + interesPeriodo);

            if (factor == 1m)
                return Math.Round(Monto / Plazo, 2);

            var cuota = Monto * interesPeriodo * factor / (factor - 1m);

            return Math.Round(cuota, 2);
        }

        public DateTime fxFechaCalculo(int codEmpresa, string? pLinea = "", decimal pPriDeduc = 0, int pDiaPago = 32)
        {
            return new MSeguimientoDB(_config)
                .fxFechaCalculo(codEmpresa, pLinea, pPriDeduc, pDiaPago);
        }

        public static string fxDescribeEstado(string? strEstado)
        {
            var e = (strEstado ?? string.Empty).Trim().ToUpperInvariant();

            return e switch
            {
                "A" => "Activo",
                "N" => "Anulado",
                "C" => "Cancelado",
                _ => ""
            };
        }

        public string fxDescribeCodigo(int CodEmpresa, string strCodigo)
        {
            strCodigo = (strCodigo ?? string.Empty).Trim();

            if (strCodigo.Length == 0)
                return string.Empty;

            const string sql = @"
                select rtrim(descripcion)
                from catalogo
                where codigo = @codigo";

            var result = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty,
                new { codigo = strCodigo }
            );

            if (result.Code != 0)
                return string.Empty;

            return (result.Result ?? string.Empty).Trim();
        }

        public string fxCBRParametro(int CodEmpresa, string pParametro)
        {
            pParametro = (pParametro ?? string.Empty).Trim();

            if (pParametro.Length == 0)
                return string.Empty;

            const string sql = @"
                select rtrim(valor)
                from cbr_parametros
                where cod_parametro = @Parametro";

            var result = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty,
                new { Parametro = pParametro }
            );

            if (result.Code != 0)
                return string.Empty;

            return (result.Result ?? string.Empty).Trim();
        }

        public int fxCBRPlazoRestante(int CodEmpresa, long pOperacion)
        {
            const string sql = @"
                select
                    isnull(plazo,0) as Plazo,
                    isnull(prideduc,0) as PriDeduc
                from reg_creditos
                where id_solicitud = @Operacion";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();

            var row = conn.QueryFirstOrDefault<CbrPlazoRestanteRow>(
                sql,
                new { Operacion = pOperacion }
            );

            if (row == null)
                return 1;

            decimal primerDeduccionActual =
                new MSeguimientoDB(_config).fxPrimerDeduccion(CodEmpresa);

            string priDeducTexto =
                row.PriDeduc.ToString("0", CultureInfo.InvariantCulture);

            if (priDeducTexto.Length < 6)
                return 1;

            int anio = int.Parse(priDeducTexto.Substring(0, 4), CultureInfo.InvariantCulture);
            int mes = int.Parse(priDeducTexto.Substring(4, 2), CultureInfo.InvariantCulture);

            int contador = 0;

            long resultado =
                Convert.ToInt64($"{anio}{mes:00}", CultureInfo.InvariantCulture);

            while (primerDeduccionActual >= resultado)
            {
                if (mes == 12)
                {
                    mes = 1;
                    anio++;
                }
                else
                {
                    mes++;
                }

                contador++;

                resultado =
                    Convert.ToInt64($"{anio}{mes:00}", CultureInfo.InvariantCulture);
            }

            contador = row.Plazo - contador;
            contador = contador + 1;

            if (contador <= 0)
                contador = 1;

            return contador;
        }

        private sealed class CbrPlazoRestanteRow
        {
            public int Plazo { get; set; } = 0;
            public decimal PriDeduc { get; set; } = 0m;
        }
        public static string fxFechaProcesoFormat(decimal pProceso)
        {
            long procesoBase = Convert.ToInt64(decimal.Truncate(pProceso));
            string procesoTexto = procesoBase.ToString("000000", CultureInfo.InvariantCulture);
            string resultado = $"{procesoTexto.Substring(0, 4)}-{procesoTexto.Substring(4, 2)}";

            decimal diferencia = Math.Round(pProceso - decimal.Truncate(pProceso), 1, MidpointRounding.AwayFromZero);

            if (diferencia == 0.1m)
            {
                resultado += "_Q1";
            }

            if (diferencia == 0.2m)
            {
                resultado += "_Q2";
            }

            return resultado;
        }


        /// <summary>
        /// Homologa la descripción del tipo de comprobante siguiendo la lógica VB6 de mCobro.
        /// </summary>
        public static string fxTipoComprobante(
            string? tipo,
            string? nCon = "0",
            string? operacion = "0")
        {
            var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            var numeroConcepto = string.IsNullOrWhiteSpace(nCon) ? "0" : nCon.Trim();
            var numeroOperacion = string.IsNullOrWhiteSpace(operacion) ? "0" : operacion.Trim();

            return tipoNormalizado switch
            {
                "1" => "Ded.Pla",
                "2" or "RE" => "Recibo",
                "3" => "Refundición",
                "4" => fxTipoComprobanteCasoCuatro(numeroConcepto, numeroOperacion),
                "5" => "Liq.Ahorros",
                "6" => "Liq.Reversión",
                "7" or "NC" => "Nota de Crédito",
                "8" or "ND" => "Nota de Débito",
                _ => tipoNormalizado
            };
        }

        private static string fxTipoComprobanteCasoCuatro(
            string numeroConcepto,
            string numeroOperacion)
        {
            if (numeroConcepto == numeroOperacion)
                return "Traspaso Deuda";

            return numeroConcepto switch
            {
                "8889" => "Readecuación",
                "8888" => "Traspaso Deuda",
                _ => "Apl.Deudas"
            };
        }
        public decimal fxCuotaPolizaVida(int CodEmpresa, decimal monto, string codigo = "")
        {
            const string sql = @"
        select dbo.fxCrd_CuotaPolizaVida_Calculo(@monto, @codigo) as Result;";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB,
                CodEmpresa,
                sql,
                0m,
                new
                {
                    monto,
                    codigo = (codigo ?? string.Empty).Trim()
                }
            ).Result;
        }

        public static decimal fxInteresesDiasPrimerCuota(DateTime fecha, decimal monto, decimal tasa)
        {
            var anio = fecha.Year;
            var mes = fecha.Month;

            if (mes == 12)
            {
                mes = 1;
                anio++;
            }
            else
            {
                mes++;
            }

            var fechaCalculo = new DateTime(
                anio,
                mes,
                1,
                0,
                0,
                0,
                DateTimeKind.Unspecified)
                .AddDays(-1);

            var dias = Math.Abs((fechaCalculo.Date - fecha.Date).Days) + 1;

            return (tasa / 36000m) * monto * dias;
        }

        public decimal fxMontoEnGeneral(int CodEmpresa, long operacion)
        {
            const string sql = @"
        select dbo.fxCrdSGTMontoDeducciones(@operacion) as Result;";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB,
                CodEmpresa,
                sql,
                0m,
                new { operacion }
            ).Result;
        }

        public bool fxCobraTasaFormaliza(
            int CodEmpresa,
            string codigo,
            string codDestino)
        {
            const string sql = @"
        select dbo.fxCrd_Calcula_Int_Formalizacion(@codigo, @codDestino) as Result;";

            var result = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                CodEmpresa,
                sql,
                0,
                new
                {
                    codigo = (codigo ?? string.Empty).Trim(),
                    codDestino = (codDestino ?? string.Empty).Trim()
                }
            ).Result;

            return result == 1;
        }

        public DateTime fxFechaCalculoFormaliza(
     int CodEmpresa,
     string linea = "",
     decimal priDeduc = 0m,
     int diaPago = 32)
        {
            const string sql = @"
        select dbo.fxCrdFormalizaIntCorte(@linea, @priDeduc, @diaPago) as Result;";

            return DbHelper.ExecuteSingleQuery<DateTime>(
                _portalDB,
                CodEmpresa,
                sql,
                DateTime.Today,
                new
                {
                    linea = (linea ?? string.Empty).Trim(),
                    priDeduc,
                    diaPago
                }
            ).Result;
        }

        public decimal fxInteresesHastaFormalizar(int CodEmpresa,long operacion,string codigo,DateTime? fecha = null,decimal? monto = null,decimal priDeduc = 0m,int diaPago = 0)
        {
            const string sqlCredito = @"
        select 
            fechaforp,
            int as tasa_int,
            interesv,
            montoapr,
            fecha_inicio_calculo
        from reg_creditos
        where id_solicitud = @operacion;";

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (conn.State != ConnectionState.Open)
                conn.Open();

            var credito = conn.QueryFirstOrDefault<CreditoFormalizaRow>(
                sqlCredito,
                new { operacion });

            if (credito == null)
                return 0m;

            var fechaCalculo = fxFechaCalculoFormaliza(
                CodEmpresa,
                codigo,
                priDeduc,
                diaPago);

            var fechaFormaliza = fecha ?? credito.fecha_inicio_calculo ?? credito.fechaforp;
            var montoApr = monto ?? credito.montoapr;
            var tasa = credito.interesv ?? credito.tasa_int;

            if (fechaCalculo.Date < fechaFormaliza.Date)
                return 0m;

            if (fxCreditoExcedente(CodEmpresa, codigo))
            {
                const string sqlRefundido = @"
            select isnull(sum(R.Saldo),0) as Monto
            from reg_creditos R
            inner join refundiciones X
                    on R.id_solicitud = X.id_solicitud
            where X.id_solicitudr = @operacion
              and X.codigo = @codigo;";

                var montoRefundido = conn.QueryFirstOrDefault<decimal>(
                    sqlRefundido,
                    new
                    {
                        operacion,
                        codigo = (codigo ?? string.Empty).Trim()
                    });

                montoApr -= montoRefundido;

                const string sqlUpdate = @"
            update reg_creditos
            set MontoCalculo = @montoApr
            where id_solicitud = @operacion;";

                conn.Execute(sqlUpdate, new { montoApr, operacion });
            }

            var dias = Math.Abs((fechaCalculo.Date - fechaFormaliza.Date).Days) + 1;
            var interes = Math.Round((tasa / 36000m) * montoApr * dias, 2);

            return interes < 0 ? 0m : interes;
        }

        public bool fxCreditoExcedente(int CodEmpresa, string codigo)
        {
            const string sql = @"
        select rtrim(valor)
        from EXC_PARAMETROS
        where COD_PARAMETRO = '05';";

            var codigoExcedente = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                CodEmpresa,
                sql,
                string.Empty
            ).Result;

            return string.Equals(
                (codigoExcedente ?? string.Empty).Trim(),
                (codigo ?? string.Empty).Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        public sealed class CreditoFormalizaRow
        {
            public DateTime fechaforp { get; set; }
            public decimal tasa_int { get; set; }
            public decimal? interesv { get; set; }
            public decimal montoapr { get; set; }
            public DateTime? fecha_inicio_calculo { get; set; }
        }

    }

}