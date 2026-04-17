using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasReporteCierresBl
    {
        private readonly FrmCajasReporteCierresDb DbfrmCajas_ReporteCierres;

        public FrmCajasReporteCierresBl(IConfiguration config) =>
            DbfrmCajas_ReporteCierres = new FrmCajasReporteCierresDb(config);

        public ErrorDto<List<CajasAperturaReporteDto>> Cajas_Aperturas_Consulta(int codEmpresa, string codCaja, DateTime fechaInicio,
            DateTime fechaCorte, string filtro)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Aperturas_Consulta(codEmpresa, codCaja, fechaInicio, fechaCorte, filtro);
        }

        public ErrorDto<List<CajasAccesoDto>> Cajas_Acceso_Consulta(int codEmpresa, string codCaja, DateTime fechaInicio,
           DateTime fechaCorte)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Accesos_Consulta(codEmpresa, codCaja, fechaInicio, fechaCorte);
        }

        public ErrorDto<List<CajasDepositoDto>> Cajas_Depositos_Consulta(int codEmpresa, string codCaja, int codApertura)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Depositos_Consulta(codEmpresa, codCaja, codApertura);
        }

        public ErrorDto<bool> Cajas_Cierre_Forzado(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Cierre_Forzado(codEmpresa, codCaja, codApertura, usuario);
        }

        public ErrorDto<bool> Cajas_Cierre_Recibe(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Cierre_Recibe(codEmpresa, codCaja, codApertura, usuario);
        }

        public ErrorDto<bool> Cajas_Cierre_Revisa(int codEmpresa, string codCaja, int codApertura, string usuario)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Cierre_Revisa(codEmpresa, codCaja, codApertura, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Definicion_Lista(int codEmpresa)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Definicion_Lista(codEmpresa);
        }

        public ErrorDto<bool> Cajas_Cierre_Forzar(int codEmpresa,string codCaja,int codApertura,string usuario)
        {
            return DbfrmCajas_ReporteCierres.Cajas_Cierre_Forzar(codEmpresa, codCaja, codApertura, usuario);
        }
    }
}