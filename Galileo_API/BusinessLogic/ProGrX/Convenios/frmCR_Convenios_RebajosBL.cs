using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosRebajosBl
    {
        private const int CodigoMensajeControlado = -2;

        private const string MensajeRequestRequerido =
            "La información del rebajo es requerida.";

        private const string MensajeTransaccionRequerida =
            "El número de transacción es requerido.";

        private readonly FrmCrConveniosRebajosDb _db;

        public FrmCrConveniosRebajosBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _db = new FrmCrConveniosRebajosDb(config);
        }

        public ErrorDto<CrConveniosRebajosInicialData?>
            CR_Convenios_Rebajos_Inicializar(
                int codEmpresa)
        {
            return _db.CR_Convenios_Rebajos_Inicializar(
                codEmpresa);
        }

        public ErrorDto<List<CrConvenioConsultaData>>
            CR_Convenios_Rebajos_Convenios_Obtener(
                int codEmpresa)
        {
            return _db
                .CR_Convenios_Rebajos_Convenios_Obtener(
                    codEmpresa);
        }

        public ErrorDto<CrConvenioConsultaData?>
            CR_Convenios_Rebajos_Convenio_Obtener(
                int codEmpresa,
                string? codConvenio,
                string? descripcion)
        {
            return _db
                .CR_Convenios_Rebajos_Convenio_Obtener(
                    codEmpresa,
                    codConvenio,
                    descripcion);
        }

        public ErrorDto<CrConvenioConsultaData?>
            CR_Convenios_Rebajos_Convenio_Navegar(
                int codEmpresa,
                string? codConvenio,
                bool siguiente)
        {
            return _db
                .CR_Convenios_Rebajos_Convenio_Navegar(
                    codEmpresa,
                    codConvenio,
                    siguiente);
        }

        public ErrorDto<List<CrConveniosRebajosCargoData>>
            CR_Convenios_Rebajos_Cargos_Obtener(
                int codEmpresa,
                string? codConvenio)
        {
            return _db
                .CR_Convenios_Rebajos_Cargos_Obtener(
                    codEmpresa,
                    codConvenio);
        }

        public ErrorDto
            <List<CrConveniosRebajosTransaccionBusquedaData>>
            CR_Convenios_Rebajos_Transacciones_Obtener(
                int codEmpresa,
                string? codConvenio)
        {
            return _db
                .CR_Convenios_Rebajos_Transacciones_Obtener(
                    codEmpresa,
                    codConvenio);
        }

        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Transaccion_Obtener(
                int codEmpresa,
                string? codConvenio,
                int idTransac)
        {
            return _db
                .CR_Convenios_Rebajos_Transaccion_Obtener(
                    codEmpresa,
                    codConvenio,
                    idTransac);
        }

        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Transaccion_Navegar(
                int codEmpresa,
                string? codConvenio,
                int idTransac,
                bool siguiente)
        {
            return _db
                .CR_Convenios_Rebajos_Transaccion_Navegar(
                    codEmpresa,
                    codConvenio,
                    idTransac,
                    siguiente);
        }

        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Registrar(
                int codEmpresa,
                CrConveniosRebajosGuardarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeRequestRequerido,
                        CodigoMensajeControlado);
            }

            request.id_transac = 0;

            return _db.CR_Convenios_Rebajos_Guardar(
                codEmpresa,
                request);
        }

        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Actualizar(
                int codEmpresa,
                CrConveniosRebajosGuardarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeRequestRequerido,
                        CodigoMensajeControlado);
            }

            if (request.id_transac <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeTransaccionRequerida,
                        CodigoMensajeControlado);
            }

            return _db.CR_Convenios_Rebajos_Guardar(
                codEmpresa,
                request);
        }

        public ErrorDto
            CR_Convenios_Rebajos_Eliminar(
                int codEmpresa,
                CrConveniosRebajosEliminarRequest? request)
        {
            return _db.CR_Convenios_Rebajos_Eliminar(
                codEmpresa,
                request);
        }

        public ErrorDto<List<CrConveniosRebajosCobroData>>
            CR_Convenios_Rebajos_Cobros_Obtener(
                int codEmpresa,
                string? codConvenio,
                int estado)
        {
            return _db
                .CR_Convenios_Rebajos_Cobros_Obtener(
                    codEmpresa,
                    codConvenio,
                    estado);
        }

        public ErrorDto<List<CrConveniosRebajosPagoData>>
            CR_Convenios_Rebajos_Pagos_Obtener(
                int codEmpresa,
                string? codConvenio,
                int idTransac)
        {
            return _db
                .CR_Convenios_Rebajos_Pagos_Obtener(
                    codEmpresa,
                    codConvenio,
                    idTransac);
        }
    }
}