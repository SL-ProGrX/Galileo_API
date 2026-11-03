using System.Data;
using System.Globalization;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX_Inversiones;

public sealed class FrmIvrRecBancosRegistroDb
{
    private const int CodigoValidacion = -2;
    private const string ConceptoCupon = "Cupon";
    private readonly PortalDB _portalDb;

    private const string SqlLista = """
        SELECT TRANSAC_ID AS transac_id, ISNULL(Tipo_Mov_Desc, '') AS tipo_mov_desc,
               Registro_Fecha AS registro_fecha, ISNULL(Importe, 0) AS importe,
               ISNULL(cta_Mask, '') AS cta_mask, ISNULL(Tipo_Doc_Desc, '') AS tipo_doc_desc,
               ISNULL(Detalle, '') AS detalle, TES_NSOLICITUD AS tes_nsolicitud,
               ISNULL(Tes_Estado_Desc, '') AS tes_estado_desc,
               ISNULL(Cod_Divisa, '') AS cod_divisa,
               ISNULL(Tipo_Cambio, 0) AS tipo_cambio, ISNULL(Monto, 0) AS monto
        FROM vIVR_TRANSACCIONES
        WHERE REF_TIPO = @ref_tipo AND REF_CODIGO = @ref_codigo
          AND REF_CONCEPTO = @ref_concepto
        ORDER BY TRANSAC_ID;
        """;

    /// <summary>Inicializa las dependencias existentes del API.</summary>
    public FrmIvrRecBancosRegistroDb(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _portalDb = new PortalDB(config);
    }

    /// <summary>Carga inversión, moneda local, contabilidad y movimientos del contexto.</summary>
    public ErrorDto<IvrRecBancosRegistroInicialData> IVR_Rec_Bancos_Registro_Inicializar(
        int codEmpresa, IvrRecBancosRegistroContextoRequest? request)
    {
        var error = IVR_Rec_Bancos_Registro_Contexto_Validar(request);
        if (error != null)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroInicialData>(error, CodigoValidacion);

        return IVR_Rec_Bancos_Registro_Ejecutar(codEmpresa, connection =>
        {
            var codContabilidad = IVR_Rec_Bancos_Registro_Contabilidad_Consultar(connection);
            if (codContabilidad <= 0)
                return DbHelper.CreateErrorResponse<IvrRecBancosRegistroInicialData>(
                    "No se encuentra configurada la contabilidad de la empresa.", CodigoValidacion);

            const string sql = """
                SELECT TITULO_ID AS titulo_id, Administrador_Desc AS administrador_desc,
                       Instrumento_Desc AS instrumento_desc, Portafolio_Desc AS portafolio_desc,
                       ISNULL(CTA_TRANSITO, '') AS cta_transito,
                       ISNULL(CTA_TRANSITO_DESC, '') AS cta_transito_desc,
                       ISNULL(CTA_TRANSITO_DIVISA, '') AS cta_transito_divisa
                FROM vIVR_INVERSIONES WHERE TITULO_ID = @titulo_id;
                """;
            var inversion = connection.QuerySingleOrDefault<IvrRecBancosRegistroEncabezadoData>(sql, request);
            if (inversion == null)
                return DbHelper.CreateErrorResponse<IvrRecBancosRegistroInicialData>(
                    "No se encontró la inversión indicada.", CodigoValidacion);

            var local = IVR_Rec_Bancos_Registro_DivisaLocal_Consultar(connection);
            if (string.IsNullOrWhiteSpace(local))
                return DbHelper.CreateErrorResponse<IvrRecBancosRegistroInicialData>(
                    "No se encuentra configurada la divisa local.", CodigoValidacion);

            return DbHelper.CreateOkResponse(new IvrRecBancosRegistroInicialData
            {
                inversion = inversion,
                divisa_local = local,
                cod_contabilidad = codContabilidad,
                movimientos = IVR_Rec_Bancos_Registro_Lista_Consultar(connection, request!)
            });
        });
    }

