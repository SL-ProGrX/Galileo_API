using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvMargenUtilidadBl
    {
        private readonly FrmInvMargenUtilidadDb _db;

        public FrmInvMargenUtilidadBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmInvMargenUtilidadDb(config);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_MargenUtilidad_Lineas_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_MargenUtilidad_Lineas_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_MargenUtilidad_Sublineas_Obtener(
                int CodEmpresa,
                int codLinea)
        {
            return _db
                .INV_MargenUtilidad_Sublineas_Obtener(
                    CodEmpresa,
                    codLinea);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_MargenUtilidad_Precios_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_MargenUtilidad_Precios_Obtener(
                    CodEmpresa);
        }

        public ErrorDto
            INV_MargenUtilidad_Cambios_Aplicar(
                int CodEmpresa,
                InvMargenUtilidadAplicarRequest request)
        {
            return _db
                .INV_MargenUtilidad_Cambios_Aplicar(
                    CodEmpresa,
                    request);
        }
    }
}