using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrGarantiasPatrimonialesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCreditos = 3;
        private const string MovimientoRegistra = "Registra";
        private const string MovimientoElimina = "Elimina";

        public FrmCrGarantiasPatrimonialesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene garantías patrimoniales configuradas para el formulario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(GARANTIA) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from CRD_GARANTIA_TIPOS
                    where FORMULARIO = 'F01'
                    order by DESCRIPCION;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene estados de persona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(COD_ESTADO) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from afi_Estados_Persona
                    order by DESCRIPCION;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene operadoras.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_GarantiasPatrimoniales_Operadoras_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        rtrim(cast(COD_OPERADORA as varchar(20))) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from fnd_Operadoras
                    order by DESCRIPCION;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene lista de garantías patrimoniales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrGarantiasPatrimonialesListaResult> CR_GarantiasPatrimoniales_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var filtrosResult = ObtenerFiltros(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrGarantiasPatrimonialesListaResult>(
                    filtrosResult.Description ?? "Parámetros inválidos.",
                    -1,
                    ListaVacia());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var filtros = filtrosResult.Result ?? new FiltrosGarantiasPatrimoniales();
                var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
                var paginacion = filtros.paginacion <= 0 ? int.MaxValue : filtros.paginacion;
                var offset = filtros.paginacion <= 0 ? 0 : pagina * paginacion;
                var filtro = (filtros.filtro ?? string.Empty).Trim();
                var like = filtro.Length > 0 ? $"%{filtro}%" : null;
                var sortField = NormalizarSortField(filtros.sortField);
                var sortOrder = filtros.sortOrder == 1 ? 1 : 0;

                const string sql = @"
            create table #Resultado
            (
                COD_OPERADORA smallint not null,
                COD_PLAN varchar(10) not null,
                DESCRIPCION varchar(100) null,
                PATRIMONIO smallint not null,
                TIPO varchar(2) null,
                LINEA_ID int not null,
                MEMBRESIA_INICIO smallint not null,
                MEMBRESIA_CORTE int not null,
                PORCENTAJE decimal(10,2) not null
            );

            insert into #Resultado
            exec spCrd_Garantia_Ahorros_Consulta @garantia, @cod_estado;

            select count(1)
            from #Resultado
            where @filtro is null
               or cast(LINEA_ID as varchar(20)) like @like
               or cast(COD_OPERADORA as varchar(20)) like @like
               or COD_PLAN like @like
               or DESCRIPCION like @like
               or cast(MEMBRESIA_INICIO as varchar(20)) like @like
               or cast(MEMBRESIA_CORTE as varchar(20)) like @like
               or cast(PORCENTAJE as varchar(30)) like @like
               or case when PATRIMONIO = 1 then 'Sí' else 'No' end like @like;

            select
                LINEA_ID as linea_id,
                COD_OPERADORA as cod_operadora,
                COD_PLAN as cod_plan,
                rtrim(isnull(DESCRIPCION,'')) as descripcion,
                MEMBRESIA_INICIO as membresia_inicio,
                MEMBRESIA_CORTE as membresia_corte,
                PORCENTAJE as porcentaje,
                cast(case when PATRIMONIO = 1 then 1 else 0 end as bit) as patrimonio,
                case when PATRIMONIO = 1 then 'Sí' else 'No' end as patrimonio_descripcion
            from #Resultado
            where @filtro is null
               or cast(LINEA_ID as varchar(20)) like @like
               or cast(COD_OPERADORA as varchar(20)) like @like
               or COD_PLAN like @like
               or DESCRIPCION like @like
               or cast(MEMBRESIA_INICIO as varchar(20)) like @like
               or cast(MEMBRESIA_CORTE as varchar(20)) like @like
               or cast(PORCENTAJE as varchar(30)) like @like
               or case when PATRIMONIO = 1 then 'Sí' else 'No' end like @like
            order by
                case when @sortField = '' and @sortOrder = 0 then PATRIMONIO end desc,
                case when @sortField = '' and @sortOrder = 0 then TIPO end asc,
                case when @sortField = '' and @sortOrder = 0 then PORCENTAJE end desc,
                case when @sortField = '' and @sortOrder = 0 then COD_PLAN end asc,

                case when @sortField = 'linea_id' and @sortOrder = 1 then LINEA_ID end asc,
                case when @sortField = 'linea_id' and @sortOrder = 0 then LINEA_ID end desc,

                case when @sortField = 'cod_operadora' and @sortOrder = 1 then COD_OPERADORA end asc,
                case when @sortField = 'cod_operadora' and @sortOrder = 0 then COD_OPERADORA end desc,

                case when @sortField = 'cod_plan' and @sortOrder = 1 then COD_PLAN end asc,
                case when @sortField = 'cod_plan' and @sortOrder = 0 then COD_PLAN end desc,

                case when @sortField = 'descripcion' and @sortOrder = 1 then DESCRIPCION end asc,
                case when @sortField = 'descripcion' and @sortOrder = 0 then DESCRIPCION end desc,

                case when @sortField = 'membresia_inicio' and @sortOrder = 1 then MEMBRESIA_INICIO end asc,
                case when @sortField = 'membresia_inicio' and @sortOrder = 0 then MEMBRESIA_INICIO end desc,

                case when @sortField = 'membresia_corte' and @sortOrder = 1 then MEMBRESIA_CORTE end asc,
                case when @sortField = 'membresia_corte' and @sortOrder = 0 then MEMBRESIA_CORTE end desc,

                case when @sortField = 'porcentaje' and @sortOrder = 1 then PORCENTAJE end asc,
                case when @sortField = 'porcentaje' and @sortOrder = 0 then PORCENTAJE end desc,

                case when @sortField = 'patrimonio' and @sortOrder = 1 then PATRIMONIO end asc,
                case when @sortField = 'patrimonio' and @sortOrder = 0 then PATRIMONIO end desc,
                PATRIMONIO desc,
                TIPO asc,
                PORCENTAJE desc,
                COD_PLAN asc
            offset @offset rows fetch next @paginacion rows only;";

                using var multi = conn.QueryMultiple(sql, new
                {
                    garantia = filtros.garantia,
                    cod_estado = filtros.cod_estado,
                    filtro = filtro.Length > 0 ? filtro : null,
                    like,
                    sortField,
                    sortOrder,
                    offset,
                    paginacion
                });

                var total = multi.ReadFirst<int>();
                var lista = multi.Read<CrGarantiasPatrimonialesData>().ToList();

                return DbHelper.CreateOkResponse(new CrGarantiasPatrimonialesListaResult
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CrGarantiasPatrimonialesListaResult>(ex.Message, -1, ListaVacia());
            }
        }

        /// <summary>
        /// Exporta lista de garantías patrimoniales.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrGarantiasPatrimonialesListaResult> CR_GarantiasPatrimoniales_Lista_Export(int CodEmpresa, string parametros)
        {
            var filtrosResult = ObtenerFiltros(parametros);
            if (filtrosResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrGarantiasPatrimonialesListaResult>(
                    filtrosResult.Description ?? "Parámetros inválidos.",
                    -1,
                    ListaVacia());
            }

            var filtros = filtrosResult.Result ?? new FiltrosGarantiasPatrimoniales();
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return CR_GarantiasPatrimoniales_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }

        /// <summary>
        /// Guarda una garantía patrimonial.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_GarantiasPatrimoniales_Guardar(int CodEmpresa, CrGarantiasPatrimonialesRegistroRequest request, string usuario)
        {
            return EjecutarMovimiento(CodEmpresa, request, usuario, "A", MovimientoRegistra);
        }

        /// <summary>
        /// Elimina una garantía patrimonial.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_GarantiasPatrimoniales_Eliminar(int CodEmpresa, CrGarantiasPatrimonialesRegistroRequest request, string usuario)
        {
            return EjecutarMovimiento(CodEmpresa, request, usuario, "E", MovimientoElimina);
        }

        private ErrorDto EjecutarMovimiento(int CodEmpresa, CrGarantiasPatrimonialesRegistroRequest request, string usuario, string movimiento, string bitacoraMovimiento)
        {
            var validacion = ValidarRequest(request, usuario, movimiento);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Execute(
                    "spCrd_Garantia_Ahorros_Registro",
                    new
                    {
                        Garantia = request.garantia.Trim(),
                        Estado = request.cod_estado.Trim(),
                        Linea = request.linea_id.GetValueOrDefault(),
                        MembresiaInicio = request.membresia_inicio.GetValueOrDefault(),
                        MembresiaCorte = request.membresia_corte.GetValueOrDefault(),
                        Patrimonio = request.patrimonio.GetValueOrDefault() ? 1 : 0,
                        Operadora = request.cod_operadora.GetValueOrDefault(),
                        Plan = request.cod_plan.Trim(),
                        Porcentaje = request.porcentaje.GetValueOrDefault(),
                        Usuario = usuario.Trim(),
                        Mov = movimiento
                    },
                    commandType: CommandType.StoredProcedure);

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.Trim().ToUpperInvariant(),
                    DetalleMovimiento = CrearDetalleBitacora(request),
                    Movimiento = bitacoraMovimiento,
                    Modulo = ModuloCreditos
                });

                return DbHelper.OkResponse("Proceso realizado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto ValidarRequest(CrGarantiasPatrimonialesRegistroRequest request, string usuario, string movimiento)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse("Solicitud inválida.");
            }

            if (string.IsNullOrWhiteSpace(request.garantia))
            {
                return DbHelper.ErrorResponse("Debe seleccionar la garantía.");
            }

            if (string.IsNullOrWhiteSpace(request.cod_estado))
            {
                return DbHelper.ErrorResponse("Debe seleccionar el estado de la persona.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse("Usuario requerido.");
            }

            if (movimiento == "E")
            {
                if (!request.linea_id.HasValue || request.linea_id.Value <= 0)
                {
                    return DbHelper.ErrorResponse("Debe seleccionar un registro válido para eliminar.");
                }

                return DbHelper.OkResponse("Ok");
            }

            if (!request.porcentaje.HasValue || request.porcentaje.Value < 0 || request.porcentaje.Value > 999)
            {
                return DbHelper.ErrorResponse("Porcentaje no es válido");
            }

            if (!request.membresia_inicio.HasValue)
            {
                return DbHelper.ErrorResponse("Rango de Inicio la membresía no es válido");
            }

            if (!request.membresia_corte.HasValue)
            {
                return DbHelper.ErrorResponse("Rango de Corte la membresía no es válido");
            }

            if (!request.cod_operadora.HasValue)
            {
                return DbHelper.ErrorResponse("Debe seleccionar la operadora.");
            }

            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return DbHelper.ErrorResponse("Debe seleccionar el plan.");
            }

            return DbHelper.OkResponse("Ok");
        }

        private static ErrorDto<FiltrosGarantiasPatrimoniales> ObtenerFiltros(string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosGarantiasPatrimoniales>(parametros)
                              ?? new FiltrosGarantiasPatrimoniales();

                if (string.IsNullOrWhiteSpace(filtros.garantia))
                {
                    return DbHelper.CreateErrorResponse<FiltrosGarantiasPatrimoniales>("Debe seleccionar la garantía.");
                }

                if (string.IsNullOrWhiteSpace(filtros.cod_estado))
                {
                    return DbHelper.CreateErrorResponse<FiltrosGarantiasPatrimoniales>("Debe seleccionar el estado de la persona.");
                }

                return DbHelper.CreateOkResponse(filtros);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosGarantiasPatrimoniales>(ex.Message);
            }
        }
        private static string CrearDetalleBitacora(CrGarantiasPatrimonialesRegistroRequest request)
        {
            return string.Concat(
                "Garantia de s/Ahorros, Linea: ", request.linea_id.GetValueOrDefault(),
                ", Gar: ", request.garantia?.Trim(),
                ", Est: ", request.cod_estado?.Trim(),
                " Plan : ", request.cod_plan?.Trim(),
                " Porcentaje : ", request.porcentaje.GetValueOrDefault(),
                ", Mem.I: ", request.membresia_inicio.GetValueOrDefault(),
                ", Mem.C: ", request.membresia_corte.GetValueOrDefault());
        }

        private static CrGarantiasPatrimonialesListaResult ListaVacia()
        {
            return new CrGarantiasPatrimonialesListaResult
            {
                total = 0,
                lista = new List<CrGarantiasPatrimonialesData>()
            };
        }
        private static string NormalizarSortField(string? sortField)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            return field switch
            {
                "linea_id" => "linea_id",
                "cod_operadora" => "cod_operadora",
                "cod_plan" => "cod_plan",
                "descripcion" => "descripcion",
                "membresia_inicio" => "membresia_inicio",
                "membresia_corte" => "membresia_corte",
                "porcentaje" => "porcentaje",
                "patrimonio" => "patrimonio",
                _ => string.Empty
            };
        }
    }
}