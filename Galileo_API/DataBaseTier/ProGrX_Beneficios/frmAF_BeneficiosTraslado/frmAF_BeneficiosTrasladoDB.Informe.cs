using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosTrasladoDB
    {
        /// <summary>
        /// Obtiene el informe superior de remesas con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de remesas y total.</returns>
        public ErrorDto<AfiBeneficiosRemesasDtoLista> AfiInformesTop_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiInformesTopFiltros>(filtros) ?? new AfiInformesTopFiltros();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneficiosRemesasDtoLista();
                var parametros = new DynamicParameters();

                var where = string.Empty;
                if (!string.IsNullOrWhiteSpace(filtro.filtro))
                {
                    where = @"WHERE cod_remesa LIKE '%' + @filtro + '%'
                                 OR usuario LIKE '%' + @filtro + '%'
                                 OR CONVERT(VARCHAR(19), fecha, 120) LIKE '%' + @filtro + '%'";
                    parametros.Add("filtro", filtro.filtro);
                }

                var countSql = $"SELECT COUNT(*) FROM AFI_BENEFICIOS_REMESAS {where}";
                response.Total = connection.QueryFirstOrDefault<int>(countSql, parametros);

                var paginado = string.Empty;
                if (filtro.pagina != null)
                {
                    paginado = " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY ";
                    parametros.Add("offset", filtro.pagina);
                    parametros.Add("fetch", filtro.paginacion);
                }

                var dataSql = $"SELECT * FROM AFI_BENEFICIOS_REMESAS {where} ORDER BY fecha DESC {paginado}";
                response.Beneficios = connection.Query<AfiBeneficiosRemesasDto>(dataSql, parametros).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene el cubo de beneficios (consulta detallada) por rango de fechas mediante SP.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="remesa">Parámetros del cubo (rango de fechas y detalle).</param>
        /// <returns>Lista de datos del cubo.</returns>
        public ErrorDto<List<CuboBeneficiosData>> Cubo_Beneficios_Obtener(int CodCliente, CuboParametros remesa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var fechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio, "yyyy-MM-dd") + " 00:00:00";
                var fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_corte, "yyyy-MM-dd") + " 23:59:59";

                return connection.Query<CuboBeneficiosData>(
                    "EXEC spAFI_Bene_Cubo_Consulta @fechaInicio, @fechaCorte, @detalle",
                    new { fechaInicio, fechaCorte, remesa.detalle }).ToList();
            });
        }
    }
}
