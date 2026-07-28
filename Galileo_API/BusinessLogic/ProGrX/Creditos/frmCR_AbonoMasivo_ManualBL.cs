
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using static Galileo_API.Models.ProGrX.Creditos.FrmCrAbonoMasivoManualModels;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrAbonoMasivoManualBl
    {
       
        private readonly FrmCrAbonoMasivoManualDb _db;

        public FrmCrAbonoMasivoManualBl(IConfiguration config)
        {
            _db = new FrmCrAbonoMasivoManualDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CR_AbonoMasivo_Manual_Operadoras_Obtener(int codEmpresa)
           => _db.CR_AbonoMasivo_Manual_Operadoras_Obtener(codEmpresa);
        public ErrorDto<List<DropDownListaGenericaModel>> CR_AbonoMasivo_Manual_Planes_Obtener(int codEmpresa, string operadora)
           => _db.CR_AbonoMasivo_Manual_Planes_Obtener(codEmpresa, operadora);
        public ErrorDto<CrAplicacionAbonoMasivoProcesarResponse> ProcesarAbonosMasivos(int codEmpresa, CrAplicacionAbonoMasivoProcesarRequest request)
              => _db.ProcesarAbonosMasivos(codEmpresa, request);
        public ErrorDto<CrAplicacionAbonoMasivoResponse> CR_AbonoMasivo_Manual_CargaDeducciones_Procesar(int codEmpresa, CrAplicacionAbonoMasivoRequest request)
              => _db.CR_AbonoMasivo_Manual_CargaDeducciones_Procesar(codEmpresa, request);

    }
}
    