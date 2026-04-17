using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOControlListaBL
    {
        private readonly FrmCOControlListaDB _db;

        public FrmCOControlListaBL(IConfiguration config)
        {
            _db = new FrmCOControlListaDB(config);
        }

        #region Principal
        public ErrorDto<CoControlListaBuscarResponse> CoControlLista_Buscar(
            int codEmpresa,
            string filtros
            )
        {
            CoControlListaBuscarRequest request = JsonConvert.DeserializeObject<CoControlListaBuscarRequest>(filtros) ?? new CoControlListaBuscarRequest();
            return _db.CoControlLista_Buscar(codEmpresa, request);
        }

        public ErrorDto<CoControlListaUsuarioScrollResponse> CoControlLista_UsuarioScroll_Obtener(
            int codEmpresa,
            string filtros)
        {


            CoControlListaUsuarioScrollRequest request = JsonConvert.DeserializeObject<CoControlListaUsuarioScrollRequest>(filtros) ?? new CoControlListaUsuarioScrollRequest();
            return _db.CoControlLista_UsuarioScroll_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<CoControlListaUsuarioBusquedaRow>> CoControlLista_Usuarios_Obtener(
                int codEmpresa,
                string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaUsuarioBusquedaRequest>(filtros)
                     ?? new CoControlListaUsuarioBusquedaRequest();

            return _db.CoControlLista_Usuarios_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Garantias_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Garantias_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Antiguedades_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Antiguedades_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Carteras_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Carteras_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Oficinas_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Oficinas_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Instituciones_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Instituciones_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Gestiones_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Gestiones_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Causas_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Causas_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Arreglos_Obtener(int codEmpresa)
        {
            return _db.CoControlLista_Arreglos_Obtener(codEmpresa);
        }

        public ErrorDto<List<CoControlListaPersonaBusquedaRow>> CoControlLista_Personas_Obtener(
            int codEmpresa,
            string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaPersonaBusquedaRequest>(filtros)
                   ?? new CoControlListaPersonaBusquedaRequest();

            return _db.CoControlLista_Personas_Obtener(codEmpresa, request);
        }

        public ErrorDto<int> CoControlLista_NotificarMarcados_Procesar(
             int codEmpresa,
             CoControlListaNotificarMarcadosRequest request)
        {
            return _db.CoControlLista_NotificarMarcados_Procesar(codEmpresa, request);
        }

        #endregion

        #region Operaciones

        public ErrorDto<CoControlListaOperacionesResponse> CoControlLista_Operaciones_Obtener(
                int codEmpresa,
                string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaOperacionesRequest>(filtros)
                     ?? new CoControlListaOperacionesRequest();

            return _db.CoControlLista_Operaciones_Obtener(codEmpresa, request);
        }

        #endregion

        #region Datos Persona

        public ErrorDto<CoControlListaDatosPersonalesResponse> CoControlLista_DatosPersonales_Obtener(
            int codEmpresa,
            string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaDatosPersonalesRequest>(filtros)
                   ?? new CoControlListaDatosPersonalesRequest();

            return _db.CoControlLista_DatosPersonales_Obtener(codEmpresa, request);
        }

        #endregion

        #region Gestiones
        public ErrorDto<CoControlListaGestionesResponse> Co_ControlLista_Gestiones_Consulta(
    int codEmpresa,
    string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaGestionesRequest>(filtros)
                     ?? new CoControlListaGestionesRequest();

            return _db.Co_ControlLista_Gestiones_Consulta(codEmpresa, request);
        }

        public ErrorDto<bool> CoControlLista_Notificacion_Procesar(
            int codEmpresa,
            CoControlListaNotificacionRequest request)
        {
            request ??= new CoControlListaNotificacionRequest();
            return _db.CoControlLista_Notificacion_Procesar(codEmpresa, request);
        }
        #endregion

        #region Fiadores

        public ErrorDto<List<CoControlListaFiadorRow>> CoControlLista_Fiadores_Obtener(
    int codEmpresa,
    string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaFiadoresRequest>(filtros)
                     ?? new CoControlListaFiadoresRequest();

            return _db.CoControlLista_Fiadores_Obtener(codEmpresa, request);
        }

        #endregion

        #region Traslados

        public ErrorDto<List<CoControlListaUsuarioBusquedaRow>> CoControlLista_UsuariosTraslado_Obtener(
            int codEmpresa,
            string request)
        {
            var filtros = JsonConvert.DeserializeObject<CoControlListaUsuarioBusquedaRequest>(request)
                     ?? new CoControlListaUsuarioBusquedaRequest();
            return _db.CoControlLista_UsuariosTraslado_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<bool> CoControlLista_AplicarMarcados_Procesar(
            int codEmpresa,
            CoControlListaAplicarMarcadosRequest request)
        {
            return _db.CoControlLista_AplicarMarcados_Procesar(codEmpresa, request);
        }

        public ErrorDto<bool> CoControlLista_TrasladarMarcados_Procesar(
            int codEmpresa,
            CoControlListaTrasladarMarcadosRequest request)
        {
            return _db.CoControlLista_TrasladarMarcados_Procesar(codEmpresa, request);
        }



        #endregion

        #region Gestiones Modal

        public ErrorDto<CoControlListaGestionActualResponse> CoControlLista_GestionActual_Obtener(
            int codEmpresa,
            string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaGestionActualRequest>(filtros)
                   ?? new CoControlListaGestionActualRequest();

            return _db.CoControlLista_GestionActual_Obtener(codEmpresa, request);
        }

        public ErrorDto<CoControlListaGestionDetalleResponse> CoControlLista_GestionDetalle_Obtener(
            int codEmpresa,
            string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaGestionDetalleRequest>(filtros)
                    ?? new CoControlListaGestionDetalleRequest();

            return _db.CoControlLista_GestionDetalle_Obtener(codEmpresa, request);
        }

        public ErrorDto<bool> CoControlLista_Gestion_Procesar(
            int codEmpresa,
            CoControlListaGestionProcesarRequest request)
        {
            request ??= new CoControlListaGestionProcesarRequest();
            return _db.CoControlLista_Gestion_Procesar(codEmpresa, request);
        }

        #endregion

        #region Cartera

        public ErrorDto<CoControlListaResumenCarteraUsuarioResponse> CoControlLista_ResumenCarteraUsuario_Obtener(
    int codEmpresa,
    string filtros)
        {
            var request = JsonConvert.DeserializeObject<CoControlListaResumenCarteraUsuarioRequest>(filtros)
                    ?? new CoControlListaResumenCarteraUsuarioRequest();

            return _db.CoControlLista_ResumenCarteraUsuario_Obtener(codEmpresa, request);
        }

        #endregion
    }
}
