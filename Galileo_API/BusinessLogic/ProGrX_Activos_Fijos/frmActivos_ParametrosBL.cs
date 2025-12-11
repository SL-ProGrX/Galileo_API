using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosParametrosBL
    {
        private readonly FrmActivosParametrosDB _db;

        public FrmActivosParametrosBL(IConfiguration config)
        {
            _db = new FrmActivosParametrosDB(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Parametros_Contabilidad_Obtener(int CodEmpresa)
        {
            return _db.Activos_Parametros_Contabilidad_Obtener(CodEmpresa);
        }

        public ErrorDto Activos_Parametros_EstablecerMes(int CodEmpresa, DateTime periodo)
        {
            return _db.Activos_Parametros_EstablecerMes(CodEmpresa, periodo);
        }


        public ErrorDto<ActivosParametrosData?> Activos_Parametros_Consultar(int CodEmpresa)
        {
            return _db.Activos_Parametros_Consultar(CodEmpresa);
        }

        public ErrorDto Activos_Parametros_Guardar(int CodEmpresa, ActivosParametrosData datos)
        {
            return _db.Activos_Parametros_Guardar(CodEmpresa, datos);
        }
    }
}
