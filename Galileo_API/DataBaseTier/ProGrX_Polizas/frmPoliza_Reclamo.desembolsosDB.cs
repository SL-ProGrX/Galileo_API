using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public partial class FrmPolizaReclamoDB
    {
        private sealed class PolizaReclamoDesembolsoOrigen
        {
            public string? Cedula { get; set; }
            public string? Plan { get; set; }
            public int? Contrato { get; set; }
            public int? Motivo { get; set; }
        }

        private sealed class PolizaReclamoDesembolsoSaldo
        {
            public string? Cedula { get; set; }
            public int Operadora { get; set; }
            public decimal Aportes { get; set; }
            public decimal Rendimiento { get; set; }
            public decimal? RendSaldo { get; set; }
            public DateTime? RendCorte { get; set; }
        }

        private sealed class PolizaReclamoDesembolsoContexto
        {
            public required Microsoft.Data.SqlClient.SqlConnection Connection { get; init; }
            public required PolizaReclamoDesembolsoAplicaRequest Request { get; init; }
            public required PolizaReclamoDesembolsoSaldo Saldo { get; init; }
            public required string Plan { get; init; }
            public required string Cuenta { get; init; }
            public required string Usuario { get; init; }
            public required string Oficina { get; init; }
            public required string CtaRetiros { get; init; }
            public required int CodEmpresa { get; init; }
        }

        /// <summary>
        /// Aplica la liquidación, el asiento y el traslado a Tesorería con los
        /// procedimientos existentes mediante los procedimientos existentes.
        /// </summary>
        public ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_Aplica(
            int codEmpresa, PolizaReclamoDesembolsoAplicaRequest request)
        {
            var validacionSolicitud = Poliza_Reclamo_Desembolsos_ValidarSolicitud(request);
            if (validacionSolicitud.Code < 0) return validacionSolicitud;

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                connection.Open();
                try
                {
                    var plan = request.plan.Trim();
                    var usuario = request.usuario.Trim();
                    var cuenta = request.metodo_pago == 1 ? request.cuenta.Trim() : string.Empty;
                    var origen = connection.QuerySingleOrDefault<PolizaReclamoDesembolsoOrigen>(
                        @"SELECT RTRIM(CEDULA) AS Cedula, RTRIM(COD_PLAN) AS [Plan],
                             CODIGO_FONDO AS Contrato, MOTIVO_RECLAMO AS Motivo
                      FROM dbo.VIV_RECLAMO_POLIZA 
                      WHERE Id = @ReclamoId",
                        new { ReclamoId = request.reclamo_id });
                    if (origen == null || origen.Motivo == 1 || origen.Plan != plan || origen.Contrato != request.contrato)
                        return Poliza_Reclamo_Desembolsos_Rechazar("El reclamo no permite desembolsos con ese plan y contrato.");

                    var saldo = connection.QuerySingleOrDefault<PolizaReclamoDesembolsoSaldo>(
                        @"SELECT RTRIM(C.CEDULA) AS Cedula, C.COD_OPERADORA AS Operadora,
                             ISNULL(C.APORTES,0) AS Aportes, ISNULL(C.RENDIMIENTO,0) AS Rendimiento,
                             C.REND_SALDO AS RendSaldo, C.REND_CORTE AS RendCorte
                      FROM dbo.FND_CONTRATOS C 
                      INNER JOIN dbo.FND_PLANES P ON P.COD_OPERADORA=C.COD_OPERADORA AND P.COD_PLAN=C.COD_PLAN
                      WHERE C.COD_PLAN=@Plan AND C.COD_CONTRATO=@Contrato",
                        new { Plan = plan, Contrato = request.contrato });
                    if (saldo == null || saldo.Cedula != origen.Cedula)
                        return Poliza_Reclamo_Desembolsos_Rechazar("El contrato no pertenece al titular del reclamo.");
                    if (request.monto > saldo.Aportes + saldo.Rendimiento)
                        return Poliza_Reclamo_Desembolsos_Rechazar("El contrato tiene fondos insuficientes.");

                    if (request.metodo_pago == 1 && !connection.ExecuteScalar<bool>(
                        @"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.CUENTAS_AHORROS
                      WHERE CEDULA=@Cedula AND RTRIM(CUENTA)=@Cuenta) THEN 1 ELSE 0 END",
                        new { Cedula = origen.Cedula, Cuenta = cuenta }))
                        return Poliza_Reclamo_Desembolsos_Rechazar("La cuenta de transferencia no pertenece al titular.");
                    if (!connection.ExecuteScalar<bool>(
                        @"SELECT CASE WHEN EXISTS(SELECT 1 FROM dbo.TES_BANCOS
                      WHERE ID_BANCO=@BancoId) THEN 1 ELSE 0 END",
                        new { BancoId = request.banco_id }))
                        return Poliza_Reclamo_Desembolsos_Rechazar("El banco seleccionado no existe.");

                    var oficina = connection.QueryFirstOrDefault<string>(
                        @"SELECT TOP (1) COD_OFICINA FROM dbo.SIF_OFICINAS
                      WHERE ESTADO=1 AND OFICINA_OMISION=1");
                    var ctaRetiros = connection.QueryFirstOrDefault<string>(
                        @"SELECT RTRIM(CTA_RETIROS) FROM dbo.FND_OPERADORAS
                      WHERE COD_OPERADORA=@Operadora", new { saldo.Operadora });
                    if (string.IsNullOrWhiteSpace(oficina) || string.IsNullOrWhiteSpace(ctaRetiros))
                        return Poliza_Reclamo_Desembolsos_Rechazar("Falta la configuración contable del fondo.");

                    return Poliza_Reclamo_Desembolsos_Ejecutar(new PolizaReclamoDesembolsoContexto
                    {
                        Connection = connection,
                        Request = request,
                        Saldo = saldo,
                        Plan = plan,
                        Cuenta = cuenta,
                        Usuario = usuario,
                        Oficina = oficina,
                        CtaRetiros = ctaRetiros,
                        CodEmpresa = codEmpresa
                    });
                }
                catch (Exception ex)
                {
                    return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(
                        $"Error al aplicar el desembolso: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(
                    $"Error al abrir la conexión del desembolso: {ex.Message}");
            }
        }

        /// <summary>Ejecuta los procedimientos existentes y verifica la solicitud de Tesorería.</summary>
        private ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_Ejecutar(
            PolizaReclamoDesembolsoContexto contexto)
        {
            var connection = contexto.Connection;
            var request = contexto.Request;
            var saldo = contexto.Saldo;
            var plan = contexto.Plan;
            var cuenta = contexto.Cuenta;
            var usuario = contexto.Usuario;
            var oficina = contexto.Oficina;
            var ctaRetiros = contexto.CtaRetiros;
            var codEmpresa = contexto.CodEmpresa;
            // El procedimiento compartido no devuelve el consecutivo.
            var anterior = connection.ExecuteScalar<int>(
                @"SELECT ISNULL(MAX(CONSEC),0) FROM dbo.FND_LIQUIDACION");
            var aporte = Math.Min(saldo.Aportes, request.monto);
            var rendimiento = request.monto - aporte;
            connection.Execute("dbo.spFndRetLiq_Masivo_Complemento", new
            {
                Operadora = saldo.Operadora,
                Plan = plan,
                Contrato = request.contrato,
                TipoMov = "R",
                TipoDoc = "FLIQ",
                Concepto = "FND006",
                DocRef = "",
                Aporte = aporte,
                Rendimiento = rendimiento,
                Multa = 0m,
                Notas = "Desembolso de Reclamo de Poliza",
                Usuario = usuario,
                Oficina = oficina,
                TipoGestion = "D",
                RetencionCod = "",
                Cuenta = ctaRetiros,
                BancoId = request.banco_id,
                BancoTipo = request.metodo_pago == 1 ? "TE" : "CK",
                BancoCuenta = cuenta,
                AppName = "ProGrX",
                TipoApl = "R"
            }, commandType: System.Data.CommandType.StoredProcedure);
            var liq = connection.ExecuteScalar<int>(
                @"SELECT ISNULL(MAX(CONSEC),0) FROM dbo.FND_LIQUIDACION
                      WHERE COD_OPERADORA=@Operadora AND COD_PLAN=@Plan AND COD_CONTRATO=@Contrato",
                new { saldo.Operadora, Plan = plan, Contrato = request.contrato });
            if (liq <= anterior)
                throw new InvalidOperationException("No se pudo identificar la liquidación creada.");

            if (request.monto < saldo.Aportes + saldo.Rendimiento)
            {
                var updated = connection.Execute(
                    @"UPDATE dbo.FND_CONTRATOS SET APORTES=@Aportes,
                          RENDIMIENTO=@Rendimiento, REND_SALDO=@RendSaldo, REND_CORTE=@RendCorte
                          WHERE COD_OPERADORA=@Operadora AND COD_PLAN=@Plan AND COD_CONTRATO=@Contrato",
                    new
                    {
                        Aportes = saldo.Aportes - aporte,
                        Rendimiento = saldo.Rendimiento - rendimiento,
                        saldo.RendSaldo,
                        saldo.RendCorte,
                        saldo.Operadora,
                        Plan = plan,
                        Contrato = request.contrato
                    });
                if (updated != 1)
                    throw new InvalidOperationException("No se pudo conservar el saldo restante.");
            }
            connection.Execute("dbo.spFndRetLiqAsiento", new { LiqNum = liq },
                commandType: System.Data.CommandType.StoredProcedure);
            connection.Execute("dbo.spFndRetLiqTesoreria",
                new { LiqNum = liq, Usuario = usuario, Token = "" },
                commandType: System.Data.CommandType.StoredProcedure);
            var solicitud = connection.ExecuteScalar<int?>(
                @"SELECT SOLICITUD_TESORERIA FROM dbo.FND_LIQUIDACION WHERE CONSEC=@Liq",
                new { Liq = liq });
            if (solicitud.GetValueOrDefault() <= 0)
                throw new InvalidOperationException("No se generó la solicitud de pago en Tesorería.");

            connection.Execute(@"UPDATE dbo.VIV_RECLAMO_POLIZA SET METODO_PAGO=@Metodo
                                     WHERE Id=@ReclamoId",
                new { Metodo = request.metodo_pago, ReclamoId = request.reclamo_id });

            var respuesta = new PolizaReclamoDesembolsoAplicaResponse
            {
                mensaje = $"Se aplicó la liquidación de Fondos No. {liq} y la solicitud de Tesorería No. {solicitud.GetValueOrDefault()}.",
                movimiento = "Aplica",
                liq = liq,
                solicitud_tesoreria = solicitud.GetValueOrDefault()
            };
            try
            {
                _dbBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = respuesta.mensaje,
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });
            }
            catch
            {
                // El fallo de bitácora no cambia un desembolso confirmado.
            }
            return DbHelper.CreateOkResponse(respuesta);
        }

        /// <summary>Valida los datos de entrada antes de abrir la conexión.</summary>
        private static ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_ValidarSolicitud(
            PolizaReclamoDesembolsoAplicaRequest request)
        {
            if (request == null)
                return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(PolizaReclamoConstFrm.valRequest, -2);
            var validacion = ValidarRequestBasico<PolizaReclamoDesembolsoAplicaResponse>(request.reclamo_id, request.usuario);
            if (validacion.Code < 0) return validacion;
            if (request.monto <= 0 || request.monto > 99999999999999.99m)
                return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar un monto válido.", -2);
            if (string.IsNullOrWhiteSpace(request.plan) || request.plan.Trim().Length > 10
                || request.contrato <= 0 || request.banco_id <= 0 || request.metodo_pago is not (1 or 2))
                return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Plan, contrato, banco o método de pago inválido.", -2);
            if (request.metodo_pago == 1 && (string.IsNullOrWhiteSpace(request.cuenta) || request.cuenta.Trim().Length > 30))
                return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>("Debe indicar la cuenta del titular.", -2);

            return DbHelper.CreateOkResponse<PolizaReclamoDesembolsoAplicaResponse>();
        }

        private static ErrorDto<PolizaReclamoDesembolsoAplicaResponse> Poliza_Reclamo_Desembolsos_Rechazar(
            string mensaje)
        {
            return DbHelper.CreateErrorResponse<PolizaReclamoDesembolsoAplicaResponse>(mensaje, -2);
        }
    }
}
