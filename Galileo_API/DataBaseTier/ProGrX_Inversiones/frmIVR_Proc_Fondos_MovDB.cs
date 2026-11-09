using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones;

public sealed class FrmIvrProcFondosMovDb
{
    private const int CodigoValidacion = -2;
    private const string ConceptoAporte = "FI_APO";
    private const string ConceptoRetiro = "FI_RET";
    private const string EstadoSolicitud = "S";
    private const string EstadoAplicado = "A";
    private const string EstadoAnulado = "X";

    private readonly PortalDB _portalDb;

    public FrmIvrProcFondosMovDb(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _portalDb = new PortalDB(config);
    }

    /// <summary>Obtiene los datos iniciales de un fondo y su último corte.</summary>
    public ErrorDto<IvrProcFondosMovInicialData>
        IVR_Proc_Fondos_Mov_Inicializar(
            int codEmpresa,
            int tituloId,
            string concepto)
    {
        if (tituloId <= 0 || concepto is not (ConceptoAporte or ConceptoRetiro))
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovInicialData>(
                "La inversión o el tipo de movimiento no es válido.",
                CodigoValidacion);
        }

        var response = DbHelper.WithConn(
            _portalDb,
            codEmpresa,
            connection =>
            {
                const string sqlCorte = """
                    SELECT ISNULL(MAX(CORTE), dbo.MyGetdate())
                    FROM IVR_CIERRES;
                    """;

                const string sqlDivisa = """
                    SELECT TOP (1) LTRIM(RTRIM(COD_DIVISA))
                    FROM vSys_Divisas
                    WHERE DIVISA_LOCAL = 1;
                    """;

                const string sqlInversion = """
                    SELECT TOP (1)
                        TITULO_ID AS titulo_id,
                        ISNULL(Administrador_Desc, '') AS administrador_desc,
                        ISNULL(Cod_Administrador, '') AS cod_administrador,
                        ISNULL(Instrumento_Desc, '') AS instrumento_desc,
                        ISNULL(Cod_Instrumento, '') AS cod_instrumento,
                        ISNULL(Portafolio_Desc, '') AS portafolio_desc,
                        ISNULL(Cod_Portafolio, '') AS cod_portafolio,
                        ISNULL(Cod_Divisa, '') AS cod_divisa,
                        ISNULL(Monto_Principal, 0) AS monto_principal,
                        ISNULL(Participacion_Numero, 0)
                            AS participacion_numero,
                        ISNULL(Participacion_Valor, 0)
                            AS participacion_valor
                    FROM vIVR_INVERSIONES
                    WHERE TITULO_ID = @titulo_id;
                    """;

                var corte = connection.QuerySingle<DateTime>(sqlCorte);
                var divisaLocal =
                    connection.QueryFirstOrDefault<string>(sqlDivisa)
                    ?? string.Empty;
                var inversion = connection.QueryFirstOrDefault
                    <IvrProcFondosMovInversionData>(
                        sqlInversion,
                        new { titulo_id = tituloId });

                return new IvrProcFondosMovInicialData
                {
                    inversion = inversion,
                    fecha_ultimo_corte = corte,
                    fecha_movimiento = corte.AddDays(1),
                    divisa_local = divisaLocal,
                    divisa = inversion?.cod_divisa ?? string.Empty,
                    tipo_cambio = 1,
                    concepto = concepto,
                    tipo_mov = concepto == ConceptoAporte ? "A" : "R"
                };
            });

