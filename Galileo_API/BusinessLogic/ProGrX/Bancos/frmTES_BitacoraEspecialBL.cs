using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.DataBaseTier.TES;

namespace Galileo_API.BusinessLogic.TES
{
    public class FrmTesBitacoraEspecialBL
    {
        private readonly FrmTesBitacoraEspecialDB _db;

        public FrmTesBitacoraEspecialBL(IConfiguration config)
        {
            _db = new FrmTesBitacoraEspecialDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int codEmpresa)
        {
            return _db.TES_Bancos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Movimientos_Obtener(int codEmpresa)
        {
            return _db.TES_Tipos_Movimientos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Doc_Obtener(int codEmpresa)
        {
            return _db.TES_Tipos_Doc_Obtener(codEmpresa);
        }

        public ErrorDto<List<BitacoraEspecialDto>> BitacoraEspecial_Buscar(int codEmpresa, FiltrosBitacoraEspecial filtros)
        {
            return _db.BitacoraEspecial_Buscar(codEmpresa, filtros);
        }
        
        public ErrorDto TES_Historial_Actualizar(int codEmpresa, string id, string usuario, string nsolicitud)
        {
            return _db.TES_Historial_Actualizar(codEmpresa, id, usuario, nsolicitud);
        }
        
    }

}