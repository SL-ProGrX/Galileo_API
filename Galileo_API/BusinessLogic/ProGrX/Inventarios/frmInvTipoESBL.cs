using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvTipoEsBl
    {
        private const int CodigoValidacion = -2;

        private readonly FrmInvTipoEsDb _db;

        public FrmInvTipoEsBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmInvTipoEsDb(config);
        }

        public ErrorDto<TipoESList> INV_TipoES_Lista_Obtener(
            int CodEmpresa,
            int CodContabilidad,
            string? filtros)
        {
            var resultadoVacio = new TipoESList
            {
                total = 0,
                lista = []
            };

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return new ErrorDto<TipoESList>
                {
                    Code = CodigoValidacion,
                    Description =
                        "Los filtros de consulta son requeridos.",
                    Result = resultadoVacio
                };
            }

            try
            {
                var filtrosDeserializados =
                    JsonConvert.DeserializeObject<TipoESFiltros>(
                        filtros);

                if (filtrosDeserializados is null)
                {
                    return new ErrorDto<TipoESList>
                    {
                        Code = CodigoValidacion,
                        Description =
                            "Los filtros de consulta no son v&aacute;lidos.",
                        Result = resultadoVacio
                    };
                }

                return _db.INV_TipoES_Lista_Obtener(
                    CodEmpresa,
                    CodContabilidad,
                    filtrosDeserializados);
            }
            catch (JsonException)
            {
                return new ErrorDto<TipoESList>
                {
                    Code = CodigoValidacion,
                    Description =
                        "El formato de los filtros de consulta no es v&aacute;lido.",
                    Result = resultadoVacio
                };
            }
        }

        public ErrorDto<List<TipoEsDto>> INV_TipoES_Tipo_Buscar(
            int CodEmpresa,
            int CodContabilidad,
            string? tipo)
        {
            return _db.INV_TipoES_Tipo_Buscar(
                CodEmpresa,
                CodContabilidad,
                tipo);
        }

        public ErrorDto INV_TipoES_Registrar(
            int CodEmpresa,
            TipoEsGuardarRequest? request)
        {
            return _db.INV_TipoES_Registrar(
                CodEmpresa,
                request);
        }

        public ErrorDto INV_TipoES_Actualizar(
            int CodEmpresa,
            TipoEsGuardarRequest? request)
        {
            return _db.INV_TipoES_Actualizar(
                CodEmpresa,
                request);
        }
        public ErrorDto INV_TipoES_Eliminar(
            int CodEmpresa,
            TipoEsEliminarRequest? request)
        {
            return _db.INV_TipoES_Eliminar(
                CodEmpresa,
                request);
        }
    }
}