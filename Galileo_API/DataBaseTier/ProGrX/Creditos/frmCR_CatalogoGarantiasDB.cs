using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCatalogoGarantiasDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrCatalogoGarantiasDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrCatalogoGarantiasDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene el listado de tipos de garantias registrados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrGarantiaTiposData>> CrGarantiaTipos_Obtener(int codEmpresa)
        {
            string query = @"select G.*, 
            isnull(Cta.Descripcion,'') as 'Cta_Desc', isnull(Cta.Cod_Cuenta_Mask,'') as 'Cta_Mask' 
            from crd_garantia_tipos G left join vCNTX_CUENTAS_LOCAL Cta on Cta.cod_Cuenta = G.cod_cuenta_incobrable 
            order by G.garantia";
            return DbHelper.ExecuteListQuery<CrGarantiaTiposData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guarda tipo de garantia (Inserta o Actualiza dependiendo si existe o no)
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrGarantiaTipos_Guardar(int codEmpresa, string usuario, CrGarantiaTiposData request)
        {
            var existe = ExisteGarantia(codEmpresa, request.garantia);

            var resp = existe
                ? ActualizarGarantia(codEmpresa, usuario, request)
                : InsertarGarantia(codEmpresa, usuario, request);

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Elimina un tipo de garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="garantia"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrGarantiaTipos_Eliminar(int codEmpresa, string garantia, string usuario)
        {
            const string sqlDelete = @"DELETE FROM crd_garantia_tipos WHERE garantia = @Garantia;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new
                {
                    Garantia = garantia.Trim()
                }
            );

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Tipo de Garantía : {garantia}"
            );

            return respDelete;
        }

        /// <summary>
        /// Actualiza tipo de garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarGarantia(int codEmpresa, string usuario, CrGarantiaTiposData request)
        {
            const string sqlUpdate = @"
            UPDATE crd_garantia_tipos
            SET
                descripcion              = @Descripcion,
                formulario               = @Formulario,
                maximos_utiliza          = @MaximosUtiliza,
                maximos_monto            = @MaximosMonto,
                prioridad                = @Prioridad,
                cod_cuenta_incobrable    = @CodCuentaIncobrable,
                porc_mitigador           = @PorcMitigador,
                ref_plazo                = @RefPlazo,
                ref_tasa                 = @RefTasa,
                v_disponible             = @VDisponible
            WHERE garantia = @Garantia;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    Garantia = request.garantia,
                    Descripcion = request.descripcion,
                    Formulario = request.formulario.ToUpper(),
                    MaximosUtiliza = request.maximos_utiliza ? 1 : 0,
                    MaximosMonto = request.maximos_monto,
                    Prioridad = request.prioridad,
                    CodCuentaIncobrable = request.cta_mask,
                    PorcMitigador = request.porc_mitigador,
                    RefPlazo = request.ref_plazo,
                    RefTasa = request.ref_tasa,
                    VDisponible = request.v_disponible ? 1 : 0
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Tipo de Garantía : {request.garantia}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Insertar tipo de garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarGarantia(int codEmpresa, string usuario, CrGarantiaTiposData request)
        {
            const string sqlInsert = @"
            INSERT INTO crd_garantia_tipos
            (
                garantia,
                descripcion,
                formulario,
                maximos_utiliza,
                maximos_monto,
                prioridad,
                cod_cuenta_incobrable,
                porc_mitigador,
                ref_plazo,
                ref_tasa,
                v_disponible
            )
            VALUES
            (
                @Garantia,
                @Descripcion,
                @Formulario,
                @MaximosUtiliza,
                @MaximosMonto,
                @Prioridad,
                @CodCuentaIncobrable,
                @PorcMitigador,
                @RefPlazo,
                @RefTasa,
                @VDisponible
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    Garantia = request.garantia,
                    Descripcion = request.descripcion,
                    Formulario = request.formulario.ToUpper(),
                    MaximosUtiliza = request.maximos_utiliza ? 1 : 0,
                    MaximosMonto = request.maximos_monto,
                    Prioridad = request.prioridad,
                    CodCuentaIncobrable = request.cta_mask,
                    PorcMitigador = request.porc_mitigador,
                    RefPlazo = request.ref_plazo,
                    RefTasa = request.ref_tasa,
                    VDisponible = request.v_disponible ? 1 : 0
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Tipo de Garantía : {request.garantia}"
            );

            return respInsert;
        }

        /// <summary>
        /// Validar existencia de tipo de garantia
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="garantia"></param>
        /// <returns></returns>
        private bool ExisteGarantia(int codEmpresa, string garantia)
        {
            const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0) 
            FROM crd_garantia_tipos WHERE garantia = @Garantia;";

            var resp = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sqlExiste,
                0,
                new
                {
                    Garantia = garantia.Trim()
                }
            );

            return resp.Result > 0;
        }

        /// <summary>
        /// Registrar en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
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
