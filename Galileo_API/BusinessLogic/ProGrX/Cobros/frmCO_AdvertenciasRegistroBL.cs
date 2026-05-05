using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models;


namespace Galileo.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAdvertenciasRegistroBL
    {

        private readonly FrmCoAdvertenciasRegistroDB _db;

        public FrmCoAdvertenciasRegistroBL(IConfiguration config)
        {
            _db = new FrmCoAdvertenciasRegistroDB(config);
        }

        public ErrorDto<int> CoAdvertenciasRegistro_Guardar(int CodEmpresa, string usuario, CoAdvertenciasRegistroData datos)
        {         
            return _db.CoAdvertenciasRegistro_Guardar(CodEmpresa, usuario, datos);
        }
        public ErrorDto<List<CoAdvertenciasRegistroData>> CoAdvertenciasRegistro_Consultar(int CodEmpresa, string cedula, string cod_advertencia, int linea)
        {
            return _db.CoAdvertenciasRegistro_Consultar(CodEmpresa, cedula, cod_advertencia, linea);
        }
        public ErrorDto CoAdvertenciasRegistro_Delete(int CodEmpresa, string usuario, string cedula, string cod_advertencia, int linea)
        {
            return _db.CoAdvertenciasRegistro_Delete(CodEmpresa, usuario,cedula, cod_advertencia, linea);
        }
        public ErrorDto<DropDownListaGenericaModel> CoAdvertenciasRegistro_TipoAdvertencia(int CodEmpresa, string cod_advertencia, int orden)
        {
            return _db.CoAdvertenciasRegistro_TipoAdvertencia(CodEmpresa, cod_advertencia, orden);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAdvertiencia_Consultar(int CodEmpresa)
        {
            return _db.TiposAdvertiencia_Consultar(CodEmpresa);
        }
        public ErrorDto<List<CoAdvertenciasRegistroSociosData>> CoAdvertenciasRegistroSocios_Obtener(int CodEmpresa)
        {
            return _db.CoAdvertenciasRegistroSocios_Obtener(CodEmpresa );
        }
        public ErrorDto CoAdvertenciasRegistroResolucion_Guardar(int CodEmpresa, string usuario, CoAdvertenciasRegistroData datos)
        {
            return _db.CoAdvertenciasRegistroResolucion_Guardar(CodEmpresa, usuario, datos);
        }
        public ErrorDto<string> CoAdvertenciasRegistroNombreSocios_Consultar(int CodEmpresa, string cedula)
        {
            return _db.CoAdvertenciasRegistroNombreSocios_Consultar(CodEmpresa, cedula);
        }
    }
}