        if (response.Code != 0)
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovInicialData>(
                response.Description ?? "Error al inicializar el fondo.");
        }

        if (response.Result?.inversion == null)
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovInicialData>(
                "No se localizó la inversión indicada.",
                CodigoValidacion);
        }

        if (string.IsNullOrWhiteSpace(response.Result.divisa_local))
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovInicialData>(
                "No se encuentra configurada la divisa local.",
                CodigoValidacion);
        }

        return response;
    }

    /// <summary>Consulta un movimiento del fondo mediante el procedimiento usado por VB6.</summary>
    public ErrorDto<IvrProcFondosMovMovimientoData>
        IVR_Proc_Fondos_Mov_Movimiento_Consultar(
            int codEmpresa,
            int tituloId,
            int transacId)
    {
        if (tituloId <= 0 || transacId <= 0)
        {
            return DbHelper.CreateErrorResponse
                <IvrProcFondosMovMovimientoData>(
                    "Indique la inversión y la transacción.",
                    CodigoValidacion);
        }

        var response = DbHelper.WithConn(
            _portalDb,
            codEmpresa,
            connection => connection.QueryFirstOrDefault
                <IvrProcFondosMovMovimientoData>(
                    """
                    EXEC spIVR_FONDO_MOV_CONSULTA
                        @titulo_id, @transac_id;
                    """,
                    new
                    {
                        titulo_id = tituloId,
                        transac_id = transacId
                    }));

        if (response.Code != 0)
        {
            return DbHelper.CreateErrorResponse
                <IvrProcFondosMovMovimientoData>(
                    response.Description ?? "Error al consultar el movimiento.");
        }

        return response.Result == null
            ? DbHelper.CreateErrorResponse
                <IvrProcFondosMovMovimientoData>(
                    "No se localizó el movimiento.",
                    CodigoValidacion)
            : response;
    }

    /// <summary>Obtiene los movimientos bancarios asociados a la solicitud del fondo.</summary>
    public ErrorDto<IvrProcFondosMovTransaccionesData>
        IVR_Proc_Fondos_Mov_Transacciones_Obtener(
            int codEmpresa,
            int transacId,
            string concepto)
    {
        if (transacId <= 0 || concepto is not (ConceptoAporte or ConceptoRetiro))
        {
            return DbHelper.CreateErrorResponse
                <IvrProcFondosMovTransaccionesData>(
                    "La transacción o el concepto no es válido.",
                    CodigoValidacion);
        }

        return DbHelper.WithConn(
            _portalDb,
            codEmpresa,
            connection =>
            {
                const string sql = """
                    SELECT TRANSAC_ID AS transac_id,
                           ISNULL(Tipo_Mov_Desc, '') AS tipo_mov_desc,
                           Registro_Fecha AS registro_fecha,
                           ISNULL(Importe, 0) AS importe,
                           ISNULL(cta_Mask, '') AS cta_mask,
                           ISNULL(Tipo_Doc_Desc, '') AS tipo_doc_desc,
                           ISNULL(Detalle, '') AS detalle,
                           TES_NSOLICITUD AS tes_nsolicitud,
                           ISNULL(Tes_Estado_Desc, '')
                               AS tes_estado_desc,
                           ISNULL(Cod_Divisa, '') AS cod_divisa,
                           ISNULL(Tipo_Cambio, 0) AS tipo_cambio,
                           ISNULL(Monto, 0) AS monto
                    FROM vIVR_TRANSACCIONES
                    WHERE REF_TIPO = 'F'
                      AND REF_CODIGO = @ref_codigo
                      AND REF_CONCEPTO = @concepto
                    ORDER BY TRANSAC_ID;
                    """;

                var rows = connection.Query
                    <IvrProcCuponesTransaccionData>(
                        sql,
                        new
                        {
                            ref_codigo = transacId.ToString(
                                CultureInfo.InvariantCulture),
                            concepto
                        }).ToList();

                return new IvrProcFondosMovTransaccionesData
                {
                    transacciones = rows,
                    registrado = rows.Sum(row => row.importe)
                };
            });
    }

    /// <summary>Guarda una solicitud de aportación o retiro.</summary>
    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Solicitud_Guardar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request)
        => IVR_Proc_Fondos_Mov_Registrar(
            codEmpresa, request, EstadoSolicitud);

    /// <summary>Procesa una solicitud con el importe bancario completamente registrado.</summary>
    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Procesar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request)
        => IVR_Proc_Fondos_Mov_Registrar(
            codEmpresa, request, EstadoAplicado);

    /// <summary>Anula una solicitud del fondo que sigue en estado de solicitud.</summary>
    public ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Eliminar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request)
        => IVR_Proc_Fondos_Mov_Registrar(
            codEmpresa, request, EstadoAnulado);

    /// <summary>Valida y ejecuta el registro de movimientos del fondo.</summary>
    private ErrorDto<IvrProcFondosMovGuardarData>
        IVR_Proc_Fondos_Mov_Registrar(
            int codEmpresa,
            IvrProcFondosMovGuardarRequest? request,
            string estado)
    {
        if (!IVR_Proc_Fondos_Mov_Datos_Validar(request))
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovGuardarData>(
                "Los datos del movimiento no son válidos.",
                CodigoValidacion);
        }
        if (estado != EstadoSolicitud && request.transac_id <= 0)
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovGuardarData>(
                "Indique una solicitud registrada.",
                CodigoValidacion);
        }

        if (estado == EstadoAplicado
            && string.IsNullOrWhiteSpace(request.documento))
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovGuardarData>(
                "Indique el número de documento.",
                CodigoValidacion);
        }

        var response = DbHelper.WithConn(
            _portalDb,
            codEmpresa,
            connection =>
            {
                const string sqlCorte = """
                    SELECT ISNULL(MAX(CORTE), dbo.MyGetdate())
                    FROM IVR_CIERRES;
                    """;

                var ultimoCorte =
                    connection.QuerySingle<DateTime>(sqlCorte);

                if (estado == EstadoAplicado)
                {
                    IVR_Proc_Fondos_Mov_Aplicacion_Validar(
                        connection, request, ultimoCorte);
                }
                var tipoMov =
                    request.concepto == ConceptoAporte ? "A" : "R";
                var montoFirmado =
                    tipoMov == "A" ? request.monto : -request.monto;

                return connection.QueryFirstOrDefault
                    <IvrProcFondosMovGuardarData>(
                        """
                        EXEC spIVR_FONDO_MOV_REGISTRA
                            @titulo_id, @tipo_mov, @concepto,
                            @fecha, @documento, @monto, @reservado,
                            @n_participaciones, @v_participaciones,
                            @divisa, @tipo_cambio, @notas,
                            @usuario, @estado, @transac_id, @seq_id;
                        """,
                        new
                        {
                            request.titulo_id,
                            tipo_mov = tipoMov,
                            request.concepto,
                            request.fecha,
                            documento = request.documento.Trim(),
                            monto = montoFirmado,
                            reservado = 0,
                            request.n_participaciones,
                            request.v_participaciones,
                            divisa = request.divisa.Trim(),
                            request.tipo_cambio,
                            notas = request.notas.Trim(),
                            usuario = request.usuario.Trim(),
                            estado,
                            request.transac_id,
                            request.seq_id
                        });
            });

        if (response.Code != 0)
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovGuardarData>(
                response.Description ?? "Error al registrar el movimiento.");
        }

        if (estado != EstadoAnulado
            && (response.Result == null
                || response.Result.transac_id <= 0))
        {
            return DbHelper.CreateErrorResponse<IvrProcFondosMovGuardarData>(
                "El proceso no devolvió el número de transacción.");
        }

        return DbHelper.CreateOkResponse(
            response.Result ?? new IvrProcFondosMovGuardarData
            {
                transac_id = request.transac_id,
                seq_id = request.seq_id,
                estado = estado
            });
    }

    /// <summary>Valida el contenido de la solicitud antes de registrar.</summary>
    private static bool IVR_Proc_Fondos_Mov_Datos_Validar(
        IvrProcFondosMovGuardarRequest? request)
    {
        if (request is null)
            return false;

        if (request.titulo_id <= 0
            || request.concepto is not (ConceptoAporte or ConceptoRetiro)
            || request.transac_id < 0
            || request.seq_id < 0
            || request.monto < 0
            || request.v_participaciones <= 0
            || request.n_participaciones < 0
            || request.tipo_cambio == 0)
            return false;

        return !string.IsNullOrWhiteSpace(request.divisa)
            && request.divisa.Length <= 10
            && !string.IsNullOrWhiteSpace(request.usuario)
            && request.usuario.Length <= 30
            && request.documento is { Length: <= 30 }
            && request.notas is { Length: <= 500 };
    }

    /// <summary>Valida corte e importe bancario antes de aplicar.</summary>
    private static void IVR_Proc_Fondos_Mov_Aplicacion_Validar(
        SqlConnection connection,
        IvrProcFondosMovGuardarRequest request,
        DateTime ultimoCorte)
    {
        if (request.fecha < ultimoCorte)
            throw new InvalidOperationException(
                "La fecha es menor al último corte.");

        var registrado = IVR_Proc_Fondos_Mov_Registrado_Consultar(
            connection, request.transac_id, request.concepto);
        var importeLocal = decimal.Round(
            request.monto
            * Convert.ToDecimal(
                MProGrxMain.fxSys_Tipo_Cambio_Apl(request.tipo_cambio)),
            2,
            MidpointRounding.AwayFromZero);

        if (importeLocal <= 0 || importeLocal != registrado)
            throw new InvalidOperationException(
                "El importe del movimiento no está registrado completamente.");
    }
    /// <summary>Suma el importe de los registros bancarios vinculados al fondo.</summary>
    private static decimal IVR_Proc_Fondos_Mov_Registrado_Consultar(
        SqlConnection connection,
        int transacId,
        string concepto)
    {
        const string sql = """
            SELECT ISNULL(SUM(ISNULL(Importe, 0)), 0)
            FROM vIVR_TRANSACCIONES
            WHERE REF_TIPO = 'F'
              AND REF_CODIGO = @ref_codigo
              AND REF_CONCEPTO = @concepto;
            """;

        return connection.QuerySingle<decimal>(
            sql,
            new
            {
                ref_codigo = transacId.ToString(
                    CultureInfo.InvariantCulture),
                concepto
            });
    }
}