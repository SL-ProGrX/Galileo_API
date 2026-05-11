using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaTablaImpRentaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaTablaImpRentaDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config))
        {
        }

        public FrmPreaTablaImpRentaDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Obtiene la tabla de impuesto de renta para estudio de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPreaTablaImpRentaData>> CrPreaTablaImpRenta_Obtener(int codEmpresa)
        {
            const string query = @"
                SELECT
                    idx,
                    desde,
                    hasta,
                    porcentaje
                FROM crd_prea_tabla_impuesto
                ORDER BY desde;";

            return DbHelper.ExecuteListQuery<CrdPreaTablaImpRentaData>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Guarda un rango de impuesto de renta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTablaImpRenta_Guardar(int codEmpresa, string usuario, CrdPreaTablaImpRentaData request)
        {
            var validacion = ValidarRequest(request);
            if (validacion.Code < 0)
                return validacion;

            var resp = request.idx > 0
                ? ActualizarTablaImpRenta(codEmpresa, usuario, request)
                : InsertarTablaImpRenta(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un rango de impuesto de renta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idx"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTablaImpRenta_Eliminar(int codEmpresa, int idx, string usuario)
        {
            if (idx <= 0)
                return DbHelper.ErrorResponse("Debe indicar un ID valido.", -2);

            const string sqlDelete = @"
                DELETE Crd_Prea_Tabla_Impuesto
                WHERE idx = @Idx;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Idx = idx
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Rango Estudio de Credito - Impuesto Renta [ID]: {idx}");

            return respDelete;
        }

        /// <summary>
        /// Inserta un rango nuevo de impuesto de renta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarTablaImpRenta(int codEmpresa, string usuario, CrdPreaTablaImpRentaData request)
        {
            const string sqlInsert = @"
                INSERT INTO Crd_Prea_Tabla_Impuesto
                (
                    desde,
                    hasta,
                    porcentaje,
                    registro_usuario,
                    registro_fecha
                )
                VALUES
                (
                    @Desde,
                    @Hasta,
                    @Porcentaje,
                    @Usuario,
                    GETDATE()
                );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    Desde = request.desde,
                    Hasta = request.hasta,
                    Porcentaje = request.porcentaje,
                    Usuario = usuario
                });

            if (respInsert.Code < 0)
                return respInsert;

            var ultimoIdx = ObtenerUltimoIdx(codEmpresa);

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Rango Estudio de Credito - Impuesto Renta [ID]: {ultimoIdx}");

            return respInsert;
        }

        /// <summary>
        /// Actualiza un rango existente de impuesto de renta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarTablaImpRenta(int codEmpresa, string usuario, CrdPreaTablaImpRentaData request)
        {
            const string sqlUpdate = @"
                UPDATE Crd_Prea_Tabla_Impuesto
                SET
                    desde = @Desde,
                    hasta = @Hasta,
                    porcentaje = @Porcentaje,
                    modifica_usuario = @Usuario,
                    modifica_fecha = GETDATE()
                WHERE idx = @Idx;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Idx = request.idx,
                    Desde = request.desde,
                    Hasta = request.hasta,
                    Porcentaje = request.porcentaje,
                    Usuario = usuario
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Rango Estudio de Credito - Impuesto Renta [ID]: {request.idx}");

            return respUpdate;
        }

        /// <summary>
        /// Valida el rango recibido antes de guardar.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static ErrorDto ValidarRequest(CrdPreaTablaImpRentaData request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("Debe indicar la informacion a guardar.", -2);

            if (request.desde < 0 || request.hasta < 0 || request.porcentaje < 0)
                return DbHelper.ErrorResponse("Los valores no pueden ser negativos.", -2);

            if (request.hasta < request.desde)
                return DbHelper.ErrorResponse("El monto hasta debe ser mayor o igual al monto desde.", -2);

            return DbHelper.OkResponse("Validacion correcta.");
        }

        /// <summary>
        /// Obtiene el ultimo identificador generado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private int ObtenerUltimoIdx(int codEmpresa)
        {
            const string sql = @"
                SELECT ISNULL(MAX(idx), 0)
                FROM Crd_Prea_Tabla_Impuesto;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                0);

            if (resp.Code < 0)
                return 0;

            return resp.Result;
        }

        /// <summary>
        /// Registra el movimiento en bitacora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
