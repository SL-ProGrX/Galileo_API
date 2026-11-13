using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    /// <summary>
    /// Acceso a datos de frmPreAnalisis (ficha de preanalisis por operacion). Migrado desde VB6 (PGX DLL Estudio Credito).
    /// </summary>
    public partial class FrmPreAnalisisDB
    {
        private const int VModulo = 3;
        private const string TipoCuota = "C";
        private const string TipoRetencion = "R";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;

        public FrmPreAnalisisDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        private sealed class FilaObligacion
        {
            public long id_solicitud { get; set; }
            public string codigo { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public decimal saldo { get; set; }
            public decimal plazo { get; set; }
            public decimal cuota { get; set; }
            public decimal amortizado { get; set; }
            public string? poliza { get; set; }
            public string? retencion { get; set; }
            public decimal? int_c { get; set; }
            public decimal? int_m { get; set; }
            public decimal? amortiza_mora { get; set; }
        }

        /// <summary>
        /// Carga la operacion, sus obligaciones internas, el registro de preanalisis y los fiadores (txtOperacion_LostFocus de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="operacion">Numero de operacion (ID_SOLICITUD).</param>
        /// <returns>Datos de la operacion; la operacion viene nula cuando no existe.</returns>
        public ErrorDto<FrmPreAnalisisCargarResponse> Prea_frmPreAnalisis_Cargar(int codEmpresa, long operacion)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var response = new FrmPreAnalisisCargarResponse
                {
                    operacion = ObtenerOperacion(conn, operacion)
                };

                if (response.operacion == null)
                {
                    return DbHelper.CreateOkResponse(response);
                }

                response.obligaciones = ObtenerObligaciones(conn, response.operacion.cedula, operacion);
                response.cuotas_refundidas = response.obligaciones
                    .Where(x => x.seleccionada)
                    .Sum(x => x.cuota);
                response.registro = ObtenerRegistro(conn, operacion);
                response.fiadores = ObtenerFiadores(conn, operacion);

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmPreAnalisisCargarResponse());
            }
        }

        /// <summary>
        /// Registra o elimina las refundiciones segun las obligaciones seleccionadas y calcula los totales (tlb_ButtonClick de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="request">Operacion y obligaciones seleccionadas.</param>
        /// <returns>Obligaciones internas, cuotas refundidas, total de cuota y total de saldo.</returns>
        public ErrorDto<FrmPreAnalisisCalcularResponse> Prea_frmPreAnalisis_Calcular(
            int codEmpresa,
            FrmPreAnalisisCalcularRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var operacion = ObtenerOperacion(conn, request.operacion);
                if (operacion == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "La operación indicada no existe.",
                        -1,
                        new FrmPreAnalisisCalcularResponse());
                }

                var obligaciones = ObtenerObligaciones(conn, operacion.cedula, request.operacion);
                var existentes = obligaciones.ToDictionary(x => x.id_solicitud, x => x.seleccionada);
                foreach (var obligacion in obligaciones)
                {
                    obligacion.seleccionada = request.seleccionadas.Contains(obligacion.id_solicitud);
                }

                if (SobrepasaMontoSolicitado(obligaciones, operacion.monto_solicitado))
                {
                    return DbHelper.CreateErrorResponse(
                        "Los Saldos A Refundir Sobrepasan Al Monto Solicitado",
                        -1,
                        new FrmPreAnalisisCalcularResponse());
                }

                conn.Open();
                using var transaction = conn.BeginTransaction();
                foreach (var obligacion in obligaciones)
                {
                    AplicarRefundicion(conn, transaction, operacion, obligacion, existentes[obligacion.id_solicitud]);
                }

                transaction.Commit();
                return DbHelper.CreateOkResponse(CalcularTotales(obligaciones));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmPreAnalisisCalcularResponse());
            }
        }

        /// <summary>
        /// Guarda la ficha del deudor (Pra_Principal) y los fiadores (cmdGuardarDeudor_Click de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="request">Datos del deudor y fiadores.</param>
        /// <returns>Indica si se guardo (solo operaciones en estado R o P) y los fiadores actualizados.</returns>
        public ErrorDto<FrmPreAnalisisDeudorGuardarResponse> Prea_frmPreAnalisis_Deudor_Guardar(
            int codEmpresa,
            FrmPreAnalisisDeudorGuardarRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var operacion = ObtenerOperacion(conn, request.operacion);
                if (operacion == null || !EstadoPermiteGuardar(operacion.estado))
                {
                    return DbHelper.CreateOkResponse(new FrmPreAnalisisDeudorGuardarResponse());
                }

                var mensajeValidacion = ValidarDeudor(request);
                if (mensajeValidacion != null)
                {
                    return DbHelper.CreateErrorResponse(
                        mensajeValidacion,
                        -1,
                        new FrmPreAnalisisDeudorGuardarResponse());
                }

                conn.Open();
                using var transaction = conn.BeginTransaction();
                var existe = ExisteRegistro(conn, transaction, request.operacion);
                if (existe)
                {
                    ActualizarRegistro(conn, transaction, request);
                }
                else
                {
                    InsertarRegistro(conn, transaction, request);
                }

                foreach (var fiador in request.fiadores.Where(x => !string.IsNullOrWhiteSpace(x.cedulaf)))
                {
                    GuardarFiador(conn, transaction, request, fiador);
                }

                transaction.Commit();

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    existe ? "Modifica" : "Registra",
                    existe
                        ? $"Modifico Ficha de Preanalisis a OP# {request.operacion}"
                        : $"Registro Ficha de Preanalisis a OP# {request.operacion}");

                return DbHelper.CreateOkResponse(new FrmPreAnalisisDeudorGuardarResponse
                {
                    guardado = true,
                    fiadores = ObtenerFiadores(conn, request.operacion)
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new FrmPreAnalisisDeudorGuardarResponse());
            }
        }

        /// <summary>
        /// Guarda las observaciones del analista, comite y junta directiva (cmdGuardaObservaciones_Click de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="request">Operacion y observaciones.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto Prea_frmPreAnalisis_Observaciones_Guardar(
            int codEmpresa,
            FrmPreAnalisisObservacionesGuardarRequest request)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                const string sql = @"
                    update Pra_Principal
                    set Observacion_Analista = @observacion_analista,
                        Observacion_Comite = @observacion_comite,
                        Observacion_JD = @observacion_jd
                    where id_Solicitud = @operacion;";

                conn.Execute(sql, new
                {
                    request.observacion_analista,
                    request.observacion_comite,
                    request.observacion_jd,
                    request.operacion
                });

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Modifica",
                    $"Modifico Ficha de Preanalisis a OP# {request.operacion}");

                return DbHelper.OkResponse("Datos Actualizados");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Lista las operaciones en estado R o P para la busqueda (sbBusqueda de VB6).
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la conexion del cliente.</param>
        /// <param name="filtros">JSON con filtros lazy (FiltrosLazyLoadData).</param>
        /// <returns>Total y pagina de operaciones.</returns>
        public ErrorDto<TablasListaGenericaModel> Prea_frmPreAnalisis_Operaciones_Lista(int codEmpresa, string filtros)
        {
            var result = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
            };

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var filtrosLazy = DbHelper.DeserializeOrNew<FiltrosLazyLoadData>(filtros);
                var sortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["operacion"] = 1,
                    ["codigo"] = 2,
                    ["cedula"] = 3,
                    ["nombre"] = 4,
                    ["descripcion"] = 5
                };
                var lazy = LazyLoadHelper.Build(filtrosLazy, sortMap, "operacion");

                const string sqlCount = @"
                    select count(*)
                    from reg_creditos R
                    inner join Socios S on R.cedula = S.cedula
                    inner join Catalogo C on R.codigo = C.codigo
                    where R.Estadosol in ('R','P')
                      and (@filtro is null
                           or cast(R.id_solicitud as varchar(20)) like @filtro
                           or R.cedula like @filtro
                           or S.nombre like @filtro
                           or R.codigo like @filtro
                           or C.descripcion like @filtro);";

                result.Result.total = conn.QueryFirstOrDefault<int>(sqlCount, lazy.Params);

                const string sqlLista = @"
                    select R.id_Solicitud as operacion,
                           rtrim(R.codigo) as codigo,
                           rtrim(R.cedula) as cedula,
                           rtrim(S.nombre) as nombre,
                           rtrim(C.descripcion) as descripcion
                    from reg_creditos R
                    inner join Socios S on R.cedula = S.cedula
                    inner join Catalogo C on R.codigo = C.codigo
                    where R.Estadosol in ('R','P')
                      and (@filtro is null
                           or cast(R.id_solicitud as varchar(20)) like @filtro
                           or R.cedula like @filtro
                           or S.nombre like @filtro
                           or R.codigo like @filtro
                           or C.descripcion like @filtro)
                    order by
                        case when @sortCode = 1 and @isAsc = 1 then R.id_solicitud end asc,
                        case when @sortCode = 1 and @isAsc = 0 then R.id_solicitud end desc,
                        case when @sortCode = 2 and @isAsc = 1 then R.codigo end asc,
                        case when @sortCode = 2 and @isAsc = 0 then R.codigo end desc,
                        case when @sortCode = 3 and @isAsc = 1 then R.cedula end asc,
                        case when @sortCode = 3 and @isAsc = 0 then R.cedula end desc,
                        case when @sortCode = 4 and @isAsc = 1 then S.nombre end asc,
                        case when @sortCode = 4 and @isAsc = 0 then S.nombre end desc,
                        case when @sortCode = 5 and @isAsc = 1 then C.descripcion end asc,
                        case when @sortCode = 5 and @isAsc = 0 then C.descripcion end desc
                    offset @offset rows
                    fetch next @pageSize rows only;";

                result.Result.lista = conn.Query<FrmPreAnalisisOperacionLista>(sqlLista, lazy.Params).ToList();
            }
            catch (SqlException ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }

        private static FrmPreAnalisisOperacionData? ObtenerOperacion(IDbConnection conn, long operacion)
        {
            const string sql = @"
                select R.Id_solicitud as id_solicitud,
                       isnull(R.MontoSol, 0) as monto_solicitado,
                       rtrim(R.Codigo) as codigo,
                       rtrim(R.Cedula) as cedula,
                       isnull(R.Cuota, 0) as cuota,
                       rtrim(S.Nombre) as nombre,
                       rtrim(C.Descripcion) as descripcion,
                       R.estadosol as estado
                from Reg_creditos R
                inner join Socios S on R.Cedula = S.Cedula
                inner join Catalogo C on R.Codigo = C.Codigo
                where R.Id_Solicitud = @operacion;";

            return conn.QueryFirstOrDefault<FrmPreAnalisisOperacionData>(sql, new { operacion });
        }

        private static List<FrmPreAnalisisObligacionData> ObtenerObligaciones(IDbConnection conn, string cedula, long operacion)
        {
            const string sql = @"
                select R.Id_Solicitud as id_solicitud,
                       rtrim(R.Codigo) as codigo,
                       rtrim(C.Descripcion) as descripcion,
                       isnull(R.Saldo, 0) as saldo,
                       isnull(R.plazo, 0) as plazo,
                       isnull(R.Cuota, 0) as cuota,
                       isnull(R.amortiza, 0) as amortizado,
                       C.poliza as poliza,
                       C.retencion as retencion,
                       V.IntC as int_c,
                       V.IntM as int_m,
                       V.Amortiza as amortiza_mora
                from Reg_Creditos R
                inner join Catalogo C on R.Codigo = C.Codigo
                left join Vista_Morosidad V on R.id_solicitud = V.id_solicitud
                where R.Cedula = @cedula
                  and R.Saldo > 0
                  and R.Estado = 'A'
                  and R.plazo < 900;";

            const string sqlRefundiciones = "select id_solicitud from Refundiciones where Id_solicitudR = @operacion;";
            const string sqlRetencion = "select id_solicitud from Refunde_retencion where Id_solicitudR = @operacion;";

            var filas = conn.Query<FilaObligacion>(sql, new { cedula }).ToList();
            var refundidas = conn.Query<long>(sqlRefundiciones, new { operacion }).ToHashSet();
            var refundidasRetencion = conn.Query<long>(sqlRetencion, new { operacion }).ToHashSet();

            return filas.Select(fila => MapearObligacion(fila, refundidas, refundidasRetencion)).ToList();
        }

        private static FrmPreAnalisisObligacionData MapearObligacion(
            FilaObligacion fila,
            HashSet<long> refundidas,
            HashSet<long> refundidasRetencion)
        {
            var esRetencion = fila.retencion == "S" || fila.poliza == "S";
            var amortizaMora = fila.amortiza_mora ?? 0;
            var tipo = esRetencion ? TipoRetencion : TipoCuota;

            return new FrmPreAnalisisObligacionData
            {
                id_solicitud = fila.id_solicitud,
                codigo = fila.codigo,
                descripcion = fila.descripcion,
                tipo = tipo,
                saldo = esRetencion
                    ? (fila.plazo * fila.cuota) - (fila.amortizado + amortizaMora)
                    : fila.saldo,
                amortiza = amortizaMora,
                int_c = fila.int_c ?? 0,
                int_m = fila.int_m ?? 0,
                cuota = fila.cuota,
                seleccionada = esRetencion
                    ? refundidasRetencion.Contains(fila.id_solicitud)
                    : refundidas.Contains(fila.id_solicitud)
            };
        }

        private static FrmPreAnalisisRegistroData? ObtenerRegistro(IDbConnection conn, long operacion)
        {
            const string sql = @"
                select isnull(SALARIO_LIQUIDO, 0) as salario_liquido,
                       isnull(Obligaciones_Ext_Saldo, 0) as obligaciones_ext_saldo,
                       isnull(Obligaciones_Ext_Cuota, 0) as obligaciones_ext_cuota,
                       isnull(Porc_Salario_Devengado, 0) as porc_salario_devengado,
                       isnull(Porc_Liquidez, 0) as porc_liquidez,
                       rtrim(isnull(OBSERVACION_ANALISTA, '')) as observacion_analista,
                       rtrim(isnull(OBSERVACION_COMITE, '')) as observacion_comite,
                       rtrim(isnull(OBSERVACION_JD, '')) as observacion_jd
                from Pra_Principal
                where id_solicitud = @operacion;";

            return conn.QueryFirstOrDefault<FrmPreAnalisisRegistroData>(sql, new { operacion });
        }

        private static List<FrmPreAnalisisFiadorData> ObtenerFiadores(IDbConnection conn, long operacion)
        {
            const string sql = @"
                select rtrim(CedulaF) as cedulaf,
                       rtrim(Nombre) as nombre,
                       isnull(Salario, 0) as salario,
                       isnull(Liquidez, 0) as liquidez,
                       isnull(Devengado, 0) as devengado,
                       FIA_CONSEC as fia_consec
                from Fiadores
                where id_solicitud = @operacion;";

            return conn.Query<FrmPreAnalisisFiadorData>(sql, new { operacion }).ToList();
        }

        private static bool SobrepasaMontoSolicitado(
            IEnumerable<FrmPreAnalisisObligacionData> obligaciones,
            decimal montoSolicitado)
        {
            decimal acumulado = 0;
            foreach (var obligacion in obligaciones.Where(x => x.seleccionada))
            {
                acumulado += ImporteObligacion(obligacion);
                if (acumulado > montoSolicitado)
                {
                    return true;
                }
            }

            return false;
        }

        private static decimal ImporteObligacion(FrmPreAnalisisObligacionData obligacion)
        {
            return obligacion.tipo == TipoCuota
                ? obligacion.saldo + obligacion.int_c + obligacion.int_m
                : obligacion.saldo + obligacion.amortiza;
        }

        private static decimal SaldoObligacion(FrmPreAnalisisObligacionData obligacion)
        {
            return obligacion.tipo == TipoCuota
                ? obligacion.saldo - obligacion.amortiza + obligacion.int_c + obligacion.int_m
                : obligacion.amortiza + obligacion.saldo;
        }

        private static FrmPreAnalisisCalcularResponse CalcularTotales(IEnumerable<FrmPreAnalisisObligacionData> obligaciones)
        {
            var seleccionadas = obligaciones.Where(x => x.seleccionada).ToList();

            return new FrmPreAnalisisCalcularResponse
            {
                obligaciones_internas = seleccionadas.Sum(ImporteObligacion),
                cuotas_refundidas = seleccionadas.Sum(x => x.cuota),
                total_cuota = seleccionadas.Sum(x => x.cuota),
                total_saldo = seleccionadas.Sum(SaldoObligacion)
            };
        }

        private static void AplicarRefundicion(
            IDbConnection conn,
            IDbTransaction transaction,
            FrmPreAnalisisOperacionData operacion,
            FrmPreAnalisisObligacionData obligacion,
            bool existe)
        {
            if (obligacion.seleccionada)
            {
                if (!existe)
                {
                    InsertarRefundicion(conn, transaction, operacion, obligacion);
                }

                return;
            }

            var sqlEliminar = obligacion.tipo == TipoCuota
                ? "delete from Refundiciones where id_solicitud = @id and id_solicitudr = @operacion;"
                : "delete from Refunde_Retencion where id_solicitud = @id and id_solicitudr = @operacion;";

            conn.Execute(
                sqlEliminar,
                new { id = obligacion.id_solicitud, operacion = operacion.id_solicitud },
                transaction);
        }

        private static void InsertarRefundicion(
            IDbConnection conn,
            IDbTransaction transaction,
            FrmPreAnalisisOperacionData operacion,
            FrmPreAnalisisObligacionData obligacion)
        {
            const string sqlCuota = @"
                insert into Refundiciones(ID_SOLICITUD, CODIGO, CODIGOR, MONTO, FECHA, ID_SOLICITUDR, INTCOR, INTMOR, SALDO_ANTERIOR)
                values(@id, @codigo, @codigoR, @saldo, convert(date, dbo.MyGetdate()), @operacion, @int_c, @int_m, @saldoAnterior);";

            const string sqlRetencion = @"
                insert into Refunde_Retencion(ID_SOLICITUD, CODIGO, CODIGOR, MONTO, FECHA, ID_SOLICITUDR, MORA, SALDO_ANTERIOR)
                values(@id, @codigo, @codigoR, @saldo, convert(date, dbo.MyGetdate()), @operacion, @amortiza, @saldo);";

            var parametros = new
            {
                id = obligacion.id_solicitud,
                codigo = obligacion.codigo,
                codigoR = operacion.codigo,
                saldo = obligacion.saldo,
                operacion = operacion.id_solicitud,
                int_c = obligacion.int_c,
                int_m = obligacion.int_m,
                amortiza = obligacion.amortiza,
                saldoAnterior = obligacion.saldo - obligacion.amortiza
            };

            conn.Execute(obligacion.tipo == TipoCuota ? sqlCuota : sqlRetencion, parametros, transaction);
        }

        private static string? ValidarDeudor(FrmPreAnalisisDeudorGuardarRequest request)
        {
            if (request.operacion <= 0
                || string.IsNullOrWhiteSpace(request.codigo)
                || !request.salario_liquido.HasValue
                || !request.obligaciones_ext_saldo.HasValue
                || !request.obligaciones_ext_cuota.HasValue
                || !request.porc_salario_devengado.HasValue
                || !request.porc_liquidez.HasValue)
            {
                return "Faltan Datos";
            }

            return request.porc_liquidez.Value > 100 ? "Porcentajes Deudor Incorrectos" : null;
        }

        private static bool EstadoPermiteGuardar(string? estado)
        {
            return estado == "R" || estado == "P";
        }

        private static bool ExisteRegistro(IDbConnection conn, IDbTransaction transaction, long operacion)
        {
            const string sql = "select count(*) from Pra_Principal where id_solicitud = @operacion;";
            return conn.QueryFirstOrDefault<int>(sql, new { operacion }, transaction) > 0;
        }

        private static void InsertarRegistro(
            IDbConnection conn,
            IDbTransaction transaction,
            FrmPreAnalisisDeudorGuardarRequest request)
        {
            const string sql = @"
                insert into Pra_Principal(Id_Solicitud, Codigo, Salario_Liquido, Obligaciones_Ext_Saldo,
                    Obligaciones_Ext_Cuota, Salario_Neto, Porc_Salario_Devengado, Porc_Liquidez, Total_cuota, Total_saldo)
                values(@operacion, @codigo, @salario_liquido, @obligaciones_ext_saldo,
                    @obligaciones_ext_cuota, @salario_neto, @porc_salario_devengado, @porc_liquidez, @total_cuota, @total_saldo);";

            conn.Execute(sql, ParametrosRegistro(request), transaction);
        }

        private static void ActualizarRegistro(
            IDbConnection conn,
            IDbTransaction transaction,
            FrmPreAnalisisDeudorGuardarRequest request)
        {
            const string sql = @"
                update Pra_Principal
                set Codigo = @codigo,
                    Salario_Liquido = @salario_liquido,
                    Obligaciones_Ext_Saldo = @obligaciones_ext_saldo,
                    Obligaciones_Ext_Cuota = @obligaciones_ext_cuota,
                    Salario_Neto = @salario_neto,
                    Porc_Salario_Devengado = @porc_salario_devengado,
                    Porc_Liquidez = @porc_liquidez,
                    Total_cuota = @total_cuota,
                    Total_saldo = @total_saldo
                where id_Solicitud = @operacion;";

            conn.Execute(sql, ParametrosRegistro(request), transaction);
        }

        private static object ParametrosRegistro(FrmPreAnalisisDeudorGuardarRequest request)
        {
            return new
            {
                request.operacion,
                codigo = request.codigo.Trim(),
                request.salario_liquido,
                request.obligaciones_ext_saldo,
                request.obligaciones_ext_cuota,
                request.salario_neto,
                request.porc_salario_devengado,
                request.porc_liquidez,
                request.total_cuota,
                request.total_saldo
            };
        }

        private static void GuardarFiador(
            IDbConnection conn,
            IDbTransaction transaction,
            FrmPreAnalisisDeudorGuardarRequest request,
            FrmPreAnalisisFiadorData fiador)
        {
            const string sqlActualizar = @"
                update Fiadores
                set CEDULAF = @cedulaf, NOMBRE = @nombre, SALARIO = @salario, Liquidez = @liquidez, Devengado = @devengado
                where Fia_Consec = @fia_consec;";

            const string sqlInsertar = @"
                insert into Fiadores(ID_SOLICITUD, CODIGO, CEDULAF, NOMBRE, FIRMA, ESTADO, SALARIO, LIQUIDEZ, DEVENGADO)
                values(@operacion, @codigo, @cedulaf, @nombre, 'N', 'A', @salario, @liquidez, @devengado);";

            var parametros = new
            {
                request.operacion,
                codigo = request.codigo.Trim(),
                cedulaf = fiador.cedulaf.Trim(),
                nombre = fiador.nombre.Trim(),
                fiador.salario,
                fiador.liquidez,
                fiador.devengado,
                fiador.fia_consec
            };

            conn.Execute(fiador.fia_consec.HasValue ? sqlActualizar : sqlInsertar, parametros, transaction);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = VModulo
            });
        }
    }
}
