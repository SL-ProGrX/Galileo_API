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

        /// <summary>Obtiene las pólizas disponibles para MAC Hogar.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Listado de pólizas disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizaMacHogar_Polizas_Lista(int codEmpresa)
        {
            return _db.Cr_PolizaMacHogar_Polizas_Lista(codEmpresa);
        }

        /// <summary>Obtiene la fecha actual del servidor de la empresa.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <returns>Fecha actual del servidor.</returns>
        public DateTime fxFechaServidor(int codEmpresa)
        {
            return _db.fxFechaServidor(codEmpresa);
        }

        #region Envio

        /// <summary>Genera el corte de envío de MAC Hogar.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <param name="request">Póliza, corte y tipo de movimiento.</param>
        /// <returns>Registros del corte en el orden original.</returns>
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Envio_Consulta(
            int codEmpresa,
            string usuario,
            CrPolizaMacHogarEnvioConsultaRequest request)
        {
            return _db.Cr_PolizaMacHogar_Envio_Consulta(codEmpresa, usuario, request);
        }

        #endregion

        #region Consulta
        /// <summary>Consulta un corte previamente generado de MAC Hogar.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <param name="request">Póliza, corte y tipo de movimiento.</param>
        /// <returns>Registros del corte solicitado.</returns>
        public ErrorDto<List<CrPolizaMacHogarEnvioRow>> Cr_PolizaMacHogar_Consulta_Obtener(
          int codEmpresa,
          string usuario,
          CrPolizaMacHogarEnvioConsultaRequest request)
        {
            return _db.Cr_PolizaMacHogar_Consulta_Obtener(codEmpresa, usuario, request);
        }
        #endregion

        #region Beneficiarios
        /// <summary>Obtiene los beneficiarios de MAC Hogar.</summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario que ejecuta la consulta.</param>
        /// <returns>Beneficiarios registrados para el tipo PC.</returns>
        public ErrorDto<List<CrPolizaMacHogarBeneficiariosRowDto>> Cr_PolizaMacHogar_Beneficiarios_Lista(
           int codEmpresa,
           string usuario)
        {
            return _db.Cr_PolizaMacHogar_Beneficiarios_Lista(codEmpresa, usuario);
        }
        #endregion
    }
}