    /// <summary>Obtiene movimientos y recalcula registrado y pendiente sin persistencia.</summary>
    public ErrorDto<IvrRecBancosRegistroListaData> IVR_Rec_Bancos_Registro_Transacciones_Obtener(
        int codEmpresa, IvrRecBancosRegistroContextoRequest? request)
    {
        var error = IVR_Rec_Bancos_Registro_Contexto_Validar(request);
        if (error != null)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroListaData>(error, CodigoValidacion);

        return IVR_Rec_Bancos_Registro_Ejecutar(codEmpresa, connection =>
            DbHelper.CreateOkResponse(IVR_Rec_Bancos_Registro_Lista_Consultar(connection, request!)));
    }

    /// <summary>Resuelve cuenta interna o enmascarada dentro de la contabilidad de la empresa.</summary>
    public ErrorDto<CtnxCuentasDto> IVR_Rec_Bancos_Registro_Cuenta_Obtener(
        int codEmpresa, IvrRecBancosRegistroCuentaRequest? request)
    {
        var cuenta = request?.cod_cuenta?.Trim() ?? string.Empty;
        if (cuenta.Length is 0 or > 60)
            return DbHelper.CreateErrorResponse<CtnxCuentasDto>(
                "Indique una cuenta contable válida.", CodigoValidacion);

        return IVR_Rec_Bancos_Registro_Ejecutar(codEmpresa, connection =>
        {
            var codContabilidad = IVR_Rec_Bancos_Registro_Contabilidad_Consultar(connection);
            if (codContabilidad <= 0)
                return DbHelper.CreateErrorResponse<CtnxCuentasDto>(
                    "No se encuentra configurada la contabilidad de la empresa.", CodigoValidacion);

            const string sql = """
                SELECT RTRIM(COD_CUENTA) AS cod_cuenta,
                       RTRIM(COD_CUENTA_MASK) AS cod_cuenta_Mask,
                       RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion,
                       ACEPTA_MOVIMIENTOS AS acepta_movimientos,
                       RTRIM(ISNULL(CUENTA_MADRE, '')) AS cuenta_madre,
                       RTRIM(ISNULL(COD_DIVISA, '')) AS cod_divisa
                FROM CntX_Cuentas
                WHERE COD_CONTABILIDAD = @contabilidad
                  AND (COD_CUENTA = @cuenta OR COD_CUENTA_MASK = @cuenta);
                """;
            var data = connection.QuerySingleOrDefault<CtnxCuentasDto>(sql,
                new { contabilidad = codContabilidad, cuenta });
            if (data == null)
                return DbHelper.CreateErrorResponse<CtnxCuentasDto>(
                    "La cuenta no existe en la contabilidad de la empresa.", CodigoValidacion);
            if (!data.acepta_movimientos)
                return DbHelper.CreateErrorResponse<CtnxCuentasDto>(
                    "La cuenta seleccionada no acepta movimientos.", CodigoValidacion);
            return DbHelper.CreateOkResponse(data);
        });
    }

    /// <summary>Registra Cuenta en Tránsito mediante el SP y confirma la fila dentro de la transacción.</summary>
    public ErrorDto<IvrRecBancosRegistroGuardarData> IVR_Rec_Bancos_Registro_Guardar(
        int codEmpresa, IvrRecBancosRegistroGuardarRequest? request)
    {
        var error = IVR_Rec_Bancos_Registro_Guardar_Validar(request);
        if (error != null)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroGuardarData>(error, CodigoValidacion);

        var cuenta = IVR_Rec_Bancos_Registro_Cuenta_Obtener(codEmpresa,
            new IvrRecBancosRegistroCuentaRequest { cod_cuenta = request!.cod_cuenta });
        if (cuenta.Code != 0 || cuenta.Result == null)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroGuardarData>(
                cuenta.Description ?? "No fue posible consultar la cuenta.", cuenta.Code == 0 ? -1 : cuenta.Code ?? -1);

        return IVR_Rec_Bancos_Registro_Ejecutar(codEmpresa, connection =>
            IVR_Rec_Bancos_Registro_Guardar_Ejecutar(connection, request, cuenta.Result));
    }

