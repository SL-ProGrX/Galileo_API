using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using System.Data;
using System.Linq;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.CargaArchivos
{
    public class CcProcesoMensualCargaArchivosDb
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;
        public CcProcesoMensualCargaArchivosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);

        }
        public ErrorDto<CcProcesoMensualCargaDeduccionesResponse> CargarDeduccionesGenerico(CcProcesoMensualCargaDeduccionesRequest request, IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> reglas)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, request.CodEmpresa);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var configuracion = ObtenerConfiguracionCarga(connection, transaction, request.CodInstitucion);

                EliminarCargaAnterior(connection, transaction, request);


                var registros = request.TipoCarga switch
                {
                    CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_ExcelNew =>
                        CrearRegistrosPrmCargadoDetallePorFila(
                            request,
                            configuracion,
                            usarCodigoObreroPatronal: true,
                            insertarSoloSiMontoMayorQueCero: true),

                    CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduc_Csv_Integra =>
                        CrearRegistrosPrmCargadoDetallePorFila(
                            request,
                            configuracion,
                            usarCodigoObreroPatronal: false,
                            insertarSoloSiMontoMayorQueCero: false),

                    CcProcesoMensualCargaDeduccionesTipo.sbCargaDeduccionesArchivoPlano =>
                           CrearRegistrosPrmCargadoDesdeFilasProcesadas(request),
                    _ =>
                        CrearRegistrosPrmCargado(
                            request,
                            reglas,
                            configuracion)
                };

                InsertarRegistrosPrmCargado(connection, transaction, registros);

                RevisarCedulasCargadas(connection, transaction, request);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "03",
                                                        CodInstitucion = request.CodInstitucion,
                                                        Proceso = request.FechaProceso,
                                                        Gestion = "R",
                                                        Usuario = request.Usuario,
                                                        Documento = "Pla.Num." + request.Pago
                                                    }, transaction);


                 _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = request.CodEmpresa,
                    Usuario = request.Usuario,
                    DetalleMovimiento = $"PRM-CREDITO Carga Deducciones Inst: {request.CodInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });

                MarcarInstitucionCargaRealizada(connection, transaction, request.CodInstitucion);

                transaction.Commit();

                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualCargaDeduccionesResponse
                    {
                        Cargado = true,
                        RegistrosProcesados = request.Filas.Count,
                        RegistrosInsertados = registros.Count,
                        Mensaje = "Información cargada correctamente."
                    });
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse<CcProcesoMensualCargaDeduccionesResponse>(
                    ex.Message,
                    -1,
                    new CcProcesoMensualCargaDeduccionesResponse());
            }
        }
        private static List<CcProcesoMensualPrmCargadoDbModel> CrearRegistrosPrmCargadoDesdeFilasProcesadas(CcProcesoMensualCargaDeduccionesRequest request)
        {
            var registros = new List<CcProcesoMensualPrmCargadoDbModel>();

            foreach (var fila in request.Filas)
            {
                if (string.IsNullOrWhiteSpace(fila.Cedula))
                {
                    continue;
                }

                if (fila.Monto <= 0)
                {
                    continue;
                }

                registros.Add(new CcProcesoMensualPrmCargadoDbModel
                {
                    CodInstitucion = request.CodInstitucion,
                    Pago = request.Pago,
                    FechaProceso = request.FechaProceso,
                    Tipo = fila.Tipo ?? 3,
                    Cedula = fila.Cedula.Trim(),
                    Monto = fila.Monto,
                    CodDeduccion = fila.Codigo.Trim(),
                    Up = fila.Up.Trim(),
                    Ut = fila.Ut.Trim()
                });
            }

            return registros;
        }
        private static CcProcesoMensualCargaConfigDbModel ObtenerConfiguracionCarga(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
               SELECT
                    ISNULL(I.planilla, '') AS Planilla,
                    ISNULL(I.codigo_aportes, '') AS CodigoAportes,
                    ISNULL(I.codigo_creditos, '') AS CodigoCreditos,
                    ISNULL(Ta.COD_DEDUCCION, I.codigo_aportes) AS CodigoObrero,
                    ISNULL(Tp.COD_DEDUCCION, 'x-PAT-x') AS CodigoPatronal
                FROM instituciones I
                LEFT JOIN vPrm_Codigos_Patrimonio Ta
                    ON I.cod_institucion = Ta.cod_institucion
                   AND Ta.Tipo = 'O'
                LEFT JOIN vPrm_Codigos_Patrimonio Tp
                    ON I.cod_institucion = Tp.cod_institucion
                   AND Tp.Tipo = 'P'
                WHERE I.cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualCargaConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion },
                transaction: transaction) ?? new CcProcesoMensualCargaConfigDbModel();
        }
        private static List<CcProcesoMensualPrmCargadoDbModel> CrearRegistrosPrmCargado(CcProcesoMensualCargaDeduccionesRequest request, IEnumerable<CcProcesoMensualReglaDeduccionConfig> reglas, CcProcesoMensualCargaConfigDbModel configuracion)
        {
            var registros = new List<CcProcesoMensualPrmCargadoDbModel>();

            foreach (var fila in request.Filas.Where(f => !string.IsNullOrWhiteSpace(f.Cedula)))
            {
                foreach (var regla in reglas)
                {
                    if (!DebeAplicarRegla(regla, fila, configuracion))
                    {
                        continue;
                    }

                    var monto = ObtenerMonto(fila, regla.ColumnasOrigen);

                    if (regla.InsertaSoloSiMontoMayorQueCero && monto <= 0)
                    {
                        continue;
                    }

                    registros.Add(new CcProcesoMensualPrmCargadoDbModel
                    {
                        CodInstitucion = request.CodInstitucion,
                        Pago = request.Pago,
                        FechaProceso = request.FechaProceso,
                        Tipo = regla.Tipo,
                        Cedula = fila.Cedula.Trim(),
                        Monto = monto,
                        CodDeduccion = regla.CodDeduccion
                    });
                }
            }

            return registros;
        }
        private static bool DebeAplicarRegla(CcProcesoMensualReglaDeduccionConfig regla, CcProcesoMensualCargaDeduccionFilaRequest fila, CcProcesoMensualCargaConfigDbModel configuracion)
        {
            if (regla.RequiereAportesHabilitados && EsCodigoNo(configuracion.CodigoAportes))
            {
                return false;
            }

            if (regla.RequiereCreditosHabilitados && EsCodigoNo(configuracion.CodigoCreditos))
            {
                return false;
            }

            if (regla.RequiereColumnaExistente && !TieneTodasLasColumnas(fila, regla.ColumnasOrigen))
            {
                return false;
            }

            return true;
        }
        private static List<CcProcesoMensualPrmCargadoDbModel> CrearRegistrosPrmCargadoDetallePorFila(
           CcProcesoMensualCargaDeduccionesRequest request,
           CcProcesoMensualCargaConfigDbModel configuracion,
           bool usarCodigoObreroPatronal,
           bool insertarSoloSiMontoMayorQueCero)
        {
            var registros = new List<CcProcesoMensualPrmCargadoDbModel>();

            foreach (var fila in request.Filas)
            {
                if (string.IsNullOrWhiteSpace(fila.Cedula))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fila.Codigo))
                {
                    continue;
                }

                if (EsEncabezadoCodigoDeduccion(fila.Codigo))
                {
                    continue;
                }

                if (insertarSoloSiMontoMayorQueCero && fila.Monto <= 0)
                {
                    continue;
                }

                var codigo = fila.Codigo.Trim();

                registros.Add(new CcProcesoMensualPrmCargadoDbModel
                {
                    CodInstitucion = request.CodInstitucion,
                    Pago = request.Pago,
                    FechaProceso = request.FechaProceso,
                    Tipo = ObtenerTipoDetallePorFila(
                        codigo,
                        configuracion,
                        usarCodigoObreroPatronal),
                    Cedula = fila.Cedula.Trim(),
                    Monto = fila.Monto,
                    CodDeduccion = codigo
                });
            }

            return registros;
        }

        private static bool EsEncabezadoCodigoDeduccion(string codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                "codigodeduccion",
                StringComparison.OrdinalIgnoreCase);
        }

        private static int ObtenerTipoDetallePorFila(string codigo, CcProcesoMensualCargaConfigDbModel configuracion, bool usarCodigoObreroPatronal)
        {
            if (usarCodigoObreroPatronal)
            {
                return EsCodigoAporteOPatronal(codigo, configuracion) ? 1 : 3;
            }

            return string.Equals(
                codigo,
                configuracion.CodigoAportes?.Trim(),
                StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 3;
        }

        private static bool EsCodigoAporteOPatronal(string codigo, CcProcesoMensualCargaConfigDbModel configuracion)
        {
            return string.Equals(
                    codigo,
                    configuracion.CodigoObrero?.Trim(),
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    codigo,
                    configuracion.CodigoPatronal?.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool TieneTodasLasColumnas(CcProcesoMensualCargaDeduccionFilaRequest fila, IEnumerable<string> columnasOrigen)
        {
            return columnasOrigen.All(fila.Montos.ContainsKey);
        }
        private static decimal ObtenerMonto(CcProcesoMensualCargaDeduccionFilaRequest fila, IEnumerable<string> columnasOrigen)
        { 
            return columnasOrigen
                .Where(fila.Montos.ContainsKey)
                .Sum(columna => fila.Montos[columna]);
              
        }
        private static bool EsCodigoNo(string? codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                "NO",
                StringComparison.OrdinalIgnoreCase);
        }

        private static void EliminarCargaAnterior(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCargaDeduccionesRequest request)
        {
            const string query = @"
            DELETE prm_cargado
            WHERE fecha_proceso = @FechaProceso
              AND pago = @Pago
              AND cod_institucion = @CodInstitucion";

            connection.Execute(
                query,
                new
                {
                    request.FechaProceso,
                    request.Pago,
                    request.CodInstitucion
                },
                transaction: transaction);
        }

        private static void InsertarRegistrosPrmCargado(IDbConnection connection, IDbTransaction transaction, IReadOnlyCollection<CcProcesoMensualPrmCargadoDbModel> registros)
        {
            if (registros.Count == 0)
            {
                return;
            }

            const string query = @"
                    INSERT INTO prm_cargado(
                        cod_institucion,
                        pago,
                        fecha_proceso,
                        tipo,
                        cedula,
                        monto,
                        cod_deduccion,
                        UP,
                        UT)
                    VALUES(
                        @CodInstitucion,
                        @Pago,
                        @FechaProceso,
                        @Tipo,
                        @Cedula,
                        @Monto,
                        @CodDeduccion,
                        @Up,
                        @Ut)";

            foreach (var lote in registros.Chunk(500))
            {
                connection.Execute(
                    query,
                    lote,
                    transaction: transaction);
            }
        }

        private static void RevisarCedulasCargadas(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCargaDeduccionesRequest request)
        {
            const string query = @"
            EXEC spPrmCargado_Revision_Cedulas
                @CodInstitucion,
                @FechaProceso,
                @Pago";

            connection.Execute(
                query,
                new
                {
                    request.CodInstitucion,
                    request.FechaProceso,
                    request.Pago
                },
                transaction: transaction);
        }

        private static void MarcarInstitucionCargaRealizada(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
            UPDATE instituciones
            SET pr_carga = 1
            WHERE cod_institucion = @CodInstitucion";

            connection.Execute(
                query,
                new { CodInstitucion = codInstitucion },
                transaction: transaction);
        }
    }

    internal sealed class CcProcesoMensualPrmCargadoDbModel
    {
        public int CodInstitucion { get; set; }
        public int Pago { get; set; }
        public decimal FechaProceso { get; set; }
        public int Tipo { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string CodDeduccion { get; set; } = string.Empty;
        public string Up { get; set; } = string.Empty;
        public string Ut { get; set; } = string.Empty;

    }
}
