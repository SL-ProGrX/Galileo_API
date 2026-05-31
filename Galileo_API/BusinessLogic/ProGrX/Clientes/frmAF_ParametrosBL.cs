using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAFParametrosBL
    {
        private readonly FrmAFParametrosDB DB_AF_Parametros;

        public FrmAFParametrosBL(IConfiguration config)
        {
            DB_AF_Parametros = new FrmAFParametrosDB(config);
        }

        public ErrorDto<AfParametrosLista> AF_Parametros_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            return DB_AF_Parametros.AF_Parametros_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto AF_Parametros_Actualizar(int CodEmpresa, string Usuario, string Codigo, string Valor)
        {
            return DB_AF_Parametros.AF_Parametros_Actualizar(CodEmpresa, Usuario, Codigo, Valor);
        }
    }
}