    /// <summary>Elimina las filas seleccionadas; revierte el lote si alguna no pertenece al contexto.</summary>
    public ErrorDto<int> IVR_Rec_Bancos_Registro_Eliminar(
        int codEmpresa, IvrRecBancosRegistroEliminarRequest? request)
    {
        var error = IVR_Rec_Bancos_Registro_Contexto_Validar(request);
        if (error != null)
            return DbHelper.CreateErrorResponse<int>(error, CodigoValidacion);
        if (request!.transacciones == null || request.transacciones.Count is 0 or > 500
            || request.transacciones.Any(id => id <= 0))
            return DbHelper.CreateErrorResponse<int>("Seleccione entre 1 y 500 movimientos válidos.", CodigoValidacion);

        return IVR_Rec_Bancos_Registro_Ejecutar(codEmpresa, connection =>
        {
            using var transaction = connection.BeginTransaction();
            var scope = new RegistroScope(connection, transaction);
            var validation = IVR_Rec_Bancos_Registro_Origen_Validar(scope, request);
            if (validation != null)
                return DbHelper.CreateErrorResponse<int>(validation, CodigoValidacion);

            var ids = request.transacciones.Distinct().ToArray();
            const string sql = """
                DECLARE @eliminadas TABLE (transac_id int);
                DELETE FROM IVR_TRANSACCIONES
                OUTPUT DELETED.TRANSAC_ID INTO @eliminadas
                WHERE TRANSAC_ID IN @ids AND REF_TIPO = @ref_tipo
                  AND REF_CODIGO = @ref_codigo AND REF_CONCEPTO = @ref_concepto;
                SELECT COUNT(*) FROM @eliminadas;
                """;
            var eliminados = connection.QuerySingle<int>(sql, new
            {
                ids,
                request.ref_tipo,
                ref_codigo = request.ref_codigo.ToString(CultureInfo.InvariantCulture),
                request.ref_concepto
            }, transaction);
            if (eliminados != ids.Length)
                return DbHelper.CreateErrorResponse<int>(
                    "Uno o más movimientos ya no existen o no pertenecen a esta solicitud.", CodigoValidacion);
            transaction.Commit();
            return DbHelper.CreateOkResponse(eliminados, "Movimientos eliminados satisfactoriamente.");
        });
    }

