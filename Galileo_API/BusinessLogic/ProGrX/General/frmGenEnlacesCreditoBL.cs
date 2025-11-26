using Galileo.DataBaseTier;
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

        public ErrorDto<EnlaceCreditoLista> EnlacesCreditoConsultar(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _db.EnlacesCreditoConsultar(codEmpresa, pagina, paginacion, filtro);
        }

        public ErrorDto<List<CodigoCreditoDto>> CodigoCredito_ObtenerTodos(int codEmpresa, string cod_institucion)
        {
            return _db.CodigoCredito_ObtenerTodos(codEmpresa, cod_institucion);
        }

        public ErrorDto EnlaceCredito_Actualizar(EnlaceCreditoDto request)
        {
            return _db.EnlaceCredito_Actualizar(request);
        }
    }
}
