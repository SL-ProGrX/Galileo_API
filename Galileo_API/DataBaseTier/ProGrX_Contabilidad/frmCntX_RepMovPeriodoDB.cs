using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepMovPeriodoDb
    {
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del reporte de movimientos del periodo.
        /// </summary>
        /// <param name="config">Configuración usada para resolver las conexiones por empresa.</param>
        public FrmCntXRepMovPeriodoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los periodos fiscales disponibles para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de periodos ordenada del más reciente al más antiguo.</returns>
        public ErrorDto<List<CntxRepMovPeriodoPeriodoDto>> CntX_PeriodosRepMov_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    id_cierre AS item,
                    RTRIM(descripcion) AS descripcion,
                    CAST(activo AS bit) AS activo
                FROM CntX_Cierres
                WHERE cod_contabilidad = @cod_contabilidad
                ORDER BY activo DESC, id_cierre DESC;";

            return DbHelper.ExecuteListQuery<CntxRepMovPeriodoPeriodoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Obtiene las unidades de negocio disponibles para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de unidades de negocio.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_UnidadesRepMov_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_unidad) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Unidades
                WHERE cod_contabilidad = @cod_contabilidad;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Obtiene los centros de costo de la contabilidad y, cuando aplica, de una unidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="unidad">Código de unidad o C para consolidado.</param>
        /// <returns>Lista de centros de costo disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostosRepMov_Listar(
            int codEmpresa,
            int codContabilidad,
            string unidad)
        {
            const string sql = @"
                SELECT
                    RTRIM(CC.cod_centro_costo) AS item,
                    RTRIM(CC.descripcion) AS descripcion
                FROM CntX_Centro_Costos CC
                WHERE CC.cod_contabilidad = @cod_contabilidad
                  AND (
                        @unidad = 'C'
                        OR EXISTS (
                            SELECT 1
                            FROM CntX_Unidades_CC UCC
                            WHERE UCC.cod_contabilidad = CC.cod_contabilidad
                              AND UCC.cod_centro_costo = CC.cod_centro_costo
                              AND UCC.cod_unidad = @unidad
                        )
                  );";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_contabilidad = codContabilidad,
                    unidad = string.IsNullOrWhiteSpace(unidad) ? "C" : unidad.Trim()
                });
        }

        /// <summary>
        /// Obtiene las áreas de trabajo disponibles para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de áreas de trabajo.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Areas_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    cod_area AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Area_Definicion
                WHERE cod_contabilidad = @cod_contabilidad;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Prepara, de forma transaccional, los datos consumidos por el informe solicitado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <returns>Resultado de la preparación de datos.</returns>
        public ErrorDto<bool> GenerarReporte(
            int codEmpresa,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => PrepararReporte(connection, codContabilidad, f));
        }

        /// <summary>
        /// Valida los filtros y coordina la rama de catálogo o resultados dentro de una transacción.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <returns>True cuando toda la preparación finaliza correctamente.</returns>
        private static bool PrepararReporte(
            SqlConnection connection,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            ValidarFiltros(f);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (f.reporte == "03")
                {
                    PrepararResultados(connection, transaction, codContabilidad, f);
                }
                else
                {
                    PrepararCatalogo(connection, transaction, codContabilidad, f);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Valida los valores mínimos y los códigos permitidos antes de modificar tablas de preparación.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarFiltros(CntxRepMovPeriodoFiltroDto f)
        {
            ArgumentNullException.ThrowIfNull(f);

            ValidarCamposRequeridos(f);
            ValidarCodigos(f);
            ValidarNivel(f);
        }

        /// <summary>
        /// Valida los campos obligatorios del formulario.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarCamposRequeridos(CntxRepMovPeriodoFiltroDto f)
        {
            if (string.IsNullOrWhiteSpace(f.usuario))
                throw new ArgumentException("Usuario es requerido.");

            if (!f.periodo.HasValue)
                throw new ArgumentException("Periodo es requerido.");

            if (f.tipo == "02" && string.IsNullOrWhiteSpace(f.area))
                throw new ArgumentException("Área de trabajo es requerida.");

            if (f.tipo == "02" && !int.TryParse(f.area, out _))
                throw new ArgumentException("Área de trabajo inválida.");
        }

        /// <summary>
        /// Valida los códigos cerrados enviados por la interfaz.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarCodigos(CntxRepMovPeriodoFiltroDto f)
        {
            if (f.tipo is not ("01" or "02"))
                throw new ArgumentException("Tipo de consulta inválido.");

            if (f.reporte is not ("01" or "02" or "03" or "04" or "05" or "06" or "07" or "08" or "09"))
                throw new ArgumentException("Tipo de reporte inválido.");

            if (f.mostrar is not ("A" or "N"))
                throw new ArgumentException("Forma de cálculo inválida.");
        }

        /// <summary>
        /// Valida el nivel según la rama de resultados o catálogo.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarNivel(CntxRepMovPeriodoFiltroDto f)
        {
            if (f.reporte == "03")
            {
                if (f.nivel is not ("Unidad" or "Centro"))
                    throw new ArgumentException("Nivel de resultados inválido.");

                return;
            }

            if (!int.TryParse(f.nivel, out var nivel) || nivel is < 1 or > 8)
                throw new ArgumentException("Nivel contable inválido.");
        }

        /// <summary>
        /// Prepara el catálogo con el procedimiento utilizado por el formulario VB6.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros validados del reporte.</param>
        private static void PrepararCatalogo(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            var parametros = CrearParametros(codContabilidad, f);
            parametros.Add("Reporte", f.reporte, DbType.String);

            connection.Execute(
                "dbo.spCntX_Movimientos_Periodo",
                parametros,
                transaction,
                commandTimeout: 0,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Prepara los resultados con el procedimiento utilizado por el formulario VB6.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros validados del reporte.</param>
        private static void PrepararResultados(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            var parametros = CrearParametros(codContabilidad, f);
            parametros.Add("Nivel", f.nivel, DbType.String);

            connection.Execute(
                "dbo.spCntX_Movimientos_Periodo_Resultados",
                parametros,
                transaction,
                commandTimeout: 0,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Construye los parámetros compartidos respetando los valores NULL enviados por VB6.
        /// </summary>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros validados del reporte.</param>
        /// <returns>Parámetros de la ejecución del procedimiento.</returns>
        private static DynamicParameters CrearParametros(
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            var parametros = new DynamicParameters();
            parametros.Add("Contabilidad", codContabilidad, DbType.Int32);
            parametros.Add("Periodo", f.periodo, DbType.Int32);
            parametros.Add("Tipo", f.tipo, DbType.String);
            parametros.Add("Usuario", f.usuario!.Trim(), DbType.String);
            parametros.Add("Area", f.tipo == "02" ? int.Parse(f.area!) : (int?)null, DbType.Int32);
            parametros.Add("Unidad", NormalizarOpcion(f.unidad, "C"), DbType.String);
            parametros.Add("CentroCosto", NormalizarOpcion(f.centroCosto, "T"), DbType.String);
            parametros.Add("Acumulados", f.mostrar == "A" ? 1 : 0, DbType.Int32);
            parametros.Add("DevolverDatos", 0, DbType.Int32);
            return parametros;
        }

        /// <summary>
        /// Convierte las opciones de consolidado y todos en parámetros SQL nulos.
        /// </summary>
        /// <param name="valor">Código recibido desde la pantalla.</param>
        /// <param name="opcionTodos">Código que representa la opción global.</param>
        /// <returns>Código específico o null para consultar todas las opciones.</returns>
        private static string? NormalizarOpcion(string? valor, string opcionTodos)
        {
            var normalizado = valor?.Trim();
            return string.IsNullOrEmpty(normalizado) || normalizado == opcionTodos
                ? null
                : normalizado;
        }
    }
}
