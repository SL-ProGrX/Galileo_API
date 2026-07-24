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

            var parametros = new DynamicParameters();
            var where = ConstruirWhere(filtros, parametros);

            var offset = filtros.pagina ?? 0;
            var fetch = filtros.paginacion ?? 10;
            parametros.Add("offset", offset);
            parametros.Add("fetch", fetch);

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new VBeneficiosIntegralDtoLista();

                var sqlCount = $"SELECT COUNT(*) FROM vBeneficios_Integral {where}";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, parametros);

                var sql = $@"SELECT 0 AS Btn, Cod_Beneficio, Consec, Cedula, NOMBRE_BENEFICIARIO AS nombre_beneficiario,
                                    Monto, Estado_Desc AS estado_desc, Beneficio_Desc AS beneficio_desc,
                                    Solicita, Solicita_Nombre AS solicita_nombre, Registra_Fecha AS registra_fecha,
                                    Registra_User AS registra_user, Autoriza_Fecha AS autoriza_fecha, Autoriza_User AS autoriza_user,
                                    Empresa_Desc AS empresa_desc, Departamento_Desc AS departamento_desc, Oficina_Desc AS oficina_desc
                             FROM vBeneficios_Integral {where}
                             ORDER BY Registra_fecha DESC, Beneficio_Desc, Consec DESC
                             OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Beneficios = connection.Query<VBeneficiosIntegralDto>(sql, parametros).ToList();
                return response;
            });
        }

        /// <summary>
        /// Construye la cláusula WHERE parametrizada a partir de los filtros del monitor.
        /// </summary>
        private static string ConstruirWhere(BeneficiosMonitorFiltros filtros, DynamicParameters parametros)
        {
            var condiciones = new List<string>();

            AgregarFiltroFecha(filtros, condiciones, parametros);
            AgregarFiltrosExactos(filtros, condiciones, parametros);
            AgregarFiltrosTexto(filtros, condiciones, parametros);
            AgregarFiltroBeneficios(filtros, condiciones, parametros);
            AgregarFiltroGlobal(filtros, condiciones, parametros);

            return condiciones.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", condiciones);
        }

        /// <summary>
        /// Agrega el filtro por rango de fechas según el tipo (Registra/Autoriza/Pago).
        /// </summary>
        private static void AgregarFiltroFecha(BeneficiosMonitorFiltros filtros, List<string> condiciones, DynamicParameters parametros)
        {
            if (string.IsNullOrEmpty(filtros.fecha) || string.IsNullOrEmpty(filtros.fecha_inicio) || string.IsNullOrEmpty(filtros.fecha_corte))
            {
                return;
            }

            var columna = filtros.fecha switch
            {
                "R" => "Registra_Fecha",
                "A" => "Autoriza_Fecha",
                "P" => "Pago_Fecha",
                _ => null
            };

            if (columna == null)
            {
                return;
            }

            parametros.Add("fechaInicio", DateTimeOffset.Parse(filtros.fecha_inicio, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " 00:00:00");
            parametros.Add("fechaCorte", DateTimeOffset.Parse(filtros.fecha_corte, CultureInfo.InvariantCulture).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " 23:59:59");
            condiciones.Add($"{columna} BETWEEN @fechaInicio AND @fechaCorte");
        }

        /// <summary>
        /// Agrega los filtros de igualdad (estado, institución, oficina, estado de persona, unidad).
        /// </summary>
        private static void AgregarFiltrosExactos(BeneficiosMonitorFiltros filtros, List<string> condiciones, DynamicParameters parametros)
        {
            if (filtros.estado != TodosOpcion)
            {
                parametros.Add("estado", filtros.estado);
                condiciones.Add("Estado = @estado");
            }

            if (filtros.institucion != TodosOpcion)
            {
                parametros.Add("institucion", filtros.institucion);
                condiciones.Add("Cod_Institucion = @institucion");
            }
            else
            {
                filtros.unidad = string.Empty;
            }

            if (filtros.oficina != TodosOpcion)
            {
                parametros.Add("oficina", filtros.oficina);
                condiciones.Add("cod_Oficina = @oficina");
            }

            if (filtros.estado_persona != TodosOpcion)
            {
                parametros.Add("estadoPersona", filtros.estado_persona);
                condiciones.Add("EstadoActual = @estadoPersona");
            }

            if (!string.IsNullOrEmpty(filtros.unidad))
            {
                parametros.Add("unidad", filtros.unidad.Trim());
                condiciones.Add("Departamento_Desc = @unidad");
            }
        }

        /// <summary>
        /// Agrega los filtros de coincidencia parcial (usuarios, nombres, solicitante).
        /// </summary>
        private static void AgregarFiltrosTexto(BeneficiosMonitorFiltros filtros, List<string> condiciones, DynamicParameters parametros)
        {
            AgregarLike(filtros.usuario_registra, "Registra_Usuario", "usuarioRegistra", condiciones, parametros);
            AgregarLike(filtros.usuario_autoriza, "Autoriza_Usuario", "usuarioAutoriza", condiciones, parametros);
            AgregarLike(filtros.beneficiario_nombre, "NOMBRE_BENEFICIARIO", "beneficiarioNombre", condiciones, parametros);
            AgregarLike(filtros.solicita_id, "Solicita", "solicitaId", condiciones, parametros);
            AgregarLike(filtros.solicita_nombre, "Solicita_Nombre", "solicitaNombre", condiciones, parametros);
        }

        /// <summary>
        /// Agrega una condición LIKE parametrizada si el valor no está vacío.
        /// </summary>
        private static void AgregarLike(string? valor, string columna, string param, List<string> condiciones, DynamicParameters parametros)
        {
            if (string.IsNullOrEmpty(valor))
            {
                return;
            }

            parametros.Add(param, $"%{valor}%");
            condiciones.Add($"{columna} LIKE @{param}");
        }

        /// <summary>
        /// Agrega el filtro de beneficios (lista separada por comas) usando IN parametrizado.
        /// </summary>
        private static void AgregarFiltroBeneficios(BeneficiosMonitorFiltros filtros, List<string> condiciones, DynamicParameters parametros)
        {
            if (string.IsNullOrEmpty(filtros.beneficio_id))
            {
                return;
            }

            var beneficios = filtros.beneficio_id.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            parametros.Add("beneficios", beneficios);
            condiciones.Add("Cod_Beneficio IN @beneficios");
        }

        /// <summary>
        /// Agrega el filtro global de búsqueda (código, cédula o nombre).
        /// </summary>
        private static void AgregarFiltroGlobal(BeneficiosMonitorFiltros filtros, List<string> condiciones, DynamicParameters parametros)
        {
            if (string.IsNullOrEmpty(filtros.vfiltro))
            {
                return;
            }

            parametros.Add("vfiltro", $"%{filtros.vfiltro}%");
            condiciones.Add("(Cod_Beneficio LIKE @vfiltro OR cedula LIKE @vfiltro OR NOMBRE_BENEFICIARIO LIKE @vfiltro)");
        }
    }
}
