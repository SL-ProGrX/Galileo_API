using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed partial class FrmIvrRecTitulosDb
    {
        /// <summary>
        /// Obtiene la fecha de trabajo y los catálogos requeridos por el registro de inversiones.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Datos iniciales del formulario.</returns>
        public ErrorDto<IvrRecTitulosInicialData> IVR_Rec_Titulos_Inicializar(
            int codEmpresa)
        {
            const string sql = """
                SELECT GETDATE() AS fecha_servidor,
                       ISNULL((SELECT TOP 1 RTRIM(COD_DIVISA)
                               FROM vSys_Divisas
                               WHERE DIVISA_LOCAL = 1), '') AS divisa_local;

                SELECT RTRIM(COD_INSTRUMENTO) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       RTRIM(TIPO) AS tipo,
                       0 AS dias
                FROM IVR_INSTRUMENTOS
                WHERE ACTIVO = 1
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_RECURSO) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM IVR_FUENTE_RECURSOS
                WHERE ACTIVA = 1
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_EMISOR) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM IVR_EMISORES
                WHERE ACTIVO = 1
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_ADMINISTRADOR) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM IVR_ADMINISTRADOR
                WHERE ESTADO = 'A'
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_PORTAFOLIO) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM IVR_PORTAFOLIOS
                WHERE ACTIVO = 1
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_CATEGORIA) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM IVR_CATEGORIA_TIPOS
                WHERE ACTIVO = 1
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_RESERVA) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM IVR_RESERVAS
                WHERE ACTIVA = 1
                ORDER BY DESCRIPCION;

                SELECT RTRIM(COD_PERIODICIDAD) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo,
                       ISNULL(DIAS, 0) AS dias
                FROM IVR_PERIODICIDAD
                WHERE ACTIVA = 1
                ORDER BY DIAS;

                SELECT RTRIM(COD_DIVISA) AS codigo,
                       RTRIM(DESCRIPCION) AS descripcion,
                       '' AS tipo, 0 AS dias
                FROM vSys_Divisas
                ORDER BY DESCRIPCION;
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    using var grid = connection.QueryMultiple(sql);
                    var encabezado = grid.ReadSingle<IvrRecTitulosInicialData>();
                    encabezado.instrumentos = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.recursos = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.emisores = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.administradores = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.portafolios = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.clasificaciones = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.reservas = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.periodicidades = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    encabezado.divisas = grid.Read<IvrRecTitulosCatalogoData>().AsList();
                    return encabezado;
                });
        }

        /// <summary>
        /// Busca inversiones por identificador, operación, serie o descripciones principales.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="filtro">Texto de búsqueda.</param>
        /// <returns>Inversiones coincidentes.</returns>
        public ErrorDto<List<IvrRecTitulosBusquedaData>> IVR_Rec_Titulos_Buscar(
            int codEmpresa,
            string? filtro)
        {
            const string sql = """
                SELECT TOP (300)
                       TITULO_ID AS titulo_id,
                       RTRIM(ISNULL(OPERACION, '')) AS operacion,
                       RTRIM(ISNULL(SERIE, '')) AS serie,
                       RTRIM(ISNULL(ESTADO_DESC, '')) AS estado_desc,
                       RTRIM(ISNULL(INSTRUMENTO_DESC, '')) AS instrumento_desc,
                       RTRIM(ISNULL(ADMINISTRADOR_DESC, '')) AS administrador_desc,
                       RTRIM(ISNULL(RECURSO_DESC, '')) AS recurso_desc
                FROM vIVR_INVERSIONES
                WHERE @filtro = ''
                   OR CONVERT(varchar(20), TITULO_ID) LIKE '%' + @filtro + '%'
                   OR ISNULL(OPERACION, '') LIKE '%' + @filtro + '%'
                   OR ISNULL(SERIE, '') LIKE '%' + @filtro + '%'
                   OR ISNULL(INSTRUMENTO_DESC, '') LIKE '%' + @filtro + '%'
                   OR ISNULL(ADMINISTRADOR_DESC, '') LIKE '%' + @filtro + '%'
                ORDER BY TITULO_ID DESC;
                """;

            var texto = IVR_Rec_Titulos_Texto_Normalizar(filtro, 80);
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.Query<IvrRecTitulosBusquedaData>(
                    sql,
                    new { filtro = texto }).AsList());
        }

        /// <summary>
        /// Obtiene la inversión anterior o siguiente respecto al identificador actual.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador actual.</param>
        /// <param name="siguiente">Indica la dirección de navegación.</param>
        /// <returns>Identificador de la inversión encontrada.</returns>
        public ErrorDto<IvrRecTitulosNavegacionData?> IVR_Rec_Titulos_Navegar(
            int codEmpresa,
            long tituloId,
            bool siguiente)
        {
            const string sqlSiguiente = """
                SELECT TOP (1) TITULO_ID AS titulo_id
                FROM vIVR_INVERSIONES
                WHERE TITULO_ID > @tituloId
                ORDER BY TITULO_ID ASC;
                """;
            const string sqlAnterior = """
                SELECT TOP (1) TITULO_ID AS titulo_id
                FROM vIVR_INVERSIONES
                WHERE TITULO_ID < @tituloId
                ORDER BY TITULO_ID DESC;
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.QueryFirstOrDefault<IvrRecTitulosNavegacionData>(
                    siguiente ? sqlSiguiente : sqlAnterior,
                    new { tituloId }));
        }

        /// <summary>
        /// Obtiene toda la información principal de una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <returns>Información de la inversión.</returns>
        public ErrorDto<IvrRecTitulosData?> IVR_Rec_Titulos_Obtener(
            int codEmpresa,
            long tituloId)
        {
            if (!IVR_Rec_Titulos_Id_Valido(tituloId))
            {
                return DbHelper.CreateErrorResponse<IvrRecTitulosData?>(
                    MensajeInversionInvalida,
                    CodigoValidacion);
            }

            const string sql = """
                SELECT TITULO_ID AS titulo_id,
                       RTRIM(ISNULL(ESTADO, '')) AS estado,
                       RTRIM(ISNULL(ESTADO_DESC, '')) AS estado_desc,
                       RTRIM(ISNULL(IB_DOCUMENTO, '')) AS ib_documento,
                       LEFT(RTRIM(ISNULL(INSTRUMENTO_IDX, '')), 1) AS tipo_instrumento,
                       RTRIM(ISNULL(INSTRUMENTO_IDX, '')) AS instrumento_idx,
                       RTRIM(ISNULL(INSTRUMENTO_DESC, '')) AS instrumento_desc,
                       RTRIM(ISNULL(COD_EMISOR, '')) AS cod_emisor,
                       RTRIM(ISNULL(EMISOR_DESC, '')) AS emisor_desc,
                       RTRIM(ISNULL(COD_ADMINISTRADOR, '')) AS cod_administrador,
                       RTRIM(ISNULL(ADMINISTRADOR_DESC, '')) AS administrador_desc,
                       RTRIM(ISNULL(COD_CATEGORIA, '')) AS cod_categoria,
                       RTRIM(ISNULL(CATEGORIA_DESC, '')) AS categoria_desc,
                       RTRIM(ISNULL(COD_RECURSO, '')) AS cod_recurso,
                       RTRIM(ISNULL(RECURSO_DESC, '')) AS recurso_desc,
                       RTRIM(ISNULL(COD_PORTAFOLIO, '')) AS cod_portafolio,
                       RTRIM(ISNULL(PORTAFOLIO_DESC, '')) AS portafolio_desc,
                       RTRIM(ISNULL(COD_RESERVA, '')) AS cod_reserva,
                       RTRIM(ISNULL(RESERVA_DESC, '')) AS reserva_desc,
                       RTRIM(ISNULL(COD_DIVISA, '')) AS cod_divisa,
                       RTRIM(ISNULL(DIVISA_DESC, '')) AS divisa_desc,
                       RTRIM(ISNULL(COD_PERIODICIDAD, '')) AS cod_periodicidad,
                       RTRIM(ISNULL(PERIODICIDAD_DESC, '')) AS periodicidad_desc,
                       ISNULL(DETALLE, '') AS detalle,
                       RTRIM(ISNULL(EJECUTIVO_CUENTA, '')) AS ejecutivo_cuenta,
                       RTRIM(ISNULL(OPERACION, '')) AS operacion,
                       RTRIM(ISNULL(SERIE, '')) AS serie,
                       RTRIM(ISNULL(ISIN, '')) AS isin,
                       RTRIM(ISNULL(CUSIP, '')) AS cusip,
                       ISNULL(BASE_INTERESES, 360) AS base_intereses,
                       CONVERT(bit, ISNULL(CALCULA_INTERESES, 0)) AS calcula_intereses,
                       ISNULL(TIPO_CAMBIO, 0) AS tipo_cambio,
                       ISNULL(VALOR_FACIAL, 0) AS valor_facial,
                       ISNULL(VALOR_TRANSADO, 0) AS valor_transado,
                       ISNULL(COSTO_NETO, 0) AS costo_neto,
                       ISNULL(COMISION_PUESTO, 0) AS comision_puesto,
                       ISNULL(COMISION_BNV, 0) AS comision_bnv,
                       ISNULL(IVA, 0) AS iva,
                       ISNULL(TASA_INICIAL, 0) AS tasa_inicial,
                       ISNULL(REND_NETO, 0) AS rend_neto,
                       ISNULL(REND_NOMINAL, 0) AS rend_nominal,
                       ISNULL(TASA_EFECTIVA, 0) AS tasa_efectiva,
                       ISNULL(TIR, 0) AS tir,
                       ISNULL(NDIAS_INVERSION, 0) AS ndias_inversion,
                       ISNULL(INTERES_ACUM_DIAS, 0) AS interes_acum_dias,
                       ISNULL(INTERES_ACUM_MONTO, 0) AS interes_acum_monto,
                       FECHA_PAGO AS fecha_pago,
                       ISNULL(PYD_BASE, 0) AS pyd_base,
                       ISNULL(PRECIO, 0) AS precio,
                       ISNULL(PARTICIPACION_NUMERO, 0) AS participacion_numero,
                       ISNULL(PARTICIPACION_VALOR, 0) AS participacion_valor,
                       ISNULL(MONTO_PRINCIPAL, 0) AS monto_principal,
                       FECHA_COMPRA AS fecha_compra,
                       FECHA_VENCIMIENTO AS fecha_vencimiento,
                       ISNULL(ISR_MONTO, 0) AS isr_monto,
                       ISNULL(VALOR_LIBROS, 0) AS valor_libros,
                       FECHA_CORTE AS fecha_corte
                FROM vIVR_INVERSIONES
                WHERE TITULO_ID = @tituloId;
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.QueryFirstOrDefault<IvrRecTitulosData>(
                    sql,
                    new { tituloId }));
        }

        /// <summary>
        /// Obtiene las adquisiciones registradas para una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <returns>Adquisiciones vinculadas con la inversión.</returns>
        public ErrorDto<List<IvrRecTitulosTransaccionData>> IVR_Rec_Titulos_Adquisiciones_Obtener(
            int codEmpresa,
            long tituloId)
        {
            const string sql = """
                SELECT TRANSAC_ID AS transac_id,
                       RTRIM(ISNULL(TIPO_MOV_DESC, '')) AS tipo_mov_desc,
                       REGISTRO_FECHA AS registro_fecha,
                       ISNULL(IMPORTE, 0) AS importe,
                       RTRIM(ISNULL(CTA_MASK, '')) AS cta_mask,
                       RTRIM(ISNULL(TIPO_DOC_DESC, '')) AS tipo_doc_desc,
                       ISNULL(DETALLE, '') AS detalle,
                       TES_NSOLICITUD AS tes_nsolicitud,
                       RTRIM(ISNULL(TES_ESTADO_DESC, '')) AS tes_estado_desc,
                       RTRIM(ISNULL(COD_DIVISA, '')) AS cod_divisa,
                       ISNULL(TIPO_CAMBIO, 0) AS tipo_cambio,
                       ISNULL(MONTO, 0) AS monto
                FROM vIVR_TRANSACCIONES
                WHERE REF_TIPO = 'T'
                  AND REF_CODIGO = CONVERT(varchar(30), @tituloId)
                  AND REF_CONCEPTO = 'T'
                ORDER BY TRANSAC_ID DESC;
                """;
            return IVR_Rec_Titulos_Lista_Obtener<IvrRecTitulosTransaccionData>(codEmpresa, tituloId, sql);
        }

        /// <summary>
        /// Obtiene las comisiones asociadas a una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <returns>Comisiones vinculadas con la inversión.</returns>
        public ErrorDto<List<IvrRecTitulosComisionData>> IVR_Rec_Titulos_Comisiones_Obtener(
            int codEmpresa,
            long tituloId)
        {
            const string sql = """
                SELECT RTRIM(ISNULL(TC.COD_COMISION, '')) AS cod_comision,
                       RTRIM(ISNULL(CT.DESCRIPCION, '')) AS descripcion,
                       ISNULL(TC.MONTO, 0) AS monto,
                       RTRIM(ISNULL(CT.COD_CUENTA_MASK, '')) AS cod_cuenta_mask
                FROM IVR_TITULO_COMISIONES TC
                INNER JOIN vIVR_COMISIONES_TIPOS CT
                    ON TC.COD_COMISION = CT.COD_COMISION
                WHERE TC.TITULO_ID = @tituloId
                ORDER BY TC.COD_COMISION;
                """;
            return IVR_Rec_Titulos_Lista_Obtener<IvrRecTitulosComisionData>(codEmpresa, tituloId, sql);
        }

        /// <summary>
        /// Obtiene los flujos de intereses o primas y descuentos de una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <param name="tipo">Tipo de flujo solicitado.</param>
        /// <returns>Flujos calculados para la inversión.</returns>
        public ErrorDto<List<IvrRecTitulosFlujoData>> IVR_Rec_Titulos_Flujos_Obtener(
            int codEmpresa,
            long tituloId,
            string? tipo)
        {
            var esInteres = string.Equals(tipo, "INT", StringComparison.OrdinalIgnoreCase);
            const string sqlIntereses = """
                SELECT CORTE AS corte,
                       '' AS tipo,
                       ISNULL(DIAS, 0) AS dias,
                       ISNULL(MNT_PERIODO, 0) AS monto,
                       ISNULL(ACUM_DIAS, 0) AS acum_dias,
                       ISNULL(ACUM_MNT, 0) + ISNULL(MNT_APLICADO, 0) AS acum_monto,
                       0 AS saldo, 0 AS inversion, 0 AS monto_diario,
                       ISNULL(MNT_PERIODO, 0) AS interes_calculado,
                       ISNULL(MNT_APLICADO, 0) AS interes_aplicado,
                       ISNULL(ACUM_MNT, 0) + ISNULL(MNT_PERIODO, 0) - ISNULL(ISR_ACUMULADO, 0) AS acum_monto_neto,
                       ISNULL(CUPON_PAGO, 0) + ISNULL(ISR_ACUMULADO, 0) AS cupon,
                       ISNULL(ISR_MONTO, 0) AS isr_aplicado,
                       ISNULL(ISR_SALDO, 0) AS isr_saldo,
                       ISNULL(ISR_ACUMULADO, 0) AS isr_acumulado
                FROM IVR_TITULO_INTERESES
                WHERE TITULO_ID = @tituloId
                ORDER BY CORTE;
                """;
            const string sqlPyd = """
                SELECT CORTE AS corte,
                       RTRIM(ISNULL(TIPO, '')) AS tipo,
                       ISNULL(PYD_DIAS, 0) AS dias,
                       ISNULL(PYD_MONTO, 0) AS monto,
                       ISNULL(PYD_ACUM_DIAS, 0) AS acum_dias,
                       ISNULL(PYD_ACUM_MNT, 0) AS acum_monto,
                       ISNULL(PYD_SALDO, 0) AS saldo,
                       ISNULL(SALDO_INVERSION, 0) AS inversion,
                       ISNULL(PYD_DIARIA, 0) AS monto_diario,
                       0 AS interes_calculado, 0 AS interes_aplicado,
                       0 AS acum_monto_neto, 0 AS cupon,
                       0 AS isr_aplicado, 0 AS isr_saldo, 0 AS isr_acumulado
                FROM IVR_TITULO_PYD
                WHERE TITULO_ID = @tituloId
                ORDER BY CORTE;
                """;
            return IVR_Rec_Titulos_Lista_Obtener<IvrRecTitulosFlujoData>(
                codEmpresa,
                tituloId,
                esInteres ? sqlIntereses : sqlPyd);
        }

        /// <summary>
        /// Obtiene los cierres contabilizados para una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <returns>Cierres contabilizados de la inversión.</returns>
        public ErrorDto<List<IvrRecTitulosCierreData>> IVR_Rec_Titulos_Cierres_Obtener(
            int codEmpresa,
            long tituloId)
        {
            const string sql = """
                SELECT CORTE AS corte,
                       ISNULL(VALOR_FACIAL, 0) AS valor_facial,
                       ISNULL(VALOR_LIBROS, 0) AS valor_libros,
                       ISNULL(VALOR_MERCADO, 0) AS valor_mercado,
                       ISNULL(TASA_ACTUAL, 0) AS tasa_actual,
                       RTRIM(ISNULL(PYD_TIPO, '')) AS pyd_tipo,
                       ISNULL(PYD_MES_MONTO, 0) AS pyd_mes_monto,
                       ISNULL(PYD_MONTO_ACUMULADO, 0) AS pyd_monto_acumulado,
                       ISNULL(PYD_BASE, 0) - ISNULL(PYD_MONTO_ACUMULADO, 0) AS pyd_saldo,
                       ISNULL(DIAS_MES, 0) AS dias_mes,
                       ISNULL(DIAS_MES_MONTO, 0) AS dias_mes_monto,
                       ISNULL(DIAS_ACUMULADOS, 0) AS dias_acumulados,
                       ISNULL(DIAS_ACUM_MONTO, 0) AS dias_acum_monto
                FROM IVR_CIERRE_TITULOS
                WHERE TITULO_ID = @tituloId AND ESTADO = 'A'
                ORDER BY CORTE DESC;
                """;
            return IVR_Rec_Titulos_Lista_Obtener<IvrRecTitulosCierreData>(codEmpresa, tituloId, sql);
        }

        /// <summary>
        /// Obtiene los movimientos de fondos de una inversión aplicando los filtros indicados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <param name="estado">Estado de los movimientos.</param>
        /// <param name="tipo">Tipo de movimiento.</param>
        /// <returns>Movimientos de fondos que cumplen los filtros.</returns>
        public ErrorDto<List<IvrRecTitulosFondoData>> IVR_Rec_Titulos_Fondos_Obtener(
            int codEmpresa,
            long tituloId,
            string? estado,
            string? tipo)
        {
            const string sql = """
                SELECT TOP (150)
                       FI_TRANSAC AS fi_transac,
                       RTRIM(ISNULL(ESTADO, '')) AS estado,
                       ISNULL(SEQ_ID, 0) AS seq_id,
                       RTRIM(ISNULL(TIPO_MOV, '')) AS tipo_mov,
                       FECHA AS fecha,
                       ISNULL(SALDO_ACTUAL, 0) AS saldo_actual,
                       ISNULL(MNT_PRINCIPAL, 0) AS mnt_principal,
                       ISNULL(MNT_INTERESES, 0) AS mnt_intereses,
                       ISNULL(N_PARTICIPACIONES, 0) AS n_participaciones,
                       ISNULL(V_PARTICIPACIONES, 0) AS v_participaciones
                FROM IVR_TITULOS_FI_MOV
                WHERE TITULO_ID = @tituloId
                  AND (@estado = 'T' OR ESTADO = @estado)
                  AND (@tipo = 'T' OR TIPO_MOV = @tipo)
                ORDER BY SEQ_ID DESC, FECHA DESC;
                """;

            if (!IVR_Rec_Titulos_Id_Valido(tituloId))
            {
                return DbHelper.CreateErrorResponse<List<IvrRecTitulosFondoData>>(
                    MensajeInversionInvalida,
                    CodigoValidacion,
                    []);
            }

            var estadoSeguro = IVR_Rec_Titulos_Filtro_Normalizar(estado, "T");
            var tipoSeguro = IVR_Rec_Titulos_Filtro_Normalizar(tipo, "T");
            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.Query<IvrRecTitulosFondoData>(
                    sql,
                    new { tituloId, estado = estadoSeguro, tipo = tipoSeguro }).AsList());

            if (response.Result != null)
            {
                foreach (var item in response.Result)
                {
                    item.tipo_mov_desc = IVR_Rec_Titulos_Tipo_Movimiento_Descripcion(item.tipo_mov);
                }
            }

            return response;
        }

        /// <summary>
        /// Obtiene los cupones registrados o proyectados de una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <param name="proyectados">Indica si se consultan cupones proyectados.</param>
        /// <returns>Cupones de la inversión.</returns>
        public ErrorDto<List<IvrRecTitulosCuponData>> IVR_Rec_Titulos_Cupones_Obtener(
            int codEmpresa,
            long tituloId,
            bool proyectados)
        {
            const string sqlRegistrados = """
                SELECT CUPON_ID AS cupon_id,
                       ISNULL(SEQ_ID, 0) AS seq_id,
                       FECHA_INICIO AS fecha_inicio,
                       FECHA_CORTE AS fecha_corte,
                       ISNULL(MNT_INTERES, 0) AS mnt_interes,
                       ISNULL(MNT_INTERES_APL, 0) AS mnt_interes_apl,
                       ISNULL(MNT_PRINCIPAL, 0) AS mnt_principal,
                       RTRIM(ISNULL(DOCUMENTO, '')) AS documento,
                       RTRIM(ISNULL(TCON, '')) + '_' + CONVERT(varchar(30), ISNULL(NCON, 0)) AS referencia
                FROM IVR_TITULO_CUPONES
                WHERE ESTADO = 'A' AND TITULO_ID = @tituloId
                ORDER BY FECHA_INICIO;
                """;
            const string sqlProyectados = """
                SELECT SEQ_ID AS cupon_id,
                       ISNULL(SEQ_ID, 0) AS seq_id,
                       FECHA_INICIO AS fecha_inicio,
                       FECHA_CORTE AS fecha_corte,
                       ISNULL(MNT_INTERES, 0) AS mnt_interes,
                       ISNULL(MNT_INTERES_APL, 0) AS mnt_interes_apl,
                       ISNULL(MNT_PRINCIPAL, 0) AS mnt_principal,
                       RTRIM(ISNULL(DOCUMENTO, '')) AS documento,
                       RTRIM(ISNULL(TCON, '')) + '_' + CONVERT(varchar(30), ISNULL(NCON, 0)) AS referencia
                FROM IVR_TITULO_CUPONES_PROY
                WHERE TITULO_ID = @tituloId
                ORDER BY FECHA_INICIO;
                """;
            return IVR_Rec_Titulos_Lista_Obtener<IvrRecTitulosCuponData>(
                codEmpresa,
                tituloId,
                proyectados ? sqlProyectados : sqlRegistrados);
        }

        /// <summary>
        /// Obtiene los asientos relacionados con una inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <returns>Asientos relacionados con la inversión.</returns>
        public ErrorDto<List<IvrRecTitulosAsientoData>> IVR_Rec_Titulos_Asientos_Obtener(
            int codEmpresa,
            long tituloId)
        {
            const string sql = """
                EXEC spIVR_Asientos_Consulta
                     @inicio,
                     @corte,
                     '',
                     @detalle,
                     '';
                """;

            if (!IVR_Rec_Titulos_Id_Valido(tituloId))
            {
                return DbHelper.CreateErrorResponse<List<IvrRecTitulosAsientoData>>(
                    MensajeInversionInvalida,
                    CodigoValidacion,
                    []);
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var fechaCompra = connection.QueryFirstOrDefault<DateTime?>(
                        "SELECT FECHA_COMPRA FROM vIVR_INVERSIONES WHERE TITULO_ID = @tituloId;",
                        new { tituloId }) ?? DateTime.Today;
                    return connection.Query<IvrRecTitulosAsientoData>(
                        sql,
                        new
                        {
                            inicio = fechaCompra.Date,
                            corte = DateTime.Today.AddDays(1).AddTicks(-1),
                            detalle = $"Inversion Id: {tituloId}",
                        }).AsList();
                });
        }

        /// <summary>
        /// Obtiene el asiento contable principal registrado directamente en la inversión.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tituloId">Identificador de la inversión.</param>
        /// <returns>Líneas contables del asiento principal.</returns>
        public ErrorDto<List<IvrRecTitulosAsientoDetalleData>> IVR_Rec_Titulos_Asiento_Titulo_Obtener(
            int codEmpresa,
            long tituloId)
        {
            const string sql = """
                SELECT A.LINEA_ID AS linea_id,
                       RTRIM(ISNULL(A.COD_CUENTA_MASK, '')) AS cod_cuenta_mask,
                       RTRIM(ISNULL(A.DESCRIPCION, '')) AS descripcion,
                       ISNULL(A.MONTO_DEBITO, 0) AS monto_debito,
                       ISNULL(A.MONTO_CREDITO, 0) AS monto_credito,
                       RTRIM(ISNULL(A.COD_DIVISA, '')) AS cod_divisa,
                       ISNULL(A.TIPO_CAMBIO, 0) AS tipo_cambio,
                       RTRIM(ISNULL(A.NUM_ASIENTO, '')) AS num_asiento,
                       RTRIM(ISNULL(A.DOCUMENTO, '')) AS documento,
                       ISNULL(A.DETALLE, '') AS detalle,
                       ISNULL(A.REFERENCIA, '') AS referencia
                FROM IVR_TITULOS T
                INNER JOIN vIVR_ASIENTOS_DETALLE A
                    ON T.NUM_ASIENTO = A.NUM_ASIENTO
                WHERE T.TITULO_ID = @tituloId
                ORDER BY A.LINEA_ID;
                """;
            return IVR_Rec_Titulos_Lista_Obtener<IvrRecTitulosAsientoDetalleData>(
                codEmpresa,
                tituloId,
                sql);
        }

        /// <summary>
        /// Obtiene el detalle de un asiento seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="tipoAsiento">Tipo de asiento.</param>
        /// <param name="numeroAsiento">Número de asiento.</param>
        /// <returns>Detalle contable del asiento.</returns>
        public ErrorDto<List<IvrRecTitulosAsientoDetalleData>> IVR_Rec_Titulos_Asiento_Detalle_Obtener(
            int codEmpresa,
            string? tipoAsiento,
            string? numeroAsiento)
        {
            const string sql = """
                SELECT LINEA_ID AS linea_id,
                       RTRIM(ISNULL(COD_CUENTA_MASK, '')) AS cod_cuenta_mask,
                       RTRIM(ISNULL(DESCRIPCION, '')) AS descripcion,
                       ISNULL(MONTO_DEBITO, 0) AS monto_debito,
                       ISNULL(MONTO_CREDITO, 0) AS monto_credito,
                       RTRIM(ISNULL(COD_DIVISA, '')) AS cod_divisa,
                       ISNULL(TIPO_CAMBIO, 0) AS tipo_cambio,
                       RTRIM(ISNULL(NUM_ASIENTO, '')) AS num_asiento,
                       RTRIM(ISNULL(DOCUMENTO, '')) AS documento,
                       ISNULL(DETALLE, '') AS detalle,
                       ISNULL(REFERENCIA, '') AS referencia
                FROM vIVR_ASIENTOS_DETALLE
                WHERE TIPO_ASIENTO = @tipoAsiento
                  AND NUM_ASIENTO = @numeroAsiento
                ORDER BY LINEA_ID;
                """;
            var tipoSeguro = IVR_Rec_Titulos_Texto_Normalizar(tipoAsiento, 10);
            var numeroSeguro = IVR_Rec_Titulos_Texto_Normalizar(numeroAsiento, 30);
            if (tipoSeguro.Length == 0 || numeroSeguro.Length == 0)
            {
                return DbHelper.CreateErrorResponse<List<IvrRecTitulosAsientoDetalleData>>(
                    "Seleccione un asiento válido.",
                    CodigoValidacion,
                    []);
            }
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.Query<IvrRecTitulosAsientoDetalleData>(
                    sql,
                    new { tipoAsiento = tipoSeguro, numeroAsiento = numeroSeguro }).AsList());
        }

        private ErrorDto<List<T>> IVR_Rec_Titulos_Lista_Obtener<T>(
            int codEmpresa,
            long tituloId,
            string sql)
        {
            if (!IVR_Rec_Titulos_Id_Valido(tituloId))
            {
                return DbHelper.CreateErrorResponse<List<T>>(
                    MensajeInversionInvalida,
                    CodigoValidacion,
                    []);
            }
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection.Query<T>(sql, new { tituloId }).AsList());
        }

        private static string IVR_Rec_Titulos_Filtro_Normalizar(
            string? valor,
            string valorPredeterminado)
        {
            var texto = (valor ?? string.Empty).Trim().ToUpperInvariant();
            return texto.Length == 1 && char.IsLetter(texto[0])
                ? texto
                : valorPredeterminado;
        }
    }
}
