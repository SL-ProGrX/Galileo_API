using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public partial class FrmCajasFndaportacionesDB
    {
        /// <summary>
        /// Aplica el aporte y registra su distribución en subcuentas
        /// sin iniciar una transacción desde el DB.
        /// </summary>
        private ErrorDto<FondosAporteAplicarResultDto?> Cajas_FNDAportaciones_Aporte_Registrar(
            int codEmpresa, FondosAporteAplicarDto request, string codOficina)
        {
            var spIniciado = false;
            var spRespondio = false;
            var subcuentasRegistradas = 0;
            string? numeroDocumento = null;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                Cajas_FNDAportaciones_Subcuentas_Validar(
                    connection,
                    request);

                spIniciado = true;

                var aplica =
                    connection.QueryFirstOrDefault<FondosAporteAplicarResultDto>(
                        @"EXEC spCajas_Fondos_Abono
                            @Operadora,
                            @Plan,
                            @Contrato,
                            @TipoDoc,
                            @Aportes,
                            @Rendimiento,
                            @Caja_Codigo,
                            @Caja_SesionId,
                            @Caja_Apertura,
                            @Caja_Tiquete,
                            @Usuario,
                            @Caja_Oficina,
                            @Notas,
                            @Documento,
                            @Deposito,
                            @ReciboDigital,
                            @GestionId;",
                        new
                        {
                            Operadora = request.operadora,
                            Plan = request.plan?.Trim(),
                            Contrato = request.contrato,
                            TipoDoc = request.tipodoc!.Trim(),
                            Aportes = request.aporte,
                            Rendimiento = 0,
                            Caja_Codigo = request.caja,
                            Caja_SesionId = request.sesionid,
                            Caja_Apertura = request.apertura,
                            Caja_Tiquete = request.tiquete,
                            Usuario = request.usuario,
                            Caja_Oficina =
                                string.IsNullOrWhiteSpace(request.oficina)
                                    ? codOficina
                                    : request.oficina,
                            Notas = request.notas ?? string.Empty,
                            Documento = string.Empty,
                            Deposito = string.Empty,
                            ReciboDigital = request.recibodigital,
                            GestionId = request.gestionid
                        });

                spRespondio = true;
                numeroDocumento = aplica?.NumDoc;

                if (aplica?.Pass != 1)
                {
                    return DbHelper.CreateErrorResponse
                        <FondosAporteAplicarResultDto?>(
                            "El procedimiento no confirmó la aplicación. " +
                            "Puede haber realizado cambios parciales; revise " +
                            "el documento, los asientos y el contrato antes " +
                            "de reintentar.",
                            -1,
                            aplica);
                }

                if (string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    return DbHelper.CreateErrorResponse
                        <FondosAporteAplicarResultDto?>(
                            "El procedimiento confirmó la aplicación, pero " +
                            "no devolvió el número de documento. Revise la " +
                            "operación antes de reintentar.",
                            -1,
                            aplica);
                }

                Cajas_FNDAportaciones_Subcuentas_Registrar(
                    connection,
                    request,
                    numeroDocumento,
                    ref subcuentasRegistradas);

                return DbHelper.CreateOkResponse
                    <FondosAporteAplicarResultDto?>(aplica);
            }
            catch (InvalidOperationException ex) when (!spIniciado)
            {
                return DbHelper.CreateErrorResponse
                    <FondosAporteAplicarResultDto?>(
                        ex.Message);
            }
            catch (Exception)
            {
                if (!spIniciado)
                {
                    return DbHelper.CreateErrorResponse
                        <FondosAporteAplicarResultDto?>(
                            "No se pudo validar o iniciar el aporte. " +
                            "El procedimiento de aplicación no se ejecutó.");
                }

                if (!spRespondio)
                {
                    return DbHelper.CreateErrorResponse
                        <FondosAporteAplicarResultDto?>(
                            "Falló la ejecución de spCajas_Fondos_Abono. " +
                            "El procedimiento pudo haber registrado el " +
                            "documento, asientos o movimientos antes del " +
                            "error. Revise la operación antes de reintentar.");
                }

                return DbHelper.CreateErrorResponse
                    <FondosAporteAplicarResultDto?>(
                        $"El aporte se aplicó con el documento " +
                        $"{numeroDocumento}, pero falló el registro de " +
                        $"subcuentas. Se completaron " +
                        $"{subcuentasRegistradas} subcuenta(s); la llamada " +
                        "que falló pudo insertar el detalle sin actualizar " +
                        "su saldo. Revise el desglose antes de reintentar.");
            }
        }

        /// <summary>
        /// Valida el CDP y el desglose contra el contrato y las
        /// subcuentas activas antes de ejecutar el SP de aplicación.
        /// </summary>
        private static void Cajas_FNDAportaciones_Subcuentas_Validar(
            SqlConnection connection, FondosAporteAplicarDto request)
        {
            var contrato =
                connection.QuerySingle<FondosContratoDatosDto>(
                    @"SELECT
                          ISNULL(P.cuenta_maestra, 0) AS cuenta_maestra,
                          P.tipo_cdp,
                          C.aportes,
                          C.inversion
                      FROM fnd_contratos C
                      INNER JOIN fnd_planes P
                          ON P.cod_operadora = C.cod_operadora
                         AND P.cod_plan = C.cod_plan
                      WHERE C.cod_operadora = @operadora
                        AND C.cod_plan = @plan
                        AND C.cod_contrato = @contrato;",
                    request);

            if (contrato.tipo_cdp == 1 &&
                !Cajas_FNDAportaciones_Cdp_Valido(
                    contrato,
                    request.aporte))
            {
                throw new InvalidOperationException(
                    "El aporte del CDP debe corresponder a la " +
                    "inversión inicial y no puede repetirse.");
            }

            var cuentas = request.subcuentas ?? [];

            if (contrato.cuenta_maestra != 1)
            {
                if (cuentas.Count > 0)
                {
                    throw new InvalidOperationException(
                        "Este contrato no es una cuenta maestra.");
                }

                return;
            }

            if (cuentas.Count == 0 ||
                cuentas.Any(c => c.valorfijo < 0) ||
                cuentas.Select(c => c.idx).Distinct().Count() !=
                    cuentas.Count ||
                cuentas.Sum(c => c.valorfijo) != request.aporte)
            {
                throw new InvalidOperationException(
                    "El desglose de subcuentas debe coincidir con el " +
                    "aporte y no contener valores negativos o repetidos.");
            }

            var ids = cuentas.Select(c => c.idx).ToArray();

            var activas = connection.QuerySingle<int>(
                @"SELECT COUNT(*)
                  FROM fnd_subCuentas
                  WHERE cod_operadora = @operadora
                    AND cod_plan = @plan
                    AND cod_contrato = @contrato
                    AND estado = 'A'
                    AND IdX IN @ids;",
                new
                {
                    request.operadora,
                    request.plan,
                    request.contrato,
                    ids
                });

            if (activas != ids.Length)
            {
                throw new InvalidOperationException(
                    "El desglose contiene subcuentas que no están " +
                    "activas en este contrato.");
            }
        }

        /// <summary>
        /// Un CDP recibe únicamente el aporte de su inversión inicial.
        /// </summary>
        private static bool Cajas_FNDAportaciones_Cdp_Valido(
            FondosContratoDatosDto contrato,
            decimal aporte)
            => contrato.aportes == 0 &&
               contrato.inversion == aporte;

        /// <summary>
        /// Registra directamente el detalle y acumula el aporte de
        /// cada subcuenta. Cuenta solo las llamadas terminadas.
        /// </summary>
        private static void Cajas_FNDAportaciones_Subcuentas_Registrar(
            SqlConnection connection,
            FondosAporteAplicarDto request,
            string documento,
            ref int subcuentasRegistradas)
        {
            const string sql = @"
                INSERT INTO fnd_SubCuentas_detalle
                    (idx, Cod_operadora, Cod_plan, Cod_Contrato,
                     Fecha, Monto, Fecha_Proceso, Tcon, Ncon)
                VALUES
                    (@idx, @operadora, @plan, @contrato,
                     dbo.MyGetdate(), @monto, 0, @tipo, @documento);

                UPDATE fnd_subCuentas
                SET Aportes = Aportes + @monto
                WHERE cod_operadora = @operadora
                  AND cod_plan = @plan
                  AND cod_contrato = @contrato
                  AND IdX = @idx;";

            // El proceso original conserva Fecha_Proceso = 0.
            foreach (var cuenta in
                (request.subcuentas ?? [])
                    .Where(c => c.valorfijo > 0))
            {
                connection.Execute(
                    sql,
                    new
                    {
                        cuenta.idx,
                        request.operadora,
                        request.plan,
                        request.contrato,
                        monto = cuenta.valorfijo,
                        tipo = request.tipodoc,
                        documento
                    });

                subcuentasRegistradas++;
            }
        }
    }
}