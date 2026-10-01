using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslComiteBL
    {
        private const int CodigoValidacion = -2;

        private readonly FrmFslComiteDB _db;

        public FrmFslComiteBL(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslComiteDB(config);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslComiteDto>>
            FSL_Comite_Comites_Obtener(
        int CodEmpresa,
        string filtros)
        {
            if (!FSL_Comite_Filtros_Deserializar(
                filtros,
                out FslComitePaginacionFiltros request))
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslComiteDto>());
            }

            return _db.FSL_Comite_Comites_Obtener(
                CodEmpresa,
                request);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Comite_ComitesActivos_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_Comite_ComitesActivos_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslComiteMiembroDto>>
            FSL_Comite_Miembros_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (!FSL_Comite_Filtros_Deserializar(
                filtros,
                out FslComiteMiembrosFiltros request))
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslComiteMiembroDto>());
            }

            return _db.FSL_Comite_Miembros_Obtener(
                CodEmpresa,
                request);
        }

        public ErrorDto FSL_Comite_Comite_Registrar(
            int CodEmpresa,
            FslComiteGuardarRequest request)
        {
            return _db.FSL_Comite_Comite_Registrar(
                CodEmpresa,
                request);
        }

        public ErrorDto FSL_Comite_Comite_Actualizar(
            int CodEmpresa,
            FslComiteGuardarRequest request)
        {
            return _db.FSL_Comite_Comite_Actualizar(
                CodEmpresa,
                request);
        }

        public ErrorDto FSL_Comite_Comite_Eliminar(
            int CodEmpresa,
            string codComite,
            string usuario)
        {
            return _db.FSL_Comite_Comite_Eliminar(
                CodEmpresa,
                codComite,
                usuario);
        }

        public ErrorDto FSL_Comite_Miembro_Registrar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request)
        {
            return _db.FSL_Comite_Miembro_Registrar(
                CodEmpresa,
                request);
        }

        public ErrorDto FSL_Comite_Miembro_Actualizar(
            int CodEmpresa,
            FslComiteMiembroGuardarRequest request)
        {
            return _db.FSL_Comite_Miembro_Actualizar(
                CodEmpresa,
                request);
        }

        public ErrorDto FSL_Comite_Miembro_Eliminar(
            int CodEmpresa,
            string codComite,
            string cedula,
            string usuario)
        {
            return _db.FSL_Comite_Miembro_Eliminar(
                CodEmpresa,
                codComite,
                cedula,
                usuario);
        }

        private static bool
            FSL_Comite_Filtros_Deserializar<T>(
                string filtros,
                out T request)
            where T : new()
        {
            request = new T();

            if (string.IsNullOrWhiteSpace(
                filtros))
            {
                return true;
            }

            try
            {
                request =
                    JsonConvert.DeserializeObject<T>(
                        filtros) ??
                    new T();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}