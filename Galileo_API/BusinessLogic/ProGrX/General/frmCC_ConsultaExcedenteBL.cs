using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmCcConsultaExcedenteBl
    {
        readonly FrmCcConsultaExcedenteDb DbfrmCC_ConsultaExcedente;

        public FrmCcConsultaExcedenteBl(IConfiguration config)
        {
            DbfrmCC_ConsultaExcedente = new FrmCcConsultaExcedenteDb(config);
        }

        public ErrorDto<List<CCPeriodoList>>
            CC_ConsultaExcedente_Periodos_Obtener(int codEmpresa)
        {
            return DbfrmCC_ConsultaExcedente
                .CC_ConsultaExcedente_Periodos_Obtener(codEmpresa);
        }

        public ErrorDto<CcConsultaExcedentePersonaData>
            CC_ConsultaExcedente_Persona_Obtener(
                int codEmpresa,
                string cedula,
                string usuario)
        {
            return DbfrmCC_ConsultaExcedente
                .CC_ConsultaExcedente_Persona_Obtener(
                    codEmpresa,
                    cedula,
                    usuario);
        }

        public ErrorDto<CcConsultaExcedenteResultadoData>
            CC_ConsultaExcedente_Consultar(
                int codEmpresa,
                int idPeriodo,
                string cedula)
        {
            return DbfrmCC_ConsultaExcedente
                .CC_ConsultaExcedente_Consultar(
                    codEmpresa,
                    idPeriodo,
                    cedula);
        }

        public ErrorDto CC_ConsultaExcedente_Email_Enviar(
            int codEmpresa,
            CcConsultaExcedenteEmailRequest request)
        {
            return DbfrmCC_ConsultaExcedente
                .CC_ConsultaExcedente_Email_Enviar(codEmpresa, request);
        }

        public ErrorDto CC_ConsultaExcedente_Reporte_Bitacora_Registrar(
            int codEmpresa,
            CcConsultaExcedenteBitacoraRequest request)
        {
            return DbfrmCC_ConsultaExcedente
                .CC_ConsultaExcedente_Reporte_Bitacora_Registrar(
                    codEmpresa,
                    request);
        }

        public List<CCPeriodoList> CC_Periodos_Obtener(int CodEmpresa)
        {
            return DbfrmCC_ConsultaExcedente.CC_Periodos_Obtener(CodEmpresa);
        }

        public CCExcPeriodoData CC_Exc_Periodos_Obtener(int CodEmpresa, int Id_Periodo)
        {
            return DbfrmCC_ConsultaExcedente.CC_Exc_Periodos_Obtener(CodEmpresa, Id_Periodo);
        }

        public ErrorDto CC_ValidaCedula_Obtener(int CodEmpresa, string Cedula, string Usuario)
        {
            return DbfrmCC_ConsultaExcedente.CC_ValidaCedula_Obtener(CodEmpresa, Cedula, Usuario);
        }

        public CCConsultaExcedenteData CC_ConsultaExcedente_Obtener(int CodEmpresa, int Id_Periodo, string Cedula)
        {
            return DbfrmCC_ConsultaExcedente.CC_ConsultaExcedente_Obtener(CodEmpresa, Id_Periodo, Cedula);
        }

        public List<VSifAuxCreditosMovDetalle> CC_NotasMora_Obtener(int CodEmpresa, int NC_Mora, string Cedula)
        {
            return DbfrmCC_ConsultaExcedente.CC_NotasMora_Obtener(CodEmpresa, NC_Mora, Cedula);
        }

        public List<VSifAuxCreditosMovDetalle> CC_NotasOPCF_Obtener(int CodEmpresa, int NC_OPCF, string Cedula)
        {
            return DbfrmCC_ConsultaExcedente.CC_NotasOPCF_Obtener(CodEmpresa, NC_OPCF, Cedula);
        }

        public List<VSifAuxCreditosMovDetalle> CC_NotasSaldos_Obtener(int CodEmpresa, int NC_Saldos, string Cedula)
        {
            return DbfrmCC_ConsultaExcedente.CC_NotasSaldos_Obtener(CodEmpresa, NC_Saldos, Cedula);
        }
    }
}
