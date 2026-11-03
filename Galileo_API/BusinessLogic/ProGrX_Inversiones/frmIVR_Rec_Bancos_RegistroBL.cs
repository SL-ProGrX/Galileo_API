using Galileo.DataBaseTier.ProGrX_Inversiones;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Extensions.Configuration;

namespace Galileo_API.BusinessLogic.ProGrX_Inversiones
{
    public sealed class FrmIvrRecBancosRegistroBl
    {
        private readonly FrmIvrRecBancosRegistroDb _db;

        public FrmIvrRecBancosRegistroBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmIvrRecBancosRegistroDb(config);
        }

        public ErrorDto<IvrRecBancosRegistroInicialData>
            IVR_Rec_Bancos_Registro_Inicializar(
                int codEmpresa,
                IvrRecBancosRegistroContextoRequest? request)
        {
            return _db.IVR_Rec_Bancos_Registro_Inicializar(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrRecBancosRegistroListaData>
            IVR_Rec_Bancos_Registro_Transacciones_Obtener(
                int codEmpresa,
                IvrRecBancosRegistroContextoRequest? request)
        {
            return _db.IVR_Rec_Bancos_Registro_Transacciones_Obtener(
                codEmpresa,
                request);
        }

        public ErrorDto<CtnxCuentasDto>
            IVR_Rec_Bancos_Registro_Cuenta_Obtener(
                int codEmpresa,
                IvrRecBancosRegistroCuentaRequest? request)
        {
            return _db.IVR_Rec_Bancos_Registro_Cuenta_Obtener(
                codEmpresa,
                request);
        }

        public ErrorDto<IvrRecBancosRegistroGuardarData>
            IVR_Rec_Bancos_Registro_Guardar(
                int codEmpresa,
                IvrRecBancosRegistroGuardarRequest? request)
        {
            return _db.IVR_Rec_Bancos_Registro_Guardar(
                codEmpresa,
                request);
        }

        public ErrorDto<int>
            IVR_Rec_Bancos_Registro_Eliminar(
                int codEmpresa,
                IvrRecBancosRegistroEliminarRequest? request)
        {
            return _db.IVR_Rec_Bancos_Registro_Eliminar(
                codEmpresa,
                request);
        }
    }
}
