
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCBitacoraEspecialModels;


namespace Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar
{
    public class FrmCxCBitacoraEspecialBL
    {

        private readonly FrmCxCBitacoraEspecialDb _db;

        public FrmCxCBitacoraEspecialBL(IConfiguration config) => _db = new FrmCxCBitacoraEspecialDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialMovimientos_Obtener(int codEmpresa)
        {
             
            return _db.CxCBitacoraEspecialMovimientos_Obtener(codEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialPersonas_Obtener(int codEmpresa)
        {

            return _db.CxCBitacoraEspecialPersonas_Obtener(codEmpresa);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialUsuarios_Obtener(int codEmpresa)
        {

            return _db.CxCBitacoraEspecialUsuarios_Obtener(codEmpresa);
        }
        public ErrorDto CxCBitacoraEspecial_Actualizar(int codEmpresa, string usuario, int idBitacora)
        {
            return _db.CxCBitacoraEspecial_Actualizar(codEmpresa, usuario, idBitacora);
        }
        public ErrorDto<BitacoraEspeciaLista> CxCBitacoraEspecialBuscar(int codEmpresa, BitacoraEspeciaFiltros filtros, bool esExportar)
        {
            return _db.CxCBitacoraEspecialBuscar(codEmpresa, filtros, esExportar);
        }
    }
}
