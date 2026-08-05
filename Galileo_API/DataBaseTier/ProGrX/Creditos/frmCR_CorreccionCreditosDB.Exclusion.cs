using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCorreccionCreditosDb
    {
        /// <summary>Excluye una operación activa y conserva la contabilización del formulario VB6.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Operación, usuario y nota de auditoría.</param>
        /// <returns>Resultado de la exclusión y comprobante generado.</returns>
        public ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Operacion_Excluir(
            int codEmpresa,
            CrCorreccionCreditosExcluirRequest request)
        {
            if (request is null || request.operacion <= 0)
                return CR_CorreccionCreditos_Exclusion_Error("Debe indicar una operación válida.");

            request.usuario = (request.usuario ?? string.Empty).Trim();
            request.notas = CR_CorreccionCreditos_Texto_Limitar((request.notas ?? string.Empty).Trim(), 500);
            if (string.IsNullOrWhiteSpace(request.usuario) || string.IsNullOrWhiteSpace(request.notas))
                return CR_CorreccionCreditos_Exclusion_Error("Debe indicar usuario y nota de exclusión.");

            var globales = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, request.usuario);
            if (globales.Code != 0 || globales.Result is null)
                return CR_CorreccionCreditos_Exclusion_Error(
                    globales.Description ?? "No fue posible obtener Globales.");

            var operacionResponse = CR_CorreccionCreditos_OperacionBase_Obtener(
                codEmpresa,
                request.operacion,
                globales.Result.GlngFechaCR);
            if (operacionResponse.Code != 0 || operacionResponse.Result is null)
                return CR_CorreccionCreditos_Exclusion_Error("La operación no se encuentra activa.");

            try
            {
                using var conn = _portalDb.CreateConnection(codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();
                var resultado = operacionResponse.Result.retencion
                    ? CR_CorreccionCreditos_Retencion_Excluir(conn, tx, request)
                    : CR_CorreccionCreditos_Credito_Excluir(
                        codEmpresa,
                        conn,
                        tx,
                        request,
                        operacionResponse.Result,
                        globales.Result.SysPlanPagos,
                        globales.Result.GEnlace,
                        globales.Result.GOficinaTitular,
                        globales.Result.GlngFechaCR);
                tx.Commit();

                MCredito.SbBitacoraCredito(_portalDb, codEmpresa, new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = request.usuario,
                    movimiento = operacionResponse.Result.retencion ? "07" : "14",
                    detalle = $"Saldo: {operacionResponse.Result.saldo:N2}",
                    tipo = operacionResponse.Result.retencion ? "R" : "C",
                    operacion = request.operacion,
                    codigo = operacionResponse.Result.codigo,
                    notas = request.notas
                });
                CR_CorreccionCreditos_Reporte_Adjuntar(codEmpresa, request.usuario, resultado);
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (DbException ex)
            {
                return CR_CorreccionCreditos_Exclusion_Error(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return CR_CorreccionCreditos_Exclusion_Error(ex.Message);
            }
        }

        /// <summary>Excluye un crédito ordinario y genera su nota de crédito.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos de exclusión.</param>
        /// <param name="operacion">Datos actuales de la operación.</param>
        /// <param name="sysPlanPagos">Indicador de planes de pago.</param>
        /// <param name="enlace">Enlace contable.</param>
        /// <param name="oficinaTitular">Oficina titular.</param>
        /// <param name="fechaCredito">Proceso vigente de crédito.</param>
        /// <returns>Resultado con la nota de crédito.</returns>
        private CrCorreccionCreditosResultado CR_CorreccionCreditos_Credito_Excluir(
            int codEmpresa,
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosExcluirRequest request,
            CrCorreccionCreditosOperacionBase operacion,
            int sysPlanPagos,
            int enlace,
            string oficinaTitular,
            decimal fechaCredito)
        {
            const string tipoDocumento = "NC";
            var cuentaDocumento = _mRecibos.FxDocumentoCuenta(codEmpresa, tipoDocumento);
            if (string.IsNullOrWhiteSpace(cuentaDocumento))
                throw new InvalidOperationException(
                    "No se puede excluir la operación porque no existe una cuenta contable válida.");

            var numeroDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, tipoDocumento);
            var cuentas = CR_CorreccionCreditos_OperacionCuentas_Obtener(conn, tx, request.operacion);
            var aplicarRequest = new CrCorreccionCreditosAplicarRequest
            {
                operacion = request.operacion,
                usuario = request.usuario,
                notas = request.notas
            };
            CR_CorreccionCreditos_Documento_Insertar(
                conn,
                tx,
                aplicarRequest,
                operacion,
                tipoDocumento,
                numeroDocumento,
                cuentas.saldo,
                oficinaTitular,
                "CRD011",
                $"Saldo Anterior {cuentas.saldo:N2}",
                "Interés Corriente 0.00",
                "Interés Moratorio 0.00",
                $"Amortización {cuentas.saldo:N2}",
                $"Divisa: {cuentas.cod_Divisa} / Tipo Cambio: {cuentas.TipoCambio}",
                "EXCLUYE");
            CR_CorreccionCreditos_Asiento_Insertar(
                conn, tx, cuentas, tipoDocumento, numeroDocumento, cuentas.saldo,
                "C", cuentas.ctaamortiza, enlace, cuentas.cod_unidad, cuentas.cod_centro_costo);
            CR_CorreccionCreditos_Asiento_Insertar(
                conn, tx, cuentas, tipoDocumento, numeroDocumento, cuentas.saldo,
                "D", cuentaDocumento, enlace, cuentas.cod_unidad, cuentas.cod_centro_costo);

            if (sysPlanPagos == 1)
            {
                conn.Execute(@"
                    exec spCrdPlanPagoAbonoEC @Operacion,'CRD011',@Usuario,'NC',
                         @Documento,0,0,@Saldo,0,dbo.MyGetdate(),'',1;",
                    new
                    {
                        Operacion = request.operacion,
                        Usuario = request.usuario,
                        Documento = numeroDocumento,
                        Saldo = cuentas.saldo
                    }, tx);
            }
            else
            {
                CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(@"
                    update reg_creditos
                       set estado='C', saldo=0, amortiza=montoapr,
                           observacion=concat(isnull(observacion,''),@Observacion)
                     where id_solicitud=@Operacion and estado='A';",
                    new
                    {
                        Operacion = request.operacion,
                        Observacion = $" SE EXCLUYE CON N.C. # {numeroDocumento}"
                    }, tx));
                conn.Execute(@"
                    insert CREDITOS_DT
                        (CODIGO,ID_SOLICITUD,CUOTA,ABONO,INTCP,AMORTIZA,FECHAS,FECHAP,
                         TCON,NCON,ESTADO,cod_concepto,usuario,cod_Caja)
                    values
                        (@Codigo,@Operacion,0,@Saldo,0,@Saldo,dbo.MyGetdate(),@FechaCredito,
                         'NC',@Documento,'A','CRD011',@Usuario,'');",
                    new
                    {
                        Codigo = operacion.codigo,
                        Operacion = request.operacion,
                        Saldo = cuentas.saldo,
                        FechaCredito = fechaCredito,
                        Documento = numeroDocumento,
                        Usuario = request.usuario
                    }, tx);
            }

            return new CrCorreccionCreditosResultado
            {
                mensaje = $"Exclusión aplicada con Nota de Crédito #{numeroDocumento}.",
                tipo_documento = tipoDocumento,
                numero_documento = Convert.ToInt32(numeroDocumento)
            };
        }

        /// <summary>Excluye una operación perteneciente a una línea de retención.</summary>
        /// <param name="conn">Conexión abierta.</param>
        /// <param name="tx">Transacción activa.</param>
        /// <param name="request">Datos de exclusión.</param>
        /// <returns>Resultado de la exclusión.</returns>
        private static CrCorreccionCreditosResultado CR_CorreccionCreditos_Retencion_Excluir(
            IDbConnection conn,
            IDbTransaction tx,
            CrCorreccionCreditosExcluirRequest request)
        {
            CR_CorreccionCreditos_Actualizacion_Asegurar(conn.Execute(@"
                update reg_creditos
                   set estado='C', montoapr=amortiza, saldo=0
                 where id_solicitud=@Operacion and estado='A';
                delete CRD_OPERACION_PLAN_PAGOS
                 where id_solicitud=@Operacion and estado in ('A','P');
                delete CRD_OPERACION_TRANSAC
                 where id_solicitud=@Operacion and estado='A';",
                new { Operacion = request.operacion }, tx));
            return CR_CorreccionCreditos_Resultado_Exito("Exclusión aplicada y registrada en la bitácora de créditos.");
        }

        /// <summary>Crea una respuesta funcional de error para exclusión.</summary>
        /// <param name="mensaje">Descripción del error.</param>
        /// <returns>Respuesta de error homologada.</returns>
        private static ErrorDto<CrCorreccionCreditosResultado> CR_CorreccionCreditos_Exclusion_Error(
            string mensaje)
            => DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new CrCorreccionCreditosResultado());
    }
}
