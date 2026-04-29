using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCcPlanillaCtaCorreccionDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MCobroDb _mCobroDb;
        private readonly MProGrxMain _mProGrxMain;

        private const int vModulo = 10;
        private const int TipoConsultaCuotas = 1;
        private const int TipoConsultaBitacora = 2;
        private const string NombreFormulario = "frmCC_PlanillaCtaCorreccion";
        private const string BotonAutorizado = "Autorizado";

        private const string MensajeNoAutorizado = "Su usuario no está autorizado para modificar cuotas de planillas...!";

        private const string SqlInstituciones = @"
select
    cast(cod_institucion as varchar(20)) as item,
    rtrim(descripcion) as descripcion
from instituciones
order by descripcion;";

        private const string SqlPersonasF4 = @"
select
    rtrim(isnull(cedula, '')) as cedula,
    rtrim(isnull(nombre, '')) as nombre
from socios
where (@texto = '' or cedula like @like or nombre like @like)
order by nombre, cedula;";

        private const string SqlListaCuotas = @"
select
    1 as tipo_consulta,
    rtrim(cast(d.id_consecutivo as varchar(30))) as id_registro,
    rtrim(cast(d.id_consecutivo as varchar(30))) as referencia,
    rtrim(isnull(d.cod_deduccion, '')) as cod_deduccion,
    '' as proceso_bitacora,
    cast(isnull(d.id_solicitud, 0) as bigint) as id_solicitud,
    rtrim(isnull(d.codigo, '')) as linea,
    rtrim(isnull(d.cedula, '')) as cedula,
    rtrim(isnull(s.nombre, '')) as nombre,
    cast(isnull(d.morosidad, 0) as smallint) as indicador_mora,
    cast(isnull(d.cuota, 0) as decimal(14,2)) as cuota,
    cast(0 as decimal(14,2)) as cuota_anterior,
    '' as registro_usuario,
    cast(null as datetime) as registro_fecha
from prm_enviado_detalle d
left join socios s on d.cedula = s.cedula
where d.cod_institucion = @cod_institucion
  and d.fecpro = @proceso
  and (@operacion = '' or cast(d.id_solicitud as varchar(30)) = @operacion)
  and (@linea = '' or rtrim(isnull(d.codigo, '')) = @linea)
  and (@cedula = '' or rtrim(isnull(d.cedula, '')) = @cedula)
  and (@nombreLike is null or s.nombre like @nombreLike);";

        private const string SqlListaBitacora = @"
select
    2 as tipo_consulta,
    rtrim(cast(d.id_seq as varchar(30))) as id_registro,
    rtrim(isnull(d.referencia, '')) as referencia,
    '' as cod_deduccion,
    rtrim(cast(d.proceso as varchar(20))) as proceso_bitacora,
    cast(isnull(d.id_solicitud, 0) as bigint) as id_solicitud,
    rtrim(isnull(d.linea, '')) as linea,
    rtrim(isnull(d.cedula, '')) as cedula,
    rtrim(isnull(s.nombre, '')) as nombre,
    cast(isnull(d.indicador_mora, 0) as smallint) as indicador_mora,
    cast(isnull(d.cuota_nueva, 0) as decimal(14,2)) as cuota,
    cast(isnull(d.cuota_anterior, 0) as decimal(14,2)) as cuota_anterior,
    rtrim(isnull(d.registro_usuario, '')) as registro_usuario,
    d.registro_fecha
