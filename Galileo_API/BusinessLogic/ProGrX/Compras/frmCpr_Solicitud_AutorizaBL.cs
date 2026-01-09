using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprSolicitudAutorizaBL
    {
        readonly FrmCprSolicitudAutorizaDB _db;

        public FrmCprSolicitudAutorizaBL(IConfiguration config)
        {
            _db = new FrmCprSolicitudAutorizaDB(config);
        }

        public ErrorDto<List<CprSolicitudAdjudicaConsulta>> CprSolicitudAdjudica_Consultar(int CodEmpresa, int cpr_id)
        {
            return _db.CprSolicitudAdjudica_Consultar(CodEmpresa, cpr_id);
        }

        public ErrorDto<List<CprSolicitudAdjudicaProductosDto>> CprSolicitudAdjudicaProductos_Consultar(int CodEmpresa, int cpr_id, int proveedor, string? cotizacion)
        {
            return _db.CprSolicitudAdjudicaProductos_Consultar(CodEmpresa, cpr_id, proveedor, cotizacion);
        }

        public ErrorDto CprSolicitudAdjudicaProv_Upsert(int CodEmpresa, string adjudica)
        {
            return _db.CprSolicitudAdjudicaProv_Upsert(CodEmpresa, adjudica);
        }

        public ErrorDto<string> CprSolicitudRecomendacion_Obtener(int CodEmpresa, int cpr_id)
        {
            return _db.CprSolicitudRecomendacion_Obtener(CodEmpresa, cpr_id);
        }

        public ErrorDto<string> CprSolicitudNumContrato_Obtener(int CodEmpresa, int cpr_id)
        {
            return _db.CprSolicitudNumContrato_Obtener(CodEmpresa, cpr_id);
        }

        public ErrorDto CprSolicitudRecomendacion_Guardar(int CodEmpresa, int cpr_id, string recomendacion, string? cod_contrato, bool requiereContrato)
        {
            return _db.CprSolicitudRecomendacion_Guardar(CodEmpresa, cpr_id, recomendacion, cod_contrato, requiereContrato);
        }
        
        public ErrorDto CprSolicitudAdjudicacion_Cerrar(int CodEmpresa, int cpr_id, string usuario)
        {
            return _db.CprSolicitudAdjudicacion_Cerrar(CodEmpresa, cpr_id, usuario);
        }
    }
}