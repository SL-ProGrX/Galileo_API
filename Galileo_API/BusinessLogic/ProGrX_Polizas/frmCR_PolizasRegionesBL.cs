using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizasRegionesBL
    {
        private readonly FrmCrPolizasRegionesDB _db;
        public FrmCrPolizasRegionesBL(IConfiguration config)
        {
            _db = new FrmCrPolizasRegionesDB(config);
        }

        public ErrorDto<List<CrdPolizasRegionDto>> Crd_Polizas_Region_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _db.Crd_Polizas_Region_Obtener(CodEmpresa, cod_poliza);
        }

        public ErrorDto Crd_Polizas_Region_Guardar(int CodEmpresa, string usuario, CrdPolizasRegionGuardarDto dto)
        {
            return _db.Crd_Polizas_Region_Guardar(CodEmpresa, usuario, dto);
        }

        public ErrorDto Crd_Polizas_Region_Eliminar(int CodEmpresa, string cod_poliza, int cod_region)
        {
            return _db.Crd_Polizas_Region_Eliminar(CodEmpresa, cod_poliza , cod_region);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Polizas_RegionLista_Obtener(int CodEmpresa, string cod_poliza)
        {
            return _db.Crd_Polizas_RegionLista_Obtener(CodEmpresa, cod_poliza);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Provincias_Listar(int CodEmpresa)
        {
            return _db.Crd_Provincias_Listar(CodEmpresa);
        }

        public ErrorDto<List<CrdPolizasRegionCantonDto>> Crd_Polizas_Region_Cantones_Listar(
                int CodEmpresa,
                string cod_poliza,
                int cod_region,
                string? provincia,
                CrdCantonesModo modo)
        {
            return _db.Crd_Polizas_Region_Cantones_Listar(CodEmpresa, cod_poliza, cod_region, provincia, modo);
        }

        public ErrorDto Crd_Polizas_Region_Canton_Asignar(int CodEmpresa, string usuario, CrdPolizasRegionAsignarCantonDto dto)
        {
            return _db.Crd_Polizas_Region_Canton_Asignar(CodEmpresa, usuario, dto);
        }


    }
}
