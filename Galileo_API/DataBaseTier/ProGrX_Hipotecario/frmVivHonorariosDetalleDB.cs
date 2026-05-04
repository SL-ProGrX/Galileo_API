using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivHonorariosDetalleDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmVivHonorariosDetalleDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivHonorariosDetalleDb(PortalDB portalDb, MSecurityMainDb bitacora)
        {
            _portalDb = portalDb;
            _bitacora = bitacora;
        }

        /// <summary>
        /// Carga la información de operación, deudor y profesional.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <param name="idContacto"></param>
        /// <returns></returns>
        public ErrorDto<VivHonorariosDetalleOperacionData?> VivHonorariosDetalle_ObtenerOperacion(
            int codEmpresa, int operacion, int idContacto)
        {
            const string query = @"
                SELECT TOP 1
                    ISNULL(R.Id_Solicitud, 0) AS operacion,
                    RTRIM(ISNULL(R.Cedula, '')) AS cedula,
                    RTRIM(ISNULL(S.Nombre, '')) AS nombre,
                    RTRIM(ISNULL(VC.Identificacion, '')) AS identificacion_contacto,
                    RTRIM(ISNULL(VC.Nombre, '')) AS nombre_contacto
                FROM REG_CREDITOS AS R
                INNER JOIN SOCIOS AS S
                    ON R.Cedula = S.Cedula
                LEFT JOIN ViviendaContactos AS VC
                    ON VC.IdContacto = @IdContacto
                WHERE R.Id_Solicitud = @Operacion;";

            return DbHelper.ExecuteSingleQuery<VivHonorariosDetalleOperacionData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    Operacion = operacion,
                    IdContacto = idContacto
                });
        }

        /// <summary>
        /// Obtiene los rubros de honorarios.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<VivHonorariosDetalleLineaData>> VivHonorariosDetalle_ObtenerLineas(int codEmpresa)
        {
            const string query = @"
                SELECT
                    RTRIM(ISNULL(Descripcion, '')) AS descripcion,
                    CAST(0 AS decimal(12, 2)) AS monto,
                    RTRIM(ISNULL(Codigo, '')) AS codigo
                FROM ViviendaTiposDesembolsos AS Concepto
                WHERE NivelFormalizacion = 1
                  AND AplicaAbogado = 1;";

            return DbHelper.ExecuteListQuery<VivHonorariosDetalleLineaData>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Guarda el detalle de honorarios.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivHonorariosDetalle_Guardar(
            int codEmpresa, string usuario, VivHonorariosDetalleGuardarRequest request)
        {
            if (request.id_contacto <= 0 || request.id_garantia <= 0 || string.IsNullOrWhiteSpace(request.profesional))
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Faltan datos obligatorios para guardar el detalle de honorarios."
                };
            }

            var lineas = request.lineas ?? new List<VivHonorariosDetalleGuardarLinea>();

            var respAsignacion = EjecutarAsignacionGarantia(
                codEmpresa,
                request.id_garantia,
                request.id_contacto,
                request.profesional,
                usuario);

            if (respAsignacion.Code < 0)
            {
                return respAsignacion;
            }

            foreach (var linea in lineas.Where(l => l != null && l.monto > 0))
            {
                var respDetalle = EjecutarDetalleHonorario(
                    codEmpresa,
                    request.id_contacto,
                    request.id_garantia,
                    request.profesional,
                    linea.codigo,
                    linea.monto,
                    usuario);

                if (respDetalle.Code < 0)
                {
                    return respDetalle;
                }
            }

            var respPendiente = EjecutarDesembolsoPendiente(
                codEmpresa,
                request.id_contacto,
                request.id_garantia,
                usuario);

            if (respPendiente.Code < 0)
            {
                return respPendiente;
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                "APLICA - WEB",
                $"Asignación Garantia Vivienda: {request.id_garantia} Contacto: {request.id_contacto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion registrada correctamente..."
            };
        }

        private ErrorDto EjecutarAsignacionGarantia(
            int codEmpresa, int idGarantia, int idContacto, string profesional, string usuario)
        {
            const string sql = @"
                EXEC dbo.spCRDVivAsingaGarantia_A
                    @IdGarantia,
                    @IdContacto,
                    @Profesional,
                    @AsingacionUsuario,
                    @AsignacionFecha;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = idGarantia,
                    IdContacto = idContacto,
                    Profesional = NormalizarTexto(profesional),
                    AsingacionUsuario = usuario,
                    AsignacionFecha = DBNull.Value
                });
        }

        private ErrorDto EjecutarDetalleHonorario(
            int codEmpresa, int idContacto, int idGarantia, string profesional, string codigo, decimal monto, string usuario)
        {
            const string sql = @"
                EXEC spCRDVivDesembolsosPendientesDT_A
                    @IdContacto,
                    @IdGarantia,
                    @Tipo,
                    @Codigo,
                    @Monto,
                    @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdContacto = idContacto,
                    IdGarantia = idGarantia,
                    Tipo = NormalizarTexto(profesional),
                    Codigo = NormalizarTexto(codigo),
                    Monto = monto,
                    Usuario = usuario
                });
        }

        private ErrorDto EjecutarDesembolsoPendiente(
            int codEmpresa, int idContacto, int idGarantia, string usuario)
        {
            const string sql = @"
                EXEC spCRDViviendaDesembolsoPendiente
                    @IdContacto,
                    @Tipo,
                    @IdGarantia,
                    @Usuario;";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdContacto = idContacto,
                    Tipo = "A",
                    IdGarantia = idGarantia,
                    Usuario = usuario
                });
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }
    }
}
