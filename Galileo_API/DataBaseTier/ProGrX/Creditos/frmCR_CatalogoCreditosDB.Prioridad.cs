using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {
        /// <summary>
        /// Obtiene las prioridades de deduccion por linea de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoPrioridadData>> CrCatalogoCreditos_Prioridad_Obtener(int codEmpresa)
        {
            const string query = @"
                SELECT
                    codigo,
                    ISNULL(descripcion, '') AS descripcion,
                    CONVERT(bit, ISNULL(linea_interna, 0)) AS linea_interna,
                    CONVERT(bit, CASE WHEN ISNULL(retencion, 'N') = 'N' AND ISNULL(poliza, 'N') = 'N' THEN 1 ELSE 0 END) AS libre,
                    CONVERT(bit, CASE WHEN ISNULL(convenio, 'N') = 'S' THEN 1 ELSE 0 END) AS convenio,
                    ISNULL(prioridad, 0) AS prioridad
                FROM Catalogo
                ORDER BY ISNULL(prioridad, 0), codigo;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoPrioridadData>(
                _portalDb,
                codEmpresa,
                query,
                new { });
        }

        /// <summary>
        /// Guarda la prioridad de deduccion de una linea de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoCreditos_Prioridad_Guardar(int codEmpresa, CrCatalogoCreditoPrioridadGuardarRequest request)
        {
            request.codigo = request.codigo?.Trim().ToUpperInvariant() ?? string.Empty;
            request.usuario = request.usuario?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.codigo))
                return new ErrorDto { Code = -1, Description = "Debe indicar la linea de credito." };

            if (request.prioridad < 0)
                return new ErrorDto { Code = -1, Description = "La prioridad no es valida." };

            const string query = @"
                UPDATE Catalogo
                SET prioridad = @prioridad
                WHERE codigo = @codigo;";

            var respuesta = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, query, request);
            if (respuesta.Code < 0)
                return respuesta;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Modifica - WEB",
                $"Prioridad deduccion x Linea: {request.codigo}");

            return respuesta;
        }
    }
}
