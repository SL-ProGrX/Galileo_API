using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvUnidadesBl
    {
        private const int CodigoValidacion = -2;

        private readonly FrmInvUnidadesDb _db;

        public FrmInvUnidadesBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmInvUnidadesDb(config);
        }

        public ErrorDto<UnidadesDataLista> INV_Unidades_Lista_Obtener(
            int CodEmpresa,
            string? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return INV_Unidades_Lista_Error(
                    "Los filtros de consulta son requeridos.");
            }

            try
            {
                var filtrosDeserializados =
                    JsonConvert.DeserializeObject<FiltrosLazyLoadData>(
                        filtros);

                return filtrosDeserializados is null
                    ? INV_Unidades_Lista_Error(
                        "Los filtros de consulta no son v&aacute;lidos.")
                    : _db.INV_Unidades_Lista_Obtener(
                        CodEmpresa,
                        filtrosDeserializados);
            }
            catch (JsonException)
            {
                return INV_Unidades_Lista_Error(
                    "El formato de los filtros de consulta no es v&aacute;lido.");
            }
        }

        public ErrorDto<List<UnidadMedicionDto>>
            INV_Unidades_Detalle_Obtener(int CodEmpresa)
        {
            return _db.INV_Unidades_Detalle_Obtener(CodEmpresa);
        }

        public ErrorDto<List<UnidadMedicion>>
            INV_Unidades_Catalogo_Obtener(int CodEmpresa)
        {
            return _db.INV_Unidades_Catalogo_Obtener(CodEmpresa);
        }

        public ErrorDto INV_Unidades_Registrar(
            int CodEmpresa,
            UnidadMedicionDto? request)
        {
            return _db.INV_Unidades_Registrar(
                CodEmpresa,
                request);
        }

        public ErrorDto INV_Unidades_Actualizar(
            int CodEmpresa,
            UnidadMedicionDto? request)
        {
            return _db.INV_Unidades_Actualizar(
                CodEmpresa,
                request);
        }

        public ErrorDto INV_Unidades_Eliminar(
            int CodEmpresa,
            string? unidad,
            string? usuario)
        {
            return _db.INV_Unidades_Eliminar(
                CodEmpresa,
                unidad,
                usuario);
        }

        private static ErrorDto<UnidadesDataLista>
            INV_Unidades_Lista_Error(string mensaje)
        {
            return new ErrorDto<UnidadesDataLista>
            {
                Code = CodigoValidacion,
                Description = mensaje,
                Result = new UnidadesDataLista
                {
                    total = 0,
                    unidades = []
                }
            };
        }
    }
}