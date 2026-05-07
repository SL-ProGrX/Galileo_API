using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmParametrosBl
    {
        private readonly FrmParametrosDb _db;

        public FrmParametrosBl(IConfiguration config)
            => _db = new FrmParametrosDb(config);

        public ErrorDto<ParametrosOtrosData?> Parametros_ObtenerOtros(int codEmpresa)
        {
            return _db.Parametros_ObtenerOtros(codEmpresa);
        }

        public ErrorDto Parametros_GuardarOtros(
            int codEmpresa, string usuario, ParametrosOtrosGuardarRequest request)
        {
            return _db.Parametros_GuardarOtros(codEmpresa, usuario, request);
        }

        public ErrorDto<List<ParametrosCodigoData>> Parametros_ObtenerCodigos(
            int codEmpresa, string garantia, string orden)
        {
            return _db.Parametros_ObtenerCodigos(codEmpresa, garantia, orden);
        }

        public ErrorDto Parametros_ActualizarCodigo(
            int codEmpresa, string usuario, ParametrosCodigoActualizarRequest request)
        {
            return _db.Parametros_ActualizarCodigo(codEmpresa, usuario, request);
        }

        public ErrorDto<List<ParametrosMembresiaData>> Parametros_ObtenerMembresias(
            int codEmpresa, string garantia)
        {
            return _db.Parametros_ObtenerMembresias(codEmpresa, garantia);
        }

        public ErrorDto Parametros_GuardarMembresias(
            int codEmpresa, string usuario, ParametrosMembresiasGuardarRequest request)
        {
            return _db.Parametros_GuardarMembresias(codEmpresa, usuario, request);
        }
    }
}