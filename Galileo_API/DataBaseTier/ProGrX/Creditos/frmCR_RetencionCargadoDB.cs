using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrRetencionCargadoDb
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MCobroDb _mCobroDb;
        private readonly MSeguimientoDB _mSeguimientoDb;

        public FrmCrRetencionCargadoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mCobroDb = new MCobroDb(config);
            _mSeguimientoDb = new MSeguimientoDB(config);
        }

        /// <summary>
        /// Obtiene la informacion inicial de la pantalla.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionCargadoPantallaData> CrRetencionCargado_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
        {
            usuario = NormalizarTexto(usuario);

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new CrRetencionCargadoPantallaData());
            }

            var globalesResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
            if (globalesResp.Code != 0 || globalesResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    globalesResp.Description ?? "No fue posible obtener los parametros globales.",
                    globalesResp.Code.GetValueOrDefault(-1),
                    new CrRetencionCargadoPantallaData());
            }

            var clientesResp = CrRetencionCargado_Clientes_Obtener(codEmpresa);
            if (clientesResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    clientesResp.Description ?? "No fue posible obtener los clientes.",
                    clientesResp.Code.GetValueOrDefault(-1),
                    new CrRetencionCargadoPantallaData());
            }

            var institucionesResp = CrRetencionCargado_Instituciones_Obtener(codEmpresa);
            if (institucionesResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    institucionesResp.Description ?? "No fue posible obtener las instituciones.",
                    institucionesResp.Code.GetValueOrDefault(-1),
                    new CrRetencionCargadoPantallaData());
            }

            var procesoBase = Convert.ToInt64(globalesResp.Result.GlngFechaCR);
            var procesos = CrRetencionCargado_Procesos_Construir(codEmpresa, procesoBase);

            return DbHelper.CreateOkResponse(new CrRetencionCargadoPantallaData
            {
                clientes = clientesResp.Result ?? new List<DropDownListaGenericaModel>(),
                instituciones = institucionesResp.Result ?? new List<DropDownListaGenericaModel>(),
                tipos_deduccion = new List<DropDownListaGenericaModel>
                {
                    new() { item = "I", descripcion = "Indefinida" },
                    new() { item = "P", descripcion = "A Plazo" }
                },
                procesos = procesos,
                proceso_default = procesos.FirstOrDefault()?.item?.ToString() ?? string.Empty,
                tipo_deduccion_default = "I",
                archivo_excel_default = true,
                revisar_institucion_default = true
            });
        }

        /// <summary>
        /// Obtiene las deductoras segun la institucion seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrRetencionCargado_Deductoras_Obtener(
            int codEmpresa,
            int codInstitucion)
        {
            if (codInstitucion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la institucion.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = @"
                select
                    cast(COD_DEDUCTORA as varchar(20)) as item,
                    rtrim(DESCRIPCION) as descripcion
                from vAFI_Deductoras
                where cod_institucion = @CodInstitucion
                order by DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodInstitucion = codInstitucion });
        }

        /// <summary>
        /// Obtiene la frecuencia y primera deduccion para la deductora seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codDeductora"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionCargadoDeductoraDetalleData> CrRetencionCargado_DeductoraDetalle_Obtener(
            int codEmpresa,
            int codDeductora)
        {
            if (codDeductora <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la deductora.",
                    -2,
                    new CrRetencionCargadoDeductoraDetalleData());
            }

            const string sql = @"
                select
                    isnull(Frecuencia, 'M') as frecuencia_id
                from instituciones
                where cod_institucion = @CodDeductora;";

            var frecuenciaResp = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                "M",
                new { CodDeductora = codDeductora });

            if (frecuenciaResp.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    frecuenciaResp.Description ?? "No fue posible obtener la frecuencia de la deductora.",
                    frecuenciaResp.Code.GetValueOrDefault(-1),
                    new CrRetencionCargadoDeductoraDetalleData());
            }

            string frecuenciaId = NormalizarTexto(frecuenciaResp.Result ?? "M");
            decimal primerDeduccion = _mSeguimientoDb.fxPrimerDeduccion(
                codEmpresa,
                pDeductora: codDeductora);

            return DbHelper.CreateOkResponse(new CrRetencionCargadoDeductoraDetalleData
            {
                frecuencias = CrRetencionCargado_Frecuencias_Construir(frecuenciaId),
                frecuencia_id = CrRetencionCargado_FrecuenciaSeleccionada_Resolver(primerDeduccion, frecuenciaId),
                frecuencia_descripcion = frecuenciaId == "Q" ? "1er Quincena" : "Mensual",
                primer_deduccion = Convert.ToInt64(Math.Truncate(primerDeduccion)).ToString(CultureInfo.InvariantCulture)
            });
        }

        /// <summary>
        /// Carga el detalle temporal del archivo y ejecuta la revision del proceso.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrRetencionCargadoCargaData> CrRetencionCargado_Cargar(
            int codEmpresa,
            string usuario,
            CrRetencionCargadoCargaRequest request)
        {
            usuario = NormalizarTexto(usuario);
            request.codigo = NormalizarTexto(request.codigo);
            request.tipo_deduccion = NormalizarTexto(request.tipo_deduccion);

            var validacion = CrRetencionCargado_Carga_Validar(request, usuario);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "Solicitud invalida.",
                    validacion.Code.GetValueOrDefault(-2),
                    new CrRetencionCargadoCargaData());
            }

            var fechaServidorResp = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
            if (fechaServidorResp.Code != 0 || fechaServidorResp.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    fechaServidorResp.Description ?? "No fue posible obtener la fecha servidor.",
                    fechaServidorResp.Code.GetValueOrDefault(-1),
                    new CrRetencionCargadoCargaData());
            }

            DateTime fechaServidor = fechaServidorResp.Result.fxFechaServidor ?? DateTime.Now;
            long proceso = Convert.ToInt64(request.proceso, CultureInfo.InvariantCulture);

            List<CrRetencionCargadoCargaLineaDb> lineas = new();
            CrRetencionCargadoTotalesData totales = new();

            int numeroLinea = 0;

            foreach (var item in request.items.Where(x => !string.IsNullOrWhiteSpace(x.cedula)))
            {
                numeroLinea++;

                var linea = CrRetencionCargado_Linea_Construir(
                    item,
                    request,
                    fechaServidor,
                    numeroLinea);

                totales.monto += linea.monto;

                switch (linea.movimiento_id)
                {
                    case "I":
                        totales.inclusion++;
                        break;
                    case "E":
                        totales.exclusion++;
                        break;
                    case "C":
                        totales.cambio++;
                        break;
                    default:
                        totales.errores++;
                        break;
                }

                if (linea.movimiento_id != "X")
                {
                    lineas.Add(linea);
                }
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                const string sqlDelete = @"
                    delete CRD_RETENCION_CARGADO_H
                    where codigo = @Codigo
                      and proceso = @Proceso
                      and cod_institucion = @CodInstitucion;";

                conn.Execute(sqlDelete, new
                {
                    Codigo = request.codigo,
                    Proceso = proceso,
                    CodInstitucion = request.cod_institucion
                }, tx);

                if (lineas.Count > 0)
                {
                    const string sqlInsert = @"
                        insert into CRD_RETENCION_CARGADO_H
                        (
                            LINEA,
                            CODIGO,
                            COD_INSTITUCION,
                            COD_DEDUCTORA,
                            PROCESO,
                            CEDULA,
                            MONTO,
                            NOMBRE,
                            MOVIMIENTO,
                            TIPO,
                            EXISTE_INST,
                            PLAZO,
                            CUOTA,
                            OPERACION,
                            FORMALIZA
                        )
                        values
                        (
                            @linea,
                            @codigo,
                            @cod_institucion,
                            @cod_deductora,
                            @proceso,
                            @cedula,
                            @monto,
                            @nombre,
                            @movimiento_id,
                            'I',
                            null,
                            @plazo,
                            @cuota,
                            @operacion,
                            @formaliza
                        );";

                    conn.Execute(sqlInsert, lineas, tx);
                }

                const string sqlRevisado = @"
                    exec spCrd_Retenciones_Cargado_Revisado
                        @Codigo,
                        @CodInstitucion,
                        @Proceso;";

                var detalle = conn.Query<CrRetencionCargadoDetalleData>(
                    sqlRevisado,
                    new
                    {
                        Codigo = request.codigo,
                        CodInstitucion = request.cod_institucion,
                        Proceso = proceso
                    },
                    tx).ToList();

                tx.Commit();

                totales.casos = detalle.Count;

                return DbHelper.CreateOkResponse(new CrRetencionCargadoCargaData
                {
                    detalle = detalle,
                    totales = totales
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrRetencionCargadoCargaData());
            }
        }

        /// <summary>
        /// Aplica el cargado temporal de retenciones.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrRetencionCargado_Aplicar(
            int codEmpresa,
            string usuario,
            CrRetencionCargadoAplicarRequest request)
        {
            usuario = NormalizarTexto(usuario);
            request.codigo = NormalizarTexto(request.codigo);

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar el cliente.", -2);
            }

            if (request.cod_institucion <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la institucion.", -2);
            }

            if (!long.TryParse(request.proceso, NumberStyles.Any, CultureInfo.InvariantCulture, out var proceso) || proceso <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un proceso valido.", -2);
            }

            if (request.detalle is null || request.detalle.Count == 0)
            {
                return DbHelper.ErrorResponse("No existen deducciones cargadas...[verifique!]", -2);
            }

            decimal priDeduc = CrRetencionCargado_PriDeduc_Construir(
                request.proceso,
                request.frecuencia_id);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var tx = conn.BeginTransaction();

                var cambios = request.detalle
                    .Where(x => NormalizarTexto(x.existe_inst) == "CAMBIAR" && !string.IsNullOrWhiteSpace(x.cedula))
                    .ToList();

                if (cambios.Count > 0)
                {
                    const string sqlUpdate = @"
                        update CRD_RETENCION_CARGADO_H
                           set EXISTE_INST = 'Cambiar'
                         where codigo = @Codigo
                           and cod_institucion = @CodInstitucion
                           and proceso = @Proceso
                           and cedula = @Cedula;";

                    conn.Execute(
                        sqlUpdate,
                        cambios.Select(x => new
                        {
                            Codigo = request.codigo,
                            CodInstitucion = request.cod_institucion,
                            Proceso = proceso,
                            Cedula = NormalizarTexto(x.cedula)
                        }),
                        tx);
                }

                const string sqlProcesa = @"
                    exec spCrd_Retenciones_Cargado_Procesa
                        @Codigo,
                        @CodInstitucion,
                        @Proceso,
                        @Usuario,
                        @PriDeduc;";

                conn.Execute(sqlProcesa, new
                {
                    Codigo = request.codigo,
                    CodInstitucion = request.cod_institucion,
                    Proceso = proceso,
                    Usuario = usuario,
                    PriDeduc = priDeduc
                }, tx);

                tx.Commit();

                return DbHelper.OkResponse("Cargado y Actualizacion de Retenciones aplicadas satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private ErrorDto<List<DropDownListaGenericaModel>> CrRetencionCargado_Clientes_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(codigo) as item,
                    rtrim(descripcion) + '  [' + rtrim(codigo) + ']' as descripcion
                from catalogo
                where retencion = 'S'
                  and activo = 1
                  and codigo not in (select codigo_ase from fnd_planes)
                order by codigo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        private ErrorDto<List<DropDownListaGenericaModel>> CrRetencionCargado_Instituciones_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    cast(cod_institucion as varchar(20)) as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                where activa = 1
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        private List<DropDownListaGenericaModel> CrRetencionCargado_Procesos_Construir(
            int codEmpresa,
            long procesoBase)
        {
            List<DropDownListaGenericaModel> procesos = new();

            if (procesoBase <= 0)
            {
                return procesos;
            }

            decimal procesoActual = procesoBase;

            procesos.Add(new DropDownListaGenericaModel
            {
                item = procesoBase.ToString(CultureInfo.InvariantCulture),
                descripcion = procesoBase.ToString(CultureInfo.InvariantCulture)
            });

            for (int i = 1; i <= 6; i++)
            {
                procesoActual = _mCobroDb.fxFechaProcesoSiguiente(codEmpresa, procesoActual);
                long procesoLong = Convert.ToInt64(Math.Truncate(procesoActual));

                procesos.Add(new DropDownListaGenericaModel
                {
                    item = procesoLong.ToString(CultureInfo.InvariantCulture),
                    descripcion = procesoLong.ToString(CultureInfo.InvariantCulture)
                });
            }

            return procesos;
        }

        private static List<DropDownListaGenericaModel> CrRetencionCargado_Frecuencias_Construir(string frecuenciaId)
        {
            return frecuenciaId == "Q"
                ? new List<DropDownListaGenericaModel>
                {
                    new() { item = "1", descripcion = "1er Quincena" },
                    new() { item = "2", descripcion = "2da Quincena" }
                }
                : new List<DropDownListaGenericaModel>
                {
                    new() { item = "0", descripcion = "Mensual" }
                };
        }

        private static string CrRetencionCargado_FrecuenciaSeleccionada_Resolver(decimal primerDeduccion, string frecuenciaId)
        {
            if (frecuenciaId != "Q")
            {
                return "0";
            }

            decimal parteDecimal = primerDeduccion - Math.Truncate(primerDeduccion);
            return parteDecimal == 0.2m ? "2" : "1";
        }

        private static ErrorDto CrRetencionCargado_Carga_Validar(
            CrRetencionCargadoCargaRequest request,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse("Debe indicar el usuario.", -2);
            }

            if (string.IsNullOrWhiteSpace(request.codigo))
            {
                return DbHelper.ErrorResponse("Debe indicar el cliente.", -2);
            }

            if (request.cod_institucion <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la institucion.", -2);
            }

            if (request.cod_deductora <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar la deductora.", -2);
            }

            if (!long.TryParse(request.proceso, NumberStyles.Any, CultureInfo.InvariantCulture, out var proceso) || proceso <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar un proceso valido.", -2);
            }

            if (request.items is null || request.items.Count == 0)
            {
                return DbHelper.ErrorResponse("No se recibieron lineas para procesar.", -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static CrRetencionCargadoCargaLineaDb CrRetencionCargado_Linea_Construir(
            CrRetencionCargadoCargaItemRequest item,
            CrRetencionCargadoCargaRequest request,
            DateTime fechaServidor,
            int numeroLinea)
        {
            string movimientoId = CrRetencionCargado_Movimiento_Resolver(item.movimiento);
            bool esIndefinida = request.tipo_deduccion.StartsWith("I", StringComparison.OrdinalIgnoreCase);

            int plazo = request.archivo_excel
                ? (item.plazo ?? 999)
                : (esIndefinida ? 999 : 1);

            decimal cuota = request.archivo_excel
                ? (item.cuota ?? 0)
                : (plazo <= 0 ? 0 : Math.Round(item.monto / plazo, 2));

            return new CrRetencionCargadoCargaLineaDb
            {
                linea = numeroLinea,
                codigo = request.codigo,
                cod_institucion = request.cod_institucion,
                cod_deductora = request.cod_deductora,
                proceso = Convert.ToInt64(request.proceso, CultureInfo.InvariantCulture),
                cedula = NormalizarTexto(item.cedula),
                nombre = (item.nombre ?? string.Empty).Trim(),
                monto = item.monto,
                movimiento_id = movimientoId,
                plazo = plazo,
                cuota = cuota,
                operacion = request.archivo_excel ? (item.operacion ?? string.Empty).Trim() : string.Empty,
                formaliza = request.archivo_excel ? (item.formalizacion ?? fechaServidor) : fechaServidor
            };
        }

        private static string CrRetencionCargado_Movimiento_Resolver(string movimiento)
        {
            string valor = NormalizarTexto(movimiento);

            return valor switch
            {
                "I" or "1" => "I",
                "E" or "3" => "E",
                "C" or "2" => "C",
                _ => "X"
            };
        }

        private static decimal CrRetencionCargado_PriDeduc_Construir(string proceso, string frecuenciaId)
        {
            string frecuenciaNormalizada = (frecuenciaId ?? "0").Trim();
            string valor = $"{proceso}.{frecuenciaNormalizada}";

            return decimal.TryParse(valor, NumberStyles.Any, CultureInfo.InvariantCulture, out var priDeduc)
                ? priDeduc
                : 0m;
        }

        private static string NormalizarTexto(string valor)
        {
            return (valor ?? string.Empty).Trim();
        }

        private sealed class CrRetencionCargadoCargaLineaDb
        {
            public int linea { get; set; }
            public string codigo { get; set; } = string.Empty;
            public int cod_institucion { get; set; }
            public int cod_deductora { get; set; }
            public long proceso { get; set; }
            public string cedula { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public decimal monto { get; set; }
            public string movimiento_id { get; set; } = string.Empty;
            public int plazo { get; set; }
            public decimal cuota { get; set; }
            public string operacion { get; set; } = string.Empty;
            public DateTime formaliza { get; set; }
        }
    }
}