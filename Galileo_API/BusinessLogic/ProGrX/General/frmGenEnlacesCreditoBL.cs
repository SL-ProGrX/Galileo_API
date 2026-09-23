using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.BusinessLogic
{
    public class FrmGenEnlacesCreditoBl
    {
        readonly FrmGenEnlacesCreditoDb _db;

        public FrmGenEnlacesCreditoBl(IConfiguration config)
        {
            _db = new FrmGenEnlacesCreditoDb(config);
        }

        public ErrorDto Gen_EnlacesCredito_Sincronizar(int CodEmpresa)
            => _db.Gen_EnlacesCredito_Sincronizar(CodEmpresa);

        public ErrorDto<GenEnlacesCreditoLista> Gen_EnlacesCreditoLista_Obtener(int CodEmpresa, string jfiltros)
            => _db.Gen_EnlacesCreditoLista_Obtener(CodEmpresa, DbHelper.DeserializeOrNew<FiltrosLazyLoadData>(jfiltros));

        public ErrorDto<List<GenEnlacesCreditoData>> Gen_EnlacesCredito_Obtener(int CodEmpresa, string jfiltros)
            => _db.Gen_EnlacesCredito_Obtener(CodEmpresa, DbHelper.DeserializeOrNew<FiltrosLazyLoadData>(jfiltros));

        public ErrorDto<List<DropDownListaGenericaModel>> Gen_EnlacesCreditoCatalogo_Obtener(int CodEmpresa, int cod_institucion)
            => _db.Gen_EnlacesCreditoCatalogo_Obtener(CodEmpresa, cod_institucion);

        public ErrorDto Gen_EnlacesCredito_Actualizar(int CodEmpresa, GenEnlacesCreditoData enlace)
            => _db.Gen_EnlacesCredito_Actualizar(CodEmpresa, enlace);
    }
}
