using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosCambioTipoBL
    {
        private readonly FrmActivosCambioTipoDb _db;

        public FrmActivosCambioTipoBL(IConfiguration config)
        {
            _db = new FrmActivosCambioTipoDb(config);
        }
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipos_Obtener(int CodEmpresa)
        {
            return _db.Activos_Tipos_Obtener(CodEmpresa);
        }

        public ErrorDto<ActivosPrincipalesData?> Activos_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            return _db.Activos_DatosActivo_Consultar(CodEmpresa, placa);
        }

        public ErrorDto<List<ActivosData>> Activos_Obtener(int CodEmpresa)
        {
            return _db.Activos_Obtener(CodEmpresa);
        }
    }
}
