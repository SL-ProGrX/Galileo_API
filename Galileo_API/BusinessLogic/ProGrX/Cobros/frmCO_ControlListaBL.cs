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
    }
}
