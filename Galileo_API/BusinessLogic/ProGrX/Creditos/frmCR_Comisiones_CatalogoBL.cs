using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrComisionesCatalogoBL
    {
        private readonly FrmCrComisionesCatalogoDB _db;

        public FrmCrComisionesCatalogoBL(IConfiguration config)
        {
            _db = new FrmCrComisionesCatalogoDB(config);
        }

        public ErrorDto<List<CrComisionesCatalogoData>> Cr_ComisionesCatalogo_Obtener(int codEmpresa)
            => _db.Cr_ComisionesCatalogo_Obtener(codEmpresa);

        public ErrorDto Cr_ComisionesCatalogo_Guardar(int codEmpresa, CrComisionesCatalogoGuardarRequest request)
            => _db.Cr_ComisionesCatalogo_Guardar(codEmpresa, request);

        public ErrorDto Cr_ComisionesCatalogo_Eliminar(int codEmpresa, CrComisionesCatalogoEliminarRequest request)
            => _db.Cr_ComisionesCatalogo_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrComisionesCatalogoPorcentajeData>> Cr_ComisionesCatalogo_Porcentajes_Obtener(
            int codEmpresa,
            CrComisionesCatalogoPorcentajesRequest request)
            => _db.Cr_ComisionesCatalogo_Porcentajes_Obtener(codEmpresa, request);

        public ErrorDto Cr_ComisionesCatalogo_Porcentaje_Guardar(
            int codEmpresa,
            CrComisionesCatalogoPorcentajeGuardarRequest request)
            => _db.Cr_ComisionesCatalogo_Porcentaje_Guardar(codEmpresa, request);

        public ErrorDto Cr_ComisionesCatalogo_Porcentaje_Eliminar(
            int codEmpresa,
            CrComisionesCatalogoPorcentajeEliminarRequest request)
            => _db.Cr_ComisionesCatalogo_Porcentaje_Eliminar(codEmpresa, request);

        public ErrorDto<List<CrComisionesCatalogoLineaData>> Cr_ComisionesCatalogo_Lineas_Obtener(
            int codEmpresa,
            CrComisionesCatalogoLineasRequest request)
            => _db.Cr_ComisionesCatalogo_Lineas_Obtener(codEmpresa, request);

        public ErrorDto Cr_ComisionesCatalogo_Linea_Asignar(
            int codEmpresa,
            CrComisionesCatalogoLineaAsignarRequest request)
            => _db.Cr_ComisionesCatalogo_Linea_Asignar(codEmpresa, request);

        public ErrorDto<CrComisionesCatalogoCuentaLookupData?> Cr_ComisionesCatalogo_Cuenta_Obtener(
            int codEmpresa,
            string cuenta)
            => _db.Cr_ComisionesCatalogo_Cuenta_Obtener(codEmpresa, cuenta);

    }
}
