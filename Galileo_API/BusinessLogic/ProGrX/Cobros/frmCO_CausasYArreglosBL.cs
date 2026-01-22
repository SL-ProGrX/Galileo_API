using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.DataBaseTier.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOCausasYArreglosBL
    {
        private readonly FrmCOCausasYArreglosDB _db;

        public FrmCOCausasYArreglosBL(IConfiguration config)
        {
            _db = new FrmCOCausasYArreglosDB(config);
        }
        public ErrorDto<COCausaMorosidadListaResult> Co_CausasMorosidad_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            return _db.Co_CausasMorosidad_Lista_Obtener(CodEmpresa, jfiltros);
        }
        public ErrorDto<COCausaMorosidadListaResult> Co_CausasMorosidad_Lista_Export(int CodEmpresa, string jfiltros)
        {
            return _db.Co_CausasMorosidad_Lista_Export(CodEmpresa, jfiltros);
        }
        public ErrorDto Co_CausasMorosidad_Guardar(int CodEmpresa, string usuario, COCausaMorosidadData causa)
        {
            return _db.Co_CausasMorosidad_Guardar(CodEmpresa, usuario, causa);
        }
        public ErrorDto Co_CausasMorosidad_Eliminar(int CodEmpresa, string usuario, string cod_causa)
        {
            return _db.Co_CausasMorosidad_Eliminar(CodEmpresa, usuario, cod_causa);
        }
        public ErrorDto<COArregloPagoTipoListaResult> Co_TiposArreglos_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            return _db.Co_TiposArreglos_Lista_Obtener(CodEmpresa, jfiltros);
        }
        public ErrorDto<COArregloPagoTipoListaResult> Co_TiposArreglos_Lista_Export(int CodEmpresa, string jfiltros)
        {
            return _db.Co_TiposArreglos_Lista_Export(CodEmpresa, jfiltros);
        }
        public ErrorDto Co_TiposArreglos_Guardar(int CodEmpresa, string usuario, COArregloPagoTipoData tipo)
        {
            return _db.Co_TiposArreglos_Guardar(CodEmpresa, usuario, tipo);
        }
        public ErrorDto Co_TiposArreglos_Eliminar(int CodEmpresa, string usuario, string cod_arreglo)
        {
            return _db.Co_TiposArreglos_Eliminar(CodEmpresa, usuario, cod_arreglo);
        }
    }
}
