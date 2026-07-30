using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Activos;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmArfMonitorDb
    {
        private readonly PortalDB _portalDB;

        public FrmArfMonitorDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Busca las operaciones del monitor de arrendamientos financieros.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        /// <returns>Operaciones que cumplen los filtros solicitados.</returns>
        public ErrorDto<List<ArfMonitorTablaDto>> Buscar(
            int codEmpresa,
            ArfMonitorFiltroDto filtros
        )
        {
            var sql = new StringBuilder();
            var where = new StringBuilder();

            ConstruirSelect(sql, filtros);
            ConstruirFiltros(where, filtros);

            if (where.Length > 0)
                sql.Append(" WHERE ").Append(where);

            return DbHelper.ExecuteListQuery<ArfMonitorTablaDto>(
                _portalDB,
                codEmpresa,
                sql.ToString(),
                ObtenerParametros(filtros)
            );
        }

        /// <summary>
        /// Define la vista de consulta según el tipo de fecha solicitado.
        /// </summary>
        /// <param name="sql">Constructor de la sentencia SQL.</param>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        private static void ConstruirSelect(
            StringBuilder sql,
            ArfMonitorFiltroDto filtros
        )
        {
            sql.Append(
                filtros.tipo_fecha == "Cierre"
                    ? "SELECT * FROM vARF_Cierre_Operacion_Consulta "
                    : "SELECT * FROM vARF_Operacion_Consulta "
            );
        }

        /// <summary>
        /// Construye los filtros SQL comunes y los específicos del tipo de fecha.
        /// </summary>
        /// <param name="where">Constructor de la cláusula WHERE.</param>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        private static void ConstruirFiltros(
            StringBuilder where,
            ArfMonitorFiltroDto filtros
        )
        {
            if (filtros.tipo_fecha == "Cierre")
                AgregarFiltro(where, "CORTE = @corte", filtros.corte);
            else
                AgregarFiltroFecha(where, filtros);

            AgregarFiltro(where, "COD_LOCAL = @cod_unidad", filtros.cod_unidad);
            AgregarFiltro(
                where,
                "COD_ACREEDOR = @cod_arrendador",
                filtros.cod_arrendador
            );
        }

        /// <summary>
        /// Agrega el rango de fechas cuando está habilitado para la consulta.
        /// </summary>
        /// <param name="where">Constructor de la cláusula WHERE.</param>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        private static void AgregarFiltroFecha(
            StringBuilder where,
            ArfMonitorFiltroDto filtros
        )
        {
            if (!filtros.fecha_inicio.HasValue || !filtros.fecha_corte.HasValue)
                return;

            if (filtros.usar_fechas == true)
                return;

            string campoFecha = ObtenerCampoFecha(filtros.tipo_fecha);
            AgregarCondicion(
                where,
                $"{campoFecha} BETWEEN @fechaInicio AND @fechaCorte"
            );
        }

        /// <summary>
        /// Obtiene el campo de fecha correspondiente a la selección del usuario.
        /// </summary>
        /// <param name="tipoFecha">Tipo de fecha seleccionado.</param>
        /// <returns>Nombre del campo SQL que se filtrará.</returns>
        private static string ObtenerCampoFecha(string? tipoFecha)
        {
            return tipoFecha switch
            {
                "Registro" => "REGISTRO_FECHA",
                "Activación" => "ACTIVA_FECHA",
                "Inicio" => "FECHA_INICIO",
                "Finaliza" => "FECHA_FINALIZA",
                _ => "ACTIVA_FECHA"
            };
        }

        /// <summary>
        /// Agrega una condición solamente cuando el valor del filtro está informado.
        /// </summary>
        /// <param name="where">Constructor de la cláusula WHERE.</param>
        /// <param name="condicion">Condición SQL parametrizada.</param>
        /// <param name="valor">Valor utilizado para determinar si aplica el filtro.</param>
        private static void AgregarFiltro(
            StringBuilder where,
            string condicion,
            string? valor
        )
        {
            if (string.IsNullOrWhiteSpace(valor))
                return;

            AgregarCondicion(where, condicion);
        }

        /// <summary>
        /// Agrega una condición a la cláusula WHERE respetando los conectores AND.
        /// </summary>
        /// <param name="where">Constructor de la cláusula WHERE.</param>
        /// <param name="condicion">Condición SQL que se agregará.</param>
        private static void AgregarCondicion(
            StringBuilder where,
            string condicion
        )
        {
            if (where.Length > 0)
                where.Append(" AND ");

            where.Append(condicion);
        }

        /// <summary>
        /// Construye los parámetros utilizados por la consulta principal.
        /// </summary>
        /// <param name="filtros">Filtros seleccionados en el monitor.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object ObtenerParametros(ArfMonitorFiltroDto filtros)
        {
            return new
            {
                filtros.cod_unidad,
                filtros.cod_arrendador,
                filtros.corte,
                fechaInicio = filtros.fecha_inicio?.Date,
                fechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddSeconds(-1)
            };
        }

        /// <summary>
        /// Busca las unidades disponibles para el filtro del monitor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de unidades disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(
            int codEmpresa
        )
        {
            const string sql = @"
                SELECT
                    RTRIM(COD_LOCAL) AS item,
                    descripcion
                FROM ARF_UNIDADES
                ORDER BY COD_LOCAL;
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql
            );
        }

        /// <summary>
        /// Busca los arrendadores disponibles para el filtro del monitor.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Lista de arrendadores disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Buscar(
            int codEmpresa
        )
        {
            const string sql = @"
                SELECT
                    RTRIM(COD_ACREEDOR) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM ARF_ACREEDORES
                ORDER BY COD_ACREEDOR;
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql
            );
        }

        /// <summary>
        /// Busca los cierres disponibles para consultar el auxiliar histórico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Fechas de cierre ordenadas de la más reciente a la más antigua.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cierres_Buscar(
            int codEmpresa
        )
        {
            const string sql = @"
                SELECT
                    CONVERT(varchar(19), Corte, 120) AS item,
                    CONVERT(varchar(10), Corte, 23) AS descripcion
                FROM ARF_CIERRES
                ORDER BY Corte DESC;
            ";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql
            );
        }
    }
}
