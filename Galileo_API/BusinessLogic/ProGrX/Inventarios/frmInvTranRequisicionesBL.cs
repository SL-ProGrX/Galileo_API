using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTranRequisicionesBL
    {
        private readonly FrmInvTranRequisicionesDB _db;
        public FrmInvTranRequisicionesBL(IConfiguration config)
        {
            _db = new FrmInvTranRequisicionesDB(config);
        }

        public ErrorDto<TranRequisicionData> InvTranRequisicion_Obtener(int CodEmpresa, int CodRequisicion)
        {
            return _db.InvTranRequisicion_Obtener(CodEmpresa, CodRequisicion);
        }

        public ErrorDto<List<InvReqProduc>> InvRequesicionProduc_Obtener(int CodEmpresa, int CodRequisicion)
        {
            return _db.InvRequesicionProduc_Obtener(CodEmpresa, CodRequisicion);
        }

        public ErrorDto<TranRequisicionData> InvTranRequisicion_scroll(int CodEmpresa, int scrollValue, int? CodRequisicion)
        {
            return _db.InvTranRequisicion_scroll(CodEmpresa, scrollValue, CodRequisicion);
        }

        public ErrorDto InvTranRequisicion_Insertar(int CodEmpresa, TranRequisicionData request)
        {
            return _db.InvTranRequisicion_Insertar(CodEmpresa, request);
        }

        public ErrorDto InvTranRequisicion_Actualizar(int CodEmpresa, TranRequisicionData request)
        {
            return _db.InvTranRequisicion_Actualizar(CodEmpresa, request);
        }

        public ErrorDto InvTranRequesicion_Eliminar(int CodEmpresa, int CodRequisicion)
        {
            return _db.InvTranRequesicion_Eliminar(CodEmpresa, CodRequisicion);
        }

        public ErrorDto InvRequesicionProduc_Insertar(int CodEmpresa, int CodRequisicion, List<InvReqProduc> producLineas)
        {
            return _db.InvRequesicionProduc_Insertar(CodEmpresa, CodRequisicion, producLineas);
        }

        public ErrorDto<List<TranRequisicionData>> InvTranPlantilla_Obtener(int CodEmpresa, int? CodRequisicion, string? GeneraUser, string? GeneraFecha)
        {
            return _db.InvTranPlantilla_Obtener(CodEmpresa, CodRequisicion, GeneraUser, GeneraFecha);
        }

        public ErrorDto<List<TranRequisicionData>> InvTranRequisiciones_Lista(int CodEmpresa, string usuario, string columna, string estado)
        {
            return _db.InvTranRequisiciones_Lista(CodEmpresa, usuario, columna, estado);
        }

        public ErrorDto InvRequisicionProduc_Eliminar(int CodEmpresa, int CodRequisicion, int Linea)
        {
            return _db.InvRequisicionProduc_Eliminar(CodEmpresa, CodRequisicion, Linea);
        }

        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            return _db.UENS_Obtener(CodEmpresa);
        }

        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuarioRecibeLista_Obtener(int CodEmpresa, string cod_unidad)
        {
            return _db.UsuarioRecibeLista_Obtener(CodEmpresa, cod_unidad);
        }

        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuariosActivoLista_Obtener(int CodEmpresa)
        {
            return _db.UsuariosActivoLista_Obtener(CodEmpresa);
        }

        public ErrorDto<InvRequesicionesActivosLista> ProductosRequesicionesActivo_Obtener(int CodEmpresa, string invReqFiltros)
        {
            return _db.ProductosRequesicionesActivo_Obtener(CodEmpresa, invReqFiltros);
        }

        public ErrorDto InvRequisicion_Autorizar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            return _db.InvRequisicion_Autorizar(CodEmpresa, CodRequisicion, Usuario, Estado);
        }

        public ErrorDto InvRequisicion_Procesar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            return _db.InvRequisicion_Procesar(CodEmpresa, CodRequisicion, Usuario, Estado);
        }

        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_unidad, string cod_proceso)
        {
            return _db.ValidaAutorizacion(CodEmpresa, usuario, cod_unidad, cod_proceso);
        }

        public ErrorDto<List<string>> ObtenerUsuario(int CodEmpresa)
        {
            return _db.ObtenerUsuario(CodEmpresa);
        }
    }
}