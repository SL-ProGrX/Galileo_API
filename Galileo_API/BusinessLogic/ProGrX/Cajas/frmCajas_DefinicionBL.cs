using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasDefinicionBL
    {
        private readonly FrmCajasDefinicionDB _db;

        public FrmCajasDefinicionBL(IConfiguration config)
        {
            _db = new FrmCajasDefinicionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> ObtenerOficinasActivas(int codEmpresa)
        {
            return _db.ObtenerOficinasActivas(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasDefinicion_Cajas_Obtener(int codEmpresa)
        {
            return _db.CajasDefinicion_Cajas_Obtener(codEmpresa);
        }

        public ErrorDto<CajasDefinicionDetalleModel?> CajasDefinicion_CajaDetalle_Obtener(int codEmpresa, string codCaja, string gEnlace)
        {
            return _db.CajasDefinicion_CajaDetalle_Obtener(codEmpresa, codCaja, gEnlace);
        }

        public ErrorDto<List<CajasDivisaPoliticaModel>> CajasDefinicion_DivisasPolitica_Obtener(int codEmpresa, string codCaja, string gEnlace)
        {
            return _db.CajasDefinicion_DivisasPolitica_Obtener(codEmpresa, codCaja, gEnlace);
        }

        public ErrorDto<List<CajasRecaudadorModel>> CajasDefinicion_Recaudadores_Obtener(int codEmpresa)
        {
            return _db.CajasDefinicion_Recaudadores_Obtener(codEmpresa);
        }

        public ErrorDto<List<CajasServicioAsignadoModel>> CajasDefinicion_ServiciosAsignados_Obtener(int codEmpresa, string codCaja, string codRecaudador)
        {
            return _db.CajasDefinicion_ServiciosAsignados_Obtener(codEmpresa, codCaja, codRecaudador);
        }

        public ErrorDto<bool> CajasDefinicion_ServicioAsignar_Insertar(int codEmpresa, CajasServicioAsignarParams param)
        {
            return _db.CajasDefinicion_ServicioAsignar_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDefinicion_ServicioAsignar_Eliminar(int codEmpresa, CajasServicioAsignarParams param)
        {
            return _db.CajasDefinicion_ServicioAsignar_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresCatalogo_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            return _db.CajasDefinicion_AuxiliaresCatalogo_Obtener(codEmpresa, param);
        }

        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresFondos_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            return _db.CajasDefinicion_AuxiliaresFondos_Obtener(codEmpresa, param);
        }

        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresCxc_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            return _db.CajasDefinicion_AuxiliaresCxc_Obtener(codEmpresa, param);
        }

        public ErrorDto<List<CajasAuxiliarAsignadoModel>> CajasDefinicion_AuxiliaresFfp_Obtener(int codEmpresa, CajasAuxiliarFiltroParams param)
        {
            return _db.CajasDefinicion_AuxiliaresFfp_Obtener(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDefinicion_AuxiliarAsignar_Insertar(int codEmpresa, string usuario, CajasAuxiliarAsignarParams param)
        {
            return _db.CajasDefinicion_AuxiliarAsignar_Insertar(codEmpresa, usuario, param);
        }

        public ErrorDto<bool> CajasDefinicion_AuxiliarAsignar_Eliminar(int codEmpresa, string usuario, CajasAuxiliarAsignarParams param)
        {
            return _db.CajasDefinicion_AuxiliarAsignar_Eliminar(codEmpresa, usuario, param);
        }

        public ErrorDto<List<CajasFormaPagoAsignadoModel>> CajasDefinicion_FormasPago_Obtener(int codEmpresa, string codCaja)
        {
            return _db.CajasDefinicion_FormasPago_Obtener(codEmpresa, codCaja);
        }

        public ErrorDto<bool> CajasDefinicion_FormaPagoAsignar_Insertar(int codEmpresa, CajasFormaPagoAsignarParams param)
        {
            return _db.CajasDefinicion_FormaPagoAsignar_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDefinicion_FormaPagoAsignar_Eliminar(int codEmpresa, CajasFormaPagoAsignarParams param)
        {
            return _db.CajasDefinicion_FormaPagoAsignar_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CajasDocumentoAsignadoModel>> CajasDefinicion_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            return _db.CajasDefinicion_Documentos_Obtener(codEmpresa, codCaja);
        }

        public ErrorDto<bool> CajasDefinicion_DocumentoAsignar_Insertar(int codEmpresa, CajasDocumentoAsignarParams param)
        {
            return _db.CajasDefinicion_DocumentoAsignar_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDefinicion_DocumentoAsignar_Eliminar(int codEmpresa, CajasDocumentoAsignarParams param)
        {
            return _db.CajasDefinicion_DocumentoAsignar_Eliminar(codEmpresa, param);
        }

        public ErrorDto<List<CajasUsuarioHistorialModel>> CajasDefinicion_UsuariosHistorial_Obtener(int codEmpresa, string codCaja)
        {
            return _db.CajasDefinicion_UsuariosHistorial_Obtener(codEmpresa, codCaja);
        }

        public ErrorDto<bool> CajasDefinicion_Caja_Insertar(int codEmpresa, CajasDefinicionInsertParams param)
        {
            return _db.CajasDefinicion_Caja_Insertar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDefinicion_Caja_Copiar(int codEmpresa, CajasDefinicionCopiaParams param)
        {
            return _db.CajasDefinicion_Caja_Copiar(codEmpresa, param);
        }

        public ErrorDto<bool> CajasDefinicion_Caja_Eliminar(int codEmpresa, string codCaja, string usuario)
        {
            return _db.CajasDefinicion_Caja_Eliminar(codEmpresa, codCaja, usuario);
        }
    }
}