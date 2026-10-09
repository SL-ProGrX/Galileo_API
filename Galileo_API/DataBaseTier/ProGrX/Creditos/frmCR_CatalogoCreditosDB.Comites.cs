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
        /// Obtiene los comites de estudio de credito configurables por linea.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoCreditoComiteEstudioData>> CrCatalogoCreditos_ComitesEstudio_Obtener(int codEmpresa, string codigo)
        {
            codigo = codigo.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<CrCatalogoCreditoComiteEstudioData>>
                {
                    Code = -1,
                    Description = "Debe consultar una linea de credito."
                };
            }

            const string query = "EXEC spCRD_ComitesPreanalisis_Consulta @Codigo;";

            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
                connection.Query(query, new { Codigo = codigo })
                    .Select(MapearComiteEstudio)
                    .ToList());
        }

        private static CrCatalogoCreditoComiteEstudioData MapearComiteEstudio(dynamic fila)
        {
            IDictionary<string, object> valores = (IDictionary<string, object>)fila;
            return new CrCatalogoCreditoComiteEstudioData
            {
                id = ValorComiteEntero(valores, 0, "id"),
                linea = ValorComiteTexto(valores, 1, "linea", "codigo", "cod_linea"),
                id_comite = ValorComiteEntero(valores, 2, "id_comite", "idcomite"),
                comite = ValorComiteTexto(valores, 3, "comite", "descripcion"),
                porcentaje = ValorComiteDecimal(valores, 4, "porcentaje", "porc_extras")
            };
        }

        private static object? ValorComite(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
        {
            foreach (string nombre in alias)
            {
                KeyValuePair<string, object> valor = fila.FirstOrDefault(item =>
                    string.Equals(item.Key, nombre, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(valor.Key))
                {
                    return valor.Value is DBNull ? null : valor.Value;
                }
            }

            object? valorPosicional = fila.Values.ElementAtOrDefault(posicion);
            return valorPosicional is DBNull ? null : valorPosicional;
        }

        private static string ValorComiteTexto(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
            => Convert.ToString(
                ValorComite(fila, posicion, alias),
                CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;

        private static int ValorComiteEntero(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
        {
            object? valor = ValorComite(fila, posicion, alias);
            return valor is null ? 0 : Convert.ToInt32(valor, CultureInfo.InvariantCulture);
        }

        private static decimal ValorComiteDecimal(
            IDictionary<string, object> fila,
            int posicion,
            params string[] alias)
        {
            object? valor = ValorComite(fila, posicion, alias);
            return valor is null ? 0 : Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Guarda el porcentaje de extras por comite para estudio de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int> CrCatalogoCreditos_ComiteEstudio_Guardar(int codEmpresa, CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            NormalizarComiteEstudioRequest(request);
            if (string.IsNullOrWhiteSpace(request.codigo) ||
                request.comite.id_comite <= 0 ||
                string.IsNullOrWhiteSpace(request.comite.comite))
            {
                return new ErrorDto<int>
                {
                    Code = -1,
                    Description = "Debe indicar la linea y el comite."
                };
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                connection.Execute(
                    "UPDATE comites SET descripcion = @Descripcion WHERE id_comite = @IdComite;",
                    new
                    {
                        Descripcion = request.comite.comite,
                        IdComite = request.comite.id_comite
                    },
                    transaction);

                int id = connection.QueryFirstOrDefault<int>(
                    "EXEC spCrd_ComitesPreanalisis_Add @Id, @Codigo, @IdComite, @Porcentaje, @Usuario;",
                    new
                    {
                        Id = request.comite.id,
                        Codigo = request.codigo,
                        IdComite = request.comite.id_comite,
                        Porcentaje = request.comite.porcentaje,
                        Usuario = request.usuario
                    },
                    transaction);

                transaction.Commit();

                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.comite.id == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Config: Porc. Extras [Linea: {request.codigo}, Id Reg: {id}...Comite: {request.comite.comite}] Porc: {request.comite.porcentaje:N2}");

                return new ErrorDto<int>
                {
                    Code = 0,
                    Result = id
                };
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
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

            const string query = @"
                DELETE FROM CRD_ComitesPreanalisis
                WHERE id = @Id;";

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
    }
}
