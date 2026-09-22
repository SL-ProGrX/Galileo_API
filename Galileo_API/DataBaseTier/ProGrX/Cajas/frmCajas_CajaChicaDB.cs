using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo_API.DataBaseTier;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCajaChicaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MRecibos _mRecibos;
        public FrmCajasCajaChicaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mRecibos = new MRecibos(config);
        }

        /// <summary>
        /// Método para buscar servicios asignados a una caja específica que coincidan con un término de búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <param name="servicioBusqueda"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCajaChicaServiciosDto>> Cajas_CajaChicaServicios_Buscar(
                int codEmpresa,
                string codCaja,
                string servicioBusqueda)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                        SELECT
                            S.cod_servicio        AS cod_servicio,
                            S.descripcion         AS serviciodesc,
                            R.cod_recaudador      AS cod_recaudador,
                            R.descripcion         AS recaudadordesc
                        FROM cajas_servicios_asignados X
                        INNER JOIN cajas_servicios S
                            ON X.cod_recaudador = S.cod_recaudador
                           AND X.cod_servicio   = S.cod_servicio
                        INNER JOIN cajas_recaudador R
                            ON S.cod_recaudador = R.cod_recaudador
                        WHERE
                            X.cod_caja = @CodCaja
                            AND S.descripcion LIKE '%' + @ServicioBusqueda + '%'
                            AND S.cod_concepto IN ('CAJ002')
                        ORDER BY S.descripcion;";

                var data = conn
                    .Query<CajasCajaChicaServiciosDto>(sql, new
                    {
                        CodCaja = codCaja,
                        ServicioBusqueda = servicioBusqueda?.Trim() ?? string.Empty
                    })
                    .ToList();

                return data;
            });
        }


        /// <summary>
        /// Método para obtener los tipos de documentos asociados a una caja específica.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajaChicaDocumentos_Obtener(
                int codEmpresa,
                string codCaja)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                            SELECT
                                RTRIM(Doc.Tipo_Documento) AS item,
                                RTRIM(Doc.Descripcion)   AS descripcion
                            FROM Cajas_Documentos Cj
                            INNER JOIN SIF_Documentos Doc
                                ON Cj.Tipo_Documento = Doc.Tipo_Documento
                            WHERE
                                Cj.Cod_Caja = @CodCaja
                                AND Doc.Tipo_Documento IN ('CAJA', 'CAJRE')
                            ORDER BY Doc.Descripcion;";

                var data = conn
                    .Query<DropDownListaGenericaModel>(sql, new
                    {
                        CodCaja = codCaja
                    })
                    .ToList();

                return data;
            });
        }

        /// <summary>
        /// Método para obtener las divisas disponibles en una contabilidad específica.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_CajaChicaDivisas_Obtener(
            int codEmpresa,
            int codContabilidad)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                SELECT
                    RTRIM(cod_divisa)   AS item,
                    RTRIM(descripcion) AS descripcion
                FROM cntx_divisas
                WHERE cod_contabilidad = @CodContabilidad
                ORDER BY cod_divisa;";

                var data = conn
                    .Query<DropDownListaGenericaModel>(sql, new
                    {
                        CodContabilidad = codContabilidad
                    })
                    .ToList();

                return data;
            });
        }

        /// <summary>
        /// Método para obtener el tipo de cambio de una divisa específica en una contabilidad dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<CajasCajaChicaTipoCambioRsDto> Cajas_CajaChicaTipoCambio_Obtener(
                int codEmpresa,
                int codContabilidad,
                string codDivisa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                SELECT
                    tc_venta  AS tc_venta,
                    tc_compra AS tc_compra
                FROM cntx_divisas
                WHERE
                    cod_divisa = @CodDivisa
                    AND cod_contabilidad = @CodContabilidad;";

                var data = conn.QueryFirstOrDefault<CajasCajaChicaTipoCambioRsDto>(sql, new
                {
                    CodDivisa = codDivisa,
                    CodContabilidad = codContabilidad
                });

                // Si no existe la divisa, devolvemos 0 / 0 como hacía VB
                return data ?? new CajasCajaChicaTipoCambioRsDto
                {
                    tc_venta = 0,
                    tc_compra = 0
                };
            });
        }

        /// <summary>
        /// Método para buscar socios según un filtro de nombre.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtroNombre"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasCajaChicaSociosBusquedaRsDto>> Cajas_CajaChicaSocios_Buscar(
                int codEmpresa,
                string? filtroNombre)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                        SELECT
                            Cedula   AS cedula,
                            CedulaR  AS cedular,
                            Nombre   AS nombre
                        FROM socios
                        WHERE
                            (@Filtro IS NULL OR @Filtro = '' OR Nombre LIKE '%' + @Filtro + '%')
                        ORDER BY Nombre;";

                var data = conn
                    .Query<CajasCajaChicaSociosBusquedaRsDto>(sql, new
                    {
                        Filtro = filtroNombre?.Trim()
                    })
                    .ToList();

                return data;
            });
        }


        /// <summary>
        /// Obtiene el nombre del socio por cédula, equivalente a fxNombre(txtCedula.Text).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<CajasCajaChicaSociosBusquedaRsDto?> Cajas_CajaChicaSocio_Cedula_Obtener(
                int codEmpresa,
                string cedula)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                        SELECT TOP 1
                            Cedula   AS cedula,
                            CedulaR  AS cedular,
                            Nombre   AS nombre
                        FROM socios
                        WHERE Cedula = @Cedula;";

                return conn.QueryFirstOrDefault<CajasCajaChicaSociosBusquedaRsDto>(sql, new
                {
                    Cedula = cedula?.Trim() ?? string.Empty
                });
            });
        }
        //======= Procesa Cajas_CajaChica_Guardar =======||

        /// <summary>
        /// Método para procesar el retiro y aplicar en caja chica.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CajasCajaChicaAplicarDbResponseDto> Cajas_CajaChicaRetiro_Aplicar(
                 CajasCajaChicaAplicarDbRequestDto req)
        {
            if (req == null)
                return DbHelper.CreateErrorResponse<CajasCajaChicaAplicarDbResponseDto>("La solicitud es requerida.");

            var validacion = ValidarRetiro(req);
            if (!string.IsNullOrWhiteSpace(validacion))
                return DbHelper.CreateErrorResponse<CajasCajaChicaAplicarDbResponseDto>(validacion);

            return DbHelper.WithConn(_portalDB, req.codempresa, conn =>
            {
                conn.Open();
                using var tx = conn.BeginTransaction();

                ValidarAperturaAbierta(conn, tx, req);
                PrepararDatosAplicacion(conn, tx, req);

                var srv = ObtenerServiciosDatos(conn, tx, req);
                CompletarDescripcionesServicio(conn, tx, req);

                InsertarSifTransacciones(conn, tx, req, srv);
                InsertarCajasServiciosTransac(conn, tx, req, srv);

                RegistrarAsientoDebito(conn, tx, req, srv);
                RegistrarAsientoCredito(conn, tx, req, srv);

                RegistrarFormaPagoEfectivo(conn, tx, req, srv);

                tx.Commit();

                return new CajasCajaChicaAplicarDbResponseDto
                {
                    tipo_documento = req.tipo_documento,
                    numdoc = req.numdoc
                };
            });
        }

        private static CajasCajaChicaServiciosDatosRsDto ObtenerServiciosDatos(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req)
        {
            const string sql = @"
        EXEC spCajas_ServiciosDatos
            @CodRecaudador,
            @CodServicio,
            @Monto,
            @CodCaja;";

            var data = conn.QueryFirstOrDefault<CajasCajaChicaServiciosDatosRsDto>(
                sql,
                new
                {
                    CodRecaudador = req.cod_recaudador,
                    CodServicio = req.cod_servicio,
                    Monto = req.monto,
                    CodCaja = req.cod_caja
                },
                transaction: tx);

            if (data == null)
                throw new InvalidOperationException("No se obtuvieron datos del servicio (spCajas_ServiciosDatos).");

            return data;
        }

        private static void InsertarSifTransacciones(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req,
            CajasCajaChicaServiciosDatosRsDto srv)
        {
            // líneas como VB (80 chars)
            var linea1 = Safe80($"{req.cod_recaudador} - {req.recaudador}".Trim());
            var linea2 = Safe80($"N.Ref        ..: {req.nref ?? ""}");
            var linea3 = Safe80($"Divisa       ..: {req.cod_divisa}");
            var linea4 = Safe80($"Concepto/Serv..: {req.cod_servicio} - {req.servicio}");

            const string sql = @"
        INSERT INTO SIF_TRANSACCIONES
        (
            COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
            Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
            Referencia_01, Referencia_02, Referencia_03, cod_oficina,
            linea1, linea2, linea3, linea4, detalle, documento, cod_caja, cod_apertura
        )
        VALUES
        (
            @CodTransaccion, @TipoDocumento, dbo.MyGetdate(), @Usuario,
            @Cedula, @Nombre, @CodConcepto, @Monto, 'P',
            @Ref01, @Ref02, @Ref03, @CodOficina,
            @Linea1, @Linea2, @Linea3, @Linea4, @Detalle, @DocumentoDeposito, @CodCaja, @CodApertura
        );";

            conn.Execute(sql, new
            {
                CodTransaccion = req.numdoc,
                TipoDocumento = req.tipo_documento,
                Usuario = req.usuario,
                Cedula = req.cedula,
                Nombre = Safe60(req.nombre),
                CodConcepto = srv.cod_concepto,
                Monto = req.monto,
                Ref01 = req.cod_recaudador,
                Ref02 = req.cod_servicio,
                Ref03 = Safe30(req.nref),
                CodOficina = req.cod_oficina,
                Linea1 = linea1,
                Linea2 = linea2,
                Linea3 = linea3,
                Linea4 = linea4,
                Detalle = req.detalle ?? "",
                DocumentoDeposito = req.documento_deposito,
                CodCaja = req.cod_caja,
                CodApertura = req.cod_apertura
            }, transaction: tx);
        }

        private static void InsertarCajasServiciosTransac(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req,
            CajasCajaChicaServiciosDatosRsDto srv)
        {
            // evita condición de carrera: lock de tabla/índice dentro de la transacción
            const string sqlLinea = @"
        SELECT ISNULL(MAX(Linea), 0) + 1
        FROM CAJAS_SERVICIOS_TRANSAC WITH (UPDLOCK, HOLDLOCK);";

            var linea = conn.ExecuteScalar<int>(sqlLinea, transaction: tx);

            const string sql = @"
        INSERT INTO CAJAS_SERVICIOS_TRANSAC
        (
            Linea, Cod_Caja, Cod_Apertura, Cod_Recaudador, Cod_Servicio,
            Tipo_Documento, Cod_Transaccion, num_referencia,
            monto, comision, impuesto, neto, cod_divisa, Tipo_Cambio
        )
        VALUES
        (
            @Linea, @CodCaja, @CodApertura, @CodRecaudador, @CodServicio,
            @TipoDocumento, @CodTransaccion, @NumReferencia,
            @MntBruto, @Comision, @Impuesto, @MntNeto, @CodDivisa, @TipoCambio
        );";

            conn.Execute(sql, new
            {
                Linea = linea,
                CodCaja = req.cod_caja,
                CodApertura = req.cod_apertura,
                CodRecaudador = req.cod_recaudador,
                CodServicio = req.cod_servicio,
                TipoDocumento = req.tipo_documento,
                CodTransaccion = req.numdoc,
                NumReferencia = Safe30(req.nref),
                MntBruto = srv.mnt_bruto,
                Comision = srv.comision,
                Impuesto = srv.impuesto,
                MntNeto = srv.mnt_neto,
                CodDivisa = req.cod_divisa,
                TipoCambio = req.tipo_cambio
            }, transaction: tx);
        }

        private static void RegistrarAsientoDebito(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req,
            CajasCajaChicaServiciosDatosRsDto srv)
        {
            const string sql = @"
        EXEC spSIFDocsAsiento
            @TipoDocumento,
            @NumDoc,
            @MontoAplicado,
            'D',
            @Divisa,
            @TipoCambioAsiento,
            @CodContabilidad,
            @CodUnidad,
            @CodCentroCosto,
            @Cuenta,
            @Ref01,
            @Ref02,
            @Ref03;";

            conn.Execute(sql, new
            {
                TipoDocumento = req.tipo_documento,
                NumDoc = req.numdoc,
                MontoAplicado = req.monto_aplicado,
                Divisa = req.cod_divisa,
                TipoCambioAsiento = 1, // en VB el débito manda 1
                CodContabilidad = req.cod_contabilidad,
                CodUnidad = srv.cod_unidad,
                CodCentroCosto = srv.cod_centro_costo,
                Cuenta = srv.cod_cuenta,
                Ref01 = req.cod_recaudador,
                Ref02 = req.cod_servicio,
                Ref03 = Safe30(req.nref)
            }, transaction: tx);
        }

        private static void RegistrarAsientoCredito(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req,
            CajasCajaChicaServiciosDatosRsDto srv)
        {
            const string sql = @"
        EXEC spSIFDocsAsiento
            @TipoDocumento,
            @NumDoc,
            @MontoAplicado,
            'C',
            @Divisa,
            @TipoCambioAsiento,
            @CodContabilidad,
            @CodUnidad,
            @CodCentroCosto,
            @Cuenta,
            @Ref01,
            @Ref02,
            @Ref03;";

            conn.Execute(sql, new
            {
                TipoDocumento = req.tipo_documento,
                NumDoc = req.numdoc,
                MontoAplicado = req.monto_aplicado,
                Divisa = req.cod_divisa,
                TipoCambioAsiento = req.tipo_cambio, // en VB el crédito manda pTipoCambio
                CodContabilidad = req.cod_contabilidad,
                CodUnidad = srv.cod_unidad,
                CodCentroCosto = srv.cod_centro_costo,
                Cuenta = srv.ef_cta,
                Ref01 = req.cod_recaudador,
                Ref02 = req.cod_servicio,
                Ref03 = Safe30(req.nref)
            }, transaction: tx);
        }

        private static void RegistrarFormaPagoEfectivo(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req,
            CajasCajaChicaServiciosDatosRsDto srv)
        {
            const string sql = @"
        EXEC spCajas_IntercambioRegistra
            @TipoDocumento,
            @NumDoc,
            @EfCodigo,
            @Monto,
            @EfCuenta,
            @CodUnidad,
            @Usuario,
            @Concepto;";

            conn.Execute(sql, new
            {
                TipoDocumento = req.tipo_documento,
                NumDoc = req.numdoc,
                EfCodigo = srv.ef_codigo,
                Monto = req.monto,
                EfCuenta = srv.ef_cta,
                CodUnidad = srv.cod_unidad,
                Usuario = req.usuario,
                Concepto = "Retiro en Cajas"
            }, transaction: tx);
        }

        private static string ValidarRetiro(CajasCajaChicaAplicarDbRequestDto req)
        {
            var mensajes = new List<string>();

            if (req.monto <= 0)
                mensajes.Add("- El Monto de la Transacción no es valido!");
            if (string.IsNullOrWhiteSpace(req.cod_caja))
                mensajes.Add("- No se ha indicado ninguna caja con Apertura disponible?");
            if (req.cod_apertura <= 0)
                mensajes.Add("- No se ha indicado ninguna caja con Apertura disponible?");
            if (string.IsNullOrWhiteSpace(req.tipo_documento))
                mensajes.Add("- Debe seleccionar el documento.");
            if (string.IsNullOrWhiteSpace(req.cod_divisa))
                mensajes.Add("- Debe seleccionar la divisa.");
            if (string.IsNullOrWhiteSpace(req.cod_recaudador) || string.IsNullOrWhiteSpace(req.cod_servicio))
                mensajes.Add("- Debe seleccionar el servicio.");
            if (req.cod_contabilidad <= 0)
                mensajes.Add("- No se ha indicado la contabilidad.");
            if (string.IsNullOrWhiteSpace(req.usuario))
                mensajes.Add("- No se ha indicado el usuario.");

            return string.Join(Environment.NewLine, mensajes.Distinct());
        }

        private static void ValidarAperturaAbierta(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req)
        {
            const string sql = @"
                SELECT Estado
                FROM cajas_aperturas_main
                WHERE cod_caja = @CodCaja
                  AND cod_apertura = @CodApertura;";

            var estado = conn.QueryFirstOrDefault<string>(
                sql,
                new
                {
                    CodCaja = req.cod_caja,
                    CodApertura = req.cod_apertura
                },
                transaction: tx);

            if (string.IsNullOrWhiteSpace(estado) || estado.Trim().Equals("C", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"- La apertura ..:{req.cod_apertura} de esta caja ha sido cerrada!");
        }

        private void PrepararDatosAplicacion(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req)
        {
            req.detalle = MProGrxMain.sbSIFCleanTxtInject(req.detalle);
            req.nombre = Safe60(req.nombre);
            req.nref = Safe30(req.nref);

            req.tipo_cambio = ObtenerTipoCambio(conn, tx, req);
            var factorTipoCambio = (decimal)MProGrxMain.fxSys_Tipo_Cambio_Apl(req.tipo_cambio);
            req.monto_aplicado = req.monto * factorTipoCambio;
            req.numdoc = _mRecibos.FxDocumentoConsecutivo(req.codempresa, req.tipo_documento).ToString();
        }

        private static decimal ObtenerTipoCambio(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req)
        {
            const string sql = @"
                SELECT dbo.fxCajas_TipoCambio(
                    @CodContabilidad,
                    @CodDivisa,
                    dbo.MyGetdate(),
                    'C') AS TipoCambio;";

            var tipoCambio = conn.QueryFirstOrDefault<decimal?>(
                sql,
                new
                {
                    CodContabilidad = req.cod_contabilidad,
                    CodDivisa = req.cod_divisa
                },
                transaction: tx);

            return tipoCambio ?? 1m;
        }

        private static void CompletarDescripcionesServicio(
            IDbConnection conn,
            IDbTransaction tx,
            CajasCajaChicaAplicarDbRequestDto req)
        {
            if (!string.IsNullOrWhiteSpace(req.recaudador) && !string.IsNullOrWhiteSpace(req.servicio))
                return;

            const string sql = @"
                SELECT
                    S.descripcion AS serviciodesc,
                    R.descripcion AS recaudadordesc
                FROM cajas_servicios S
                INNER JOIN cajas_recaudador R
                    ON S.cod_recaudador = R.cod_recaudador
                WHERE S.cod_recaudador = @CodRecaudador
                  AND S.cod_servicio = @CodServicio;";

            var servicio = conn.QueryFirstOrDefault<CajasCajaChicaServiciosDto>(
                sql,
                new
                {
                    CodRecaudador = req.cod_recaudador,
                    CodServicio = req.cod_servicio
                },
                transaction: tx);

            req.servicio = string.IsNullOrWhiteSpace(req.servicio)
                ? servicio?.serviciodesc ?? string.Empty
                : req.servicio;
            req.recaudador = string.IsNullOrWhiteSpace(req.recaudador)
                ? servicio?.recaudadordesc ?? string.Empty
                : req.recaudador;
        }
        // helpers de recorte (imitan Mid(...,1,N))
        private static string Safe30(string? s) => (s ?? "").Trim().Length <= 30 ? (s ?? "").Trim() : (s ?? "").Trim().Substring(0, 30);
        private static string Safe60(string? s) => (s ?? "").Trim().Length <= 60 ? (s ?? "").Trim() : (s ?? "").Trim().Substring(0, 60);
        private static string Safe80(string? s) => (s ?? "").Length <= 80 ? (s ?? "") : (s ?? "").Substring(0, 80);

    }
}
