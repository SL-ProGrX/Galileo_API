using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

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

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoComiteEstudioData>(
                _portalDb,
                codEmpresa,
                query,
                new { Codigo = codigo });
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
            if (string.IsNullOrWhiteSpace(request.codigo) || request.comite.id_comite <= 0)
            {
                return new ErrorDto<int>
                {
                    Code = -1,
                    Description = "Debe indicar la linea y el comite."
                };
            }

            const string query = "EXEC spCrd_ComitesPreanalisis_Add @Id, @Codigo, @IdComite, @Porcentaje, @Usuario;";

            var respuesta = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                query,
                request.comite.id,
                new
                {
                    Id = request.comite.id,
                    Codigo = request.codigo,
                    IdComite = request.comite.id_comite,
                    Porcentaje = request.comite.porcentaje,
                    Usuario = request.usuario
                });

            if (respuesta.Code >= 0)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    request.comite.id == 0 ? "Registra - WEB" : "Modifica - WEB",
                    $"Config: Porc. Extras [Linea: {request.codigo}, Id Reg: {respuesta.Result}...Comite: {request.comite.comite}] Porc: {request.comite.porcentaje:N2}");
            }

            return respuesta;
        }
    }
}
