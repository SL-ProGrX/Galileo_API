using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCRConsultaCreditosMoraBL
    {
        private readonly FrmCRConsultaCreditosMoraDB _db;

        public FrmCRConsultaCreditosMoraBL(IConfiguration config)
        {
            _db = new FrmCRConsultaCreditosMoraDB(config);
        }

        public ErrorDto<CrConsultaCreditosMoraHeaderDto> CR_ConsultaCreditosMora_Header_Obtener(int CodEmpresa, string cedula)
        {
            return _db.CR_ConsultaCreditosMora_Header_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>> CR_ConsultaCreditosMora_Detalle_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CR_ConsultaCreditosMora_Detalle_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraDetalleDto>> CR_ConsultaCreditosMora_Detalle_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.CR_ConsultaCreditosMora_Detalle_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>> CR_ConsultaCreditosMora_Garantia_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CR_ConsultaCreditosMora_Garantia_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<CrConsultaCreditosMoraListaResult<CrConsultaCreditosMoraGarantiaDto>> CR_ConsultaCreditosMora_Garantia_Lista_Export(int CodEmpresa, string parametros)
        {
            return _db.CR_ConsultaCreditosMora_Garantia_Lista_Export(CodEmpresa, parametros);
        }
    }
}