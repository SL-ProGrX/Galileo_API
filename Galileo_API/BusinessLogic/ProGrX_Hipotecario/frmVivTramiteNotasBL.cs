using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivTramiteNotasBl
    {
        private readonly FrmVivTramiteNotasDb _db;

        public FrmVivTramiteNotasBl(IConfiguration config)
            => _db = new FrmVivTramiteNotasDb(config);

        public ErrorDto<VivTramiteNotaOperacionData?> VivTramiteNotas_ObtenerInformacionOperacion(
            int codEmpresa, string numeroOperacion, int idGarantia)
        {
            return _db.VivTramiteNotas_ObtenerInformacionOperacion(codEmpresa, numeroOperacion, idGarantia);
        }

        public ErrorDto<List<VivTramiteNotaData>> VivTramiteNotas_ObtenerLista(
            int codEmpresa, int idGarantia, string profesional)
        {
            return _db.VivTramiteNotas_ObtenerLista(codEmpresa, idGarantia, profesional);
        }

        public ErrorDto VivTramiteNotas_Guardar(
            int codEmpresa, string usuario, VivTramiteNotaGuardarRequest request)
        {
            return _db.VivTramiteNotas_Guardar(codEmpresa, usuario, request);
        }
    }
}
