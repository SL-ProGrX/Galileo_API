using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.DataBaseTier.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndPlazosFrecuenciasBl
    {
        private readonly FrmFndPlazosFrecuenciasDb _db;

        public FrmFndPlazosFrecuenciasBl(IConfiguration config)
        {
            _db = new FrmFndPlazosFrecuenciasDb(config);
        }

        public ErrorDto<List<FndPlazoVencimientoModel>> PlazosVencimiento_Obtener(int codEmpresa)
        {
            return _db.PlazosVencimiento_Obtener(codEmpresa);
        }

        public ErrorDto<FndPlazoVencimientoSaveResult> PlazosVencimiento_Guardar(int codEmpresa, string usuario, FndPlazoVencimientoModel plazo, string mov)
        {
            return _db.PlazosVencimiento_Guardar(codEmpresa, usuario, plazo, mov);
        }

        public ErrorDto<FndPlazoVencimientoSaveResult> PlazosVencimiento_Eliminar(int codEmpresa, int idPlazo, string usuario)
        {
            return _db.PlazosVencimiento_Eliminar(codEmpresa, idPlazo, usuario);
        }

        public ErrorDto<List<FndFrecuenciaCuponModel>> FrecuenciaCupon_Obtener(int codEmpresa)
        {
            return _db.FrecuenciaCupon_Obtener(codEmpresa);
        }

        public ErrorDto<FndFrecuenciaCuponSaveResult> FrecuenciaCupon_Guardar(int codEmpresa, string usuario, FndFrecuenciaCuponModel frecuencia, string mov)
        {
            return _db.FrecuenciaCupon_Guardar(codEmpresa, usuario, frecuencia, mov);
        }

        public ErrorDto<FndFrecuenciaCuponSaveResult> FrecuenciaCupon_Eliminar(int codEmpresa, int idFrecuenciaCupon, string usuario)
        {
            return _db.FrecuenciaCupon_Eliminar(codEmpresa, idFrecuenciaCupon, usuario);
        }
    }
}