using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    public partial class CoopeSanGabrielValidator
    {
        public ErrorDto fxTesEmisionSinpeTiempoReal(
            int codEmpresa,
            int nSolicitud,
            DateTime fecha,
            string usuario,
            int docBase,
            int contador)
        {
            Parametros parametros = new()
            {
                codEmpresa = codEmpresa,
                nSolicitud = nSolicitud,
                usuario = usuario,
                fecha = fecha
            };
            return null;
        }
    }
}
