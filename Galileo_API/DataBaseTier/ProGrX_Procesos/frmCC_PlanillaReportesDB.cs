using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCCPlanillaReportesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MCobroDb _mCobroDb;
        private readonly MProGrxMain _mProGrxMain;
        private const int ScrollSiguiente = 1;
        private const int ScrollAnterior = 2;
        private const string MensajeProcesoInvalido = "El proceso indicado no es válido.";
        private const string MensajeInstitucionInvalida = "La institución indicada no existe.";
        private const string RESUMEN = "Resumen";
        private const string DETALLE = "Detalle";

        public FrmCCPlanillaReportesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
            _mProGrxMain = new MProGrxMain(config);
        }
        /// <summary>
        /// Obtiene el catálogo fijo de opciones principales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CcPlanillaReporteCatalogoDto>> CC_PlanillaReportes_Catalogo_Obtener(int CodEmpresa)
        {
            _ = CodEmpresa;

            var lista = new List<CcPlanillaReporteCatalogoDto>
            {
                new() { codigo = "01", descripcion = "Genera Deducciones" },
                new() { codigo = "02", descripcion = "Carga Deducciones" },
                new() { codigo = "03", descripcion = "Desgloce" },
                new() { codigo = "04", descripcion = "Aplicacion a Patrimonio" },
                new() { codigo = "05", descripcion = "Aplicacion a Creditos" },
                new() { codigo = "06", descripcion = "Envio a Fondos" },
                new() { codigo = "07", descripcion = "Bitacora" },
                new() { codigo = "08", descripcion = "Analisis de Efectividad de Cobro x Planillas" },
                new() { codigo = "09", descripcion = "Cliente Corporativo" }
            };

            return DbHelper.CreateOkResponse(lista);
        }
        /// <summary>
        /// Obtiene el catálogo fijo de subreportes según opción principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigoOpcion"></param>
        /// <returns></returns>
        public static ErrorDto<List<CcPlanillaReporteTipoDto>> CC_PlanillaReportes_TiposReporte_Obtener(int CodEmpresa, string? codigoOpcion)
        {
            _ = CodEmpresa;

            string opcion = (codigoOpcion ?? string.Empty).Trim();

            var lista = CrearTiposReporte(opcion);

            return DbHelper.CreateOkResponse(lista);
        }
        /// <summary>
        /// Obtiene parámetros iniciales de la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaReportesParametrosInicialesDto> CC_PlanillaReportes_ParametrosIniciales_Obtener(int CodEmpresa,int codInstitucion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlInstitucion = @"
                select
                    cast(cod_institucion as int) as cod_institucion,
                    rtrim(descripcion) as descripcion,
                    isnull(frecuencia, 'M') as frecuencia_id,
                    isnull(porc_aporte, 0) as porc_aporte,
                    isnull(porc_ahorro, 0) as porc_ahorro
                from instituciones
                where cod_institucion = @codInstitucion;";

                decimal proceso = _mProGrxMain.glngFechaCR(CodEmpresa);
                DateTime fechaServidor = _mProGrxMain.fxFechaServidor(CodEmpresa, 0);

                var institucion = conn.QueryFirstOrDefault<CcPlanillaInstitucionInfoDto>(
                    sqlInstitucion,
                    new { codInstitucion });

                if (institucion == null)
                {
                    return DbHelper.CreateErrorResponse<CcPlanillaReportesParametrosInicialesDto>(
                        MensajeInstitucionInvalida,
                        -2,
                        new CcPlanillaReportesParametrosInicialesDto());
                }

                var result = new CcPlanillaReportesParametrosInicialesDto
                {
                    proceso = proceso,
                    proceso_format = MCobroDb.fxFechaProcesoFormat(proceso),
                    frecuencia_pago = (institucion.frecuencia_id ?? "M").Trim(),
                    cod_institucion = institucion.cod_institucion,
                    institucion_descripcion = (institucion.descripcion ?? string.Empty).Trim(),
                    fecha_inicio = fechaServidor,
                    fecha_corte = fechaServidor,
                    tipos_cobro = CrearTiposCobro()
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcPlanillaReportesParametrosInicialesDto>(
                    ex.Message,
                    -1,
                    new CcPlanillaReportesParametrosInicialesDto());
            }
        }
        /// <summary>
        /// Obtiene dropdown de instituciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                select
                    cast(cod_institucion as varchar(20)) as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                order by descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Obtiene información de una institución.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaInstitucionInfoDto> CC_PlanillaReportes_Institucion_Obtener(int CodEmpresa, int codInstitucion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
                select
                    cast(cod_institucion as int) as cod_institucion,
                    rtrim(descripcion) as descripcion,
                    isnull(frecuencia, 'M') as frecuencia_id,
                    isnull(porc_aporte, 0) as porc_aporte,
                    isnull(porc_ahorro, 0) as porc_ahorro
                from instituciones
                where cod_institucion = @codInstitucion;";

                var item = conn.QueryFirstOrDefault<CcPlanillaInstitucionInfoDto>(sql, new
                {
                    codInstitucion
                });

                if (item == null)
                {
                    return DbHelper.CreateErrorResponse<CcPlanillaInstitucionInfoDto>(
                        MensajeInstitucionInvalida,
                        -2,
                        new CcPlanillaInstitucionInfoDto());
                }

                item.descripcion = (item.descripcion ?? string.Empty).Trim();
                item.frecuencia_id = (item.frecuencia_id ?? "M").Trim();

                return DbHelper.CreateOkResponse(item);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcPlanillaInstitucionInfoDto>(
                    ex.Message,
                    -1,
                    new CcPlanillaInstitucionInfoDto());
            }
        }
        /// <summary>
        /// Navega al siguiente o anterior proceso disponible.
        /// scrollCode: 1=siguiente, 2=anterior.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="procesoActual"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaProcesoScrollDto> CC_PlanillaReportes_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            try
            {
                if (procesoActual <= 0)
                {
                    return DbHelper.CreateErrorResponse<CcPlanillaProcesoScrollDto>(
                        MensajeProcesoInvalido,
                        -2,
                        new CcPlanillaProcesoScrollDto());
                }

                decimal proceso = scrollCode switch
                {
                    ScrollSiguiente => _mCobroDb.fxFechaProcesoSiguiente(CodEmpresa, procesoActual),
                    ScrollAnterior => _mCobroDb.fxFechaProcesoAnterior(CodEmpresa, procesoActual),
                    _ => procesoActual
                };

                var result = new CcPlanillaProcesoScrollDto
                {
                    proceso = proceso,
                    proceso_format = MCobroDb.fxFechaProcesoFormat(proceso)
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcPlanillaProcesoScrollDto>(
                    ex.Message,
                    -1,
                    new CcPlanillaProcesoScrollDto());
            }
        }
        /// <summary>
        /// Obtiene líneas disponibles para análisis de efectividad.
        /// Si todasInstituciones = true, no filtra por institución.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="proceso"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="todasInstituciones"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaReportes_Lineas_Dropdown_Obtener(int CodEmpresa,decimal proceso,int? codInstitucion,bool todasInstituciones)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(p.codigo) as item,
                        rtrim(c.descripcion) + '   [' + rtrim(p.codigo) + ']' as descripcion
                    from PRM_CREDITOS p
                    inner join CATALOGO c on p.CODIGO = c.CODIGO
                    where p.FECHA_PROCESO = @proceso
                      and (@todasInstituciones = 1 or p.COD_INSTITUCION = @codInstitucion)
                    group by c.DESCRIPCION, p.CODIGO
                    order by c.DESCRIPCION;";

                return conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    proceso,
                    codInstitucion,
                    todasInstituciones = todasInstituciones ? 1 : 0
                }).ToList();
            });
        }
        /// <summary>
        /// Obtiene catálogo fijo de tipos de cobro del análisis.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public static ErrorDto<List<CcPlanillaTipoCobroDto>> CC_PlanillaReportes_TiposCobro_Obtener(int CodEmpresa)
        {
            _ = CodEmpresa;
            return DbHelper.CreateOkResponse(CrearTiposCobro());
        }
        private static List<CcPlanillaReporteTipoDto> CrearTiposReporte(string opcion)
        {
            return opcion switch
            {
                "01" => new List<CcPlanillaReporteTipoDto>
            {
                CrearTipo(opcion, "01", RESUMEN),
                CrearTipo(opcion, "02", DETALLE),
                CrearTipo(opcion, "03", "Línea"),
                CrearTipo(opcion, "04", "Línea Detalle"),
                CrearTipo(opcion, "05", "Base Actual")
            },

                "02" => AgregarTipos(
                    TiposBasicos(opcion),
                    CrearTipo(opcion, "03", "No Localizados")),

                "03" => AgregarTipos(
                    TiposBasicos(opcion),
                    CrearTipo(opcion, "03", "Agrupado: Línea"),
                    CrearTipo(opcion, "04", "Agrupado: Persona")),

                "07" => new List<CcPlanillaReporteTipoDto>
            {
                CrearTipo(opcion, "01", "Fechas"),
                CrearTipo(opcion, "02", "Proceso + Fechas"),
                CrearTipo(opcion, "03", "Proceso + Institución")
            },

                "08" => new List<CcPlanillaReporteTipoDto>
            {
                CrearTipo(opcion, "01", RESUMEN),
                CrearTipo(opcion, "02", DETALLE),
                CrearTipo(opcion, "03", "Tipo"),
                CrearTipo(opcion, "04", "Estadística"),
                CrearTipo(opcion, "05", "Línea"),
                CrearTipo(opcion, "06", "Línea Resumen"),
                CrearTipo(opcion, "07", "Persona"),
                CrearTipo(opcion, "08", "Persona Resumen")
            },

                "09" => TiposBasicosConResumen(opcion),

                _ => new List<CcPlanillaReporteTipoDto>
            {
                CrearTipo(opcion, "01", RESUMEN)
            }
            };
        }
        private static CcPlanillaReporteTipoDto CrearTipo(string opcion, string codigoReporte, string descripcion)
        {
            return new CcPlanillaReporteTipoDto
            {
                codigo_opcion = opcion,
                codigo_reporte = codigoReporte,
                descripcion = descripcion
            };
        }
        private static List<CcPlanillaTipoCobroDto> CrearTiposCobro()
        {
            return new List<CcPlanillaTipoCobroDto>
            {
                new() { item = "1000", descripcion = "Todos" },
                new() { item = "1", descripcion = "Cobro Registrado" },
                new() { item = "2", descripcion = "Cobro No Registrado" },
                new() { item = "3", descripcion = "Cobro Registrado / No Enviado" },
                new() { item = "4", descripcion = "Cobro Apl. NC." },
                new() { item = "5", descripcion = "Sobrante Enviado a Fondo" }
            };
        }
        private static List<CcPlanillaReporteTipoDto> TiposBasicos(string opcion)
        {
            return new List<CcPlanillaReporteTipoDto>
    {
        CrearTipo(opcion, "01", RESUMEN),
        CrearTipo(opcion, "02", DETALLE)
    };
        }
        private static List<CcPlanillaReporteTipoDto> TiposBasicosConResumen(string opcion)
        {
            return new List<CcPlanillaReporteTipoDto>
    {
        CrearTipo(opcion, "01", RESUMEN),
        CrearTipo(opcion, "02", DETALLE),
        CrearTipo(opcion, "03", "Línea Resumen"),
        CrearTipo(opcion, "04", "Persona Resumen")
    };
        }
        private static List<CcPlanillaReporteTipoDto> AgregarTipos(List<CcPlanillaReporteTipoDto> baseList,params CcPlanillaReporteTipoDto[] extras)
        {
            baseList.AddRange(extras);
            return baseList;
        }
    }
}