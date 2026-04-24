using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaTiposSalariosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;

        public FrmPreaTiposSalariosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmPreaTiposSalariosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de tipos de salarios.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPreaTiposSalariosData>> CrPreaTiposSalarios_Obtener(int codEmpresa)
        {
            const string query = @"
                SELECT
                    tipo_salario,
                    descripcion,
                    prioridad,
                    meses,
                    CAST(modifica_devengado AS bit) AS modifica_devengado,
                    CAST(modifica_rebajo_extras AS bit) AS modifica_rebajo_extras,
                    CAST(modifica_extras_fijas AS bit) AS modifica_extras_fijas,
                    operacion,
                    CAST(activo AS bit) AS activo
                FROM Crd_Prea_Tipo_Salario
                ORDER BY tipo_salario;";

            return DbHelper.ExecuteListQuery<CrdPreaTiposSalariosData>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Guarda un tipo de salario, agregando uno nuevo o actualizando uno existente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTiposSalarios_Guardar(int codEmpresa, string usuario, CrdPreaTiposSalariosData request)
        {
            var resp = ExisteTipoSalario(codEmpresa, request.tipo_salario)
                ? ActualizarTipoSalario(codEmpresa, usuario, request)
                : InsertarTipoSalario(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un tipo de salario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipoSalario"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrPreaTiposSalarios_Eliminar(int codEmpresa, string tipoSalario, string usuario)
        {
            const string sqlDelete = @"
                DELETE Crd_Prea_Tipo_Salario
                WHERE tipo_salario = @TipoSalario;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    TipoSalario = tipoSalario?.Trim()
                });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Estudio de Credito - Tipo Salario Id: {tipoSalario}"
            );

            return respDelete;
        }

        /// <summary>
        /// Valida si existe un tipo de salario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipoSalario"></param>
        /// <returns></returns>
        private bool ExisteTipoSalario(int codEmpresa, string tipoSalario)
        {
            const string sqlExiste = @"
                SELECT ISNULL(COUNT(*), 0)
                FROM Crd_Prea_Tipo_Salario
                WHERE tipo_salario = @TipoSalario;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    TipoSalario = tipoSalario?.Trim()
                });

            if (resp.Code < 0)
                return false;

            return resp.Result > 0;
        }

        /// <summary>
        /// Actualiza un tipo de salario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarTipoSalario(int codEmpresa, string usuario, CrdPreaTiposSalariosData request)
        {
            const string sqlUpdate = @"
                UPDATE Crd_Prea_Tipo_Salario
                SET
                    descripcion = @Descripcion,
                    prioridad = @Prioridad,
                    meses = @Meses,
                    modifica_devengado = @ModificaDevengado,
                    modifica_rebajo_extras = @ModificaRebajoExtras,
                    modifica_extras_fijas = @ModificaExtrasFijas,
                    operacion = @Operacion,
                    activo = @Activo 
                WHERE tipo_salario = @TipoSalario;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    TipoSalario = request.tipo_salario?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Prioridad = request.prioridad,
                    Meses = request.meses,
                    ModificaDevengado = request.modifica_devengado ? 1 : 0,
                    ModificaRebajoExtras = request.modifica_rebajo_extras ? 1 : 0,
                    ModificaExtrasFijas = request.modifica_extras_fijas ? 1 : 0,
                    Operacion = request.operacion?.Trim(),
                    Activo = request.activo ? 1 : 0
                });

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"PreAnalisis Tipo Salario Cod : {request.tipo_salario}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Agrega un tipo de salario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarTipoSalario(int codEmpresa, string usuario, CrdPreaTiposSalariosData request)
        {
            const string sqlInsert = @"
                INSERT INTO Crd_Prea_Tipo_Salario
                (
                    tipo_salario,
                    descripcion,
                    prioridad,
                    meses,
                    modifica_devengado,
                    modifica_rebajo_extras,
                    modifica_extras_fijas,
                    operacion,
                    activo 
                )
                VALUES
                (
                    @TipoSalario,
                    @Descripcion,
                    @Prioridad,
                    @Meses,
                    @ModificaDevengado,
                    @ModificaRebajoExtras,
                    @ModificaExtrasFijas,
                    @Operacion,
                    @Activo 
                );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    TipoSalario = request.tipo_salario?.Trim(),
                    Descripcion = request.descripcion?.Trim(),
                    Prioridad = request.prioridad,
                    Meses = request.meses,
                    ModificaDevengado = request.modifica_devengado ? 1 : 0,
                    ModificaRebajoExtras = request.modifica_rebajo_extras ? 1 : 0,
                    ModificaExtrasFijas = request.modifica_extras_fijas ? 1 : 0,
                    Operacion = request.operacion?.Trim(),
                    Activo = request.activo ? 1 : 0
                });

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"PreAnalisis Tipo de Salario Cod : {request.tipo_salario}"
            );

            return respInsert;
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