using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesAccesosBL
    {

        private readonly FrmTesAccesosDB _AccesosDb;

        public FrmTesAccesosBL(IConfiguration config)
        {
            _AccesosDb = new FrmTesAccesosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosBancos_Obtener(int CodEmpresa)
        {
            return _AccesosDb.Tes_AccesosBancos_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Tes_AccesosCuentas_Obtener(int CodEmpresa, string cod_banco)
        {
            return _AccesosDb.Tes_AccesosCuentas_Obtener(CodEmpresa, cod_banco);
        }

        public ErrorDto<TesAccesosUsuariosLista> Tes_AccesosUsuarioBuscar_Obtener(int CodEmpresa, string filtro)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro) ?? new FiltrosLazyLoadData();
            return _AccesosDb.Tes_AccesosUsuarioBuscar_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto<DropDownListaGenericaModel> Tes_AccesosUsuarioBuscar_scroll(int CodEmpresa, string nombre, int? scroll)
        {
            return _AccesosDb.Tes_AccesosUsuarioBuscar_scroll(CodEmpresa, nombre, scroll);
        }

        #region Cuentas

        public ErrorDto<List<TesAccesosUsuariosData>> Tes_AccesosUsuarios_Obtener(int CodEmpresa, int cod_banco)
        {
            return _AccesosDb.Tes_AccesosUsuarios_Obtener(CodEmpresa, cod_banco);
        }

        public ErrorDto Tes_AccesosCuentas_Asignar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosDb.Tes_AccesosCuentas_Asignar(CodEmpresa, id_banco, nombre);
        }

        public ErrorDto Tes_AccesosCuentas_Eliminar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosDb.Tes_AccesosCuentas_Eliminar(CodEmpresa, id_banco, nombre);
        }

        #endregion

        #region Usuarios
        public ErrorDto<List<TesAccesosBancosData>> Tes_AccesosUserBancos_Obtener(int CodEmpresa, string nombre, string cod_grupo)
        {
            return _AccesosDb.Tes_AccesosUserBancos_Obtener(CodEmpresa, nombre, cod_grupo);
        }

        public ErrorDto Tes_AccesosUsuarios_Asignar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosDb.Tes_AccesosUsuarios_Asignar(CodEmpresa, id_banco, nombre);
        }

        public ErrorDto Tes_AccesosUsuarios_Eliminar(int CodEmpresa, int id_banco, string nombre)
        {
            return _AccesosDb.Tes_AccesosUsuarios_Eliminar(CodEmpresa, id_banco, nombre);
        }


        #endregion

        #region Accesos

        public ErrorDto<List<TesAccesosBancosData>> Tes_AccesosBancoUser_Obtener(int CodEmpresa, string nombre)
        {
            return _AccesosDb.Tes_AccesosBancoUser_Obtener(CodEmpresa, nombre);
        }

        public ErrorDto<List<TesAccesosDocumentosData>> Tes_AccesosDocumentos_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            return _AccesosDb.Tes_AccesosDocumentos_Obtener(CodEmpresa, usuario, id_banco);
        }

        public ErrorDto<List<TesAccesosConceptosData>> Tes_AccesosConceptos_Obtener(int CodEmpresa, string usuario, int id_banco)
        {
            return _AccesosDb.Tes_AccesosConceptos_Obtener(CodEmpresa, usuario, id_banco);
        }

        public ErrorDto<List<TesAccesosUnidadesData>> Tes_AccesosUnidades_Obtener(int CodEmpresa, string usuario, int id_banco, int contabilidad)
        {
            return _AccesosDb.Tes_AccesosUnidades_Obtener(CodEmpresa, usuario, id_banco, contabilidad);
        }

        public ErrorDto<TesAccesosFirmasData> Tes_AccesosFirmas_Obtener(int CodEmpresa, int id_banco, string usuario)
        {
            return _AccesosDb.Tes_AccesosFirmas_Obtener(CodEmpresa, id_banco, usuario);
        }

        public ErrorDto Tes_AccesosDocumentos_Guardar(int CodEmpresa, string usuario, int id_banco, TesAccesosDocumentosData documento)
        {
            return _AccesosDb.Tes_AccesosDocumentos_Guardar(CodEmpresa, usuario, id_banco, documento);
        }

        public ErrorDto Tes_AccesosConceptos_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked, TesAccesosConceptosData concepto)
        {
            return _AccesosDb.Tes_AccesosConceptos_Guardar(CodEmpresa, usuario, id_banco, itemChecked, concepto);
        }

        public ErrorDto Tes_AccesosUnidades_Guardar(int CodEmpresa, string usuario, int id_banco, bool itemChecked, TesAccesosUnidadesData unidad)
        {
            return _AccesosDb.Tes_AccesosUnidades_Guardar(CodEmpresa, usuario, id_banco, itemChecked, unidad);
        }

        public ErrorDto Tes_AccesosFirmas_Guardar(int CodEmpresa, TesAccesosFirmasData firmas)
        {
            return _AccesosDb.Tes_AccesosFirmas_Guardar(CodEmpresa, firmas);
        }

        #endregion

        #region Copia

        public ErrorDto Tes_AccesosUsuarios_Copiar(int CodEmpresa, string usuarioOrigen, string usuarioDestino)
        {
            return _AccesosDb.Tes_AccesosUsuarios_Copiar(CodEmpresa, usuarioOrigen, usuarioDestino);
        }

        public ErrorDto Tes_AccesosUsuarios_EliminarInactivos(int CodEmpresa)
        {
            return _AccesosDb.Tes_AccesosUsuarios_EliminarInactivos(CodEmpresa);
        }

        #endregion
    }
}