from prm_cambios d
left join socios s on d.cedula = s.cedula
where d.cod_institucion = @cod_institucion
  and d.proceso = @proceso
  and (@operacion = '' or cast(d.id_solicitud as varchar(30)) = @operacion)
  and (@linea = '' or rtrim(isnull(d.linea, '')) = @linea)
  and (@cedula = '' or rtrim(isnull(d.cedula, '')) = @cedula)
  and (@usuario = '' or rtrim(isnull(d.registro_usuario, '')) = @usuario)
  and (@nombreLike is null or s.nombre like @nombreLike);";

        public FrmCcPlanillaCtaCorreccionDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mCobroDb = new MCobroDb(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene el dropdown de instituciones para la pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaCtaCorreccion_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var lista = conn.Query<DropDownListaGenericaModel>(SqlInstituciones).ToList();
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene el proceso actual, siguiente o anterior usando las funciones de cobro existentes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="procesoActual"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaCtaCorreccionProcesoScrollDto> CC_PlanillaCtaCorreccion_Proceso_Scroll_Obtener(int CodEmpresa, int scrollCode, decimal procesoActual)
        {
            try
            {
                decimal baseProceso = ResolveProceso(CodEmpresa, procesoActual);

                decimal proceso = scrollCode switch
                {
                    1 => _mCobroDb.fxFechaProcesoSiguiente(CodEmpresa, baseProceso),
                    2 => _mCobroDb.fxFechaProcesoAnterior(CodEmpresa, baseProceso),
                    _ => baseProceso
                };

                return DbHelper.CreateOkResponse(new CcPlanillaCtaCorreccionProcesoScrollDto
                {
                    proceso = proceso,
                    proceso_format = MCobroDb.fxFechaProcesoFormat(proceso)
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CcPlanillaCtaCorreccionProcesoScrollDto());
            }
        }

        /// <summary>
        /// Obtiene el F4 de personas por cédula o nombre.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<CcPlanillaCtaCorreccionPersonaF4Dto>> CC_PlanillaCtaCorreccion_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                string filtro = Clean(texto);
                string? like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";

                var lista = conn.Query<CcPlanillaCtaCorreccionPersonaF4Dto>(SqlPersonasF4, new
                {
                    texto = filtro,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CcPlanillaCtaCorreccionPersonaF4Dto>>(
                    ex.Message,
                    -1,
                    new List<CcPlanillaCtaCorreccionPersonaF4Dto>());
            }
        }

        /// <summary>
        /// Obtiene la lista principal de la pantalla según el tipo de consulta indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaCtaCorreccionListaResult> CC_PlanillaCtaCorreccion_Lista_Obtener(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest req)
        {
            return EjecutarLista(CodEmpresa, req, true);
        }

        /// <summary>
        /// Obtiene la exportación de la lista principal sin paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaCtaCorreccionListaResult> CC_PlanillaCtaCorreccion_Lista_Export(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest req)
        {
            return EjecutarLista(CodEmpresa, req, false);
        }

        /// <summary>
        /// Actualiza la cuota manual de la planilla validando derecho y registrando bitácora web.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto CC_PlanillaCtaCorreccion_Cuota_Actualizar(int CodEmpresa, CcPlanillaCtaCorreccionActualizarCuotaRequest req)
        {
            try
            {
                string usuario = Clean(req.usuario);

                if (!TieneDerechoModificar(CodEmpresa, usuario))
                {
                    return DbHelper.ErrorResponse(MensajeNoAutorizado, -2);
                }

                if (req.cuota == req.cuota_anterior)
                {
                    return DbHelper.CreateOkResponse();
                }

                int proceso = Convert.ToInt32(ResolveProceso(CodEmpresa, req.proceso));
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var p = new DynamicParameters();
                p.Add("@Institucion", req.cod_institucion, DbType.Int32);
                p.Add("@Proceso", proceso, DbType.Int32);
                p.Add("@Usuario", usuario, DbType.String);
                p.Add("@Operacion", req.operacion, DbType.Int64);
                p.Add("@Linea", Clean(req.linea), DbType.String);
                p.Add("@Cedula", Clean(req.cedula), DbType.String);
                p.Add("@Cuota", req.cuota, DbType.Decimal);
                p.Add("@CuotaAnt", req.cuota_anterior, DbType.Decimal);
                p.Add("@Referencia", Clean(req.referencia), DbType.String);
                p.Add("@MoraInd", req.mora_ind, DbType.Int16);
                p.Add("@CodDeduccion", Clean(req.cod_deduccion), DbType.String);

                conn.Execute(
                    "spPrm_CreditoCambiosManuales_Registro",
                    p,
                    commandType: CommandType.StoredProcedure);

                LogBitacora(new LogBitacoraParams
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    Institucion = req.cod_institucion,
                    Proceso = proceso,
                    Operacion = req.operacion,
                    Cedula = req.cedula,
                    Referencia = req.referencia,
                    CuotaAnterior = req.cuota_anterior,
                    CuotaNueva = req.cuota
                });

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private ErrorDto<CcPlanillaCtaCorreccionListaResult> EjecutarLista(int CodEmpresa, CcPlanillaCtaCorreccionListaRequest? req, bool usarPaginacion)
        {
            try
            {
                req ??= new CcPlanillaCtaCorreccionListaRequest
                {
                    cod_institucion = 0,
                    proceso = 0
                };
                req.filtros ??= new FiltrosLazyLoadData();
                req.proceso = ResolveProceso(CodEmpresa, req.proceso);

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                List<CcPlanillaCtaCorreccionListaItemDto> lista = QueryLista(conn, req);
                lista = ApplyGlobalFilter(lista, req.filtros.filtro).ToList();
                lista = ApplySort(lista, req.filtros).ToList();

                int total = lista.Count;

                if (usarPaginacion)
                {
                    lista = ApplyPagination(lista, req.filtros).ToList();
                }

                return DbHelper.CreateOkResponse(new CcPlanillaCtaCorreccionListaResult
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CcPlanillaCtaCorreccionListaResult());
            }
        }

        private static List<CcPlanillaCtaCorreccionListaItemDto> QueryLista(SqlConnection conn, CcPlanillaCtaCorreccionListaRequest req)
        {
            string operacion = Clean(req.operacion);
            string linea = Clean(req.linea);
            string cedula = Clean(req.cedula);
            string usuario = Clean(req.usuario);
            string nombre = Clean(req.nombre);
            string? nombreLike = string.IsNullOrWhiteSpace(nombre) ? null : $"%{nombre}%";

            string sql = NormalizeTipoConsulta(req.tipo_consulta) == TipoConsultaBitacora
                ? SqlListaBitacora
                : SqlListaCuotas;

            return conn.Query<CcPlanillaCtaCorreccionListaItemDto>(sql, new
            {
                req.cod_institucion,
                proceso = Convert.ToInt32(req.proceso),
                operacion,
                linea,
                cedula,
                usuario,
                nombreLike
            }).ToList();
        }

        private static IEnumerable<CcPlanillaCtaCorreccionListaItemDto> ApplyGlobalFilter(IEnumerable<CcPlanillaCtaCorreccionListaItemDto> rows, string? filtro)
        {
            string texto = Clean(filtro);
            if (string.IsNullOrWhiteSpace(texto))
            {
                return rows;
            }

            return rows.Where(r =>
                ContainsText(r.id_registro, texto) ||
                ContainsText(r.referencia, texto) ||
                ContainsText(r.cod_deduccion, texto) ||
                ContainsText(r.proceso_bitacora, texto) ||
                ContainsText(r.id_solicitud.ToString(CultureInfo.InvariantCulture), texto) ||
                ContainsText(r.linea, texto) ||
                ContainsText(r.cedula, texto) ||
                ContainsText(r.nombre, texto) ||
                ContainsText(r.registro_usuario, texto) ||
                ContainsText(r.indicador_mora.ToString(CultureInfo.InvariantCulture), texto) ||
                ContainsText(r.cuota.ToString("0.##", CultureInfo.InvariantCulture), texto) ||
                ContainsText(r.cuota_anterior.ToString("0.##", CultureInfo.InvariantCulture), texto));
        }

        private static IEnumerable<CcPlanillaCtaCorreccionListaItemDto> ApplySort(IEnumerable<CcPlanillaCtaCorreccionListaItemDto> rows, FiltrosLazyLoadData? filtros)
        {
            string sortField = Clean(filtros?.sortField).ToLowerInvariant();
            bool asc = (filtros?.sortOrder ?? 0) == 0;

            Func<CcPlanillaCtaCorreccionListaItemDto, object?> keySelector = sortField switch
            {
                "referencia" => x => x.referencia,
                "cod_deduccion" => x => x.cod_deduccion,
                "proceso_bitacora" => x => x.proceso_bitacora,
                "id_solicitud" => x => x.id_solicitud,
                "linea" => x => x.linea,
                "cedula" => x => x.cedula,
                "nombre" => x => x.nombre,
                "indicador_mora" => x => x.indicador_mora,
                "cuota" => x => x.cuota,
                "cuota_anterior" => x => x.cuota_anterior,
                "registro_usuario" => x => x.registro_usuario,
                "registro_fecha" => x => x.registro_fecha,
                _ => x => x.id_registro
            };

            return asc
                ? rows.OrderBy(keySelector)
                : rows.OrderByDescending(keySelector);
        }

        private static IEnumerable<CcPlanillaCtaCorreccionListaItemDto> ApplyPagination(IEnumerable<CcPlanillaCtaCorreccionListaItemDto> rows, FiltrosLazyLoadData? filtros)
        {
            int offset = filtros?.pagina ?? 0;
            int fetch = filtros?.paginacion ?? 0;

            if (offset < 0)
            {
                offset = 0;
            }

            if (fetch <= 0)
            {
                return rows;
            }

            return rows.Skip(offset).Take(fetch);
        }

        private decimal ResolveProceso(int codEmpresa, decimal procesoActual)
        {
            if (procesoActual > 0)
            {
                return procesoActual;
            }

            decimal proceso = _mProGrxMain.glngFechaCR(codEmpresa);
            return proceso > 0 ? proceso : 0;
        }

        private bool TieneDerechoModificar(int empresaId, string usuario)
        {
            int acceso = _securityMainDb.Derecho(new ParametrosAccesoDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                Modulo = vModulo,
                FormName = NombreFormulario,
                Boton = BotonAutorizado
            });

            return acceso > 0;
        }
         private sealed class LogBitacoraParams
        {
            public int EmpresaId { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public int Institucion { get; set; }
            public int Proceso { get; set; }
            public long Operacion { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Referencia { get; set; } = string.Empty;
            public decimal CuotaAnterior { get; set; }
            public decimal CuotaNueva { get; set; }
        }
        private void LogBitacora(LogBitacoraParams p)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = p.EmpresaId,
                Usuario = p.Usuario,
                DetalleMovimiento =
                    $"Ajuste manual de cuota planilla. Institución: {p.Institucion}, Proceso: {p.Proceso}, Operación: {p.Operacion}, Cédula: {p.Cedula}, Referencia: {p.Referencia}, Cuota anterior: {p.CuotaAnterior:0.00}, Cuota nueva: {p.CuotaNueva:0.00}",
                Movimiento = "Modifica - WEB",
                Modulo = vModulo
            });
        }

        private static int NormalizeTipoConsulta(int tipoConsulta)
            => tipoConsulta == TipoConsultaBitacora ? TipoConsultaBitacora : TipoConsultaCuotas;

        private static bool ContainsText(string? source, string filtro)
            => !string.IsNullOrWhiteSpace(source) &&
               source.Contains(filtro, StringComparison.OrdinalIgnoreCase);

        private static string Clean(string? value)
            => (value ?? string.Empty).Trim();
    }
}