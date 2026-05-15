using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCalculoRendimientoBL
    {
        private readonly FrmFndCalculoRendimientoDb _Db;

        public FrmFndCalculoRendimientoBL(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _Db = new FrmFndCalculoRendimientoDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Operadoras_Lista(int CodEmpresa)
        {
            return _Db.Operadoras_Lista(CodEmpresa);
        }

        public ErrorDto<FndPlanDatosDto> Plan_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _Db.Plan_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        public ErrorDto<FndPlanDatosDto> Plan_Scroll(int CodEmpresa, int CodOperadora, string? CodPlan, int ScrollCode)
        {
            return _Db.Plan_Scroll(CodEmpresa, CodOperadora, CodPlan, ScrollCode);
        }

        public ErrorDto<FndRendimientoResultadoDto> AplicarRendimientos(int CodEmpresa, FndRendimientoRequestDto dto)
        {
            return _Db.AplicarRendimientos(CodEmpresa, dto);
        }

        public ErrorDto<List<FndHistorialRendDto>> HistorialRend_Lista(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _Db.HistorialRend_Lista(CodEmpresa, CodOperadora, CodPlan);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Planes_Lista(int CodEmpresa, int CodOperadora)
        {
            return _Db.Planes_Lista(CodEmpresa, CodOperadora);
        }

        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return _Db.FechaServidor_Obtener(CodEmpresa);
        }

    }
}