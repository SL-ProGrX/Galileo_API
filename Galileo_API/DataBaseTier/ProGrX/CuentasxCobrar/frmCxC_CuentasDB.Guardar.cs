using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public partial class FrmCxcCuentasDB
    {
        #region Guardar

        private sealed class CxCCuentasGuardarContext
        {
            public string usuario { get; init; } = string.Empty;
            public string cedula { get; init; } = string.Empty;
            public string cod_concepto { get; init; } = string.Empty;
            public string cod_oficina { get; init; } = string.Empty;
            public string emitir_tipo { get; init; } = string.Empty;
            public string estado { get; init; } = "R";
            public string? cedula_pagador { get; init; }
            public string? cedula_autorizado { get; init; }
            public string? notas { get; init; }
            public string? emitir_banco { get; init; }
            public string? emitir_cuenta { get; init; }
            public string? num_documento { get; init; }
            public string? cod_contrato { get; init; }
            public int plazo_dias { get; init; }
            public int freq_pago { get; init; }
            public DateTime? fecha_inicio { get; init; }
            public int adelanto_comision_apl { get; init; }
        }

        private sealed class CxCCuentasConceptoGuardarData
        {
            public int requiere_contrato { get; init; }
            public int proceso_descuento { get; init; }
        }

        private static CxCCuentasGuardarContext CrearGuardarContext(CxCCuentasSaveParams param)
        {
            return new CxCCuentasGuardarContext
            {
                usuario = NormalizarTexto(param.usuario),
                cedula = NormalizarTexto(param.cedula),
                cod_concepto = NormalizarTexto(param.cod_concepto),
                cod_oficina = NormalizarTexto(param.cod_oficina),
                emitir_tipo = NormalizarTexto(param.emitir_tipo),
                estado = string.IsNullOrWhiteSpace(param.estado) ? "R" : NormalizarMayusculas(param.estado),
                cedula_pagador = string.IsNullOrWhiteSpace(param.cedula_pagador) ? null : NormalizarTexto(param.cedula_pagador),
                cedula_autorizado = string.IsNullOrWhiteSpace(param.cedula_autorizado) ? null : NormalizarTexto(param.cedula_autorizado),
                notas = string.IsNullOrWhiteSpace(param.notas) ? null : param.notas.Trim(),
                emitir_banco = string.IsNullOrWhiteSpace(param.emitir_banco) ? null : NormalizarTexto(param.emitir_banco),
                emitir_cuenta = string.IsNullOrWhiteSpace(param.emitir_cuenta) ? null : NormalizarTexto(param.emitir_cuenta),
                num_documento = string.IsNullOrWhiteSpace(param.num_documento) ? null : param.num_documento.Trim(),
                cod_contrato = string.IsNullOrWhiteSpace(param.cod_contrato) ? null : NormalizarTexto(param.cod_contrato),
                plazo_dias = param.plazo * 30,
                freq_pago = param.chk_cta_apl ? 30 : 0,
                fecha_inicio = param.chk_cta_apl ? param.fecha_inicio : null,
                adelanto_comision_apl = param.adelanto_comision_apl ? 1 : 0
            };
        }

        private static string? ValidarGuardarRequest(CxCCuentasSaveParams param, CxCCuentasGuardarContext context)
        {
            if (string.IsNullOrWhiteSpace(context.usuario) ||
                string.IsNullOrWhiteSpace(context.cedula) ||
                string.IsNullOrWhiteSpace(context.cod_concepto) ||
                string.IsNullOrWhiteSpace(context.cod_oficina) ||
                string.IsNullOrWhiteSpace(context.emitir_tipo))
            {
                return "Faltan datos requeridos para guardar la operación.";
            }

            if (param.monto <= 0)
            {
                return "El monto debe ser mayor a cero.";
            }

            if (param.plazo <= 0)
            {
                return "El plazo debe ser mayor a cero.";
            }

            if (param.cuota <= 0)
            {
                return "La cuota debe ser mayor a cero.";
            }

            if (param.chk_cta_apl && param.fecha_inicio is null)
            {
                return "La fecha de inicio es requerida cuando aplica cuenta.";
            }

            return null;
        }

        private static CxCCuentasConceptoGuardarData? ObtenerConceptoGuardar(
            SqlConnection conn,
            string codConcepto)
        {
            const string sql = @"
                SELECT TOP 1
                    ISNULL(Requiere_Contrato, 0) AS requiere_contrato,
                    ISNULL(Proceso_Descuento, 0) AS proceso_descuento
                FROM CxC_Conceptos
                WHERE cod_Concepto = @codConcepto
                  AND Activo = 1;";

            return conn.QueryFirstOrDefault<CxCCuentasConceptoGuardarData>(
                sql,
                new { codConcepto });
        }

        private static bool ExisteContratoValidoGuardar(
            SqlConnection conn,
            string codContrato,
            string cedula,
            string codConcepto)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CxC_Contratos Cnt
                LEFT JOIN CxC_Personas_Contratos Per
                    ON Cnt.Cod_Contrato = Per.cod_contrato
                   AND Per.cedula = @cedula
                INNER JOIN CxC_Conceptos_Contratos Cc
                    ON Cnt.Cod_Contrato = Cc.cod_Contrato
                WHERE Cnt.cod_Contrato = @codContrato
                  AND Cc.cod_concepto = @codConcepto
                  AND (Per.Cedula IS NOT NULL OR Cnt.Suscripcion_Abierta = 1);";

            return conn.QuerySingleOrDefault<int>(
                sql,
                new
                {
                    codContrato,
                    cedula,
                    codConcepto
                }) > 0;
        }

        private static bool ExistePagadorValidoGuardar(
            SqlConnection conn,
            string codContrato,
            string cedula,
            string cedulaPagador)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CxC_Contratos_Pagadores Cp
                INNER JOIN CxC_Contratos Cn
                    ON Cp.Cod_Contrato = Cn.Cod_Contrato
                INNER JOIN CxC_Personas Per
                    ON Cp.cedula = Per.cedula
                LEFT JOIN CxC_Personas_Contratos_Pagadores PcP
                    ON Cp.Cod_Contrato = PcP.cod_Contrato
                   AND Cp.Cedula = PcP.cedula_Pagador
                   AND PcP.cedula = @cedula
                WHERE Cn.Cod_Contrato = @codContrato
                  AND (PcP.cedula IS NOT NULL OR Cn.Pagadores_Abierto = 1)
                  AND Cp.Cedula = @cedulaPagador;";

            return conn.QuerySingleOrDefault<int>(
                sql,
                new
                {
                    codContrato,
                    cedula,
                    cedulaPagador
                }) > 0;
        }

        private static bool ExisteAutorizadorValidoGuardar(
            SqlConnection conn,
            string cedula,
            string cedulaAutorizado)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM CXC_PERSONAS_AUTORIZADOS
                WHERE Cedula_Autorizado = @cedulaAutorizado
                  AND cedula = @cedula;";

            return conn.QuerySingleOrDefault<int>(
                sql,
                new
                {
                    cedula,
                    cedulaAutorizado
                }) > 0;
        }

        private static string ObtenerValidacionDisponibleGuardar(
            SqlConnection conn,
            string cedula,
            decimal monto,
            string codConcepto)
        {
            const string sql = @"
                SELECT dbo.fxCxC_Persona_Disponible_Valida(@cedula, @monto, @codConcepto);";

            return conn.QueryFirstOrDefault<string>(
                       sql,
                       new
                       {
                           cedula,
                           monto,
                           codConcepto
                       }) ?? string.Empty;
        }

        private static string? ValidarReglasGuardarEnDb(
            SqlConnection conn,
            CxCCuentasSaveParams param,
            CxCCuentasGuardarContext context)
        {
            var concepto = ObtenerConceptoGuardar(conn, context.cod_concepto);

            if (concepto is null)
            {
                return "No existe o no está activo el código de concepto de CxC a utilizar.";
            }

            var requiereContrato =
                concepto.requiere_contrato == 1 || concepto.proceso_descuento == 1;

            if (requiereContrato && string.IsNullOrWhiteSpace(context.cod_contrato))
            {
                return "Es necesario el uso de algún contrato activo para esta cuenta.";
            }

            if (requiereContrato &&
                !string.IsNullOrWhiteSpace(context.cod_contrato) &&
                !ExisteContratoValidoGuardar(conn, context.cod_contrato, context.cedula, context.cod_concepto))
            {
                return "El concepto de CxC requiere que exista un contrato registrado a la persona y asociado a este concepto.";
            }

            if (concepto.proceso_descuento == 1)
            {
                if (string.IsNullOrWhiteSpace(context.cedula_pagador))
                {
                    return "El pagador no está registrado bajo el contrato individual (x Persona).";
                }

                if (string.IsNullOrWhiteSpace(context.cod_contrato) ||
                    !ExistePagadorValidoGuardar(conn, context.cod_contrato, context.cedula, context.cedula_pagador))
                {
                    return "El pagador no está registrado bajo el contrato individual (x Persona).";
                }

                if (string.IsNullOrWhiteSpace(context.cedula_autorizado))
                {
                    return "No se localizó al autorizador de la cesión.";
                }

                if (!ExisteAutorizadorValidoGuardar(conn, context.cedula, context.cedula_autorizado))
                {
                    return "El autorizador no está registrado.";
                }
            }

            var mensajeDisponible = ObtenerValidacionDisponibleGuardar(
                conn,
                context.cedula,
                param.monto,
                context.cod_concepto);

            if (!string.IsNullOrWhiteSpace(mensajeDisponible))
            {
                return mensajeDisponible.Trim();
            }

            return null;
        }

        private static bool ExisteOperacionGuardar(SqlConnection conn, long operacion)
        {
            if (operacion <= 0)
            {
                return false;
            }

            return conn.QuerySingleOrDefault<int>(
                @"SELECT COUNT(1)
                  FROM CxC_Cuentas
                  WHERE Operacion = @operacion;",
                new { operacion }) > 0;
        }

        private static long ObtenerNuevaOperacionGuardar(SqlConnection conn)
        {
            return conn.QuerySingle<long>(
                @"SELECT ISNULL(MAX(Operacion), 0) + 1
                  FROM CxC_Cuentas;");
        }

        private static DynamicParameters CrearParametrosGuardar(
            long operacion,
            CxCCuentasSaveParams param,
            CxCCuentasGuardarContext context)
        {
            var parametros = new DynamicParameters();

            parametros.Add("operacion", operacion);
            parametros.Add("cedula", context.cedula);
            parametros.Add("cedula_pagador", context.cedula_pagador);
            parametros.Add("cedula_autorizado", context.cedula_autorizado);
            parametros.Add("cod_concepto", context.cod_concepto);
            parametros.Add("cod_oficina", context.cod_oficina);
            parametros.Add("notas", context.notas);
            parametros.Add("monto", param.monto);
            parametros.Add("emitir_tipo", context.emitir_tipo);
            parametros.Add("emitir_banco", context.emitir_banco);
            parametros.Add("emitir_cuenta", context.emitir_cuenta);
            parametros.Add("tasa_corriente", param.tasa_corriente);
            parametros.Add("tasa_mora", param.tasa_mora);
            parametros.Add("cuota", param.cuota);
            parametros.Add("plazo_dias", context.plazo_dias);
            parametros.Add("plazo", param.plazo);
            parametros.Add("estado", context.estado);
            parametros.Add("num_documento", context.num_documento);
            parametros.Add("cod_contrato", context.cod_contrato);
            parametros.Add("usuario", context.usuario);
            parametros.Add("adelanto_monto", param.adelanto_monto);
            parametros.Add("adelanto_porcentaje", param.adelanto_porcentaje);
            parametros.Add("adelanto_comision_apl", context.adelanto_comision_apl);
            parametros.Add("adelanto_comision", param.adelanto_comision);
            parametros.Add("adelanto_comision_dias", param.adelanto_comision_dias);
            parametros.Add("freq_pago", context.freq_pago);
            parametros.Add("fecha_inicio", context.fecha_inicio);

            return parametros;
        }

        private static void InsertarOperacionGuardar(SqlConnection conn, DynamicParameters parametros)
        {
            const string sqlInsert = @"
                INSERT INTO CxC_Cuentas
                (
                    OPERACION,
                    CEDULA,
                    CEDULA_PAGADOR,
                    COD_CONCEPTO,
                    COD_OFICINA,
                    NOTAS,
                    MONTO,
                    SALDO,
                    REBAJOS_TOTAL,
                    EMITIR_TIPO,
                    EMITIR_BANCO,
                    EMITIR_CUENTA,
                    DESEMBOLSO_MONTO,
                    TIPO_PLAZO,
                    TASA_CORRIENTE,
                    TASA_MORA,
                    CUOTA,
                    DIAS_PLAZO,
                    PLAZO,
                    AMORTIZA,
                    INTERESC,
                    ESTADO,
                    NUM_DOCUMENTO,
                    COD_CONTRATO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO,
                    FECHA_ULTMOV,
                    AUTORIZA_ESTADO,
                    ADELANTO_MONTO,
                    ADELANTO_PORCENTAJE,
                    DESEMBOLSO_REALIZADO,
                    DESEMBOLSO_PENDIENTE,
                    CEDULA_AUTORIZADO,
                    ADELANTO_COMISION_APL,
                    ADELANTO_COMISION,
                    ADELANTO_COMISION_DIAS,
                    FREQ_PAGO,
                    FECHA_INICIO
                )
                VALUES
                (
                    @operacion,
                    @cedula,
                    @cedula_pagador,
                    @cod_concepto,
                    @cod_oficina,
                    @notas,
                    @monto,
                    @monto,
                    0,
                    @emitir_tipo,
                    @emitir_banco,
                    @emitir_cuenta,
                    @monto,
                    'M',
                    @tasa_corriente,
                    @tasa_mora,
                    @cuota,
                    @plazo_dias,
                    @plazo,
                    0,
                    0,
                    'R',
                    @num_documento,
                    @cod_contrato,
                    dbo.MyGetdate(),
                    @usuario,
                    dbo.MyGetdate(),
                    'P',
                    @adelanto_monto,
                    @adelanto_porcentaje,
                    0,
                    0,
                    @cedula_autorizado,
                    @adelanto_comision_apl,
                    @adelanto_comision,
                    @adelanto_comision_dias,
                    @freq_pago,
                    @fecha_inicio
                );";

            conn.Execute(sqlInsert, parametros);
        }

        private static void ActualizarOperacionGuardar(SqlConnection conn, DynamicParameters parametros)
        {
            const string sqlUpdate = @"
                UPDATE CxC_Cuentas
                SET
                    CEDULA_PAGADOR = @cedula_pagador,
                    CEDULA_AUTORIZADO = @cedula_autorizado,
                    COD_CONCEPTO = @cod_concepto,
                    COD_OFICINA = @cod_oficina,
                    NOTAS = @notas,
                    MONTO = @monto,
                    EMITIR_TIPO = @emitir_tipo,
                    EMITIR_BANCO = @emitir_banco,
                    EMITIR_CUENTA = @emitir_cuenta,
                    TASA_CORRIENTE = @tasa_corriente,
                    TASA_MORA = @tasa_mora,
                    CUOTA = @cuota,
                    COD_CONTRATO = @cod_contrato,
                    ESTADO = @estado,
                    NUM_DOCUMENTO = @num_documento,
                    DESEMBOLSO_MONTO = @monto,
                    DIAS_PLAZO = @plazo_dias,
                    PLAZO = @plazo,
                    ADELANTO_MONTO = @adelanto_monto,
                    ADELANTO_PORCENTAJE = @adelanto_porcentaje,
                    ADELANTO_COMISION_APL = @adelanto_comision_apl,
                    ADELANTO_COMISION = @adelanto_comision,
                    ADELANTO_COMISION_DIAS = @adelanto_comision_dias,
                    FREQ_PAGO = @freq_pago,
                    FECHA_INICIO = @fecha_inicio
                WHERE Operacion = @operacion;";

            conn.Execute(sqlUpdate, parametros);
        }

        private static void RecalcularOperacionGuardada(
            SqlConnection conn,
            long operacion,
            string usuario,
            decimal monto)
        {
            var facturasRegistradas = conn.QuerySingleOrDefault<int>(
                @"SELECT COUNT(1)
                  FROM CxC_Cuentas
                  WHERE Operacion = @operacion;",
                new { operacion });

            if (facturasRegistradas > 0)
            {
                conn.Execute(
                    @"exec spCxC_Operacion_Facturas_Actualiza @operacion, 0, @usuario;",
                    new { operacion, usuario });

                return;
            }

            conn.Execute(
                @"exec spCxC_CuentaCargosActualiza @operacion, @monto;",
                new { operacion, monto });
        }

        /// <summary>
        /// Guarda una operación de CxC en modo inserción o actualización.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="param">Datos de la operación.</param>
        /// <returns>Número de operación guardada.</returns>
        public ErrorDto<long> CxCCuentas_Guardar(int codEmpresa, CxCCuentasSaveParams param)
        {
            var response = new ErrorDto<long>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            if (param is null)
            {
                response.Code = -1;
                response.Description = "Los datos de la operación son requeridos.";
                return response;
            }

            var context = CrearGuardarContext(param);
            var mensajeValidacion = ValidarGuardarRequest(param, context);

            if (!string.IsNullOrWhiteSpace(mensajeValidacion))
            {
                response.Code = -1;
                response.Description = mensajeValidacion;
                return response;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var mensajeReglasDb = ValidarReglasGuardarEnDb(conn, param, context);

                if (!string.IsNullOrWhiteSpace(mensajeReglasDb))
                {
                    response.Code = -1;
                    response.Description = mensajeReglasDb;
                    response.Result = 0;
                    return response;
                }

                var existeOperacion = ExisteOperacionGuardar(conn, param.operacion);
                var operacion = existeOperacion ? param.operacion : ObtenerNuevaOperacionGuardar(conn);
                var parametros = CrearParametrosGuardar(operacion, param, context);

                if (existeOperacion)
                {
                    ActualizarOperacionGuardar(conn, parametros);
                }
                else
                {
                    InsertarOperacionGuardar(conn, parametros);
                }

                RecalcularOperacionGuardada(conn, operacion, context.usuario, param.monto);
                response.Result = operacion;
            }
            catch (DbException ex)
            {
                response.Code = -1;
                response.Description = $"No fue posible guardar la operación. {ex.Message}";
                response.Result = 0;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error inesperado al guardar la operación. {ex.Message}";
                response.Result = 0;
            }

            return response;
        }

        #endregion
    }
}
