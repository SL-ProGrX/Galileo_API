using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public sealed class FrmCrExcedenteDatosBl
    {
        private readonly FrmCrExcedenteDatosDb _db;

        public FrmCrExcedenteDatosBl(
            IConfiguration config)
        {
            _db = new FrmCrExcedenteDatosDb(config);
        }

        public ErrorDto<MCredito.CrExcedenteDisponibleData>
            Cr_ExcedenteDatos_Obtener(
                int codEmpresa,
                string cedula)
        {
            return _db.Cr_ExcedenteDatos_Obtener(
                codEmpresa,
                cedula);
        }
    }
}