using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    /// <summary>
    /// Parametros de los reportes de frmPreAnalisis (formulas que VB6 enviaba a Crystal).
    /// </summary>
    public partial class FrmPreAnalisisDB
    {
        private const string TipoReporteEspecial = "E";
        private const string GarantiaAhorro = "A";
        private const string GarantiaFiduciaria = "F";
        // Wingdings: U+F06F (cuadro vacio) y U+F0FC (marca de verificacion), tal como los mostraba Crystal.
        private const string IndicadorMenor = "\uF06F";
        private const string IndicadorMayor = "\uF0FC";

        private sealed class ParametrosPrea
        {
            public int meses_transcurridos { get; set; }
            public decimal porc_fiduciarios { get; set; }
            public decimal porc_cancelado { get; set; }
        }

        private sealed class DatosSocioReporte
        {
            public DateTime fechaingreso { get; set; }
            public DateTime fecha { get; set; }
            public string? estadoactual { get; set; }
        }

        private sealed class FilaIntereses
        {
            public decimal interes { get; set; }
            public decimal monto { get; set; }
        }

        private sealed class FilaCreditoAnterior
        {
            public int prideduc { get; set; }
            public int fecult { get; set; }
        }

        /// <summary>
        /// Lista los comites con su acta actual (sbComites y cboComite_Click de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <returns>Comites con descripcion y acta.</returns>
        public ErrorDto<List<FrmPreAnalisisComiteData>> Prea_frmPreAnalisis_Comites_Lista(int codEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                const string sql = @"
                    select ID_COMITE as id_comite,
                           rtrim(DESCRIPCION) as descripcion,
                           isnull(ACTA, 0) as acta
                    from COMITES
                    order by ID_COMITE;";

                return DbHelper.CreateOkResponse(conn.Query<FrmPreAnalisisComiteData>(sql).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<FrmPreAnalisisComiteData>());
            }
        }

        /// <summary>
        /// Lista las solicitudes de un comite y acta (reporte de PreAnalisis Especial por acta).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="idComite">Identificador del comite.</param>
        /// <param name="acta">Numero de acta.</param>
        /// <returns>Numeros de solicitud.</returns>
        public ErrorDto<List<long>> Prea_frmPreAnalisis_Reporte_Solicitudes(int codEmpresa, int idComite, int acta)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                const string sql = @"
                    select id_solicitud
                    from reg_creditos
                    where Acta = @acta and Id_comite = @idComite
                    order by id_solicitud;";

                return DbHelper.CreateOkResponse(conn.Query<long>(sql, new { acta, idComite }).ToList());
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<long>());
            }
        }

        /// <summary>
        /// Calcula los parametros de los reportes de una operacion (imgPreAnalisis_Click y sbReporteEspecial de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="operacion">Numero de operacion (ID_SOLICITUD).</param>
        /// <param name="tipo">E para PreAnalisis Especial; cualquier otro valor para PreAnalisis.</param>
        /// <returns>Parametros del reporte por nombre.</returns>
        public ErrorDto<Dictionary<string, string>> Prea_frmPreAnalisis_Reporte_Parametros(
            int codEmpresa,
            long operacion,
            string tipo)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var datos = ObtenerOperacion(conn, operacion);
                if (datos == null || datos.monto_solicitado == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "# Operación Incorrecto",
                        -1,
                        new Dictionary<string, string>());
                }

                var parametros = string.Equals(tipo, TipoReporteEspecial, StringComparison.OrdinalIgnoreCase)
                    ? ParametrosEspecial(conn, datos)
                    : ParametrosAnalisis(conn, datos);

                return DbHelper.CreateOkResponse(parametros);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new Dictionary<string, string>());
            }
        }

        private static Dictionary<string, string> ParametrosAnalisis(
            IDbConnection conn,
            FrmPreAnalisisOperacionData operacion)
        {
            var fechaServidor = FechaServidor(conn);
            var fechaIngreso = FechaIngresoMembresia(conn, operacion.id_solicitud, fechaServidor);

            return new Dictionary<string, string>
            {
                ["fecha"] = fechaServidor.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                ["CATEGORIA"] = CalificacionPersona(conn, operacion.cedula),
                ["MEMBRESIA"] = CalculoMembresia(fechaIngreso, fechaServidor).ToUpperInvariant()
            };
        }

        private static Dictionary<string, string> ParametrosEspecial(
            IDbConnection conn,
            FrmPreAnalisisOperacionData operacion)
        {
            var prea = ObtenerParametrosPrea(conn);
            var fechaServidor = FechaServidor(conn);
            var fechaCalculo = FechaCalculo(conn, fechaServidor);
            var garantia = GarantiaCodigo(conn, operacion.codigo);

            var parametros = garantia == GarantiaAhorro
                ? ParametrosGarantiaAhorro(conn, operacion, prea)
                : ParametrosGarantiaOtra(conn, operacion, prea, garantia, fechaServidor);

            parametros["Intereses"] = InteresesHastaFormalizar(conn, operacion.id_solicitud, fechaServidor, fechaCalculo)
                .ToString(CultureInfo.InvariantCulture);
            parametros["Fecha"] =
                $"{fechaServidor.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} - {fechaCalculo.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
            parametros["CATEGORIA"] = CalificacionPersona(conn, operacion.cedula);

            return parametros;
        }

        private static Dictionary<string, string> ParametrosGarantiaAhorro(
            IDbConnection conn,
            FrmPreAnalisisOperacionData operacion,
            ParametrosPrea prea)
        {
            var ahorro = AhorroConsolidado(conn, operacion.cedula);

            return new Dictionary<string, string>
            {
                ["Ahorro"] = ahorro.HasValue ? (ahorro.Value * PorcentajeAhorro(conn)).ToString("N2", CultureInfo.InvariantCulture) : string.Empty,
                ["Leyenda"] = "MONTO POR AHORRO",
                ["PorcentajeCancelado"] = TextoPorcentajeCancelado(prea),
                ["PorcentajeAhorro"] = IndicadorCreditoAnterior(conn, operacion, prea.porc_cancelado)
            };
        }

        private static Dictionary<string, string> ParametrosGarantiaOtra(
            IDbConnection conn,
            FrmPreAnalisisOperacionData operacion,
            ParametrosPrea prea,
            string garantia,
            DateTime fechaServidor)
        {
            var parametros = new Dictionary<string, string>
            {
                ["Membresia"] = MembresiaTexto(conn, operacion.cedula, garantia, fechaServidor),
                ["MesesCancelados"] = $"{prea.meses_transcurridos} MESES CANCELADOS DEL CREDITO ANTERIOR",
                ["Meses18"] = IndicadorCreditoAnterior(conn, operacion, prea.meses_transcurridos)
            };

            if (garantia == GarantiaFiduciaria)
            {
                var ahorro = AhorroConsolidado(conn, operacion.cedula);
                parametros["Ahorro"] = ahorro.HasValue
                    ? (ahorro.Value * (prea.porc_fiduciarios / 100)).ToString("N2", CultureInfo.InvariantCulture)
                    : string.Empty;
                parametros["Leyenda"] = $"MONTO {prea.porc_fiduciarios.ToString(CultureInfo.InvariantCulture)}% AHORRO";
                parametros["PorcentajeCancelado"] = TextoPorcentajeCancelado(prea);
                parametros["PorcentajeAhorro"] = IndicadorCreditoAnterior(conn, operacion, prea.porc_cancelado);
            }

            return parametros;
        }

        private static string TextoPorcentajeCancelado(ParametrosPrea prea)
        {
            return $"{prea.porc_cancelado.ToString(CultureInfo.InvariantCulture)} PORCIENTO CANCELADO DEL CREDITO ANTERIOR";
        }

        private static ParametrosPrea ObtenerParametrosPrea(IDbConnection conn)
        {
            const string sql = @"
                select top 1 isnull(Meses_Transcurridos, 0) as meses_transcurridos,
                             isnull(Porc_Fiduciarios, 0) as porc_fiduciarios,
                             isnull(Porc_Cancelado, 0) as porc_cancelado
                from Pra_Parametros;";

            return conn.QueryFirstOrDefault<ParametrosPrea>(sql) ?? new ParametrosPrea();
        }

        private static DateTime FechaServidor(IDbConnection conn)
        {
            return conn.QueryFirstOrDefault<DateTime>("select dbo.MyGetdate();");
        }

        private static DateTime FechaCalculo(IDbConnection conn, DateTime fechaServidor)
        {
            const string sql = "select top 1 cr_fecha_calculo from par_ahcr;";
            return conn.QueryFirstOrDefault<DateTime?>(sql) ?? fechaServidor;
        }

        private static decimal PorcentajeAhorro(IDbConnection conn)
        {
            const string sql = "select top 1 isnull(CR_POR_AHORRO, 0) from par_ahcr;";
            return conn.QueryFirstOrDefault<decimal>(sql) / 100;
        }

        private static decimal? AhorroConsolidado(IDbConnection conn, string cedula)
        {
            const string sql = @"
                select top 1 isnull(Ahorro, 0) + isnull(Capitaliza, 0)
                from Ahorro_Consolidado
                where Cedula = @cedula;";

            return conn.QueryFirstOrDefault<decimal?>(sql, new { cedula });
        }

        private static string GarantiaCodigo(IDbConnection conn, string codigo)
        {
            const string sql = "select top 1 GARANTIA from Pra_Codigos where Codigo = @codigo;";
            var garantia = conn.QueryFirstOrDefault<string?>(sql, new { codigo });

            if (garantia == null)
            {
                var existe = conn.QueryFirstOrDefault<int>(
                    "select count(*) from Pra_Codigos where Codigo = @codigo;",
                    new { codigo }) > 0;
                return existe ? string.Empty : GarantiaAhorro;
            }

            return garantia switch
            {
                "V" => GarantiaFiduciaria,
                "F" => GarantiaFiduciaria,
                "E" => "E",
                _ => string.Empty
            };
        }

        private static string MembresiaTexto(IDbConnection conn, string cedula, string garantia, DateTime fechaServidor)
        {
            const string sqlSocio = "select top 1 FechaIngreso from Socios where Cedula = @cedula;";
            const string sqlMembresia = @"
                select top 1 Monto
                from Pra_Membresias
                where Garantia = @garantia and @meses between Desde and Hasta;";

            var fechaIngreso = conn.QueryFirstOrDefault<DateTime?>(sqlSocio, new { cedula });
            var meses = fechaIngreso.HasValue ? DiferenciaMeses(fechaIngreso.Value, fechaServidor) : 0;
            var monto = conn.QueryFirstOrDefault<decimal?>(sqlMembresia, new { garantia, meses });

            return monto.HasValue
                ? $"{meses} Meses - {monto.Value.ToString(CultureInfo.InvariantCulture)}"
                : string.Empty;
        }

        private static string IndicadorCreditoAnterior(
            IDbConnection conn,
            FrmPreAnalisisOperacionData operacion,
            decimal limiteMeses)
        {
            const string sql = @"
                select top 1 isnull(Prideduc, 0) as prideduc, isnull(Fecult, 0) as fecult
                from Reg_Creditos
                where Cedula = @cedula and Codigo = @codigo and Estado = 'A' and id_solicitud <> @operacion;";

            var fila = conn.QueryFirstOrDefault<FilaCreditoAnterior>(
                sql,
                new { operacion.cedula, operacion.codigo, operacion = operacion.id_solicitud });

            if (fila == null)
            {
                return " ";
            }

            var meses = Math.Abs(MesesAbsolutos(fila.fecult) - MesesAbsolutos(fila.prideduc));
            return meses < limiteMeses ? IndicadorMenor : IndicadorMayor;
        }

        private static int MesesAbsolutos(int aaaamm)
        {
            return ((aaaamm / 100) * 12) + (aaaamm % 100);
        }

        private static int DiferenciaMeses(DateTime desde, DateTime hasta)
        {
            return ((hasta.Year - desde.Year) * 12) + (hasta.Month - desde.Month);
        }

        private static decimal InteresesHastaFormalizar(
            IDbConnection conn,
            long operacion,
            DateTime fechaFormaliza,
            DateTime fechaCalculo)
        {
            const string sql = @"
                select top 1 isnull(interesv, [int]) as interes, isnull(montosol, 0) as monto
                from reg_creditos
                where id_solicitud = @operacion;";

            var fila = conn.QueryFirstOrDefault<FilaIntereses>(sql, new { operacion });
            if (fila == null || fechaCalculo.Date < fechaFormaliza.Date)
            {
                return 0;
            }

            var dias = Math.Abs((fechaFormaliza.Date - fechaCalculo.Date).Days) + 1;
            return fila.interes / 36000 * fila.monto * dias;
        }

        private static DateTime FechaIngresoMembresia(IDbConnection conn, long operacion, DateTime fechaServidor)
        {
            const string sql = @"
                select S.fechaingreso as fechaingreso, dbo.MyGetdate() as fecha, S.estadoactual as estadoactual
                from socios S
                inner join reg_creditos R on S.cedula = R.cedula
                where R.id_solicitud = @operacion;";

            var datos = conn.QueryFirstOrDefault<DatosSocioReporte>(sql, new { operacion });
            if (datos == null)
            {
                return fechaServidor;
            }

            return datos.estadoactual == "S" ? datos.fechaingreso : datos.fecha;
        }

        private static string CalculoMembresia(DateTime fecha, DateTime fechaServidor)
        {
            var dias = (fechaServidor.Date - fecha.Date).Days;
            var anios = 0;
            var meses = 0;

            while (dias > 365)
            {
                anios++;
                dias -= 365;
            }

            while (dias > 30)
            {
                meses++;
                dias -= 30;
            }

            var resultado = string.Empty;
            if (anios > 0)
            {
                resultado += $"{anios} año(s)";
            }

            if (meses > 0)
            {
                resultado += (resultado.Length > 0 ? ", " : string.Empty) + $"{meses} mes(es)";
            }

            if (dias > 0)
            {
                resultado += (resultado.Length > 0 ? " con " : string.Empty) + $"{dias} dia(s) ";
            }

            return resultado;
        }

        private static string CalificacionPersona(IDbConnection conn, string cedula)
        {
            var cedulaLimpia = cedula.Trim();

            const string sqlJudicial = @"
                select count(*) from reg_creditos
                where cedula = @cedula and estado = 'A' and proceso in ('J');";
            if (conn.QueryFirstOrDefault<int>(sqlJudicial, new { cedula = cedulaLimpia }) > 0)
            {
                return "D";
            }

            if (TieneTraspasoNoReversado(conn, cedulaLimpia))
            {
                return "D";
            }

            const string sqlMorosidadActual = @"
                select count(*)
                from reg_creditos R
                inner join Morosidad M on R.id_solicitud = M.id_solicitud
                where R.cedula = @cedula and R.estado = 'A' and M.estado = 'A';";
            if (conn.QueryFirstOrDefault<int>(sqlMorosidadActual, new { cedula = cedulaLimpia }) > 0)
            {
                return "C";
            }

            const string sqlMorosidadReciente = @"
                select count(*)
                from reg_creditos R
                inner join Morosidad M on R.id_solicitud = M.id_solicitud
                where R.cedula = @cedula and M.fechap >= @fechaProceso;";
            var fechaProceso = PeriodoSeisMesesAtras(conn);
            if (conn.QueryFirstOrDefault<int>(sqlMorosidadReciente, new { cedula = cedulaLimpia, fechaProceso }) > 0)
            {
                return "B";
            }

            return "A";
        }

        private static bool TieneTraspasoNoReversado(IDbConnection conn, string cedula)
        {
            const string sqlTraspasos = @"
                select id_solicitud from reg_creditos
                where cedula = @cedula and estado = 'A' and proceso in ('T');";
            const string sqlReferencias = @"
                select count(*) from reg_creditos
                where Estado = 'A' and Referencia = @idSolicitud;";

            return conn.Query<long>(sqlTraspasos, new { cedula })
                .Any(idSolicitud => conn.QueryFirstOrDefault<int>(sqlReferencias, new { idSolicitud }) > 0);
        }

        private static int PeriodoSeisMesesAtras(IDbConnection conn)
        {
            var fechaServidor = FechaServidor(conn);
            var fecha = FechaCalculo(conn, fechaServidor);
            var periodo = (fecha.Year * 100) + fecha.Month;

            for (var i = 1; i <= 5; i++)
            {
                periodo = PeriodoAnterior(periodo);
            }

            return periodo;
        }

        private static int PeriodoAnterior(int periodo)
        {
            var mes = periodo % 100;
            var anio = periodo / 100;

            return mes == 1 ? ((anio - 1) * 100) + 12 : (anio * 100) + (mes - 1);
        }
    }
}
