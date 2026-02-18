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

        #region Definicion

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

        #endregion

        #region Asignacion

        public ErrorDto<List<CrdTreeNodeDto>> Crd_Asignacion_Arbol_Raiz(int CodEmpresa)
        {
            return _db.Crd_Asignacion_Arbol_Raiz(CodEmpresa);
        }

        public ErrorDto<List<CrdTreeNodeDto>> Crd_Asignacion_Arbol_Hijos(int CodEmpresa, string nodeKey)
        {
            return _db.Crd_Asignacion_Arbol_Hijos(CodEmpresa, nodeKey);
        }

        public ErrorDto<List<CrdCatalogoPolizasAsignacionDto>> Crd_CatalogoPolizas_Asignacion_Obtener(
           int CodEmpresa,
           string codigo,
           string cod_destino,
           string garantia)
        {
            return _db.Crd_CatalogoPolizas_Asignacion_Obtener(CodEmpresa, codigo, cod_destino, garantia);
        }

        public ErrorDto Crd_CatalogoPolizas_Asignacion_Actualizar(
            int CodEmpresa,
            string usuario,
            CrdCatalogoPolizasAsignacionUpdateDto datos)
        {
            return _db.Crd_CatalogoPolizas_Asignacion_Actualizar(CodEmpresa, usuario, datos);
        }

        public ErrorDto Crd_CatalogoPolizas_Guardar(int CodEmpresa, string usuario, CrdCatalogoPolizasGuardarDto dto)
        {
            return _db.Crd_CatalogoPolizas_Guardar(CodEmpresa, usuario, dto);
        }

        #endregion

        #region Acreedores

        public ErrorDto<List<CrdPolizasAcreedoresGridDto>> Crd_PolizasAcreedores_Grid_Obtener(int CodEmpresa)
        {
            return _db.Crd_PolizasAcreedores_Grid_Obtener(CodEmpresa);
        }

        public ErrorDto Crd_PolizasAcreedores_Eliminar(
                int CodEmpresa,
                string usuario,
                string cod_acreedor)
        {
            return _db.Crd_PolizasAcreedores_Eliminar(CodEmpresa, usuario, cod_acreedor);
        }

        public ErrorDto Crd_PolizasAcreedores_Guardar(
            int CodEmpresa,
            string usuario,
            CrdPolizasAcreedoresGridSaveDto datos)
        {
           return _db.Crd_PolizasAcreedores_Guardar(CodEmpresa, usuario, datos);
        }

        #endregion

        #region Busquedas

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Catalogo_Retencion_Buscar(
            int CodEmpresa,
            string? codigo = null,
            string? ordenarPor = "item")
        {
            return _db.Crd_Catalogo_Retencion_Buscar(CodEmpresa, codigo, ordenarPor);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Catalogo_Cargos_Buscar(
           int CodEmpresa,
           string? codigo = null,
           string? ordenarPor = "item")
        {
            return _db.Crd_Catalogo_Cargos_Buscar(CodEmpresa, codigo, ordenarPor); 
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Catalogo_Unidades_Buscar(
            int CodEmpresa,
            string? codigo = null,
            string? ordenarPor = "item")
        {
            return _db.Crd_Catalogo_Unidades_Buscar(CodEmpresa, codigo, ordenarPor);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Catalogo_CentroCostos_Buscar(
            int CodEmpresa,
            string? codigo = null,
            string? ordenarPor = "item")
        {
            return _db.Crd_Catalogo_CentroCostos_Buscar(CodEmpresa, codigo, ordenarPor);
        }

        #endregion
    }
}
