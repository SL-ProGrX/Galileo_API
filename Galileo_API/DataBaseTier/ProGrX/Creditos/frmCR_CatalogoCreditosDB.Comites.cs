using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {
        /// <summary>
        /// Obtiene los comités de estudio de crédito configurables por línea.
        /// </summary>
        public ErrorDto<List<CrCatalogoCreditoComiteEstudioData>> CrCatalogoCreditos_ComitesEstudio_Obtener(
                int codEmpresa,
                string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe consultar una línea de crédito.",
                    -1,
                    new List<CrCatalogoCreditoComiteEstudioData>());
            }

            const string query =
                "EXEC spCRD_ComitesPreanalisis_Consulta @Codigo;";

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection
                    .Query(query, new { Codigo = codigo })
                    .Select(MapearComiteEstudio)
                    .ToList());
        }

        private static CrCatalogoCreditoComiteEstudioData
            MapearComiteEstudio(dynamic fila)
        {
            IDictionary<string, object> valores =
                (IDictionary<string, object>)fila;

            return new CrCatalogoCreditoComiteEstudioData
            {
                id = ValorComiteEntero(valores, 0, "id"),
                linea = ValorComiteTexto(
                    valores, 1, "linea", "codigo", "cod_linea"),
                id_comite = ValorComiteEntero(
                    valores, 2, "id_comite", "idcomite"),
                comite = ValorComiteTexto(
                    valores, 3, "comite", "descripcion"),
                porcentaje = ValorComiteDecimal(
                    valores, 4, "porcentaje", "porc_extras")
            };
        }

        private static object? ValorComite(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
        {
            foreach (string nombre in alias)
            {
                KeyValuePair<string, object> valor =
                    fila.FirstOrDefault(item =>
                        string.Equals(
                            item.Key,
                            nombre,
                            StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(valor.Key))
                {
                    return valor.Value is DBNull
                        ? null
                        : valor.Value;
                }
            }

            object? valorPosicional =
                fila.Values.ElementAtOrDefault(posicion);

            return valorPosicional is DBNull
                ? null
                : valorPosicional;
        }

        private static string ValorComiteTexto(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
            => Convert.ToString(
                ValorComite(fila, posicion, alias),
                CultureInfo.InvariantCulture)?.Trim()
               ?? string.Empty;

        private static int ValorComiteEntero(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
        {
            object? valor = ValorComite(fila, posicion, alias);

            return valor is null
                ? 0
                : Convert.ToInt32(
                    valor,
                    CultureInfo.InvariantCulture);
        }

        private static decimal ValorComiteDecimal(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
        {
            object? valor = ValorComite(fila, posicion, alias);

            return valor is null
                ? 0
                : Convert.ToDecimal(
                    valor,
                    CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Actualiza la descripción del comité y guarda su porcentaje
        /// mediante spCrd_ComitesPreanalisis_Add, sin transacción externa.
        /// </summary>
        public ErrorDto<int> CrCatalogoCreditos_ComiteEstudio_Guardar(
            int codEmpresa,
            CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            if (!CrCatalogoCreditos_ComiteEstudio_SolicitudValida(request))
            {
                return DbHelper.CreateErrorResponse<int>(
                    "Debe indicar la línea y el comité.");
            }

            bool procedimientoIniciado = false;
            int id;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                connection.Execute(
                    """
                    UPDATE comites
                    SET descripcion = @Descripcion
                    WHERE id_comite = @IdComite;
                    """,
                    new
                    {
                        Descripcion = request.comite.comite,
                        IdComite = request.comite.id_comite
                    });

                procedimientoIniciado = true;

                id = connection.QueryFirstOrDefault<int>(
                    """
                    EXEC spCrd_ComitesPreanalisis_Add
                         @Id,
                         @Codigo,
                         @IdComite,
                         @Porcentaje,
                         @Usuario;
                    """,
                    new
                    {
                        Id = request.comite.id,
                        Codigo = request.codigo,
                        IdComite = request.comite.id_comite,
                        Porcentaje = request.comite.porcentaje,
                        Usuario = request.usuario
                    });

                if (id <= 0)
                {
                    return DbHelper.CreateErrorResponse<int>(
                        "El procedimiento no devolvió un identificador válido. " +
                        "La descripción del comité pudo quedar actualizada. " +
                        "Revise el registro antes de reintentar.");
                }
            }
            catch (SqlException)
            {
                return DbHelper.CreateErrorResponse<int>(
                    procedimientoIniciado
                        ? "Falló el guardado del porcentaje. La descripción del " +
                          "comité pudo quedar actualizada. Revise ambos datos " +
                          "antes de reintentar."
                        : "Falló la actualización de la descripción del comité. " +
                          "Revise su estado antes de reintentar.");
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<int>(
                    procedimientoIniciado
                        ? "No se pudo completar el guardado. La descripción " +
                          "del comité pudo quedar actualizada. Revise ambos " +
                          "datos antes de reintentar."
                        : "No se pudo iniciar o completar la actualización " +
                          "del comité. Revise su estado antes de reintentar.");
            }

            try
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.comite.id == 0
                        ? "Registra - WEB"
                        : "Modifica - WEB",
                    $"Config: Porc. Extras [Linea: {request.codigo}, " +
                    $"Id Reg: {id}...Comite: {request.comite.comite}] " +
                    $"Porc: {request.comite.porcentaje:N2}");
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<int>(
                    "El comité y su porcentaje se guardaron con el " +
                    $"identificador {id}, pero falló el registro en bitácora. " +
                    "Verifique la bitácora antes de reintentar.",
                    -1,
                    id);
            }

            return DbHelper.CreateOkResponse(id);
        }

        /// <summary>
        /// Elimina la configuración de un comité para una línea de crédito.
        /// </summary>
        public ErrorDto CrCatalogoCreditos_ComiteEstudio_Eliminar(
            int codEmpresa,
            string codigo,
            int id,
            string usuario)
        {
            codigo = codigo.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(codigo) || id <= 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Debe indicar la línea y el comité."
                };
            }

            const string query = """
                DELETE FROM CRD_ComitesPreanalisis
                WHERE id = @Id;
                """;

            var respuesta = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new { Id = id });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    usuario,
                    "Elimina - WEB",
                    $"Config: Porc. Extras [Linea: {codigo}, Id Reg: {id}]");
            }

            return respuesta;
        }

        /// <summary>
/// Normaliza y valida los datos necesarios para guardar el comité.
/// </summary>
private static bool CrCatalogoCreditos_ComiteEstudio_SolicitudValida(
    CrCatalogoCreditoComiteEstudioGuardarRequest? request)
{
    if (request?.comite is null)
    {
        return false;
    }

    NormalizarComiteEstudioRequest(request);

    return !string.IsNullOrWhiteSpace(request.codigo)
        && request.comite.id_comite > 0
        && !string.IsNullOrWhiteSpace(request.comite.comite);
}
    }
}