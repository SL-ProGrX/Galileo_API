using Newtonsoft.Json;
using Galileo_API.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;

namespace Galileo_API.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndCdpsTasasBl
    {
        private readonly FrmFndCdpsTasasDb _db;

        public FrmFndCdpsTasasBl(IConfiguration config) => _db = new FrmFndCdpsTasasDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_CdpsTasas_Catalogo_Obtener(int CodEmpresa, int Index)
        {
            return _db.Fnd_CdpsTasas_Catalogo_Obtener(CodEmpresa, Index);
        }

        public ErrorDto<TablasListaGenericaModel> Fnd_CdpsTasas_Obtener(int CodEmpresa, bool Exporta, string Filtros)
        {
            FiltrosLazyLoadData filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(Filtros) ?? new FiltrosLazyLoadData();
            return _db.Fnd_CdpsTasas_Obtener(CodEmpresa, Exporta, filtros);
        }

        public ErrorDto Fnd_CdpsTasas_Config_Guardar(int CodEmpresa, FndCdpsTasaRefData Data)
        {
            return _db.Fnd_CdpsTasas_Config_Guardar(CodEmpresa, Data);   
        }

        public ErrorDto<List<FndCdpsTasaPlanesDto>> Fnd_CdpsTasas_Planes_Obtener(int CodEmpresa, string CodTasaRef, string? Filtro)
        {
            return _db.Fnd_CdpsTasas_Planes_Obtener(CodEmpresa, CodTasaRef, Filtro);
        }

        public ErrorDto Fnd_CdpsTasas_Plan_Asignar(int CodEmpresa, string CodTasaRef, string CodPlan, string Usuario, int Accion)
        {
            return _db.Fnd_CdpsTasas_Plan_Asignar(CodEmpresa, CodTasaRef, CodPlan, Usuario, Accion);
        }

        public ErrorDto<List<FndCdpTasasVencimientoDto>> Fnd_CdpsTasas_Vencimiento_Obtener(int CodEmpresa, string CodTasaRef, int IdPlazo)
        {
            return _db.Fnd_CdpsTasas_Vencimiento_Obtener(CodEmpresa, CodTasaRef, IdPlazo);
        }

        public ErrorDto Fnd_CdpsTasas_Vencimiento_Guardar(int CodEmpresa, string CodTasaRef, int IdCupon, int IdPlazo, decimal Tasa, string Usuario)
        {
            return _db.Fnd_CdpsTasas_Vencimiento_Guardar(CodEmpresa, CodTasaRef, IdCupon, IdPlazo, Tasa, Usuario);
        }

        public ErrorDto Fnd_CdpsTasas_Estado_Actualizar(int CodEmpresa, string CodTasaRef, bool Estado, string Notas, string Usuario)
        {
            return _db.Fnd_CdpsTasas_Estado_Actualizar(CodEmpresa, CodTasaRef, Estado, Notas, Usuario);
        }

        public ErrorDto<List<FndCdpsTasaBitacoraDto>> Fnd_CdpsTasas_Bitacora_Obtener(int CodEmpresa, string CodTasaRef)
        {
            return _db.Fnd_CdpsTasas_Bitacora_Obtener(CodEmpresa, CodTasaRef);
        }

        public ErrorDto Fnd_CdpsTasas_Eliminar(int CodEmpresa, string CodTasaRef, string Usuario)
        {
            return _db.Fnd_CdpsTasas_Eliminar(CodEmpresa, CodTasaRef, Usuario);
        }
    }
}
