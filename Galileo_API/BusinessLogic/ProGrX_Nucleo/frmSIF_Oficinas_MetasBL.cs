using Galileo.DataBaseTier.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSifOficinasMetasBL(IConfiguration config)
    {
        private readonly FrmSifOficinasMetasBD _db = new FrmSifOficinasMetasBD(config);

        public ErrorDto<SifOficinasMetaLista> Sif_OficinasMetasLista_Obtener(int CodEmpresa, string oficina, int anio, string usuario)
        {
            return _db.Sif_OficinasMetasLista_Obtener(CodEmpresa, oficina, anio, usuario);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Sif_OficinasMetasPeriodos_Obtener(int CodEmpresa, string oficina)
        {
            return _db.Sif_OficinasMetasPeriodos_Obtener(CodEmpresa,  oficina);
        }

        public ErrorDto Sif_OficinasMetas_Actualizar(int CodEmpresa, string oficina, string usuario, List<SifOficinasMetaData> metas)
        {
            return _db.Sif_OficinasMetas_Actualizar(CodEmpresa, oficina, usuario, metas);
        }
      
    }
}