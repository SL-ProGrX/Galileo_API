using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslConsultaDB
    {
        private const string TodosOpcion = "TODOS";

        /// <summary>
        /// Consulta los expedientes Fosol aplicando los filtros indicados (vista vFSL_CasosLista).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con los filtros de la consulta.</param>
        /// <returns>Lista de expedientes.</returns>
        public ErrorDto<List<FslConsultaExpedienteDatos>> FslConsultaExpedientes_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FslConsultaFiltros>(filtros) ?? new FslConsultaFiltros();

            var parametros = new DynamicParameters();
            var estados = filtro.estado == "TODOS"
                ? new[] { "A", "R", "P", "X", "Y" }
                : new[] { filtro.estado };
            parametros.Add("estados", estados);

            var condiciones = new List<string>();
            AgregarFiltroResolucion(filtro, condiciones);
            AgregarFiltroPlanCausa(filtro, condiciones, parametros);
            AgregarFiltroEnfermedad(filtro, condiciones, parametros);
            AgregarFiltroFechas(filtro, condiciones, parametros);
            AgregarFiltroBusqueda(filtro, condiciones, parametros);
            AgregarFiltrosSimples(filtro, condiciones, parametros);
            AgregarFiltrosFunciones(filtro, condiciones, parametros);

            var extra = condiciones.Count == 0 ? string.Empty : " AND " + string.Join(" AND ", condiciones);

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var sql = $@"SELECT COD_EXPEDIENTE AS cod_expediente, CEDULA AS cedula, NOMBRE AS nombre, EDAD AS edad,
                                    REGISTRO_USUARIO AS registro_usuario, REGISTRO_FECHA AS registro_fecha, ESTADO_DESC AS estado_desc,
                                    PLAN_DESC AS plan_desc, CAUSA_DESC AS causa_desc, ENFERMEDAD_DESC AS enfermedad_desc,
                                    COMITE_DESC AS comite_desc, RESOLUCION_FECHA AS resolucion_fecha,
                                    TOTAL_DISPONIBLE AS total_disponible, TOTAL_APLICADO AS total_aplicado, TOTAL_SOBRANTE AS total_sobrante,
                                    PRESENTA_CEDULA AS presenta_cedula, PRESENTA_NOMBRE AS presenta_nombre
                             FROM vFSL_CasosLista
                             WHERE Estado IN @estados {extra}";

                return connection.Query<FslConsultaExpedienteDatos>(sql, parametros).ToList();
            });
        }

        /// <summary>
        /// Agrega el filtro de resolución aprobada.
        /// </summary>
        private static void AgregarFiltroResolucion(FslConsultaFiltros filtro, List<string> condiciones)
        {
            if (filtro.estado == "AP")
            {
                condiciones.Add("RESOLUCION_ESTADO = 'Y'");
            }
        }

        /// <summary>
        /// Agrega el filtro de plan y causas.
        /// </summary>
        private static void AgregarFiltroPlanCausa(FslConsultaFiltros filtro, List<string> condiciones, DynamicParameters parametros)
        {
            if (filtro.cod_plan == TodosOpcion)
            {
                return;
            }

            parametros.Add("cod_plan", filtro.cod_plan);
            condiciones.Add("COD_PLAN = @cod_plan");

            var causas = filtro.cod_causa.Select(c => c.item).ToArray();
            parametros.Add("causas", causas);
            condiciones.Add("COD_CAUSA IN @causas");
        }

        /// <summary>
        /// Agrega el filtro de enfermedades (excluye 'TODOS').
        /// </summary>
        private static void AgregarFiltroEnfermedad(FslConsultaFiltros filtro, List<string> condiciones, DynamicParameters parametros)
        {
            if (filtro.cod_enfermedad.Count == 0)
            {
                return;
            }

            var enfermedades = filtro.cod_enfermedad.Select(e => e.item).Where(i => i != TodosOpcion).ToArray();
            parametros.Add("enfermedades", enfermedades);
            condiciones.Add("COD_ENFERMEDAD IN @enfermedades");
        }

        /// <summary>
        /// Agrega el filtro de rango de fechas según el estado.
        /// </summary>
        private static void AgregarFiltroFechas(FslConsultaFiltros filtro, List<string> condiciones, DynamicParameters parametros)
        {
            if (filtro.fechas)
            {
                return;
            }

            parametros.Add("fechaInicio", filtro.fecha_inicio);
            parametros.Add("fechaCorte", filtro.fecha_corte);

            var columna = filtro.estado is "A" or "R" ? "Resolucion_Fecha" : "Registro_Fecha";
            condiciones.Add($"{columna} BETWEEN @fechaInicio AND @fechaCorte");
        }

        /// <summary>
        /// Agrega el filtro de búsqueda por texto (cédula, cédula/nombre del presentante) y nombre.
        /// </summary>
        private static void AgregarFiltroBusqueda(FslConsultaFiltros filtro, List<string> condiciones, DynamicParameters parametros)
        {
            if (!string.IsNullOrEmpty(filtro.texto_buscar))
            {
                var columna = filtro.cod_buscarPor switch
                {
                    "01" => "cedula",
                    "02" => "Presenta_Cedula",
                    "03" => "Presenta_Nombre",
                    _ => null
                };

                if (columna != null)
                {
                    parametros.Add("textoBuscar", $"%{filtro.texto_buscar}%");
                    condiciones.Add($"{columna} LIKE @textoBuscar");
                }
            }

            if (!string.IsNullOrEmpty(filtro.nombre))
            {
                parametros.Add("nombre", $"%{filtro.nombre}%");
                condiciones.Add("Nombre LIKE @nombre");
            }
        }

        /// <summary>
        /// Agrega los filtros simples de igualdad (tipo, estado persona, comité, expediente, usuario).
        /// </summary>
        private static void AgregarFiltrosSimples(FslConsultaFiltros filtro, List<string> condiciones, DynamicParameters parametros)
        {
            if (filtro.cod_tipo != TodosOpcion)
            {
                parametros.Add("codTipo", filtro.cod_tipo);
                condiciones.Add("Tipo_Desembolso = @codTipo");
            }

            if (!string.IsNullOrEmpty(filtro.estadoPersona))
            {
                parametros.Add("estadoPersona", filtro.estadoPersona);
                condiciones.Add("EstadoActual = @estadoPersona");
            }

            if (filtro.cod_comite != TodosOpcion)
            {
                parametros.Add("codComite", filtro.cod_comite);
                condiciones.Add("COD_COMITE = @codComite");

                if (filtro.resueltoMiembro != TodosOpcion)
                {
                    parametros.Add("resueltoMiembro", filtro.resueltoMiembro);
                    condiciones.Add("dbo.fxFSL_Expediente_ComiteMiembro(Cod_Expediente, @resueltoMiembro) >= 1");
                }
            }

            if (!string.IsNullOrEmpty(filtro.expediente))
            {
                parametros.Add("expediente", filtro.expediente);
                condiciones.Add("cod_expediente = @expediente");
            }

            if (!string.IsNullOrEmpty(filtro.usuario))
            {
                parametros.Add("usuario", filtro.usuario);
                condiciones.Add("Registro_Usuario = @usuario");
            }
        }

        /// <summary>
        /// Agrega los filtros basados en funciones (gestión y apelación registrada).
        /// </summary>
        private static void AgregarFiltrosFunciones(FslConsultaFiltros filtro, List<string> condiciones, DynamicParameters parametros)
        {
            if (filtro.gestionRegistrada != TodosOpcion)
            {
                parametros.Add("gestionRegistrada", filtro.gestionRegistrada);
                condiciones.Add("dbo.fxFSL_Expediente_GestionRegistrada(cod_Expediente, @gestionRegistrada) >= 1");
            }

            if (filtro.apelacionRegistrada != TodosOpcion)
            {
                parametros.Add("apelacionRegistrada", filtro.apelacionRegistrada);
                condiciones.Add("dbo.fxFSL_Expediente_ApelacionRegistrada(cod_Expediente, @apelacionRegistrada) >= 1");
            }
        }
    }
}
