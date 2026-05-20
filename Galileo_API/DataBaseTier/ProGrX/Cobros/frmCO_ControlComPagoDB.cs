using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public partial class FrmCoControlComPagoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCobroDb _mCobroDb;
        private const int ModuloCobros = 4;
        private const int MaxTopRemesas = 500;
        private const string SpConsultaBancos = "spCbrComision_ConsultaBancos";

        public FrmCoControlComPagoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mCobroDb = new MCobroDb(config);
        }

        /// <summary>
        /// Consulta las ultimas remesas registradas para el control de pago de comisiones.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="top">Cantidad maxima de remesas a retornar.</param>
        public ErrorDto<List<CoControlComPagoRemesaData>> CO_ControlComPago_Remesas_Obtener(int CodEmpresa, int top)
        {
            var topSeguro = NormalizarTop(top);
            const string sql = @"
                SELECT TOP (@top)
                    r.cod_remesa,
                    r.usuario,
                    r.fecha,
                    r.estado,
                    CASE r.estado
                        WHEN 'A' THEN 'Remesa Abierta'
                        WHEN 'C' THEN 'Remesa Cerrada'
                        WHEN 'T' THEN 'Remesa Trasladada'
                        WHEN 'P' THEN 'Remesa en Proceso'
                        ELSE r.estado
                    END AS estado_descripcion,
                    r.fecha_inicio,
                    r.fecha_corte,
                    ISNULL(r.notas,'') AS notas,
                    (
                        SELECT COUNT(1)
                        FROM dbo.CBR_REMESAS_PAGO p
                        WHERE p.cod_remesa = r.cod_remesa
                    ) AS detalle_pago
                FROM dbo.CBR_REMESAS r
                ORDER BY r.fecha DESC;";

            return DbHelper.ExecuteListQuery<CoControlComPagoRemesaData>(
                _portalDB,
                CodEmpresa,
                sql,
                new { top = topSeguro });
        }

        /// <summary>
        /// Consulta una remesa por su identificador.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa.</param>
        public ErrorDto<CoControlComPagoRemesaData> CO_ControlComPago_Remesa_Obtener(int CodEmpresa, int cod_remesa)
        {
            const string sql = @"
                SELECT
                    cod_remesa,
                    usuario,
                    fecha,
                    estado,
                    CASE estado
                        WHEN 'A' THEN 'Remesa Abierta'
                        WHEN 'C' THEN 'Remesa Cerrada'
                        WHEN 'T' THEN 'Remesa Trasladada'
                        WHEN 'P' THEN 'Remesa en Proceso'
                        ELSE estado
                    END AS estado_descripcion,
                    fecha_inicio,
                    fecha_corte,
                    ISNULL(notas,'') AS notas
                FROM dbo.CBR_REMESAS
                WHERE cod_remesa = @cod_remesa;";

            return DbHelper.ExecuteSingleQuery<CoControlComPagoRemesaData>(
                _portalDB,
                CodEmpresa,
                sql,
                new CoControlComPagoRemesaData(),
                new { cod_remesa });
        }

        /// <summary>
        /// Inserta o actualiza la remesa de pago de comisiones.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se registra la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta el registro o modificacion.</param>
        /// <param name="request">Datos de la remesa a registrar o modificar.</param>
        public ErrorDto<int> CO_ControlComPago_Remesa_Guardar(int CodEmpresa, string usuario, CoControlComPagoRemesaGuardarRequest request)
        {
            var validacion = ValidarRemesaGuardar(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<int>(validacion.Description ?? "Datos de remesa invalidos.");
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                var existe = request.cod_remesa > 0
                    ? conn.ExecuteScalar<int>("SELECT COUNT(1) FROM dbo.CBR_REMESAS WHERE cod_remesa = @cod_remesa;", new { request.cod_remesa })
                    : 0;

                var codRemesa = existe > 0
                    ? CO_ControlComPago_Remesa_Actualizar(conn, request, usuario)
                    : CO_ControlComPago_Remesa_Insertar(conn, request, usuario);

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Remesa Comisiones de Cobros: {codRemesa}",
                    existe > 0 ? "Modifica - WEB" : "Registra - WEB");

                return DbHelper.CreateOkResponse(codRemesa);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se elimina la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta la eliminacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa abierta.</param>
        public ErrorDto CO_ControlComPago_Remesa_Eliminar(int CodEmpresa, string usuario, int cod_remesa)
        {
            const string validarSql = @"
                SELECT COUNT(1)
                FROM dbo.CBR_REMESAS
                WHERE cod_remesa = @cod_remesa
                    AND estado = 'A';";

            const string eliminarSql = @"
                DELETE dbo.CBR_REMESAS
                WHERE cod_remesa = @cod_remesa
                    AND estado = 'A';";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                var existeAbierta = conn.ExecuteScalar<int>(validarSql, new { cod_remesa });
                if (existeAbierta == 0)
                {
                    return DbHelper.ErrorResponse("Solo se pueden eliminar remesas abiertas.", -2);
                }

                conn.Execute(eliminarSql, new { cod_remesa });
                RegistrarBitacora(CodEmpresa, usuario, $"Remesa Comisiones de Cobros: {cod_remesa}", "Elimina - WEB");

                return DbHelper.OkResponse("Ok");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Consulta las remesas por estado para combos de carga o traslado.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="estado">Estado funcional de la remesa.</param>
        public ErrorDto<List<CoControlComPagoRemesaComboData>> CO_ControlComPago_RemesasPorEstado_Obtener(int CodEmpresa, string estado)
        {
            var estadoSeguro = NormalizarEstado(estado);
            const string sql = @"
                SELECT
                    cod_remesa,
                    RIGHT('0000' + CONVERT(varchar(10), cod_remesa), 4)
                        + '...' + RTRIM(usuario)
                        + '...' + CONVERT(varchar(19), fecha, 120)
                        + ' I:' + CONVERT(varchar(10), fecha_inicio, 103)
                        + ' C:' + CONVERT(varchar(10), fecha_corte, 103) AS descripcion,
                    usuario,
                    fecha,
                    fecha_inicio,
                    fecha_corte
                FROM dbo.CBR_REMESAS
                WHERE estado = @estado
                ORDER BY fecha DESC;";

            return DbHelper.ExecuteListQuery<CoControlComPagoRemesaComboData>(
                _portalDB,
                CodEmpresa,
                sql,
                new { estado = estadoSeguro });
        }

        /// <summary>
        /// Consulta las cuentas bancarias disponibles para una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa que define el rango de corte.</param>
        public ErrorDto<List<CoControlComPagoBancoData>> CO_ControlComPago_CargaBancos_Obtener(int CodEmpresa, int cod_remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();

            var rango = ObtenerRangoRemesa(conn, cod_remesa);
            var result = DbHelper.ExecuteStoredProcedureList<CoControlComPagoBancoSpDto>(
                _portalDB.ObtenerDbConnStringEmpresa(CodEmpresa),
                SpConsultaBancos,
                new { inicio = InicioDia(rango.fecha_inicio), corte = FinDia(rango.fecha_corte) });

            return new ErrorDto<List<CoControlComPagoBancoData>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result?
                    .Select(banco => new CoControlComPagoBancoData
                    {
                        id_banco = banco.id_Banco,
                        banco_desc = (banco.BancoDesc ?? string.Empty).Trim()
                    })
                    .ToList() ?? new List<CoControlComPagoBancoData>()
            };
        }

        /// <summary>
        /// Consulta las oficinas disponibles para el panel de informes de remesas.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_ControlComPago_ReportesOficinas_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_oficina) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM dbo.SIF_Oficinas
                ORDER BY cod_oficina;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql);
        }

        /// <summary>
        /// Consulta usuarios pendientes de carga para la remesa seleccionada.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se consulta la informacion.</param>
        /// <param name="cod_remesa">Identificador de la remesa que define el rango de corte.</param>
        /// <param name="id_banco">Banco usado como filtro opcional.</param>
        public ErrorDto<List<CoControlComPagoCargaData>> CO_ControlComPago_CargaPendientes_Obtener(int CodEmpresa, int cod_remesa, int? id_banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var rango = ObtenerRangoRemesa(conn, cod_remesa);
                var sql = id_banco.HasValue
                    ? "EXEC dbo.spCbrComision_ConsultaPendientes @inicio, @corte, @id_banco;"
                    : "EXEC dbo.spCbrComision_ConsultaPendientes @inicio, @corte;";

                return conn.Query<CoControlComPagoCargaData>(
                    sql,
                    new { inicio = InicioDia(rango.fecha_inicio), corte = FinDia(rango.fecha_corte), id_banco }).ToList();
            });
        }

        /// <summary>
        /// Carga en la remesa los usuarios seleccionados.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se aplica la carga.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="request">Datos de la remesa y usuarios seleccionados.</param>
        public ErrorDto<CoControlComPagoProcesoResult> CO_ControlComPago_Carga_Aplicar(int CodEmpresa, string usuario, CoControlComPagoCargaAplicarRequest request)
        {
            if (request is null || request.cod_remesa <= 0 || request.usuarios.Count == 0)
            {
                return DbHelper.CreateErrorResponse("Debe seleccionar al menos un usuario para cargar.", -2, new CoControlComPagoProcesoResult());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                ValidarRemesaAbiertaOProceso(conn, request.cod_remesa);

                var procesados = 0;
                foreach (var usuarioCarga in request.usuarios.Select(NormalizarUsuario).Where(x => x.Length > 0).Distinct())
                {
                    conn.Execute(
                        "EXEC dbo.spCbrComision_RemesaCarga @cod_remesa, @usuario;",
                        new { request.cod_remesa, usuario = usuarioCarga },
                        commandTimeout: 600);
                    procesados++;
                }

                if (procesados > 0)
                {
                    RegistrarBitacora(CodEmpresa, usuario, $"Carga Remesa Cobros a Tesoreria : {request.cod_remesa}", "Aplica - WEB");
                }

                return DbHelper.CreateOkResponse(new CoControlComPagoProcesoResult { procesados = procesados });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new CoControlComPagoProcesoResult());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -2, new CoControlComPagoProcesoResult());
            }
        }

        /// <summary>
        /// Cierra una remesa abierta o en proceso.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa sobre la que se cierra la remesa.</param>
        /// <param name="usuario">Usuario que ejecuta el cierre.</param>
        /// <param name="cod_remesa">Identificador de la remesa abierta o en proceso.</param>
        public ErrorDto CO_ControlComPago_Remesa_Cerrar(int CodEmpresa, string usuario, int cod_remesa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Open();

                ValidarRemesaAbiertaOProceso(conn, cod_remesa);
                conn.Execute("EXEC dbo.spCbrComision_RemesaCierra @cod_remesa;", new { cod_remesa }, commandTimeout: 600);
                RegistrarBitacora(CodEmpresa, usuario, $"Remesa de Fondos Remesa : {cod_remesa}", "Genera - WEB");

                return DbHelper.OkResponse("Remesa cerrada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -2);
            }
        }

        private static int CO_ControlComPago_Remesa_Insertar(SqlConnection conn, CoControlComPagoRemesaGuardarRequest request, string usuario)
        {
            const string sql = @"
                DECLARE @cod_remesa int;

                SELECT @cod_remesa = ISNULL(MAX(cod_remesa), 0) + 1
                FROM dbo.CBR_REMESAS WITH (UPDLOCK, HOLDLOCK);

                INSERT INTO dbo.CBR_REMESAS
                (
                    cod_remesa,
                    usuario,
                    fecha,
                    estado,
                    fecha_inicio,
                    fecha_corte,
                    notas
                )
                VALUES
                (
                    @cod_remesa,
                    @usuario,
                    dbo.MyGetdate(),
                    'A',
                    @fecha_inicio,
                    @fecha_corte,
                    @notas
                );

                SELECT @cod_remesa;";

            return conn.ExecuteScalar<int>(sql, new
            {
                usuario = NormalizarUsuario(usuario),
                fecha_inicio = request.fecha_inicio.Date,
                fecha_corte = request.fecha_corte.Date,
                notas = NormalizarNotas(request.notas)
            });
        }

        private static int CO_ControlComPago_Remesa_Actualizar(SqlConnection conn, CoControlComPagoRemesaGuardarRequest request, string usuario)
        {
            const string sql = @"
                UPDATE dbo.CBR_REMESAS
                SET
                    usuario = @usuario,
                    fecha_inicio = @fecha_inicio,
                    fecha_corte = @fecha_corte,
                    notas = @notas
                WHERE cod_remesa = @cod_remesa
                    AND estado <> 'C';

                SELECT @@ROWCOUNT;";

            var rows = conn.ExecuteScalar<int>(sql, new
            {
                request.cod_remesa,
                usuario = NormalizarUsuario(usuario),
                fecha_inicio = request.fecha_inicio.Date,
                fecha_corte = request.fecha_corte.Date,
                notas = NormalizarNotas(request.notas)
            });

            if (rows == 0)
            {
                throw new InvalidOperationException("No se puede modificar la remesa porque ya fue cerrada o no existe.");
            }

            return request.cod_remesa;
        }

        private static ErrorDto ValidarRemesaGuardar(CoControlComPagoRemesaGuardarRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse("Los datos de la remesa son requeridos.", -2);
            }

            if (request.fecha_inicio == default || request.fecha_corte == default)
            {
                return DbHelper.ErrorResponse("Las fechas de inicio y corte son requeridas.", -2);
            }

            if (request.fecha_inicio.Date > request.fecha_corte.Date)
            {
                return DbHelper.ErrorResponse("La fecha de inicio no puede ser mayor a la fecha de corte.", -2);
            }

            return DbHelper.OkResponse("Ok");
        }

        private static CoControlComPagoRemesaData ObtenerRangoRemesa(SqlConnection conn, int codRemesa)
        {
            const string sql = @"
                SELECT cod_remesa, fecha_inicio, fecha_corte
                FROM dbo.CBR_REMESAS
                WHERE cod_remesa = @codRemesa;";

            return conn.QueryFirstOrDefault<CoControlComPagoRemesaData>(sql, new { codRemesa })
                ?? throw new InvalidOperationException("No se encontro la remesa seleccionada.");
        }

        private static void ValidarRemesaAbiertaOProceso(SqlConnection conn, int codRemesa)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.CBR_REMESAS
                WHERE cod_remesa = @codRemesa
                    AND estado IN ('A','P');";

            var existe = conn.ExecuteScalar<int>(sql, new { codRemesa });
            if (existe == 0)
            {
                throw new InvalidOperationException("La remesa actual ya se encuentra cerrada.");
            }
        }

        private static int NormalizarTop(int top)
        {
            if (top <= 0)
            {
                return 50;
            }

            return Math.Min(top, MaxTopRemesas);
        }

        private static string NormalizarEstado(string estado)
        {
            var estadoSeguro = (estado ?? string.Empty).Trim().ToUpperInvariant();
            return estadoSeguro is "A" or "C" or "T" or "P" ? estadoSeguro : "A";
        }

        private static string NormalizarUsuario(string? usuario)
        {
            return (usuario ?? string.Empty).Trim();
        }

        private static string NormalizarNotas(string? notas)
        {
            return (notas ?? string.Empty).Trim();
        }

        private static DateTime InicioDia(DateTime fecha)
        {
            return fecha.Date;
        }

        private static DateTime FinDia(DateTime fecha)
        {
            return fecha.Date.AddDays(1).AddSeconds(-1);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarUsuario(usuario),
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = ModuloCobros
            });
        }
    }
}
