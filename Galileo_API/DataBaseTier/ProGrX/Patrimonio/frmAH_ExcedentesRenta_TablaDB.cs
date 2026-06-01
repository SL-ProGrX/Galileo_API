using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhExcedentesRentaTablaDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 2;

        public FrmAhExcedentesRentaTablaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la tabla de renta de excedentes.
        /// </summary>ValidarRequest

        public ErrorDto<List<RentaExcedenteDto>> AH_ExcedentesRentaTabla_Obtener(int codEmpresa)
        {
            const string sql = @"
SELECT
    ID_RENTA AS Id_Renta,
    DESDE AS Desde,
    HASTA AS Hasta,
    PORCENTAJE AS Porcentaje
FROM EXC_RENTA_TABLA
ORDER BY ID_RENTA;";

            return DbHelper.ExecuteListQuery<RentaExcedenteDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Inserta o actualiza un registro de la tabla de renta.
        /// </summary>
        public ErrorDto AH_ExcedentesRentaTabla_Guardar(int codEmpresa, string usuario, RentaExcedenteDto request)
        {
            var validacion = ValidarRequest(request);
            if (validacion.Code < 0)
                return validacion;

            return request.Id_Renta <= 0
                ? InsertarRenta(codEmpresa, usuario, request)
                : ActualizarRenta(codEmpresa, usuario, request);
        }

        /// <summary>
        /// Elimina un registro de la tabla de renta.
        /// </summary>
        public ErrorDto AH_ExcedentesRentaTabla_Eliminar(int codEmpresa, int idRenta, string usuario)
        {
            if (idRenta <= 0)
                return DbHelper.ErrorResponse("Debe indicar un Id_Renta valido.", -2);

            const string sql = @"DELETE EXC_RENTA_TABLA WHERE ID_RENTA = @Id_Renta;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new { Id_Renta = idRenta });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Elimina - WEB", $"Excedentes: Tabla Renta Id: {idRenta}");
            return resp;
        }

        private ErrorDto InsertarRenta(int codEmpresa, string usuario, RentaExcedenteDto request)
        {
            var idRenta = ObtenerSiguienteIdRenta(codEmpresa);
            if (idRenta <= 0)
                return DbHelper.ErrorResponse("No fue posible generar el Id_Renta.", -2);

            const string sql = @"
INSERT INTO EXC_RENTA_TABLA
(
    ID_RENTA,
    DESDE,
    HASTA,
    PORCENTAJE,
    registro_fecha,
    registro_usuario
)
VALUES
(
    @Id_Renta,
    @Desde,
    @Hasta,
    @Porcentaje,
    dbo.MyGetdate(),
    @Usuario
);";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Id_Renta = idRenta,
                    request.Desde,
                    request.Hasta,
                    request.Porcentaje,
                    Usuario = usuario
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Registra - WEB", $"Excedentes: Tabla Renta Id: {idRenta}");
            return DbHelper.OkResponse("Informacion guardada satisfactoriamente...");
        }

        private ErrorDto ActualizarRenta(int codEmpresa, string usuario, RentaExcedenteDto request)
        {
            const string sql = @"
UPDATE EXC_RENTA_TABLA
SET
    DESDE = @Desde,
    HASTA = @Hasta,
    PORCENTAJE = @Porcentaje
WHERE ID_RENTA = @Id_Renta;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    request.Id_Renta,
                    request.Desde,
                    request.Hasta,
                    request.Porcentaje
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(codEmpresa, usuario, "Modifica - WEB", $"Excedentes: Tabla Renta Id: {request.Id_Renta}");
            return DbHelper.OkResponse("Informacion guardada satisfactoriamente...");
        }

        private static ErrorDto ValidarRequest(RentaExcedenteDto? request)
        {
            if (request == null)
                return DbHelper.ErrorResponse("Debe indicar la informacion a guardar.", -2);

            if (request.Desde < 0 || request.Hasta < 0 || request.Porcentaje < 0)
                return DbHelper.ErrorResponse("Los valores no pueden ser negativos.", -2);

            if (request.Hasta < request.Desde)
                return DbHelper.ErrorResponse("El valor Hasta debe ser mayor o igual al valor Desde.", -2);

            return DbHelper.OkResponse("Validacion correcta.");
        }

        private int ObtenerSiguienteIdRenta(int codEmpresa)
        {
            const string sql = @"
SELECT ISNULL(MAX(ID_RENTA), 0) + 1
FROM EXC_RENTA_TABLA;";

            var resp = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sql, 0);
            return resp.Code < 0 ? 0 : resp.Result;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
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
