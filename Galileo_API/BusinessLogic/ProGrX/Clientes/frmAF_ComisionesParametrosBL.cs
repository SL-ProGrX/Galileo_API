using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFComisionesParametrosBL
    {
        private readonly FrmAFComisionesParametrosDB _db;

        public FrmAFComisionesParametrosBL(IConfiguration config)
        {
            _db = new FrmAFComisionesParametrosDB(config);
        }

        public ErrorDto<TablasListaGenericaModel> AF_ComisionesParametros_Obtener(int CodEmpresa, string filtro)
        {
            FiltrosLazyLoadData? jfiltro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtro);
            if (jfiltro is null)
                throw new ArgumentException("Filtro JSON inválido.", nameof(filtro));

            return _db.AF_ComisionesParametros_Obtener(CodEmpresa, jfiltro);
        }

        public ErrorDto AF_ComisionesParametros_Guardar(int CodEmpresa, int Contabilidad, string Usuario, string Parametros)
        {
            AFComisionesParametrosDto? param = JsonConvert.DeserializeObject<AFComisionesParametrosDto>(Parametros);
            if (param is null)
                throw new ArgumentException("Parametros JSON inválido.", nameof(Parametros));

            return _db.AF_ComisionesParametros_Guardar(CodEmpresa, Contabilidad, Usuario, param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_ComisionesParametros_Busqueda(int CodEmpresa, int Contabilidad, string Parametro)
        {
            return _db.AF_ComisionesParametros_Busqueda(CodEmpresa, Contabilidad, Parametro);
        }
    }
}