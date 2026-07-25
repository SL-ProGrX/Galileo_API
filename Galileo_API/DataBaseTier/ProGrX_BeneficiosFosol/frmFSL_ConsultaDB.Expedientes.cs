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
        private const string ExpedientesSql = @"
            SELECT COD_EXPEDIENTE AS cod_expediente, CEDULA AS cedula, NOMBRE AS nombre, EDAD AS edad,
                   REGISTRO_USUARIO AS registro_usuario, REGISTRO_FECHA AS registro_fecha, ESTADO_DESC AS estado_desc,
                   PLAN_DESC AS plan_desc, CAUSA_DESC AS causa_desc, ENFERMEDAD_DESC AS enfermedad_desc,
                   COMITE_DESC AS comite_desc, RESOLUCION_FECHA AS resolucion_fecha,
                   TOTAL_DISPONIBLE AS total_disponible, TOTAL_APLICADO AS total_aplicado, TOTAL_SOBRANTE AS total_sobrante,
                   PRESENTA_CEDULA AS presenta_cedula, PRESENTA_NOMBRE AS presenta_nombre
            FROM vFSL_CasosLista
            WHERE Estado IN @estados
              AND (@resolucionAprobada = 0 OR RESOLUCION_ESTADO = 'Y')
              AND (@aplicaPlan = 0 OR (COD_PLAN = @codPlan AND COD_CAUSA IN @causas))
              AND (@aplicaEnfermedad = 0 OR COD_ENFERMEDAD IN @enfermedades)
              AND (@aplicaFechas = 0 OR
                   (@usarFechaResolucion = 1 AND Resolucion_Fecha BETWEEN @fechaInicio AND @fechaCorte) OR
                   (@usarFechaResolucion = 0 AND Registro_Fecha BETWEEN @fechaInicio AND @fechaCorte))
              AND (@buscarCedula = 0 OR cedula LIKE @textoBuscar)
              AND (@buscarPresentaCedula = 0 OR Presenta_Cedula LIKE @textoBuscar)
              AND (@buscarPresentaNombre = 0 OR Presenta_Nombre LIKE @textoBuscar)
              AND (@aplicaNombre = 0 OR Nombre LIKE @nombre)
              AND (@aplicaTipo = 0 OR Tipo_Desembolso = @codTipo)
              AND (@aplicaEstadoPersona = 0 OR EstadoActual = @estadoPersona)
              AND (@aplicaComite = 0 OR COD_COMITE = @codComite)
              AND CASE
                      WHEN @aplicaMiembro = 0 THEN 1
                      WHEN dbo.fxFSL_Expediente_ComiteMiembro(Cod_Expediente, @resueltoMiembro) >= 1 THEN 1
                      ELSE 0
                  END = 1
              AND (@aplicaExpediente = 0 OR cod_expediente = @expediente)
              AND (@aplicaUsuario = 0 OR Registro_Usuario = @usuario)
              AND CASE
                      WHEN @aplicaGestion = 0 THEN 1
                      WHEN dbo.fxFSL_Expediente_GestionRegistrada(cod_Expediente, @gestionRegistrada) >= 1 THEN 1
                      ELSE 0
                  END = 1
              AND CASE
                      WHEN @aplicaApelacion = 0 THEN 1
                      WHEN dbo.fxFSL_Expediente_ApelacionRegistrada(cod_Expediente, @apelacionRegistrada) >= 1 THEN 1
                      ELSE 0
                  END = 1;";

        /// <summary>
        /// Consulta los expedientes Fosol aplicando los filtros indicados (vista vFSL_CasosLista).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con los filtros de la consulta.</param>
        /// <returns>Lista de expedientes.</returns>
        public ErrorDto<List<FslConsultaExpedienteDatos>> FslConsultaExpedientes_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FslConsultaFiltros>(filtros) ?? new FslConsultaFiltros();
            var parametros = FslConsultaExpedientes_CrearParametros(filtro);

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<FslConsultaExpedienteDatos>(ExpedientesSql, parametros).ToList());
        }

        /// <summary>
        /// Crea los parámetros y activadores de los filtros de expedientes.
        /// </summary>
        private static DynamicParameters FslConsultaExpedientes_CrearParametros(FslConsultaFiltros filtro)
        {
            var parametros = new DynamicParameters();
            var estados = filtro.estado == TodosOpcion
                ? new[] { "A", "R", "P", "X", "Y" }
                : new[] { filtro.estado };
            var causas = filtro.cod_causa.Select(c => c.item).ToArray();
            var enfermedades = filtro.cod_enfermedad.Select(e => e.item).Where(i => i != TodosOpcion).ToArray();
            var aplicaComite = filtro.cod_comite != TodosOpcion;

            parametros.Add("estados", estados);
            parametros.Add("resolucionAprobada", filtro.estado == "AP");
            parametros.Add("aplicaPlan", filtro.cod_plan != TodosOpcion);
            parametros.Add("codPlan", filtro.cod_plan);
            parametros.Add("causas", causas);
            parametros.Add("aplicaEnfermedad", filtro.cod_enfermedad.Count > 0);
            parametros.Add("enfermedades", enfermedades);
            parametros.Add("aplicaFechas", !filtro.fechas);
            parametros.Add("usarFechaResolucion", filtro.estado is "A" or "R");
            parametros.Add("fechaInicio", filtro.fecha_inicio);
            parametros.Add("fechaCorte", filtro.fecha_corte);
            var aplicaTexto = !string.IsNullOrEmpty(filtro.texto_buscar);
            parametros.Add("buscarCedula", aplicaTexto && filtro.cod_buscarPor == "01");
            parametros.Add("buscarPresentaCedula", aplicaTexto && filtro.cod_buscarPor == "02");
            parametros.Add("buscarPresentaNombre", aplicaTexto && filtro.cod_buscarPor == "03");
            parametros.Add("textoBuscar", $"%{filtro.texto_buscar}%");
            parametros.Add("aplicaNombre", !string.IsNullOrEmpty(filtro.nombre));
            parametros.Add("nombre", $"%{filtro.nombre}%");
            parametros.Add("aplicaTipo", filtro.cod_tipo != TodosOpcion);
            parametros.Add("codTipo", filtro.cod_tipo);
            parametros.Add("aplicaEstadoPersona", !string.IsNullOrEmpty(filtro.estadoPersona));
            parametros.Add("estadoPersona", filtro.estadoPersona);
            parametros.Add("aplicaComite", aplicaComite);
            parametros.Add("codComite", filtro.cod_comite);
            parametros.Add("aplicaMiembro", aplicaComite && filtro.resueltoMiembro != TodosOpcion);
            parametros.Add("resueltoMiembro", filtro.resueltoMiembro);
            parametros.Add("aplicaExpediente", !string.IsNullOrEmpty(filtro.expediente));
            parametros.Add("expediente", filtro.expediente);
            parametros.Add("aplicaUsuario", !string.IsNullOrEmpty(filtro.usuario));
            parametros.Add("usuario", filtro.usuario);
            parametros.Add("aplicaGestion", filtro.gestionRegistrada != TodosOpcion);
            parametros.Add("gestionRegistrada", filtro.gestionRegistrada);
            parametros.Add("aplicaApelacion", filtro.apelacionRegistrada != TodosOpcion);
            parametros.Add("apelacionRegistrada", filtro.apelacionRegistrada);

            return parametros;
        }
    }
}
