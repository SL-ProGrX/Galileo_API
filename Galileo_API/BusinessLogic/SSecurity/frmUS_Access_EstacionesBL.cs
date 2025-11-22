using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class FrmUsAccessEstacionesBl
    {
        private readonly IConfiguration _config;

        public FrmUsAccessEstacionesBl(IConfiguration config)
        {
            _config = config;
        }

        public List<EstacionDto> EstacionesObtener(int codEmpresa)
        {
            return new FrmUsAccessEstacionesDb(_config).ObtenerEstacionesPorCliente(codEmpresa);
        }

        public ErrorDto EstacionRegistrar(EstacionGuardarDto request)
        {
            return new FrmUsAccessEstacionesDb(_config).EstacionRegistrar(request);
        }

        public List<EstacionSinVincularDto> EstacionesSinVincularObtener(int codEmpresa)
        {
            return new FrmUsAccessEstacionesDb(_config).EstacionesSinVincularObtener(codEmpresa);
        }

        public ErrorDto EstacionVincular(EstacionVinculaDto estacionDto)
        {
            return new FrmUsAccessEstacionesDb(_config).EstacionVincular(estacionDto);
        }

        public ErrorDto EstacionEliminar(EstacionEliminarDto estacionDto)
        {
            return new FrmUsAccessEstacionesDb(_config).EstacionEliminar(estacionDto);
        }
    }
}