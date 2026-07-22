using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrComisionesBancosDb
    {
        private const string TipoCheques = "CHEQUES";
        private const string TipoTransferencias = "TRANSFERENCIAS";

        private readonly PortalDB _portalDb;

        public FrmCrComisionesBancosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Incorpora los bancos faltantes en CRD_Bancos_Autorizados
        /// y devuelve la lista completa.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComisionesBancosItem>>
            CR_frmCR_Comisiones_Bancos_Inicializar(
                int codEmpresa,
                CrComisionesBancosInicializarRequest request)
        {
            string usuario = (request.usuario ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    new List<CrComisionesBancosItem>());
            }

            const string sqlInsertar = @"
                insert into CRD_Bancos_Autorizados
                (
                    id_banco,
                    cheques,
                    transferencias,
                    registro_fecha,
                    registro_usuario
                )
                select
                    B.id_banco,
                    0,
                    0,
                    Getdate(),
                    @Usuario
                from Tes_Bancos B
                where not exists
                (
                    select 1
                    from CRD_Bancos_Autorizados X with (updlock, holdlock)
                    where X.id_banco = B.id_banco
                );";

            return DbHelper.WithConn(
            _portalDb,
            codEmpresa,
            connection =>
            {
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    connection.Execute(
                        sqlInsertar,
                        new { Usuario = usuario },
                        transaction);

                    List<CrComisionesBancosItem> bancos =
                        ConsultarBancos(connection, transaction);

                    transaction.Commit();

                    return bancos;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        /// <summary>
        /// Obtiene los bancos configurados para el pago de comisiones.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrComisionesBancosItem>>
            CR_frmCR_Comisiones_Bancos_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    X.id_banco,
                    rtrim(B.descripcion) as descripcion,
                    convert(bit, isnull(X.cheques, 0)) as cheques,
                    convert(bit, isnull(X.transferencias, 0))
                        as transferencias
                from CRD_Bancos_Autorizados X
                inner join Tes_Bancos B
                    on X.id_banco = B.id_banco
                order by B.id_banco;";

            return DbHelper.ExecuteListQuery<CrComisionesBancosItem>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Actualiza el tipo de desembolso autorizado para un banco.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_frmCR_Comisiones_Bancos_Actualizar(
            int codEmpresa,
            CrComisionesBancosActualizarRequest request)
        {
            string tipo = (request.tipo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (request.id_banco <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El banco indicado no es válido.",
                    -2);
            }

            if (tipo is not (TipoCheques or TipoTransferencias))
            {
                return DbHelper.ErrorResponse(
                    "El tipo de desembolso no es válido.",
                    -2);
            }

            const string sql = @"
                update CRD_Bancos_Autorizados
                set
                    cheques =
                        case
                            when @Tipo = 'CHEQUES' then @Valor
                            else cheques
                        end,
                    transferencias =
                        case
                            when @Tipo = 'TRANSFERENCIAS' then @Valor
                            else transferencias
                        end
                where id_banco = @IdBanco;";

            var respuesta = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdBanco = request.id_banco,
                    Tipo = tipo,
                    Valor = request.valor
                });

            if (respuesta.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    respuesta.Description ??
                    "No fue posible actualizar el banco.",
                    respuesta.Code.GetValueOrDefault(-1));
            }

            if (respuesta.Result == 0)
            {
                return DbHelper.ErrorResponse(
                    "No se encontró el banco indicado.",
                    -2);
            }

            return DbHelper.CreateOkResponse();
        }

        private static List<CrComisionesBancosItem> ConsultarBancos(
            System.Data.IDbConnection connection,
            System.Data.IDbTransaction transaction)
        {
            const string sql = @"
                select
                    X.id_banco,
                    rtrim(B.descripcion) as descripcion,
                    convert(bit, isnull(X.cheques, 0)) as cheques,
                    convert(bit, isnull(X.transferencias, 0))
                        as transferencias
                from CRD_Bancos_Autorizados X
                inner join Tes_Bancos B
                    on X.id_banco = B.id_banco
                order by B.id_banco;";

            return connection.Query<CrComisionesBancosItem>(
                    sql,
                    transaction: transaction)
                .ToList();
        }
    }
}