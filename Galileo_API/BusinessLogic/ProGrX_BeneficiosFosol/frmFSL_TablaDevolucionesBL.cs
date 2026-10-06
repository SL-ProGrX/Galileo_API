using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTablaDevolucionesBl
    {
        private const int CodigoValidacion = -2;

        private readonly FrmFslTablaDevolucionesDb _db;

        public FrmFslTablaDevolucionesBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslTablaDevolucionesDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_TablaDevoluciones_Garantias_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_TablaDevoluciones_Garantias_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslTablaDevolucionDto>>
            FSL_TablaDevoluciones_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            try
            {
                var request =
                    DbHelper.DeserializeOrNew<
                        FslTablaDevolucionesFiltros>(
                            filtros);

                return _db
                    .FSL_TablaDevoluciones_Lista_Obtener(
                        CodEmpresa,
                        request);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTablaDevolucionDto>());
            }
        }

        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Registrar(
                int CodEmpresa,
                FslTablaDevolucionGuardarRequest request)
        {
            return _db
                .FSL_TablaDevoluciones_Devolucion_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Actualizar(
                int CodEmpresa,
                FslTablaDevolucionGuardarRequest request)
        {
            return _db
                .FSL_TablaDevoluciones_Devolucion_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Eliminar(
                int CodEmpresa,
                int codDevolucion,
                string usuario)
        {
            return _db
                .FSL_TablaDevoluciones_Devolucion_Eliminar(
                    CodEmpresa,
                    codDevolucion,
                    usuario);
        }
    }
}