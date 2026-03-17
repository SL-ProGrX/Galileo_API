using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaMacHogarBL
    {
        private readonly FrmCrPolizaMacHogarDB _db;

        public FrmCrPolizaMacHogarBL(IConfiguration config)
        {
            _db = new FrmCrPolizaMacHogarDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizaMacHogar_Polizas_Lista(int codEmpresa)
        {
            return _db.Cr_PolizaMacHogar_Polizas_Lista(codEmpresa);
        }

        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _db.fxFechaServidor(codEmpresa);
        }

        #region Envio

        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Envio_Consulta(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            return _db.Cr_PolizaMacHogar_Envio_Consulta(codEmpresa, usuario, request);
        }

        #endregion

        #region Recepcion
        public ErrorDto<List<CrPolizaMacHogarRecepcionRowDto>> Cr_PolizaMacHogar_Recepcion_Validar(
          int codEmpresa,
          string usuario,
          CrPolizaMacHogarRecepcionValidarRequest request)
        {
            return _db.Cr_PolizaMacHogar_Recepcion_Validar(codEmpresa, usuario, request);
        }

        public ErrorDto Cr_PolizaMacHogar_Recepcion_Procesar(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarRecepcionProcesarRequest request)
        {
            return _db.Cr_PolizaMacHogar_Recepcion_Procesar(codEmpresa, usuario, request);
        }
        #endregion

        #region Consulta
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Consulta_Obtener(
          int codEmpresa,
          string usuario,
          CrPolizaMacHogarEnvioConsultaRequest request)
        {
            return _db.Cr_PolizaMacHogar_Consulta_Obtener(codEmpresa, usuario, request);
        }
        #endregion

        #region Beneficiarios
        public ErrorDto<List<CrPolizaMacHogarBeneficiariosRowDto>> Cr_PolizaMacHogar_Beneficiarios_Lista(
           int codEmpresa,
           string usuario,
           string poliza)
        {
            return _db.Cr_PolizaMacHogar_Beneficiarios_Lista(codEmpresa, usuario, poliza);
        }
        #endregion
    }
}
