using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfCambioInfoEstadisticaBL
    {
        private readonly FrmAfCambioInfoEstadisticaDB _db;
        public FrmAfCambioInfoEstadisticaBL(IConfiguration config)
        {
            _db = new FrmAfCambioInfoEstadisticaDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_CambioInfoEstadistica_Listas_Obtener(int CodEmpresa, string vTipo)
        {
            return _db.AF_CambioInfoEstadistica_Listas_Obtener(CodEmpresa, vTipo);
        }

        public ErrorDto AF_CambioInfoEstadistica_Procesar(int CodEmpresa, string usuario, string vTipo, int vCodigo, List<AfCambioInfoEstadisticaDatos> cedulas)
        {
            return _db.AF_CambioInfoEstadistica_Procesar(CodEmpresa, usuario, vTipo, vCodigo, cedulas);
        }
    }
}
