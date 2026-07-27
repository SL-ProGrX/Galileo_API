using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        private const int CrSeguimientoTramitesModulo = 3;

        /// <summary>
        /// Valida y registra la recepción de una solicitud de crédito conservando las reglas del formulario VB6.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionGuardarResult>
            Cr_SeguimientoTramites_Recepcion_Guardar(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGuardarRequest request)
        {
            var result = new CrSeguimientoTramitesRecepcionGuardarResult();
            string? mensajeRequest = Cr_SeguimientoTramites_Recepcion_Request_Validar(request);

            if (!string.IsNullOrWhiteSpace(mensajeRequest))
            {
                return DbHelper.CreateErrorResponse(mensajeRequest, -2, result);
            }

            var globalesResp = _mainDb.sbSifParametrosInicializa(codEmpresa, request.usuario.Trim());
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    result);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                CrSeguimientoTramitesRecepcionValidacion validacion =
                    Cr_SeguimientoTramites_Recepcion_Validar(
                        conn,
                        request,
                        globalesResp.Result.fxFechaServidor ?? DateTime.Today);

                if (validacion.mensajes.Count > 0)
                {
                    return DbHelper.CreateErrorResponse(
                        string.Join(Environment.NewLine, validacion.mensajes),
                        -2,
                        result);
                }

                using IDbTransaction transaction = conn.BeginTransaction();
                try
                {
                    Cr_SeguimientoTramites_Recepcion_CodigoDependencias_Actualizar(
                        conn,
                        transaction,
                        request);

                    CrSeguimientoTramitesRecepcionGuardarRaw raw =
                        Cr_SeguimientoTramites_Recepcion_Procedimiento_Ejecutar(
                            conn,
                            transaction,
                            request,
                            validacion.base_calculo,
                            globalesResp.Result.GOficinaApoyo,
                            globalesResp.Result.GOficinaTitular);

                    transaction.Commit();
                    result = Cr_SeguimientoTramites_Recepcion_Resultado_Crear(raw);
                    Cr_SeguimientoTramites_Recepcion_Bitacora_Registrar(
                        codEmpresa,
                        request.usuario,
                        result);

                    return DbHelper.CreateOkResponse(result, result.mensaje);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        private static string? Cr_SeguimientoTramites_Recepcion_Request_Validar(
            CrSeguimientoTramitesRecepcionGuardarRequest? request)
        {
            if (request is null)
            {
                return "La solicitud de recepción es inválida.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "Debe indicar el usuario.";
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return "Debe indicar la identificación.";
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return "Debe indicar la línea de crédito.";
            }

            return null;
        }

        private static void Cr_SeguimientoTramites_Recepcion_CodigoDependencias_Actualizar(
            IDbConnection conn,
            IDbTransaction transaction,
            CrSeguimientoTramitesRecepcionGuardarRequest request)
        {
            if (request.operacion <= 0
                || string.Equals(
                    request.codigo.Trim(),
                    request.codigo_anterior.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            const string sql = """
                update fiadores
                set codigo = @Codigo
                where id_solicitud = @Operacion;

                update refundiciones
                set codigor = @Codigo
                where id_solicitudr = @Operacion;

                update desembolsos
                set codigo = @Codigo
                where id_solicitud = @Operacion;
                """;

            conn.Execute(
                sql,
                new { Codigo = request.codigo.Trim(), Operacion = request.operacion },
                transaction);
        }

        private static CrSeguimientoTramitesRecepcionGuardarRaw
            Cr_SeguimientoTramites_Recepcion_Procedimiento_Ejecutar(
                IDbConnection conn,
                IDbTransaction transaction,
                CrSeguimientoTramitesRecepcionGuardarRequest request,
                string baseCalculo,
                string oficinaApoyo,
                string oficinaTitular)
        {
            bool aplicaFondo = string.Equals(
                request.garantia.Trim(),
                "Y",
                StringComparison.OrdinalIgnoreCase);
            string cuentaBancaria = string.IsNullOrWhiteSpace(request.cuenta_bancaria)
                ? "0"
                : Cr_SeguimientoTramites_Filtro_Normalizar(request.cuenta_bancaria, 40);

            var parameters = new
            {
                Operacion = request.operacion,
                Codigo = Cr_SeguimientoTramites_Filtro_Normalizar(request.codigo, 10).ToUpperInvariant(),
                Destino = Cr_SeguimientoTramites_Filtro_Normalizar(request.destino, 10),
                Garantia = Cr_SeguimientoTramites_Filtro_Normalizar(request.garantia, 10),
                EstadoSolicitud = Cr_SeguimientoTramites_Filtro_Normalizar(
                    request.estado_solicitud,
                    1),
                Cedula = Cr_SeguimientoTramites_Filtro_Normalizar(request.cedula, 20),
                request.monto,
                request.plazo,
                request.tasa,
                request.cuota,
                TasaPtsBono = request.tasa_pts_bono,
                FSolicita = request.fecha_solicitud,
                Divisa = Cr_SeguimientoTramites_Filtro_Normalizar(request.divisa, 10),
                BaseCalculo = Cr_SeguimientoTramites_Filtro_Normalizar(baseCalculo, 10),
                ComiteId = request.comite_id,
                PriDeduc = (decimal?)null,
                Observacion = Cr_SeguimientoTramites_Filtro_Normalizar(request.observacion, 2000),
                OficinaPresenta = Cr_SeguimientoTramites_Filtro_Normalizar(
                    request.oficina_presenta,
                    10),
                OficinaApoyo = Cr_SeguimientoTramites_Filtro_Normalizar(oficinaApoyo, 10),
                OficinaTitular = Cr_SeguimientoTramites_Filtro_Normalizar(oficinaTitular, 10),
                Promotor_Id = request.promotor_id,
                BancoId = request.banco_id,
                Cuenta_Bancaria = cuentaBancaria,
                EmiteTipo = Cr_SeguimientoTramites_Filtro_Normalizar(request.emite_tipo, 10),
                ProveedorId = string.Equals(
                    request.emite_tipo.Trim(),
                    "CP",
                    StringComparison.OrdinalIgnoreCase)
                        ? request.proveedor_id
                        : null,
                FndGarantia = aplicaFondo
                    ? Cr_SeguimientoTramites_Filtro_Normalizar(request.fnd_garantia, 10)
                    : string.Empty,
                FndContrato = aplicaFondo ? request.fnd_contrato : 0,
                Fecha_Vence = string.Equals(baseCalculo.Trim(), "07", StringComparison.Ordinal)
                    ? request.fecha_vence
                    : null,
                I_Exp_Digital = request.ind_expediente_digital ? 1 : 0,
                I_Pagare_Manual = request.pagare_manual ? 1 : 0,
                Formulario = request.formulario,
                I_TrasladoSalario = request.ind_traslado_salario ? 1 : 0,
                I_Deduce_Planilla = request.ind_deduce_planilla ? 1 : 0,
                Actividad_Id = Cr_SeguimientoTramites_Recepcion_Opcional_Normalizar(
                    request.actividad_id,
                    10),
                Canal_Id = Cr_SeguimientoTramites_Recepcion_Opcional_Normalizar(
                    request.canal_id,
                    10),
                IVA_Mnt = 0m,
                Usuario = Cr_SeguimientoTramites_Filtro_Normalizar(request.usuario, 30)
            };

            return conn.QuerySingle<CrSeguimientoTramitesRecepcionGuardarRaw>(
                "spCrd_SGT_Recepcion",
                parameters,
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        private static string? Cr_SeguimientoTramites_Recepcion_Opcional_Normalizar(
            string? value,
            int longitudMaxima)
        {
            string normalizado = Cr_SeguimientoTramites_Filtro_Normalizar(value, longitudMaxima);
            return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
        }

        private static CrSeguimientoTramitesRecepcionGuardarResult
            Cr_SeguimientoTramites_Recepcion_Resultado_Crear(
                CrSeguimientoTramitesRecepcionGuardarRaw raw)
        {
            bool inicial = raw.inicial == 1;
            return new CrSeguimientoTramitesRecepcionGuardarResult
            {
                operacion = raw.operacion,
                inicial = inicial,
                mensaje = inicial
                    ? "Solicitud Registrada Satisfactoriamente..."
                    : "Solicitud Actualizada Satisfactoriamente..."
            };
        }

        private void Cr_SeguimientoTramites_Recepcion_Bitacora_Registrar(
            int codEmpresa,
            string usuario,
            CrSeguimientoTramitesRecepcionGuardarResult result)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario.Trim(),
                Modulo = CrSeguimientoTramitesModulo,
                Movimiento = result.inicial ? "Registra - WEB" : "Modifica - WEB",
                DetalleMovimiento = $"Recepción de la Operación : {result.operacion}"
            });
        }
    }
}
