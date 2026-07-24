using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosMonitorDB
    {
        private const string TodosOpcion = "T";

        /// <summary>
        /// Obtiene los beneficios del monitor (vBeneficios_Integral) aplicando filtros dinámicos y paginación.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtroString">JSON con los filtros del monitor.</param>
        /// <returns>Lista de beneficios y total.</returns>
        public ErrorDto<VBeneficiosIntegralDtoLista> BeneficiosMonitor_Obtener(int CodCliente, string filtroString)
        {
            var filtros = JsonConvert.DeserializeObject<BeneficiosMonitorFiltros>(filtroString) ?? new BeneficiosMonitorFiltros();
            var parametros = CrearParametrosMonitor(filtros);

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new VBeneficiosIntegralDtoLista();

                const string sqlCount = @"SELECT COUNT(*)
                                          FROM vBeneficios_Integral
                                          WHERE (@aplicaFecha = 0
                                                 OR (@fechaTipo = 'R' AND Registra_Fecha BETWEEN @fechaInicio AND @fechaCorte)
                                                 OR (@fechaTipo = 'A' AND Autoriza_Fecha BETWEEN @fechaInicio AND @fechaCorte)
                                                 OR (@fechaTipo = 'P' AND Pago_Fecha BETWEEN @fechaInicio AND @fechaCorte))
                                            AND (@aplicaEstado = 0 OR Estado = @estado)
                                            AND (@aplicaInstitucion = 0 OR Cod_Institucion = @institucion)
                                            AND (@aplicaOficina = 0 OR cod_Oficina = @oficina)
                                            AND (@aplicaEstadoPersona = 0 OR EstadoActual = @estadoPersona)
                                            AND (@aplicaUnidad = 0 OR Departamento_Desc = @unidad)
                                            AND (@aplicaUsuarioRegistra = 0 OR Registra_Usuario LIKE @usuarioRegistra)
                                            AND (@aplicaUsuarioAutoriza = 0 OR Autoriza_Usuario LIKE @usuarioAutoriza)
                                            AND (@aplicaBeneficiario = 0 OR NOMBRE_BENEFICIARIO LIKE @beneficiarioNombre)
                                            AND (@aplicaSolicitaId = 0 OR Solicita LIKE @solicitaId)
                                            AND (@aplicaSolicitaNombre = 0 OR Solicita_Nombre LIKE @solicitaNombre)
                                            AND (@aplicaBeneficios = 0 OR Cod_Beneficio IN @beneficios)
                                            AND (@aplicaFiltroGlobal = 0
                                                 OR Cod_Beneficio LIKE @vfiltro
                                                 OR cedula LIKE @vfiltro
                                                 OR NOMBRE_BENEFICIARIO LIKE @vfiltro)";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, parametros);

                const string sql = @"SELECT 0 AS Btn, Cod_Beneficio, Consec, Cedula, NOMBRE_BENEFICIARIO AS nombre_beneficiario,
                                            Monto, Estado_Desc AS estado_desc, Beneficio_Desc AS beneficio_desc,
                                            Solicita, Solicita_Nombre AS solicita_nombre, Registra_Fecha AS registra_fecha,
                                            Registra_User AS registra_user, Autoriza_Fecha AS autoriza_fecha, Autoriza_User AS autoriza_user,
                                            Empresa_Desc AS empresa_desc, Departamento_Desc AS departamento_desc, Oficina_Desc AS oficina_desc
                                     FROM vBeneficios_Integral
                                     WHERE (@aplicaFecha = 0
                                            OR (@fechaTipo = 'R' AND Registra_Fecha BETWEEN @fechaInicio AND @fechaCorte)
                                            OR (@fechaTipo = 'A' AND Autoriza_Fecha BETWEEN @fechaInicio AND @fechaCorte)
                                            OR (@fechaTipo = 'P' AND Pago_Fecha BETWEEN @fechaInicio AND @fechaCorte))
                                       AND (@aplicaEstado = 0 OR Estado = @estado)
                                       AND (@aplicaInstitucion = 0 OR Cod_Institucion = @institucion)
                                       AND (@aplicaOficina = 0 OR cod_Oficina = @oficina)
                                       AND (@aplicaEstadoPersona = 0 OR EstadoActual = @estadoPersona)
                                       AND (@aplicaUnidad = 0 OR Departamento_Desc = @unidad)
                                       AND (@aplicaUsuarioRegistra = 0 OR Registra_Usuario LIKE @usuarioRegistra)
                                       AND (@aplicaUsuarioAutoriza = 0 OR Autoriza_Usuario LIKE @usuarioAutoriza)
                                       AND (@aplicaBeneficiario = 0 OR NOMBRE_BENEFICIARIO LIKE @beneficiarioNombre)
                                       AND (@aplicaSolicitaId = 0 OR Solicita LIKE @solicitaId)
                                       AND (@aplicaSolicitaNombre = 0 OR Solicita_Nombre LIKE @solicitaNombre)
                                       AND (@aplicaBeneficios = 0 OR Cod_Beneficio IN @beneficios)
                                       AND (@aplicaFiltroGlobal = 0
                                            OR Cod_Beneficio LIKE @vfiltro
                                            OR cedula LIKE @vfiltro
                                            OR NOMBRE_BENEFICIARIO LIKE @vfiltro)
                                     ORDER BY Registra_fecha DESC, Beneficio_Desc, Consec DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Beneficios = connection.Query<VBeneficiosIntegralDto>(sql, parametros).ToList();
                return response;
            });
        }

        /// <summary>
        /// Crea los parámetros de los filtros fijos del monitor.
        /// </summary>
        private static DynamicParameters CrearParametrosMonitor(BeneficiosMonitorFiltros filtros)
        {
            var parametros = new DynamicParameters();
            var fechaInicioTexto = filtros.fecha_inicio ?? string.Empty;
            var fechaCorteTexto = filtros.fecha_corte ?? string.Empty;
            var aplicaFecha = filtros.fecha is "R" or "A" or "P"
                && fechaInicioTexto.Length > 0
                && fechaCorteTexto.Length > 0;
            var beneficios = filtros.beneficio_id?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? Array.Empty<string>();

            parametros.Add("aplicaFecha", aplicaFecha);
            parametros.Add("fechaTipo", filtros.fecha ?? string.Empty);
            parametros.Add("fechaInicio", aplicaFecha
                ? DateTimeOffset.Parse(fechaInicioTexto, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " 00:00:00"
                : string.Empty);
            parametros.Add("fechaCorte", aplicaFecha
                ? DateTimeOffset.Parse(fechaCorteTexto, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " 23:59:59"
                : string.Empty);
            parametros.Add("aplicaEstado", filtros.estado != TodosOpcion);
            parametros.Add("estado", filtros.estado);
            parametros.Add("aplicaInstitucion", filtros.institucion != TodosOpcion);
            parametros.Add("institucion", filtros.institucion);
            parametros.Add("aplicaOficina", filtros.oficina != TodosOpcion);
            parametros.Add("oficina", filtros.oficina);
            parametros.Add("aplicaEstadoPersona", filtros.estado_persona != TodosOpcion);
            parametros.Add("estadoPersona", filtros.estado_persona);
            parametros.Add("aplicaUnidad", filtros.institucion != TodosOpcion && !string.IsNullOrEmpty(filtros.unidad));
            parametros.Add("unidad", filtros.unidad?.Trim() ?? string.Empty);
            parametros.Add("aplicaUsuarioRegistra", !string.IsNullOrEmpty(filtros.usuario_registra));
            parametros.Add("usuarioRegistra", $"%{filtros.usuario_registra ?? string.Empty}%");
            parametros.Add("aplicaUsuarioAutoriza", !string.IsNullOrEmpty(filtros.usuario_autoriza));
            parametros.Add("usuarioAutoriza", $"%{filtros.usuario_autoriza ?? string.Empty}%");
            parametros.Add("aplicaBeneficiario", !string.IsNullOrEmpty(filtros.beneficiario_nombre));
            parametros.Add("beneficiarioNombre", $"%{filtros.beneficiario_nombre ?? string.Empty}%");
            parametros.Add("aplicaSolicitaId", !string.IsNullOrEmpty(filtros.solicita_id));
            parametros.Add("solicitaId", $"%{filtros.solicita_id ?? string.Empty}%");
            parametros.Add("aplicaSolicitaNombre", !string.IsNullOrEmpty(filtros.solicita_nombre));
            parametros.Add("solicitaNombre", $"%{filtros.solicita_nombre ?? string.Empty}%");
            parametros.Add("aplicaBeneficios", beneficios.Length > 0);
            parametros.Add("beneficios", beneficios.Length > 0 ? beneficios : new[] { string.Empty });
            parametros.Add("aplicaFiltroGlobal", !string.IsNullOrEmpty(filtros.vfiltro));
            parametros.Add("vfiltro", $"%{filtros.vfiltro}%");
            parametros.Add("offset", filtros.pagina ?? 0);
            parametros.Add("fetch", filtros.paginacion ?? 10);

            return parametros;
        }
    }
}