    /// <summary>Abre la conexión con DbHelper y conserva separados errores controlados y técnicos.</summary>
    private ErrorDto<T> IVR_Rec_Bancos_Registro_Ejecutar<T>(
        int codEmpresa, Func<SqlConnection, ErrorDto<T>> action)
    {
        var result = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            return action(connection);
        });
        return result.Code == 0 && result.Result != null
            ? result.Result
            : DbHelper.CreateErrorResponse<T>(result.Description ?? "No fue posible completar la operación.");
    }

    /// <summary>Ejecuta el SP con sus parámetros reales; no interpreta ExecuteNonQuery como resultado de negocio.</summary>
    private static ErrorDto<IvrRecBancosRegistroGuardarData> IVR_Rec_Bancos_Registro_Guardar_Ejecutar(
        SqlConnection connection, IvrRecBancosRegistroGuardarRequest request, CtnxCuentasDto cuenta)
    {
        using var transaction = connection.BeginTransaction();
        var scope = new RegistroScope(connection, transaction);
        var error = IVR_Rec_Bancos_Registro_Origen_Validar(scope, request);
        if (error != null)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroGuardarData>(error, CodigoValidacion);

        var local = IVR_Rec_Bancos_Registro_DivisaLocal_Consultar(connection, transaction);
        if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(cuenta.cod_divisa))
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroGuardarData>(
                "Verifique la divisa local y la divisa de la cuenta contable.", CodigoValidacion);

        var tipoCambio = string.Equals(cuenta.cod_divisa, local, StringComparison.OrdinalIgnoreCase)
            ? 1m : request.tipo_cambio;

        // El SP usa MAX + 1. El bloqueo se mantiene hasta confirmar o revertir la inserción.
        const string sqlConsecutivo = """
            SELECT ISNULL(MAX(CONVERT(bigint, TRANSAC_ID)), 0) + 1
            FROM IVR_TRANSACCIONES WITH (TABLOCKX, HOLDLOCK);
            """;
        var siguiente = connection.QuerySingle<long>(sqlConsecutivo, transaction: transaction);
        if (siguiente > int.MaxValue)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroGuardarData>(
                "Se agotó el consecutivo disponible para transacciones.", CodigoValidacion);

        connection.Execute("dbo.spIVR_TRANSAC_REGISTRA", new
        {
            Tipo = request.ref_tipo,
            Codigo = request.ref_codigo,
            Concepto = request.ref_concepto,
            TipoMov = request.tipo_mov,
            TipoDoc = "CC",
            Divisa = cuenta.cod_divisa,
            Monto = request.monto,
            TipoCambio = tipoCambio,
            Notas = request.notas.Trim(),
            Cuenta = cuenta.cod_cuenta,
            BancoId = 0,
            Usuario = request.usuario.Trim()
        }, transaction, commandType: CommandType.StoredProcedure);

        const string sqlConfirmar = """
            SELECT COUNT(*) FROM IVR_TRANSACCIONES
            WHERE TRANSAC_ID = @id AND REF_TIPO = @ref_tipo
              AND REF_CODIGO = @ref_codigo AND REF_CONCEPTO = @ref_concepto
              AND COD_CUENTA = @cuenta AND MONTO = @monto AND TIPO_DOC = 'CC';
            """;
        var insertados = connection.QuerySingle<int>(sqlConfirmar, new
        {
            id = (int)siguiente,
            request.ref_tipo,
            ref_codigo = request.ref_codigo.ToString(CultureInfo.InvariantCulture),
            request.ref_concepto,
            cuenta = cuenta.cod_cuenta,
            request.monto
        }, transaction);
        if (insertados != 1)
            return DbHelper.CreateErrorResponse<IvrRecBancosRegistroGuardarData>(
                "El procedimiento no dejó el movimiento esperado en IVR_TRANSACCIONES.");
        transaction.Commit();
        return DbHelper.CreateOkResponse(new IvrRecBancosRegistroGuardarData
        {
            transac_id = (int)siguiente
        }, "Movimiento registrado satisfactoriamente.");
    }

    /// <summary>Consulta la moneda local; reutiliza la transacción cuando corresponde.</summary>
    private static string? IVR_Rec_Bancos_Registro_DivisaLocal_Consultar(
        SqlConnection connection, SqlTransaction? transaction = null)
    {
        const string sql = "SELECT RTRIM(COD_DIVISA) FROM vSys_Divisas WHERE DIVISA_LOCAL = 1;";
        return connection.QuerySingleOrDefault<string>(sql, transaction: transaction);
    }

    /// <summary>Obtiene el enlace contable sin exigir un período contable abierto.</summary>
    private static int IVR_Rec_Bancos_Registro_Contabilidad_Consultar(SqlConnection connection)
    {
        const string sql = "SELECT TOP (1) COD_EMPRESA_ENLACE FROM sif_empresa;";
        return connection.QuerySingleOrDefault<int>(sql);
    }

    /// <summary>Obtiene el mismo detalle de sbIVR_Transac_Load y suma IMPORTE, no MONTO.</summary>
    private static IvrRecBancosRegistroListaData IVR_Rec_Bancos_Registro_Lista_Consultar(
        SqlConnection connection, IvrRecBancosRegistroContextoRequest request)
    {
        var rows = connection.Query<IvrProcCuponesTransaccionData>(SqlLista, new
        {
            request.ref_tipo,
            ref_codigo = request.ref_codigo.ToString(CultureInfo.InvariantCulture),
            request.ref_concepto
        }).ToList();
        var registrado = rows.Sum(row => row.importe);
        return new IvrRecBancosRegistroListaData
        {
            transacciones = rows,
            requerido = request.monto_requerido,
            registrado = registrado,
            pendiente = request.monto_requerido - registrado
        };
    }

    /// <summary>Comprueba inversión y, para Cupones, pertenencia y estado S antes de mutar.</summary>
    private static string? IVR_Rec_Bancos_Registro_Origen_Validar(
        RegistroScope scope, IvrRecBancosRegistroContextoRequest request)
    {
        const string sqlTitulo = "SELECT COUNT(*) FROM IVR_TITULOS WHERE TITULO_ID = @titulo_id;";
        if (scope.Connection.QuerySingle<int>(sqlTitulo, request, scope.Transaction) != 1)
            return "No se encontró la inversión indicada.";

        if (request.ref_tipo == "T" && request.ref_codigo != request.titulo_id)
            return "La referencia no corresponde a la inversión indicada.";

        if (request.ref_tipo != "C")
            return null;

        const string sqlCupon = """
            SELECT ESTADO FROM IVR_TITULO_CUPONES WITH (UPDLOCK, HOLDLOCK)
            WHERE CUPON_ID = @ref_codigo AND TITULO_ID = @titulo_id;
            """;
        var estado = scope.Connection.QuerySingleOrDefault<string>(sqlCupon, request, scope.Transaction);
        return estado?.Trim() == "S" ? null
            : "El cupón no pertenece a la inversión o ya no se encuentra en Solicitud.";
    }

    /// <summary>Valida el contexto compartido y limita los orígenes a los observados en VB6.</summary>
    private static string? IVR_Rec_Bancos_Registro_Contexto_Validar(
        IvrRecBancosRegistroContextoRequest? request)
    {
        if (request == null || request.titulo_id <= 0 || request.ref_codigo <= 0)
            return "Indique la inversión y la referencia del proceso.";
        var origenValido = (request.ref_tipo, request.ref_concepto) switch
        {
            ("C", ConceptoCupon) => request.tipo_mov == "C",
            ("T", "T") => request.tipo_mov == "C",
            ("L", "Liq") => true,
            ("F", "FI_APO") => request.tipo_mov == "C",
            ("F", "FI_RET") => request.tipo_mov == "D",
            _ => false
        };
        if (!origenValido || request.tipo_mov is not ("C" or "D"))
            return "El contexto del movimiento no es válido.";
        if (string.IsNullOrWhiteSpace(request.divisa) || request.divisa.Length > 10)
            return "Indique una divisa válida.";
        if (request.tipo_cambio == 0 || Math.Abs(request.tipo_cambio) > 999999.999999m
            || decimal.Round(request.tipo_cambio, 6) != request.tipo_cambio)
            return "El tipo de cambio debe ser distinto de cero y tener hasta seis decimales.";
        if (request.monto_requerido < 0 || request.monto_requerido > 9999999999999999.99m
            || decimal.Round(request.monto_requerido, 2) != request.monto_requerido)
            return "El monto requerido no es válido.";
        return null;
    }

    /// <summary>Valida importes y longitudes conforme a la firma real del procedimiento.</summary>
    private static string? IVR_Rec_Bancos_Registro_Guardar_Validar(
        IvrRecBancosRegistroGuardarRequest? request)
    {
        var error = IVR_Rec_Bancos_Registro_Contexto_Validar(request);
        if (error != null)
            return error;
        if (request!.monto <= 0 || request.monto > 9999999999999999.99m
            || decimal.Round(request.monto, 2) != request.monto)
            return "Indique un monto mayor que cero y con hasta dos decimales.";
        if (request.notas == null || request.notas.Length > 500)
            return "Las notas deben contener como máximo 500 caracteres.";
        if (string.IsNullOrWhiteSpace(request.usuario) || request.usuario.Length > 30)
            return "Indique un usuario válido de hasta 30 caracteres.";
        return null;
    }

    private sealed record RegistroScope(SqlConnection Connection, SqlTransaction Transaction);
}
