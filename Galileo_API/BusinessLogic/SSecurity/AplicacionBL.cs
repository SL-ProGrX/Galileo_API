using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.BusinessLogic
{
    public class AplicacionBL
    {
        private readonly IConfiguration _config;

        public AplicacionBL(IConfiguration config)
        {
            _config = config;
        }

        #region MÉTODOS APP_BANK

        public List<Aplicacion> Aplicacion_ObtenerTodos()
        {
            return new AplicacionDB(_config).Aplicacion_ObtenerTodos();
        }

        public ErrorDto Aplicacion_Insertar(Aplicacion request)
        {
            return new AplicacionDB(_config).Aplicacion_Insertar(request);
        }

        public ErrorDto Aplicacion_Eliminar(Aplicacion request)
        {
            return new AplicacionDB(_config).Aplicacion_Eliminar(request);
        }

        public ErrorDto Aplicacion_Actualizar(Aplicacion request)
        {
            return new AplicacionDB(_config).Aplicacion_Actualizar(request);
        }

        #endregion


        #region MÉTODOS APP_BLOCK

        public List<Bloqueo> Bloqueo_ObtenerTodos(string Cod_App)
        {
            return new AplicacionDB(_config).Bloqueo_ObtenerTodos(Cod_App);
        }

        public ErrorDto Bloqueo_Insertar(Bloqueo request)
        {
            return new AplicacionDB(_config).Bloqueo_Insertar(request);
        }

        public ErrorDto Bloqueo_Eliminar(Bloqueo request)
        {
            return new AplicacionDB(_config).Bloqueo_Eliminar(request);
        }

        #endregion


        #region MÉTODOS APP_UPDATE

        public List<Actualizacion> Actualizacion_ObtenerTodos(string Cod_App)
        {
            return new AplicacionDB(_config).Actualizacion_ObtenerTodos(Cod_App);
        }

        public ErrorDto Actualizacion_Insertar(Actualizacion request)
        {
            return new AplicacionDB(_config).Actualizacion_Insertar(request);
        }

        public ErrorDto Actualizacion_Eliminar(Actualizacion request)
        {
            return new AplicacionDB(_config).Actualizacion_Eliminar(request);
        }

        #endregion 
    }
}
