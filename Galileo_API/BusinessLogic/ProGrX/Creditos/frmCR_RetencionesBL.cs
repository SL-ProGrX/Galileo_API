using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrRetencionesBL
    {
        private readonly FrmCrRetencionesDB _db;

        public FrmCrRetencionesBL(IConfiguration config)
        {
            _db = new FrmCrRetencionesDB(config);
        }

        public List<RetencionCreditoData> AF_CR_Retenciones_Obtener(int codEmpresa, int idSolicitud)
        {
            return _db.AF_CR_Retenciones_Obtener(codEmpresa, idSolicitud);
        }

        public List<SocioData> AF_CR_Retenciones_ObtenerSocios(int codEmpresa)
        {
            return _db.AF_CR_Retenciones_ObtenerSocios(codEmpresa);
        }

        public List<CatalogoRetencionData> AF_CR_Retenciones_ObtenerCatalogoRetencion(int codEmpresa)
        {
            return _db.AF_CR_Retenciones_ObtenerCatalogoRetencion(codEmpresa);
        }

        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerDeductorasCombo(int codEmpresa, string codInstitucion)
        {
            return _db.AF_CR_Retenciones_ObtenerDeductorasCombo(codEmpresa, codInstitucion);
        }

        public List<InstitucionFrecuenciaData> AF_CR_Retenciones_ObtenerInstitucionFrecuencia(int codEmpresa, string codDeductora)
        {
            return _db.AF_CR_Retenciones_ObtenerInstitucionFrecuencia(codEmpresa, codDeductora);
        }

        public List<SocioDeduccionData> AF_CR_Retenciones_ObtenerSocioDeduccion(int codEmpresa, string cedula)
        {
            return _db.AF_CR_Retenciones_ObtenerSocioDeduccion(codEmpresa, cedula);
        }

        public List<PrimerDeduccionData> AF_CR_Retenciones_ObtenerPrimerDeduccion(int codEmpresa, string codDeductora)
        {
            return _db.AF_CR_Retenciones_ObtenerPrimerDeduccion(codEmpresa, codDeductora);
        }

        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerDestinosPorCodigo(int codEmpresa, string codigo)
        {
            return _db.AF_CR_Retenciones_ObtenerDestinosPorCodigo(codEmpresa, codigo);
        }

        public List<DropDownListaGenericaModel> AF_CR_Retenciones_ObtenerGarantiasPorLinea(int codEmpresa, string linea)
        {
            return _db.AF_CR_Retenciones_ObtenerGarantiasPorLinea(codEmpresa, linea);
        }

        public List<CatalogoDetalleData> AF_CR_Retenciones_ObtenerCatalogoDetalle(int codEmpresa, string codigo)
        {
            return _db.AF_CR_Retenciones_ObtenerCatalogoDetalle(codEmpresa, codigo);
        }

        public List<SiguienteSolicitudData> AF_CR_Retenciones_ObtenerSiguienteSolicitud(int codEmpresa, int idSolicitudActual, bool siguiente)
        {
            return _db.AF_CR_Retenciones_ObtenerSiguienteSolicitud(codEmpresa, idSolicitudActual, siguiente);
        }

        public ErrorDto AF_CR_Retenciones_InsertarCredito(int codEmpresa, InsertarCreditoRequest req)
        {
            return _db.AF_CR_Retenciones_InsertarCredito(codEmpresa, req);
        }

        public ValidacionPreviaInsertarCreditoResponse AF_CR_Retenciones_ValidarAntesInsertar(int codEmpresa, string codigo, string cedula)
        {
            return _db.AF_CR_Retenciones_ValidarAntesInsertar(codEmpresa, codigo, cedula);
        }
    }
}
