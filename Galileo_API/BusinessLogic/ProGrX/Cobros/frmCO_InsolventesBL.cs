using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoInsolventesModels;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCOInsolventesBL
    {

        private readonly FrmCOInsolventesDB _db;

        public FrmCOInsolventesBL(IConfiguration config)
        {
            _db = new FrmCOInsolventesDB(config);
        }

        public ErrorDto<List<CbrInsolventeGridItem>> CoInsolventes_Buscar(int codEmpresa, CbrInsolventesBuscarRequest request)
                => _db.CoInsolventes_Buscar(codEmpresa, request);

        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Registrar(
                 int codEmpresa,
                 CbrInsolventeRegistrarRequest request,
                 string usuario)
             => _db.CoInsolventes_Registrar(codEmpresa, request, usuario);

        public ErrorDto<CbrSpMovimientoResult> CoInsolventes_Reversar(
             int codEmpresa,
             CbrInsolventeRegistrarRequest request,
             string usuario)
           => _db.CoInsolventes_Reversar(codEmpresa, request, usuario);

        public ErrorDto<List<CbrInsolventeSocioResult>> CoInsolventes_Socios_Obtener(int codEmpresa)
        => _db.CoInsolventes_Socios_Obtener(codEmpresa);
    }
}
