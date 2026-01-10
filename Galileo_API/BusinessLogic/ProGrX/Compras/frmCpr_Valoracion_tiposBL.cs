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

        public ErrorDto<CprValoraEsquemaDtoList> EsquemaValoracion_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _db.EsquemaValoracion_Obtener(CodEmpresa, pagina, paginacion, filtro);
        }

        public ErrorDto<CprValoraItemsDtoList> ValoracionItems_Obtener(int CodEmpresa, string val_id, int? pagina, int? paginacion, string? filtro)
        {
            return _db.ValoracionItems_Obtener(CodEmpresa, val_id, pagina, paginacion, filtro);
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