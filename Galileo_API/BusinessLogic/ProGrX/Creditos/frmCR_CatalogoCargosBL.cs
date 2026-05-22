using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoCargosBl
    {
        private readonly FrmCrCatalogoCargosDb _db;

        public FrmCrCatalogoCargosBl(IConfiguration config)
        {
            _db = new FrmCrCatalogoCargosDb(config);
        }

        public ErrorDto<List<CrCatalogoCargoData>> CrCatalogoCargos_Obtener(int codEmpresa)
            => _db.CrCatalogoCargos_Obtener(codEmpresa);

        public ErrorDto CrCatalogoCargos_Guardar(int codEmpresa, CrCatalogoCargoGuardarRequest request)
            => _db.CrCatalogoCargos_Guardar(codEmpresa, request);

        public ErrorDto CrCatalogoCargos_Eliminar(int codEmpresa, CrCatalogoCargoEliminarRequest request)
            => _db.CrCatalogoCargos_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrCatalogoCargoArbolData>> CrCatalogoCargos_AsignacionArbol_Obtener(int codEmpresa)
            => _db.CrCatalogoCargos_AsignacionArbol_Obtener(codEmpresa);

        public ErrorDto<List<CrCatalogoCargoAsignacionData>> CrCatalogoCargos_AsignacionCargos_Obtener(
            int codEmpresa,
            CrCatalogoCargoAsignacionObtenerRequest request)
            => _db.CrCatalogoCargos_AsignacionCargos_Obtener(codEmpresa, request);

        public ErrorDto CrCatalogoCargos_Asignacion_Guardar(int codEmpresa, CrCatalogoCargoAsignacionGuardarRequest request)
            => _db.CrCatalogoCargos_Asignacion_Guardar(codEmpresa, request);

        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogoCargos_TablaAplicacionCargos_Obtener(int codEmpresa)
            => _db.CrCatalogoCargos_TablaAplicacionCargos_Obtener(codEmpresa);

        public ErrorDto<List<CrCatalogoCargoTablaAplicacionData>> CrCatalogoCargos_TablaAplicacion_Obtener(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionObtenerRequest request)
            => _db.CrCatalogoCargos_TablaAplicacion_Obtener(codEmpresa, request);

        public ErrorDto CrCatalogoCargos_TablaAplicacion_Guardar(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionGuardarRequest request)
            => _db.CrCatalogoCargos_TablaAplicacion_Guardar(codEmpresa, request);

        public ErrorDto CrCatalogoCargos_TablaAplicacion_Eliminar(
            int codEmpresa,
            CrCatalogoCargoTablaAplicacionEliminarRequest request)
            => _db.CrCatalogoCargos_TablaAplicacion_Eliminar(codEmpresa, request);
    }
}