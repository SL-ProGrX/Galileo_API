using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoDesembolsosBL
    {
        private readonly FrmCrSeguimientoDesembolsosDB _db;

        public FrmCrSeguimientoDesembolsosBL(IConfiguration config)
        {
            _db = new FrmCrSeguimientoDesembolsosDB(config);
        }

        public ErrorDto<CrSeguimientoDesembolsosInicializarDto> CR_SeguimientoDesembolsos_Inicializar(
            int CodEmpresa,
            long operacion,
            string usuario)
        {
            return _db.CR_SeguimientoDesembolsos_Inicializar(CodEmpresa, operacion, usuario);
        }

        public ErrorDto<TablasListaGenericaModel> CR_SeguimientoDesembolsos_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            return _db.CR_SeguimientoDesembolsos_Lista_Obtener(CodEmpresa, parametros);
        }

        public ErrorDto<TablasListaGenericaModel> CR_SeguimientoDesembolsos_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return _db.CR_SeguimientoDesembolsos_Lista_Export(CodEmpresa, parametros);
        }

        public ErrorDto<CrSeguimientoDesembolsosData> CR_SeguimientoDesembolsos_Detalle_Obtener(
            int CodEmpresa,
            long idDesembolso)
        {
            return _db.CR_SeguimientoDesembolsos_Detalle_Obtener(CodEmpresa, idDesembolso);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoDesembolsos_Conceptos_Obtener(
            int CodEmpresa,
            string? texto)
        {
            return _db.CR_SeguimientoDesembolsos_Conceptos_Obtener(CodEmpresa, texto);
        }

        public ErrorDto<CrSeguimientoDesembolsosConceptoDto> CR_SeguimientoDesembolsos_Concepto_Info_Obtener(
            int CodEmpresa,
            int codConcepto)
        {
            return _db.CR_SeguimientoDesembolsos_Concepto_Info_Obtener(CodEmpresa, codConcepto);
        }

        public ErrorDto<List<CrSeguimientoDesembolsosBancoDto>> CR_SeguimientoDesembolsos_Bancos_Obtener(
            int CodEmpresa,
            string usuario)
        {
            return _db.CR_SeguimientoDesembolsos_Bancos_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto<List<CrSeguimientoDesembolsosCuentaBancariaDto>> CR_SeguimientoDesembolsos_CuentasBancarias_Obtener(
            int CodEmpresa,
            string identificacion,
            int bancoId,
            int divisaCheck)
        {
            return _db.CR_SeguimientoDesembolsos_CuentasBancarias_Obtener(
                CodEmpresa,
                identificacion,
                bancoId,
                divisaCheck);
        }

        public ErrorDto<CrSeguimientoDesembolsosResumenDto> CR_SeguimientoDesembolsos_Guardar(
            int CodEmpresa,
            CrSeguimientoDesembolsosGuardarRequest request)
        {
            return _db.CR_SeguimientoDesembolsos_Guardar(CodEmpresa, request);
        }

        public ErrorDto<CrSeguimientoDesembolsosResumenDto> CR_SeguimientoDesembolsos_Eliminar(
            int CodEmpresa,
            CrSeguimientoDesembolsosEliminarRequest request)
        {
            return _db.CR_SeguimientoDesembolsos_Eliminar(CodEmpresa, request);
        }
    }
}