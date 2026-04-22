using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaClasificacionesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;
        private readonly string vEliminaM = "Elimina - WEB";
        private readonly string vRegistraM = "Registra - WEB";
        private readonly string vModificaM = "Modifica - WEB";


        public FrmPreaClasificacionesDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmPreaClasificacionesDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene las razones de clasificacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaClasificacionRazonData>> PreaClasificacion_Razones_Obtener(int codEmpresa)
        {
            const string query = @"select cod_razon,descripcion,color 
                from Crd_Clasificacion_Razon order by cod_razon";
            return DbHelper.ExecuteListQuery<PreaClasificacionRazonData>(
                _portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene el catalogo de clasificacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="catalogo"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaClasificacionData>> PreaClasificacion_Catalogo_Obtener(int codEmpresa, string catalogo)
        {
            string query = "";
            switch(catalogo)
            {
                case "garantia":
                query = @"select 
                    A.cod_garantia as codigo,A.descripcion, rtrim(B.cod_Razon) as razon, rtrim(B.descripcion) as razon_desc 
                    from Crd_Clasificacion_Garantia A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon 
                    order by A.cod_Garantia";
                break;

                case "mora":
                query = @"select A.cod_mora as codigo, case 
                    when A.tipo = 'A' then 'Al Día'
                    when A.tipo = 'M' then 'Mora'
                    when A.tipo = 'C' then 'Cobro (Ejecutado)'
                    when A.tipo = 'I' then 'Incobrable' end as Tipo 
                    ,A.desde,A.hasta,rtrim(B.cod_Razon) as razon, rtrim(B.descripcion) as razon_desc 
                    from Cbr_Clasificacion_Mora A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon
                    order by A.cod_mora";
                 break;

                case "capacidad":
                query = @"select 
                    A.cod_capacidad as codigo,A.desde,A.hasta,rtrim(B.cod_Razon) as razon, rtrim(B.descripcion) as razon_desc 
                    from Crd_Clasificacion_Capacidad A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon
                    order by A.cod_capacidad";
                    break;

                case "endeudamiento":
                query = @"select 
                    A.cod_endeudamiento as codigo,A.desde,A.hasta,rtrim(B.cod_Razon) as razon, rtrim(B.descripcion) as razon_desc 
                    from Crd_Clasificacion_endeudamiento A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon
                    order by A.cod_endeudamiento";
                break;

                case "historial":
                query = @"select 
                    A.cod_historial as codigo,A.descripcion, rtrim(B.cod_Razon) as razon, rtrim(B.descripcion) as razon_desc 
                    from Crd_Clasificacion_historial A inner join Crd_Clasificacion_Razon B on A.cod_Razon = B.Cod_Razon 
                    order by A.cod_historial";
                    break;

                default:
                    break;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return new ErrorDto<List<PreaClasificacionData>>
                {
                    Code = -1,
                    Description = "El catálogo solicitado no es válido.",
                    Result = new List<PreaClasificacionData>()
                };
            }

            return DbHelper.ExecuteListQuery<PreaClasificacionData>(
                _portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guarda o actualiza una razon de clasificacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Razon_Guardar(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            var resp = ExisteRazon(codEmpresa, request.cod_razon)
                ? ActualizarRazon(codEmpresa, usuario, request)
                : InsertarRazon(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = resp.Description
            };
        }

        /// <summary>
        /// Elimina una razon de clasificacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codRazon"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Razon_Eliminar(int codEmpresa, string codRazon, string usuario)
        {
            const string sqlDelete = @"delete Crd_Clasificacion_Razon where cod_razon = @CodRazon;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodRazon = codRazon?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vEliminaM,
                detalle: $"PreAnalisis (Razon) : {codRazon}"
            );

            return respDelete;
        }

        #region Razon helpers
        private bool ExisteRazon(int codEmpresa, string codRazon)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) as Existe 
            FROM Crd_Clasificacion_Razon WHERE cod_razon = @CodRazon;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodRazon = codRazon.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarRazon(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Clasificacion_Razon
            SET
                descripcion = @Descripcion,
                color = @Color
            WHERE cod_razon = @CodRazon;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodRazon = request.cod_razon?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Color = request.color?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vModificaM,
                detalle: $"PreAnalisis (Razon) : {request.cod_razon}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarRazon(int codEmpresa, string usuario, PreaClasificacionRazonData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Clasificacion_Razon
            (
                cod_razon,
                descripcion,
                color
            )
            VALUES
            (
                @CodRazon,
                @Descripcion,
                @Color
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodRazon = request.cod_razon?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Color = request.color?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vRegistraM,
                detalle: $"PreAnalisis (Razon) : {request.cod_razon}"
            );

            return respInsert;
        }
        #endregion

        /// <summary>
        /// Guarda o actualiza una clasificacion de garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Garantia_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            var resp = ExisteGarantia(codEmpresa, request.codigo)
                ? ActualizarGarantia(codEmpresa, usuario, request)
                : InsertarGarantia(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = resp.Description
            };
        }

        /// <summary>
        /// Elimina una clasificacion de garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codGarantia"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Garantia_Eliminar(int codEmpresa, string codGarantia, string usuario)
        {
            const string sqlDeleteDt = @"delete Crd_Clasificacion_Garantia_Dt where cod_garantia = @CodGarantia;";
            const string sqlDelete = @"delete Crd_Clasificacion_Garantia where cod_garantia = @CodGarantia;";

            var respDeleteDt = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDeleteDt,
                new
                {
                    CodGarantia = codGarantia
                });

            if (respDeleteDt.Code < 0)
                return respDeleteDt;

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodGarantia = codGarantia
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vEliminaM,
                detalle: $"Clasificacion Garantia : {codGarantia}"
            );

            return respDelete;
        }

        #region Garantia helpers
        private bool ExisteGarantia(int codEmpresa, string codGarantia)
        {
            const string sqlExiste = @"
            SELECT ISNULL(COUNT(*), 0) as Existe
            FROM Crd_Clasificacion_Garantia
            WHERE cod_Garantia = @CodGarantia;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodGarantia = codGarantia.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarGarantia(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Clasificacion_Garantia
            SET
                descripcion = @Descripcion,
                cod_razon = @CodRazon
            WHERE cod_Garantia = @CodGarantia;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodGarantia = request.codigo?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    CodRazon = request.razon?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vModificaM,
                detalle: $"Clasificacion Garantía : {request.codigo}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarGarantia(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Clasificacion_Garantia
            (
                cod_Garantia,
                descripcion,
                cod_razon
            )
            VALUES
            (
                @CodGarantia,
                @Descripcion,
                @CodRazon
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodGarantia = request.codigo?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    CodRazon = request.razon?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vRegistraM,
                detalle: $"Clasificacion Garantía : {request.codigo}"
            );

            return respInsert;
        }
        #endregion

        /// <summary>
        /// Guarda o actualiza una clasificacion de mora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Mora_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            var resp = ExisteMora(codEmpresa, request.codigo)
                ? ActualizarMora(codEmpresa, usuario, request)
                : InsertarMora(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = resp.Description
            };
        }

        /// <summary>
        /// Elimina una clasificacion de mora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codMora"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Mora_Eliminar(int codEmpresa, string codMora, string usuario)
        {
            const string sqlDelete = @"DELETE Cbr_Clasificacion_Mora WHERE cod_Mora = @CodMora;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodMora = codMora?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vEliminaM,
                detalle: $"Clasificacion Mora : {codMora}"
            );

            return respDelete;
        }

        #region Mora helpers
        private bool ExisteMora(int codEmpresa, string codMora)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) as Existe 
            FROM Cbr_Clasificacion_Mora WHERE cod_mora = @CodMora;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodMora = codMora.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarMora(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlUpdate = @"
            UPDATE Cbr_Clasificacion_Mora 
            SET
                tipo = @Tipo,
                desde = @Desde,
                hasta = @Hasta,
                cod_razon = @CodRazon
            WHERE cod_mora = @CodMora;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodMora = request.codigo?.Trim(),
                    Tipo = request.tipo?.Substring(0, 1),
                    Desde = request.desde,
                    Hasta = request.hasta,
                    CodRazon = request.razon?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vModificaM,
                detalle: $"Clasificacion Mora : {request.codigo}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarMora(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlInsert = @"
            INSERT INTO Cbr_Clasificacion_Mora
            (
                cod_mora,
                tipo,
                desde,
                hasta,
                cod_razon
            )
            VALUES
            (
                @CodMora,
                @Tipo,
                @Desde,
                @Hasta,
                @CodRazon
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodMora = request.codigo?.Trim(),
                    Tipo = request.tipo?.Substring(0, 1),
                    Desde = request.desde,
                    Hasta = request.hasta,
                    CodRazon = request.razon?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vRegistraM,
                detalle: $"Clasificacion Mora : {request.codigo}"
            );

            return respInsert;
        }
        #endregion

        /// <summary>
        /// Guarda o actualiza una clasificacion de capacidad
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Capacidad_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            var resp = ExisteCapacidad(codEmpresa, request.codigo)
                ? ActualizarCapacidad(codEmpresa, usuario, request)
                : InsertarCapacidad(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = resp.Description
            };
        }

        /// <summary>
        /// Elimina una clasificacion de capacidad
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCapacidad"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Capacidad_Eliminar(int codEmpresa, string codCapacidad, string usuario)
        {
            const string sqlDelete = @"DELETE Crd_Clasificacion_Capacidad 
            WHERE cod_capacidad = @CodCapacidad;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodCapacidad = codCapacidad?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vEliminaM,
                detalle: $"Clasificacion Capacidad : {codCapacidad}"
            );

            return respDelete;
        }

        #region Capacidad helpers
        private bool ExisteCapacidad(int codEmpresa, string codCapacidad)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) as Existe 
            FROM Crd_Clasificacion_Capacidad WHERE cod_capacidad = @CodCapacidad;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodCapacidad = codCapacidad.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarCapacidad(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Clasificacion_Capacidad
            SET
                desde = @Desde,
                hasta = @Hasta,
                cod_razon = @CodRazon
            WHERE cod_capacidad = @CodCapacidad;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodCapacidad = request.codigo?.Trim(),
                    Desde = request.desde,
                    Hasta = request.hasta,
                    CodRazon = request.razon?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vModificaM,
                detalle: $"Clasificacion Capacidad : {request.codigo}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarCapacidad(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Clasificacion_Capacidad
            (
                cod_capacidad,
                desde,
                hasta,
                cod_razon
            )
            VALUES
            (
                @CodCapacidad,
                @Desde,
                @Hasta,
                @CodRazon
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodCapacidad = request.codigo?.Trim(),
                    Desde = request.desde,
                    Hasta = request.hasta,
                    CodRazon = request.razon?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vRegistraM,
                detalle: $"Clasificacion Capacidad : {request.codigo}"
            );

            return respInsert;
        }
        #endregion

        /// <summary>
        /// Guarda o actualiza una clasificacion de endeudamiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Endeudamiento_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            var resp = ExisteEndeudamiento(codEmpresa, request.codigo)
                ? ActualizarEndeudamiento(codEmpresa, usuario, request)
                : InsertarEndeudamiento(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = resp.Description
            };
        }

        /// <summary>
        /// Elimina una clasificacion de endeudamiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codEndeudamiento"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Endeudamiento_Eliminar(int codEmpresa, string codEndeudamiento, string usuario)
        {
            const string sqlDelete = @"DELETE Crd_Clasificacion_Endeudamiento 
            WHERE cod_Endeudamiento = @CodEndeudamiento;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodEndeudamiento = codEndeudamiento?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vEliminaM,
                detalle: $"Clasificacion Endeudamiento : {codEndeudamiento}"
            );

            return respDelete;
        }

        #region Endeudamiento helpers
        private bool ExisteEndeudamiento(int codEmpresa, string codEndeudamiento)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) as Existe 
            FROM Crd_Clasificacion_Endeudamiento WHERE cod_Endeudamiento = @CodEndeudamiento;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodEndeudamiento = codEndeudamiento.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarEndeudamiento(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Clasificacion_Endeudamiento
            SET
                desde = @Desde,
                hasta = @Hasta,
                cod_razon = @CodRazon
            WHERE cod_Endeudamiento = @CodEndeudamiento;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodEndeudamiento = request.codigo?.Trim(),
                    Desde = request.desde,
                    Hasta = request.hasta,
                    CodRazon = request.razon?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vModificaM,
                detalle: $"Clasificacion Endeudamiento : {request.codigo}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarEndeudamiento(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Clasificacion_Endeudamiento
            (
                cod_Endeudamiento,
                desde,
                hasta,
                cod_razon
            )
            VALUES
            (
                @CodEndeudamiento,
                @Desde,
                @Hasta,
                @CodRazon
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodEndeudamiento = request.codigo?.Trim(),
                    Desde = request.desde,
                    Hasta = request.hasta,
                    CodRazon = request.razon?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vRegistraM,
                detalle: $"Clasificacion Endeudamiento : {request.codigo}"
            );

            return respInsert;
        }

        #endregion

        /// <summary>
        /// Guarda o actualiza una clasificacion de historial
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Historial_Guardar(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            var resp = ExisteHistorial(codEmpresa, request.codigo)
                ? ActualizarHistorial(codEmpresa, usuario, request)
                : InsertarHistorial(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = resp.Description
            };
        }

        /// <summary>
        /// Elimina una clasificacion de historial
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codHistorial"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Historial_Eliminar(int codEmpresa, string codHistorial, string usuario)
        {
            const string sqlDelete = @"DELETE Crd_Clasificacion_Historial
            WHERE cod_historial = @CodHistorial;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    CodHistorial = codHistorial?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vEliminaM,
                detalle: $"Clasificacion Historial : {codHistorial}"
            );

            return respDelete;
        }

        #region Historial helpers
        private bool ExisteHistorial(int codEmpresa, string codHistorial)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) as Existe
            FROM Crd_Clasificacion_Historial WHERE cod_historial = @CodHistorial;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    CodHistorial = codHistorial.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        private ErrorDto ActualizarHistorial(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlUpdate = @"
            UPDATE Crd_Clasificacion_Historial
            SET
                descripcion = @Descripcion,
                cod_razon = @CodRazon
            WHERE cod_historial = @CodHistorial;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodHistorial = request.codigo?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    CodRazon = request.razon?.Trim()
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vModificaM,
                detalle: $"Clasificacion Historial : {request.codigo}"
            );

            return respUpdate;
        }

        private ErrorDto InsertarHistorial(int codEmpresa, string usuario, PreaClasificacionData request)
        {
            const string sqlInsert = @"
            INSERT INTO Crd_Clasificacion_Historial
            (
                cod_historial,
                descripcion,
                cod_razon
            )
            VALUES
            (
                @CodHistorial,
                @Descripcion,
                @CodRazon
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    CodHistorial = request.codigo?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    CodRazon = request.razon?.Trim()
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: vRegistraM,
                detalle: $"Clasificacion Historial : {request.codigo}"
            );

            return respInsert;
        }
        #endregion

        /// <summary>
        /// Obtiene las clasificaciones asignadas a una garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codGarantia"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaClasificacionGarantiaData>> PreaClasificacion_Garantia_Obtener(int codEmpresa, string codGarantia)
        {
            const string query = @"select Gt.GARANTIA, Gt.DESCRIPCION, 
            case when isnull(Gr.COD_GARANTIA,'') = '' then 0 else 1 end 'asignado' 
            from  CRD_GARANTIA_TIPOS Gt 
            left join CRD_CLASIFICACION_GARANTIA_DT Gr on Gt.GARANTIA = Gr.GARANTIA and Gr.COD_GARANTIA = @CodGarantia 
            order by Gr.COD_GARANTIA desc, Gt.DESCRIPCION";
            return DbHelper.ExecuteListQuery<PreaClasificacionGarantiaData>(
                _portalDb, codEmpresa, query, new { CodGarantia = codGarantia?.Trim() });
        }

        /// <summary>
        /// Asigna o desasigna una clasificacion a una garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codGarantia"></param>
        /// <param name="garantia"></param>
        /// <param name="asignado"></param>
        /// <returns></returns>
        public ErrorDto PreaClasificacion_Garantia_Asignar(int codEmpresa, string codGarantia, string garantia, bool asignado)
        {
            string sql = "";
            if (asignado)
            {
                sql = @"insert Crd_clasificacion_Garantia_DT (cod_garantia,garantia) 
                values(@CodGarantia, @Garantia);";
            } 
            else
            {
                sql = @"delete Crd_clasificacion_Garantia_DT 
                where cod_Garantia = @CodGarantia and garantia = @Garantia;";
            }

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodGarantia = codGarantia?.Trim(),
                    Garantia = garantia?.Trim()
                });

            if (resp.Code < 0)
                return resp;

            return resp;
        }

        /// <summary>
        /// Registra en bitacora
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
