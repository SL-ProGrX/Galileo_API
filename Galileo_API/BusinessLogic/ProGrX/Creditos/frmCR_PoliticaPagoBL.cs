using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPoliticaPagoBl
    {
        private readonly FrmCrPoliticaPagoDb _db;

        public FrmCrPoliticaPagoBl(IConfiguration config)
        {
            _db = new FrmCrPoliticaPagoDb(config);
        }

        public ErrorDto<List<CrPoliticaPagoData>> CR_PoliticaPago_Obtener(int codEmpresa)
        {
            return _db.CR_PoliticaPago_Obtener(codEmpresa);
        }

        public ErrorDto CR_PoliticaPago_Guardar(
            int codEmpresa,
            string usuario,
            CrPoliticaPagoData request)
        {
            return _db.CR_PoliticaPago_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CR_PoliticaPago_Eliminar(
            int codEmpresa,
            string usuario,
            int idPolitica)
        {
            return _db.CR_PoliticaPago_Eliminar(codEmpresa, usuario, idPolitica);
        }

        public ErrorDto<List<CrPoliticaPagoTrasladoData>> CR_PoliticaPago_Traslados_Obtener(
            int codEmpresa,
            string tipo)
        {
            return _db.CR_PoliticaPago_Traslados_Obtener(codEmpresa, tipo);
        }

        public ErrorDto CR_PoliticaPago_Traslados_Guardar(
            int codEmpresa,
            string usuario,
            CrPoliticaPagoTrasladoGuardarRequest request)
        {
            return _db.CR_PoliticaPago_Traslados_Guardar(codEmpresa, usuario, request);
        }

        public ErrorDto CR_PoliticaPago_Traslados_Eliminar(
            int codEmpresa,
            string usuario,
            int idSeq)
        {
            return _db.CR_PoliticaPago_Traslados_Eliminar(codEmpresa, usuario, idSeq);
        }

        public ErrorDto CR_PoliticaPago_TablasPago_Actualizar(int codEmpresa, string usuario)
        {
            return _db.CR_PoliticaPago_TablasPago_Actualizar(codEmpresa, usuario);
        }
    }
}
