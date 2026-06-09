using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPPlantillasBL
    {
        private readonly FrmCxPPlantillasDB _db;

        public FrmCxPPlantillasBL(IConfiguration config)
        {
            _db = new FrmCxPPlantillasDB(config);
        }

        public ErrorDto<List<PlantillaDto>> Plantillas_Obtener(int CodEmpresa)
        {
            return _db.Plantillas_Obtener(CodEmpresa);
        }

        public ErrorDto<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            return _db.Unidades_Obtener(CodEmpresa);
        }

        public ErrorDto<List<CentroCosto>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            return _db.CentrosCosto_Obtener(CodEmpresa, Cod_Unidad);
        }

        public ErrorDto<PlantillaDto> PlantillaDetalle_Obtener(int CodEmpresa, string Cod_Plantilla)
        {
            return _db.PlantillaDetalle_Obtener(CodEmpresa, Cod_Plantilla);
        }

        public ErrorDto<PlantillaDto> PlantillaDetalle_Scroll(int CodEmpresa, int scroll, string Cod_Plantilla)
        {
            return _db.PlantillaDetalle_Scroll(CodEmpresa, scroll, Cod_Plantilla);
        }

        public ErrorDto<List<PlantillaAsientoDto>> PlantillaAsientos_Obtener(int CodEmpresa, string Cod_Plantilla)
        {
            return _db.PlantillaAsientos_Obtener(CodEmpresa, Cod_Plantilla);
        }

        public ErrorDto Plantilla_Actualizar(int CodEmpresa, PlantillaDto data)
        {
            return _db.Plantilla_Actualizar(CodEmpresa, data);
        }

        public ErrorDto Plantilla_Insertar(int CodEmpresa, PlantillaDto data)
        {
            return _db.Plantilla_Insertar(CodEmpresa, data);
        }

        public ErrorDto PlantillaAsiento_Insertar(int CodEmpresa, PlantillaAsientoDto data)
        {
            return _db.PlantillaAsiento_Insertar(CodEmpresa, data);
        }

        public ErrorDto PlantillaAsiento_Actualizar(int CodEmpresa, PlantillaAsientoDto data)
        {
            return _db.PlantillaAsiento_Actualizar(CodEmpresa, data);
        }

        public ErrorDto PlantillaAsiento_Borrar(int CodEmpresa, PlantillaAsientoDto data)
        {
            return _db.PlantillaAsiento_Borrar(CodEmpresa, data);
        }

        public ErrorDto Plantilla_Borrar(int CodEmpresa, string Cod_Plantilla)
        {
            return _db.Plantilla_Borrar(CodEmpresa, Cod_Plantilla);
        }
    }//end class
}//end namespace