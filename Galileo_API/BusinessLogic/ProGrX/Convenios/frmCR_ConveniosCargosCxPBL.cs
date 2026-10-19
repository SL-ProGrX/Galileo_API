using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosCargosCxPBl
    {
        private readonly FrmCrConveniosCargosCxPDb _db;
        private readonly MConveniosDB _mConvenios;

        private const string EstadoCerrada = "C";
        private const int MaxDocumento = 30;
        private const int MaxDetalle = 500;

        private const string MsgSinOrden = "No se ha indicado el convenio y la orden a consultar.";
        private const string MsgSinUsuario = "Debe indicar el usuario.";
        private const string MsgOrdenNoExiste = "La orden indicada no existe...verifique!";
        private const string MsgOrdenCerrada = "No se puede modificar una orden en estado CERRADA!!!";
        private const string MsgCargoInvalido = "Indique un Código de Cargo válido!";
        private const string MsgMontoInvalido = "El monto del cargo no es válido!";
        private const string MsgMontoExcede = "El monto del cargo EXCEDE el disponible!";

        public FrmCrConveniosCargosCxPBl(IConfiguration config)
        {
            _db = new FrmCrConveniosCargosCxPDb(config);
            _mConvenios = new MConveniosDB(config);
        }

        public ErrorDto<CrCargosCxPCabecera?> CrConveniosCargosCxP_Cabecera_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse<CrCargosCxPCabecera?>(MsgSinOrden, -2, null);

            var respuesta = _db.CrConveniosCargosCxP_Cabecera_Obtener(codEmpresa, codConvenio, codOrden);
            if ((respuesta.Code ?? 0) == 0 && respuesta.Result == null)
                return DbHelper.CreateErrorResponse<CrCargosCxPCabecera?>(MsgOrdenNoExiste, -2, null);

            return respuesta;
        }

        /// <summary>
        /// sbConsultaCargos + sbCalculaTotales: rebajos registrados, total, disponible y vencido.
        /// </summary>
        public ErrorDto<CrCargosCxPConsulta> CrConveniosCargosCxP_Consulta_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse(MsgSinOrden, -2, new CrCargosCxPConsulta());

            var cabecera = CrConveniosCargosCxP_Cabecera_Obtener(codEmpresa, codConvenio, codOrden);
            if ((cabecera.Code ?? 0) != 0 || cabecera.Result == null)
                return DbHelper.CreateErrorResponse(
                    cabecera.Description ?? MsgOrdenNoExiste, cabecera.Code ?? -2, new CrCargosCxPConsulta());

            var lineas = _db.CrConveniosCargosCxP_Lineas_Obtener(codEmpresa, codConvenio, codOrden);
            if ((lineas.Code ?? 0) != 0)
                return DbHelper.CreateErrorResponse(
                    lineas.Description ?? string.Empty, lineas.Code ?? -1, new CrCargosCxPConsulta());

            var listado = lineas.Result ?? new List<CrCargosCxPLinea>();
            var total = listado.Sum(l => l.monto ?? 0m);

            return DbHelper.CreateOkResponse(new CrCargosCxPConsulta
            {
                lineas = listado,
                total = total,
                disponible = cabecera.Result.disponible - total,
                rebajo_vencido = _db.CrConveniosCargosCxP_RebajoVencido_Obtener(codEmpresa, codConvenio, codOrden)
            });
        }

        public ErrorDto<List<CrCargosCxPPendiente>> CrConveniosCargosCxP_Pendientes_Obtener(
            int codEmpresa,
            string codConvenio,
            int codOrden)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return DbHelper.CreateErrorResponse(MsgSinOrden, -2, new List<CrCargosCxPPendiente>());

            return _db.CrConveniosCargosCxP_Pendientes_Obtener(codEmpresa, codConvenio, codOrden);
        }

        public ErrorDto<List<CrCargosCxPCargo>> CrConveniosCargosCxP_Cargos_Obtener(
            int codEmpresa,
            string codConvenio)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse(MsgSinOrden, -2, new List<CrCargosCxPCargo>());

            return _db.CrConveniosCargosCxP_Cargos_Obtener(codEmpresa, codConvenio);
        }

        /// <summary>
        /// txtCargoCod_LostFocus: fxSIFCCodigos("D", código, "CargosProv").
        /// </summary>
        public ErrorDto<string> CrConveniosCargosCxP_CargoDescripcion_Obtener(int codEmpresa, string codCargo)
            => _mConvenios.FxSIFCCodigos(codEmpresa, 0, "D", codCargo, "CargosProv");

        /// <summary>
        /// btnGuardar_Click: valida y registra el rebajo.
        /// </summary>
        public ErrorDto CrConveniosCargosCxP_Registrar(int codEmpresa, CrCargosCxPRegistroRequest request)
        {
            var (cabecera, error) = ObtenerOrdenModificable(
                codEmpresa, request.cod_convenio, request.cod_orden, request.usuario);
            if (error != null)
                return error;

            if (string.IsNullOrWhiteSpace(request.cod_cargo))
                return DbHelper.ErrorResponse(MsgCargoInvalido, -2);

            if (request.monto <= 0)
                return DbHelper.ErrorResponse(MsgMontoInvalido, -2);

            var total = TotalRebajos(codEmpresa, request.cod_convenio, request.cod_orden);
            if (request.monto > cabecera!.disponible - total)
                return DbHelper.ErrorResponse(MsgMontoExcede, -2);

            request.cod_convenio = request.cod_convenio.Trim();
            request.cod_cargo = request.cod_cargo.Trim();
            request.documento = Truncar(request.documento, MaxDocumento);
            request.detalle = Truncar(request.detalle, MaxDetalle);
            request.usuario = request.usuario.Trim();

            var respuesta = _db.CrConveniosCargosCxP_Registrar(codEmpresa, request);

            return (respuesta.Code ?? 0) == 0
                ? DbHelper.OkResponse("Cargo registrado satisfactoriamente.")
                : respuesta;
        }

        /// <summary>
        /// vGrid_KeyDown: edita la línea; sin monto mayor a cero el VB6 no ejecuta nada.
        /// </summary>
        public ErrorDto<decimal?> CrConveniosCargosCxP_Editar(int codEmpresa, CrCargosCxPEdicionRequest request)
        {
            var (_, error) = ObtenerOrdenModificable(
                codEmpresa, request.cod_convenio, request.cod_orden, request.usuario);
            if (error != null)
                return DbHelper.CreateErrorResponse<decimal?>(error.Description ?? string.Empty, error.Code ?? -2);

            if (request.monto <= 0)
                return DbHelper.CreateOkResponse<decimal?>(null, "Sin cambios.");

            request.cod_convenio = request.cod_convenio.Trim();
            request.cod_cargo = (request.cod_cargo ?? string.Empty).Trim();
            request.documento = Truncar(request.documento, MaxDocumento);
            request.detalle = Truncar(request.detalle, MaxDetalle);
            request.usuario = request.usuario.Trim();

            return _db.CrConveniosCargosCxP_Editar(codEmpresa, request);
        }

        /// <summary>
        /// vGrid_ButtonClicked (columna Borrar).
        /// </summary>
        public ErrorDto CrConveniosCargosCxP_Borrar(int codEmpresa, CrCargosCxPBorradoRequest request)
        {
            var (_, error) = ObtenerOrdenModificable(
                codEmpresa, request.cod_convenio, request.cod_orden, request.usuario);
            if (error != null)
                return error;

            request.cod_convenio = request.cod_convenio.Trim();
            request.usuario = request.usuario.Trim();

            var respuesta = _db.CrConveniosCargosCxP_Borrar(codEmpresa, request);

            return (respuesta.Code ?? 0) == 0
                ? DbHelper.OkResponse("Rebajo eliminado satisfactoriamente.")
                : respuesta;
        }

        /// <summary>
        /// Valida usuario y orden, y que la orden no esté Cerrada (vEstado = "C" en el VB6).
        /// </summary>
        private (CrCargosCxPCabecera? cabecera, ErrorDto? error) ObtenerOrdenModificable(
            int codEmpresa,
            string codConvenio,
            int codOrden,
            string usuario)
        {
            if (!OrdenValida(codConvenio, codOrden))
                return (null, DbHelper.ErrorResponse(MsgSinOrden, -2));

            if (string.IsNullOrWhiteSpace(usuario))
                return (null, DbHelper.ErrorResponse(MsgSinUsuario, -2));

            var cabecera = _db.CrConveniosCargosCxP_Cabecera_Obtener(codEmpresa, codConvenio, codOrden);
            if ((cabecera.Code ?? 0) != 0)
                return (null, DbHelper.ErrorResponse(cabecera.Description ?? string.Empty, cabecera.Code ?? -1));

            if (cabecera.Result == null)
                return (null, DbHelper.ErrorResponse(MsgOrdenNoExiste, -2));

            return cabecera.Result.estado == EstadoCerrada
                ? (null, DbHelper.ErrorResponse(MsgOrdenCerrada, -2))
                : (cabecera.Result, null);
        }

        private decimal TotalRebajos(int codEmpresa, string codConvenio, int codOrden)
        {
            var lineas = _db.CrConveniosCargosCxP_Lineas_Obtener(codEmpresa, codConvenio, codOrden);

            return (lineas.Result ?? new List<CrCargosCxPLinea>()).Sum(l => l.monto ?? 0m);
        }

        private static bool OrdenValida(string? codConvenio, int codOrden)
            => !string.IsNullOrWhiteSpace(codConvenio) && codOrden > 0;

        private static string Truncar(string? valor, int largo)
        {
            var texto = (valor ?? string.Empty).Trim();

            return texto.Length > largo ? texto[..largo] : texto;
        }
    }
}
