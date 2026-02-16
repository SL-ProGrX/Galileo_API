using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrCatalogoPolizasBL
    {
        private readonly FrmCrCatalogoPolizasDB _db;

        public FrmCrCatalogoPolizasBL(IConfiguration config)
        {
            _db = new FrmCrCatalogoPolizasDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CatalogoPolizas_GrupoAplicacion_Listar(int CodEmpresa)
        {
            return _db.Crd_CatalogoPolizas_GrupoAplicacion_Listar(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CatalogoPolizas_Aseguradoras_Listar(int CodEmpresa)
        {
            return _db.Crd_CatalogoPolizas_Aseguradoras_Listar(CodEmpresa);
        }

        public ErrorDto<List<CrdCatalogoPolizasListDto>> Crd_CatalogoPolizas_Obtener(int CodEmpresa)
        {
            return _db.Crd_CatalogoPolizas_Obtener(CodEmpresa);
        }

        public ErrorDto<CrdCatalogoPolizasConsultaDto?> Crd_CatalogoPoliza_Obtener(int CodEmpresa, string? cod_poliza)
        {
            return _db.Crd_CatalogoPoliza_Obtener(CodEmpresa, cod_poliza);
        }

        public ErrorDto<CrdCatalogoPolizasConsultaDto?> Crd_CatalogoPolizas_Navegar(
               int CodEmpresa,
               string cod_poliza,
               string direccion // "N" = siguiente, "A" = anterior
           )
        {
            return _db.Crd_CatalogoPolizas_Navegar(CodEmpresa, cod_poliza, direccion);
        }

        public ErrorDto<bool> Crd_CatalogoPolizas_ActualizarMasivo(int CodEmpresa, string usuario)
        {
            return _db.Crd_CatalogoPolizas_ActualizarMasivo(CodEmpresa, usuario);
        }

        public ErrorDto<List<CrdCatalogoPolizasAcreedorDto>> Crd_CatalogoPolizas_Acreedores_Obtener(
           int CodEmpresa,
           string? cod_poliza)
        {
            return _db.Crd_CatalogoPolizas_Acreedores_Obtener(CodEmpresa, cod_poliza);
        }

        public ErrorDto<bool> Crd_CatalogoPolizas_Acreedor_Asignar(int CodEmpresa, string usuario, CrdCatalogoPolizasAcreedorAsignarReq req)
        {
            return _db.Crd_CatalogoPolizas_Acreedor_Asignar(CodEmpresa, usuario, req);
        }

        public ErrorDto<List<CrdCatalogoPolizasGarantiaDto>>
        Crd_CatalogoPolizas_Garantias_Listar(int CodEmpresa, string? cod_poliza)
        {
            return _db.Crd_CatalogoPolizas_Garantias_Listar(CodEmpresa, cod_poliza);
        }

        public ErrorDto<CrdCatalogoPolizasGarantiaAsignaDto?>
       Crd_CatalogoPolizas_Garantia_Asignar(
           int CodEmpresa,
           string usuario,
           CrdCatalogoPolizasGarantiaAsignarReq req)
        {
            return _db.Crd_CatalogoPolizas_Garantia_Asignar(CodEmpresa, usuario, req);
        }

    }
}
