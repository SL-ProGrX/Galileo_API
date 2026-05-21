using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndLiquidaAutoConfigBl
    {
        private readonly FrmFndLiquidaAutoConfigDb _Db;

        public FrmFndLiquidaAutoConfigBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _Db = new FrmFndLiquidaAutoConfigDb(config);
        }

        public ErrorDto<List<FndLiqAutoParametroDto>> Parametros_Lista(int codEmpresa)
        {
            return _Db.Parametros_Lista(codEmpresa);
        }

        public ErrorDto<List<FndLiqAutoPlanesDto>> Planes_Lista(int CodEmpresa)
        {
            return _Db.Planes_Lista(CodEmpresa);
        }

        public ErrorDto<List<FndLiqAutoPlanesPatronalDto>> LiqAuto_Planes_Patronal_Lista(int codEmpresa)
        {
            return _Db.LiqAuto_Planes_Patronal_Lista(codEmpresa);
        }

        public ErrorDto<List<FndLiqAutoReporteDto>> LiqAuto_Reportes_Lista(int codEmpresa, int anio, int mes)
        {
            return _Db.LiqAuto_Reportes_Lista(codEmpresa, anio, mes);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Operadoras_Lista(int codEmpresa)
        {
            return _Db.Operadoras_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Procesos_Lista(int codEmpresa)
        {
            return _Db.Procesos_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PlanesReporte_Lista(int codEmpresa)
        {
            return _Db.PlanesReporte_Lista(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> PlanesBuscar_Lista(int codEmpresa, string operadora)
        {
            return _Db.PlanesBuscar_Lista(codEmpresa, operadora);
        }

        public ErrorDto<bool> Planes_Guardar(int CodEmpresa, FndLiqAutoPlanesAddRequestDto dto)
        {
            return _Db.Planes_Guardar(CodEmpresa, dto);
        }

        public ErrorDto<bool> Parametros_Guardar(int CodEmpresa, FndLiqAutoParametroGuardarRequestDto dto)
        {
            return _Db.Parametros_Guardar(CodEmpresa, dto);
        }
    }
}
