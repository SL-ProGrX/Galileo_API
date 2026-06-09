namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    using Galileo.DataBaseTier;
    using Galileo.Models.ERROR;
    using Galileo.Models.Security;
    using Galileo_API.Models.ProGrX.Creditos;

    public class FrmCrCatalogoExcDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoExcDB(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoExcDB(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el listado de disponibilidad EXC.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrCatalogoExcDisponibleModel>> CrCatalogoExc_Disponible_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                select 
                    MES as Mes,
                    ACUMULADO_MES as Acumulado_Mes,
                    ACUMULADO_PORC as Acumulado_Porc,
                    CAPGEN as CapGen,
                    REGISTRO_FECHA as Registro_Fecha,
                    REGISTRO_USUARIO as Registro_Usuario,
                    MODIFICA_FECHA as Modifica_Fecha,
                    MODIFICA_USUARIO as Modifica_Usuario
                from EXC_DISPONIBLE
                order by mes";

            return DbHelper.ExecuteListQuery<CrCatalogoExcDisponibleModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Guarda disponibilidad de excedentes (Inserta o Actualiza dependiendo si existe o no).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoExc_Disponible_Guardar(int CodEmpresa, CrCatalogoExcDisponibleGuardarRequest request)
        {
            var existe = ExisteDisponible(CodEmpresa, request.Mes);

            var resp = existe
                ? ActualizarDisponible(CodEmpresa, request)
                : InsertarDisponible(CodEmpresa, request);

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                CodEmpresa,
                request.Usuario,
                movimiento: existe ? "Modifica - WEB" : "Registra - WEB",
                detalle: $"Disponible Excedentes Mes: {request.Mes}");

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina registro de excedentes disponible por mes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrCatalogoExc_Disponible_Eliminar(int CodEmpresa, CrCatalogoExcDisponibleEliminarRequest request)
        {
            const string sqlDelete = @"
                delete EXC_DISPONIBLE
                where MES = @Mes";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlDelete,
                new { request.Mes });

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                CodEmpresa,
                request.Usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Disponible Excedentes Mes: {request.Mes}");

            return respDelete;
        }

        /// <summary>
        /// Valida si existe un registro de excedentes disponible para el mes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        private bool ExisteDisponible(int CodEmpresa, int mes)
        {
            const string sqlExiste = @"
                select isnull(count(*),0) as Existe
                from EXC_DISPONIBLE
                where MES = @Mes";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                CodEmpresa,
                sqlExiste,
                0,
                new { Mes = mes });

            return resp.Result > 0;
        }

        /// <summary>
        /// Inserta registro de excedentes disponible.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarDisponible(int CodEmpresa, CrCatalogoExcDisponibleGuardarRequest request)
        {
            const string sqlInsert = @"
                insert into EXC_DISPONIBLE
                (
                    MES,
                    ACUMULADO_MES,
                    ACUMULADO_PORC,
                    CAPGEN,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                values
                (
                    @Mes,
                    @Acumulado_Mes,
                    @Acumulado_Porc,
                    @CapGen,
                    dbo.MyGetDate(),
                    @Usuario
                )";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlInsert,
                new
                {
                    request.Mes,
                    request.Acumulado_Mes,
                    request.Acumulado_Porc,
                    request.CapGen,
                    Usuario = (request.Usuario ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Actualiza registro de excedentes disponible.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarDisponible(int CodEmpresa, CrCatalogoExcDisponibleGuardarRequest request)
        {
            const string sqlUpdate = @"
                update EXC_DISPONIBLE
                set 
                    ACUMULADO_MES = @Acumulado_Mes,
                    ACUMULADO_PORC = @Acumulado_Porc,
                    CAPGEN = @CapGen,
                    MODIFICA_FECHA = dbo.mygetdate(),
                    MODIFICA_USUARIO = @Usuario
                where MES = @Mes";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sqlUpdate,
                new
                {
                    request.Mes,
                    request.Acumulado_Mes,
                    request.Acumulado_Porc,
                    request.CapGen,
                    Usuario = (request.Usuario ?? string.Empty).Trim()
                });
        }

        /// <summary>
        /// Registrar en bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int CodEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
