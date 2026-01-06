using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using Galileo.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasClaveBl(FrmCajasClaveDb dbfrmCajas_Clave)
    {
        private readonly FrmCajasClaveDb DbfrmCajas_Clave = dbfrmCajas_Clave;

        public FrmCajasClaveBl(IConfiguration config)
            : this(new FrmCajasClaveDb(config))
        {
        }

        public ErrorDto<List<CajasUsuarioDto>> Cajas_Usuario_Obtener(int codEmpresa, string usuario)
        {
            return DbfrmCajas_Clave.Cajas_Usuario_Obtener(codEmpresa, usuario
            );
        }

        public ErrorDto<bool> Cajas_Cambio_Clave(int codEmpresa, string usuario, string claveActual,
            string claveNueva, List<string> cajas)
        {
            return DbfrmCajas_Clave.Cajas_Cambio_Clave(codEmpresa, usuario, claveActual,
                claveNueva, cajas
            );
        }
    }
}