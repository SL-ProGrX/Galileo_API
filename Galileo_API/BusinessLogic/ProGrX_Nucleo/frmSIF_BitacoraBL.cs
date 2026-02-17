using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.BusinessLogic
{
    public class FrmSifBitacoraBL(IConfiguration config)
    {
        private readonly FrmSifBitacoraDB _db = new(config);

        public ErrorDto<SifBitacoraLista> Bitacora_Obtener(int codEmpresa, string filtros)
        {
            return _db.Bitacora_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> BitacoraModulos_Obtener(int codEmpresa)
        {
            return _db.BitacoraModulos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> BitacoraUsuarios_Obtener(int CodEmpresa)
        {
            return _db.BitacoraUsuarios_Obtener(CodEmpresa);
        }

    }
}