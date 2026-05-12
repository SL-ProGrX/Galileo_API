using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaAcreedoresBL
    {
        private readonly FrmCrApaAcreedoresDB _db;

        public FrmCrApaAcreedoresBL(IConfiguration config)
        {
            _db = new FrmCrApaAcreedoresDB(config);
        }

        public ErrorDto<FrmCrApaAcreedoresGridLista> CR_APA_Acreedores_Obtener(
            int codEmpresa,
            string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro)
                          ?? new FiltrosLazyLoadData();

            return _db.CR_APA_Acreedores_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<FrmCrApaAcreedorDatosDto> CR_APA_Acreedor_Obtener(
    int codEmpresa,
    string cod_acreedor)
        {
            return _db.CR_APA_Acreedor_Obtener(codEmpresa, cod_acreedor);
        }

        public ErrorDto<int> CR_APA_Acreedor_Insertar(
    int codEmpresa,
    FrmCrApaAcreedorGuardarRequest request)
        {
            return _db.CR_APA_Acreedor_Insertar(codEmpresa, request);
        }

        public ErrorDto<int> CR_APA_Acreedor_Actualizar(
            int codEmpresa,
            FrmCrApaAcreedorGuardarRequest request)
        {
            return _db.CR_APA_Acreedor_Actualizar(codEmpresa, request);
        }

        public ErrorDto<List<FrmCrApaBancoDto>> CR_APA_Bancos_Obtener(int codEmpresa)
        {
            return _db.CR_APA_Bancos_Obtener(codEmpresa);
        }

        public ErrorDto<FrmCrApaBancoDto> CR_APA_Banco_Obtener(
            int codEmpresa,
            int id_banco)
        {
            return _db.CR_APA_Banco_Obtener(codEmpresa, id_banco);
        }

        public ErrorDto<FrmCrApaContactosListaDto> CR_APA_Contactos_Obtener(
    int codEmpresa,
    string cod_acreedor,
    string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro)
                          ?? new FiltrosLazyLoadData();

            return _db.CR_APA_Contactos_Obtener(codEmpresa, cod_acreedor, filtros);
        }

        public ErrorDto<int> CR_APA_Contacto_Guardar(
            int codEmpresa,
            FrmCrApaContactoGuardarRequest request)
        {
            return _db.CR_APA_Contacto_Guardar(codEmpresa, request);
        }

        public ErrorDto<int> CR_APA_Contacto_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string codigo)
        {
            return _db.CR_APA_Contacto_Eliminar(codEmpresa, cod_acreedor, codigo);
        }

        public ErrorDto<FrmCrApaAutorizadosListaDto> CR_APA_Autorizados_Obtener(
    int codEmpresa,
    string cod_acreedor,
    string filtro)
        {
            var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro)
                          ?? new FiltrosLazyLoadData();

            return _db.CR_APA_Autorizados_Obtener(codEmpresa, cod_acreedor, filtros);
        }

        public ErrorDto<int> CR_APA_Autorizado_Guardar(
            int codEmpresa,
            FrmCrApaAutorizadoGuardarRequest request)
        {
            return _db.CR_APA_Autorizado_Guardar(codEmpresa, request);
        }

        public ErrorDto<int> CR_APA_Autorizado_Eliminar(
            int codEmpresa,
            string cod_acreedor,
            string cedula)
        {
            return _db.CR_APA_Autorizado_Eliminar(codEmpresa, cod_acreedor, cedula);
        }
    }
}
