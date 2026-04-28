using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivControlAsignacionGarantiaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmVivControlAsignacionGarantiaDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivControlAsignacionGarantiaDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de garantías pendientes de asignación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipoProfesional"></param>
        /// <returns></returns>
        public ErrorDto<List<VivControlAsignacionGarantiaPendienteData>> VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes(int codEmpresa, string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Pendiente @TipoProfesional;";

            return DbHelper.ExecuteListQuery<VivControlAsignacionGarantiaPendienteData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    TipoProfesional = tipoProfesional?.Trim()
                });
        }

        /// <summary>
        /// Obtiene la lista de profesionales por zona y garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idZona"></param>
        /// <param name="tipoProfesional"></param>
        /// <param name="idGarantia"></param>
        /// <returns></returns>
        public ErrorDto<List<VivControlAsignacionProfesionalData>> VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales(int codEmpresa, int idZona, string tipoProfesional, long idGarantia)
        {
            const string query = @"EXEC spCRDVivTraerProfAsingaGarantia @IdZona, @TipoProfesional, @IdGarantia;";

            return DbHelper.ExecuteListQuery<VivControlAsignacionProfesionalData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdZona = idZona,
                    TipoProfesional = tipoProfesional?.Trim(),
                    IdGarantia = idGarantia
                });
        }

        /// <summary>
        /// Asigna una garantía a un profesional.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivControlAsignacionGarantia_Asignacion_Aplicar(int codEmpresa, string usuario, VivControlAsignacionGarantiaAsignarRequest request)
        {
            const string sql = @"EXEC spCRDVivAsingaGarantia_A 
                @IdGarantia,
                @IdContacto,
                @TipoProfesional,
                @Usuario,
                @FechaAsignacion;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    TipoProfesional = request.tipoProfesional?.Trim(),
                    Usuario = usuario,
                    FechaAsignacion = request.fecha_asignacion
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Aplica - WEB",
                detalle: $"Asignación Garantía Hipotecaria {request.idGarantia} Contacto: {request.idContacto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion registrada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina la asignación de una garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idGarantia"></param>
        /// <param name="idContacto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto VivControlAsignacionGarantia_Asignacion_Borrar(int codEmpresa, long idGarantia, int idContacto, string usuario)
        {
            const string sql = @"EXEC spCRDVivAsingaGarantia_B @IdGarantia, @IdContacto;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = idGarantia,
                    IdContacto = idContacto
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Borrar - WEB",
                detalle: $"Asignación Garantía Hipotecaria {idGarantia} Contacto: {idContacto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = "Informacion eliminada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene la lista de profesionales según el tipo de proceso del formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipoLista"></param>
        /// <param name="tipoProfesional"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> VivControlAsignacionGarantia_ObtenerProfesionales(
            int codEmpresa, string tipoLista, string tipoProfesional)
        {
            string query = tipoLista?.Trim().ToUpper() switch
            {
                "ENTREGA_PROF" => @"
            SELECT DISTINCT
                VGT.IdContacto AS item,
                VC.Nombre AS descripcion
            FROM ViviendaGarantiaTramite AS VGT
            INNER JOIN ViviendaContactos AS VC
                ON VGT.IdContacto = VC.IdContacto
            WHERE VGT.AsignacionFecha IS NOT NULL
              AND VGT.EntregaFecha IS NULL
              AND VGT.Tipo = @TipoProfesional
            ORDER BY VC.Nombre;",

                "RECEPCION_PROF" => @"
            SELECT DISTINCT
                VGT.IdContacto AS item,
                VC.Nombre AS descripcion
            FROM ViviendaGarantiaTramite AS VGT
            INNER JOIN ViviendaContactos AS VC
                ON VGT.IdContacto = VC.IdContacto
            WHERE VGT.EntregaFecha IS NOT NULL
              AND VGT.RecepcionFecha IS NULL
              AND VGT.Tipo = @TipoProfesional
            ORDER BY VC.Nombre;",

                "REGISTRO_PROF" => @"
            SELECT DISTINCT
                VGT.IdContacto AS item,
                VC.Nombre AS descripcion
            FROM ViviendaGarantiaTramite AS VGT
            INNER JOIN ViviendaContactos AS VC
                ON VGT.IdContacto = VC.IdContacto
            WHERE VGT.EntregaFecha IS NOT NULL
              AND VGT.RecepcionFecha IS NOT NULL
              AND VGT.RegistroFecha IS NULL
              AND VGT.Tipo = @TipoProfesional
            ORDER BY VC.Nombre;",

                "FIRMAS_PROF" => @"
            SELECT DISTINCT
                VGT.IdContacto AS item,
                VC.Nombre AS descripcion
            FROM ViviendaGarantiaTramite AS VGT
            INNER JOIN ViviendaContactos AS VC
                ON VGT.IdContacto = VC.IdContacto
            WHERE VGT.EntregaFecha IS NOT NULL
              AND VGT.EntregaUsuario IS NOT NULL
              AND VGT.FirmasFecha IS NULL
              AND VGT.Tipo = @TipoProfesional
            ORDER BY VC.Nombre;",

                "RECIBO_PROF" => @"
            SELECT DISTINCT
                VGT.IdContacto AS item,
                VC.Nombre AS descripcion
            FROM ViviendaGarantiaTramite AS VGT
            INNER JOIN ViviendaContactos AS VC
                ON VGT.IdContacto = VC.IdContacto
            WHERE VGT.EntregaFecha IS NOT NULL
              AND VGT.FirmasFecha IS NOT NULL
              AND VGT.RegistroFecha IS NULL
              AND VGT.Tipo = @TipoProfesional
            ORDER BY VC.Nombre;",

                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Tipo de lista inválido.",
                    Result = null
                };
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    TipoProfesional = tipoProfesional?.Trim()
                });
        }

        /// <summary>
        /// Obtiene la lista de garantías del tab de entrega por profesional.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idContacto"></param>
        /// <param name="tipoProfesional"></param>
        /// <returns></returns>
        public ErrorDto<List<VivControlEntregaGarantiaData>> VivControlAsignacionGarantia_Entrega_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Entrega @IdContacto, @TipoProfesional;";

            return DbHelper.ExecuteListQuery<VivControlEntregaGarantiaData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdContacto = idContacto,
                    TipoProfesional = tipoProfesional?.Trim()
                });
        }

        /// <summary>
        /// Aplica o elimina la entrega de una garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivControlAsignacionGarantia_Entrega_Aplicar(int codEmpresa, string usuario, VivControlEntregaGarantiaRequest request)
        {
            const string sql = @"EXEC spCRDVivEntregaGarantia_M @IdGarantia, @IdContacto, @Usuario, @Aplicar;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = request.aplicar?.Trim()
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: request.aplicar == "S" ? "APLICA - WEB" : "BORRA - WEB",
                detalle: $"Entrega Garantía Hipotecaria {request.idGarantia} Contacto: {request.idContacto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = request.aplicar == "S"
                    ? "Informacion registrada satisfactoriamente..."
                    : "Informacion eliminada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene la última observación de la garantía para el profesional seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idGarantia"></param>
        /// <param name="tipoProfesional"></param>
        /// <returns></returns>
        public ErrorDto<VivControlAsignacionGarantiaNotaData?> VivControlAsignacionGarantia_ObtenerUltimaNota(int codEmpresa, long idGarantia, string tipoProfesional)
        {
            const string query = @"
                SELECT TOP 1
                    VGT.Nota AS ultima_nota,
                    CASE VGT.Estado
                        WHEN 'R' THEN 'Garantía Registrada'
                        WHEN 'X' THEN 'Proceso de avaluo'
                        WHEN 'A' THEN 'Avaluo Registrado'
                        WHEN 'Y' THEN 'Proceso de registro'
                        WHEN 'S' THEN 'Solicitada'
                        ELSE ''
                    END AS estado,
                    VGT.Usuario AS usuario,
                    CONVERT(nvarchar(30), VGT.Fecha, 103) AS fecha_registro,
                    VG.NumeroOperacion AS numero_operacion,
                    VG.NumeroFinca AS numero_finca
                FROM ViviendaGarantiaTramiteNotas AS VGT
                INNER JOIN ViviendaContactos AS VC
                    ON VGT.IdContacto = VC.IdContacto
                INNER JOIN ViviendaGarantia AS VG
                    ON VGT.IdGarantia = VG.IdGarantia
                WHERE VGT.IdGarantia = @IdGarantia
                  AND VGT.Tipo = @TipoProfesional
                ORDER BY VGT.Fecha DESC;";

            return DbHelper.ExecuteSingleQuery<VivControlAsignacionGarantiaNotaData>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    IdGarantia = idGarantia,
                    TipoProfesional = tipoProfesional?.Trim()
                });
        }

        /// <summary>
        /// Obtiene la lista de garantías para el tab de recepción/firma.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idContacto"></param>
        /// <param name="tipoProfesional"></param>
        /// <returns></returns>
        public ErrorDto<List<VivControlRecibeGarantiaData>> VivControlAsignacionGarantia_Recepcion_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Recepcion @IdContacto, @TipoProfesional;";

            return DbHelper.ExecuteListQuery<VivControlRecibeGarantiaData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdContacto = idContacto,
                    TipoProfesional = tipoProfesional?.Trim()
                });
        }

        /// <summary>
        /// Aplica o elimina la recepción/firma de una garantía.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivControlAsignacionGarantia_Recepcion_Aplicar(int codEmpresa, string usuario, VivControlRecibeGarantiaRequest request)
        {
            string sql;
            object param;

            if (request.tipoProfesional?.Trim() == "I")
            {
                sql = @"EXEC spCRDVivRecepcionGarantia_M @IdGarantia, @IdContacto, @Usuario, @Aplicar;";
                param = new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = request.aplicar?.Trim()
                };
            }
            else
            {
                sql = @"EXEC spCRDVivCtlAsignacionGarantia @IdGarantia, @IdContacto, @Usuario, @Aplicar, 'A', 'F';";
                param = new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = request.aplicar?.Trim()
                };
            }

            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: request.aplicar == "S" ? "APLICA - WEB" : "BORRA - WEB",
                detalle: request.tipoProfesional?.Trim() == "I"
                    ? $"Recepción Garantía Hipotecaria {request.idGarantia} Contacto: {request.idContacto}"
                    : $"Asignación Garantía Hipotecaria(F): {request.idGarantia} Contacto: {request.idContacto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = request.aplicar == "S"
                    ? "Informacion registrada satisfactoriamente..."
                    : "Informacion eliminada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Obtiene la lista de garantías para el tab de registro/recibo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idContacto"></param>
        /// <param name="tipoProfesional"></param>
        /// <returns></returns>
        public ErrorDto<List<VivControlRegistroGarantiaData>> VivControlAsignacionGarantia_Registro_ObtenerGarantias(int codEmpresa, long idContacto, string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Registro @IdContacto, @TipoProfesional;";

            return DbHelper.ExecuteListQuery<VivControlRegistroGarantiaData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdContacto = idContacto,
                    TipoProfesional = tipoProfesional?.Trim()
                });
        }

        /// <summary>
        /// Aplica el recibo de garantía inscrita para abogados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivControlAsignacionGarantia_Registro_Aplicar(int codEmpresa, string usuario, VivControlRegistroGarantiaRequest request)
        {
            if (request.tipoProfesional?.Trim() == "I")
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El registro de avalúo de ingenieros se realiza desde frmVivRegistroAvaluo."
                };
            }

            const string sql = @"EXEC spCRDVivCtlAsignacionGarantia @IdGarantia, @IdContacto, @Usuario, @Aplicar, 'A', 'I';";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = request.aplicar?.Trim()
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: request.aplicar == "S" ? "APLICA - WEB" : "BORRA - WEB",
                detalle: $"Asignación Garantía Hipotecaria(I): {request.idGarantia} Contacto: {request.idContacto}"
            );

            return new ErrorDto
            {
                Code = 0,
                Description = request.aplicar == "S"
                    ? "Informacion registrada satisfactoriamente..."
                    : "Informacion eliminada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Registra en bitácora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
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
    }
}
