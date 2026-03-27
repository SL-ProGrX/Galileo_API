using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrDesemConceptosDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmCrDesemConceptosDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmCrDesemConceptosDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtener la lista de conceptos para desembolsos 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConceptoDesembData>> CrDesembConceptos_Obtener(int codEmpresa)
        {
            string query = @"select 
            COD_CONDEB,descripcion,cod_cuenta,retiene,modifica,difiere,DIFIERE_CUENTA,activo 
            from CONCEPTO_DESEMB order by descripcion";
            return DbHelper.ExecuteListQuery<CrConceptoDesembData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Guardar un concepto para desembolso
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codConta"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrDesembConcepto_Guardar(int codEmpresa, string usuario, int codConta, CrConceptoDesembData request)
        {
            var existeCuenta = FxVerificaCuenta(
                codEmpresa,
                codConta,
                request.cod_cuenta
            );
            if (!existeCuenta)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = " - Especifique una cuenta contable válida!"
                };
            }
            if (request.difiere) { 
                var existeCtaDif = FxVerificaCuenta(
                    codEmpresa,
                    codConta,
                    request.difiere_cuenta
                );
                if (!existeCtaDif)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = " - La Cuenta Contable para Diferir no es válida!"
                    };
                }
            }
            var resp = request.cod_condeb > 0
                ? ActualizarConceptoDesembolso(codEmpresa, usuario, request)
                : InsertarConceptoDesembolso(codEmpresa, usuario, request);
            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Información guardada satisfactoriamente..."
            };
        }

        /// <summary>
        /// Eliminar un concepto para desembolso
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCondeb"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CrDesembConcepto_Eliminar(int codEmpresa, int codCondeb, string usuario)
        {
            const string sqlDelete = @"delete CONCEPTO_DESEMB where COD_CONDEB = @CodCondeb;";

            var respDelete = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlDelete,
                new { CodCondeb = codCondeb }
            );

            if (respDelete.Code < 0)
                return respDelete;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Elimina - WEB",
                detalle: $"Concepto de Desembolso : {codCondeb}"
            );

            return respDelete;
        }

        /// <summary>
        /// Actualizar un concepto para desembolso
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto ActualizarConceptoDesembolso(int codEmpresa, string usuario, CrConceptoDesembData request)
        {
            const string sqlUpdate = @"
            UPDATE CONCEPTO_DESEMB
            SET
                descripcion = @Descripcion,
                cod_cuenta = @CodCuenta,
                retiene = @Retiene,
                modifica = @Modifica,
                difiere = @Difiere,
                difiere_cuenta = @DifiereCuenta,
                activo = @Activo
            WHERE cod_condeb = @CodCondeb;";

            var respUpdate = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlUpdate,
                new
                {
                    CodCondeb = request.cod_condeb,
                    Descripcion = request.descripcion,
                    CodCuenta = request.cod_cuenta,
                    Retiene = request.retiene ? 1 : 0,
                    Modifica = request.modifica ? 1 : 0,
                    Difiere = request.difiere ? 1 : 0,
                    DifiereCuenta = request.difiere_cuenta,
                    Activo = request.activo ? 1 : 0
                }
            );

            if (respUpdate.Code < 0)
                return respUpdate;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Modifica - WEB",
                detalle: $"Concepto Desembolso : {request.descripcion}"
            );

            return respUpdate;
        }

        /// <summary>
        /// Insertar un concepto para desembolso
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private ErrorDto InsertarConceptoDesembolso(int codEmpresa, string usuario, CrConceptoDesembData request)
        {
            const string sqlInsert = @"
            INSERT INTO CONCEPTO_DESEMB
            (
                descripcion,
                cod_cuenta,
                retiene,
                modifica,
                difiere,
                difiere_cuenta,
                activo
            )
            VALUES
            (
                @Descripcion,
                @CodCuenta,
                @Retiene,
                @Modifica,
                @Difiere,
                @DifiereCuenta,
                @Activo
            );";

            var respInsert = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlInsert,
                new
                {
                    Descripcion = request.descripcion,
                    CodCuenta = request.cod_cuenta,
                    Retiene = request.retiene ? 1 : 0,
                    Modifica = request.modifica ? 1 : 0,
                    Difiere = request.difiere ? 1 : 0,
                    DifiereCuenta = request.difiere_cuenta,
                    Activo = request.activo ? 1 : 0
                }
            );

            if (respInsert.Code < 0)
                return respInsert;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Registra - WEB",
                detalle: $"Concepto Desembolso : {request.descripcion}"
            );

            return respInsert;
        }

        /// <summary>
        /// Verificar que la cuenta contable exista y acepte movimientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="strCuenta"></param>
        /// <returns></returns>
        public bool FxVerificaCuenta(int codEmpresa, int codContabilidad, string strCuenta)
        {
            const string query = @"select isnull(count(*), 0)
                from CntX_Cuentas 
                where cod_contabilidad = @codConta 
                  and cod_cuenta = @codCuenta 
                  and acepta_movimientos = 1;";

            int existe = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0,
                new
                {
                    codConta = codContabilidad,
                    codCuenta = strCuenta
                }).Result;

            return existe > 0;
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
