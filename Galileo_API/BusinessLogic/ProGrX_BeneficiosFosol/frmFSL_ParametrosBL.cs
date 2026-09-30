using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslParametrosBl
    {
        private readonly FrmFslParametrosDb _db;

        public FrmFslParametrosBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmFslParametrosDb(config);
        }

        public ErrorDto FSL_Parametros_Inicializar(
            int CodCliente)
        {
            return _db.FSL_Parametros_Inicializar(
                CodCliente);
        }

        public ErrorDto<FslParametrosListaDto>
            FSL_Parametros_Lista_Obtener(
                int CodCliente,
                string filtros)
        {
            return _db.FSL_Parametros_Lista_Obtener(
                CodCliente,
                filtros);
        }

        public ErrorDto FSL_Parametros_Actualizar(
            int CodCliente,
            FslParametroActualizarRequest request)
        {
            return _db.FSL_Parametros_Actualizar(
                CodCliente,
                request);
        }
    }
}