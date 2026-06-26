using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTomaFisicaEjecucionBL
    {        
        private readonly FrmInvTomaFisicaEjecucionDB _db;

        public FrmInvTomaFisicaEjecucionBL(IConfiguration config)
        {
            _db = new FrmInvTomaFisicaEjecucionDB(config);
        }

        public ErrorDto<List<EntradasTomaFisicaDto>> Obtener_Entradas(int CodEmpresa)
        {
            return _db.Obtener_Entradas(CodEmpresa);
        }
        public ErrorDto<List<SalidasTomaFisicaDto>> Obtener_Salidas(int CodEmpresa)
        {
            return _db.Obtener_Salidas(CodEmpresa);
        }
        public ErrorDto ProcesarTomaFisica(int CodEmpresa, int consecutivo, string usuario, string cod_entrada, string cod_salida)
        {
            return _db.ProcesarTomaFisica(CodEmpresa, consecutivo, usuario, cod_entrada, cod_salida);
        }
    }
}