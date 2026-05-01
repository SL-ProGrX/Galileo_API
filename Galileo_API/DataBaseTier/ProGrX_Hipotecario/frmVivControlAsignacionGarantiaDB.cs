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
        public ErrorDto<List<VivControlAsignacionGarantiaPendienteData>> VivControlAsignacionGarantia_Asignacion_ObtenerGarantiasPendientes(
            int codEmpresa,
            string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Pendiente @TipoProfesional;";

            return DbHelper.ExecuteListQuery<VivControlAsignacionGarantiaPendienteData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    TipoProfesional = NormalizarTexto(tipoProfesional)
                });
        }

        /// <summary>
        /// Obtiene la lista de profesionales por zona y garantía.
        /// </summary>
        public ErrorDto<List<VivControlAsignacionProfesionalData>> VivControlAsignacionGarantia_Asignacion_ObtenerProfesionales(
            int codEmpresa,
            int idZona,
            string tipoProfesional,
            long idGarantia)
        {
            const string query = @"EXEC spCRDVivTraerProfAsingaGarantia @IdZona, @TipoProfesional, @IdGarantia;";

            return DbHelper.ExecuteListQuery<VivControlAsignacionProfesionalData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdZona = idZona,
                    TipoProfesional = NormalizarTexto(tipoProfesional),
                    IdGarantia = idGarantia
                });
        }

        /// <summary>
        /// Asigna una garantía a un profesional.
        /// </summary>
        public ErrorDto VivControlAsignacionGarantia_Asignacion_Aplicar(
            int codEmpresa,
            string usuario,
            VivControlAsignacionGarantiaAsignarRequest request)
        {
            const string sql = @"EXEC spCRDVivAsingaGarantia_A 
                @IdGarantia,
                @IdContacto,
                @TipoProfesional,
                @Usuario,
                @FechaAsignacion;";

            return EjecutarAccion(
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    TipoProfesional = NormalizarTexto(request.tipoProfesional),
                    Usuario = usuario,
                    FechaAsignacion = request.fecha_asignacion
                },
                usuario,
                "Aplica - WEB",
                $"Asignación Garantía Hipotecaria {request.idGarantia} Contacto: {request.idContacto}",
                "Informacion registrada satisfactoriamente...");
        }

        /// <summary>
        /// Elimina la asignación de una garantía.
        /// </summary>
        public ErrorDto VivControlAsignacionGarantia_Asignacion_Borrar(
            int codEmpresa,
            long idGarantia,
            int idContacto,
            string usuario)
        {
            const string sql = @"EXEC spCRDVivAsingaGarantia_B @IdGarantia, @IdContacto;";

            return EjecutarAccion(
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = idGarantia,
                    IdContacto = idContacto
                },
                usuario,
                "Borrar - WEB",
                $"Asignación Garantía Hipotecaria {idGarantia} Contacto: {idContacto}",
                "Informacion eliminada satisfactoriamente...");
        }

        /// <summary>
        /// Obtiene la lista de profesionales según el tipo de proceso del formulario.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> VivControlAsignacionGarantia_ObtenerProfesionales(
            int codEmpresa,
            string tipoLista,
            string tipoProfesional)
        {
            var query = ObtenerQueryProfesionales(tipoLista);

            if (string.IsNullOrWhiteSpace(query))
            {
                return ErrorResultado<List<DropDownListaGenericaModel>>("Tipo de lista inválido.");
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    TipoProfesional = NormalizarTexto(tipoProfesional)
                });
        }

        /// <summary>
        /// Obtiene la lista de garantías del tab de entrega por profesional.
        /// </summary>
        public ErrorDto<List<VivControlEntregaGarantiaData>> VivControlAsignacionGarantia_Entrega_ObtenerGarantias(
            int codEmpresa,
            long idContacto,
            string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Entrega @IdContacto, @TipoProfesional;";

            return EjecutarListaPorContacto<VivControlEntregaGarantiaData>(
                codEmpresa,
                query,
                idContacto,
                tipoProfesional);
        }

        /// <summary>
        /// Aplica o elimina la entrega de una garantía.
        /// </summary>
        public ErrorDto VivControlAsignacionGarantia_Entrega_Aplicar(
            int codEmpresa,
            string usuario,
            VivControlEntregaGarantiaRequest request)
        {
            const string sql = @"EXEC spCRDVivEntregaGarantia_M @IdGarantia, @IdContacto, @Usuario, @Aplicar;";

            return EjecutarAccion(
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = NormalizarTexto(request.aplicar)
                },
                usuario,
                request.aplicar == "S" ? "APLICA - WEB" : "BORRA - WEB",
                $"Entrega Garantía Hipotecaria {request.idGarantia} Contacto: {request.idContacto}",
                request.aplicar == "S"
                    ? "Informacion registrada satisfactoriamente..."
                    : "Informacion eliminada satisfactoriamente...");
        }

        /// <summary>
        /// Obtiene la última observación de la garantía para el profesional seleccionado.
        /// </summary>
        public ErrorDto<VivControlAsignacionGarantiaNotaData?> VivControlAsignacionGarantia_ObtenerUltimaNota(
            int codEmpresa,
            long idGarantia,
            string tipoProfesional)
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
                    TipoProfesional = NormalizarTexto(tipoProfesional)
                });
        }

        /// <summary>
        /// Obtiene la lista de garantías para el tab de recepción/firma.
        /// </summary>
        public ErrorDto<List<VivControlRecibeGarantiaData>> VivControlAsignacionGarantia_Recepcion_ObtenerGarantias(
            int codEmpresa,
            long idContacto,
            string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Recepcion @IdContacto, @TipoProfesional;";

            return EjecutarListaPorContacto<VivControlRecibeGarantiaData>(
                codEmpresa,
                query,
                idContacto,
                tipoProfesional);
        }

        /// <summary>
        /// Aplica o elimina la recepción/firma de una garantía.
        /// </summary>
        public ErrorDto VivControlAsignacionGarantia_Recepcion_Aplicar(
            int codEmpresa,
            string usuario,
            VivControlRecibeGarantiaRequest request)
        {
            var esIngeniero = NormalizarTexto(request.tipoProfesional) == "I";

            var sql = esIngeniero
                ? @"EXEC spCRDVivRecepcionGarantia_M @IdGarantia, @IdContacto, @Usuario, @Aplicar;"
                : @"EXEC spCRDVivCtlAsignacionGarantia @IdGarantia, @IdContacto, @Usuario, @Aplicar, 'A', 'F';";

            var detalle = esIngeniero
                ? $"Recepción Garantía Hipotecaria {request.idGarantia} Contacto: {request.idContacto}"
                : $"Asignación Garantía Hipotecaria(F): {request.idGarantia} Contacto: {request.idContacto}";

            return EjecutarAccion(
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = NormalizarTexto(request.aplicar)
                },
                usuario,
                request.aplicar == "S" ? "APLICA - WEB" : "BORRA - WEB",
                detalle,
                request.aplicar == "S"
                    ? "Informacion registrada satisfactoriamente..."
                    : "Informacion eliminada satisfactoriamente...");
        }

        /// <summary>
        /// Obtiene la lista de garantías para el tab de registro/recibo.
        /// </summary>
        public ErrorDto<List<VivControlRegistroGarantiaData>> VivControlAsignacionGarantia_Registro_ObtenerGarantias(
            int codEmpresa,
            long idContacto,
            string tipoProfesional)
        {
            const string query = @"EXEC spViv_Garantia_Asignacion_Registro @IdContacto, @TipoProfesional;";

            return EjecutarListaPorContacto<VivControlRegistroGarantiaData>(
                codEmpresa,
                query,
                idContacto,
                tipoProfesional);
        }

        /// <summary>
        /// Aplica el recibo de garantía inscrita para abogados.
        /// </summary>
        public ErrorDto VivControlAsignacionGarantia_Registro_Aplicar(
            int codEmpresa,
            string usuario,
            VivControlRegistroGarantiaRequest request)
        {
            if (NormalizarTexto(request.tipoProfesional) == "I")
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El registro de avalúo de ingenieros se realiza desde frmVivRegistroAvaluo."
                };
            }

            const string sql = @"EXEC spCRDVivCtlAsignacionGarantia @IdGarantia, @IdContacto, @Usuario, @Aplicar, 'A', 'I';";

            return EjecutarAccion(
                codEmpresa,
                sql,
                new
                {
                    IdGarantia = request.idGarantia,
                    IdContacto = request.idContacto,
                    Usuario = usuario,
                    Aplicar = NormalizarTexto(request.aplicar)
                },
                usuario,
                request.aplicar == "S" ? "APLICA - WEB" : "BORRA - WEB",
                $"Asignación Garantía Hipotecaria(I): {request.idGarantia} Contacto: {request.idContacto}",
                request.aplicar == "S"
                    ? "Informacion registrada satisfactoriamente..."
                    : "Informacion eliminada satisfactoriamente...");
        }

        /// <summary>
        /// Obtiene los tiempos de seguimiento por profesional.
        /// </summary>
        public ErrorDto<VivControlTiemposSeguimientoData> VivControlAsignacionGarantia_ObtenerTiemposSeguimiento(
            int codEmpresa,
            string profesional)
        {
            const string query = @"
                SELECT
                    Profesional AS profesional,
                    Proceso AS proceso,
                    TiempoMaximo AS tiempoMaximo,
                    TiempoAlerta AS tiempoAlerta
                FROM ViviendaTiemposSeguimiento
                WHERE Profesional = @Profesional;";

            var resp = DbHelper.ExecuteListQuery<VivControlTiempoSeguimientoRowData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    Profesional = NormalizarTexto(profesional)
                });

            if (resp.Code < 0)
            {
                return MapearError<List<VivControlTiempoSeguimientoRowData>, VivControlTiemposSeguimientoData>(resp);
            }

            var result = CrearTiemposSeguimiento(profesional);

            foreach (var item in resp.Result ?? new List<VivControlTiempoSeguimientoRowData>())
            {
                AplicarTiempoSeguimiento(result, item);
            }

            return new ErrorDto<VivControlTiemposSeguimientoData>
            {
                Code = 0,
                Description = "OK",
                Result = result
            };
        }

        /// <summary>
        /// Obtiene el registro de calculo de honorarios.
        /// </summary>
        public ErrorDto<VivControlHonorariosRegistraData> VivControlAsignacionGarantia_Asignacion_ValidaHonorariosRegistra(
            int codEmpresa,
            int idGarantia)
        {
            const string query = @"
                SELECT
                    RegistraCalHonorarios,
                    RegistraCalHonorariosDt
                FROM ViviendaGarantia
                WHERE IdGarantia = @IdGarantia;";

            var row = DbHelper.ExecuteSingleQuery<dynamic>(
                _portalDb,
                codEmpresa,
                query,
                null,
                new
                {
                    IdGarantia = idGarantia
                });

            if (row.Code < 0)
            {
                return MapearError<dynamic, VivControlHonorariosRegistraData>(row);
            }

            var registra = false;

            if (row.Result != null)
            {
                var reg1 = Convert.ToInt32(row.Result.RegistraCalHonorarios ?? 0);
                var reg2 = Convert.ToInt32(row.Result.RegistraCalHonorariosDt ?? 0);
                registra = reg1 == 1 && reg2 == 1;
            }

            return new ErrorDto<VivControlHonorariosRegistraData>
            {
                Code = 0,
                Description = "OK",
                Result = new VivControlHonorariosRegistraData
                {
                    registraHonorarios = registra
                }
            };
        }

        /// <summary>
        /// Registra en bitácora.
        /// </summary>
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

        #region Helpers
        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        private ErrorDto<List<T>> EjecutarListaPorContacto<T>(
            int codEmpresa,
            string query,
            long idContacto,
            string tipoProfesional)
        {
            return DbHelper.ExecuteListQuery<T>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    IdContacto = idContacto,
                    TipoProfesional = NormalizarTexto(tipoProfesional)
                });
        }

        private ErrorDto EjecutarAccion(
            int codEmpresa,
            string sql,
            object parametros,
            string usuario,
            string movimiento,
            string detalle,
            string mensajeExito)
        {
            var resp = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parametros);

            if (resp.Code < 0)
            {
                return resp;
            }

            RegistrarBitacora(codEmpresa, usuario, movimiento, detalle);

            return new ErrorDto
            {
                Code = 0,
                Description = mensajeExito
            };
        }

        private static ErrorDto<T> ErrorResultado<T>(string descripcion)
        {
            return new ErrorDto<T>
            {
                Code = -1,
                Description = descripcion,
                Result = default
            };
        }

        private static ErrorDto<TDestino> MapearError<TOrigen, TDestino>(ErrorDto<TOrigen> origen)
        {
            return new ErrorDto<TDestino>
            {
                Code = origen.Code,
                Description = origen.Description,
                Result = default
            };
        }

        private static VivControlTiemposSeguimientoData CrearTiemposSeguimiento(string profesional)
        {
            return new VivControlTiemposSeguimientoData
            {
                profesional = NormalizarTexto(profesional),
                gTMaxEntregaAbogado = 0,
                gTAlertaEntregaAbogado = 0,
                gTMaxFirmasAbogado = 0,
                gTAlertaFirmasAbogado = 0,
                gTMaxInscripcionAbogado = 0,
                gTAlertaInscripcionAbogado = 0,
                gTMaxEntregaIngeniero = 0,
                gTAlertaEntregaIngeniero = 0,
                gTMaxRecepcionIngeniero = 0,
                gTAlertaRecepcionIngeniero = 0,
                gTMaxRegistroIngeniero = 0,
                gTAlertaRegistroIngeniero = 0
            };
        }

        private static void AplicarTiempoSeguimiento(
            VivControlTiemposSeguimientoData result,
            VivControlTiempoSeguimientoRowData item)
        {
            var profesional = NormalizarTexto(item.profesional);
            var proceso = NormalizarTexto(item.proceso);

            if (profesional == "A")
            {
                switch (proceso)
                {
                    case "E":
                        result.gTMaxEntregaAbogado = item.tiempoMaximo;
                        result.gTAlertaEntregaAbogado = item.tiempoAlerta;
                        return;

                    case "F":
                        result.gTMaxFirmasAbogado = item.tiempoMaximo;
                        result.gTAlertaFirmasAbogado = item.tiempoAlerta;
                        return;

                    case "I":
                        result.gTMaxInscripcionAbogado = item.tiempoMaximo;
                        result.gTAlertaInscripcionAbogado = item.tiempoAlerta;
                        return;
                }
            }

            if (profesional == "I")
            {
                switch (proceso)
                {
                    case "E":
                        result.gTMaxEntregaIngeniero = item.tiempoMaximo;
                        result.gTAlertaEntregaIngeniero = item.tiempoAlerta;
                        return;

                    case "R":
                        result.gTMaxRecepcionIngeniero = item.tiempoMaximo;
                        result.gTAlertaRecepcionIngeniero = item.tiempoAlerta;
                        return;

                    case "X":
                        result.gTMaxRegistroIngeniero = item.tiempoMaximo;
                        result.gTAlertaRegistroIngeniero = item.tiempoAlerta;
                        return;
                }
            }
        }

        private static string ObtenerQueryProfesionales(string tipoLista)
        {
            return NormalizarTexto(tipoLista).ToUpper() switch
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
                    ORDER BY VGT.IdContacto;",

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
                    ORDER BY VGT.IdContacto;",

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
                    ORDER BY VGT.IdContacto;",

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
                    ORDER BY VGT.IdContacto;",

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
                    ORDER BY VGT.IdContacto;",

                _ => string.Empty
            };
        }

        #endregion  
    }
}