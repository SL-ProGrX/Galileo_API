using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCarteraSensibilizacionBl
    {
        private readonly FrmCrCarteraSensibilizacionDb _db;

        public FrmCrCarteraSensibilizacionBl(IConfiguration config)
        {
            _db = new FrmCrCarteraSensibilizacionDb(config);
        }

        public ErrorDto<CrCarteraSensibilizacionPantallaData> CrCarteraSensibilizacion_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _db.CrCarteraSensibilizacion_Pantalla_Obtener(codEmpresa, usuario);

        public ErrorDto<CrCarteraSensibilizacionPantallaData> CrCarteraSensibilizacion_Linea_Combos_Obtener(
            int codEmpresa,
            string codigo,
            bool todasLineas)
            => _db.CrCarteraSensibilizacion_Linea_Combos_Obtener(codEmpresa, codigo, todasLineas);

        public ErrorDto<List<DropDownListaGenericaModel>> CrCarteraSensibilizacion_Catalogo_Obtener(
            int codEmpresa)
            => _db.CrCarteraSensibilizacion_Catalogo_Obtener(codEmpresa);

        public ErrorDto<CrCarteraSensibilizacionResultadoData> CrCarteraSensibilizacion_Buscar(
            int codEmpresa,
            CrCarteraSensibilizacionRequest request)
            => _db.CrCarteraSensibilizacion_Buscar(codEmpresa, request);

        public ErrorDto<CrCarteraSensibilizacionGenerarData> CrCarteraSensibilizacion_Generar(
            int codEmpresa,
            CrCarteraSensibilizacionResultadoData request)
            => _db.CrCarteraSensibilizacion_Generar(codEmpresa, request);

        public ErrorDto<List<CrCarteraSensibilizacionLiquidezItem>> CrCarteraSensibilizacion_Liquidez_Obtener(
            int codEmpresa)
            => _db.CrCarteraSensibilizacion_Liquidez_Obtener(codEmpresa);
    }
}