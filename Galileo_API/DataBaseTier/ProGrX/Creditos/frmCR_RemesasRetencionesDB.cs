using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrRemesasRetencionesDb
    {
        private const int Modulo = 3;
        private const int IdComiteRemesa = 6;
        private const int LargoNombreSocio = 30;
        private const string ObservacionRemesa = "PROCESO AUTOMATICO : REMESAS";

        private const int SinInconsistencia = 0;
        private const int CodigoNoExiste = 1;
        private const int CodigoNoRetencion = 2;
        private const int OperacionEnCobro = 3;

        private const string SqlCatalogoObtener = @"
            select top 1
                isnull(rtrim(retencion), '') as retencion
            from Catalogo
            where codigo = @Codigo";

        private const string SqlOperacionActivaExiste = @"
            select coalesce(count(*), 0)
            from reg_creditos
            where estado = 'A'
              and codigo = @Codigo
              and cedula = @Cedula";

        private const string SqlSocioExiste = @"
            select coalesce(count(*), 0)
            from socios
            where cedula = @Cedula";

        private const string SqlSocioInsertar = @"
            insert socios(id_promotor, cedula, nombre, estadoactual, fechaingreso)
            values(1, @Cedula, @Nombre, 'N', @Fecha)";

        private const string SqlAhorroConsolidadoInsertar = @"
            insert ahorro_consolidado(cedula, ahorro, aporte)
            values(@Cedula, 0, 0)";

        private const string SqlCreditoInsertar = @"
            insert reg_creditos(
                codigo, id_comite, cedula, montosol, montoapr, monto_girado,
                saldo, amortiza, interesc, saldo_mes, cuota, [int], interesv, plazo,
                userrec, userres, userfor, usertesoreria, tesoreria, fechasol, fechares,
                fechaforp, fechaforf, fecha_calculo_int, garantia, primer_cuota,
                tdocumento, ndocumento, pagare, firma_deudor, premio, observacion,
                estado, prideduc, fecult, estadosol)
            values(
                @Codigo, @IdComite, @Cedula, @Cuota, @Cuota, 0,
                @Cuota, 0, 0, @Cuota, @Cuota, 0, 0, @Plazo,
                @Usuario, @Usuario, @Usuario, @Usuario, @Fecha, @Fecha, @Fecha,
                @Fecha, @Fecha, @Fecha, 'N', 'N',
                'OT', '', 0, 1, 0, @Observacion,
                'A', @PriDeduc, @FecUlt, 'F')";

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MProGrXSecurityMainDb _mProGrXSecurityMainDb;

        public FrmCrRemesasRetencionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mProGrXSecurityMainDb = new MProGrXSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los valores iniciales de la pantalla: primer deducción (anio y mes)
        /// calculada a partir de la fecha de proceso de crédito (VB6: sbLimpiaDatos / fxPrimerDeduccion).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario de sesión.</param>
        /// <returns>Año, mes de primer deducción y fecha de proceso.</returns>
        public ErrorDto<CrRemesasRetencionesPantallaData> Cr_RemesasRetenciones_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
        {
            var fechaProceso = Cr_RemesasRetenciones_FechaProceso_Obtener(codEmpresa, usuario);
            if (fechaProceso.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    fechaProceso.Description ?? "No fue posible obtener la fecha de proceso.",
                    -1,
                    new CrRemesasRetencionesPantallaData());
            }

            var proceso = fechaProceso.Result;
            var anio = (int)(proceso / 100);
            var mes = (int)(proceso % 100);

            if (mes == 12)
            {
                mes = 1;
                anio++;
            }
            else
            {
                mes++;
            }

            return DbHelper.CreateOkResponse(new CrRemesasRetencionesPantallaData
            {
                anio = anio,
                mes = mes,
                fecha_proceso = proceso
            });
        }

        /// <summary>
        /// Valida las líneas leídas del archivo de remesa contra el catálogo y las operaciones activas
        /// (VB6: cmdBuscar_Click).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Líneas del archivo.</param>
        /// <returns>Detalle con inconsistencias y totales a aplicar / rechazar.</returns>
        public ErrorDto<CrRemesasRetencionesValidarData> Cr_RemesasRetenciones_Validar(
            int codEmpresa,
            CrRemesasRetencionesValidarRequest request)
        {
            var lineas = Cr_RemesasRetenciones_Lineas_Normalizar(request?.lineas);
            if (lineas.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El archivo no contiene líneas para procesar.",
                    -2,
                    new CrRemesasRetencionesValidarData());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                var detalle = Cr_RemesasRetenciones_Detalle_Validar(conn, null, lineas);

                return DbHelper.CreateOkResponse(new CrRemesasRetencionesValidarData
                {
                    detalle = detalle,
                    totales = Cr_RemesasRetenciones_Totales_Calcular(detalle)
                });
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al validar la información del archivo.",
                    -1,
                    new CrRemesasRetencionesValidarData());
            }
        }

        /// <summary>
        /// Aplica la remesa: registra como no socio a quien no exista y crea la operación de retención
        /// para cada línea sin inconsistencias (VB6: cmdAplicar_Click).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que aplica.</param>
        /// <param name="request">Primer deducción y líneas del archivo.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto Cr_RemesasRetenciones_Aplicar(
            int codEmpresa,
            string usuario,
            CrRemesasRetencionesAplicarRequest request)
        {
            usuario = (usuario ?? string.Empty).Trim();

            var validacion = Cr_RemesasRetenciones_Aplicar_Validar(usuario, request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            var fechaProceso = Cr_RemesasRetenciones_FechaProceso_Obtener(codEmpresa, usuario);
            if (fechaProceso.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    fechaProceso.Description ?? "No fue posible obtener la fecha de proceso.");
            }

            var primeraDeduccion = request.anio * 100L + request.mes;
            if (fechaProceso.Result > primeraDeduccion)
            {
                return DbHelper.ErrorResponse(
                    "La fecha de la primer deducción es menor a la fecha de proceso actual...",
                    -2);
            }

            var lineas = Cr_RemesasRetenciones_Lineas_Normalizar(request.lineas);
            if (lineas.Count == 0)
            {
                return DbHelper.ErrorResponse("No existen casos para procesar.", -2);
            }

            List<CrRemesasRetencionesLineaData> aplicadas;

            try
            {
                var contexto = new CrRemesasRetencionesAplicarContexto
                {
                    Usuario = usuario,
                    Fecha = _mProGrxMain.fxFechaServidor(codEmpresa, 0).Date,
                    PriDeduc = primeraDeduccion,
                    FecUlt = fechaProceso.Result
                };

                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                using var tx = conn.BeginTransaction();

                aplicadas = Cr_RemesasRetenciones_Detalle_Validar(conn, tx, lineas)
                    .Where(x => x.inconsistencia == SinInconsistencia)
                    .ToList();

                foreach (var linea in aplicadas)
                {
                    Cr_RemesasRetenciones_Linea_Aplicar(conn, tx, linea, contexto);
                }

                tx.Commit();
            }
            catch (Exception)
            {
                return DbHelper.ErrorResponse(
                    "Error al aplicar la remesa de retenciones. No se registró ningún caso.");
            }

            Cr_RemesasRetenciones_Bitacora_Registrar(codEmpresa, usuario, aplicadas);

            return DbHelper.OkResponse(
                $"Casos Procesados Satisfactoriamente... ({aplicadas.Count})");
        }

        /// <summary>
        /// Obtiene la fecha de proceso de crédito (GLOBALES.glngFechaCR) desde el modelo Globales.
        /// </summary>
        private ErrorDto<long> Cr_RemesasRetenciones_FechaProceso_Obtener(int codEmpresa, string usuario)
        {
            var usuarioNormalizado = (usuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, 0L);
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuarioNormalizado);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parámetros globales.",
                    -1,
                    0L);
            }

            return DbHelper.CreateOkResponse(Convert.ToInt64(globalesResp.Result.GlngFechaCR));
        }

        /// <summary>
        /// Valida usuario, primer deducción y existencia de líneas antes de aplicar.
        /// </summary>
        private static ErrorDto Cr_RemesasRetenciones_Aplicar_Validar(
            string usuario,
            CrRemesasRetencionesAplicarRequest? request)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);
            }

            if (request?.lineas is null || request.lineas.Count == 0)
            {
                return DbHelper.ErrorResponse("No existen casos para procesar.", -2);
            }

            if (request.anio < 1900 || request.anio > 9999 || request.mes < 1 || request.mes > 12)
            {
                return DbHelper.ErrorResponse("Indique un año y mes válidos para la primer deducción.", -2);
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Descarta líneas sin código (VB6: If Not IsNull(!Codigo)) y normaliza los textos.
        /// </summary>
        private static List<CrRemesasRetencionesLineaRequest> Cr_RemesasRetenciones_Lineas_Normalizar(
            List<CrRemesasRetencionesLineaRequest>? lineas)
        {
            return (lineas ?? new List<CrRemesasRetencionesLineaRequest>())
                .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.codigo))
                .Select(x => new CrRemesasRetencionesLineaRequest
                {
                    codigo = x.codigo.Trim(),
                    plazo = x.plazo,
                    cuota = x.cuota,
                    cedula = (x.cedula ?? string.Empty).Trim(),
                    nombre = (x.nombre ?? string.Empty).Trim()
                })
                .ToList();
        }

        /// <summary>
        /// Clasifica cada línea según su inconsistencia.
        /// </summary>
        private static List<CrRemesasRetencionesLineaData> Cr_RemesasRetenciones_Detalle_Validar(
            IDbConnection conn,
            IDbTransaction? tx,
            List<CrRemesasRetencionesLineaRequest> lineas)
        {
            var catalogo = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var detalle = new List<CrRemesasRetencionesLineaData>(lineas.Count);

            foreach (var linea in lineas)
            {
                var inconsistencia = Cr_RemesasRetenciones_Inconsistencia_Obtener(conn, tx, linea, catalogo);

                detalle.Add(new CrRemesasRetencionesLineaData
                {
                    codigo = linea.codigo,
                    plazo = linea.plazo,
                    cuota = linea.cuota,
                    cedula = linea.cedula,
                    nombre = linea.nombre,
                    inconsistencia = inconsistencia,
                    detalle_inc = Cr_RemesasRetenciones_DetalleInc_Obtener(inconsistencia)
                });
            }

            return detalle;
        }

        /// <summary>
        /// Determina la inconsistencia de una línea: 1 código inexistente, 2 no es retención,
        /// 3 operación activa existente, 0 sin inconsistencia.
        /// </summary>
        private static int Cr_RemesasRetenciones_Inconsistencia_Obtener(
            IDbConnection conn,
            IDbTransaction? tx,
            CrRemesasRetencionesLineaRequest linea,
            Dictionary<string, string?> catalogo)
        {
            if (!catalogo.TryGetValue(linea.codigo, out var retencion))
            {
                retencion = conn.QueryFirstOrDefault<string?>(
                    SqlCatalogoObtener,
                    new { Codigo = linea.codigo },
                    tx);
                catalogo[linea.codigo] = retencion;
            }

            if (retencion is null)
            {
                return CodigoNoExiste;
            }

            if (retencion == "N")
            {
                return CodigoNoRetencion;
            }

            var existe = conn.ExecuteScalar<int>(
                SqlOperacionActivaExiste,
                new { Codigo = linea.codigo, Cedula = linea.cedula },
                tx);

            return existe > 0 ? OperacionEnCobro : SinInconsistencia;
        }

        private static string Cr_RemesasRetenciones_DetalleInc_Obtener(int inconsistencia)
        {
            return inconsistencia switch
            {
                CodigoNoExiste => "EL CODIGO NO EXISTE EN EL CATALOGO",
                CodigoNoRetencion => "EL CODIGO NO ES UNA RETENCION",
                OperacionEnCobro => "YA EXISTE UNA OPERACION EN COBRO",
                _ => "NO HAY INCONSISTENCIA"
            };
        }

        private static CrRemesasRetencionesTotalesData Cr_RemesasRetenciones_Totales_Calcular(
            List<CrRemesasRetencionesLineaData> detalle)
        {
            var aplicar = detalle.Where(x => x.inconsistencia == SinInconsistencia).ToList();
            var rechazar = detalle.Where(x => x.inconsistencia != SinInconsistencia).ToList();

            return new CrRemesasRetencionesTotalesData
            {
                casos_apl = aplicar.Count,
                cuota_apl = aplicar.Sum(x => x.cuota),
                saldo_apl = aplicar.Sum(x => x.cuota * x.plazo),
                casos_inc = rechazar.Count,
                cuota_inc = rechazar.Sum(x => x.cuota),
                saldo_inc = rechazar.Sum(x => x.cuota * x.plazo)
            };
        }

        /// <summary>
        /// Registra como no socio a la persona si no existe y crea la operación de retención.
        /// </summary>
        private static void Cr_RemesasRetenciones_Linea_Aplicar(
            IDbConnection conn,
            IDbTransaction tx,
            CrRemesasRetencionesLineaData linea,
            CrRemesasRetencionesAplicarContexto contexto)
        {
            Cr_RemesasRetenciones_Socio_Asegurar(conn, tx, linea, contexto.Fecha);

            conn.Execute(
                SqlCreditoInsertar,
                new
                {
                    Codigo = linea.codigo,
                    IdComite = IdComiteRemesa,
                    Cedula = linea.cedula,
                    Cuota = linea.cuota,
                    Plazo = linea.plazo,
                    Usuario = contexto.Usuario,
                    Fecha = contexto.Fecha,
                    Observacion = ObservacionRemesa,
                    PriDeduc = contexto.PriDeduc,
                    FecUlt = contexto.FecUlt
                },
                tx);
        }

        private static void Cr_RemesasRetenciones_Socio_Asegurar(
            IDbConnection conn,
            IDbTransaction tx,
            CrRemesasRetencionesLineaData linea,
            DateTime fecha)
        {
            var existe = conn.ExecuteScalar<int>(SqlSocioExiste, new { Cedula = linea.cedula }, tx);
            if (existe > 0)
            {
                return;
            }

            var nombre = linea.nombre.ToUpperInvariant();
            if (nombre.Length > LargoNombreSocio)
            {
                nombre = nombre[..LargoNombreSocio];
            }

            conn.Execute(SqlSocioInsertar, new { Cedula = linea.cedula, Nombre = nombre, Fecha = fecha }, tx);
            conn.Execute(SqlAhorroConsolidadoInsertar, new { Cedula = linea.cedula }, tx);
        }

        private void Cr_RemesasRetenciones_Bitacora_Registrar(
            int codEmpresa,
            string usuario,
            List<CrRemesasRetencionesLineaData> aplicadas)
        {
            foreach (var linea in aplicadas)
            {
                var cuota = linea.cuota.ToString(CultureInfo.InvariantCulture);

                _mProGrXSecurityMainDb.Bitacora(new MProGrXSecurityMainBitacora
                {
                    CodEmpresa = codEmpresa,
                    usuario = usuario.ToUpperInvariant(),
                    strTipoMovimiento = "Registra",
                    strDetalleMovimiento = $"REMESA : COD.{linea.codigo} CED.{linea.cedula} CTA: {cuota}",
                    vModulo = Modulo
                });
            }
        }

        private sealed class CrRemesasRetencionesAplicarContexto
        {
            public string Usuario { get; init; } = string.Empty;
            public DateTime Fecha { get; init; }
            public long PriDeduc { get; init; }
            public long FecUlt { get; init; }
        }
    }
}
