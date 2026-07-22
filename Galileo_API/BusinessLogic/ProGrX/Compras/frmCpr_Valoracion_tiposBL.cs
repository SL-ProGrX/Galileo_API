using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprValoracionTiposBL
    {
        private readonly FrmCprValoracionTiposDB _db;

        public FrmCprValoracionTiposBL(IConfiguration config)
        {
            _db = new FrmCprValoracionTiposDB(config);
        }

        public ErrorDto<CprValoraEsquemaDtoList> CPR_frmCpr_Valoracion_Tipos_EsquemaValoracion_Obtener(
            int codEmpresa,
            CprValoraConsultaRequest request)
        {
            return _db.CPR_frmCpr_Valoracion_Tipos_EsquemaValoracion_Obtener(codEmpresa, request);
        }

        public ErrorDto<CprValoraItemsDtoList> CPR_frmCpr_Valoracion_Tipos_ValoracionItems_Obtener(
            int codEmpresa,
            string val_id,
            CprValoraConsultaRequest request)
        {
            return _db.CPR_frmCpr_Valoracion_Tipos_ValoracionItems_Obtener(codEmpresa, val_id, request);
        }

        public ErrorDto EsquemaValoracion_Upsert(int CodEmpresa, string usuario, CprValoraEsquemaDto request)
        {
            return _db.EsquemaValoracion_Upsert(CodEmpresa, usuario, request);
        }

        public ErrorDto EsquemaValoracion_Delete(int CodEmpresa, string val_id)
        {
            return _db.EsquemaValoracion_Delete(CodEmpresa, val_id);
        }

        public ErrorDto ValoracionItems_Upsert(int CodEmpresa, string usuario, string val_id, CprValoraItemsDto request)
        {
            return _db.ValoracionItems_Upsert(CodEmpresa, usuario, val_id, request);
        }

        public ErrorDto ValoracionItems_Delete(int CodEmpresa, string val_id, string val_item)
        {
            return _db.ValoracionItems_Delete(CodEmpresa, val_id, val_item);
        }
    }
}
