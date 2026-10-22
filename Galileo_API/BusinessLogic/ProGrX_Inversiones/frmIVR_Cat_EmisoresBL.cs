using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrCatEmisoresBl
    {
        private readonly FrmIvrCatEmisoresDb _db;

        public FrmIvrCatEmisoresBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmIvrCatEmisoresDb(config);
        }

        public ErrorDto<List<IvrCatEmisorDto>> IVR_Cat_Emisores_Lista_Obtener(
            int codEmpresa)
        {
            return _db.IVR_Cat_Emisores_Lista_Obtener(codEmpresa);
        }

        public ErrorDto<List<IvrCatEmisorSectorDto>> IVR_Cat_Emisores_Sectores_Obtener(
            int codEmpresa)
        {
            return _db.IVR_Cat_Emisores_Sectores_Obtener(codEmpresa);
        }

        public ErrorDto IVR_Cat_Emisores_Registrar(
            int codEmpresa,
            IvrCatEmisorGuardarRequest? request)
        {
            return _db.IVR_Cat_Emisores_Registrar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Emisores_Actualizar(
            int codEmpresa,
            IvrCatEmisorGuardarRequest? request)
        {
            return _db.IVR_Cat_Emisores_Actualizar(codEmpresa, request);
        }

        public ErrorDto IVR_Cat_Emisores_Eliminar(
            int codEmpresa,
            string? codEmisor,
            string? usuario)
        {
            return _db.IVR_Cat_Emisores_Eliminar(codEmpresa, codEmisor, usuario);
        }
    }
}
