using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmInvReporteInventariosBL
    {
        private readonly FrmInvReporteInventariosDB _db;

        public FrmInvReporteInventariosBL(
            IConfiguration config)
        {
            _db = new FrmInvReporteInventariosDB(
                config);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_ReporteInventarios_Bodegas_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_ReporteInventarios_Bodegas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_ReporteInventarios_Lineas_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_ReporteInventarios_Lineas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_ReporteInventarios_Sublineas_Obtener(
                int CodEmpresa,
                int CodLinea)
        {
            return _db
                .INV_ReporteInventarios_Sublineas_Obtener(
                    CodEmpresa,
                    CodLinea);
        }
    }
}