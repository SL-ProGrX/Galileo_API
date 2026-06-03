using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrConsultaBitacoraBL
    {
        private readonly FrmCrConsultaBitacoraDB DB;

        public FrmCrConsultaBitacoraBL(IConfiguration config)
        {
            DB = new FrmCrConsultaBitacoraDB(config);
        }

        public ErrorDto<CrConsultaBitacoraEncabezadoDto> CR_ConsultaBitacora_Encabezado_Obtener(int CodEmpresa, string cedula)
        {
            return DB.CR_ConsultaBitacora_Encabezado_Obtener(CodEmpresa, cedula);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraRegistroDto>> CR_ConsultaBitacora_Registro_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Registro_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraRegistroDto>> CR_ConsultaBitacora_Registro_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Registro_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraCreditosDto>> CR_ConsultaBitacora_Creditos_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Creditos_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraCreditosDto>> CR_ConsultaBitacora_Creditos_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Creditos_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraFondosDto>> CR_ConsultaBitacora_Fondos_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Fondos_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraFondosDto>> CR_ConsultaBitacora_Fondos_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Fondos_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraPatrimonioDto>> CR_ConsultaBitacora_Patrimonio_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Patrimonio_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraPatrimonioDto>> CR_ConsultaBitacora_Patrimonio_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Patrimonio_Lista_Export(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraBancosDto>> CR_ConsultaBitacora_Bancos_Lista_Obtener(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Bancos_Lista_Obtener(CodEmpresa, request);
        }

        public ErrorDto<CrConsultaBitacoraLista<CrConsultaBitacoraBancosDto>> CR_ConsultaBitacora_Bancos_Lista_Export(int CodEmpresa, CrConsultaBitacoraRequest request)
        {
            return DB.CR_ConsultaBitacora_Bancos_Lista_Export(CodEmpresa, request);
        }
    }
}