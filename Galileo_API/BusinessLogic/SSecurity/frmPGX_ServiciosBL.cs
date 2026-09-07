using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;


namespace Galileo.BusinessLogic
{
    public class FrmPgxServiciosBL
    {
        private readonly IConfiguration _config;

        public FrmPgxServiciosBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<ServicioSuscripcion>> Servicio_ObtenerTodos()
        {
            return new FrmPgxServiciosDB(_config).Servicio_ObtenerTodos();
        }

        public ErrorDto Servicio_Insertar(ServicioSuscripcion request)
        {
            return new FrmPgxServiciosDB(_config).Servicio_Insertar(request);
        }

        public ErrorDto Servicio_Eliminar(ServicioSuscripcion request)
        {
            return new FrmPgxServiciosDB(_config).Servicio_Eliminar(request);
        }

        public ErrorDto Servicio_Actualizar(ServicioSuscripcion request)
        {
            return new FrmPgxServiciosDB(_config).Servicio_Actualizar(request);
        }
    }
}
