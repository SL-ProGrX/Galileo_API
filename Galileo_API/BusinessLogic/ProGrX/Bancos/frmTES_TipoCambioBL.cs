using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesTipoCambioBL
    {
        private readonly FrmTesTipoCambioDB _bd;

        public FrmTesTipoCambioBL(IConfiguration config)
        {
            _bd = new FrmTesTipoCambioDB(config);
        }

        public ErrorDto<TesTipoCambioDivisasTipoCambio> Tes_TipoCambio_Obtener(string jTipoCambio)
        {
            TesTipoCambioConsulta tipoCambio = JsonConvert.DeserializeObject<TesTipoCambioConsulta>(jTipoCambio) ?? new TesTipoCambioConsulta();
            return _bd.Tes_TipoCambio_Obtener(tipoCambio);
        }

        public ErrorDto<double> Tes_TipoCambio_MontoCambiar(decimal pTipoCambio)
        { 
            return _bd.Tes_TipoCambio_MontoCambiar(pTipoCambio);
        }

        public ErrorDto<string> Tes_tipoCambioDivisa_Obterner(int CodEmpresa, string cod_divisa)
        {
            return _bd.Tes_tipoCambioDivisa_Obterner(CodEmpresa, cod_divisa);
        }
    }
}
