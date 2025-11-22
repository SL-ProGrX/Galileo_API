using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsAccessHorariosBl
    {
        private readonly IConfiguration _config;

        public FrmUsAccessHorariosBl(IConfiguration config)
        {
            _config = config;
        }

        public List<HorarioDto> HorariosObtener(int empresaId)
        {
            return new FrmUsAccessHorariosDb(_config).ObtenerHorariosPorEmpresa(empresaId);
        }

        public ErrorDto HorarioRegistrar(HorarioDto request)
        {
            return new FrmUsAccessHorariosDb(_config).HorarioRegistrar(request);
        }

        public ErrorDto HorarioEliminar(HorarioDto request)
        {
            return new FrmUsAccessHorariosDb(_config).HorarioEliminar(request);
        }
    }
}
