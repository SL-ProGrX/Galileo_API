using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Conciliacion;

namespace Galileo_API.BusinessLogic.ProGrX_Conciliacion
{
    public sealed class FrmCierreMensualAseBl
    {
        private readonly FrmCierreMensualAseDb _db;

        public FrmCierreMensualAseBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmCierreMensualAseDb(config);
        }

        public ErrorDto
            Conciliacion_CierreMensualASE_Cierre_Ejecutar(
                int codEmpresa, string usuario)
        {
            return _db
                .Conciliacion_CierreMensualASE_Cierre_Ejecutar(
                    codEmpresa, usuario);
        }
    }
}