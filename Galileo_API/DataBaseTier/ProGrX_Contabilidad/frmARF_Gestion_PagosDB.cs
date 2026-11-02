using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad;

public class FrmArfGestionPagosDb
{
    private readonly PortalDB _portalDb;

    public FrmArfGestionPagosDb(IConfiguration config)
    {
        _portalDb = new PortalDB(config);
    }

    public ErrorDto<DateTime> FechaServidor(int codEmpresa)
    {
        return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            cn.QuerySingle<DateTime>("SELECT GETDATE()"));
    }

    public ErrorDto<List<ArfGestionPagoDto>> Consultar(int codEmpresa, ArfGestionPagosFiltroDto filtro, string usuario)
    {
        if (filtro.Corte == default || string.IsNullOrWhiteSpace(usuario))
            return DbHelper.CreateErrorResponse<List<ArfGestionPagoDto>>("La fecha de corte y el usuario son requeridos.");

        return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
            cn.Query<ArfGestionPagoDto>(
                "exec spARF_Operacion_Pagos_Consulta @Corte, @CodAcreedor, @CodLocal, @Usuario",
                new { Corte = filtro.Corte.Date, CodAcreedor = filtro.Arrendador, CodLocal = filtro.Unidad, Usuario = usuario },
                commandType: CommandType.Text).ToList());
    }

    public ErrorDto<int> Generar(int codEmpresa, List<ArfGenerarPagoDto> pagos, string usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario) || pagos == null || pagos.Count == 0 ||
            pagos.Any(p => p.Operacion <= 0 || p.Id_Seq <= 0 || p.Corte == default))
            return DbHelper.CreateErrorResponse<int>("Debe seleccionar pagos válidos.");

        return DbHelper.WithConn(_portalDb, codEmpresa, cn =>
        {
            var generados = 0;
            foreach (var pago in pagos)
            {
                cn.Execute(
                    "exec spARF_Operacion_Pagos @Operacion, @Corte, @Monto, @Seq, @Usuario",
                    new { pago.Operacion, Corte = pago.Corte.Date, Monto = pago.Cuota, Seq = pago.Id_Seq, Usuario = usuario },
                    commandType: CommandType.Text);
                generados++;
            }
            return generados;
        });
    }
}
