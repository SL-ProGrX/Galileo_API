using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvUnidadesConvBL
    {
        private readonly FrmInvUnidadesConvDB _db;

        public FrmInvUnidadesConvBL(IConfiguration config)
        {
            _db = new FrmInvUnidadesConvDB(config);
        }

        public ErrorDto<List<UnidadMedicionConv>> UnidadMedicion_Obtener(int CodCliente)
        {
            return _db.UnidadMedicion_Obtener(CodCliente);
        }

        public ErrorDto<UnidadesConvLista> UnidadConvLista_Obtener(int CodCliente, string cod_unidad)
        {
            return _db.UnidadConvLista_Obtener(CodCliente, cod_unidad);
        }

        public ErrorDto UnidadConv_Guardar(int CodCliente, UnidadMedicionConvData equivalencia)
        {
            return _db.UnidadConv_Guardar(CodCliente, equivalencia);
        }

        public ErrorDto UnidadConv_Eliminar(int CodCliente, string cod_unidad, string cod_unidad_d)
        {
            return _db.UnidadConv_Eliminar(CodCliente, cod_unidad, cod_unidad_d);
        }
    }
}