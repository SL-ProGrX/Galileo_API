using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad;

public class FrmArfGestionPagosBl
{
    private readonly FrmArfGestionPagosDb _db;

    public FrmArfGestionPagosBl(IConfiguration config)
    {
        _db = new FrmArfGestionPagosDb(config);
    }

    public ErrorDto<DateTime> FechaServidor(int codEmpresa) => _db.FechaServidor(codEmpresa);

    public ErrorDto<List<ArfGestionPagoDto>> Consultar(int codEmpresa, ArfGestionPagosFiltroDto filtro, string usuario)
        => _db.Consultar(codEmpresa, filtro, usuario);

    public ErrorDto<int> Generar(int codEmpresa, List<ArfGenerarPagoDto> pagos, string usuario)
        => _db.Generar(codEmpresa, pagos, usuario);
}
