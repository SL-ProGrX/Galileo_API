using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public sealed class FrmCrOperacionCtaBulletBl
    {
        private readonly FrmCrOperacionCtaBulletDb _db;

        public FrmCrOperacionCtaBulletBl(
            IConfiguration config)
        {
            _db =
                new FrmCrOperacionCtaBulletDb(
                    config);
        }

        public ErrorDto<CrOperacionCtaBulletData>
            CrOperacionCtaBullet_Operacion_Obtener(
                int codEmpresa,
                int operacion)
        {
            return _db
                .CrOperacionCtaBullet_Operacion_Obtener(
                    codEmpresa,
                    operacion);
        }

        public ErrorDto
            CrOperacionCtaBullet_Guardar(
                int codEmpresa,
                CrOperacionCtaBulletGuardarRequest request)
        {
            return _db
                .CrOperacionCtaBullet_Guardar(
                    codEmpresa,
                    request);
        }
    }
}