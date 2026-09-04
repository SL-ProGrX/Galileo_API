using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public sealed class FrmInvParametrosBl
    {
        private readonly FrmInvParametrosDb _db;

        public FrmInvParametrosBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmInvParametrosDb(config);
        }

        public ErrorDto<ParametrosGenDto?>
            INV_Parametros_Parametros_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_Parametros_Parametros_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<List<CntXContaDto>>
            INV_Parametros_Contabilidades_Obtener(
                int CodEmpresa)
        {
            return _db
                .INV_Parametros_Contabilidades_Obtener(
                    CodEmpresa);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_Parametros_Cuentas_Descripciones_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            return _db
                .INV_Parametros_Cuentas_Descripciones_Obtener(
                    CodEmpresa,
                    codContabilidad);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_Parametros_Asientos_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            return _db
                .INV_Parametros_Asientos_Obtener(
                    CodEmpresa,
                    codContabilidad);
        }

        public ErrorDto
            INV_Parametros_Actualizar(
                int CodEmpresa,
                ParametrosGenDto request)
        {
            return _db
                .INV_Parametros_Actualizar(
                    CodEmpresa,
                    request);
        }
    }
}