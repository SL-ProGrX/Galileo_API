using Galileo.DataBaseTier.ProGrX;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;

namespace Galileo.BusinessLogic.ProGrX
{
    public class FrmDsbDashboardBL
    {
        private readonly FrmDsbDashboardDB _db;

        public FrmDsbDashboardBL(IConfiguration config)
        {
            _db = new FrmDsbDashboardDB(config);
        }

        public ErrorDto<List<DashboardCategoriaData>> Categorias_Obtener(int codEmpresa, string usuario)
            => _db.Categorias_Obtener(codEmpresa, usuario);

        public ErrorDto<DashboardClientesData> Clientes_Obtener(int codEmpresa, string usuario)
            => _db.Clientes_Obtener(codEmpresa, usuario);

        public ErrorDto<List<DashboardClientesPuntoData>> Clientes_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
            => _db.Clientes_Tendencia_Obtener(codEmpresa, usuario, origen, indicador, filtro);

        public ErrorDto<List<DashboardTopFilaData>> Clientes_Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad)
            => _db.Clientes_Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad);

        public ErrorDto<DashboardCreditosData> Creditos_Obtener(int codEmpresa, string usuario)
            => _db.Creditos_Obtener(codEmpresa, usuario);

        public ErrorDto<List<DashboardClientesPuntoData>> Creditos_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
            => _db.Creditos_Tendencia_Obtener(codEmpresa, usuario, origen, indicador, filtro);

        public ErrorDto<List<DashboardTopFilaData>> Creditos_Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad)
            => _db.Creditos_Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad);

        public ErrorDto<DashboardModuloData> Modulo_Obtener(
            int codEmpresa, string usuario, string categoria)
            => _db.Modulo_Obtener(codEmpresa, usuario, categoria);

        public ErrorDto<List<DashboardModuloPuntoData>> Modulo_Tendencia_Obtener(
            int codEmpresa, string usuario, string categoria, string origen,
            string indicador, string? filtro)
            => _db.Modulo_Tendencia_Obtener(
                codEmpresa, usuario, categoria, origen, indicador, filtro);

        public ErrorDto<List<DashboardTopFilaData>> Modulo_Top_Obtener(
            int codEmpresa, string usuario, string categoria, string codigo,
            int dias, int cantidad)
            => _db.Modulo_Top_Obtener(
                codEmpresa, usuario, categoria, codigo, dias, cantidad);

        public ErrorDto<DashboardAhorrosData> Ahorros_Obtener(int codEmpresa, string usuario)
            => _db.Ahorros_Obtener(codEmpresa, usuario);

        public ErrorDto<List<DashboardClientesPuntoData>> Ahorros_Tendencia_Obtener(
            int codEmpresa, string usuario, string origen, string indicador, string? filtro)
            => _db.Ahorros_Tendencia_Obtener(codEmpresa, usuario, origen, indicador, filtro);

        public ErrorDto<List<DashboardTopFilaData>> Ahorros_Top_Obtener(
            int codEmpresa, string usuario, string codigo, int dias, int cantidad)
            => _db.Ahorros_Top_Obtener(codEmpresa, usuario, codigo, dias, cantidad);
    }
}
