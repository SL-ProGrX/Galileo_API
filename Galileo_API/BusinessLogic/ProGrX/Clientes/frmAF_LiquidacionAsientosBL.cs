using Newtonsoft.Json;
using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfLiquidacionAsientosBL
    {
        private readonly FrmAfLiquidacionAsientosDB _DB;

        public FrmAfLiquidacionAsientosBL(IConfiguration config)
        {
            _DB = new FrmAfLiquidacionAsientosDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqAsientosTipo_Obtener(int CodEmpresa, string accion)
        {
            return _DB.AF_LiqAsientosTipo_Obtener(CodEmpresa, accion);
        }

        public ErrorDto<List<TokenConsultaModel>> AF_LiqAsientosToken_Obtener(int CodEmpresa, string usuario)
        {
            return _DB.AF_LiqAsientosToken_Obtener(CodEmpresa, usuario);
        }

        public ErrorDto AF_LiqAsientoToken_Nuevo(int CodEmpresa, string usuario)
        {
            return _DB.AF_LiqAsientoToken_Nuevo(CodEmpresa, usuario);
        }

        public ErrorDto<List<LiquidacionAsientoModel>> AF_LiquidacionAsiento_Obtener(int CodEmpresa,string filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
                throw new ArgumentNullException(nameof(filtros), "El parámetro 'filtros' no puede ser nulo o vacío.");

            FiltrosSolicitud jfiltro = JsonConvert.DeserializeObject<FiltrosSolicitud>(filtros)
                ?? throw new InvalidOperationException("No se pudo deserializar 'filtros' a FiltrosSolicitud.");

            return _DB.AF_LiquidacionAsiento_Obtener(CodEmpresa, jfiltro);
        }

        public ErrorDto Af_LiquidacionAsiento_Generar(int CodEmpresa, string usuario, string filtros, List<LiquidacionAsientoModel> liquidaciones)
        {
            FiltrosSolicitud jfiltro = JsonConvert.DeserializeObject<FiltrosSolicitud>(filtros)
                ?? throw new InvalidOperationException("No se pudo deserializar 'filtros' a FiltrosSolicitud.");
            return _DB.Af_LiquidacionAsiento_Generar(CodEmpresa, usuario, jfiltro, liquidaciones);
        }

    }
}