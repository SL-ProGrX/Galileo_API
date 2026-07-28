using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhConciliadorPatronalDB
    {
        private readonly PortalDB _portalDb;

        public FrmAhConciliadorPatronalDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las instituciones activas para el proceso de conciliación patronal.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Ah_ConciliadorPatronal_Instituciones_Obtener(
            int codEmpresa)
        {
            const string sql = @"
select
    cast(COD_INSTITUCION as varchar(20)) as item,
    '[' + rtrim(isnull(COD_DIVISA, '')) + ']  ' + rtrim(isnull(DESCRIPCION, '')) as descripcion
from INSTITUCIONES
where ACTIVA = 1
order by COD_INSTITUCION;";

            var result = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);

            if (result.Code == 0 && result.Result != null)
            {
                result.Result.Insert(0, new DropDownListaGenericaModel
                {
                    item = "0",
                    descripcion = "TODOS"
                });
            }

            return result;
        }

        /// <summary>
        /// Normaliza y valida los registros cargados desde Excel para poblar el histórico.
        /// </summary>
        public static ErrorDto<List<FrmAhConciliadorPatronalHistoricoDto>> Ah_ConciliadorPatronal_Cargado(
            int codEmpresa,
            FrmAhConciliadorPatronalCargadoRequest? request)
        {
            var result = new List<FrmAhConciliadorPatronalHistoricoDto>();
            var validacion = Ah_ConciliadorPatronal_ValidarRequestBase(request, result);

            if (validacion != null)
            {
                return validacion;
            }

            var registrosNormalizados = Ah_ConciliadorPatronal_NormalizarRegistros(
                request.registros,
                out var validacionRegistros);

            if (validacionRegistros != null)
            {
                return validacionRegistros;
            }

            return DbHelper.CreateOkResponse(registrosNormalizados);
        }

        /// <summary>
        /// Aplica el proceso de conciliación patronal ejecutando el SP por cada fila cargada.
        /// </summary>
        public ErrorDto<FrmAhConciliadorPatronalAplicarResponse> Ah_ConciliadorPatronal_Aplicar(
            int codEmpresa,
            FrmAhConciliadorPatronalCargadoRequest? request)
        {
            var response = new FrmAhConciliadorPatronalAplicarResponse();
            var validacion = Ah_ConciliadorPatronal_ValidarRequestBase(request, response);

            if (validacion != null)
            {
                return validacion;
            }

            var registrosNormalizados = Ah_ConciliadorPatronal_NormalizarRegistros(
                request.registros,
                out var validacionRegistrosAplicar,
                response);

            if (validacionRegistrosAplicar != null)
            {
                return validacionRegistrosAplicar;
            }

            const string sqlAplicar = @"
exec spPAT_Concilia_Patronal_Registro
    @identificacion,
    @id_alterna,
    @nombre,
    @patronal,
    @cod_institucion,
    @fecha_corte,
    @registro_usuario,
    @tipo_analisis;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    foreach (var registro in registrosNormalizados)
                    {
                        conn.Execute(
                            sqlAplicar,
                            new
                            {
                                identificacion = registro.identificacion,
                                id_alterna = registro.id_alterna,
                                nombre = registro.nombre,
                                patronal = registro.patronal,
                                cod_institucion = request.cod_institucion,
                                fecha_corte = request.fecha_corte,
                                registro_usuario = request.registro_usuario,
                                tipo_analisis = request.tipo_analisis
                            },
                            transaction: transaction,
                            commandType: CommandType.Text);
                    }

                    transaction.Commit();

                    response.accion = "APLICAR";
                    response.total_registros = registrosNormalizados.Count;
                    response.mensaje = "Información Actualizada Satisfactoriamente!";

                    return DbHelper.CreateOkResponse(response);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene los casos de conciliación comparando el lote cargado contra la base actual.
        /// </summary>
        public ErrorDto<List<FrmAhConciliadorPatronalConciliacionDto>> Ah_ConciliadorPatronal_Conciliacion_Obtener(
            int codEmpresa,
            FrmAhConciliadorPatronalConciliacionRequest? request)
        {
            var result = new List<FrmAhConciliadorPatronalConciliacionDto>();
            var validacion = Ah_ConciliadorPatronal_ValidarConciliacionRequest(request, result);

            if (validacion != null)
            {
                return validacion;
            }

            var registrosNormalizados = Ah_ConciliadorPatronal_NormalizarRegistros(
                request.registros,
                out var validacionRegistros);

            if (validacionRegistros != null)
            {
                return DbHelper.CreateErrorResponse<List<FrmAhConciliadorPatronalConciliacionDto>>(
                    validacionRegistros.Description, -1, result);
            }

            const string sqlConciliacion = @"
;with base_patrimonial as (
    select
        r.Identificacion as identificacion,
        isnull(r.Empleado_Id, '') as id_alterna,
        ltrim(rtrim(coalesce(nullif(r.Nombre_Completo, ''), nullif(r.Nombre, ''), p.Nombre, ''))) as nombre,
        cast(isnull(p.Patronal, 0) as decimal(18,2)) as patronal
    from vRH_Personas r
    inner join vPAT_Consolidado p
        on p.Cedula = r.Identificacion
)
select
    x.identificacion,
    x.id_alterna,
    x.nombre,
    x.patronal
from (
    select
        c.identificacion,
        c.id_alterna,
        c.nombre,
        c.patronal
    from #PatronalCarga c
    left join base_patrimonial b
        on b.identificacion = c.identificacion
    where @localizados = 'P'
      and b.identificacion is null

    union all

    select
        b.identificacion,
        b.id_alterna,
        b.nombre,
        b.patronal
    from base_patrimonial b
    left join #PatronalCarga c
        on c.identificacion = b.identificacion
    where @localizados = 'B'
      and c.identificacion is null
) x
order by x.nombre, x.identificacion;";

            return Ah_ConciliadorPatronal_EjecutarConsultaConTablaTemporal(
    codEmpresa,
    registrosNormalizados,
    conn => conn.Query<FrmAhConciliadorPatronalConciliacionDto>(
        sqlConciliacion,
        new { localizados = request.localizados }).ToList());
        }

        /// <summary>
        /// Obtiene los resultados comparando el lote cargado contra la base actual.
        /// </summary>
        public ErrorDto<List<FrmAhConciliadorPatronalResultadoDto>> Ah_ConciliadorPatronal_Resultados_Obtener(
            int codEmpresa,
            FrmAhConciliadorPatronalResultadosRequest? request)
        {
            var result = new List<FrmAhConciliadorPatronalResultadoDto>();
            var validacion = Ah_ConciliadorPatronal_ValidarResultadosRequest(request, result);

            if (validacion != null)
            {
                return validacion;
            }

            var registrosNormalizados = Ah_ConciliadorPatronal_NormalizarRegistros(
                request.registros,
                out var validacionRegistros);

            if (validacionRegistros != null)
            {
                return DbHelper.CreateErrorResponse<List<FrmAhConciliadorPatronalResultadoDto>>(
                    validacionRegistros.Description,
                    -1,
                    result);
            }

            const string sqlResultados = @"
;with base_patrimonial as (
    select
        r.Identificacion as identificacion,
        isnull(r.Empleado_Id, '') as id_alterna,
        ltrim(rtrim(coalesce(nullif(r.Nombre_Completo, ''), nullif(r.Nombre, ''), p.Nombre, ''))) as nombre,
        cast(isnull(p.Patronal, 0) as decimal(18,2)) as aporte_registrado
    from vRH_Personas r
    left join vPAT_Consolidado p
        on p.Cedula = r.Identificacion
)
select
    coalesce(c.identificacion, b.identificacion) as identificacion,
    coalesce(nullif(c.id_alterna, ''), b.id_alterna, '') as id_alterna,
    coalesce(nullif(c.nombre, ''), b.nombre, '') as nombre,
    cast(isnull(c.patronal, 0) as decimal(18,2)) as patronal,
    cast(isnull(b.aporte_registrado, 0) as decimal(18,2)) as aporte_registrado,
    cast(isnull(c.patronal, 0) - isnull(b.aporte_registrado, 0) as decimal(18,2)) as diferencia
from #PatronalCarga c
full outer join base_patrimonial b
    on b.identificacion = c.identificacion
where
    @resultado = 'C'
    or cast(isnull(c.patronal, 0) - isnull(b.aporte_registrado, 0) as decimal(18,2)) <> 0
order by nombre, identificacion;";

            return Ah_ConciliadorPatronal_EjecutarConsultaConTablaTemporal(
     codEmpresa,
     registrosNormalizados,
     conn => conn.Query<FrmAhConciliadorPatronalResultadoDto>(
         sqlResultados,
         new { resultado = request.resultado }).ToList());

        }

        private ErrorDto<List<T>> Ah_ConciliadorPatronal_EjecutarConsultaConTablaTemporal<T>(
    int codEmpresa,
    List<FrmAhConciliadorPatronalHistoricoDto> registros,
    Func<SqlConnection, List<T>> ejecutarConsulta)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                Ah_ConciliadorPatronal_CrearTablaTemporal(conn);
                Ah_ConciliadorPatronal_CargarTablaTemporal(conn, registros);

                var data = ejecutarConsulta(conn);
                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<T>());
            }
        }

        private static void Ah_ConciliadorPatronal_CrearTablaTemporal(SqlConnection conn)
        {
            const string sql = @"
create table #PatronalCarga
(
    identificacion varchar(50) not null,
    id_alterna varchar(50) not null,
    nombre varchar(250) not null,
    patronal decimal(18,2) not null
);";

            conn.Execute(sql);
        }

        private static void Ah_ConciliadorPatronal_CargarTablaTemporal(
            SqlConnection conn,
            List<FrmAhConciliadorPatronalHistoricoDto> registros)
        {
            using var tabla = new DataTable();
            tabla.Columns.Add("identificacion", typeof(string));
            tabla.Columns.Add("id_alterna", typeof(string));
            tabla.Columns.Add("nombre", typeof(string));
            tabla.Columns.Add("patronal", typeof(decimal));

            foreach (var registro in registros)
            {
                tabla.Rows.Add(
                    registro.identificacion,
                    registro.id_alterna,
                    registro.nombre,
                    registro.patronal);
            }

            using var bulk = new SqlBulkCopy(conn);
            bulk.DestinationTableName = "#PatronalCarga";
            bulk.WriteToServer(tabla);
        }

        private static ErrorDto<T>? Ah_ConciliadorPatronal_ValidarRequestBase<T>(
            FrmAhConciliadorPatronalCargadoRequest? request,
            T defaultResult)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    defaultResult);
            }

            if (request.cod_institucion < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La institucion no es valida.",
                    -2,
                    defaultResult);
            }

            if (request.fecha_corte == DateTime.MinValue)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha de corte es requerida.",
                    -2,
                    defaultResult);
            }

            request.tipo_analisis = Ah_ConciliadorPatronal_NormalizarTipoAnalisis(request.tipo_analisis);
            if (string.IsNullOrWhiteSpace(request.tipo_analisis))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de analisis es requerido.",
                    -2,
                    defaultResult);
            }

            request.registro_usuario = Ah_ConciliadorPatronal_NormalizarTexto(request.registro_usuario, 50);
            if (string.IsNullOrWhiteSpace(request.registro_usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario del sistema es requerido.",
                    -2,
                    defaultResult);
            }

            if (request.registros == null || request.registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No existen casos cargados, verifique.",
                    -2,
                    defaultResult);
            }

            return null;
        }

        private static ErrorDto<T>? Ah_ConciliadorPatronal_ValidarConciliacionRequest<T>(
            FrmAhConciliadorPatronalConciliacionRequest? request,
            T defaultResult)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    defaultResult);
            }

            if (request.cod_institucion < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La institucion no es valida.",
                    -2,
                    defaultResult);
            }

            if (request.fecha_corte == DateTime.MinValue)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha de corte es requerida.",
                    -2,
                    defaultResult);
            }

            request.localizados = Ah_ConciliadorPatronal_NormalizarLocalizados(request.localizados);
            if (string.IsNullOrWhiteSpace(request.localizados))
            {
                return DbHelper.CreateErrorResponse(
                    "El criterio de no localizados es requerido.",
                    -2,
                    defaultResult);
            }

            if (request.registros == null || request.registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No existen casos cargados, verifique.",
                    -2,
                    defaultResult);
            }

            return null;
        }

        private static ErrorDto<T>? Ah_ConciliadorPatronal_ValidarResultadosRequest<T>(
            FrmAhConciliadorPatronalResultadosRequest? request,
            T defaultResult)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    defaultResult);
            }

            if (request.cod_institucion < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La institucion no es valida.",
                    -2,
                    defaultResult);
            }

            if (request.fecha_corte == DateTime.MinValue)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha de corte es requerida.",
                    -2,
                    defaultResult);
            }

            request.resultado = Ah_ConciliadorPatronal_NormalizarResultado(request.resultado);
            if (string.IsNullOrWhiteSpace(request.resultado))
            {
                return DbHelper.CreateErrorResponse(
                    "El criterio de resultado es requerido.",
                    -2,
                    defaultResult);
            }

            if (request.registros == null || request.registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No existen casos cargados, verifique.",
                    -2,
                    defaultResult);
            }

            return null;
        }

        private static List<FrmAhConciliadorPatronalHistoricoDto> Ah_ConciliadorPatronal_NormalizarRegistros(
            List<FrmAhConciliadorPatronalHistoricoDto> registros,
            out ErrorDto<List<FrmAhConciliadorPatronalHistoricoDto>>? validacion)
        {
            var normalizados = new List<FrmAhConciliadorPatronalHistoricoDto>();
            validacion = null;

            for (int i = 0; i < registros.Count; i++)
            {
                var fila = registros[i] ?? new FrmAhConciliadorPatronalHistoricoDto();
                var registro = Ah_ConciliadorPatronal_NormalizarRegistro(fila);

                validacion = Ah_ConciliadorPatronal_ValidarRegistro(
                    registro,
                    i + 1);

                if (validacion != null)
                {
                    return new List<FrmAhConciliadorPatronalHistoricoDto>();
                }

                normalizados.Add(registro);
            }

            return normalizados;
        }

        private static List<FrmAhConciliadorPatronalHistoricoDto> Ah_ConciliadorPatronal_NormalizarRegistros(
            List<FrmAhConciliadorPatronalHistoricoDto> registros,
            out ErrorDto<FrmAhConciliadorPatronalAplicarResponse>? validacion,
            FrmAhConciliadorPatronalAplicarResponse response)
        {
            var normalizados = new List<FrmAhConciliadorPatronalHistoricoDto>();
            validacion = null;

            for (int i = 0; i < registros.Count; i++)
            {
                var fila = registros[i] ?? new FrmAhConciliadorPatronalHistoricoDto();
                var registro = Ah_ConciliadorPatronal_NormalizarRegistro(fila);

                validacion = Ah_ConciliadorPatronal_ValidarRegistroAplicar(
                    registro,
                    i + 1,
                    response);

                if (validacion != null)
                {
                    return new List<FrmAhConciliadorPatronalHistoricoDto>();
                }

                normalizados.Add(registro);
            }

            return normalizados;
        }

        private static FrmAhConciliadorPatronalHistoricoDto Ah_ConciliadorPatronal_NormalizarRegistro(
            FrmAhConciliadorPatronalHistoricoDto fila)
        {
            return new FrmAhConciliadorPatronalHistoricoDto
            {
                identificacion = Ah_ConciliadorPatronal_NormalizarTexto(fila.identificacion, 50),
                id_alterna = Ah_ConciliadorPatronal_NormalizarTexto(fila.id_alterna, 50),
                nombre = Ah_ConciliadorPatronal_NormalizarTexto(fila.nombre, 250),
                patronal = decimal.Round(fila.patronal, 2, MidpointRounding.AwayFromZero)
            };
        }

        private static ErrorDto<List<FrmAhConciliadorPatronalHistoricoDto>>? Ah_ConciliadorPatronal_ValidarRegistro(
            FrmAhConciliadorPatronalHistoricoDto registro,
            int numeroFila)
        {
            if (string.IsNullOrWhiteSpace(registro.identificacion))
            {
                return DbHelper.CreateErrorResponse(
                    $"La fila {numeroFila} no contiene identificacion valida.",
                    -2,
                    new List<FrmAhConciliadorPatronalHistoricoDto>());
            }

            if (string.IsNullOrWhiteSpace(registro.nombre))
            {
                return DbHelper.CreateErrorResponse(
                    $"La fila {numeroFila} no contiene nombre valido.",
                    -2,
                    new List<FrmAhConciliadorPatronalHistoricoDto>());
            }

            if (registro.patronal < 0)
            {
                return DbHelper.CreateErrorResponse(
                    $"La fila {numeroFila} contiene un aporte patronal invalido.",
                    -2,
                    new List<FrmAhConciliadorPatronalHistoricoDto>());
            }

            return null;
        }

        private static ErrorDto<FrmAhConciliadorPatronalAplicarResponse>? Ah_ConciliadorPatronal_ValidarRegistroAplicar(
            FrmAhConciliadorPatronalHistoricoDto registro,
            int numeroFila,
            FrmAhConciliadorPatronalAplicarResponse response)
        {
            if (string.IsNullOrWhiteSpace(registro.identificacion))
            {
                return DbHelper.CreateErrorResponse(
                    $"La fila {numeroFila} no contiene identificacion valida.",
                    -2,
                    response);
            }

            if (string.IsNullOrWhiteSpace(registro.nombre))
            {
                return DbHelper.CreateErrorResponse(
                    $"La fila {numeroFila} no contiene nombre valido.",
                    -2,
                    response);
            }

            if (registro.patronal < 0)
            {
                return DbHelper.CreateErrorResponse(
                    $"La fila {numeroFila} contiene un aporte patronal invalido.",
                    -2,
                    response);
            }

            return null;
        }

        private static string Ah_ConciliadorPatronal_NormalizarTipoAnalisis(string? tipoAnalisis)
        {
            var tipo = (tipoAnalisis ?? string.Empty).Trim().ToUpperInvariant();
            return tipo is "R" or "T" ? tipo : string.Empty;
        }

        private static string Ah_ConciliadorPatronal_NormalizarLocalizados(string? localizados)
        {
            var valor = (localizados ?? string.Empty).Trim().ToUpperInvariant();
            return valor is "P" or "B" ? valor : string.Empty;
        }

        private static string Ah_ConciliadorPatronal_NormalizarResultado(string? resultado)
        {
            var valor = (resultado ?? string.Empty).Trim().ToUpperInvariant();
            return valor is "C" or "D" ? valor : string.Empty;
        }

        private static string Ah_ConciliadorPatronal_NormalizarTexto(string? valor, int maximo)
        {
            var texto = (valor ?? string.Empty).Trim();
            return texto.Length <= maximo ? texto : texto[..maximo];
        }
    }
}
