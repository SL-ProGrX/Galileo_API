using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoReqCarBl
    {
        private readonly FrmCrSeguimientoReqCarDb _db;

        public FrmCrSeguimientoReqCarBl(IConfiguration config)
        {
            _db = new FrmCrSeguimientoReqCarDb(config);
        }

        public ErrorDto<CrSeguimientoReqCarCargaInicialData> CrSeguimientoReqCar_CargaInicial_Obtener(
            int codEmpresa,
            string request)
        {
            CrSeguimientoReqCarCargaInicialRequest filtros =
                JsonConvert.DeserializeObject<CrSeguimientoReqCarCargaInicialRequest>(request)
                ?? new CrSeguimientoReqCarCargaInicialRequest();

            return _db.CrSeguimientoReqCar_CargaInicial_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<List<CrSeguimientoReqCarRequisitoData>> CrSeguimientoReqCar_Requisitos_Obtener(
            int codEmpresa,
            string request)
        {
            CrSeguimientoReqCarRequisitosRequest filtros =
                JsonConvert.DeserializeObject<CrSeguimientoReqCarRequisitosRequest>(request)
                ?? new CrSeguimientoReqCarRequisitosRequest();

            return _db.CrSeguimientoReqCar_Requisitos_Obtener(codEmpresa, filtros);
        }

        public ErrorDto<CrSeguimientoReqCarCargosData> CrSeguimientoReqCar_Cargos_Obtener(
            int codEmpresa,
            string request)
        {
            CrSeguimientoReqCarCargosRequest filtros =
                JsonConvert.DeserializeObject<CrSeguimientoReqCarCargosRequest>(request)
                ?? new CrSeguimientoReqCarCargosRequest();

            return _db.CrSeguimientoReqCar_Cargos_Obtener(codEmpresa, filtros);
        }

        public ErrorDto CrSeguimientoReqCar_Requisitos_Guardar(
            int codEmpresa,
            CrSeguimientoReqCarRequisitosGuardarRequest request)
            => _db.CrSeguimientoReqCar_Requisitos_Guardar(codEmpresa, request);

        public ErrorDto CrSeguimientoReqCar_Cargo_Aplicar(
            int codEmpresa,
            CrSeguimientoReqCarCargoAplicarRequest request)
            => _db.CrSeguimientoReqCar_Cargo_Aplicar(codEmpresa, request);

        public ErrorDto CrSeguimientoReqCar_Prima_Guardar(
            int codEmpresa,
            CrSeguimientoReqCarPrimaGuardarRequest request)
            => _db.CrSeguimientoReqCar_Prima_Guardar(codEmpresa, request);
    }
}