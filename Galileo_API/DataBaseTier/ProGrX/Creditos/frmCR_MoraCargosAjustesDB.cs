using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrMoraCargosAjustesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCredito = 3;
        private const string MensajeOperacionRequerida = "Debe indicar la operaci&oacute;n.";

        public FrmCrMoraCargosAjustesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la informacion principal de la operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
        public ErrorDto<CrMoraCargosAjustesOperacionData?> CrMoraCargosAjustes_ConsultaOperacion_Obtener(
            int codEmpresa,
            int operacionId)
        {
            const string query = @"
                select top 1
                    R.id_solicitud as operacion,
                    rtrim(isnull(R.cedula, '')) as cedula,
                    rtrim(isnull(R.codigo, '')) as codigo,
                    rtrim(isnull(S.nombre, '')) as nombre,
                    rtrim(isnull(C.descripcion, '')) as descripcion,
                    rtrim(isnull(R.documento_referido, '')) as num_documento,
                    isnull(R.opex, 0) as opex,
                    case when isnull(R.opex, 0) = 1 then 'OPEX' else '' end as opex_descripcion,
                    rtrim(isnull(X.descripcion, '')) as destinox,
                    rtrim(isnull(Y.descripcion, '')) as recursox,
                    rtrim(isnull(Ofi.descripcion, '')) as oficinax,
                    rtrim(isnull(Gar.descripcion, '')) as garantiax,
                    isnull(
                        R.plazo + datediff(
                            mm,
                            Getdate(),
                            convert(
                                datetime,
                                substring(convert(varchar(6), R.prideduc), 1, 4) + '/' +
                                substring(convert(varchar(6), R.prideduc), 5, 2) + '/28'
                            )
                        ),
                    0) as plazo_faltante
                from reg_creditos R
                inner join Catalogo C
                    on R.codigo = C.codigo
                inner join Socios S
                    on R.cedula = S.cedula
                left join CRD_GARANTIA_TIPOS Gar
                    on R.Garantia = Gar.garantia
                left join Catalogo_destinos X
                    on R.cod_destino = X.cod_destino
                left join Catalogo_grupos Y
                    on R.cod_grupo = Y.cod_grupo
                left join SIF_Oficinas Ofi
                    on R.cod_oficina_r = Ofi.cod_Oficina
                where R.estado = 'A'
                  and R.proceso <> 'J'
                  and R.id_solicitud = @OperacionId;";

            return DbHelper.ExecuteSingleQuery<CrMoraCargosAjustesOperacionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { OperacionId = operacionId });
        }

        /// <summary>
        /// Obtiene las cuotas en mora de la operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
        public ErrorDto<List<CrMoraCargosAjustesCuotasData>> CrMoraCargosAjustes_CuotasMora_Obtener(
            int codEmpresa,
            int operacionId)
        {
            if (operacionId <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionRequerida,
                    -2,
                    new List<CrMoraCargosAjustesCuotasData>());
            }

            int sysPlanPagos = CrMoraCargosAjustes_SysPlanPagos_Obtener(codEmpresa);

            string query = sysPlanPagos == 1
                ? @"
                    select
                        isnull(ID_SEQ, 0) as linea,
                        convert(varchar(20), FECHA_PROCESO) as proceso,
                        FECHA_PAGO as fecha_inicio,
                        FECHA_PAGO as fecha_corte,
                        isnull(INTCOR, 0) as int_cor,
                        isnull(INTMOR, 0) as int_mor,
                        isnull(PRINCIPAL, 0) as principal,
                        isnull(CARGOS, 0) as cargos,
                        convert(varchar(20), isnull(MORA_DIAS, 0)) as dias_mora
                    from CRD_OPERACION_TRANSAC
                    where MORA_DIAS > 0
                      and ESTADO = 'A'
                      and ID_SOLICITUD = @OperacionId
                    order by FECHA_PROCESO desc, ID_SEQ desc;"
                : @"
                    select
                        isnull(ID_MORO, 0) as linea,
                        convert(varchar(20), FECHAP) as proceso,
                        FECULT as fecha_inicio,
                        FECULT as fecha_corte,
                        isnull(INTC, 0) as int_cor,
                        isnull(INTM, 0) as int_mor,
                        isnull(AMORTIZA, 0) as principal,
                        isnull(CARGO, 0) as cargos,
                        'N/A' as dias_mora
                    from morosidad
                    where estado = 'A'
                      and id_solicitud = @OperacionId
                    order by FECHAP desc, ID_MORO desc;";

            return DbHelper.ExecuteListQuery<CrMoraCargosAjustesCuotasData>(
                _portalDb,
                codEmpresa,
                query,
                new { OperacionId = operacionId });
        }

        /// <summary>
        /// Obtiene los cargos de la operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacionId"></param>
        /// <returns></returns>
        public ErrorDto<List<CrMoraCargosAjustesCargosData>> CrMoraCargosAjustes_Cargos_Obtener(
            int codEmpresa,
            int operacionId)
        {
            if (operacionId <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeOperacionRequerida,
                    -2,
                    new List<CrMoraCargosAjustesCargosData>());
            }

            int sysPlanPagos = CrMoraCargosAjustes_SysPlanPagos_Obtener(codEmpresa);

            string query = sysPlanPagos == 1
                ? @"
                    select
                        isnull(C.LINEA, 0) as id_cargo,
                        convert(varchar(20), M.FECHA_PROCESO) as proceso,
                        C.FECHA as registro_fecha,
                        rtrim(isnull(C.USUARIO, '')) as registro_usuario,
                        isnull(C.MONTO, 0) as monto,
                        isnull(C.MONTO, 0) as saldo,
                        rtrim(isnull(C.Detalle, '')) as notas,
                        isnull(M.ID_SEQ, 0) as id_mora
                    from CRD_OPERACION_TRANSAC M
                    inner join CRD_OPERACION_TRANSAC_CARGOS C
                        on M.ID_SOLICITUD = C.ID_SOLICITUD
                       and M.ID_SEQ = C.ID_SEQ
                    where M.CARGOS > 0
                      and M.ESTADO = 'A'
                      and C.MOV_MONTO = 0
                      and M.ID_SOLICITUD = @OperacionId
                    order by C.LINEA desc;"
                : @"
                    select
                        isnull(C.id_cargo, 0) as id_cargo,
                        convert(varchar(20), M.FechaP) as proceso,
                        C.fecha as registro_fecha,
                        rtrim(isnull(C.usuario, '')) as registro_usuario,
                        isnull(C.monto, 0) as monto,
                        isnull(C.monto, 0) as saldo,
                        rtrim(isnull(G.DESCRIPCION, '')) as notas,
                        isnull(C.id_moro, 0) as id_mora
                    from MOROSIDAD_CARGOS C
                    inner join CBR_GESTIONES G
                        on C.COD_GESTION = G.COD_GESTION
                    inner join Morosidad M
                        on M.id_Moro = C.id_Moro
                    where M.ESTADO = 'A'
                      and M.ID_SOLICITUD = @OperacionId
                    order by C.id_cargo desc;";

            return DbHelper.ExecuteListQuery<CrMoraCargosAjustesCargosData>(
                _portalDb,
                codEmpresa,
                query,
                new { OperacionId = operacionId });
        }

        /// <summary>
        /// Aplica el ajuste de fecha documento al plan de pagos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrMoraCargosAjustes_Fecha_Aplicar(
            int codEmpresa,
            CrMoraCargosAjustesFechaRequest request)
        {
            request.usuario = CrMoraCargosAjustes_NormalizarTexto(request.usuario);

            if (request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MensajeOperacionRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);
            }

            var operacionBaseResp = CrMoraCargosAjustes_OperacionBase_Obtener(codEmpresa, request.operacion);
            if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
            {
                return DbHelper.ErrorResponse(
                    operacionBaseResp.Description ?? "No se encontr&oacute; la operaci&oacute;n activa.",
                    operacionBaseResp.Code.GetValueOrDefault(-1));
            }

            const string query = @"exec spCrdPlanPagosMoraActualizaOp @OperacionId, @FechaDoc;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    OperacionId = request.operacion,
                    FechaDoc = request.fecha_documento
                });

            if (resp.Code != 0)
            {
                return resp;
            }

            MCredito.SbBitacoraCredito(
                _portalDb,
                codEmpresa,
                new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = request.usuario,
                    movimiento = "06",
                    detalle = "Ajusta seg&uacute;n Fecha Documento",
                    tipo = "C",
                    operacion = request.operacion,
                    codigo = operacionBaseResp.Result.codigo,
                    notas = $"Fecha de Corte del Documento : {request.fecha_documento:dd/MM/yyyy}"
                });

            return new ErrorDto
            {
                Code = 0,
                Description = "Ajuste de Fecha de Documento en Plan de Pagos Realizado Satisfactoriamente...!"
            };
        }

        /// <summary>
        /// Elimina la mora de las cuotas seleccionadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrMoraCargosAjustes_CuotasMora_Eliminar(
            int codEmpresa,
            CrMoraCargosAjustesCuotasEliminarRequest request)
        {
            request.usuario = CrMoraCargosAjustes_NormalizarTexto(request.usuario);
            request.notas = CrMoraCargosAjustes_NormalizarTexto(request.notas);

            return CrMoraCargosAjustes_Eliminar_Ejecutar(
                new CrMoraCargosAjustesEliminarContext<CrMoraCargosAjustesCuotasData>
                {
                    CodEmpresa = codEmpresa,
                    Operacion = request.operacion,
                    Usuario = request.usuario,
                    Notas = request.notas,
                    Lista = request.lista,
                    MensajeExito = "Reversiones realizadas Satisfactoriamente...",
                    EjecutarEliminacion = CrMoraCargosAjustes_Cuota_Eliminar,
                    RegistrarBitacora = CrMoraCargosAjustes_Cuota_Bitacora
                });
        }

        /// <summary>
        /// Elimina los cargos seleccionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrMoraCargosAjustes_Cargos_Eliminar(
            int codEmpresa,
            CrMoraCargosAjustesCargosEliminarRequest request)
        {
            request.usuario = CrMoraCargosAjustes_NormalizarTexto(request.usuario);
            request.notas = CrMoraCargosAjustes_NormalizarTexto(request.notas);

            return CrMoraCargosAjustes_Eliminar_Ejecutar(
                new CrMoraCargosAjustesEliminarContext<CrMoraCargosAjustesCargosData>
                {
                    CodEmpresa = codEmpresa,
                    Operacion = request.operacion,
                    Usuario = request.usuario,
                    Notas = request.notas,
                    Lista = request.lista,
                    MensajeExito = "Reversi&oacute;n realizada Satisfactoriamente...",
                    EjecutarEliminacion = CrMoraCargosAjustes_Cargo_Eliminar,
                    RegistrarBitacora = CrMoraCargosAjustes_Cargo_Bitacora
                });
        }

        private ErrorDto CrMoraCargosAjustes_Eliminar_Ejecutar<T>(
            CrMoraCargosAjustesEliminarContext<T> context)
        {
            var validacion = CrMoraCargosAjustes_Eliminar_Validar(
                context.Operacion,
                context.Usuario,
                context.Notas,
                context.Lista?.Count ?? 0);

            if (validacion is not null)
            {
                return validacion;
            }

            var operacionBaseResp = CrMoraCargosAjustes_OperacionBase_Obtener(
                context.CodEmpresa,
                context.Operacion);

            if (operacionBaseResp.Code != 0 || operacionBaseResp.Result is null)
            {
                return DbHelper.ErrorResponse(
                    operacionBaseResp.Description ?? "No se encontr&oacute; la operaci&oacute;n activa.",
                    operacionBaseResp.Code.GetValueOrDefault(-1));
            }

            int sysPlanPagos = CrMoraCargosAjustes_SysPlanPagos_Obtener(context.CodEmpresa);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, context.CodEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                foreach (var item in context.Lista!)
                {
                    context.EjecutarEliminacion(conn, tx, item, sysPlanPagos, context.Operacion);
                }

                tx.Commit();

                foreach (var item in context.Lista)
                {
                    context.RegistrarBitacora(item, operacionBaseResp.Result, context);
                }

                return new ErrorDto
                {
                    Code = 0,
                    Description = context.MensajeExito
                };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private void CrMoraCargosAjustes_Cuota_Eliminar(
            IDbConnection conn,
            IDbTransaction tx,
            CrMoraCargosAjustesCuotasData item,
            int sysPlanPagos,
            int operacion)
        {
            if (sysPlanPagos == 1)
            {
                const string sqlPlan = @"
                    update CRD_OPERACION_TRANSAC
                       set mora_dias = 0,
                           intMor = 0
                     where ID_SEQ = @Linea
                       and id_solicitud = @Operacion;

                    update CRD_OPERACION_PLAN_PAGOS
                       set mora_dias = 0,
                           intMor = 0
                     where ID_SEQ = @Linea
                       and id_solicitud = @Operacion;";

                conn.Execute(sqlPlan, new
                {
                    Linea = item.linea,
                    Operacion = operacion
                }, tx);

                return;
            }

            const string sqlMora = @"
                update morosidad
                   set estado = 'N'
                 where id_moro = @Linea;";

            conn.Execute(sqlMora, new
            {
                Linea = item.linea
            }, tx);
        }

        private void CrMoraCargosAjustes_Cargo_Eliminar(
            IDbConnection conn,
            IDbTransaction tx,
            CrMoraCargosAjustesCargosData item,
            int sysPlanPagos,
            int operacion)
        {
            if (sysPlanPagos == 1)
            {
                const string sqlPlan = @"
                    delete CRD_OPERACION_TRANSAC_CARGOS
                     where Linea = @IdCargo
                       and id_seq = @IdMora
                       and id_solicitud = @Operacion;

                    update CRD_OPERACION_TRANSAC
                       set Cargos = Cargos - @Monto
                     where id_seq = @IdMora
                       and id_solicitud = @Operacion;

                    update CRD_OPERACION_PLAN_PAGOS
                       set Cargos = Cargos - @Monto
                     where id_seq = @IdMora
                       and id_solicitud = @Operacion;";

                conn.Execute(sqlPlan, new
                {
                    IdCargo = item.id_cargo,
                    IdMora = item.id_mora,
                    Monto = item.monto,
                    Operacion = operacion
                }, tx);

                return;
            }

            const string sqlMora = @"
                delete morosidad_cargos
                 where id_cargo = @IdCargo;

                update morosidad
                   set Cargo = Cargo - @Monto
                 where id_moro = @IdMora;";

            conn.Execute(sqlMora, new
            {
                IdCargo = item.id_cargo,
                IdMora = item.id_mora,
                Monto = item.monto
            }, tx);
        }

        private void CrMoraCargosAjustes_Cuota_Bitacora(
            CrMoraCargosAjustesCuotasData item,
            CrMoraCargosAjustesOperacionBaseData operacionBase,
            CrMoraCargosAjustesEliminarContext<CrMoraCargosAjustesCuotasData> context)
        {
            MCredito.SbBitacoraCredito(
                _portalDb,
                context.CodEmpresa,
                new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = context.Usuario,
                    movimiento = "06",
                    detalle = $"Id..:{item.linea}",
                    tipo = "C",
                    operacion = context.Operacion,
                    codigo = operacionBase.codigo,
                    notas = $"Int.Mor..: {item.int_mor}   Dias..: {item.dias_mora}    Notas..: {context.Notas}"
                });

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = context.CodEmpresa,
                Usuario = context.Usuario.ToUpperInvariant(),
                DetalleMovimiento = $"Morosidad OP: {context.Operacion} ID: {item.linea}",
                Movimiento = "Anula - WEB",
                Modulo = ModuloCredito
            });
        }

        private void CrMoraCargosAjustes_Cargo_Bitacora(
            CrMoraCargosAjustesCargosData item,
            CrMoraCargosAjustesOperacionBaseData operacionBase,
            CrMoraCargosAjustesEliminarContext<CrMoraCargosAjustesCargosData> context)
        {
            MCredito.SbBitacoraCredito(
                _portalDb,
                context.CodEmpresa,
                new MCredito.CrBitacoraCreditoRequest
                {
                    usuario = context.Usuario,
                    movimiento = "23",
                    detalle = item.notas,
                    tipo = "C",
                    operacion = context.Operacion,
                    codigo = operacionBase.codigo,
                    notas = $"Monto..: {item.monto}   Id..: {item.id_cargo}    Notas..: {context.Notas}"
                });

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = context.CodEmpresa,
                Usuario = context.Usuario.ToUpperInvariant(),
                DetalleMovimiento = $"Cargos OP: {context.Operacion} Id: {item.id_cargo} Monto..: {item.monto}",
                Movimiento = "Elimina - WEB",
                Modulo = ModuloCredito
            });
        }

        private ErrorDto<CrMoraCargosAjustesOperacionBaseData?> CrMoraCargosAjustes_OperacionBase_Obtener(
            int codEmpresa,
            int operacionId)
        {
            const string query = @"
                select top 1
                    rtrim(isnull(codigo, '')) as codigo
                from reg_creditos
                where id_solicitud = @OperacionId;";

            return DbHelper.ExecuteSingleQuery<CrMoraCargosAjustesOperacionBaseData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new { OperacionId = operacionId });
        }

        private int CrMoraCargosAjustes_SysPlanPagos_Obtener(int codEmpresa)
        {
            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, string.Empty);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return 0;
            }

            return globalesResp.Result.SysPlanPagos;
        }

        private static ErrorDto? CrMoraCargosAjustes_Eliminar_Validar(
            int operacion,
            string usuario,
            string notas,
            int totalItems)
        {
            if (operacion <= 0)
            {
                return DbHelper.ErrorResponse(MensajeOperacionRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);
            }

            if (notas.Length < 15)
            {
                return DbHelper.ErrorResponse(
                    "No ha especificado una Nota v&aacute;lida para registrar el cambio...?",
                    -2);
            }

            if (totalItems <= 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar al menos un registro para ajustar.",
                    -2);
            }

            return null;
        }

        private static string CrMoraCargosAjustes_NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private sealed class CrMoraCargosAjustesEliminarContext<T>
        {
            public int CodEmpresa { get; set; } = 0;
            public int Operacion { get; set; } = 0;
            public string Usuario { get; set; } = string.Empty;
            public string Notas { get; set; } = string.Empty;
            public IList<T>? Lista { get; set; }
            public string MensajeExito { get; set; } = string.Empty;
            public Action<IDbConnection, IDbTransaction, T, int, int> EjecutarEliminacion { get; set; } = default!;
            public Action<T, CrMoraCargosAjustesOperacionBaseData, CrMoraCargosAjustesEliminarContext<T>> RegistrarBitacora { get; set; } = default!;
        }
    }
}