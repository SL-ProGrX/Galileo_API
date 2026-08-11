using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneFormulariosDB
    {
        /// <summary>
        /// Obtiene el reporte pivote de respuestas de un formulario mediante SP.
        /// </summary>
        /// <param name="datos">Datos del reporte (formulario, rango de fechas).</param>
        /// <returns>Resultado del reporte.</returns>
        public ErrorDto<object> AfBeneficiosReporte_Obtener(FrmReporteDatos datos)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), datos.codCliente, connection =>
                (object)connection.Query("spBene_W_FormRespuestasPivot", new
                {
                    ID_FORM = datos.id_frm,
                    FechaInicio = datos.fechaInicio,
                    FechaFin = datos.fechaFin
                }, commandType: CommandType.StoredProcedure).ToList());

            return result;
        }

        /// <summary>
        /// Obtiene el reporte de respuestas de un formulario por socio, completando datos faltantes.
        /// </summary>
        /// <param name="datos">Datos del reporte (formulario, cédula).</param>
        /// <returns>Lista de datos del reporte.</returns>
        public ErrorDto<List<ReporteFormularioDatos>> AfBeneficiosReporteSocio_Obtener(FrmReporteDatos datos)
        {
            return DbHelper.WithConn(CreatePortalDb(), datos.codCliente, connection =>
            {
                var lista = connection.Query<ReporteFormularioDatos>(
                    "EXEC spAFI_Bene_FormularioRepSocio @id_frm, @cedula",
                    new { datos.id_frm, cedula = datos.cedula?.Trim() }).ToList();

                CompletarDatosReporte(lista);
                return lista;
            });
        }

        /// <summary>
        /// Completa el beneficio, cédula y fecha en las filas que los tengan nulos (toma la primera fila válida).
        /// </summary>
        private static void CompletarDatosReporte(List<ReporteFormularioDatos> lista)
        {
            var primera = lista.FirstOrDefault();
            if (primera == null)
            {
                return;
            }

            foreach (var item in lista.Where(x => x.cod_beneficio == null))
            {
                item.cod_beneficio = primera.cod_beneficio;
                item.cedula = primera.cedula;
                item.registro_fecha = primera.registro_fecha;
            }
        }
    }
}
