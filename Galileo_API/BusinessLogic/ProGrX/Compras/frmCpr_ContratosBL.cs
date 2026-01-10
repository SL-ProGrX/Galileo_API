using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprContratosBL
    {
        readonly FrmCprContratosDB _db;

        public FrmCprContratosBL(IConfiguration config)
        {
            _db = new FrmCprContratosDB(config);
        }

        public ErrorDto<CprContratosDto> CprContrato_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _db.CprContrato_Obtener(CodEmpresa, cod_contrato);
        }

        public ErrorDto<CprContratosLista> CprContratosLista_Obtener(int CodEmpresa, string filtros)
        {
            return _db.CprContratosLista_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto CprContrato_Insertar(int CodEmpresa, CprContratosDto contrato)
        {
            return _db.CprContrato_Insertar(CodEmpresa, contrato);
        }

        public ErrorDto CprContrato_Actualizar(int CodEmpresa, CprContratosDto contrato)
        {
            return _db.CprContrato_Actualizar(CodEmpresa, contrato);
        }

        public ErrorDto CprContrato_Eliminar(int CodEmpresa, string cod_contrato, string usuario)
        {
            return _db.CprContrato_Eliminar(CodEmpresa, cod_contrato, usuario);
        }

        public ErrorDto<List<CprContratosAdendumsDto>> CprContrato_Adendums_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _db.CprContrato_Adendums_Obtener(CodEmpresa, cod_contrato);
        }

        public ErrorDto CprContrato_Adendum_Guardar(int CodEmpresa, CprContratosAdendumsDto adendum)
        {
            return _db.CprContrato_Adendum_Guardar(CodEmpresa, adendum);
        }

        public ErrorDto CprContrato_Adendum_Eliminar(int CodEmpresa, int id_adendum, string usuario)
        {
            return _db.CprContrato_Adendum_Eliminar(CodEmpresa, id_adendum, usuario);
        }

        public ErrorDto<List<CprContratosEstadosDto>> CprContrato_Estados_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _db.CprContrato_Estados_Obtener(CodEmpresa, cod_contrato);
        }

        public ErrorDto CprContrato_Estados_Guardar(int CodEmpresa, CprContratosEstadosDto estado)
        {
            return _db.CprContrato_Estados_Guardar(CodEmpresa, estado);
        }

        public ErrorDto CprContrato_Estados_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            return _db.CprContrato_Estados_Eliminar(CodEmpresa, linea_id, usuario);
        }

        public ErrorDto<List<CprContratosProductosDto>> CprContrato_Productos_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _db.CprContrato_Productos_Obtener(CodEmpresa, cod_contrato);
        }

        public ErrorDto CprContrato_Producto_Guardar(int CodEmpresa, CprContratosProductosDto producto)
        {
            return _db.CprContrato_Producto_Guardar(CodEmpresa, producto);
        }

        public ErrorDto CprContrato_Producto_Eliminar(int CodEmpresa, int linea_id, string usuario)
        {
            return _db.CprContrato_Producto_Eliminar(CodEmpresa, linea_id, usuario);
        }

        public ErrorDto<List<CprContratosProrrogasDto>> CprContrato_Prorroga_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _db.CprContrato_Prorroga_Obtener(CodEmpresa, cod_contrato);
        }

        public ErrorDto CprContrato_Prorroga_Guardar(int CodEmpresa, CprContratosProrrogasDto prorroga)
        {
            return _db.CprContrato_Prorroga_Guardar(CodEmpresa, prorroga);
        }

        public ErrorDto CprContrato_Prorroga_Eliminar(int CodEmpresa, int id_prorroga, string usuario)
        {
            return _db.CprContrato_Prorroga_Eliminar(CodEmpresa, id_prorroga, usuario);
        }

        public ErrorDto<List<CprContratosBitacoraDto>> CprContrato_Bitacora_Obtener(int CodEmpresa, string cod_contrato)
        {
            return _db.CprContrato_Bitacora_Obtener(CodEmpresa, cod_contrato);
        }

        public Task<ErrorDto> CprContratoNotificacion_Enviar(int CodEmpresa, string cod_contrato, string mensaje, string usuario)
        {
            return _db.CprContratoNotificacion_Enviar(CodEmpresa, cod_contrato, mensaje, usuario);
        }

        public ErrorDto<List<CprContratosDto>> CprContratosPorSolicitud_Obtener(int CodEmpresa, int cpr_id)
        {
            return _db.CprContratosPorSolicitud_Obtener(CodEmpresa, cpr_id);
        }
    }
}