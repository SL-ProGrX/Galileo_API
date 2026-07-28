using Newtonsoft.Json;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFCongelarBL
    {
        private readonly  FrmAFCongelarDB _db;

        public FrmAFCongelarBL(IConfiguration config)
        {
            _db = new FrmAFCongelarDB(config);
        }

        #region Consulta

        public ErrorDto<List<DropDownListaGenericaModel>> AF_Congela_Socios_Obtener(int CodEmpresa)
        {
            return _db.AF_Congela_Socios_Obtener(CodEmpresa);
        }

        public ErrorDto<TablasListaGenericaModel> AF_BloqueosCongelamientos_Obtener(int CodEmpresa, string filtrosCongelar, string filtros)
        {
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros);
            return _db.AF_BloqueosCongelamientos_Obtener(CodEmpresa, filtrosCongelar, filtro);
        }

        public ErrorDto<List<AFCongelarDto>> AF_BloqueosCongelamientos_Exportar(int CodEmpresa, string filtrosCongelar)
        {
            return _db.AF_BloqueosCongelamientos_Exportar(CodEmpresa, filtrosCongelar);
        }
        #endregion

        #region Registro

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CongelarCausaLista_Obtener(int CodEmpresa)
        {
            return _db.AF_CongelarCausaLista_Obtener(CodEmpresa);
        }

        public ErrorDto<int> AF_BloqueosCongelamientos_Guardar(int CodEmpresa, string usuario, AFCongelarDto congelar)
        {
            return _db.AF_BloqueosCongelamientos_Guardar(CodEmpresa, usuario, congelar);
        }

        #endregion

        #region Mantenimiento

        public ErrorDto<List<AFCongelaCausaDto>> AF_CongelarCausaMant_Obtener(int CodEmpresa)
        {
            return _db.AF_CongelarCausaMant_Obtener(CodEmpresa);
        }

        public ErrorDto AF_CongelarCausaMant_Eliminar(int CodEmpresa, string cod_causa)
        {
            return _db.AF_CongelarCausaMant_Eliminar(CodEmpresa, cod_causa);
        }

        public ErrorDto AF_CongelarCausaMant_Guardar(int CodEmpresa, string usuario, AFCongelaCausaDto causa)
        {
            return _db.AF_CongelarCausaMant_Guardar(CodEmpresa, usuario, causa);
        }

        #endregion
    }
}